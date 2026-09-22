// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 September 2026
// PURPOSE              : Public entry points for the XDM conversion helpers of the (internal) VmEngine.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-09-2026     | Creation — API freeze stage C: exposes Cast/TryCast/ValueMatchesType/ApplyFunctionConversion/InvokeFunctionItem |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// Public wrappers over the virtual machine's XDM conversion helpers: XPath 3.1 §19 casts,
/// sequence-type matching, function-conversion rules, and dynamic function-item invocation.
/// </summary>
public static class XdmConversions
{
    /// <summary>
    /// Casts a value to the named type using the XPath 3.1 §19 cast rules.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <param name="typeName">The target type name (e.g. <c>xs:integer</c>), optionally with an occurrence indicator.</param>
    /// <returns>The casted value.</returns>
    public static XdmValue Cast(XdmValue value, string typeName)
        => VmEngine.Cast(value, typeName);

    /// <summary>
    /// Casts a value to the named type using the XPath 3.1 §19 cast rules.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <param name="typeName">The target type name (e.g. <c>xs:integer</c>), optionally with an occurrence indicator.</param>
    /// <param name="context">An optional evaluation context used for namespace resolution and user-defined schema types.</param>
    /// <returns>The casted value.</returns>
    public static XdmValue Cast(XdmValue value, string typeName, EvaluationContext? context)
        => VmEngine.Cast(value, typeName, context);

    /// <summary>
    /// Attempts to cast a value to the named type using the XPath 3.1 §19 cast rules.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <param name="typeName">The target type name (e.g. <c>xs:integer</c>), optionally with an occurrence indicator.</param>
    /// <param name="result">The casted value when the cast succeeds.</param>
    /// <returns><c>true</c> when the cast succeeds.</returns>
    public static bool TryCast(XdmValue value, string typeName, out XdmValue result)
        => VmEngine.TryCast(value, typeName, out result);

    /// <summary>
    /// Attempts to cast a value to the named type using the XPath 3.1 §19 cast rules.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <param name="typeName">The target type name (e.g. <c>xs:integer</c>), optionally with an occurrence indicator.</param>
    /// <param name="context">An optional evaluation context used for namespace resolution and user-defined schema types.</param>
    /// <param name="result">The casted value when the cast succeeds.</param>
    /// <returns><c>true</c> when the cast succeeds.</returns>
    public static bool TryCast(XdmValue value, string typeName, EvaluationContext? context, out XdmValue result)
        => VmEngine.TryCast(value, typeName, context, out result);

    /// <summary>
    /// Checks whether an XDM value matches a declared type name (e.g. "xs:string", "element(foo)").
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <param name="typeName">The sequence type string to match against.</param>
    /// <returns><c>true</c> when the value matches the type.</returns>
    public static bool ValueMatchesType(XdmValue value, string typeName)
        => VmEngine.ValueMatchesType(value, typeName);

    /// <summary>
    /// Checks whether a value matches a sequence type, with an optional evaluation
    /// context that enables signature-aware matching of named function items.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <param name="typeName">The sequence type string to match against.</param>
    /// <param name="context">An optional evaluation context used to match named function items by signature.</param>
    /// <returns><c>true</c> when the value matches the type.</returns>
    public static bool ValueMatchesType(XdmValue value, string typeName, EvaluationContext? context)
        => VmEngine.ValueMatchesType(value, typeName, context);

    /// <summary>
    /// Applies the XPath 3.1 function-conversion rules to a value: subtype substitution,
    /// numeric promotion, and URI promotion. Raises XPTY0004 when no rule applies.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target sequence type string.</param>
    /// <param name="context">An optional evaluation context used for namespace resolution and function-item coercion.</param>
    /// <returns>The converted value.</returns>
    public static XdmValue ApplyFunctionConversion(XdmValue value, string targetType, EvaluationContext? context = null)
        => VmEngine.ApplyFunctionConversion(value, targetType, context);

    /// <summary>
    /// Invokes a function item dynamically.
    /// </summary>
    /// <param name="func">The function item to invoke.</param>
    /// <param name="context">The evaluation context for the call.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>The value produced by the function item.</returns>
    public static XdmValue InvokeFunctionItem(FunctionItem func, EvaluationContext context, ReadOnlySpan<XdmValue> args)
        => VmEngine.InvokeFunctionItem(func, context, args);

    /// <summary>
    /// Invokes a value that is expected to be a single function item, map, or array.
    /// </summary>
    /// <param name="funcValue">The value to invoke.</param>
    /// <param name="context">The evaluation context for the call.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>The value produced by the invocation.</returns>
    public static XdmValue InvokeFunctionItem(XdmValue funcValue, EvaluationContext context, ReadOnlySpan<XdmValue> args)
        => VmEngine.InvokeFunctionItem(funcValue, context, args);
}
