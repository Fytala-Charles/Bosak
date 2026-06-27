// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Annotation holding prefixes that must be undeclared on an element.
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
/// Annotates an <see cref="System.Xml.Linq.XElement"/> with a list of namespace prefixes
/// that must be explicitly undeclared (e.g. <c>xmlns:prefix=""</c>) when the element is
/// serialized. LINQ-to-XML cannot store such declarations directly.
/// </summary>
public sealed class PrefixedNamespaceUndeclarations
{
    /// <summary>
    /// The prefixes to undeclare on the annotated element.
    /// </summary>
    public List<string> Prefixes { get; } = new();
}
