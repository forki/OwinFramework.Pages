using OwinFramework.Pages.Core.Debug;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class PageDrawing: DrawingElement
    {
        public PageDrawing(DebugSvgDrawing drawing, DebugPage debugPage)
        {
            LeftMargin = 20;
            RightMargin = 20;
            TopMargin = 20;
            BottomMargin = 20;

            var page = new RectangleDrawing { CssClass = "page" };
            AddChild(page);

            var text = new TextDrawing();
            text.Text.Add("Page '" + debugPage.Name + "'");

            if (debugPage.Routes != null)
            {
                foreach (var route in debugPage.Routes)
                    text.Text.Add(route.Route);
            }

            text.CalculateSize();
            page.AddChild(text);

            if (debugPage.Layout != null)
            {
                var layout = new LayoutDrawing(drawing, debugPage.Layout);
                layout.Top = text.Top + text.Height + 8;
                page.AddChild(layout);
            }
        }

        public override SvgElement Draw(SvgDocument document)
        {
            CalculateSize();

            var group = base.Draw(document);
            document.Children.Add(@group);
            return @group;
        }
    }
}