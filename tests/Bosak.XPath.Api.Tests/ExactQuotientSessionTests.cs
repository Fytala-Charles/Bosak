// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded exact quotient and remainder arithmetic.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; bounded quotient/remainder profile (1.50.0).                                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Promote finite division controls and retain unavailable grammar checks (1.51.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Api.Tests;

/// <summary>Exercises bounded integer quotient/remainder through public session selection and dispatch.</summary>
public sealed class ExactQuotientSessionTests
{
    private static Session Compile(string query,int digits=128,int scale=128)
    {var result=Session.CompileXPath(query,new SchemaQueryLimits(4,8,8,digits,scale));Assert.Equal(SchemaResultKind.Supported,result.Kind);return result.Value;}
    private static XdmValue Eval(Session s,EvaluationContext? c=null)=>s.Run([],op=>op.Evaluate(c??new EvaluationContext()));
    /// <summary>Signs, promotion and precedence produce canonical exact results.</summary>
    /// <param name="query">A quotient/remainder expression.</param>
    /// <param name="text">Expected scalar text.</param>
    [Theory]
    [InlineData("10 idiv 3","3")][InlineData("-3 idiv 2","-1")][InlineData("3 idiv -2","-1")][InlineData("-3 idiv -2","1")]
    [InlineData("-3.5 idiv 3","-1")][InlineData("3.0 idiv 4","0")][InlineData("10 mod 3","1")][InlineData("-10 mod 3","-1")]
    [InlineData("10 mod -3","1")][InlineData("-10 mod -3","-1")][InlineData("4.5 mod 1.2","0.9")][InlineData("1 mod 0.3","0.1")]
    [InlineData("1+20 idiv 3*2","13")][InlineData("20 idiv 3 idiv 2","3")][InlineData("20 mod 6*2","4")]
    [InlineData("(20 idiv 3) mod 4","2")][InlineData("0 idiv -2","0")][InlineData("0 mod 2.0","0")]
    public void ExactResults(string query,string text)
    {using var s=Compile("string("+query+")");Assert.Equal(text,Eval(s).StringValue);Assert.Equal("bosak-exact-quotient-v3",s.Environment.BehaviorProfile);Assert.Equal("bosak-exact-quotient-static-v3",s.Environment.DescriptorFormatVersion);}
    /// <summary>Zero divisors fail dynamically and the same session remains reusable with valid input.</summary>
    /// <param name="op">Numeric operator.</param>
    [Theory][InlineData("idiv")][InlineData("mod")]
    public void DynamicZeroDoesNotPoisonSession(string op)
    {
        using var s=Compile("9 "+op+" $x");var c=new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(0m));
        Assert.Contains("FOAR0001",Assert.Throws<DivideByZeroException>(()=>Eval(s,c)).Message);
        c.WithVariable("x",XdmValue.FromInteger(2));Assert.Equal(op=="idiv"?new BigInteger(4):BigInteger.One,Eval(s,c).BigIntegerValue);
    }
    /// <summary>Integer quotient and decimal remainder keep their required primitive result kinds.</summary>
    [Fact] public void KindsAndComparisons()
    {
        using var q=Compile("9.5 idiv 2.0");Assert.Equal(XdmValueKind.Integer,Eval(q).Kind);
        using var r=Compile("9 mod 2.0");Assert.Equal(XdmValueKind.Decimal,Eval(r).Kind);
        using var compare=Compile("9 idiv 2 eq 4");Assert.True(Eval(compare).BooleanValue);
        using var old=Compile("1+2");Assert.Equal("bosak-exact-arithmetic-v1",old.Environment.BehaviorProfile);
        using var scalar=Compile("1 eq 2");Assert.Equal("bosak-exact-scalars-v2",scalar.Environment.BehaviorProfile);
    }
    /// <summary>Scale alignment respects finite capacities even where a mathematical remainder would fit.</summary>
    [Fact] public void CapacityAndExtremeScales()
    {
        using var q=Compile("99 idiv .1",2,2);Assert.Throws<NotSupportedException>(()=>Eval(q));
        var c=new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue)));
        using var small=Compile("$x idiv 1",2,int.MaxValue);Assert.Equal(BigInteger.Zero,Eval(small,c).BigIntegerValue);
        using var large=Compile("1 mod $x",2,int.MaxValue);Assert.Throws<NotSupportedException>(()=>Eval(large,c));
    }
    /// <summary>Floating values, Boolean operands and decimal division remain outside the profile.</summary>
    [Fact] public void RetainedRefusalsAndFreshBindings()
    {
        foreach(var query in new[]{"1 treat as xs:integer","1e2 idiv 2","(1 eq 1) mod 2","idiv 2","1 mod","string(1 to 2)"})
            Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(query,new SchemaQueryLimits(4,8,8,128,128)).Kind);
        using var s=Compile("$x mod 3");Assert.Throws<NotSupportedException>(()=>Eval(s,new EvaluationContext().WithVariable("x",XdmValue.FromDouble(4))));
        for(int i=0;i<30;i++)Assert.Equal(new BigInteger(i%3),Eval(s,new EvaluationContext().WithVariable("x",XdmValue.FromInteger(i))).BigIntegerValue);
        using var cancel=new CancellationTokenSource();cancel.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([],op=>op.Evaluate(new EvaluationContext()),cancel.Token));
    }
}
