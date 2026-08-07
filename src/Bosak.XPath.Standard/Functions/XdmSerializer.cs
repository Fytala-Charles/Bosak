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
//                      | Charles Korthout | 0.2   | 25-07-2026     | Serialization 3.1 fidelity: declaration/doctype matrix, html-family rules, adaptive constructor forms, JSON char-maps, CDATA/indent/namespace fixup, undeclare-prefixes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 28-07-2026     | Omit redundant xmlns declarations; guard xmlns="" double-write when self-declared |
//                      | Charles Korthout | 0.4   | 01-08-2026     | omit-xml-declaration defaults to true (Serialization 3.1 §3; copy-5101)          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 01-08-2026     | HTML matrix: version-dependent void lists, boolean attrs, script raw text, CDATA islands, xhtml prefix normalization |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 07-08-2026     | XML/XHTML: escape NEL (#x85), LS (#x2028) and C1 controls (#x7F-#x9F) as character references in text and attribute content (K2-Serialization-5/6/9/10) |
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

    /// <summary>HTML5 void elements (serialized without an end tag by the html method).</summary>
    private static readonly HashSet<string> HtmlVoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen",
        "link", "meta", "param", "source", "track", "wbr"
    };

    /// <summary>HTML 4.0 void elements: unlike the HTML5 list, this includes frame and
    /// isindex but not keygen, basefont, bgsound, source, track or wbr.</summary>
    private static readonly HashSet<string> Html40VoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "frame", "hr",
        "img", "input", "isindex", "link", "meta", "param"
    };

    /// <summary>HTML boolean attributes, written in minimized form (name only) when the
    /// attribute value equals the attribute name ignoring case (Serialization 3.1 §6.1.7).</summary>
    private static readonly HashSet<string> HtmlBooleanAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "checked", "compact", "declare", "defer", "disabled", "ismap",
        "multiple", "nohref", "noresize", "noshade", "nowrap", "readonly", "selected"
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
        public bool OmitXmlDeclaration = true;            // Serialization 3.1 §3: default "yes"
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
        public bool IncludeContentType = true;
        public bool EscapeUriAttributes = true;
        public bool? UndeclarePrefixes;   // null: default (yes for XML 1.1)

        /// <summary>The separator used between top-level items.</summary>
        public string Separator => ItemSeparator ?? (Method == "adaptive" ? "\n" : " ");
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
        => Serialize(input, optionsArg, null);

    /// <summary>
    /// Serializes <paramref name="input"/> with parameters taken from <paramref name="optionsArg"/>
    /// applied over <paramref name="baseParams"/> (the static output declarations, when any):
    /// explicitly supplied parameters take precedence.
    /// </summary>
    public static string Serialize(XdmValue input, XdmValue optionsArg, SerializationParameters? baseParams)
    {
        SerializationParameters parameters;
        if (optionsArg.IsMap)
        {
            parameters = ParseMapOptions(optionsArg.MapValue, baseParams);
        }
        else if (optionsArg.IsNode)
        {
            parameters = ParseElementOptions(UnwrapDocument(optionsArg.NodeValue), baseParams);
        }
        else if (optionsArg.IsUndefined)
        {
            parameters = baseParams ?? new SerializationParameters();
        }
        else if (optionsArg.IsSequence && optionsArg.SequenceValue is not null)
        {
            var items = ToItemList(optionsArg);
            if (items.Count == 0)
                parameters = baseParams ?? new SerializationParameters();
            else if (items.Count == 1 && items[0].IsNode)
                parameters = ParseElementOptions(UnwrapDocument(items[0].NodeValue), baseParams);
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

    // Unicode normalization of source text (normalization-form parameter), applied
    // per text run BEFORE character mapping (map replacements are not normalized).
    private static string ApplyNormalizationForm(string result, string? form)
        => form is null or "none" ? result
            : form.Trim().ToUpperInvariant() switch
            {
                "NFC" => result.Normalize(System.Text.NormalizationForm.FormC),
                "NFD" => result.Normalize(System.Text.NormalizationForm.FormD),
                "NFKC" => result.Normalize(System.Text.NormalizationForm.FormKC),
                "NFKD" => result.Normalize(System.Text.NormalizationForm.FormKD),
                "FULLY-NORMALIZED" => result.Normalize(System.Text.NormalizationForm.FormC),
                _ => throw new InvalidOperationException($"SENR0003: Unknown normalization form '{form}'.")
            };

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

    /// <summary>
    /// Builds serialization parameters from an <c>output:serialization-parameters</c>
    /// document (the <c>output:parameter-document</c> option).
    /// </summary>
    internal static SerializationParameters ParametersFromElementForm(IXdmNode document, SerializationParameters? baseParams = null)
        => ParseElementOptions(UnwrapDocument(document), baseParams);

    /// <summary>
    /// Builds serialization parameters from static output declarations
    /// (<c>declare option output:* "..."</c>). QName-valued parameters arrive as
    /// space-separated expanded <c>{uri}local</c> tokens (unprefixed names are plain).
    /// </summary>
    internal static SerializationParameters ParametersFromOutputDictionary(
        IReadOnlyDictionary<(string NamespaceUri, string LocalName), string> options,
        SerializationParameters? baseParams = null)
    {
        var p = baseParams ?? new SerializationParameters();
        foreach (var ((ns, local), value) in options)
        {
            if (ns != OutputNs)
                continue; // vendor options are ignored
            switch (local)
            {
                case "parameter-document": break; // resolved separately by fn:serialize
                case "method": p.Method = value.Trim().ToLowerInvariant(); break;
                case "indent": p.Indent = AsElementBoolean(value, local); break;
                case "omit-xml-declaration": p.OmitXmlDeclaration = AsElementBoolean(value, local); break;
                case "standalone":
                    p.Standalone = value.Trim() == "omit" ? null : AsElementBoolean(value, local);
                    break;
                case "item-separator": p.ItemSeparator = value; break;
                case "encoding": p.Encoding = value; break;
                case "version": p.Version = value; break;
                case "media-type": p.MediaType = value; break;
                case "doctype-system": p.DoctypeSystem = value; break;
                case "doctype-public": p.DoctypePublic = value; break;
                case "normalization-form": p.NormalizationForm = value; break;
                case "json-node-output-method": p.JsonNodeOutputMethod = value.Trim().ToLowerInvariant(); break;
                case "html-version":
                    if (decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var hv))
                        p.HtmlVersion = hv;
                    break;
                case "allow-duplicate-names": p.AllowDuplicateNames = AsElementBoolean(value, local); break;
                case "cdata-section-elements": p.CdataSectionElements = ParseExpandedQNameSet(value); break;
                case "suppress-indentation": p.SuppressIndentation = ParseExpandedQNameSet(value); break;
                case "include-content-type": p.IncludeContentType = AsElementBoolean(value, local); break;
                case "escape-uri-attributes": p.EscapeUriAttributes = AsElementBoolean(value, local); break;
                case "undeclare-prefixes": p.UndeclarePrefixes = AsElementBoolean(value, local); break;
                case "byte-order-mark":
                    break;
                default:
                    throw new InvalidOperationException(
                        $"SEPM0017: Unknown serialization parameter '{local}' in the serialization namespace.");
            }
        }
        return p;
    }

    private static HashSet<(string Ns, string Local)> ParseExpandedQNameSet(string value)
    {
        var result = new HashSet<(string, string)>();
        foreach (var token in value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.StartsWith('{'))
            {
                int close = token.IndexOf('}');
                result.Add((token[1..close], token[(close + 1)..]));
            }
            else
            {
                result.Add((string.Empty, token));
            }
        }
        return result;
    }

    // =====================================================================================
    // Option parsing — map form (option parameter conventions)
    // =====================================================================================

    private static SerializationParameters ParseMapOptions(XdmMap map, SerializationParameters? baseParams = null)
    {
        // Option parameter conventions: when parameters are supplied as a map,
        // omit-xml-declaration defaults to true (serialize-xml-127a).
        var p = baseParams ?? new SerializationParameters { OmitXmlDeclaration = true };
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
                // unrecognized ones are ignored (this includes absent-namespace QNames;
                // QT3 bug 29373).
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
            case "include-content-type":
                p.IncludeContentType = AsBoolean(name, rawValue);
                break;
            case "escape-uri-attributes":
                p.EscapeUriAttributes = AsBoolean(name, rawValue);
                break;
            case "undeclare-prefixes":
                p.UndeclarePrefixes = AsBoolean(name, rawValue);
                break;
            case "byte-order-mark":
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

    private static SerializationParameters ParseElementOptions(IXdmNode node, SerializationParameters? baseParams = null)
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

        var p = baseParams ?? new SerializationParameters();
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
                case "include-content-type": p.IncludeContentType = AsElementBoolean(value, local); break;
                case "escape-uri-attributes": p.EscapeUriAttributes = AsElementBoolean(value, local); break;
                case "undeclare-prefixes": p.UndeclarePrefixes = AsElementBoolean(value, local); break;
                case "byte-order-mark":
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
            // EQName braced form: Q{uri}local.
            if (lexical.StartsWith("Q{", StringComparison.Ordinal))
            {
                int close = lexical.IndexOf('}');
                if (close > 1)
                {
                    set.Add((lexical[2..close], lexical[(close + 1)..]));
                    continue;
                }
            }
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
                // Unprefixed names use the default element namespace in scope.
                set.Add((ResolvePrefix(element, "") ?? string.Empty, lexical));
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
        // Sequence normalization: arrays in the input sequence are flattened (recursively)
        // into their members before serialization; maps and function items cannot appear.
        items = FlattenArrays(items);

        // SEPM0009: omit-xml-declaration=yes conflicts with any explicit standalone value.
        if (p.OmitXmlDeclaration && p.Standalone is not null)
            throw new InvalidOperationException(
                "SEPM0009: The omit-xml-declaration parameter must not be 'yes' when a standalone value is set.");

        // SEPM0004: standalone or a doctype requires a single top-level element node.
        if ((p.Standalone is not null || p.DoctypeSystem is not null || p.DoctypePublic is not null)
            && p.Method is "xml" or "xhtml" or "html")
        {
            int elementCount = 0;
            int nodeCount = 0;
            foreach (var item in items)
            {
                if (!item.IsNode)
                    continue;
                nodeCount++;
                if (item.NodeValue.NodeKind == XdmNodeKind.Element)
                {
                    elementCount++;
                }
                else if (item.NodeValue.NodeKind == XdmNodeKind.Document)
                {
                    int docElements = 0;
                    foreach (var _ in item.NodeValue.Children(XdmNodeKind.Element))
                        docElements++;
                    if (docElements == 1)
                        elementCount++;
                }
            }
            if (elementCount != 1 || nodeCount != 1)
                throw new InvalidOperationException(
                    "SEPM0004: A standalone or doctype parameter requires the sequence to consist of a single element or document node.");
        }

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

        // XML declaration (only for xml/xhtml; emitted when not omitted, and always
        // when an explicit standalone value forces it).
        if (p.Method is "xml" or "xhtml" && (!p.OmitXmlDeclaration || p.Standalone is not null))
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

        // DOCTYPE rules (Serialization 3.1 §5/§6, per the QT3 ser/* matrix):
        // - doctype-system always produces a SYSTEM (optionally PUBLIC+SYSTEM) doctype.
        // - doctype-public alone produces a PUBLIC-only doctype for the html method only
        //   (an XML doctype requires a system identifier; xhtml falls back to the version rule).
        // - html with html-version set, and xhtml with html-version 5+, get the bare
        //   '<!DOCTYPE html>' regardless of doctype-public; html with only 'version' gets
        //   it when no doctype-public/system is set. In all cases a top-level non-html
        //   document element suppresses the bare form.
        bool firstIsHtml = FirstTopLevelElementIsHtml(items);
        bool bareDoctype =
            (p.Method == "html" && p.HtmlVersion is not null && firstIsHtml)
            || (p.Method == "xhtml" && p.HtmlVersion is >= 5 && firstIsHtml);
        if (p.DoctypeSystem is not null
            && (p.Method is "xml" or "xhtml" || (p.Method == "html" && p.HtmlVersion is null)))
        {
            var docName = FirstElementName(items);
            if (docName is not null)
            {
                var doctype = new StringBuilder("<!DOCTYPE ").Append(docName);
                if (p.DoctypePublic is not null)
                    doctype.Append(" PUBLIC \"").Append(p.DoctypePublic).Append('"');
                doctype.Append(p.DoctypePublic is null ? " SYSTEM \"" : " \"").Append(p.DoctypeSystem).Append('"');
                doctype.Append('>');
                writer.Raw(doctype.ToString());
            }
        }
        else if (!bareDoctype && p.DoctypePublic is not null && p.Method == "html" && p.HtmlVersion is null)
        {
            var docName = FirstElementName(items);
            if (docName is not null)
                writer.Raw($"<!DOCTYPE {docName} PUBLIC \"{p.DoctypePublic}\">");
        }
        else if (bareDoctype)
        {
            writer.Raw("<!DOCTYPE html>");
        }
        else if (p.Method == "html" && p.HtmlVersion is null && firstIsHtml
                 && decimal.TryParse(p.Version, NumberStyles.Float, CultureInfo.InvariantCulture, out var fallbackVersion)
                 && fallbackVersion >= 5)
        {
            writer.Raw("<!DOCTYPE html>");
        }

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
                    // SENR0001: free-standing attribute/namespace nodes cannot be serialized
                    // (any method; the xml family pre-check rejects them earlier).
                    throw new InvalidOperationException(
                        $"SENR0001: Cannot serialize a free-standing {node.NodeKind.ToString().ToLowerInvariant()} node.");
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

    // Recursively replaces arrays in a top-level item list with their members
    // (serialization sequence normalization for the xml method family).
    private static List<XdmValue> FlattenArrays(List<XdmValue> items)
    {
        if (!items.Any(i => i.IsArray))
            return items;
        var flattened = new List<XdmValue>(items.Count);
        void Append(XdmValue value)
        {
            if (value.IsArray && value.ArrayValue is not null)
            {
                foreach (var member in value.ArrayValue.Values)
                    Append(member);
            }
            else
            {
                flattened.Add(value);
            }
        }
        foreach (var item in items)
            Append(item);
        return flattened;
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
            => node.LocalName.Equals("html", StringComparison.OrdinalIgnoreCase)
               && (node.NamespaceUri.Length == 0 || node.NamespaceUri == "http://www.w3.org/1999/xhtml");
    }

    // The qualified name of the first element in the sequence (through document nodes).
    private static string? FirstElementName(List<XdmValue> items)
    {
        foreach (var item in items)
        {
            if (!item.IsNode)
                continue;
            var node = item.NodeValue;
            if (node.NodeKind == XdmNodeKind.Element)
                return node.Prefix.Length > 0 ? node.Prefix + ":" + node.LocalName : node.LocalName;
            if (node.NodeKind == XdmNodeKind.Document)
            {
                foreach (var childValue in node.Children(XdmNodeKind.Element))
                    return FirstElementName(new List<XdmValue> { childValue });
            }
        }
        return null;
    }

    /// <summary>Recursive node writer implementing the xml/xhtml/html output rules.</summary>
    private sealed class NodeWriter
    {
        private readonly SerializationParameters _p;
        private readonly StringBuilder _sb = new();
        private bool _metaInjected;
        private int _xmlSpacePreserve;
        // Set while writing the content of a foreign-namespace "XML island" element:
        // its direct text children serialize as CDATA sections (HTML §6.1.7).
        private bool _foreignIslandCdata;
        private readonly List<(string Prefix, string Uri)> _nsScopes = new();

        public NodeWriter(SerializationParameters p) => _p = p;

        public void Raw(string s) => _sb.Append(s);

        /// <summary>Writes an atomic value's string form with text-node escaping.</summary>
        public void Text(string s) => WriteTextContent(s);

        /// <summary>Writes text as a CDATA section, splitting any embedded <c>]]&gt;</c> terminator.</summary>
        private void WriteCdataText(string s)
        {
            _sb.Append("<![CDATA[").Append(s.Replace("]]>", "]]]]><![CDATA[>", StringComparison.Ordinal)).Append("]]>");
        }

        public override string ToString() => _sb.ToString();

        public void WriteNode(IXdmNode node, int depth, bool suppressIndent)
        {
            // Text method: elements and documents contribute their descendant text content
            // only; comments and processing instructions are dropped entirely.
            if (_p.Method == "text" && node.NodeKind is XdmNodeKind.Element or XdmNodeKind.Document)
            {
                foreach (var childValue in node.Children())
                    WriteNode(childValue.NodeValue, depth, suppressIndent);
                return;
            }
            if (_p.Method == "text" && node.NodeKind is XdmNodeKind.Comment or XdmNodeKind.ProcessingInstruction)
                return;

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
                    // A text child of a foreign-namespace element whose parent is an HTML
                    // element serializes as a CDATA section (html-18/19a).
                    if (_foreignIslandCdata)
                        WriteCdataText(node.StringValue);
                    else
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
                    // HTML processing instructions end with '>' (not '?>').
                    _sb.Append(_p.Method == "html" ? '>' : "?>");
                    break;
                default:
                    WriteTextContent(node.StringValue);
                    break;
            }
        }

        private void WriteElement(IXdmNode element, int depth, bool suppressIndent)
        {
            // HTML 5: elements in the XHTML namespace serialize with their local name and a
            // default-namespace declaration (prefixes are not used in HTML); HTML 4 keeps them.
            string effectivePrefix = element.Prefix;
            string effectiveNs = element.NamespaceUri;
            bool html5 = _p.HtmlVersion is >= 5
                         || (_p.HtmlVersion is null
                             && decimal.TryParse(_p.Version, NumberStyles.Float, CultureInfo.InvariantCulture, out var docVersion)
                             && docVersion >= 5);
            // HTML 5 prefix normalization: elements in the XHTML namespace (html and
            // xhtml methods) — and any namespace at all for the xhtml method — serialize
            // with their local name and a default-namespace declaration (Serialization-xhtml-50/51/52).
            bool dropXhtmlPrefix = html5 && element.Prefix.Length > 0
                && (_p.Method == "xhtml"
                    ? element.NamespaceUri.Length > 0
                    : _p.Method == "html" && element.NamespaceUri == "http://www.w3.org/1999/xhtml");
            if (dropXhtmlPrefix)
                effectivePrefix = "";
            string name = effectivePrefix.Length > 0 ? effectivePrefix + ":" + element.LocalName : element.LocalName;
            _sb.Append('<').Append(name);
            // Namespace fixup: the element's expanded name must be bound in scope; a
            // declaration is added when the parent scope lacks it (or an undeclaration
            // when the element is in no namespace under a default one).
            int scopeMark = _nsScopes.Count;
            (string Prefix, string Uri)? addedDecl = null;
            {
                string? inScope = LookupNamespace(effectivePrefix);
                if (effectiveNs.Length == 0)
                {
                    // An element in no namespace under a default one needs an undeclaration —
                    // unless the element already declares xmlns="" itself (K2-InScopePrefixesFunc-28).
                    if (effectivePrefix.Length == 0 && !string.IsNullOrEmpty(inScope))
                    {
                        bool selfUndeclared = false;
                        foreach (var attrValue in element.Attributes())
                        {
                            var a = attrValue.NodeValue;
                            if (a.LocalName == "xmlns" && a.NamespaceUri.Length == 0 && a.StringValue.Length == 0)
                            {
                                selfUndeclared = true;
                                break;
                            }
                        }
                        if (!selfUndeclared)
                        {
                            _sb.Append(" xmlns=\"\"");
                            addedDecl = ("", "");
                        }
                    }
                }
                else if (inScope != effectiveNs && effectivePrefix != "xml")
                {
                    // Not needed when the element's own attributes already declare it.
                    bool selfDeclared = false;
                    foreach (var attrValue in element.Attributes())
                    {
                        var a = attrValue.NodeValue;
                        if (IsNamespaceDeclaration(a) &&
                            (effectivePrefix.Length == 0
                                ? a.LocalName == "xmlns" && a.NamespaceUri.Length == 0
                                : a.LocalName == effectivePrefix && a.NamespaceUri == XmlnsNs))
                        {
                            selfDeclared = true;
                            break;
                        }
                    }
                    if (!selfDeclared)
                    {
                        addedDecl = (effectivePrefix, effectiveNs);
                        _sb.Append(effectivePrefix.Length == 0
                            ? $" xmlns=\"{effectiveNs}\""
                            : $" xmlns:{effectivePrefix}=\"{effectiveNs}\"");
                    }
                }
            }
            bool entersPreserve = false;
            foreach (var attrValue in element.Attributes())
            {
                var attr = attrValue.NodeValue;
                // Prefix normalization (HTML5): a prefixed namespace declaration is redundant
                // once the element is rewritten to the default-namespace form — unless one of
                // the element's attributes actually uses that prefix (xhtml-50/51/52).
                if (dropXhtmlPrefix && IsNamespaceDeclaration(attr)
                    && (attr.LocalName == "xmlns" && attr.NamespaceUri.Length == 0
                        ? attr.StringValue == element.NamespaceUri
                        : !UsesAttributePrefix(element, attr.LocalName)))
                    continue;
                if (attr.LocalName == "space" && attr.Prefix == "xml" && attr.StringValue == "preserve")
                    entersPreserve = true;
                // XML 1.1 prefixed namespace undeclarations travel as a placeholder URI
                // and serialize back as xmlns:p="".
                bool isXml11Undeclaration = attr.NamespaceUri == XmlnsNs
                    && attr.StringValue.StartsWith("urn:bosak-xml11-undecl:", StringComparison.Ordinal);
                // Push the element's own namespace declarations for its children.
                if (IsNamespaceDeclaration(attr))
                {
                    var declValue = isXml11Undeclaration ? "" : attr.StringValue;
                    var declPrefix = attr.LocalName == "xmlns" && attr.NamespaceUri.Length == 0 ? "" : attr.LocalName;
                    // Namespace fixup: a declaration identical to one already in scope is
                    // redundant and omitted (the binding remains in scope from the ancestor).
                    if (LookupNamespace(declPrefix) == declValue)
                        continue;
                    if (attr.LocalName == "xmlns" && attr.NamespaceUri.Length == 0)
                        _nsScopes.Add(("", declValue));
                    else if (attr.NamespaceUri == XmlnsNs)
                        _nsScopes.Add((attr.LocalName, declValue));
                }
                _sb.Append(' ').Append(AttributeName(attr));
                var attrText = isXml11Undeclaration ? "" : attr.StringValue;
                // HTML boolean attributes minimize to the bare name when the value equals
                // the name ignoring case (Serialization-html-12/13: selected="SELECTED").
                if (_p.Method == "html"
                    && HtmlBooleanAttributes.Contains(attr.LocalName)
                    && attrText.Equals(attr.LocalName, StringComparison.OrdinalIgnoreCase))
                    continue;
                _sb.Append("=\"");
                // HTML URI attributes are percent-encoded unless escape-uri-attributes is off.
                if (_p.Method == "html" && _p.EscapeUriAttributes && IsUriAttribute(attr.LocalName))
                    attrText = EscapeUri(attrText);
                WriteAttributeContent(attrText);
                _sb.Append('"');
            }
            if (addedDecl is not null)
                _nsScopes.Add(addedDecl.Value);

            // XML 1.1 prefixed namespace undeclarations recorded at parse time round-trip
            // as xmlns:p="" when undeclare-prefixes applies (explicit yes, or XML 1.1 default).
            bool undeclare = _p.UndeclarePrefixes ?? _p.Version == "1.1";
            if (undeclare && element is Bosak.XPath.Providers.Xml.XDocumentNode xn && xn.Xml11UndeclaredPrefixes.Count > 0)
            {
                foreach (var undeclaredPrefix in xn.Xml11UndeclaredPrefixes)
                {
                    _sb.Append(" xmlns:").Append(undeclaredPrefix).Append("=\"\"");
                    _nsScopes.Add((undeclaredPrefix, ""));
                }
            }

            var children = new List<IXdmNode>();
            foreach (var childValue in element.Children())
                children.Add(childValue.NodeValue);

            bool isHtml = _p.Method == "html";
            // Version-dependent HTML void lists (Serialization 3.1 §6.1.4): frame and
            // isindex are void in HTML 4.0 only; keygen, basefont and bgsound are HTML5-era.
            bool htmlVoidElement = html5
                ? HtmlVoidElements.Contains(element.LocalName)
                : Html40VoidElements.Contains(element.LocalName);
            bool inXhtmlNs = element.NamespaceUri == "http://www.w3.org/1999/xhtml";
            bool foreignNs = element.NamespaceUri.Length > 0 && !inXhtmlNs;
            // The bare void form (no end tag) applies to the html method only: in HTML5
            // regardless of namespace, in HTML 4.0 only for no-namespace elements (html-3).
            bool isVoid = isHtml && htmlVoidElement && !foreignNs && (html5 || !inXhtmlNs);
            bool injectingMeta = InjectMeta(element);
            // A foreign-namespace element whose parent is an HTML element has its text
            // children serialized as CDATA sections (Serialization 3.1 §6.1.7, html-18/19a);
            // the rule does not recurse into nested foreign elements.
            bool parentForeignIsland = _foreignIslandCdata;
            bool foreignIsland = isHtml && foreignNs && !parentForeignIsland;

            if (children.Count == 0 && !injectingMeta && !isVoid)
            {
                if (isHtml && htmlVoidElement && inXhtmlNs && !html5)
                {
                    // HTML 4.0: an XHTML-namespace void element serializes as XML (html-3).
                    _sb.Append("/>");
                }
                else if (_p.Method == "xhtml" && htmlVoidElement
                         && (inXhtmlNs || (html5 && element.NamespaceUri.Length == 0)))
                {
                    // XHTML void elements self-close with a space: HTML5 recognizes them
                    // in no namespace as well (xhtml-2); HTML 4.0 only in the XHTML
                    // namespace (xhtml-1a).
                    _sb.Append(" />");
                }
                else if (isHtml && foreignNs)
                {
                    // HTML "XML islands": foreign-namespace empty elements self-close
                    // XML-style (Serialization-html-5/6).
                    _sb.Append("/>");
                }
                else if (isHtml || _p.Method == "xhtml")
                {
                    // HTML/XHTML: non-void empty elements get a separate end tag.
                    _sb.Append("></").Append(name).Append('>');
                }
                else
                {
                    _sb.Append("/>");
                }
                _nsScopes.RemoveRange(scopeMark, _nsScopes.Count - scopeMark);
                return;
            }
            _sb.Append('>');

            if (injectingMeta)
            {
                _metaInjected = true;
                if (isHtml && _p.HtmlVersion is >= 5)
                {
                    // HTML5: the short charset form.
                    _sb.Append("<meta charset=\"").Append(_p.Encoding ?? "UTF-8").Append("\">");
                }
                else
                {
                    _sb.Append("<meta http-equiv=\"content-type\" content=\"text/html; charset=")
                      .Append(_p.Encoding ?? "UTF-8").Append('"');
                    _sb.Append(isHtml ? ">" : "/>");
                }
            }
            if (isVoid)
            {
                _nsScopes.RemoveRange(scopeMark, _nsScopes.Count - scopeMark);
                return;
            }

            bool cdata = _p.Method is "xml" or "xhtml" && InCdataList(element);
            // HTML: listed elements in no namespace get raw (unescaped) text; namespaced
            // ones keep CDATA sections. Under HTML5, CDATA survives only in foreign
            // (non-XHTML) content — XHTML-namespace and no-namespace listed content is raw.
            // HTML script and style content is always raw (Serialization 3.1 §6.1.9).
            bool rawHtmlText = _p.Method == "html"
                && (IsRawTextHtmlElement(element)
                    || (InCdataList(element)
                        && (html5
                            ? element.NamespaceUri is "" or "http://www.w3.org/1999/xhtml"
                            : element.NamespaceUri.Length == 0)));
            bool cdataHtml = _p.Method == "html" && InCdataList(element)
                && (html5
                    ? element.NamespaceUri is not ("" or "http://www.w3.org/1999/xhtml")
                    : element.NamespaceUri.Length > 0);
            // xml:space="preserve" suppresses indentation for the element's content.
            if (entersPreserve)
                _xmlSpacePreserve++;
            bool indentContent = _p.Indent
                                 && !suppressIndent
                                 && _xmlSpacePreserve == 0
                                 && children.Count > 0
                                 && ElementOnlyContent(children);

            var savedForeignIslandCdata = _foreignIslandCdata;
            _foreignIslandCdata = foreignIsland;
            try
            {
                if (indentContent)
                {
                    foreach (var child in children)
                    {
                        _sb.Append('\n');
                        for (int i = 0; i <= depth; i++)
                            _sb.Append("   ");
                        // Suppression propagates to the whole subtree (not just the listed element).
                        WriteNode(child, depth + 1, suppressIndent || IsSuppressed(child));
                    }
                    _sb.Append('\n');
                    for (int i = 0; i < depth; i++)
                        _sb.Append("   ");
                }
                else if (cdata || cdataHtml)
                {
                    WriteCdataChildren(children, depth, suppressIndent);
                }
                else if (rawHtmlText)
                {
                    // Raw text content: only script/style nests raw elements (attributes and
                    // text unescaped); listed (cdata-section) elements nest normal elements.
                    WriteRawTextChildren(children, depth, suppressIndent, rawNesting: IsRawTextHtmlElement(element));
                }
                else
                {
                    foreach (var child in children)
                    {
                        // A pre-existing content-type meta is replaced by the injected one.
                        if (injectingMeta && IsContentTypeMeta(child))
                            continue;
                        WriteNode(child, depth + 1, suppressIndent || IsSuppressed(child));
                    }
                }
            }
            finally
            {
                _foreignIslandCdata = savedForeignIslandCdata;
            }

            if (entersPreserve)
                _xmlSpacePreserve--;
            _nsScopes.RemoveRange(scopeMark, _nsScopes.Count - scopeMark);
            _sb.Append("</").Append(name).Append('>');
        }

        // The URI currently bound to a prefix in the writer's declaration scopes.
        private string? LookupNamespace(string prefix)
        {
            for (int i = _nsScopes.Count - 1; i >= 0; i--)
            {
                if (_nsScopes[i].Prefix == prefix)
                    return _nsScopes[i].Uri;
            }
            return null;
        }

        /// <summary>Whether a meta element must be injected into this (head) element.</summary>
        private bool InjectMeta(IXdmNode element)
            => _p.Method is "html" or "xhtml"
               && _p.IncludeContentType
               && !_metaInjected
               && element.LocalName.Equals("head", StringComparison.OrdinalIgnoreCase)
               && (element.NamespaceUri.Length == 0 || element.NamespaceUri == "http://www.w3.org/1999/xhtml");

        // An existing meta with http-equiv=content-type (any casing) is replaced by the
        // injected one when content-type injection applies.
        private static bool IsContentTypeMeta(IXdmNode node)
        {
            if (node.NodeKind != XdmNodeKind.Element || !node.LocalName.Equals("meta", StringComparison.OrdinalIgnoreCase))
                return false;
            foreach (var attrValue in node.Attributes())
            {
                var attr = attrValue.NodeValue;
                if (attr.LocalName.Equals("http-equiv", StringComparison.OrdinalIgnoreCase))
                    return attr.StringValue.Equals("content-type", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private void WriteRawTextChildren(List<IXdmNode> children, int depth, bool suppressIndent, bool rawNesting)
        {
            foreach (var child in children)
            {
                if (child.NodeKind == XdmNodeKind.Text)
                    _sb.Append(child.StringValue);
                else if (child.NodeKind == XdmNodeKind.Element && rawNesting)
                    WriteRawElement(child);
                else
                    WriteNode(child, depth + 1, suppressIndent || IsSuppressed(child));
            }
        }

        /// <summary>
        /// Writes an element nested inside an HTML raw-text element (script/style): start
        /// and end tags are written literally, but neither text nor attribute values are
        /// escaped (Serialization-html-9/10 — "&amp;" stays "&" inside raw-text content).
        /// </summary>
        private void WriteRawElement(IXdmNode element)
        {
            string name = QualifiedName(element);
            _sb.Append('<').Append(name);
            foreach (var attrValue in element.Attributes())
            {
                var attr = attrValue.NodeValue;
                _sb.Append(' ').Append(AttributeName(attr)).Append("=\"").Append(attr.StringValue).Append('"');
            }
            var children = new List<IXdmNode>();
            foreach (var childValue in element.Children())
                children.Add(childValue.NodeValue);
            if (children.Count == 0 && HtmlVoidElements.Contains(element.LocalName))
                return;
            _sb.Append('>');
            foreach (var child in children)
            {
                if (child.NodeKind == XdmNodeKind.Text)
                    _sb.Append(child.StringValue);
                else if (child.NodeKind == XdmNodeKind.Element)
                    WriteRawElement(child);
                else
                    WriteNode(child, 0, suppressIndent: false);
            }
            _sb.Append("</").Append(name).Append('>');
        }

        // HTML raw-text elements (Serialization 3.1 §6.1.9): script and style content is
        // not escaped when the element is in no namespace or the XHTML namespace.
        private static bool IsRawTextHtmlElement(IXdmNode element)
            => (element.LocalName.Equals("script", StringComparison.OrdinalIgnoreCase)
                || element.LocalName.Equals("style", StringComparison.OrdinalIgnoreCase))
               && element.NamespaceUri is "" or "http://www.w3.org/1999/xhtml";

        // Whether any attribute of the element carries the given namespace prefix: a
        // declaration for it must be kept even under HTML5 prefix normalization.
        private static bool UsesAttributePrefix(IXdmNode element, string prefix)
        {
            foreach (var attrValue in element.Attributes())
            {
                var attr = attrValue.NodeValue;
                if (attr.NamespaceUri.Length > 0 && attr.NamespaceUri != XmlnsNs && attr.Prefix == prefix)
                    return true;
            }
            return false;
        }

        // HTML attributes known to carry URIs (Serialization 3.1 §7.4.13).
        private static bool IsUriAttribute(string localName)
            => localName is "href" or "src" or "action" or "cite" or "data" or "longdesc" or "usemap";

        // Percent-encodes characters outside the printable ASCII range as UTF-8 %HH bytes.
        private static string EscapeUri(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c <= 0x7E)
                {
                    sb.Append(c);
                    continue;
                }
                string ch = char.IsHighSurrogate(c) && i + 1 < s.Length
                    ? s.Substring(i++, 2)
                    : c.ToString();
                foreach (var b in Encoding.UTF8.GetBytes(ch))
                    sb.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private bool IsSuppressed(IXdmNode node)
        {
            if (node.NodeKind != XdmNodeKind.Element || _p.SuppressIndentation is null)
                return false;
            // The html method matches element names case-insensitively.
            if (_p.Method == "html")
            {
                foreach (var (ns, local) in _p.SuppressIndentation)
                {
                    if (ns == node.NamespaceUri && local.Equals(node.LocalName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            return _p.SuppressIndentation.Contains((node.NamespaceUri, node.LocalName));
        }

        // Whether an element is listed in cdata-section-elements (case-insensitive for html).
        private bool InCdataList(IXdmNode element)
        {
            if (_p.CdataSectionElements is null)
                return false;
            if (_p.Method == "html")
            {
                foreach (var (ns, local) in _p.CdataSectionElements)
                {
                    if (ns == element.NamespaceUri && local.Equals(element.LocalName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            return _p.CdataSectionElements.Contains((element.NamespaceUri, element.LocalName));
        }

        private static bool ElementOnlyContent(List<IXdmNode> children)
        {
            foreach (var child in children)
            {
                // Indentation is only added for element-only content: a text, comment,
                // or processing-instruction child makes the content mixed.
                if (child.NodeKind is XdmNodeKind.Text or XdmNodeKind.Comment or XdmNodeKind.ProcessingInstruction)
                    return false;
            }
            return true;
        }

        private void WriteCdataChildren(List<IXdmNode> children, int depth, bool suppressIndent)
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
                WriteNode(child, depth + 1, suppressIndent || IsSuppressed(child));
            }
            FlushCdata(textRun);
        }

        private void FlushCdata(StringBuilder textRun)
        {
            if (textRun.Length == 0)
                return;
            string text = textRun.ToString();
            textRun.Clear();
            // Characters not representable in the encoding are output as character
            // references, splitting the CDATA section around them; embedded "]]>"
            // sequences are likewise split across two sections.
            _sb.Append("<![CDATA[");
            var segment = new StringBuilder();
            void FlushSegment()
            {
                if (segment.Length == 0) return;
                _sb.Append(segment.ToString().Replace("]]>", "]]]]><![CDATA[>"));
                segment.Clear();
            }
            foreach (char c in text)
            {
                if (!IsRepresentableInEncoding(c))
                {
                    FlushSegment();
                    _sb.Append("]]>").Append("&#x").Append(((int)c).ToString("X", CultureInfo.InvariantCulture)).Append(';').Append("<![CDATA[");
                }
                else
                {
                    segment.Append(c);
                }
            }
            FlushSegment();
            _sb.Append("]]>");
        }

        // Whether a character is representable in the chosen output encoding
        // (conservative: unknown encodings accept everything).
        private bool IsRepresentableInEncoding(char c)
        {
            if (_p.Encoding is null || _p.Encoding.StartsWith("utf", StringComparison.OrdinalIgnoreCase))
                return true;
            if (_p.Encoding.Equals("us-ascii", StringComparison.OrdinalIgnoreCase)
                || _p.Encoding.Equals("ascii", StringComparison.OrdinalIgnoreCase))
                return c <= 0x7F;
            if (_p.Encoding.StartsWith("iso-8859", StringComparison.OrdinalIgnoreCase)
                || _p.Encoding.Equals("latin1", StringComparison.OrdinalIgnoreCase))
                return c <= 0xFF;
            return true;
        }

        private void WriteTextContent(string s)
        {
            // Normalization-form applies to source text before character mapping.
            s = ApplyNormalizationForm(s, _p.NormalizationForm);
            foreach (char c in s)
            {
                if (_p.CharacterMaps is not null && _p.CharacterMaps.TryGetValue(c.ToString(), out var replacement))
                {
                    _sb.Append(replacement);
                    continue;
                }
                // The text output method writes characters raw (no escaping at all).
                if (_p.Method == "text")
                {
                    _sb.Append(c);
                    continue;
                }
                switch (c)
                {
                    case '&': _sb.Append("&amp;"); break;
                    case '<': _sb.Append("&lt;"); break;
                    case '>': _sb.Append("&gt;"); break;
                    case '\r': _sb.Append("&#xD;"); break;
                    default:
                        // XML 1.1: control characters other than tab/LF/CR serialize as char refs.
                        if (c < ' ' && c is not ('\t' or '\n'))
                            _sb.Append("&#x").Append(((int)c).ToString("X", CultureInfo.InvariantCulture)).Append(';');
                        // XML/XHTML: NEL, LINE SEPARATOR, and the C1 control range serialize as
                        // character references so they survive re-parsing (Serialization 3.1 §4;
                        // K2-Serialization-5/9/10).
                        else if ((_p.Method is "xml" or "xhtml") && (c is >= '\u007F' and <= '\u009F' or '\u2028'))
                            _sb.Append("&#x").Append(((int)c).ToString("X", CultureInfo.InvariantCulture)).Append(';');
                        else
                            _sb.Append(c);
                        break;
                }
            }
        }

        private void WriteAttributeContent(string s)
        {
            // Normalization-form applies to source text before character mapping.
            s = ApplyNormalizationForm(s, _p.NormalizationForm);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (_p.CharacterMaps is not null && _p.CharacterMaps.TryGetValue(c.ToString(), out var replacement))
                {
                    _sb.Append(replacement);
                    continue;
                }
                switch (c)
                {
                    case '&':
                        // HTML: an ampersand immediately followed by a left curly brace is
                        // not escaped (Serialization 3.1 §6.1.9 — Serialization-html-11).
                        if (_p.Method == "html" && i + 1 < s.Length && s[i + 1] == '{')
                            _sb.Append('&');
                        else
                            _sb.Append("&amp;");
                        break;
                    case '<': _sb.Append("&lt;"); break;
                    case '"': _sb.Append("&quot;"); break;
                    case '\t': _sb.Append("&#x9;"); break;
                    case '\n': _sb.Append("&#xA;"); break;
                    case '\r': _sb.Append("&#xD;"); break;
                    default:
                        // XML 1.1: remaining control characters serialize as char refs.
                        if (c < ' ')
                            _sb.Append("&#x").Append(((int)c).ToString("X", CultureInfo.InvariantCulture)).Append(';');
                        // XML/XHTML: NEL, LINE SEPARATOR, and the C1 control range serialize as
                        // character references so they survive re-parsing (Serialization 3.1 §4;
                        // K2-Serialization-6/9).
                        else if ((_p.Method is "xml" or "xhtml") && (c is >= '\u007F' and <= '\u009F' or '\u2028'))
                            _sb.Append("&#x").Append(((int)c).ToString("X", CultureInfo.InvariantCulture)).Append(';');
                        else
                            _sb.Append(c);
                        break;
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
            // SENR0001: free-standing attribute/namespace nodes cannot be serialized.
            if (value.NodeValue.NodeKind is XdmNodeKind.Attribute or XdmNodeKind.Namespace)
                throw new InvalidOperationException(
                    $"SENR0001: Cannot serialize a free-standing {value.NodeValue.NodeKind.ToString().ToLowerInvariant()} node.");
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
        // Normalization-form applies to source text before character mapping.
        value = ApplyNormalizationForm(value, p.NormalizationForm);
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (char c in value)
        {
            // Character maps apply to JSON string content before escaping.
            if (p.CharacterMaps is not null && p.CharacterMaps.TryGetValue(c.ToString(), out var replacement))
            {
                sb.Append(replacement);
                continue;
            }
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
        string separator = p.Separator;
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
                // Typed strings outside the string family (dates, durations, g* types)
                // serialize in constructor form with the primitive type name; string-family
                // values (including subtypes) are quoted.
                if (TypedStringFamily(value) is { } family)
                {
                    sb.Append("xs:").Append(family).Append("(\"").Append(value.StringValue).Append("\")");
                }
                else
                {
                    sb.Append('"').Append(value.StringValue.Replace("\"", "\"\"")).Append('"');
                }
                return;
            case XdmValueKind.Boolean:
                sb.Append(value.BooleanValue ? "true()" : "false()");
                return;
            case XdmValueKind.Integer:
            case XdmValueKind.Decimal:
                sb.Append(value.ToString());
                return;
            case XdmValueKind.Double:
                sb.Append(CanonicalDouble(value.DoubleValue));
                return;
            case XdmValueKind.Float:
                // Adaptive: floats use constructor form; negative infinity stays lexical.
                if (double.IsNegativeInfinity(value.DoubleValue))
                    sb.Append("-INF");
                else
                    sb.Append("xs:float(\"").Append(value.ToString()).Append("\")");
                return;
        }

        if (value.Kind == XdmValueKind.QName)
        {
            var q = value.QNameValue;
            sb.Append("Q{").Append(q.NamespaceUri).Append('}').Append(q.LocalName);
            return;
        }

        if (value.IsFunction)
        {
            // Adaptive: a named function serializes as name#arity, an anonymous one as
            // (anonymous-function)#arity.
            if (value.FunctionValue is NamedFunctionItem nfi)
            {
                sb.Append(FunctionPrefix(nfi.NamespaceUri)).Append(nfi.LocalName).Append('#').Append(nfi.Arity);
            }
            else if (value.FunctionValue is FunctionItem fi)
            {
                sb.Append("(anonymous-function)#").Append(fi.Arity);
            }
            return;
        }

        if (value.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time
            or XdmValueKind.Duration or XdmValueKind.Uri)
        {
            // Remaining atomic types serialize in constructor form with the primitive
            // type name: xs:TYPE("lexical").
            string typeName = PrimitiveTypeName(value.SchemaTypeName ?? value.Kind switch
            {
                XdmValueKind.DateTime => "dateTime",
                XdmValueKind.Date => "date",
                XdmValueKind.Time => "time",
                XdmValueKind.Duration => "duration",
                _ => "anyURI"
            });
            sb.Append("xs:").Append(typeName).Append("(\"").Append(value.ToString()).Append("\")");
            return;
        }

        if (value.IsNode)
        {
            var node = value.NodeValue;
            if (node.NodeKind == XdmNodeKind.Attribute)
            {
                sb.Append(node.Prefix.Length > 0 ? node.Prefix + ":" + node.LocalName : node.LocalName);
                sb.Append("=\"");
                sb.Append(node.StringValue.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;"));
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

    // Conventional prefix for a function namespace in adaptive serialization.
    private static string FunctionPrefix(string ns) => ns switch
    {
        "http://www.w3.org/2005/xpath-functions" => "fn:",
        "http://www.w3.org/2005/xpath-functions/math" => "math:",
        "http://www.w3.org/2005/xpath-functions/map" => "map:",
        "http://www.w3.org/2005/xpath-functions/array" => "array:",
        "http://www.w3.org/2001/XMLSchema" => "xs:",
        "http://www.w3.org/2005/xquery-local-functions" => "local:",
        "" => "",
        var other => $"Q{{{other}}}"
    };

    // The primitive type name used in adaptive constructor-form serialization.
    private static string PrimitiveTypeName(string typeName) => typeName switch
    {
        "dateTimeStamp" => "dateTime",
        "yearMonthDuration" or "dayTimeDuration" => "duration",
        _ => typeName
    };

    // Constructor-form family for Kind String values in adaptive serialization:
    // null for the string family (plain quoted form), otherwise the primitive type name.
    private static string? TypedStringFamily(XdmValue value)
        => value.SchemaTypeName is null || StringFamilyNames.Contains(value.SchemaTypeName)
            ? null
            : PrimitiveTypeName(value.SchemaTypeName);

    // String-family type names (case-insensitive): values of these types serialize as
    // plain quoted strings (and share one map-key family).
    private static readonly HashSet<string> StringFamilyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "", "string", "untypedAtomic", "anyURI",
        "normalizedString", "token", "language", "NMTOKEN", "NMTOKENS",
        "Name", "NCName", "ID", "IDREF", "IDREFS", "ENTITY", "ENTITIES",
        "ncname", "id", "idref", "idrefs", "entity", "entities", "nmtoken", "nmtokens",
        "name", "normalizedstring"
    };

    // Canonical xs:double lexical form (mantissa with one digit before the point and at
    // least one after, exponent without sign padding): 1.0e0, 1.5e0, 1.0e-3.
    private static string CanonicalDouble(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsPositiveInfinity(d)) return "INF";
        if (double.IsNegativeInfinity(d)) return "-INF";
        var s = d.ToString("0.################E0", CultureInfo.InvariantCulture);
        int e = s.IndexOf('E');
        var mantissa = s[..e];
        if (!mantissa.Contains('.'))
            mantissa += ".0";
        return mantissa + "e" + s[(e + 1)..];
    }
}
