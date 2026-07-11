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

    /// <summary>
    /// Resolved effective character map for this output definition. Populated by the
    /// stylesheet when the output properties are prepared for serialization.
    /// </summary>
    public Dictionary<char, string>? CharacterMap { get; set; }

    // Internal flags tracking which properties were explicitly set on the parsed xsl:output element.
    internal bool MethodSpecified { get; set; }
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
            props.Standalone = standalone;
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
        if (source.UseCharacterMapsSpecified)
        {
            // Later xsl:output declarations and xsl:result-document instruction-level
            // references take precedence over earlier ones. Prepend the source list so
            // that the first occurrence of a character-map name wins during resolution.
            var merged = new List<XsQName>(source.UseCharacterMaps);
            foreach (var q in target.UseCharacterMaps)
                if (!merged.Any(existing => existing.LocalName == q.LocalName && existing.NamespaceUri == q.NamespaceUri))
                    merged.Add(q);
            target.UseCharacterMaps = merged;
            target.UseCharacterMapsSpecified = true;
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
            UseCharacterMaps = UseCharacterMaps,
            UseCharacterMapsSpecified = UseCharacterMapsSpecified,
            CharacterMap = CharacterMap
        };
        return clone;
    }

    private static bool ParseYesNo(string value, bool defaultValue)
    {
        var trimmed = value.Trim();
        return trimmed.ToLowerInvariant() switch
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
}
