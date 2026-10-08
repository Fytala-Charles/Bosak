// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Specifies the XPath language version compatibility for compilation
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 08-10-2026     | REQ-118: added XPath40 (4.0-S0 version gate)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Api;

/// <summary>
/// Specifies the XPath language version compatibility for compilation.
/// The default (<see cref="XPath31"/>) is unchanged from previous releases;
/// <see cref="XPath40"/> opts in to XPath 4.0 features (REQ-118 version gate,
/// dossier docs/REQ-118-xpath-xslt-40.md).
/// </summary>
public enum XPathCompatibility
{
    /// <summary>
    /// XPath 4.0 (opt-in). Allows the constructs of XPath 3.1 plus the XPath 4.0
    /// surfaces as they land: 4.0-only F&amp;O functions (slice 4.0-S1: fn:replicate,
    /// fn:slice, fn:items-at, fn:foot, fn:trunk, fn:insert-separator, fn:char,
    /// fn:characters); grammar features land in later slices.
    /// </summary>
    XPath40 = 40,

    /// <summary>
    /// XPath 3.1 (default). Allows all XPath 3.1 constructs including
    /// maps, arrays, higher-order functions, the simple map operator (!),
    /// arrow operator (=>), and string concatenation (||). XPath 4.0-only
    /// functions raise XPST0017 during compilation.
    /// </summary>
    XPath31 = 31,

    /// <summary>
    /// XPath 3.0. Rejects XPath 3.1-specific constructs (maps, arrays,
    /// namespace-node() kind test) but allows higher-order functions.
    /// </summary>
    XPath30 = 30,

    /// <summary>
    /// XPath 2.0 compatibility mode. Rejects all XPath 3.0+ constructs
    /// including higher-order functions, the simple map operator, arrow
    /// operator, and string concatenation.
    /// </summary>
    XPath20 = 20,
}
