﻿using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building website pages
    /// </summary>
    public interface IPageDefinition
    {
        /// <summary>
        /// Gives a name to the page
        /// </summary>
        IPageDefinition Name(string name);

        /// <summary>
        /// Sets the title for this page
        /// </summary>
        /// <param name="title"></param>
        IPageDefinition Title(string title);

        /// <summary>
        /// Sets the title for this page
        /// </summary>
        /// <param name="titleFunc">A delegate that will calculate the 
        /// title for this page</param>
        IPageDefinition Title(Func<IRenderContext, IDataContext, string> titleFunc);

        /// <summary>
        /// Specifies that this page is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this page is
        /// part of</param>
        IPageDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this page is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// page is part of</param>
        IPageDefinition PartOf(string packageName);

        /// <summary>
        /// Sets the css style of the body tag
        /// </summary>
        /// <param name="cssStyle">A valid css style definition, for example "margin: 10;"</param>
        /// <returns></returns>
        IPageDefinition BodyStyle(string cssStyle);

        /// <summary>
        /// Overrides the default asset deployment scheme for this page
        /// </summary>
        IPageDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Specifies the module that this page will be deployed with
        /// </summary>
        IPageDefinition Module(IModule module);

        /// <summary>
        /// Specifies the name of the module that this page will be deployed with
        /// </summary>
        IPageDefinition Module(string moduleName);

        /// <summary>
        /// Specifies the relative path to this page on the website
        /// </summary>
        /// <param name="path">The URL path to this page</param>
        IPageDefinition Path(string path);

        /// <summary>
        /// Specifies the relative path to this page on the website
        /// </summary>
        /// <param name="methods">The http methods to route to this page</param>
        IPageDefinition Methods(params Methods[] methods);

        /// <summary>
        /// Specifies the relative path to this page on the website
        /// </summary>
        /// <param name="filter">Serve this page for requests that match this filter</param>
        /// <param name="priority">Filters are evaluated from highest to lowest priority</param>
        IPageDefinition RequestFilter(IRequestFilter filter, int priority = 0);

        /// <summary>
        /// Defaines the layout of this page. If no layout is specified
        /// then the page will have the default layout which has one unanmed
        /// region
        /// </summary>
        IPageDefinition Layout(ILayout layout);

        /// <summary>
        /// Defines the name of the layout of this page. If no layout is specified
        /// then the page will have the default layout which has one unanmed
        /// region
        /// </summary>
        IPageDefinition Layout(string name);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a specific component
        /// </summary>
        IPageDefinition Component(string regionName, IComponent component);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a named component
        /// </summary>
        IPageDefinition Component(string regionName, string componentName);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a specific layout
        /// </summary>
        IPageDefinition RegionLayout(string regionName, ILayout layout);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a named layout
        /// </summary>
        IPageDefinition RegionLayout(string regionName, string layoutName);

        /// <summary>
        /// Builds the page
        /// </summary>
        IPage Build();
    }
}
