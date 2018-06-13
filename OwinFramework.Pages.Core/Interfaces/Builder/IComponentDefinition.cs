﻿using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building components. A component:
    /// * Has an optional name so that is can be referenced
    /// * Can override the default asset deployment mechanism
    /// * Can bind to one or more data objects
    /// * Outputs content into various parts of the page (head, body etc)
    /// * Generates unique asset names within the namespace of its package
    /// * Can have dependencies on other components
    /// </summary>
    public interface IComponentDefinition
    {
        /// <summary>
        /// Sets the name of the component so that it can be referenced
        /// by other elements
        /// </summary>
        IComponentDefinition Name(string name);

        /// <summary>
        /// Specifies that this component is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this component is
        /// part of</param>
        /// <returns></returns>
        IComponentDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this component is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// component is part of</param>
        /// <returns></returns>
        IComponentDefinition PartOf(string packageName);

        /// <summary>
        /// Specifies that this component is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this component is deployed in</param>
        IComponentDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// layout is deployed in</param>
        IComponentDefinition DeployIn(string moduleName);

        /// <summary>
        /// Overrides the default asset deployment scheme for this component
        /// </summary>
        IComponentDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this component binds to.
        /// Provides context for data binding expressions within the component</typeparam>
        IComponentDefinition BindTo<T>() where T: class;

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        IComponentDefinition BindTo(Type dataType);

        /// <summary>
        /// Specifies that this component has a dependency on a specifc data context.
        /// You can call this multiple times to add more dependencies
        /// </summary>
        /// <param name="dataContextName">The name of the context handler that
        /// must be executed before rendering this component</param>
        IComponentDefinition DataContext(string dataContextName);

        /// <summary>
        /// Tells the component to render some html
        /// </summary>
        /// <param name="assetName">The name of the asset must uniquely identify a 
        /// text asset. The asset manager can be used to provdide localized verions</param>
        /// <param name="html">The html to render in the default locale</param>
        IComponentDefinition Render(string assetName, string html);

        /// <summary>
        /// Builds the module
        /// </summary>
        IComponent Build();
    }
}
