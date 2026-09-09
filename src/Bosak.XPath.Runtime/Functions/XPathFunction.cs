// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Delegate signature for all XPath/XQuery extension functions
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
//                      | Charles Korthout | 0.2   | 13-07-2026     | Added DynamicImplementation for context-dependent dynamic calls                          |
//                      | Charles Korthout | 0.3   | 13-07-2026     | Added ParameterTypeNames/ReturnTypeName for function-item type tests                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 25-07-2026     | Added IsVariadic for variable-arity functions (fn:concat arity 2+)                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 01-09-2026     | Added IsHiddenFromFunctionLookup for pseudo-functions (xsl:original)                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.51  | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// Delegate signature for all XPath/XQuery extension functions.
/// </summary>
/// <param name="context">The evaluation context for the call.</param>
/// <param name="arguments">The evaluated call arguments.</param>
/// <returns>The function result.</returns>
public delegate XdmValue XPathFunction(EvaluationContext context, ReadOnlySpan<XdmValue> arguments);

/// <summary>
/// Strongly-typed variant for zero-argument functions to avoid array allocation.
/// </summary>
/// <param name="context">The evaluation context for the call.</param>
/// <returns>The function result.</returns>
public delegate XdmValue XPathFunction0(EvaluationContext context);

/// <summary>
/// Strongly-typed variant for one-argument functions.
/// </summary>
/// <param name="context">The evaluation context for the call.</param>
/// <param name="arg1">The first argument.</param>
/// <returns>The function result.</returns>
public delegate XdmValue XPathFunction1(EvaluationContext context, XdmValue arg1);

/// <summary>
/// Strongly-typed variant for two-argument functions.
/// </summary>
/// <param name="context">The evaluation context for the call.</param>
/// <param name="arg1">The first argument.</param>
/// <param name="arg2">The second argument.</param>
/// <returns>The function result.</returns>
public delegate XdmValue XPathFunction2(EvaluationContext context, XdmValue arg1, XdmValue arg2);

/// <summary>
/// Metadata describing an XPath function signature for static analysis and dispatch.
/// </summary>
public sealed class FunctionSignature
{
    /// <summary>The function's namespace URI (e.g. the <c>fn:</c> namespace).</summary>
    public required string NamespaceUri { get; init; }

    /// <summary>The function's local name.</summary>
    public required string LocalName { get; init; }

    /// <summary>The function's declared arity.</summary>
    public required int Arity { get; init; }

    /// <summary>The coarse parameter kinds used for dispatch, one entry per declared parameter.</summary>
    public required IReadOnlyList<XdmValueKind> ParameterTypes { get; init; }

    /// <summary>The coarse result kind used for dispatch.</summary>
    public required XdmValueKind ReturnType { get; init; }

    /// <summary>The implementation invoked when the function is called statically.</summary>
    public required XPathFunction Implementation { get; init; }

    /// <summary>
    /// Optional implementation used when the function is invoked through a function item
    /// (named function reference or partial application) rather than a static call.
    /// XSLT context-dependent functions such as <c>current-group</c> must raise a dynamic
    /// error on dynamic invocation even when a static call would succeed.
    /// </summary>
    public XPathFunction? DynamicImplementation { get; init; }

    /// <summary>
    /// Optional declared parameter sequence types as strings (e.g. <c>xs:long?</c>),
    /// available for XSLT user functions. Used for precise function-item type tests
    /// (<c>instance of function(...) as ...</c>).
    /// </summary>
    public IReadOnlyList<string?>? ParameterTypeNames { get; init; }

    /// <summary>
    /// Optional declared return sequence type as a string. See <see cref="ParameterTypeNames"/>.
    /// </summary>
    public string? ReturnTypeName { get; init; }

    /// <summary>
    /// When true, the function accepts any arity greater than or equal to <see cref="Arity"/>
    /// (e.g. <c>fn:concat</c> with arity 2+). Resolution falls back to a variadic signature
    /// when no exact-arity registration exists.
    /// </summary>
    public bool IsVariadic { get; init; }

    /// <summary>
    /// When true, the function is a pseudo-function that is not exposed through
    /// <c>fn:function-lookup</c> — it remains callable by name and through function items.
    /// Used for XSLT's <c>xsl:original</c>, which is only available lexically inside an
    /// overriding component (XSLT 3.0 §3.5.7.2).
    /// </summary>
    public bool IsHiddenFromFunctionLookup { get; init; }
}
