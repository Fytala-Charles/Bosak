// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 mei 2026
// PURPOSE              : Pluggable URI resolver for xsl:import and xsl:include.
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

namespace Bosak.XPath.Xslt.Api;

/// <summary>
/// Resolves href URIs referenced by xsl:import and xsl:include into loadable XDocuments.
/// </summary>
public interface IXsltUriResolver
{
    /// <summary>
    /// Resolves an href relative to a base URI and returns the parsed document.
    /// </summary>
    /// <param name="href">The href attribute value from xsl:import or xsl:include.</param>
    /// <param name="baseUri">The base URI of the stylesheet containing the reference, or null.</param>
    /// <returns>The parsed XDocument of the referenced stylesheet.</returns>
    XDocument Resolve(string href, string? baseUri);
}
