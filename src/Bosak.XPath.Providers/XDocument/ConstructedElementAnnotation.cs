// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 augustus 2026
// PURPOSE              : Marks an XElement constructed by an XQuery element constructor.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotation attached to an <see cref="System.Xml.Linq.XElement"/> that was created by an
/// XQuery direct or computed element constructor. The annotation tells <see cref="XDocumentNode"/>
/// that the element's XDM type annotation is <c>xs:anyType</c> rather than <c>xs:untyped</c>.
/// </summary>
internal sealed class ConstructedElementAnnotation
{
    /// <summary>
    /// Singleton annotation instance.
    /// </summary>
    public static readonly ConstructedElementAnnotation Instance = new();

    private ConstructedElementAnnotation() { }
}
