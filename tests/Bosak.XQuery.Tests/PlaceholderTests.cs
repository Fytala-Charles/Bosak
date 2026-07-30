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
//                      | Charles Korthout | 1.1   | 25-07-2026     | Added computed constructor and prefixed window variable tests                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 25-07-2026     | Added switch and typeswitch expression tests                                              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.3   | 25-07-2026     | Added output declaration and serialization tests                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.4   | 26-07-2026     | 12 unit tests for declare function/declare variable (happy paths and XQST/XPST/XPTY error codes) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 27-07-2026     | try/catch named-code and global-variable-not-caught tests |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 27-07-2026     | 12 string-constructor tests (literals, interpolations, escapes, errors) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.7   | 27-07-2026     | 8 ordered/unordered/ordering-declaration tests |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 28-07-2026     | 4 constructor namespace-semantics tests |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 29-07-2026     | 12 typed variable declaration and namespace undeclaration tests |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.10  | 29-07-2026     | 7 namespace declaration static-error tests (XQST0033/XQST0070/XPST0003) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.11  | 29-07-2026     | 7 annotation tests (inline annotations, assertions, XQST0045, XPath-mode rejection) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.12  | 29-07-2026     | 9 character/entity reference tests (expansion, XQST0090, XPST0003, XPath no-expansion) |
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
        // xml:space is an ordinary attribute for element constructors: it does not
        // preserve boundary whitespace (XQuery 3.1 §3.9.1.1; K2-Serialization-41).
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<out xml:space=\"preserve\">  { 1 }  </out>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("1", result.StringValue);
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

    [Fact]
    public void XQuery_Window_PrefixedVariable_Resolves()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare namespace window = \"foo:bar\";\n" +
            "string(for tumbling window $window:w in (1 to 3) start $s when true() end $e when false() return <w>{$window:w}</w>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("1 2 3", result.StringValue);
    }

    [Fact]
    public void XQuery_ComputedElement_StaticName()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("name(element foo { \"text\" })");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("foo", result.StringValue);
    }

    [Fact]
    public void XQuery_ComputedElement_ComputedNameAndAtomicContent()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("(name(element { \"dyn\" } { 1, 2, 3 }), string(element { \"dyn\" } { 1, 2, 3 }))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(new[] { "dyn", "1 2 3" }, ToStrings(result));
    }

    [Fact]
    public void XQuery_ComputedElement_EQNameName()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("(name(element { \"Q{http://u}x\" } {}), namespace-uri(element { \"Q{http://u}x\" } {}))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(new[] { "x", "http://u" }, ToStrings(result));
    }

    [Fact]
    public void XQuery_ComputedElement_EmptyContent()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("(string(count(element e {})), string(element e {}))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(new[] { "1", "" }, ToStrings(result));
    }

    [Fact]
    public void XQuery_ComputedElement_TextNodeMergesWithoutSeparator()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(element e { 7, text{\"t\"}, text{\"t\"}, 8 })");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("7tt8", result.StringValue);
    }

    [Fact]
    public void XQuery_ComputedAttribute_AddsToElement()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("string(<e>{ attribute href { \"http://x\" } }</e>/@href)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("http://x", result.StringValue);
    }

    [Fact]
    public void XQuery_ComputedAttribute_XmlPrefixCoerced()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("prefix-from-QName(node-name(attribute { QName(\"http://www.w3.org/XML/1998/namespace\", \"space\") } { \"preserve\" }))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("xml", result.StringValue);
    }

    [Fact]
    public void XQuery_ComputedTextCommentAndPI()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("(string(text { \"abc\" }), string(comment { \"c\" }), name(processing-instruction pi { \"d\" }), string(processing-instruction pi { \"d\" }))");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(new[] { "abc", "c", "pi", "d" }, ToStrings(result));
    }

    [Fact]
    public void XQuery_ComputedDocument_WrapsContent()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("count(document { element a { \"x\" }, \"tail\" }/a)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal(1L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_ComputedNamespace_BecomesDeclaration()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("contains(serialize(element e { namespace z { \"http://z\" } }), \"xmlns:z\")");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void XQuery_ComputedComment_DoubleHyphen_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("comment { \"a--b\" }");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XQDY0072", ex.Message);
    }

    [Fact]
    public void XQuery_ComputedPI_NestedTerminator_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("processing-instruction p { \"a?>b\" }");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XQDY0026", ex.Message);
    }

    [Fact]
    public void XQuery_ComputedElement_XmlnsPrefix_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("element { \"xmlns:x\" } {}");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XQDY0096", ex.Message);
    }

    [Fact]
    public void XQuery_ComputedAttribute_AfterContent_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("element e { \"x\", attribute a { \"b\" } }");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("XQTY0024", ex.Message);
    }

    [Fact]
    public void XQuery_Switch_BasicMatch()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("switch (\"b\") case \"a\" return \"A\" case \"b\" return \"B\" default return \"?\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("B", result.StringValue);
    }

    [Fact]
    public void XQuery_Switch_MultiValueCase()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("switch (2) case 1 case 2 return \"small\" case 3 return \"three\" default return \"big\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("small", result.StringValue);
    }

    [Fact]
    public void XQuery_Switch_NoMatch_UsesDefault()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("switch (\"z\") case \"a\" return \"A\" default return \"def\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("def", result.StringValue);
    }

    [Fact]
    public void XQuery_Switch_NestedOperand()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("switch (switch (1) case 1 return \"x\" default return \"y\") case \"x\" return \"inner\" default return \"outer\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("inner", result.StringValue);
    }

    [Fact]
    public void XQuery_Switch_LaterCaseError_NotSurfaced()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("switch (1) case 1 return \"ok\" case (1 div 0) return \"err\" default return \"d\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("ok", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_AtomicTypes()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch (42) case xs:string return \"string\" case xs:integer return \"integer\" default return \"other\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("integer", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_FirstMatchWins()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch (42) case xs:decimal return \"decimal\" case xs:integer return \"integer\" default return \"other\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("decimal", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_NodeKind()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch (<a/>) case text() return \"text\" case element() return \"element\" default return \"other\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("element", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_CaseVariable()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch (\"hi\") case $s as xs:string return concat(\"got:\", $s) default return \"other\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("got:hi", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_DefaultVariable()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch (1.5) case xs:integer return \"int\" default $d return concat(\"default:\", $d)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("default:1.5", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_EmptySequence()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch (()) case empty-sequence() return \"empty\" default return \"nonempty\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("empty", result.StringValue);
    }

    [Fact]
    public void XQuery_Typeswitch_OccurrenceIndicator()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("typeswitch ((1, 2)) case xs:integer return \"one\" case xs:integer+ return \"many\" default return \"other\"");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("many", result.StringValue);
    }

    [Fact]
    public void XQuery_OutputOption_TextMethod()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
            "declare option output:method \"text\";\n" +
            "fn:serialize(<a>x</a>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("x", result.StringValue);
    }

    [Fact]
    public void XQuery_OutputOption_ItemSeparator()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
            "declare option output:item-separator \"|\";\n" +
            "fn:serialize(1 to 3)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("<?xml version=\"1.0\" encoding=\"UTF-8\"?>1|2|3", result.StringValue);
    }

    [Fact]
    public void XQuery_OutputOption_Indent()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
            "declare option output:indent \"yes\";\n" +
            "fn:serialize(<a><b/><c/></a>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Contains("\n   <b/>", result.StringValue);
    }

    [Fact]
    public void XQuery_OutputOption_Standalone()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
            "declare option output:standalone \"yes\";\n" +
            "fn:serialize(<a/>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Contains("standalone=\"yes\"", result.StringValue);
    }

    [Fact]
    public void XQuery_OutputOption_EQNameForm()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare option Q{http://www.w3.org/2010/xslt-xquery-serialization}method \"text\";\n" +
            "fn:serialize(<a>x</a>)");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("x", result.StringValue);
    }

    [Fact]
    public void XQuery_OutputOption_Duplicate_Rejected()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() => compiler.Compile(
            "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
            "declare option output:method \"xml\";\n" +
            "declare option output:method \"text\";\n" +
            "fn:serialize(<a/>)"));
        Assert.Contains("XQST0110", ex.Message);
    }

    [Fact]
    public void XQuery_Serialize_MapWithXmlMethod_Rejected()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("fn:serialize(map{\"a\":1})");
        var ctx = new XQueryContext();
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(ctx));
        Assert.Contains("SENR0001", ex.Message);
    }

    [Fact]
    public void XQuery_Serialize_AdaptiveDefaultSeparator()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("fn:serialize((1,2,3), map{\"method\":\"adaptive\"})");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Equal("1\n2\n3", result.StringValue);
    }

    [Fact]
    public void XQuery_Serialize_UndeclarePrefixes()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "fn:serialize(fn:parse-xml(\"<?xml version='1.1'?><p:chapter xmlns:p='http://example.com/p'><section xmlns:p=''><para/></section></p:chapter>\"), " +
            "map{\"version\":\"1.1\",\"undeclare-prefixes\":true()})");
        var ctx = new XQueryContext();
        var result = executable.Evaluate(ctx);

        Assert.Contains("xmlns:p=\"\"", result.StringValue);
    }

    // ------------------------------------------------------------------
    // User-defined functions and variables (prolog declarations)
    // ------------------------------------------------------------------

    [Fact]
    public void XQuery_DeclareFunction_SimpleCall()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("declare function local:double($x) { $x * 2 }; local:double(21)");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(42L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareFunction_TypedRecursive()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare function local:fact($n as xs:integer) as xs:integer { if ($n le 1) then 1 else $n * local:fact($n - 1) }; local:fact(5)");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(120L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareFunction_EmptyBody_ReturnsEmptySequence()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("declare function local:nothing() { }; count(local:nothing())");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(0L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareFunction_WrongArity_ThrowsXPST0017()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare function local:f($x) { $x }; local:f(1, 2)").Evaluate(new XQueryContext()));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareFunction_DuplicateParameter_ThrowsXQST0039()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare function local:f($x, $x) { $x }; 1"));
        Assert.Contains("XQST0039", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareFunction_DuplicateDeclaration_ThrowsXQST0034()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare function local:f() { 1 }; declare function local:f() { 2 }; local:f()"));
        Assert.Contains("XQST0034", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareFunction_ReservedNamespace_ThrowsXQST0045()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare function fn:mine() { 1 }; 1"));
        Assert.Contains("XQST0045", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareFunction_ParameterTypeMismatch_ThrowsXPTY0004()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare function local:f($x as xs:integer) { $x }; local:f('abc')").Evaluate(new XQueryContext()));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_Chain()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("declare variable $a := 40; declare variable $b := $a + 2; $b");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(42L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareVariable_Duplicate_ThrowsXQST0049()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $a := 1; declare variable $a := 2; $a"));
        Assert.Contains("XQST0049", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_Circular_ThrowsXQST0054()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $a := $b; declare variable $b := $a; $a").Evaluate(new XQueryContext()));
        Assert.Contains("XQST0054", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_UsedByFunction()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("declare variable $base := 100; declare function local:add($x) { $x + $base }; local:add(5)");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(105L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_TryCatch_NamedCodeInQuery()
    {
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "try { fn:error(fn:QName('http://www.w3.org/2005/xqt-errors', 'err:FOER0001')) } " +
            "catch err:FOAR0001 { 'first' } catch err:FOER0001 { 'second' } catch * { 'fallback' }");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("second", result.StringValue);
    }

    [Fact]
    public void XQuery_TryCatch_GlobalVariableErrorNotCaught()
    {
        // Errors raised while evaluating a global variable initializer are NOT caught by
        // try/catch (QT3 try-006/007).
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile(
            "declare variable $boom := fn:error(fn:QName('http://www.w3.org/2005/xqt-errors', 'err:FOER0001')); " +
            "try { $boom } catch * { 'caught' }");
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(new XQueryContext()));
        Assert.Contains("FOER0001", ex.Message);
    }

    private static string EvalStrC(string query)
    {
        var result = new XQueryCompiler().Compile(query).Evaluate(new XQueryContext());
        return result.ToString();
    }

    [Fact]
    public void XQuery_StringConstructor_Literal()
    {
        Assert.Equal("hello", EvalStrC("``[hello]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_Empty()
    {
        Assert.Equal("", EvalStrC("``[]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_Interpolation()
    {
        Assert.Equal("There were 10 green bottles",
            EvalStrC("declare variable $n := 10; ``[There were `{$n}` green bottles]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_AdjacentInterpolationsNoSeparator()
    {
        Assert.Equal("101112",
            EvalStrC("declare variable $n := 10; ``[`{$n}``{$n+1}``{$n+2}`]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_SequenceJoinedWithSpaces()
    {
        Assert.Equal("1 2 3", EvalStrC("``[`{1 to 3}`]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_EmptyInterpolation()
    {
        // QT3 string-constructor-024 shape.
        Assert.Equal("` ** `", EvalStrC("``[` *`{}`* `]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_BacktickEscapes()
    {
        // QT3 string-constructor-019: doubled backtick is literal; `{` starts an interpolation.
        Assert.Equal("`10`", EvalStrC("declare variable $n := 10; ``[``{$n}``]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_NoReferenceExpansion()
    {
        Assert.Equal("&lt;", EvalStrC("``[&lt;]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_NewlinesPreserved()
    {
        Assert.Equal("a\nb", EvalStrC("``[a\nb]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_NestedInInterpolation()
    {
        Assert.Equal("There were at least 10 green bottles",
            EvalStrC("declare variable $n := 10; ``[There were `{``[at least `{$n}`]``}` green bottles]``"));
    }

    [Fact]
    public void XQuery_StringConstructor_MapInterpolation_ThrowsFOTY0013()
    {
        var ex = Assert.ThrowsAny<Exception>(() => EvalStrC("``[`{map{'a':1}}`]``"));
        Assert.Contains("FOTY0013", ex.Message);
    }

    [Fact]
    public void XQuery_StringConstructor_Unterminated_ThrowsXPST0003()
    {
        var ex = Assert.ThrowsAny<Exception>(() => EvalStrC("``[abc]`"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_OrderedExpression_IsIdentity()
    {
        Assert.Equal("1 2 3", EvalStrC("fn:string-join(ordered { 1 to 3 } ! xs:string(.), ' ')"));
    }

    [Fact]
    public void XQuery_UnorderedExpression_IsIdentity()
    {
        Assert.Equal("1 2 3", EvalStrC("fn:string-join(unordered { 1 to 3 } ! xs:string(.), ' ')"));
    }

    [Fact]
    public void XQuery_OrderedUnordered_EmptyBodies()
    {
        Assert.Equal("(sequence)", EvalStrC("ordered {}"));
        Assert.Equal("(sequence)", EvalStrC("unordered {}"));
    }

    [Fact]
    public void XQuery_DeclareOrdering_Accepted()
    {
        Assert.Equal("42", EvalStrC("declare ordering unordered; 42"));
        Assert.Equal("42", EvalStrC("declare ordering ordered; 42"));
    }

    [Fact]
    public void XQuery_DeclareOrdering_Duplicate_ThrowsXQST0065()
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
            EvalStrC("declare ordering unordered; declare ordering ordered; 42"));
        Assert.Contains("XQST0065", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareDefaultOrderEmpty_GreatestAppliesToOrderBy()
    {
        // emptyorderdecl-2 shape: the empty key sorts last with 'empty greatest'.
        var result = new XQueryCompiler().Compile(
            "declare default order empty greatest; " +
            "for $i in (<a>1</a>,<a>4</a>,<a></a>,<a>7</a>) order by zero-or-one($i/text()) ascending return xs:string($i)")
            .Evaluate(new XQueryContext());
        Assert.Equal(new[] { "1", "4", "7", "" }, ToStrings(result));
    }

    [Fact]
    public void XQuery_DeclareDefaultOrderEmpty_ExplicitLeastWinsOverPrologGreatest()
    {
        var result = new XQueryCompiler().Compile(
            "declare default order empty greatest; " +
            "for $i in (<a>1</a>,<a>4</a>,<a></a>,<a>7</a>) order by zero-or-one($i/text()) ascending empty least return xs:string($i)")
            .Evaluate(new XQueryContext());
        Assert.Equal(new[] { "", "1", "4", "7" }, ToStrings(result));
    }


    [Fact]
    public void XQuery_DeclareDefaultOrderEmpty_Duplicate_ThrowsXQST0069()
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
            EvalStrC("declare default order empty least; declare default order empty greatest; 42"));
        Assert.Contains("XQST0069", ex.Message);
    }

    [Fact]
    public void XQuery_Constructor_AttributeNameBindingNotInherited()
    {
        // K2-NameTest-30: the parent's attribute-name bindings are not inherited by children.
        var result = new XQueryCompiler().Compile("""
            declare namespace a = "http://example.com/1";
            declare namespace b = "http://example.com/2";
            let $e := <e a:n1="content" b:n1="content"><a:n1/><b:n1/><n1/></e>
            return (empty(namespace-uri-for-prefix("b", $e/*:n1[1])),
                    empty(namespace-uri-for-prefix("a", $e/*:n1[2])),
                    namespace-uri-for-prefix("a", $e/*:n1[1]))
            """).Evaluate(new XQueryContext());
        var values = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            values.Add(item.ToString()!);
        Assert.Equal(new[] { "true", "true", "http://example.com/1" }, values);
    }

    [Fact]
    public void XQuery_Constructor_ExplicitDeclarationsPropagateToChildren()
    {
        // K2-InScopePrefixesFunc-9: a parent's xmlns declarations are inherited by children.
        var result = new XQueryCompiler().Compile(
            "for $i in fn:in-scope-prefixes(<e xmlns:p=\"http://example.com\" xmlns:a=\"http://example.com\"> <b/> </e>/b) order by $i return $i")
            .Evaluate(new XQueryContext());
        Assert.Equal(new[] { "a", "p", "xml" }, ToStrings(result));
    }

    [Fact]
    public void XQuery_Constructor_ElementNameBindingOnChild()
    {
        // The child's own element-name prefix is in its in-scope namespaces.
        var result = new XQueryCompiler().Compile("""
            declare namespace a = "http://example.com/1";
            namespace-uri-for-prefix("a", <e><a:n1/></e>/a:n1)
            """).Evaluate(new XQueryContext());
        Assert.Equal("http://example.com/1", result.ToString());
    }

    [Fact]
    public void XQuery_LetAsElementPrefixedName_NamespaceMismatch_ThrowsXPTY0004()
    {
        // K2-DirectConElemNamespace-79: element(P:L) distinguishes the rebound namespace.
        var compiler = new XQueryCompiler();
        var executable = compiler.Compile("""
            declare namespace P = "http://ns.example.com/URL1";
            let $e := document{(<X1:L xmlns:X1="http://ns.example.com/URL1">1</X1:L>, <X2:L xmlns:X2="http://ns.example.com/URL2">2</X2:L>)}
            return <outer xmlns:P="http://ns.example.com/URL1"> {
                let $outer as element(P:L) := $e/element(P:L)
                return <inner xmlns:P="http://ns.example.com/URL2"> {
                    let $inner as element(P:L) := $outer return $inner
                } </inner>
            } </outer>
            """);
        var ex = Assert.ThrowsAny<Exception>(() => executable.Evaluate(new XQueryContext()));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_SimpleCall()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/greet", """
                module namespace g = "http://example.com/greet";
                declare function g:hello($name as xs:string) as xs:string { "hello " || $name };
                """);
        var executable = compiler.Compile(
            "import module namespace g = \"http://example.com/greet\"; g:hello(\"world\")");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("hello world", result.StringValue);
    }

    [Fact]
    public void XQuery_ImportModule_PublicVariableVisible()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/const", """
                module namespace c = "http://example.com/const";
                declare variable $c:answer := 42;
                """);
        var executable = compiler.Compile(
            "import module namespace c = \"http://example.com/const\"; $c:answer + 1");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(43L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_ImportModule_TwoModulesShareNamespace()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/impl", """
                module namespace impl = "http://example.com/impl";
                declare function impl:f1($a as xs:string) as xs:string { $a };
                """, "http://example.com/impl1.xqm")
            .WithModule("http://example.com/impl", """
                module namespace impl = "http://example.com/impl";
                declare function impl:f1($a as xs:string, $b as xs:string) as xs:string { $a || $b };
                """, "http://example.com/impl2.xqm");
        var executable = compiler.Compile(
            "import module namespace impl = \"http://example.com/impl\" at \"http://example.com/impl1.xqm\", \"http://example.com/impl2.xqm\"; impl:f1(\"a\") || impl:f1(\"b\", \"c\")");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("abc", result.StringValue);
    }

    [Fact]
    public void XQuery_ImportModule_CircularImportsAllowed()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/a", """
                module namespace a = "http://example.com/a";
                import module namespace b = "http://example.com/b";
                declare function a:ok() as xs:string { b:ok() };
                """)
            .WithModule("http://example.com/b", """
                module namespace b = "http://example.com/b";
                import module namespace a = "http://example.com/a";
                declare function b:ok() as xs:string { "ok" };
                """);
        var executable = compiler.Compile(
            "import module namespace a = \"http://example.com/a\"; a:ok()");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("ok", result.StringValue);
    }

    [Fact]
    public void XQuery_ImportModule_LibraryBaseUriAppliesInsideModule()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/lib", """
                module namespace lib = "http://example.com/lib";
                declare base-uri "http://www.example.org/correct/";
                declare variable $lib:node := <a><b/></a>;
                """);
        var executable = compiler.Compile(
            "import module namespace lib = \"http://example.com/lib\"; declare base-uri \"http://www.example.org/wrong/\"; base-uri($lib:node)");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal("http://www.example.org/correct/", result.ToString());
    }

    [Fact]
    public void XQuery_ImportModule_NamespaceUriWhitespaceNormalized()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://www.w3.org/Test Modules/test", """
                module namespace test = "http://www.w3.org/Test   Modules/test";
                declare function test:ok() as xs:string { "ok" };
                """);
        var executable = compiler.Compile(
            "import module namespace test=\"  http://www.w3.org/Test Modules/test \"; test:ok()");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("ok", result.StringValue);
    }

    [Fact]
    public void XQuery_LibraryModule_AsQuery_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("module namespace m = \"http://example.com/m\"; \"expr\""));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_DuplicateImport_ThrowsXQST0047()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile(
                "import module namespace a = \"http://example.com/m\"; import module namespace b = \"http://example.com/m\"; 1"));
        Assert.Contains("XQST0047", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_EmptyNamespace_ThrowsXQST0088()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace a = \"\"; 1"));
        Assert.Contains("XQST0088", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_XmlPrefix_ThrowsXQST0070()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace xml = \"http://example.com/m\"; 1"));
        Assert.Contains("XQST0070", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_NotFound_ThrowsXQST0059()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace m = \"http://example.com/missing\"; 1"));
        Assert.Contains("XQST0059", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_WrongTargetNamespace_ThrowsXQST0059()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/registered", """
                module namespace m = "http://example.com/actual";
                declare function m:ok() as xs:string { "ok" };
                """);
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace m = \"http://example.com/registered\"; m:ok()"));
        Assert.Contains("XQST0059", ex.Message);
    }

    [Fact]
    public void XQuery_LibraryModule_DeclarationOutsideTargetNamespace_ThrowsXQST0048()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/lib", """
                module namespace lib = "http://example.com/lib";
                declare namespace other = "http://example.com/other";
                declare function other:f() as xs:string { "x" };
                """);
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace lib = \"http://example.com/lib\"; 1"));
        Assert.Contains("XQST0048", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_PrivateFunction_ThrowsXPST0017()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/lib", """
                module namespace lib = "http://example.com/lib";
                declare %private function lib:f() as xs:integer { 23 };
                declare function lib:g() as xs:integer { lib:f() };
                """);
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace lib = \"http://example.com/lib\"; lib:f()"));
        Assert.Contains("XPST0017", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_PrivateVariable_ThrowsXPST0008()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/lib", """
                module namespace lib = "http://example.com/lib";
                declare %private variable $lib:secret := 23;
                """);
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("import module namespace lib = \"http://example.com/lib\"; $lib:secret"));
        Assert.Contains("XPST0008", ex.Message);
    }

    [Fact]
    public void XQuery_ImportModule_PrivateVisibleInsideOwnModule()
    {
        var compiler = new XQueryCompiler()
            .WithModule("http://example.com/lib", """
                module namespace lib = "http://example.com/lib";
                declare %private function lib:f() as xs:integer { 23 };
                declare %private variable $lib:two := 2;
                declare function lib:g() as xs:integer { lib:f() + $lib:two };
                """);
        var executable = compiler.Compile(
            "import module namespace lib = \"http://example.com/lib\"; lib:g()");
        var result = executable.Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(25L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareFunction_ConflictingAnnotations_ThrowsXQST0106()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare %private %public function local:foo() { () }; 1"));
        Assert.Contains("XQST0106", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_DuplicateAnnotation_ThrowsXQST0116()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare %public %public variable $foo := (); 1"));
        Assert.Contains("XQST0116", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_InitializerIsExprSingle_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $i := 1, 1; 1"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_UntypedAtomicNotConverted_ThrowsXPTY0004()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $i as xs:integer := xs:untypedAtomic(\"1\"); $i")
                .Evaluate(new XQueryContext()));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_NoNumericPromotion_ThrowsXPTY0004()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $i as xs:double := 1; $i")
                .Evaluate(new XQueryContext()));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_NoUriPromotion_ThrowsXPTY0004()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $i as xs:string := xs:anyURI(\"http://www.example.com/\"); $i")
                .Evaluate(new XQueryContext()));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_TypedMatch_Passes()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("declare variable $i as xs:integer := 2; $i")
            .Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(2L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_DeclareVariable_NodeAtomizedToUntypedAtomic_Matches()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("declare variable $v as xs:untypedAtomic := <e>text</e>; string($v)")
            .Evaluate(new XQueryContext());

        Assert.Equal("text", result.StringValue);
    }

    [Fact]
    public void XQuery_DeclareVariable_ElementAtomicPlus_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $v as element(*, xs:untyped+)+ := <e/>; 1"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareVariable_ElementAtomicQuestionMark_Allowed()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("declare variable $v as element(*, xs:untyped?)+ := <e/>; exists($v/*)")
            .Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Boolean, result.Kind);
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void XQuery_DeclareVariable_UndeclaredPrefix_ThrowsXPST0081()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace prefix = \"\"; declare variable $prefix:x external; 1"));
        Assert.Contains("XPST0081", ex.Message);
    }

    [Fact]
    public void XQuery_ExternalVariable_TypedMismatch_ThrowsXPTY0004()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $x as xs:integer external; $x")
                .Evaluate(new XQueryContext().WithVariable("x", XdmValue.FromString("abc"))));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void XQuery_ExternalVariable_TypedMatch_Passes()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("declare variable $x as xs:integer external; $x")
            .Evaluate(new XQueryContext().WithVariable("x", XdmValue.FromInteger(42)));

        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(42L, result.IntegerValue);
    }

    [Fact]
    public void XQuery_UndeclareXsPrefix_ThrowsXPST0081()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace xs = \"\"; xs:integer(1)")
                .Evaluate(new XQueryContext()));
        Assert.Contains("XPST0081", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareNamespace_DuplicatePrefix_ThrowsXQST0033()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace p = \"http://example.com/\"; declare namespace p = \"http://example.com/other\"; 1"));
        Assert.Contains("XQST0033", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareNamespace_UndeclareCountsAsDeclaration_ThrowsXQST0033()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace p = \"http://example.com/\"; declare namespace p = \"\"; 1"));
        Assert.Contains("XQST0033", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareNamespace_XmlPrefix_ThrowsXQST0070()
    {
        var compiler = new XQueryCompiler();
        // Even binding xml to its proper namespace name is rejected (namespaceDecl-3).
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace xml = \"http://www.w3.org/XML/1998/namespace\"; \"a\""));
        Assert.Contains("XQST0070", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareNamespace_XmlnsPrefix_ThrowsXQST0070()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace xmlns = \"http://example.com/examples\"; \"a\""));
        Assert.Contains("XQST0070", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareNamespace_XmlNamespaceName_ThrowsXQST0070()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace foo = \"http://www.w3.org/XML/1998/namespace\"; \"a\""));
        Assert.Contains("XQST0070", ex.Message);
    }

    [Fact]
    public void XQuery_DeclareNamespace_AfterVariable_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $x := 2; declare namespace p = \"http://example.com/\"; 1"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_SetterDeclaration_AfterVariable_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare variable $x := 2; declare base-uri \"http://example.com/\"; 1"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_InlineFunctionAnnotation_Works()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("declare namespace eg = \"http://example.com\"; %eg:sequential function () { \"bar\" } ()")
            .Evaluate(new XQueryContext());

        Assert.Equal("bar", result.StringValue);
    }

    [Fact]
    public void XQuery_InlineFunctionAnnotations_WithParamsAndMultiple_Work()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("declare namespace eg = \"http://example.com\"; %eg:sequential(\"abc\", 3) %eg:memo-function function () { \"bar\" } ()")
            .Evaluate(new XQueryContext());

        Assert.Equal("bar", result.StringValue);
    }

    [Fact]
    public void XQuery_InlineFunctionAnnotation_NonLiteralArgument_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("declare namespace eg = \"http://example.com\"; %eg:sequential(true()) function () { 1 } ()"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_FunctionTestAnnotationAssertion_Parses()
    {
        var compiler = new XQueryCompiler();
        // () does not match function(*) regardless of the (ignored) assertion.
        var result = compiler.Compile("declare namespace eg = \"http://example.com\"; () instance of %eg:x(\"abc\", 12e34, 567) function(*)")
            .Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Boolean, result.Kind);
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void XQuery_FunctionTestAnnotationAssertion_ReservedNamespace_ThrowsXQST0045()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("() instance of %xs:x function(*)")
                .Evaluate(new XQueryContext()));
        Assert.Contains("XQST0045", ex.Message);
    }

    [Fact]
    public void XQuery_FunctionTestAnnotationAssertion_PublicPrivateAllowed()
    {
        var compiler = new XQueryCompiler();
        // %public/%private are in no namespace: allowed and ignored; the arity-0 function
        // does not match function(xs:integer) (accepted by any-of in annotation-assertion-20).
        var result = compiler.Compile("declare %public function local:three() as xs:integer {3}; local:three#0 instance of %public %private function(xs:integer) as xs:integer")
            .Evaluate(new XQueryContext());

        Assert.Equal(XdmValueKind.Boolean, result.Kind);
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void XPath_InlineFunctionAnnotation_ThrowsXPST0003()
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
            Bosak.XPath.Api.XPath31Expression.Compile("let $add := %Q{http://example.com/speed}fast function($x, $y) {$x + $y} return $add(2,2)"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_StringLiteral_CharacterReferenceExpands()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("\"&#8364;\"")
            .Evaluate(new XQueryContext());

        Assert.Equal("€", result.StringValue);
    }

    [Fact]
    public void XQuery_StringLiteral_AstralCharacterReferenceExpands()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("\"&#x1F600;\"")
            .Evaluate(new XQueryContext());

        Assert.Equal("\U0001F600", result.StringValue);
    }

    [Fact]
    public void XQuery_StringLiteral_EntityReferencesExpand()
    {
        var compiler = new XQueryCompiler();
        var result = compiler.Compile("\"&amp;&lt;&gt;\"")
            .Evaluate(new XQueryContext());

        Assert.Equal("&<>", result.StringValue);
    }

    [Fact]
    public void XQuery_StringLiteral_NulCharacterReference_ThrowsXQST0090()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("\"&#x00;\""));
        Assert.Contains("XQST0090", ex.Message);
    }

    [Fact]
    public void XQuery_StringLiteral_ZeroCharacterReference_ThrowsXQST0090()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("'&#x0;'"));
        Assert.Contains("XQST0090", ex.Message);
    }

    [Fact]
    public void XQuery_StringLiteral_SignedCharacterReference_ThrowsXPST0003()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("\"&#+20;\""));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void XQuery_Constructor_OverflowHexCharacterReference_ThrowsXQST0090()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("<p>FA&#xFF000000F6;IL</p>"));
        Assert.Contains("XQST0090", ex.Message);
    }

    [Fact]
    public void XQuery_Constructor_OverflowDecimalCharacterReference_ThrowsXQST0090()
    {
        var compiler = new XQueryCompiler();
        var ex = Assert.ThrowsAny<Exception>(() =>
            compiler.Compile("<p>FA&#18446744073709551862;IL</p>"));
        Assert.Contains("XQST0090", ex.Message);
    }

    [Fact]
    public void XPath_StringLiteral_ReferencesDoNotExpand()
    {
        var result = Bosak.XPath.Api.XPath31Expression.Compile("\"&amp;\"")
            .Evaluate(new Bosak.XPath.Runtime.Vm.EvaluationContext());

        Assert.Equal("&amp;", result.StringValue);
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
