// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded exact terminating decimal division.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact terminating division profile (1.51.0).                                     |
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

/// <summary>Checks finite exact division through real public session profiles and captured limits.</summary>
public sealed class ExactDivisionSessionTests
{
    private static Session Compile(string query,int digits=128,int scale=128)
    {var r=Session.CompileXPath(query,new SchemaQueryLimits(4,8,8,digits,scale));Assert.Equal(SchemaResultKind.Supported,r.Kind);return r.Value;}
    private static XdmValue Eval(Session s,EvaluationContext? c=null)=>s.Run([],op=>op.Evaluate(c??new EvaluationContext()));
    /// <summary>Exact division composes with precedence, signs, quotient, remainder and scalar conversion.</summary>
    /// <param name="query">An exact terminating expression.</param>
    /// <param name="text">Expected exact string.</param>
    [Theory]
    [InlineData("1 div 8","0.125")][InlineData("-1 div 8","-0.125")][InlineData("1 div -8","-0.125")][InlineData("-1 div -8","0.125")]
    [InlineData("6 div 3","2")][InlineData("0 div 3","0")][InlineData("3 div 6","0.5")][InlineData("1.20 div .03","40")]
    [InlineData("1+8 div 2*3","13")][InlineData("8 div 2 div 2","2")][InlineData("9 idiv 2 div 8","0.5")]
    [InlineData("9 mod 4 div 8","0.125")][InlineData("1 div 2 eq .5","true")][InlineData("1 div 2 < .4","false")]
    [InlineData("79228162514264337593543950335.2 div 2","39614081257132168796771975167.6")]
    public void ExactScalarResults(string query,string text)
    {using var s=Compile("string("+query+")");Assert.Equal(text,Eval(s).StringValue);Assert.Equal("bosak-exact-division-v4",s.Environment.BehaviorProfile);Assert.Equal("bosak-exact-division-static-v4",s.Environment.DescriptorFormatVersion);}
    /// <summary>Nontermination and zero are dynamic refusals and do not poison reusable sessions.</summary>
    [Fact] public void DynamicErrorsAndKinds()
    {
        using var s=Compile("1 div $x");var c=new EvaluationContext().WithVariable("x",XdmValue.FromInteger(3));Assert.Throws<NotSupportedException>(()=>Eval(s,c));
        c.WithVariable("x",XdmValue.FromInteger(0));Assert.Contains("FOAR0001",Assert.Throws<DivideByZeroException>(()=>Eval(s,c)).Message);
        c.WithVariable("x",XdmValue.FromInteger(2));var q=Eval(s,c);Assert.Equal(XdmValueKind.Decimal,q.Kind);Assert.Equal(new BigInteger(5),q.ExactDecimalValue.Coefficient);Assert.Equal(1,q.ExactDecimalValue.Scale);
        using var integral=Compile("6 div 3");Assert.Equal(XdmValueKind.Decimal,Eval(integral).Kind);
    }
    /// <summary>Supplied and result limits refuse without rounding, including extreme-scale values.</summary>
    [Fact] public void QuotasAndExtremeScale()
    {
        using var growth=Compile("1 div 32",3,8);Assert.Throws<NotSupportedException>(()=>Eval(growth));
        using var scale=Compile("1 div 8",8,2);Assert.Throws<NotSupportedException>(()=>Eval(scale));
        using var input=Compile("1 div $x",3,2);foreach(var v in new[]{XdmValue.FromInteger(1000),XdmValue.FromDecimal(1.000m),XdmValue.FromDouble(2),XdmValue.FromInteger(2,"int"),XdmValue.FromInteger(2,"integer","Q{u}T")})
            Assert.Throws<NotSupportedException>(()=>Eval(input,new EvaluationContext().WithVariable("x",v)));
        var c=new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue)));
        using var tiny=Compile("$x div 1",2,int.MaxValue);Assert.Equal(int.MaxValue,Eval(tiny,c).ExactDecimalValue.Scale);
        using var huge=Compile("1 div $x",2,int.MaxValue);Assert.Throws<NotSupportedException>(()=>Eval(huge,c));
        using var text=Compile("string($x div 1)",2,int.MaxValue);Assert.Throws<NotSupportedException>(()=>Eval(text,c));
    }
    /// <summary>Prior profile fingerprints retain their scope while v4 captures policy and limit differences.</summary>
    [Fact] public void ProfileAndPolicyCapture()
    {
        using var a=Compile("1 div 2",8,8);using var b=Compile("1 div 2",9,8);using var c=Compile("1 div 2",8,8);
        Assert.Equal(a.Environment.StaticContextFingerprint,c.Environment.StaticContextFingerprint);Assert.NotEqual(a.Environment.StaticContextFingerprint,b.Environment.StaticContextFingerprint);
        foreach(var pair in new[]{("1+2","bosak-exact-arithmetic-v1"),("1 eq 2","bosak-exact-scalars-v2"),("1 idiv 2","bosak-exact-quotient-v3")})
        {using var old=Compile(pair.Item1);Assert.Equal(pair.Item2,old.Environment.BehaviorProfile);}
        foreach(var query in new[]{"div 2","1 div","(1 eq 2) div 2","1e2 div 2","string(string(1 div 2))"})Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(query,new SchemaQueryLimits(4,8,8,128,128)).Kind);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("1 div 2",new SchemaQueryLimits(4,8,8,128,128),new CompileOptions{EnableJit=true}).Kind);
    }
    /// <summary>Repeated evaluation isolates inputs and observes cancellation before raw entry.</summary>
    [Fact] public void RepeatedDivisionAndCancellation()
    {
        using var s=Compile("$x div 8");for(int i=0;i<40;i++){var result=Eval(s,new EvaluationContext().WithVariable("x",XdmValue.FromInteger(i)));Assert.Equal(i/8m,result.DecimalValue);}
        using var cancellation=new CancellationTokenSource();cancellation.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([],op=>op.Evaluate(new EvaluationContext()),cancellation.Token));
    }
}
