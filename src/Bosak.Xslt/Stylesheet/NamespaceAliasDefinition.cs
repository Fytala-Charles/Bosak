// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Represents a parsed xsl:namespace-alias declaration.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a parsed <c>xsl:namespace-alias</c> declaration.
/// </summary>
public sealed class NamespaceAliasDefinition
{
    /// <summary>The source prefix (empty string for <c>#default</c>).</summary>
    public string SourcePrefix { get; init; } = "";

    /// <summary>The result prefix (empty string for <c>#default</c>).</summary>
    public string ResultPrefix { get; init; } = "";

    /// <summary>The namespace URI associated with <see cref="SourcePrefix"/> in the declaration's context.</summary>
    public string SourceUri { get; init; } = "";

    /// <summary>The namespace URI associated with <see cref="ResultPrefix"/> in the declaration's context.</summary>
    public string ResultUri { get; init; } = "";

    /// <summary>The import precedence of the declaring stylesheet module (lower value = higher precedence).</summary>
    public int ImportPrecedence { get; init; }

    /// <summary>The original declaration element, used for diagnostics.</summary>
    public XElement Element { get; init; } = null!;

    /// <summary>
    /// Parses an <c>xsl:namespace-alias</c> element in the context of the given stylesheet module.
    /// </summary>
    public static NamespaceAliasDefinition FromElement(XElement element, Stylesheet stylesheet)
    {
        var stylePrefix = element.Attribute("stylesheet-prefix")?.Value ?? "";
        var resultPrefix = element.Attribute("result-prefix")?.Value ?? "";

        if (string.IsNullOrEmpty(stylePrefix) || string.IsNullOrEmpty(resultPrefix))
            throw new InvalidOperationException("XTSE0010: xsl:namespace-alias requires both stylesheet-prefix and result-prefix attributes.");

        string sourceUri;
        if (stylePrefix == "#default")
            sourceUri = element.GetDefaultNamespace()?.NamespaceName ?? "";
        else
            sourceUri = element.GetNamespaceOfPrefix(stylePrefix)?.NamespaceName
                ?? throw new InvalidOperationException($"XTSE0010: Namespace prefix '{stylePrefix}' is not declared for xsl:namespace-alias.");

        string resultUri;
        if (resultPrefix == "#default")
            resultUri = element.GetDefaultNamespace()?.NamespaceName ?? "";
        else
            resultUri = element.GetNamespaceOfPrefix(resultPrefix)?.NamespaceName
                ?? throw new InvalidOperationException($"XTSE0010: Namespace prefix '{resultPrefix}' is not declared for xsl:namespace-alias.");

        if (sourceUri == resultUri)
            throw new InvalidOperationException("XTSE0010: xsl:namespace-alias stylesheet-prefix and result-prefix identify the same namespace URI.");

        return new NamespaceAliasDefinition
        {
            SourcePrefix = stylePrefix,
            ResultPrefix = resultPrefix,
            SourceUri = sourceUri,
            ResultUri = resultUri,
            ImportPrecedence = stylesheet.ImportPrecedence,
            Element = element
        };
    }
}
