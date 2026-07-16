// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : Holds parsed xsl:output serialization properties.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 06-07-2026     | Added normalization-form output property and merge support for multiple xsl:output      |
//                      | Charles Korthout | 0.3   | 11-07-2026     | Added doctype, cdata-section-elements, escape-uri-attributes, include-content-type,     |
//                      |                  |       |                | media-type, byte-order-mark, html-version, and suppress-indentation properties.        |
//                      | Charles Korthout | 0.4   | 11-07-2026     | Parse html-version as decimal; accept 5.00/+5.0 and reject invalid values (XTSE0020).   |
//                      | Charles Korthout | 0.5   | 11-07-2026     | Added use-character-maps parsing/merge and resolved CharacterMap storage.               |
//                      | Charles Korthout | 0.6   | 11-07-2026     | Use-character-maps merge now prepends source lists so later declarations take precedence. |
//                      | Charles Korthout | 0.7   | 11-07-2026     | Empty doctype-public/doctype-system attributes are treated as explicit values           |
//                      | Charles Korthout | 0.8   | 11-07-2026     | Added method="json", json-node-output-method, allow-duplicate-names, escape-solidus,  |
//                      |                  |       |                | and parameter-document parsing with inline character maps.                              |
//                      | Charles Korthout | 0.9   | 11-07-2026     | Added item-separator output property for text serialization.                            |
//                      | Charles Korthout | 1.0   | 12-07-2026     | Normalize standalone values (yes/no/omit) and restrict SEPM0009 to XML/XHTML.          |
//                      | Charles Korthout | 1.1   | 12-07-2026     | Added build-tree output property for raw result-document collection.                    |
//                      | Charles Korthout | 1.2   | 12-07-2026     | Made yes/no parsing case-sensitive while still accepting true/false/1/0.                |
//                      | Charles Korthout | 1.3   | 12-07-2026     | Append use-character-maps during merge so last-wins resolution is correct.              |
//                      | Charles Korthout | 1.4   | 12-07-2026     | Merge multiple xsl:output use-character-maps lists in declaration order.                |
//                      | Charles Korthout | 1.5   | 13-07-2026     | Added EffectiveVersion and ImplicitResultTree for default method inference (BC rule).   |
//                      | Charles Korthout | 1.6   | 15-07-2026     | Added FromMap to support fn:transform serialization-params option.                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Parsed properties from xsl:output that control result tree serialization.
/// </summary>
public sealed class OutputProperties
{
    /// <summary>Serialization method: "xml", "html", "xhtml", or "text".</summary>
    public string Method { get; set; } = "xml";

    /// <summary>Whether to omit the XML declaration.</summary>
    public bool OmitXmlDeclaration { get; set; } = true;

    /// <summary>Whether to indent the output.</summary>
    public bool Indent { get; set; } = false;

    /// <summary>Character encoding (informational; output is always UTF-16 string).</summary>
    public string Encoding { get; set; } = "UTF-8";

    /// <summary>XML version for the declaration (e.g. "1.0").</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>HTML version for HTML/XHTML serialization (e.g. "5.0", "1.1", "1.0").</summary>
    public string HtmlVersion { get; set; } = "5.0";

    /// <summary>Standalone attribute for the XML declaration, or "omit" to suppress the pseudo-attribute.</summary>
    public string? Standalone { get; set; }

    /// <summary>Whether prefixed namespace undeclarations are permitted (XML 1.1).</summary>
    public bool UndeclarePrefixes { get; set; } = false;

    /// <summary>Unicode normalization form applied during serialization: "NFC", "NFD", "NFKC", "NFKD", "fully-normalized", or "none".</summary>
    public string NormalizationForm { get; set; } = "none";

    /// <summary>System identifier for the DOCTYPE declaration.</summary>
    public string? DoctypeSystem { get; set; }

    /// <summary>Public identifier for the DOCTYPE declaration.</summary>
    public string? DoctypePublic { get; set; }

    /// <summary>Element QNames whose text children should be wrapped in CDATA sections.</summary>
    public IReadOnlyList<XsQName> CdataSectionElements { get; set; } = Array.Empty<XsQName>();

    /// <summary>Element QNames for which indentation should be suppressed.</summary>
    public IReadOnlyList<XsQName> SuppressIndentation { get; set; } = Array.Empty<XsQName>();

    /// <summary>Whether URI-valued attributes in HTML/XHTML should be percent-encoded.</summary>
    public bool EscapeUriAttributes { get; set; } = false;

    /// <summary>Whether to include a Content-Type meta element in HTML/XHTML output.</summary>
    public bool IncludeContentType { get; set; } = true;

    /// <summary>Media type for the Content-Type meta element and serialization metadata.</summary>
    public string? MediaType { get; set; }

    /// <summary>Whether to emit a byte-order mark for supported encodings.</summary>
    public bool ByteOrderMark { get; set; } = false;

    /// <summary>Named character maps used by this output definition.</summary>
    public IReadOnlyList<XsQName> UseCharacterMaps { get; set; } = Array.Empty<XsQName>();

    /// <summary>Method used to serialize nodes when the output method is JSON.</summary>
    public string JsonNodeOutputMethod { get; set; } = "xml";

    /// <summary>Whether JSON output may contain duplicate keys in the same object.</summary>
    public bool AllowDuplicateNames { get; set; } = false;

    /// <summary>Whether JSON output escapes solidus characters as <c>\/</c>.</summary>
    public bool EscapeSolidus { get; set; } = false;

    /// <summary>
    /// String inserted between top-level items when serializing with <c>method="text"</c>.
    /// When unspecified, adjacent atomic values are separated by a single space.
    /// </summary>
    public string ItemSeparator { get; set; } = " ";

    /// <summary>
    /// Whether the result tree should be built for this output definition. When
    /// <c>false</c>, top-level items are preserved as raw XDM values for serialization.
    /// </summary>
    public bool BuildTree { get; set; } = true;

    /// <summary>
    /// Resolved effective character map for this output definition. Populated by the
    /// stylesheet when the output properties are prepared for serialization. Keys are
    /// Unicode codepoints.
    /// </summary>
    public Dictionary<int, string>? CharacterMap { get; set; }

    // Internal flags tracking which properties were explicitly set on the parsed xsl:output element.
    internal bool MethodSpecified { get; set; }

    /// <summary>
    /// The effective XSLT version of the principal stylesheet module, used for the
    /// backwards-compatibility rule in default output-method inference.
    /// </summary>
    internal string? EffectiveVersion { get; set; }

    /// <summary>
    /// True when the result tree being serialized was generated implicitly rather than
    /// by an explicit <c>xsl:result-document</c> instruction.
    /// </summary>
    internal bool ImplicitResultTree { get; set; }
    internal bool OmitXmlDeclarationSpecified { get; set; }
    internal bool IndentSpecified { get; set; }
    internal bool EncodingSpecified { get; set; }
    internal bool VersionSpecified { get; set; }
    internal bool HtmlVersionSpecified { get; set; }
    internal bool StandaloneSpecified { get; set; }
    internal bool UndeclarePrefixesSpecified { get; set; }
    internal bool NormalizationFormSpecified { get; set; }
    internal bool DoctypeSystemSpecified { get; set; }
    internal bool DoctypePublicSpecified { get; set; }
    internal bool CdataSectionElementsSpecified { get; set; }
    internal bool SuppressIndentationSpecified { get; set; }
    internal bool EscapeUriAttributesSpecified { get; set; }
    internal bool IncludeContentTypeSpecified { get; set; }
    internal bool MediaTypeSpecified { get; set; }
    internal bool ByteOrderMarkSpecified { get; set; }
    internal bool UseCharacterMapsSpecified { get; set; }
    internal bool JsonNodeOutputMethodSpecified { get; set; }
    internal bool AllowDuplicateNamesSpecified { get; set; }
    internal bool EscapeSolidusSpecified { get; set; }
    internal bool ItemSeparatorSpecified { get; set; }
    internal bool BuildTreeSpecified { get; set; }
    internal bool CharacterMapSpecified { get; set; }

    /// <summary>Parses an xsl:output element into <see cref="OutputProperties"/>.</summary>
    public static OutputProperties FromElement(XElement element)
    {
        var props = new OutputProperties();

        var method = element.Attribute("method")?.Value;
        if (!string.IsNullOrEmpty(method))
        {
            props.Method = method.Trim();
            props.MethodSpecified = true;
        }

        var omit = element.Attribute("omit-xml-declaration")?.Value;
        if (!string.IsNullOrEmpty(omit))
        {
            props.OmitXmlDeclaration = ParseYesNo(omit, defaultValue: true);
            props.OmitXmlDeclarationSpecified = true;
        }

        var indent = element.Attribute("indent")?.Value;
        if (!string.IsNullOrEmpty(indent))
        {
            props.Indent = ParseYesNo(indent, defaultValue: false);
            props.IndentSpecified = true;
        }

        var encoding = element.Attribute("encoding")?.Value;
        if (!string.IsNullOrEmpty(encoding))
        {
            props.Encoding = encoding;
            props.EncodingSpecified = true;
        }

        var version = element.Attribute("version")?.Value;
        if (!string.IsNullOrEmpty(version))
        {
            props.Version = version;
            props.VersionSpecified = true;
        }

        var outputVersion = element.Attribute("output-version")?.Value;
        if (!string.IsNullOrEmpty(outputVersion))
        {
            props.Version = outputVersion;
            props.VersionSpecified = true;
        }

        var htmlVersion = element.Attribute("html-version")?.Value;
        if (!string.IsNullOrEmpty(htmlVersion))
        {
            var trimmed = htmlVersion.Trim();
            if (!decimal.TryParse(trimmed, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
            {
                throw new InvalidOperationException(
                    $"XTSE0020: Invalid value for html-version attribute: '{htmlVersion}'.");
            }

            // Canonicalize the most common values; otherwise keep the parsed decimal form.
            var canonical = parsed switch
            {
                1.0m => "1.0",
                1.1m => "1.1",
                4.0m => "4.0",
                5.0m => "5.0",
                _ => parsed.ToString(CultureInfo.InvariantCulture)
            };

            // For xhtml, only 1.0 and 5.0 are defined by the serialization spec.
            var effectiveMethod = props.Method;
            if (effectiveMethod == "xhtml" && parsed != 1.0m && parsed != 5.0m)
            {
                throw new InvalidOperationException(
                    $"XTSE0020: Invalid value for html-version attribute: '{htmlVersion}'.");
            }

            props.HtmlVersion = canonical;
            props.HtmlVersionSpecified = true;
        }

        var standalone = element.Attribute("standalone")?.Value;
        if (!string.IsNullOrEmpty(standalone))
        {
            var trimmed = standalone.Trim();
            var normalized = trimmed switch
            {
                "yes" or "true" or "1" => "yes",
                "no" or "false" or "0" => "no",
                "omit" => "omit",
                _ => throw new InvalidOperationException($"XTSE0020: Invalid value '{trimmed}' for standalone attribute.")
            };
            props.Standalone = normalized;
            props.StandaloneSpecified = true;
        }

        var undeclare = element.Attribute("undeclare-prefixes")?.Value;
        if (!string.IsNullOrEmpty(undeclare))
        {
            props.UndeclarePrefixes = ParseYesNo(undeclare, defaultValue: false);
            props.UndeclarePrefixesSpecified = true;
        }

        var normalizationForm = element.Attribute("normalization-form")?.Value;
        if (!string.IsNullOrEmpty(normalizationForm))
        {
            props.NormalizationForm = normalizationForm;
            props.NormalizationFormSpecified = true;
        }

        // An explicitly empty doctype-public/doctype-system overrides any inherited value.
        var doctypeSystemAttr = element.Attribute("doctype-system");
        if (doctypeSystemAttr != null)
        {
            props.DoctypeSystem = doctypeSystemAttr.Value;
            props.DoctypeSystemSpecified = true;
        }

        var doctypePublicAttr = element.Attribute("doctype-public");
        if (doctypePublicAttr != null)
        {
            props.DoctypePublic = doctypePublicAttr.Value;
            props.DoctypePublicSpecified = true;
        }

        var cdataSectionElements = element.Attribute("cdata-section-elements")?.Value;
        if (!string.IsNullOrEmpty(cdataSectionElements))
        {
            props.CdataSectionElements = ParseQNameList(element, cdataSectionElements);
            props.CdataSectionElementsSpecified = true;
        }

        var suppressIndentation = element.Attribute("suppress-indentation")?.Value;
        if (!string.IsNullOrEmpty(suppressIndentation))
        {
            props.SuppressIndentation = ParseQNameList(element, suppressIndentation);
            props.SuppressIndentationSpecified = true;
        }

        var escapeUri = element.Attribute("escape-uri-attributes")?.Value;
        if (!string.IsNullOrEmpty(escapeUri))
        {
            props.EscapeUriAttributes = ParseYesNo(escapeUri, defaultValue: false);
            props.EscapeUriAttributesSpecified = true;
        }

        var includeContentType = element.Attribute("include-content-type")?.Value;
        if (!string.IsNullOrEmpty(includeContentType))
        {
            props.IncludeContentType = ParseYesNo(includeContentType, defaultValue: true);
            props.IncludeContentTypeSpecified = true;
        }

        var mediaType = element.Attribute("media-type")?.Value;
        if (!string.IsNullOrEmpty(mediaType))
        {
            props.MediaType = mediaType;
            props.MediaTypeSpecified = true;
        }

        var byteOrderMark = element.Attribute("byte-order-mark")?.Value;
        if (!string.IsNullOrEmpty(byteOrderMark))
        {
            props.ByteOrderMark = ParseYesNo(byteOrderMark, defaultValue: false);
            props.ByteOrderMarkSpecified = true;
        }

        var jsonNodeOutputMethod = element.Attribute("json-node-output-method")?.Value;
        if (!string.IsNullOrEmpty(jsonNodeOutputMethod))
        {
            props.JsonNodeOutputMethod = jsonNodeOutputMethod.Trim();
            props.JsonNodeOutputMethodSpecified = true;
        }

        var allowDuplicateNames = element.Attribute("allow-duplicate-names")?.Value;
        if (!string.IsNullOrEmpty(allowDuplicateNames))
        {
            props.AllowDuplicateNames = ParseYesNo(allowDuplicateNames, defaultValue: false);
            props.AllowDuplicateNamesSpecified = true;
        }

        var escapeSolidus = element.Attribute("escape-solidus")?.Value;
        if (!string.IsNullOrEmpty(escapeSolidus))
        {
            props.EscapeSolidus = ParseYesNo(escapeSolidus, defaultValue: false);
            props.EscapeSolidusSpecified = true;
        }

        var itemSeparator = element.Attribute("item-separator")?.Value;
        if (itemSeparator != null)
        {
            // The special value #absent is preserved so serialization can apply the
            // method-specific default rules for absent item separators.
            props.ItemSeparator = itemSeparator;
            props.ItemSeparatorSpecified = true;
        }

        var buildTree = element.Attribute("build-tree")?.Value;
        if (!string.IsNullOrEmpty(buildTree))
        {
            props.BuildTree = ParseYesNo(buildTree, defaultValue: true);
            props.BuildTreeSpecified = true;
        }

        var useCharacterMaps = element.Attribute("use-character-maps")?.Value;
        if (!string.IsNullOrEmpty(useCharacterMaps))
        {
            // Character-map names are QNames, but unprefixed names are in no namespace
            // (they name stylesheet objects, not result elements).
            props.UseCharacterMaps = ParseCharacterMapList(element, useCharacterMaps);
            props.UseCharacterMapsSpecified = true;
        }

        // The default for omit-xml-declaration depends on the output method and is
        // applied by ResultTreeSerializer.ApplyMethodDefaults. This keeps parsed
        // declarations independent so that named output definitions can be merged
        // across import precedence without leaking method-specific defaults.

        ValidateDoctypePublic(props);

        return props;
    }

    /// <summary>
    /// Parses a map of serialization parameters (option-parameter conventions) into
    /// <see cref="OutputProperties"/>. Keys are string-valued serialization parameter
    /// names; values are the corresponding XDM values.
    /// </summary>
    public static OutputProperties FromMap(XdmMap map)
    {
        var props = new OutputProperties();

        foreach (var kvp in map.Entries)
        {
            string name;
            if (kvp.Key.Kind == XdmValueKind.String)
            {
                name = kvp.Key.StringValue;
            }
            else if (kvp.Key.Kind == XdmValueKind.QName)
            {
                // QName keys identify implementation-defined parameters; ignore.
                continue;
            }
            else
            {
                throw new InvalidOperationException(
                    $"XPTY0004: fn:transform serialization-params keys must be xs:string or xs:QName, got {kvp.Key.Kind}.");
            }

            ApplyMapParameter(props, name, kvp.Value);
        }

        return props;
    }

    private static void ApplyMapParameter(OutputProperties props, string name, XdmValue rawValue)
    {
        // Option parameter conventions: an empty sequence leaves the parameter at its default.
        if (rawValue.IsUndefined)
            return;
        if (rawValue.IsSequence && rawValue.SequenceValue is not null)
        {
            bool any = false;
            foreach (var _ in XdmSequence.FromSource(rawValue.SequenceValue))
            {
                any = true;
                break;
            }
            if (!any)
                return;
        }

        switch (name)
        {
            case "method":
                props.Method = AsMethodName(name, rawValue);
                props.MethodSpecified = true;
                break;
            case "indent":
                props.Indent = AsBoolean(name, rawValue);
                props.IndentSpecified = true;
                break;
            case "omit-xml-declaration":
                props.OmitXmlDeclaration = AsBoolean(name, rawValue);
                props.OmitXmlDeclarationSpecified = true;
                break;
            case "standalone":
                props.Standalone = AsStandalone(rawValue);
                props.StandaloneSpecified = true;
                break;
            case "item-separator":
                props.ItemSeparator = AsString(name, rawValue);
                props.ItemSeparatorSpecified = true;
                break;
            case "encoding":
                props.Encoding = AsString(name, rawValue);
                props.EncodingSpecified = true;
                break;
            case "version":
                props.Version = AsString(name, rawValue);
                props.VersionSpecified = true;
                break;
            case "media-type":
                props.MediaType = AsString(name, rawValue);
                props.MediaTypeSpecified = true;
                break;
            case "doctype-system":
                props.DoctypeSystem = AsString(name, rawValue);
                props.DoctypeSystemSpecified = true;
                break;
            case "doctype-public":
                props.DoctypePublic = AsString(name, rawValue);
                props.DoctypePublicSpecified = true;
                break;
            case "normalization-form":
                props.NormalizationForm = AsString(name, rawValue);
                props.NormalizationFormSpecified = true;
                break;
            case "json-node-output-method":
                props.JsonNodeOutputMethod = AsString(name, rawValue).ToLowerInvariant();
                props.JsonNodeOutputMethodSpecified = true;
                break;
            case "html-version":
                {
                    var d = AsDecimal(name, rawValue);
                    props.HtmlVersion = d switch
                    {
                        1.0m => "1.0",
                        1.1m => "1.1",
                        4.0m => "4.0",
                        5.0m => "5.0",
                        _ => d.ToString(CultureInfo.InvariantCulture)
                    };
                    props.HtmlVersionSpecified = true;
                }
                break;
            case "allow-duplicate-names":
                props.AllowDuplicateNames = AsBoolean(name, rawValue);
                props.AllowDuplicateNamesSpecified = true;
                break;
            case "escape-solidus":
                props.EscapeSolidus = AsBoolean(name, rawValue);
                props.EscapeSolidusSpecified = true;
                break;
            case "escape-uri-attributes":
                props.EscapeUriAttributes = AsBoolean(name, rawValue);
                props.EscapeUriAttributesSpecified = true;
                break;
            case "include-content-type":
                props.IncludeContentType = AsBoolean(name, rawValue);
                props.IncludeContentTypeSpecified = true;
                break;
            case "undeclare-prefixes":
                props.UndeclarePrefixes = AsBoolean(name, rawValue);
                props.UndeclarePrefixesSpecified = true;
                break;
            case "byte-order-mark":
                props.ByteOrderMark = AsBoolean(name, rawValue);
                props.ByteOrderMarkSpecified = true;
                break;
            case "use-character-maps":
                props.CharacterMap = AsCharacterMap(rawValue);
                props.CharacterMapSpecified = true;
                break;
            case "cdata-section-elements":
                props.CdataSectionElements = AsQNameSet(name, rawValue);
                props.CdataSectionElementsSpecified = true;
                break;
            case "suppress-indentation":
                props.SuppressIndentation = AsQNameSet(name, rawValue);
                props.SuppressIndentationSpecified = true;
                break;
            case "build-tree":
                props.BuildTree = AsBoolean(name, rawValue);
                props.BuildTreeSpecified = true;
                break;
            default:
                // Unrecognized string keys are ignored (option parameter conventions).
                break;
        }

        ValidateDoctypePublic(props);
    }

    /// <summary>
    /// Parses an <c>output:serialization-parameters</c> document into
    /// <see cref="OutputProperties"/>. Inline character maps defined by
    /// <c>output:character-map</c> children are stored directly in
    /// <see cref="CharacterMap"/>.
    /// </summary>
    public static OutputProperties FromSerializationParameters(XDocument document)
    {
        var props = new OutputProperties();
        var root = document.Root;
        if (root == null)
            return props;

        foreach (var child in root.Elements())
        {
            var localName = child.Name.LocalName;
            if (localName == "use-character-maps")
            {
                var map = new Dictionary<int, string>();
                foreach (var cm in child.Elements())
                {
                    if (cm.Name.LocalName != "character-map")
                        continue;
                    var charAttr = cm.Attribute("character")?.Value;
                    var mapString = cm.Attribute("map-string")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(charAttr))
                    {
                        var cp = ParseCharacterMapCodepoint(charAttr);
                        map[cp] = mapString;
                    }
                }
                if (map.Count > 0)
                {
                    props.CharacterMap = map;
                    props.CharacterMapSpecified = true;
                }
                continue;
            }

            var value = child.Attribute("value")?.Value;
            if (string.IsNullOrEmpty(value))
                continue;

            // Reuse the xsl:output attribute parser by wrapping the parameter as an attribute.
            var wrapper = new XElement(XName.Get("output", XslNamespace.NamespaceName), new XAttribute(localName, value));
            var parsed = FromElement(wrapper);

            // Transfer only the property that was just set.
            switch (localName)
            {
                case "method":
                    props.Method = parsed.Method;
                    props.MethodSpecified = true;
                    break;
                case "omit-xml-declaration":
                    props.OmitXmlDeclaration = parsed.OmitXmlDeclaration;
                    props.OmitXmlDeclarationSpecified = true;
                    break;
                case "indent":
                    props.Indent = parsed.Indent;
                    props.IndentSpecified = true;
                    break;
                case "encoding":
                    props.Encoding = parsed.Encoding;
                    props.EncodingSpecified = true;
                    break;
                case "version":
                case "output-version":
                    props.Version = parsed.Version;
                    props.VersionSpecified = true;
                    break;
                case "html-version":
                    props.HtmlVersion = parsed.HtmlVersion;
                    props.HtmlVersionSpecified = true;
                    break;
                case "standalone":
                    props.Standalone = parsed.Standalone;
                    props.StandaloneSpecified = true;
                    break;
                case "undeclare-prefixes":
                    props.UndeclarePrefixes = parsed.UndeclarePrefixes;
                    props.UndeclarePrefixesSpecified = true;
                    break;
                case "normalization-form":
                    props.NormalizationForm = parsed.NormalizationForm;
                    props.NormalizationFormSpecified = true;
                    break;
                case "doctype-system":
                    props.DoctypeSystem = parsed.DoctypeSystem;
                    props.DoctypeSystemSpecified = true;
                    break;
                case "doctype-public":
                    props.DoctypePublic = parsed.DoctypePublic;
                    props.DoctypePublicSpecified = true;
                    break;
                case "cdata-section-elements":
                    props.CdataSectionElements = parsed.CdataSectionElements;
                    props.CdataSectionElementsSpecified = true;
                    break;
                case "suppress-indentation":
                    props.SuppressIndentation = parsed.SuppressIndentation;
                    props.SuppressIndentationSpecified = true;
                    break;
                case "escape-uri-attributes":
                    props.EscapeUriAttributes = parsed.EscapeUriAttributes;
                    props.EscapeUriAttributesSpecified = true;
                    break;
                case "include-content-type":
                    props.IncludeContentType = parsed.IncludeContentType;
                    props.IncludeContentTypeSpecified = true;
                    break;
                case "media-type":
                    props.MediaType = parsed.MediaType;
                    props.MediaTypeSpecified = true;
                    break;
                case "byte-order-mark":
                    props.ByteOrderMark = parsed.ByteOrderMark;
                    props.ByteOrderMarkSpecified = true;
                    break;
                case "json-node-output-method":
                    props.JsonNodeOutputMethod = parsed.JsonNodeOutputMethod;
                    props.JsonNodeOutputMethodSpecified = true;
                    break;
                case "allow-duplicate-names":
                    props.AllowDuplicateNames = parsed.AllowDuplicateNames;
                    props.AllowDuplicateNamesSpecified = true;
                    break;
                case "escape-solidus":
                    props.EscapeSolidus = parsed.EscapeSolidus;
                    props.EscapeSolidusSpecified = true;
                    break;
                case "item-separator":
                    props.ItemSeparator = parsed.ItemSeparator;
                    props.ItemSeparatorSpecified = true;
                    break;
                case "build-tree":
                    props.BuildTree = parsed.BuildTree;
                    props.BuildTreeSpecified = true;
                    break;
                case "use-character-maps":
                    props.UseCharacterMaps = parsed.UseCharacterMaps;
                    props.UseCharacterMapsSpecified = true;
                    break;
            }
        }

        return props;
    }

    private static readonly XNamespace XslNamespace = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>
    /// Validates that a public identifier contains only pubid characters.
    /// Raises XTSE0020 for invalid public identifiers.
    /// </summary>
    private static void ValidateDoctypePublic(OutputProperties props)
    {
        if (string.IsNullOrEmpty(props.DoctypePublic))
            return;

        foreach (var ch in props.DoctypePublic)
        {
            if (!IsPubidChar(ch))
            {
                throw new InvalidOperationException(
                    $"XTSE0020: Invalid character '{ch}' in doctype-public value.");
            }
        }
    }

    private static bool IsPubidChar(char ch)
    {
        if (ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9')
            return true;
        return ch is ' ' or '\r' or '\n' or '-' or '\'' or '(' or ')' or '+' or ',' or '.' or
            '/' or ':' or '=' or '?' or ';' or '!' or '*' or '#' or '@' or '$' or '_' or '%';
    }

    /// <summary>
    /// Merges explicitly specified properties from <paramref name="source"/> into
    /// <paramref name="target"/>, leaving unspecified properties unchanged.
    /// </summary>
    public static void Merge(OutputProperties target, OutputProperties source)
    {
        if (source.MethodSpecified) { target.Method = source.Method; target.MethodSpecified = true; }
        if (source.OmitXmlDeclarationSpecified) { target.OmitXmlDeclaration = source.OmitXmlDeclaration; target.OmitXmlDeclarationSpecified = true; }
        if (source.IndentSpecified) { target.Indent = source.Indent; target.IndentSpecified = true; }
        if (source.EncodingSpecified) { target.Encoding = source.Encoding; target.EncodingSpecified = true; }
        if (source.VersionSpecified) { target.Version = source.Version; target.VersionSpecified = true; }
        if (source.HtmlVersionSpecified) { target.HtmlVersion = source.HtmlVersion; target.HtmlVersionSpecified = true; }
        if (source.StandaloneSpecified) { target.Standalone = source.Standalone; target.StandaloneSpecified = true; }
        if (source.UndeclarePrefixesSpecified) { target.UndeclarePrefixes = source.UndeclarePrefixes; target.UndeclarePrefixesSpecified = true; }
        if (source.NormalizationFormSpecified) { target.NormalizationForm = source.NormalizationForm; target.NormalizationFormSpecified = true; }
        if (source.DoctypeSystemSpecified) { target.DoctypeSystem = source.DoctypeSystem; target.DoctypeSystemSpecified = true; }
        if (source.DoctypePublicSpecified) { target.DoctypePublic = source.DoctypePublic; target.DoctypePublicSpecified = true; }
        if (source.CdataSectionElementsSpecified)
        {
            var merged = new List<XsQName>(target.CdataSectionElements);
            foreach (var q in source.CdataSectionElements)
                if (!merged.Any(existing => existing.LocalName == q.LocalName && existing.NamespaceUri == q.NamespaceUri))
                    merged.Add(q);
            target.CdataSectionElements = merged;
            target.CdataSectionElementsSpecified = true;
        }
        if (source.SuppressIndentationSpecified)
        {
            var merged = new List<XsQName>(target.SuppressIndentation);
            foreach (var q in source.SuppressIndentation)
                if (!merged.Any(existing => existing.LocalName == q.LocalName && existing.NamespaceUri == q.NamespaceUri))
                    merged.Add(q);
            target.SuppressIndentation = merged;
            target.SuppressIndentationSpecified = true;
        }
        if (source.EscapeUriAttributesSpecified) { target.EscapeUriAttributes = source.EscapeUriAttributes; target.EscapeUriAttributesSpecified = true; }
        if (source.IncludeContentTypeSpecified) { target.IncludeContentType = source.IncludeContentType; target.IncludeContentTypeSpecified = true; }
        if (source.MediaTypeSpecified) { target.MediaType = source.MediaType; target.MediaTypeSpecified = true; }
        if (source.ByteOrderMarkSpecified) { target.ByteOrderMark = source.ByteOrderMark; target.ByteOrderMarkSpecified = true; }
        if (source.JsonNodeOutputMethodSpecified) { target.JsonNodeOutputMethod = source.JsonNodeOutputMethod; target.JsonNodeOutputMethodSpecified = true; }
        if (source.AllowDuplicateNamesSpecified) { target.AllowDuplicateNames = source.AllowDuplicateNames; target.AllowDuplicateNamesSpecified = true; }
        if (source.EscapeSolidusSpecified) { target.EscapeSolidus = source.EscapeSolidus; target.EscapeSolidusSpecified = true; }
        if (source.ItemSeparatorSpecified) { target.ItemSeparator = source.ItemSeparator; target.ItemSeparatorSpecified = true; }
        if (source.BuildTreeSpecified) { target.BuildTree = source.BuildTree; target.BuildTreeSpecified = true; }
        if (source.UseCharacterMapsSpecified)
        {
            // Later xsl:output declarations and xsl:result-document instruction-level
            // references take precedence over earlier ones. Append the source list so that
            // the last occurrence of a character-map name wins during resolution.
            var merged = new List<XsQName>(target.UseCharacterMaps);
            foreach (var q in source.UseCharacterMaps)
                if (!merged.Any(existing => existing.LocalName == q.LocalName && existing.NamespaceUri == q.NamespaceUri))
                    merged.Add(q);
            target.UseCharacterMaps = merged;
            target.UseCharacterMapsSpecified = true;
        }

        if (source.CharacterMapSpecified && source.CharacterMap != null)
        {
            var merged = target.CharacterMap != null ? new Dictionary<int, string>(target.CharacterMap) : new Dictionary<int, string>();
            foreach (var kvp in source.CharacterMap)
                merged[kvp.Key] = kvp.Value;
            target.CharacterMap = merged;
            target.CharacterMapSpecified = true;
        }
    }

    /// <summary>
    /// Creates a shallow copy of these output properties.
    /// </summary>
    public OutputProperties Clone()
    {
        var clone = new OutputProperties
        {
            Method = Method,
            MethodSpecified = MethodSpecified,
            OmitXmlDeclaration = OmitXmlDeclaration,
            OmitXmlDeclarationSpecified = OmitXmlDeclarationSpecified,
            Indent = Indent,
            IndentSpecified = IndentSpecified,
            Encoding = Encoding,
            EncodingSpecified = EncodingSpecified,
            Version = Version,
            VersionSpecified = VersionSpecified,
            HtmlVersion = HtmlVersion,
            HtmlVersionSpecified = HtmlVersionSpecified,
            Standalone = Standalone,
            StandaloneSpecified = StandaloneSpecified,
            UndeclarePrefixes = UndeclarePrefixes,
            UndeclarePrefixesSpecified = UndeclarePrefixesSpecified,
            NormalizationForm = NormalizationForm,
            NormalizationFormSpecified = NormalizationFormSpecified,
            DoctypeSystem = DoctypeSystem,
            DoctypeSystemSpecified = DoctypeSystemSpecified,
            DoctypePublic = DoctypePublic,
            DoctypePublicSpecified = DoctypePublicSpecified,
            CdataSectionElements = CdataSectionElements,
            CdataSectionElementsSpecified = CdataSectionElementsSpecified,
            SuppressIndentation = SuppressIndentation,
            SuppressIndentationSpecified = SuppressIndentationSpecified,
            EscapeUriAttributes = EscapeUriAttributes,
            EscapeUriAttributesSpecified = EscapeUriAttributesSpecified,
            IncludeContentType = IncludeContentType,
            IncludeContentTypeSpecified = IncludeContentTypeSpecified,
            MediaType = MediaType,
            MediaTypeSpecified = MediaTypeSpecified,
            ByteOrderMark = ByteOrderMark,
            ByteOrderMarkSpecified = ByteOrderMarkSpecified,
            JsonNodeOutputMethod = JsonNodeOutputMethod,
            JsonNodeOutputMethodSpecified = JsonNodeOutputMethodSpecified,
            AllowDuplicateNames = AllowDuplicateNames,
            AllowDuplicateNamesSpecified = AllowDuplicateNamesSpecified,
            EscapeSolidus = EscapeSolidus,
            EscapeSolidusSpecified = EscapeSolidusSpecified,
            ItemSeparator = ItemSeparator,
            ItemSeparatorSpecified = ItemSeparatorSpecified,
            BuildTree = BuildTree,
            BuildTreeSpecified = BuildTreeSpecified,
            UseCharacterMaps = UseCharacterMaps,
            UseCharacterMapsSpecified = UseCharacterMapsSpecified,
            CharacterMap = CharacterMap,
            CharacterMapSpecified = CharacterMapSpecified,
            EffectiveVersion = EffectiveVersion,
            ImplicitResultTree = ImplicitResultTree
        };
        return clone;
    }

    private static int ParseCharacterMapCodepoint(string value)
    {
        if (value.Length == 0)
            throw new InvalidOperationException("XTSE0010: character-map character attribute is empty.");
        if (value.Length == 1)
            return value[0];
        if (value.Length == 2 && char.IsHighSurrogate(value[0]) && char.IsLowSurrogate(value[1]))
            return char.ConvertToUtf32(value[0], value[1]);
        if (System.Text.Rune.TryGetRuneAt(value, 0, out var rune))
            return rune.Value;
        throw new InvalidOperationException("XTSE0010: character-map character attribute is not a single character.");
    }

    private static bool ParseYesNo(string value, bool defaultValue)
    {
        var trimmed = value.Trim();
        return trimmed switch
        {
            "yes" or "true" or "1" => true,
            "no" or "false" or "0" => false,
            _ => throw new InvalidOperationException(
                $"XTSE0020: Invalid value '{trimmed}' for yes/no attribute.")
        };
    }

    private static IReadOnlyList<XsQName> ParseQNameList(XElement context, string value)
    {
        var list = new List<XsQName>();
        foreach (var token in value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var name = token.Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            string localName;
            string namespaceUri;
            string prefix;

            if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
            {
                int closeBrace = name.IndexOf('}');
                if (closeBrace >= 2)
                {
                    namespaceUri = name[2..closeBrace];
                    localName = name[(closeBrace + 1)..];
                    prefix = "";
                    list.Add(new XsQName(localName, namespaceUri, prefix));
                    continue;
                }
            }

            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                prefix = name[..colon];
                localName = name[(colon + 1)..];
                var resolvedNs = context.GetNamespaceOfPrefix(prefix);
                namespaceUri = resolvedNs?.NamespaceName ?? "";
            }
            else
            {
                prefix = "";
                localName = name;
                namespaceUri = context.GetDefaultNamespace()?.NamespaceName ?? "";
            }

            list.Add(new XsQName(localName, namespaceUri, prefix));
        }

        return list;
    }

    /// <summary>
    /// Parses a space-separated list of character-map names. Prefixed names are resolved
    /// using in-scope namespace declarations; unprefixed names are always in no namespace.
    /// </summary>
    private static IReadOnlyList<XsQName> ParseCharacterMapList(XElement context, string value)
    {
        var list = new List<XsQName>();
        foreach (var token in value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var name = token.Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            string localName;
            string namespaceUri;
            string prefix;

            if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
            {
                int closeBrace = name.IndexOf('}');
                if (closeBrace >= 2)
                {
                    namespaceUri = name[2..closeBrace];
                    localName = name[(closeBrace + 1)..];
                    prefix = "";
                    list.Add(new XsQName(localName, namespaceUri, prefix));
                    continue;
                }
            }

            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                prefix = name[..colon];
                localName = name[(colon + 1)..];
                var resolvedNs = context.GetNamespaceOfPrefix(prefix);
                namespaceUri = resolvedNs?.NamespaceName ?? "";
            }
            else
            {
                prefix = "";
                localName = name;
                namespaceUri = "";
            }

            list.Add(new XsQName(localName, namespaceUri, prefix));
        }

        return list;
    }

    // ------------------------------------------------------------------
    // Map serialization parameter helpers (fn:transform serialization-params)
    // ------------------------------------------------------------------

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
            $"XPTY0004: Serialization parameter '{name}' requires xs:boolean, got {value.Kind}.");
    }

    private static string AsStandalone(XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.Boolean)
            return value.BooleanValue ? "yes" : "no";
        if (IsUntypedAtomic(value))
        {
            return value.StringValue.Trim() switch
            {
                "true" or "1" => "yes",
                "false" or "0" => "no",
                "omit" => "omit",
                _ => throw new InvalidOperationException(
                    $"XPTY0004: Cannot convert xs:untypedAtomic('{value.StringValue}') for serialization parameter 'standalone'.")
            };
        }
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter 'standalone' requires xs:boolean or 'omit', got {value.Kind}.");
    }

    private static string AsString(string name, XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.String)
            return value.StringValue;
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter '{name}' requires xs:string, got {value.Kind}.");
    }

    private static string AsMethodName(string name, XdmValue rawValue)
    {
        var value = AtomizeOptionValue(rawValue);
        if (value.Kind == XdmValueKind.String)
            return value.StringValue.ToLowerInvariant();
        if (value.Kind == XdmValueKind.QName)
            return value.QNameValue.LocalName.ToLowerInvariant();
        throw new InvalidOperationException(
            $"XPTY0004: Serialization parameter '{name}' requires xs:string, got {value.Kind}.");
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
                    $"XPTY0004: Serialization parameter '{name}' requires a numeric value, got {value.Kind}.");
        }
    }

    private static IReadOnlyList<XsQName> AsQNameSet(string name, XdmValue rawValue)
    {
        var list = new List<XsQName>();
        foreach (var item in ToItemList(rawValue))
        {
            if (item.IsArray)
            {
                var arr = item.ArrayValue;
                for (int i = 1; i <= arr.Count; i++)
                    AddQName(arr.Get(i));
                continue;
            }
            AddQName(item);
        }
        return list;

        void AddQName(XdmValue item)
        {
            var value = AtomizeOptionValue(item);
            if (value.Kind != XdmValueKind.QName)
                throw new InvalidOperationException(
                    $"XPTY0004: Serialization parameter '{name}' requires xs:QName values, got {value.Kind}.");
            var q = value.QNameValue;
            list.Add(new XsQName(q.LocalName, q.NamespaceUri, ""));
        }
    }

    private static Dictionary<int, string> AsCharacterMap(XdmValue rawValue)
    {
        if (!rawValue.IsMap)
            throw new InvalidOperationException(
                $"XPTY0004: Serialization parameter 'use-character-maps' requires a map, got {rawValue.Kind}.");
        var result = new Dictionary<int, string>();
        foreach (var kvp in rawValue.MapValue.Entries)
        {
            var keyValue = AtomizeOptionValue(kvp.Key);
            if (keyValue.Kind != XdmValueKind.String || IsUntypedAtomic(keyValue))
                throw new InvalidOperationException(
                    $"XPTY0004: fn:transform use-character-maps keys must be xs:string, got {keyValue.Kind}.");
            string key = keyValue.StringValue;
            if (key.Length != 1)
                throw new InvalidOperationException(
                    $"SEPM0016: fn:transform use-character-maps key '{key}' must be a single character.");

            var valueValue = AtomizeOptionValue(kvp.Value);
            if (valueValue.Kind != XdmValueKind.String || IsUntypedAtomic(valueValue))
                throw new InvalidOperationException(
                    $"XPTY0004: fn:transform use-character-maps values must be xs:string, got {valueValue.Kind}.");
            result[key[0]] = valueValue.StringValue;
        }
        return result;
    }
}
