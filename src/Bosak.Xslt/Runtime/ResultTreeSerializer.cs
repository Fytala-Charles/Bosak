// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Serializes an XDM result tree to XML, HTML, XHTML, or text.
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
//                      | Charles Korthout | 0.6   | 26-06-2026     | Basic method="html" serialization unwraps __xdm_doc__ and omits XML declaration        |
//                      | Charles Korthout | 0.7   | 26-06-2026     | Apply normalization-form to HTML output and atomic values                                |
//                      | Charles Korthout | 0.8   | 11-07-2026     | Added method="xhtml", doctype, cdata-section-elements, escape-uri-attributes,          |
//                      |                  |       |                | include-content-type, byte-order-mark, and html-version support.                       |
//                      | Charles Korthout | 0.9   | 11-07-2026     | Preserve CDATA nodes during namespace-normalization so cdata-section-elements works. |
//                      | Charles Korthout | 1.0   | 11-07-2026     | Added SerializeXmlFragment to serialize __xdm_doc__ wrapper children for XML method.   |
//                      | Charles Korthout | 1.1   | 11-07-2026     | Infer xhtml/html serialization method from result root element when not specified.     |
//                      | Charles Korthout | 1.2   | 11-07-2026     | Validate encoding (SESU0007) and standalone+omit-declaration (SEPM0009) during serialize. |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Serializes XDM result trees to XML, HTML, XHTML, or text strings.
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
        InferMethod(props, value);
        ApplyMethodDefaults(props);
        ValidateOutputProperties(props);

        if (props.Method == "text")
        {
            return SerializeAsText(value);
        }

        if (props.Method == "html")
        {
            return SerializeAsHtml(value, props);
        }

        if (props.Method == "xhtml")
        {
            return SerializeAsXhtml(value, props);
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

    /// <summary>
    /// Applies method-specific default values for properties that were not explicitly set.
    /// </summary>
    private static void ApplyMethodDefaults(Stylesheet.OutputProperties props)
    {
        var method = props.Method;

        if (!props.EscapeUriAttributesSpecified)
        {
            // Default is true for HTML, false for XML and XHTML.
            props.EscapeUriAttributes = method == "html";
        }

        if (!props.IncludeContentTypeSpecified)
        {
            // Default is true for HTML and XHTML.
            props.IncludeContentType = method is "html" or "xhtml";
        }

        if (!props.MediaTypeSpecified)
        {
            props.MediaType = method switch
            {
                "xml" => "text/xml",
                "html" => "text/html",
                "xhtml" => "text/html",
                "text" => "text/plain",
                _ => "text/xml"
            };
        }

        if (!props.ByteOrderMarkSpecified)
        {
            // Default is yes for UTF-16 and UTF-32, no for UTF-8 and others.
            var enc = props.Encoding.Trim().ToUpperInvariant();
            props.ByteOrderMark = enc is "UTF-16" or "UTF-16LE" or "UTF-16BE" or "UTF-32" or "UTF-32LE" or "UTF-32BE";
        }

        if (!props.HtmlVersionSpecified)
        {
            // Default to 1.0 for XHTML so legacy XSLT 2.0 tests pass without
            // an explicit html-version attribute. HTML defaults to 5.0.
            props.HtmlVersion = method == "xhtml" ? "1.0" : "5.0";
        }
    }

    /// <summary>
    /// Validates serialization properties that have cross-attribute constraints
    /// or require a supported encoding. Raises XSLT serialization errors.
    /// </summary>
    private static void ValidateOutputProperties(Stylesheet.OutputProperties props)
    {
        ValidateEncoding(props.Encoding);

        // SEPM0009: standalone pseudo-attribute is not allowed when the XML
        // declaration is omitted.
        if (props.OmitXmlDeclaration && props.Standalone is "yes" or "no")
        {
            throw new XsltRuntimeException("SEPM0009",
                "The standalone pseudo-attribute is not allowed when the XML declaration is omitted.",
                XdmValue.Undefined);
        }
    }

    /// <summary>
    /// Validates that the requested encoding name is supported by the runtime.
    /// Raises SESU0007 for unsupported encodings.
    /// </summary>
    private static void ValidateEncoding(string encodingName)
    {
        var enc = encodingName.Trim().ToUpperInvariant();
        switch (enc)
        {
            case "UTF-8":
            case "UTF8":
            case "UTF-16":
            case "UTF-16LE":
            case "UTF16":
            case "UTF16LE":
            case "UTF-16BE":
            case "UTF16BE":
            case "UTF-32":
            case "UTF-32LE":
            case "UTF32":
            case "UTF32LE":
            case "UTF-32BE":
            case "UTF32BE":
                return;
        }

        try
        {
            _ = System.Text.Encoding.GetEncoding(encodingName);
        }
        catch
        {
            throw new XsltRuntimeException("SESU0007",
                $"Unsupported encoding '{encodingName}'.",
                XdmValue.Undefined);
        }
    }

    /// <summary>
    /// Infers the serialization method from the result tree when no method
    /// was explicitly specified on xsl:output.
    /// </summary>
    private static void InferMethod(Stylesheet.OutputProperties props, XdmValue value)
    {
        if (props.MethodSpecified)
            return;

        var rootElement = GetResultRootElement(value);
        if (rootElement == null)
            return;

        if (rootElement.Name.LocalName == "html" &&
            rootElement.Name.NamespaceName == "http://www.w3.org/1999/xhtml")
        {
            props.Method = "xhtml";
            props.MethodSpecified = true;
            if (!props.OmitXmlDeclarationSpecified)
                props.OmitXmlDeclaration = false;
        }
        else if (rootElement.Name.LocalName == "html" &&
                 rootElement.Name.NamespaceName == "")
        {
            props.Method = "html";
            props.MethodSpecified = true;
            if (!props.OmitXmlDeclarationSpecified)
                props.OmitXmlDeclaration = true;
        }
    }

    /// <summary>
    /// Returns the single root element of a result value, or the first child
    /// element of a fragment wrapper, for method inference.
    /// </summary>
    private static XElement? GetResultRootElement(XdmValue value)
    {
        if (!value.IsNode || value.NodeValue == null)
            return null;

        if (value.NodeValue is not XDocumentNode xdn)
            return null;

        if (xdn.UnderlyingObject is XDocument doc)
            return doc.Root;

        if (xdn.UnderlyingObject is XElement elem)
        {
            if (elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "")
                return elem.Elements().FirstOrDefault();
            return elem;
        }

        return null;
    }

    private static string SerializeAsText(XdmValue value)
    {
        var sb = new System.Text.StringBuilder();
        CollectText(value, sb);
        return sb.ToString();
    }

    private static string SerializeAsHtml(XdmValue value, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        WriteByteOrderMark(writer, props);

        // Flatten the value into a list of nodes/atomics.
        var items = FlattenItems(value).ToList();

        // Determine the root element for DOCTYPE and meta injection.
        XElement? rootElement = null;
        XDocument? rootDocument = null;
        foreach (var item in items)
        {
            if (item.IsNode && item.NodeValue != null)
            {
                var node = item.NodeValue;
                if (node is XDocumentNode xdn)
                {
                    if (xdn.UnderlyingObject is XDocument doc)
                    {
                        rootDocument = doc;
                        rootElement = doc.Root;
                        break;
                    }
                    if (xdn.UnderlyingObject is XElement elem)
                    {
                        rootElement = elem;
                        break;
                    }
                }
            }
        }

        // Apply content-type meta injection if enabled.
        if (props.IncludeContentType && rootElement != null)
        {
            rootElement = InsertContentTypeMeta(rootElement, props);
            if (rootDocument != null)
            {
                var newDoc = new XDocument(rootDocument.Nodes().Select(n => n is XElement e ? rootElement! : n));
                rootDocument = newDoc;
            }
        }

        // Apply normalization if requested.
        if (TryGetNormalizationForm(props) is { } normForm)
        {
            if (rootDocument != null)
                rootDocument = (XDocument)NormalizeXNode(rootDocument, normForm);
            else if (rootElement != null)
                rootElement = (XElement)NormalizeXNode(rootElement, normForm);
        }

        WriteDoctype(writer, rootElement, props);

        if (rootDocument != null)
        {
            foreach (var child in rootDocument.Nodes())
                WriteHtmlNode(writer, child, props, 0);
        }
        else if (rootElement != null)
        {
            WriteHtmlNode(writer, rootElement, props, 0);
        }
        else
        {
            foreach (var item in items)
            {
                if (item.IsNode && item.NodeValue != null)
                    WriteHtmlNode(writer, item.NodeValue, props, 0);
                else if (!item.IsUndefined)
                    WriteHtmlEscaped(writer, item.ToString());
            }
        }

        writer.Flush();
        return writer.ToString();
    }

    private static string SerializeAsXhtml(XdmValue value, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        WriteByteOrderMark(writer, props);

        var items = FlattenItems(value).ToList();
        var nodeItems = items
            .Where(i => i.IsNode && i.NodeValue != null)
            .Select(i => i.NodeValue!)
            .ToList();

        // Determine serialization mode. A single document or element node is
        // serialized in document mode; everything else is a fragment.
        bool isDocumentMode = nodeItems.Count == 1 && nodeItems[0] is XDocumentNode xdn &&
            (xdn.UnderlyingObject is XDocument or XElement);

        XElement? rootElement = null;
        XDocument? rootDocument = null;
        List<XNode> fragmentNodes;

        if (isDocumentMode && nodeItems[0] is XDocumentNode docNode)
        {
            if (docNode.UnderlyingObject is XDocument doc)
            {
                rootDocument = doc;
                rootElement = doc.Root;
            }
            else if (docNode.UnderlyingObject is XElement elem)
            {
                rootElement = elem;
            }
            fragmentNodes = new List<XNode>();
        }
        else
        {
            // Fragment mode: build a list of XNodes from the items.
            fragmentNodes = new List<XNode>();
            foreach (var node in nodeItems)
            {
                if (node is XDocumentNode innerXdn)
                {
                    if (innerXdn.UnderlyingObject is XDocument doc)
                    {
                        foreach (var child in doc.Nodes())
                            fragmentNodes.Add(child);
                    }
                    else if (innerXdn.UnderlyingObject is XElement elem)
                    {
                        fragmentNodes.Add(elem);
                    }
                    else if (innerXdn.UnderlyingObject is XNode xn)
                    {
                        fragmentNodes.Add(xn);
                    }
                }
            }

            // Find the first html element for content-type meta injection.
            rootElement = fragmentNodes.OfType<XElement>()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, "html", StringComparison.OrdinalIgnoreCase));
        }

        // Apply content-type meta injection if enabled.
        if (props.IncludeContentType && rootElement != null)
        {
            var newRoot = InsertContentTypeMeta(rootElement, props);
            if (rootDocument != null)
            {
                var newDoc = new XDocument(rootDocument.Nodes().Select(n => n is XElement ? newRoot : n));
                rootDocument = newDoc;
            }
            else if (isDocumentMode)
            {
                rootElement = newRoot;
            }
            else
            {
                // Replace the html element in the fragment list.
                for (int i = 0; i < fragmentNodes.Count; i++)
                {
                    if (fragmentNodes[i] == rootElement)
                    {
                        fragmentNodes[i] = newRoot;
                        break;
                    }
                }
            }
            rootElement = newRoot;
        }

        // Apply normalization if requested.
        if (TryGetNormalizationForm(props) is { } normForm)
        {
            if (rootDocument != null)
                rootDocument = (XDocument)NormalizeXNode(rootDocument, normForm);
            else if (isDocumentMode && rootElement != null)
                rootElement = (XElement)NormalizeXNode(rootElement, normForm);
            else
            {
                for (int i = 0; i < fragmentNodes.Count; i++)
                    fragmentNodes[i] = NormalizeXNode(fragmentNodes[i], normForm);
            }
        }

        // XML declaration.
        if (!props.OmitXmlDeclaration && (nodeItems.Count > 0))
        {
            writer.Write("<?xml version=\"");
            writer.Write(props.Version);
            writer.Write("\" encoding=\"");
            writer.Write(props.Encoding);
            writer.Write("\"");
            if (props.Standalone is "yes" or "no")
            {
                writer.Write(" standalone=\"");
                writer.Write(props.Standalone);
                writer.Write("\"");
            }
            writer.Write("?>");
        }

        // DOCTYPE is only emitted in document mode or when explicitly requested.
        if (isDocumentMode)
        {
            WriteDoctype(writer, rootElement, props);
        }
        else if (!string.IsNullOrEmpty(props.DoctypeSystem) || !string.IsNullOrEmpty(props.DoctypePublic))
        {
            WriteDoctype(writer, fragmentNodes.OfType<XElement>().FirstOrDefault(), props);
        }

        var initialBindings = new Dictionary<string, string> { ["xml"] = "http://www.w3.org/XML/1998/namespace" };
        if (rootDocument != null)
        {
            foreach (var child in rootDocument.Nodes())
                WriteXhtmlNode(writer, child, props, 0, new Dictionary<string, string>(initialBindings));
        }
        else if (isDocumentMode && rootElement != null)
        {
            WriteXhtmlNode(writer, rootElement, props, 0, new Dictionary<string, string>(initialBindings));
        }
        else
        {
            foreach (var node in fragmentNodes)
            {
                WriteXhtmlNode(writer, node, props, 0, new Dictionary<string, string>(initialBindings));
            }
            foreach (var item in items.Where(i => !i.IsNode && !i.IsUndefined))
            {
                WriteXmlEscaped(writer, item.ToString(), props);
            }
        }

        writer.Flush();
        return writer.ToString();
    }

    private static IEnumerable<XdmValue> FlattenItems(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                foreach (var flat in FlattenItems(item))
                    yield return flat;
        }
        else if (value.IsNode && value.NodeValue != null)
        {
            var node = value.NodeValue;
            if (node.NodeKind == XdmNodeKind.Document &&
                node is XDocumentNode xdn &&
                xdn.UnderlyingObject is XDocument doc)
            {
                foreach (var child in doc.Nodes())
                    yield return XdmValue.FromNode(new XDocumentNode(child));
            }
            else if (node is XDocumentNode xdn2 &&
                     xdn2.UnderlyingObject is XElement elem &&
                     elem.Name.LocalName == "__xdm_doc__" &&
                     elem.Name.NamespaceName == "")
            {
                foreach (var child in elem.Nodes())
                    yield return XdmValue.FromNode(new XDocumentNode(child));
            }
            else
            {
                yield return value;
            }
        }
        else
        {
            yield return value;
        }
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
        ApplyMethodDefaults(props);

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

    private static string SerializeSequence(IXdmSequence sequence, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        WriteByteOrderMark(writer, props);
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

    private static string SerializeXElement(XElement element, Stylesheet.OutputProperties props)
    {
        if (element.Name.LocalName == "__xdm_doc__" && element.Name.NamespaceName == "")
            return SerializeXmlFragment(element, props);

        if (props.Version == "1.1")
            return SerializeRaw(element, props);

        ValidateXml10(element);
        return SerializeWithEncoding(element, props);
    }

    private static string SerializeXmlFragment(XElement wrapper, Stylesheet.OutputProperties props)
    {
        var node = (XNode)wrapper;

        // Apply Unicode normalization if requested before encoding-aware writing.
        if (TryGetNormalizationForm(props) is { } normForm)
            node = NormalizeXNode(node, normForm);

        // Wrap CDATA section elements.
        node = WrapCdataSections(node, props);

        // Namespace undeclarations are only permitted in XML 1.1 output when
        // undeclare-prefixes="yes" is in effect. Remove them for other cases.
        node = NormalizeForXmlWriter(node, props);

        var fragment = (XElement)node;

        var encoding = GetEncodingWithBom(props.Encoding, props.ByteOrderMark);

        using var stream = new System.IO.MemoryStream();
        var settings = CreateXmlWriterSettings(props, encoding);
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        using (var xmlWriter = XmlWriter.Create(stream, settings))
        {
            if (!props.OmitXmlDeclaration)
            {
                xmlWriter.WriteProcessingInstruction("xml",
                    $"version=\"{props.Version}\" encoding=\"{props.Encoding}\"" +
                    (props.Standalone is "yes" or "no" ? $" standalone=\"{props.Standalone}\"" : ""));
            }

            WriteDoctype(xmlWriter, fragment.Elements().FirstOrDefault(), props);

            foreach (var child in fragment.Nodes())
                child.WriteTo(xmlWriter);

            xmlWriter.Flush();
        }

        var result = encoding.GetString(stream.ToArray());
        return ConvertHexEntitiesToDecimal(result);
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

        // Wrap CDATA section elements.
        node = WrapCdataSections(node, props);

        // Namespace undeclarations are only permitted in XML 1.1 output when
        // undeclare-prefixes="yes" is in effect. Remove them for other cases.
        node = NormalizeForXmlWriter(node, props);

        // Content-type meta is not inserted for XML method.

        // Use the specified output encoding so XmlWriter emits numeric character
        // references for characters that cannot be represented in that encoding.
        var encoding = GetEncodingWithBom(props.Encoding, props.ByteOrderMark);

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
                        (props.Standalone is "yes" or "no" ? $" standalone=\"{props.Standalone}\"" : ""));
                }

                WriteDoctype(xmlWriter, doc.Root, props);

                foreach (var child in doc.Nodes())
                    child.WriteTo(xmlWriter);
            }
            else
            {
                WriteDoctype(xmlWriter, node as XElement, props);
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
    /// Returns a wrapped XDM node whose underlying XML tree has all text, attribute,
    /// comment, and processing-instruction values normalized to the given Unicode form.
    /// </summary>
    private static IXdmNode NormalizeXdmNode(IXdmNode node, System.Text.NormalizationForm form)
    {
        if (node is XDocumentNode xdocNode && xdocNode.UnderlyingObject is XNode xnode)
            return new XDocumentNode(NormalizeXNode(xnode, form));
        return node;
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

    /// <summary>
    /// Returns a deep clone with text children of cdata-section-elements wrapped as CDATA nodes.
    /// </summary>
    private static XNode WrapCdataSections(XNode node, Stylesheet.OutputProperties props)
    {
        if (props.CdataSectionElements.Count == 0)
            return node;

        switch (node)
        {
            case XDocument doc:
                var clonedDoc = new XDocument(doc.Declaration);
                foreach (var child in doc.Nodes())
                    clonedDoc.Add(WrapCdataSections(child, props));
                return clonedDoc;
            case XElement element:
                var cloned = new XElement(element.Name);
                foreach (var attr in element.Attributes())
                    cloned.SetAttributeValue(attr.Name, attr.Value);
                bool wrapChildren = IsCdataSectionElement(element.Name, props);
                foreach (var child in element.Nodes())
                {
                    if (wrapChildren && child is XText text && !(child is XCData))
                    {
                        cloned.Add(new XCData(text.Value));
                    }
                    else
                    {
                        cloned.Add(WrapCdataSections(child, props));
                    }
                }
                return cloned;
            default:
                return node;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // HTML serialization
    // ---------------------------------------------------------------------------------------------

    private static void WriteHtmlNode(TextWriter writer, IXdmNode node, Stylesheet.OutputProperties props, int depth)
    {
        if (node is not XDocumentNode xdn)
        {
            writer.Write(node.ToXmlString());
            return;
        }

        var obj = xdn.UnderlyingObject;
        switch (obj)
        {
            case XDocument doc:
                foreach (var child in doc.Nodes())
                    WriteHtmlNode(writer, new XDocumentNode(child), props, depth);
                break;
            case XElement elem when elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "":
                foreach (var child in elem.Nodes())
                    WriteHtmlNode(writer, new XDocumentNode(child), props, depth);
                break;
            case XElement elem:
                WriteHtmlElement(writer, elem, props, depth);
                break;
            case XText text:
                WriteHtmlEscaped(writer, text.Value);
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

    private static void WriteHtmlNode(TextWriter writer, XNode node, Stylesheet.OutputProperties props, int depth)
    {
        switch (node)
        {
            case XElement elem when elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "":
                foreach (var child in elem.Nodes())
                    WriteHtmlNode(writer, child, props, depth);
                break;
            case XElement elem:
                WriteHtmlElement(writer, elem, props, depth);
                break;
            case XText text:
                WriteHtmlEscaped(writer, text.Value);
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

    private static void WriteHtmlElement(TextWriter writer, XElement element, Stylesheet.OutputProperties props, int depth)
    {
        var localName = element.Name.LocalName;
        var isEmpty = !element.Nodes().Any();
        var isRawContent = IsHtmlRawContentElement(localName);

        writer.Write('<');
        writer.Write(localName);

        foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            writer.Write(' ');
            writer.Write(attr.Name.LocalName);
            writer.Write("=\"");
            var value = attr.Value;
            if (props.EscapeUriAttributes && IsUriAttribute(attr.Name))
                value = EscapeUriAttribute(value);
            WriteHtmlEscaped(writer, value);
            writer.Write('"');
        }

        if (isEmpty)
        {
            if (IsHtmlEmptyElement(localName) || IsHtmlVoidElement(localName))
            {
                writer.Write('>');
            }
            else
            {
                writer.Write('>');
                writer.Write("</");
                writer.Write(localName);
                writer.Write('>');
            }
            return;
        }

        writer.Write('>');

        if (isRawContent)
        {
            foreach (var child in element.Nodes())
            {
                if (child is XText text)
                    writer.Write(text.Value);
                else if (child is XElement childElem)
                    WriteHtmlElement(writer, childElem, props, depth + 1);
                else
                    WriteHtmlNode(writer, child, props, depth + 1);
            }
        }
        else
        {
            bool hasElementChildren = element.Elements().Any();
            foreach (var child in element.Nodes())
            {
                if (props.Indent && hasElementChildren && child is XElement)
                {
                    writer.WriteLine();
                    writer.Write(new string(' ', (depth + 1) * 2));
                }
                WriteHtmlNode(writer, child, props, depth + 1);
            }
            if (props.Indent && hasElementChildren)
            {
                writer.WriteLine();
                writer.Write(new string(' ', depth * 2));
            }
        }

        writer.Write("</");
        writer.Write(localName);
        writer.Write('>');
    }

    private static void WriteHtmlEscaped(TextWriter writer, string value)
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
                case '"':
                    writer.Write("&quot;");
                    break;
                case '\r':
                    writer.Write("&#13;");
                    break;
                default:
                    writer.Write(ch);
                    break;
            }
        }
    }

    // ---------------------------------------------------------------------------------------------
    // XHTML serialization
    // ---------------------------------------------------------------------------------------------

    private static void WriteXhtmlNode(TextWriter writer, IXdmNode node, Stylesheet.OutputProperties props, int depth, Dictionary<string, string> inScopeBindings)
    {
        if (node is not XDocumentNode xdn)
        {
            writer.Write(node.ToXmlString());
            return;
        }

        var obj = xdn.UnderlyingObject;
        switch (obj)
        {
            case XDocument doc:
                foreach (var child in doc.Nodes())
                    WriteXhtmlNode(writer, new XDocumentNode(child), props, depth, new Dictionary<string, string>(inScopeBindings));
                break;
            case XElement elem when elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "":
                foreach (var child in elem.Nodes())
                    WriteXhtmlNode(writer, new XDocumentNode(child), props, depth, new Dictionary<string, string>(inScopeBindings));
                break;
            case XElement elem:
                WriteXhtmlElement(writer, elem, props, depth, inScopeBindings);
                break;
            case XText text:
                WriteXmlEscaped(writer, text.Value, props);
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

    private static void WriteXhtmlNode(TextWriter writer, XNode node, Stylesheet.OutputProperties props, int depth, Dictionary<string, string> inScopeBindings)
    {
        switch (node)
        {
            case XElement elem when elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "":
                foreach (var child in elem.Nodes())
                    WriteXhtmlNode(writer, child, props, depth, new Dictionary<string, string>(inScopeBindings));
                break;
            case XElement elem:
                WriteXhtmlElement(writer, elem, props, depth, inScopeBindings);
                break;
            case XText text:
                WriteXmlEscaped(writer, text.Value, props);
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

    private static void WriteXhtmlElement(TextWriter writer, XElement element, Stylesheet.OutputProperties props, int depth, Dictionary<string, string> inScopeBindings)
    {
        var localName = Xml11NameCodec.DecodeName(element.Name.LocalName);
        var nsUri = element.Name.NamespaceName;
        var isInXhtmlNs = nsUri == "http://www.w3.org/1999/xhtml";
        var isEmpty = !element.Nodes().Any();
        var isVoid = isInXhtmlNs && IsHtmlVoidElement(localName);
        // XHTML method treats script/style content as PCDATA, so it is escaped.
        var isRawContent = false;
        var wrapCdata = IsCdataSectionElement(element.Name, props);

        // Determine effective namespace bindings for this element.
        var targetBindings = element.Annotation<NamespaceInheritanceContext>()?.Bindings ?? ComputeBindingsFromAttributes(element);
        var prefixOrder = element.Annotation<NamespaceInheritanceContext>()?.PrefixOrder;
        var declarations = new Dictionary<string, string>();
        var elemPrefix = string.IsNullOrEmpty(nsUri)
            ? ""
            : FindOrDeclarePrefix(nsUri, targetBindings, inScopeBindings, declarations, element);

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
        writer.Write(localName);

        // Default namespace declaration first.
        if (targetBindings.TryGetValue("", out var defaultUri) &&
            (!inScopeBindings.TryGetValue("", out _) || inScopeBindings[""] != defaultUri))
        {
            writer.Write(" xmlns=\"");
            WriteXmlEscaped(writer, defaultUri, props);
            writer.Write('"');
            inScopeBindings[""] = defaultUri;
        }

        // Prefixed namespace declarations.
        EmitNamespaceDeclarations(writer, element, targetBindings, prefixOrder, inScopeBindings, declarations, props);

        // Prefixed namespace undeclarations.
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
                        continue;
                    if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri != "")
                    {
                        writer.Write(" xmlns:");
                        writer.Write(prefix);
                        writer.Write("=\"\"");
                        inScopeBindings[prefix] = "";
                    }
                }
            }
        }

        // Non-namespace attributes.
        for (int i = 0; i < nonNsAttributes.Count; i++)
        {
            var attr = nonNsAttributes[i];
            var attrPrefix = attributePrefixes[i];
            var attrLocalName = Xml11NameCodec.DecodeName(attr.Name.LocalName);

            writer.Write(' ');
            if (!string.IsNullOrEmpty(attrPrefix))
            {
                writer.Write(attrPrefix);
                writer.Write(':');
            }
            writer.Write(attrLocalName);
            writer.Write("=\"");
            var value = Xml11NameCodec.DecodeValue(attr.Value);
            if (props.EscapeUriAttributes && IsUriAttribute(attr.Name))
                value = EscapeUriAttribute(value);
            WriteXmlEscaped(writer, value, props);
            writer.Write('"');
        }

        if (isVoid)
        {
            writer.Write(" />");
            return;
        }

        if (isEmpty)
        {
            writer.Write('>');
            writer.Write("</");
            writer.Write(localName);
            writer.Write('>');
            return;
        }

        writer.Write('>');

        if (isRawContent)
        {
            foreach (var child in element.Nodes())
            {
                if (child is XText text)
                    writer.Write(text.Value);
                else if (child is XElement childElem)
                    WriteXhtmlElement(writer, childElem, props, depth + 1, new Dictionary<string, string>(inScopeBindings));
                else
                    WriteXhtmlNode(writer, child, props, depth + 1, new Dictionary<string, string>(inScopeBindings));
            }
        }
        else if (wrapCdata)
        {
            foreach (var child in element.Nodes())
            {
                if (child is XText text)
                {
                    WriteCdataText(writer, text.Value);
                }
                else if (child is XElement childElem)
                {
                    WriteXhtmlElement(writer, childElem, props, depth + 1, new Dictionary<string, string>(inScopeBindings));
                }
                else
                {
                    WriteXhtmlNode(writer, child, props, depth + 1, new Dictionary<string, string>(inScopeBindings));
                }
            }
        }
        else
        {
            bool hasElementChildren = element.Elements().Any();
            foreach (var child in element.Nodes())
            {
                if (props.Indent && hasElementChildren && child is XElement && !IsSuppressIndentationElement((child as XElement)!.Name, props))
                {
                    writer.WriteLine();
                    writer.Write(new string(' ', (depth + 1) * 2));
                }
                WriteXhtmlNode(writer, child, props, depth + 1, new Dictionary<string, string>(inScopeBindings));
            }
            if (props.Indent && hasElementChildren && !IsSuppressIndentationElement(element.Name, props))
            {
                writer.WriteLine();
                writer.Write(new string(' ', depth * 2));
            }
        }

        writer.Write("</");
        if (!string.IsNullOrEmpty(elemPrefix))
        {
            writer.Write(elemPrefix);
            writer.Write(':');
        }
        writer.Write(localName);
        writer.Write('>');
    }

    private static void WriteXmlEscaped(TextWriter writer, string value, Stylesheet.OutputProperties props)
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
                case '"':
                    writer.Write("&quot;");
                    break;
                case '\r':
                    writer.Write("&#13;");
                    break;
                default:
                    if (props.Version == "1.1" && MustEscapeInXml11(ch, isAttribute: false))
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

    private static void WriteCdataText(TextWriter writer, string value)
    {
        // Split any ]]> inside the text as required by the spec.
        writer.Write("<![CDATA[");
        writer.Write(value.Replace("]]>", "]]]]><![CDATA[>"));
        writer.Write("]]>");
    }

    private static void EmitNamespaceDeclarations(TextWriter writer, XElement element, Dictionary<string, string> targetBindings, List<string>? prefixOrder, Dictionary<string, string> inScopeBindings, Dictionary<string, string> declarations, Stylesheet.OutputProperties props)
    {
        if (prefixOrder != null)
        {
            foreach (var prefix in prefixOrder)
            {
                if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                    continue;
                if (!targetBindings.TryGetValue(prefix, out var uri))
                    continue;
                if (string.IsNullOrEmpty(uri))
                    continue;
                if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri == uri)
                    continue;
                writer.Write(" xmlns:");
                writer.Write(prefix);
                writer.Write("=\"");
                WriteXmlEscaped(writer, uri, props);
                writer.Write('"');
                inScopeBindings[prefix] = uri;
            }
        }
        else
        {
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
                WriteXmlEscaped(writer, attr.Value, props);
                writer.Write('"');
                inScopeBindings[prefix] = attr.Value;
            }
        }

        // Generated prefixes that were needed but not covered above.
        foreach (var (prefix, uri) in declarations)
        {
            if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                continue;
            if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri == uri)
                continue;
            writer.Write(" xmlns:");
            writer.Write(prefix);
            writer.Write("=\"");
            WriteXmlEscaped(writer, uri, props);
            writer.Write('"');
            inScopeBindings[prefix] = uri;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Output-property helpers
    // ---------------------------------------------------------------------------------------------

    private static void WriteByteOrderMark(TextWriter writer, Stylesheet.OutputProperties props)
    {
        if (!props.ByteOrderMark)
            return;

        var enc = props.Encoding.Trim().ToUpperInvariant();
        if (enc is "UTF-8" or "UTF8")
        {
            writer.Write('\uFEFF');
        }
        else if (enc is "UTF-16" or "UTF-16LE" or "UTF16" or "UTF-16LE")
        {
            writer.Write('\uFEFF');
        }
        else if (enc is "UTF-32" or "UTF-32LE" or "UTF32" or "UTF-32LE")
        {
            writer.Write('\uFEFF');
        }
        // Big-endian forms are informational; string output is UTF-16.
    }

    private static System.Text.Encoding GetEncodingWithBom(string encodingName, bool includeBom)
    {
        var enc = encodingName.Trim().ToUpperInvariant();
        return enc switch
        {
            "UTF-8" or "UTF8" => new System.Text.UTF8Encoding(includeBom),
            "UTF-16" or "UTF-16LE" or "UTF16" or "UTF16LE" => new System.Text.UnicodeEncoding(false, includeBom),
            "UTF-16BE" or "UTF16BE" => new System.Text.UnicodeEncoding(true, includeBom),
            "UTF-32" or "UTF-32LE" or "UTF32" or "UTF32LE" => new System.Text.UTF32Encoding(false, includeBom),
            "UTF-32BE" or "UTF32BE" => new System.Text.UTF32Encoding(true, includeBom),
            _ => System.Text.Encoding.GetEncoding(encodingName)
        };
    }

    private static void WriteDoctype(TextWriter writer, XElement? rootElement, Stylesheet.OutputProperties props)
    {
        var rootName = rootElement?.Name.LocalName;
        if (string.IsNullOrEmpty(rootName))
            return;

        if (!string.IsNullOrEmpty(props.DoctypePublic))
        {
            writer.Write("<!DOCTYPE ");
            writer.Write(rootName);
            writer.Write(" PUBLIC \"");
            writer.Write(props.DoctypePublic);
            writer.Write("\"");
            if (!string.IsNullOrEmpty(props.DoctypeSystem))
            {
                writer.Write(" \"");
                writer.Write(props.DoctypeSystem);
                writer.Write("\"");
            }
            writer.Write(">");
        }
        else if (!string.IsNullOrEmpty(props.DoctypeSystem))
        {
            writer.Write("<!DOCTYPE ");
            writer.Write(rootName);
            writer.Write(" SYSTEM \"");
            writer.Write(props.DoctypeSystem);
            writer.Write("\">");
        }
        else if (props.Method == "xhtml")
        {
            // Default DOCTYPE for XHTML is only emitted for html-version 5.0.
            // Legacy XHTML 1.0/1.1 tests expect no DOCTYPE unless explicitly specified.
            var htmlVersion = props.HtmlVersion;
            if (htmlVersion == "5.0" && rootName == "html")
            {
                writer.Write("<!DOCTYPE html>");
            }
        }
    }

    private static void WriteDoctype(XmlWriter writer, XElement? rootElement, Stylesheet.OutputProperties props)
    {
        var rootName = rootElement?.Name.LocalName;
        if (string.IsNullOrEmpty(rootName))
            return;

        if (!string.IsNullOrEmpty(props.DoctypePublic))
        {
            writer.WriteDocType(rootName, props.DoctypePublic, props.DoctypeSystem, null);
        }
        else if (!string.IsNullOrEmpty(props.DoctypeSystem))
        {
            writer.WriteDocType(rootName, null, props.DoctypeSystem, null);
        }
        else if (props.Method == "xhtml")
        {
            var htmlVersion = props.HtmlVersion;
            if (htmlVersion == "5.0" && rootName == "html")
            {
                writer.WriteDocType("html", null, null, null);
            }
        }
    }

    private static XElement InsertContentTypeMeta(XElement rootElement, Stylesheet.OutputProperties props)
    {
        // Only insert for html root element (with or without XHTML namespace).
        if (!string.Equals(rootElement.Name.LocalName, "html", StringComparison.OrdinalIgnoreCase))
            return rootElement;

        var head = rootElement.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "head", StringComparison.OrdinalIgnoreCase));
        if (head == null)
            return rootElement;

        // Check if a meta http-equiv Content-Type already exists.
        XElement? existingMeta = null;
        foreach (var meta in head.Elements().Where(e => string.Equals(e.Name.LocalName, "meta", StringComparison.OrdinalIgnoreCase)))
        {
            var httpEquiv = meta.Attribute("http-equiv")?.Value;
            if (string.Equals(httpEquiv, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                existingMeta = meta;
                break;
            }
        }

        var mediaType = props.MediaType ?? (props.Method == "xhtml" ? "text/html" : "text/html");
        var charset = props.Encoding;
        var content = $"{mediaType}; charset={charset}";

        XElement newMeta;
        if (existingMeta != null)
        {
            // Update the existing meta element's content attribute.
            newMeta = new XElement(existingMeta.Name);
            foreach (var attr in existingMeta.Attributes())
            {
                if (attr.Name.LocalName == "content")
                    continue;
                if (attr.Name.LocalName == "media-type")
                    continue;
                newMeta.SetAttributeValue(attr.Name, attr.Value);
            }
            newMeta.SetAttributeValue("content", content);
        }
        else
        {
            newMeta = new XElement(XName.Get("meta", rootElement.Name.NamespaceName),
                new XAttribute("http-equiv", "Content-Type"),
                new XAttribute("content", content));
        }

        var newHead = new XElement(head.Name);
        foreach (var attr in head.Attributes())
            newHead.SetAttributeValue(attr.Name, attr.Value);

        if (existingMeta == null)
        {
            newHead.Add(newMeta);
        }

        foreach (var child in head.Nodes())
        {
            if (child is XElement elem && elem == existingMeta)
                newHead.Add(newMeta);
            else
                newHead.Add(child);
        }

        var newRoot = new XElement(rootElement.Name);
        foreach (var attr in rootElement.Attributes())
            newRoot.SetAttributeValue(attr.Name, attr.Value);
        foreach (var child in rootElement.Elements())
        {
            if (child == head)
                newRoot.Add(newHead);
            else
                newRoot.Add(child);
        }

        return newRoot;
    }

    private static bool IsCdataSectionElement(XName elementName, Stylesheet.OutputProperties props)
    {
        foreach (var qname in props.CdataSectionElements)
        {
            if (qname.LocalName == elementName.LocalName && qname.NamespaceUri == elementName.NamespaceName)
                return true;
        }
        return false;
    }

    private static bool IsSuppressIndentationElement(XName elementName, Stylesheet.OutputProperties props)
    {
        foreach (var qname in props.SuppressIndentation)
        {
            if (qname.LocalName == elementName.LocalName && qname.NamespaceUri == elementName.NamespaceName)
                return true;
        }
        return false;
    }

    private static bool IsUriAttribute(XName attributeName)
    {
        var localName = attributeName.LocalName;
        var nsUri = attributeName.NamespaceName;

        // Only attributes in no namespace are URI-valued in HTML/XHTML.
        if (!string.IsNullOrEmpty(nsUri))
            return false;

        return localName.ToLowerInvariant() switch
        {
            "href" or "src" or "action" or "cite" or "longdesc" or "profile" or "usemap" or
            "classid" or "codebase" or "data" or "formaction" or "poster" or "background" or
            "dynsrc" or "lowsrc" => true,
            _ => false,
        };
    }

    private static string EscapeUriAttribute(string value)
    {
        // Normalize to NFC so precomposed characters are encoded consistently
        // with the W3C test expectations.
        var normalized = value.Normalize(System.Text.NormalizationForm.FormC);
        var sb = new System.Text.StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (ch <= 0x7F)
            {
                sb.Append(ch);
            }
            else
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(new[] { ch });
                foreach (var b in bytes)
                {
                    sb.Append('%');
                    sb.Append(b.ToString("X2"));
                }
            }
        }
        return sb.ToString();
    }

    private static bool IsHtmlVoidElement(string localName)
    {
        return localName.ToLowerInvariant() switch
        {
            "area" or "base" or "br" or "col" or "embed" or "hr" or "img" or "input" or
            "link" or "meta" or "param" or "source" or "track" or "wbr" => true,
            _ => false,
        };
    }

    private static bool IsHtmlRawContentElement(string localName)
    {
        return localName.ToLowerInvariant() is "script" or "style" or "textarea" or "title";
    }

    private static bool IsHtmlEmptyElement(string localName)
    {
        // Older HTML empty elements (HTML 4 and earlier).
        return localName.ToLowerInvariant() switch
        {
            "area" or "base" or "basefont" or "br" or "col" or "frame" or "hr" or "img" or
            "input" or "isindex" or "link" or "meta" or "param" => true,
            _ => false,
        };
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
        // Apply normalization and CDATA wrapping for raw path as well.
        if (TryGetNormalizationForm(props) is { } normForm)
            node = NormalizeXNode(node, normForm);
        node = WrapCdataSections(node, props);

        var sb = new System.Text.StringBuilder();
        using var writer = new StringWriter(sb);

        WriteByteOrderMark(writer, props);

        if (!props.OmitXmlDeclaration && props.Version == "1.1")
        {
            writer.Write("<?xml version=\"");
            writer.Write(props.Version);
            writer.Write("\" encoding=\"");
            writer.Write(props.Encoding);
            writer.Write("\"");
            if (props.Standalone is "yes" or "no")
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
            WriteDoctype(writer, doc.Root, props);
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
            WriteDoctype(writer, node as XElement, props);
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
            case XCData cdata:
                WriteCdataText(writer, cdata.Value);
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

            case XCData cdata:
                return new XCData(cdata.Value);
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
