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
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
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
