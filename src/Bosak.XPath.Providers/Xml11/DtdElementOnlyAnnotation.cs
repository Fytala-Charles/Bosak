// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 August 2026
// PURPOSE              : Marks an XElement as declared with element-only content in its DTD.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-08-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 21-09-2026     | API freeze stage C: internalized type                                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Annotation attached to an <see cref="System.Xml.Linq.XElement"/> that records
/// that the element was declared with element-only content in the document's DTD.
/// </summary>
internal sealed class DtdElementOnlyAnnotation
{
}
