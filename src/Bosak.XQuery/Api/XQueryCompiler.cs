// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Compiles XQuery 3.1 source into an executable query plan.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-06-2026     | Creation — placeholder skeleton                                                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;

namespace Bosak.XQuery.Api;

/// <summary>
/// Compiles XQuery 3.1 source text into an <see cref="XQueryExecutable"/> that can be executed repeatedly.
/// </summary>
public sealed class XQueryCompiler
{
    /// <summary>
    /// Compiles the supplied XQuery source text.
    /// </summary>
    /// <param name="query">The XQuery 3.1 source text.</param>
    /// <returns>An executable query plan.</returns>
    public XQueryExecutable Compile(string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);
        // TODO: Implement XQuery parsing, prolog analysis, and compilation to VM bytecode.
        return new XQueryExecutable(query);
    }
}
