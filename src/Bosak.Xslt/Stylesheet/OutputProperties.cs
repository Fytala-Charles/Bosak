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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Parsed properties from xsl:output that control result tree serialization.
/// </summary>
public sealed class OutputProperties
{
    /// <summary>Serialization method: "xml", "html", or "text".</summary>
    public string Method { get; set; } = "xml";

    /// <summary>Whether to omit the XML declaration.</summary>
    public bool OmitXmlDeclaration { get; set; } = true;

    /// <summary>Whether to indent the output.</summary>
    public bool Indent { get; set; } = false;

    /// <summary>Character encoding (informational; output is always UTF-16 string).</summary>
    public string Encoding { get; set; } = "UTF-8";

    /// <summary>XML version for the declaration (e.g. "1.0").</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>Standalone attribute for the XML declaration.</summary>
    public string? Standalone { get; set; }

    /// <summary>Whether prefixed namespace undeclarations are permitted (XML 1.1).</summary>
    public bool UndeclarePrefixes { get; set; } = false;

    /// <summary>Unicode normalization form applied during serialization: "NFC", "NFD", "NFKC", "NFKD", "fully-normalized", or "none".</summary>
    public string NormalizationForm { get; set; } = "none";

    // Internal flags tracking which properties were explicitly set on the parsed xsl:output element.
    internal bool MethodSpecified { get; set; }
    internal bool OmitXmlDeclarationSpecified { get; set; }
    internal bool IndentSpecified { get; set; }
    internal bool EncodingSpecified { get; set; }
    internal bool VersionSpecified { get; set; }
    internal bool StandaloneSpecified { get; set; }
    internal bool UndeclarePrefixesSpecified { get; set; }
    internal bool NormalizationFormSpecified { get; set; }

    /// <summary>Parses an xsl:output element into <see cref="OutputProperties"/>.</summary>
    public static OutputProperties FromElement(System.Xml.Linq.XElement element)
    {
        var props = new OutputProperties();

        var method = element.Attribute("method")?.Value;
        if (!string.IsNullOrEmpty(method))
        {
            props.Method = method;
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

        // XSLT default for omit-xml-declaration is "no" unless the serialization
        // method is "text". The property-object default is "yes" for ad-hoc
        // serialization that has no xsl:output instruction.
        if (props.Method == "text")
        {
            props.OmitXmlDeclaration = true;
            props.OmitXmlDeclarationSpecified = true;
        }
        else if (!props.OmitXmlDeclarationSpecified)
        {
            props.OmitXmlDeclaration = false;
            props.OmitXmlDeclarationSpecified = true;
        }

        return props;
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
        if (source.StandaloneSpecified) { target.Standalone = source.Standalone; target.StandaloneSpecified = true; }
        if (source.UndeclarePrefixesSpecified) { target.UndeclarePrefixes = source.UndeclarePrefixes; target.UndeclarePrefixesSpecified = true; }
        if (source.NormalizationFormSpecified) { target.NormalizationForm = source.NormalizationForm; target.NormalizationFormSpecified = true; }
    }

    private static bool ParseYesNo(string value, bool defaultValue)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "yes" or "true" or "1" => true,
            "no" or "false" or "0" => false,
            _ => defaultValue
        };
    }
}
