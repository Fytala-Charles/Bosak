// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 July 2026
// PURPOSE              : Marker annotation for namespace nodes created by computed namespace constructors, which are parentless.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Marker annotation attached to the owner element of a namespace node created by a
/// computed namespace constructor (<c>namespace p {"uri"}</c>). Such nodes are
/// parentless per XQuery 3.1 (nscons-012: <c>exists($ns/..)</c> is false), unlike
/// namespace nodes reached through the namespace axis, whose owner is their element.
/// </summary>
public sealed class ParentlessNamespaceNode
{
}
