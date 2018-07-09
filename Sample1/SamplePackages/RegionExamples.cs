﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace Sample1.SamplePackages
{
    /// <summary>
    /// This code simply demonstrates some of the ways that you can define regions
    /// </summary>
    [IsPackage("regions")]
    public class RegionExamples : OwinFramework.Pages.Framework.Runtime.Package
    {
        public RegionExamples(IPackageDependenciesFactory dependencies) : base(dependencies)
        {
        }

        /// <summary>
        /// Sample data contract just to illustrate syntax
        /// </summary>
        private class NewsItem
        {
            public DateTime Published { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }

        /// <summary>
        /// 1. Most easy, least flexible
        /// 
        /// Decorate any class with the [IsRegion] attribute and the fluent builder
        /// will construct and initialize an instance of the Region class for you.
        /// Other elements can refer to this region by its name.
        /// </summary>
        [IsRegion("region1")]
        [Container("ul")]
        [Repeat(typeof(NewsItem), null, "li")]
        private class RegionExample1
        {}

        /// <summary>
        /// 2. Flexibiliy of overriding virtual methods in the base class
        /// 
        /// By inheriting from Region you can override the virtual methods to alter 
        /// the regions behavior. In this case the fluent builder needs to know how 
        /// to construct your class so you will need to pass the optional factory 
        /// method when you register with the fluent builder
        /// </summary>
        [IsRegion("region2")]
        [Container("ul")]
        [Repeat(typeof(NewsItem), null, "li")]
        private class RegionExample2 : Region
        {
            public RegionExample2(IRegionDependenciesFactory dependencies)
                : base(dependencies)
            {
            }
        }

        /// <summary>
        /// 3. Flexible registration and referencing of the region by name
        /// 
        /// Removing the [IsRegion] attribute means that the fluent builder will no longer 
        /// register the name of the region automatically, but you can explicitly register
        /// with the fluent builder by passing the Type and a factory, or constructing an
        /// instance and registering that.
        /// This also gives you the option of not registering the region by name and just
        /// use the class directly. If you go take this approach then the custom attributes
        /// will not automatically be applied. You can however make the attributes work
        /// by using the Region Builder.
        /// </summary>
        [Container("ul")]
        [Repeat(typeof(NewsItem), null, "li")]
        private class RegionExample3 : Region
        {
            public RegionExample3(IRegionDependenciesFactory dependencies) : base(dependencies)
            {
            }
        }

        /// <summary>
        /// 4. More flexibiliy in defining the attributes of the region
        /// 
        /// This is an example of defining the region properties in code rather than using
        /// attributes. This is more powerful because we can introduce logic in C# to
        /// define the properties dynamically at runtime.
        /// In this example you can still use the Fluent Builder to register the region by
        /// name, and you can still use the Region Builder to override the properties using
        /// attributes and fluent syntax.
        /// </summary>
        private class RegionExample4: Region
        {
            public RegionExample4(IRegionDependenciesFactory dependencies) : base(dependencies)
            {
                WriteOpen = w => w.WriteOpenTag("ul");
                WriteClose = w => w.WriteCloseTag("ul");

                WriteChildOpen = w => w.WriteOpenTag("li");
                WriteChildClose = w => w.WriteCloseTag("li");

                RepeatType = typeof(NewsItem);
            }
        }

        /// <summary>
        /// 5. Most complicated and most flexible.
        /// 
        /// This is an example of directly implementing IRegion. This is quite a lot of
        /// work and should never be necessary, but it can be done if you really want to.
        /// You can still register your region with the fluent builder but many of the custom
        /// attributes will not do anything.
        /// This class can
        /// </summary>
        private class RegionExample5 : IRegion
        {
            #region These properties do not need to do anything

            IElement IRegion.Content { get { return null; } }
            string IRegion.RepeatScope { get; set; }
            Type IRegion.RepeatType { get; set; }
            string IRegion.ListScope { get; set; }
            Type IRegion.ListType { get { return null; } }
            string IElement.Name { get; set; }
            IPackage IElement.Package { get; set; }
            IModule IElement.Module { get; set; }
            AssetDeployment IElement.AssetDeployment { get; set; }

            #endregion

            #region You have to specify that this is a region

            ElementType IElement.ElementType { get { return ElementType.Region; } }

            #endregion

            #region You must support the creation of instances

            private bool _isInstance;

            bool IRegion.IsInstance { get { return _isInstance; } }

            IRegion IRegion.CreateInstance(IElement content)
            {
                return new RegionExample3 
                { 
                    _content = content,
                    _isInstance = true
                };
            }

            #endregion

            #region You must support at least this much initialization

            void IElement.Initialize(IInitializationData initializationData)
            {
                initializationData.HasElement(this);
                _content.Initialize(initializationData);
            }

            #endregion

            #region This example demonstrates simply wrapping another element and adding no new functionality

            private IElement _content;

            void IRegion.Populate(IElement content)
            {
                _content = content;
            }

            IWriteResult IRegion.WriteHtml(IRenderContext context, IDataScopeProvider scope, IElement content)
            {
                if (content == null) return WriteResult.Continue();

                var data = context.Data;
                context.SelectDataContext(scope.Id);

                var result = content.WriteHtml(context);

                context.Data = data;
                return result;
            }

            IWriteResult IRegion.WriteHead(IRenderContext context, IDataScopeProvider scope, bool includeChildren)
            {
                return includeChildren ? _content.WriteHead(context) : WriteResult.Continue();
            }

            IWriteResult IRegion.WriteInitializationScript(IRenderContext context, IDataScopeProvider scope, bool includeChildren)
            {
                return includeChildren ? _content.WriteInitializationScript(context) : WriteResult.Continue();
            }

            IWriteResult IRegion.WriteTitle(IRenderContext context, IDataScopeProvider scope, bool includeChildren)
            {
                return includeChildren ? _content.WriteTitle(context) : WriteResult.Continue();
            }

            IEnumerator<IElement> IElement.GetChildren()
            {
                return _content.AsEnumerable().GetEnumerator();
            }

            IWriteResult IElement.WriteStaticCss(ICssWriter writer)
            {
                return _content.WriteStaticCss(writer);
            }

            IWriteResult IElement.WriteStaticJavascript(IJavascriptWriter writer)
            {
                return _content.WriteStaticJavascript(writer);
            }

            IWriteResult IElement.WriteDynamicCss(ICssWriter writer, bool includeChildren)
            {
                return includeChildren ? _content.WriteDynamicCss(writer) : WriteResult.Continue();
            }

            IWriteResult IElement.WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren)
            {
                return includeChildren ? _content.WriteDynamicJavascript(writer) : WriteResult.Continue();
            }

            IWriteResult IElement.WriteInitializationScript(IRenderContext context, bool includeChildren)
            {
                return includeChildren ? _content.WriteInitializationScript(context) : WriteResult.Continue();
            }

            IWriteResult IElement.WriteTitle(IRenderContext context, bool includeChildren)
            {
                return includeChildren ? _content.WriteTitle(context) : WriteResult.Continue();
            }

            IWriteResult IElement.WriteHead(IRenderContext context, bool includeChildren)
            {
                return includeChildren ? _content.WriteHead(context) : WriteResult.Continue();
            }

            IWriteResult IElement.WriteHtml(IRenderContext context, bool includeChildren)
            {
                return includeChildren ? _content.WriteHtml(context) : WriteResult.Continue();
            }

            #endregion

            #region These methods do not need any implementation

            DebugElement IElement.GetDebugInfo()
            {
                return null;
            }

            DebugRegion IRegion.GetDebugInfo()
            {
                return null;
            }

            #endregion
        }
        
        public override IPackage Build(IFluentBuilder fluentBuilder)
        {
            // This is an example of how to define a new region that is like an
            // existing one but with a different name and has different properties.
            //
            // The new region will be registered with the fluent builder and its name
            // will be resolvable by other elements in the solution.
            //
            // This technique will work for any of the RegionExample1..RegionExample4
            // above, but will not work for RegionExample5
            fluentBuilder.BuildUpRegion(new RegionExample2(Dependencies.RegionDependenciesFactory))
                .Name("region2a")
                .BindTo<NewsItem>("breaking-news")
                .ClassNames("news", "breaking")
                .Build();

            // This is an example of how you can construct and register a fully custom
            // implementation of IRegion with the fluent builder
            var regionExample5 = new RegionExample5() as IRegion;
            regionExample5.Name = "region5";
            fluentBuilder.Register(regionExample5);

            return this;
        }
    }
}