﻿using System;
using System.IO;
using System.Reflection;
using Ioc.Modules;
using Microsoft.Owin;
using Ninject;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using Sample1.Pages;
using Urchin.Client.Sources;
using OwinFramework.Pages.Core;
using Sample1;

[assembly: OwinStartup(typeof(Startup))]

namespace Sample1
{
    public class Startup
    {
        private static IDisposable _configurationFileSource;

        public void Configuration(IAppBuilder app)
        {
            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            var hostingEnvironment = ninject.Get<IHostingEnvironment>();
            var configFile = new FileInfo(hostingEnvironment.MapPath("config.json"));
            _configurationFileSource = ninject.Get<FileSource>().Initialize(configFile, TimeSpan.FromSeconds(5));

            var config = ninject.Get<IConfiguration>();

            var builder = ninject.Get<IBuilder>().EnableTracing();

            builder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample1/pages");
            builder.Register(ninject.Get<OwinFramework.NotFound.NotFoundMiddleware>()).ConfigureWith(config, "/sample1/notFound");
            builder.Register(ninject.Get<OwinFramework.Documenter.DocumenterMiddleware>()).ConfigureWith(config, "/sample1/documenter").RunFirst();

            app.UseBuilder(builder);

            var router = ninject.Get<IRequestRouter>();
            router.Register(new FullCustomPage(), new FilterByPath("/pages/*.html"));
        }
    }
}
