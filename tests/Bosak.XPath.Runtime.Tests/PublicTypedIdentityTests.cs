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
//                      | Charles Korthout | 0.3 | 06-11-2026     | Verify bounded public provenance observation and expiry (1.77.0). |
//                      | Charles Korthout | 0.4 | 06-11-2026     | Verify complete ordinary container transport and raw-bound access (1.79.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5 | 06-11-2026     | Qualify wider ordinary complete-container representations and expiry (1.81.0). |
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
        internal Fixture(bool original=true,int cache=8,int atoms=8,CancellationToken opening=default)
        {
            Advisor=new(Assessor);Assessor.Advisor=Advisor;((Authority)Assessor.Selected).Types.UnionWith([Governing,Actual]);Advisor.Origins.Add(Origin);
            Session=Session.CompileXPath("$value",new SchemaQueryLimits(4,cache,atoms,2048,2048),assessor:Assessor).Value;
            Snapshot=Session.OpenSnapshot(Input,Input,original?Origin:null,cancellationToken:opening).Value;
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

    /// <summary>PV01 genuine public scalar carriers expose cached references without allocating a view.</summary>
    /// <param name="kind">Representative atomic representation.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)]
    [InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)][InlineData(11)]
    public void PV01_ObservedCarriers(int kind)
    {
        using var f=new Fixture();var c=f.Value();
        var plain=kind switch {0=>XdmValue.FromString("value"),1=>XdmValue.FromBoolean(true),2=>XdmValue.FromInteger(42),
            3=>XdmValue.FromInteger(BigInteger.Pow(10,70)),4=>XdmValue.FromDecimal(1.2300m),5=>XdmValue.FromDouble(-0.0d),
            6=>XdmValue.FromFloat(float.NaN),7=>XdmValue.FromQName(new("urn:x","n","p")),
            8=>XdmValue.FromDate(DateTimeOffset.UnixEpoch),9=>XdmValue.FromDateTime(DateTimeOffset.UnixEpoch),
            10=>XdmValue.FromDuration("P2D"),_=>XdmValue.FromString("AA==","base64Binary")};
        c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);int checks=f.Input.Checks;
        c.OnRead=_=>throw new Exception("metadata replay poison");f.Advisor.Hook=_=>throw new Exception("advisor poison");
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));
        Assert.Same(f.Governing,evidence!.DeclaredType);Assert.Same(f.Actual,evidence.ActualType);Assert.Same(f.Origin,evidence.OriginalAssessment);
        Assert.Same(XdmValue.SchemaTransportMarker(value),evidence);Assert.True(value.SchemaTransportMatches(plain));Assert.Equal(checks,f.Input.Checks);
        f.Advisor.Hook=null;
    }

    /// <summary>PV02 one observed atomic never erases its original list shape or invents an origin.</summary>
    /// <param name="original">Whether the admission has original assessment.</param>
    /// <param name="list">Whether the provider container is a singleton list.</param>
    [Theory][InlineData(true,true)][InlineData(true,false)][InlineData(false,true)][InlineData(false,false)]
    public void PV02_ListAndOrigin(bool original,bool list)
    {
        using var f=new Fixture(original);var c=f.Value(list:list,original:original);var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Same(f.Governing,evidence!.DeclaredType);
        Assert.Same(f.Actual,evidence.ActualType);Assert.Same(original?f.Origin:null,evidence.OriginalAssessment);
        Assert.Equal(list,c.List);Assert.Equal(1,f.Snapshot.Runtime.TypedScalarCount);
    }

    /// <summary>PV03 engine snapshot/session/opening cancellation expires all observed getters.</summary>
    /// <param name="mode">Engine expiry source.</param>
    [Theory][InlineData("snapshot")][InlineData("session")][InlineData("cancel")]
    public void PV03_ObservationExpiry(string mode)
    {
        using var cancellation=new CancellationTokenSource();using var f=new Fixture(opening:cancellation.Token);
        var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));
        if(mode=="snapshot")f.Snapshot.Dispose();else if(mode=="session")f.Session.Dispose();else cancellation.Cancel();
        int checks=f.Input.Checks;
        Assert.False(SchemaAtomics.TryGetProvenance(in value,out var missing));Assert.Null(missing);
        foreach(var getter in new Func<object?>[]{()=>evidence!.DeclaredType,()=>evidence!.ActualType,()=>evidence!.OriginalAssessment})
        {var error=Record.Exception(()=>getter());if(mode=="cancel")Assert.IsAssignableFrom<OperationCanceledException>(error);else Assert.IsType<ObjectDisposedException>(error);}
        Assert.Equal(checks,f.Input.Checks);
    }

    /// <summary>PV04 exact payload and type tags remain bound to the registered scalar.</summary>
    /// <param name="mode">Altered representation.</param>
    [Theory][InlineData("value")][InlineData("kind")][InlineData("tag")][InlineData("user")]
    public void PV04_ReboundCarrier(string mode)
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        var plain=mode switch {"value"=>XdmValue.FromInteger(2),"kind"=>XdmValue.FromString("1"),"tag"=>XdmValue.FromInteger(1,"long"),_=>XdmValue.FromInteger(1,"integer","Q{urn:x}T")};
        var rebound=XdmValue.SchemaTransportAttach(plain,XdmValue.SchemaTransportMarker(value)!);
        Assert.False(SchemaAtomics.TryGetProvenance(in rebound,out var evidence));Assert.Null(evidence);
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out _));Assert.Equal(1,f.Snapshot.Runtime.TypedScalarCount);
    }

    /// <summary>PV05 descriptive metadata is not fresh provider authorization and invokes no callbacks.</summary>
    [Fact] public void PV05_NoAuthorityRefresh()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        f.Advisor.Available=false;f.Advisor.Hook=_=>throw new Exception("provider poison");((Authority)f.Assessor.Selected).OnOwn=_=>throw new Exception("authority poison");
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Same(f.Actual,evidence!.ActualType);
        Assert.Same(f.Origin,evidence.OriginalAssessment);f.Advisor.Hook=null;((Authority)f.Assessor.Selected).OnOwn=null;
        Assert.Throws<ObjectDisposedException>(()=>f.Run(op=>op.Evaluate(Context(value))));
    }

    /// <summary>PV06 warm observation/getters allocate no caller objects and consume no extra scalar slots.</summary>
    [Fact] public void PV06_ObservationCost()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        for(int i=0;i<100;i++){SchemaAtomics.TryGetProvenance(in value,out var p);_=p!.ActualType;}
        long start=GC.GetAllocatedBytesForCurrentThread();bool valid=true;ISchemaAtomicProvenance? last=null;
        for(int i=0;i<1000;i++){valid&=SchemaAtomics.TryGetProvenance(in value,out last);_=last!.DeclaredType;_=last.ActualType;_=last.OriginalAssessment;}
        long bytes=GC.GetAllocatedBytesForCurrentThread()-start;output.WriteLine($"Provenance warm observation: {bytes} caller bytes / 1000 observations with three getters");
        Assert.True(valid);Assert.Equal(0,bytes);Assert.Equal(1,f.Snapshot.Runtime.TypedScalarCount);
    }

    /// <summary>PV07 close after observation cannot turn a retained metadata view into live authority.</summary>
    /// <returns>The controlled close/getter check.</returns>
    [Fact] public async Task PV07_CloseAfterObservation()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));
        await Task.Run(f.Snapshot.Dispose);int checks=f.Input.Checks;
        Assert.Throws<ObjectDisposedException>(()=>evidence!.DeclaredType);Assert.False(SchemaAtomics.TryGetProvenance(in value,out _));Assert.Equal(checks,f.Input.Checks);
    }


    /// <summary>PV08 observed metadata cannot issue a new provider-owned container or scalar attachment.</summary>
    [Fact] public void PV08_ObservationIsNotAuthority()
    {
        using var f=new Fixture();var c=f.Value();var value=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);
        Assert.True(SchemaAtomics.TryGetProvenance(in value,out var evidence));
        var forged=new Container{Governing=f.Governing,Fast=true,Items=[new(XdmValue.FromInteger(1),evidence!)]};
        Assert.Throws<ArgumentException>(()=>f.Run(op=>op.ReadTypedValue(f.Snapshot,forged)));
        Assert.Equal(1,f.Snapshot.Runtime.TypedScalarCount);Assert.True(SchemaAtomics.TryGetProvenance(in value,out _));
    }

    /// <summary>TC01 preserves governing type, list flag and complete cardinality independently.</summary>
    /// <param name="list">Original list flag.</param>
    /// <param name="count">Complete cardinality.</param>
    [Theory][InlineData(false,0)][InlineData(false,1)][InlineData(false,3)][InlineData(true,0)][InlineData(true,1)][InlineData(true,3)]
    public void TC01_CompleteShape(bool list,int count)
    {
        using var f=new Fixture(cache:1);var c=f.Value(count,list);
        f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;Assert.Same(f.Governing,v.GoverningType);Assert.Equal(list,v.IsList);var items=v.AtomicItems.ToArray();Assert.Equal(count,items.Length);Assert.Equal(count==1,v.TryGetSingleAtomic(out var one));if(count!=1)Assert.Null(one.TypeEvidence);for(int n=0;n<count;n++){Assert.Equal(n+1,items[n].Value.IntegerValue);Assert.Same(f.Actual,items[n].TypeEvidence.ActualType);Assert.Same(f.Governing,items[n].TypeEvidence.DeclaredType);Assert.Same(f.Origin,items[n].TypeEvidence.OriginalAssessment);Assert.Null(XdmValue.SchemaTransportMarker(items[n].Value));}Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>TC02 selected families remain plain without acquiring an atomic marker.</summary>
    /// <param name="kind">Selected primitive family.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void TC02_SelectedFamilies(int kind)
    {
        using var f=new Fixture();var c=f.Value();var plain=kind switch{0=>XdmValue.FromString("text"),1=>XdmValue.FromBoolean(true),2=>XdmValue.FromInteger(BigInteger.Pow(10,1000)),_=>XdmValue.FromDecimal(new XdmExactDecimal(12300,4,2048,2048))};c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;Assert.True(v.TryGetSingleAtomic(out var item));Assert.Equal(plain,item.Value);var scalar=item.Value;Assert.False(SchemaAtomics.TryGetProvenance(in scalar,out _));return 0;});
    }
    /// <summary>TC03 mixed union selections and optional original origin retain exact ordered handles.</summary>
    /// <param name="original">Whether original assessment is present.</param>
    [Theory][InlineData(false)][InlineData(true)] public void TC03_UnionMembers(bool original)
    {
        using var f=new Fixture(original);var c=f.Value(3,true,original);var alternate=new Handle();((Authority)f.Assessor.Selected).Types.Add(alternate);c.Items[1]=new(XdmValue.FromString("middle"),new Provenance(c,alternate,f.Governing,original?f.Origin:null));
        f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;var items=v.AtomicItems.ToArray();Assert.Same(alternate,items[1].TypeEvidence.DeclaredType);Assert.Same(f.Governing,items[1].TypeEvidence.ActualType);Assert.Same(f.Actual,items[2].TypeEvidence.ActualType);Assert.Equal("middle",items[1].Value.StringValue);Assert.Same(original?f.Origin:null,items[1].TypeEvidence.OriginalAssessment);return 0;});
    }
    /// <summary>TC04 a previously unsupported family now has complete transport without scalar reservations.</summary>
    [Fact] public void TC04_WiderFamilySupported()
    {
        using var f=new Fixture(cache:1);var c=f.Value(3,true);c.Items[1]=new(XdmValue.FromDouble(2d),new Provenance(c,f.Governing,f.Actual,f.Origin));
        f.Run(op=>{Assert.Equal(3,op.ReadTypedContainer(f.Snapshot,c).Value.AtomicItems.Count());Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);Assert.Equal(1,op.ReadTypedValue(f.Snapshot,f.Value()).Value.IntegerValue);return 0;});
    }
    /// <summary>TC05 malformed later members are rejected after a wider valid family.</summary>
    /// <param name="kind">Malformed representation.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void TC05_InvalidTail(int kind)
    {
        using var f=new Fixture(cache:1);var c=f.Value(3,true);c.Items[0]=new(XdmValue.FromDouble(1d),new Provenance(c,f.Governing,f.Actual,f.Origin));
        c.Items[2]=kind switch{0=>default,1=>new(XdmValue.Undefined,new Provenance(c,f.Governing,f.Actual,f.Origin)),2=>new(XdmValue.FromString(null!),new Provenance(c,f.Governing,f.Actual,f.Origin)),_=>new(XdmValue.FromInteger(3),new Provenance(c,f.Governing,new Handle(),f.Origin))};
        f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>TC06 repeated empty and list reads never allocate persistent scalar entries or replay cached getters.</summary>
    [Fact] public void TC06_RepeatedUncached()
    {
        using var f=new Fixture(cache:1);var c=f.Value(2,true);var empty=f.Value(0,true);f.Run(op=>{for(int i=0;i<12;i++){Assert.Empty(op.ReadTypedContainer(f.Snapshot,empty).Value.AtomicItems);Assert.Equal(2,op.ReadTypedContainer(f.Snapshot,c).Value.AtomicItems.Count());}Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);Assert.Equal(1,op.ReadTypedValue(f.Snapshot,f.Value()).Value.IntegerValue);return 0;});Assert.Equal(12,c.Disposals);Assert.Equal(12,empty.Disposals);
    }
    /// <summary>TC07 late container, enumerable, cursor and evidence access refuses before provider callbacks.</summary>
    [Fact] public void TC07_CompleteExpiry()
    {
        using var f=new Fixture();var c=f.Value();ISchemaTypedValue view=null!;IEnumerable<SchemaAtomicItem> seq=null!;IEnumerator<SchemaAtomicItem> cursor=null!;SchemaAtomicItem copied=default;SchemaQueryOperation raw=null!;
        f.Run(op=>{raw=op;view=op.ReadTypedContainer(f.Snapshot,c).Value;seq=view.AtomicItems;cursor=seq.GetEnumerator();Assert.True(cursor.MoveNext());copied=cursor.Current;return 0;});
        f.Advisor.Hook=_=>throw new Exception("late callback poison");int checks=f.Input.Checks;
        Assert.Throws<ObjectDisposedException>(()=>view.GoverningType);Assert.Throws<ObjectDisposedException>(()=>view.IsList);Assert.Throws<ObjectDisposedException>(()=>view.AtomicItems);Assert.Throws<ObjectDisposedException>(()=>view.TryGetSingleAtomic(out _));Assert.Throws<ObjectDisposedException>(()=>seq.GetEnumerator());Assert.Throws<ObjectDisposedException>(()=>cursor.MoveNext());Assert.Throws<ObjectDisposedException>(()=>cursor.Current);Assert.Throws<ObjectDisposedException>(()=>cursor.Reset());Assert.Throws<ObjectDisposedException>(()=>copied.TypeEvidence.DeclaredType);Assert.Throws<ObjectDisposedException>(()=>copied.TypeEvidence.ActualType);Assert.Throws<ObjectDisposedException>(()=>copied.TypeEvidence.OriginalAssessment);Assert.Throws<ObjectDisposedException>(()=>raw.ReadTypedContainer(f.Snapshot,c));cursor.Dispose();cursor.Dispose();Assert.Equal(1,copied.Value.IntegerValue);Assert.Equal(checks,f.Input.Checks);
    }
    /// <summary>TC08 cursor positioning, separate enumeration, singleton list fast path and disposal are explicit.</summary>
    [Fact] public void TC08_CursorProtocol()
    {
        using var f=new Fixture();var c=f.Value(1,true);f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;Assert.True(v.TryGetSingleAtomic(out _));Assert.True(v.IsList);using var a=v.AtomicItems.GetEnumerator();using var b=v.AtomicItems.GetEnumerator();Assert.Throws<InvalidOperationException>(()=>a.Current);Assert.True(a.MoveNext());Assert.Throws<InvalidOperationException>(()=>b.Current);Assert.True(b.MoveNext());Assert.Equal(a.Current.Value,b.Current.Value);Assert.False(a.MoveNext());Assert.False(a.MoveNext());Assert.Throws<InvalidOperationException>(()=>a.Current);Assert.Throws<NotSupportedException>(()=>b.Reset());b.Dispose();Assert.Throws<ObjectDisposedException>(()=>b.Current);Assert.Throws<ObjectDisposedException>(()=>b.MoveNext());return 0;});
    }
    /// <summary>TC09 complete views revalidate current container, governing/member types and origin ownership.</summary>
    /// <param name="kind">Revoked ownership.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void TC09_LiveOwnership(int kind)
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;v.TryGetSingleAtomic(out var cell);if(kind==0)f.Advisor.Values.Clear();else if(kind==1)((Authority)f.Assessor.Selected).Types.Remove(f.Governing);else if(kind==2)((Authority)f.Assessor.Selected).Types.Remove(f.Actual);else f.Advisor.Origins.Clear();Assert.Throws<ArgumentException>(()=>v.IsList);Assert.Throws<ArgumentException>(()=>cell.TypeEvidence.ActualType);return 0;});
    }
    /// <summary>TC10 full reads enforce limits before extra Current and preserve valid retry capacity.</summary>
    [Fact] public void TC10_AtomCapacity()
    {
        using var f=new Fixture(cache:1,atoms:2);var c=f.Value(3,true);c.ForbiddenCurrent=2;f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(2,c.Currents);Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);Assert.True(op.ReadTypedContainer(f.Snapshot,f.Value()).Value.TryGetSingleAtomic(out _));return 0;});
    }
    /// <summary>TC11 delivered adapters cannot attest provider ownership on a round trip.</summary>
    [Fact] public void TC11_DeliveryIsNotIssuer()
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(f.Snapshot,v));Assert.Throws<ArgumentException>(()=>op.ReadTypedValue(f.Snapshot,v));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>TC12 source getters are not replayed by borrowed metadata access, but each new read is fresh.</summary>
    [Fact] public void TC12_NoGetterReplay()
    {
        using var f=new Fixture();var c=f.Value(2,true);f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;int reads=c.Reads.Count;c.Fail="governing";Assert.True(v.IsList);Assert.Equal(2,v.AtomicItems.Count());Assert.Same(f.Governing,v.GoverningType);Assert.Equal(reads,c.Reads.Count);Assert.Throws<InvalidOperationException>(()=>op.ReadTypedContainer(f.Snapshot,c));return 0;});
    }
    /// <summary>TC13 foreign snapshot/container refusal precedes source getters.</summary>
    [Fact] public void TC13_ForeignBeforeGetters()
    {
        using var f=new Fixture();using var g=new Fixture();var c=f.Value();c.Fail="governing";g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(g.Snapshot,c));return 0;});Assert.Empty(c.Reads);
    }
    /// <summary>TC14 cancellation during admission prevents delivery without registry mutation.</summary>
    [Fact] public void TC14_Cancellation()
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();var c=f.Value();f.Advisor.Hook=s=>{if(s=="value")cancel.Cancel();};Assert.Throws<OperationCanceledException>(()=>f.Run(op=>op.ReadTypedContainer(f.Snapshot,c),cancel.Token));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);
    }
    /// <summary>TC15 immutable copied metadata is not replaced by provider getter mutation.</summary>
    [Fact] public void TC15_MetadataSnapshot()
    {
        using var f=new Fixture();var c=f.Value();f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;c.Governing=new Handle();c.Items=[];Assert.Same(f.Governing,v.GoverningType);Assert.True(v.TryGetSingleAtomic(out var item));Assert.Equal(1,item.Value.IntegerValue);Assert.Same(f.Actual,item.TypeEvidence.ActualType);return 0;});
    }
    /// <summary>TC16 original provider failure identity survives read refusal.</summary>
    [Fact] public void TC16_FailureIdentity()
    {
        using var f=new Fixture();var c=f.Value();var failure=new AggregateException(new Exception("provider"));c.OnRead=s=>{if(s=="governing")throw failure;};Assert.Same(failure,Assert.Throws<AggregateException>(()=>f.Run(op=>op.ReadTypedContainer(f.Snapshot,c))));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);
    }
    /// <summary>TC17 records caller materialization/warm getter cost without adopting a performance budget.</summary>
    [Fact] public void TC17_Cost()
    {
        using var f=new Fixture();var c=f.Value(3,true);c.Record=false;f.Run(op=>{_ = op.ReadTypedContainer(f.Snapshot,c).Value;long before=GC.GetAllocatedBytesForCurrentThread();long start=System.Diagnostics.Stopwatch.GetTimestamp();var v=op.ReadTypedContainer(f.Snapshot,c).Value;long coldTicks=System.Diagnostics.Stopwatch.GetTimestamp()-start;long cold=GC.GetAllocatedBytesForCurrentThread()-before;before=GC.GetAllocatedBytesForCurrentThread();start=System.Diagnostics.Stopwatch.GetTimestamp();for(int i=0;i<8;i++){_ = v.GoverningType;_ = v.IsList;Assert.Equal(3,v.AtomicItems.Count());}long warmTicks=System.Diagnostics.Stopwatch.GetTimestamp()-start;long warm=GC.GetAllocatedBytesForCurrentThread()-before;output.WriteLine($"Complete container cost: cold caller bytes {cold}; cold ticks {coldTicks}; warm reads 8; warm caller bytes {warm}; warm ticks {warmTicks}; frequency {System.Diagnostics.Stopwatch.Frequency}");Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }

    /// <summary>TC18 complete reads enforce captured digit and scale limits without reservations.</summary>
    /// <param name="scale">Select scale rather than digit excess.</param>
    [Theory][InlineData(false)][InlineData(true)] public void TC18_NumericBounds(bool scale)
    {
        using var f=new Fixture(cache:1);var c=f.Value();var value=scale?XdmValue.FromDecimal(new XdmExactDecimal(1,2049,4096,4096)):XdmValue.FromInteger(BigInteger.Pow(10,2049));c.Items=[new(value,new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>TC19 revocation during a live raw callback denies all later view use before provider callbacks.</summary>
    /// <param name="kind">Snapshot, session or cancellation revocation.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void TC19_LiveRevocation(int kind)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();var c=f.Value();var error=Record.Exception(()=>f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;if(kind==0)f.Snapshot.Dispose();else if(kind==1)f.Session.Dispose();else cancel.Cancel();int checks=f.Input.Checks;f.Advisor.Hook=_=>throw new Exception("revoked callback poison");if(kind==2)Assert.Throws<OperationCanceledException>(()=>v.IsList);else Assert.Throws<ObjectDisposedException>(()=>v.IsList);Assert.Equal(checks,f.Input.Checks);return 0;},cancel.Token));Assert.Null(error);
    }
    /// <summary>TC20 blocked getter work retains actual source resources through logical close until raw drain.</summary>
    /// <returns>Completion after resource release.</returns>
    [Fact] public async Task TC20_ActualBlockedTail()
    {
        var path=Path.Combine(Path.GetTempPath(),"bosak-container-"+Guid.NewGuid().ToString("N")+".tmp");using var file=new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None,4096,FileOptions.DeleteOnClose);using var f=new Fixture();f.Input.Resource=file;var c=f.Value();var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=Task.Run(()=>f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;f.Advisor.Hook=s=>{if(s=="value"){entered.TrySetResult();release.Task.GetAwaiter().GetResult();}};return v.IsList;}));
        try{await entered.Task.WaitAsync(TimeSpan.FromSeconds(20));f.Session.Dispose();Assert.False(file.SafeFileHandle.IsClosed);Assert.Equal(0,f.Input.Releases);}finally{release.TrySetResult();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await raw);await f.Session.DrainAsync();Assert.True(file.SafeFileHandle.IsClosed);Assert.Equal(1,f.Input.Releases);
    }
    /// <summary>TC21 a fresh raw operation cannot reactivate a prior complete view.</summary>
    [Fact] public void TC21_IndependentRawViews()
    {
        using var f=new Fixture();var c=f.Value();var old=f.Run(op=>op.ReadTypedContainer(f.Snapshot,c).Value);f.Run(op=>{var fresh=op.ReadTypedContainer(f.Snapshot,c).Value;int checks=f.Input.Checks;Assert.Throws<ObjectDisposedException>(()=>old.GoverningType);Assert.Equal(checks,f.Input.Checks);Assert.True(fresh.TryGetSingleAtomic(out _));return 0;});Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);
    }
    /// <summary>TC22 acquisition, enumeration and cleanup failures retain the primary error and retry safety.</summary>
    /// <param name="stage">Failing provider cursor stage.</param>
    [Theory][InlineData("enumerator")][InlineData("current")][InlineData("dispose")] public void TC22_ProviderCursorFailure(string stage)
    {
        using var f=new Fixture(cache:1);var c=f.Value(2,true);c.Fail=stage;f.Run(op=>{Assert.Throws<InvalidOperationException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);c.Fail=null;Assert.Equal(2,op.ReadTypedContainer(f.Snapshot,c).Value.AtomicItems.Count());return 0;});
    }
    /// <summary>TC23 numeric profiles refuse complete transport without reading container content.</summary>
    [Fact] public void TC23_OtherProfileUnsupported()
    {
        using var f=new Fixture();var c=f.Value();c.Fail="governing";using var numeric=Session.CompileXPath("$value + 0",new SchemaQueryLimits(4,8,8,2048,2048),assessor:f.Assessor).Value;using var snapshot=numeric.OpenSnapshot(f.Input,f.Input,f.Origin).Value;numeric.Run([snapshot],op=>{Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedContainer(snapshot,c).Kind);return 0;});Assert.Empty(c.Reads);
    }

    internal static XdmValue PrimitiveCase(int kind)=>kind switch
    {
        0=>XdmValue.FromString("urn:payload","anyURI","urn:wide:string"),
        1=>XdmValue.FromInteger(42,"integer","urn:wide:integer"),
        2=>XdmValue.FromDecimal(new XdmExactDecimal(12300,4,2048,2048),"decimal","urn:wide:decimal"),
        3=>XdmValue.FromBoolean(true,"boolean","urn:wide:boolean"),
        4=>XdmValue.FromDouble(1.25d,"double","urn:wide:double"),
        5=>XdmValue.FromFloat(1.5f,"float","urn:wide:float"),
        6=>TemporalCase(0,10000),7=>TemporalCase(1,0),8=>TemporalCase(2,10000),
        9=>XdmValue.FromDuration("P01Y02M","yearMonthDuration","urn:wide:duration"),
        10=>XdmValue.FromQName(new XsQName("local","urn:wide","p"),"NOTATION","urn:wide:qname"),
        _=>throw new ArgumentOutOfRangeException(nameof(kind))
    };
    internal static XdmValue FloatCase(int example)
    {
        double value=(example%4) switch{0=>BitConverter.Int64BitsToDouble(unchecked((long)0x7ff800000000002aUL)),1=>double.PositiveInfinity,2=>double.NegativeInfinity,_=>BitConverter.Int64BitsToDouble(long.MinValue)};
        return example<4?XdmValue.FromDouble(value,"double","urn:wide:float-edge"):XdmValue.FromFloat((float)value,"float","urn:wide:float-edge");
    }
    internal static XdmValue QNameCase(int example)=>example switch
    {
        0=>XdmValue.FromQName(new("local","urn:n","p")),1=>XdmValue.FromQName(new("local","urn:n","other")),
        2=>XdmValue.FromQName(new("local","","")),_=>XdmValue.FromQName(new("local","urn:n","p"),"NOTATION","urn:user:q")
    };
    internal static XdmValue BadQName(int example)=>XdmValue.FromQName(example switch
    {0=>new XsQName(null!,"urn:n","p"),1=>new XsQName("","urn:n","p"),2=>new XsQName("local",null!,"p"),_=>new XsQName("local","urn:n",null!)});
    internal static XdmValue TemporalCase(int kind,long year)
    {
        bool wrapper=year!=0;var date=new XPathDateTime(year,2,3,4,5,6,789,150,!wrapper);
        return kind switch{0=>XdmValue.FromDate(date,wrapper,"date","urn:wide:date"),1=>XdmValue.FromTime(date,wrapper,"time","urn:wide:time"),_=>XdmValue.FromDateTime(date,wrapper,year==0?"dateTimeStamp":"gYear","urn:wide:dateTime")};
    }
    internal static void AssertPrimitive(XdmValue expected,XdmValue actual)
    {
        Assert.Equal(expected.Kind,actual.Kind);Assert.Equal(expected.SchemaTypeName,actual.SchemaTypeName);Assert.Equal(expected.UserSchemaTypeName,actual.UserSchemaTypeName);Assert.True(expected.SchemaTransportMatches(actual));Assert.Null(XdmValue.SchemaTransportMarker(actual));
        if(expected.Kind is XdmValueKind.Double or XdmValueKind.Float)Assert.Equal(BitConverter.DoubleToInt64Bits(expected.DoubleValue),BitConverter.DoubleToInt64Bits(actual.DoubleValue));
        if(expected.Kind==XdmValueKind.QName){Assert.Equal(expected.QNameValue.LocalName,actual.QNameValue.LocalName);Assert.Equal(expected.QNameValue.NamespaceUri,actual.QNameValue.NamespaceUri);Assert.Equal(expected.QNameValue.Prefix,actual.QNameValue.Prefix);}
        if(expected.Kind==XdmValueKind.Duration)Assert.Equal(expected.DurationValue,actual.DurationValue);
        if(expected.Kind==XdmValueKind.String)Assert.Equal(expected.StringValue,actual.StringValue);
        if(expected.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime)
        {
            var a=expected.Kind switch{XdmValueKind.Date=>expected.DateXPathValue,XdmValueKind.Time=>expected.TimeXPathValue,_=>expected.DateTimeXPathValue};var b=actual.Kind switch{XdmValueKind.Date=>actual.DateXPathValue,XdmValueKind.Time=>actual.TimeXPathValue,_=>actual.DateTimeXPathValue};
            Assert.Equal(a.Year,b.Year);Assert.Equal(a.Month,b.Month);Assert.Equal(a.Day,b.Day);Assert.Equal(a.Hour,b.Hour);Assert.Equal(a.Minute,b.Minute);Assert.Equal(a.Second,b.Second);Assert.Equal(a.Millisecond,b.Millisecond);Assert.Equal(a.TimezoneOffsetMinutes,b.TimezoneOffsetMinutes);Assert.Equal(a.HasTimezone,b.HasTimezone);Assert.Equal(expected.HasTimezone,actual.HasTimezone);
        }
    }
    private static void WideRead(Fixture f,XdmValue plain)
    {
        var c=f.Value();c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;Assert.True(v.TryGetSingleAtomic(out var item));AssertPrimitive(plain,item.Value);Assert.Same(f.Actual,item.TypeEvidence.ActualType);Assert.Same(f.Governing,item.TypeEvidence.DeclaredType);Assert.Same(f.Origin,item.TypeEvidence.OriginalAssessment);Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW01 all eleven public factory kinds preserve their captured ordinary representation.</summary>
    /// <param name="kind">Public factory selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)] public void PW01_Factories(int kind)
    {using var f=new Fixture();WideRead(f,PrimitiveCase(kind));}
    /// <summary>PW02 floating NaN, infinities and negative zero preserve captured stored bits.</summary>
    /// <param name="example">Floating representation selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)] public void PW02_FloatBits(int example)
    {using var f=new Fixture();WideRead(f,FloatCase(example));}
    /// <summary>PW03 QName prefixes and NOTATION/user tags remain exact rather than expanded-name-only equality.</summary>
    /// <param name="example">QName carrier selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void PW03_QNameFields(int example)
    {using var f=new Fixture();WideRead(f,QNameCase(example));}
    /// <summary>PW04 malformed QName field structure refuses the complete batch without scalar slots.</summary>
    /// <param name="example">Missing QName field.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void PW04_MalformedQName(int example)
    {
        using var f=new Fixture();var c=f.Value(2,true);c.Items=[new(PrimitiveCase(4),new Provenance(c,f.Governing,f.Actual,f.Origin)),new(BadQName(example),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW05 extended years, all fields and independent timezone flags survive without conversion.</summary>
    /// <param name="kind">Temporal kind.</param><param name="year">Supplied extended year.</param>
    [Theory][InlineData(0,0L)][InlineData(0,10000L)][InlineData(1,0L)][InlineData(1,10000L)][InlineData(2,0L)][InlineData(2,10000L)] public void PW05_Temporal(int kind,long year)
    {using var f=new Fixture();WideRead(f,TemporalCase(kind,year));}
    /// <summary>PW06 lexical durations are transported without canonicalization or parsing.</summary>
    /// <param name="example">Lexical duration selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void PW06_Duration(int example)
    {using var f=new Fixture();WideRead(f,XdmValue.FromDuration(example switch{0=>"P02D",1=>"P01Y02M",_=>"PT01.230S"},example==1?"yearMonthDuration":"dayTimeDuration","urn:user:d"));}
    /// <summary>PW07 null duration lexical payloads are structural errors after a valid wider prefix.</summary>
    [Fact] public void PW07_NullDuration()
    {
        using var f=new Fixture();var c=f.Value(2,true);c.Items=[new(PrimitiveCase(4),new Provenance(c,f.Governing,f.Actual,f.Origin)),new(XdmValue.FromDuration(null!),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW08 a complete eleven-kind union list retains ordered source type metadata.</summary>
    [Fact] public void PW08_MixedEleven()
    {
        using var f=new Fixture(atoms:16,cache:1);var c=f.Value(11,true);var types=Enumerable.Range(0,11).Select(_=>new Handle()).ToArray();((Authority)f.Assessor.Selected).Types.UnionWith(types);c.Items=Enumerable.Range(0,11).Select(i=>new SchemaAtomicItem(PrimitiveCase(i),new Provenance(c,f.Governing,types[i],f.Origin))).ToArray();f.Run(op=>{var v=op.ReadTypedContainer(f.Snapshot,c).Value;Assert.True(v.IsList);Assert.Same(f.Governing,v.GoverningType);var cells=v.AtomicItems.ToArray();Assert.Equal(11,cells.Length);for(int i=0;i<11;i++){AssertPrimitive(PrimitiveCase(i),cells[i].Value);Assert.Same(types[i],cells[i].TypeEvidence.ActualType);}Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW09 URI/binary lexical strings remain String-kind carriers rather than invented enum factories.</summary>
    /// <param name="annotation">Existing String annotation.</param>
    [Theory][InlineData("anyURI")][InlineData("hexBinary")][InlineData("base64Binary")] public void PW09_AnnotatedString(string annotation)
    {using var f=new Fixture();WideRead(f,XdmValue.FromString("AA==",annotation,"urn:user:lexical"));}
    /// <summary>PW10 copied wider plain values survive raw expiry while all evidence getters revoke.</summary>
    /// <param name="kind">Additional primitive family.</param>
    [Theory][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)] public void PW10_Escape(int kind)
    {
        using var f=new Fixture();var c=f.Value();var plain=PrimitiveCase(kind);c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];var cell=f.Run(op=>{op.ReadTypedContainer(f.Snapshot,c).Value.TryGetSingleAtomic(out var value);return value;});int checks=f.Input.Checks;AssertPrimitive(plain,cell.Value);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.DeclaredType);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.ActualType);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.OriginalAssessment);Assert.Equal(checks,f.Input.Checks);
    }
    /// <summary>PW11 wider list prefixes do not bypass atom limits or consume scalar capacity.</summary>
    [Fact] public void PW11_MixedLimits()
    {
        using var f=new Fixture(atoms:2,cache:1);var c=f.Value(3,true);c.Items[0]=new(PrimitiveCase(10),new Provenance(c,f.Governing,f.Actual,f.Origin));c.ForbiddenCurrent=2;f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedContainer(f.Snapshot,c));Assert.Equal(2,c.Currents);Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW12 wider materialization cost is observed without accepting retained-output or native budgets.</summary>
    [Fact] public void PW12_Cost()
    {
        using var f=new Fixture();var c=f.Value(2,true);c.Record=false;c.Items=[new(PrimitiveCase(8),new Provenance(c,f.Governing,f.Actual,f.Origin)),new(PrimitiveCase(10),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{long before=GC.GetAllocatedBytesForCurrentThread();var view=op.ReadTypedContainer(f.Snapshot,c).Value;long cold=GC.GetAllocatedBytesForCurrentThread()-before;_ = view.AtomicItems.Count();before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<2;i++){_ = view.IsList;_ = view.AtomicItems.Count();}long warm=GC.GetAllocatedBytesForCurrentThread()-before;output.WriteLine($"Ordinary wider cost: cold caller bytes {cold}; warm reads 2; warm caller bytes {warm}");return 0;});
    }
}
