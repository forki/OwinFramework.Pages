﻿using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Restful.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Service : IService, IDebuggable
    {
        public ElementType ElementType { get { return ElementType.Service; } }

        public string Name { get; set; }
        public IPackage Package { get; set; }
        public string RequiredPermission { get; set; }
        public bool AllowAnonymous { get; set; }
        public Func<IOwinContext, bool> AuthenticationFunc { get { return null; } }
        string IRunable.CacheCategory { get; set; }
        CachePriority IRunable.CachePriority { get; set; }

        public Service(IServiceDependenciesFactory serviceDependenciesFactory)
        {
        }

        public void Initialize()
        {
        }

        public Task Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            throw new NotImplementedException();
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugService() as T;
        }

    }
}
