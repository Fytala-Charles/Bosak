// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Holds query execution context for XQuery evaluation.
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

using System.IO;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XQuery.Api;

/// <summary>
/// Provides the execution context for an XQuery, including the context item,
/// external variables, and namespace bindings.
/// </summary>
public sealed class XQueryContext
{
    /// <summary>
    /// The underlying XPath evaluation context.
    /// </summary>
    public EvaluationContext EvaluationContext { get; } = new EvaluationContext();

    /// <summary>
    /// Sets the context item for the query (the "dot").
    /// </summary>
    /// <param name="item">The initial context item.</param>
    /// <returns>This context for fluent chaining.</returns>
    public XQueryContext WithContextItem(XdmValue item)
    {
        EvaluationContext.WithFocus(item, 1, 1);
        return this;
    }

    /// <summary>
    /// Binds an external variable for the query.
    /// </summary>
    /// <param name="name">The variable name (no $ prefix).</param>
    /// <param name="value">The variable value.</param>
    /// <returns>This context for fluent chaining.</returns>
    public XQueryContext WithVariable(string name, XdmValue value)
    {
        EvaluationContext.WithVariable(name, value);
        return this;
    }

    /// <summary>
    /// Registers a resolver used to load schemas referenced by <c>import schema</c>.
    /// </summary>
    /// <param name="resolver">A function that receives the target namespace URI and location hints and returns a schema stream, or null if not found.</param>
    /// <returns>This context for fluent chaining.</returns>
    public XQueryContext WithSchemaResolver(Func<string, IReadOnlyList<string>, Stream?> resolver)
    {
        EvaluationContext.SchemaResolver = resolver;
        return this;
    }
}
