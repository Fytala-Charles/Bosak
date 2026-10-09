// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Unit tests verifying correctness of the underlying implementation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation (REQ-123 fn:op, F&O 4.0 §18.4): all 31 operators, keyword forms, error cases     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>
/// Tests for fn:op (F&O 4.0 §18.4): returns fn($x, $y) { $x ⊙ $y } for the 31 supported
/// binary operators. Semantics pinned against qt4tests fn/op.xml (fn-op-* cases).
/// </summary>
public class FnOpTests
{
    private static readonly string[] AllOperators =
    [
        ",", "and", "or", "+", "-", "*", "div", "idiv", "mod",
        "=", "<", "<=", ">", ">=", "!=",
        "eq", "lt", "le", "gt", "ge", "ne",
        "<<", ">>", "is", "is-not", "precedes", "follows", "precedes-or-is", "follows-or-is",
        "||", "|", "union", "except", "intersect", "to", "otherwise",
    ];

    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    private static string[] Seq40(string xpath)
    {
        var result = Eval40(xpath);
        if (result.IsUndefined)
            return [];
        if (!result.IsSequence)
            return [result.ToString()!];
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    private static InvalidOperationException Error40(string xpath)
        => Assert.Throws<InvalidOperationException>(() => Eval40(xpath));

    // ----- every supported operator yields an arity-2 function -----------------

    [Fact]
    public void Op_AllSupportedOperators_Arity2()
    {
        foreach (var op in AllOperators)
            Assert.Equal("true", Seq40($"function-arity(fn:op('{op}')) = 2")[0]);
    }

    [Fact]
    public void Op_AllSupportedOperators_EmptyOperandsYieldFalsy()
    {
        // fn-op-004 shape: every operator maps ((), ()) to () or false, so not() holds.
        foreach (var op in AllOperators)
            Assert.Equal("true", Seq40($"not(fn:op('{op}')((), ()))")[0]);
    }

    // ----- arithmetic -----------------------------------------------------------

    [Fact]
    public void Op_Add_Integers()
    {
        Assert.Equal(["4"], Seq40("fn:op('+')(2, 2)"));
        Assert.Equal(["true"], Seq40("fn:op('+')(2, 2) instance of xs:integer"));
    }

    [Fact]
    public void Op_Subtract_Doubles()
    {
        Assert.Equal(["1"], Seq40("fn:op('-')(2e0, 1e0)"));
        Assert.Equal(["true"], Seq40("fn:op('-')(2e0, 1e0) instance of xs:double"));
    }

    [Fact]
    public void Op_Arithmetic_DecimalOperands()
    {
        Assert.Equal(["2.4", "0", "1.44", "1", "0", "1"],
            Seq40("('+', '-', '*', 'div', 'mod', 'idiv') ! fn:op(.)(1.2, 1.2)"));
    }

    [Fact]
    public void Op_Arithmetic_DoubleOperandPromotes()
    {
        Assert.Equal(["2.4", "0", "1.44", "1", "0", "1"],
            Seq40("('+', '-', '*', 'div', 'mod', 'idiv') ! fn:op(.)(1.2e0, 1.2)"));
    }

    [Fact]
    public void Op_Multiply_DurationOperand()
    {
        // PT300S*2 + PT300S/2 = PT750S (lexical rendering may normalize to PT12M30S)
        Assert.Equal(["true"],
            Seq40("sum(('*', 'div') ! fn:op(.)(xs:dayTimeDuration('PT300S'), 2)) = xs:dayTimeDuration('PT750S')"));
    }

    [Fact]
    public void Op_Add_NodeOperandsAtomize()
    {
        Assert.Equal(["2"], Seq40("fn:op('div')(parse-xml('<a>10</a>'), parse-xml('<a>5</a>'))"));
    }

    [Fact]
    public void Op_Div_IntegerByZero_FOAR0001()
    {
        var ex = Error40("fn:op('div')(10, 0)");
        Assert.Contains("FOAR0001", ex.Message);
    }

    [Fact]
    public void Op_Add_UntypedAtomicJunk_FORG0001()
    {
        var ex = Error40("fn:op('+')(0, xs:untypedAtomic('junk'))");
        Assert.Contains("FORG0001", ex.Message);
    }

    // ----- higher-order usage ---------------------------------------------------

    [Fact]
    public void Op_ForEachPair_SpecExample()
    {
        Assert.Equal(["22", "24", "26", "28", "30"],
            Seq40("for-each-pair(21 to 25, 1 to 5, fn:op('+'))"));
        Assert.Equal(["20", "20", "20", "20", "20"],
            Seq40("for-each-pair(21 to 25, 1 to 5, fn:op('-'))"));
    }

    [Fact]
    public void Op_PartialApplication_PipelineIncrement()
    {
        Assert.Equal(["8"], Seq40("let $inc := fn:op('+')(?, 1) return 5 => $inc() => $inc() => $inc()"));
    }

    // ----- boolean operators ----------------------------------------------------

    [Fact]
    public void Op_AndOr_TruthTable()
    {
        Assert.Equal(["true", "false", "false", "false", "true", "true", "true", "false"],
            Seq40("for $s in ('and', 'or'), $x in (true(), false()), $y in (true(), false()) return fn:op($s)($x, $y)"));
    }

    // ----- value comparisons ----------------------------------------------------

    [Fact]
    public void Op_ValueComparisons_Singletons()
    {
        // eq/ne/gt/ge/lt/le against (2,2,2) over (1,2,3)
        Assert.Equal(["false", "true", "false"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('eq')) } ? *"));
        Assert.Equal(["true", "false", "true"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('ne')) } ? *"));
        Assert.Equal(["false", "false", "true"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('gt')) } ? *"));
        Assert.Equal(["false", "true", "true"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('ge')) } ? *"));
        Assert.Equal(["true", "false", "false"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('lt')) } ? *"));
        Assert.Equal(["true", "true", "false"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('le')) } ? *"));
    }

    [Fact]
    public void Op_ValueComparison_EmptyOperand_EmptyResult()
    {
        foreach (var op in new[] { "eq", "ne", "gt", "ge", "lt", "le" })
            Assert.Equal([], Seq40($"fn:op('{op}')(42, ())"));
    }

    [Fact]
    public void Op_ValueComparison_MultiItemOperand_XPTY0004()
    {
        var ex = Error40("fn:op('eq')(42, 1 to 10)");
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Op_ValueComparison_ImplicitTimezoneFromContext()
    {
        // fn-op-vc-005: 12:00:00+01:00 eq 12:00:00 holds iff the implicit timezone is +01:00.
        Assert.Equal("true",
            Seq40("fn:op('eq')(xs:time('12:00:00+01:00'), xs:time('12:00:00')) = (implicit-timezone() = xs:dayTimeDuration('PT1H'))")[0]);
    }

    [Fact]
    public void Op_ValueComparison_DefaultCollationApplies()
    {
        var expr = XPath31Expression.Compile("fn:op('eq')('fred', 'FRED')",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        var context = new EvaluationContext
        {
            DefaultCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive"
        };
        Assert.True(expr.Evaluate(context).GetEffectiveBooleanValue());
    }

    // ----- general comparisons --------------------------------------------------

    [Fact]
    public void Op_GeneralComparisons_Existential()
    {
        Assert.Equal(["false", "true", "false"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('=')) } ? *"));
        Assert.Equal(["true", "false", "true"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('!=')) } ? *"));
        Assert.Equal(["false", "false", "true"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('>')) } ? *"));
        Assert.Equal(["false", "true", "true"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('>=')) } ? *"));
        Assert.Equal(["true", "false", "false"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('<')) } ? *"));
        Assert.Equal(["true", "true", "false"], Seq40("array { for-each-pair((1,2,3), (2,2,2), fn:op('<=')) } ? *"));
    }

    // ----- node comparisons -----------------------------------------------------

    [Fact]
    public void Op_NodeComparisons_SymbolicAndKeywordForms()
    {
        // pairs (a,b), (b,a), (c,c) against $in//b, $in//a, $in//c
        Assert.Equal(["false", "false", "true"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('is')) } ? *"));
        Assert.Equal(["true", "false", "false"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('<<')) } ? *"));
        Assert.Equal(["true", "false", "false"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('precedes')) } ? *"));
        Assert.Equal(["false", "true", "false"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('>>')) } ? *"));
        Assert.Equal(["false", "true", "false"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('follows')) } ? *"));
    }

    [Fact]
    public void Op_NodeComparisons_OrIsFormsIncludeIdentity()
    {
        Assert.Equal(["true", "false", "true"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('precedes-or-is')) } ? *"));
        Assert.Equal(["false", "true", "true"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('follows-or-is')) } ? *"));
        Assert.Equal(["true", "true", "false"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return array { for-each-pair(($in//a, $in//b, $in//c), ($in//b, $in//a, $in//c), fn:op('is-not')) } ? *"));
    }

    [Fact]
    public void Op_Is_NonNodeOperands_XPTY0004()
    {
        var ex = Error40("fn:op('is')(1, 2)");
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ----- venn operators -------------------------------------------------------

    [Fact]
    public void Op_VennOperators_NodeSequences()
    {
        Assert.Equal(["a", "b", "c"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return fn:op('union')(($in//a, $in//c), $in//b) ! name()"));
        Assert.Equal(["a", "b", "c"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return fn:op('|')(($in//a, $in//c), $in//b) ! name()"));
        Assert.Equal(["c"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return fn:op('intersect')(($in//a, $in//c), ($in//b, $in//c)) ! name()"));
        Assert.Equal(["a"],
            Seq40("let $in := parse-xml('<doc><a/><b/><c/></doc>') return fn:op('except')(($in//a, $in//c), ($in//b, $in//c)) ! name()"));
    }

    [Fact]
    public void Op_Union_NonNodeOperands_XPTY0004()
    {
        var ex = Error40("fn:op('union')(1, 2)");
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ----- string concat ----------------------------------------------------------

    [Fact]
    public void Op_StringConcat()
    {
        Assert.Equal(["abcdef"], Seq40("fn:op('||')('abc', 'def')"));
    }

    // ----- range ------------------------------------------------------------------

    [Fact]
    public void Op_To_Count()
    {
        Assert.Equal(["19"], Seq40("count(fn:op('to')(2, 20))"));
    }

    // ----- otherwise ----------------------------------------------------------------

    [Fact]
    public void Op_Otherwise_EmptyLeftYieldsRight()
    {
        Assert.Equal(["42"], Seq40("fn:op('otherwise')((), 42)"));
    }

    [Fact]
    public void Op_Otherwise_NonEmptyLeftYieldsLeft()
    {
        Assert.Equal(["1"], Seq40("fn:op('otherwise')(1, 42)"));
    }

    // ----- comma --------------------------------------------------------------------

    [Fact]
    public void Op_Comma_ConcatenatesSequences()
    {
        Assert.Equal(["1", "2", "3", "4"], Seq40("fn:op(',')((1, 2), (3, 4))"));
    }

    // ----- validation -----------------------------------------------------------------

    [Theory]
    [InlineData("!")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" div ")]
    [InlineData("/")]
    public void Op_UnsupportedOperator_XPTY0004(string op)
    {
        var ex = Error40($"function-arity(fn:op('{op}')) = 2");
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ----- keyword arguments -----------------------------------------------------------

    [Fact]
    public void Op_KeywordArgument()
    {
        Assert.Equal(["4"], Seq40("fn:op(operator := '+')(2, 2)"));
    }
}
