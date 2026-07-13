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
    /// The explicit Unicode codepoint-to-string mappings declared by <c>xsl:output-character</c>
    /// children, in declaration order.
    /// </summary>
    public IReadOnlyList<(int Codepoint, string String)> Mappings { get; }

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

        var mappings = new List<(int, string)>();
        foreach (var child in element.Elements(XName.Get("output-character", Stylesheet.XslNamespace)))
        {
            foreach (var attr in child.Attributes())
            {
                var attrName = attr.Name.LocalName;
                if (attrName != "character" && attrName != "string" && string.IsNullOrEmpty(attr.Name.NamespaceName))
                    throw new InvalidOperationException("XTSE0010: Unrecognized attribute on xsl:output-character.");
            }

            var charAttr = child.Attribute("character")?.Value;
            var stringAttr = child.Attribute("string")?.Value;
            if (string.IsNullOrEmpty(charAttr))
                throw new InvalidOperationException("XTSE0010: xsl:output-character must have a character attribute.");

            var codepoint = ParseCodepoint(charAttr);
            mappings.Add((codepoint, stringAttr ?? string.Empty));
        }

        return new CharacterMapDefinition(expandedName, useMaps, mappings);
    }

    private static int ParseCodepoint(string value)
    {
        if (value.Length == 0)
            throw new InvalidOperationException("XTSE0010: xsl:output-character character attribute is empty.");
        if (value.Length == 1)
            return value[0];
        if (value.Length == 2 && char.IsHighSurrogate(value[0]) && char.IsLowSurrogate(value[1]))
            return char.ConvertToUtf32(value[0], value[1]);
        if (System.Text.Rune.TryGetRuneAt(value, 0, out var rune))
            return rune.Value;
        throw new InvalidOperationException("XTSE0010: xsl:output-character character attribute is not a single character.");
    }

    private CharacterMapDefinition(string expandedName, IReadOnlyList<string> useCharacterMaps, IReadOnlyList<(int, string)> mappings)
    {
        ExpandedName = expandedName;
        UseCharacterMaps = useCharacterMaps;
        Mappings = mappings;
    }
}
