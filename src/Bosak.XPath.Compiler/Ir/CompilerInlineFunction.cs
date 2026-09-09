// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Compile-time representation of an inline function stored in the literal pool.
// SPECIAL NOTES        : Part of the AST-to-IR compilation pipeline.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// Compile-time representation of an inline function, stored in the IR literal pool.
/// The VM converts this to <see cref="Bosak.XPath.Runtime.Functions.InlineFunctionItem"/>.
/// </summary>
/// <param name="Parameters">The parameter names of the inline function.</param>
/// <param name="Body">The compiled body of the inline function.</param>
/// <param name="ParameterTypes">The declared type name of each parameter, or null per parameter.</param>
/// <param name="ReturnType">The declared return type name, or null.</param>
public sealed record CompilerInlineFunction(IReadOnlyList<string> Parameters, IrModule Body, IReadOnlyList<string?> ParameterTypes, string? ReturnType);
