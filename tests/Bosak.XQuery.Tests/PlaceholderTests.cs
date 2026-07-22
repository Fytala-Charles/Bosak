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
}
