// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies bounded primitive numeric constructor conversion.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; exact numeric constructor profile (1.52.0).                                     |
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

/// <summary>Checks bounded primitive numeric construction through captured public sessions.</summary>
public sealed class ExactConstructorSessionTests
{
    private static Session Compile(string query,int digits=128,int scale=128,CompileOptions? options=null)
    {var r=Session.CompileXPath(query,new SchemaQueryLimits(4,8,8,digits,scale),options);Assert.Equal(SchemaResultKind.Supported,r.Kind);return r.Value;}
    private static XdmValue Eval(Session s,EvaluationContext? c=null)=>s.Run([],op=>op.Evaluate(c??new EvaluationContext()));
    /// <summary>Constructors select exact primitive kinds and apply specified signed integer truncation.</summary>
    /// <param name="query">A numeric constructor expression.</param>
    /// <param name="text">Expected copied string.</param>
    [Theory]
    [InlineData("xs:integer(3.99)","3")][InlineData("xs:integer(-3.99)","-3")][InlineData("xs:integer(-.99)","0")]
    [InlineData("xs:decimal(123)","123")][InlineData("xs:decimal(-.125)","-0.125")][InlineData("xs:integer(9 div 2)","4")]
    [InlineData("xs:integer(9 idiv 2)","4")][InlineData("xs:decimal(9 mod 2)","1")][InlineData("xs:integer(1+2*3.5)","8")]
    [InlineData("xs:integer(79228162514264337593543950335.9)","79228162514264337593543950335")]
    public void ExactNumericConstructors(string query,string text)
    {using var s=Compile("string("+query+")");Assert.Equal(text,Eval(s).StringValue);Assert.Equal("bosak-exact-constructors-v5",s.Environment.BehaviorProfile);Assert.Equal("bosak-exact-constructors-static-v5",s.Environment.DescriptorFormatVersion);}
    /// <summary>Constructors create plain primitive values without manufacturing schema authority.</summary>
    [Fact] public void PrimitiveKindsAndDynamicInputs()
    {
        using var i=Compile("xs:integer($x)");using var d=Compile("xs:decimal($x)");
        var c=new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(-12.34m));Assert.Equal(new BigInteger(-12),Eval(i,c).BigIntegerValue);
        var result=Eval(d,c);Assert.Equal(XdmValueKind.Decimal,result.Kind);Assert.Null(result.UserSchemaTypeName);Assert.Equal(-12.34m,result.DecimalValue);
        c.WithVariable("x",XdmValue.FromInteger(7));Assert.Equal(XdmValueKind.Decimal,Eval(d,c).Kind);
        c.WithVariable("x",XdmValue.FromDecimal(0.5m));Assert.Equal(BigInteger.Zero,Eval(i,c).BigIntegerValue);
    }
    /// <summary>Namespace capture authenticates xs and refuses aliases, Boolean values, strings and wider cast grammar.</summary>
    [Fact] public void NamespaceAndUnsupportedGrammar()
    {
        var ns=new Dictionary<string,string>{{"xs","urn:wrong"}};
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("xs:integer(1)",new SchemaQueryLimits(4,8,8,128,128),new CompileOptions{Namespaces=ns}).Kind);
        ns["xs"]="http://www.w3.org/2001/XMLSchema";using var s=Compile("xs:integer(1.9)",options:new CompileOptions{Namespaces=ns});ns["xs"]="urn:changed";Assert.Equal(BigInteger.One,Eval(s).BigIntegerValue);
        foreach(string query in new[]{"xs:integer('1')","xs:integer(1 eq 1)","xs:integer(xs:decimal(1))","xs:integer(1)+1","1 cast as xs:integer","n:integer(1)","integer(1)","xs:double(1)","xs:integer()","xs:integer(1,2)","xs:integer(1e2)"})
            Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(query,new SchemaQueryLimits(4,8,8,128,128)).Kind);
    }
    /// <summary>Inputs and arithmetic limits cannot be bypassed by a shrinking constructor or extreme scale.</summary>
    [Fact] public void QuotasAndExtremeScale()
    {
        using var i=Compile("xs:integer($x)",3,2);
        foreach(var value in new[]{XdmValue.FromInteger(1000),XdmValue.FromDecimal(1.000m),XdmValue.FromDouble(1),XdmValue.FromString("1"),XdmValue.FromInteger(1,"int"),XdmValue.FromInteger(1,"integer","Q{u}T")})
            Assert.Throws<NotSupportedException>(()=>Eval(i,new EvaluationContext().WithVariable("x",value)));
        using var intermediate=Compile("xs:integer(99+.1)",2,2);Assert.Throws<NotSupportedException>(()=>Eval(intermediate));
        using var tiny=Compile("xs:integer($x)",2,int.MaxValue);Assert.Equal(BigInteger.Zero,Eval(tiny,new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(new XdmExactDecimal(-1,int.MaxValue,1,int.MaxValue)))).BigIntegerValue);
        using var nonterm=Compile("xs:integer(1 div 3)");Assert.Throws<NotSupportedException>(()=>Eval(nonterm));
    }
    /// <summary>Captured policy/limits remain stable across repeated work and cancellation before entry.</summary>
    [Fact] public void CapturedProfileAndRepeatedUse()
    {
        using var a=Compile("xs:integer($x)",8,8);using var b=Compile("xs:integer($x)",9,8);using var c=Compile("xs:integer($x)",8,8);
        Assert.Equal(a.Environment.StaticContextFingerprint,c.Environment.StaticContextFingerprint);Assert.NotEqual(a.Environment.StaticContextFingerprint,b.Environment.StaticContextFingerprint);
        for(int j=-20;j<20;j++)Assert.Equal(new BigInteger(j),Eval(a,new EvaluationContext().WithVariable("x",XdmValue.FromDecimal(j+(j<0?-0.75m:0.75m)))).BigIntegerValue);
        using var cancel=new CancellationTokenSource();cancel.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>a.Run([],op=>op.Evaluate(new EvaluationContext()),cancel.Token));
    }
}
