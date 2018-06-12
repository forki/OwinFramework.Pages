﻿using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;

/* 
 * This page demonstrates how to define page elements declaratively. Notice how the
 * classes do no have to have any implementation, have no base class and do not
 * implement any interfaces. The page can entirely defined using attributes only.
 * 
 * We do NOT recommend building your whole website this way, it is not very maintainable,
 * but this is a good technique to use for adding a few very simple elements
 * where writing a class and overriding methods is overkill.
 * 
 * This technique is very useful for defining modules and layouts, and maybe regions
 * if they are simple and not too many. Note that you can always change your mind later
 * and re-define your elements using a class or template without changing any of the
 * places where they are used.
 */

namespace Sample1.SamplePages
{
    /*
     * A package defines a namespace. These are used to import 3rd party libraries
     * and avoid naming conflicts. You can change the namespace of an imported
     * package within your application
     */

    /// <summary>
    /// Defines a package called 'Application' in which all the JavaScript methods
    /// and css classes will be in the 'sample1' namespace
    /// </summary>
    [IsPackage("Application", "sample1")]
    internal class ApplicationPackage { }

    /*
     * A module is a deployment container. All the JavaScript and css
     * for the module will be combined in one file. By default you will end 
     * up with one JavaScript and one css file per module, but you can change
     * this for each module, and you can override this behaviour on each
     * element.
     */

    /// <summary>
    /// Defines a module that contains all of the scripts and styles for 
    /// elements that are related to the site navigation
    /// </summary>
    [IsModule("Navigation", AssetDeployment.PerModule)]
    internal class NavigationModule { }

    /// <summary>
    /// Defines a module that contains all of the scripts and styles for 
    /// elements that are related to the site content
    /// </summary>
    [IsModule("Content", AssetDeployment.PerModule)]
    internal class ContentModule { }

    /*
     * Components are the lowest level in the element heirachy. They define
     * the html that is rendered when the page is requested. Regions and layouts
     * also render html, but this is structural (non-visual)
     */

    [IsComponent("header.mainMenu")]
    [PartOf("Application")]
    [RenderHtml("menu.main", "<p>This is where the main menu html goes</p>")]
    internal class MainMenuComponent { }

    [IsComponent("footer.standard")]
    [PartOf("Application")]
    [RenderHtml("footer.standard", "<p>This is where the html for the page footer goes</p>")]
    internal class StandardFooterComponent { }

    /*
     * A region is a container for a single component or layout. The container can 
     * use html, JavaScript and css to define its behaviour, for example the size, 
     * scroll bars, content overflow, visibiliy and position on the page.
     */

    /// <summary>
    /// Defines a region called 'main.header'
    /// </summary>
    [IsRegion("main.header")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("div", "", "header")]
    internal class MainHeaderRegion { }

    /// <summary>
    /// Defines a region called 'body'
    /// </summary>
    [IsRegion("body")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("div", "", "body")]
    internal class BodyRegion { }

    /// <summary>
    /// Defines a region called 'main.footer'
    /// </summary>
    [IsRegion("main.footer")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("div", "", "footer")]
    internal class MainFooterRegion { }

    /// <summary>
    /// Defines a region called '2col.vertical.fixed.left'
    /// </summary>
    [IsRegion("2col.vertical.fixed.left")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("div", "", "left")]
    internal class LeftRegion { }

    /// <summary>
    /// Defines a region called '2col.vertical.fixed.right'
    /// </summary>
    [IsRegion("2col.vertical.fixed.right")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("div", "", "right")]
    internal class RightRegion { }

    /* <summary>
     * A layout is an arrangement of regions. Regions can be grouped
     * inside containers. For example a layout could define a header bar
     * main body and footer bar.
     * Each page has a layout that defines how the page content is arranged.
     * You can also place a layout inside of a region to create a nested
     * structure of layouts within layouts.
     * The layout can be configured to contain certain content by defult
     * in each region but this can be overriden in each place where the
     * layout is used. For example you can use the exact same layout for
     * every page on your website where the contents of the header and
     * footer region are the default on every page but the main region
     * is overriden on each page to make every page unique, but every page
     * have the same header and footer.
     */

    /// <summary>
    /// Defines a layout called 'main' that has three regions. The regions
    /// are rendered in the order 'header', 'body' then 'footer'. By default
    /// the 'header' region contains the 'main.header' region, the 'body' region
    /// contains the 'body' region and the 'footer' region contains 'main.footer'.
    /// The contents of each region can be overriden for each instance of this layout
    /// </summary>
    [IsLayout("main", "header(body,footer)")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [UsesRegion("header", "main.header")]
    [UsesRegion("body", "body")]
    [UsesRegion("footer", "main.footer")]
    internal class MainLayout { }

    /// <summary>
    /// Defines the layout of the 'body' region for the home page
    /// </summary>
    [IsLayout("homePage", "left.main")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("div", "", "2col.vertical.fixed")]
    [ChildContainer(null)]
    [UsesRegion("left", "2col.vertical.fixed.left")]
    [UsesRegion("main", "2col.vertical.fixed.right")]
    internal class HomePageLayout { }

    /*
     * Pages are rendered in response to requests. In order to see your pages
     * you have to define a route for them. Routes can include wildcards and
     * you can attach multiple [Route()] attrinutes.
     * Every page needs a layout, and usually at least one region of the layout 
     * is overriden to make this page unique.
     * You can create self-documentation for your website by adding attributes 
     * like [Description()], [Option()], and [Example()]
     */

    /// <summary>
    /// Defines a page that is rendered in response to requets for '/home.html'
    /// Uses the 'main' layout but changes the contents of each region.
    /// </summary>
    [IsPage]
    [Description("<p>This is an example of how to add a page declatively using attributes</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for the home page</p>")]
    [Option(OptionType.Header, "Accept", "Must contain text/html, which is only available response format")]
    [Example("<a href='/home.html'>/home.html</a>")]
    [PartOf("Application")]
    [UsesLayout("main")]
    [Route("/home.html", Methods.Get)]
    [PageTitle("Sample website")]
    [Style("font-size: 18px;")]
    [RegionComponent("header", "header.mainMenu")]
    [RegionLayout("body", "homePage")]
    [RegionComponent("footer", "footer.standard")]
    internal class HomePage { }
}