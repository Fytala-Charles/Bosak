// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Records namespace URIs excluded from the result tree for an element.
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
/// Annotates an <see cref="System.Xml.Linq.XElement"/> with the set of namespace URIs that
/// are excluded from the result tree by <c>xsl:exclude-result-prefixes</c>. This is used
/// when creating namespace nodes to avoid false XTDE0430 conflicts.
/// </summary>
public sealed class ExcludedNamespaceUris
{
    /// <summary>
    /// The excluded namespace URIs.
    /// </summary>
    public HashSet<string> Uris { get; } = new();
}
