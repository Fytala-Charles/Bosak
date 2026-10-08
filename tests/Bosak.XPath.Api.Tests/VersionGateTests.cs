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
}
