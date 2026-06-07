using Xunit;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using System.Xml.Linq;

namespace Bosak.Xslt.Tests;

public class AtomicValueMatchTests
{
    [Fact]
    public void AtomicValue_PredicatePattern_MatchesInteger()
    {
        var compiler = new Bosak.Xslt.Patterns.PatternCompiler();
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        
        // Compile atomic match for .[. instance of xs:integer]
        var atomicMatch = compiler.CompileAtomicMatch(".[. instance of xs:integer]");
        Assert.NotNull(atomicMatch);
        
        // Test with integer value
        var intValue = XdmValue.FromInteger(42);
        Assert.True(atomicMatch(intValue, ctx), "Should match integer");
        
        // Test with string value
        var strValue = XdmValue.FromString("hello");
        Assert.False(atomicMatch(strValue, ctx), "Should not match string");
    }
}
