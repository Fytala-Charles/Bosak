// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Unit tests for individual VM opcodes via direct IR construction.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class VmOpcodeTests
{
    private static XdmValue Run(params IrInstruction[] instructions)
    {
        var module = new IrModule(instructions, Array.Empty<object?>());
        var ctx = new EvaluationContext();
        return VmEngine.Execute(module, ctx);
    }

    private static XdmValue Run(IrInstruction[] instructions, object?[] literalPool)
    {
        var module = new IrModule(instructions, literalPool);
        var ctx = new EvaluationContext();
        return VmEngine.Execute(module, ctx);
    }

    // ------------------------------------------------------------------
    // Literals
    // ------------------------------------------------------------------

    [Fact]
    public void LoadInteger()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 0, 0, 0, 0),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 42L });
        Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void LoadString()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadString, 0, 0, 0, 0),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { "hello" });
        Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void LoadBoolean_True()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadBoolean, 0, 0, 0, 1),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void LoadBoolean_False()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadBoolean, 0, 0, 0, 0),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void LoadEmptySequence()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadEmptySequence, 0),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.True(result.IsSequence);
        Assert.True(result.SequenceValue!.TryGetLength(out var len));
        Assert.Equal(0, len);
    }

    // ------------------------------------------------------------------
    // Arithmetic
    // ------------------------------------------------------------------

    [Fact]
    public void Add()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.Add, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 20L });
        Assert.Equal(30, result.IntegerValue);
    }

    [Fact]
    public void Subtract()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.Subtract, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 20L, 10L });
        Assert.Equal(10, result.IntegerValue);
    }

    [Fact]
    public void Multiply()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.Multiply, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 6L, 7L });
        Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Divide()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.Divide, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 2L });
        Assert.Equal(5.0m, result.DecimalValue);
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    [Fact]
    public void Equal_True()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.Equal, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 5L, 5L });
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Equal_False()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.Equal, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 5L, 6L });
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void GreaterThan()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.GreaterThan, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 5L });
        Assert.True(result.BooleanValue);
    }

    // ------------------------------------------------------------------
    // Boolean logic
    // ------------------------------------------------------------------

    [Fact]
    public void And_BothTrue()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadBoolean, 1, 0, 0, 1),
            new IrInstruction(IrOpCode.LoadBoolean, 2, 0, 0, 1),
            new IrInstruction(IrOpCode.And, 0, 1, 2),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void And_LeftFalse()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadBoolean, 1, 0, 0, 0),
            new IrInstruction(IrOpCode.LoadBoolean, 2, 0, 0, 1),
            new IrInstruction(IrOpCode.And, 0, 1, 2),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void Or_LeftTrue()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadBoolean, 1, 0, 0, 1),
            new IrInstruction(IrOpCode.LoadBoolean, 2, 0, 0, 0),
            new IrInstruction(IrOpCode.Or, 0, 1, 2),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Not()
    {
        var result = Run(
            new IrInstruction(IrOpCode.LoadBoolean, 1, 0, 0, 1),
            new IrInstruction(IrOpCode.Not, 0, 1),
            new IrInstruction(IrOpCode.Return, 0));
        Assert.False(result.BooleanValue);
    }

    // ------------------------------------------------------------------
    // Sequences
    // ------------------------------------------------------------------

    [Fact]
    public void SequenceBuild()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.SequenceStart, 0),
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.SequenceAdd, 0, 1),
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 1),
                new IrInstruction(IrOpCode.SequenceAdd, 0, 1),
                new IrInstruction(IrOpCode.SequenceEnd, 0),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 20L });
        Assert.True(result.IsSequence);
    }

    [Fact]
    public void Concatenate()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.SequenceStart, 1),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 0),
                new IrInstruction(IrOpCode.SequenceAdd, 1, 2),
                new IrInstruction(IrOpCode.SequenceEnd, 1),
                new IrInstruction(IrOpCode.SequenceStart, 2),
                new IrInstruction(IrOpCode.LoadInteger, 3, 0, 0, 1),
                new IrInstruction(IrOpCode.SequenceAdd, 2, 3),
                new IrInstruction(IrOpCode.SequenceEnd, 2),
                new IrInstruction(IrOpCode.Concatenate, 0, 1, 2),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 1L, 2L });
        Assert.True(result.IsSequence);
    }

    // ------------------------------------------------------------------
    // Maps and Arrays
    // ------------------------------------------------------------------

    [Fact]
    public void MapBuildAndLookup()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.Map, 0),
                new IrInstruction(IrOpCode.LoadString, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.MapAdd, 0, 1, 2),
                new IrInstruction(IrOpCode.LoadString, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.Lookup, 0, 0, 1),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { "a", 1L });
        Assert.Equal(1, result.IntegerValue);
    }

    [Fact]
    public void ArrayBuildAndLookup()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.Array, 0),
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.ArrayAdd, 0, 1),
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 1),
                new IrInstruction(IrOpCode.ArrayAdd, 0, 1),
                new IrInstruction(IrOpCode.LoadInteger, 1, 0, 0, 2),
                new IrInstruction(IrOpCode.Lookup, 0, 0, 1),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 20L, 2L });
        Assert.Equal(20, result.IntegerValue);
    }

    [Fact]
    public void LookupWildcard_Map()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.Map, 0),
                new IrInstruction(IrOpCode.LoadString, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.LoadInteger, 2, 0, 0, 1),
                new IrInstruction(IrOpCode.MapAdd, 0, 1, 2),
                new IrInstruction(IrOpCode.LookupWildcard, 0, 0),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { "a", 1L });
        Assert.True(result.IsSequence);
    }

    // ------------------------------------------------------------------
    // Conditional
    // ------------------------------------------------------------------

    [Fact]
    public void JumpIfTrue_TakesJump()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadBoolean, 1, 0, 0, 1),
                new IrInstruction(IrOpCode.JumpIfTrue, 1, 0, 0, 4),
                new IrInstruction(IrOpCode.LoadInteger, 0, 0, 0, 0),
                new IrInstruction(IrOpCode.Return, 0),
                new IrInstruction(IrOpCode.LoadInteger, 0, 0, 0, 1),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 20L });
        Assert.Equal(20, result.IntegerValue);
    }

    [Fact]
    public void JumpIfFalse_TakesJump()
    {
        var result = Run(
            new IrInstruction[] {
                new IrInstruction(IrOpCode.LoadBoolean, 1, 0, 0, 0),
                new IrInstruction(IrOpCode.JumpIfFalse, 1, 0, 0, 4),
                new IrInstruction(IrOpCode.LoadInteger, 0, 0, 0, 0),
                new IrInstruction(IrOpCode.Return, 0),
                new IrInstruction(IrOpCode.LoadInteger, 0, 0, 0, 1),
                new IrInstruction(IrOpCode.Return, 0)
            },
            new object?[] { 10L, 20L });
        Assert.Equal(20, result.IntegerValue);
    }
}
