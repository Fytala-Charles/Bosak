// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 July 2026
// PURPOSE              : Marker annotation for namespace bindings implied by attribute names, which do not propagate to descendants.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Marker annotation attached to an <see cref="System.Xml.Linq.XAttribute"/> namespace
/// declaration that was materialized because an attribute's name used the prefix
/// (an implied binding). Per XQuery direct-constructor semantics, such bindings are part
/// of the element's own in-scope namespaces but are NOT inherited by nested constructors
/// (K2-NameTest-30/31), unlike explicit xmlns declarations and element-name bindings,
/// which do propagate (K2-DirectConElemNamespace-40/41, K2-InScopePrefixesFunc-9).
/// </summary>
public sealed class NonPropagatingNamespaceBinding
{
}
