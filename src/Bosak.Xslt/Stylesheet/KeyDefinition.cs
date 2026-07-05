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
//                      | Charles Korthout | 0.2   | 11-06-2026     | Expand key name to Clark notation; capture @composite; validate required attrs/content   |
//                      | Charles Korthout | 0.3   | 26-06-2026     | Capture xsl:key @collation for effective collation resolution                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Linq;
using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a single xsl:key declaration within a stylesheet.
/// </summary>
public sealed class KeyDefinition
{
    /// <summary>The expanded key name in Clark notation (<c>{uri}local</c>).</summary>
    public string Name { get; }

    /// <summary>The match pattern string.</summary>
    public string Match { get; }

    /// <summary>The use expression string, if the key is defined by @use; otherwise null.</summary>
    public string? Use { get; }

    /// <summary>Whether the key is defined by a sequence constructor child (alternative to @use).</summary>
    public bool HasUseContent { get; }

    /// <summary>Whether this is a composite key (XSLT 3.0).</summary>
    public bool Composite { get; }

    /// <summary>The explicit collation URI for comparing key values, if any.</summary>
    public string? Collation { get; }

    /// <summary>The parent stylesheet.</summary>
    public Stylesheet Stylesheet { get; }

    /// <summary>The original xsl:key element (for namespace resolution).</summary>
    public XElement? Element { get; }

    public KeyDefinition(string name, string match, string? use, bool hasUseContent, bool composite, Stylesheet stylesheet, XElement? element = null, string? collation = null)
    {
        Name = name;
        Match = match;
        Use = use;
        HasUseContent = hasUseContent;
        Composite = composite;
        Stylesheet = stylesheet;
        Element = element;
        Collation = collation;
    }

    /// <summary>
    /// Creates a <see cref="KeyDefinition"/> from an xsl:key element.
    /// </summary>
    public static KeyDefinition FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name");
        var matchAttr = element.Attribute("match");
        var useAttr = element.Attribute("use");
        var compositeAttr = element.Attribute("composite");

        if (nameAttr == null || string.IsNullOrEmpty(nameAttr.Value))
            throw new InvalidOperationException("XTSE0010: xsl:key must have a @name attribute.");
        if (matchAttr == null || string.IsNullOrEmpty(matchAttr.Value))
            throw new InvalidOperationException("XTSE0010: xsl:key must have a @match attribute.");

        // xsl:key may contain a sequence constructor (as an alternative to @use) or
        // xsl:fallback children. The only disallowed XSLT child is xsl:template.
        foreach (var child in element.Elements())
        {
            if (child.Name == XName.Get("template", Stylesheet.XslNamespace))
                throw new InvalidOperationException("XTSE0010: xsl:key must not contain an xsl:template child.");
        }

        var expandedName = ExpandKeyName(nameAttr.Value, element);
        var composite = compositeAttr != null &&
                        (compositeAttr.Value == "yes" || compositeAttr.Value == "true" || compositeAttr.Value == "1");

        bool hasUseContent = useAttr == null && element.Elements().Any(e => e.Name != XName.Get("fallback", Stylesheet.XslNamespace));
        var collationAttr = element.Attribute("collation");
        var collation = collationAttr != null ? collationAttr.Value : null;

        return new KeyDefinition(expandedName, matchAttr.Value, useAttr?.Value, hasUseContent, composite, stylesheet, element, collation);
    }

    /// <summary>
    /// Expands an xsl:key/@name QName using the namespace declarations in scope on the element.
    /// Unprefixed names have no namespace.
    /// </summary>
    private static string ExpandKeyName(string name, XElement element)
    {
        var colon = name.IndexOf(':');
        if (colon <= 0 || colon == name.Length - 1)
            return "{" + "}" + name;

        var prefix = name[..colon];
        var local = name[(colon + 1)..];
        var ns = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
        return "{" + ns + "}" + local;
    }
}
