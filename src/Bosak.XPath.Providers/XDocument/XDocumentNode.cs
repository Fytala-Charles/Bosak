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
//                      | Charles Korthout | 1.1   | 11-06-2026     | Override Equals/GetHashCode for IXdmNode identity-based equality                         |
//                      | Charles Korthout | 1.2   | 13-06-2026     | Composite DocumentOrder includes global creation sequence for cross-document sorting    |
//                      | Charles Korthout | 1.3   | 25-06-2026     | Added DocumentUri property/setter distinct from BaseUri                                |
//                      | Charles Korthout | 1.4   | 25-06-2026     | Fixed following/preceding axes for attribute and namespace nodes                       |
//                      | Charles Korthout | 1.5   | 26-06-2026     | GetNamespaceAxis adds implied default namespaces only when not explicitly declared     |
//                      | Charles Korthout | 1.6   | 28-06-2026     | ResolveXmlBase honors external-entity node base URIs; fixes resolve-uri-021          |
//                      | Charles Korthout | 1.7   | 18-07-2026     | Exposed XDocumentType properties for DTD-based ID/IDREF support                        |
//                      | Charles Korthout | 1.8   | 19-07-2026     | GetNamespaceAxis returns xml first, then namespaces in root-to-current order         |
//                      | Charles Korthout | 1.9   | 19-07-2026     | Added IsId property using PSVI for schema-validated ID nodes                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.0   | 20-07-2026     | Added HasNoTypedValue using PSVI for complex element-only/empty elements               |
//                      | Charles Korthout | 2.1   | 20-07-2026     | Namespace-node identity uses owner+prefix+URI (Axes123)                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
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
    private static readonly ConditionalWeakTable<System.Xml.Linq.XDocument, StrongBox<long>> DocumentSequences = new();
    private static readonly ConditionalWeakTable<XElement, StrongBox<long>> ElementTreeSequences = new();
    private static long _sequenceCounter;
    private static readonly object SequenceLock = new();

    private readonly XObject _node;
    private readonly bool _isNamespaceNode;
    private readonly XElement? _namespaceOwner;

    public XDocumentNode(XObject node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>Gets the underlying LINQ to XML node.</summary>
    public XObject UnderlyingObject => _node;

    /// <summary>
    /// Annotation stored on an <see cref="XDocument"/> to record its document URI
    /// independently of its base URI. Temporary trees have no document URI.
    /// </summary>
    private sealed class DocumentUriAnnotation(string uri)
    {
        public string Uri { get; } = uri;
    }

    /// <summary>
    /// Sets the document URI for this node when it represents a document node.
    /// </summary>
    /// <param name="uri">The absolute document URI, or empty string to clear it.</param>
    public void SetDocumentUri(string uri)
    {
        if (_node is System.Xml.Linq.XDocument doc)
        {
            if (string.IsNullOrEmpty(uri))
                doc.RemoveAnnotations<DocumentUriAnnotation>();
            else
                doc.AddAnnotation(new DocumentUriAnnotation(uri));
        }
    }

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

    /// <summary>
    /// Returns <c>true</c> when the containing document was loaded as XML 1.1 and
    /// therefore stores encoded names.
    /// </summary>
    private bool IsXml11Document
    {
        get
        {
            if (_node is System.Xml.Linq.XDocument doc)
                return doc.Annotation<Xml11Annotation>() != null;
            return _node.Document?.Annotation<Xml11Annotation>() != null;
        }
    }

    private string Decode(string name) => IsXml11Document ? Xml11NameCodec.DecodeName(name) : name;

    public string LocalName
    {
        get
        {
            if (_isNamespaceNode)
            {
                return _node is XAttribute attr && attr.Name.LocalName != "xmlns"
                    ? Decode(attr.Name.LocalName)
                    : string.Empty;
            }
            return _node switch
            {
                XElement e => Decode(e.Name.LocalName),
                XAttribute a => Decode(a.Name.LocalName),
                XProcessingInstruction pi => pi.Target,
                _ => string.Empty
            };
        }
    }

    public string EncodedLocalName
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

    public string Prefix
    {
        get
        {
            if (_isNamespaceNode)
                return string.Empty;
            return _node switch
            {
                XElement e => Decode(e.GetPrefixOfNamespace(e.Name.Namespace) ?? string.Empty),
                XAttribute a => Decode((a.Parent as XElement)?.GetPrefixOfNamespace(a.Name.Namespace) ?? string.Empty),
                _ => string.Empty
            };
        }
    }

    public string EncodedPrefix
    {
        get
        {
            if (_isNamespaceNode)
                return string.Empty;
            return _node switch
            {
                XElement e => e.GetPrefixOfNamespace(e.Name.Namespace) ?? string.Empty,
                XAttribute a => (a.Parent as XElement)?.GetPrefixOfNamespace(a.Name.Namespace) ?? string.Empty,
                _ => string.Empty
            };
        }
    }

    public string StringValue
    {
        get
        {
            if (_isNamespaceNode)
                return _node is XAttribute attr ? attr.Value : string.Empty;
            return _node switch
            {
                XElement e => e.Value,
                XAttribute a => IsXml11Document ? Xml11NameCodec.DecodeValue(a.Value) : a.Value,
                XText t => t.Value,
                XComment c => c.Value,
                XProcessingInstruction pi => pi.Data,
                System.Xml.Linq.XDocument d => GetSyntheticWrapper(d) is { } wrapper
                    ? wrapper.Value
                    : (d.Root != null ? d.Root.Value : string.Concat(d.Nodes().OfType<XText>().Select(t => t.Value))),
                _ => string.Empty
            };
        }
    }

    public XdmValue TypedValue => XdmValue.FromString(StringValue);

    /// <summary>
    /// Gets a value indicating whether this node has no typed value per XDM.
    /// For elements this is true when schema validation produced a complex type
    /// with element-only or empty content (no simple typed value), which means
    /// <c>fn:data()</c> must raise FOTY0012.
    /// </summary>
    public bool HasNoTypedValue
    {
        get
        {
            if (_node is not XElement element)
                return false;

            var info = element.GetSchemaInfo();
            if (info?.SchemaType is XmlSchemaComplexType complex)
            {
                // Complex types with element-only or empty content have no typed value.
                return complex.ContentType is XmlSchemaContentType.ElementOnly
                    or XmlSchemaContentType.Empty;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this node has the XDM is-id property.
    /// For elements this is true when the typed value is a single xs:ID atomic value
    /// (including derived types, union members and singleton lists of xs:ID).
    /// For attributes this is true for ID-typed attributes, including <c>id</c> and
    /// <c>xml:id</c> attributes even when no schema is available.
    /// </summary>
    public bool IsId => ComputeIsId();

    public bool IsSameNode(IXdmNode other)
    {
        if (other is not XDocumentNode xn)
            return false;
        if (_isNamespaceNode != xn._isNamespaceNode)
            return false;

        // Namespace nodes are virtual properties of an element; the underlying
        // XAttribute objects are created on the fly, so reference equality fails.
        // Compare by owner element + prefix + URI instead (Axes123).
        if (_isNamespaceNode)
        {
            return ReferenceEquals(_namespaceOwner, xn._namespaceOwner)
                   && LocalName == xn.LocalName
                   && StringValue == xn.StringValue;
        }

        return ReferenceEquals(_node, xn._node);
    }

    public override bool Equals(object? obj)
        => obj is IXdmNode other && IsSameNode(other);

    public override int GetHashCode()
    {
        if (_isNamespaceNode)
        {
            return HashCode.Combine(
                RuntimeHelpers.GetHashCode(_namespaceOwner),
                LocalName,
                StringValue);
        }
        return RuntimeHelpers.GetHashCode(_node);
    }

    public long DocumentOrder
    {
        get
        {
            // Namespace nodes are virtual; their document order is determined by the
            // owner element so that all namespace nodes of an element sort together and
            // before the namespace nodes of any descendant.
            if (_isNamespaceNode && _namespaceOwner is not null)
            {
                return new XDocumentNode(_namespaceOwner).DocumentOrder;
            }

            var doc = _node.Document;
            if (doc is not null)
            {
                if (!OrderMaps.TryGetValue(doc, out var map))
                {
                    map = ComputeDocumentOrder(doc);
                    OrderMaps.AddOrUpdate(doc, map);
                }
                long local = map.TryGetValue(_node, out var idx) ? idx : 0;
                long seq = GetDocumentSequence(doc);
                return CombineOrder(seq, local);
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
                long local = map.TryGetValue(_node, out var idx) ? idx : 0;
                long seq = GetElementTreeSequence(treeRoot);
                return CombineOrder(seq, local);
            }

            return 0;
        }
    }

    private static long CombineOrder(long sequence, long local)
        => (sequence << 32) | (local & 0xffffffffL);

    private static long GetDocumentSequence(System.Xml.Linq.XDocument doc)
    {
        if (DocumentSequences.TryGetValue(doc, out var box))
            return box.Value;
        lock (SequenceLock)
        {
            if (DocumentSequences.TryGetValue(doc, out box))
                return box.Value;
            var seq = Interlocked.Increment(ref _sequenceCounter);
            DocumentSequences.AddOrUpdate(doc, new StrongBox<long>(seq));
            return seq;
        }
    }

    private static long GetElementTreeSequence(XElement root)
    {
        if (ElementTreeSequences.TryGetValue(root, out var box))
            return box.Value;
        lock (SequenceLock)
        {
            if (ElementTreeSequences.TryGetValue(root, out box))
                return box.Value;
            var seq = Interlocked.Increment(ref _sequenceCounter);
            ElementTreeSequences.AddOrUpdate(root, new StrongBox<long>(seq));
            return seq;
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

    public string DocumentUri => ComputeDocumentUri();

    public bool HasDocumentType => GetDocumentType() is not null;

    public string DocumentTypeName => GetDocumentType()?.Name ?? string.Empty;

    public string PublicId => GetDocumentType()?.PublicId ?? string.Empty;

    public string SystemId => GetDocumentType()?.SystemId ?? string.Empty;

    public string InternalSubset => GetDocumentType()?.InternalSubset ?? string.Empty;

    private XDocumentType? GetDocumentType()
    {
        if (_node is System.Xml.Linq.XDocument doc)
            return doc.DocumentType;

        var containingDoc = _node.Document;
        if (containingDoc is not null)
            return containingDoc.DocumentType;

        return null;
    }

    private string ComputeDocumentUri()
    {
        if (_node is System.Xml.Linq.XDocument doc)
        {
            var annotation = doc.Annotation<DocumentUriAnnotation>();
            if (annotation != null)
                return annotation.Uri;
            return doc.BaseUri ?? string.Empty;
        }

        // Fallback for non-document nodes: report the containing document's URI.
        var containingDoc = _node.Document;
        if (containingDoc != null)
        {
            var docAnnotation = containingDoc.Annotation<DocumentUriAnnotation>();
            if (docAnnotation != null)
                return docAnnotation.Uri;
            return containingDoc.BaseUri ?? string.Empty;
        }

        return string.Empty;
    }

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

        // For attributes, text, comments, and PIs, prefer the node's own base URI
        // when it differs from the document base (e.g., nodes originating from an
        // external parsed entity). Otherwise fall back to the parent element.
        var nodeBase = _node.BaseUri;
        var docBase = _node.Document?.BaseUri;
        if (!string.IsNullOrEmpty(nodeBase) && nodeBase != docBase)
            return nodeBase;

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
    /// When an element originates from an external parsed entity, its own
    /// <see cref="XObject.BaseUri"/> is used as the starting point and ancestor
    /// elements outside that entity are ignored.
    /// </summary>
    private static string ResolveXmlBase(XElement element)
    {
        var nodeBase = element.BaseUri;
        var docBase = element.Document?.BaseUri;
        bool isEntityNode = !string.IsNullOrEmpty(nodeBase) && nodeBase != docBase;
        return ResolveXmlBase(element, isEntityNode ? nodeBase : null);
    }

    private static string ResolveXmlBase(XElement element, string? initialBase)
    {
        // Collect xml:base attributes from this element up to the root.
        // For an external-entity node, stop at the entity boundary so that
        // xml:base attributes in the including document do not leak in.
        var chain = new List<string>();
        XElement? root = null;
        var current = element;
        while (current != null)
        {
            if (initialBase != null && current.BaseUri != initialBase)
                break;

            root = current;
            var xmlBase = current.Attribute(XNamespace.Xml + "base")?.Value;
            if (xmlBase != null)
                chain.Add(xmlBase);
            current = current.Parent;
        }

        if (chain.Count == 0 && root == null)
            return element.BaseUri ?? string.Empty;

        // Start with the supplied/entity base URI, the document's base URI,
        // an annotation on the document, or an annotation on the root element.
        string baseUri = initialBase ?? element.Document?.BaseUri ?? string.Empty;
        if (string.IsNullOrEmpty(baseUri) && element.Document != null && initialBase == null)
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

        bool xml11 = IsXml11Document;
        string? encodedLocalName = localName is not null && xml11 ? Xml11NameCodec.EncodeName(localName) : localName;

        var items = new List<XdmValue>();
        foreach (var attr in element.Attributes())
        {
            if (encodedLocalName is not null && attr.Name.LocalName != encodedLocalName)
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

        string elementNs = element.Name.NamespaceName;
        bool elementNsIsNonEmpty = !string.IsNullOrEmpty(elementNs);
        bool hasExplicitDefaultForElementNs = false;
        bool hasPrefixDeclarationForElementNs = false;

        // Collect namespace declarations from current element up to the root.
        // Because we walk upward, this produces current-to-root order; we will
        // reverse it below so the final axis order is root-to-current.
        var collected = new List<XdmValue>();
        while (current is not null)
        {
            foreach (var attr in current.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;

                string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                AddNamespaceNode(collected, seen, prefix, attr.Value, element);

                if (elementNsIsNonEmpty)
                {
                    if (prefix == string.Empty && attr.Value == elementNs)
                        hasExplicitDefaultForElementNs = true;
                    else if (prefix != string.Empty && attr.Value == elementNs)
                        hasPrefixDeclarationForElementNs = true;
                }
            }

            current = current.Parent;

            // Stop walking up when we hit an element that was created with
            // inherit-namespaces="no" (XSLT 3.0 §11.9.2).
            if (current is XElement parent && parent.Annotation<NamespaceInheritanceBarrier>() != null)
                break;
        }

        // Namespaces implied by the element name itself. LINQ to XML stores the
        // namespace URI on the XName but does not materialize an xmlns attribute
        // for every element (e.g. elements created by json-to-xml). Treat a
        // non-empty element namespace as an implied default-namespace binding only
        // when it is not already declared explicitly (either as default or prefixed).
        if (elementNsIsNonEmpty && !hasExplicitDefaultForElementNs && !hasPrefixDeclarationForElementNs)
        {
            AddNamespaceNode(collected, seen, string.Empty, elementNs, element);
        }

        // Reverse from current-to-root into root-to-current order.
        collected.Reverse();

        // The xml namespace is always implicitly in scope and must be first.
        if (seen.Add("xml"))
        {
            items.Add(XdmValue.FromNode(new XDocumentNode(
                new XAttribute(XNamespace.Xmlns + "xml", XNamespace.Xml.NamespaceName),
                element)));
        }

        items.AddRange(collected);
        return MaterializedSequence.FromList(items);
    }

    /// <summary>
    /// Adds a namespace node for <paramref name="prefix"/> -> <paramref name="uri"/>
    /// if the prefix has not been seen yet. An empty <paramref name="uri"/>
    /// undeclares the prefix and stops inheritance for it.
    /// </summary>
    private static void AddNamespaceNode(List<XdmValue> items, HashSet<string> seen, string prefix, string uri, XElement owner)
    {
        if (uri == string.Empty)
        {
            seen.Add(prefix);
            return;
        }

        if (!seen.Add(prefix))
            return;

        bool xml11 = owner.Document?.Annotation<Xml11Annotation>() != null;
        string storagePrefix = xml11 ? Xml11NameCodec.EncodeName(prefix) : prefix;
        XAttribute declaration = string.IsNullOrEmpty(storagePrefix)
            ? new XAttribute("xmlns", uri)
            : new XAttribute(XNamespace.Xmlns + storagePrefix, uri);
        items.Add(XdmValue.FromNode(new XDocumentNode(declaration, owner)));
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

        // Attributes and namespace nodes are not children of their parent element,
        // so they have no preceding siblings.
        if (_node is XAttribute || _isNamespaceNode)
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

        // Attributes and namespace nodes are not children of their parent element,
        // so the normal "siblings after current" walk would skip the element's children.
        // Per XDM document order, attributes/namespaces precede the element's children,
        // so the following axis from an attribute/namespace includes all descendants of
        // the parent element, followed by the normal walk up from the parent element.
        var current = _node;
        if (_node is XAttribute || _isNamespaceNode)
        {
            if (_node.Parent is XElement attrParent)
            {
                foreach (var child in attrParent.Nodes())
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(child)));
                    AddDescendants(child, items);
                }
                current = attrParent;
            }
            else
            {
                return MaterializedSequence.FromList(items);
            }
        }

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

        // Attributes and namespace nodes are not children of their parent element.
        // Per XDM document order they precede the element's children, so the preceding
        // axis from an attribute/namespace must start from the parent element itself,
        // not from the attribute/namespace (which would incorrectly include the
        // element's children as "preceding" siblings).
        var current = _node;
        if (_node is XAttribute || _isNamespaceNode)
        {
            if (_node.Parent is XElement attrParent)
                current = attrParent;
            else
                return MaterializedSequence.FromList(items);
        }

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

    // ------------------------------------------------------------------
    // ID-node helpers
    // ------------------------------------------------------------------

    private bool ComputeIsId()
    {
        if (_isNamespaceNode)
            return false;

        if (_node is XAttribute attr)
            return IsIdAttribute(attr);

        if (_node is XElement element)
            return IsIdElement(element);

        return false;
    }

    private static bool IsIdAttribute(XAttribute attr)
    {
        var info = attr.GetSchemaInfo();
        if (info is not null)
        {
            if (IsIdSchemaType(info.MemberType, attr.Value))
                return true;
            if (IsIdSchemaType(info.SchemaType, attr.Value))
                return true;
        }

        // Infoset fallback: attributes named "id" (no namespace) or "xml:id" are IDs.
        if (attr.Name.LocalName == "id" && attr.Name.NamespaceName.Length == 0)
            return true;
        if (attr.Name.LocalName == "id" && attr.Name.NamespaceName == "http://www.w3.org/XML/1998/namespace")
            return true;

        return false;
    }

    private static bool IsIdElement(XElement element)
    {
        var info = element.GetSchemaInfo();
        if (info is null)
            return false;

        if (info.IsNil)
            return false;

        if (IsIdSchemaType(info.MemberType, element.Value))
            return true;
        if (IsIdSchemaType(info.SchemaType, element.Value))
            return true;

        return false;
    }

    private static bool IsIdSchemaType(XmlSchemaType? type, string value)
    {
        if (type is null)
            return false;

        if (type is XmlSchemaSimpleType simple && simple.Datatype is not null)
        {
            if (simple.Datatype.Variety == XmlSchemaDatatypeVariety.List)
            {
                // A list-of-ID element is considered an ID only when its typed value
                // is a single xs:ID atomic value.
                if (simple.Datatype.TypeCode == XmlTypeCode.Id)
                {
                    var tokens = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    return tokens.Length == 1;
                }
                return false;
            }
        }

        return type.TypeCode == XmlTypeCode.Id;
    }

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
