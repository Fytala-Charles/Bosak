// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 mei 2026
// PURPOSE              : Represents a parsed xsl:mode declaration with on-no-match behavior.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 05-06-2026     | Added OnMultipleMatch enum and parsing for xsl:mode on-multiple-match                  |
//                      | Charles Korthout | 0.3   | 07-06-2026     | Added DeepSkip to OnNoMatch enum and parsing; fixes next-match-034                     |
//                      | Charles Korthout | 0.4   | 08-06-2026     | FromElement expands mode name QNames to Clark notation                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Specifies the built-in template rule behavior when no explicit template matches a node.
/// </summary>
public enum OnNoMatch
{
    /// <summary>Shallow-copy the node.</summary>
    ShallowCopy,
    /// <summary>Skip the element node, apply-templates to children only.</summary>
    ShallowSkip,
    /// <summary>Copy only text nodes, ignore elements.</summary>
    TextOnlyCopy,
    /// <summary>Deep-copy the entire subtree.</summary>
    DeepCopy,
    /// <summary>Skip the element node and all descendants.</summary>
    DeepSkip,
    /// <summary>Throw an error when no template matches.</summary>
    Fail
}

/// <summary>
/// Specifies the behavior when multiple template rules match with the same priority.
/// </summary>
public enum OnMultipleMatch
{
    /// <summary>Use the last matching template (default in XSLT 3.0).</summary>
    UseLast,
    /// <summary>Throw an error when multiple templates have the same priority.</summary>
    Fail
}

/// <summary>
/// Represents a parsed xsl:mode declaration.
/// </summary>
public sealed class ModeDefinition
{
    /// <summary>The mode name (empty string for the default mode).</summary>
    public string Name { get; }

    /// <summary>The behavior when no template matches a node.</summary>
    public OnNoMatch OnNoMatch { get; }

    /// <summary>The behavior when multiple templates match with the same priority.</summary>
    public OnMultipleMatch OnMultipleMatch { get; }

    public ModeDefinition(string name, OnNoMatch onNoMatch, OnMultipleMatch onMultipleMatch = OnMultipleMatch.UseLast)
    {
        Name = name;
        OnNoMatch = onNoMatch;
        OnMultipleMatch = onMultipleMatch;
    }

    /// <summary>
    /// Parses an xsl:mode element into a <see cref="ModeDefinition"/>.
    /// </summary>
    public static ModeDefinition? FromElement(XElement element)
    {
        var name = ExpandModeName(element.Attribute("name")?.Value ?? "", element);
        var onNoMatch = element.Attribute("on-no-match")?.Value?.ToLowerInvariant() switch
        {
            "shallow-copy" => OnNoMatch.ShallowCopy,
            "shallow-skip" => OnNoMatch.ShallowSkip,
            "text-only-copy" => OnNoMatch.TextOnlyCopy,
            "deep-copy" => OnNoMatch.DeepCopy,
            "deep-skip" => OnNoMatch.DeepSkip,
            "fail" => OnNoMatch.Fail,
            _ => OnNoMatch.ShallowSkip
        };
        var onMultipleMatch = element.Attribute("on-multiple-match")?.Value?.ToLowerInvariant() switch
        {
            "fail" => OnMultipleMatch.Fail,
            _ => OnMultipleMatch.UseLast
        };
        return new ModeDefinition(name, onNoMatch, onMultipleMatch);
    }

    private static string ExpandModeName(string mode, XElement element)
    {
        if (mode == "#current" || mode == "#default" || mode == "#all")
            return mode;

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return mode;

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return $"{{{attr.Value}}}{local}";
                }
            }
            current = current.Parent;
        }
        return mode;
    }
}
