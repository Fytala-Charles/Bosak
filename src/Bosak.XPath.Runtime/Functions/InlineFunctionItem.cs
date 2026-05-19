// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Runtime representation of an inline function for the VM.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
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
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Functions;

/// <summary>
/// An inline function with parameter names and a compiled body module.
/// </summary>
public sealed record InlineFunctionItem(IReadOnlyList<string> Parameters, IrModule Body) : FunctionItem
{
    public override int Arity => Parameters.Count;
}
