// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Source file for IrLowererTests in the Development project
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
using Bosak.XPath.Compiler.Optimizer;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Xunit;

namespace Bosak.XPath.Compiler.Tests;

public class IrLowererTests
{
    private static IrModule Lower(string xpath)
    {
        var ast = XPathParser.Parse(xpath);
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(ast);
        return new IrLowerer().Lower(optimized);
    }

    private static IrInstruction[] Instructions(string xpath)
    {
        return Lower(xpath).Instructions.ToArray();
    }

    // ------------------------------------------------------------------
    // Literals
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_BooleanLiteral_LoadsBoolean()
    {
        var instrs = Instructions("true()");
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[0].OpCode);
        Assert.Equal(1, instrs[0].Operand);
        Assert.Equal(IrOpCode.Return, instrs[1].OpCode);
    }

    [Fact]
    public void Lower_BooleanLiteral_False_LoadsBoolean()
    {
        var instrs = Instructions("false()");
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[0].OpCode);
        Assert.Equal(0, instrs[0].Operand);
    }

    [Fact]
    public void Lower_IntegerLiteral_LoadsInteger()
    {
        var module = Lower("42");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(42L, module.LiteralPool[instrs[0].Operand]);
        Assert.Equal(IrOpCode.Return, instrs[1].OpCode);
    }

    [Fact]
    public void Lower_DecimalLiteral_LoadsDecimal()
    {
        var module = Lower("3.14");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadDecimal, instrs[0].OpCode);
        Assert.Equal(3.14m, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_DoubleLiteral_LoadsDouble()
    {
        var module = Lower("1e3");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadDouble, instrs[0].OpCode);
        Assert.Equal(1000.0, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_StringLiteral_LoadsString()
    {
        var module = Lower("'hello'");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadString, instrs[0].OpCode);
        Assert.Equal("hello", module.LiteralPool[instrs[0].Operand]);
    }

    // ------------------------------------------------------------------
    // Variables & Context
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_VariableReference_LoadsVariable()
    {
        var module = Lower("$x");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadVariable, instrs[0].OpCode);
        Assert.Equal("x", module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_VariableReference_WithPrefix_LoadsVariable()
    {
        var module = Lower("$ns:x");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadVariable, instrs[0].OpCode);
        Assert.Equal("ns:x", module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_ContextItem_LoadsContextItem()
    {
        var instrs = Instructions(".");
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadContextItem, instrs[0].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[1].OpCode);
    }

    // ------------------------------------------------------------------
    // Binary expressions (optimizer folds constants)
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_Addition_FoldedToLiteral()
    {
        var module = Lower("1 + 2");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds 1 + 2 to IntegerLiteralNode(3)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(3L, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_StringConcat_FoldedToLiteral()
    {
        var module = Lower("'a' || 'b'");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds 'a' || 'b' to StringLiteralNode("ab")
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadString, instrs[0].OpCode);
        Assert.Equal("ab", module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_Comparison_FoldedToLiteral()
    {
        var module = Lower("1 eq 2");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds 1 eq 2 to BooleanLiteralNode(false)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[0].OpCode);
        Assert.Equal(0, instrs[0].Operand);
    }

    // ------------------------------------------------------------------
    // Boolean short-circuit (optimizer folds constants)
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_And_FoldedToLiteral()
    {
        var module = Lower("true() and false()");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds true() and false() to BooleanLiteralNode(false)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[0].OpCode);
        Assert.Equal(0, instrs[0].Operand);
    }

    [Fact]
    public void Lower_Or_FoldedToLiteral()
    {
        var module = Lower("true() or false()");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds true() or false() to BooleanLiteralNode(true)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[0].OpCode);
        Assert.Equal(1, instrs[0].Operand);
    }

    // ------------------------------------------------------------------
    // Unary expressions (optimizer folds constants)
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_UnaryMinus_FoldedToNegativeLiteral()
    {
        var module = Lower("-5");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds -5 to IntegerLiteralNode(-5)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(-5L, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_UnaryPlus_FoldedToLiteral()
    {
        var module = Lower("+5");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds +5 to IntegerLiteralNode(5)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(5L, module.LiteralPool[instrs[0].Operand]);
    }

    // ------------------------------------------------------------------
    // If expressions (optimizer folds dead branches)
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_If_FoldedToThenBranch()
    {
        var module = Lower("if (true()) then 1 else 2");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds true() -> then eliminates if -> IntegerLiteralNode(1)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(1L, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_If_VariableCondition_PreservesStructure()
    {
        // Use a variable so the condition cannot be constant-folded
        var instrs = Instructions("if ($x) then 1 else 2");
        // LoadVariable $x
        // JumpIfFalse -> else
        // LoadInteger 1
        // Jump -> end
        // LoadInteger 2
        // Return
        Assert.Equal(6, instrs.Length);
        Assert.Equal(IrOpCode.LoadVariable, instrs[0].OpCode);
        Assert.Equal(IrOpCode.JumpIfFalse, instrs[1].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[2].OpCode);
        Assert.Equal(IrOpCode.Jump, instrs[3].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[4].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[5].OpCode);

        // JumpIfFalse should go to else (instruction 4)
        Assert.Equal(4, instrs[1].Operand);
        // Jump should go to end (instruction 5, which is Return)
        Assert.Equal(5, instrs[3].Operand);
    }

    // ------------------------------------------------------------------
    // Function calls
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_FunctionCall_NoArgs()
    {
        var module = Lower("fn:current-date()");
        var instrs = module.Instructions.ToArray();
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.Call, instrs[0].OpCode);
        Assert.Equal(0, instrs[0].RegisterB); // first arg reg
        Assert.Equal(0, instrs[0].RegisterC); // arg count
        Assert.Equal("fn:current-date", module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_FunctionCall_WithArgs()
    {
        var module = Lower("concat('a', 'b')");
        var instrs = module.Instructions.ToArray();
        // LoadString 'a'
        // LoadString 'b'
        // Call resultReg, firstArgReg, argCount=2, funcName
        // Return
        Assert.Equal(4, instrs.Length);
        Assert.Equal(IrOpCode.LoadString, instrs[0].OpCode);
        Assert.Equal(IrOpCode.LoadString, instrs[1].OpCode);
        Assert.Equal(IrOpCode.Call, instrs[2].OpCode);
        Assert.Equal(2, instrs[2].RegisterC); // 2 args
        Assert.Equal("concat", module.LiteralPool[instrs[2].Operand]);
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_Path_RelativeStep()
    {
        var module = Lower("child::foo");
        var instrs = module.Instructions.ToArray();
        Assert.True(instrs.Length >= 3);
        Assert.Equal(IrOpCode.LoadContextItem, instrs[0].OpCode);
        Assert.Equal(IrOpCode.Child, instrs[1].OpCode);
        Assert.Equal(IrOpCode.NameTest, instrs[2].OpCode);
        Assert.Equal("foo", module.LiteralPool[instrs[2].Operand]);
        Assert.Equal(IrOpCode.Return, instrs[^1].OpCode);
    }

    [Fact]
    public void Lower_Path_TwoSteps()
    {
        var module = Lower("foo/bar");
        var instrs = module.Instructions.ToArray();
        Assert.True(instrs.Length >= 5);
        Assert.Equal(IrOpCode.LoadContextItem, instrs[0].OpCode);
        Assert.Equal(IrOpCode.Child, instrs[1].OpCode);
        Assert.Equal(IrOpCode.NameTest, instrs[2].OpCode);
        Assert.Equal("foo", module.LiteralPool[instrs[2].Operand]);
        Assert.Equal(IrOpCode.Child, instrs[3].OpCode);
        Assert.Equal(IrOpCode.NameTest, instrs[4].OpCode);
        Assert.Equal("bar", module.LiteralPool[instrs[4].Operand]);
    }

    [Fact]
    public void Lower_Path_WithPredicate()
    {
        var instrs = Instructions("foo[1]");
        // LoadContextItem
        // Child axis
        // NameTest foo
        // Subscript [1]
        // Return
        Assert.True(instrs.Length >= 5);
        Assert.Equal(IrOpCode.LoadContextItem, instrs[0].OpCode);
        Assert.Equal(IrOpCode.Child, instrs[1].OpCode);
        Assert.Equal(IrOpCode.NameTest, instrs[2].OpCode);
        Assert.Equal(IrOpCode.Subscript, instrs[3].OpCode);
        Assert.Equal(1, instrs[3].Operand);
        Assert.Equal(IrOpCode.Return, instrs[^1].OpCode);
    }

    [Fact]
    public void Lower_Path_WithGeneralPredicate()
    {
        var instrs = Instructions("foo[bar]");
        // LoadContextItem
        // Child axis
        // NameTest foo
        // Filter (with predicate code inline)
        // Jump (skip predicate)
        // [predicate code]
        // LoadContextItem
        // Child axis
        // NameTest bar
        // Return (predicate return)
        // Return (main)
        Assert.True(instrs.Length >= 8);
        Assert.Equal(IrOpCode.LoadContextItem, instrs[0].OpCode);
        Assert.Equal(IrOpCode.Child, instrs[1].OpCode);
        Assert.Equal(IrOpCode.NameTest, instrs[2].OpCode);
        Assert.Equal(IrOpCode.Filter, instrs[3].OpCode);
        Assert.Equal(IrOpCode.Jump, instrs[4].OpCode);
        // Predicate entry point should be after Jump
        int predicateEntry = instrs[4].Operand;
        Assert.True(predicateEntry > 4);
        Assert.Equal(IrOpCode.LoadContextItem, instrs[5].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[^1].OpCode);

        // Filter should point to predicate entry (which is instruction 5)
        Assert.Equal(5, instrs[3].Operand);
    }

    // ------------------------------------------------------------------
    // Sequence expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_Sequence_SingleItem()
    {
        var instrs = Instructions("(1)");
        // Parentheses are unwrapped by optimizer
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
    }

    [Fact]
    public void Lower_Sequence_MultipleItems()
    {
        var instrs = Instructions("(1, 2, 3)");
        // SequenceStart
        // LoadInteger 1
        // SequenceAdd
        // LoadInteger 2
        // SequenceAdd
        // LoadInteger 3
        // SequenceAdd
        // SequenceEnd
        // Return
        Assert.Equal(9, instrs.Length);
        Assert.Equal(IrOpCode.SequenceStart, instrs[0].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[1].OpCode);
        Assert.Equal(IrOpCode.SequenceAdd, instrs[2].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[3].OpCode);
        Assert.Equal(IrOpCode.SequenceAdd, instrs[4].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[5].OpCode);
        Assert.Equal(IrOpCode.SequenceAdd, instrs[6].OpCode);
        Assert.Equal(IrOpCode.SequenceEnd, instrs[7].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[8].OpCode);
    }

    // ------------------------------------------------------------------
    // Range expressions (optimizer expands small ranges)
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_RangeExpression_ExpandedByOptimizer()
    {
        var instrs = Instructions("1 to 5");
        // Optimizer expands 1 to 5 into a sequence expression
        Assert.Equal(IrOpCode.SequenceStart, instrs[0].OpCode);
        Assert.Equal(IrOpCode.SequenceEnd, instrs[^2].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[^1].OpCode);
    }

    // ------------------------------------------------------------------
    // Complex expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Lower_NestedArithmetic_FoldedToLiteral()
    {
        var module = Lower("1 + 2 * 3");
        var instrs = module.Instructions.ToArray();
        // Optimizer folds to IntegerLiteralNode(7)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(7L, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_Parenthesized_Unwrapped()
    {
        var module = Lower("(1 + 2)");
        var instrs = module.Instructions.ToArray();
        // Optimizer unwraps parentheses and folds to IntegerLiteralNode(3)
        Assert.Equal(2, instrs.Length);
        Assert.Equal(IrOpCode.LoadInteger, instrs[0].OpCode);
        Assert.Equal(3L, module.LiteralPool[instrs[0].Operand]);
    }

    [Fact]
    public void Lower_VariableArithmetic_PreservesStructure()
    {
        // Use a variable so the expression cannot be constant-folded
        var instrs = Instructions("$x + 1");
        Assert.Equal(4, instrs.Length);
        Assert.Equal(IrOpCode.LoadVariable, instrs[0].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[1].OpCode);
        Assert.Equal(IrOpCode.Add, instrs[2].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[3].OpCode);
    }

    [Fact]
    public void Lower_VariableComparison_PreservesStructure()
    {
        var instrs = Instructions("$x lt 10");
        Assert.Equal(4, instrs.Length);
        Assert.Equal(IrOpCode.LoadVariable, instrs[0].OpCode);
        Assert.Equal(IrOpCode.LoadInteger, instrs[1].OpCode);
        Assert.Equal(IrOpCode.LessThan, instrs[2].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[3].OpCode);
    }

    [Fact]
    public void Lower_VariableAnd_PreservesShortCircuit()
    {
        var instrs = Instructions("$x and $y");
        // LoadVariable $x
        // JumpIfFalse -> falsePath
        // LoadVariable $y
        // JumpIfFalse -> falsePath
        // LoadBoolean true
        // Jump -> end
        // falsePath: LoadBoolean false
        // Return
        Assert.Equal(8, instrs.Length);
        Assert.Equal(IrOpCode.LoadVariable, instrs[0].OpCode);
        Assert.Equal(IrOpCode.JumpIfFalse, instrs[1].OpCode);
        Assert.Equal(IrOpCode.LoadVariable, instrs[2].OpCode);
        Assert.Equal(IrOpCode.JumpIfFalse, instrs[3].OpCode);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[4].OpCode);
        Assert.Equal(IrOpCode.Jump, instrs[5].OpCode);
        Assert.Equal(IrOpCode.LoadBoolean, instrs[6].OpCode);
        Assert.Equal(IrOpCode.Return, instrs[7].OpCode);
    }
}
