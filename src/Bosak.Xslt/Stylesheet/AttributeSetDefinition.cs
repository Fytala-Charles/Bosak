// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 June 2026
// PURPOSE              : Represents a parsed xsl:attribute-set declaration.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a parsed <c>xsl:attribute-set</c> declaration.
/// Attribute sets accumulate across imports/includes (merge semantics).
/// </summary>
public sealed class AttributeSetDefinition
{
    /// <summary>The resolved local name of the attribute set.</summary>
    public string LocalName { get; }

    /// <summary>The resolved namespace URI of the attribute set.</summary>
    public string NamespaceUri { get; }

    /// <summary>
    /// The raw <c>use-attribute-sets</c> value: a space-separated list of QName references.
    /// Resolved at runtime to handle cross-module references correctly.
    /// </summary>
    public string? UseAttributeSets { get; }

    /// <summary>The original <c>xsl:attribute-set</c> element.</summary>
    public XElement Element { get; }

    /// <summary>The import precedence of the stylesheet module that declared this set.</summary>
    public int ImportPrecedence { get; }

    public AttributeSetDefinition(string localName, string namespaceUri, string? useAttributeSets, XElement element, int importPrecedence)
    {
        LocalName = localName;
        NamespaceUri = namespaceUri;
        UseAttributeSets = useAttributeSets;
        Element = element;
        ImportPrecedence = importPrecedence;
    }

    /// <summary>
    /// Parses an <c>xsl:attribute-set</c> element into an <see cref="AttributeSetDefinition"/>.
    /// </summary>
    public static AttributeSetDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(nameAttr))
            return null;

        var nameVal = nameAttr.Trim();
        string localName;
        string nsUri;

        int colon = nameVal.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = nameVal.Substring(0, colon);
            localName = nameVal.Substring(colon + 1);
            nsUri = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
        }
        else
        {
            localName = nameVal;
            nsUri = "";
        }

        var useAttrSets = element.Attribute("use-attribute-sets")?.Value;

        return new AttributeSetDefinition(localName, nsUri, useAttrSets, element, stylesheet.ImportPrecedence);
    }
}
