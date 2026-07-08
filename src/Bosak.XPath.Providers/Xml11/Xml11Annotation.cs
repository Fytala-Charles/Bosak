// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 July 2026
// PURPOSE              : Marks an XDocument as originating from XML 1.1 so that name decoding is applied.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotation attached to an <see cref="System.Xml.Linq.XDocument"/> that was loaded
/// from an XML 1.1 document. The annotation tells <see cref="XDocumentNode"/> to
/// decode XML 1.1-only name characters that were encoded so that .NET could store them.
/// </summary>
internal sealed class Xml11Annotation
{
    /// <summary>
    /// Singleton annotation instance.
    /// </summary>
    public static readonly Xml11Annotation Instance = new();

    private Xml11Annotation() { }
}
