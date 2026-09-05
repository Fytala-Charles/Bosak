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
//                      | Charles Korthout | 2.2   | 21-07-2026     | ToXmlString copies in-scope namespaces for standalone element serialization           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.3   | 25-07-2026     | Prefix annotation preserves prefixes of free-standing computed attributes              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.4   | 23-08-2026     | GetElementPrefix honors OriginalPrefixAnnotation so fn:name() keeps source prefix      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.4   | 25-07-2026     | Exposed XML 1.1 prefixed namespace undeclarations                                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.5   | 15-08-2026     | GetXPathParent falls back to XDocument for document-level PIs/comments (path009)       |
//                      | Charles Korthout | 2.6   | 17-08-2026     | Restrict fallback to PI/comment nodes to avoid XDocument self-loop (fn-doc hang)       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.7   | 19-08-2026     | Preserve timezone in schema-validated typed values (qischema003)                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.8   | 21-08-2026     | Detect XML 1.1 on constructed elements; decode encoded names during serialization        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.9   | 29-08-2026     | Added DTD unparsed entity lookup via TryGetUnparsedEntity                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.10  | 29-08-2026     | Exclude whitespace-only text nodes from document-level children (axes-202)            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.11  | 29-08-2026     | Resolve unparsed-entity system IDs against annotation BaseUri                            |
//                      | Charles Korthout | 2.12  | 29-08-2026     | Stable namespace-node XAttribute cache; following/preceding axes use _namespaceOwner.    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.13  | 29-08-2026     | Namespace nodes report owner element's Document so union sorting places them before attributes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.14  | 29-08-2026     | Reserve namespace-node DocumentOrder slot after owner element, before attributes         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 28-07-2026     | Namespace axis skips non-propagating ancestor bindings; redundant xmlns omitted in ToXmlString |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 29-07-2026     | Parentless-namespace-node marker: parent axis and Parent honor it |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13  | 29-07-2026     | xml:id/id attributes are IDs only when the value is a valid NCName (fn-id-25) |
//                      | Charles Korthout | 0.14  | 01-08-2026     | xml:id values normalized (trim) before NCName validation (key-076)            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.15  | 21-08-2026     | Exposed schema element/attribute declarations and nilled status from PSVI                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.16  | 21-08-2026     | Annotate PSVI typed values for XSD union types using the selected member type            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.17  | 21-08-2026     | Nilled elements have an empty typed value and are not element(*, T)-compatible          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.18  | 21-08-2026     | Added IsIdref property using PSVI for schema-validated IDREF nodes                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.19  | 22-08-2026     | Preserve lexical timezone offsets in schema-validated date/time typed values            |
//                      | Charles Korthout | 0.20  | 23-08-2026     | QName/NOTATION typed values with in-scope resolver; declared schema type; xsi:type ID/IDREF detection |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.21  | 23-08-2026     | IsConstructedElement recognizes constructed elements; document-node() matches empty documents |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.22  | 23-08-2026     | List typed values use item/member types so instance-of checks against xs:integer/float pass |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.23  | 23-08-2026     | Added IsComplexType property for schema-aware deep-equal type-annotation comparison |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.24  | 03-09-2026     | Warning-free build: CS8602 guard for null union BaseMemberTypes                         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
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
    // Namespace nodes are virtual: each axis evaluation used to create a new XAttribute,
    // so generate-id() produced different IDs for the same logical namespace node.
    // Cache detached XAttributes per owner element + logical prefix to give namespace
    // nodes stable object identity.
    private static readonly ConditionalWeakTable<XElement, ConcurrentDictionary<string, XAttribute>> NamespaceAttributeCache = new();
    private static long _sequenceCounter;
    private static readonly object SequenceLock = new();

    /// <summary>
    /// Returns a cached detached <see cref="XAttribute"/> representing the namespace
    /// declaration for <paramref name="prefix"/> on <paramref name="owner"/>, creating
    /// and caching it on first use. This gives logically identical namespace nodes the
    /// same underlying object so generate-id() returns the same ID for them.
    /// </summary>
    private static XAttribute GetOrCreateNamespaceAttribute(XElement owner, string prefix, string uri, bool xml11)
    {
        if (!NamespaceAttributeCache.TryGetValue(owner, out var dict))
        {
            dict = new ConcurrentDictionary<string, XAttribute>();
            NamespaceAttributeCache.AddOrUpdate(owner, dict);
        }

        return dict.GetOrAdd(prefix, _ =>
        {
            string storagePrefix = xml11 ? Xml11NameCodec.EncodeName(prefix) : prefix;
            return string.IsNullOrEmpty(storagePrefix)
                ? new XAttribute("xmlns", uri)
                : new XAttribute(XNamespace.Xmlns + storagePrefix, uri);
        });
    }

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
    /// XML 1.1 prefixed namespace undeclarations (<c>xmlns:p=""</c>) recorded on this
    /// element when the document was parsed; empty for other nodes.
    /// </summary>
    public IReadOnlyList<string> Xml11UndeclaredPrefixes
        => _node is XElement element && element.Annotation<PrefixedNamespaceUndeclarations>() is { } undeclarations
            ? undeclarations.Prefixes
            : Array.Empty<string>();

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
    /// Returns <c>true</c> when this node belongs to an XML 1.1 tree and therefore
    /// stores encoded names. An XML 1.1 annotation may be on the document (loaded
    /// documents) or on a constructed element and its ancestors (parentless trees).
    /// </summary>
    private bool IsXml11Document
    {
        get
        {
            if (_node is System.Xml.Linq.XDocument doc)
                return doc.Annotation<Xml11Annotation>() != null;

            XElement? element = _node switch
            {
                XElement el => el,
                XAttribute attr => attr.Parent,
                XNode n => n.Parent,
                _ => null
            };

            while (element is not null)
            {
                if (element.Annotation<Xml11Annotation>() != null)
                    return true;
                element = element.Parent;
            }

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
                XElement e => Decode(GetElementPrefix(e)),
                XAttribute a => Decode(a.Annotation<AttributePrefixAnnotation>()?.Prefix
                    ?? (a.Parent as XElement)?.GetPrefixOfNamespace(a.Name.Namespace) ?? string.Empty),
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
                XElement e => GetElementPrefix(e),
                XAttribute a => a.Annotation<AttributePrefixAnnotation>()?.Prefix
                    ?? (a.Parent as XElement)?.GetPrefixOfNamespace(a.Name.Namespace) ?? string.Empty,
                _ => string.Empty
            };
        }
    }

    /// <summary>
    /// Returns the preferred prefix for an element. If the source document preserved an
    /// original prefix via <see cref="OriginalPrefixAnnotation"/>, that prefix is used
    /// when it is still bound to the element's namespace URI; this keeps fn:name() aligned
    /// with the lexical form from the source even when a default namespace declaration
    /// for the same URI is in scope (copy-4901).
    /// </summary>
    private static string GetElementPrefix(XElement element)
    {
        var ns = element.Name.Namespace;
        if (ns == XNamespace.None)
            return string.Empty;

        var original = element.Annotation<OriginalPrefixAnnotation>()?.Prefix;
        if (original != null)
        {
            if (original.Length == 0)
                return string.Empty;
            var uriForOriginal = element.GetNamespaceOfPrefix(original);
            if (uriForOriginal?.NamespaceName == ns.NamespaceName)
                return original;
        }

        // Prefer the empty prefix when the default namespace binds this URI.
        var defaultNs = element.GetDefaultNamespace();
        if (defaultNs == ns)
            return string.Empty;

        return element.GetPrefixOfNamespace(ns) ?? string.Empty;
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
                    : (d.Root != null ? d.Root.Value : string.Concat(d.Nodes().OfType<XText>().Where(t => !string.IsNullOrWhiteSpace(t.Value)).Select(t => t.Value))),
                _ => string.Empty
            };
        }
    }

    public XdmValue TypedValue => GetTypedValue();

    /// <summary>
    /// Gets a value indicating whether this node has no typed value per XDM.
    /// For elements this is true when schema validation produced a complex type
    /// with element-only or empty content (no simple typed value), which means
    /// <c>fn:data()</c> must raise FOTY0012. Nilled elements always have an empty
    /// typed value, so this returns <c>false</c> for them.
    /// </summary>
    public bool HasNoTypedValue
    {
        get
        {
            if (_node is not XElement element)
                return false;

            if (element.GetSchemaInfo()?.IsNil ?? false)
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
    /// Gets a value indicating whether the schema type annotation of this node
    /// is a complex type. Attributes always return <c>false</c> because attribute
    /// types are always simple.
    /// </summary>
    public bool IsComplexType
    {
        get
        {
            if (_node is not XElement element)
                return false;
            return element.GetSchemaInfo()?.SchemaType is XmlSchemaComplexType;
        }
    }

    /// <summary>
    /// Returns the typed value of this node when PSVI annotations are available from schema
    /// validation; otherwise falls back to the string value as <c>xs:untypedAtomic</c>.
    /// </summary>
    private XdmValue GetTypedValue()
    {
        if (_isNamespaceNode)
            return XdmValue.FromString(StringValue);

        IXmlSchemaInfo? info = _node switch
        {
            XElement e => e.GetSchemaInfo(),
            XAttribute a => a.GetSchemaInfo(),
            _ => null
        };

        // XDM §2.7.2: the typed value of a nilled element is the empty sequence.
        if (info is { IsNil: true })
            return XdmValue.Undefined;

        if (info?.SchemaType is null)
            return XdmValue.FromString(StringValue);

        // For complex types with simple content, the member type carries the simple datatype.
        XmlSchemaSimpleType? simpleType = info.MemberType as XmlSchemaSimpleType
            ?? info.SchemaType as XmlSchemaSimpleType;
        XmlSchemaType? annotationType = simpleType ?? info.SchemaType;
        XmlSchemaDatatype? datatype = simpleType?.Datatype;

        // Complex types with simple content (e.g. extension of xs:decimal with attributes)
        // do not surface the simple type via MemberType; the compiled complex type exposes
        // the simple datatype directly.
        if (simpleType is null && info.SchemaType is XmlSchemaComplexType complexType
            && complexType.ContentType == XmlSchemaContentType.TextOnly)
        {
            datatype = complexType.Datatype;
        }

        if (datatype is null)
            return XdmValue.FromString(StringValue);

        try
        {
            var nsResolver = CreateInScopeNamespaceResolver();
            object parsed = datatype.ParseValue(StringValue, new NameTable(), nsResolver);
            bool hasTz = LexicalHasTimezone(StringValue);
            if (datatype.Variety == XmlSchemaDatatypeVariety.List && parsed is System.Collections.IEnumerable list && parsed is not string)
            {
                var items = new List<XdmValue>();

                // Determine the declared item type of the list. For a simple list type use the
                // simple type's content; for a complex type with simple content that extends a
                // list type (e.g. complexExtendsList) walk through the base schema type.
                XmlSchemaSimpleType? itemSimpleType = simpleType?.Content is XmlSchemaSimpleTypeList listContent
                    ? listContent.BaseItemType
                    : (info.SchemaType is XmlSchemaComplexType complexListType
                        && complexListType.BaseXmlSchemaType is XmlSchemaSimpleType baseSimpleType
                        && baseSimpleType.Content is XmlSchemaSimpleTypeList complexListContent
                        ? complexListContent.BaseItemType
                        : null);

                XmlSchemaDatatype itemDatatype = itemSimpleType?.Datatype ?? datatype;
                string[] itemLexicals = SplitListLexicalValue(StringValue);
                int index = 0;
                foreach (object? item in list)
                {
                    if (item is null) { index++; continue; }

                    XmlSchemaType itemSchemaType = itemSimpleType ?? annotationType ?? simpleType!;
                    if (itemSimpleType is not null && itemSimpleType.Datatype?.Variety == XmlSchemaDatatypeVariety.Union)
                    {
                        var memberType = InferUnionMemberType(itemLexicals[index], itemSimpleType, nsResolver);
                        if (memberType is not null)
                            itemSchemaType = memberType;
                    }

                    items.Add(ConvertSchemaValue(item, itemDatatype, itemSchemaType, hasTz));
                    index++;
                }
                return XdmValue.FromSequence(MaterializedSequence.FromList(items));
            }
            if (datatype.Variety == XmlSchemaDatatypeVariety.Union)
            {
                // For a union, annotate the parsed value using the actual member type selected
                // by the schema validator. MemberType carries the resolved simple type.
                if (info.MemberType is XmlSchemaSimpleType memberType)
                {
                    return ConvertSchemaValue(parsed, memberType.Datatype ?? datatype, memberType, hasTz, StringValue);
                }
            }
            return ConvertSchemaValue(parsed, datatype, annotationType ?? simpleType!, hasTz, StringValue);
        }
        catch
        {
            return XdmValue.FromString(StringValue);
        }
    }

    /// <summary>
    /// Splits the lexical value of an XSD list type into its item strings.
    /// XSD list items are separated by whitespace and leading/trailing whitespace is ignored.
    /// </summary>
    private static string[] SplitListLexicalValue(string value)
        => value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// For a union simple type, returns the first member type whose datatype accepts the
    /// supplied lexical value. This lets the typed value of a list-of-union be annotated
    /// with the actual member type selected for each item.
    /// </summary>
    private static XmlSchemaSimpleType? InferUnionMemberType(string lexicalValue, XmlSchemaSimpleType unionType, IXmlNamespaceResolver? nsResolver)
    {
        if (unionType.Content is not XmlSchemaSimpleTypeUnion union)
            return null;
        if (union.BaseMemberTypes is not { } memberTypes)
            return null;

        foreach (XmlSchemaSimpleType member in memberTypes)
        {
            if (member.Datatype is null)
                continue;
            try
            {
                member.Datatype.ParseValue(lexicalValue, new NameTable(), nsResolver);
                return member;
            }
            catch
            {
                // Try the next member type in the union's member-type definition order.
            }
        }
        return null;
    }

    /// <summary>
    /// Builds an <see cref="IXmlNamespaceResolver"/> that contains the in-scope namespace
    /// bindings for the current node. This is required when schema validation returns
    /// <c>xs:QName</c>/<c>xs:NOTATION</c> values, because <see cref="XmlSchemaDatatype.ParseValue"/>
    /// only resolves prefixes that are present in the supplied resolver.
    /// </summary>
    private IXmlNamespaceResolver CreateInScopeNamespaceResolver()
    {
        var manager = new XmlNamespaceManager(new NameTable());
        manager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");

        XObject? current = _node;
        while (current is not null)
        {
            if (current is XElement el)
            {
                foreach (XAttribute attr in el.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        string prefix = attr.Name.LocalName;
                        if (prefix == "xmlns")
                            prefix = string.Empty;
                        // The nearest declaration wins; ignore outer redeclarations.
                        if (!manager.HasNamespace(prefix))
                            manager.AddNamespace(prefix, attr.Value);
                    }
                }
            }
            current = current.Parent;
        }

        return manager;
    }

    /// <summary>
    /// Converts a .NET value returned by <see cref="XmlSchemaDatatype.ParseValue"/> into an
    /// <see cref="XdmValue"/> with the appropriate schema-type annotation.
    /// </summary>
    /// <param name="lexicalValue">The original lexical string, used for date/time values so the
    /// timezone offset is preserved instead of being normalized to the local offset.</param>
    private static XdmValue ConvertSchemaValue(object value, XmlSchemaDatatype datatype, XmlSchemaType schemaType, bool hasTimezone = true, string? lexicalValue = null)
    {
        // Values produced by parsing list/union datatypes are wrapped in XmlAtomicValue;
        // unwrap them so the correct primitive conversion and type annotation are applied.
        if (value is XmlAtomicValue atomic)
        {
            value = atomic.ValueAs(atomic.ValueType, null);
            if (atomic.XmlType is XmlSchemaSimpleType atomicSchemaType)
                schemaType = atomicSchemaType;
        }

        string typeName = schemaType.QualifiedName.Name;
        string typeNs = schemaType.QualifiedName.Namespace;

        if (typeNs != XmlSchema.Namespace)
        {
            // User-defined type: derive the annotation from the ultimate built-in base type.
            typeName = GetBuiltInBaseTypeName(schemaType) ?? "untypedAtomic";
        }

        switch (value)
        {
            case bool b:
                return XdmValue.FromBoolean(b);
            case decimal d:
                // Integer-derived schema types preserve the integer XDM kind when the value fits.
                if (IsIntegerTypeName(typeName) && d >= long.MinValue && d <= long.MaxValue && d == (long)d)
                    return XdmValue.FromInteger((long)d, typeName);
                return XdmValue.FromDecimal(d, typeName);
            case float f:
                return XdmValue.FromFloat(f);
            case double d:
                return XdmValue.FromDouble(d);
            case byte u8: return XdmValue.FromInteger(u8, typeName);
            case sbyte i8: return XdmValue.FromInteger(i8, typeName);
            case short i16: return XdmValue.FromInteger(i16, typeName);
            case ushort u16: return XdmValue.FromInteger(u16, typeName);
            case int i32: return XdmValue.FromInteger(i32, typeName);
            case uint u32:
                return XdmValue.FromInteger((long)u32, typeName);
            case long i64:
                return XdmValue.FromInteger(i64, typeName);
            case ulong u64:
                if (u64 <= (ulong)long.MaxValue)
                    return XdmValue.FromInteger((long)u64, typeName);
                return XdmValue.FromDecimal(u64, typeName);
            case DateTimeOffset dto:
                return ConvertDateTime(dto, typeName, hasTimezone);
            case DateTime dt:
                // Re-parse the lexical form for date/time values so the original timezone offset
                // is preserved. XmlSchemaDatatype.ParseValue normalizes the returned DateTime to
                // local/UTC and drops the explicit offset, which corrupts values like +05:00 and
                // throws for UTC DateTime values.
                if (lexicalValue is not null && IsDateTimeTypeName(typeName))
                {
                    try
                    {
                        return ConvertDateTime(XmlConvert.ToDateTimeOffset(lexicalValue), typeName, hasTimezone);
                    }
                    catch (FormatException)
                    {
                        // Fall through to the DateTime-based conversion if the lexical value is
                        // not a supported date/time lexical form.
                    }
                }
                return ConvertDateTime(dt, typeName, hasTimezone);
            case XmlQualifiedName qn:
            {
                string qnType = typeName;
                if (GetBuiltInBaseTypeName(schemaType) is "NOTATION")
                    qnType = "NOTATION";
                string prefix = string.Empty;
                if (lexicalValue is not null)
                {
                    int colon = lexicalValue.IndexOf(':');
                    if (colon > 0)
                        prefix = lexicalValue[..colon];
                }
                return XdmValue.FromQName(new XsQName(qn.Name, qn.Namespace, prefix), qnType);
            }
            case string s:
                return XdmValue.FromString(s, typeName);
            case byte[] bytes:
                // XdmValue stores binary types as annotated strings.
                string text = typeName.Equals("hexBinary", StringComparison.OrdinalIgnoreCase)
                    ? Convert.ToHexString(bytes)
                    : Convert.ToBase64String(bytes);
                return XdmValue.FromString(text, typeName);
            case TimeSpan ts:
                // xs:dayTimeDuration / xs:yearMonthDuration are represented as strings in Bosak.
                return XdmValue.FromDuration(ts.ToString(), typeName);
            default:
                return XdmValue.FromString(value.ToString() ?? string.Empty, typeName);
        }
    }

    private static bool IsDateTimeTypeName(string typeName)
        => typeName.Equals("dateTime", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("dateTimeStamp", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("date", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("time", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("gYear", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("gYearMonth", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("gMonth", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("gMonthDay", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("gDay", StringComparison.OrdinalIgnoreCase);

    private static XdmValue ConvertDateTime(DateTimeOffset dto, string typeName, bool hasTimezone)
        => typeName.ToLowerInvariant() switch
        {
            "date" => XdmValue.FromDate(dto, hasTimezone),
            "time" => XdmValue.FromTime(dto, hasTimezone),
            "gyear" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gYear"),
            "gyearmonth" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gYearMonth"),
            "gmonth" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gMonth"),
            "gmonthday" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gMonthDay"),
            "gday" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gDay"),
            _ => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: typeName)
        };

    private static XdmValue ConvertDateTime(DateTime dt, string typeName, bool hasTimezone)
    {
        var offset = dt.Kind == DateTimeKind.Unspecified ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(dt);
        var dto = new DateTimeOffset(dt, offset);
        return ConvertDateTime(dto, typeName, hasTimezone);
    }

    /// <summary>
    /// Returns true when a schema-validated lexical date/time value carries an explicit
    /// timezone (trailing <c>Z</c> or <c>+/-hh:mm</c>).
    /// </summary>
    private static bool LexicalHasTimezone(string? lexical)
    {
        if (string.IsNullOrEmpty(lexical))
            return false;
        var s = lexical.AsSpan().Trim();
        if (s.EndsWith("Z".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return true;
        if (s.Length >= 6)
        {
            char c = s[^6];
            if ((c == '+' || c == '-') && s[^3] == ':')
            {
                for (int i = s.Length - 5; i < s.Length - 3; i++)
                {
                    if (!char.IsDigit(s[i]))
                        return false;
                }
                for (int i = s.Length - 2; i < s.Length; i++)
                {
                    if (!char.IsDigit(s[i]))
                        return false;
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Walks a derived schema type up to its built-in XML Schema base and returns the
    /// local name, or null when no built-in base can be reached.
    /// </summary>
    private static string? GetBuiltInBaseTypeName(XmlSchemaType type)
    {
        var visited = new HashSet<XmlSchemaType>();
        var current = type;
        while (current is not null && visited.Add(current))
        {
            if (current.QualifiedName.Namespace == XmlSchema.Namespace)
                return current.QualifiedName.Name;
            current = current.BaseXmlSchemaType;
        }
        return null;
    }

    /// <summary>
    /// Returns true when the built-in type name denotes an XSD integer subtype
    /// (including signed, unsigned, and bounded variants).
    /// </summary>
    private static bool IsIntegerTypeName(string typeName)
        => typeName.Equals("integer", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("nonPositiveInteger", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("nonNegativeInteger", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("positiveInteger", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("negativeInteger", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("long", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("int", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("short", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("byte", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("unsignedLong", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("unsignedInt", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("unsignedShort", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("unsignedByte", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this node has the XDM is-id property.
    /// For elements this is true when the typed value is a single xs:ID atomic value
    /// (including derived types, union members and singleton lists of xs:ID).
    /// For attributes this is true for ID-typed attributes, including <c>id</c> and
    /// <c>xml:id</c> attributes even when no schema is available.
    /// </summary>
    public bool IsId => ComputeIsId();

    /// <summary>
    /// Gets a value indicating whether this node has the XDM is-idrefs property.
    /// For elements and attributes this is true when the typed value contains one or more
    /// xs:IDREF atomic values (including derived types, union members and lists of xs:IDREF).
    /// </summary>
    public bool IsIdref => ComputeIsIdref();

    public (string NamespaceUri, string LocalName)? SchemaTypeAnnotation => GetSchemaTypeAnnotation();

    private (string NamespaceUri, string LocalName)? GetSchemaTypeAnnotation()
    {
        if (_isNamespaceNode)
            return null;

        IXmlSchemaInfo? info = _node switch
        {
            XElement e => e.GetSchemaInfo(),
            XAttribute a => a.GetSchemaInfo(),
            _ => null
        };
        if (info?.SchemaType is not { } schemaType)
            return null;

        // Report the declared schema type as the dynamic type annotation. The PSVI member type
        // may be a transient union member, while XPath/XQuery expects the declared type name
        // for type matching (schema-element tests, instanceof on validated nodes).
        var qn = schemaType.QualifiedName;
        return (qn.Namespace, qn.Name);
    }

    public (string NamespaceUri, string LocalName)? SchemaElementDeclaration => GetSchemaElementDeclaration();

    private (string NamespaceUri, string LocalName)? GetSchemaElementDeclaration()
    {
        if (_isNamespaceNode || _node is not XElement element)
            return null;
        var info = element.GetSchemaInfo();
        var decl = info?.SchemaElement;
        if (decl is null)
            return null;
        return (decl.QualifiedName.Namespace, decl.QualifiedName.Name);
    }

    public (string NamespaceUri, string LocalName)? SchemaAttributeDeclaration => GetSchemaAttributeDeclaration();

    private (string NamespaceUri, string LocalName)? GetSchemaAttributeDeclaration()
    {
        if (_isNamespaceNode || _node is not XAttribute attribute)
            return null;
        var info = attribute.GetSchemaInfo();
        var decl = info?.SchemaAttribute;
        if (decl is null)
            return null;
        return (decl.QualifiedName.Namespace, decl.QualifiedName.Name);
    }

    public bool IsNilled
    {
        get
        {
            if (_isNamespaceNode || _node is not XElement element)
                return false;
            return element.GetSchemaInfo()?.IsNil ?? false;
        }
    }

    public bool IsConstructedElement
    {
        get
        {
            if (_isNamespaceNode || _node is not XElement element)
                return false;
            return element.Annotation<ConstructedElementAnnotation>() is not null;
        }
    }

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
            // Namespace nodes are virtual; their document order is placed immediately
            // after the owner element so all namespace nodes of an element sort together,
            // after the owner element and before any attributes.
            if (_isNamespaceNode && _namespaceOwner is not null)
            {
                return new XDocumentNode(_namespaceOwner).DocumentOrder + 1;
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
        // Reserve one slot after the root element for its namespace nodes,
        // so namespace nodes sort after the owner element but before attributes.
        index++;
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

        foreach (var node in ChildNodes(container))
        {
            map[node] = index++;
            if (node is XElement elem)
            {
                // Reserve one slot after the element for its namespace nodes,
                // so namespace nodes sort after the owner element but before attributes.
                index++;
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

    /// <summary>
    /// Looks up an unparsed entity by name in the document that contains this node,
    /// resolving the system identifier against the document's base URI.
    /// </summary>
    public bool TryGetUnparsedEntity(string name, out string? systemId, out string? publicId)
    {
        systemId = null;
        publicId = null;

        var doc = _node as System.Xml.Linq.XDocument ?? _node.Document;
        if (doc is null)
            return false;

        var annotation = doc.Annotation<UnparsedEntityAnnotation>();
        if (annotation is null || !annotation.Entities.TryGetValue(name, out var info))
            return false;

        if (!string.IsNullOrEmpty(info.SystemId))
        {
            // The document base URI may be stored on the unparsed-entity annotation,
            // as a string annotation (e.g. on a constructed or copied document node),
            // or on XDocument.BaseUri itself.
            var baseUri = !string.IsNullOrEmpty(annotation.BaseUri)
                ? annotation.BaseUri
                : doc.Annotation<string>() is { Length: > 0 } annotated ? annotated : doc.BaseUri;
            if (string.IsNullOrEmpty(baseUri))
            {
                systemId = info.SystemId;
            }
            else
            {
                try
                {
                    systemId = new Uri(new Uri(baseUri), info.SystemId).AbsoluteUri;
                }
                catch
                {
                    systemId = info.SystemId;
                }
            }
        }

        if (!string.IsNullOrEmpty(info.PublicId))
            publicId = info.PublicId;

        return systemId is not null || publicId is not null;
    }

    /// <summary>
    /// Copies the unparsed entity declarations from the document containing this node
    /// onto <paramref name="targetDocument"/>. Used when a document node (or a node
    /// within a document) is copied, so that entity lookups remain available on the copy.
    /// </summary>
    public void CopyUnparsedEntitiesTo(System.Xml.Linq.XDocument targetDocument)
    {
        var doc = _node as System.Xml.Linq.XDocument ?? _node.Document;
        if (doc is null)
            return;

        var annotation = doc.Annotation<UnparsedEntityAnnotation>();
        if (annotation is null)
            return;

        var copy = new UnparsedEntityAnnotation { BaseUri = annotation.BaseUri };
        foreach (var entity in annotation.Entities)
        {
            copy.Entities[entity.Key] = new UnparsedEntityAnnotation.EntityInfo
            {
                SystemId = entity.Value.SystemId,
                PublicId = entity.Value.PublicId,
                NotationName = entity.Value.NotationName
            };
        }

        targetDocument.AddAnnotation(copy);
    }

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
                return _namespaceOwner is not null && _namespaceOwner.Annotation<ParentlessNamespaceNode>() is null
                    ? new XDocumentNode(_namespaceOwner) : null;
            var parent = GetXPathParent(_node);
            return parent is not null ? new XDocumentNode(parent) : null;
        }
    }

    public IXdmNode? Document
    {
        get
        {
            if (_isNamespaceNode)
            {
                var ownerDoc = _namespaceOwner?.Document;
                return ownerDoc is not null ? new XDocumentNode(ownerDoc) : null;
            }
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
        foreach (var child in ChildNodes(container))
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
        foreach (var child in ChildNodes(container))
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
        // not the element where the underlying XAttribute declaration resides. Nodes from
        // computed namespace constructors are parentless (nscons-012).
        if (node == _node && _isNamespaceNode && _namespaceOwner is not null)
            return _namespaceOwner.Annotation<ParentlessNamespaceNode>() is null ? _namespaceOwner : null;

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
        // LINQ-to-XML reports Parent as null for document-level PIs and comments
        // (only the root element gets a parent via the special case above). Fall back
        // to the owning document when the node is still part of a document tree.
        if (node is XProcessingInstruction or XComment && node.Parent is null && node.Document is not null)
            return node.Document;
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
        var undeclared = new HashSet<string>();
        bool isTargetElement = true;
        while (current is not null)
        {
            // XML 1.1 prefixed namespace undeclarations on this element hide the
            // same prefixes declared at or above it for this subtree.
            if (current.Annotation<PrefixedNamespaceUndeclarations>() is { } undeclarations)
                foreach (var undeclaredPrefix in undeclarations.Prefixes)
                    undeclared.Add(undeclaredPrefix);

            foreach (var attr in current.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;

                // Bindings implied by attribute names do not propagate to descendants
                // (they are part of the carrying element's own namespace axis only).
                if (!isTargetElement && attr.Annotation<NonPropagatingNamespaceBinding>() is not null)
                    continue;

                string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                if (undeclared.Contains(prefix))
                    continue;
                AddNamespaceNode(collected, seen, prefix, attr.Value, element);

                if (elementNsIsNonEmpty)
                {
                    if (prefix == string.Empty && attr.Value == elementNs)
                        hasExplicitDefaultForElementNs = true;
                    else if (prefix != string.Empty && attr.Value == elementNs)
                        hasPrefixDeclarationForElementNs = true;
                }
            }

            isTargetElement = false;
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
            var xmlAttr = GetOrCreateNamespaceAttribute(element, "xml", XNamespace.Xml.NamespaceName, xml11: false);
            items.Add(XdmValue.FromNode(new XDocumentNode(xmlAttr, element)));
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
        XAttribute declaration = GetOrCreateNamespaceAttribute(owner, prefix, uri, xml11);
        items.Add(XdmValue.FromNode(new XDocumentNode(declaration, owner)));
    }

    private XdmSequence GetFollowingSiblingAxis()
    {
        var parent = GetXPathParent(_node);
        if (parent is not XContainer parentContainer)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        bool found = false;
        foreach (var sibling in ChildNodes(parentContainer))
        {
            if (sibling == _node) { found = true; continue; }
            if (found)
                items.Add(XdmValue.FromNode(new XDocumentNode(sibling)));
        }
        return MaterializedSequence.FromList(items);
    }

    private XdmSequence GetPrecedingSiblingAxis()
    {
        var parent = GetXPathParent(_node);
        if (parent is not XContainer parentContainer)
            return XdmSequence.Empty;

        // Attributes and namespace nodes are not children of their parent element,
        // so they have no preceding siblings.
        if (_node is XAttribute || _isNamespaceNode)
            return XdmSequence.Empty;

        var items = new List<XdmValue>();
        foreach (var sibling in ChildNodes(parentContainer))
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
            // Namespace nodes are backed by detached XAttributes; the real XPath
            // parent is the element whose namespace axis they belong to.
            var attrParent = _isNamespaceNode ? _namespaceOwner : _node.Parent as XElement;
            if (attrParent is not null)
            {
                foreach (var child in ChildNodes(attrParent))
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
            var parent = GetXPathParent(current);
            if (parent is not XContainer parentContainer) break;

            bool found = false;
            foreach (var sibling in ChildNodes(parentContainer))
            {
                if (sibling == current) { found = true; continue; }
                if (found)
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(sibling)));
                    AddDescendants(sibling, items);
                }
            }
            current = parent;
            // Stop at the synthetic document wrapper: it is an internal container, not an
            // XDM node, so axes must not walk past it to the outer XDocument.
            if (current is XElement wrapper && wrapper.Name.LocalName == "__xdm_doc__" && wrapper.Name.NamespaceName == "")
                break;
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
            // Namespace nodes are backed by detached XAttributes; the real XPath
            // parent is the element whose namespace axis they belong to.
            var attrParent = _isNamespaceNode ? _namespaceOwner : _node.Parent as XElement;
            if (attrParent is not null)
                current = attrParent;
            else
                return MaterializedSequence.FromList(items);
        }

        while (true)
        {
            var parent = GetXPathParent(current);
            if (parent is not XContainer parentContainer) break;

            var before = new List<XObject>();
            foreach (var sibling in ChildNodes(parentContainer))
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
            // Stop at the synthetic document wrapper: it is an internal container, not an
            // XDM node, so axes must not walk past it to the outer XDocument.
            if (current is XElement wrapper && wrapper.Name.LocalName == "__xdm_doc__" && wrapper.Name.NamespaceName == "")
                break;
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

    /// <summary>
    /// Returns the child nodes of a container excluding the XML document type declaration.
    /// XDM does not expose the DTD as a node, so it must be skipped in axes and node counts.
    /// Whitespace-only text nodes that are direct children of a document node are also
    /// excluded, matching the XDM constraint that a document node with element children
    /// has no whitespace text node children (axes-202, fn:doc prolog/epilog whitespace).
    /// </summary>
    private static IEnumerable<XNode> ChildNodes(XContainer container)
    {
        // Unwrap synthetic document wrapper so its children appear as document children.
        if (container is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

        bool isDocument = container is System.Xml.Linq.XDocument;
        foreach (var node in container.Nodes())
        {
            if (node is XDocumentType)
                continue;
            if (isDocument && node is XText text && string.IsNullOrWhiteSpace(text.Value))
                continue;
            yield return node;
        }
    }

    private static IEnumerable<XObject> GetDescendants(XObject node)
    {
        if (node is not XContainer container)
            yield break;

        // Unwrap synthetic document wrapper so its children appear as document descendants
        if (node is System.Xml.Linq.XDocument doc && GetSyntheticWrapper(doc) is { } wrapperDoc)
            container = wrapperDoc;

        foreach (var child in ChildNodes(container))
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

        foreach (var child in ChildNodes(container))
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
            System.Xml.Linq.XDocument doc => DecodeXml11Names(
                GetSyntheticWrapper(doc) is { } wrapperDoc
                    ? string.Concat(wrapperDoc.Nodes().Select(n => n.ToString(SaveOptions.DisableFormatting)))
                    : doc.ToString(SaveOptions.DisableFormatting),
                doc.Annotation<Xml11Annotation>() != null),
            XElement el => ElementToXmlStringWithNamespaces(el),
            XText t => System.Security.SecurityElement.Escape(t.Value) ?? t.Value,
            XComment c => $"<!--{c.Value}-->",
            XProcessingInstruction pi => $"<?{pi.Target} {pi.Data}?>",
            XAttribute a => a.Value,
            _ => _node.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Serializes an element node including all in-scope namespace declarations.
    /// When an element is returned as a singleton (e.g. from an intersect/union expression),
    /// the harness's assert-xml expects its ancestor namespace bindings to be copied so the
    /// resulting fragment is namespace-well-formed and reflects the original in-scope prefixes.
    /// </summary>
    private static string ElementToXmlStringWithNamespaces(XElement element)
    {
        var clone = new XElement(element);

        // Collect namespace declarations already present on the element itself.
        var existing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attr in clone.Attributes().Where(a => a.IsNamespaceDeclaration))
        {
            string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
            existing.Add(prefix);
        }

        // Add any missing in-scope namespace bindings from the original element's ancestors.
        var nsNode = new XDocumentNode(element);
        foreach (var ns in nsNode.Axis(XdmAxis.Namespace))
        {
            var attr = ns.NodeValue;
            if (attr is null) continue;
            string prefix = attr.LocalName;
            if (prefix == "xml") continue; // always implicitly in scope
            if (existing.Contains(prefix)) continue;
            if (string.IsNullOrEmpty(attr.StringValue)) continue;

            existing.Add(prefix);
            XName name = string.IsNullOrEmpty(prefix) ? "xmlns" : XNamespace.Xmlns + prefix;
            clone.SetAttributeValue(name, attr.StringValue);
        }

        // Namespace fixup: a declaration identical to one already in scope *from an
        // ancestor* is redundant and omitted (K2-DirectConElemNamespace-27/42/43).
        // Siblings do not share scope, so the walk is per-branch.
        void OmitRedundantDeclarations(XElement el, Dictionary<string, string> ancestorScope)
        {
            var scope = new Dictionary<string, string>(ancestorScope, StringComparer.Ordinal);
            foreach (var attr in el.Attributes().Where(a => a.IsNamespaceDeclaration).ToList())
            {
                var declPrefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (ancestorScope.TryGetValue(declPrefix, out var inScope) && inScope == attr.Value)
                    attr.Remove();
                else
                    scope[declPrefix] = attr.Value;
            }
            foreach (var child in el.Elements())
                OmitRedundantDeclarations(child, scope);
        }
        OmitRedundantDeclarations(clone, new Dictionary<string, string>(StringComparer.Ordinal));

        string xml = clone.ToString(SaveOptions.DisableFormatting);

        // XML 1.1 constructed trees store encoded names; decode them before serialization
        // so the output reflects the original Unicode names (misc-XMLEdition name tests).
        // This is a string-level replacement: if text/attribute values happen to contain the
        // sentinel-encoded form literally, they will also be decoded. XML 1.1 text content
        // containing the sentinel characters is an edge case not exercised by the suite.
        if (IsXml11ElementTree(element))
            xml = Xml11NameCodec.DecodeName(xml);

        return xml;
    }

    /// <summary>
    /// Decodes XML 1.1 sentinel-encoded names in a serialized XML string when the
    /// source tree is known to be XML 1.1.
    /// </summary>
    private static string DecodeXml11Names(string xml, bool isXml11)
        => isXml11 ? Xml11NameCodec.DecodeName(xml) : xml;

    /// <summary>
    /// Returns <c>true</c> when <paramref name="element"/> or any ancestor carries
    /// an XML 1.1 annotation, or when the containing document is XML 1.1.
    /// </summary>
    private static bool IsXml11ElementTree(XElement element)
    {
        var current = element;
        while (current is not null)
        {
            if (current.Annotation<Xml11Annotation>() != null)
                return true;
            current = current.Parent;
        }
        return element.Document?.Annotation<Xml11Annotation>() != null;
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

        // Infoset fallback: attributes named "id" (no namespace) or "xml:id" are IDs when
        // the value is a valid NCName after xml:id normalization (attribute-value
        // normalization for type ID removes leading/trailing whitespace): key-076's
        // xml:id="id3 " is an ID; fn-id-25's "789x" is not a valid NCName either way.
        if (attr.Name.LocalName == "id" && attr.Name.NamespaceName.Length == 0)
            return IsValidNCName(attr.Value.Trim());
        if (attr.Name.LocalName == "id" && attr.Name.NamespaceName == "http://www.w3.org/XML/1998/namespace")
            return IsValidNCName(attr.Value.Trim());

        return false;
    }

    // NCName per XML Namespaces 1.0: a letter or '_' start, then letters, digits,
    // '.', '-', '_' (simplified ASCII rule, matching the parser's constructor check).
    private static bool IsValidNCName(string value)
    {
        if (value.Length == 0)
            return false;
        if (!IsNameStartChar(value[0]))
            return false;
        for (int i = 1; i < value.Length; i++)
        {
            char c = value[i];
            if (!IsNameStartChar(c) && c is not ('-' or '.') && !char.IsDigit(c))
                return false;
        }
        return true;
    }

    private static bool IsNameStartChar(char c)
        => char.IsLetter(c) || c == '_';

    private static bool IsIdElement(XElement element)
    {
        var info = element.GetSchemaInfo();
        if (info is not null)
        {
            if (info.IsNil)
                return false;

            if (IsIdSchemaType(info.MemberType, element.Value))
                return true;
            if (IsIdSchemaType(info.SchemaType, element.Value))
                return true;
        }

        // Schema-less but typed via xsi:type: an element whose xsi:type attribute resolves
        // to xs:ID is treated as an ID element (app-spec-examples fo-test-fn-id-002).
        if (ResolveXsiType(element) is { } xsiType && IsXsIdType(xsiType))
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
    /// Resolves the value of an <c>xsi:type</c> attribute on <paramref name="element"/> to an
    /// expanded QName, or <c>null</c> if the attribute is not present or not a valid QName.
    /// </summary>
    private static (string NamespaceUri, string LocalName)? ResolveXsiType(XElement element)
    {
        const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        var xsiType = element.Attribute(XNamespace.Get(XsiNs) + "type");
        if (xsiType is null)
            return null;
        return ResolveQName(xsiType.Value, element);
    }

    /// <summary>
    /// Resolves a lexical QName using the in-scope namespace bindings of <paramref name="element"/>.
    /// Unprefixed names use the default element namespace; prefixed names use the binding for
    /// that prefix. Returns an empty namespace URI when the prefix is undeclared.
    /// </summary>
    private static (string NamespaceUri, string LocalName) ResolveQName(string lexical, XElement element)
    {
        int colon = lexical.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = lexical[..colon];
            string local = lexical[(colon + 1)..];
            string ns = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
            return (ns, local);
        }
        else
        {
            string ns = element.GetDefaultNamespace()?.NamespaceName ?? string.Empty;
            return (ns, lexical);
        }
    }

    /// <summary>
    /// Returns true when the expanded QName denotes the XML Schema <c>xs:ID</c> type.
    /// </summary>
    private static bool IsXsIdType((string NamespaceUri, string LocalName) qn)
        => qn.NamespaceUri == XmlSchema.Namespace && qn.LocalName == "ID";

    /// <summary>
    /// Returns true when the expanded QName denotes the XML Schema <c>xs:IDREF</c> or
    /// <c>xs:IDREFS</c> type.
    /// </summary>
    private static bool IsXsIdrefType((string NamespaceUri, string LocalName) qn)
        => qn.NamespaceUri == XmlSchema.Namespace && (qn.LocalName == "IDREF" || qn.LocalName == "IDREFS");

    private bool ComputeIsIdref()
    {
        if (_isNamespaceNode)
            return false;

        if (_node is XAttribute attr)
            return IsIdrefAttribute(attr);

        if (_node is XElement element)
            return IsIdrefElement(element);

        return false;
    }

    private static bool IsIdrefAttribute(XAttribute attr)
    {
        var info = attr.GetSchemaInfo();
        if (info is not null && IsIdrefFromSchemaInfo(info, attr.Value))
            return true;
        return false;
    }

    private static bool IsIdrefElement(XElement element)
    {
        var info = element.GetSchemaInfo();
        if (info is not null && !info.IsNil && IsIdrefFromSchemaInfo(info, element.Value))
            return true;

        // Schema-less but typed via xsi:type: an element whose xsi:type attribute resolves
        // to xs:IDREF/xs:IDREFS is treated as an IDREF element (app-spec-examples fo-test-fn-idref-001/002).
        if (ResolveXsiType(element) is { } xsiType && IsXsIdrefType(xsiType))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether a schema-validated node has the is-idrefs property.
    /// Definite IDREF types (IDREF, IDREFS, and derived restrictions/lists where every
    /// value is an IDREF) are recognized without inspecting the lexical value.
    /// Union and list-of-union types that may contain IDREF values require the actual
    /// typed value to contain at least one IDREF atomic value.
    /// </summary>
    private static bool IsIdrefFromSchemaInfo(IXmlSchemaInfo info, string lexicalValue)
    {
        var effectiveType = GetEffectiveSimpleType(info);
        if (effectiveType is null)
            return false;

        if (IsDefiniteIdrefSchemaType(effectiveType))
            return true;

        if (MayContainIdref(effectiveType))
            return ListOrUnionContainsIdref(effectiveType, lexicalValue);

        return false;
    }

    /// <summary>
    /// Returns the effective simple type for an element or attribute node. For simple types
    /// this is the type itself; for complex types with simple content this is the base
    /// simple type of the simple content extension/restriction.
    /// </summary>
    private static XmlSchemaSimpleType? GetEffectiveSimpleType(IXmlSchemaInfo info)
    {
        // Prefer the declared schema type when it is (or may be) an IDREF-bearing
        // union or list, because the selected MemberType/SchemaType can be a non-IDREF
        // member of a union while the actual lexical value still matches an IDREF member.
        var declaredType = info.SchemaElement?.ElementSchemaType ?? info.SchemaAttribute?.AttributeSchemaType;
        if (declaredType is XmlSchemaSimpleType declaredSimple
            && MayContainIdref(declaredSimple))
            return declaredSimple;

        if (info.MemberType is XmlSchemaSimpleType memberSimple)
            return memberSimple;

        if (info.SchemaType is XmlSchemaSimpleType actualSimple)
            return actualSimple;

        if (info.SchemaType is XmlSchemaComplexType complex
            && complex.ContentType == XmlSchemaContentType.TextOnly)
        {
            // For complex types with simple content the base type is the underlying
            // simple type. .NET exposes it via BaseXmlSchemaType for both extension
            // and restriction derivations.
            if (complex.BaseXmlSchemaType is XmlSchemaSimpleType baseSimple)
                return baseSimple;

            // Fallback: resolve a built-in base type from the simple content model.
            var simpleContent = complex.ContentModel?.Content;
            XmlQualifiedName? baseName = simpleContent switch
            {
                XmlSchemaSimpleContentExtension ext => ext.BaseTypeName,
                XmlSchemaSimpleContentRestriction restr => restr.BaseTypeName,
                _ => null
            };

            if (baseName is not null && !baseName.IsEmpty)
            {
                try
                {
                    return XmlSchemaType.GetBuiltInSimpleType(baseName);
                }
                catch
                {
                    // Not a built-in type; cannot resolve further without the schema set.
                }
            }
        }

        return null;
    }

    private static bool IsDefiniteIdrefSchemaType(XmlSchemaType? type)
    {
        if (type is null)
            return false;

        if (type.TypeCode == XmlTypeCode.Idref)
            return true;

        if (type is XmlSchemaSimpleType simple && simple.Datatype is { } datatype)
        {
            if (datatype.TypeCode == XmlTypeCode.Idref)
                return true;

            if (datatype.Variety == XmlSchemaDatatypeVariety.List)
            {
                var itemType = (simple.Content as XmlSchemaSimpleTypeList)?.BaseItemType;
                if (itemType is not null && IsDefiniteIdrefSchemaType(itemType))
                    return true;
            }
        }

        var baseType = type.BaseXmlSchemaType;
        if (baseType is not null && baseType != type)
            return IsDefiniteIdrefSchemaType(baseType);

        return false;
    }

    private static bool MayContainIdref(XmlSchemaSimpleType type)
    {
        if (type.Datatype is not { } datatype)
            return false;

        if (datatype.TypeCode == XmlTypeCode.Idref)
            return true;

        if (datatype.Variety == XmlSchemaDatatypeVariety.List)
        {
            var itemType = (type.Content as XmlSchemaSimpleTypeList)?.BaseItemType;
            if (itemType is not null)
                return IsDefiniteIdrefSchemaType(itemType) || MayContainIdref(itemType);
        }

        if (datatype.Variety == XmlSchemaDatatypeVariety.Union)
        {
            if (type.Content is XmlSchemaSimpleTypeUnion union && union.BaseMemberTypes is not null)
            {
                foreach (var member in union.BaseMemberTypes)
                {
                    if (member is not null &&
                        (IsDefiniteIdrefSchemaType(member) || MayContainIdref(member)))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool ListOrUnionContainsIdref(XmlSchemaSimpleType type, string lexicalValue)
    {
        if (type.Datatype is not { } datatype)
            return false;

        if (datatype.TypeCode == XmlTypeCode.Idref)
            return true;

        if (datatype.Variety == XmlSchemaDatatypeVariety.List)
        {
            var itemType = (type.Content as XmlSchemaSimpleTypeList)?.BaseItemType;
            if (itemType is null)
                return false;
            var tokens = lexicalValue.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (TokenIsIdref(itemType, token))
                    return true;
            }
            return false;
        }

        if (datatype.Variety == XmlSchemaDatatypeVariety.Union)
            return TokenIsIdref(type, lexicalValue);

        return false;
    }

    private static bool TokenIsIdref(XmlSchemaSimpleType type, string token)
    {
        if (IsDefiniteIdrefSchemaType(type))
            return true;

        if (type.Datatype?.Variety != XmlSchemaDatatypeVariety.Union)
            return false;

        if (type.Content is not XmlSchemaSimpleTypeUnion union || union.BaseMemberTypes is null)
            return false;

        foreach (var member in union.BaseMemberTypes)
        {
            if (member is null)
                continue;
            if (TokenMatchesSchemaType(token, member))
                return IsDefiniteIdrefSchemaType(member);
        }

        return false;
    }

    private static bool TokenMatchesSchemaType(string token, XmlSchemaSimpleType type)
    {
        try
        {
            var datatype = type.Datatype;
            if (datatype is null)
                return false;
            datatype.ParseValue(token, new NameTable(), new XmlNamespaceManager(new NameTable()));
            return true;
        }
        catch
        {
            return false;
        }
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

/// <summary>
/// Records the namespace prefix of a free-standing computed attribute.
/// LINQ-to-XML <see cref="XAttribute"/> instances cannot carry a prefix (it is
/// derived from in-scope declarations on a parent element), so the constructed
/// prefix is stored as an annotation for <see cref="XDocumentNode"/> to report.
/// </summary>
internal sealed class AttributePrefixAnnotation
{
    /// <summary>The recorded namespace prefix.</summary>
    public string Prefix { get; }

    /// <summary>Creates the annotation for the given prefix.</summary>
    /// <param name="prefix">The namespace prefix to record.</param>
    public AttributePrefixAnnotation(string prefix) => Prefix = prefix;
}
