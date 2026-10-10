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
//                      | Charles Korthout | 0.6   | 08-10-2026     | REQ-118 4.0-S4: pipeline '->' (§4.20), mapping arrow '=!>' (§4.22.2), focus functions    |
//                      |                  |       |                | (§4.6.6.1), 'for member'/'for key value' (§4.14.1) — semantics + 3.1 XPST0003 gates    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 08-10-2026     | REQ-118 4.0-S5: higher-order fn:some/every/index-where/partition/take-while/drop-      |
//                      |                  |       |                | while/while-do/do-until/partial-apply/transitive-closure XPST0017 gate (3.1 mode)      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 08-10-2026     | fn:identity XPST0017 gate row (4.0-S5 follow-up)                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 08-10-2026     | REQ-118 4.0-S6a: enum types (§3.2.6) + choice item types (§3.2.5): 3.1 XPST0003 gates,  |
//                      |                  |       |                | instance-of, cast/castable, function-parameter coercion (in-order §3.4.2 rule 02)       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 08-10-2026     | REQ-118 4.0-S6b: structural record types (§3.2.10) + 'but with' (§4.15.4): 3.1        |
//                      |                  |       |                | XPST0003 gates (record(...), cast targets, 'but with') + 4.0 semantics smoke           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 09-10-2026     | REQ-123 4.0-Exp S1: fn:scan is experimental-only — XPST0017 at 3.1 AND frozen        |
//                      |                  |       |                | XPath40 (call + named-function-ref forms), works at XPath40Experimental               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 09-10-2026     | REQ-123 fn:op slice: frozen-level gate rows (XPST0017 at 3.1, works at frozen        |
//                      |                  |       |                | XPath40 — NOT experimental)                                                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13  | 09-10-2026     | REQ-123 parse-csv slice: frozen-level gate rows for fn:parse-csv/fn:csv-to-xml/       |
//                      |                  |       |                | fn:csv-doc (XPST0017 at 3.1, works at frozen XPath40 — NOT experimental)                |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.14  | 09-10-2026     | REQ-123 element-to-map slice: frozen-level gate rows for fn:element-to-map/fn:map-to-   |
//                      |                  |       |                | element/fn:element-to-map-plan/fn:jvalue (XPST0017 at 3.1, works at frozen XPath40)    |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.15  | 10-10-2026     | REQ-123 fn:atomic-equal slice: frozen-level gate rows for fn:atomic-equal (XPST0017 at   |
//                      |                  |       |                | 3.1, works at frozen XPath40 — NOT experimental)                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.16  | 10-10-2026     | REQ-123 focus-constructors slice: arity-0 xs:* constructors (PR661) XPST0017 at 3.1 +   |
//                      |                  |       |                | arity-1 regression rows (arity-aware gate) + frozen XPath40 works                         |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.17  | 10-10-2026     | REQ-123 sort-with slice: frozen-level gate rows for fn:sort-with/array:sort-with/         |
//                      |                  |       |                | fn:atomic-type-annotation/fn:is-NaN + fn:compare 3.1 regression rows (arity-2/3 strings)  |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.18  | 10-10-2026     | REQ-123 compare-tail slice: fn:collation-available XPST0017 at 3.1 + frozen-XPath40      |
//                      |                  |       |                | works; # QName literals XPST0003 at 3.1 + frozen-XPath40 works                            |
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

    // Evaluates in 4.0 mode and stringifies every item of the result sequence.
    private static List<string> Seq40(string xpath)
    {
        var result = Eval40(xpath);
        var items = new List<string>();
        if (result.IsSequence && result.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                items.Add(item.ToString());
        }
        else if (!result.IsUndefined)
        {
            items.Add(result.ToString());
        }
        return items;
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

    // ------------------------------------------------------------------
    // REQ-118 4.0-S5: higher-order functions are 4.0-only
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:some((1, 2), fn:boolean#1)")]
    [InlineData("fn:every((1, 2), fn:boolean#1)")]
    [InlineData("fn:index-where((1, 2), fn:boolean#1)")]
    [InlineData("fn:partition((1, 2), fn:boolean#2)")]
    [InlineData("fn:take-while((1, 2), fn:boolean#1)")]
    [InlineData("fn:drop-while((1, 2), fn:boolean#1)")]
    [InlineData("fn:while-do(1, fn:boolean#1, fn:identity#1)")]
    [InlineData("fn:do-until(1, fn:identity#1, fn:boolean#1)")]
    [InlineData("fn:partial-apply(fn:concat#2, map{})")]
    [InlineData("fn:transitive-closure((), fn:identity#1)")]
    [InlineData("fn:identity((1, 2))")]
    public void Compile_HigherOrderFunctions_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Compile_XPath30Mode_AlsoRejectsXPath40Functions()
    {
        var options = new CompileOptions { Compatibility = XPathCompatibility.XPath30 };
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("fn:foot((1, 2))", options));
        Assert.Contains("XPST0017", ex.Message);
    }

    // ------------------------------------------------------------------
    // REQ-123 4.0-Exp S1: experimental-only functions are rejected at 3.1
    // AND at the frozen XPath40 level; opt-in via XPath40Experimental
    // ------------------------------------------------------------------

    private static XdmValue Eval40Exp(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40Experimental });
        return expr.Evaluate(new EvaluationContext());
    }

    [Theory]
    [InlineData("fn:scan((1, 2), 0, function($a, $b, $p) { $a + $b })")]
    [InlineData("fn:scan#3")]
    public void Compile_ExperimentalFunctions_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Theory]
    [InlineData("fn:scan((1, 2), 0, function($a, $b, $p) { $a + $b })")]
    [InlineData("fn:scan#3")]
    public void Compile_ExperimentalFunctions_FrozenXPath40_ThrowXpst0017(string expression)
    {
        var options = new CompileOptions { Compatibility = XPathCompatibility.XPath40 };
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression, options));
        Assert.Contains("XPST0017", ex.Message);
        Assert.Contains("XPath40Experimental", ex.Message);
    }

    [Fact]
    public void Evaluate_Scan_XPath40Experimental_Works()
    {
        Assert.Equal("15",
            Eval40Exp("fn:foot(fn:scan(1 to 5, 0, function($a, $b, $p) { $a + $b })?*)").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-123 fn:op slice: frozen-level 4.0 function — XPST0017 at 3.1,
    // available at the frozen XPath40 level (NOT experimental)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:op('+')(2, 2)")]
    [InlineData("fn:op#1")]
    public void Compile_Frozen40Functions_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Evaluate_Op_FrozenXPath40_Works()
    {
        var expr = XPath31Expression.Compile(
            "fn:op('+')(2, 2)",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("4", expr.Evaluate(new EvaluationContext()).ToString());
    }

    // ------------------------------------------------------------------
    // REQ-123 parse-csv slice: frozen-level 4.0 CSV functions — XPST0017
    // at 3.1, available at the frozen XPath40 level (NOT experimental)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:parse-csv('a,b')")]
    [InlineData("fn:parse-csv#2")]
    [InlineData("fn:csv-to-xml('a,b')")]
    [InlineData("fn:csv-doc('a.csv')")]
    public void Compile_CsvFunctions_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Evaluate_ParseCsv_FrozenXPath40_Works()
    {
        var expr = XPath31Expression.Compile(
            "fn:parse-csv('a,b')?rows[1]?2",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("b", expr.Evaluate(new EvaluationContext()).ToString());
    }

    [Fact]
    public void FunctionLookup_ScanIn31AndFrozen40_ReturnsEmpty()
    {
        Assert.True(Eval("fn:function-lookup(xs:QName('fn:scan'), 3)").IsUndefined);
        var frozen40 = XPath31Expression.Compile(
            "fn:function-lookup(xs:QName('fn:scan'), 3)",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.True(frozen40.Evaluate(new EvaluationContext()).IsUndefined);
    }

    [Fact]
    public void FunctionLookup_ScanInXPath40Experimental_ReturnsFunction()
    {
        var result = Eval40Exp("fn:function-lookup(xs:QName('fn:scan'), 3)");
        Assert.True(result.IsFunction);
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

    // ------------------------------------------------------------------
    // REQ-118 4.0-S4: pipeline operator '->' (XPath 4.0 §4.20)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_PipelineArrow_31Mode_ThrowsXpst0003()
    {
        var ex1 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("'a b c' -> tokenize(.)"));
        Assert.Contains("XPST0003", ex1.Message);
        var options = new CompileOptions { Compatibility = XPathCompatibility.XPath30 };
        var ex2 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("1 -> count(.)", options));
        Assert.Contains("XPST0003", ex2.Message);
    }

    [Fact]
    public void Evaluate_Pipeline_SpecExample_TokenizeCountConcat()
    {
        Assert.Equal("count=3", Eval40("'a b c' -> tokenize(.) -> count(.) -> concat('count=', .)").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_SpecExample_SumOfPowers()
    {
        Assert.Equal("2046", Eval40("(1 to 10) ! math:pow(2, .) -> sum(.)").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_SpecExample_ReduceWithIf()
    {
        Assert.Equal("a; b; c", Eval40("('a', 'b', 'c') -> (if (count(.) lt 10) then string-join(., '; ') else 'long')").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_SpecExample_MapReduceChain()
    {
        Assert.Equal("THE. CAT. SAT. ON. THE. MAT.",
            Eval40("\"The cat sat on the mat\" => tokenize() =!> concat('.') =!> upper-case() => string-join(' ')").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_BindsWholeSequenceAsContextValue()
    {
        Assert.Equal("3", Eval40("(1, 2, 3) -> count(.)").ToString());
        Assert.Equal("0", Eval40("() -> count(.)").ToString());
        // A predicate on '.' applies to the whole bound sequence, not per item.
        Assert.Equal("2", Eval40("(1, 2, 3) -> .[2]").ToString());
        // (1, 2, 3) => avg() and (1, 2, 3) -> avg(.) both yield the whole-sequence average.
        Assert.Equal("2", Eval40("(1, 2, 3) -> avg(.)").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_SetsFixedFocusPositionOneSizeOne()
    {
        Assert.Equal("1 1", Eval40("(5, 6, 7) -> string-join((string(position()), string(last())), ' ')").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_LeftAssociativeAndPrecedence()
    {
        // '->' binds tighter than '+' (PipelineExpr is an operand of the additive level).
        Assert.Equal("3", Eval40("(1, 2) -> count(.) + 1").ToString());
        // Chained pipelines are left-associative.
        Assert.Equal("count=3", Eval40("'a b c' -> tokenize(.) -> concat('count=', string(count(.)))").ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_RhsMayUseAnyExpression()
    {
        Assert.Equal("9", Eval40("(1, 2, 3) -> (sum(.) + count(.))").ToString());
        // The pipeline RHS is an ArrowExpr per the spec grammar; a FLWOR RHS needs parens.
        Assert.Equal(["10", "20", "30"], Seq40("(1, 2, 3) -> (for $x in . return $x * 10)"));
    }

    [Fact]
    public void Evaluate_Pipeline_LhsErrorPropagates()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("(1 div 0) -> count(.)"));
        Assert.Contains("FOAR0001", ex.Message);
    }

    [Fact]
    public void Evaluate_Pipeline_FocusRestoredAfterEvaluation()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var ctx = new EvaluationContext().WithFocus(XdmValue.FromNode(root), 1, 1);
        var expr = XPath31Expression.Compile("('x', 'y') -> count(.) || name(.)",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("2root", expr.Evaluate(ctx).ToString());
    }

    [Fact]
    public void Evaluate_Pipeline_CastPrecedenceFromSpecGrammar()
    {
        // CastExpr ::= PipelineExpr ("cast" "as" CastTarget)? — the cast operand is a pipeline.
        Assert.Equal("3", Eval40("(1, 2, 3) -> count(.) cast as xs:integer").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S4: mapping arrow '=!>' (XPath 4.0 §4.22.2)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_MappingArrow_31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("(1, 2, 3) =!> avg()"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_MappingArrow_AppliesFunctionPerItem()
    {
        // (1, 2, 3) =!> avg() ≡ (1, 2, 3) ! avg(.) — each item averaged with itself.
        var result = Eval40("(1, 2, 3) =!> avg()");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(["1", "2", "3"], items);
    }

    [Fact]
    public void Evaluate_MappingArrow_WithArgumentsPrependsContextItem()
    {
        Assert.Equal("1! 2! 3!", Eval40("(1, 2, 3) =!> concat('!') => string-join(' ')").ToString());
        Assert.Equal("aX bX", Eval40("('a', 'b') =!> concat('X') => string-join(' ')").ToString());
    }

    [Fact]
    public void Evaluate_MappingArrow_DynamicAndInlineFunctionTargets()
    {
        Assert.Equal("2 4", Eval40("(1, 2) =!> (function($x) { $x * 2 })() => string-join(' ')").ToString());
        Assert.Equal(["2", "2.414213562373095", "2.732050807568877", "3", "3.23606797749979"],
            Seq40("(1 to 5) =!> xs:double() =!> math:sqrt() =!> fn($a) { $a + 1 }()"));
    }

    [Fact]
    public void Evaluate_MappingArrow_SingletonSourceSameAsSequenceArrow()
    {
        Assert.Equal(["ABC"], Seq40("'abc' =!> upper-case()"));
        Assert.Equal("ABC", Eval40("'abc' => upper-case()").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S4: focus functions fn { E } / fn ( XPath 4.0 §4.6.6, §4.6.6.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_FocusFunction_31Mode_ThrowsXpst0003()
    {
        var ex1 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("fn { . + 1 }"));
        Assert.Contains("XPST0003", ex1.Message);
        var ex2 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("fn($a) { $a + 1 }"));
        Assert.Contains("XPST0003", ex2.Message);
        var ex3 = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("function { . + 1 }"));
        Assert.Contains("XPST0003", ex3.Message);
    }

    [Fact]
    public void Evaluate_FocusFunction_SpecExamples()
    {
        // fn:every is a separate F&O 4.0 addition (not yet implemented); the spec's
        // focus-function example here uses fn:for-each instead.
        Assert.Equal(["10", "20", "30"], Seq40("fn:for-each((1, 2, 3), fn { . * 10 })"));
        Assert.Equal("4", Eval40("fn { . + 1 }(3)").ToString());
        Assert.Equal("5", Eval40("function { . + 1 }(4)").ToString());
    }

    [Fact]
    public void Evaluate_FocusFunction_BindsArgumentAsWholeContextValue()
    {
        // The argument is bound to the context value as a whole (position 1, size 1).
        Assert.Equal("1 1 2", Eval40("fn { string-join((string(position()), string(last()), string(count(.))), ' ') }((9, 8))").ToString());
    }

    [Fact]
    public void Evaluate_FocusFunction_AfterMappingArrow()
    {
        Assert.Equal("\"a\" \"b\"", Eval40("'a b' => tokenize() =!> fn { concat('\"', ., '\"') }() => string-join(' ')").ToString());
    }

    [Fact]
    public void Evaluate_FocusFunction_WrongArityThrows()
    {
        Assert.Throws<InvalidOperationException>(() => Eval40("fn { . }(1, 2)"));
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S4: 'for member' (XPath 4.0 §4.14.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_ForMember_31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("for member $m in [1, 2] return $m"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_ForMember_IteratesMembers()
    {
        Assert.Equal(["2", "4", "6"], Seq40("for member $m in [1, 2, 3] return $m * 2"));
        Assert.Equal("0", Eval40("count(for member $m in [] return $m)").ToString());
    }

    [Fact]
    public void Evaluate_ForMember_MultiItemAndArrayMembersAreBoundWhole()
    {
        Assert.Equal(["2", "1"], Seq40("for member $m in [(1, 2), 3] return count($m)"));
        Assert.Equal(["2", "1"], Seq40("for member $m in [[1, 2], [3]] return array:size($m)"));
    }

    [Fact]
    public void Evaluate_ForMember_SequenceOfArraysCountsPositionsAcrossArrays()
    {
        Assert.Equal(["1", "2", "3", "4"], Seq40("for member $m in ([1, 2], [3, 4]) return $m"));
        Assert.Equal(["1", "2", "3"], Seq40("for member $m at $p in ([5, 6], [7]) return $p"));
    }

    [Fact]
    public void Evaluate_ForMember_SpecExample_ParseJson()
    {
        Assert.Equal(["3", "30"], Seq40(
            "for member $map in parse-json('[{ \"x\": 1, \"y\": 2 }, { \"x\": 10, \"y\": 20 }]') return $map ! (?x + ?y)"));
    }

    [Fact]
    public void Evaluate_ForMember_NonArrayItemThrowsXpty0141()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("for member $m in (1, 2) return $m"));
        Assert.Contains("XPTY0141", ex.Message);
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S4: 'for key value' (XPath 4.0 §4.14.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_ForKeyValue_31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("for key $k value $v in map{'a': 1} return $v"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_ForKeyValue_IteratesEntries()
    {
        Assert.Equal(["x=1", "y=2"], Seq40("for key $k value $v in map{'x': 1, 'y': 2} return concat($k, '=', $v)"));
        Assert.Equal(["a"], Seq40("for key $k in map{'a': 1} return $k"));
        Assert.Equal(["1", "2"], Seq40("for value $v in map{'a': 1, 'b': 2} return $v"));
    }

    [Fact]
    public void Evaluate_ForKeyValue_PositionalCountsAcrossMaps()
    {
        Assert.Equal(["1", "2", "3"], Seq40(
            "for key $k value $v at $p in (map{'a': 1}, map{'b': 2, 'c': 3}) return $p"));
    }

    [Fact]
    public void Compile_ForKeyValue_DuplicateKeyValueNames_ThrowsXqst0089()
    {
        var ex = Assert.Throws<XPathParseException>(() => Eval40("for key $k value $k in map{'a': 1} return $k"));
        Assert.Contains("XQST0089", ex.Message);
    }

    [Fact]
    public void Evaluate_ForKeyValue_NonMapItemThrowsXpty0141()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("for key $k in (map{}, 'x') return $k"));
        Assert.Contains("XPTY0141", ex.Message);
    }

    [Fact]
    public void Evaluate_ForKeyValue_SpecExample_Template()
    {
        var result = Seq40("for key $key value $value in map{'x': 1, 'y': 2} return concat($key, '=', $value)");
        Assert.Contains("x=1", result);
        Assert.Contains("y=2", result);
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S6a: enum types (XPath 4.0 §3.2.6)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_EnumType_31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("\"red\" instance of enum(\"red\")"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Compile_EnumType_EmptyMemberList_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => Eval40("() instance of enum()"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_Enum_InstanceOf_MembershipIsCodepointSensitive()
    {
        Assert.Equal("true", Eval40("\"green\" instance of enum(\"red\", \"green\")").ToString());
        Assert.Equal("false", Eval40("\"yellow\" instance of enum(\"red\", \"green\")").ToString());
        Assert.Equal("false", Eval40("\"Red\" instance of enum(\"red\")").ToString());
        // xs:untypedAtomic is not an instance of xs:string, so it never matches (§3.2.6):
        Assert.Equal("false", Eval40("xs:untypedAtomic(\"red\") instance of enum(\"red\")").ToString());
    }

    [Fact]
    public void Evaluate_Enum_InstanceOf_OccurrenceIndicatorsApply()
    {
        Assert.Equal("true", Eval40("() instance of enum(\"red\")?").ToString());
        Assert.Equal("false", Eval40("() instance of enum(\"red\")").ToString());
        Assert.Equal("true", Eval40("(\"a\", \"b\") instance of enum(\"a\", \"b\")*").ToString());
        Assert.Equal("false", Eval40("(\"a\", \"c\") instance of enum(\"a\", \"b\")*").ToString());
    }

    [Fact]
    public void Evaluate_Enum_Cast_MemberSucceedsAsPlainString()
    {
        Assert.Equal("green", Eval40("\"green\" cast as enum(\"red\", \"green\")").ToString());
        // Instances are not re-annotated: the result is a plain xs:string.
        Assert.Equal("true", Eval40("(\"green\" cast as enum(\"red\", \"green\")) instance of xs:string").ToString());
    }

    [Fact]
    public void Evaluate_Enum_Cast_NonMemberFailsForg0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("\"yellow\" cast as enum(\"red\", \"green\")"));
        Assert.Contains("FORG0001", ex.Message);
    }

    [Fact]
    public void Evaluate_Enum_Castable_ReportsMembership()
    {
        Assert.Equal("true", Eval40("\"red\" castable as enum(\"red\", \"green\")").ToString());
        Assert.Equal("false", Eval40("\"yellow\" castable as enum(\"red\", \"green\")").ToString());
    }

    [Fact]
    public void Evaluate_Enum_Cast_NonStringAtomCoercedViaXsString()
    {
        // §3.4.2 rule 05: a non-string atom is cast to xs:string before membership is checked.
        Assert.Equal("5", Eval40("5 cast as enum(\"5\", \"6\")").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S6a: choice item types (XPath 4.0 §3.2.5)
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_ChoiceItemType_31Mode_ThrowsXpst0003()
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("5 instance of (xs:integer|xs:string)"));
        Assert.Contains("XPST0003", ex.Message);
        var castEx = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile("5 cast as (xs:integer|xs:string)"));
        Assert.Contains("XPST0003", castEx.Message);
    }

    [Fact]
    public void Evaluate_Choice_InstanceOf_AnyAlternativeMatches()
    {
        Assert.Equal("true", Eval40("5 instance of (xs:integer|xs:string)").ToString());
        Assert.Equal("true", Eval40("\"x\" instance of (xs:integer|xs:string)").ToString());
        Assert.Equal("false", Eval40("5.5 instance of (xs:integer|xs:string)").ToString());
        Assert.Equal("true", Eval40("(5, \"x\") instance of (xs:integer|xs:string)*").ToString());
        // Choices may mix function-family item types:
        Assert.Equal("true", Eval40("map{} instance of (map(*)|array(*))").ToString());
        Assert.Equal("true", Eval40("[1] instance of (map(*)|array(*))").ToString());
        Assert.Equal("false", Eval40("5 instance of (map(*)|array(*))").ToString());
    }

    [Fact]
    public void Evaluate_Choice_Cast_FirstCoercibleAlternativeWins()
    {
        Assert.Equal("true", Eval40("(\"2024-01-01\" cast as (xs:date|xs:dateTime)) instance of xs:date").ToString());
        // Declaration order: an integer input coerces to the FIRST alternative xs:string
        // (§3.4.2 rule 02 and the spec's fn:char example), even though it also matches
        // xs:positiveInteger.
        Assert.Equal("true", Eval40("(5 cast as (xs:string|xs:positiveInteger)) instance of xs:string").ToString());
    }

    [Fact]
    public void Evaluate_Choice_Cast_NoAlternativeSucceedsForg0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("\"nope\" cast as (xs:date|xs:dateTime)"));
        Assert.Contains("FORG0001", ex.Message);
        Assert.Equal("false", Eval40("\"nope\" castable as (xs:date|xs:dateTime)").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S6a: function parameter/return coercion (§3.4.2 rule 02)
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_Enum_FunctionArgument_MatchingMemberPasses()
    {
        Assert.Equal("green", Eval40("function($x as enum(\"red\", \"green\")) {$x}(\"green\")").ToString());
    }

    [Fact]
    public void Evaluate_Enum_FunctionArgument_NonMemberThrowsXpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Eval40("function($x as enum(\"red\", \"green\")) {$x}(\"yellow\")"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Evaluate_Enum_FunctionReturnType_Enforced()
    {
        Assert.Equal("ok", Eval40("function() as enum(\"ok\") {\"ok\"}()").ToString());
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("function() as enum(\"ok\") {\"nope\"}()"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Evaluate_Choice_FunctionArgument_InOrderCoercionYieldsString()
    {
        // The spec's fn:char semantics: an integer argument against
        // (xs:string|xs:positiveInteger) arrives coerced to the string.
        Assert.Equal("true", Eval40(
            "function($x as (xs:string|xs:positiveInteger)) {$x instance of xs:string}(5)").ToString());
    }

    [Fact]
    public void Evaluate_Choice_FunctionArgument_MixedNodeAndAtomicAlternatives()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a>hi</a></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var ctx = new EvaluationContext().WithFocus(XdmValue.FromNode(root), 1, 1);
        var expr = XPath31Expression.Compile(
            "function($x as (element(a)|xs:string)) {$x instance of xs:string}(/root/a)",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        // No node-kind alternative matches /root/a against the atomic branch semantics:
        // the node is atomized and coerced to xs:string.
        Assert.Equal("true", expr.Evaluate(ctx).ToString());
    }

    [Fact]
    public void Evaluate_Choice_FunctionArgument_AllNodeKindAlternativesPassNodeThrough()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var ctx = new EvaluationContext().WithFocus(XdmValue.FromNode(root), 1, 1);
        var expr = XPath31Expression.Compile(
            "function($x as (element(a)|element(b))) {$x instance of element(a)}(/root/a)",
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("true", expr.Evaluate(ctx).ToString());
    }

    // ------------------------------------------------------------------
    // REQ-118 4.0-S6b: structural record types + 'but with' (§3.2.10, §4.15.4)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("1 instance of record(a)")]
    [InlineData("1 instance of record(*)")]
    [InlineData("1 instance of record(a as xs:integer, b)")]
    [InlineData("1 cast as record(a)")]
    [InlineData("1 castable as record(*)")]
    [InlineData("map{\"a\": 1} but with map{\"a\": 2}")]
    public void Compile31_RecordTypesAndButWith_XPST0003(string xpath)
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile(xpath));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_RecordType_BasicSemanticsAvailableIn40()
    {
        // Cast creates an annotated record; instance-of and lookup work; 'but with'
        // merges entries under the record's annotation.
        Assert.Equal("true", Eval40("(map{\"a\": 1} cast as record(a)) instance of record(a)").ToString());
        Assert.Equal("2", Eval40("((map{\"a\": 1} cast as record(a)) but with map{\"a\": 2})?a").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-123 element-to-map slice: frozen-level 4.0 §17.6 functions —
    // XPST0017 at 3.1, available at the frozen XPath40 level (NOT experimental)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:element-to-map(parse-xml('<a/>')/a)")]
    [InlineData("fn:element-to-map#2")]
    [InlineData("fn:map-to-element(map{'a': '1'})")]
    [InlineData("fn:map-to-element#2")]
    [InlineData("fn:element-to-map-plan(parse-xml('<a/>')/a)")]
    [InlineData("fn:element-to-map-plan#1")]
    [InlineData("fn:jvalue(1)")]
    [InlineData("fn:jvalue#1")]
    public void Compile_ElementMapFunctions_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Evaluate_ElementToMap_FrozenXPath40_Works()
    {
        Assert.Equal("x",
            Eval40("fn:element-to-map(parse-xml('<a>x</a>')/a)?a").ToString());
        Assert.Equal("7",
            Eval40("""fn:element-to-map(parse-xml('<a id="7">x</a>')/a)?a?'@id'""").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-123 fn:atomic-equal slice: frozen-level 4.0 §2.2.1 function —
    // XPST0017 at 3.1, available at the frozen XPath40 level (NOT experimental)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:atomic-equal('a', 'a')")]
    [InlineData("fn:atomic-equal#2")]
    public void Compile_AtomicEqual_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void Evaluate_AtomicEqual_FrozenXPath40_Works()
    {
        Assert.Equal("true", Eval40("fn:atomic-equal(xs:double('NaN'), xs:float('NaN'))").ToString());
        Assert.Equal("false", Eval40("fn:atomic-equal(1.1, 1.1e0)").ToString());
        Assert.Equal("true",
            Eval40("fn:atomic-equal(xs:hexBinary('ff'), xs:base64Binary(xs:hexBinary('ff')))").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-123 focus-constructors slice: the arity-0 form of every built-in
    // xs:* constructor (spec PR661) is 4.0-only — XPST0017 at 3.1, available
    // at the frozen XPath40 level (NOT experimental). The arity-1 forms must
    // stay 3.1-legal (the version gate is arity-aware).
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("xs:integer()")]
    [InlineData("xs:string()")]
    [InlineData("xs:date()")]
    [InlineData("xs:unsignedLong()")]
    [InlineData("xs:QName()")]
    [InlineData("xs:integer#0")]
    [InlineData("xs:date#0")]
    public void Compile_FocusConstructors_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
        Assert.Contains("XPath40", ex.Message);
    }

    [Theory]
    [InlineData("xs:integer('42')")]
    [InlineData("xs:date('2026-10-10')")]
    [InlineData("xs:unsignedLong('42')")]
    [InlineData("xs:string(42)")]
    [InlineData("xs:integer#1")]
    public void Compile_ArityOneConstructors_DefaultOptions_StillWork(string expression)
    {
        var expr = XPath31Expression.Compile(expression);
        Assert.NotNull(expr);
    }

    [Fact]
    public void Evaluate_FocusConstructors_FrozenXPath40_Works()
    {
        Assert.Equal(new List<string> { "42" }, Seq40("'42' ! xs:integer()"));
        Assert.Equal(new List<string> { "2026-10-10" }, Seq40("xs:date('2026-10-10') ! xs:string()"));
        Assert.Equal(new List<string> { "true" }, Seq40("true() ! xs:boolean()"));
    }

    // ------------------------------------------------------------------
    // REQ-123 sort-with cluster: sort-with family, atomic-type-annotation,
    // is-NaN are 4.0-only (frozen level); fn:compare stays 3.1-compatible
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:sort-with((1, 2), fn:compare#2)")]
    [InlineData("fn:sort-with#2")]
    [InlineData("array:sort-with([1, 2], fn:compare#2)")]
    [InlineData("fn:atomic-type-annotation(1)")]
    [InlineData("fn:atomic-type-annotation#1")]
    [InlineData("fn:is-NaN(1)")]
    [InlineData("fn:is-NaN#1")]
    public void Compile_SortWithCluster_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
        Assert.Contains("XPath40", ex.Message);
    }

    [Theory]
    [InlineData("fn:compare('a', 'b')")]
    [InlineData("fn:compare('a', 'b', 'http://www.w3.org/2005/xpath-functions/collation/codepoint')")]
    [InlineData("fn:compare#2")]
    [InlineData("fn:compare#3")]
    [InlineData("fn:sort((3, 1, 2))")]
    [InlineData("array:sort([3, 1, 2])")]
    public void Compile_SortWithCluster_3Point1Functions_StillWork(string expression)
    {
        var expr = XPath31Expression.Compile(expression);
        Assert.NotNull(expr);
    }

    [Fact]
    public void Evaluate_SortWithCluster_FrozenXPath40_Works()
    {
        Assert.Equal(new List<string> { "1", "2", "3" }, Seq40("fn:sort-with((3, 1, 2), fn:compare#2)"));
        Assert.Equal(new List<string> { "1", "2" }, Seq40("array:sort-with([2, 1], fn:compare#2)?*"));
        Assert.Equal("true", Eval40("fn:is-NaN(0 div 0e0)").ToString());
        Assert.Equal("true", Eval40("fn:atomic-type-annotation(1)?name = xs:QName('xs:integer')").ToString());
    }

    // ------------------------------------------------------------------
    // REQ-123 compare-tail cluster: collation-available is 4.0-only
    // (frozen level); # QName literals are 4.0-only syntax (XPST0003 in 3.1)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("fn:collation-available('http://www.w3.org/2005/xpath-functions/collation/codepoint')")]
    [InlineData("fn:collation-available#1")]
    public void Compile_CollationAvailable_DefaultOptions_ThrowXpst0017(string expression)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0017", ex.Message);
        Assert.Contains("XPath40", ex.Message);
    }

    [Fact]
    public void Evaluate_CollationAvailable_FrozenXPath40_Works()
    {
        Assert.Equal("true", Eval40("fn:collation-available(fn:default-collation())").ToString());
        Assert.Equal("true", Eval40("fn:collation-available('http://www.w3.org/2005/xpath-functions/collation/unicode-case-insensitive')").ToString());
        Assert.Equal("false", Eval40("fn:collation-available('ftp://not-a-collation/')").ToString());
    }

    [Theory]
    [InlineData("#local")]
    [InlineData("#fn:null")]
    [InlineData("#xml:space")]
    [InlineData("#Q{http://www.example.com}ex:a")]
    public void Compile_QNameLiterals_DefaultOptions_ThrowXpst0003(string expression)
    {
        var ex = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile(expression));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void Evaluate_QNameLiterals_FrozenXPath40_Works()
    {
        Assert.Equal(new List<string> { "http://www.example.com", "ex", "a" },
            Seq40("#Q{http://www.example.com}ex:a ! (fn:namespace-uri-from-QName(.), fn:prefix-from-QName(.), fn:local-name-from-QName(.))"));
        Assert.Equal("true", Eval40("#xml:space eq xs:QName('xml:space')").ToString());
    }
}
