// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies public compatible handoff, raw proof scope and bounded singleton identity.
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
//                      | Charles Korthout | 0.2 | 06-11-2026     | Qualify leased compatible provenance and exact operation expiry (1.78.0). |
//                      | Charles Korthout | 0.3 | 06-11-2026     | Keep compatible complete-container transport explicitly unsupported (1.79.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4 | 06-11-2026     | Qualify complete compatible containers and operation-local windows (1.80.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5 | 06-11-2026     | Qualify wider compatible representations with unchanged raw proof guards (1.81.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using static Bosak.XPath.Runtime.Tests.PublicTypedIdentityTests;
using System.Numerics;
using System.Reflection;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
using Pair=Bosak.XPath.Runtime.Tests.CompatibleAdmissionTests.Fixture;
using Provider=Bosak.XPath.Runtime.Tests.CompatibleAdmissionTests.Provider;
using Container=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Container;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies CV01-CV32 on the public compatible entry and operation route.</summary>
/// <param name="output">Captures bounded leased-observation cost diagnostics.</param>
public sealed class PublicCompatibleTests(ITestOutputHelper output)
{
    private static Pair New(SchemaQueryLimits? limits=null)=>new(publicIdentity:true,limits:limits);
    private static SchemaQuerySnapshot Open(Pair f)=>f.Target.OpenCompatibleSnapshot(f.Original).Snapshot!;
    private static EvaluationContext Context(XdmValue v)=>new EvaluationContext().WithVariable("compatible",v);
    private static Container Value(Pair f,XdmValue? v=null,int count=1,bool list=false,bool fast=true)
    {
        var governing=new Handle();var declared=new Handle();var actual=new Handle();f.SourceProvider.Authority.Types.UnionWith([governing,declared,actual]);
        var c=new Container{Governing=governing,List=list,Fast=fast};c.Items=Enumerable.Range(0,count).Select(_=>new SchemaAtomicItem(v??XdmValue.FromInteger(7),new Provenance(c,declared,actual,f.SourceProvider.Origin))).ToArray();
        f.SourceProvider.TypedAllowed=true;f.SourceProvider.Values.Add(c);return c;
    }
    private static XdmValue Read(Pair f,SchemaQuerySnapshot s,Container c)=>f.Target.Run([s],op=>op.ReadTypedValue(s,c).Value);
    private static XdmValue Eval(Pair f,SchemaQuerySnapshot s,XdmValue v)=>f.Target.Run([s],op=>op.Evaluate(Context(v)));
    /// <summary>CV01 exact selector requires an assessor and captures its distinct public environment.</summary>
    [Fact] public void CV01_PublicSelector()
    {using var f=New();Assert.Equal(CompatibleAdmissionPlan.PublicProfile,f.Target.Environment.BehaviorProfile);Assert.Equal(CompatibleAdmissionPlan.PublicDescriptor,f.Target.Environment.DescriptorFormatVersion);using var ordinary=Session.CompileXPath("$compatible",new(1,4,4,128,128)).Value;Assert.NotEqual(CompatibleAdmissionPlan.PublicProfile,ordinary.Environment.BehaviorProfile);Assert.Equal(SchemaCompatibilityDecision.Unsupported,ordinary.OpenCompatibleSnapshot(f.Original).Decision);using var p=new Provider{Compatibility=true};using var trimmed=Session.CompileXPath("  $compatible  ",new(1,4,4,128,128),assessor:p).Value;Assert.Equal(f.Target.Environment.StaticContextFingerprint,trimmed.Environment.StaticContextFingerprint);}
    /// <summary>CV02 target capability absence refuses compilation without operation acquisition.</summary>
    /// <param name="mode">Missing optional capability.</param>
    [Theory][InlineData("lifetime")][InlineData("authority")]
    public void CV02_TargetCapabilities(string mode)
    {using var p=new Provider{Lifetime=mode!="lifetime",Compatibility=mode!="authority"};Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("$compatible",new(1,4,4,128,128),assessor:p).Kind);Assert.Equal(0,p.Entries);}
    /// <summary>CV02/03 missing original origin or source lifetime returns Unsupported before entry.</summary>
    /// <param name="origin">Original assessment exists.</param><param name="lifetime">Source supports admission.</param>
    [Theory][InlineData(false,true)][InlineData(true,false)]
    public void CV03_OriginalLineage(bool origin,bool lifetime)
    {using var f=new Pair(origin,new Provider{Lifetime=lifetime},publicIdentity:true);var r=f.Target.OpenCompatibleSnapshot(f.Original);Assert.Equal(SchemaCompatibilityDecision.Unsupported,r.Decision);Assert.Null(r.Snapshot);Assert.Equal(0,f.SourceProvider.Entries);Assert.Equal(0,f.TargetProvider.Entries);}
    /// <summary>CV04 required source and live engine lineage reject before proof use.</summary>
    [Fact] public void CV04_NullDisposedSource()
    {using var f=New();Assert.Throws<ArgumentNullException>(()=>f.Target.OpenCompatibleSnapshot(null!));f.Original.Dispose();Assert.Throws<ObjectDisposedException>(()=>Open(f));Assert.Equal(0,f.TargetProvider.Establishes);}
    /// <summary>CV05 raw views cannot bypass public handoff on either profile.</summary>
    [Fact] public void CV05_RawViewRefusal()
    {using var f=New();using var s=Open(f);Assert.Equal(SchemaResultKind.Unsupported,f.Target.OpenSnapshot(f.Input,f.Input,f.SourceProvider.Origin,f.TargetProvider.LastView).Kind);Assert.Equal(SchemaResultKind.Unsupported,f.Source.OpenSnapshot(f.Input,f.Input,f.SourceProvider.Origin,f.TargetProvider.LastView).Kind);}
    /// <summary>CV06/07 exact endpoint proof returns one immutable successful target snapshot.</summary>
    [Fact] public void CV06_ExactEndpoints()
    {using var f=New();var r=f.Target.OpenCompatibleSnapshot(f.Original);using var s=r.Snapshot!;Assert.Equal(SchemaCompatibilityDecision.Compatible,r.Decision);Assert.Null(r.DiagnosticCode);Assert.Same(f.Source.Advisor,f.TargetProvider.LastSource);Assert.Same(f.Target.Advisor,f.TargetProvider.LastTarget);Assert.Same(f.Target.Environment,f.TargetProvider.LastEnvironment);Assert.Same(f.Original.Binding,s.Binding);Assert.Null(s.TypedState);Assert.NotNull(s.CompatibleState);}
    /// <summary>CV08 negative decisions preserve bounded distinct diagnostics and refund admission.</summary>
    /// <param name="decision">Owned negative proof.</param>
    [Theory][InlineData(SchemaCompatibilityDecision.Incompatible)][InlineData(SchemaCompatibilityDecision.Unsupported)]
    public void CV08_NegativeOutcomes(SchemaCompatibilityDecision decision)
    {using var f=New();f.TargetProvider.Decision=decision;var r=f.Target.OpenCompatibleSnapshot(f.Original);Assert.Equal(decision,r.Decision);Assert.Null(r.Snapshot);Assert.Contains(decision==SchemaCompatibilityDecision.Incompatible?"INCOMPATIBLE":"UNSUPPORTED",r.DiagnosticCode);Assert.DoesNotContain("view",f.TargetProvider.Calls);f.TargetProvider.Decision=SchemaCompatibilityDecision.Compatible;using var s=Open(f);}
    /// <summary>CV09 malformed proof values reject rather than mint an admission.</summary>
    /// <param name="mode">Malformed answer.</param>
    [Theory][InlineData("null")][InlineData("unknown")][InlineData("view")]
    public void CV09_Malformed(string mode)
    {using var f=New();f.TargetProvider.NullResult=mode=="null";f.TargetProvider.NullView=mode=="view";if(mode=="unknown")f.TargetProvider.Decision=(SchemaCompatibilityDecision)99;Assert.Throws<ArgumentException>(()=>Open(f));}
    /// <summary>CV10 equal fingerprints cannot authenticate copied endpoint claims.</summary>
    /// <param name="mode">Wrong exact endpoint.</param>
    [Theory][InlineData("source")][InlineData("target")][InlineData("environment")]
    public void CV10_CopiedEndpoints(string mode)
    {using var f=New();using var other=New();f.TargetProvider.ChangeView=v=>{if(mode=="source")v.Source=other.Source.Advisor!;else if(mode=="target")v.Target=other.Target.Advisor!;else v.Environment=other.Target.Environment;return v;};Assert.Throws<ArgumentException>(()=>Open(f));}
    /// <summary>CV11 each raw operation gets one fresh proof and current source-issued cached identity.</summary>
    [Fact] public void CV11_RawReproof()
    {using var f=New();using var s=Open(f);var c=Value(f);var v=Read(f,s,c);var old=f.TargetProvider.LastView;c.OnRead=_=>throw new Exception("cached getter replay");var again=Read(f,s,c);Assert.NotSame(old,f.TargetProvider.LastView);Assert.Same(XdmValue.SchemaTransportMarker(v),XdmValue.SchemaTransportMarker(again));Assert.Equal(v,Eval(f,s,v));Assert.Equal(4,f.TargetProvider.Establishes);Assert.Equal(0,f.Original.Runtime.TypedScalarCount);Assert.Equal(1,s.Runtime.TypedScalarCount);}
    /// <summary>CV12 a later negative proof denies before work and typed getters.</summary>
    /// <param name="decision">Changed proof decision.</param>
    [Theory][InlineData(SchemaCompatibilityDecision.Incompatible)][InlineData(SchemaCompatibilityDecision.Unsupported)]
    public void CV12_NegativeReproof(SchemaCompatibilityDecision decision)
    {using var f=New();using var s=Open(f);var c=Value(f);f.TargetProvider.Decision=decision;bool work=false;Assert.Throws<NotSupportedException>(()=>f.Target.Run([s],op=>{work=true;return op.ReadTypedValue(s,c);}));Assert.False(work);Assert.Empty(c.Reads);}
    /// <summary>CV13 actual proof and view callbacks cannot deliver after source revocation.</summary>
    /// <param name="stage">Proof callback boundary.</param>
    [Theory][InlineData("establish")][InlineData("decision")][InlineData("view")][InlineData("view-source")][InlineData("view-target")][InlineData("view-environment")]
    public void CV13_ProofRevocation(string stage)
    {using var f=New();using var s=Open(f);f.TargetProvider.Hook=x=>{if(x==stage)f.Source.Dispose();};Assert.Throws<ObjectDisposedException>(()=>f.Target.Run([s],_=>1));Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);Assert.Equal(f.TargetProvider.Entries,f.TargetProvider.Releases);}
    /// <summary>CV14 real input and both backend resources survive close until blocked typed tail completes.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task CV14_ActualResources()
    {
        using var f=New();using var s=Open(f);var c=Value(f,fast:false);using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        using var input=NewFile();using var source=NewFile();using var target=NewFile();f.Input.Resource=input;f.SourceProvider.Resource=source;f.TargetProvider.Resource=target;
        c.OnRead=x=>{if(x!="current")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(15)));Assert.True(input.CanWrite);Assert.True(source.CanWrite);Assert.True(target.CanWrite);source.WriteByte(1);target.WriteByte(2);};
        var pending=Task.Run(()=>Read(f,s,c));try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));f.Source.Dispose();f.Target.Dispose();f.Input.Dispose();f.SourceProvider.Dispose();f.TargetProvider.Dispose();Assert.True(input.CanWrite);Assert.True(source.CanWrite);Assert.True(target.CanWrite);Assert.False(f.Source.DrainAsync().IsCompleted);Assert.False(f.Target.DrainAsync().IsCompleted);}finally{resume.Set();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await pending.WaitAsync(TimeSpan.FromSeconds(10)));await f.Source.DrainAsync();await f.Target.DrainAsync();Assert.False(input.CanWrite);Assert.False(source.CanWrite);Assert.False(target.CanWrite);Assert.Equal(1,c.Disposals);
    }
    private static FileStream NewFile()=>new(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
    /// <summary>CV15 second entry failure rolls back once and returns the target slot.</summary>
    [Fact] public void CV15_EntryRollback()
    {using var f=New();f.TargetProvider.NullLease=true;Assert.Throws<InvalidOperationException>(()=>Open(f));Assert.Equal(1,f.SourceProvider.Releases);Assert.Equal(0,f.SourceProvider.Active);Assert.Equal(0,f.TargetProvider.Active);f.TargetProvider.NullLease=false;using var s=Open(f);}
    /// <summary>CV15 primary and both cleanup exception identities/order are preserved.</summary>
    [Fact] public void CV15_CleanupOrdering()
    {using var f=New();using var s=Open(f);var primary=new ApplicationException("work");var target=new AggregateException(new Exception("target"));var source=new ApplicationException("source");f.TargetProvider.Release=()=>throw target;f.SourceProvider.Release=()=>throw source;var e=Assert.Throws<AggregateException>(()=>f.Target.Run<int>([s],_=>throw primary));Assert.Equal(new Exception[]{primary,target,source},e.InnerExceptions);}
    /// <summary>CV16 source tuple issuance and original authority ownership precede all evidence use.</summary>
    /// <param name="mode">Revoked source claim.</param>
    [Theory][InlineData("container")][InlineData("origin")][InlineData("governing")][InlineData("declared")][InlineData("actual")]
    public void CV16_SourceAuthentication(string mode)
    {using var f=New();using var s=Open(f);var c=Value(f);var v=Read(f,s,c);c.OnRead=_=>throw new Exception("getter replay");if(mode=="container")f.SourceProvider.Values.Clear();else if(mode=="origin")f.SourceProvider.OriginOwned=false;else f.SourceProvider.Authority.Types.Remove(mode=="governing"?c.Governing:mode=="declared"?((Provenance)c.Items[0].TypeEvidence).Declared:((Provenance)c.Items[0].TypeEvidence).Actual);Assert.Throws<ArgumentException>(()=>Eval(f,s,v));}
    /// <summary>CV17/18 original components and origin remain unchanged without reassessment or target claims.</summary>
    [Fact] public void CV17_OriginalEvidence()
    {using var f=New();using var s=Open(f);var c=Value(f);var v=Read(f,s,c);var entry=Assert.IsType<QueryTypedScalarCache.Entry>(XdmValue.SchemaTransportMarker(v));var p=(Provenance)c.Items[0].TypeEvidence;Assert.Same(c.Governing,entry.Group.Governing);Assert.Same(p.Declared,entry.Group.Cells[0].Declared);Assert.Same(p.Actual,entry.Group.Cells[0].Actual);Assert.Same(f.SourceProvider.Origin,entry.Group.Cells[0].Origin);Assert.Empty(f.TargetProvider.Authority.Types);}
    /// <summary>CV19 four eligible carriers retain exact payload and descriptive annotations.</summary>
    /// <param name="kind">Primitive carrier.</param>
    [Theory][InlineData("string")][InlineData("boolean")][InlineData("integer")][InlineData("decimal")]
    public void CV19_ExactIdentity(string kind)
    {using var f=New();using var s=Open(f);var plain=kind switch{"string"=>XdmValue.FromString("source","xs:string","urn:source"),"boolean"=>XdmValue.FromBoolean(true),"integer"=>XdmValue.FromInteger(BigInteger.Pow(10,80)),_=>XdmValue.FromDecimal(new XdmExactDecimal(12345000,6,128,128))};var v=Read(f,s,Value(f,plain));Assert.True(Eval(f,s,v).SchemaTransportMatches(plain));}
    /// <summary>CV20 unsupported shapes and carriers reserve no scalar slots.</summary>
    /// <param name="mode">Unsupported source shape/family.</param>
    [Theory][InlineData("empty")][InlineData("list")][InlineData("multi")][InlineData("double")]
    public void CV20_Eligibility(string mode)
    {using var f=New();using var s=Open(f);var c=Value(f,mode=="double"?XdmValue.FromDouble(2):null,mode=="empty"?0:mode=="multi"?2:1,mode=="list",false);Assert.Equal(SchemaResultKind.Unsupported,f.Target.Run([s],op=>op.ReadTypedValue(s,c)).Kind);Assert.Equal(0,s.Runtime.TypedScalarCount);}
    /// <summary>CV21 finite atom/digit/scale limits refuse before installation and excess Current.</summary>
    /// <param name="mode">Capacity exceeded.</param>
    [Theory][InlineData("atoms")][InlineData("digits")][InlineData("scale")]
    public void CV21_Capacity(string mode)
    {using var f=New(new(1,4,1,3,2));using var s=Open(f);var c=Value(f,mode=="digits"?XdmValue.FromInteger(1000):mode=="scale"?XdmValue.FromDecimal(0.001m):null,mode=="atoms"?2:1,fast:false);c.ForbiddenCurrent=1;Assert.Throws<NotSupportedException>(()=>Read(f,s,c));Assert.Equal(1,c.Currents);Assert.Equal(1,c.Disposals);Assert.Equal(0,s.Runtime.TypedScalarCount);}
    /// <summary>CV22 provider failure leaves no target prefix or source mutation; retry can succeed.</summary>
    [Fact] public void CV22_TransactionalCache()
    {using var f=New();using var s=Open(f);var c=Value(f,fast:false);c.Fail="actual";Assert.Throws<InvalidOperationException>(()=>Read(f,s,c));Assert.Equal(0,s.Runtime.TypedScalarCount);Assert.Equal(0,f.Original.Runtime.TypedScalarCount);c.Fail=null;Read(f,s,c);Assert.Equal(1,s.Runtime.TypedScalarCount);Assert.Equal(0,f.Original.Runtime.TypedScalarCount);}
    /// <summary>CV23 absent, original, foreign and altered transport markers refuse.</summary>
    /// <param name="mode">Foreign marker or altered representation.</param>
    [Theory][InlineData("plain")][InlineData("source")][InlineData("foreign")][InlineData("tampered")]
    public void CV23_Transport(string mode)
    {using var f=New();using var s=Open(f);var c=Value(f);var v=Read(f,s,c);XdmValue bad;if(mode=="source")bad=f.Source.Run([f.Original],op=>op.ReadTypedValue(f.Original,c).Value);else if(mode=="foreign"){using var other=New();using var t=Open(other);bad=Read(other,t,Value(other));}else bad=mode=="plain"?XdmValue.FromInteger(7):XdmValue.SchemaTransportAttach(XdmValue.FromInteger(8),XdmValue.SchemaTransportMarker(v)!);Assert.Throws<ArgumentException>(()=>Eval(f,s,bad));}
    /// <summary>CV24 context capture refuses focus, renamed/extra variables, namespaces, services and structured input.</summary>
    /// <param name="mode">Unsupported dynamic context.</param>
    [Theory][InlineData("focus")][InlineData("name")][InlineData("extra")][InlineData("namespace")][InlineData("loader")][InlineData("map")]
    public void CV24_Context(string mode)
    {using var f=New();using var s=Open(f);var v=Read(f,s,Value(f));var context=mode=="name"?new EvaluationContext().WithVariable("value",v):Context(v);if(mode=="focus")context.WithFocus(XdmValue.FromString("focus"),1,1);if(mode=="extra")context.WithVariable("other",v);if(mode=="namespace")context.WithNamespace("p","urn:p");if(mode=="loader")context.DocumentLoader=_=>throw new Exception("loader poison");if(mode=="map")context=Context(XdmValue.FromMap(new XdmMap()));Assert.Throws<NotSupportedException>(()=>f.Target.Run([s],op=>op.Evaluate(context)));}
    /// <summary>CV24 exact grammar refuses other expressions before binding.</summary>
    /// <param name="expression">Unsupported selector.</param>
    [Theory][InlineData("$compatible + 1")][InlineData("($compatible)")][InlineData("$p:compatible")]
    public void CV24_Grammar(string expression)
    {using var p=new Provider{Compatibility=true};Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath(expression,new(1,4,4,128,128),assessor:p).Kind);Assert.Equal(0,p.Binds);}
    /// <summary>CV25 target snapshot and scalar ceilings retain existing units.</summary>
    [Fact] public void CV25_Quotas()
    {using var f=New(new(1,1,4,128,128));var s=Open(f);Assert.Throws<ArgumentException>(()=>Open(f));var v=Read(f,s,Value(f));Assert.Throws<NotSupportedException>(()=>Read(f,s,Value(f)));Assert.Equal(v,Eval(f,s,v));s.Dispose();using var retry=Open(f);Assert.Equal(0,retry.Runtime.TypedScalarCount);}
    /// <summary>CV26 transitive targets and wrong profiles cannot import compatible admissions.</summary>
    [Fact] public void CV26_NoTransitiveImport()
    {using var f=New();using var s=Open(f);using var other=New();var r=other.Target.OpenCompatibleSnapshot(s);Assert.Equal(SchemaCompatibilityDecision.Unsupported,r.Decision);Assert.Null(r.Snapshot);Assert.Equal(SchemaCompatibilityDecision.Unsupported,f.Source.OpenCompatibleSnapshot(f.Original).Decision);Assert.Equal(SchemaResultKind.Unsupported,f.Target.OpenSnapshot(f.Input,f.Input).Kind);}
    /// <summary>CV27 asynchronous raw ownership survives a canceled wait; escaped operations cannot replay callbacks.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task CV27_AsyncRawAndWait()
    {
        using var f=New();using var s=Open(f);var c=Value(f);var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);SchemaQueryOperation? escaped=null;
        var task=f.Target.RunAsync([s],async op=>{escaped=op;entered.SetResult();await resume.Task;return op.Evaluate(Context(op.ReadTypedValue(s,c).Value));});await entered.Task;using var cancel=new CancellationTokenSource();cancel.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>task.WaitAsync(cancel.Token));Assert.False(task.IsCompleted);Assert.False(f.Target.DrainAsync().IsCompleted);resume.SetResult();Assert.Equal(7,(await task).IntegerValue);int calls=f.SourceProvider.Calls.Count;Assert.Throws<ObjectDisposedException>(()=>escaped!.Evaluate(Context(XdmValue.FromInteger(7))));Assert.Equal(calls,f.SourceProvider.Calls.Count);await f.Target.DrainAsync();Assert.Equal(2,f.TargetProvider.Establishes);
    }
    /// <summary>CV27 both endpoint closure during deferred work prevents delivery after the await.</summary>
    /// <param name="source">Close original session rather than target.</param>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CV27_AsyncClosure(bool source)
    {using var f=New();using var s=Open(f);var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var task=f.Target.RunAsync([s],async _=>{entered.SetResult();await resume.Task;return 7;});await entered.Task;if(source)f.Source.Dispose();else f.Target.Dispose();Assert.False(f.Target.DrainAsync().IsCompleted);resume.SetResult();await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await task);await f.Target.DrainAsync();Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);}
    /// <summary>CV28 provider callback reentry refuses while sequential calls remain allowed in one raw operation.</summary>
    [Fact] public void CV28_Reentry()
    {using var f=New();using var s=Open(f);var c=Value(f);f.Target.Run([s],op=>{bool checkedOnce=false;c.OnRead=stage=>{if(stage!="governing"||checkedOnce)return;checkedOnce=true;Assert.Throws<InvalidOperationException>(()=>op.ReadTypedValue(s,c));};var v=op.ReadTypedValue(s,c).Value;Assert.Equal(v,op.Evaluate(Context(v)));Assert.True(checkedOnce);return 0;});}
    /// <summary>CV28 independent asynchronous operations acquire independent paired admissions and proofs.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task CV28_IndependentOperations()
    {using var f=New();using var s=Open(f);var entered=new CountdownEvent(2);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);async Task<int> Work(SchemaQueryOperation _){entered.Signal();await resume.Task;return 1;}var first=f.Target.RunAsync([s],Work);var second=f.Target.RunAsync([s],Work);Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));Assert.Equal(2,f.SourceProvider.Active);Assert.Equal(2,f.TargetProvider.Active);resume.SetResult();Assert.Equal(new[]{1,1},await Task.WhenAll(first,second));}
    /// <summary>CV29 public delta has one session method and a sealed engine-created immutable result.</summary>
    [Fact] public void CV29_ResultSurface()
    {var type=typeof(SchemaQueryCompatibilityResult);Assert.True(type.IsSealed);Assert.Empty(type.GetConstructors());Assert.Equal(new[]{"Decision","DiagnosticCode","Snapshot"},type.GetProperties().Select(p=>p.Name).Order().ToArray());Assert.All(type.GetProperties(),p=>Assert.Null(p.SetMethod));Assert.Single(typeof(Session).GetMethods(),m=>m.Name=="OpenCompatibleSnapshot");}
    /// <summary>CV30/31 wrong snapshot sets and unselected node/constructor routes refuse without provider getters.</summary>
    [Fact] public void CV30_ClosedMembership()
    {using var f=New();using var s=Open(f);Assert.Throws<ArgumentException>(()=>f.Target.Run([], _=>1));Assert.Throws<ArgumentException>(()=>f.Target.Run([f.Original],_=>1));Assert.Throws<ArgumentException>(()=>f.Target.Run([s,s],_=>1));f.Target.Run([s],op=>{Assert.Throws<NotSupportedException>(()=>op.GetRoot(s));Assert.Throws<ArgumentException>(()=>op.ReadTypedValue(f.Original,Value(f)));return 0;});}
    /// <summary>CV32 descriptors freeze all count/representation limits and caller options.</summary>
    [Fact] public void CV32_Descriptor()
    {using var f=New();foreach(var limits in new[]{new SchemaQueryLimits(2,4,4,128,128),new(1,5,4,128,128),new(1,4,5,128,128),new(1,4,4,129,128),new(1,4,4,128,127)}){using var other=New(limits);Assert.NotEqual(f.Target.Environment.StaticContextFingerprint,other.Target.Environment.StaticContextFingerprint);}var ns=new Dictionary<string,string>{{"p","urn:one"}};using var frozen=new Pair(publicIdentity:true,options:new CompileOptions{Namespaces=ns});ns["p"]="urn:two";Assert.Equal("urn:one",frozen.Target.Environment.StaticContext.Namespaces["p"]);}

    /// <summary>CV27 cleanup-induced endpoint closure denies a result after raw cleanup.</summary>
    /// <param name="source">Close the original session rather than target.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void CV27_CleanupRevocation(bool source)
    {using var f=New();using var s=Open(f);f.TargetProvider.Release=source?f.Source.Dispose:f.Target.Dispose;Assert.Throws<ObjectDisposedException>(()=>f.Target.Run([s],_=>7));Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);}
    /// <summary>CV28 overlapping calls within the same raw operation cannot share a provider window.</summary>
    /// <returns>A task completing after the guarded lifecycle assertions.</returns>
    [Fact] public async Task CV28_Overlap()
    {using var f=New();using var s=Open(f);var c=Value(f);using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();await f.Target.RunAsync([s],async op=>{c.OnRead=stage=>{if(stage!="governing")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));};var first=Task.Run(()=>op.ReadTypedValue(s,c));try{Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));Assert.Throws<InvalidOperationException>(()=>op.ReadTypedValue(s,c));}finally{resume.Set();}Assert.Equal(SchemaResultKind.Supported,(await first).Kind);return 0;});}
    /// <summary>CV10 changed view endpoints at callback completion cannot authorize delivery.</summary>
    [Fact] public void CV10_FinalViewCurrentness()
    {using var f=New();using var s=Open(f);Assert.Throws<ArgumentException>(()=>f.Target.Run([s],_=>{f.TargetProvider.LastView!.Source=null!;return 7;}));}

    /// <summary>LP01 the new member returns original source metadata under the existing paired raw leases.</summary>
    /// <param name="kind">Supported compatible scalar family.</param>
    [Theory][InlineData("string")][InlineData("boolean")][InlineData("integer")][InlineData("decimal")]
    public void LP01_LeasedSourceMetadata(string kind)
    {
        using var f=New();using var snapshot=Open(f);
        var plain=kind switch{"string"=>XdmValue.FromString("value"),"boolean"=>XdmValue.FromBoolean(true),"decimal"=>XdmValue.FromDecimal(1.2300m),_=>XdmValue.FromInteger(7)};
        var c=Value(f,plain);var original=(Provenance)c.Items[0].TypeEvidence;
        f.Target.Run([snapshot],op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var observed=op.ReadAtomicProvenance(value);Assert.Equal(SchemaResultKind.Supported,observed.Kind);
            int sourceEntries=f.SourceProvider.Entries,targetEntries=f.TargetProvider.Entries,proofs=f.TargetProvider.Establishes;
            Assert.Same(original.Declared,observed.Value.DeclaredType);Assert.Same(original.Actual,observed.Value.ActualType);Assert.Same(f.SourceProvider.Origin,observed.Value.OriginalAssessment);
            Assert.False(SchemaAtomics.TryGetProvenance(in value,out _));Assert.Equal(sourceEntries,f.SourceProvider.Entries);Assert.Equal(targetEntries,f.TargetProvider.Entries);Assert.Equal(proofs,f.TargetProvider.Establishes);
            Assert.Equal(1,f.SourceProvider.Active);Assert.Equal(1,f.TargetProvider.Active);Assert.Equal(1,snapshot.Runtime.TypedScalarCount);return 0;
        });
        Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);Assert.Equal(f.TargetProvider.Entries,f.TargetProvider.Releases);
    }

    /// <summary>LP02 a retained view and operation cannot replay provider callbacks after raw completion.</summary>
    [Fact] public void LP02_ExpiredView()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);SchemaQueryOperation? retained=null;XdmValue value=default;
        var observed=f.Target.Run([snapshot],op=>{retained=op;value=op.ReadTypedValue(snapshot,c).Value;return op.ReadAtomicProvenance(value).Value;});
        int sourceCalls=f.SourceProvider.Calls.Count,targetCalls=f.TargetProvider.Calls.Count;
        foreach(var getter in new Func<object?>[]{()=>observed.DeclaredType,()=>observed.ActualType,()=>observed.OriginalAssessment})Assert.Throws<ObjectDisposedException>(()=>getter());
        Assert.Throws<ObjectDisposedException>(()=>retained!.ReadAtomicProvenance(value));
        Assert.Equal(sourceCalls,f.SourceProvider.Calls.Count);Assert.Equal(targetCalls,f.TargetProvider.Calls.Count);Assert.False(SchemaAtomics.TryGetProvenance(in value,out _));
    }

    /// <summary>LP03 invalid source/plain/foreign/rebound carriers cannot acquire a provenance view.</summary>
    /// <param name="mode">Wrong carrier identity or representation.</param>
    [Theory][InlineData("plain")][InlineData("rebound")][InlineData("source")][InlineData("foreign")][InlineData("undefined")][InlineData("map")]
    public void LP03_InvalidCarrier(string mode)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var correct=Read(f,snapshot,c);
        using var other=New();using var otherSnapshot=Open(other);var otherValue=Read(other,otherSnapshot,Value(other));
        var original=f.Source.Run([f.Original],op=>op.ReadTypedValue(f.Original,c).Value);
        var value=mode switch{"plain"=>XdmValue.FromInteger(7),"rebound"=>XdmValue.SchemaTransportAttach(XdmValue.FromInteger(8),XdmValue.SchemaTransportMarker(correct)!),
            "source"=>original,"foreign"=>otherValue,"map"=>XdmValue.FromMap(new XdmMap()),_=>XdmValue.Undefined};
        f.Target.Run([snapshot],op=>{Assert.Throws<ArgumentException>(()=>op.ReadAtomicProvenance(value));return 0;});Assert.Equal(1,snapshot.Runtime.TypedScalarCount);
    }

    /// <summary>LP04 every getter refuses current proof/source/type changes before publishing metadata.</summary>
    /// <param name="mode">Revoked current fact.</param>
    [Theory][InlineData("declared")][InlineData("actual")][InlineData("origin")][InlineData("container")][InlineData("view-source")][InlineData("view-target")][InlineData("view-environment")]
    public void LP04_CurrentValidation(string mode)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var original=(Provenance)c.Items[0].TypeEvidence;
        f.Target.Run([snapshot],op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var view=op.ReadAtomicProvenance(value).Value;
            var proof=f.TargetProvider.LastView!;var source=proof.Source;var target=proof.Target;var environment=proof.Environment;
            if(mode=="declared")f.SourceProvider.Authority.Types.Remove(original.Declared);else if(mode=="actual")f.SourceProvider.Authority.Types.Remove(original.Actual);
            else if(mode=="origin")f.SourceProvider.OriginOwned=false;else if(mode=="container")f.SourceProvider.Values.Remove(c);
            else if(mode=="view-source")proof.Source=null!;else if(mode=="view-target")proof.Target=null!;else proof.Environment=f.Source.Environment;
            try{foreach(var getter in new Func<object?>[]{()=>view.DeclaredType,()=>view.ActualType,()=>view.OriginalAssessment})Assert.Throws<ArgumentException>(()=>getter());}
            finally{f.SourceProvider.Authority.Types.Add(original.Declared);f.SourceProvider.Authority.Types.Add(original.Actual);f.SourceProvider.OriginOwned=true;f.SourceProvider.Values.Add(c);proof.Source=source;proof.Target=target;proof.Environment=environment;}
            return 0;
        });
    }

    /// <summary>LP05 source/target closure, snapshot revocation and raw cancellation revoke all getters.</summary>
    /// <param name="mode">Raw lifetime revocation source.</param>
    [Theory][InlineData("source")][InlineData("target")][InlineData("original")][InlineData("snapshot")][InlineData("cancel")]
    public void LP05_Revocation(string mode)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);using var cancellation=new CancellationTokenSource();
        var failure=Record.Exception(()=>f.Target.Run([snapshot],op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var view=op.ReadAtomicProvenance(value).Value;
            if(mode=="source")f.Source.Dispose();else if(mode=="target")f.Target.Dispose();else if(mode=="original")f.Original.Dispose();else if(mode=="snapshot")snapshot.Dispose();else cancellation.Cancel();
            foreach(var getter in new Func<object?>[]{()=>view.DeclaredType,()=>view.ActualType,()=>view.OriginalAssessment})
            {var error=Record.Exception(()=>getter());if(mode=="cancel")Assert.IsAssignableFrom<OperationCanceledException>(error);else Assert.IsType<ObjectDisposedException>(error);}
            return 0;
        },cancellation.Token));
        if(mode=="cancel")Assert.IsAssignableFrom<OperationCanceledException>(failure);else Assert.IsType<ObjectDisposedException>(failure);
    }

    /// <summary>LP06 later raw episodes reprove while stable scalar identity and old-view expiry remain unchanged.</summary>
    [Fact] public void LP06_FreshEpisodeStableScalar()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var value=Read(f,snapshot,c);ISchemaAtomicProvenance? old=null;object? firstProof=null;
        f.Target.Run([snapshot],op=>{firstProof=f.TargetProvider.LastView;old=op.ReadAtomicProvenance(value).Value;_=old.ActualType;return 0;});
        c.OnRead=_=>throw new Exception("cached cell replay");
        f.Target.Run([snapshot],op=>
        {
            Assert.NotSame(firstProof,f.TargetProvider.LastView);var view=op.ReadAtomicProvenance(value).Value;Assert.NotSame(old,view);
            Assert.Throws<ObjectDisposedException>(()=>old!.DeclaredType);Assert.Same(((Provenance)c.Items[0].TypeEvidence).Actual,view.ActualType);
            Assert.Same(XdmValue.SchemaTransportMarker(value),XdmValue.SchemaTransportMarker(op.ReadTypedValue(snapshot,c).Value));Assert.Equal(value,op.Evaluate(Context(value)));return 0;
        });
        Assert.Null(typeof(QueryTypedScalarCache).GetField("_issuer",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(snapshot.Runtime.RequireTypedScalars()));
    }

    /// <summary>LP07 ordinary and host operations explicitly leave leased compatible observation unsupported.</summary>
    [Fact] public void LP07_OtherProfileUnsupported()
    {
        using var session=Session.CompileXPath("true()",new(1,4,4,128,128)).Value;
        session.Run([],op=>{Assert.Equal(SchemaResultKind.Unsupported,op.ReadAtomicProvenance(XdmValue.FromInteger(7)).Kind);return 0;});
    }

    /// <summary>LP08 reentrant metadata observation refuses before a nested provider window can publish.</summary>
    [Fact] public void LP08_Reentry()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);
        f.Target.Run([snapshot],op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var view=op.ReadAtomicProvenance(value).Value;bool checkedReentry=false;
            f.TargetProvider.Hook=stage=>{if(stage!="available"||checkedReentry)return;checkedReentry=true;Assert.Throws<InvalidOperationException>(()=>view.ActualType);};
            try{_=view.DeclaredType;Assert.True(checkedReentry);}finally{f.TargetProvider.Hook=null;}
            return 0;
        });
    }

    /// <summary>LP09 an actual blocked getter retains input/source/target files through closure and canceled observation.</summary>
    /// <returns>The completed real provider tail and drain check.</returns>
    [Fact] public async Task LP09_ActualGetterTail()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);
        using var input=NewFile();using var source=NewFile();using var target=NewFile();f.Input.Resource=input;f.SourceProvider.Resource=source;f.TargetProvider.Resource=target;
        using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();Task<Exception?>? getter=null;
        var pending=f.Target.RunAsync([snapshot],async op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var view=op.ReadAtomicProvenance(value).Value;
            f.TargetProvider.Hook=stage=>{if(stage!="available")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(20)));Assert.True(input.CanWrite);Assert.True(source.CanWrite);Assert.True(target.CanWrite);source.WriteByte(1);target.WriteByte(2);};
            getter=Task.Run(()=>Record.Exception(()=>view.ActualType));
            try{await getter;}finally{f.TargetProvider.Hook=null;}return 0;
        });
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));f.Source.Dispose();f.Target.Dispose();f.Input.Dispose();f.SourceProvider.Dispose();f.TargetProvider.Dispose();
            using var cancellation=new CancellationTokenSource();cancellation.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>pending.WaitAsync(cancellation.Token));
            Assert.True(input.CanWrite);Assert.True(source.CanWrite);Assert.True(target.CanWrite);Assert.False(pending.IsCompleted);
            Assert.False(f.Source.DrainAsync().IsCompleted);Assert.False(f.Target.DrainAsync().IsCompleted);resume.Set();
            await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await pending.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.IsType<ObjectDisposedException>(await getter!);await f.Source.DrainAsync();await f.Target.DrainAsync();
            Assert.False(input.CanWrite);Assert.False(source.CanWrite);Assert.False(target.CanWrite);
        }
        finally{resume.Set();try{await pending.WaitAsync(TimeSpan.FromSeconds(10));}catch(ObjectDisposedException){}f.TargetProvider.Hook=null;}
    }

    /// <summary>LP10 overlapping getters in one operation cannot enter a second provider window.</summary>
    /// <returns>The serialized getter completion.</returns>
    [Fact] public async Task LP10_Overlap()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();
        await f.Target.RunAsync([snapshot],async op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var view=op.ReadAtomicProvenance(value).Value;
            f.TargetProvider.Hook=stage=>{if(stage!="available")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(15)));};
            var first=Task.Run(()=>view.ActualType);
            try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.Throws<InvalidOperationException>(()=>view.DeclaredType);}
            finally{resume.Set();}
            Assert.NotNull(await first);f.TargetProvider.Hook=null;return 0;
        });
    }

    /// <summary>LP11 source getter failures retain their original object rather than becoming false results.</summary>
    [Fact] public void LP11_OriginalFailureIdentity()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var failure=new AggregateException(new Exception("provider failure"));
        f.Target.Run([snapshot],op=>
        {
            var value=op.ReadTypedValue(snapshot,c).Value;var view=op.ReadAtomicProvenance(value).Value;
            f.TargetProvider.Hook=stage=>{if(stage=="available")throw failure;};
            try{Assert.Same(failure,Assert.Throws<AggregateException>(()=>view.OriginalAssessment));}finally{f.TargetProvider.Hook=null;}return 0;
        });
    }

    /// <summary>LP12 records bounded caller allocation/latency/callback cost without selecting owner budgets.</summary>
    [Fact] public void LP12_CostObservation()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var value=Read(f,snapshot,c);
        f.Target.Run([snapshot],op=>
        {
            var coldStart=Stopwatch.GetTimestamp();long coldBytes=GC.GetAllocatedBytesForCurrentThread();
            var view=op.ReadAtomicProvenance(value).Value;_=view.DeclaredType;_=view.ActualType;_=view.OriginalAssessment;
            coldBytes=GC.GetAllocatedBytesForCurrentThread()-coldBytes;long coldTicks=Stopwatch.GetTimestamp()-coldStart;
            for(int i=0;i<3;i++){_=view.DeclaredType;_=view.ActualType;_=view.OriginalAssessment;}
            const int runs=8;int callbacks=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count;
            long start=Stopwatch.GetTimestamp(),bytes=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<runs;i++){_=view.DeclaredType;_=view.ActualType;_=view.OriginalAssessment;}
            bytes=GC.GetAllocatedBytesForCurrentThread()-bytes;long ticks=Stopwatch.GetTimestamp()-start;
            callbacks=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count-callbacks;
            output.WriteLine($"Leased provenance cost: cold caller bytes {coldBytes}; cold ticks {coldTicks}; warm reads {runs}; warm caller bytes {bytes}; warm ticks {ticks}; callbacks {callbacks}; frequency {Stopwatch.Frequency}");
            Assert.True(coldBytes>0);Assert.True(bytes>0);Assert.True(ticks>0);Assert.True(callbacks>0);Assert.Equal(1,snapshot.Runtime.TypedScalarCount);return 0;
        });
    }

    /// <summary>LP13 independent raw operations retain separate view epochs while sharing stable scalar identity.</summary>
    /// <returns>The completed two-operation epoch check.</returns>
    [Fact] public async Task LP13_IndependentOperationViews()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var value=Read(f,snapshot,c);
        var firstReady=new TaskCompletionSource<ISchemaAtomicProvenance>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReady=new TaskCompletionSource<ISchemaAtomicProvenance>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishFirst=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstEnded=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first=f.Target.RunAsync([snapshot],async op=>
        {var view=op.ReadAtomicProvenance(value).Value;firstReady.SetResult(view);await finishFirst.Task;return 0;});
        var second=f.Target.RunAsync([snapshot],async op=>
        {
            var view=op.ReadAtomicProvenance(value).Value;secondReady.SetResult(view);await firstEnded.Task;
            var expired=await firstReady.Task;int calls=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count;
            Assert.Throws<ObjectDisposedException>(()=>expired.ActualType);Assert.Equal(calls,f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count);
            Assert.Same(((Provenance)c.Items[0].TypeEvidence).Actual,view.ActualType);Assert.False(SchemaAtomics.TryGetProvenance(in value,out _));return 0;
        });
        try
        {
            await Task.WhenAll(firstReady.Task,secondReady.Task).WaitAsync(TimeSpan.FromSeconds(10));Assert.NotSame(await firstReady.Task,await secondReady.Task);
            finishFirst.SetResult();await first.WaitAsync(TimeSpan.FromSeconds(10));firstEnded.SetResult();await second.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally{finishFirst.TrySetResult();firstEnded.TrySetResult();await Task.WhenAll(first,second).WaitAsync(TimeSpan.FromSeconds(10));}
        Assert.Equal(1,snapshot.Runtime.TypedScalarCount);Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);Assert.Equal(f.TargetProvider.Entries,f.TargetProvider.Releases);
    }

    /// <summary>TC24 the compatible profile now delivers the existing complete-container signature.</summary>
    [Fact] public void TC24_CompatibleContainerSupported()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);f.Target.Run([snapshot],op=>{var result=op.ReadTypedContainer(snapshot,c);Assert.Equal(SchemaResultKind.Supported,result.Kind);Assert.True(result.Value.TryGetSingleAtomic(out var cell));Assert.Equal(7,cell.Value.IntegerValue);return 0;});Assert.Equal(0,snapshot.Runtime.TypedScalarCount);
    }

    /// <summary>CC01 complete compatible containers preserve list shape independently of cardinality.</summary>
    /// <param name="list">Source list flag.</param><param name="count">Complete source cardinality.</param>
    [Theory][InlineData(false,0)][InlineData(false,1)][InlineData(false,3)][InlineData(true,0)][InlineData(true,1)][InlineData(true,3)]
    public void CC01_Shape(bool list,int count)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,count:count,list:list,fast:count==1);
        f.Target.Run([snapshot],op=>{int proofs=f.TargetProvider.Establishes;var v=op.ReadTypedContainer(snapshot,c).Value;Assert.Same(c.Governing,v.GoverningType);Assert.Equal(list,v.IsList);Assert.Equal(count,v.AtomicItems.Count());Assert.Equal(count==1,v.TryGetSingleAtomic(out var single));if(count!=1)Assert.Null(single.TypeEvidence);Assert.Equal(proofs,f.TargetProvider.Establishes);Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>CC02 selected primitive cells remain plain with separately guarded original metadata.</summary>
    /// <param name="kind">Selected primitive family.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void CC02_Families(int kind)
    {
        using var f=New();using var snapshot=Open(f);var plain=kind switch{0=>XdmValue.FromString("value"),1=>XdmValue.FromBoolean(true),2=>XdmValue.FromInteger(42),_=>XdmValue.FromDecimal(1.25m)};var c=Value(f,plain);var original=(Provenance)c.Items[0].TypeEvidence;
        f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;Assert.True(v.TryGetSingleAtomic(out var cell));Assert.Equal(plain,cell.Value);Assert.Same(original.Declared,cell.TypeEvidence.DeclaredType);Assert.Same(original.Actual,cell.TypeEvidence.ActualType);Assert.Same(f.SourceProvider.Origin,cell.TypeEvidence.OriginalAssessment);var scalar=cell.Value;Assert.Null(XdmValue.SchemaTransportMarker(scalar));Assert.False(SchemaAtomics.TryGetProvenance(in scalar,out _));return 0;});
    }
    /// <summary>CC03 ordered selected union handles retain source identity rather than target remapping.</summary>
    [Fact] public void CC03_UnionMembers()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,count:3,list:true,fast:false);var alternate=new Handle();f.SourceProvider.Authority.Types.Add(alternate);c.Items[1]=new(XdmValue.FromString("middle"),new Provenance(c,alternate,c.Governing,f.SourceProvider.Origin));var original=(Provenance)c.Items[2].TypeEvidence;
        f.Target.Run([snapshot],op=>{var items=op.ReadTypedContainer(snapshot,c).Value.AtomicItems.ToArray();Assert.Equal(3,items.Length);Assert.Same(alternate,items[1].TypeEvidence.DeclaredType);Assert.Same(c.Governing,items[1].TypeEvidence.ActualType);Assert.Same(original.Actual,items[2].TypeEvidence.ActualType);Assert.Equal("middle",items[1].Value.StringValue);return 0;});
    }
    /// <summary>CC04 malformed later members cannot hide behind a wider valid prefix.</summary>
    /// <param name="kind">Invalid later representation.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void CC04_InvalidTail(int kind)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,XdmValue.FromDouble(1d),3,true,false);var evidence=c.Items[0].TypeEvidence;c.Items[2]=kind switch{0=>default,1=>new(XdmValue.Undefined,evidence),2=>new(XdmValue.FromString(null!),evidence),_=>new(XdmValue.FromInteger(3),new Provenance(c,c.Governing,new Handle(),f.SourceProvider.Origin))};
        f.Target.Run([snapshot],op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(snapshot,c));Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>CC05 a previously unsupported primitive family now has complete transport without reservations.</summary>
    [Fact] public void CC05_WiderFamilySupported()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,XdmValue.FromDouble(1),3,true,false);f.Target.Run([snapshot],op=>{Assert.Equal(3,op.ReadTypedContainer(snapshot,c).Value.AtomicItems.Count());Assert.Equal(0,snapshot.Runtime.TypedScalarCount);Assert.Equal(7,op.ReadTypedValue(snapshot,Value(f)).Value.IntegerValue);return 0;});
    }
    /// <summary>CC06 every borrowed public path expires before late provider callbacks while copied payloads survive.</summary>
    [Fact] public void CC06_AllExpiry()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);ISchemaTypedValue view=null!;IEnumerable<SchemaAtomicItem> sequence=null!;IEnumerator<SchemaAtomicItem> cursor=null!;SchemaAtomicItem cell=default;SchemaQueryOperation raw=null!;
        f.Target.Run([snapshot],op=>{raw=op;view=op.ReadTypedContainer(snapshot,c).Value;sequence=view.AtomicItems;cursor=sequence.GetEnumerator();Assert.True(cursor.MoveNext());cell=cursor.Current;return 0;});int callbacks=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count;
        Assert.Throws<ObjectDisposedException>(()=>view.GoverningType);Assert.Throws<ObjectDisposedException>(()=>view.IsList);Assert.Throws<ObjectDisposedException>(()=>view.AtomicItems);Assert.Throws<ObjectDisposedException>(()=>view.TryGetSingleAtomic(out _));Assert.Throws<ObjectDisposedException>(()=>sequence.GetEnumerator());Assert.Throws<ObjectDisposedException>(()=>cursor.MoveNext());Assert.Throws<ObjectDisposedException>(()=>cursor.Current);Assert.Throws<ObjectDisposedException>(()=>cursor.Reset());Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.DeclaredType);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.ActualType);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.OriginalAssessment);Assert.Throws<ObjectDisposedException>(()=>raw.ReadTypedContainer(snapshot,c));cursor.Dispose();cursor.Dispose();Assert.Equal(7,cell.Value.IntegerValue);Assert.Equal(callbacks,f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count);
    }
    /// <summary>CC07 every captured type and exact current proof/issuer relation remains live on later access.</summary>
    /// <param name="mode">Revoked ownership or proof endpoint.</param>
    [Theory][InlineData("governing")][InlineData("declared")][InlineData("actual")][InlineData("origin")][InlineData("container")][InlineData("view-source")][InlineData("view-target")][InlineData("view-environment")]
    public void CC07_CurrentOwnership(string mode)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var atom=(Provenance)c.Items[0].TypeEvidence;
        f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;v.TryGetSingleAtomic(out var cell);var proof=f.TargetProvider.LastView!;var source=proof.Source;var target=proof.Target;var environment=proof.Environment;
            if(mode=="governing")f.SourceProvider.Authority.Types.Remove(c.Governing);else if(mode=="declared")f.SourceProvider.Authority.Types.Remove(atom.Declared);else if(mode=="actual")f.SourceProvider.Authority.Types.Remove(atom.Actual);else if(mode=="origin")f.SourceProvider.OriginOwned=false;else if(mode=="container")f.SourceProvider.Values.Remove(c);else if(mode=="view-source")proof.Source=null!;else if(mode=="view-target")proof.Target=null!;else proof.Environment=f.Source.Environment;
            try{Assert.Throws<ArgumentException>(()=>v.IsList);Assert.Throws<ArgumentException>(()=>cell.TypeEvidence.ActualType);}
            finally{f.SourceProvider.Authority.Types.UnionWith([c.Governing,atom.Declared,atom.Actual]);f.SourceProvider.OriginOwned=true;f.SourceProvider.Values.Add(c);proof.Source=source;proof.Target=target;proof.Environment=environment;}return 0;});
    }
    /// <summary>CC08 source/target/raw revocation invalidates complete views without reviving authority.</summary>
    /// <param name="mode">Lifetime revocation.</param>
    [Theory][InlineData("source")][InlineData("target")][InlineData("original")][InlineData("snapshot")][InlineData("cancel")] public void CC08_Revocation(string mode)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);using var cancel=new CancellationTokenSource();var failure=Record.Exception(()=>f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;if(mode=="source")f.Source.Dispose();else if(mode=="target")f.Target.Dispose();else if(mode=="original")f.Original.Dispose();else if(mode=="snapshot")snapshot.Dispose();else cancel.Cancel();int callbacks=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count;if(mode=="cancel")Assert.ThrowsAny<OperationCanceledException>(()=>v.IsList);else Assert.Throws<ObjectDisposedException>(()=>v.IsList);Assert.Equal(callbacks,f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count);return 0;},cancel.Token));if(mode=="cancel")Assert.IsAssignableFrom<OperationCanceledException>(failure);else Assert.IsType<ObjectDisposedException>(failure);
    }
    /// <summary>CC09 captured digit and scale bounds cover complete compatible payloads.</summary>
    /// <param name="scale">Whether scale exceeds the captured ceiling.</param>
    [Theory][InlineData(false)][InlineData(true)] public void CC09_NumericBounds(bool scale)
    {
        using var f=New(new(1,1,3,8,8));using var snapshot=Open(f);var value=scale?XdmValue.FromDecimal(new XdmExactDecimal(1,9,64,64)):XdmValue.FromInteger(BigInteger.Pow(10,9));var c=Value(f,value);f.Target.Run([snapshot],op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedContainer(snapshot,c));Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>CC10 complete atom capacity refuses before excess Current and permits a valid retry.</summary>
    [Fact] public void CC10_AtomCapacity()
    {
        using var f=New(new(1,1,2,64,64));using var snapshot=Open(f);var c=Value(f,count:3,list:true,fast:false);c.ForbiddenCurrent=2;f.Target.Run([snapshot],op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedContainer(snapshot,c));Assert.Equal(2,c.Currents);Assert.Equal(0,snapshot.Runtime.TypedScalarCount);Assert.True(op.ReadTypedContainer(snapshot,Value(f)).Value.TryGetSingleAtomic(out _));return 0;});
    }
    /// <summary>CC11 complete reads preserve stable compatible scalar identities and quota accounting.</summary>
    [Fact] public void CC11_ScalarCacheUnchanged()
    {
        using var f=New(new(1,1,3,64,64));using var snapshot=Open(f);var c=Value(f);var scalar=Read(f,snapshot,c);var marker=XdmValue.SchemaTransportMarker(scalar);var empty=Value(f,count:0,list:true,fast:false);var list=Value(f,count:3,list:true,fast:false);
        f.Target.Run([snapshot],op=>{Assert.Empty(op.ReadTypedContainer(snapshot,empty).Value.AtomicItems);Assert.Equal(3,op.ReadTypedContainer(snapshot,list).Value.AtomicItems.Count());Assert.Same(marker,XdmValue.SchemaTransportMarker(op.ReadTypedValue(snapshot,c).Value));Assert.Equal(scalar,op.Evaluate(Context(scalar)));Assert.Equal(1,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>CC12 later metadata uses captured cells but every new complete read re-admits source contents.</summary>
    [Fact] public void CC12_NoGetterReplay()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;int reads=c.Reads.Count;c.Fail="governing";Assert.True(v.TryGetSingleAtomic(out var cell));Assert.NotNull(cell.TypeEvidence.ActualType);Assert.Equal(reads,c.Reads.Count);Assert.Throws<InvalidOperationException>(()=>op.ReadTypedContainer(snapshot,c));return 0;});
    }
    /// <summary>CC13 compatible cursor state changes and selection run in the same serialized window.</summary>
    [Fact] public void CC13_CursorProtocol()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,list:true);f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;Assert.True(v.IsList);Assert.True(v.TryGetSingleAtomic(out _));using var a=v.AtomicItems.GetEnumerator();using var b=v.AtomicItems.GetEnumerator();Assert.Throws<InvalidOperationException>(()=>a.Current);Assert.True(a.MoveNext());Assert.True(b.MoveNext());Assert.Equal(a.Current.Value,b.Current.Value);Assert.False(a.MoveNext());Assert.False(a.MoveNext());Assert.Throws<InvalidOperationException>(()=>a.Current);Assert.Throws<NotSupportedException>(()=>b.Reset());b.Dispose();Assert.Throws<ObjectDisposedException>(()=>b.MoveNext());return 0;});
    }
    /// <summary>CC14 a provider cannot reenter complete metadata observation in an active window.</summary>
    [Fact] public void CC14_Reentry()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;bool checkedOnce=false;f.TargetProvider.Hook=stage=>{if(stage!="available"||checkedOnce)return;checkedOnce=true;Assert.Throws<InvalidOperationException>(()=>v.GoverningType);};try{Assert.False(v.IsList);Assert.True(checkedOnce);}finally{f.TargetProvider.Hook=null;}return 0;});
    }
    /// <summary>CC15 overlapping cursor work refuses a second window without advancing the cursor.</summary>
    /// <returns>The serialized cursor completion.</returns>
    [Fact] public async Task CC15_Overlap()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();await f.Target.RunAsync([snapshot],async op=>{var v=op.ReadTypedContainer(snapshot,c).Value;using var cursor=v.AtomicItems.GetEnumerator();f.TargetProvider.Hook=stage=>{if(stage!="available")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(20)));};var first=Task.Run(()=>cursor.MoveNext());try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));Assert.Throws<InvalidOperationException>(()=>cursor.MoveNext());}finally{resume.Set();}Assert.True(await first);f.TargetProvider.Hook=null;Assert.Equal(7,cursor.Current.Value.IntegerValue);Assert.False(cursor.MoveNext());return 0;});
    }
    /// <summary>CC16 an actual complete getter tail retains all three file resources until raw drain.</summary>
    /// <returns>The completed provider tail.</returns>
    [Fact] public async Task CC16_ActualGetterTail()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);using var input=NewFile();using var source=NewFile();using var target=NewFile();f.Input.Resource=input;f.SourceProvider.Resource=source;f.TargetProvider.Resource=target;using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();Task<Exception?> getter=Task.FromResult<Exception?>(null);
        var pending=f.Target.RunAsync([snapshot],async op=>{var v=op.ReadTypedContainer(snapshot,c).Value;f.TargetProvider.Hook=stage=>{if(stage!="available")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(20)));Assert.True(input.CanWrite&&source.CanWrite&&target.CanWrite);};getter=Task.Run<Exception?>(()=>Record.Exception(()=>v.IsList));try{await getter;}finally{f.TargetProvider.Hook=null;}return 0;});
        try{Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));f.Source.Dispose();f.Target.Dispose();f.Input.Dispose();f.SourceProvider.Dispose();f.TargetProvider.Dispose();Assert.True(input.CanWrite&&source.CanWrite&&target.CanWrite);Assert.False(pending.IsCompleted);Assert.False(f.Target.DrainAsync().IsCompleted);resume.Set();await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await pending.WaitAsync(TimeSpan.FromSeconds(10)));Assert.IsType<ObjectDisposedException>(await getter);await f.Source.DrainAsync();await f.Target.DrainAsync();Assert.False(input.CanWrite||source.CanWrite||target.CanWrite);}finally{resume.Set();try{await pending.WaitAsync(TimeSpan.FromSeconds(10));}catch(ObjectDisposedException){}f.TargetProvider.Hook=null;}
    }
    /// <summary>CC17 two active raw operations retain independent complete views without latest-proof replacement.</summary>
    /// <returns>Completion of both operation episodes.</returns>
    [Fact] public async Task CC17_IndependentRawViews()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var firstReady=new TaskCompletionSource<ISchemaTypedValue>(TaskCreationOptions.RunContinuationsAsynchronously);var secondReady=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var finishFirst=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var firstEnded=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first=f.Target.RunAsync([snapshot],async op=>{firstReady.SetResult(op.ReadTypedContainer(snapshot,c).Value);await finishFirst.Task;return 0;});var second=f.Target.RunAsync([snapshot],async op=>{var v=op.ReadTypedContainer(snapshot,c).Value;secondReady.SetResult();await firstEnded.Task;int callbacks=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count;Assert.Throws<ObjectDisposedException>(()=>firstReady.Task.Result.IsList);Assert.Equal(callbacks,f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count);Assert.True(v.TryGetSingleAtomic(out _));return 0;});
        try{await Task.WhenAll(firstReady.Task,secondReady.Task).WaitAsync(TimeSpan.FromSeconds(10));finishFirst.SetResult();await first.WaitAsync(TimeSpan.FromSeconds(10));firstEnded.SetResult();await second.WaitAsync(TimeSpan.FromSeconds(10));}finally{finishFirst.TrySetResult();firstEnded.TrySetResult();await Task.WhenAll(first,second).WaitAsync(TimeSpan.FromSeconds(10));}Assert.Equal(0,snapshot.Runtime.TypedScalarCount);Assert.Equal(f.SourceProvider.Entries,f.SourceProvider.Releases);Assert.Equal(f.TargetProvider.Entries,f.TargetProvider.Releases);
    }
    /// <summary>CC18 later provider errors retain their original object.</summary>
    [Fact] public void CC18_FailureIdentity()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);var failure=new AggregateException(new Exception("provider"));f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;f.TargetProvider.Hook=stage=>{if(stage=="available")throw failure;};try{Assert.Same(failure,Assert.Throws<AggregateException>(()=>v.GoverningType));}finally{f.TargetProvider.Hook=null;}return 0;});
    }
    /// <summary>CC19 records complete compatible caller cost without adopting native or retained-memory budgets.</summary>
    [Fact] public void CC19_Cost()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,count:2,list:true,fast:false);
        f.Target.Run([snapshot],op=>
        {
            long bytes=GC.GetAllocatedBytesForCurrentThread(),start=Stopwatch.GetTimestamp();
            var v=op.ReadTypedContainer(snapshot,c).Value;
            long coldTicks=Stopwatch.GetTimestamp()-start,cold=GC.GetAllocatedBytesForCurrentThread()-bytes;
            const int runs=2;
            void Warm(){for(int i=0;i<runs;i++){_ = v.GoverningType;_ = v.IsList;Assert.Equal(2,v.AtomicItems.Count());}}
            int sourceBefore=f.SourceProvider.Calls.Count,targetBefore=f.TargetProvider.Calls.Count;Warm();
            // Reserve fixture tracing storage outside the warm allocation window; every callback is still recorded.
            int sourceDelta=f.SourceProvider.Calls.Count-sourceBefore,targetDelta=f.TargetProvider.Calls.Count-targetBefore;
            f.SourceProvider.Calls.EnsureCapacity(checked(f.SourceProvider.Calls.Count+sourceDelta));
            f.TargetProvider.Calls.EnsureCapacity(checked(f.TargetProvider.Calls.Count+targetDelta));
            bytes=GC.GetAllocatedBytesForCurrentThread();start=Stopwatch.GetTimestamp();Warm();
            long warmTicks=Stopwatch.GetTimestamp()-start,warm=GC.GetAllocatedBytesForCurrentThread()-bytes;
            output.WriteLine($"Compatible container cost: cold caller bytes {cold}; cold ticks {coldTicks}; warm reads {runs}; warm caller bytes {warm}; warm ticks {warmTicks}; frequency {Stopwatch.Frequency}");
            Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;
        });
    }
    /// <summary>CC20 engine delivery and foreign containers cannot mint provider issuance.</summary>
    [Fact] public void CC20_DeliveryNotIssuer()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f);f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(snapshot,v));var foreign=new Container{Fail="governing"};Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(snapshot,foreign));Assert.Empty(foreign.Reads);Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }

    private static void WideRead(Pair f,SchemaQuerySnapshot snapshot,XdmValue plain)
    {
        var c=Value(f,plain);var original=(Provenance)c.Items[0].TypeEvidence;
        f.Target.Run([snapshot],op=>{int proofs=f.TargetProvider.Establishes;var v=op.ReadTypedContainer(snapshot,c).Value;Assert.True(v.TryGetSingleAtomic(out var cell));AssertPrimitive(plain,cell.Value);Assert.Same(original.Declared,cell.TypeEvidence.DeclaredType);Assert.Same(original.Actual,cell.TypeEvidence.ActualType);Assert.Same(f.SourceProvider.Origin,cell.TypeEvidence.OriginalAssessment);Assert.Equal(proofs,f.TargetProvider.Establishes);Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }

    /// <summary>PW01 All eleven public factory kinds retain exact compatible representations.</summary>
    /// <param name="kind">Public factory selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)] public void PW01_Factories(int kind)
    {using var f=New();using var snapshot=Open(f);WideRead(f,snapshot,PrimitiveCase(kind));}

    /// <summary>PW02 Captured NaN, infinities and negative zero retain stored bits.</summary>
    /// <param name="example">Floating selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)] public void PW02_FloatBits(int example)
    {using var f=New();using var snapshot=Open(f);WideRead(f,snapshot,FloatCase(example));}

    /// <summary>PW03 QName prefix and NOTATION/user tags survive exact paired-window transport.</summary>
    /// <param name="example">QName selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void PW03_QNameFields(int example)
    {using var f=New();using var snapshot=Open(f);WideRead(f,snapshot,QNameCase(example));}

    /// <summary>PW06 Lexical durations are transported without canonicalization.</summary>
    /// <param name="example">Duration selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void PW06_Duration(int example)
    {using var f=New();using var snapshot=Open(f);WideRead(f,snapshot,XdmValue.FromDuration(example switch{0=>"P02D",1=>"P01Y02M",_=>"PT01.230S"},example==1?"yearMonthDuration":"dayTimeDuration","urn:user:d"));}

    /// <summary>PW04 malformed QName structure rejects the whole compatible batch.</summary>
    /// <param name="example">Malformed field selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void PW04_MalformedQName(int example)
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,count:2,list:true,fast:false);var evidence=c.Items[0].TypeEvidence;c.Items=[new(PrimitiveCase(4),evidence),new(BadQName(example),evidence)];f.Target.Run([snapshot],op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(snapshot,c));Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW05 temporal fields and independent timezone flags survive without DateTimeOffset conversion.</summary>
    /// <param name="kind">Temporal kind.</param><param name="year">Extended year.</param>
    [Theory][InlineData(0,0L)][InlineData(0,10000L)][InlineData(1,0L)][InlineData(1,10000L)][InlineData(2,0L)][InlineData(2,10000L)] public void PW05_Temporal(int kind,long year)
    {using var f=New();using var snapshot=Open(f);WideRead(f,snapshot,TemporalCase(kind,year));}
    /// <summary>PW07 null duration payloads are structural errors after a valid wider prefix.</summary>
    [Fact] public void PW07_NullDuration()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,count:2,list:true,fast:false);var evidence=c.Items[0].TypeEvidence;c.Items=[new(PrimitiveCase(4),evidence),new(XdmValue.FromDuration(null!),evidence)];f.Target.Run([snapshot],op=>{Assert.Throws<ArgumentException>(()=>op.ReadTypedContainer(snapshot,c));Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW08 an eleven-kind compatible union list retains order and each source selected type.</summary>
    [Fact] public void PW08_MixedEleven()
    {
        using var f=New(new(1,1,16,2048,2048));using var snapshot=Open(f);var c=Value(f,count:11,list:true,fast:false);var types=Enumerable.Range(0,11).Select(_=>new Handle()).ToArray();f.SourceProvider.Authority.Types.UnionWith(types);c.Items=Enumerable.Range(0,11).Select(i=>new SchemaAtomicItem(PrimitiveCase(i),new Provenance(c,c.Governing,types[i],f.SourceProvider.Origin))).ToArray();f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;Assert.True(v.IsList);var cells=v.AtomicItems.ToArray();Assert.Equal(11,cells.Length);for(int i=0;i<11;i++){AssertPrimitive(PrimitiveCase(i),cells[i].Value);Assert.Same(types[i],cells[i].TypeEvidence.ActualType);}Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW09 annotated URI/binary lexical Strings retain their original public representation.</summary>
    /// <param name="annotation">String type annotation.</param>
    [Theory][InlineData("anyURI")][InlineData("hexBinary")][InlineData("base64Binary")] public void PW09_AnnotatedString(string annotation)
    {using var f=New();using var snapshot=Open(f);WideRead(f,snapshot,XdmValue.FromString("AA==",annotation,"urn:user:lexical"));}
    /// <summary>PW10 immutable wider payloads escape while evidence expires before provider callbacks.</summary>
    /// <param name="kind">Additional primitive family.</param>
    [Theory][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)] public void PW10_Escape(int kind)
    {
        using var f=New();using var snapshot=Open(f);var plain=PrimitiveCase(kind);var c=Value(f,plain);var cell=f.Target.Run([snapshot],op=>{op.ReadTypedContainer(snapshot,c).Value.TryGetSingleAtomic(out var value);return value;});int callbacks=f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count;AssertPrimitive(plain,cell.Value);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.DeclaredType);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.ActualType);Assert.Throws<ObjectDisposedException>(()=>cell.TypeEvidence.OriginalAssessment);Assert.Equal(callbacks,f.SourceProvider.Calls.Count+f.TargetProvider.Calls.Count);
    }
    /// <summary>PW11 wider compatible list prefixes cannot bypass complete atom capacity.</summary>
    [Fact] public void PW11_MixedLimits()
    {
        using var f=New(new(1,1,2,64,64));using var snapshot=Open(f);var c=Value(f,count:3,list:true,fast:false);var evidence=c.Items[0].TypeEvidence;c.Items[0]=new(PrimitiveCase(10),evidence);c.ForbiddenCurrent=2;f.Target.Run([snapshot],op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedContainer(snapshot,c));Assert.Equal(2,c.Currents);Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW12 wider compatible cost retains all tracing callbacks without adopting native budgets.</summary>
    [Fact] public void PW12_Cost()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,PrimitiveCase(8));f.Target.Run([snapshot],op=>{long before=GC.GetAllocatedBytesForCurrentThread();var v=op.ReadTypedContainer(snapshot,c).Value;long cold=GC.GetAllocatedBytesForCurrentThread()-before;void Warm(){for(int i=0;i<2;i++){_ = v.IsList;v.TryGetSingleAtomic(out _);}}int a=f.SourceProvider.Calls.Count,b=f.TargetProvider.Calls.Count;Warm();int da=f.SourceProvider.Calls.Count-a,db=f.TargetProvider.Calls.Count-b;f.SourceProvider.Calls.EnsureCapacity(checked(f.SourceProvider.Calls.Count+da));f.TargetProvider.Calls.EnsureCapacity(checked(f.TargetProvider.Calls.Count+db));before=GC.GetAllocatedBytesForCurrentThread();Warm();long warm=GC.GetAllocatedBytesForCurrentThread()-before;output.WriteLine($"Compatible wider cost: cold caller bytes {cold}; warm reads 2; warm caller bytes {warm}");Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW13 wider complete transport does not broaden compatible scalar identity/evaluation.</summary>
    [Fact] public void PW13_ScalarProfileUnchanged()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,PrimitiveCase(4));f.Target.Run([snapshot],op=>{Assert.True(op.ReadTypedContainer(snapshot,c).Value.TryGetSingleAtomic(out var cell));Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(snapshot,c).Kind);Assert.Throws<ArgumentException>(()=>op.Evaluate(Context(cell.Value)));Assert.Equal(0,snapshot.Runtime.TypedScalarCount);return 0;});
    }
    /// <summary>PW14 wider cells still require current exact source type ownership on every metadata read.</summary>
    [Fact] public void PW14_CurrentOwnership()
    {
        using var f=New();using var snapshot=Open(f);var c=Value(f,PrimitiveCase(10));var original=(Provenance)c.Items[0].TypeEvidence;f.Target.Run([snapshot],op=>{var v=op.ReadTypedContainer(snapshot,c).Value;v.TryGetSingleAtomic(out var cell);f.SourceProvider.Authority.Types.Remove(original.Actual);try{Assert.Throws<ArgumentException>(()=>v.IsList);Assert.Throws<ArgumentException>(()=>cell.TypeEvidence.ActualType);}finally{f.SourceProvider.Authority.Types.Add(original.Actual);}return 0;});
    }
}
