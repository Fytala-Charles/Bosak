// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies bounded public typed numeric execution and plain computed results.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; public typed numeric execution controls (1.60.0)                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 05-11-2026     | Retain scope controls and reuse numeric authority fixture (1.60.0).                        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
using Input=Bosak.XPath.Runtime.Tests.SchemaAdvisorAdapterTests.Input;
using Assessor=Bosak.XPath.Runtime.Tests.SchemaAdvisorAdapterTests.Assessor;
using Advisor=Bosak.XPath.Runtime.Tests.SchemaAdvisorAdapterTests.Advisor;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Origin=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Origin;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
using NumericAuthority=Bosak.XPath.Runtime.Tests.TypedNumericProjectionTests.NumericAuthority;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Qualifies public numeric execution with live authority and unmarked computed results.</summary>
/// <param name="output">Records complete public numeric-path allocation observations.</param>
public sealed class PublicTypedNumericTests(ITestOutputHelper output)
{
    private sealed class Fixture:IDisposable
    {
        internal readonly Session Session;internal readonly Input Input=new();internal readonly Assessor Assessor=new();internal readonly Advisor Advisor;
        internal readonly Handle Actual=new(),Governing=new();internal readonly Origin Origin=new();internal readonly NumericAuthority Authority;internal readonly SchemaQuerySnapshot Snapshot;
        internal Fixture(string source="$value + 0",bool integer=true,int digits=2048,bool original=true)
        {
            Authority=new(Actual,Governing){Family=integer?"integer":"decimal"};Assessor.Selected=Authority;Advisor=new(Assessor);Assessor.Advisor=Advisor;Advisor.Origins.Add(Origin);
            Session=Session.CompileXPath(source,new SchemaQueryLimits(4,8,8,digits,2048),assessor:Assessor).Value;Snapshot=Session.OpenSnapshot(Input,Input,original?Origin:null).Value;
        }
        internal Container Value(XdmValue value,bool original=true,bool list=false)
        {var c=new Container{Governing=Governing,List=list,Fast=true,Items=[]};c.Items=[new(value,new Provenance(c,Governing,Actual,original?Origin:null))];Advisor.Values[c]=(Input,original?Origin:null);return c;}
        internal T Run<T>(Func<SchemaQueryOperation,T> work,CancellationToken token=default)=>Session.Run([Snapshot],work,token);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    private static EvaluationContext Context(XdmValue value)=>new EvaluationContext().WithVariable("value",value);
    /// <summary>Public exact Integer operations compute plain results while original evidence remains reusable.</summary>
    /// <param name="source">Bounded source.</param>
    /// <param name="expected">Integer result.</param>
    [Theory][InlineData("$value + 0",2)][InlineData("$value + 3",5)][InlineData("$value - 5",-3)][InlineData("$value * 4",8)][InlineData("-$value",-2)][InlineData("+$value",2)][InlineData("$value idiv 2",1)][InlineData("$value mod 2",0)][InlineData("$value + ($value * 2)",6)]
    public void IntegerOperations(string source,long expected)
    {
        using var f=new Fixture(source);var c=f.Value(XdmValue.FromInteger(2,"integer","derived"));f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,c).Value;var marker=XdmValue.SchemaTransportMarker(value);var result=op.Evaluate(Context(value));Assert.Equal(expected,result.IntegerValue);Assert.Null(XdmValue.SchemaTransportMarker(result));Assert.Null(result.UserSchemaTypeName);Assert.Same(marker,XdmValue.SchemaTransportMarker(value));Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(result)));Assert.Equal(expected,op.Evaluate(Context(value)).IntegerValue);return 0;});Assert.Same(f.Advisor,f.Session.Advisor);Assert.Equal("bosak-typed-exact-numeric-v1",f.Session.Environment.BehaviorProfile);Assert.Equal("bosak-typed-exact-numeric-descriptor-v1",f.Session.Environment.DescriptorFormatVersion);
    }
    /// <summary>Comparisons return plain Boolean computation results.</summary>
    /// <param name="source">Comparison source.</param>
    /// <param name="expected">Boolean result.</param>
    [Theory][InlineData("$value eq 2",true)][InlineData("$value ne 2",false)][InlineData("$value lt 3",true)][InlineData("$value ge 3",false)][InlineData("$value = 2",true)]
    public void Comparisons(string source,bool expected)
    {
        using var f=new Fixture(source);f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(2))).Value;var result=op.Evaluate(Context(value));Assert.Equal(expected,result.BooleanValue);Assert.Null(XdmValue.SchemaTransportMarker(result));return 0;});
    }
    /// <summary>Decimal member projection and terminating division retain exact primitive computation.</summary>
    /// <param name="source">Arithmetic source.</param>
    /// <param name="expected">Decimal result.</param>
    [Theory][InlineData("$value + 0.5","1.75")][InlineData("$value div 2","0.625")]
    public void DecimalOperations(string source,string expected)
    {
        using var f=new Fixture(source,false,original:false);var c=f.Value(XdmValue.FromDecimal(1.25m,"decimal","derived"),false,true);f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,c).Value;var result=op.Evaluate(Context(value));Assert.Equal(decimal.Parse(expected,System.Globalization.CultureInfo.InvariantCulture),result.DecimalValue);Assert.Null(XdmValue.SchemaTransportMarker(result));return 0;});
    }
    /// <summary>Wide integers do not narrow to CLR integer or floating-point storage.</summary>
    [Fact] public void WideExactInteger()
    {using var f=new Fixture("$value + 1");var n=BigInteger.Pow(10,1000);f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(n))).Value;Assert.Equal(n+1,op.Evaluate(Context(value)).BigIntegerValue);return 0;});}
    /// <summary>Unselected source forms refuse before provider binding.</summary>
    /// <param name="source">Unavailable source.</param>
    [Theory][InlineData("string($value)")][InlineData("xs:integer($value)")][InlineData("$other + 1")][InlineData("$value + $other")][InlineData("1 + 2")][InlineData("doc('x')")][InlineData("($value,$value)")]
    public void UnsupportedGrammarBeforeBind(string source)
    {var a=new Assessor{Hook=_=>throw new Exception("bind poison")};Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(source,new SchemaQueryLimits(4,8,8,2048,2048),assessor:a).Kind);Assert.Equal(0,a.Binds);}
    /// <summary>Names and primitive values do not replace positive actual-type authorization.</summary>
    [Fact] public void TypeAuthorityRequired()
    {
        using var f=new Fixture();f.Authority.Family="string";f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(1,"integer","xs:integer"))).Value;Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(value)));return 0;});
    }
    /// <summary>Plain values and foreign evidence cannot become authenticated numeric operands.</summary>
    [Fact] public void PlainAndForeignOperandsRefuse()
    {
        using var f=new Fixture();using var g=new Fixture();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(2))).Value);g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(value)));Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(XdmValue.FromInteger(2))));return 0;});
    }
    /// <summary>Revalidation catches lost authority after computation has started and permits a valid retry.</summary>
    [Fact] public void MidExecutionAuthorityRevocation()
    {
        using var f=new Fixture("$value * $value + 1");var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(2))).Value);int queries=0;f.Authority.Hook=s=>{if(s=="resolve-integer"&&++queries==4){f.Authority.ForceStage="derive-integer";f.Authority.ForceKind=SchemaResultKind.Unsupported;}};
        f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(value)));f.Authority.Hook=null;f.Authority.ForceStage="";Assert.Equal(5,op.Evaluate(Context(value)).IntegerValue);return 0;});Assert.True(queries>=4);
    }
    /// <summary>Cancellation and close during arithmetic checkpoints prevent result delivery.</summary>
    /// <param name="close">Whether to close the session rather than cancel.</param>
    [Theory][InlineData(false)][InlineData(true)] public void InterruptionDuringExecution(bool close)
    {
        using var f=new Fixture("$value * $value + 1");var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(2))).Value);using var cancel=new CancellationTokenSource();int queries=0;f.Authority.Hook=s=>{if(s=="resolve-integer"&&++queries==4){if(close)f.Session.Dispose();else cancel.Cancel();}};
        f.Run(op=>{if(close)Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(Context(value)));else Assert.Throws<OperationCanceledException>(()=>op.Evaluate(Context(value)));Assert.Equal(0,f.Input.Releases);return 0;},cancel.Token);
    }
    /// <summary>Nonterminating and zero division retain the previously qualified bounded refusal/error behavior.</summary>
    /// <param name="source">Division expression.</param>
    [Theory][InlineData("$value div 3")][InlineData("$value div 0")]
    public void DivisionBoundaries(string source)
    {using var f=new Fixture(source);f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(1))).Value;if(source.EndsWith(" 0",StringComparison.Ordinal))Assert.Contains("FOAR0001",Assert.Throws<DivideByZeroException>(()=>op.Evaluate(Context(value))).Message);else Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(value)));return 0;});}
    /// <summary>Intermediate digit limits are enforced after authorized operand projection.</summary>
    [Fact] public void IntermediateCapacityRefuses()
    {using var f=new Fixture("$value * $value",digits:3);f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(999))).Value;Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(value)));return 0;});}
    /// <summary>Existing public typed identity retains its distinct profile and marked result.</summary>
    [Fact] public void IdentityRemainsIdentity()
    {using var f=new Fixture("$value");f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,f.Value(XdmValue.FromInteger(2))).Value;Assert.Same(XdmValue.SchemaTransportMarker(value),XdmValue.SchemaTransportMarker(op.Evaluate(Context(value))));return 0;});Assert.Equal("bosak-typed-identity-v1",f.Session.Environment.BehaviorProfile);}
    /// <summary>Warm public computation revalidates ownership without replaying provider payload getters.</summary>
    [Fact] public void WarmReadAndAllocationObservation()
    {
        using var f=new Fixture("$value + 0");f.Authority.Record=false;var c=f.Value(XdmValue.FromInteger(2));var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);int reads=c.Reads.Count;c.Fail="governing";
        f.Run(op=>{var context=Context(value);for(int i=0;i<100;i++)op.Evaluate(context);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)op.Evaluate(context);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;output.WriteLine($"Public typed numeric compute: {bytes} bytes/call");Assert.InRange(bytes,1,32768);return 0;});Assert.Equal(reads,c.Reads.Count);
    }

    /// <summary>Captured numeric limits and arithmetic lane distinguish immutable public descriptors.</summary>
    [Fact] public void DescriptorSeparatesLimitsAndLane()
    {
        using var normal=new Fixture("$value + 0");using var tight=new Fixture("$value + 0",digits:3);using var compare=new Fixture("$value eq 0");Assert.NotEqual(normal.Session.Environment.StaticContextFingerprint,tight.Session.Environment.StaticContextFingerprint);Assert.NotEqual(normal.Session.Environment.StaticContextFingerprint,compare.Session.Environment.StaticContextFingerprint);Assert.Same(normal.Session.Environment,normal.Assessor.Seen);Assert.Equal(normal.Session.Environment.BehaviorProfile,compare.Session.Environment.BehaviorProfile);
    }
}
