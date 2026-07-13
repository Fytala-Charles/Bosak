// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Delegate signature for all XPath/XQuery extension functions
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-07-2026     | Added DynamicImplementation for context-dependent dynamic calls                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// Delegate signature for all XPath/XQuery extension functions.
/// </summary>
public delegate XdmValue XPathFunction(EvaluationContext context, ReadOnlySpan<XdmValue> arguments);

/// <summary>
/// Strongly-typed variant for zero-argument functions to avoid array allocation.
/// </summary>
public delegate XdmValue XPathFunction0(EvaluationContext context);

/// <summary>
/// Strongly-typed variant for one-argument functions.
/// </summary>
public delegate XdmValue XPathFunction1(EvaluationContext context, XdmValue arg1);

/// <summary>
/// Strongly-typed variant for two-argument functions.
/// </summary>
public delegate XdmValue XPathFunction2(EvaluationContext context, XdmValue arg1, XdmValue arg2);

/// <summary>
/// Metadata describing an XPath function signature for static analysis and dispatch.
/// </summary>
public sealed class FunctionSignature
{
    public required string NamespaceUri { get; init; }
    public required string LocalName { get; init; }
    public required int Arity { get; init; }
    public required IReadOnlyList<XdmValueKind> ParameterTypes { get; init; }
    public required XdmValueKind ReturnType { get; init; }
    public required XPathFunction Implementation { get; init; }

    /// <summary>
    /// Optional implementation used when the function is invoked through a function item
    /// (named function reference or partial application) rather than a static call.
    /// XSLT context-dependent functions such as <c>current-group</c> must raise a dynamic
    /// error on dynamic invocation even when a static call would succeed.
    /// </summary>
    public XPathFunction? DynamicImplementation { get; init; }
}
