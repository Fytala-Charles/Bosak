// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Records the namespace bindings inherited by an element's children.
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

using System.Collections.Generic;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotates an <see cref="System.Xml.Linq.XElement"/> with the namespace bindings that
/// are in scope for its children. This is used to calculate prefixed namespace
/// undeclarations when <c>inherit-namespaces="no"</c> is in effect.
/// </summary>
public sealed class NamespaceInheritanceContext
{
    /// <summary>
    /// Maps prefix (empty string for the default namespace) to namespace URI.
    /// </summary>
    public Dictionary<string, string> Bindings { get; } = new();

    /// <summary>
    /// The prefixes from <see cref="Bindings"/> in the order in which they entered scope.
    /// </summary>
    public List<string> PrefixOrder { get; } = new();
}
