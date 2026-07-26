// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Tests verifying the XQuery project compiles, links, and executes basic queries end-to-end.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-06-2026     | Creation — placeholder skeleton                                                          |
//                      | Charles Korthout | 0.2   | 22-07-2026     | Added first end-to-end FLWOR query test                                                  |
//                      | Charles Korthout | 0.3   | 22-07-2026     | Added order by clause tests                                                              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 23-07-2026     | Added count clause tests                                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 25-07-2026     | Added group by clause tests                                                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 25-07-2026     | Added window clause tests                                                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 25-07-2026     | Window tests use input-sequence end positions; no-end-condition and XQST0103 tests      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 25-07-2026     | Added 'as' type declaration, entity reference, base-uri, collation, stable order tests  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 25-07-2026     | Added g-date group/distinct and map-key timezone-presence tests                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.0   | 25-07-2026     | Added direct constructor tests (elements, attributes, comments, PIs, scoping)          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.XQuery.Tests;

public class PlaceholderTests
{
    [Fact]
    public void XQueryCompiler_CanBeInstantiated()
    {
        var compiler = new XQueryCompiler();
        Assert.NotNull(compiler);
    }

    [Fact]
    public void XQueryCompiler_Compile_ReturnsExecutable()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("1 + 1");
        Assert.NotNull(executable);
    }

    [Fact]
    public void XQueryExecutable_Evaluate_ReturnsXdmValue()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("'hello'");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);
        Assert.True(result.Kind == XdmValueKind.String || result.Kind == XdmValueKind.Sequence);
    }

    [Fact]
    public void XQuery_For_ReturnsSequence()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in 1 to 3 return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var sequence = XdmSequence.FromSource(result.SequenceValue!);
        var items = new List<long>();
        foreach (var item in sequence)
        {
            Assert.Equal(XdmValueKind.Integer, item.Kind);
            items.Add(item.IntegerValue);
        }

        Assert.Equal(new[] { 1L, 2L, 3L }, items);
    }

    [Fact]
    public void XQuery_Let_ReturnsBoundValue()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("let $x := 42 return $x");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(42L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareNamespace_ResolvesFunction()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("declare namespace math = 'http://www.w3.org/2005/xpath-functions/math'; math:pi()");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.True(result.DoubleValue > 3.14 && result.DoubleValue < 3.15);
    }

    [Fact]
    public void XQuery_ForLet_Mixed_ReturnsSequence()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3) let $j := $i * 2 return $j");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 2L, 4L, 6L }, items);
    }

    [Fact]
    public void XQuery_OrderBy_Ascending_Integers()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (3, 1, 2) order by $i return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 3L }, items);
    }

    [Fact]
    public void XQuery_OrderBy_Descending_Integers()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (3, 1, 2) order by $i descending return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 3L, 2L, 1L }, items);
    }

    [Fact]
    public void XQuery_OrderBy_Strings()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $s in ('cherry', 'apple', 'banana') order by $s return $s");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToStrings(result);
        Assert.Equal(new[] { "apple", "banana", "cherry" }, items);
    }

    [Fact]
    public void XQuery_OrderBy_WithWhere()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (5, 1, 4, 2, 3) where $i > 2 order by $i return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 3L, 4L, 5L }, items);
    }

    [Fact]
    public void XQuery_OrderBy_WithLet()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (3, 1, 2) let $d := $i * 2 order by $d descending return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 3L, 2L, 1L }, items);
    }

    [Fact]
    public void XQuery_OrderBy_MultipleKeys()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (2, 1, 2, 1) order by $i, -$i return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 1L, 2L, 2L }, items);
    }

    [Fact]
    public void XQuery_Count_Simple()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in ('a', 'b', 'c') count $n return $n");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 3L }, items);
    }

    [Fact]
    public void XQuery_Count_WithWhere()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (5, 1, 4, 2, 3) where $i > 2 count $n return $n");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 3L }, items);
    }

    [Fact]
    public void XQuery_Count_WithLet()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3) let $j := $i * 2 count $n return ($j, $n)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 2L, 1L, 4L, 2L, 6L, 3L }, items);
    }

    [Fact]
    public void XQuery_Count_PreOrderBy()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (3, 1, 2) count $n order by $i return ($i, $n)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 2L, 3L, 3L, 1L }, items);
    }

    [Fact]
    public void XQuery_Count_PostOrderBy()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (3, 1, 2) order by $i count $n return ($i, $n)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 1L, 2L, 2L, 3L, 3L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_Simple()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3, 4, 5, 6) group by $g := $i mod 2 return ($g, count($i))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 3L, 0L, 3L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_ByComputedStringKey()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $w in ('apple', 'avocado', 'banana', 'cherry', 'apricot') group by $k := substring($w, 1, 1) return $k || ':' || count($w)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToStrings(result);
        Assert.Equal(new[] { "a:3", "b:1", "c:1" }, items);
    }

    [Fact]
    public void XQuery_GroupBy_AggregatesNonGroupingVariables()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3, 4) group by $g := $i mod 2 return ($g, $i)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 1L, 3L, 0L, 2L, 4L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_WithWhere()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3, 4, 5, 6) where $i > 2 group by $g := $i mod 2 return ($g, count($i))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 0L, 2L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_WithOrderBy()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3, 4, 5, 6) group by $g := $i mod 3 order by $g return ($g, count($i))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 0L, 2L, 1L, 2L, 2L, 2L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_WithCount()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1 to 6) group by $g := $i mod 2 count $n return ($g, $n)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 1L, 0L, 2L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_MultipleSpecs()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (1, 2, 3, 4, 5) group by $a := $i mod 2, $b := $i gt 3 return $a || ':' || $b || ':' || count($i)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToStrings(result);
        Assert.Equal(new[] { "1:false:2", "0:false:1", "0:true:1", "1:true:1" }, items);
    }

    [Fact]
    public void XQuery_GroupBy_XPathMode_Rejected()
    {
        var ex = Assert.ThrowsAny<Exception>(() => XPathParser.Parse("for $i in (1, 2) group by $i return $i"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_Window_Tumbling_Simple()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w in (2, 4, 6, 8, 10) start at $s when true() end at $e when $e - $s = 1 return count($w)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 2L, 2L, 1L }, items);
    }

    [Fact]
    public void XQuery_Window_Tumbling_OnlyEnd()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w in (2, 4, 6, 8, 10) start at $s when true() only end at $e when $e - $s = 1 return count($w)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 2L, 2L }, items);
    }

    [Fact]
    public void XQuery_Window_Sliding_Overlapping()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for sliding window $w in (1, 2, 3) start at $s when true() end at $e when $e - $s = 1 return string-join($w, ',')");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToStrings(result);
        Assert.Equal(new[] { "1,2", "2,3", "3" }, items);
    }

    [Fact]
    public void XQuery_Window_StartEndVariables()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w in (5, 6, 7, 8) start $s at $sp when true() end $e at $ep when $ep = 2 return ($sp, $ep, $w)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 5L, 6L, 3L, 4L, 7L, 8L }, items);
    }

    [Fact]
    public void XQuery_Window_PreviousNext()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w in (1, 2, 3, 4) start $s next $n when $n = 3 end $e when true() return $w");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 2L }, items);
    }

    [Fact]
    public void XQuery_Window_WithOrderBy()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w in (3, 1, 2, 4) start when true() end at $p when $p = 2 order by sum($w) descending return sum($w)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 6L, 4L }, items);
    }

    [Fact]
    public void XQuery_Window_Tumbling_NoEndCondition()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w in (2, 4, 6, 8, 10, 12, 14) start $first when $first mod 3 = 0 return string-join($w, ',')");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToStrings(result);
        Assert.Equal(new[] { "6,8,10", "12,14" }, items);
    }

    [Fact]
    public void XQuery_Window_DuplicateVariable_Rejected()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() => compiler.Compile("for tumbling window $w in (1, 2) start $s when true() end $s when true() return $w"));
        Assert.Contains("XQST0103", ex.Message);
    }

    [Fact]
    public void XQuery_Window_XPathMode_Rejected()
    {
        var ex = Assert.ThrowsAny<Exception>(() => XPathParser.Parse("for tumbling window $w in (1, 2) start when true() end when true() return $w"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_ForBinding_TypeDeclaration_Matches()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $x as xs:integer in (1, 2, 3) return $x * 2");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 2L, 4L, 6L }, items);
    }

    [Fact]
    public void XQuery_ForBinding_TypeDeclaration_Mismatch_Throws()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $x as xs:integer in (1, 'a') return $x");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_LetBinding_TypeDeclaration_Mismatch_Throws()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("let $x as xs:integer := 'a' return $x");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_QuantifiedBinding_TypeDeclaration_Mismatch_Throws()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("every $a as xs:anyURI in (1 to 5) satisfies $a - 10");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_StringLiteral_EntityReferences()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("'fish &amp; chips &lt;3 &#65;&#x42;'");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("fish & chips <3 AB", result.StringValue);
    }

    [Fact]
    public void XQuery_StringLiteral_InvalidReference_Rejected()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() => compiler.Compile("'a string &;'"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareBaseUri_SetsStaticBaseUri()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("declare base-uri 'http://example.com/base'; static-base-uri()");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("http://example.com/base", result.ToString());
    }

    [Fact]
    public void XQuery_OrderBy_UnknownCollation_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $s in ('a', 'b') order by $s collation 'http://example.org/bogus' return $s");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XQST0076", ex.Message);
    }

    [Fact]
    public void XQuery_StableOrderBy()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $i in (3, 1, 2) stable order by $i return $i");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = ToIntegers(result);
        Assert.Equal(new[] { 1L, 2L, 3L }, items);
    }

    [Fact]
    public void XQuery_GroupBy_DeclaredType_Mismatch_Throws()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for $x in (1, 'a', 2) group by $g as xs:integer := $x return $g");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_GroupBy_GDateKeys_UseImplicitTimezone()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("let $date := adjust-date-to-timezone(xs:date(\"2015-10-10\"), implicit-timezone()) let $keys := ($date cast as xs:gYear, xs:gYear(\"2015\"), xs:gYear(\"2014\")) return count(for $k in $keys group by $k return $k)");
        var ctx = new XQueryContext();
        ctx.EvaluationContext.ImplicitTimezoneOffsetMinutes = 120;
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(2L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DistinctValues_GDates_UseImplicitTimezone()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("let $date := adjust-date-to-timezone(xs:date(\"2015-10-10\"), implicit-timezone()) let $keys := ($date cast as xs:gYear, xs:gYear(\"2015\"), xs:gYear(\"2014\")) return count(distinct-values($keys))");
        var ctx = new XQueryContext();
        ctx.EvaluationContext.ImplicitTimezoneOffsetMinutes = 120;
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(2L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_MapKeys_TimezonePresence_IsSignificant()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("map:size(map:merge((map:entry(xs:time('01:30:00'), 1), map:entry(adjust-time-to-timezone(xs:time('01:30:00'), implicit-timezone()), 2))))");
        var ctx = new XQueryContext();
        ctx.EvaluationContext.ImplicitTimezoneOffsetMinutes = 120;
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(2L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_Constructor_Simple()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("<out>hello</out>");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsNode, "Expected an element node result.");
        Assert.Equal("out", result.NodeValue.LocalName);
        Assert.Equal("hello", result.NodeValue.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_ComputedAttributeAndContent()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("let $e := <out a=\"two {1 + 1}\">{ for $i in (1, 2) return <item n=\"{$i}\">{$i}</item> }</out> return ($e/@a, $e/item[1]/@n, string($e))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.IsSequence, "Expected a sequence result.");
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(new[] { "two 2", "1", "12" }, items);
    }

    [Fact]
    public void XQuery_Constructor_NestedAtomicJoining()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<out>{ (1, 2, 3) }{ (4, 5) }</out>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("1 2 34 5", result.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_BoundaryWhitespaceStripped()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<out>  { 1 }  </out>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("1", result.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_XmlSpacePreserve()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<out xml:space=\"preserve\">  { 1 }  </out>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("  1  ", result.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_CommentAndPI()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<out><!-- c --><?pi d?>text</out>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("text", result.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_StandalonePIConstructor()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<pi>{<?pi x?>}</pi>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_EntityReferencesAndBraceEscapes()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<out>fish &amp; chips {{ok}}</out>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("fish & chips {ok}", result.StringValue);
    }

    [Fact]
    public void XQuery_Constructor_DuplicateAttribute_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("<out a=\"1\" a=\"2\"/>");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XQDY0025", ex.Message);
    }

    [Fact]
    public void XQuery_Constructor_UndeclaredPrefix_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("<foo:out/>");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XPST0081", ex.Message);
    }

    [Fact]
    public void XQuery_Constructor_MismatchedTag_Rejected()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() => compiler.Compile("<a></b>"));
        Assert.Contains("XQST0118", ex.Message);
    }

    [Fact]
    public void XQuery_ForBinding_AllowingEmpty()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("count(for $x allowing empty in () return 1)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(1L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_Predicate_NodeResult_AlwaysTrue()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("count(<root><a/><b/></root>/*[self::*])");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(2L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_Window_VariableDoesNotLeak_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("for tumbling window $w1 in (for tumbling window $w in (1 to 4) start when true() end at $p when $p = 2 return $w) start when true() end at $p when $p = 2 return $w");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XPST0008", ex.Message);
    }

    private static List<long> ToIntegers(XdmValue value)
    {
        var sequence = XdmSequence.FromSource(value.SequenceValue!);
        var items = new List<long>();
        foreach (var item in sequence)
        {
            Assert.Equal(XdmValueKind.Integer, item.Kind);
            items.Add(item.IntegerValue);
        }
        return items;
    }

    private static List<string> ToStrings(XdmValue value)
    {
        var sequence = XdmSequence.FromSource(value.SequenceValue!);
        var items = new List<string>();
        foreach (var item in sequence)
        {
            Assert.Equal(XdmValueKind.String, item.Kind);
            items.Add(item.StringValue);
        }
        return items;
    }
}
