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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.XPath.Xslt.Stylesheet;

/// <summary>
/// Specifies the built-in template rule behavior when no explicit template matches a node.
/// </summary>
public enum OnNoMatch
{
    /// <summary>Shallow-copy the node (default in XSLT 3.0).</summary>
    ShallowCopy,
    /// <summary>Skip the element node, apply-templates to children only.</summary>
    ShallowSkip,
    /// <summary>Copy only text nodes, ignore elements.</summary>
    TextOnlyCopy,
    /// <summary>Deep-copy the entire subtree.</summary>
    DeepCopy,
    /// <summary>Throw an error when no template matches.</summary>
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

    public ModeDefinition(string name, OnNoMatch onNoMatch)
    {
        Name = name;
        OnNoMatch = onNoMatch;
    }

    /// <summary>
    /// Parses an xsl:mode element into a <see cref="ModeDefinition"/>.
    /// </summary>
    public static ModeDefinition? FromElement(XElement element)
    {
        var name = element.Attribute("name")?.Value ?? "";
        var onNoMatch = element.Attribute("on-no-match")?.Value?.ToLowerInvariant() switch
        {
            "shallow-skip" => OnNoMatch.ShallowSkip,
            "text-only-copy" => OnNoMatch.TextOnlyCopy,
            "deep-copy" => OnNoMatch.DeepCopy,
            "fail" => OnNoMatch.Fail,
            _ => OnNoMatch.ShallowCopy
        };
        return new ModeDefinition(name, onNoMatch);
    }
}
