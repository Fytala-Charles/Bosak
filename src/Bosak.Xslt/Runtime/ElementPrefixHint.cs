// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 July 2026
// PURPOSE              : Records the preferred namespace prefix chosen for a constructed element.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Annotates an <see cref="System.Xml.Linq.XElement"/> with the prefix that the XSLT
/// processor selected for the element's own namespace URI. The serializer uses this
/// hint to preserve sibling prefixes that map to the same namespace URI.
/// </summary>
internal sealed class ElementPrefixHint
{
    /// <summary>
    /// The preferred prefix for the element's namespace URI, or <c>null</c> if the
    /// element should use the default namespace.
    /// </summary>
    public string? Prefix { get; init; }
}
