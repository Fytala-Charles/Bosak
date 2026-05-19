// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Specifies the XPath language version compatibility for compilation
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Api;

/// <summary>
/// Specifies the XPath language version compatibility for compilation.
/// </summary>
public enum XPathCompatibility
{
    /// <summary>
    /// XPath 3.1 (default). Allows all XPath 3.1 constructs including
    /// maps, arrays, higher-order functions, the simple map operator (!),
    /// arrow operator (=>), and string concatenation (||).
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
