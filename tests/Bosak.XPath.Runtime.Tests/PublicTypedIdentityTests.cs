// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies public assessor-bound single-atomic typed identity and refusal boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; public typed identity controls (1.58.0)                                                |
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
using Authority=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Authority;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Origin=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Origin;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Qualifies public compile, admission, typed reads and identity evaluation without engine candidate entry points.</summary>
/// <param name="output">Records public-path allocation observations.</param>
public sealed class PublicTypedIdentityTests(ITestOutputHelper output)
{
    private sealed class Fixture:IDisposable
    {
        internal readonly Session Session;internal readonly Input Input=new();internal readonly Assessor Assessor=new();internal readonly Advisor Advisor;
        internal readonly Handle Governing=new(),Actual=new();internal readonly Origin Origin=new();internal readonly SchemaQuerySnapshot Snapshot;
        internal Fixture(bool original=true,int cache=8,int atoms=8)
        {
            Advisor=new(Assessor);Assessor.Advisor=Advisor;((Authority)Assessor.Selected).Types.UnionWith([Governing,Actual]);Advisor.Origins.Add(Origin);
            Session=Session.CompileXPath("$value",new SchemaQueryLimits(4,cache,atoms,2048,2048),assessor:Assessor).Value;
            Snapshot=Session.OpenSnapshot(Input,Input,original?Origin:null).Value;
        }
        internal Container Value(int count=1,bool list=false,bool original=true)
        {
            var c=new Container{Governing=Governing,List=list,Fast=count==1};c.Items=Enumerable.Range(0,count).Select(i=>new SchemaAtomicItem(XdmValue.FromInteger(i+1),new Provenance(c,Governing,Actual,original?Origin:null))).ToArray();Advisor.Values.Add(c,(Input,original?Origin:null));return c;
        }
        internal T Run<T>(Func<SchemaQueryOperation,T> work,CancellationToken token=default)=>Session.Run([Snapshot],work,token);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    private static EvaluationContext Context(XdmValue value)=>new EvaluationContext().WithVariable("value",value);
    /// <summary>Public-only flow preserves authenticated scalar identity across all representative atomic kinds.</summary>
    /// <param name="kind">Payload selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)]
    public void PublicRoundTrip(int kind)
    {
        using var f=new Fixture();var c=f.Value();XdmValue plain=kind switch{0=>XdmValue.FromInteger(BigInteger.Pow(10,1000)),1=>XdmValue.FromString("value","xs:string","urn:user:type"),2=>XdmValue.FromBoolean(true),3=>XdmValue.FromDouble(-0d),4=>XdmValue.FromDecimal(new XdmExactDecimal(12300,4,2048,2048)),5=>XdmValue.FromQName(new XsQName("n","urn:n","p")),6=>XdmValue.FromDateTime(new DateTimeOffset(2026,11,5,12,0,0,TimeSpan.Zero)),_=>XdmValue.FromDuration("P2D")};c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        Assert.Same(f.Advisor,f.Session.Advisor);Assert.Same(f.Session.Environment,f.Assessor.Seen);Assert.Equal("bosak-typed-identity-v1",f.Session.Environment.BehaviorProfile);Assert.Equal("bosak-public-typed-identity-descriptor-v1",f.Session.Environment.DescriptorFormatVersion);
        var copied=f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,c).Value;var result=op.Evaluate(Context(value));Assert.Equal(plain,result);return (XdmValue)(object)result;});f.Run(op=>{Assert.Equal(plain,op.Evaluate(Context(copied)));return 0;});Assert.Equal(0,f.Input.Roots);Assert.Equal(0,f.Advisor.Disposals);Assert.Equal(0,f.Assessor.Disposals);
    }
    /// <summary>Singleton lists preserve actual atomic payload and optional original assessment without getter replay.</summary>
    /// <param name="original">Whether original assessment is present.</param>
    [Theory][InlineData(false)][InlineData(true)] public void SingletonListWarmReuse(bool original)
    {
        using var f=new Fixture(original);var c=f.Value(1,true,original);var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);int reads=c.Reads.Count;c.Fail="governing";
        for(int i=0;i<10;i++)f.Run(op=>{Assert.Equal(1,op.Evaluate(Context(op.ReadTypedValue(f.Snapshot,c).Value)).IntegerValue);Assert.Equal(1,op.Evaluate(Context(value)).IntegerValue);return 0;});Assert.Equal(reads,c.Reads.Count);Assert.Same(f.Input,f.Advisor.LastInput);
    }
    /// <summary>Empty/multi-atomic projection refuses before publication and leaves capacity for a valid singleton.</summary>
    /// <param name="count">Unsupported atomic count.</param>
    [Theory][InlineData(0)][InlineData(2)][InlineData(3)] public void UnsupportedShapeDoesNotConsumeCache(int count)
    {
        using var f=new Fixture(cache:1);var c=f.Value(count,true);f.Run(op=>{var refused=op.ReadTypedValue(f.Snapshot,c);Assert.Equal(SchemaResultKind.Unsupported,refused.Kind);Assert.Equal("BOSAK-TYPED-IDENTITY-UNSUPPORTED",refused.DiagnosticCode);Assert.Equal(1,op.ReadTypedValue(f.Snapshot,f.Value()).Value.IntegerValue);return 0;});
    }
    /// <summary>Full atom quotas apply before shape refusal and a failed read can be retried without reservations.</summary>
    [Fact] public void AtomQuotaAndRetry()
    {
        using var f=new Fixture(cache:1,atoms:2);var c=f.Value(3,true);c.ForbiddenCurrent=2;f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedValue(f.Snapshot,c));Assert.Equal(2,c.Currents);Assert.Equal(1,op.ReadTypedValue(f.Snapshot,f.Value()).Value.IntegerValue);return 0;});
    }
    /// <summary>Malformed multi-atomic metadata is not silently hidden behind Unsupported projection.</summary>
    [Fact] public void ValidateBeforeShapeRefusal()
    {
        using var f=new Fixture(cache:1);var c=f.Value(2,true);c.Items[1]=new(XdmValue.FromInteger(2),new Provenance(c,f.Governing,new Handle(),f.Origin));f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedValue(f.Snapshot,c));Assert.Equal(1,op.ReadTypedValue(f.Snapshot,f.Value()).Value.IntegerValue);return 0;});
    }
    /// <summary>Unsupported grammar refuses before invoking a poisoned assessor.</summary>
    /// <param name="source">Unavailable source.</param>
    [Theory][InlineData("true()")][InlineData("$v")][InlineData("$value + $other")][InlineData("string($value)")]
    public void GrammarRefusesBeforeBind(string source)
    {var a=new Assessor{Hook=_=>throw new Exception("bind poison")};Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(source,new SchemaQueryLimits(4,8,8,2048,2048),assessor:a).Kind);Assert.Equal(0,a.Binds);}
    /// <summary>Public binding propagates Unsupported without reading Value and never disposes borrowed endpoints.</summary>
    [Fact] public void UnsupportedBinding()
    {
        var a=new Assessor();a.Outcome=()=>new UnsupportedAdvisor();Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("$value",new SchemaQueryLimits(4,8,8,2048,2048),assessor:a).Kind);Assert.Equal(0,a.Disposals);
    }
    private sealed class UnsupportedAdvisor:SchemaResult<ISchemaAssessmentAdvisor>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind=>SchemaResultKind.Unsupported;
        /// <inheritdoc/>
        public override ISchemaAssessmentAdvisor Value=>throw new Exception("unsupported poison");
        /// <inheritdoc/>
        public override string? DiagnosticCode=>null;
    }
    /// <summary>Unknown original assessment refuses and compatibility remains outside the profile.</summary>
    [Fact] public void OriginalAssessmentOwnership()
    {
        using var f=new Fixture();var input=new Input();Assert.Throws<ArgumentException>(()=>f.Session.OpenSnapshot(input,input,new Origin()));Assert.Equal(0,input.Releases);Assert.Equal(0,input.Roots);
    }
    /// <summary>Foreign raw snapshot and foreign evidence refuse without invoking the other advisor.</summary>
    [Fact] public void ForeignSnapshotAndEvidence()
    {
        using var f=new Fixture();using var g=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);f.Advisor.Hook=_=>throw new Exception("foreign advisor poison");g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedValue(f.Snapshot,c));Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(value)));return 0;});
    }
    /// <summary>Plain and structured data cannot acquire authentication from public identity evaluation.</summary>
    [Fact] public void PlainAndStructuredInputsRefuse()
    {
        using var f=new Fixture();f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(XdmValue.FromString("plain"))));Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(XdmValue.FromSequence(XdmSequence.Empty))));Assert.Throws<NotSupportedException>(()=>op.Evaluate(new EvaluationContext()));Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(XdmValue.FromInteger(1)).WithVariable("extra",XdmValue.FromInteger(2))));return 0;});
    }
    /// <summary>Services and nondefault execution state refuse before provider membership calls.</summary>
    [Fact] public void ContextServicesRefuse()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);f.Advisor.Hook=_=>throw new Exception("service provider poison");f.Run(op=>{var context=Context(value);context.ResourcePolicy=SchemaProviderSessionBinding.ClosedResources();Assert.Throws<NotSupportedException>(()=>op.Evaluate(context));return 0;});
    }
    /// <summary>Current issuer revocation refuses warm public evaluation without replaying payload getters.</summary>
    [Fact] public void RevokedContainerRefuses()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);int reads=c.Reads.Count;f.Advisor.Values.Clear();f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(value)));return 0;});Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Real source resources survive logical close until raw callback completion and drain.</summary>
    /// <returns>Completion of source drain.</returns>
    [Fact] public async Task ActualResourceClose()
    {
        string path=Path.Combine(Path.GetTempPath(),"bosak-public-typed-"+Guid.NewGuid().ToString("N")+".tmp");using var file=new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None,4096,FileOptions.DeleteOnClose);using var f=new Fixture();f.Input.Resource=file;var c=f.Value();
        f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,c).Value;f.Session.Dispose();Assert.False(file.SafeFileHandle.IsClosed);Assert.Equal(0,f.Input.Releases);Assert.Throws<ObjectDisposedException>(()=>op.Evaluate(Context(value)));return 0;});await f.Session.DrainAsync();Assert.True(file.SafeFileHandle.IsClosed);Assert.Equal(1,f.Input.Releases);Assert.Equal(0,f.Advisor.Disposals);
    }
    /// <summary>Public cancellation prevents delivery while preserving actual raw-owner cleanup.</summary>
    [Fact] public void CancellationDuringMembership()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);using var cancel=new CancellationTokenSource();f.Advisor.Hook=s=>{if(s=="value")cancel.Cancel();};f.Run(op=>{Assert.Throws<OperationCanceledException>(()=>op.Evaluate(Context(value)));return 0;},cancel.Token);
    }
    /// <summary>Escaped public operations expire while copied values need a new admitted raw operation.</summary>
    [Fact] public void EscapedOperationRefuses()
    {
        using var f=new Fixture();SchemaQueryOperation? escaped=null;var value=f.Run(op=>{escaped=op;return op.ReadTypedValue(f.Snapshot,f.Value()).Value;});Assert.Throws<ObjectDisposedException>(()=>escaped!.Evaluate(Context(value)));f.Run(op=>{Assert.Equal(1,op.Evaluate(Context(value)).IntegerValue);return 0;});
    }
    /// <summary>Observes the complete public evaluation path including bounded context capture and owner resolution.</summary>
    [Fact] public void PublicAllocationObservation()
    {
        using var f=new Fixture();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,f.Value()).Value);f.Run(op=>{var context=Context(value);for(int i=0;i<100;i++)op.Evaluate(context);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)op.Evaluate(context);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;output.WriteLine($"Public typed identity evaluate: {bytes} bytes/call");Assert.InRange(bytes,1,16384);return 0;});
    }

    /// <summary>Capacity refusal leaves the original scalar identity usable and does not evict it.</summary>
    [Fact] public void CapacityRefusalPreservesExisting()
    {
        using var f=new Fixture(cache:1);var c=f.Value();f.Run(op=>{var value=op.ReadTypedValue(f.Snapshot,c).Value;Assert.Throws<NotSupportedException>(()=>op.ReadTypedValue(f.Snapshot,f.Value()));Assert.Equal(1,op.Evaluate(Context(value)).IntegerValue);Assert.Equal(value,op.ReadTypedValue(f.Snapshot,c).Value);return 0;});
    }
    /// <summary>Two admitted public inputs keep distinct issued ownership while evaluation resolves each scalar exactly.</summary>
    [Fact] public void MultipleSnapshotsUseExactInput()
    {
        using var f=new Fixture();var second=new Input();using var snapshot=f.Session.OpenSnapshot(second,second,f.Origin).Value;var a=f.Value();var b=f.Value();f.Advisor.Values[b]=(second,f.Origin);
        f.Session.Run([f.Snapshot,snapshot],op=>{var x=op.ReadTypedValue(f.Snapshot,a).Value;var y=op.ReadTypedValue(snapshot,b).Value;Assert.Throws<ArgumentException>(()=>op.ReadTypedValue(snapshot,a));Assert.Equal(1,op.Evaluate(Context(x)).IntegerValue);Assert.Equal(1,op.Evaluate(Context(y)).IntegerValue);return 0;});
    }
    /// <summary>Post-acquisition origin failure releases the actual admission once and permits a valid retry.</summary>
    [Fact] public void AdmissionRollbackAndRetry()
    {
        using var f=new Fixture();var input=new Input();int origins=0;f.Advisor.Hook=s=>{if(s=="origin"&&++origins==2)throw new InvalidOperationException("origin failure");};
        Assert.Equal("origin failure",Assert.Throws<InvalidOperationException>(()=>f.Session.OpenSnapshot(input,input,f.Origin)).Message);Assert.Equal(1,input.Releases);f.Advisor.Hook=null;using var retry=f.Session.OpenSnapshot(input,input,f.Origin).Value;Assert.Equal(0,f.Advisor.Disposals);
    }
    /// <summary>Focus and namespaced variable substitutions refuse before advisor calls.</summary>
    [Fact] public void ContextShapeRefusesBeforeProvider()
    {
        using var f=new Fixture();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,f.Value()).Value);f.Advisor.Hook=_=>throw new Exception("context provider poison");
        f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(Context(value).WithFocus(XdmValue.FromString("focus"),1,1)));Assert.Throws<NotSupportedException>(()=>op.Evaluate(new EvaluationContext().WithVariable("value",value,"urn:foreign")));return 0;});
    }
    /// <summary>Cancellation during public binding does not dispose borrowed assessor/advisor resources.</summary>
    [Fact] public void BindingCancellationPreservesBorrowedOwners()
    {
        using var cancel=new CancellationTokenSource();var a=new Assessor();var advisor=new Advisor(a);a.Advisor=advisor;a.Hook=s=>{if(s=="bind")cancel.Cancel();};Assert.Throws<OperationCanceledException>(()=>Session.CompileXPath("$value",new SchemaQueryLimits(4,8,8,2048,2048),assessor:a,cancellationToken:cancel.Token));Assert.Equal(0,a.Disposals);Assert.Equal(0,advisor.Disposals);
    }
    /// <summary>A Supported advisor with a foreign endpoint refuses through the public compile path.</summary>
    [Fact] public void MalformedBindingRefuses()
    {
        var a=new Assessor();var advisor=new Advisor(a){Endpoint=new Assessor()};a.Advisor=advisor;Assert.Throws<ArgumentException>(()=>Session.CompileXPath("$value",new SchemaQueryLimits(4,8,8,2048,2048),assessor:a));Assert.Equal(0,advisor.Disposals);
    }
}
