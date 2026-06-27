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

    /// <summary>Parses an xsl:output element into <see cref="OutputProperties"/>.</summary>
    public static OutputProperties FromElement(System.Xml.Linq.XElement element)
    {
        var props = new OutputProperties();

        var method = element.Attribute("method")?.Value;
        if (!string.IsNullOrEmpty(method))
            props.Method = method;

        var omit = element.Attribute("omit-xml-declaration")?.Value;
        if (!string.IsNullOrEmpty(omit))
            props.OmitXmlDeclaration = ParseYesNo(omit, defaultValue: true);

        var indent = element.Attribute("indent")?.Value;
        if (!string.IsNullOrEmpty(indent))
            props.Indent = ParseYesNo(indent, defaultValue: false);

        var encoding = element.Attribute("encoding")?.Value;
        if (!string.IsNullOrEmpty(encoding))
            props.Encoding = encoding;

        var version = element.Attribute("version")?.Value;
        if (!string.IsNullOrEmpty(version))
            props.Version = version;

        var standalone = element.Attribute("standalone")?.Value;
        if (!string.IsNullOrEmpty(standalone))
            props.Standalone = standalone;

        var undeclare = element.Attribute("undeclare-prefixes")?.Value;
        if (!string.IsNullOrEmpty(undeclare))
            props.UndeclarePrefixes = ParseYesNo(undeclare, defaultValue: false);

        // Default for method="text": omit declaration
        if (props.Method == "text")
            props.OmitXmlDeclaration = true;

        return props;
    }

    private static bool ParseYesNo(string value, bool defaultValue)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "yes" => true,
            "no" => false,
            _ => defaultValue
        };
    }
}
