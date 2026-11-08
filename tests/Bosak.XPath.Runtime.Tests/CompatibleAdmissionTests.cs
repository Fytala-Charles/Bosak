// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies authenticated private compatible proof admission and fresh guarded reproof.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 05-11-2026     | Share paired fixtures with private compatible cache controls (1.69.0).                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3 | 05-11-2026     | Preserve private controls alongside public compatible route (1.70.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies actual Establish/endpoint transactions without public compatible data activation.</summary>
public sealed class CompatibleAdmissionTests
{
    private sealed class Outcome<T>(T value):SchemaResult<T>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind=>SchemaResultKind.Supported;
        /// <inheritdoc/>
        public override T Value=>value;
        /// <inheritdoc/>
        public override string? DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    internal sealed class Origin:ISchemaAssessmentOrigin
    {ISchemaSourceBinding ISchemaAssessmentOrigin.Source=>throw new Exception("origin source poison");}
    internal sealed class Input:ISchemaAssessmentInput,IQueryOperationLifetime,IDisposable
    {
        private sealed class Source:ISchemaSourceBinding;
        private sealed class Snapshot:ISchemaSnapshotBinding;
        private readonly Source _source=new();private readonly Snapshot _snapshot=new();
        internal bool Available=true;internal int Entries,Releases,Active;internal bool Closing;internal FileStream? Resource;internal Action? Hook;
        IXdmNode ISchemaAssessmentInput.Root=>throw new Exception("root poison");
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_snapshot;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)=>throw new Exception("node ownership poison");
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token){Hook?.Invoke();token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Available,this);}
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token){Entries++;Active++;return new Lease(this);}
        private sealed class Lease(Input owner):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!owner.Available,this);}
            /// <inheritdoc/>
            public void Dispose(){owner.Releases++;owner.Active--;if(owner.Closing&&owner.Active==0)owner.Resource?.Dispose();}
        }
        /// <inheritdoc/>
        public void Dispose(){Available=false;Closing=true;if(Active==0)Resource?.Dispose();}
    }
    internal sealed class Provider:ISchemaAssessor,IDisposable
    {
        internal readonly QueryTypedContainerTests.Authority Authority=new();internal ISchemaAuthority? Selected;
        internal ISchemaAssessmentOrigin Origin=new Origin();internal bool OriginOwned=true,Lifetime=true,Compatibility,Available=true,LeaseAvailable=true,NullLease,NullResult,NullView;
        internal int Entries,Releases,Active,Establishes,Binds;internal bool Closing;internal FileStream? Resource;
        internal SchemaCompatibilityDecision Decision=SchemaCompatibilityDecision.Compatible;internal Action<string>? Hook;internal Action? Release;
        internal ISchemaAssessor? Endpoint;internal IQuerySemanticEnvironment? EnvironmentOverride;
        internal Func<View,View>? ChangeView;internal View? LastView;internal ISchemaAssessmentAdvisor? LastSource,LastTarget;internal IQuerySemanticEnvironment? LastEnvironment;
        internal readonly List<string> Calls=[];
        internal bool TypedAllowed;internal ISchemaAssessmentInput? TypedInput;
        internal readonly HashSet<ISchemaTypedValue> Values=new(ReferenceEqualityComparer.Instance);
        internal void Hit(string stage){lock(Calls)Calls.Add(stage);Hook?.Invoke(stage);}
        ISchemaAuthority ISchemaAssessor.Authority{get{Hit("authority");return Selected??Authority;}}
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("language poison");
        string ISchemaAssessor.AssessmentProfile=>throw new Exception("profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment environment,CancellationToken token)
        {Binds++;return new Outcome<ISchemaAssessmentAdvisor>(Lifetime?(Compatibility?new CompatibleAdvisor(this,environment):new LifeAdvisor(this,environment)):new BareAdvisor(this,environment));}
        private class BareAdvisor(Provider owner,IQuerySemanticEnvironment environment):ISchemaAssessmentAdvisor
        {
            internal readonly Provider Owner=owner;internal readonly IQuerySemanticEnvironment Environment=environment;
            ISchemaAssessor ISchemaAssessmentAdvisor.Assessor{get{Owner.Hit("endpoint");return Owner.Endpoint??Owner;}}
            IQuerySemanticEnvironment ISchemaAssessmentAdvisor.QueryEnvironment{get{Owner.Hit("environment");return Owner.EnvironmentOverride??Environment;}}
            void ISchemaAssessmentAdvisor.ThrowIfUnavailable(CancellationToken token){Owner.Hit("available");token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!Owner.Available,this);}
            bool ISchemaAssessmentAdvisor.OwnsAssessment(ISchemaAssessmentOrigin origin){Owner.Hit("origin");return Owner.OriginOwned&&ReferenceEquals(origin,Owner.Origin);}
            bool ISchemaAssessmentAdvisor.OwnsNodeFacts(ISchemaNodeFacts facts)=>throw new Exception("facts poison");
            bool ISchemaAssessmentAdvisor.OwnsNode(IXdmNode node,ISchemaNodeFacts facts,ISchemaAssessmentInput input,ISchemaAssessmentOrigin origin)=>throw new Exception("tuple poison");
            bool ISchemaAssessmentAdvisor.OwnsTypedValue(ISchemaTypedValue value,ISchemaAssessmentInput input,ISchemaAssessmentOrigin? origin){if(!Owner.TypedAllowed)throw new Exception("typed value poison");Owner.Hit("typed-issuance");return Owner.Values.Contains(value)&&ReferenceEquals(input,Owner.TypedInput)&&ReferenceEquals(origin,Owner.Origin);}
            ISchemaAssessmentResult ISchemaAssessmentAdvisor.Assess(SchemaAssessmentRequest request,CancellationToken token)=>throw new Exception("reassessment poison");
            SchemaResult<ISchemaTypedValue> ISchemaAssessmentAdvisor.ConstructSimpleValue(XdmValue value,ISchemaComponentHandle type,ISchemaAssessmentEpisode episode,CancellationToken token)=>throw new Exception("construction poison");
        }
        private class LifeAdvisor:BareAdvisor,IQueryOperationLifetime
        {
            internal LifeAdvisor(Provider owner,IQuerySemanticEnvironment environment):base(owner,environment){}
            IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token)
            {
                Interlocked.Increment(ref Owner.Entries);Interlocked.Increment(ref Owner.Active);
                try{Owner.Hit("enter");if(Owner.NullLease){Owner.Leave(false);return null!;}return new Lease(Owner);}
                catch{Owner.Leave(false);throw;}
            }
            private sealed class Lease(Provider owner):IQueryOperationLease
            {
                /// <inheritdoc/>
                public void ThrowIfUnavailable(CancellationToken token){owner.Hit("lease");token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!owner.LeaseAvailable,this);}
                /// <inheritdoc/>
                public void Dispose(){owner.Leave(true);}
            }
        }
        private void Leave(bool release)
        {if(release){Interlocked.Increment(ref Releases);Hit("release");}if(Interlocked.Decrement(ref Active)==0&&Closing)Resource?.Dispose();if(release)Release?.Invoke();}
        private sealed class CompatibleAdvisor:LifeAdvisor,ISchemaCompatibilityAuthority
        {
            internal CompatibleAdvisor(Provider owner,IQuerySemanticEnvironment environment):base(owner,environment){}
            ISchemaCompatibilityResult ISchemaCompatibilityAuthority.Establish(ISchemaAssessmentAdvisor source,ISchemaAssessmentAdvisor target,IQuerySemanticEnvironment environment,CancellationToken token)
            {
                Interlocked.Increment(ref Owner.Establishes);Owner.LastSource=source;Owner.LastTarget=target;Owner.LastEnvironment=environment;Owner.Hit("establish");
                var view=new View(Owner,source,target,environment);if(Owner.ChangeView is not null)view=Owner.ChangeView(view);Owner.LastView=view;
                return Owner.NullResult?null!:new Proof(Owner,Owner.Decision,Owner.NullView?null:view);
            }
        }
        private sealed class Proof(Provider owner,SchemaCompatibilityDecision decision,View? view):ISchemaCompatibilityResult
        {
            SchemaCompatibilityDecision ISchemaCompatibilityResult.Decision{get{owner.Hit("decision");return decision;}}
            ISchemaCompatibilityView? ISchemaCompatibilityResult.View{get{owner.Hit("view");if(decision!=SchemaCompatibilityDecision.Compatible)throw new Exception("negative View poison");return view;}}
            ImmutableArray<SchemaAssessmentDiagnostic> ISchemaCompatibilityResult.Diagnostics=>throw new Exception("diagnostics poison");
        }
        /// <inheritdoc/>
        public void Dispose(){Available=false;Closing=true;if(Volatile.Read(ref Active)==0)Resource?.Dispose();}
    }
    internal sealed class View(Provider owner,ISchemaAssessmentAdvisor source,ISchemaAssessmentAdvisor target,IQuerySemanticEnvironment environment):ISchemaCompatibilityView
    {
        internal ISchemaAssessmentAdvisor Source=source,Target=target;internal IQuerySemanticEnvironment Environment=environment;
        ISchemaAssessmentAdvisor ISchemaCompatibilityView.Source{get{owner.Hit("view-source");return Source;}}
        ISchemaAssessmentAdvisor ISchemaCompatibilityView.Target{get{owner.Hit("view-target");return Target;}}
        IQuerySemanticEnvironment ISchemaCompatibilityView.ExecutionEnvironment{get{owner.Hit("view-environment");return Environment;}}
        SchemaResult<bool> ISchemaCompatibilityView.IsDerivedFrom(ISchemaComponentHandle actual,ISchemaComponentHandle expected,CancellationToken token)=>throw new Exception("derivation poison");
        SchemaResult<bool> ISchemaCompatibilityView.MatchesDeclaration(ISchemaNodeFacts facts,ISchemaComponentHandle expected,CancellationToken token)=>throw new Exception("declaration poison");
    }
    internal sealed class Fixture:IDisposable
    {
        internal readonly Provider SourceProvider,TargetProvider;internal readonly Input Input=new();internal readonly Session Source,Target;internal readonly SchemaQuerySnapshot Original;
        internal Fixture(bool original=true,Provider? source=null,Provider? target=null,SchemaQueryLimits? limits=null,CompileOptions? options=null,bool identity=false,bool publicIdentity=false)
        {
            SourceProvider=source??new();TargetProvider=target??new Provider{Compatibility=true};var l=limits??new SchemaQueryLimits(1,4,4,128,128);
            Source=Session.CompileXPath("$value",l,assessor:SourceProvider).Value;Original=Source.OpenSnapshot(Input,Input,original?SourceProvider.Origin:null).Value;
            SourceProvider.TypedInput=Input;
            Target=(publicIdentity?Session.CompileXPath("$compatible",l,options,TargetProvider):identity?Session.CompileCompatibleIdentityCandidate("$compatible",l,TargetProvider,options):Session.CompileCompatibleAdmissionCandidate("$compatible",l,TargetProvider,options)).Value;SourceProvider.Calls.Clear();TargetProvider.Calls.Clear();
        }
        internal CompatibleAdmissionPlan.Result Open()=>Target.OpenCompatibleCandidate(Original);
        /// <inheritdoc/>
        public void Dispose(){Target.Dispose();Source.Dispose();Input.Dispose();SourceProvider.Dispose();TargetProvider.Dispose();}
    }
    /// <summary>AC01 exact captured endpoints from independent authorities produce one private neutral admission.</summary>
    [Fact] public void AC01_AuthenticatedAdmission()
    {
        using var f=new Fixture();var result=f.Open();Assert.Equal(SchemaCompatibilityDecision.Compatible,result.Decision);Assert.Null(result.DiagnosticCode);using var admitted=result.Value!;
        Assert.Same(f.Original.Runtime,admitted.Snapshot.RetainedSource);Assert.Null(admitted.Snapshot.Schema);Assert.Equal(0,admitted.Snapshot.TypedScalarCount);
        Assert.Same(f.Source.Advisor,f.TargetProvider.LastSource);Assert.Same(f.Target.Advisor,f.TargetProvider.LastTarget);Assert.Same(f.Target.Environment,f.TargetProvider.LastEnvironment);Assert.NotSame(f.SourceProvider.Authority,f.TargetProvider.Authority);
        Assert.Equal(1,f.SourceProvider.Entries);Assert.Equal(1,f.TargetProvider.Entries);Assert.Equal(1,f.SourceProvider.Releases);Assert.Equal(1,f.TargetProvider.Releases);
    }
    /// <summary>AC02 public selector and execution stay unavailable; internal factory exposes only distinct admission metadata.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task AC02_PublicBoundary()
    {
        using var f=new Fixture();var p=new Provider{Compatibility=true};using var publicSession=Session.CompileXPath("$compatible",new(1,4,4,128,128),assessor:p).Value;Assert.Equal(CompatibleAdmissionPlan.PublicProfile,publicSession.Environment.BehaviorProfile);Assert.Equal(1,p.Binds);
        Assert.Equal(CompatibleAdmissionPlan.Profile,f.Target.Environment.BehaviorProfile);Assert.Equal(CompatibleAdmissionPlan.Descriptor,f.Target.Environment.DescriptorFormatVersion);
        Assert.Throws<NotSupportedException>(()=>f.Target.Run([],op=>1));await Assert.ThrowsAsync<NotSupportedException>(()=>f.Target.RunAsync([],op=>Task.FromResult(1)));
        Assert.Equal(SchemaResultKind.Unsupported,f.Target.OpenSnapshot(f.Input,f.Input).Kind);Assert.Equal(0,f.TargetProvider.Entries);
    }
    /// <summary>AC03 target optional capabilities refuse before resource acquisition.</summary>
    /// <param name="mode">Missing target capability.</param>
    [Theory][InlineData("lifetime")][InlineData("authority")] public void AC03_TargetCapabilities(string mode)
    {using var p=new Provider{Lifetime=mode!="lifetime",Compatibility=mode!="authority"};var r=Session.CompileCompatibleAdmissionCandidate("$compatible",new(1,4,4,128,128),p);Assert.Equal(SchemaResultKind.Unsupported,r.Kind);Assert.Equal(0,p.Entries);}
    /// <summary>AC04 missing source lifetime/origin produces Unsupported without provider entry or hidden source claims.</summary>
    /// <param name="lifetime">Source supports a real optional lifetime.</param><param name="origin">Source has original assessment.</param>
    [Theory][InlineData(false,true)][InlineData(true,false)] public void AC04_SourceCapabilities(bool lifetime,bool origin)
    {using var f=new Fixture(origin,new Provider{Lifetime=lifetime});var r=f.Open();Assert.Equal(SchemaCompatibilityDecision.Unsupported,r.Decision);Assert.Null(r.Value);Assert.Equal(0,f.SourceProvider.Entries);Assert.Equal(0,f.TargetProvider.Entries);}
    /// <summary>AC05 negative proofs preserve all three decisions and never touch View/Diagnostics or retain quota.</summary>
    /// <param name="decision">Owned negative proof.</param>
    [Theory][InlineData(SchemaCompatibilityDecision.Incompatible)][InlineData(SchemaCompatibilityDecision.Unsupported)] public void AC05_NegativeDecisions(SchemaCompatibilityDecision decision)
    {using var f=new Fixture();f.TargetProvider.Decision=decision;var r=f.Open();Assert.Equal(decision,r.Decision);Assert.Null(r.Value);Assert.Contains(decision==SchemaCompatibilityDecision.Incompatible?"INCOMPATIBLE":"UNSUPPORTED",r.DiagnosticCode);Assert.DoesNotContain("view",f.TargetProvider.Calls);f.TargetProvider.Decision=SchemaCompatibilityDecision.Compatible;using var retry=f.Open().Value!;Assert.Equal(2,f.SourceProvider.Releases);Assert.Equal(2,f.TargetProvider.Releases);}
    /// <summary>AC06 null/unknown/Compatible-null outcomes are malformed and reserve no permanent target slot.</summary>
    /// <param name="mode">Malformed proof.</param>
    [Theory][InlineData("null")][InlineData("unknown")][InlineData("view")] public void AC06_MalformedProof(string mode)
    {using var f=new Fixture();f.TargetProvider.NullResult=mode=="null";f.TargetProvider.NullView=mode=="view";if(mode=="unknown")f.TargetProvider.Decision=(SchemaCompatibilityDecision)99;Assert.Throws<ArgumentException>(()=>f.Open());f.TargetProvider.NullResult=f.TargetProvider.NullView=false;f.TargetProvider.Decision=SchemaCompatibilityDecision.Compatible;using var retry=f.Open().Value!;}
    /// <summary>AC07 copied/spoofed endpoint/environment claims are not authenticated by matching fingerprints.</summary>
    /// <param name="mode">Wrong view endpoint.</param>
    [Theory][InlineData("source")][InlineData("target")][InlineData("environment")][InlineData("null-source")][InlineData("null-target")][InlineData("null-environment")]
    public void AC07_EndpointMismatch(string mode)
    {using var f=new Fixture();using var other=new Fixture();Assert.Equal(f.Target.Environment.StaticContextFingerprint,other.Target.Environment.StaticContextFingerprint);f.TargetProvider.ChangeView=v=>{switch(mode){case "source":v.Source=other.Source.Advisor!;break;case "target":v.Target=other.Target.Advisor!;break;case "environment":v.Environment=other.Target.Environment;break;case "null-source":v.Source=null!;break;case "null-target":v.Target=null!;break;default:v.Environment=null!;break;}return v;};Assert.Throws<ArgumentException>(()=>f.Open());}
    /// <summary>AC08 source binding/input/origin metadata is exact and immutable before proof use.</summary>
    /// <param name="mode">Wrong original metadata.</param>
    [Theory][InlineData("binding")][InlineData("input")][InlineData("origin")][InlineData("ownership")]
    public void AC08_OriginalMetadata(string mode)
    {
        using var f=new Fixture();using var other=new Fixture();var source=f.Original;
        if(mode=="binding"){source=new SchemaQuerySnapshot(f.Original.Runtime,f.Original.Binding,other.Original.OriginalBinding);source.TypedState=f.Original.TypedState;}
        else if(mode=="input")f.Original.TypedState=new(f.Original.TypedState!.Issuer,other.Input,f.SourceProvider.Origin);
        else if(mode=="origin")f.Original.TypedState=new(f.Original.TypedState!.Issuer,f.Input,other.SourceProvider.Origin);
        else f.SourceProvider.OriginOwned=false;
        Assert.Throws<ArgumentException>(()=>f.Target.OpenCompatibleCandidate(source));Assert.Equal(0,f.TargetProvider.Establishes);
    }
    /// <summary>AC09 disposed/null source and wrong consumer session cannot supply a live source grant.</summary>
    [Fact] public void AC09_LivenessAndOwnership()
    {using var f=new Fixture();Assert.Throws<ArgumentNullException>(()=>f.Target.OpenCompatibleCandidate(null!));f.Original.Dispose();Assert.Throws<ObjectDisposedException>(()=>f.Open());Assert.Equal(0,f.TargetProvider.Entries);}
    /// <summary>AC10 reproof is fresh for every internal raw operation, without retaining a view as snapshot authority.</summary>
    [Fact] public void AC10_FreshReproof()
    {using var f=new Fixture();using var admitted=f.Open().Value!;var first=f.TargetProvider.LastView;Assert.Equal(7,f.Target.RunCompatibleCandidate(admitted,_=>7));Assert.NotSame(first,f.TargetProvider.LastView);var second=f.TargetProvider.LastView;Assert.Equal(8,f.Target.RunCompatibleCandidate(admitted,_=>8));Assert.NotSame(second,f.TargetProvider.LastView);Assert.Equal(3,f.TargetProvider.Establishes);Assert.Equal(3,f.SourceProvider.Entries);Assert.Equal(3,f.SourceProvider.Releases);}
    /// <summary>AC11 later Incompatible/Unsupported proofs deny work rather than reuse an admission-time positive result.</summary>
    /// <param name="decision">Changed proof capability.</param>
    [Theory][InlineData(SchemaCompatibilityDecision.Incompatible)][InlineData(SchemaCompatibilityDecision.Unsupported)] public void AC11_NegativeReproof(SchemaCompatibilityDecision decision)
    {using var f=new Fixture();using var a=f.Open().Value!;f.TargetProvider.Decision=decision;bool called=false;Assert.Throws<NotSupportedException>(()=>f.Target.RunCompatibleCandidate(a,_=>{called=true;return 1;}));Assert.False(called);Assert.Equal(2,f.TargetProvider.Establishes);Assert.Equal(2,f.TargetProvider.Releases);}
    /// <summary>AC12 one target admission per input, quotas and disposal keep original host ownership unchanged.</summary>
    [Fact] public void AC12_QuotaAndNeutrality()
    {using var f=new Fixture();var first=f.Open().Value!;Assert.Throws<ArgumentException>(()=>f.Open());first.Dispose();first.Dispose();using var second=f.Open().Value!;Assert.Equal(1,f.Input.Entries);Assert.Equal(0,f.Input.Releases);Assert.Null(second.Snapshot.Schema);Assert.Equal(0,second.Snapshot.TypedScalarCount);}
    /// <summary>AC13 every endpoint/proof callback is guarded against source or target revocation before using its answer.</summary>
    /// <param name="stage">Provider callback to revoke.</param>
    [Theory][InlineData("enter")][InlineData("lease")][InlineData("available")][InlineData("endpoint")][InlineData("environment")][InlineData("authority")][InlineData("origin")]
    [InlineData("establish")][InlineData("decision")][InlineData("view")][InlineData("view-source")][InlineData("view-target")][InlineData("view-environment")]
    public void AC13_CallbackRevocation(string stage)
    {using var f=new Fixture();Action<string> revoke=s=>{if(s==stage)f.SourceProvider.Available=false;};f.SourceProvider.Hook=revoke;f.TargetProvider.Hook=revoke;Assert.Throws<ObjectDisposedException>(()=>f.Open());Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);Assert.Equal(f.TargetProvider.Entries,f.TargetProvider.Releases);}
    /// <summary>AC14 changed captured advisor endpoints/authorities/environments are malformed rather than new authority roots.</summary>
    /// <param name="stage">Changed identity.</param>
    [Theory][InlineData("source-endpoint")][InlineData("target-endpoint")][InlineData("source-authority")][InlineData("target-authority")][InlineData("source-environment")][InlineData("target-environment")]
    public void AC14_BindingChange(string stage)
    {using var f=new Fixture();using var other=new Fixture();f.TargetProvider.Hook=s=>{if(s!="establish")return;var p=stage.StartsWith("source")?f.SourceProvider:f.TargetProvider;if(stage.EndsWith("endpoint"))p.Endpoint=other.SourceProvider;else if(stage.EndsWith("authority"))p.Selected=other.SourceProvider.Authority;else p.EnvironmentOverride=other.Target.Environment;};Assert.Throws<ArgumentException>(()=>f.Open());}
    /// <summary>AC15 direct proof/work and both admission cleanup failures preserve original exception identities and order.</summary>
    [Fact] public void AC15_FailureOrdering()
    {using var f=new Fixture();var primary=new ApplicationException("proof");var target=new AggregateException(new ApplicationException("target cleanup"));var source=new ApplicationException("source cleanup");f.TargetProvider.Hook=s=>{if(s=="establish")throw primary;};f.TargetProvider.Release=()=>throw target;f.SourceProvider.Release=()=>throw source;var error=Assert.Throws<AggregateException>(()=>f.Open());Assert.Equal(new Exception[]{primary,target,source},error.InnerExceptions);Assert.Equal(1,f.SourceProvider.Releases);Assert.Equal(1,f.TargetProvider.Releases);}
    /// <summary>AC16 cleanup revocation prevents a positive admission from escaping after its raw provider scope has ended.</summary>
    /// <param name="target">Close target rather than source.</param>
    [Theory][InlineData(false)][InlineData(true)] public void AC16_CleanupRevocation(bool target)
    {using var f=new Fixture();f.TargetProvider.Release=target?f.Target.Dispose:f.Source.Dispose;Assert.Throws<ObjectDisposedException>(()=>f.Open());Assert.Equal(1,f.SourceProvider.Releases);Assert.Equal(1,f.TargetProvider.Releases);Assert.Equal(target?0:1,f.Input.Releases);}
    /// <summary>AC17 pre-publication fault exposes no admission and returns the target slot.</summary>
    [Fact] public void AC17_PublicationFailure()
    {using var f=new Fixture();Assert.Throws<InvalidOperationException>(()=>f.Target.OpenCompatibleCandidate(f.Original,publicationFault:0));using var retry=f.Open().Value!;Assert.Equal(2,f.TargetProvider.Releases);}
    /// <summary>AC18 retained original metadata cannot be replaced after admission or during provider entry.</summary>
    /// <param name="duringEntry">Mutate while opening rather than after admission.</param>
    [Theory][InlineData(false)][InlineData(true)] public void AC18_MetadataMutation(bool duringEntry)
    {using var f=new Fixture();void Mutate()=>f.Original.TypedState=new(f.Original.TypedState!.Issuer,f.Input,f.SourceProvider.Origin);if(duringEntry){f.SourceProvider.Hook=s=>{if(s=="enter")Mutate();};Assert.Throws<ArgumentException>(()=>f.Open());}else{using var a=f.Open().Value!;Mutate();Assert.Throws<ArgumentException>(()=>f.Target.RunCompatibleCandidate(a,_=>1));}}
    /// <summary>AC19 an actual view that changes during work cannot authorize delivery.</summary>
    [Fact] public void AC19_ViewMutationDuringWork()
    {using var f=new Fixture();using var a=f.Open().Value!;Assert.Throws<ArgumentException>(()=>f.Target.RunCompatibleCandidate(a,_=>{f.TargetProvider.LastView!.Source=null!;return 1;}));}
    /// <summary>AC20 execution is confined to the exact consuming session and live admission.</summary>
    [Fact] public void AC20_ConsumerConfinement()
    {using var f=new Fixture();using var other=new Fixture();var a=f.Open().Value!;Assert.Throws<ArgumentException>(()=>other.Target.RunCompatibleCandidate(a,_=>1));a.Dispose();Assert.Throws<ObjectDisposedException>(()=>f.Target.RunCompatibleCandidate(a,_=>1));Assert.Equal(1,f.TargetProvider.Establishes);}
    /// <summary>AC21 blocked real Establish keeps source/target backend and actual original-input handles alive through close/drain.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task AC21_ActualProofResources()
    {
        using var f=new Fixture();using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        using var inputFile=NewFile();using var sourceFile=NewFile();using var targetFile=NewFile();f.Input.Resource=inputFile;f.SourceProvider.Resource=sourceFile;f.TargetProvider.Resource=targetFile;
        f.TargetProvider.Hook=s=>{if(s!="establish")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(15)));Assert.True(inputFile.CanWrite);Assert.True(sourceFile.CanWrite);Assert.True(targetFile.CanWrite);sourceFile.WriteByte(1);targetFile.WriteByte(2);};
        var open=Task.Run(f.Open);
        try
        {Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));f.Source.Dispose();f.Target.Dispose();f.Input.Dispose();f.SourceProvider.Dispose();f.TargetProvider.Dispose();Assert.True(inputFile.CanWrite);Assert.True(sourceFile.CanWrite);Assert.True(targetFile.CanWrite);Assert.False(f.Source.DrainAsync().IsCompleted);Assert.False(f.Target.DrainAsync().IsCompleted);using var c=new CancellationTokenSource();c.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>f.Target.DrainAsync(c.Token));Assert.False(f.Target.DrainAsync().IsCompleted);}
        finally{resume.Set();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await open.WaitAsync(TimeSpan.FromSeconds(10)));await f.Source.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));await f.Target.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(inputFile.CanWrite);Assert.False(sourceFile.CanWrite);Assert.False(targetFile.CanWrite);Assert.DoesNotContain("decision",f.TargetProvider.Calls);Assert.Equal(1,f.Input.Releases);
    }
    private static FileStream NewFile()=>new(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
    /// <summary>AC22 canceled actual proof work refuses before result getters and releases every raw admission.</summary>
    [Fact] public void AC22_RawCancellation()
    {using var f=new Fixture();using var c=new CancellationTokenSource();f.TargetProvider.Hook=s=>{if(s=="establish")c.Cancel();};Assert.ThrowsAny<OperationCanceledException>(()=>f.Target.OpenCompatibleCandidate(f.Original,c.Token));Assert.DoesNotContain("decision",f.TargetProvider.Calls);Assert.Equal(1,f.SourceProvider.Releases);Assert.Equal(1,f.TargetProvider.Releases);}
    /// <summary>AC23 exact selector and actual captured limits/options distinguish the internal admission descriptor.</summary>
    [Fact] public void AC23_FrozenDescriptor()
    {var ns=new Dictionary<string,string>{{"p","urn:one"}};using var f=new Fixture(options:new CompileOptions{Namespaces=ns});ns["p"]="urn:two";Assert.Equal("urn:one",f.Target.Environment.StaticContext.Namespaces["p"]);foreach(var l in new[]{new SchemaQueryLimits(2,4,4,128,128),new(1,5,4,128,128),new(1,4,5,128,128),new(1,4,4,129,128),new(1,4,4,128,127)}){using var other=new Fixture(limits:l,options:new CompileOptions{Namespaces=new Dictionary<string,string>{{"p","urn:one"}}});Assert.NotEqual(f.Target.Environment.StaticContextFingerprint,other.Target.Environment.StaticContextFingerprint);}}
    /// <summary>AC24 unsupported syntax/options refuses before binding; no schema predicate/constructor grammar is activated.</summary>
    /// <param name="source">Unsupported source.</param>
    [Theory][InlineData("$compatible + 1")][InlineData("($compatible)")][InlineData("$p:compatible")][InlineData("$value")]
    public void AC24_Grammar(string source)
    {using var p=new Provider{Compatibility=true};Assert.Equal(SchemaResultKind.Unsupported,Session.CompileCompatibleAdmissionCandidate(source,new(1,4,4,128,128),p).Kind);Assert.Equal(0,p.Binds);}
    /// <summary>AC25 null/throwing real admission rolls back before any proof or publication, then refunds the target slot.</summary>
    /// <param name="source">Source rather than target entry fails.</param><param name="throwing">Throw instead of returning null.</param>
    [Theory][InlineData(false,false)][InlineData(true,false)][InlineData(false,true)][InlineData(true,true)]
    public void AC25_AcquisitionRollback(bool source,bool throwing)
    {using var f=new Fixture();var p=source?f.SourceProvider:f.TargetProvider;var failure=new ApplicationException("entry");if(throwing)p.Hook=s=>{if(s=="enter")throw failure;};else p.NullLease=true;var error=Record.Exception(()=>f.Open());if(throwing)Assert.Same(failure,error);else Assert.IsType<InvalidOperationException>(error);Assert.Equal(0,f.TargetProvider.Establishes);Assert.Equal(0,f.SourceProvider.Active);Assert.Equal(0,f.TargetProvider.Active);p.Hook=null;p.NullLease=false;using var retry=f.Open().Value!;}
    /// <summary>AC26 a source callback closure is protected by the real source retainer and cannot publish a target snapshot.</summary>
    [Fact] public void AC26_InputCallbackClosure()
    {using var f=new Fixture();using var file=NewFile();f.Input.Resource=file;f.Input.Hook=()=>{f.Source.Dispose();Assert.True(file.CanWrite);};Assert.Throws<ObjectDisposedException>(()=>f.Open());Assert.Equal(0,f.SourceProvider.Entries);Assert.Equal(0,f.TargetProvider.Entries);Assert.Equal(1,f.Input.Releases);f.Input.Dispose();Assert.False(file.CanWrite);}
    /// <summary>AC27 retaining a raw proof guard cannot revive authorization or replay view/provider getters after completion.</summary>
    [Fact] public void AC27_ExpiredProofGuard()
    {using var f=new Fixture();using var a=f.Open().Value!;Action? retained=null;f.Target.RunCompatibleCandidate(a,check=>{retained=check;return 1;});int sourceCalls=f.SourceProvider.Calls.Count,targetCalls=f.TargetProvider.Calls.Count;Assert.Throws<ObjectDisposedException>(()=>retained!());Assert.Equal(sourceCalls,f.SourceProvider.Calls.Count);Assert.Equal(targetCalls,f.TargetProvider.Calls.Count);Assert.Equal(0,f.SourceProvider.Active);Assert.Equal(0,f.TargetProvider.Active);}
    /// <summary>AC28 independent raw operations renew proofs/admissions separately on one neutral target snapshot.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task AC28_IndependentRawProofs()
    {
        using var f=new Fixture();using var a=f.Open().Value!;using var entered=new CountdownEvent(2);using var resume=new ManualResetEventSlim();
        int Work(Action check){entered.Signal();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));check();return 1;}
        var first=Task.Run(()=>f.Target.RunCompatibleCandidate(a,Work));var second=Task.Run(()=>f.Target.RunCompatibleCandidate(a,Work));
        try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.Equal(2,f.SourceProvider.Active);Assert.Equal(2,f.TargetProvider.Active);Assert.Equal(3,f.TargetProvider.Establishes);Assert.Equal(1,f.SourceProvider.Releases);}
        finally{resume.Set();}
        Assert.Equal(new[]{1,1},await Task.WhenAll(first,second));Assert.Equal(3,f.SourceProvider.Entries);Assert.Equal(3,f.TargetProvider.Entries);Assert.Equal(3,f.SourceProvider.Releases);Assert.Equal(3,f.TargetProvider.Releases);Assert.Equal(1,f.Input.Entries);
    }
}
