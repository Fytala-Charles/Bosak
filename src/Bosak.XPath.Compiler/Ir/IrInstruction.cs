// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A single instruction in the XPath intermediate representation. Uses a compact, register-based enc...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 01-06-2026     | Expanded register fields from byte to ushort to support >255 registers                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Runtime.InteropServices;

namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// A single instruction in the XPath intermediate representation.
/// Uses a compact, register-based encoding.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct IrInstruction
{
    public readonly IrOpCode OpCode;
    public readonly ushort RegisterA;
    public readonly ushort RegisterB;
    public readonly ushort RegisterC;
    public readonly int Operand;           // Jump offsets, literal pool indices, etc.

    public IrInstruction(IrOpCode opCode, ushort regA = 0, ushort regB = 0, ushort regC = 0, int operand = 0)
    {
        OpCode = opCode;
        RegisterA = regA;
        RegisterB = regB;
        RegisterC = regC;
        Operand = operand;
    }

    public override string ToString()
        => $"{OpCode} r{RegisterA}, r{RegisterB}, r{RegisterC}, #{Operand}";
}

/// <summary>
/// A compiled XPath expression represented as a sequence of IR instructions
/// and an associated literal pool.
/// </summary>
public sealed class IrModule
{
    private readonly IrInstruction[] _instructions;
    private readonly object?[] _literalPool;

    public IrModule(IrInstruction[] instructions, object?[] literalPool, int maxRegisterCount)
    {
        _instructions = instructions;
        _literalPool = literalPool;
        MaxRegisterCount = maxRegisterCount;
    }

    public ReadOnlySpan<IrInstruction> Instructions => _instructions;
    public ReadOnlySpan<object?> LiteralPool => _literalPool;
    public int InstructionCount => _instructions.Length;
    public int MaxRegisterCount { get; }
}
