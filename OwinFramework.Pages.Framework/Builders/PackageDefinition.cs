﻿using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class PackageDefinition : IPackageDefinition
    {
        private readonly IFluentBuilder _builder;
        private readonly INameManager _nameManager;
        private readonly Runtime.Package _package;

        public PackageDefinition(
            Runtime.Package package,
            IFluentBuilder builder,
            INameManager nameManager)
        {
            _package = package;
            _builder = builder;
            _nameManager = nameManager;
        }

        IPackageDefinition IPackageDefinition.Name(string name)
        {
            _package.Name = name;
            return this;
        }

        IPackageDefinition IPackageDefinition.NamespaceName(string namespaceName)
        {
            _package.NamespaceName = namespaceName;
            return this;
        }

        IPackageDefinition IPackageDefinition.Module(string moduleName)
        {
            _nameManager.AddResolutionHandler((nm, p) => 
                p.Module = nm.ResolveModule(moduleName),
                _package);
            return this;
        }

        IPackageDefinition IPackageDefinition.Module(IModule module)
        {
            _package.Module = module;
            return this;
        }

        IPackage IPackageDefinition.Build()
        {
            _builder.Register(_package);
            return _package;
        }
    }
}
