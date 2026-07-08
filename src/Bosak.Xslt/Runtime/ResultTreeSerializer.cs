// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Serializes an XDM result tree to an XML string.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added OutputProperties support for xsl:output (method, indent, omit-declaration)       |
//                      | Charles Korthout | 0.3   | 01-06-2026     | Encoding-aware serialization; hex-to-decimal entity conversion                         |
//                      | Charles Korthout | 0.4   | 26-06-2026     | Raw XML 1.1 serializer for prefixed namespace undeclarations                          |
//                      | Charles Korthout | 0.5   | 06-07-2026     | Apply xsl:output normalization-form during serialization                                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Serializes XDM result trees to XML strings.
/// </summary>
public static class ResultTreeSerializer
{
    /// <summary>
    /// Serializes an XDM value to an XML string using default settings.
    /// </summary>
    public static string Serialize(XdmValue value)
        => Serialize(value, null);

    /// <summary>
    /// Serializes an XDM value to a string using the provided output properties.
    /// </summary>
    public static string Serialize(XdmValue value, Stylesheet.OutputProperties? output)
    {
        if (value.IsUndefined)
            return string.Empty;

        var props = output ?? new Stylesheet.OutputProperties();

        if (props.Method == "text")
        {
            return SerializeAsText(value);
        }

        // method="xml" (default)
        if (value.IsNode)
        {
            return SerializeNode(value.NodeValue, props);
        }

        if (value.IsSequence && value.SequenceValue != null)
        {
            return SerializeSequence(value.SequenceValue, props);
        }

        // Atomic value
        return value.ToString();
    }

    private static string SerializeAsText(XdmValue value)
    {
        var sb = new System.Text.StringBuilder();
        CollectText(value, sb);
        return sb.ToString();
    }

    private static void CollectText(XdmValue value, System.Text.StringBuilder sb)
    {
        if (value.IsUndefined)
            return;

        if (value.IsNode && value.NodeValue != null)
        {
            CollectTextFromNode(value.NodeValue, sb);
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                CollectText(item, sb);
        }
        else
        {
            sb.Append(value.ToString());
        }
    }

    private static void CollectTextFromNode(IXdmNode node, System.Text.StringBuilder sb)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Text:
                sb.Append(node.StringValue);
                break;
            case XdmNodeKind.Element:
                foreach (var child in node.Axis(XdmAxis.Child))
                    CollectTextFromNode(child.NodeValue!, sb);
                break;
            case XdmNodeKind.Document:
                foreach (var child in node.Axis(XdmAxis.Child))
                    CollectTextFromNode(child.NodeValue!, sb);
                break;
            // Attributes, comments, PIs, and namespace nodes are ignored for text output
        }
    }

    private static string SerializeNode(IXdmNode? node, Stylesheet.OutputProperties? output = null)
    {
        if (node == null)
            return string.Empty;

        var props = output ?? new Stylesheet.OutputProperties();

        // For XDocument-backed nodes, use XmlWriter for proper serialization control
        if (node is XDocumentNode xdocNode)
        {
            var obj = xdocNode.UnderlyingObject;
            if (obj is XElement elem)
            {
                return SerializeXElement(elem, props);
            }
            if (obj is XDocument doc)
            {
                return SerializeXDocument(doc, props);
            }
            return obj?.ToString() ?? string.Empty;
        }

        // Fallback: build an XML representation
        return node.ToXmlString();
    }

    private static string SerializeXElement(XElement element, Stylesheet.OutputProperties props)
    {
        if (props.Version == "1.1")
            return SerializeRaw(element, props);

        ValidateXml10(element);
        return SerializeWithEncoding(element, props);
    }

    private static string SerializeXDocument(XDocument document, Stylesheet.OutputProperties props)
    {
        if (props.Version == "1.1")
            return SerializeRaw(document, props);

        ValidateXml10(document);
        return SerializeWithEncoding(document, props);
    }

    private static string SerializeWithEncoding(XNode node, Stylesheet.OutputProperties props)
    {
        // Apply Unicode normalization if requested before encoding-aware writing.
        if (TryGetNormalizationForm(props) is { } normForm)
            node = NormalizeXNode(node, normForm);

        // Namespace undeclarations are only permitted in XML 1.1 output when
        // undeclare-prefixes="yes" is in effect. Remove them for other cases.
        node = NormalizeForXmlWriter(node, props);

        // Use the specified output encoding so XmlWriter emits numeric character
        // references for characters that cannot be represented in that encoding.
        System.Text.Encoding encoding;
        try
        {
            encoding = System.Text.Encoding.GetEncoding(props.Encoding);
        }
        catch
        {
            encoding = new System.Text.UTF8Encoding(false);
        }

        // Ensure we never emit a BOM, which would corrupt string comparisons.
        if (encoding is System.Text.UTF8Encoding utf8 && utf8.GetPreamble().Length > 0)
            encoding = new System.Text.UTF8Encoding(false);
        else if (encoding is System.Text.UnicodeEncoding utf16 && utf16.GetPreamble().Length > 0)
            encoding = new System.Text.UnicodeEncoding(false, false);
        else if (encoding is System.Text.UTF32Encoding utf32 && utf32.GetPreamble().Length > 0)
            encoding = new System.Text.UTF32Encoding(false, false);

        using var stream = new System.IO.MemoryStream();
        var settings = CreateXmlWriterSettings(props, encoding);
        using (var xmlWriter = XmlWriter.Create(stream, settings))
        {
            if (node is XDocument doc)
            {
                if (!props.OmitXmlDeclaration)
                {
                    xmlWriter.WriteProcessingInstruction("xml",
                        $"version=\"{props.Version}\" encoding=\"{props.Encoding}\"" +
                        (props.Standalone != null ? $" standalone=\"{props.Standalone}\"" : ""));
                }

                foreach (var child in doc.Nodes())
                    child.WriteTo(xmlWriter);
            }
            else
            {
                node.WriteTo(xmlWriter);
            }
            xmlWriter.Flush();
        }

        var result = encoding.GetString(stream.ToArray());
        // XmlWriter emits hexadecimal character references by default;
        // the XSLT test suite expects decimal references.
        return ConvertHexEntitiesToDecimal(result);
    }

    private static string ConvertHexEntitiesToDecimal(string xml)
    {
        // Fast path: no hex entities
        if (xml.IndexOf("&#x", StringComparison.Ordinal) < 0)
            return xml;

        var sb = new System.Text.StringBuilder(xml.Length);
        int i = 0;
        while (i < xml.Length)
        {
            int start = xml.IndexOf("&#x", i, StringComparison.Ordinal);
            if (start < 0)
            {
                sb.Append(xml, i, xml.Length - i);
                break;
            }

            sb.Append(xml, i, start - i);

            int end = xml.IndexOf(';', start + 3);
            if (end < 0)
            {
                sb.Append(xml, start, xml.Length - start);
                break;
            }

            var hexValue = xml.Substring(start + 3, end - start - 3);
            if (int.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out int codepoint))
            {
                sb.Append("&#");
                sb.Append(codepoint);
                sb.Append(';');
            }
            else
            {
                sb.Append(xml, start, end - start + 1);
            }

            i = end + 1;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Maps the XSLT <c>normalization-form</c> output property to a .NET
    /// <see cref="NormalizationForm"/> value, or <c>null</c> when no normalization
    /// is requested.
    /// </summary>
    private static System.Text.NormalizationForm? TryGetNormalizationForm(Stylesheet.OutputProperties props)
    {
        return props.NormalizationForm?.Trim().ToUpperInvariant() switch
        {
            "NFC" => System.Text.NormalizationForm.FormC,
            "NFD" => System.Text.NormalizationForm.FormD,
            "NFKC" => System.Text.NormalizationForm.FormKC,
            "NFKD" => System.Text.NormalizationForm.FormKD,
            "NONE" or null or "" => null,
            _ => null
        };
    }

    /// <summary>
    /// Returns a deep clone of the supplied node with all text, attribute, comment,
    /// and processing-instruction values normalized to the given Unicode form.
    /// </summary>
    private static XNode NormalizeXNode(XNode node, System.Text.NormalizationForm form)
    {
        switch (node)
        {
            case XDocument doc:
                var clonedDoc = new XDocument(doc.Declaration);
                foreach (var child in doc.Nodes())
                    clonedDoc.Add(NormalizeXNode(child, form));
                return clonedDoc;
            case XElement element:
                var clonedElem = new XElement(element.Name);
                foreach (var attr in element.Attributes())
                    clonedElem.SetAttributeValue(attr.Name, attr.Value.Normalize(form));
                foreach (var child in element.Nodes())
                    clonedElem.Add(NormalizeXNode(child, form));
                return clonedElem;
            case XText text:
                return new XText(text.Value.Normalize(form));
            case XComment comment:
                return new XComment(comment.Value.Normalize(form));
            case XProcessingInstruction pi:
                return new XProcessingInstruction(pi.Target, pi.Data.Normalize(form));
            case XDocumentType docType:
                return new XDocumentType(docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset);
            default:
                return node;
        }
    }

    private static string SerializeSequence(IXdmSequence sequence, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        var settings = CreateXmlWriterSettings(props);
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        using var xmlWriter = XmlWriter.Create(writer, settings);

        foreach (var item in XdmSequence.FromSource(sequence))
        {
            if (item.IsNode && item.NodeValue != null)
            {
                WriteNode(xmlWriter, item.NodeValue);
            }
            else if (!item.IsUndefined)
            {
                xmlWriter.WriteString(item.ToString());
            }
        }

        xmlWriter.Flush();
        return writer.ToString();
    }

    private static XmlWriterSettings CreateXmlWriterSettings(Stylesheet.OutputProperties props, System.Text.Encoding? encoding = null)
    {
        return new XmlWriterSettings
        {
            OmitXmlDeclaration = props.OmitXmlDeclaration,
            Indent = props.Indent,
            Encoding = encoding ?? System.Text.Encoding.UTF8,
            ConformanceLevel = ConformanceLevel.Document
        };
    }

    private static void WriteNode(XmlWriter writer, IXdmNode node)
    {
        if (node is XDocumentNode xdocNode)
        {
            var obj = xdocNode.UnderlyingObject;
            switch (obj)
            {
                case XDocument doc:
                    foreach (var child in doc.Nodes())
                        child.WriteTo(writer);
                    break;
                case XElement elem:
                    if (elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "")
                    {
                        foreach (var child in elem.Nodes())
                            child.WriteTo(writer);
                    }
                    else
                    {
                        elem.WriteTo(writer);
                    }
                    break;
                case XText text:
                    writer.WriteString(text.Value);
                    break;
                case XComment comment:
                    writer.WriteComment(comment.Value);
                    break;
                case XProcessingInstruction pi:
                    writer.WriteProcessingInstruction(pi.Target, pi.Data);
                    break;
            }
        }
        else
        {
            writer.WriteString(node.ToXmlString());
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Raw XML 1.1 serializer for prefixed namespace undeclarations
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the node tree contains any <see cref="PrefixedNamespaceUndeclarations"/>
    /// annotations that require raw XML 1.1 serialization.
    /// </summary>
    private static bool HasUndeclarationAnnotations(XNode node)
    {
        if (node is XElement element)
        {
            if (element.Annotation<PrefixedNamespaceUndeclarations>() != null)
                return true;
            foreach (var child in element.Elements())
            {
                if (HasUndeclarationAnnotations(child))
                    return true;
            }
        }
        else if (node is XDocument doc)
        {
            foreach (var child in doc.Elements())
            {
                if (HasUndeclarationAnnotations(child))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Serializes a node tree using raw string output so that prefixed namespace
    /// undeclarations (<c>xmlns:prefix=""</c>) required by <c>inherit-namespaces="no"</c>
    /// can be emitted. The existing <see cref="XmlWriter"/> path cannot represent them.
    /// </summary>
    private static string SerializeRaw(XNode node, Stylesheet.OutputProperties props)
    {
        var sb = new System.Text.StringBuilder();
        using var writer = new StringWriter(sb);

        if (!props.OmitXmlDeclaration && props.Version == "1.1")
        {
            writer.Write("<?xml version=\"");
            writer.Write(props.Version);
            writer.Write("\" encoding=\"");
            writer.Write(props.Encoding);
            writer.Write("\"");
            if (props.Standalone != null)
            {
                writer.Write(" standalone=\"");
                writer.Write(props.Standalone);
                writer.Write("\"");
            }
            writer.Write("?>");
        }

        var inScope = new Dictionary<string, string>
        {
            ["xml"] = "http://www.w3.org/XML/1998/namespace"
        };

        if (node is XDocument doc)
        {
            foreach (var child in doc.Nodes())
                SerializeRawNode(writer, child, props, 0, inScope);
        }
        else if (node is XElement wrapper &&
                 wrapper.Name.LocalName == "__xdm_doc__" &&
                 wrapper.Name.NamespaceName == "")
        {
            foreach (var child in wrapper.Nodes())
                SerializeRawNode(writer, child, props, 0, inScope);
        }
        else
        {
            SerializeRawNode(writer, node, props, 0, inScope);
        }

        writer.Flush();
        return sb.ToString();
    }

    private static void SerializeRawNode(TextWriter writer, XNode node, Stylesheet.OutputProperties props, int depth, Dictionary<string, string> inScopeBindings)
    {
        switch (node)
        {
            case XElement elem:
                SerializeRawElement(writer, elem, props, depth, inScopeBindings);
                break;
            case XText text:
                WriteEscaped(writer, text.Value, isAttribute: false, props);
                break;
            case XComment comment:
                writer.Write("<!--");
                writer.Write(comment.Value);
                writer.Write("-->");
                break;
            case XProcessingInstruction pi:
                writer.Write("<?");
                writer.Write(pi.Target);
                writer.Write(' ');
                writer.Write(pi.Data);
                writer.Write("?>");
                break;
        }
    }

    private static void SerializeRawElement(TextWriter writer, XElement element, Stylesheet.OutputProperties props, int depth, Dictionary<string, string> inScopeBindings)
    {
        // Determine the namespace bindings that should be in scope on this element.
        var targetContext = element.Annotation<NamespaceInheritanceContext>();
        var targetBindings = targetContext?.Bindings ?? ComputeBindingsFromAttributes(element);

        // Collect the declarations that must be emitted on this start tag so that
        // the element name, attribute names, and required namespace nodes are bound.
        var declarations = new Dictionary<string, string>();
        var elemPrefix = string.IsNullOrEmpty(element.Name.NamespaceName)
            ? ""
            : FindOrDeclarePrefix(element.Name.NamespaceName, targetBindings, inScopeBindings, declarations, element);

        var nonNsAttributes = element.Attributes().Where(a => !a.IsNamespaceDeclaration).ToList();
        var attributePrefixes = new List<string>(nonNsAttributes.Count);
        foreach (var attr in nonNsAttributes)
        {
            var attrPrefix = string.IsNullOrEmpty(attr.Name.NamespaceName)
                ? ""
                : FindOrDeclarePrefix(attr.Name.NamespaceName, targetBindings, inScopeBindings, declarations, element);
            attributePrefixes.Add(attrPrefix);
        }

        writer.Write('<');
        if (!string.IsNullOrEmpty(elemPrefix))
        {
            writer.Write(elemPrefix);
            writer.Write(':');
        }
        writer.Write(Xml11NameCodec.DecodeName(element.Name.LocalName));

        // Emit declarations for bindings that differ from those already in scope.

        // The default namespace is written first to match conventional serialization.
        if (targetBindings.TryGetValue("", out var defaultUri) &&
            (!inScopeBindings.TryGetValue("", out _) || inScopeBindings[""] != defaultUri))
        {
            writer.Write(" xmlns=\"");
            WriteEscaped(writer, defaultUri, isAttribute: true, props);
            writer.Write('"');
            inScopeBindings[""] = defaultUri;
        }

        // Emit prefixed namespace declarations in the order in which they entered scope.
        var prefixOrder = targetContext?.PrefixOrder;
        if (prefixOrder != null)
        {
            foreach (var prefix in prefixOrder)
            {
                if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                    continue;
                if (!targetBindings.TryGetValue(prefix, out var uri))
                    continue;
                if (string.IsNullOrEmpty(uri))
                    continue; // undeclarations handled separately
                if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri == uri)
                    continue;
                writer.Write(" xmlns:");
                writer.Write(prefix);
                writer.Write("=\"");
                WriteEscaped(writer, uri, isAttribute: true, props);
                writer.Write('"');
                inScopeBindings[prefix] = uri;
            }
        }
        else
        {
            // Fall back to explicit namespace attributes in document order.
            foreach (var attr in element.Attributes().Where(a => a.IsNamespaceDeclaration))
            {
                var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                    continue;
                if (string.IsNullOrEmpty(attr.Value))
                    continue;
                if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri == attr.Value)
                    continue;
                writer.Write(" xmlns:");
                writer.Write(prefix);
                writer.Write("=\"");
                WriteEscaped(writer, attr.Value, isAttribute: true, props);
                writer.Write('"');
                inScopeBindings[prefix] = attr.Value;
            }
        }

        // Emit any generated prefixes that were needed but not covered above.
        foreach (var (prefix, uri) in declarations)
        {
            if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                continue;
            if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri == uri)
                continue;
            writer.Write(" xmlns:");
            writer.Write(prefix);
            writer.Write("=\"");
            WriteEscaped(writer, uri, isAttribute: true, props);
            writer.Write('"');
            inScopeBindings[prefix] = uri;
        }

        // Prefixed namespace undeclarations required by inherit-namespaces="no".
        // These are only legal in XML 1.1 output and only when undeclare-prefixes="yes".
        if (CanUndeclarePrefixes(props))
        {
            var undecl = element.Annotation<PrefixedNamespaceUndeclarations>();
            if (undecl != null)
            {
                foreach (var prefix in undecl.Prefixes)
                {
                    if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                        continue;
                    if (targetBindings.ContainsKey(prefix))
                        continue; // element redeclares this prefix itself
                    if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri != "")
                    {
                        writer.Write(" xmlns:");
                        writer.Write(prefix);
                        writer.Write("=\"\"");
                        inScopeBindings[prefix] = "";
                    }
                }
            }

            // Explicit default/prefixed namespace undeclarations (xmlns="", xmlns:p="").
            foreach (var attr in element.Attributes().Where(a => a.IsNamespaceDeclaration && string.IsNullOrEmpty(a.Value)))
            {
                var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (prefix == "xml" || prefix == "xmlns")
                    continue;
                if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri != "")
                {
                    if (prefix == "")
                    {
                        writer.Write(" xmlns=\"\"");
                    }
                    else
                    {
                        writer.Write(" xmlns:");
                        writer.Write(prefix);
                        writer.Write("=\"\"");
                    }
                    inScopeBindings[prefix] = "";
                }
            }
        }

        // Non-namespace attributes.
        for (int i = 0; i < nonNsAttributes.Count; i++)
        {
            var attr = nonNsAttributes[i];
            var attrPrefix = attributePrefixes[i];

            writer.Write(' ');
            if (!string.IsNullOrEmpty(attrPrefix))
            {
                writer.Write(attrPrefix);
                writer.Write(':');
            }
            writer.Write(Xml11NameCodec.DecodeName(attr.Name.LocalName));
            writer.Write("=\"");
            WriteEscaped(writer, Xml11NameCodec.DecodeValue(attr.Value), isAttribute: true, props);
            writer.Write('"');
        }

        var children = element.Nodes().ToList();
        if (children.Count == 0)
        {
            if (props.Indent)
                writer.Write(" />");
            else
                writer.Write("/>");
            return;
        }

        writer.Write('>');

        bool hasElementChildren = children.Any(c => c is XElement);
        foreach (var child in children)
        {
            if (props.Indent && hasElementChildren && child is XElement)
            {
                writer.WriteLine();
                writer.Write(new string(' ', (depth + 1) * 2));
            }
            // Each child gets a fresh copy of the in-scope bindings; namespace
            // declarations do not leak from one sibling to the next.
            SerializeRawNode(writer, child, props, depth + 1, new Dictionary<string, string>(inScopeBindings));
        }

        if (props.Indent && hasElementChildren)
        {
            writer.WriteLine();
            writer.Write(new string(' ', depth * 2));
        }

        writer.Write("</");
        if (!string.IsNullOrEmpty(elemPrefix))
        {
            writer.Write(elemPrefix);
            writer.Write(':');
        }
        writer.Write(Xml11NameCodec.DecodeName(element.Name.LocalName));
        writer.Write('>');
    }

    private static string FindOrDeclarePrefix(string uri, Dictionary<string, string> targetBindings, Dictionary<string, string> inScopeBindings, Dictionary<string, string> declarations, XElement element)
    {
        if (string.IsNullOrEmpty(uri))
            return "";

        // Prefer the default namespace if it is already bound to this URI.
        if (targetBindings.TryGetValue("", out var defaultTargetUri) && defaultTargetUri == uri)
            return "";
        if (inScopeBindings.TryGetValue("", out var defaultScopeUri) && defaultScopeUri == uri)
            return "";

        // Prefer a non-empty prefix that is already targeted for this element.
        foreach (var (prefix, boundUri) in targetBindings)
        {
            if (boundUri == uri && !string.IsNullOrEmpty(prefix))
                return prefix;
        }

        // Prefer a non-empty prefix already in scope.
        var scopePrefix = GetPrefixForUri(inScopeBindings, uri);
        if (scopePrefix != null)
            return scopePrefix;

        // Prefer a prefix explicitly declared on the element.
        foreach (var attr in element.Attributes().Where(a => a.IsNamespaceDeclaration))
        {
            if (attr.Value == uri)
            {
                var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (!inScopeBindings.ContainsKey(prefix) && !declarations.ContainsKey(prefix))
                {
                    declarations[prefix] = uri;
                    return prefix;
                }
            }
        }

        // Generate a fresh prefix.
        int index = 1;
        string generated;
        do
        {
            generated = $"ns{index - 1}";
            index++;
        } while (inScopeBindings.ContainsKey(generated) || declarations.ContainsKey(generated));

        declarations[generated] = uri;
        return generated;
    }

    private static Dictionary<string, string> ComputeBindingsFromAttributes(XElement element)
    {
        var bindings = new Dictionary<string, string>();
        foreach (var attr in element.Attributes().Where(a => a.IsNamespaceDeclaration))
        {
            var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
            if (string.IsNullOrEmpty(attr.Value))
                bindings.Remove(prefix);
            else
                bindings[prefix] = attr.Value;
        }
        return bindings;
    }

    private static string? GetPrefixForUri(Dictionary<string, string> inScopeBindings, string uri)
    {
        if (string.IsNullOrEmpty(uri))
            return "";

        foreach (var (prefix, boundUri) in inScopeBindings)
        {
            if (boundUri == uri)
                return prefix;
        }

        return null;
    }

    /// <summary>
    /// Validates that the node tree can be serialized as XML 1.0. Throws a
    /// serialization error (SERE0005/SERE0006) when it contains XML 1.1-only
    /// names or characters.
    /// </summary>
    private static void ValidateXml10(XNode node)
    {
        switch (node)
        {
            case XDocument document:
                foreach (var child in document.Nodes())
                    ValidateXml10(child);
                break;

            case XElement element:
                if (Xml11NameCodec.IsEncoded(element.Name.LocalName))
                    throw new InvalidOperationException("SERE0005: The result contains an element name that is not valid in XML 1.0.");
                foreach (var attr in element.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        if (ContainsInvalidXml10AttributeValue(Xml11NameCodec.DecodeValue(attr.Value)))
                            throw new InvalidOperationException("SERE0006: The result contains a character that is not valid in XML 1.0.");
                        continue;
                    }
                    if (Xml11NameCodec.IsEncoded(attr.Name.LocalName))
                        throw new InvalidOperationException("SERE0005: The result contains an attribute name that is not valid in XML 1.0.");
                    var decodedValue = Xml11NameCodec.DecodeValue(attr.Value);
                    if (ContainsInvalidXml10AttributeValue(decodedValue))
                        throw new InvalidOperationException("SERE0006: The result contains a character that is not valid in XML 1.0.");
                }
                foreach (var child in element.Nodes())
                    ValidateXml10(child);
                break;

            case XText text:
                if (ContainsInvalidXml10TextValue(Xml11NameCodec.DecodeValue(text.Value)))
                    throw new InvalidOperationException("SERE0006: The result contains a character that is not valid in XML 1.0.");
                break;

            case XComment comment:
                if (ContainsInvalidXml10TextValue(Xml11NameCodec.DecodeValue(comment.Value)))
                    throw new InvalidOperationException("SERE0006: The result contains a character that is not valid in XML 1.0.");
                break;

            case XProcessingInstruction pi:
                if (ContainsInvalidXml10TextValue(Xml11NameCodec.DecodeValue(pi.Data)))
                    throw new InvalidOperationException("SERE0006: The result contains a character that is not valid in XML 1.0.");
                break;
        }
    }

    private static bool CanUndeclarePrefixes(Stylesheet.OutputProperties props)
        => props.Version == "1.1" && props.UndeclarePrefixes;

    /// <summary>
    /// Returns a clone of the node tree with namespace undeclarations removed when
    /// they are not permitted by the effective output properties.
    /// </summary>
    private static XNode NormalizeForXmlWriter(XNode node, Stylesheet.OutputProperties props)
    {
        var allowUndeclare = CanUndeclarePrefixes(props);
        switch (node)
        {
            case XDocument doc:
                var clonedDoc = new XDocument(doc.Declaration);
                foreach (var child in doc.Nodes())
                    clonedDoc.Add(NormalizeForXmlWriter(child, props));
                return clonedDoc;

            case XElement element:
                var cloned = new XElement(element.Name);
                foreach (var attr in element.Attributes())
                {
                    // Prefixed namespace undeclarations (xmlns:prefix="") are only
                    // allowed in XML 1.1 output with undeclare-prefixes="yes".
                    // Default namespace undeclarations (xmlns="") are valid in XML 1.0
                    // and must be preserved so the namespace axis survives serialization.
                    if (!allowUndeclare && attr.IsNamespaceDeclaration && string.IsNullOrEmpty(attr.Value) && attr.Name.LocalName != "xmlns")
                        continue;
                    cloned.SetAttributeValue(attr.Name, attr.Value);
                }
                if (!allowUndeclare)
                    cloned.RemoveAnnotations<PrefixedNamespaceUndeclarations>();
                foreach (var child in element.Nodes())
                    cloned.Add(NormalizeForXmlWriter(child, props));
                return cloned;

            case XText text:
                return new XText(text.Value);
            case XComment comment:
                return new XComment(comment.Value);
            case XProcessingInstruction pi:
                return new XProcessingInstruction(pi.Target, pi.Data);
            case XDocumentType docType:
                return new XDocumentType(docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset);
            default:
                return node;
        }
    }

    private static bool ContainsInvalidXml10TextValue(string value)
    {
        foreach (char ch in value)
        {
            int cp = ch;
            if (cp is >= 0x01 and <= 0x08 or 0x0B or 0x0C or 0x0E or 0x0F or >= 0x10 and <= 0x1F)
                return true;
            if (cp is >= 0x7F and <= 0x9F)
                return true;
        }
        return false;
    }

    private static bool ContainsInvalidXml10AttributeValue(string value)
    {
        foreach (char ch in value)
        {
            int cp = ch;
            if (cp is >= 0x01 and <= 0x08 or 0x0B or 0x0C or 0x0E or 0x0F or >= 0x10 and <= 0x1F)
                return true;
            if (cp is >= 0x7F and <= 0x9F)
                return true;
        }
        return false;
    }

    private static void WriteEscaped(TextWriter writer, string value, bool isAttribute, Stylesheet.OutputProperties props)
    {
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '<':
                    writer.Write("&lt;");
                    break;
                case '>':
                    writer.Write("&gt;");
                    break;
                case '&':
                    writer.Write("&amp;");
                    break;
                case '"' when isAttribute:
                    writer.Write("&quot;");
                    break;
                case '\r':
                    writer.Write("&#13;");
                    break;
                default:
                    if (props.Version == "1.1" && MustEscapeInXml11(ch, isAttribute))
                    {
                        writer.Write("&#");
                        writer.Write((int)ch);
                        writer.Write(';');
                    }
                    else
                    {
                        writer.Write(ch);
                    }
                    break;
            }
        }
    }

    private static bool MustEscapeInXml11(char ch, bool isAttribute)
    {
        int cp = ch;
        // C0 controls (except tab, line feed, carriage return).
        if (cp is >= 0x01 and <= 0x1F && cp is not 0x09 and not 0x0A and not 0x0D)
            return true;
        // C1 controls and NEL (U+0085).
        if (cp is >= 0x7F and <= 0x9F)
            return true;
        // Line separator (U+2028).
        if (cp == 0x2028)
            return true;
        // Tab must be escaped in attribute values for XML 1.1 validity.
        if (isAttribute && cp == 0x09)
            return true;
        return false;
    }
}
