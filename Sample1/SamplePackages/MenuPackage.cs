﻿using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace Sample1.SamplePackages
{
    /// <summary>
    /// This demonstrates how to write a code only package. This package defines
    /// a layout called 'menu' that will display a list of 'MenuItem' objects
    /// from the data context.
    /// To use this package put the 'Menu' region into a layout and write a
    /// data provider to add a list of MenuItem objects to the
    /// data context.
    /// Note that adding the [IsPackage] attribute will make this package
    /// register automatically if you register the assembly that contains it, 
    /// which means that you can not override the namespace.
    /// If you want to override the namespace on a package that has the
    /// [IsPackage] attibute make sure that you register it manually before
    /// registering the assembly that contains it.
    /// </summary>
    [IsPackage("menu")]
    public class MenuPackage : OwinFramework.Pages.Framework.Runtime.Package
    {
        // I created and tested the CSS/Html for this package here:
        // https://www.w3schools.com/code/tryit.asp?filename=FSITSDF3RKHE

        public MenuPackage(IPackageDependenciesFactory dependencies)
            : base(dependencies) { }

        public class MenuItem
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public IList<MenuItem> SubMenu { get; set; }

            public override string ToString()
            {
                return 
                    SubMenu == null 
                    ? Name
                    : Name + " with " + SubMenu.Count + " submenus";
            }
        }

        /// <summary>
        /// This is how you would write a data provider that is configured in code. In
        /// this case the configuration is in the constructor
        /// </summary>
        private class SubMenuDataProviderBare: DataProvider
        {
            public SubMenuDataProviderBare(IDataProviderDependenciesFactory dependencies) 
                : base(dependencies) 
            {
                DataConsumer.HasDependency<MenuItem>();
                Add<IList<MenuItem>>("submenu");
            }

            protected override void Supply(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
            {
                renderContext.Trace(() => "submenu provider getting menu item from " + dataContext);
                var parent = dataContext.Get<MenuItem>();
                if (parent.SubMenu == null)
                    renderContext.Trace(() => "supply submenu for menu '" + parent.Name + "' with no submenu to " + dataContext);
                else
                    renderContext.Trace(() => "supply submenu for menu '" + parent.Name + "' with " + parent.SubMenu.Count + " sub-menu items to " + dataContext);
                dataContext.Set(parent.SubMenu, "submenu");
            }
        }

        /// <summary>
        /// This is how you would write a data provider where the data it needs
        /// and the data it supplies are defined using attributes attached to the class
        /// </summary>
        [NeedsData(typeof(MenuItem))]
        [SuppliesData(typeof(IList<MenuItem>), "submenu")]
        private class SubMenuDataProviderDecorated : DataProvider
        {
            public SubMenuDataProviderDecorated(IDataProviderDependenciesFactory dependencies)
                : base(dependencies) { }

            protected override void Supply(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
            {
                var parent = dataContext.Get<MenuItem>();
                dataContext.Set(parent.SubMenu, "submenu");
            }
        }

        /// <summary>
        /// This is an example of writing a component that needs a MenuItem to work with and
        /// writes html into the body of the page
        /// </summary>
        private class MenuItemComponent : Component
        {
            public MenuItemComponent(IComponentDependenciesFactory dependencies) 
                : base(dependencies) 
            {
                PageAreas = new []{ PageArea.Body };
            }

            public override IWriteResult WritePageArea(
                IRenderContext context, 
                PageArea pageArea)
            {
                if (pageArea == PageArea.Body)
                {
                    context.Trace(() => "menu component getting menu item from " + context.Data);

                    var menuItem = context.Data.Get<MenuItem>();

                    context.Trace(() => "rendering menu item '" + menuItem.Name + "'");

                    var url = string.IsNullOrEmpty(menuItem.Url) ? "javascript:void(0);" : menuItem.Url;
                    context.Html.WriteElementLine("a", menuItem.Name, "href", url);
                }
                return WriteResult.Continue();
            }
        }

        /// <summary>
        /// This is an example if a component that just deploys static assets
        /// </summary>
        [DeployCss("ul.{ns}_menu", "list-style-type: none; overflow: hidden; white-space: nowrap;", 1)]
        [DeployCss("li.{ns}_option", "display: inline-block;", 2)]
        [DeployCss("li.{ns}_option a, a.{ns}_option", "display: inline-block; text-decoration: none;", 3)]
        [DeployCss("div.{ns}_dropdown", "display: none; position: absolute; overflow: hidden; z-index: 1;", 4)]
        [DeployCss("div.{ns}_dropdown a", "text-decoration: none; display: block; text-align: left", 5)]
        [DeployCss("li.{ns}_option:hover div.{ns}_dropdown", "display: block;", 6)]
        public class MenuStyles
        { }

        /// <summary>
        /// This is an example if a component that deploys static assets and depends on other components
        /// </summary>
        [DeployCss("ul.{ns}_menu", "margin: 0; padding: 0; background-color: #333", 1)]
        [DeployCss("li.{ns}_option a", "color: white; text-align: center; padding: 14px 16px;", 2)]
        [DeployCss("li.{ns}_option a:hover, li.{ns}_menu-option:hover a.{ns}_menu-option", "background-color: red", 3)]
        [DeployCss("div.{ns}_dropdown a:hover", "background-color: #f1f1f1;", 4)]
        [DeployCss("div.{ns}_dropdown", "background-color: #f9f9f9; min-width: 160px; box-shadow: 0px 8px 16px 0px rgba(0,0,0,0.2);", 5)]
        [DeployCss("div.{ns}_dropdown a", "color: black; padding: 12px 16px;", 6)]
        public class MenuStyle1
        { }

        /// <summary>
        /// This is an example if a component that deploys static assets and depends on other components
        /// </summary>
        [DeployCss("ul.{ns}_menu", "margin: 0; padding: 0; background-color: black", 1)]
        [DeployCss("li.{ns}_option a", "color: #ddd; text-align: left; padding: 14px 10px;", 2)]
        [DeployCss("li.{ns}_option a:hover, li.{ns}_menu-option:hover a.{ns}_menu-option", "color: #f8991d", 3)]
        [DeployCss("div.{ns}_dropdown a:hover", "background-color: #f1f1f1;", 4)]
        [DeployCss("div.{ns}_dropdown", "background-color: #666699; min-width: 160px; box-shadow: 0px 8px 16px 0px rgba(0,0,0,0.2);", 5)]
        [DeployCss("div.{ns}_dropdown a", "color: white; padding: 12px 16px;", 6)]
        public class MenuStyle2
        { }

        public override IPackage Build(IFluentBuilder builder)
        {
            // This component outputs CSS that makes the menu work as a menu
            builder.BuildUpComponent(new MenuStyles())
                .Name("menuStyles")
                .Build();

            // This component outputs CSS that defines the menu appearence
            builder.BuildUpComponent(new MenuStyle1())
                .Name("menuStyle1")
                .NeedsComponent("menuStyles")
                .Build();

            // This component outputs CSS that defines the menu appearence
            builder.BuildUpComponent(new MenuStyle2())
                .Name("menuStyle2")
                .NeedsComponent("menuStyles")
                .Build();

            // This component displays a main menu item
            var mainMenuItemComponent = builder.BuildUpComponent(
                new MenuItemComponent(Dependencies.ComponentDependenciesFactory))
                .BindTo<MenuItem>()
                .Build();

            // This component displays a submenu item
            var subMenuItemComponent = builder.BuildUpComponent(
                new MenuItemComponent(Dependencies.ComponentDependenciesFactory))
                .BindTo<MenuItem>("submenu")
                .Build();

            // This data provider extracts sub-menu items from the current menu item
            // using a custom data provider class
            var subMenuDataProvider1 = builder.BuildUpDataProvider(
                new SubMenuDataProviderBare(Dependencies.DataProviderDependenciesFactory))
                .Build();

            // This data provider extracts sub-menu items from the current menu item
            // using a custom data provider class decorated with attributes
            var subMenuDataProvider2 = builder.BuildUpDataProvider(
                new SubMenuDataProviderDecorated(Dependencies.DataProviderDependenciesFactory))
                .Build();

            // This data provider extracts sub-menu items from the current menu item
            // using fluent syntax. No custom class is needed in this case
            var subMenuDataProvider3 = builder.BuildUpDataProvider()
                .BindTo<MenuItem>()
                .Provides<IList<MenuItem>>((rc, dc, d) => 
                    {
                        var menuItem = dc.Get<MenuItem>();
                        dc.Set(menuItem.SubMenu, "submenu");
                    },
                    "submenu")
                .Build();

            // This region is a container for the options on the main menu
            var mainMenuItemRegion = builder.BuildUpRegion()
                .BindTo<MenuItem>()
                .Tag("div")
                .Component(mainMenuItemComponent)
                .Build();

            // This region is a container for the drop down menu items. It
            // renders one menu item component for each menu item in the sub-menu
            var dropDownMenuRegion = builder.BuildUpRegion()
                .Tag("div")
                .ClassNames("{ns}_dropdown")
                .ForEach<MenuItem>("submenu", null, null, "submenu")
                .Component(subMenuItemComponent)
                .Build();

            // This layout defines the main menu option and the sub-menu that
            // drops down wen the main menu option is tapped
            var menuOptionLayout = builder.BuildUpLayout()
                .Tag("li")
                .ClassNames("{ns}_option")
                .RegionNesting("head,submenu")
                .Region("head", mainMenuItemRegion)
                .Region("submenu", dropDownMenuRegion)
                .DataProvider(subMenuDataProvider1)
                .Build();

            // This region is the whole menu structure with top level menu 
            // options and sub-menus beneath each option
            builder.BuildUpRegion()
                .Name("menu")
                .Tag("ul")
                .NeedsComponent("menuStyles")
                .ClassNames("{ns}_menu")
                .ForEach<MenuItem>()
                .Layout(menuOptionLayout)
                .Build();

            return this;
        }

    }
}
