// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 July 2026
// PURPOSE              : Records the original namespace prefix used for an element in the XML source.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
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

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotates an <see cref="System.Xml.Linq.XElement"/> with the namespace prefix that
/// was used for the element in the original XML source document.
/// </summary>
public sealed class OriginalPrefixAnnotation
{
    /// <summary>
    /// The original namespace prefix, or the empty string if the element used the default
    /// namespace.
    /// </summary>
    public string Prefix { get; init; } = string.Empty;
}
