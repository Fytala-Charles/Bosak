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
//                      | Charles Korthout | 0.5   | 27-05-2026     | Lazy document order computation for proper node sorting                                |
//                      | Charles Korthout | 0.6   | 30-05-2026     | Fixed StringValue for XDocument without root element (uses all text node children)     |
//                      | Charles Korthout | 0.7   | 30-05-2026     | Added synthetic document wrapper for mixed-content document nodes                      |
//                      | Charles Korthout | 0.8   | 10-06-2026     | Fixed GetXPathParent for namespace nodes: parent axis returns _namespaceOwner         |
//                      | Charles Korthout | 0.9   | 10-06-2026     | ParentlessOrderMaps for stable document order on detached/copied element trees         |
//                      | Charles Korthout | 1.0   | 11-06-2026     | GetNamespaceAxis skips empty-URI declarations (xmlns="") and stops at inheritance barriers |
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
    private static readonly ConditionalWeakTable<XElement, Dictionary<XObject, long>> ParentlessOrderMaps = new();

    private readonly XObject _node;
    private readonly bool _isNamespaceNode;
    private readonly XElement? _namespaceOwner;

    public XDocumentNode(XObject node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>Gets the underlying LINQ to XML node.</summary>
    public XObject UnderlyingObject => _node;

    private XDocumentNode(XAttribute declaration, XElement owner)
    {
        _node = declaration;
        _isNamespaceNode = true;
        _namespaceOwner = owner;
    }

    /// <summary>
    /// Creates an <see cref="IXdmNode"/> representing a namespace node
    /// from an <see cref="XAttribute"/> namespace declaration.
    /// </summary>
    public static XDocumentNode CreateNamespaceNode(XAttribute declaration, XElement owner)
        => new XDocumentNode(declaration, owner);

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
                System.Xml.Linq.XDocument d => GetSyntheticWrapper(d) is { } wrapper
                    ? string.Concat(wrapper.Nodes().OfType<XText>().Select(t => t.Value))
                    : (d.Root != null ? d.Root.Value : string.Concat(d.Nodes().OfType<XText>().Select(t => t.Value))),
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
            if (doc is not null)
            {
                if (!OrderMaps.TryGetValue(doc, out var map))
                {
                    map = ComputeDocumentOrder(doc);
                    OrderMaps.AddOrUpdate(doc, map);
                }
                return map.TryGetValue(_node, out var idx) ? idx : 0;
            }

            // Parentless node: compute order relative to the root element of its tree.
            XElement? treeRoot = null;
            if (_node is XNode xnode)
            {
                treeRoot = GetTreeRoot(xnode);
            }
            else if (_node is XAttribute attr && attr.Parent is XElement parent)
            {
                treeRoot = GetTreeRoot(parent);
            }

            if (treeRoot is not null)
            {
                if (!ParentlessOrderMaps.TryGetValue(treeRoot, out var map))
                {
                    map = ComputeElementTreeOrder(treeRoot);
                    ParentlessOrderMaps.AddOrUpdate(treeRoot, map);
                }
                return map.TryGetValue(_node, out var idx) ? idx : 0;
            }

            return 0;
        }
    }

    private static Dictionary<XObject, long> ComputeDocumentOrder(System.Xml.Linq.XDocument doc)
    {
        var map = new Dictionary<XObject, long>();
        long index = 0;
        map[doc] = index++;
        Traverse(doc, ref index, map);
        return map;
    }

    private static XElement? GetTreeRoot(XNode node)
    {
        var current = node;
        while (current.Parent is XElement parent)
            current = parent;
        return current as XElement;
    }

    private static Dictionary<XObject, long> ComputeElementTreeOrder(XElement root)
    {
        var map = new Dictionary<XObject, long>();
        long index = 0;
        map[root] = index++;
        foreach (var attr in root.Attributes())
            map[attr] = index++;
        Traverse(root, ref index, map);
        return map;
    }

    private static void Traverse(XContainer container, ref long index, Dictionary<XObject, long> map)
    {
        // Unwrap synthetic document wrapper: its children should be indexed, not the wrapper itself
        if (container is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

        foreach (var node in container.Nodes())
        {
            map[node] = index++;
            if (node is XElement elem)
            {
                foreach (var attr in elem.Attributes())
                    map[attr] = index++;
                Traverse(elem, ref index, map);
            }
        }
    }

    public string BaseUri => ComputeBaseUri();

    private string ComputeBaseUri()
    {
        // For XDocument, check annotation first (used for constructed document nodes)
        if (_node is System.Xml.Linq.XDocument doc)
        {
            var annotatedDoc = doc.Annotation<string>();
            if (!string.IsNullOrEmpty(annotatedDoc))
                return annotatedDoc;
            return doc.BaseUri ?? string.Empty;
        }

        // For XElement, walk up and resolve xml:base attributes per XML Base spec
        if (_node is XElement elem)
            return ResolveXmlBase(elem);

        // For attributes, text, comments, PIs — use parent element's base URI
        var parent = _node.Parent;
        if (parent is XElement parentElem)
            return ResolveXmlBase(parentElem);

        // Fallback for other node types: check annotation then BaseUri
        var annotated = _node.Annotation<string>();
        if (!string.IsNullOrEmpty(annotated))
            return annotated;

        return _node.BaseUri ?? string.Empty;
    }

    /// <summary>
    /// Resolves the effective base URI of an element by walking up the ancestor
    /// chain and applying <c>xml:base</c> attributes per the XML Base specification.
    /// For constructed nodes, checks for a base URI annotation on the root element.
    /// </summary>
    private static string ResolveXmlBase(XElement element)
    {
        // Collect xml:base attributes from this element up to the root
        var chain = new List<string>();
        XElement? root = null;
        var current = element;
        while (current != null)
        {
            root = current;
            var xmlBase = current.Attribute(XNamespace.Xml + "base")?.Value;
            if (xmlBase != null)
                chain.Add(xmlBase);
            current = current.Parent;
        }

        if (chain.Count == 0 && root == null)
            return element.BaseUri ?? string.Empty;

        // Start with the document's base URI or annotation, or a base URI annotation on the root element
        string baseUri = element.Document?.BaseUri ?? string.Empty;
        if (string.IsNullOrEmpty(baseUri) && element.Document != null)
        {
            var docAnnotatedBase = element.Document.Annotation<string>();
            if (!string.IsNullOrEmpty(docAnnotatedBase))
                baseUri = docAnnotatedBase;
        }
        if (string.IsNullOrEmpty(baseUri) && root != null)
        {
            var annotatedBase = root.Annotation<string>();
            if (!string.IsNullOrEmpty(annotatedBase))
                baseUri = annotatedBase;
        }

        if (chain.Count == 0)
            return baseUri;

        // Resolve from root to this element (reverse order of collection)
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            if (Uri.IsWellFormedUriString(chain[i], UriKind.Absolute))
                baseUri = chain[i];
            else if (!string.IsNullOrEmpty(baseUri))
            {
                try
                {
                    baseUri = new Uri(new Uri(baseUri), chain[i]).AbsoluteUri;
                }
                catch (UriFormatException)
                {
                    baseUri = chain[i];
                }
            }
            else
                baseUri = chain[i];
        }

        return baseUri;
    }

    // ------------------------------------------------------------------
    // Tree navigation
    // ------------------------------------------------------------------

    public IXdmNode? Parent
    {
        get
        {
            if (_isNamespaceNode)
                return _namespaceOwner is not null ? new XDocumentNode(_namespaceOwner) : null;
            var parent = GetXPathParent(_node);
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

        // Unwrap synthetic document wrapper so its children appear as document children
        if (_node is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

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

        // Unwrap synthetic document wrapper so its children appear as document children
        if (_node is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

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

    /// <summary>
    /// Returns the XPath parent of this node. In LINQ to XML the root element's
    /// <see cref="XObject.Parent"/> is <c>null</c>; this helper maps it to the
    /// owning <see cref="XDocument"/> so ancestor axes include the document node.
    /// </summary>
    private XObject? GetXPathParent(XObject node)
    {
        // Namespace nodes: parent is the element whose namespace axis includes the node,
        // not the element where the underlying XAttribute declaration resides.
        if (node == _node && _isNamespaceNode && _namespaceOwner is not null)
            return _namespaceOwner;

        var parent = node.Parent;
        if (parent is not null)
        {
            // Skip synthetic document wrapper: children of the wrapper see the XDocument as parent
            if (parent is XElement wrapper && wrapper.Name.LocalName == "__xdm_doc__" && wrapper.Name.NamespaceName == "" && wrapper.Document is not null)
                return wrapper.Document;
            // Skip temporary sequence constructor wrapper so constructed nodes appear parentless
            if (parent is XElement wrapper2 && wrapper2.Name.LocalName == "__temp__" && wrapper2.Name.NamespaceName == "")
                return null;
            return parent;
        }
        if (node is XElement elem && elem.Document is not null && elem.Document.Root == elem)
            return elem.Document;
        return null;
    }

    private XdmSequence GetParentAxis()
    {
        var parent = GetXPathParent(_node);
        return parent is not null
            ? XdmSequence.Singleton(XdmValue.FromNode(new XDocumentNode(parent)))
            : XdmSequence.Empty;
    }

    private XdmSequence GetAncestorAxis()
    {
        var items = new List<XdmValue>();
        var current = GetXPathParent(_node);
        while (current is not null)
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(current)));
            current = GetXPathParent(current);
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetAncestorOrSelfAxis()
    {
        var items = new List<XdmValue> { XdmValue.FromNode(this) };
        var current = GetXPathParent(_node);
        while (current is not null)
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(current)));
            current = GetXPathParent(current);
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
            // Namespace declarations are not attributes in the XPath data model;
            // they belong to the namespace axis.
            if (attr.IsNamespaceDeclaration)
                continue;
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
                // An empty URI (xmlns="" or xmlns:prefix="") undeclares the namespace;
                // it does not create a namespace node, but it stops inheritance.
                if (attr.Value == string.Empty)
                {
                    seen.Add(prefix);
                    continue;
                }
                if (seen.Add(prefix))
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(attr, element)));
                }
            }
            current = current.Parent;

            // Stop walking up when we hit an element that was created with
            // inherit-namespaces="no" (XSLT 3.0 §11.9.2).
            if (current is XElement parent && parent.Annotation<NamespaceInheritanceBarrier>() != null)
                break;
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

        // Unwrap synthetic document wrapper so its children appear as document descendants
        if (node is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

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

        // Unwrap synthetic document wrapper so its children appear as document descendants
        if (node is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

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
            System.Xml.Linq.XDocument doc => GetSyntheticWrapper(doc) is { } wrapperDoc
                ? string.Concat(wrapperDoc.Nodes().Select(n => n.ToString(SaveOptions.DisableFormatting)))
                : doc.ToString(SaveOptions.DisableFormatting),
            XElement el => el.ToString(SaveOptions.DisableFormatting),
            XText t => System.Security.SecurityElement.Escape(t.Value) ?? t.Value,
            XComment c => $"<!--{c.Value}-->",
            XProcessingInstruction pi => $"<?{pi.Target} {pi.Data}?>",
            XAttribute a => a.Value,
            _ => _node.ToString() ?? string.Empty
        };
    }

    // ------------------------------------------------------------------
    // Synthetic document wrapper helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the synthetic wrapper element for document nodes that contain
    /// mixed content (multiple elements, text nodes, etc.). LINQ-to-XML
    /// XDocument cannot hold arbitrary mixed content directly, so we wrap
    /// the children in a hidden element that <see cref="XDocumentNode"/>
    /// transparently unwraps.
    /// </summary>
    private static XElement? GetSyntheticWrapper(System.Xml.Linq.XDocument doc)
        => doc.Root?.Name == XName.Get("__xdm_doc__") ? doc.Root : null;
}
