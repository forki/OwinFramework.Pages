﻿using Microsoft.Owin;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageDependencies: IPageDependencies
    {
        public IRenderContext RenderContext { get; private set; }
        public IAssetManager AssetManager { get; private set; }
        public INameManager NameManager { get; private set; }

        public PageDependencies(
            IRenderContext renderContext,
            IAssetManager assetManager,
            INameManager nameManager)
        {
            RenderContext = renderContext;
            AssetManager = assetManager;
            NameManager = nameManager;
        }

        public IPageDependencies Initialize(IOwinContext context)
        {
            RenderContext.Initialize(context);
            return this;
        }

        public void Dispose()
        {
            RenderContext.Dispose();
        }
    }
}
