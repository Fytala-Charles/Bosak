// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : Represents a parsed xsl:key declaration.
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

using System.Xml.Linq;

namespace Bosak.XPath.Xslt.Stylesheet;

/// <summary>
/// Represents a single xsl:key declaration within a stylesheet.
/// </summary>
public sealed class KeyDefinition
{
    /// <summary>The key name.</summary>
    public string Name { get; }

    /// <summary>The match pattern string.</summary>
    public string Match { get; }

    /// <summary>The use expression string.</summary>
    public string Use { get; }

    /// <summary>The parent stylesheet.</summary>
    public Stylesheet Stylesheet { get; }

    public KeyDefinition(string name, string match, string use, Stylesheet stylesheet)
    {
        Name = name;
        Match = match;
        Use = use;
        Stylesheet = stylesheet;
    }

    /// <summary>
    /// Creates a <see cref="KeyDefinition"/> from an xsl:key element.
    /// </summary>
    public static KeyDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var name = element.Attribute("name")?.Value;
        var match = element.Attribute("match")?.Value;
        var use = element.Attribute("use")?.Value;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(match) || string.IsNullOrEmpty(use))
            return null;

        return new KeyDefinition(name, match, use, stylesheet);
    }
}
