﻿using System;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleBuilder: IModuleBuilder
    {
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IModuleDependenciesFactory _moduleDependenciesFactory;

        public ModuleBuilder(
            IModuleDependenciesFactory moduleDependenciesFactory,
            IFluentBuilder fluentBuilder)
        {
            _moduleDependenciesFactory = moduleDependenciesFactory;
            _fluentBuilder = fluentBuilder;
        }

        IModuleDefinition IModuleBuilder.Module(Type declaringType)
        {
            return new ModuleDefinition(declaringType, _moduleDependenciesFactory, _fluentBuilder);
        }

    }
}
