// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A single instruction in the XPath intermediate representation. Uses a compact, register-based enc...
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
//                      | Charles Korthout | 0.2   | 01-06-2026     | Expanded register fields from byte to ushort to support >255 registers                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
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
    /// <summary>The operation the instruction performs.</summary>
    public readonly IrOpCode OpCode;
    /// <summary>The first register operand (typically the result register).</summary>
    public readonly ushort RegisterA;
    /// <summary>The second register operand.</summary>
    public readonly ushort RegisterB;
    /// <summary>The third register operand.</summary>
    public readonly ushort RegisterC;
    /// <summary>The immediate operand: a jump offset, literal pool index, or similar.</summary>
    public readonly int Operand;           // Jump offsets, literal pool indices, etc.

    /// <summary>
    /// Initializes an instruction with the given opcode and operands.
    /// </summary>
    /// <param name="opCode">The operation the instruction performs.</param>
    /// <param name="regA">The first register operand (typically the result register).</param>
    /// <param name="regB">The second register operand.</param>
    /// <param name="regC">The third register operand.</param>
    /// <param name="operand">The immediate operand: a jump offset, literal pool index, or similar.</param>
    public IrInstruction(IrOpCode opCode, ushort regA = 0, ushort regB = 0, ushort regC = 0, int operand = 0)
    {
        OpCode = opCode;
        RegisterA = regA;
        RegisterB = regB;
        RegisterC = regC;
        Operand = operand;
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Initializes a module from a lowered instruction sequence and literal pool.
    /// </summary>
    /// <param name="instructions">The IR instructions of the compiled expression.</param>
    /// <param name="literalPool">The literal pool values referenced by instruction operands.</param>
    /// <param name="maxRegisterCount">The number of registers the expression requires at runtime.</param>
    public IrModule(IrInstruction[] instructions, object?[] literalPool, int maxRegisterCount)
    {
        _instructions = instructions;
        _literalPool = literalPool;
        MaxRegisterCount = maxRegisterCount;
    }

    /// <summary>The IR instructions of the compiled expression.</summary>
    public ReadOnlySpan<IrInstruction> Instructions => _instructions;
    /// <summary>The literal pool values referenced by instruction operands.</summary>
    public ReadOnlySpan<object?> LiteralPool => _literalPool;
    /// <summary>The number of instructions in the module.</summary>
    public int InstructionCount => _instructions.Length;
    /// <summary>The number of registers the expression requires at runtime.</summary>
    public int MaxRegisterCount { get; }
}
