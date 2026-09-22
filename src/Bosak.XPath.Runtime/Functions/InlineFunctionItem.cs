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
//                      | Charles Korthout | 0.32  | 21-09-2026     | API freeze stage A: reduced accessibility (internalized Compiler types)                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// An inline function with parameter names, optional type declarations, and a compiled body module.
/// </summary>
public sealed record InlineFunctionItem : FunctionItem
{
    /// <summary>The declared parameter names.</summary>
    public IReadOnlyList<string> Parameters { get; }

    /// <summary>The compiled IR module for the function body.</summary>
    internal IrModule Body { get; }

    /// <summary>The declared parameter sequence types (null entries are untyped).</summary>
    public IReadOnlyList<string?> ParameterTypes { get; }

    /// <summary>The declared return sequence type, or null when undeclared.</summary>
    public string? ReturnType { get; }

    internal InlineFunctionItem(IReadOnlyList<string> parameters, IrModule body, IReadOnlyList<string?> parameterTypes, string? returnType)
    {
        Parameters = parameters;
        Body = body;
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
    }

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
