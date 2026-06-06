// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Represents a compiled XQuery ready for execution.
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
/// A compiled, thread-safe XQuery that can be evaluated against a context document.
/// </summary>
public sealed class XQueryExecutable
{
    private readonly string _source;

    internal XQueryExecutable(string source)
    {
        _source = source;
    }

    /// <summary>
    /// Executes the compiled query.
    /// </summary>
    /// <param name="context">The evaluation context (variables, context item, namespaces).</param>
    /// <returns>The result of the query as an <see cref="XdmValue"/>.</returns>
    public XdmValue Evaluate(XQueryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        // TODO: Wire to the XPath VM once compilation is implemented.
        return XdmValue.FromString("");
    }
}
