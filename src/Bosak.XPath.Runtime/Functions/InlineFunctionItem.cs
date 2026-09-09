// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Runtime representation of an inline function for the VM.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Added parameter/return type metadata for runtime validation                              |
//                      | Charles Korthout | 0.3   | 13-07-2026     | Added CapturedVariables for closure semantics (higher-order-functions-029/041/042)     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.31  | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// An inline function with parameter names, optional type declarations, and a compiled body module.
/// </summary>
/// <param name="Parameters">The declared parameter names.</param>
/// <param name="Body">The compiled IR module for the function body.</param>
/// <param name="ParameterTypes">The declared parameter sequence types (null entries are untyped).</param>
/// <param name="ReturnType">The declared return sequence type, or null when undeclared.</param>
public sealed record InlineFunctionItem(
    IReadOnlyList<string> Parameters,
    IrModule Body,
    IReadOnlyList<string?> ParameterTypes,
    string? ReturnType) : FunctionItem
{
    /// <inheritdoc/>
    public override int Arity => Parameters.Count;

    /// <summary>
    /// Variables in scope where the inline function was created. These implement closure
    /// semantics: when the function is invoked after the defining frame has exited (e.g.
    /// an XSLT function returning an inline function referencing its parameters), the
    /// captured values are restored into the evaluation context for the duration of the call.
    /// </summary>
    public IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue>? CapturedVariables { get; init; }
}
