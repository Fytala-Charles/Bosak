// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Compile-time representation of an inline function stored in the literal pool.
// SPECIAL NOTES        : Part of the AST-to-IR compilation pipeline.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// Compile-time representation of an inline function, stored in the IR literal pool.
/// The VM converts this to <see cref="Bosak.XPath.Runtime.Functions.InlineFunctionItem"/>.
/// </summary>
public sealed record CompilerInlineFunction(IReadOnlyList<string> Parameters, IrModule Body, IReadOnlyList<string?> ParameterTypes, string? ReturnType);
