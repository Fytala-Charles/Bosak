// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 July 2026
// PURPOSE              : Full XSLT/XQuery 3.1 serialization engine backing fn:serialize (xml/xhtml/html/text/json/adaptive).
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-07-2026     | Creation (QT3 Tier-2h: fn-serialize pool)                                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Text;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// Implements the XSLT and XQuery Serialization 3.1 specification for <c>fn:serialize</c>.
/// Supports the xml, xhtml, html, text, json and adaptive output methods; serialization
/// parameters may be supplied as a map (with option parameter conventions applied) or as an
/// <c>output:serialization-parameters</c> element.
/// </summary>
internal static class XdmSerializer
{
    /// <summary>Namespace of the serialization-parameters element form.</summary>
    private const string OutputNs = "http://www.w3.org/2010/xslt-xquery-serialization";

    /// <summary>Namespace URI carried by xmlns declaration attributes.</summary>
    private const string XmlnsNs = "http://www.w3.org/2000/xmlns/";

    /// <summary>HTML void elements (serialized without an end tag by the html method).</summary>
    private static readonly HashSet<string> HtmlVoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input",
        "link", "meta", "param", "source", "track", "wbr"
    };

    // =====================================================================================
    // Serialization parameters
    // =====================================================================================

    /// <summary>Effective serialization parameters (Serialization 3.1 §3).</summary>
    internal sealed class SerializationParameters
    {
        public string Method = "xml";                     // xml | xhtml | html | text | json | adaptive
        public string Version = "1.0";
        public string? Encoding;
        public bool Indent;
        public bool OmitXmlDeclaration = true;
        public bool? Standalone;                          // null = omit
        public string? ItemSeparator;                     // null = default (" " for xml family)
        public Dictionary<string, string>? CharacterMaps; // single-char key → replacement
        public HashSet<(string Ns, string Local)>? CdataSectionElements;
        public HashSet<(string Ns, string Local)>? SuppressIndentation;
        public decimal? HtmlVersion;
        public string JsonNodeOutputMethod = "xml";
        public bool AllowDuplicateNames;
        public string? MediaType;
        public string? DoctypeSystem;
        public string? DoctypePublic;
        public string? NormalizationForm;

        /// <summary>The separator used between top-level items.</summary>
        public string Separator => ItemSeparator ?? (Method == "text" ? string.Empty : " ");
    }

    // =====================================================================================
    // Entry points
    // =====================================================================================

    /// <summary>Serializes <paramref name="input"/> using default parameters.</summary>
    public static string Serialize(XdmValue input) => Serialize(input, new SerializationParameters());

    /// <summary>
    /// Serializes <paramref name="input"/> with parameters taken from <paramref name="optionsArg"/>:
    /// a map, an <c>output:serialization-parameters</c> element, or the empty sequence (defaults).
    /// </summary>
    public static string Serialize(XdmValue input, XdmValue optionsArg)
    {
        SerializationParameters parameters;
        if (optionsArg.IsMap)
        {
            parameters = ParseMapOptions(optionsArg.MapValue);
        }
        else if (optionsArg.IsNode)
        {
            parameters = ParseElementOptions(UnwrapDocument(optionsArg.NodeValue));
        }
        else if (optionsArg.IsUndefined)
        {
            parameters = new SerializationParameters();
        }
        else if (optionsArg.IsSequence && optionsArg.SequenceValue is not null)
        {
            var items = ToItemList(optionsArg);
            if (items.Count == 0)
                parameters = new SerializationParameters();
            else if (items.Count == 1 && items[0].IsNode)
                parameters = ParseElementOptions(UnwrapDocument(items[0].NodeValue));
            else
                throw new InvalidOperationException(
                    $"XPTY0004: fn:serialize options must be a map, a serialization-parameters element, or the empty sequence; got {optionsArg.Kind}.");
        }
        else
        {
            throw new InvalidOperationException(
                $"XPTY0004: fn:serialize options must be a map, a serialization-parameters element, or the empty sequence; got {optionsArg.Kind}.");
        }
        return Serialize(input, parameters);
    }

    /// <summary>Serializes <paramref name="input"/> with explicit parameters.</summary>
    public static string Serialize(XdmValue input, SerializationParameters parameters)
    {
        var items = ToItemList(input);
        return parameters.Method switch
        {
            "json" => SerializeJsonMethod(items, parameters),
            "adaptive" => SerializeAdaptiveMethod(items, parameters),
            _ => SerializeXmlFamily(items, parameters),
        };
    }

    private static List<XdmValue> ToItemList(XdmValue value)
    {
        var list = new List<XdmValue>();
        if (value.IsUndefined)
            return list;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                list.Add(item);
            return list;
        }
        list.Add(value);
        return list;
    }

    // =====================================================================================
    // Option parsing — map form (option parameter conventions)
    // =====================================================================================

    private static SerializationParameters ParseMapOptions(XdmMap map)
    {
        var p = new SerializationParameters();
        foreach (var kvp in map.Entries)
        {
            string name;
            if (kvp.Key.Kind == XdmValueKind.String)
            {
                name = kvp.Key.StringValue;
            }
            else if (kvp.Key.Kind == XdmValueKind.QName)
            {
                // xs:QName keys identify implementation-defined serialization parameters;
                // unrecognized ones are ignored (this includes absent-namespace QNames).
                continue;
            }
            else
            {
                throw new InvalidOperationException(
                    $"XPTY0004: fn:serialize option keys must be xs:string or xs:QName, got {kvp.Key.Kind}.");
            }
            ApplyParameter(p, name, kvp.Value);
        }
        return p;
    }

    private static void ApplyParameter(SerializationParameters p, string name, XdmValue rawValue)
    {
        // Option parameter conventions: an empty sequence leaves the parameter at its default.
        if (rawValue.IsUndefined || (rawValue.IsSequence && ToItemList(rawValue).Count == 0))
            return;

        switch (name)
        {
            case "method":
                p.Method = AsMethodName(name, rawValue);
                break;
            case "indent":
                p.Indent = AsBoolean(name, rawValue);
                break;
            case "omit-xml-declaration":
                p.OmitXmlDeclaration = AsBoolean(name, rawValue);
                break;
            case "standalone":
                p.Standalone = AsStandalone(rawValue);
                break;
            case "item-separator":
                p.ItemSeparator = AsString(name, rawValue);
                break;
            case "encoding":
                p.Encoding = AsString(name, rawValue);
                break;
            case "version":
                p.Version = AsString(name, rawValue);
                break;
            case "media-type":
                p.MediaType = AsString(name, rawValue);
                break;
            case "doctype-system":
                p.DoctypeSystem = AsString(name, rawValue);
                break;
            case "doctype-public":
                p.DoctypePublic = AsString(name, rawValue);
                break;
            case "normalization-form":
                p.NormalizationForm = AsString(name, rawValue);
                break;
            case "json-node-output-method":
                p.JsonNodeOutputMethod = AsString(name, rawValue).ToLowerInvariant();
                break;
            case "html-version":
                p.HtmlVersion = AsDecimal(name, rawValue);
                break;
            case "allow-duplicate-names":
                p.AllowDuplicateNames = AsBoolean(name, rawValue);
                break;
            case "byte-order-mark":
            case "escape-uri-attributes":
            case "include-content-type":
            case "undeclare-prefixes":
                _ = AsBoolean(name, rawValue); // recognized, no effect
                break;
            case "use-character-maps":
                p.CharacterMaps = AsCharacterMap(rawValue);
                break;
            case "cdata-section-elements":
                p.CdataSectionElements = AsQNameSet(name, rawValue);
                break;
            case "suppress-indentation":
                p.SuppressIndentation = AsQNameSet(name, rawValue);
                break;
            default:
                // Unrecognized string keys are ignored (option parameter conventions).
                break;
        }
    }

    /// <summary>Atomizes a single node option value; sequences are returned unchanged.</summary>
    private static XdmValue AtomizeOptionValue(XdmValue value)
    {
        if (value.IsNode)
            return value.NodeValue.TypedValue;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = ToItemList(value);
            if (items.Count == 1 && items[0].IsNode)
                return items[0].NodeValue.TypedValue;
        }
        return value;
    }

    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String
           && string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    private static bool AsBoolean(string name, XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.Boolean)
            return value.BooleanValue;
        if (IsUntypedAtomic(value))
        {
            return value.StringValue.Trim() switch
            {
                "true" or "1" => true,
                "false" or "0" => false,
                _ => throw new InvalidOperationException(
                    $"XPTY0004: Cannot convert xs:untypedAtomic('{value.StringValue}') to xs:boolean for serialization parameter '{name}'.")
            };
        }
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter '{name}' requires xs:boolean, got {Describe(value)}.");
    }

    private static bool? AsStandalone(XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.Boolean)
            return value.BooleanValue;
        if (IsUntypedAtomic(value))
        {
            return value.StringValue.Trim() switch
            {
                "true" or "1" => true,
                "false" or "0" => false,
                "omit" => null,
                _ => throw new InvalidOperationException(
                    $"XPTY0004: Cannot convert xs:untypedAtomic('{value.StringValue}') for serialization parameter 'standalone'.")
            };
        }
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter 'standalone' requires xs:boolean, got {Describe(value)}.");
    }

    private static string AsString(string name, XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.String)
            return value.StringValue;
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter '{name}' requires xs:string, got {Describe(value)}.");
    }

    private static string AsMethodName(string name, XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.String)
            return value.StringValue.ToLowerInvariant();
        if (value.Kind == XdmValueKind.QName)
            return value.QNameValue.LocalName.ToLowerInvariant();
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter '{name}' requires xs:string, got {Describe(value)}.");
    }

    private static decimal AsDecimal(string name, XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        switch (value.Kind)
        {
            case XdmValueKind.Decimal:
                return value.DecimalValue;
            case XdmValueKind.Integer:
                return value.IntegerValue;
            case XdmValueKind.Double:
            case XdmValueKind.Float:
                return (decimal)value.DoubleValue;
            default:
                if (IsUntypedAtomic(value)
                    && decimal.TryParse(value.StringValue.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                    return d;
                throw new InvalidOperationException(
                    $"XPTY0004: Serialization parameter '{name}' requires a numeric value, got {Describe(value)}.");
        }
    }

    private static HashSet<(string Ns, string Local)> AsQNameSet(string name, XdmValue rawValue)
    {
        var set = new HashSet<(string, string)>();
        foreach (var item in ToItemList(rawValue))
        {
            // Option parameter conventions: an array is replaced by its members.
            if (item.IsArray)
            {
                var arr = item.ArrayValue;
                for (int i = 1; i <= arr.Count; i++)
                    AddQName(arr.Get(i));
                continue;
            }
            AddQName(item);
        }
        return set;

        void AddQName(XdmValue item)
        {
            var value = AtomizeOptionValue(item);
            if (value.Kind != XdmValueKind.QName)
                throw new InvalidOperationException(
                    $"XPTY0004: Serialization parameter '{name}' requires xs:QName values, got {Describe(value)}.");
            var q = value.QNameValue;
            set.Add((q.NamespaceUri, q.LocalName));
        }
    }

    private static Dictionary<string, string> AsCharacterMap(XdmValue rawValue)
    {
        if (!rawValue.IsMap)
            throw new InvalidOperationException(
                $"XPTY0004: Serialization parameter 'use-character-maps' requires a map, got {Describe(rawValue)}.");
        var result = new Dictionary<string, string>();
        foreach (var kvp in rawValue.MapValue.Entries)
        {
            // Option parameter conventions do not apply recursively: keys and values must
            // be xs:string values exactly as supplied.
            if (kvp.Key.Kind != XdmValueKind.String || IsUntypedAtomic(kvp.Key))
                throw new InvalidOperationException(
                    $"XPTY0004: Character map keys must be xs:string, got {Describe(kvp.Key)}.");
            string key = kvp.Key.StringValue;
            if (key.Length != 1)
                throw new InvalidOperationException(
                    $"SEPM0016: Character map key '{key}' must be a single character.");
            if (kvp.Value.Kind != XdmValueKind.String || IsUntypedAtomic(kvp.Value))
                throw new InvalidOperationException(
                    $"XPTY0004: Character map values must be xs:string, got {Describe(kvp.Value)}.");
            result[key] = kvp.Value.StringValue;
        }
        return result;
    }

    private static string Describe(XdmValue value)
        => value.Kind == XdmValueKind.String && IsUntypedAtomic(value)
            ? $"xs:untypedAtomic('{value.StringValue}')"
            : value.Kind.ToString();

    // =====================================================================================
    // Option parsing — element form (output:serialization-parameters)
    // =====================================================================================

    /// <summary>Unwraps a document node to its first element child, if any.</summary>
    private static IXdmNode UnwrapDocument(IXdmNode node)
    {
        if (node.NodeKind == XdmNodeKind.Document)
        {
            foreach (var childValue in node.Children(XdmNodeKind.Element))
                return childValue.NodeValue;
        }
        return node;
    }

    private static SerializationParameters ParseElementOptions(IXdmNode node)
    {
        if (node.NodeKind != XdmNodeKind.Element
            || node.LocalName != "serialization-parameters"
            || node.NamespaceUri != OutputNs)
        {
            throw new InvalidOperationException(
                "XPTY0004: fn:serialize options element must be output:serialization-parameters.");
        }

        // The root element must not carry attributes other than namespace declarations.
        foreach (var attrValue in node.Attributes())
        {
            var attr = attrValue.NodeValue;
            if (!IsNamespaceDeclaration(attr))
                throw new InvalidOperationException(
                    $"SEPM0017: Attribute '{attr.LocalName}' is not allowed on output:serialization-parameters.");
        }

        var p = new SerializationParameters();
        var seen = new HashSet<(string Ns, string Local)>();
        foreach (var childValue in node.Children(XdmNodeKind.Element))
        {
            var child = childValue.NodeValue;

            // Duplicate parameter elements are an error (including vendor-namespace ones).
            if (!seen.Add((child.NamespaceUri, child.LocalName)))
                throw new InvalidOperationException(
                    $"SEPM0019: Duplicate serialization parameter element '{child.LocalName}'.");

            if (child.NamespaceUri != OutputNs)
            {
                // Parameters in no namespace are disallowed; vendor namespaces are ignored.
                if (child.NamespaceUri.Length == 0)
                    throw new InvalidOperationException(
                        $"SEPM0017: Serialization parameter element '{child.LocalName}' must be in a namespace.");
                continue;
            }

            string local = child.LocalName;
            if (local == "use-character-maps")
            {
                p.CharacterMaps = ParseElementCharacterMap(child);
                continue;
            }

            // Value-style parameters may only carry the 'value' attribute.
            foreach (var attrValue in child.Attributes())
            {
                var attr = attrValue.NodeValue;
                if (!IsNamespaceDeclaration(attr) && attr.LocalName != "value")
                    throw new InvalidOperationException(
                        $"SEPM0017: Attribute '{attr.LocalName}' is not allowed on output:{local}.");
            }

            string? value = GetAttribute(child, "value");
            switch (local)
            {
                case "method": p.Method = (value ?? "xml").Trim().ToLowerInvariant(); break;
                case "indent": p.Indent = AsElementBoolean(value, local); break;
                case "omit-xml-declaration": p.OmitXmlDeclaration = AsElementBoolean(value, local); break;
                case "standalone":
                    p.Standalone = value?.Trim() == "omit" ? null : AsElementBoolean(value, local);
                    break;
                case "item-separator": p.ItemSeparator = value ?? string.Empty; break;
                case "encoding": p.Encoding = value; break;
                case "version": p.Version = value ?? "1.0"; break;
                case "media-type": p.MediaType = value; break;
                case "doctype-system": p.DoctypeSystem = value; break;
                case "doctype-public": p.DoctypePublic = value; break;
                case "normalization-form": p.NormalizationForm = value; break;
                case "json-node-output-method": p.JsonNodeOutputMethod = (value ?? "xml").Trim().ToLowerInvariant(); break;
                case "html-version":
                    if (value is not null
                        && decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var hv))
                        p.HtmlVersion = hv;
                    break;
                case "allow-duplicate-names": p.AllowDuplicateNames = AsElementBoolean(value, local); break;
                case "byte-order-mark":
                case "escape-uri-attributes":
                case "include-content-type":
                case "undeclare-prefixes":
                    _ = AsElementBoolean(value, local);
                    break;
                case "cdata-section-elements":
                    p.CdataSectionElements = ParseElementQNameSet(child, value);
                    break;
                case "suppress-indentation":
                    p.SuppressIndentation = ParseElementQNameSet(child, value);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"SEPM0017: Unknown serialization parameter '{local}' in the serialization namespace.");
            }
        }
        return p;
    }

    private static bool IsNamespaceDeclaration(IXdmNode attr)
        => attr.NamespaceUri == XmlnsNs
           || (attr.LocalName == "xmlns" && attr.NamespaceUri.Length == 0);

    private static bool AsElementBoolean(string? value, string paramName)
        => value?.Trim() switch
        {
            null => false,
            "yes" or "true" or "1" => true,
            "no" or "false" or "0" => false,
            _ => throw new InvalidOperationException(
                $"SEPM0017: Invalid value '{value}' for serialization parameter '{paramName}'.")
        };

    private static string? GetAttribute(IXdmNode element, string localName)
    {
        foreach (var attrValue in element.Attributes(localName))
            return attrValue.NodeValue.StringValue;
        return null;
    }

    private static Dictionary<string, string> ParseElementCharacterMap(IXdmNode useCharacterMaps)
    {
        if (GetAttribute(useCharacterMaps, "value") is not null)
            throw new InvalidOperationException(
                "SEPM0017: use-character-maps must be defined with character-map child elements, not a value attribute.");
        var result = new Dictionary<string, string>();
        foreach (var childValue in useCharacterMaps.Children(XdmNodeKind.Element))
        {
            var child = childValue.NodeValue;
            if (child.NamespaceUri != OutputNs || child.LocalName != "character-map")
                throw new InvalidOperationException(
                    $"SEPM0017: Element '{child.LocalName}' is not allowed inside output:use-character-maps.");
            string? character = null;
            string? mapString = null;
            foreach (var attrValue in child.Attributes())
            {
                var attr = attrValue.NodeValue;
                if (IsNamespaceDeclaration(attr))
                    continue;
                switch (attr.LocalName)
                {
                    case "character": character = attr.StringValue; break;
                    case "map-string": mapString = attr.StringValue; break;
                    default:
                        throw new InvalidOperationException(
                            $"SEPM0017: Attribute '{attr.LocalName}' is not allowed on output:character-map.");
                }
            }
            character ??= string.Empty;
            if (character.Length != 1)
                throw new InvalidOperationException(
                    $"SEPM0016: Character map key '{character}' must be a single character.");
            if (result.ContainsKey(character))
                throw new InvalidOperationException(
                    $"SEPM0018: Duplicate character map entry for '{character}'.");
            result[character] = mapString ?? string.Empty;
        }
        return result;
    }

    private static HashSet<(string Ns, string Local)> ParseElementQNameSet(IXdmNode element, string? value)
    {
        var set = new HashSet<(string, string)>();
        if (string.IsNullOrWhiteSpace(value))
            return set;
        foreach (var lexical in value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            int colon = lexical.IndexOf(':');
            if (colon >= 0)
            {
                string prefix = lexical[..colon];
                string? ns = ResolvePrefix(element, prefix);
                if (ns is not null)
                    set.Add((ns, lexical[(colon + 1)..]));
            }
            else
            {
                set.Add((string.Empty, lexical));
            }
        }
        return set;
    }

    /// <summary>Resolves a namespace prefix using the in-scope bindings of an element and its ancestors.</summary>
    private static string? ResolvePrefix(IXdmNode element, string prefix)
    {
        for (IXdmNode? node = element; node is not null; node = node.Parent)
        {
            foreach (var attrValue in node.Attributes())
            {
                var attr = attrValue.NodeValue;
                if (attr.NamespaceUri == XmlnsNs && attr.LocalName == prefix)
                    return attr.StringValue;
                if (prefix.Length == 0 && attr.LocalName == "xmlns" && attr.NamespaceUri.Length == 0)
                    return attr.StringValue;
            }
        }
        return null;
    }

    // =====================================================================================
    // xml / xhtml / html / text methods
    // =====================================================================================

    private static string SerializeXmlFamily(List<XdmValue> items, SerializationParameters p)
    {
        var writer = new NodeWriter(p);

        // SENR0001: free-standing attribute/namespace nodes and function items cannot be
        // serialized by the xml, xhtml or html methods.
        if (p.Method is "xml" or "xhtml" or "html")
        {
            foreach (var item in items)
            {
                if (item.IsFunction)
                    throw new InvalidOperationException("SENR0001: Cannot serialize a function item.");
                if (item.IsNode && item.NodeValue.NodeKind is XdmNodeKind.Attribute or XdmNodeKind.Namespace)
                    throw new InvalidOperationException(
                        $"SENR0001: Cannot serialize a free-standing {item.NodeValue.NodeKind.ToString().ToLowerInvariant()} node.");
            }
        }

        // XML declaration (only for xml/xhtml when not omitted).
        if (p.Method is "xml" or "xhtml" && !p.OmitXmlDeclaration)
        {
            writer.Raw("<?xml version=\"");
            writer.Raw(p.Version);
            writer.Raw("\" encoding=\"");
            writer.Raw(p.Encoding ?? "UTF-8");
            writer.Raw("\"");
            if (p.Standalone.HasValue)
            {
                writer.Raw(" standalone=\"");
                writer.Raw(p.Standalone.Value ? "yes" : "no");
                writer.Raw("\"");
            }
            writer.Raw("?>");
        }

        // HTML5 doctype: emitted when a top-level html element is serialized.
        if (p.Method == "html" && p.HtmlVersion is >= 5 && FirstTopLevelElementIsHtml(items))
            writer.Raw("<!DOCTYPE HTML>\n");

        string separator = p.Separator;
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
                writer.Raw(separator);
            var item = items[i];
            if (item.IsFunction)
                throw new InvalidOperationException("SENR0001: Cannot serialize a function item.");
            if (item.IsNode)
            {
                var node = item.NodeValue;
                if (node.NodeKind is XdmNodeKind.Attribute or XdmNodeKind.Namespace)
                {
                    // text method: attributes and namespaces contribute their string value.
                    writer.Text(node.StringValue);
                }
                else
                {
                    writer.WriteNode(node, 0, suppressIndent: false);
                }
            }
            else if (item.IsMap || item.IsArray)
            {
                throw new InvalidOperationException("SENR0001: Cannot serialize a map or array with the " + p.Method + " method.");
            }
            else
            {
                writer.Text(item.ToString());
            }
        }
        return writer.ToString();
    }

    private static bool FirstTopLevelElementIsHtml(List<XdmValue> items)
    {
        foreach (var item in items)
        {
            if (!item.IsNode)
                return false;
            var node = item.NodeValue;
            if (node.NodeKind == XdmNodeKind.Document)
            {
                foreach (var childValue in node.Children(XdmNodeKind.Element))
                    return IsHtmlElement(childValue.NodeValue);
                return false;
            }
            if (node.NodeKind == XdmNodeKind.Element)
                return IsHtmlElement(node);
            return false;
        }
        return false;

        static bool IsHtmlElement(IXdmNode node)
            => node.LocalName.Equals("html", StringComparison.OrdinalIgnoreCase) && node.NamespaceUri.Length == 0;
    }

    /// <summary>Recursive node writer implementing the xml/xhtml/html output rules.</summary>
    private sealed class NodeWriter
    {
        private readonly SerializationParameters _p;
        private readonly StringBuilder _sb = new();
        private bool _metaInjected;

        public NodeWriter(SerializationParameters p) => _p = p;

        public void Raw(string s) => _sb.Append(s);

        /// <summary>Writes an atomic value's string form with text-node escaping.</summary>
        public void Text(string s) => WriteTextContent(s);

        public override string ToString() => _sb.ToString();

        public void WriteNode(IXdmNode node, int depth, bool suppressIndent)
        {
            switch (node.NodeKind)
            {
                case XdmNodeKind.Document:
                    foreach (var childValue in node.Children())
                        WriteNode(childValue.NodeValue, depth, suppressIndent: false);
                    break;
                case XdmNodeKind.Element:
                    WriteElement(node, depth, suppressIndent);
                    break;
                case XdmNodeKind.Text:
                    WriteTextContent(node.StringValue);
                    break;
                case XdmNodeKind.Comment:
                    _sb.Append("<!--").Append(node.StringValue).Append("-->");
                    break;
                case XdmNodeKind.ProcessingInstruction:
                    _sb.Append("<?").Append(node.LocalName);
                    string data = node.StringValue;
                    if (data.Length > 0)
                        _sb.Append(' ').Append(data);
                    _sb.Append("?>");
                    break;
                default:
                    WriteTextContent(node.StringValue);
                    break;
            }
        }

        private void WriteElement(IXdmNode element, int depth, bool suppressIndent)
        {
            string name = QualifiedName(element);
            _sb.Append('<').Append(name);
            foreach (var attrValue in element.Attributes())
            {
                var attr = attrValue.NodeValue;
                _sb.Append(' ').Append(AttributeName(attr)).Append("=\"");
                WriteAttributeContent(attr.StringValue);
                _sb.Append('"');
            }

            var children = new List<IXdmNode>();
            foreach (var childValue in element.Children())
                children.Add(childValue.NodeValue);

            bool isHtml = _p.Method == "html";
            bool isVoid = isHtml && HtmlVoidElements.Contains(element.LocalName);

            if (children.Count == 0 && !InjectMeta(element) && !isVoid)
            {
                _sb.Append("/>");
                return;
            }
            _sb.Append('>');

            if (InjectMeta(element))
            {
                _metaInjected = true;
                _sb.Append("<meta charset=\"").Append(_p.Encoding ?? "UTF-8").Append("\"");
                _sb.Append(isHtml ? ">" : "/>");
            }
            if (isVoid)
                return;

            bool cdata = _p.CdataSectionElements is not null
                         && _p.CdataSectionElements.Contains((element.NamespaceUri, element.LocalName));
            bool indentContent = _p.Indent
                                 && !suppressIndent
                                 && children.Count > 0
                                 && ElementOnlyContent(children);

            if (indentContent)
            {
                foreach (var child in children)
                {
                    _sb.Append('\n');
                    for (int i = 0; i <= depth; i++)
                        _sb.Append("   ");
                    WriteNode(child, depth + 1, IsSuppressed(child));
                }
                _sb.Append('\n');
                for (int i = 0; i < depth; i++)
                    _sb.Append("   ");
            }
            else if (cdata)
            {
                WriteCdataChildren(children, depth);
            }
            else
            {
                foreach (var child in children)
                    WriteNode(child, depth + 1, IsSuppressed(child));
            }

            _sb.Append("</").Append(name).Append('>');
        }

        /// <summary>Whether a meta charset element must be injected into this (head) element.</summary>
        private bool InjectMeta(IXdmNode element)
            => _p.Method == "html"
               && _p.HtmlVersion is >= 5
               && !_metaInjected
               && element.LocalName.Equals("head", StringComparison.OrdinalIgnoreCase)
               && element.NamespaceUri.Length == 0;

        private bool IsSuppressed(IXdmNode node)
            => node.NodeKind == XdmNodeKind.Element
               && _p.SuppressIndentation is not null
               && _p.SuppressIndentation.Contains((node.NamespaceUri, node.LocalName));

        private static bool ElementOnlyContent(List<IXdmNode> children)
        {
            foreach (var child in children)
            {
                if (child.NodeKind == XdmNodeKind.Text)
                    return false;
            }
            return true;
        }

        private void WriteCdataChildren(List<IXdmNode> children, int depth)
        {
            var textRun = new StringBuilder();
            foreach (var child in children)
            {
                if (child.NodeKind == XdmNodeKind.Text)
                {
                    textRun.Append(child.StringValue);
                    continue;
                }
                FlushCdata(textRun);
                WriteNode(child, depth + 1, IsSuppressed(child));
            }
            FlushCdata(textRun);
        }

        private void FlushCdata(StringBuilder textRun)
        {
            if (textRun.Length == 0)
                return;
            string text = textRun.ToString();
            textRun.Clear();
            // Split embedded "]]>" sequences across two CDATA sections.
            _sb.Append("<![CDATA[").Append(text.Replace("]]>", "]]]]><![CDATA[>")).Append("]]>");
        }

        private void WriteTextContent(string s)
        {
            foreach (char c in s)
            {
                if (_p.CharacterMaps is not null && _p.CharacterMaps.TryGetValue(c.ToString(), out var replacement))
                {
                    _sb.Append(replacement);
                    continue;
                }
                switch (c)
                {
                    case '&': _sb.Append("&amp;"); break;
                    case '<': _sb.Append("&lt;"); break;
                    case '>': _sb.Append("&gt;"); break;
                    case '\r': _sb.Append("&#xD;"); break;
                    default: _sb.Append(c); break;
                }
            }
        }

        private void WriteAttributeContent(string s)
        {
            foreach (char c in s)
            {
                if (_p.CharacterMaps is not null && _p.CharacterMaps.TryGetValue(c.ToString(), out var replacement))
                {
                    _sb.Append(replacement);
                    continue;
                }
                switch (c)
                {
                    case '&': _sb.Append("&amp;"); break;
                    case '<': _sb.Append("&lt;"); break;
                    case '"': _sb.Append("&quot;"); break;
                    case '\t': _sb.Append("&#x9;"); break;
                    case '\n': _sb.Append("&#xA;"); break;
                    case '\r': _sb.Append("&#xD;"); break;
                    default: _sb.Append(c); break;
                }
            }
        }

        private static string QualifiedName(IXdmNode node)
            => node.Prefix.Length > 0 ? node.Prefix + ":" + node.LocalName : node.LocalName;

        private static string AttributeName(IXdmNode attr)
        {
            if (attr.NamespaceUri == XmlnsNs)
                return "xmlns:" + attr.LocalName;
            if (attr.LocalName == "xmlns" && attr.NamespaceUri.Length == 0)
                return "xmlns";
            return QualifiedName(attr);
        }
    }

    // =====================================================================================
    // JSON method
    // =====================================================================================

    private static string SerializeJsonMethod(List<XdmValue> items, SerializationParameters p)
    {
        if (items.Count == 0)
            return "null";
        if (items.Count > 1)
            throw new InvalidOperationException(
                "SERE0023: Cannot serialize a sequence of more than one item with the JSON output method.");
        var sb = new StringBuilder();
        WriteJsonValue(items[0], sb, p);
        return sb.ToString();
    }

    private static void WriteJsonValue(XdmValue value, StringBuilder sb, SerializationParameters p)
    {
        if (value.IsUndefined)
        {
            sb.Append("null");
            return;
        }

        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = ToItemList(value);
            if (items.Count == 0)
            {
                sb.Append("null");
                return;
            }
            if (items.Count > 1)
                throw new InvalidOperationException(
                    "SERE0023: Cannot serialize a sequence of more than one item with the JSON output method.");
            WriteJsonValue(items[0], sb, p);
            return;
        }

        if (value.IsMap)
        {
            sb.Append('{');
            bool first = true;
            var seenKeys = new HashSet<string>();
            foreach (var kvp in value.MapValue.Entries)
            {
                string key = JsonMapKey(kvp.Key);
                if (!p.AllowDuplicateNames && !seenKeys.Add(key))
                    throw new InvalidOperationException(
                        $"SERE0022: Duplicate key '{key}' in map serialized with the JSON output method.");
                if (!first)
                    sb.Append(',');
                sb.Append(EncodeJsonString(key, p));
                sb.Append(':');
                WriteJsonValue(kvp.Value, sb, p);
                first = false;
            }
            sb.Append('}');
            return;
        }

        if (value.IsArray)
        {
            var arr = value.ArrayValue;
            sb.Append('[');
            for (int i = 1; i <= arr.Count; i++)
            {
                if (i > 1)
                    sb.Append(',');
                WriteJsonValue(arr.Get(i), sb, p);
            }
            sb.Append(']');
            return;
        }

        if (value.Kind == XdmValueKind.Boolean)
        {
            sb.Append(value.BooleanValue ? "true" : "false");
            return;
        }

        if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal)
        {
            sb.Append(value.ToString());
            return;
        }

        if (value.Kind is XdmValueKind.Double or XdmValueKind.Float)
        {
            double d = value.DoubleValue;
            if (double.IsNaN(d) || double.IsInfinity(d))
                throw new InvalidOperationException(
                    "SERE0020: Cannot serialize NaN, INF or -INF with the JSON output method.");
            sb.Append(value.ToString());
            return;
        }

        if (value.Kind == XdmValueKind.String)
        {
            sb.Append(EncodeJsonString(value.StringValue, p));
            return;
        }

        if (value.IsNode)
        {
            sb.Append(EncodeJsonString(JsonNodeToString(value.NodeValue, p), p));
            return;
        }

        if (value.IsFunction)
            throw new InvalidOperationException("SENR0001: Cannot serialize a function item.");

        // anyURI, QName, dates, durations, binary, ...: string representation.
        sb.Append(EncodeJsonString(value.ToString(), p));
    }

    private static string JsonMapKey(XdmValue key)
    {
        if (key.Kind == XdmValueKind.String)
            return key.StringValue;
        if (key.Kind == XdmValueKind.QName)
        {
            var q = key.QNameValue;
            if (q.NamespaceUri.Length == 0)
                return q.LocalName;
            return q.Prefix.Length > 0 ? q.Prefix + ":" + q.LocalName : q.ToString();
        }
        if (key.Kind == XdmValueKind.Boolean)
            return key.BooleanValue ? "true" : "false";
        return key.ToString();
    }

    private static string JsonNodeToString(IXdmNode node, SerializationParameters p)
    {
        // Nodes are serialized with the method named by json-node-output-method (default xml).
        var nodeParams = new SerializationParameters
        {
            Method = p.JsonNodeOutputMethod,
            OmitXmlDeclaration = true,
            Indent = false,
            CharacterMaps = p.CharacterMaps,
            CdataSectionElements = p.CdataSectionElements,
        };
        var writer = new NodeWriter(nodeParams);
        if (node.NodeKind is XdmNodeKind.Attribute or XdmNodeKind.Namespace)
            return node.StringValue;
        writer.WriteNode(node, 0, suppressIndent: false);
        return writer.ToString();
    }

    private static string EncodeJsonString(string value, SerializationParameters p)
    {
        bool escapeNonAscii = p.Encoding is not null
                              && !p.Encoding.StartsWith("utf", StringComparison.OrdinalIgnoreCase);
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (char c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '/': sb.Append("\\/"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20 || (escapeNonAscii && c > 0x7E))
                        sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                    else
                        sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    // =====================================================================================
    // Adaptive method
    // =====================================================================================

    private static string SerializeAdaptiveMethod(List<XdmValue> items, SerializationParameters p)
    {
        var sb = new StringBuilder();
        string separator = p.ItemSeparator ?? " ";
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
                sb.Append(separator);
            WriteAdaptiveValue(items[i], sb, p);
        }
        return sb.ToString();
    }

    private static void WriteAdaptiveValue(XdmValue value, StringBuilder sb, SerializationParameters p)
    {
        if (value.IsUndefined)
        {
            sb.Append("()");
            return;
        }

        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = ToItemList(value);
            sb.Append('(');
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');
                WriteAdaptiveValue(items[i], sb, p);
            }
            sb.Append(')');
            return;
        }

        switch (value.Kind)
        {
            case XdmValueKind.String:
                sb.Append('"').Append(value.StringValue.Replace("\"", "\"\"")).Append('"');
                return;
            case XdmValueKind.Boolean:
                sb.Append(value.BooleanValue ? "true()" : "false()");
                return;
            case XdmValueKind.Integer:
            case XdmValueKind.Decimal:
            case XdmValueKind.Double:
            case XdmValueKind.Float:
                sb.Append(value.ToString());
                return;
        }

        if (value.IsNode)
        {
            var node = value.NodeValue;
            if (node.NodeKind == XdmNodeKind.Attribute)
            {
                sb.Append(node.Prefix.Length > 0 ? node.Prefix + ":" + node.LocalName : node.LocalName);
                sb.Append("=\"");
                sb.Append(node.StringValue.Replace("&", "&amp;").Replace("<", "&lt;").Replace("\"", "&quot;"));
                sb.Append('"');
                return;
            }
            if (node.NodeKind == XdmNodeKind.Namespace)
            {
                sb.Append(node.LocalName).Append("=\"").Append(node.StringValue).Append('"');
                return;
            }
            var nodeParams = new SerializationParameters
            {
                Method = "xml",
                OmitXmlDeclaration = true,
                Indent = p.Indent,
            };
            var writer = new NodeWriter(nodeParams);
            writer.WriteNode(node, 0, suppressIndent: false);
            sb.Append(writer.ToString());
            return;
        }

        if (value.IsMap)
        {
            sb.Append("map{");
            bool first = true;
            foreach (var kvp in value.MapValue.Entries)
            {
                if (!first)
                    sb.Append(',');
                WriteAdaptiveValue(kvp.Key, sb, p);
                sb.Append(':');
                WriteAdaptiveValue(kvp.Value, sb, p);
                first = false;
            }
            sb.Append('}');
            return;
        }

        if (value.IsArray)
        {
            var arr = value.ArrayValue;
            sb.Append('[');
            for (int i = 1; i <= arr.Count; i++)
            {
                if (i > 1)
                    sb.Append(',');
                WriteAdaptiveValue(arr.Get(i), sb, p);
            }
            sb.Append(']');
            return;
        }

        if (value.IsFunction)
            throw new InvalidOperationException("SENR0001: Cannot serialize a function item.");

        // Remaining atomic types (anyURI, QName, dates, durations, binary): quoted string.
        sb.Append('"').Append(value.ToString().Replace("\"", "\"\"")).Append('"');
    }
}
