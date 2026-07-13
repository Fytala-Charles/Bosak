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
//                      | Charles Korthout | 1.3   | 11-07-2026     | XHTML5 DOCTYPE formatting, html-version 5.0 default, prefix stripping, void elements.  |
//                      | Charles Korthout | 1.4   | 11-07-2026     | Added xsl:character-map application to XML, HTML, XHTML, and text output.              |
//                      | Charles Korthout | 1.5   | 11-07-2026     | Encoding-aware output: escape unrepresentable characters and split CDATA sections.     |
//                      | Charles Korthout | 1.6   | 11-07-2026     | XHTML 1.0 empty elements, DOCTYPE quote/namespace rules, alien-namespace meta guard.   |
//                      | Charles Korthout | 1.7   | 11-07-2026     | Added method="json" serialization, HTML namespace declaration output, and JSON char maps.|
//                      | Charles Korthout | 1.8   | 11-07-2026     | Added item-separator awareness and SENR0001 validation for maps/arrays/functions.       |
//                      | Charles Korthout | 1.9   | 12-07-2026     | Restrict SEPM0009 to XML/XHTML methods; support standalone value normalization.         |
//                      | Charles Korthout | 1.10  | 12-07-2026     | Added SENR0001 validation for maps/arrays/functions and attribute/namespace nodes.      |
//                      | Charles Korthout | 1.11  | 12-07-2026     | XML declaration defaults: include for XML/XHTML 1.0, omit for XHTML 5.0/HTML/text/JSON. |
//                      | Charles Korthout | 1.12  | 12-07-2026     | Apply character maps before escaping; combine XML surrogate-pair NCRs.                  |
//                      | Charles Korthout | 1.13  | 12-07-2026     | Text-output normalization/BOM; SEPM0009/SEPM0010; HTML doctype-before-first-element.    |
//                      | Charles Korthout | 1.14  | 12-07-2026     | Preserve CR in XML comments; JSON node method defaults; adaptive XML declaration.       |
//                      | Charles Korthout | 1.15  | 12-07-2026     | Preserve original namespace prefixes via annotation; copy annotations through normalizers.|
//                      | Charles Korthout | 1.16  | 12-07-2026     | Route prefixed namespace undeclarations to raw XML serializer.                         |
//                      | Charles Korthout | 1.17  | 12-07-2026     | Adaptive string literals use XPath escaping (double quotes) instead of JSON escapes.   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections.Concurrent;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Standard.Json;

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
        ValidateResultTree(value, props);
        ValidateSerializableItems(value, props);

        if (props.Method == "text")
        {
            return SerializeAsText(value, props);
        }

        if (props.Method == "html")
        {
            return SerializeAsHtml(value, props);
        }

        if (props.Method == "xhtml")
        {
            return SerializeAsXhtml(value, props);
        }

        if (props.Method == "json")
        {
            return SerializeAsJson(value, props);
        }

        if (props.Method == "adaptive")
        {
            return SerializeAsAdaptive(value, props);
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

        if (method == "html" && !props.HtmlVersionSpecified && props.VersionSpecified)
        {
            // For the HTML output method, the legacy @version attribute supplies the
            // HTML version when @html-version is absent.
            props.HtmlVersion = props.Version;
            props.HtmlVersionSpecified = true;
        }

        if (!props.HtmlVersionSpecified)
        {
            // HTML defaults to 5.0. XHTML without an explicit html-version is treated
            // as 1.0 for compatibility with the XSLT 2.0 test cases in the suite.
            props.HtmlVersion = method == "xhtml" ? "1.0" : "5.0";
        }

        bool omitXmlDeclarationWasSpecified = props.OmitXmlDeclarationSpecified;
        if (!omitXmlDeclarationWasSpecified)
        {
            // XML declaration defaults: omitted for HTML, text, and JSON; included for XML
            // and XHTML 1.0. The adaptive method delegates node serialization to the XML
            // method, so an XML declaration is emitted for each serialized node by default.
            props.OmitXmlDeclaration = method switch
            {
                "html" or "text" or "json" => true,
                "xhtml" => props.HtmlVersion == "5.0" && !props.StandaloneSpecified,
                _ => false
            };
            props.OmitXmlDeclarationSpecified = true;
        }

        if (!props.MediaTypeSpecified)
        {
            props.MediaType = method switch
            {
                "xml" => "text/xml",
                "html" => "text/html",
                "xhtml" => "text/html",
                "text" => "text/plain",
                "json" => "application/json",
                "adaptive" => "text/plain",
                _ => "text/xml"
            };
        }

        if (!props.ByteOrderMarkSpecified)
        {
            // Default is yes for UTF-16 and UTF-32, no for UTF-8 and others.
            var enc = props.Encoding.Trim().ToUpperInvariant();
            props.ByteOrderMark = enc is "UTF-16" or "UTF-16LE" or "UTF-16BE" or "UTF-32" or "UTF-32LE" or "UTF-32BE";
        }

        if (method == "json" && !props.EscapeSolidusSpecified)
        {
            // XSLT/XQuery Serialization 3.1 defaults to escaping solidus for JSON.
            props.EscapeSolidus = true;
            props.EscapeSolidusSpecified = true;
        }
    }

    /// <summary>
    /// Returns <c>true</c> if any output property was explicitly supplied by an
    /// <summary>
    /// Validates serialization properties that have cross-attribute constraints
    /// or require a supported encoding. Raises XSLT serialization errors.
    /// </summary>
    private static void ValidateOutputProperties(Stylesheet.OutputProperties props)
    {
        ValidateEncoding(props.Encoding);
        ValidateNormalizationForm(props);
        ValidateHtmlVersion(props);

        // SEPM0009: standalone or a non-default version together with doctype-system
        // is not allowed when the XML declaration is omitted. This only applies to
        // methods that emit an XML declaration (XML and XHTML).
        if (props.OmitXmlDeclaration && props.Method is "xml" or "xhtml")
        {
            bool hasStandalone = props.Standalone is "yes" or "no";
            bool nonDefaultVersionWithDoctype = props.VersionSpecified && props.Version != "1.0" &&
                !string.IsNullOrEmpty(props.DoctypeSystem);
            if (hasStandalone || nonDefaultVersionWithDoctype)
            {
                throw new XsltRuntimeException("SEPM0009",
                    "The XML declaration is omitted but a serialization property requires it.",
                    XdmValue.Undefined);
            }
        }

        // SEPM0010: undeclare-prefixes is only permitted with XML 1.1.
        if (props.UndeclarePrefixes && props.Method is "xml" or "xhtml" && props.Version != "1.1")
        {
            throw new XsltRuntimeException("SEPM0010",
                "undeclare-prefixes is only allowed with XML version 1.1.",
                XdmValue.Undefined);
        }
    }

    /// <summary>
    /// Validates that the requested normalization form is supported.
    /// Raises SESU0011 for unknown normalization forms.
    /// </summary>
    private static void ValidateNormalizationForm(Stylesheet.OutputProperties props)
    {
        var form = props.NormalizationForm?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(form) || form == "NONE")
            return;

        if (form is not "NFC" and not "NFD" and not "NFKC" and not "NFKD")
        {
            throw new XsltRuntimeException("SESU0011",
                $"Unsupported normalization form '{props.NormalizationForm}'.",
                XdmValue.Undefined);
        }
    }

    /// <summary>
    /// Validates that the requested HTML version is supported for HTML output.
    /// Raises SESU0013 for unsupported HTML versions.
    /// </summary>
    private static void ValidateHtmlVersion(Stylesheet.OutputProperties props)
    {
        if (props.Method != "html")
            return;

        var version = props.HtmlVersion.Trim();
        if (version is not "4.0" and not "5.0")
        {
            throw new XsltRuntimeException("SESU0013",
                $"Unsupported HTML version '{version}'.",
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
    /// Validates that the result value can be serialized with the requested method.
    /// Raises SENR0001 if the normalized sequence contains maps, arrays, functions,
    /// attribute nodes, or namespace nodes for XML, HTML, XHTML, or text output.
    /// </summary>
    private static void ValidateSerializableItems(XdmValue value, Stylesheet.OutputProperties props)
    {
        var method = props.Method;
        if (method == "json" || method == "adaptive")
            return;

        foreach (var item in FlattenItems(value))
        {
            if (item.IsMap || item.IsArray || item.IsFunction)
            {
                throw new XsltRuntimeException("SENR0001",
                    $"Cannot serialize a {(item.IsMap ? "map" : item.IsArray ? "array" : "function")} using method '{method}'.",
                    XdmValue.Undefined);
            }

            if (item.IsNode && item.NodeValue != null)
            {
                var kind = item.NodeValue.NodeKind;
                if (kind == XdmNodeKind.Attribute || kind == XdmNodeKind.Namespace)
                {
                    throw new XsltRuntimeException("SENR0001",
                        $"Cannot serialize a {kind} node using method '{method}'.",
                        XdmValue.Undefined);
                }
            }
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
            {
                // When the method is inferred from the result tree, preserve the XML
                // declaration so the output is recognizably XHTML.
                props.OmitXmlDeclaration = false;
                props.OmitXmlDeclarationSpecified = true;
            }
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

    private static string SerializeAsText(XdmValue value, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        WriteByteOrderMark(writer, props);

        bool separatorAbsent = !props.ItemSeparatorSpecified || props.ItemSeparator == "#absent";
        var normalized = NormalizeRawSequence(value, separatorAbsent ? null : props.ItemSeparator);
        foreach (var obj in normalized)
        {
            if (obj is string s)
            {
                writer.Write(MapCharacters(s, props));
            }
            else if (obj is XdmValue xdm && xdm.IsNode && xdm.NodeValue != null)
            {
                writer.Write(TextMethodNodeString(xdm.NodeValue, props));
            }
        }

        writer.Flush();
        var result = writer.ToString();

        // Apply Unicode normalization after character mapping.
        if (TryGetNormalizationForm(props) is { } normForm)
            result = result.Normalize(normForm);

        return result;
    }

    private static string TextMethodNodeString(IXdmNode node, Stylesheet.OutputProperties props)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Comment:
                return $"<!--{node.StringValue}-->";
            case XdmNodeKind.ProcessingInstruction:
                return $"<?{node.LocalName} {node.StringValue}?>";
            case XdmNodeKind.Text:
                return MapCharacters(node.StringValue, props);
            case XdmNodeKind.Element:
            case XdmNodeKind.Document:
                var elementSb = new System.Text.StringBuilder();
                CollectTextFromNode(node, elementSb, props);
                return elementSb.ToString();
            default:
                return MapCharacters(node.StringValue, props);
        }
    }

    private static string SerializeAsJson(XdmValue value, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        WriteByteOrderMark(writer, props);

        var options = new XdmJsonOptions
        {
            Indent = props.Indent,
            AllowDuplicateNames = props.AllowDuplicateNames,
            EscapeSolidus = props.EscapeSolidus,
            NodeSerializer = node => SerializeNodeForJson(node, props),
            CharacterMap = props.CharacterMap
        };
        var json = XdmJsonSerializer.Serialize(value, options);
        writer.Write(json);
        writer.Flush();
        return writer.ToString();
    }

    private static string SerializeNodeForJson(XdmValue nodeValue, Stylesheet.OutputProperties props)
    {
        var nodeMethod = props.JsonNodeOutputMethod.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(nodeMethod) || nodeMethod == "json")
            nodeMethod = "xml";

        var nodeProps = props.Clone();
        nodeProps.Method = nodeMethod;
        nodeProps.MethodSpecified = true;
        nodeProps.OmitXmlDeclaration = true;
        nodeProps.OmitXmlDeclarationSpecified = true;
        nodeProps.JsonNodeOutputMethod = "xml";
        // Re-apply method-dependent defaults (include-content-type, media-type, byte-order-mark)
        // for the chosen node output method rather than inheriting JSON-specific defaults.
        nodeProps.IncludeContentTypeSpecified = false;
        nodeProps.MediaTypeSpecified = false;
        nodeProps.ByteOrderMarkSpecified = false;
        nodeProps.EscapeSolidusSpecified = false;
        ApplyMethodDefaults(nodeProps);
        return Serialize(nodeValue, nodeProps);
    }

    private static string SerializeAsAdaptive(XdmValue value, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        WriteByteOrderMark(writer, props);

        var items = FlattenItems(value).ToList();
        string separator = (!props.ItemSeparatorSpecified || props.ItemSeparator == "#absent")
            ? "\n"
            : props.ItemSeparator;
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
                writer.Write(separator);
            SerializeAdaptiveItem(writer, items[i], props);
        }

        writer.Flush();
        return writer.ToString();
    }

    private static void SerializeAdaptiveItem(TextWriter writer, XdmValue item, Stylesheet.OutputProperties props)
    {
        if (item.IsUndefined)
            return;

        if (item.IsMap)
        {
            writer.Write("map{");
            bool first = true;
            foreach (var kv in item.MapValue.Entries)
            {
                if (!first)
                    writer.Write(',');
                first = false;
                SerializeAdaptiveItem(writer, kv.Key, props);
                writer.Write(':');
                SerializeAdaptiveItem(writer, kv.Value, props);
            }
            writer.Write('}');
            return;
        }

        if (item.IsArray)
        {
            writer.Write('[');
            bool first = true;
            foreach (var arrItem in item.ArrayValue.Values)
            {
                if (!first)
                    writer.Write(',');
                first = false;
                SerializeAdaptiveItem(writer, arrItem, props);
            }
            writer.Write(']');
            return;
        }

        if (item.IsNode && item.NodeValue != null)
        {
            var node = item.NodeValue;
            switch (node.NodeKind)
            {
                case XdmNodeKind.Attribute:
                    writer.Write(Xml11NameCodec.EncodeName(node.LocalName));
                    writer.Write("=\"");
                    writer.Write(EscapeAdaptiveString(node.StringValue, isAttribute: true, props.CharacterMap));
                    writer.Write('"');
                    break;

                case XdmNodeKind.Text:
                    writer.Write(EscapeAdaptiveString(node.StringValue, isAttribute: false, props.CharacterMap));
                    break;

                case XdmNodeKind.Comment:
                    writer.Write("<!--");
                    writer.Write(node.StringValue);
                    writer.Write("-->");
                    break;

                case XdmNodeKind.ProcessingInstruction:
                    writer.Write("<?");
                    writer.Write(node.LocalName);
                    writer.Write(' ');
                    writer.Write(node.StringValue);
                    writer.Write("?>");
                    break;

                default:
                    {
                        var nodeProps = props.Clone();
                        nodeProps.Method = "xml";
                        nodeProps.MethodSpecified = true;
                        // Adaptive serialization delegates element/document nodes to the XML
                        // output method, preserving the effective omit-xml-declaration setting.
                        nodeProps.OmitXmlDeclaration = props.OmitXmlDeclaration;
                        nodeProps.OmitXmlDeclarationSpecified = true;
                        writer.Write(Serialize(item, nodeProps));
                        break;
                    }
            }
            return;
        }

        // Atomic value
        if (item.Kind == XdmValueKind.String)
        {
            writer.Write('"');
            writer.Write(EscapeAdaptiveString(item.StringValue, isAttribute: false, props.CharacterMap));
            writer.Write('"');
        }
        else
        {
            writer.Write(item.ToString());
        }
    }

    private static string EscapeAdaptiveString(string value, bool isAttribute, Dictionary<int, string>? characterMap)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var rune in value.EnumerateRunes())
        {
            // Character maps are applied before adaptive escaping. The replacement string is
            // output as-is and is not itself escaped or subject to further mapping.
            if (characterMap != null && characterMap.TryGetValue(rune.Value, out var replacement))
            {
                sb.Append(replacement);
                continue;
            }

            // Adaptive strings are enclosed in XPath/XQuery string literals: only the
            // delimiting quotation mark needs to be escaped, and that is done by doubling it.
            if (rune.Value == '"')
            {
                sb.Append("\"\"");
                continue;
            }

            sb.Append(rune.ToString());
        }
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

        // Remember the original root element reference so that we can locate it in
        // the flattened item list after it has been cloned for meta/normalization.
        XElement? originalRootElement = rootElement;


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
                rootDocument = (XDocument)NormalizeCommentsAndPis(rootDocument, normForm);
            else if (rootElement != null)
                rootElement = (XElement)NormalizeCommentsAndPis(rootElement, normForm);
        }

        bool doctypeNeeded = rootElement != null &&
            ((props.Method == "html" && props.HtmlVersion == "5.0" && IsHtmlRootElement(rootElement.Name)) ||
             !string.IsNullOrEmpty(props.DoctypePublic) ||
             !string.IsNullOrEmpty(props.DoctypeSystem));
        bool doctypeWritten = !doctypeNeeded;

        if (rootDocument != null)
        {
            foreach (var child in rootDocument.Nodes())
            {
                if (!doctypeWritten && child is XElement)
                {
                    WriteDoctype(writer, rootElement, props);
                    doctypeWritten = true;
                }
                WriteHtmlNode(writer, child, props, 0);
            }
        }
        else if (rootElement != null)
        {
            // Fragment containing a root element plus possible other top-level nodes.
            // The DOCTYPE must be output immediately before the root element, so we
            // iterate the original items rather than forcing the root element first.
            foreach (var item in items)
            {
                if (item.IsNode && item.NodeValue != null &&
                    item.NodeValue is XDocumentNode xdn && xdn.UnderlyingObject == originalRootElement)
                {
                    if (!doctypeWritten)
                    {
                        WriteDoctype(writer, rootElement, props);
                        doctypeWritten = true;
                    }
                    WriteHtmlNode(writer, rootElement, props, 0);
                }
                else if (item.IsNode && item.NodeValue != null)
                {
                    WriteHtmlNode(writer, item.NodeValue, props, 0);
                }
                else if (!item.IsUndefined)
                {
                    WriteHtmlEscaped(writer, item.ToString(), props);
                }
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (item.IsNode && item.NodeValue != null)
                    WriteHtmlNode(writer, item.NodeValue, props, 0);
                else if (!item.IsUndefined)
                    WriteHtmlEscaped(writer, item.ToString(), props);
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

        // HTML5 XHTML serialization requires the XHTML namespace to use the default
        // namespace rather than a prefix.
        if (props.Method == "xhtml" && props.HtmlVersion == "5.0")
        {
            if (rootDocument != null)
                rootDocument = (XDocument)NormalizeXhtmlNamespacesForHtml5(rootDocument);
            else if (isDocumentMode && rootElement != null)
                rootElement = (XElement)NormalizeXhtmlNamespacesForHtml5(rootElement);
            else
            {
                for (int i = 0; i < fragmentNodes.Count; i++)
                    fragmentNodes[i] = NormalizeXhtmlNamespacesForHtml5(fragmentNodes[i]);
            }

            rootElement = rootDocument?.Root
                ?? (isDocumentMode ? rootElement : fragmentNodes.OfType<XElement>()
                    .FirstOrDefault(e => string.Equals(e.Name.LocalName, "html", StringComparison.OrdinalIgnoreCase)));
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
                rootDocument = (XDocument)NormalizeCommentsAndPis(rootDocument, normForm);
            else if (isDocumentMode && rootElement != null)
                rootElement = (XElement)NormalizeCommentsAndPis(rootElement, normForm);
            else
            {
                for (int i = 0; i < fragmentNodes.Count; i++)
                    fragmentNodes[i] = NormalizeCommentsAndPis(fragmentNodes[i], normForm);
            }
        }

        // Wrap CDATA section elements and split around characters that cannot be
        // represented in the requested encoding.
        if (rootDocument != null)
            rootDocument = (XDocument)WrapCdataSections(rootDocument, props);
        else if (isDocumentMode && rootElement != null)
            rootElement = (XElement)WrapCdataSections(rootElement, props);
        else
        {
            for (int i = 0; i < fragmentNodes.Count; i++)
                fragmentNodes[i] = WrapCdataSections(fragmentNodes[i], props);
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

        // Determine whether a DOCTYPE declaration must be emitted and which element
        // governs its name. It is written immediately before the first element.
        bool doctypeNeeded;
        XElement? doctypeRoot;
        if (isDocumentMode)
        {
            doctypeRoot = rootElement;
            doctypeNeeded = doctypeRoot != null &&
                (!string.IsNullOrEmpty(props.DoctypePublic) ||
                 !string.IsNullOrEmpty(props.DoctypeSystem) ||
                 (props.Method == "xhtml" && props.HtmlVersion == "5.0" &&
                  IsHtmlRootElement(doctypeRoot.Name)));
        }
        else
        {
            if (!string.IsNullOrEmpty(props.DoctypePublic) || !string.IsNullOrEmpty(props.DoctypeSystem))
            {
                doctypeRoot = fragmentNodes.OfType<XElement>().FirstOrDefault();
            }
            else if (props.Method == "xhtml" && props.HtmlVersion == "5.0")
            {
                doctypeRoot = fragmentNodes.OfType<XElement>()
                    .FirstOrDefault(e => IsHtmlRootElement(e.Name));
            }
            else
            {
                doctypeRoot = null;
            }
            doctypeNeeded = doctypeRoot != null;
        }
        bool doctypeWritten = !doctypeNeeded;

        var initialBindings = new Dictionary<string, string> { ["xml"] = "http://www.w3.org/XML/1998/namespace" };
        if (rootDocument != null)
        {
            foreach (var child in rootDocument.Nodes())
            {
                if (!doctypeWritten && child is XElement)
                {
                    WriteDoctype(writer, doctypeRoot, props);
                    doctypeWritten = true;
                }
                WriteXhtmlNode(writer, child, props, 0, new Dictionary<string, string>(initialBindings));
            }
        }
        else if (isDocumentMode && rootElement != null)
        {
            WriteDoctype(writer, doctypeRoot, props);
            WriteXhtmlNode(writer, rootElement, props, 0, new Dictionary<string, string>(initialBindings));
        }
        else
        {
            foreach (var node in fragmentNodes)
            {
                if (!doctypeWritten && node is XElement)
                {
                    WriteDoctype(writer, doctypeRoot, props);
                    doctypeWritten = true;
                }
                WriteXhtmlNode(writer, node, props, 0, new Dictionary<string, string>(initialBindings));
            }
            foreach (var item in items.Where(i => !i.IsNode && !i.IsUndefined))
            {
                WriteXmlEscaped(writer, item.ToString(), props);
            }
        }

        writer.Flush();
        return NormalizeSurrogatePairEntities(writer.ToString());
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

    /// <summary>
    /// Performs sequence normalization for the text and XML output methods.
    /// Atomic items are converted to strings; adjacent strings are merged with a single
    /// space when no item-separator is supplied, otherwise the supplied separator is
    /// inserted between every pair of items.
    /// </summary>
    private static List<object> NormalizeRawSequence(XdmValue value, string? itemSeparator)
    {
        var items = FlattenItems(value).ToList();
        var normalized = new List<object>(items.Count);
        foreach (var item in items)
        {
            if (item.IsUndefined)
                continue;
            if (item.IsNode && item.NodeValue != null)
                normalized.Add(item);
            else
                normalized.Add(item.ToString());
        }

        if (itemSeparator == null)
        {
            // Absent item-separator: merge each maximal subsequence of adjacent strings
            // into a single string with a single space between the original values.
            var merged = new List<object>();
            for (int i = 0; i < normalized.Count; i++)
            {
                if (normalized[i] is string s)
                {
                    var sb = new StringBuilder(s);
                    while (i + 1 < normalized.Count && normalized[i + 1] is string next)
                    {
                        sb.Append(' ');
                        sb.Append(next);
                        i++;
                    }
                    merged.Add(sb.ToString());
                }
                else
                {
                    merged.Add(normalized[i]);
                }
            }
            return merged;
        }
        else
        {
            // Explicit item-separator (which may be empty): insert between every pair.
            var merged = new List<object>();
            for (int i = 0; i < normalized.Count; i++)
            {
                if (i > 0)
                    merged.Add(itemSeparator);
                merged.Add(normalized[i]);
            }
            return merged;
        }
    }

    /// <summary>
    /// Strips prefixes from elements in the XHTML namespace for HTML5 serialization,
    /// replacing them with the default namespace declaration. Other namespaces are
    /// left untouched.
    /// </summary>
    private static XNode NormalizeXhtmlNamespacesForHtml5(XNode node)
    {
        const string xhtmlNs = "http://www.w3.org/1999/xhtml";
        const string svgNs = "http://www.w3.org/2000/svg";
        const string mathMlNs = "http://www.w3.org/1998/Math/MathML";

        switch (node)
        {
            case XDocument doc:
                var newDoc = new XDocument();
                if (doc.Declaration != null)
                {
                    newDoc.Declaration = new XDeclaration(
                        doc.Declaration.Version,
                        doc.Declaration.Encoding,
                        doc.Declaration.Standalone);
                }
                foreach (var child in doc.Nodes())
                    newDoc.Add(NormalizeXhtmlNamespacesForHtml5(child));
                return newDoc;

            case XElement elem:
                var nsUri = elem.Name.NamespaceName;
                var isSpecialNs = nsUri == xhtmlNs || nsUri == svgNs || nsUri == mathMlNs;
                var newElem = new XElement(elem.Name);

                if (isSpecialNs)
                {
                    // HTML5 serialization uses the default namespace for XHTML, SVG, and MathML.
                    newElem.Name = XName.Get(elem.Name.LocalName, nsUri);
                    newElem.SetAttributeValue("xmlns", nsUri);
                }

                foreach (var attr in elem.Attributes().Where(a => !a.IsNamespaceDeclaration))
                    newElem.SetAttributeValue(attr.Name, attr.Value);

                foreach (var nsAttr in elem.Attributes().Where(a => a.IsNamespaceDeclaration))
                {
                    // Drop prefixed declarations for the HTML5 namespaces; the default
                    // namespace declaration added above replaces them.
                    if (nsAttr.Name.LocalName != "xmlns" &&
                        (nsAttr.Value == xhtmlNs || nsAttr.Value == svgNs || nsAttr.Value == mathMlNs))
                        continue;
                    newElem.SetAttributeValue(nsAttr.Name, nsAttr.Value);
                }

                foreach (var child in elem.Nodes())
                    newElem.Add(NormalizeXhtmlNamespacesForHtml5(child));

                // Preserve any annotations attached by the XSLT processor (e.g. the
                // preferred namespace prefix chosen for a literal result element).
                foreach (var annotation in elem.Annotations<object>())
                    newElem.AddAnnotation(annotation);

                return newElem;

            case XComment comment:
                return new XComment(comment.Value);

            case XProcessingInstruction pi:
                return new XProcessingInstruction(pi.Target, pi.Data);

            case XText text:
                return new XText(text.Value);

            default:
                return node;
        }
    }

    private static void CollectTextFromNode(IXdmNode node, System.Text.StringBuilder sb, Stylesheet.OutputProperties props)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Text:
                sb.Append(MapCharacters(node.StringValue, props));
                break;
            case XdmNodeKind.Element:
                foreach (var child in node.Axis(XdmAxis.Child))
                    CollectTextFromNode(child.NodeValue!, sb, props);
                break;
            case XdmNodeKind.Document:
                foreach (var child in node.Axis(XdmAxis.Child))
                    CollectTextFromNode(child.NodeValue!, sb, props);
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
        settings.OmitXmlDeclaration = true;
        using var xmlWriter = XmlWriter.Create(writer, settings);

        bool separatorAbsent = !props.ItemSeparatorSpecified || props.ItemSeparator == "#absent";
        var normalized = NormalizeRawSequence(XdmValue.FromSequence(XdmSequence.FromSource(sequence)), separatorAbsent ? null : props.ItemSeparator);
        foreach (var obj in normalized)
        {
            if (obj is string s)
            {
                xmlWriter.WriteString(s);
            }
            else if (obj is XdmValue xdm && xdm.IsNode && xdm.NodeValue != null)
            {
                WriteNode(xmlWriter, xdm.NodeValue);
            }
        }

        xmlWriter.Flush();
        return NormalizeSurrogatePairEntities(NormalizeXmlEmptyElements(writer.ToString()));
    }

    private static string SerializeXElement(XElement element, Stylesheet.OutputProperties props)
    {
        if (element.Name.LocalName == "__xdm_doc__" && element.Name.NamespaceName == "")
            return SerializeXmlFragment(element, props);

        if (props.Version == "1.1")
            return SerializeRaw(element, props);

        if (props.CharacterMap != null && props.CharacterMap.Count > 0)
        {
            ValidateXml10(element);
            return SerializeRaw(element, props);
        }

        ValidateXml10(element);
        return SerializeWithEncoding(element, props);
    }

    private static string SerializeXmlFragment(XElement wrapper, Stylesheet.OutputProperties props)
    {
        // XML 1.1 and trees with prefixed namespace undeclarations must use the raw
        // serializer because XmlWriter cannot represent xmlns:prefix="".
        if (props.Version == "1.1" || HasUndeclarationAnnotations(wrapper))
            return SerializeRaw(wrapper, props);

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

            bool doctypeNeeded = fragment.Elements().Any() &&
                (!string.IsNullOrEmpty(props.DoctypePublic) || !string.IsNullOrEmpty(props.DoctypeSystem));
            bool doctypeWritten = !doctypeNeeded;
            foreach (var child in fragment.Nodes())
            {
                if (!doctypeWritten && child is XElement)
                {
                    WriteDoctype(xmlWriter, child as XElement, props);
                    doctypeWritten = true;
                }
                child.WriteTo(xmlWriter);
            }

            xmlWriter.Flush();
        }

        var result = encoding.GetString(stream.ToArray());
        return NormalizeSurrogatePairEntities(NormalizeXmlEmptyElements(ConvertHexEntitiesToDecimal(result)));
    }

    private static string SerializeXDocument(XDocument document, Stylesheet.OutputProperties props)
    {
        if (props.Version == "1.1")
            return SerializeRaw(document, props);

        if (props.CharacterMap != null && props.CharacterMap.Count > 0)
        {
            ValidateXml10(document);
            return SerializeRaw(document, props);
        }

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

                bool doctypeNeeded = doc.Root != null &&
                    (!string.IsNullOrEmpty(props.DoctypePublic) || !string.IsNullOrEmpty(props.DoctypeSystem));
                bool doctypeWritten = !doctypeNeeded;
                foreach (var child in doc.Nodes())
                {
                    if (!doctypeWritten && child is XElement)
                    {
                        WriteDoctype(xmlWriter, doc.Root, props);
                        doctypeWritten = true;
                    }
                    child.WriteTo(xmlWriter);
                }
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
        return NormalizeSurrogatePairEntities(NormalizeXmlEmptyElements(ConvertHexEntitiesToDecimal(result)));
    }

    /// <summary>
    /// Removes the space that XmlWriter inserts before <c>/&gt;</c> on empty XML elements.
    /// XSLT test-suite serialization-matches assertions expect XML empty-element tags
    /// without the space, while HTML/XHTML output preserves it for compatibility.
    /// </summary>
    private static string NormalizeXmlEmptyElements(string xml)
    {
        return xml.Replace(" />", "/>");
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
    /// Combines adjacent decimal numeric character references that represent a UTF-16
    /// surrogate pair into a single reference for the Unicode scalar value.
    /// </summary>
    private static string NormalizeSurrogatePairEntities(string xml)
    {
        // Fast path: no numeric entities at all.
        if (xml.IndexOf("&#", StringComparison.Ordinal) < 0)
            return xml;

        var sb = new System.Text.StringBuilder(xml.Length);
        int i = 0;
        while (i < xml.Length)
        {
            int start = xml.IndexOf("&#", i, StringComparison.Ordinal);
            if (start < 0)
            {
                sb.Append(xml, i, xml.Length - i);
                break;
            }

            sb.Append(xml, i, start - i);

            int end = xml.IndexOf(';', start + 2);
            if (end < 0 || !int.TryParse(xml.AsSpan(start + 2, end - start - 2), out int firstCp))
            {
                sb.Append(xml, start, 1);
                i = start + 1;
                continue;
            }

            // Look for a following decimal NCR that forms a surrogate pair.
            int nextStart = end + 1;
            if (char.IsHighSurrogate((char)firstCp) &&
                nextStart + 2 < xml.Length &&
                xml[nextStart] == '&' && xml[nextStart + 1] == '#')
            {
                int nextEnd = xml.IndexOf(';', nextStart + 2);
                if (nextEnd > 0 &&
                    int.TryParse(xml.AsSpan(nextStart + 2, nextEnd - nextStart - 2), out int secondCp) &&
                    char.IsLowSurrogate((char)secondCp))
                {
                    int scalar = char.ConvertToUtf32((char)firstCp, (char)secondCp);
                    sb.Append("&#");
                    sb.Append(scalar);
                    sb.Append(';');
                    i = nextEnd + 1;
                    continue;
                }
            }

            sb.Append(xml, start, end - start + 1);
            i = end + 1;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Applies the effective character map from <paramref name="props"/> to a string value.
    /// Returns the original value when no character map is in effect.
    /// </summary>
    private static string MapCharacters(string? value, Stylesheet.OutputProperties props)
    {
        if (string.IsNullOrEmpty(value) || props.CharacterMap == null || props.CharacterMap.Count == 0)
            return value ?? string.Empty;

        return ApplyCharacterMap(value, props.CharacterMap);
    }

    /// <summary>
    /// Applies a character map to a string value, replacing each mapped character
    /// with its corresponding replacement string.
    /// </summary>
    private static string ApplyCharacterMap(string value, Dictionary<int, string> map)
    {
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (var rune in value.EnumerateRunes())
        {
            if (map.TryGetValue(rune.Value, out var replacement))
                sb.Append(replacement);
            else
                sb.Append(rune.ToString());
        }
        return sb.ToString();
    }

    /// <summary>
    /// Splits a string into normal and immune segments. Immune segments contain replacement
    /// strings produced by a character map; normal segments contain the original characters
    /// that were not mapped and remain subject to escaping and normalization.
    /// </summary>
    private static List<(bool immune, string text)> GetCharacterMapSegments(string value, Dictionary<int, string>? map)
    {
        if (map == null || map.Count == 0)
            return new List<(bool, string)> { (false, value) };

        var segments = new List<(bool immune, string text)>();
        var currentImmune = false;
        var current = new System.Text.StringBuilder();
        foreach (var rune in value.EnumerateRunes())
        {
            if (map.TryGetValue(rune.Value, out var replacement))
            {
                if (current.Length > 0 && !currentImmune)
                {
                    segments.Add((false, current.ToString()));
                    current.Clear();
                }
                currentImmune = true;
                current.Append(replacement);
            }
            else
            {
                if (current.Length > 0 && currentImmune)
                {
                    segments.Add((true, current.ToString()));
                    current.Clear();
                }
                currentImmune = false;
                current.Append(rune.ToString());
            }
        }
        if (current.Length > 0)
            segments.Add((currentImmune, current.ToString()));
        return segments;
    }

    /// <summary>
    /// Applies a Unicode normalization form to <paramref name="value"/> while keeping
    /// characters produced by a character map immune from normalization.
    /// </summary>
    private static string NormalizeWithCharacterMap(string value, Dictionary<int, string>? map, System.Text.NormalizationForm? form)
    {
        if (form == null && (map == null || map.Count == 0))
            return value;

        var segments = new List<(bool immune, string text)>();
        var currentImmune = false;
        var current = new System.Text.StringBuilder();
        foreach (var rune in value.EnumerateRunes())
        {
            if (map != null && map.TryGetValue(rune.Value, out var replacement))
            {
                if (current.Length > 0 && !currentImmune)
                {
                    segments.Add((false, current.ToString()));
                    current.Clear();
                }
                currentImmune = true;
                current.Append(replacement);
            }
            else
            {
                if (current.Length > 0 && currentImmune)
                {
                    segments.Add((true, current.ToString()));
                    current.Clear();
                }
                currentImmune = false;
                current.Append(rune.ToString());
            }
        }
        if (current.Length > 0)
            segments.Add((currentImmune, current.ToString()));

        var sb = new System.Text.StringBuilder(value.Length);
        foreach (var (immune, text) in segments)
        {
            if (immune || form == null)
                sb.Append(text);
            else
                sb.Append(text.Normalize(form.Value));
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
                foreach (var annotation in element.Annotations<object>())
                    clonedElem.AddAnnotation(annotation);
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
    /// Normalizes comment and processing-instruction values, leaving text and attribute
    /// values unchanged so that character-map replacement strings remain immune to
    /// normalization during raw serialization.
    /// </summary>
    private static XNode NormalizeCommentsAndPis(XNode node, System.Text.NormalizationForm form)
    {
        switch (node)
        {
            case XDocument doc:
                var clonedDoc = new XDocument(doc.Declaration);
                foreach (var child in doc.Nodes())
                    clonedDoc.Add(NormalizeCommentsAndPis(child, form));
                return clonedDoc;
            case XElement element:
                var clonedElem = new XElement(element.Name);
                foreach (var attr in element.Attributes())
                    clonedElem.SetAttributeValue(attr.Name, attr.Value);
                foreach (var child in element.Nodes())
                    clonedElem.Add(NormalizeCommentsAndPis(child, form));
                foreach (var annotation in element.Annotations<object>())
                    clonedElem.AddAnnotation(annotation);
                return clonedElem;
            case XComment comment:
                return new XComment(comment.Value.Normalize(form));
            case XProcessingInstruction pi:
                return new XProcessingInstruction(pi.Target, pi.Data.Normalize(form));
            default:
                return node;
        }
    }

    /// <summary>
    /// Returns a deep clone with text children of cdata-section-elements wrapped as CDATA nodes.
    /// Characters that cannot be represented in the output encoding are left as ordinary text
    /// nodes so they are serialized as numeric character references outside the CDATA section.
    /// </summary>
    private static XNode WrapCdataSections(XNode node, Stylesheet.OutputProperties props)
    {
        if (props.CdataSectionElements.Count == 0)
            return node;

        var encoding = GetEncodingWithExceptionFallback(props.Encoding);

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
                        foreach (var piece in SplitTextForCdata(text.Value, encoding))
                            cloned.Add(piece);
                    }
                    else
                    {
                        cloned.Add(WrapCdataSections(child, props));
                    }
                }
                foreach (var annotation in element.Annotations<object>())
                    cloned.AddAnnotation(annotation);
                return cloned;
            default:
                return node;
        }
    }

    /// <summary>
    /// Marker annotation placed on text nodes produced by <see cref="SplitTextForCdata"/>
    /// so that character maps are not applied to characters that were split out of a
    /// cdata-section-element text node.
    /// </summary>
    private sealed class CdataSplitAnnotation { }

    /// <summary>
    /// Splits a text value into representable CDATA runs and unrepresentable single-character
    /// text nodes. The unrepresentable characters are emitted later as numeric character references.
    /// </summary>
    private static IEnumerable<XNode> SplitTextForCdata(string text, Encoding? encoding)
    {
        if (encoding == null)
        {
            yield return new XCData(text);
            yield break;
        }

        var run = new StringBuilder();
        foreach (var ch in text)
        {
            if (CanEncode(ch, encoding))
            {
                run.Append(ch);
            }
            else
            {
                if (run.Length > 0)
                {
                    yield return new XCData(run.ToString());
                    run.Clear();
                }
                var splitText = new XText(ch.ToString());
                splitText.AddAnnotation(new CdataSplitAnnotation());
                yield return splitText;
            }
        }

        if (run.Length > 0)
            yield return new XCData(run.ToString());
    }

    // ---------------------------------------------------------------------------------------------
    // HTML serialization
    // ---------------------------------------------------------------------------------------------

    private static void WriteHtmlNode(TextWriter writer, IXdmNode node, Stylesheet.OutputProperties props, int depth, Dictionary<string, string>? inScopeBindings = null)
    {
        inScopeBindings ??= new Dictionary<string, string> { ["xml"] = "http://www.w3.org/XML/1998/namespace" };

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
                    WriteHtmlNode(writer, new XDocumentNode(child), props, depth, inScopeBindings);
                break;
            case XElement elem when elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "":
                foreach (var child in elem.Nodes())
                    WriteHtmlNode(writer, new XDocumentNode(child), props, depth, inScopeBindings);
                break;
            case XElement elem:
                WriteHtmlElement(writer, elem, props, depth, inScopeBindings);
                break;
            case XText text:
                WriteHtmlEscaped(writer, text.Value, props);
                break;
            case XComment comment:
                writer.Write("<!--");
                writer.Write(comment.Value);
                writer.Write("-->");
                break;
            case XProcessingInstruction pi:
                ValidateHtmlProcessingInstruction(pi.Data);
                writer.Write("<?");
                writer.Write(pi.Target);
                writer.Write(' ');
                writer.Write(pi.Data);
                writer.Write("?>");
                break;
        }
    }

    private static void ValidateHtmlProcessingInstruction(string data)
    {
        // HTML output cannot represent a processing instruction that contains a '>'
        // because the processor terminates the PI at the first '>'.
        if (data.IndexOf('>') >= 0)
        {
            throw new XsltRuntimeException("SERE0015",
                "A processing instruction in HTML output contains a '>' character.",
                XdmValue.Undefined);
        }
    }

    private static void WriteHtmlNode(TextWriter writer, XNode node, Stylesheet.OutputProperties props, int depth, Dictionary<string, string>? inScopeBindings = null)
    {
        inScopeBindings ??= new Dictionary<string, string> { ["xml"] = "http://www.w3.org/XML/1998/namespace" };

        switch (node)
        {
            case XElement elem when elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "":
                foreach (var child in elem.Nodes())
                    WriteHtmlNode(writer, child, props, depth, inScopeBindings);
                break;
            case XElement elem:
                WriteHtmlElement(writer, elem, props, depth, inScopeBindings);
                break;
            case XText text:
                WriteHtmlEscaped(writer, text.Value, props);
                break;
            case XComment comment:
                writer.Write("<!--");
                writer.Write(comment.Value);
                writer.Write("-->");
                break;
            case XProcessingInstruction pi:
                ValidateHtmlProcessingInstruction(pi.Data);
                writer.Write("<?");
                writer.Write(pi.Target);
                writer.Write(' ');
                writer.Write(pi.Data);
                writer.Write("?>");
                break;
        }
    }

    private static void WriteHtmlElement(TextWriter writer, XElement element, Stylesheet.OutputProperties props, int depth, Dictionary<string, string> inScopeBindings)
    {
        var localName = element.Name.LocalName;
        var isEmpty = !element.Nodes().Any();
        var isRawContent = IsHtmlRawContentElement(localName);

        writer.Write('<');
        writer.Write(localName);

        var elemNs = element.Name.NamespaceName;
        if (!string.IsNullOrEmpty(elemNs) && inScopeBindings.GetValueOrDefault("") != elemNs)
        {
            writer.Write(" xmlns=\"");
            writer.Write(elemNs);
            writer.Write('"');
            inScopeBindings[""] = elemNs;
        }

        foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            writer.Write(' ');
            writer.Write(attr.Name.LocalName);
            writer.Write("=\"");
            var value = attr.Value;
            bool isUri = props.EscapeUriAttributes && IsUriAttribute(attr.Name);
            if (isUri)
                value = EscapeUriAttribute(value);
            // Character maps do not apply to URI-valued attributes when URI escaping is enabled.
            WriteHtmlEscaped(writer, value, props, applyCharacterMap: !isUri);
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

        var childBindings = new Dictionary<string, string>(inScopeBindings);
        if (isRawContent)
        {
            foreach (var child in element.Nodes())
            {
                if (child is XText text)
                    writer.Write(MapCharacters(text.Value, props));
                else if (child is XElement childElem)
                    WriteHtmlElement(writer, childElem, props, depth + 1, childBindings);
                else
                    WriteHtmlNode(writer, child, props, depth + 1, childBindings);
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
                WriteHtmlNode(writer, child, props, depth + 1, childBindings);
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

    private static void WriteHtmlEscaped(TextWriter writer, string value, Stylesheet.OutputProperties props, bool applyCharacterMap = true)
    {
        var map = applyCharacterMap ? props.CharacterMap : null;
        var form = applyCharacterMap ? TryGetNormalizationForm(props) : null;
        foreach (var (immune, text) in GetCharacterMapSegments(value, map))
        {
            if (immune)
            {
                WriteEncodingOnly(writer, text, props);
                continue;
            }

            var normalized = form != null ? text.Normalize(form.Value) : text;
            foreach (var rune in normalized.EnumerateRunes())
            {
                var cp = rune.Value;
                switch (cp)
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
                        if (!IsRepresentable(cp, props.Encoding))
                        {
                            writer.Write("&#");
                            writer.Write(cp);
                            writer.Write(';');
                        }
                        else
                        {
                            writer.Write(rune.ToString());
                        }
                        break;
                }
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
            case XCData cdata:
                WriteCdataText(writer, cdata.Value);
                break;
            case XText text:
                WriteXmlEscaped(writer, text.Value, props, applyCharacterMap: text.Annotation<CdataSplitAnnotation>() == null);
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
                WriteXmlEscaped(writer, text.Value, props, applyCharacterMap: text.Annotation<CdataSplitAnnotation>() == null);
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
        var isVoid = isInXhtmlNs && IsXhtmlVoidElement(localName, props.HtmlVersion);
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
            WriteXmlEscaped(writer, defaultUri, props, applyCharacterMap: false);
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
            bool isUri = props.EscapeUriAttributes && IsUriAttribute(attr.Name);
            if (isUri)
                value = EscapeUriAttribute(value);
            // Character maps do not apply to URI-valued attributes when URI escaping is enabled.
            WriteXmlEscaped(writer, value, props, applyCharacterMap: !isUri);
            writer.Write('"');
        }

        // Void elements are self-closed even when they appear in no namespace, because
        // the XHTML output method recognizes the standard HTML/XHTML element names.
        bool isKnownVoid = IsHtmlVoidElement(localName) ||
            (props.HtmlVersion == "1.0" && IsHtmlEmptyElement(localName));
        if (isVoid || (isEmpty && isKnownVoid))
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
                    writer.Write(MapCharacters(text.Value, props));
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
                if (child is XCData cdata)
                {
                    WriteCdataText(writer, cdata.Value);
                }
                else if (child is XText text)
                {
                    // Unrepresentable characters are emitted as ordinary text and escaped
                    // as numeric character references by WriteXmlEscaped. Such split-out text
                    // nodes must not be altered by character maps.
                    WriteXmlEscaped(writer, text.Value, props, applyCharacterMap: text.Annotation<CdataSplitAnnotation>() == null);
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

    private static void WriteXmlEscaped(TextWriter writer, string value, Stylesheet.OutputProperties props, bool applyCharacterMap = true)
    {
        var map = applyCharacterMap ? props.CharacterMap : null;
        var form = applyCharacterMap ? TryGetNormalizationForm(props) : null;
        foreach (var (immune, text) in GetCharacterMapSegments(value, map))
        {
            if (immune)
            {
                WriteEncodingOnly(writer, text, props);
                continue;
            }

            var normalized = form != null ? text.Normalize(form.Value) : text;
            foreach (var rune in normalized.EnumerateRunes())
            {
                var cp = rune.Value;
                switch (cp)
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
                        if (props.Version == "1.1" && MustEscapeInXml11(cp, isAttribute: false))
                        {
                            writer.Write("&#");
                            writer.Write(cp);
                            writer.Write(';');
                        }
                        else if (!IsRepresentable(cp, props.Encoding))
                        {
                            writer.Write("&#");
                            writer.Write(cp);
                            writer.Write(';');
                        }
                        else
                        {
                            writer.Write(rune.ToString());
                        }
                        break;
                }
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
                WriteXmlEscaped(writer, uri, props, applyCharacterMap: false);
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
                WriteXmlEscaped(writer, attr.Value, props, applyCharacterMap: false);
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
            WriteXmlEscaped(writer, uri, props, applyCharacterMap: false);
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

    private static readonly ConcurrentDictionary<string, System.Text.Encoding> _exceptionFallbackEncodings = new();

    /// <summary>
    /// Returns an <see cref="System.Text.Encoding"/> that throws on unrepresentable characters,
    /// or <c>null</c> if the encoding name is not supported. Results are cached.
    /// </summary>
    private static System.Text.Encoding? GetEncodingWithExceptionFallback(string encodingName)
    {
        var key = encodingName.Trim().ToUpperInvariant();
        if (_exceptionFallbackEncodings.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var encoding = System.Text.Encoding.GetEncoding(encodingName,
                new System.Text.EncoderExceptionFallback(),
                new System.Text.DecoderExceptionFallback());
            _exceptionFallbackEncodings[key] = encoding;
            return encoding;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether a single character can be represented in the specified encoding.
    /// </summary>
    private static bool CanEncode(char ch, System.Text.Encoding encoding)
    {
        try
        {
            _ = encoding.GetBytes(new[] { ch });
            return true;
        }
        catch (System.Text.EncoderFallbackException)
        {
            return false;
        }
    }

    private static bool CanEncode(int codepoint, System.Text.Encoding encoding)
    {
        try
        {
            _ = encoding.GetBytes(char.ConvertFromUtf32(codepoint));
            return true;
        }
        catch (System.Text.EncoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether a character can be represented in the output encoding.
    /// Unknown encodings are treated as representable so validation errors take precedence.
    /// </summary>
    private static bool IsRepresentable(char ch, string encodingName)
    {
        var encoding = GetEncodingWithExceptionFallback(encodingName);
        if (encoding == null)
            return true;
        return CanEncode(ch, encoding);
    }

    private static bool IsRepresentable(int codepoint, string encodingName)
    {
        var encoding = GetEncodingWithExceptionFallback(encodingName);
        if (encoding == null)
            return true;
        return CanEncode(codepoint, encoding);
    }

    /// <summary>
    /// Formats a DOCTYPE declaration for the supplied root element and output properties,
    /// or returns an empty string when no DOCTYPE is required.
    /// </summary>
    private static string FormatDoctype(XElement? rootElement, Stylesheet.OutputProperties props)
    {
        var rootName = rootElement?.Name.LocalName;
        if (string.IsNullOrEmpty(rootName))
            return string.Empty;

        bool isHtmlRoot = rootElement != null && IsHtmlRootElement(rootElement.Name);
        bool isXhtml5 = props.Method == "xhtml" && props.HtmlVersion == "5.0";
        bool isHtml5 = props.Method == "html" && props.HtmlVersion == "5.0";

        if (!string.IsNullOrEmpty(props.DoctypePublic))
        {
            // XML and XHTML ignore a public identifier when no system identifier is supplied.
            if (props.Method is "xml" or "xhtml" && string.IsNullOrEmpty(props.DoctypeSystem))
            {
                if (isHtmlRoot)
                    return $"<!DOCTYPE {rootName}>";
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE ").Append(rootName).Append(" PUBLIC ").Append(LiteralDoctypeValue(props.DoctypePublic));
            if (!string.IsNullOrEmpty(props.DoctypeSystem))
                sb.Append(' ').Append(LiteralDoctypeValue(props.DoctypeSystem));
            sb.Append(">");
            return sb.ToString();
        }

        if (!string.IsNullOrEmpty(props.DoctypeSystem))
            return $"<!DOCTYPE {rootName} SYSTEM {LiteralDoctypeValue(props.DoctypeSystem)}>";

        if ((isHtml5 || isXhtml5) && isHtmlRoot)
            return $"<!DOCTYPE {rootName}>";

        return string.Empty;
    }

    /// <summary>
    /// Returns a DOCTYPE literal value quoted with single or double quotes,
    /// choosing the delimiter that does not occur in the value.
    /// </summary>
    private static string LiteralDoctypeValue(string value)
    {
        if (value.Contains('"'))
            return $"'{value}'";
        return $"\"{value}\"";
    }

    private static void WriteDoctype(TextWriter writer, XElement? rootElement, Stylesheet.OutputProperties props)
    {
        var doctype = FormatDoctype(rootElement, props);
        if (!string.IsNullOrEmpty(doctype))
            writer.Write(doctype);
    }

    private static void WriteDoctype(XmlWriter writer, XElement? rootElement, Stylesheet.OutputProperties props)
    {
        var doctype = FormatDoctype(rootElement, props);
        if (!string.IsNullOrEmpty(doctype))
            writer.WriteRaw(doctype);
    }

    private static XElement InsertContentTypeMeta(XElement rootElement, Stylesheet.OutputProperties props)
    {
        // Only insert for html/head root element in the XHTML/HTML namespace.
        const string xhtmlNs = "http://www.w3.org/1999/xhtml";
        var localName = rootElement.Name.LocalName;
        if (!string.Equals(localName, "html", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(localName, "head", StringComparison.OrdinalIgnoreCase))
            return rootElement;
        if (rootElement.Name.NamespaceName != xhtmlNs && !string.IsNullOrEmpty(rootElement.Name.NamespaceName))
            return rootElement;

        XElement? head;
        XElement? html = null;
        if (string.Equals(localName, "head", StringComparison.OrdinalIgnoreCase))
        {
            head = rootElement;
        }
        else
        {
            html = rootElement;
            head = rootElement.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "head", StringComparison.OrdinalIgnoreCase));
            if (head == null)
                return rootElement;
        }

        // Check if a meta http-equiv Content-Type already exists. HTML is case-insensitive,
        // so both the attribute name and value are compared ignoring case.
        XElement? existingMeta = null;
        foreach (var meta in head.Elements().Where(e => string.Equals(e.Name.LocalName, "meta", StringComparison.OrdinalIgnoreCase)))
        {
            var httpEquivAttr = meta.Attributes().FirstOrDefault(a => string.Equals(a.Name.LocalName, "http-equiv", StringComparison.OrdinalIgnoreCase));
            if (httpEquivAttr != null && string.Equals(httpEquivAttr.Value, "Content-Type", StringComparison.OrdinalIgnoreCase))
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
            newMeta = new XElement(XName.Get("meta", head.Name.NamespaceName),
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

        if (html == null)
            return newHead;

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

    /// <summary>
    /// Returns whether an element in the XHTML namespace should be serialized using the
    /// empty-element syntax. XHTML 1.0 follows the HTML 4 empty-element list; XHTML 5.0
    /// follows the HTML5 void-element list.
    /// </summary>
    private static bool IsXhtmlVoidElement(string localName, string htmlVersion)
    {
        return htmlVersion == "1.0"
            ? IsHtmlEmptyElement(localName)
            : IsHtmlVoidElement(localName);
    }

    private static bool IsHtmlRawContentElement(string localName)
    {
        return localName.ToLowerInvariant() is "script" or "style" or "textarea" or "title";
    }

    private static bool IsHtmlRootElement(XName name)
    {
        return string.Equals(name.LocalName, "html", StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(name.NamespaceName)
                || name.NamespaceName == "http://www.w3.org/1999/xhtml");
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
            ConformanceLevel = ConformanceLevel.Document,
            NewLineHandling = NewLineHandling.None
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
            node = NormalizeCommentsAndPis(node, normForm);
        node = WrapCdataSections(node, props);

        var sb = new System.Text.StringBuilder();
        using var writer = new StringWriter(sb);

        WriteByteOrderMark(writer, props);

        if (!props.OmitXmlDeclaration && props.Version is "1.0" or "1.1")
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
        return NormalizeSurrogatePairEntities(sb.ToString());
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
                WriteEscaped(writer, text.Value, isAttribute: false, props, applyCharacterMap: text.Annotation<CdataSplitAnnotation>() == null);
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
            WriteEscaped(writer, defaultUri, isAttribute: true, props, applyCharacterMap: false);
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
                WriteEscaped(writer, uri, isAttribute: true, props, applyCharacterMap: false);
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
                WriteEscaped(writer, attr.Value, isAttribute: true, props, applyCharacterMap: false);
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
            WriteEscaped(writer, uri, isAttribute: true, props, applyCharacterMap: false);
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

        // Prefer the prefix chosen by the XSLT processor for this element's own
        // namespace URI. This preserves sibling prefixes that map to the same URI
        // (e.g. one:h3 and my:h3 both bound to the same namespace).
        var preferredPrefix = element.Annotation<ElementPrefixHint>()?.Prefix;
        if (!string.IsNullOrEmpty(preferredPrefix) && uri == element.Name.NamespaceName)
        {
            if (inScopeBindings.TryGetValue(preferredPrefix, out var scopeUri) && scopeUri == uri)
                return preferredPrefix;
            if (!inScopeBindings.ContainsKey(preferredPrefix) && !declarations.ContainsKey(preferredPrefix))
            {
                declarations[preferredPrefix] = uri;
                return preferredPrefix;
            }
        }

        // Prefer a non-empty prefix that is already targeted for this element.
        foreach (var (prefix, boundUri) in targetBindings)
        {
            if (boundUri == uri && !string.IsNullOrEmpty(prefix))
                return prefix;
        }

        // Prefer a prefix explicitly declared on this element, even if another
        // prefix for the same URI is already in scope. This preserves the
        // original prefixes chosen by the stylesheet for sibling elements that
        // share a namespace URI but use different prefixes.
        foreach (var attr in element.Attributes().Where(a => a.IsNamespaceDeclaration))
        {
            if (attr.Value == uri)
            {
                var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (inScopeBindings.TryGetValue(prefix, out var scopeUri) && scopeUri == uri)
                    return prefix;
                if (!inScopeBindings.ContainsKey(prefix) && !declarations.ContainsKey(prefix))
                {
                    declarations[prefix] = uri;
                    return prefix;
                }
            }
        }

        // Prefer a non-empty prefix already in scope.
        var scopePrefix = GetPrefixForUri(inScopeBindings, uri);
        if (scopePrefix != null)
            return scopePrefix;

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
                foreach (var child in element.Nodes())
                    cloned.Add(NormalizeForXmlWriter(child, props));
                foreach (var annotation in element.Annotations<object>())
                    cloned.AddAnnotation(annotation);
                if (!allowUndeclare)
                    cloned.RemoveAnnotations<PrefixedNamespaceUndeclarations>();
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

    private static void WriteEscaped(TextWriter writer, string value, bool isAttribute, Stylesheet.OutputProperties props, bool applyCharacterMap = true)
    {
        var map = applyCharacterMap ? props.CharacterMap : null;
        var form = applyCharacterMap ? TryGetNormalizationForm(props) : null;
        foreach (var (immune, text) in GetCharacterMapSegments(value, map))
        {
            if (immune)
            {
                // Replacement strings are written as-is, with only encoding fallback.
                WriteEncodingOnly(writer, text, props);
                continue;
            }

            var normalized = form != null ? text.Normalize(form.Value) : text;
            foreach (var rune in normalized.EnumerateRunes())
            {
                var cp = rune.Value;
                switch (cp)
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
                        if (props.Version == "1.1" && MustEscapeInXml11(cp, isAttribute))
                        {
                            writer.Write("&#");
                            writer.Write(cp);
                            writer.Write(';');
                        }
                        else if (!IsRepresentable(cp, props.Encoding))
                        {
                            writer.Write("&#");
                            writer.Write(cp);
                            writer.Write(';');
                        }
                        else
                        {
                            writer.Write(rune.ToString());
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Writes a replacement string emitted by a character map, translating any characters
    /// that cannot be represented in the target encoding to numeric character references
    /// without applying XML/HTML escaping or further character mapping.
    /// </summary>
    private static void WriteEncodingOnly(TextWriter writer, string value, Stylesheet.OutputProperties props)
    {
        foreach (var rune in value.EnumerateRunes())
        {
            if (!IsRepresentable(rune.Value, props.Encoding))
            {
                writer.Write("&#");
                writer.Write(rune.Value);
                writer.Write(';');
            }
            else
            {
                writer.Write(rune.ToString());
            }
        }
    }

    private static bool MustEscapeInXml11(char ch, bool isAttribute)
    {
        return MustEscapeInXml11((int)ch, isAttribute);
    }

    private static bool MustEscapeInXml11(int cp, bool isAttribute)
    {
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

    /// <summary>
    /// Counts the number of element children at the top level of the result value.
    /// </summary>
    private static int GetRootElementCount(XdmValue value)
    {
        if (value.IsNode && value.NodeValue is XDocumentNode xdn)
        {
            if (xdn.UnderlyingObject is XDocument doc)
                return doc.Elements().Count();
            if (xdn.UnderlyingObject is XElement elem)
            {
                if (elem.Name.LocalName == "__xdm_doc__" && elem.Name.NamespaceName == "")
                    return elem.Elements().Count();
                return 1;
            }
        }

        if (value.IsSequence && value.SequenceValue != null)
        {
            int count = 0;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsNode && item.NodeValue is XDocumentNode itemXdn)
                {
                    if (itemXdn.UnderlyingObject is XDocument itemDoc)
                        count += itemDoc.Elements().Count();
                    else if (itemXdn.UnderlyingObject is XElement itemElem)
                    {
                        if (itemElem.Name.LocalName == "__xdm_doc__" && itemElem.Name.NamespaceName == "")
                            count += itemElem.Elements().Count();
                        else
                            count++;
                    }
                }
            }
            return count;
        }

        return 0;
    }

    /// <summary>
    /// Validates constraints that depend on the shape of the result tree.
    /// Raises SEPM0004 when standalone or a DOCTYPE is requested for a result tree
    /// that contains multiple element children.
    /// </summary>
    private static void ValidateResultTree(XdmValue value, Stylesheet.OutputProperties props)
    {
        var rootElementCount = GetRootElementCount(value);
        if (rootElementCount <= 1)
            return;

        if (props.Standalone is "yes" or "no")
        {
            throw new XsltRuntimeException("SEPM0004",
                "A standalone pseudo-attribute is not allowed when the result tree contains multiple element children.",
                XdmValue.Undefined);
        }

        if (!string.IsNullOrEmpty(props.DoctypeSystem) || !string.IsNullOrEmpty(props.DoctypePublic))
        {
            throw new XsltRuntimeException("SEPM0004",
                "A DOCTYPE declaration is not allowed when the result tree contains multiple element children.",
                XdmValue.Undefined);
        }
    }
}
