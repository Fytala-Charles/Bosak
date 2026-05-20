// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Adapts LINQ to XML (XDocument, XElement, XAttribute, etc.) to the IXdmNode interface
// SPECIAL NOTES        : Part of the XDocument node provider layer; adapts without copying into a proprietary DOM.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Fixed Prefix resolution and implemented namespace axis                                 |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Implemented ToXmlString for fn:serialize                                               |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Implemented BaseUri property for fn:base-uri and fn:document-uri                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Xml;


/// <summary>
/// Adapts LINQ to XML objects (<see cref="XObject"/>) to the <see cref="IXdmNode"/> interface.
/// Supports XDocument, XElement, XAttribute, XText, XCData, XComment, and XProcessingInstruction.
/// </summary>
public sealed class XDocumentNode : IXdmNode
{
    private static readonly ConditionalWeakTable<System.Xml.Linq.XDocument, Dictionary<XObject, long>> OrderMaps = new();

    private readonly XObject _node;
    private readonly bool _isNamespaceNode;
    private readonly XElement? _namespaceOwner;

    public XDocumentNode(XObject node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    private XDocumentNode(XAttribute declaration, XElement owner)
    {
        _node = declaration;
        _isNamespaceNode = true;
        _namespaceOwner = owner;
    }

    internal static void RegisterOrderMap(System.Xml.Linq.XDocument doc, Dictionary<XObject, long> map)
        => OrderMaps.AddOrUpdate(doc, map);

    // ------------------------------------------------------------------
    // Node metadata
    // ------------------------------------------------------------------

    public XdmNodeKind NodeKind => _isNamespaceNode ? XdmNodeKind.Namespace : GetNodeKind(_node);

    public string LocalName
    {
        get
        {
            if (_isNamespaceNode)
            {
                return _node is XAttribute attr && attr.Name.LocalName != "xmlns"
                    ? attr.Name.LocalName
                    : string.Empty;
            }
            return _node switch
            {
                XElement e => e.Name.LocalName,
                XAttribute a => a.Name.LocalName,
                XProcessingInstruction pi => pi.Target,
                _ => string.Empty
            };
        }
    }

    public string NamespaceUri => _isNamespaceNode
        ? string.Empty
        : _node switch
        {
            XElement e => e.Name.NamespaceName,
            XAttribute a => a.Name.NamespaceName,
            _ => string.Empty
        };

    public string Prefix => _isNamespaceNode
        ? string.Empty
        : _node switch
        {
            XElement e => e.GetPrefixOfNamespace(e.Name.Namespace) ?? string.Empty,
            XAttribute a => (a.Parent as XElement)?.GetPrefixOfNamespace(a.Name.Namespace) ?? string.Empty,
            _ => string.Empty
        };

    public string StringValue
    {
        get
        {
            if (_isNamespaceNode)
                return _node is XAttribute attr ? attr.Value : string.Empty;
            return _node switch
            {
                XElement e => e.Value,
                XAttribute a => a.Value,
                XText t => t.Value,
                XComment c => c.Value,
                XProcessingInstruction pi => pi.Data,
                System.Xml.Linq.XDocument d => d.Root?.Value ?? string.Empty,
                _ => string.Empty
            };
        }
    }

    public XdmValue TypedValue => XdmValue.FromString(StringValue);

    public bool IsSameNode(IXdmNode other)
        => other is XDocumentNode xn
           && ReferenceEquals(_node, xn._node)
           && _isNamespaceNode == xn._isNamespaceNode
           && ReferenceEquals(_namespaceOwner, xn._namespaceOwner);

    public long DocumentOrder
    {
        get
        {
            var doc = _node.Document;
            if (doc is null || !OrderMaps.TryGetValue(doc, out var map))
                return 0;
            return map.TryGetValue(_node, out var idx) ? idx : 0;
        }
    }

    public string BaseUri => _node.BaseUri ?? string.Empty;

    // ------------------------------------------------------------------
    // Tree navigation
    // ------------------------------------------------------------------

    public IXdmNode? Parent
    {
        get
        {
            if (_isNamespaceNode)
                return _namespaceOwner is not null ? new XDocumentNode(_namespaceOwner) : null;
            var parent = _node.Parent;
            return parent is not null ? new XDocumentNode(parent) : null;
        }
    }

    public IXdmNode? Document
    {
        get
        {
            var doc = _node.Document;
            return doc is not null ? new XDocumentNode(doc) : null;
        }
    }

    // ------------------------------------------------------------------
    // Children & Attributes
    // ------------------------------------------------------------------

    public XdmSequence Children(XdmNodeKind kind = XdmNodeKind.All)
    {
        if (_node is not XContainer container)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var child in container.Nodes())
        {
            var childKind = GetNodeKind(child);
            if (kind == XdmNodeKind.All || (kind & childKind) == childKind)
            {
                items.Add(XdmValue.FromNode(new XDocumentNode(child)));
            }
        }
        return MaterializedSequence.FromList(items);
    }

    public XdmSequence Attributes(string? localName = null, string? namespaceUri = null)
    {
        if (_node is not XElement element)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var attr in element.Attributes())
        {
            if (localName is not null && attr.Name.LocalName != localName)
                continue;
            if (namespaceUri is not null && attr.Name.NamespaceName != namespaceUri)
                continue;

            items.Add(XdmValue.FromNode(new XDocumentNode(attr)));
        }
        return MaterializedSequence.FromList(items);
    }

    // ------------------------------------------------------------------
    // Axes
    // ------------------------------------------------------------------

    public XdmSequence Axis(XdmAxis axis)
    {
        return axis switch
        {
            XdmAxis.Child => GetChildAxis(),
            XdmAxis.Descendant => GetDescendantAxis(),
            XdmAxis.DescendantOrSelf => GetDescendantOrSelfAxis(),
            XdmAxis.Parent => GetParentAxis(),
            XdmAxis.Ancestor => GetAncestorAxis(),
            XdmAxis.AncestorOrSelf => GetAncestorOrSelfAxis(),
            XdmAxis.Self => XdmSequence.Singleton(XdmValue.FromNode(this)),
            XdmAxis.Attribute => GetAttributeAxis(),
            XdmAxis.FollowingSibling => GetFollowingSiblingAxis(),
            XdmAxis.PrecedingSibling => GetPrecedingSiblingAxis(),
            XdmAxis.Following => GetFollowingAxis(),
            XdmAxis.Preceding => GetPrecedingAxis(),
            XdmAxis.Namespace => GetNamespaceAxis(),
            _ => XdmSequence.Empty
        };
    }

    // ------------------------------------------------------------------
    // Axis implementations
    // ------------------------------------------------------------------

    private XdmSequence GetChildAxis()
    {
        if (_node is not XContainer container)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var child in container.Nodes())
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(child)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetDescendantAxis()
    {
        var items = new List<XdmValue>();
        foreach (var desc in GetDescendants(_node))
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(desc)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetDescendantOrSelfAxis()
    {
        var items = new List<XdmValue> { XdmValue.FromNode(this) };
        foreach (var desc in GetDescendants(_node))
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(desc)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetParentAxis()
    {
        var parent = _node.Parent;
        return parent is not null
            ? XdmSequence.Singleton(XdmValue.FromNode(new XDocumentNode(parent)))
            : XdmSequence.Empty;
    }

    private XdmSequence GetAncestorAxis()
    {
        var items = new List<XdmValue>();
        var current = _node.Parent;
        while (current is not null)
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(current)));
            current = current.Parent;
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetAncestorOrSelfAxis()
    {
        var items = new List<XdmValue> { XdmValue.FromNode(this) };
        var current = _node.Parent;
        while (current is not null)
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(current)));
            current = current.Parent;
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetAttributeAxis()
    {
        if (_node is not XElement element)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var attr in element.Attributes())
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(attr)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetNamespaceAxis()
    {
        if (_node is not XElement element)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        var seen = new HashSet<string>();
        var current = element;

        while (current is not null)
        {
            foreach (var attr in current.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;

                string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                if (seen.Add(prefix))
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(attr, element)));
                }
            }
            current = current.Parent;
        }

        // The xml namespace is always implicitly in scope
        if (seen.Add("xml"))
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(
                new XAttribute(XNamespace.Xmlns + "xml", XNamespace.Xml.NamespaceName),
                element)));
        }

        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetFollowingSiblingAxis()
    {
        var parent = _node.Parent;
        if (parent is null)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        bool found = false;
        foreach (var sibling in parent.Nodes())
        {
            if (sibling == _node) { found = true; continue; }
            if (found)
                items.Add(XdmValue.FromNode(new XDocumentNode(sibling)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetPrecedingSiblingAxis()
    {
        var parent = _node.Parent;
        if (parent is null)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var sibling in parent.Nodes())
        {
            if (sibling == _node) break;
            items.Insert(0, XdmValue.FromNode(new XDocumentNode(sibling)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetFollowingAxis()
    {
        var items = new List<XdmValue>();
        var current = _node;

        while (true)
        {
            var parent = current.Parent;
            if (parent is null) break;

            bool found = false;
            foreach (var sibling in parent.Nodes())
            {
                if (sibling == current) { found = true; continue; }
                if (found)
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(sibling)));
                    AddDescendants(sibling, items);
                }
            }
            current = parent;
        }

        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetPrecedingAxis()
    {
        var items = new List<XdmValue>();
        var current = _node;

        while (true)
        {
            var parent = current.Parent;
            if (parent is null) break;

            var before = new List<XObject>();
            foreach (var sibling in parent.Nodes())
            {
                if (sibling == current) break;
                before.Add(sibling);
            }

            // Reverse document order: deepest descendants first, then sibling
            for (int i = before.Count - 1; i >= 0; i--)
            {
                var descs = GetDescendants(before[i]).ToList();
                for (int j = descs.Count - 1; j >= 0; j--)
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(descs[j])));
                }
                items.Add(XdmValue.FromNode(new XDocumentNode(before[i])));
            }

            current = parent;
        }

        return MaterializedSequence.FromList(items);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static XdmNodeKind GetNodeKind(XObject obj) => obj switch
    {
        System.Xml.Linq.XDocument => XdmNodeKind.Document,
        XElement => XdmNodeKind.Element,
        XAttribute => XdmNodeKind.Attribute,
        XText => XdmNodeKind.Text,
        XComment => XdmNodeKind.Comment,
        XProcessingInstruction => XdmNodeKind.ProcessingInstruction,
        _ => XdmNodeKind.None
    };

    private static IEnumerable<XObject> GetDescendants(XObject node)
    {
        if (node is not XContainer container)
            yield break;

        foreach (var child in container.Nodes())
        {
            yield return child;
            foreach (var desc in GetDescendants(child))
                yield return desc;
        }
    }

    private static void AddDescendants(XObject node, List<XdmValue> items)
    {
        if (node is not XContainer container)
            return;

        foreach (var child in container.Nodes())
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(child)));
            AddDescendants(child, items);
        }
    }

    public string ToXmlString()
    {
        if (_isNamespaceNode)
            return string.Empty;

        return _node switch
        {
            System.Xml.Linq.XDocument doc => doc.ToString(SaveOptions.DisableFormatting),
            XElement el => el.ToString(SaveOptions.DisableFormatting),
            XText t => System.Security.SecurityElement.Escape(t.Value) ?? t.Value,
            XComment c => $"<!--{c.Value}-->",
            XProcessingInstruction pi => $"<?{pi.Target} {pi.Data}?>",
            XAttribute a => a.Value,
            _ => _node.ToString() ?? string.Empty
        };
    }
}
