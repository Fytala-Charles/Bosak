// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Verifies exact public advisor binding, input mapping and guarded typed admission.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; public-contract adapter controls (1.56.0)                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
using Authority=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Authority;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Origin=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Origin;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies the internal adapter against actual public contracts and engine-created admissions.</summary>
/// <param name="output">Records local adapter warm-read costs.</param>
public sealed class SchemaAdvisorAdapterTests(ITestOutputHelper output)
{
    private sealed class Source:ISchemaSourceBinding { }
    private sealed class Stamp:ISchemaSnapshotBinding { }
    private sealed class Input:ISchemaAssessmentInput,IQueryOperationLifetime
    {
        internal bool Available=true;internal int Roots,Releases,Checks;internal Stream? Resource;
        private readonly Source _source=new();private readonly Stamp _stamp=new();
        IXdmNode ISchemaAssessmentInput.Root{get{Roots++;throw new Exception("root poison");}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_stamp;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)=>false;
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){Checks++;token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){token.ThrowIfCancellationRequested();return new Lease(this);}
        private sealed class Lease(Input owner):IQueryOperationLease
        {
            private bool _closed;
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(_closed,this);}
            /// <inheritdoc/>
            public void Dispose(){if(_closed)return;_closed=true;owner.Releases++;owner.Resource?.Dispose();}
        }
    }
    private sealed class Outcome<T>(SchemaResultKind kind,T value):SchemaResult<T>
    {
        internal Action<string>? OnRead;internal int ValueReads;
        /// <inheritdoc/>
        public override SchemaResultKind Kind{get{OnRead?.Invoke("kind");return kind;}}
        /// <inheritdoc/>
        public override T Value{get{ValueReads++;OnRead?.Invoke("value");if(kind!=SchemaResultKind.Supported)throw new Exception("unsupported value poison");return value;}}
        /// <inheritdoc/>
        public override string? DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    private sealed class Assessor:ISchemaAssessor,IDisposable
    {
        internal ISchemaAuthority Selected=new Authority();internal Advisor? Advisor;internal IQuerySemanticEnvironment? Seen;
        internal Func<SchemaResult<ISchemaAssessmentAdvisor>>? Outcome;internal Action<string>? Hook;internal int Disposals,Binds;
        ISchemaAuthority ISchemaAssessor.Authority{get{Hook?.Invoke("authority");return Selected;}}
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("descriptive language poison");
        string ISchemaAssessor.AssessmentProfile=>throw new Exception("descriptive profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment environment,CancellationToken token)
        {Binds++;Seen=environment;Hook?.Invoke("bind");if(Advisor is not null)Advisor.Environment=environment;return Outcome is null?new Outcome<ISchemaAssessmentAdvisor>(SchemaResultKind.Supported,Advisor!):Outcome();}
        /// <inheritdoc/>
        public void Dispose()=>Disposals++;
    }
    private sealed class Advisor(Assessor assessor):ISchemaAssessmentAdvisor,IDisposable
    {
        internal ISchemaAssessor Endpoint=assessor;internal IQuerySemanticEnvironment Environment=null!;internal bool Available=true;
        internal readonly Dictionary<ISchemaTypedValue,(ISchemaAssessmentInput Input,ISchemaAssessmentOrigin? Origin)> Values=new(ReferenceEqualityComparer.Instance);
        internal readonly HashSet<ISchemaAssessmentOrigin> Origins=new(ReferenceEqualityComparer.Instance);
        internal Action<string>? Hook;internal int Disposals,ValueChecks;internal ISchemaAssessmentInput? LastInput;
        ISchemaAssessor ISchemaAssessmentAdvisor.Assessor{get{Hook?.Invoke("assessor");return Endpoint;}}
        IQuerySemanticEnvironment ISchemaAssessmentAdvisor.QueryEnvironment{get{Hook?.Invoke("environment");return Environment;}}
        void ISchemaAssessmentAdvisor.ThrowIfUnavailable(CancellationToken token){Hook?.Invoke("availability");token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        bool ISchemaAssessmentAdvisor.OwnsAssessment(ISchemaAssessmentOrigin origin){Hook?.Invoke("origin");return Origins.Contains(origin);}
        bool ISchemaAssessmentAdvisor.OwnsTypedValue(ISchemaTypedValue value,ISchemaAssessmentInput input,ISchemaAssessmentOrigin? original)
        {Hook?.Invoke("value");ValueChecks++;LastInput=input;return Values.TryGetValue(value,out var pair)&&ReferenceEquals(pair.Input,input)&&ReferenceEquals(pair.Origin,original);}
        bool ISchemaAssessmentAdvisor.OwnsNodeFacts(ISchemaNodeFacts facts)=>throw new Exception("facts poison");
        bool ISchemaAssessmentAdvisor.OwnsNode(IXdmNode node,ISchemaNodeFacts facts,ISchemaAssessmentInput input,ISchemaAssessmentOrigin origin)=>throw new Exception("node poison");
        ISchemaAssessmentResult ISchemaAssessmentAdvisor.Assess(SchemaAssessmentRequest request,CancellationToken token)=>throw new Exception("assessment poison");
        SchemaResult<ISchemaTypedValue> ISchemaAssessmentAdvisor.ConstructSimpleValue(XdmValue input,ISchemaComponentHandle type,ISchemaAssessmentEpisode episode,CancellationToken token)=>throw new Exception("construction poison");
        /// <inheritdoc/>
        public void Dispose()=>Disposals++;
    }
    private sealed class Fixture:IDisposable
    {
        internal readonly Session Session=Compile();internal readonly Input Input=new();internal readonly Assessor Assessor=new();
        internal readonly Advisor Advisor;internal readonly Handle Governing=new(),Actual=new();internal readonly Origin Origin=new();
        internal readonly SchemaQuerySnapshot Snapshot;internal readonly SchemaAdvisorBinding Binding;internal readonly IQueryTypedContainerIssuer Issuer;
        internal Fixture()
        {
            Advisor=new(Assessor);Assessor.Advisor=Advisor;((Authority)Assessor.Selected).Types.UnionWith([Governing,Actual]);Advisor.Origins.Add(Origin);
            Binding=Session.BindAdvisorCandidate(Assessor)!;Snapshot=Session.OpenSnapshot(Input,Input).Value;Issuer=Session.CreateIssuerCandidate(Binding,Snapshot,Input);
        }
        internal Container Value(int count=1,bool list=false,bool original=true)
        {
            var c=new Container{Governing=Governing,List=list,Fast=count==1};c.Items=Enumerable.Range(0,count).Select(i=>new SchemaAtomicItem(XdmValue.FromInteger(i+1),new Provenance(c,Governing,Actual,original?Origin:null))).ToArray();Advisor.Values.Add(c,(Input,original?Origin:null));return c;
        }
        internal QueryRegisteredTypedContainer Read(SchemaQueryOperation op,Container value,bool original=true)=>op.ReadTypedCandidate(Snapshot,Issuer,Input,original?Origin:null,value);
        internal T Run<T>(Func<SchemaQueryOperation,T> work,CancellationToken token=default)=>Session.Run([Snapshot],work,token);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    private static Session Compile()=>Session.CompileXPath("true()",new SchemaQueryLimits(4,8,8,2048,2048)).Value;
    /// <summary>Real public environment/input projections feed list/scalar/empty metadata into the qualified registry.</summary>
    /// <param name="count">Number of atomic cells.</param>
    /// <param name="original">Whether original assessment is present.</param>
    [Theory][InlineData(0,false)][InlineData(1,false)][InlineData(1,true)][InlineData(3,true)]
    public void ActualEnvironmentAndInput(int count,bool original)
    {
        using var f=new Fixture();var c=f.Value(count,true,original);Assert.Same(f.Session.Environment,f.Assessor.Seen);Assert.Same(f.Assessor.Seen,f.Advisor.Environment);
        f.Run(op=>{var view=f.Read(op,c,original);Assert.True(view.IsList);Assert.Equal(count,view.Count);for(int i=0;i<count;i++){Assert.NotNull(XdmValue.SchemaTransportMarker(view.GetTransport(i)));Assert.Same(f.Actual,view.GetAtomic(i).ActualType);Assert.Same(original?f.Origin:null,view.GetAtomic(i).OriginalAssessment);}return 0;});
        Assert.Same(f.Input,f.Advisor.LastInput);Assert.Equal(count,f.Snapshot.Runtime.TypedScalarCount);Assert.Equal(0,f.Input.Roots);Assert.Null(f.Session.Advisor);Assert.Equal(0,f.Assessor.Disposals);Assert.Equal(0,f.Advisor.Disposals);
    }
    /// <summary>Unsupported never reads Value; malformed or incomplete outcomes refuse without implicit disposal.</summary>
    /// <param name="kind">Outcome selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public void BindingOutcomes(int kind)
    {
        using var session=Compile();var a=new Assessor();var advisor=new Advisor(a);a.Advisor=advisor;
        SchemaResultKind category=kind switch{0=>SchemaResultKind.Unsupported,1=>SchemaResultKind.NotFound,2=>SchemaResultKind.Invalid,3=>(SchemaResultKind)99,_=>SchemaResultKind.Supported};var outcome=new Outcome<ISchemaAssessmentAdvisor>(category,null!);a.Outcome=()=>outcome;
        if(kind==0){Assert.Null(session.BindAdvisorCandidate(a));Assert.Equal(0,outcome.ValueReads);}else Assert.Throws<ArgumentException>(()=>session.BindAdvisorCandidate(a));Assert.Equal(0,a.Disposals);Assert.Equal(0,advisor.Disposals);
    }
    /// <summary>Foreign or changed bound endpoints and authority cannot publish an issuer.</summary>
    /// <param name="change">Dependency changed after a valid binding.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void ChangedBindingRefuses(int change)
    {
        using var f=new Fixture();using var other=Compile();var c=f.Value();int reads=c.Reads.Count;
        if(change==0)f.Advisor.Endpoint=new Assessor();else if(change==1)f.Advisor.Environment=other.Environment;else f.Assessor.Selected=new Authority();
        f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op,c));return 0;});Assert.Equal(reads,c.Reads.Count);Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);
    }
    /// <summary>Cancellation or close during Bind/result/endpoint callbacks prevents publication.</summary>
    /// <param name="stage">Callback stage.</param>
    [Theory][InlineData("bind")][InlineData("kind")][InlineData("value")][InlineData("environment")]
    public void CancellationDuringBinding(string stage)
    {
        using var session=Compile();using var cancel=new CancellationTokenSource();var a=new Assessor();var advisor=new Advisor(a);a.Advisor=advisor;
        a.Hook=s=>{if(s==stage)cancel.Cancel();};advisor.Hook=s=>{if(s==stage)cancel.Cancel();};var outcome=new Outcome<ISchemaAssessmentAdvisor>(SchemaResultKind.Supported,advisor);outcome.OnRead=s=>{if(s==stage)cancel.Cancel();};a.Outcome=()=>outcome;
        Assert.Throws<OperationCanceledException>(()=>session.BindAdvisorCandidate(a,cancel.Token));Assert.Equal(0,a.Disposals);Assert.Equal(0,advisor.Disposals);
    }
    /// <summary>A session closed from a binding callback refuses the result and never disposes the borrowed endpoint.</summary>
    [Fact] public void CloseDuringBinding()
    {
        using var session=Compile();var a=new Assessor();a.Advisor=new(a);a.Hook=s=>{if(s=="bind")session.Dispose();};Assert.Throws<ObjectDisposedException>(()=>session.BindAdvisorCandidate(a));Assert.Equal(0,a.Advisor.Disposals);
    }
    /// <summary>Input mapping is authenticated by the engine table before any advisor callback.</summary>
    [Fact] public void WrongInputOrSessionRefusesBeforeProvider()
    {
        using var f=new Fixture();using var g=new Fixture();f.Advisor.Hook=_=>throw new Exception("provider poison");
        Assert.Throws<ArgumentException>(()=>f.Session.CreateIssuerCandidate(f.Binding,f.Snapshot,g.Input));Assert.Throws<ArgumentException>(()=>g.Session.CreateIssuerCandidate(f.Binding,g.Snapshot,g.Input));
        g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedCandidate(f.Snapshot,f.Issuer,f.Input,f.Origin,f.Value()));return 0;});
    }
    /// <summary>Foreign or revoked container and origin issuance refuse before typed metadata getters.</summary>
    /// <param name="origin">Whether origin membership rather than container issuance is revoked.</param>
    [Theory][InlineData(false)][InlineData(true)] public void IssuanceRefusesBeforeGetters(bool origin)
    {
        using var f=new Fixture();var c=f.Value();if(origin)f.Advisor.Origins.Clear();else f.Advisor.Values.Clear();f.Run(op=>{Assert.Throws<ArgumentException>(()=>f.Read(op,c));return 0;});Assert.Empty(c.Reads);
    }
    /// <summary>Warm reads use original public input and recheck endpoint resources without replaying payload getters.</summary>
    [Fact] public void WarmReuseAndResourceRevocation()
    {
        using var f=new Fixture();var c=f.Value(3,true);object? marker=null;f.Run(op=>{marker=XdmValue.SchemaTransportMarker(f.Read(op,c).GetTransport(0));return 0;});int reads=c.Reads.Count;c.Fail="governing";
        f.Run(op=>{Assert.Same(marker,XdmValue.SchemaTransportMarker(f.Read(op,c).GetTransport(0)));return 0;});Assert.Equal(reads,c.Reads.Count);f.Advisor.Available=false;f.Run(op=>{Assert.Throws<ObjectDisposedException>(()=>f.Read(op,c));return 0;});Assert.Equal(reads,c.Reads.Count);
    }
    /// <summary>Failure to consume a container publishes no entry and a valid retry succeeds.</summary>
    [Fact] public void ReadFailureAndRetry()
    {
        using var f=new Fixture();var c=f.Value(2,true);c.Fail="current";f.Run(op=>{Assert.Throws<InvalidOperationException>(()=>f.Read(op,c));Assert.Equal(0,f.Snapshot.Runtime.TypedScalarCount);c.Fail=null;Assert.Equal(2,f.Read(op,c).Count);return 0;});
    }
    /// <summary>Adapter reads retain source admission until raw completion after logical close.</summary>
    /// <returns>Completion of the session drain.</returns>
    [Fact] public async Task CloseDuringReadRetainsOwner()
    {
        using var f=new Fixture();var c=f.Value();f.Advisor.Hook=s=>{if(s=="value"){f.Session.Dispose();Assert.Equal(0,f.Input.Releases);}};f.Run(op=>{Assert.Throws<ObjectDisposedException>(()=>f.Read(op,c));return 0;});await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(1,f.Input.Releases);Assert.Equal(0,f.Advisor.Disposals);
    }
    /// <summary>The public activation guard still avoids Bind and public typed reads still refuse.</summary>
    [Fact] public void PublicGuardsRemainUnsupported()
    {
        var assessor=new Assessor{Hook=_=>throw new Exception("public guard poison")};Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("true()",new SchemaQueryLimits(4,8,8,2048,2048),assessor:assessor).Kind);Assert.Equal(0,assessor.Binds);
        using var f=new Fixture();var c=f.Value();f.Run(op=>{Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(f.Snapshot,c).Kind);return 0;});Assert.Empty(c.Reads);
    }
    /// <summary>Measures current endpoint/input checks on a warm three-cell result without payload getter replay.</summary>
    [Fact] public void WarmAllocationObservation()
    {
        using var f=new Fixture();var c=f.Value(3,true);f.Run(op=>{for(int i=0;i<100;i++)f.Read(op,c);int reads=c.Reads.Count;long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)f.Read(op,c);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;output.WriteLine($"Advisor adapter warm-list-three: {bytes} bytes/read");Assert.InRange(bytes,1,32768);Assert.Equal(reads,c.Reads.Count);return 0;});
    }

    /// <summary>Null provider outcomes refuse rather than being treated as Unsupported or an implicit advisor.</summary>
    [Fact] public void NullBindingOutcomeRefuses()
    {
        using var session=Compile();var a=new Assessor{Outcome=()=>null!};Assert.Throws<ArgumentException>(()=>session.BindAdvisorCandidate(a));Assert.Equal(0,a.Disposals);
    }
    /// <summary>A Supported result with the wrong endpoint/environment or changed authority refuses at binding.</summary>
    /// <param name="change">Endpoint dependency changed while binding.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void ForgedBindingRefuses(int change)
    {
        using var session=Compile();using var other=Compile();var a=new Assessor();var advisor=new Advisor(a);a.Advisor=advisor;
        if(change==0)a.Hook=stage=>{if(stage=="bind")advisor.Endpoint=new Assessor();};
        else if(change==1)advisor.Hook=stage=>{if(stage=="environment")advisor.Environment=other.Environment;};
        else a.Hook=stage=>{if(stage=="bind")a.Selected=new Authority();};
        Assert.Throws<ArgumentException>(()=>session.BindAdvisorCandidate(a));Assert.Equal(0,advisor.Disposals);
    }
    /// <summary>Provider binding failures propagate and never invoke borrowed endpoint disposal.</summary>
    /// <param name="stage">Getter stage that fails.</param>
    [Theory][InlineData("availability")][InlineData("assessor")][InlineData("environment")][InlineData("authority")]
    public void GetterFailuresPreserveBorrowedOwnership(string stage)
    {
        using var session=Compile();var a=new Assessor();var advisor=new Advisor(a);a.Advisor=advisor;a.Hook=s=>{if(s==stage)throw new InvalidOperationException(stage);};advisor.Hook=s=>{if(s==stage)throw new InvalidOperationException(stage);};
        Assert.Equal(stage,Assert.Throws<InvalidOperationException>(()=>session.BindAdvisorCandidate(a)).Message);Assert.Equal(0,a.Disposals);Assert.Equal(0,advisor.Disposals);
    }
    /// <summary>Escaped public raw operations cannot call the candidate adapter or touch advisor state.</summary>
    [Fact] public void EscapedOperationRefusesBeforeProvider()
    {
        using var f=new Fixture();var c=f.Value();SchemaQueryOperation? escaped=null;f.Run(op=>{escaped=op;return 0;});f.Advisor.Hook=_=>throw new Exception("expired provider poison");Assert.Throws<ObjectDisposedException>(()=>f.Read(escaped!,c));Assert.Empty(c.Reads);
    }
    /// <summary>A real file remains owned through logical close and is released after adapter raw work finishes.</summary>
    /// <returns>Completion of the public session drain.</returns>
    [Fact] public async Task ActualFileLifetime()
    {
        string path=Path.Combine(Path.GetTempPath(),"bosak-advisor-"+Guid.NewGuid().ToString("N")+".tmp");
        using var file=new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None,4096,FileOptions.DeleteOnClose);using var f=new Fixture();f.Input.Resource=file;var c=f.Value();
        f.Run(op=>{Assert.NotNull(XdmValue.SchemaTransportMarker(f.Read(op,c).GetTransport(0)));f.Session.Dispose();Assert.False(file.SafeFileHandle.IsClosed);Assert.Equal(0,f.Input.Releases);Assert.Throws<ObjectDisposedException>(()=>f.Read(op,c));return 0;});await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.True(file.SafeFileHandle.IsClosed);Assert.Equal(1,f.Input.Releases);Assert.Equal(0,f.Assessor.Disposals);Assert.Equal(0,f.Advisor.Disposals);
    }
}
