﻿using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class LayoutBuilder : ILayoutBuilder
    {
        private readonly INameManager _nameManager;

        public LayoutBuilder(
                INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        ILayoutDefinition ILayoutBuilder.Layout()
        {
            return new LayoutDefinition(_nameManager);
        }

        private class LayoutDefinition: ILayoutDefinition
        {
            private readonly INameManager _nameManager;
            private readonly BuiltLayout _layout;

            private RegionSet _regionSet;
            private Dictionary<string, object> _regionElements;
            private Dictionary<string, object> _regionLayouts;
            private Dictionary<string, object> _regionComponents;

            private string _tag;
            private string[] _classNames;
            private string _style;

            private string _nestingTag = "div";
            private string[] _nestedClassNames;
            private string _nestedStyle;

            private class RegionSet
            {
                public List<object> Elements;

                public static RegionSet Parse(string regions, ref int position)
                {
                    var result = new RegionSet
                        {
                            Elements = new List<object>()
                        };

                    var start = position;
                    while (position < regions.Length)
                    {
                        switch (regions[position])
                        {
                            case '(':
                                result.Elements.AddRange(BuildList(regions, start, position));
                                position++;
                                result.Elements.Add(Parse(regions, ref position));
                                position++;
                                start = position;
                                break;
                            case ')':
                                result.Elements.AddRange(BuildList(regions, start, position));
                                return result;
                            default:
                                position++;
                                break;
                        }
                    }
                    result.Elements.AddRange(BuildList(regions, start, position));
                    return result;
                }

                private static List<object> BuildList(string regions, int start, int end)
                {
                    var result = new List<object>();
                    if (end > start)
                    {
                        var regionNames = regions.Substring(start, end - start);
                        result.AddRange(regionNames.Split(',').Select(s => s.Trim()));
                    }
                    return result;
                }
            }

            public LayoutDefinition(
                INameManager nameManager)
            {
                _nameManager = nameManager;
                _layout = new BuiltLayout();
                _regionElements = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _regionLayouts = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _regionComponents = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public ILayoutDefinition Name(string name)
            {
                _layout.Name = name;
                return this;
            }

            public ILayoutDefinition PartOf(IPackage package)
            {
                _layout.Package = package;
                return this;
            }

            public ILayoutDefinition PartOf(string packageName)
            {
                _layout.Package = _nameManager.ResolvePackage(packageName);

                if (_layout.Package == null)
                    throw new LayoutBuilderException(
                        "Package names must be registered before layouts can refer to them. " +
                        "There is no registered package '" + packageName + "'");
                return this;
            }

            public ILayoutDefinition RegionNesting(string regionNesting)
            {
                var position = 0;
                _regionSet = RegionSet.Parse(regionNesting, ref position);
                return this;
            }

            public ILayoutDefinition AssetDeployment(AssetDeployment assetDeployment)
            {
                _layout.AssetDeployment = assetDeployment;
                return this;
            }

            public ILayoutDefinition Region(string regionName, IRegion region)
            {
                _regionElements[regionName] = region;
                return this;
            }

            public ILayoutDefinition Region(string regionName, string name)
            {
                _regionElements[regionName] = name;
                return this;
            }

            public ILayoutDefinition Component(string regionName, IComponent component)
            {
                _regionComponents[regionName] = component;
                return this;
            }

            public ILayoutDefinition Component(string regionName, string componentName)
            {
                _regionComponents[regionName] = componentName;
                return this;
            }

            public ILayoutDefinition Layout(string regionName, ILayout layout)
            {
                _regionLayouts[regionName] = layout;
                return this;
            }

            public ILayoutDefinition Layout(string regionName, string layoutName)
            {
                _regionLayouts[regionName] = layoutName;
                return this;
            }

            public ILayoutDefinition Tag(string tagName)
            {
                _tag = tagName;
                return this;
            }

            public ILayoutDefinition ClassNames(params string[] classNames)
            {
                _classNames = classNames;
                return this;
            }

            public ILayoutDefinition Style(string style)
            {
                _style = style;
                return this;
            }

            public ILayoutDefinition NestingTag(string tagName)
            {
                _nestingTag = tagName;
                return this;
            }

            public ILayoutDefinition NestedClassNames(params string[] classNames)
            {
                _nestedClassNames = classNames;
                return this;
            }

            public ILayoutDefinition NestedStyle(string style)
            {
                _nestedStyle = style;
                return this;
            }

            ILayoutDefinition ILayoutDefinition.BindTo<T>()
            {
                return this;
            }

            ILayoutDefinition ILayoutDefinition.BindTo(Type dataType)
            {
                return this;
            }

            ILayoutDefinition ILayoutDefinition.DataContext(string dataContextName)
            {
                return this;
            }

            public ILayout Build()
            {
                _nameManager.Register(_layout);
                _nameManager.AddResolutionHandler(() =>
                    {
                        _layout.Elements = new List<IElement>();

                        foreach (var regionName in _regionComponents.Keys)
                        {
                            var componentRef = _regionComponents[regionName];
                            var componentName = componentRef as string;
                            if (componentName != null)
                                _regionComponents[regionName] = _nameManager.ResolveComponent(componentName);
                        }

                        foreach (var regionName in _regionLayouts.Keys)
                        {
                            var layoutRef = _regionLayouts[regionName];
                            var layoutName = layoutRef as string;
                            if (layoutName != null)
                                _regionLayouts[regionName] = _nameManager.ResolveLayout(layoutName);
                        }

                        WriteOpeningTag();
                        AddRegionSet(_regionSet);
                        WriteClosingTag();
                    });
                return _layout;
            }

            private void AddRegionSet(RegionSet rs)
            {
                if (rs != null && rs.Elements != null)
                {
                    foreach (var element in rs.Elements)
                    {
                        var regionName = element as string;
                        var childRegionSet = element as RegionSet;

                        if (regionName != null)
                        {
                            if (!_regionElements.ContainsKey(regionName))
                            {
                                _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteElement("p", "Layout does not have a '" + regionName + "' region") });
                                continue;
                            }

                            var regionElement = _regionElements[regionName];
                            var region = regionElement as IRegion;
                            var regionElementName = (string)regionElement;

                            if (region == null && regionElementName != null)
                            {
                                region = _nameManager.ResolveRegion(regionElementName, _layout.Package);
                                if (region == null)
                                    _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteElement("p", "Unknown region element '" + regionElementName + "'") });
                            }

                            if (region != null)
                            {
                                if (_regionComponents.ContainsKey(regionName))
                                {
                                    _layout.Elements.Add(region.Wrap((IComponent)_regionComponents[regionName]));
                                }
                                else if (_regionLayouts.ContainsKey(regionName))
                                {
                                    _layout.Elements.Add(region.Wrap((ILayout)_regionLayouts[regionName]));
                                }
                                else
                                    _layout.Elements.Add(region);
                            }
                        }
                        else if (childRegionSet != null)
                        {
                            WriteNestingOpeningTag();
                            AddRegionSet(childRegionSet);
                            WriteNestingClosingTag();
                        }
                    }
                }
            }

            private void WriteOpeningTag()
            {
                if (!string.IsNullOrEmpty(_tag))
                {
                    var tagAttributes = new List<string>();
                    if (!string.IsNullOrEmpty(_style))
                    {
                        tagAttributes.Add("style");
                        tagAttributes.Add(_style);
                    }
                    if (_classNames != null && _classNames.Length > 0)
                    {
                        tagAttributes.Add("class");
                        tagAttributes.Add(string.Join(" ", _classNames));
                    }
                    _layout.Elements.Add(new StaticHtmlElement {WriteAction = w => w.WriteOpenTag(_tag, tagAttributes.ToArray())});
                }
            }

            private void WriteClosingTag()
            {
                if (!string.IsNullOrEmpty(_tag))
                    _layout.Elements.Add(new StaticHtmlElement {WriteAction = w => w.WriteCloseTag(_tag)});
            }

            private void WriteNestingOpeningTag()
            {
                if (!string.IsNullOrEmpty(_nestingTag))
                {
                    var tagAttributes = new List<string>();
                    if (!string.IsNullOrEmpty(_nestedStyle))
                    {
                        tagAttributes.Add("style");
                        tagAttributes.Add(_nestedStyle);
                    }
                    if (_nestedClassNames != null && _nestedClassNames.Length > 0)
                    {
                        tagAttributes.Add("class");
                        tagAttributes.Add(string.Join(" ", _nestedClassNames));
                    }
                    _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteOpenTag(_nestingTag, tagAttributes.ToArray()) });
                }
            }

            private void WriteNestingClosingTag()
            {
                if (!string.IsNullOrEmpty(_nestingTag))
                    _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteCloseTag(_nestingTag) });
            }
        }

        private class StaticHtmlElement: Element
        {
            public Action<IHtmlWriter> WriteAction;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                WriteAction(renderContext.Html);
                return WriteResult.Continue();
            }
        }

        private class BuiltLayout : Layout
        {
            public List<IElement> Elements;
            public AssetDeployment AssetDeployment;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                foreach (var element in Elements)
                    element.WriteHtml(renderContext, dataContext);

                return WriteResult.Continue();
            }
        }
    }
}
