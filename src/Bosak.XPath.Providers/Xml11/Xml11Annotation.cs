// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 July 2026
// PURPOSE              : Marks nodes whose names use the reversible XML-name storage codec.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-07-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 27-10-2026     | Document encoded-name metadata for loaded and constructed trees (1.26.11).             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Marks loaded XML 1.1 trees and constructed nodes whose names use the reversible
/// storage codec. This metadata controls name decoding; XML-version feature checks
/// continue to use their explicit execution/construction settings.
/// </summary>
internal sealed class Xml11Annotation
{
    /// <summary>
    /// Singleton annotation instance.
    /// </summary>
    public static readonly Xml11Annotation Instance = new();

    private Xml11Annotation() { }
}
