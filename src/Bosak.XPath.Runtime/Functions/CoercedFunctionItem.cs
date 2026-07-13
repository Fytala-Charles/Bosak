// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 juli 2026
// PURPOSE              : Function item wrapper applying XPath 3.1 function conversion rules at invocation time.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-07-2026     | Creation (higher-order-functions-038/060)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// A function item coerced to a declared function type. Per the XPath 3.1 function
/// conversion rules, invoking the wrapper converts each argument to the declared
/// parameter type, invokes the inner function, and validates the result against the
/// declared return type, raising XPTY0004 when the conversion is not possible.
/// </summary>
/// <param name="Inner">The wrapped function item.</param>
/// <param name="ParamTypes">Declared parameter sequence types from the coercion target.</param>
/// <param name="ReturnType">Declared return sequence type from the coercion target, or null.</param>
public sealed record CoercedFunctionItem(
    FunctionItem Inner,
    IReadOnlyList<string?> ParamTypes,
    string? ReturnType) : FunctionItem
{
    /// <inheritdoc/>
    public override int Arity => ParamTypes.Count;
}
