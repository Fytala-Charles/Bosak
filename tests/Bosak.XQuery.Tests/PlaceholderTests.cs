// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Placeholder tests verifying the XQuery project compiles and links.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-06-2026     | Creation — placeholder skeleton                                                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

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
}
