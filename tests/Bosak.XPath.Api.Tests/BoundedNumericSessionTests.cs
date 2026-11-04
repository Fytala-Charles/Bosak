// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies public bounded exact arithmetic, captured limits and raw-work availability.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; bounded exact arithmetic profile (1.48.0).                                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Promote scalar cases and qualify both profile dispatch lifetimes (1.49.0).              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 04-11-2026     | Promote idiv/mod cases and qualify quotient dispatch lifetime (1.50.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 04-11-2026     | Promote finite division controls and retain unavailable grammar checks (1.51.0). |
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

/// <summary>Exercises the source-free bounded exact arithmetic profile through public signatures.</summary>
public sealed class BoundedNumericSessionTests
{
    private static SchemaQueryLimits Limits(int digits=128,int scale=64)=>new(4,8,8,digits,scale);
    private static Session Compile(string expression,int digits=128,int scale=64)
    {var result=Session.CompileXPath(expression,Limits(digits,scale));Assert.Equal(SchemaResultKind.Supported,result.Kind);return result.Value;}
    private static string Text(XdmValue value)=>value.Kind==XdmValueKind.Boolean?(value.BooleanValue?"true":"false"):value.Kind==XdmValueKind.String?value.StringValue:value.Kind==XdmValueKind.Integer?value.BigIntegerValue.ToString(CultureInfo.InvariantCulture):value.ExactDecimalValue.ToInvariantString(512);
    private static XdmValue Evaluate(Session s,EvaluationContext? context=null)=>s.Run([],op=>op.Evaluate(context??new EvaluationContext()));
    /// <summary>Literals, signs, precedence and primitive promotion stay exact.</summary>
    /// <param name="source">Arithmetic query.</param>
    /// <param name="expected">Exact numeric result.</param>
    /// <param name="decimalResult">Whether decimal promotion applies.</param>
    [Theory]
    [InlineData("1+2*3","7",false)][InlineData("(1+2)*3","9",false)]
    [InlineData("--1","1",false)][InlineData("-+-2","2",false)][InlineData("-2 * -3 + 4","10",false)]
    [InlineData("0.1 + 0.2","0.3",true)][InlineData("1.2 * 2.5","3",true)][InlineData(".5 * 2.","1",true)]
    [InlineData("1 - 2","-1",false)][InlineData("+2.00","2",true)]
    [InlineData("79228162514264337593543950335 + 1","79228162514264337593543950336",false)]
    [InlineData("79228162514264337593543950335.0 + 0.1","79228162514264337593543950335.1",true)]
    [InlineData("123456789012345678901234567890.1234567890123456789","123456789012345678901234567890.1234567890123456789",true)]
    public void ExactEvaluation(string source,string expected,bool decimalResult)
    {using var s=Compile(source);var value=Evaluate(s);Assert.Equal(expected,Text(value));Assert.Equal(decimalResult?XdmValueKind.Decimal:XdmValueKind.Integer,value.Kind);Assert.Equal("bosak-exact-arithmetic-v1",s.Environment.BehaviorProfile);}
    /// <summary>Unsupported numeric syntax never falls back to legacy rounding, functions or floating point.</summary>
    /// <param name="source">Query outside the finite arithmetic grammar.</param>
    [Theory]
    [InlineData("1 treat as xs:integer")][InlineData("1 to 2")][InlineData("if (1) then 2 else 3")][InlineData("1e2")]
    [InlineData("1 instance of xs:integer")][InlineData("string(1 to 2)")][InlineData("count(/n)")][InlineData("$p:x + 1")]
    [InlineData("xs:decimal('1.2')")][InlineData("(1,2)")][InlineData("1 + true()")]
    public void RefusesUnavailableSyntax(string source)
    {Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(source,Limits()).Kind);}
    /// <summary>Supplied literal digits and scale are checked before normalization and coefficient creation.</summary>
    [Fact] public void LiteralCapacityPrecedesNormalization()
    {
        foreach(var source in new[]{"0001","1000","1.00"})Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(source,Limits(3,1)).Kind);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(new string('9',200000),Limits(32,32)).Kind);
        using var s=Compile("0001",4,0);Assert.Equal("1",Text(Evaluate(s)));
    }
    /// <summary>Intermediate quotas refuse before arithmetic result allocation, including alignment and normalized shrinkage.</summary>
    /// <param name="source">Query exceeding a finite intermediate bound.</param>
    /// <param name="digits">Coefficient capacity.</param>
    /// <param name="scale">Scale capacity.</param>
    [Theory]
    [InlineData("99 + 1",2,2)][InlineData("99*99",2,2)][InlineData("9.9 + 0.1",2,2)]
    [InlineData("2.5 * 4",2,2)][InlineData("0.1 * 0.1",4,1)][InlineData("1 + .0001",4,4)]
    public void RefusesIntermediateCapacity(string source,int digits,int scale)
    {using var s=Compile(source,digits,scale);Assert.Throws<NotSupportedException>(()=>Evaluate(s));}
    /// <summary>Numeric bindings are copied for each operation and exact integer/decimal inputs promote correctly.</summary>
    [Fact] public void NumericBindingsAreExactAndIsolated()
    {
        using var s=Compile("$x * 2 + $y");var x=BigInteger.Parse("123456789012345678901234567890",CultureInfo.InvariantCulture);
        var c=new EvaluationContext().WithVariable("x",XdmValue.FromInteger(x)).WithVariable("y",XdmValue.FromDecimal(0.5m));
        Assert.Equal("246913578024691357802469135780.5",Text(Evaluate(s,c)));
        c.WithVariable("x",XdmValue.FromInteger(2));Assert.Equal("4.5",Text(Evaluate(s,c)));
        Assert.Throws<InvalidOperationException>(()=>Evaluate(s));
    }
    /// <summary>All supplied variables, including unused inputs, must satisfy the selected primitive numeric profile.</summary>
    [Fact] public void SuppliedBindingsCannotBypassLimits()
    {
        using var s=Compile("1+1",3,2);
        foreach(var value in new[]{XdmValue.FromInteger(1000),XdmValue.FromDecimal(1.000m),XdmValue.FromDouble(1),XdmValue.FromString("1"),XdmValue.FromInteger(1,"int"),XdmValue.FromInteger(1,"integer","Q{u}T")})
            Assert.Throws<NotSupportedException>(()=>Evaluate(s,new EvaluationContext().WithVariable("unused",value)));
        using var ordinary=Compile("$x");Assert.Equal("bosak-ordinary-string-boolean-v1",ordinary.Environment.BehaviorProfile);
        Assert.Throws<NotSupportedException>(()=>Evaluate(ordinary,new EvaluationContext().WithVariable("x",XdmValue.FromInteger(1))));
    }
    /// <summary>Static descriptors identify the numeric profile and capture the selected limits.</summary>
    [Fact] public void NumericEnvironmentIsCaptured()
    {
        using var a=Compile("1+1",8,4);using var b=Compile("1+1",9,4);using var c=Compile("1+1",8,4);
        Assert.Equal("bosak-exact-arithmetic-static-v1",a.Environment.DescriptorFormatVersion);
        Assert.NotEqual(a.Environment.StaticContextFingerprint,b.Environment.StaticContextFingerprint);Assert.Equal(a.Environment.StaticContextFingerprint,c.Environment.StaticContextFingerprint);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("1+1",Limits(),new CompileOptions{EnableJit=true}).Kind);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("1+1",Limits(),new CompileOptions{StrictStaticTyping=true}).Kind);
    }
    /// <summary>Iterative grammar evaluation handles deeply nested parentheses without recursive arithmetic parsing.</summary>
    [Fact] public void DeepGroupingAndRepeatedDispatchStayFinite()
    {
        using var nested=Compile(new string('(',4000)+"1"+new string(')',4000));Assert.Equal("1",Text(Evaluate(nested)));
        using var s=Compile("$x+1");for(int i=0;i<40;i++)Assert.Equal((i+1).ToString(CultureInfo.InvariantCulture),Text(Evaluate(s,new EvaluationContext().WithVariable("x",XdmValue.FromInteger(i)))));
    }
    private sealed class Source : ISchemaSourceBinding { }
    private sealed class Binding : ISchemaSnapshotBinding { }
    private sealed class Input : ISchemaAssessmentInput,IQueryOperationLifetime
    {
        internal int Checks,Releases,RootReads;
        internal Action? OnCheck;
        private readonly Source _source=new();private readonly Binding _binding=new();
        IXdmNode ISchemaAssessmentInput.Root {get{RootReads++;throw new Exception("Numeric evaluation must not read this root.");}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_binding;
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();Checks++;OnCheck?.Invoke();token.ThrowIfCancellationRequested();}
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)=>false;
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){token.ThrowIfCancellationRequested();return new Lease(this);}
        private sealed class Lease(Input owner):IQueryOperationLease
        {
            private Input? _owner=owner;
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token)=>token.ThrowIfCancellationRequested();
            /// <inheritdoc/>
            public void Dispose(){var input=Interlocked.Exchange(ref _owner,null);if(input is not null)input.Releases++;}
        }
    }
    /// <summary>Close during numeric dispatch revokes access and defers actual owner release to raw completion.</summary>
    /// <param name="profile">Zero for arithmetic, one for scalar, two for quotient, three for division, four for division strings, five for comparisons.</param>
    /// <returns>The completed lifetime verification.</returns>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)] public async Task CloseDuringArithmeticRetainsRawLease(int profile)
    {
        var source=string.Join("+",Enumerable.Repeat("1",500));var s=Compile(profile==5?"("+source+") div 1 eq 500":profile==4?"string(("+source+") div 1)":profile==3?"("+source+") div 1":profile==2?"("+source+") idiv 1":profile==1?"string("+source+")":source);var input=new Input();var snapshot=s.OpenSnapshot(input,input).Value;
        input.OnCheck=()=>{if(input.Checks==100){s.Dispose();Assert.Equal(0,input.Releases);}};
        Assert.Throws<ObjectDisposedException>(()=>s.Run([snapshot],op=>op.Evaluate(new EvaluationContext())));
        await s.DrainAsync();Assert.Equal(1,input.Releases);Assert.Equal(0,input.RootReads);
    }
    /// <summary>Cancellation inside arithmetic/scalar dispatch preserves the reusable source admission.</summary>
    /// <param name="profile">Zero for arithmetic, one for scalar, two for quotient, three for division, four for division strings, five for comparisons.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)] public void CancellationDuringArithmeticPreservesAdmission(int profile)
    {
        var source=string.Join("+",Enumerable.Repeat("1",500));using var s=Compile(profile==5?"("+source+") div 1 eq 500":profile==4?"string(("+source+") div 1)":profile==3?"("+source+") div 1":profile==2?"("+source+") idiv 1":profile==1?"string("+source+")":source);var input=new Input();var snapshot=s.OpenSnapshot(input,input).Value;
        using var cancellation=new CancellationTokenSource();input.OnCheck=()=>{if(input.Checks==100)cancellation.Cancel();};
        Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([snapshot],op=>op.Evaluate(new EvaluationContext()),cancellation.Token));Assert.Equal(0,input.Releases);
        input.OnCheck=null;Assert.Equal(profile==5?"true":"500",Text(s.Run([snapshot],op=>op.Evaluate(new EvaluationContext()))));Assert.Equal(0,input.RootReads);
    }
    private static EvaluationContext DivisionInputs()=>new EvaluationContext().WithVariable("x",XdmValue.FromInteger(1)).WithVariable("y",XdmValue.FromInteger(BigInteger.Pow(5,500)));
    /// <summary>Closing inside division factor work revokes the raw operation before owner release.</summary>
    /// <returns>Completion of the actual session drain.</returns>
    [Fact] public async Task CloseDuringDivisionFactorsDefersRelease()
    {
        var s=Compile("$x div $y",1024,1024);var input=new Input();var snapshot=s.OpenSnapshot(input,input).Value;
        input.OnCheck=()=>{if(input.Checks==100){s.Dispose();Assert.Equal(0,input.Releases);}};
        Assert.Throws<ObjectDisposedException>(()=>s.Run([snapshot],op=>op.Evaluate(DivisionInputs())));
        await s.DrainAsync();Assert.Equal(1,input.Releases);Assert.Equal(0,input.RootReads);
    }
    /// <summary>Cancellation inside factor work unwinds raw use and permits another exact evaluation.</summary>
    [Fact] public void CancellationDuringDivisionFactorsPreservesAdmission()
    {
        using var s=Compile("$x div $y",1024,1024);var input=new Input();var snapshot=s.OpenSnapshot(input,input).Value;
        using var cancellation=new CancellationTokenSource();input.OnCheck=()=>{if(input.Checks==100)cancellation.Cancel();};
        Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([snapshot],op=>op.Evaluate(DivisionInputs()),cancellation.Token));Assert.Equal(0,input.Releases);
        input.OnCheck=null;var result=s.Run([snapshot],op=>op.Evaluate(DivisionInputs())).ExactDecimalValue;
        Assert.Equal(BigInteger.Pow(2,500),result.Coefficient);Assert.Equal(500,result.Scale);Assert.Equal(0,input.RootReads);
        s.Dispose();Assert.Equal(1,input.Releases);
    }
    /// <summary>Zero and unsupported quotients leave admitted sources reusable and release exactly once on close.</summary>
    /// <returns>Completion of drain after the final successful evaluation.</returns>
    [Fact] public async Task DivisionErrorsRetainAdmissionUntilClose()
    {
        var s=Compile("1 div $y");var input=new Input();var snapshot=s.OpenSnapshot(input,input).Value;
        Assert.Throws<DivideByZeroException>(()=>s.Run([snapshot],op=>op.Evaluate(new EvaluationContext().WithVariable("y",XdmValue.FromInteger(0)))));
        Assert.Throws<NotSupportedException>(()=>s.Run([snapshot],op=>op.Evaluate(new EvaluationContext().WithVariable("y",XdmValue.FromInteger(3)))));
        Assert.Equal(0,input.Releases);Assert.Equal(0.125m,s.Run([snapshot],op=>op.Evaluate(new EvaluationContext().WithVariable("y",XdmValue.FromInteger(8)))).DecimalValue);
        s.Dispose();await s.DrainAsync();Assert.Equal(1,input.Releases);Assert.Equal(0,input.RootReads);
    }

}
