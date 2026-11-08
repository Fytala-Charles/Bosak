// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies exact scalar profile comparison, bounded string conversion and selection.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact scalar/key integration   (1.49.0).                                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Promote finite division controls and retain unavailable grammar checks (1.51.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Api.Tests;

/// <summary>Exercises exact comparison and bounded string output through existing public sessions.</summary>
public sealed class ExactScalarSessionTests
{
    private static SchemaQueryLimits Limits(int digits=128,int scale=128)=>new(4,8,8,digits,scale);
    private static Session Compile(string source,int digits=128,int scale=128)
    {var result=Session.CompileXPath(source,Limits(digits,scale));Assert.Equal(SchemaResultKind.Supported,result.Kind);return result.Value;}
    private static XdmValue Evaluate(Session session,EvaluationContext? context=null)=>session.Run([],op=>op.Evaluate(context??new EvaluationContext()));
    /// <summary>Numeric comparisons implement exact singleton semantics and precedence.</summary>
    /// <param name="source">A comparison expression.</param>
    /// <param name="expected">Expected Boolean value.</param>
    [Theory]
    [InlineData("1 eq 1.0",true)][InlineData("1 ne 1.00",false)][InlineData("1 lt 2",true)][InlineData("2 le 2.0",true)]
    [InlineData("2 gt 1",true)][InlineData("2 ge 3",false)][InlineData("1 = 1",true)][InlineData("1 != 2",true)]
    [InlineData("1 < 2",true)][InlineData("2 <= 1",false)][InlineData("2 > 1",true)][InlineData("2 >= 2",true)]
    [InlineData("1+2*3 = (7)",true)][InlineData("-0.2 lt -0.15",true)]
    [InlineData("79228162514264337593543950335.1 > 79228162514264337593543950335",true)]
    public void ComparisonsAreExact(string source,bool expected)
    {using var s=Compile(source);Assert.Equal(expected,Evaluate(s).BooleanValue);Assert.Equal("bosak-exact-scalars-v2",s.Environment.BehaviorProfile);}
    /// <summary>Explicit scalar formatting returns canonical exact text without checked legacy narrowing.</summary>
    /// <param name="source">A bounded top-level string conversion.</param>
    /// <param name="expected">Expected copied text.</param>
    [Theory]
    [InlineData("string(1 + 2)","3")][InlineData("fn:string(0.1 + 0.2)","0.3")]
    [InlineData("string(79228162514264337593543950335.1)","79228162514264337593543950335.1")]
    [InlineData("string(-.00120)","-0.0012")][InlineData("string(1 < 2)","true")][InlineData("string(1 eq 2)","false")]
    [InlineData("string((1+2)*(3+4))","21")]
    public void ExplicitFormattingIsBoundedAndExact(string source,string expected)
    {using var s=Compile(source);Assert.Equal(expected,Evaluate(s).StringValue);Assert.Equal("bosak-exact-scalars-static-v2",s.Environment.DescriptorFormatVersion);}
    /// <summary>Boolean numeric operands, chains, arbitrary nested calls and wider language features refuse.</summary>
    /// <param name="source">An unsupported expression.</param>
    [Theory]
    [InlineData("1 lt 2 lt 3")][InlineData("1 = 1 = 1")][InlineData("(1 lt 2) + 3")][InlineData("-(1 lt 2)")]
    [InlineData("(1 lt 2) eq (2 lt 3)")][InlineData("1 lt 2 and 3 lt 4")][InlineData("string(1 to 2)")]
    [InlineData("string(string(1+2))")][InlineData("string(1+2)+3")][InlineData("concat('x',1)")]
    [InlineData("1e2 = 100")][InlineData("$p:x eq 1")]
    public void RefusesUnavailableScalarCombinations(string source)
    {Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(source,Limits()).Kind);}
    /// <summary>Huge scale comparisons do not expand powers; requested output still refuses an impossible length.</summary>
    [Fact] public void ExtremeScaleComparisonAndFormattingStaySafe()
    {
        var tiny=XdmValue.FromDecimal(new XdmExactDecimal(1,int.MaxValue,1,int.MaxValue));
        var context=new EvaluationContext().WithVariable("x",tiny);using var compare=Compile("$x < 1",2,int.MaxValue);Assert.True(Evaluate(compare,context).BooleanValue);
        using var stringify=Compile("string(+$x)",2,int.MaxValue);Assert.Throws<NotSupportedException>(()=>Evaluate(stringify,context));
        using var equality=Compile("$x eq $x",2,int.MaxValue);Assert.True(Evaluate(equality,context).BooleanValue);
    }
    /// <summary>Base primitive admission, supplied scale and intermediate limits remain active for comparisons and strings.</summary>
    [Fact] public void ScalarLimitsCannotBeBypassed()
    {
        using var s=Compile("$x eq 1",3,2);Assert.Throws<NotSupportedException>(()=>Evaluate(s,new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(1.000m))));
        using var arithmetic=Compile("string(99+1)",2,2);Assert.Throws<NotSupportedException>(()=>Evaluate(arithmetic));
        using var booleanText=Compile("string(1 eq 2)",1,0);Assert.Equal("false",Evaluate(booleanText).StringValue);
        using var numericText=Compile("string(+$x)",32,16);Assert.Equal("123.45",Evaluate(numericText,new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(123.45m))).StringValue);
    }
    /// <summary>String's fn prefix is authenticated by the captured namespace, and earlier profiles stay selected.</summary>
    [Fact] public void ProfileSelectionAndNamespaceBindingsRemainExplicit()
    {
        using var arithmetic=Compile("1+2");Assert.Equal("bosak-exact-arithmetic-v1",arithmetic.Environment.BehaviorProfile);
        using var ordinary=Compile("string($x)");Assert.Equal("bosak-ordinary-string-boolean-v1",ordinary.Environment.BehaviorProfile);Assert.Throws<NotSupportedException>(()=>Evaluate(ordinary,new EvaluationContext().WithVariable("x",XdmValue.FromInteger(1))));
        var namespaces=new Dictionary<string,string>(StringComparer.Ordinal){["fn"]="urn:other"};
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("fn:string(1+2)",Limits(),new CompileOptions{Namespaces=namespaces}).Kind);
        namespaces["fn"]="http://www.w3.org/2005/xpath-functions";var result=Session.CompileXPath("fn:string(1+2)",Limits(),new CompileOptions{Namespaces=namespaces});Assert.Equal(SchemaResultKind.Supported,result.Kind);using var s=result.Value;
        namespaces["fn"]="urn:changed";Assert.Equal("3",Evaluate(s).StringValue);
    }
    /// <summary>Scalar pooled evaluation can be repeated without retaining previous variables or result type.</summary>
    [Fact] public void RepeatedScalarEvaluationsAreIsolated()
    {
        using var s=Compile("string($x < 20)");for(int i=0;i<40;i++)Assert.Equal(i<20?"true":"false",Evaluate(s,new EvaluationContext().WithVariable("x",XdmValue.FromInteger(i))).StringValue);
        using var cancel=new CancellationTokenSource();cancel.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([],op=>op.Evaluate(new EvaluationContext()),cancel.Token));
    }
}
