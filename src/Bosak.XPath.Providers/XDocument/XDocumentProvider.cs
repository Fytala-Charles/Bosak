// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Extension methods to convert LINQ to XML objects into IXdmNode adapters
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 05-06-2026     | Preserve whitespace in elements; strip document-level whitespace-only text nodes        |
//                      | Charles Korthout | 0.3   | 25-06-2026     | LoadXml sets DocumentUri on returned document node                                      |
//                      | Charles Korthout | 0.4   | 15-07-2026     | LoadXml absolutizes relative paths before building the document URI (UriFormatException)|
//                      | Charles Korthout | 0.5   | 15-07-2026     | Added LoadXml overload with explicit baseUri for published resource URIs                |
//                      | Charles Korthout | 0.6   | 19-07-2026     | Added LoadXml overload with optional XML Schema validation and PSVI annotations         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 25-07-2026     | Added ConstructElement for XQuery element constructors (ElementConstructorHook)        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 25-07-2026     | Added ConstructContentNode; xmlns:xml redundancy and xmlns:xmlns rejection             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 25-07-2026     | Namespace fixup; in-scope namespace copying on clones; base-URI annotation             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 25-07-2026     | ConstructAttribute/ConstructDocument for computed constructors; namespace declarations and text content nodes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 25-07-2026     | Preserve XML 1.1 undeclaration annotations across the base-URI reparse                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 28-07-2026     | Non-propagating namespace-binding markers for attribute-name bindings; no content fixup; clone annotations |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 29-07-2026     | Content ns declarations win over name prefixes (generated prefixes); xmlns:xml omitted; parentless ns nodes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 07-08-2026     | Strict validation strips whitespace-only text nodes in element-only schema content (ForExprType009) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13  | 17-08-2026     | ConstructElement handles clashing attribute prefixes via generated prefixes and attribute annotations (cbcl-ns-fixup-1) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Xml;


/// <summary>
/// Provides extension methods to adapt LINQ to XML types to the XDM <see cref="IXdmNode"/> interface.
/// </summary>
public static class XDocumentProvider
{
    /// <summary>
    /// Adapts an <see cref="XDocument"/> to <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ToXdmNode(this System.Xml.Linq.XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Adapts an <see cref="XElement"/> to <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ToXdmNode(this XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var doc = element.Document;
        if (doc is not null)
        {
            var map = ComputeDocumentOrder(doc);
            XDocumentNode.RegisterOrderMap(doc, map);
        }
        return new XDocumentNode(element);
    }

    /// <summary>
    /// Builds an XDocument-backed element node from provider-neutral construction data
    /// (the <see cref="Bosak.XPath.Runtime.Vm.EvaluationContext.ElementConstructorHook"/>).
    /// Prefixes used by the tag and attributes are declared on the element so that
    /// serialization honors them; existing nodes are deep-copied into the new tree.
    /// </summary>
    /// <param name="spec">The provider-neutral element construction data.</param>
    /// <returns>The constructed element node.</returns>
    public static IXdmNode ConstructElement(XdmElementSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        XNamespace ns = spec.NamespaceUri is null ? XNamespace.None : XNamespace.Get(spec.NamespaceUri);
        var element = new XElement(ns + spec.LocalName);

        // The constructed element's base URI is the static base URI of the query
        // (honored by XDocumentNode.ResolveXmlBase through the string annotation).
        if (!string.IsNullOrEmpty(spec.BaseUri))
            element.AddAnnotation(spec.BaseUri);

        // Declare prefixes so serialization uses the source prefixes rather than generated ones.
        // Explicit xmlns declarations in the constructor always win over generated ones.
        var declared = new HashSet<string>(StringComparer.Ordinal);          // reserved prefixes
        var declaredUris = new Dictionary<string, string>(StringComparer.Ordinal); // prefix -> URI bindings on the element
        foreach (var attr in spec.Attributes)
        {
            if (attr.Prefix == "xmlns")
                declared.Add(attr.LocalName);
            else if (attr.LocalName == "xmlns" && attr.Prefix is null)
                declared.Add("");
        }

        // Computed namespace declarations in content take precedence over name-implied
        // bindings: a tag or attribute name whose prefix they redeclare with a different
        // URI gets a generated prefix instead (nscons-010/011).
        var contentNsDecls = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in spec.Content)
        {
            if (item.Kind == XdmContentKind.Namespace)
            {
                var declPrefix = item.Target ?? string.Empty;
                if (!contentNsDecls.ContainsKey(declPrefix))
                    contentNsDecls[declPrefix] = item.Text ?? string.Empty;
            }
        }
        string GeneratePrefix()
        {
            // Only probe membership: Declare() performs the declared.Add itself
            // (adding here would suppress the declaration entirely).
            for (int i = 1; ; i++)
            {
                var candidate = $"ns{i}";
                if (!declared.Contains(candidate) && !declaredUris.ContainsKey(candidate))
                    return candidate;
            }
        }

        // Adds a namespace declaration for the supplied prefix/URI if one is needed and
        // returns the actual prefix used. If the requested prefix is already bound to a
        // different URI, a generated prefix is allocated for this URI instead.
        string? Declare(string? prefix, string? nsUri, bool impliedByAttributeName = false)
        {
            if (string.IsNullOrEmpty(nsUri))
                return null;

            if (prefix is null)
            {
                if (declared.Contains(""))
                {
                    // Default namespace will be supplied by the explicit xmlns attribute;
                    // keep the tag unprefixed.
                    return "";
                }
                if (declaredUris.TryGetValue("", out var existingDefaultUri))
                {
                    return existingDefaultUri == nsUri ? "" : null;
                }
                if (declaredUris.TryAdd("", nsUri))
                {
                    element.Add(new XAttribute("xmlns", nsUri));
                    declared.Add("");
                    return "";
                }
            }

            if (prefix is ("xml" or "xmlns"))
                return null;

            // An explicit xmlns:prefix attribute in the constructor reserves that prefix.
            // The URI is assumed to match; if it does not, the query is in error and the
            // explicit declaration wins.
            string p = prefix!;

            // An explicit xmlns:prefix attribute in the constructor reserves that prefix.
            // The URI is assumed to match; if it does not, the query is in error and the
            // explicit declaration wins.
            if (declared.Contains(p) && !declaredUris.ContainsKey(p))
                return p;

            if (declaredUris.TryAdd(p, nsUri))
            {
                var declAttr = new XAttribute(XNamespace.Xmlns + p, nsUri);
                // Bindings implied by an attribute's name are part of this element's
                // in-scope namespaces but do not propagate to nested constructors.
                if (impliedByAttributeName)
                    declAttr.AddAnnotation(new NonPropagatingNamespaceBinding());
                element.Add(declAttr);
                declared.Add(p);
                return p;
            }

            if (declaredUris[p] == nsUri)
                return p;

            // Prefix is bound to a different URI; reuse an existing prefix for this URI
            // or generate a new one.
            foreach (var (existingPrefix, existingUri) in declaredUris)
            {
                if (existingUri == nsUri)
                    return existingPrefix;
            }

            string newPrefix = GeneratePrefix();
            var newDeclAttr = new XAttribute(XNamespace.Xmlns + newPrefix, nsUri);
            if (impliedByAttributeName)
                newDeclAttr.AddAnnotation(new NonPropagatingNamespaceBinding());
            element.Add(newDeclAttr);
            declaredUris[newPrefix] = nsUri;
            declared.Add(newPrefix);
            return newPrefix;
        }

        // Tag: a conflicting name-implied prefix is replaced by a generated one.
        if (spec.Prefix is not null && contentNsDecls.TryGetValue(spec.Prefix, out var tagDeclUri)
            && tagDeclUri != (spec.NamespaceUri ?? string.Empty))
            Declare(GeneratePrefix(), spec.NamespaceUri);
        else
            Declare(spec.Prefix, spec.NamespaceUri);

        foreach (var attr in spec.Attributes)
        {
            if (attr.Prefix == "xmlns")
            {
                // xmlns:xml is redundant (the xml prefix is implicitly bound);
                // xmlns:xmlns is a reserved-namespace error (XQDY0074).
                if (attr.LocalName == "xmlns")
                    throw new InvalidOperationException("XQDY0074: The 'xmlns' prefix must not be declared.");
                if (attr.LocalName != "xml")
                    element.Add(new XAttribute(XNamespace.Xmlns + attr.LocalName, attr.Value));
            }
            else if (attr.LocalName == "xmlns" && attr.Prefix is null)
            {
                element.Add(new XAttribute("xmlns", attr.Value));
            }
            else
            {
                // Attribute name: a prefix redeclared by a content namespace declaration
                // with a different URI is replaced by a generated prefix (nscons-010).
                string? actualPrefix;
                if (attr.Prefix is not null && contentNsDecls.TryGetValue(attr.Prefix, out var attrDeclUri)
                    && attrDeclUri != (attr.NamespaceUri ?? string.Empty))
                    actualPrefix = Declare(GeneratePrefix(), attr.NamespaceUri, impliedByAttributeName: true);
                else
                    actualPrefix = Declare(attr.Prefix, attr.NamespaceUri, impliedByAttributeName: true);
                XNamespace ans = attr.NamespaceUri is null ? XNamespace.None : XNamespace.Get(attr.NamespaceUri);
                var newAttr = new XAttribute(ans + attr.LocalName, attr.Value);
                if (!string.IsNullOrEmpty(actualPrefix))
                    newAttr.AddAnnotation(new AttributePrefixAnnotation(actualPrefix));
                element.Add(newAttr);
            }
        }

        string? pendingText = null;
        void FlushText()
        {
            if (pendingText is not null)
            {
                element.Add(pendingText);
                pendingText = null;
            }
        }

        foreach (var item in spec.Content)
        {
            switch (item.Kind)
            {
                case XdmContentKind.Text:
                    pendingText = pendingText is null ? item.Text : pendingText + item.Text;
                    break;
                case XdmContentKind.Comment:
                    FlushText();
                    element.Add(new XComment(item.Text ?? string.Empty));
                    break;
                case XdmContentKind.ProcessingInstruction:
                    FlushText();
                    element.Add(new XProcessingInstruction(item.Target ?? string.Empty, item.Text ?? string.Empty));
                    break;
                case XdmContentKind.Namespace:
                    FlushText();
                    var nsDeclPrefix = item.Target ?? string.Empty;
                    var nsDeclUri = item.Text ?? string.Empty;
                    // xmlns:xml is redundant: the xml prefix is implicitly bound (nscons-004).
                    if (nsDeclPrefix == "xml")
                        break;
                    // Duplicate declarations with the same URI merge silently (nscons-005/006);
                    // different-URI redeclarations were already rejected by the runtime (XQDY0102).
                    if (nsDeclPrefix.Length == 0)
                    {
                        if (!declared.Contains("") && declaredUris.TryAdd("", nsDeclUri))
                        {
                            element.Add(new XAttribute("xmlns", nsDeclUri));
                            declared.Add("");
                        }
                    }
                    else if (!declared.Contains(nsDeclPrefix) && declaredUris.TryAdd(nsDeclPrefix, nsDeclUri))
                    {
                        element.Add(new XAttribute(XNamespace.Xmlns + nsDeclPrefix, nsDeclUri));
                        declared.Add(nsDeclPrefix);
                    }
                    break;
                default:
                    FlushText();
                    var childNode = CloneNode(item.Node!.Value);
                    // XQuery: a constructed child element keeps its own computed in-scope
                    // namespace bindings — they are not "redundant" with the parent's
                    // (K2-NameTest-30/31), so no namespace fixup is applied here.
                    element.Add(childNode);
                    break;
            }
        }
        FlushText();

        return new XDocumentNode(element);
    }

    private static XElement ParentlessOwner()
    {
        var owner = new XElement("__ns_owner__");
        owner.AddAnnotation(new ParentlessNamespaceNode());
        return owner;
    }

    /// <summary>
    /// Builds an XDocument-backed comment or processing-instruction node
    /// (the <see cref="Bosak.XPath.Runtime.Vm.EvaluationContext.ContentNodeConstructorHook"/>).
    /// </summary>
    /// <param name="item">The content item to construct.</param>
    /// <returns>The constructed node.</returns>
    public static IXdmNode ConstructContentNode(XdmContentItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Kind switch
        {
            XdmContentKind.Text => new XDocumentNode(new XText(item.Text ?? string.Empty)),
            XdmContentKind.Comment => new XDocumentNode(new XComment(item.Text ?? string.Empty)),
            XdmContentKind.ProcessingInstruction => new XDocumentNode(new XProcessingInstruction(item.Target ?? string.Empty, item.Text ?? string.Empty)),
            XdmContentKind.Namespace => XDocumentNode.CreateNamespaceNode(
                string.IsNullOrEmpty(item.Target)
                    ? new XAttribute("xmlns", item.Text ?? string.Empty)
                    : new XAttribute(XNamespace.Xmlns + item.Target, item.Text ?? string.Empty),
                // Computed namespace constructors create parentless nodes (nscons-012).
                ParentlessOwner()),
            _ => throw new ArgumentException($"ConstructContentNode does not handle kind '{item.Kind}'.", nameof(item))
        };
    }

    /// <summary>
    /// Removes namespace declarations from a child element that are identical to bindings
    /// already in scope at the parent (XML namespace fixup: redundant re-declarations are
    /// omitted from the output).
    /// </summary>
    private static void ApplyNamespaceFixup(XElement parent, XElement child)
    {
        var inScope = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var ancestor in parent.AncestorsAndSelf())
        {
            foreach (var attr in ancestor.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;
                string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                inScope.TryAdd(prefix, attr.Value);
            }
        }

        var redundant = child.Attributes()
            .Where(a => a.IsNamespaceDeclaration)
            .Where(a =>
            {
                string prefix = a.Name.LocalName == "xmlns" ? string.Empty : a.Name.LocalName;
                return inScope.TryGetValue(prefix, out var uri) && uri == a.Value;
            })
            .ToList();
        foreach (var attr in redundant)
            attr.Remove();
    }

    /// <summary>
    /// Builds an XDocument-backed free-standing attribute node for a computed attribute
    /// constructor (the <see cref="Bosak.XPath.Runtime.Vm.EvaluationContext.AttributeConstructorHook"/>).
    /// </summary>
    /// <param name="attribute">The computed attribute (name and value).</param>
    /// <returns>The constructed attribute node.</returns>
    public static IXdmNode ConstructAttribute(XdmAttributeValue attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        XNamespace ns = attribute.NamespaceUri is null ? XNamespace.None : XNamespace.Get(attribute.NamespaceUri);
        var xattr = new XAttribute(ns + attribute.LocalName, attribute.Value);
        // LINQ attributes cannot carry a prefix; record it as an annotation so the
        // free-standing attribute still reports its constructed prefix.
        if (!string.IsNullOrEmpty(attribute.Prefix))
            xattr.AddAnnotation(new AttributePrefixAnnotation(attribute.Prefix));
        return new XDocumentNode(xattr);
    }

    /// <summary>
    /// Builds an XDocument-backed document node for a computed document constructor
    /// (the <see cref="Bosak.XPath.Runtime.Vm.EvaluationContext.DocumentConstructorHook"/>).
    /// </summary>
    /// <param name="content">The document content items.</param>
    /// <returns>The constructed document node.</returns>
    public static IXdmNode ConstructDocument(IReadOnlyList<XdmContentItem> content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // LINQ documents allow a single root element; XDM document nodes allow any content.
        // Use the engine's synthetic document-root wrapper for anything else.
        bool singleElementRoot = content.Count == 1 && content[0].Kind == XdmContentKind.Node &&
            content[0].Node!.Value.NodeValue.NodeKind == XdmNodeKind.Element;
        var document = new System.Xml.Linq.XDocument();
        XContainer container = document;
        if (!singleElementRoot)
        {
            var wrapper = new XElement("__xdm_doc__");
            document.Add(wrapper);
            container = wrapper;
        }

        string? pendingText = null;
        void FlushText()
        {
            if (pendingText is not null)
            {
                container.Add(pendingText);
                pendingText = null;
            }
        }

        foreach (var item in content)
        {
            switch (item.Kind)
            {
                case XdmContentKind.Text:
                    pendingText = pendingText is null ? item.Text : pendingText + item.Text;
                    break;
                case XdmContentKind.Comment:
                    FlushText();
                    container.Add(new XComment(item.Text ?? string.Empty));
                    break;
                case XdmContentKind.ProcessingInstruction:
                    FlushText();
                    container.Add(new XProcessingInstruction(item.Target ?? string.Empty, item.Text ?? string.Empty));
                    break;
                default:
                    FlushText();
                    container.Add(CloneNode(item.Node!.Value));
                    break;
            }
        }
        FlushText();
        return new XDocumentNode(document);
    }

    private static XNode CloneNode(XdmValue nodeValue)
    {
        var node = nodeValue.NodeValue;
        XNode clone;
        if (node is XDocumentNode xDocumentNode)
        {
            clone = xDocumentNode.UnderlyingObject switch
            {
                XElement element => CloneWithAttributeAnnotations(element),
                System.Xml.Linq.XDocument document => new XDocument(document),
                XText text => new XText(text.Value),
                XComment comment => new XComment(comment.Value),
                XProcessingInstruction pi => new XProcessingInstruction(pi.Target, pi.Data),
                XAttribute => throw new InvalidOperationException("FOTY0013: Cannot copy a free-standing attribute node."),
                _ => XElement.Parse(node.StringValue)
            };
        }
        else
        {
            // Non-XDocument providers: round-trip through the serialized form.
            clone = XElement.Parse(node.StringValue);
        }

        // A copied element carries the in-scope namespace declarations it needs
        // (prefixes used by its name, attribute names, and subtree).
        if (clone is XElement cloneElement && node is XDocumentNode sourceNode &&
            sourceNode.UnderlyingObject is XElement sourceElement)
        {
            CopyInScopeNamespaces(sourceElement, cloneElement);
            // Preserve annotation-based semantics: the namespace inheritance barrier,
            // prefixed namespace undeclarations, and the base-URI annotation.
            foreach (var annotation in sourceElement.Annotations<object>())
                cloneElement.AddAnnotation(annotation);
        }
        return clone;
    }

    // Clones an element; the XElement copy constructor does not carry attribute
    // annotations, so non-propagating namespace-binding markers are re-applied.
    private static XElement CloneWithAttributeAnnotations(XElement source)
    {
        var clone = new XElement(source);
        foreach (var sourceAttr in source.Attributes())
        {
            if (sourceAttr.Annotation<NonPropagatingNamespaceBinding>() is null)
                continue;
            var cloneAttr = clone.Attribute(sourceAttr.Name);
            cloneAttr?.AddAnnotation(new NonPropagatingNamespaceBinding());
        }
        return clone;
    }

    private static void CopyInScopeNamespaces(XElement source, XElement clone)
    {
        // Declarations in scope at the source element (own + inherited) are part of the copy.
        var declared = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var ancestor in source.AncestorsAndSelf())
        {
            foreach (var attr in ancestor.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;
                string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                declared.TryAdd(prefix, attr.Value);
            }
        }

        var declaredOnClone = new HashSet<string>(
            clone.Attributes().Where(a => a.IsNamespaceDeclaration)
                .Select(a => a.Name.LocalName == "xmlns" ? string.Empty : a.Name.LocalName),
            StringComparer.Ordinal);
        foreach (var (prefix, uri) in declared)
        {
            if (declaredOnClone.Contains(prefix))
                continue;
            if (prefix.Length == 0)
                clone.Add(new XAttribute("xmlns", uri));
            else if (prefix is not ("xml" or "xmlns"))
                clone.Add(new XAttribute(XNamespace.Xmlns + prefix, uri));
        }
    }

    /// <summary>
    /// Parses an XML string and returns the root as an <see cref="IXdmNode"/>.
    /// XML 1.1 declarations are accepted by encoding name characters that .NET rejects.
    /// </summary>
    public static IXdmNode ParseXml(string xml)
    {
        var document = Xml11Loader.Parse(xml, LoadOptions.PreserveWhitespace);
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Parses an XML string that is known to be XML 1.1 and returns the root as an <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ParseXml11(string xml, string? baseUri = null)
    {
        var document = Xml11Loader.ParseXml11(xml, LoadOptions.PreserveWhitespace, baseUri);
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Loads an XML file and returns the root as an <see cref="IXdmNode"/>.
    /// The file path is preserved as the document's base URI.
    /// XML 1.1 declarations are accepted by encoding name characters that .NET rejects.
    /// </summary>
    public static IXdmNode LoadXml(string filePath)
        => LoadXml(filePath, baseUri: null);

    /// <summary>
    /// Loads an XML file and returns the root as an <see cref="IXdmNode"/>.
    /// When <paramref name="baseUri"/> is supplied, it is used as the document's base URI
    /// instead of the file path. This is used by test harnesses that publish a source
    /// document under a different URI than its local file location.
    /// XML 1.1 declarations are accepted by encoding name characters that .NET rejects.
    /// </summary>
    public static IXdmNode LoadXml(string filePath, string? baseUri)
    {
        var document = Xml11Loader.Load(filePath, LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace);
        StripDocumentLevelWhitespace(document);
        if (!string.IsNullOrEmpty(baseUri))
        {
            // Reparse with the published URI so element BaseUri values reflect it;
            // XML 1.1 namespace-undeclaration annotations do not survive ToString(),
            // so they are transferred explicitly.
            var originals = document.Descendants().ToList();
            document = Xml11Loader.Parse(document.ToString(),
                LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace, baseUri);
            StripDocumentLevelWhitespace(document);
            var reparsed = document.Descendants().ToList();
            for (int i = 0; i < originals.Count; i++)
            {
                if (originals[i].Annotation<PrefixedNamespaceUndeclarations>() is { } undeclarations)
                    reparsed[i].AddAnnotation(undeclarations);
            }
        }
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        var node = new XDocumentNode(document);
        // Relative paths must be absolutized first: new Uri(relativePath) throws UriFormatException.
        var absolutePath = Uri.IsWellFormedUriString(filePath, UriKind.Absolute)
            ? filePath
            : Path.GetFullPath(filePath);
        node.SetDocumentUri(new Uri(absolutePath).AbsoluteUri);
        return node;
    }

    /// <summary>
    /// Loads an XML file, optionally validates it against the supplied XML Schema set,
    /// and returns the root as an <see cref="IXdmNode"/>.
    /// When <paramref name="baseUri"/> is supplied, it is used as the document's base URI
    /// instead of the file path. PSVI annotations are added to the tree so that
    /// <see cref="XDocumentNode.IsId"/> reflects the schema types.
    /// </summary>
    public static IXdmNode LoadXml(string filePath, string? baseUri, XmlSchemaSet? schemaSet)
    {
        var document = Xml11Loader.Load(filePath, LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace);
        StripDocumentLevelWhitespace(document);
        if (!string.IsNullOrEmpty(baseUri))
        {
            // See LoadXml(filePath, baseUri): preserve XML 1.1 undeclaration annotations
            // across the reparse for the published base URI.
            var originals = document.Descendants().ToList();
            document = Xml11Loader.Parse(document.ToString(),
                LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace, baseUri);
            StripDocumentLevelWhitespace(document);
            var reparsed = document.Descendants().ToList();
            for (int i = 0; i < originals.Count; i++)
            {
                if (originals[i].Annotation<PrefixedNamespaceUndeclarations>() is { } undeclarations)
                    reparsed[i].AddAnnotation(undeclarations);
            }
        }
        if (schemaSet is not null)
        {
            ValidateDocument(document, schemaSet);
        }
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        var node = new XDocumentNode(document);
        var absolutePath = Uri.IsWellFormedUriString(filePath, UriKind.Absolute)
            ? filePath
            : Path.GetFullPath(filePath);
        node.SetDocumentUri(new Uri(absolutePath).AbsoluteUri);
        return node;
    }

    private static void ValidateDocument(XDocument document, XmlSchemaSet schemaSet)
    {
        var errors = new List<string>();
        document.Validate(schemaSet, (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                errors.Add(e.Message);
        }, addSchemaInfo: true);
        if (errors.Count > 0)
        {
            throw new XmlSchemaValidationException(
                $"Document validation failed against the supplied schema(s):\n{string.Join("\n", errors)}");
        }
        StripElementOnlyContentWhitespace(document);
    }

    /// <summary>
    /// Removes whitespace-only text nodes from elements whose validated schema type has
    /// element-only content. A validating XDM construction discards such whitespace
    /// (XDM §3.3.1.1), so strictly validated source documents match the whitespace-free
    /// trees the QT3 expectations assume (ForExprType009, orderData.xml).
    /// </summary>
    private static void StripElementOnlyContentWhitespace(XDocument document)
    {
        foreach (var element in document.Descendants())
        {
            if (element.GetSchemaInfo()?.SchemaElement?.ElementSchemaType is not XmlSchemaComplexType complexType
                || complexType.ContentType != XmlSchemaContentType.ElementOnly)
                continue;
            foreach (var text in element.Nodes().OfType<XText>()
                         .Where(t => string.IsNullOrWhiteSpace(t.Value))
                         .ToList())
            {
                text.Remove();
            }
        }
    }

    /// <summary>
    /// Removes whitespace-only text nodes that are direct children of the document node.
    /// XPath/XQuery processors typically preserve whitespace inside elements but strip
    /// insignificant whitespace before/after the root element.
    /// </summary>
    public static void StripDocumentLevelWhitespace(System.Xml.Linq.XDocument doc)
    {
        var toRemove = doc.Nodes()
            .OfType<System.Xml.Linq.XText>()
            .Where(t => string.IsNullOrWhiteSpace(t.Value))
            .ToList();
        foreach (var node in toRemove)
            node.Remove();
    }

    // ------------------------------------------------------------------
    // Document order indexing
    // ------------------------------------------------------------------

    private static Dictionary<XObject, long> ComputeDocumentOrder(System.Xml.Linq.XDocument doc)
    {
        var map = new Dictionary<XObject, long>();
        long index = 0;

        map[doc] = index++;
        Traverse(doc, ref index, map);

        return map;
    }

    private static void Traverse(XContainer container, ref long index, Dictionary<XObject, long> map)
    {
        foreach (var node in container.Nodes())
        {
            map[node] = index++;

            if (node is XElement el)
            {
                foreach (var attr in el.Attributes())
                    map[attr] = index++;

                Traverse(el, ref index, map);
            }
        }
    }
}
