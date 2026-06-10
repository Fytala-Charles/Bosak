// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 juni 2026
// PURPOSE              : Marker annotation for XElement to block namespace inheritance to children.
// SPECIAL NOTES        : Part of the XDocument node provider layer; used by xsl:copy / xsl:element inherit-namespaces="no".
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Marker annotation attached to an <see cref="System.Xml.Linq.XElement"/> to indicate
/// that its children should not inherit the element's in-scope namespace declarations.
/// This corresponds to the XSLT <c>inherit-namespaces="no"</c> directive on
/// <c>xsl:copy</c>, <c>xsl:element</c>, and literal result elements.
/// </summary>
public sealed class NamespaceInheritanceBarrier
{
}
