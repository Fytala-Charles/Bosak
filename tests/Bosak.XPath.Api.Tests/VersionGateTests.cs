// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 oktober 2026
// PURPOSE              : Unit tests for the REQ-118 XPath 4.0 version gate (slice 4.0-S0).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-10-2026     | Creation: 3.1 rejection (XPST0017), 4.0 opt-in, function-lookup visibility               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 08-10-2026     | Part 2 batch gate assertions (fn:highest, fn:hash)                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 08-10-2026     | Part 3 (REQ-118 4.0-S2) gate assertions (map:build, array:slice, fn:parse-uri,         |
//                      |                  |       |                | fn:unix-dateTime)                                                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 08-10-2026     | REQ-118 4.0-S3a: '??' otherwise operator (3.1 rejection, semantics, precedence,        |
//                      |                  |       |                | guarding, focus) and 4.0 binary/hex/underscore numeric literals                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 08-10-2026     | REQ-118 4.0-S3b: keyword arguments (XPST0003 in 3.1, XPST0017 rules, defaults, arrow    |
//                      |                  |       |                | form) and string templates (escapes, interpolations, nesting)                            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

public class VersionGateTests
{
    private static XdmValue Eval(string xpath)
    {
        var ctx = new EvaluationContext();
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    // ------------------------------------------------------------------
    // 3.1 mode (default): 4.0-only functions are statically rejected
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_FnReplicate_DefaultOptions_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:replicate(1, 2)"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_UnprefixedReplicate_DefaultOptions_ThrowsXpst0017()
    {
        // No prefix resolves to the fn namespace by default.
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("replicate(1, 2)"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_NamedFunctionRefReplicate_DefaultOptions_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:replicate#2"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_SliceInDefaultMode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:slice((1, 2, 3), 1)"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_XPath30Mode_AlsoRejectsXPath40Functions()
    {
        var options = new CompileOptions { Compatibility = XPathCompatibility.XPath30 };
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:foot((1, 2))", options));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_Part2Functions_DefaultOptions_ThrowXpst0017()
    {
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:highest((1, 2))")).Message);
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:hash('abc')")).Message);
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:sort-by((1, 2), ())")).Message);
    }

    [Fact]
    public void FunctionLookup_Part2FunctionIn31Mode_ReturnsEmpty()
    {
        Assert.True(Eval("fn:function-lookup(xs:QName('fn:highest'), 3)").IsUndefined);
        Assert.True(Eval("fn:function-lookup(xs:QName('fn:hash'), 2)").IsUndefined);
    }

    [Fact]
    public void Compile_Part3Functions_DefaultOptions_ThrowXpst0017()
    {
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("map:build((1, 2), function($x){$x})")).Message);
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("array:slice([1, 2], 1)")).Message);
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:parse-uri('http://example.com/')")).Message);
        Assert.Contains("XPST0017",
            Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:unix-dateTime(0)")).Message);
    }

    [Fact]
    public void FunctionLookup_Part3FunctionIn31Mode_ReturnsEmpty()
    {
        Assert.True(Eval("fn:function-lookup(xs:QName('map:build'), 2)").IsUndefined);
        Assert.True(Eval("fn:function-lookup(xs:QName('array:slice'), 2)").IsUndefined);
        Assert.True(Eval("fn:function-lookup(xs:QName('fn:parse-uri'), 1)").IsUndefined);
        Assert.True(Eval("fn:function-lookup(xs:QName('fn:unix-dateTime'), 1)").IsUndefined);
    }

    [Fact]
    public void Evaluate_Part3Functions_XPath40Mode_Work()
    {
        Assert.Equal("2", Eval40("map:size(map:build((1, 2), function($x){$x mod 2}))").ToString());
        Assert.Equal("2", Eval40("array:size(array:slice([1, 2, 3], 2))").ToString());
        Assert.Equal("http", Eval40("fn:parse-uri('http://example.com/')?scheme").ToString());
        Assert.Equal("1970-01-01T00:00:00Z", Eval40("fn:unix-dateTime()").ToString());
    }

    [Fact]
    public void FunctionLookup_ReplicateIn31Mode_ReturnsEmpty()
    {
        var result = Eval("fn:function-lookup(xs:QName('fn:replicate'), 2)");
        Assert.True(result.IsUndefined);
    }

    [Fact]
    public void FunctionLookup_FootIn31Mode_ReturnsEmpty()
    {
        var result = Eval("fn:function-lookup(xs:QName('fn:foot'), 1)");
        Assert.True(result.IsUndefined);
    }

    // ------------------------------------------------------------------
    // 3.1 mode (default): removed-function behavior is unchanged
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_RemovedFunction_StillReportsRemoved()
    {
        // Braced-URI names are rejected by the parser itself (XPST0017 as a parse error).
        var ex = Assert.Throws<XPathParseException>(
            () => XPath31Expression.Compile("Q{http://www.w3.org/2005/xpath-functions}deep-equal2(1, 2)"));
        Assert.Contains("has been removed", ex.Message);
    }

    [Fact]
    public void Compile_31Function_DefaultOptions_StillWorks()
    {
        var result = Eval("fn:head((1, 2, 3))");
        Assert.Equal("1", result.ToString());
    }

    // ------------------------------------------------------------------
    // 4.0 mode (opt-in): functions compile and evaluate
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_Replicate_XPath40Mode_Succeeds()
    {
        var expr = XPath31Expression.Compile("fn:replicate('A', 2)", new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.NotNull(expr);
    }

    [Fact]
    public void Evaluate_Replicate_XPath40Mode_ReturnsReplicatedSequence()
    {
        var result = Eval40("fn:replicate(('A', 'B'), 2)");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(["A", "B", "A", "B"], items);
    }

    [Fact]
    public void Evaluate_NamedFunctionRefReplicate_XPath40Mode_IsFunctionItem()
    {
        var result = Eval40("fn:replicate#2");
        Assert.True(result.IsFunction);
    }

    [Fact]
    public void FunctionLookup_ReplicateIn40Mode_FindsFunction()
    {
        var result = Eval40("fn:function-lookup(xs:QName('fn:replicate'), 2)");
        Assert.True(result.IsFunction);
    }

    [Fact]
    public void FunctionLookupResult_ReplicateIn40Mode_IsCallable()
    {
        var result = Eval40("fn:function-lookup(xs:QName('fn:replicate'), 2)('A', 2)");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(["A", "A"], items);
    }

    [Fact]
    public void Evaluate_Slice_XPath40Mode_Works()
    {
        var result = Eval40("fn:slice(('a', 'b', 'c', 'd', 'e'), 2, 4)");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(["b", "c", "d"], items);
    }

    [Fact]
    public void Evaluate_Char_XPath40Mode_Works()
    {
        var result = Eval40("fn:char('pi')");
        Assert.Equal("\u03C0", result.ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S3a: '??' otherwise operator (XPath 4.0 §4.17, §2.6.5)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_Otherwise_Default31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("() ?? 4"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Compile_Otherwise_XPath30Mode_ThrowsXpst0003()
    {
        var options = new CompileOptions { Compatibility = XPathCompatibility.XPath30 };
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("() ?? 4", options));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_Otherwise_EmptyLeftYieldsRight()
    {
        Assert.Equal("4", Eval40("() ?? 4").ToString());
        Assert.Equal("4", Eval40("((), ()) ?? 4").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_NonEmptyLeftWins()
    {
        Assert.Equal("1", Eval40("1 ?? 4").ToString());
        Assert.Equal("false", Eval40("false() ?? true()").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_NonEmptyLeftReturnsWholeSequence()
    {
        var result = Eval40("(1, 2) ?? (3, 4)");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(["1", "2"], items);
    }

    [Fact]
    public void Evaluate_Otherwise_Chained_IsLeftAssociative()
    {
        Assert.Equal("4", Eval40("() ?? () ?? 4").ToString());
        Assert.Equal("true", Eval40("() ?? () ?? () instance of empty-sequence()").ToString());
        Assert.Equal("1", Eval40("1 ?? () ?? 4").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_BindsTighterThanComparison()
    {
        // $a = (@x otherwise @y + 1) parses the RHS as OtherwiseExpr (spec §4.17 note):
        // the comparison operand is a whole OtherwiseExpr.
        Assert.Equal("true", Eval40("4 = 4 ?? 3").ToString());
        Assert.Equal("false", Eval40("1 ?? 2 = 2").ToString());
        Assert.Equal("false", Eval40("(4 = 5) ?? 3").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_BindsLessTightlyThanArithmeticAndConcat()
    {
        Assert.Equal("3", Eval40("1 + 2 ?? 4").ToString());
        Assert.Equal("12", Eval40("1 || 2 ?? 4").ToString());
        Assert.Equal("6", Eval40("2 * 3 ?? 4").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_RightOperandIsGuarded()
    {
        // §2.6.5: the RHS cannot throw a dynamic error unless the LHS is empty,
        // so the divide-by-zero in the RHS must not surface here.
        var result = Eval40("(1, 2) ?? (1 div 0)");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(["1", "2"], items);
    }

    [Fact]
    public void Evaluate_Otherwise_LeftErrorPropagates()
    {
        // The guard applies to the RHS only; an error in the LHS is raised.
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("(1 div 0) ?? 5"));
        Assert.Contains("FOAR0001", ex.Message);
    }

    [Fact]
    public void Evaluate_Otherwise_RightErrorSurfacesWhenLeftEmpty()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("() ?? (1 div 0)"));
        Assert.Contains("FOAR0001", ex.Message);
    }

    [Fact]
    public void Evaluate_Otherwise_InPredicate()
    {
        Assert.Equal("true", Eval40("deep-equal((2, 3), (1, 2, 3)[(. ?? 0) gt 1])").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_WithLookupOperator()
    {
        Assert.Equal("5", Eval40("map{}?missing ?? 5").ToString());
        Assert.Equal("7", Eval40("map{'a': 7}?a ?? 5").ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_NodeSequenceLeftWins()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var ctx = new EvaluationContext().WithFocus(XdmValue.FromNode(root), 1, 1);
        var expr = XPath31Expression.Compile("name(/root/a ?? /root/b)", new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("a", expr.Evaluate(ctx).ToString());
    }

    [Fact]
    public void Evaluate_Otherwise_RightOperandGetsContainingFocus()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var ctx = new EvaluationContext().WithFocus(XdmValue.FromNode(root), 1, 1);
        // The RHS is evaluated with the focus of the containing expression, not
        // a focus derived from the (empty) LHS.
        var expr = XPath31Expression.Compile("b ?? name(.)", new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("root", expr.Evaluate(ctx).ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S3a: numeric literals — 0x/0b literals, underscores (§4.3.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_HexBinaryLiterals_XPath40Mode_Work()
    {
        Assert.Equal("65535", Eval40("0xffff").ToString());
        Assert.Equal("4294967295", Eval40("0xFFFF_FFFF").ToString());
        Assert.Equal("5", Eval40("0b101").ToString());
        Assert.Equal("255", Eval40("0b1111_1111").ToString());
        Assert.Equal("129", Eval40("0b1000_0001").ToString());
    }

    [Fact]
    public void Evaluate_UnderscoreSeparators_XPath40Mode_Work()
    {
        Assert.Equal("1000000", Eval40("1_000_000").ToString());
        Assert.Equal("1000000", Eval40("1_000_000.0").ToString());
        Assert.Equal("3.141592653589793", Eval40("3.14159_26535_89793e0").ToString());
    }

    [Fact]
    public void Evaluate_HexBinaryLiterals_31Mode_ThrowXpst0003()
    {
        var ex1 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("0b101"));
        Assert.Contains("XPST0003", ex1.Message);
        var ex2 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("0xffff"));
        Assert.Contains("XPST0003", ex2.Message);
        var ex3 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("1_000"));
        Assert.Contains("XPST0003", ex3.Message);
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S3b: keyword arguments (XPath 4.0 §4.6.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_KeywordArgs_31Mode_ThrowsXpst0003()
    {
        var ex1 = Assert.Throws<XPathParseException>(
            () => XPath31Expression.Compile("fn:substring(value := 'abcdef', start := 2, length := 3)"));
        Assert.Contains("XPST0003", ex1.Message);
        var ex2 = Assert.Throws<XPathParseException>(
            () => XPath31Expression.Compile("sort((3, 1, 2), key := fn:data#1)"));
        Assert.Contains("XPST0003", ex2.Message);
    }

    [Fact]
    public void Compile_PositionalAfterKeyword_XPath40Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(
            () => Eval40("fn:substring(value := 'abcdef', 2)"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_KeywordArgs_FullyKeyworded_Works()
    {
        Assert.Equal("bcd", Eval40("fn:substring(value := 'abcdef', start := 2, length := 3)").ToString());
    }

    [Fact]
    public void Evaluate_KeywordArgs_MixedWithPositionals_Works()
    {
        Assert.Equal("bcd", Eval40("fn:substring('abcdef', start := 2, length := 3)").ToString());
        Assert.Equal("bcd", Eval40("fn:substring('abcdef', 2, length := 3)").ToString());
    }

    [Fact]
    public void Evaluate_KeywordArgs_OmittedParamTakesDeclaredDefault()
    {
        // F&O 4.0: $length := () means "to the end" for fn:substring and fn:subsequence.
        Assert.Equal("bcdef", Eval40("fn:substring(value := 'abcdef', start := 2)").ToString());
        Assert.Equal("3 4", Eval40("fn:string-join(fn:subsequence(input := (1, 2, 3, 4), start := 3), ' ')").ToString());
        // fn:string-join: $separator := ""
        Assert.Equal("123", Eval40("fn:string-join(values := (1, 2, 3))").ToString());
        // fn:contains: $collation := fn:default-collation()
        Assert.Equal("false", Eval40("fn:contains(value := 'abc', substring := 'B')").ToString());
        Assert.Equal("true", Eval40(
            "fn:contains(value := 'abc', substring := 'B', collation := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')").ToString());
        // fn:hash: $algorithm := "MD5"
        Assert.Equal("900150983CD24FB0D6963F7D28E17F72", Eval40("fn:hash(value := 'abc')").ToString());
    }

    [Fact]
    public void Evaluate_KeywordArgs_SortWithKeyFunction_Works()
    {
        Assert.Equal("1 2 3", Eval40("fn:string-join(fn:sort((3, 1, 2), key := fn:data#1), ' ')").ToString());
        Assert.Equal("1 2 3 4", Eval40("fn:string-join((4, 3, 2, 1) => fn:sort(key := fn:data#1), ' ')").ToString());
    }

    [Fact]
    public void Evaluate_KeywordArgs_MapMerge_Works()
    {
        Assert.Equal("12", Eval40(
            "fn:string-join(map:merge((map{'a': 1}, map{'a': 2}), options := map{'duplicates': 'combine'})?a)").ToString());
        Assert.Equal("1", Eval40(
            "((map{'a': 1}) => map:merge(options := map{'duplicates': 'use-last'}))?a").ToString());
    }

    [Fact]
    public void Evaluate_KeywordArgs_SliceAndParseUri_Work()
    {
        Assert.Equal("a b c d", Eval40("fn:string-join(fn:slice(input := ('a', 'b', 'c', 'd'), start := 0, step := 1), ' ')").ToString());
        Assert.Equal("http", Eval40("fn:parse-uri(uri := 'http://example.com/')?scheme").ToString());
    }

    [Fact]
    public void Compile_DuplicateKeyword_XPath40Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => XPath31Expression.Compile("fn:sort((1), key := fn:data#1, key := fn:data#1)",
                new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_UnknownKeyword_XPath40Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => XPath31Expression.Compile("fn:substring(input := 'x', bogus := 1)",
                new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_KeywordMatchingPositionalParam_XPath40Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => XPath31Expression.Compile("fn:substring('abcdef', 2, start := 3)",
                new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_UnmatchedRequiredParam_XPath40Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => XPath31Expression.Compile("fn:substring(start := 2)",
                new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_KeywordOnFunctionWithoutKeywordSignature_XPath40Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => XPath31Expression.Compile("fn:count(input := 1)",
                new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_KeywordsOnDynamicCall_XPath40Mode_ThrowsXpst0017()
    {
        var ex = Assert.Throws<XPathParseException>(
            () => XPath31Expression.Compile("(fn:substring#2)(input := 'x', start := 1)",
                new CompileOptions { Compatibility = XPathCompatibility.XPath40 }));
        Assert.Contains("XPST0017", ex.Message);
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S3b: string templates (XPath 4.0 §4.10.2)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_StringTemplate_31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("`abc`"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_StringTemplate_FixedOnly_Works()
    {
        Assert.Equal("hello", Eval40("`hello`").ToString());
        Assert.Equal("", Eval40("``").ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_Interpolation_Works()
    {
        Assert.Equal("Pi is 3.1416", Eval40("`Pi is {round(math:pi(), 4)}`").ToString());
        Assert.Equal("values: 1 3 5", Eval40("`values: {(1, 3, 5)}`").ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_EmptyInterpolation_ContributesNothing()
    {
        Assert.Equal("ab", Eval40("`a{}b`").ToString());
        Assert.Equal("ab", Eval40("`a{   }b`").ToString());
        Assert.Equal("ab", Eval40("`a{(: a comment :) }b`").ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_Escapes_Work()
    {
        Assert.Equal("a{b}c", Eval40("`a{{b}}c`").ToString());
        Assert.Equal("x`y", Eval40("`x``y`").ToString());
        Assert.Equal("{literal} ` here", Eval40("`{{literal}} `` here`").ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_BothQuoteKindsNeedNoEscaping()
    {
        Assert.Equal("He said: \"I didn't.\"", Eval40("`He said: \"I didn't.\"`").ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_NestedTemplateInInterpolation_Works()
    {
        Assert.Equal("inner 2", Eval40("`{`inner {1 + 1}`}`").ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_NodeInterpolationIsAtomized()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a>5</a></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var ctx = new EvaluationContext().WithFocus(XdmValue.FromNode(root), 1, 1);
        var expr = XPath31Expression.Compile("`val={/root/a}`", new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("val=5", expr.Evaluate(ctx).ToString());
    }

    [Fact]
    public void Evaluate_StringTemplate_StringsAndCommentsInsideInterpolation_Work()
    {
        Assert.Equal("a}{b", Eval40("`a{'}' || '{' }b`").ToString());
        Assert.Equal("x1", Eval40("`x{(: c :) 1}`").ToString());
    }
}
