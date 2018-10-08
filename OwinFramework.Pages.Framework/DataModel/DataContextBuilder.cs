﻿#if DEBUG
#define DETAILED_TRACE
#endif


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Utility;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataContextBuilder : IDataContextBuilder, IDebuggable
    {
        public int Id { get; private set; }

        private readonly IDataContextFactory _dataContextFactory;
        private readonly IIdManager _idManager;
        private readonly IDataCatalog _dataCatalog;

        private DataContextBuilder[] _children;
        private DataContextBuilder _parent;

        private readonly List<IDataScope> _dataScopes;
        private readonly List<IDataSupply> _requiredDataSupplies;
        private readonly List<SuppliedDependency> _requiredSuppliedDependencies;
        private readonly List<IDataConsumer> _dataConsumers = new List<IDataConsumer>();

        private List<IDataSupply> _dataSupplies;
        private List<SuppliedDependency> _suppliedDependencies;

        private IDataSupply[] _staticSupplies;

        public DataContextBuilder(
            IDataContextFactory dataContextFactory,
            IIdManager idManager,
            IDataCatalog dataCatalog,
            IDataScopeRules dataScopeRules)
        {
            _dataContextFactory = dataContextFactory;
            _idManager = idManager;
            _dataCatalog = dataCatalog;

            Id = idManager.GetUniqueId();

            var dataScopes = dataScopeRules.DataScopes;
            _dataScopes = dataScopes == null 
                ? new List<IDataScope>()
                : dataScopes.ToList();

            var dataSupplies = dataScopeRules.DataSupplies;
            _requiredDataSupplies = dataSupplies == null
                ? new List<IDataSupply>()
                : dataSupplies.ToList();

            var suppliedDependencies = dataScopeRules.SuppliedDependencies;
            _requiredSuppliedDependencies = suppliedDependencies == null
                ? new List<SuppliedDependency>()
                : suppliedDependencies.Select(sd => new SuppliedDependency(sd)).ToList();

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " constructing from data scope rules " + dataScopeRules);

            if (_dataScopes.Count > 0)
            {
                Trace.WriteLine("Data context builder #" + Id + " scopes:");
                foreach (var s in _dataScopes) Trace.WriteLine("    " + s);
            }

            if (_requiredDataSupplies.Count > 0)
            {
                Trace.WriteLine("Data context builder #" + Id + " data supplies:");
                foreach (var s in _requiredDataSupplies) Trace.WriteLine("    " + s);
            }

            if (_requiredSuppliedDependencies.Count > 0)
            {
                Trace.WriteLine("Data context builder #" + Id + " supplied dependencies:");
                foreach (var s in _requiredSuppliedDependencies) Trace.WriteLine("    " + s);
            }
#endif
        }

        public IDataContextBuilder AddChild(IDataScopeRules dataScopeRules)
        {
            var child = new DataContextBuilder(
                _dataContextFactory, 
                _idManager, 
                _dataCatalog, 
                dataScopeRules)
            {
                _parent = this
            };

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " is the parent of #" + child.Id);
#endif

            if (_children == null)
                _children = new[] { child };
            else
                _children = _children.Concat(new[] { child }).ToArray();

            return child;
        }

        public void AddConsumerNeeds(IDataConsumer consumer)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " adding needs of consumer " + consumer);
#endif
            _dataConsumers.Add(consumer);
        }

        public void AddConsumerSupplies(IDataConsumer consumer)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " adding required supplies for consumer " + consumer);
#endif

            var needs = consumer.GetConsumerNeeds();
            var dataSupplyDependencies = needs.DataSupplyDependencies;
            var dataSupplierDependencies = needs.DataSupplierDependencies;

#if DETAILED_TRACE
            if (dataSupplyDependencies != null && dataSupplyDependencies.Count > 0)
            {
                Trace.WriteLine("  consumer needs data supplies:");
                foreach (var supply in dataSupplyDependencies)
                    Trace.WriteLine("    " + supply);
            }

            if (dataSupplierDependencies != null && dataSupplierDependencies.Count > 0)
            {
                Trace.WriteLine("  consumer needs suppliers:");
                foreach (var supplierDependency in dataSupplierDependencies)
                    Trace.WriteLine("    " + supplierDependency);
            }
#endif

            if (dataSupplyDependencies != null)
            {
                foreach (var supply in dataSupplyDependencies)
                    AddSupply(supply);
            }

            if (dataSupplierDependencies != null)
            {
                foreach (var dataSupplier in dataSupplierDependencies)
                {
                    Resolve(dataSupplier.Item1, dataSupplier.Item2, true);
                }
            }
        }

        public void ResolveSupplies()
        {
#if TRACE
            Trace.WriteLine("Data context builder #" + Id + " resolving suppliers");
#endif
            foreach (var supply in _requiredDataSupplies) AddSupply(supply);
            foreach (var supplier in _requiredSuppliedDependencies) AddDataSupplier(supplier);
            foreach (var consumer in _dataConsumers) Resolve(consumer);

            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++)
                    _children[i].ResolveSupplies();
            }

            var sortedList = GetSuppliedDependenciesOrdered();

            foreach (var sd in sortedList)
                AddSupply(sd.DataSupply);

            _staticSupplies = _dataSupplies.Where(s => s.IsStatic).ToArray();
        }

        public void SetupDataContext(IRenderContext renderContext)
        {
            var rootContext = _dataContextFactory.Create(renderContext, this);
            AddDataContext(renderContext, rootContext);
            renderContext.SelectDataContext(Id);
        }

        public bool IsInScope(IDataDependency dependency)
        {
            if (_parent == null) return true;
            if (_dataScopes == null) return false;
            return _dataScopes.Any(scope => scope.IsMatch(dependency));
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
#if TRACE
            renderContext.Trace(() => "Data context builder #" + Id + " has been notified of a missing dependency on " + missingDependency);
#endif

            Resolve(missingDependency);

            var root = this;
            while (!ReferenceEquals(root._parent, null))
                root = root._parent;

            renderContext.DeleteDataContextTree();
            root.SetupDataContext(renderContext);
        }

        #region Private implementation details

        /// <summary>
        /// Adds a data supply to this context without adding the same supply twice
        /// </summary>
        private void AddSupply(IDataSupply supply)
        {
            if (_dataSupplies == null)
                _dataSupplies = new List<IDataSupply> { supply };

            if (_dataSupplies.All(s => s != supply))
            {
#if TRACE
                Trace.WriteLine("Data context builder #" + Id + " adding supply '" + supply + "'");
#endif
                _dataSupplies.Add(supply);
                Resolve(supply as IDataConsumer);
            }
        }

        /// <summary>
        /// Adds a data supplier and specific type of data to supply to this
        /// context without duplicating data in the same scope
        /// </summary>
        private void AddDataSupplier(SuppliedDependency suppliedDependency)
        {
            if (_suppliedDependencies == null)
                _suppliedDependencies = new List<SuppliedDependency>();

            if (_suppliedDependencies.All(s => !Equals(s.DataDependency, suppliedDependency.DataDependency)))
            {
#if TRACE
                Trace.WriteLine("Data context builder #" + Id + " adding supplied dependency '" + suppliedDependency + "'");
#endif
                _suppliedDependencies.Add(suppliedDependency);

                var dataConsumer = suppliedDependency.DataSupplier as IDataConsumer;
                if (dataConsumer != null)
                {
                    AddConsumerSupplies(dataConsumer);

                    suppliedDependency.DependentSupplies = Resolve(dataConsumer);
                    if (suppliedDependency.DependentSupplies != null)
                    {
                        foreach (var dynamicSupply in suppliedDependency.DependentSupplies.Where(s => !s.IsStatic))
                        {
                            suppliedDependency.DataSupply.IsStatic = false;
                            dynamicSupply.AddOnSupplyAction(rc =>
                                {
                                    var dc = rc.GetDataContext(Id);
                                    suppliedDependency.DataSupply.Supply(rc, dc);
                                });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolves all of the data needs of a data consumer and retruns a list of the data
        /// supplies that it depends on
        /// </summary>
        private List<IDataSupply> Resolve(IDataConsumer consumer)
        {
            if (consumer == null) return null;
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " resolving consumer needs for '" + consumer + "'");
#endif
            var needs = consumer.GetConsumerNeeds();
            var dataDependencies = needs.DataDependencies;

#if DETAILED_TRACE

            if (dataDependencies != null && dataDependencies.Count > 0)
            {
                Trace.WriteLine("  consumer needs data:");
                foreach (var dependency in dataDependencies)
                    Trace.WriteLine("    " + dependency);
            }

#endif

            var dependentSupplies = new List<IDataSupply>();

            if (dataDependencies != null)
            {
                foreach (var dependency in dataDependencies)
                {
                    var supplies = Resolve(dependency);
                    if (supplies != null) dependentSupplies.AddRange(supplies);
                }
            }

            return dependentSupplies;
        }

        private List<IDataSupply> Resolve(IDataDependency dependency)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " resolving dependency '" + dependency + "'");
#endif
            var suppliedDependency = _suppliedDependencies == null 
                ? null
                : _suppliedDependencies.FirstOrDefault(sd => Equals(sd.DataDependency, dependency));

            if (suppliedDependency != null)
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " dependency '" + dependency + "' already has a supplier");
#endif
                return new List<IDataSupply> { suppliedDependency.DataSupply };
            }

            if (IsInScope(dependency))
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " dependency '" + dependency + "' is in scope, getting from data catalog");
#endif
                var supplier = _dataCatalog.FindSupplier(dependency);
                if (supplier == null)
                    throw new Exception("The data catalog does not contain a supplier of '" + dependency + "'");

                suppliedDependency = new SuppliedDependency(supplier, dependency);
                AddDataSupplier(suppliedDependency);
                return new List<IDataSupply> { suppliedDependency.DataSupply };
            }

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " dependency '" + dependency + "' is not in scope, passing to parent to resolve");
#endif
            return _parent.Resolve(dependency);
        }

        private List<IDataSupply> Resolve(IDataSupplier supplier, IDataDependency dataToSupply, bool addIfMissing)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + (addIfMissing ? " adding '" : " locating '") + dataToSupply + "' from' " + supplier + "'");
#endif
            var suppliedDependency = _suppliedDependencies == null
                ? null
                : _suppliedDependencies.FirstOrDefault(sd => Equals(sd.DataDependency, dataToSupply));

            if (suppliedDependency != null)
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " dependency '" + dataToSupply + "' already has a supplier");
#endif
                return new List<IDataSupply> { suppliedDependency.DataSupply };
            }

            if (_parent != null)
            {
                var dependentSupplies = _parent.Resolve(supplier, dataToSupply, false);
                if (dependentSupplies != null)
                {
#if DETAILED_TRACE
                    Trace.WriteLine("Data context builder #" + Id + " parent was able to resolve the dependency");
#endif
                    return dependentSupplies;
                }
            }

            if (!addIfMissing) return null;

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " dependency on '" + dataToSupply + "' must be added");
#endif
            suppliedDependency = new SuppliedDependency(supplier, dataToSupply);
            AddDataSupplier(suppliedDependency);

            Resolve(supplier as IDataConsumer);

            return new List<IDataSupply> { suppliedDependency.DataSupply };
        }

        /// <summary>
        /// Returns a list of the supplied dependencies sorted such that all list entries
        /// appear after all of the entries that they depend on
        /// </summary>
        private IEnumerable<SuppliedDependency> GetSuppliedDependenciesOrdered()
        {
            if (_suppliedDependencies == null || _suppliedDependencies.Count == 0)
                return Enumerable.Empty<SuppliedDependency>();

            Func<SuppliedDependency, SuppliedDependency, bool> isDependentOn = (d1, d2) =>
            {
                if (d1.DependentSupplies == null || d1.DependentSupplies.Count == 0) return false;
                if (ReferenceEquals(d2.DataSupply, null)) return false;
                return d1.DependentSupplies.Any(s => ReferenceEquals(s, d2.DataSupply));
            };
            var listSorter = new DependencyListSorter<SuppliedDependency>();

            lock (_suppliedDependencies)
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " sorting dependencies. Original order:");
                foreach (var d in _suppliedDependencies) Trace.WriteLine("    " + d);
#endif
                var sortedList = listSorter.Sort(_suppliedDependencies, isDependentOn);
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " Order after sorting:");
                foreach (var d in sortedList) Trace.WriteLine("    " + d);
#endif
                return sortedList;
            }
        }

        /// <summary>
        /// Recursively builds a data context tree for a render context
        /// </summary>
        private void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext.CreateChild(this);
            AddDataContext(renderContext, dataContext);
        }

        /// <summary>
        /// Recursively builds a data context tree for a render context
        /// </summary>
        private void AddDataContext(IRenderContext renderContext, IDataContext dataContext)
        {
            if (_staticSupplies != null)
            {
                for (var i = 0; i < _staticSupplies.Length; i++)
                    _staticSupplies[i].Supply(renderContext, dataContext);
            }

            renderContext.AddDataContext(Id, dataContext);

            if (_children != null)
            {
                foreach (var child in _children)
                    child.BuildDataContextTree(renderContext, dataContext);
            }
        }

        #endregion

        #region Debug info

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            var debugInfo = new DebugDataScopeRules
            {
                Instance = this,
                Name = "Data context builder #" + Id,
                Type = "Data context builder"
            };

            if (_dataScopes != null && _dataScopes.Count > 0)
            {
                debugInfo.Scopes = _dataScopes
                    .Select(s => new DebugDataScope
                        {
                            DataType = s.DataType,
                            ScopeName = s.ScopeName
                        })
                    .ToList();
            }

            if (_suppliedDependencies != null && _suppliedDependencies.Count > 0)
            {
                debugInfo.DataSupplies = _suppliedDependencies
                    .Select(ds => new DebugSuppliedDependency
                        {
                            Supplier = ds.DataSupplier.GetDebugInfo<DebugDataSupplier>(),
                            DataSupply = ds.DataSupply.GetDebugInfo<DebugDataSupply>(),
                            DataTypeSupplied = ds.DataDependency == null ? null : ds.DataDependency.GetDebugInfo<DebugDataScope>()
                        })
                    .ToList();
            }

            //if (_dataSupplies != null && _dataSupplies.Count > 0)
            //{
            //}

            if (_parent != null && parentDepth != 0)
                debugInfo.Parent = _parent.GetDebugInfo<T>(parentDepth - 1, 0);

            if (_children != null && _children.Length > 0 && childDepth != 0)
                debugInfo.Children = _children
                    .Select(c => c.GetDebugInfo<T>())
                    .Cast<DebugInfo>()
                    .ToList();

            return debugInfo as T;
        }

        #endregion

        #region DataSupplier class

        private class SuppliedDependency
        {
            public IDataSupplier DataSupplier;
            public IDataDependency DataDependency;
            public IDataSupply DataSupply;
            public List<IDataSupply> DependentSupplies;

            public SuppliedDependency(IDataSupplier dataSupplier, IDataDependency dataDependency)
            {
                DataSupplier = dataSupplier;
                DataDependency = dataDependency;
                DependentSupplies = new List<IDataSupply>();

                if (DataDependency != null && !DataSupplier.IsSupplierOf(DataDependency))
                    throw new Exception("Supplier '" + DataSupplier + "' is not a supplier of '" + DataDependency + "'");

                DataSupply = DataSupplier.GetSupply(DataDependency);
            }

            public SuppliedDependency(Tuple<IDataSupplier, IDataDependency> tuple)
                : this(tuple.Item1, tuple.Item2)
            {
            }

            public override string ToString()
            {
                if (DataDependency == null) return DataSupplier.ToString();
                return DataSupplier + " -> " + DataDependency;
            }
        }

        #endregion
    }
}
