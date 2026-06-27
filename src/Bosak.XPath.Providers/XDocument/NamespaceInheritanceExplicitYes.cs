// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Marks an element whose parent explicitly requested namespace inheritance.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotates an <see cref="System.Xml.Linq.XElement"/> to indicate that its parent had an
/// explicit <c>inherit-namespaces="yes"</c> (or equivalent) attribute. Children of such an
/// element must redeclare inherited prefixes during raw XML 1.1 serialization so that the
/// inherited namespace nodes are visibly preserved.
/// </summary>
public sealed class NamespaceInheritanceExplicitYes
{
}
