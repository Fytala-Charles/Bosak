// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 juli 2026
// PURPOSE              : Represents a parsed xsl:character-map declaration.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a parsed <c>xsl:character-map</c> declaration.
/// Character maps are resolved at serialization time and applied to text nodes
/// and attribute values, but not to the contents of CDATA sections.
/// </summary>
public sealed class CharacterMapDefinition
{
    /// <summary>The expanded name of the character map in Clark notation.</summary>
    public string ExpandedName { get; }

    /// <summary>
    /// Expanded names of character maps that are included by this map
    /// via the <c>use-character-maps</c> attribute.
    /// </summary>
    public IReadOnlyList<string> UseCharacterMaps { get; }

    /// <summary>
    /// The explicit character-to-string mappings declared by <c>xsl:output-character</c>
    /// children, in declaration order.
    /// </summary>
    public IReadOnlyList<(char Character, string String)> Mappings { get; }

    /// <summary>
    /// Parses an <c>xsl:character-map</c> element into a <see cref="CharacterMapDefinition"/>.
    /// </summary>
    public static CharacterMapDefinition FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(nameAttr))
            throw new InvalidOperationException("XTSE0010: xsl:character-map must have a name attribute.");

        var expandedName = Stylesheet.ExpandQName(element, nameAttr);

        var useMaps = new List<string>();
        var useAttr = element.Attribute("use-character-maps")?.Value;
        if (!string.IsNullOrWhiteSpace(useAttr))
        {
            foreach (var token in useAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                useMaps.Add(Stylesheet.ExpandQName(element, token));
            }
        }

        var mappings = new List<(char, string)>();
        foreach (var child in element.Elements(XName.Get("output-character", Stylesheet.XslNamespace)))
        {
            var charAttr = child.Attribute("character")?.Value;
            var stringAttr = child.Attribute("string")?.Value;
            if (string.IsNullOrEmpty(charAttr))
                continue;

            var ch = charAttr[0];
            mappings.Add((ch, stringAttr ?? string.Empty));
        }

        return new CharacterMapDefinition(expandedName, useMaps, mappings);
    }

    private CharacterMapDefinition(string expandedName, IReadOnlyList<string> useCharacterMaps, IReadOnlyList<(char, string)> mappings)
    {
        ExpandedName = expandedName;
        UseCharacterMaps = useCharacterMaps;
        Mappings = mappings;
    }
}
