// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies exact host binding, serialized callbacks and actual source raw lifetime.
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
//                      | Charles Korthout | 0.2 | 06-11-2026     | Verify exact structured host routing, staged diagnostics and final delivery (1.75.0). |
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies private host plumbing on real public session operations.</summary>
public sealed class CapturedHostBindingTests
{
    private sealed class Source:ISchemaSourceBinding;
    private sealed class Stamp:ISchemaSnapshotBinding;
    private sealed class Input:ISchemaAssessmentInput,IQueryOperationLifetime,IDisposable
    {
        internal readonly IXdmNode Node=new XDocumentNode(XDocument.Parse("<n>A</n>"));
        internal bool Available=true,All;internal int Roots,Owns,Checks,Entries,Releases,Active;internal FileStream? Resource;internal Action? OnRoot,OnOwn,OnCheck,OnRelease;
        private readonly Source _source=new();private readonly Stamp _stamp=new();
        IXdmNode ISchemaAssessmentInput.Root{get{Roots++;OnRoot?.Invoke();return Node;}}
        ISchemaSourceBinding ISchemaAssessmentInput.Source=>_source;ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot=>_stamp;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node){Owns++;OnOwn?.Invoke();return All||ReferenceEquals(node,Node);}
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken t){Checks++;t.ThrowIfCancellationRequested();OnCheck?.Invoke();ObjectDisposedException.ThrowIf(!Available,this);}
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken t){t.ThrowIfCancellationRequested();Entries++;Active++;return new Lease(this);}
        private sealed class Lease(Input owner):IQueryOperationLease
        {
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken t){t.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(!owner.Available,this);}
            /// <inheritdoc/>
            public void Dispose(){owner.Releases++;owner.Active--;if(!owner.Available&&owner.Active==0)owner.Resource?.Dispose();owner.OnRelease?.Invoke();}
        }
        /// <inheritdoc/>
        public void Dispose(){Available=false;if(Active==0)Resource?.Dispose();}
    }
    private sealed class Fixture:IDisposable
    {
        internal readonly object Executable=new();internal readonly CapturedHostBinding Binding;
        internal readonly Input Input=new();internal readonly SchemaQuerySnapshot Snapshot;
        internal Bosak.XPath.Api.Schema.SchemaQuerySession Session=>Binding.Session;
        internal Fixture(int bindings=16,int namespaces=16,SchemaQueryLimits? limits=null)
        {Binding=new(Executable,QueryHostLanguage.Xslt,new(ImmutableDictionary<string,string>.Empty,"captured-host-test"),limits??new(4,8,8,128,128),bindings,namespaces);Snapshot=Session.OpenSnapshot(Input,Input).Value;}
        internal T Call<T>(SchemaQueryOperation op,Func<CapturedHostBinding.Frame,T> producer,EvaluationContext? context=null,SchemaQuerySnapshot? match=null)
            =>Binding.Call(op,Executable,context,match,null,producer);
        /// <inheritdoc/>
        public void Dispose(){Session.Dispose();Input.Dispose();}
    }
    /// <summary>HC01/13 the engine binding carries the exact host descriptor and raw producer route.</summary>
    [Fact] public void HC01_ExactHostMetadata()
    {using var f=new Fixture();Assert.Equal(QueryHostLanguage.Xslt,f.Session.Environment.HostLanguage);Assert.Equal("3.0",f.Session.Environment.HostVersion);Assert.Equal(CapturedHostBinding.Profile,f.Session.Environment.BehaviorProfile);Assert.Equal(CapturedHostBinding.Descriptor,f.Session.Environment.DescriptorFormatVersion);f.Session.Run([f.Snapshot],op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(new()));return f.Call(op,frame=>{Assert.NotSame(frame.Context,new EvaluationContext());return 7;});});}
    /// <summary>HC02 foreign operations and executable identities reject before role getters.</summary>
    /// <param name="mode">Wrong authority identity.</param>
    [Theory][InlineData("operation")][InlineData("executable")]
    public void HC02_ForeignIdentity(string mode)
    {using var f=new Fixture();using var other=new Fixture();Assert.Equal(f.Session.Environment.StaticContextFingerprint,other.Session.Environment.StaticContextFingerprint);if(mode=="operation")Assert.Throws<ArgumentException>(()=>other.Session.Run([other.Snapshot],op=>f.Call(op,_=>1,match:f.Snapshot)));else Assert.Throws<ArgumentException>(()=>f.Session.Run([f.Snapshot],op=>f.Binding.Call(op,new object(),null,f.Snapshot,null,_=>1)));Assert.Equal(0,f.Input.Roots);}
    /// <summary>HC02 every source role must belong to the exact current raw membership.</summary>
    [Fact] public void HC02_ForeignRoleBeforeRoot()
    {using var f=new Fixture();using var other=new Fixture();Assert.Throws<ArgumentException>(()=>f.Session.Run([f.Snapshot],op=>f.Call(op,_=>1,match:other.Snapshot)));Assert.Equal(0,other.Input.Roots);Assert.Equal(0,f.Input.Roots);}
    /// <summary>HC03 dynamic nodes require exactly one owner before dispatch.</summary>
    /// <param name="ambiguous">Two sources claim a node instead of no source.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void HC03_ExactDynamicOwnership(bool ambiguous)
    {using var f=new Fixture();using var other=new Input();using var second=f.Session.OpenSnapshot(other,other).Value;f.Input.All=other.All=ambiguous;var context=new EvaluationContext().WithVariable("node",XdmValue.FromNode(ambiguous?f.Input.Node:new XDocumentNode(XDocument.Parse("<foreign/>"))));bool called=false;Assert.Throws<NotSupportedException>(()=>f.Session.Run([f.Snapshot,second],op=>f.Call(op,_=>{called=true;return 1;},context)));Assert.False(called);}
    /// <summary>HC04 shared and distinct roles retain unique source leases and select separate roots.</summary>
    [Fact] public void HC04_RolesAndLeaseUnits()
    {using var f=new Fixture();using var other=new Input();using var second=f.Session.OpenSnapshot(other,other).Value;f.Session.Run([f.Snapshot,second],op=>f.Binding.Call(op,f.Executable,null,f.Snapshot,second,frame=>{Assert.Same(f.Input.Node,frame.Match);Assert.Same(other.Node,frame.Global);return 0;}));Assert.Equal(1,f.Input.Entries);Assert.Equal(1,other.Entries);f.Session.Run([f.Snapshot],op=>f.Binding.Call(op,f.Executable,null,f.Snapshot,f.Snapshot,frame=>{Assert.Same(frame.Match,frame.Global);return 0;}));Assert.Equal(1,f.Input.Entries);}
    /// <summary>HC05 source-free raw work accepts explicit empty membership.</summary>
    [Fact] public void HC05_EmptyMembership()
    {using var f=new Fixture();Assert.Equal(7,f.Session.Run([],op=>f.Call(op,frame=>{Assert.Null(frame.Match);Assert.Null(frame.Global);return 7;})));}
    /// <summary>HC06 copied data context is separate and external mutable services refuse.</summary>
    /// <param name="mode">Rejected context state.</param>
    [Theory][InlineData("loader")][InlineData("function")][InlineData("structured")]
    public void HC06_ContextRefusal(string mode)
    {using var f=new Fixture();var context=new EvaluationContext();if(mode=="loader")context.DocumentLoader=_=>throw new Exception("loader poison");else if(mode=="function")context.LazyVariableResolver=(_,_)=>throw new Exception("resolver poison");else context.WithVariable("m",XdmValue.FromMap(new XdmMap()));Assert.Throws<NotSupportedException>(()=>f.Session.Run([f.Snapshot],op=>f.Call(op,_=>1,context)));}
    /// <summary>HC06 finite internal context bounds refuse before the producer.</summary>
    /// <param name="mode">Bound exceeded.</param>
    [Theory][InlineData("bindings")][InlineData("namespaces")]
    public void HC06_BoundedCapture(string mode)
    {using var f=new Fixture(bindings:1,namespaces:mode=="namespaces"?1:16);var c=new EvaluationContext().WithVariable("a",XdmValue.FromString("a"));if(mode=="bindings")c.WithVariable("b",XdmValue.FromString("b"));Assert.Throws<NotSupportedException>(()=>f.Session.Run([f.Snapshot],op=>f.Call(op,_=>1,c)));}
    /// <summary>HC07 closure or cancellation during a producer denies delivery.</summary>
    /// <param name="mode">Revocation source.</param>
    [Theory][InlineData("session")][InlineData("snapshot")][InlineData("input")][InlineData("cancel")]
    public void HC07_Revocation(string mode)
    {using var f=new Fixture();using var cancel=new CancellationTokenSource();var e=Record.Exception(()=>f.Session.Run([f.Snapshot],op=>f.Call(op,_=>{if(mode=="session")f.Session.Dispose();else if(mode=="snapshot")f.Snapshot.Dispose();else if(mode=="input")f.Input.Dispose();else cancel.Cancel();return 1;}),cancel.Token));if(mode=="cancel")Assert.IsAssignableFrom<OperationCanceledException>(e);else Assert.IsType<ObjectDisposedException>(e);}
    /// <summary>HC08/10 raw completion waits for an actual escaped host tail and retains the file through close.</summary>
    /// <returns>A task completing after all real host work and cleanup.</returns>
    [Fact] public async Task HC08_ActualTailAndDrain()
    {
        using var f=new Fixture();using var file=new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);f.Input.Resource=file;
        using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();Task<int>? host=null;
        var raw=f.Session.RunAsync([f.Snapshot],op=>{host=Task.Run(()=>f.Call(op,frame=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(15)));Assert.True(file.CanWrite);file.WriteByte(7);return 1;}));Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));return Task.FromResult(7);});
        try{Assert.False(raw.IsCompleted);f.Session.Dispose();f.Input.Dispose();Assert.True(file.CanWrite);Assert.False(f.Session.DrainAsync().IsCompleted);using var c=new CancellationTokenSource();c.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>f.Session.DrainAsync(c.Token));Assert.False(raw.IsCompleted);}
        finally{resume.Set();}
        await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await host!);await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await raw);await f.Session.DrainAsync();Assert.False(file.CanWrite);Assert.Equal(1,f.Input.Releases);
    }
    /// <summary>HC09 original work/cleanup exception objects and provider aggregate boundaries remain ordered.</summary>
    [Fact] public void HC09_FailureOrdering()
    {using var f=new Fixture();var primary=new ApplicationException("host");var cleanup=new AggregateException(new Exception("cleanup"));f.Input.OnRelease=()=>throw cleanup;var error=Assert.Throws<AggregateException>(()=>f.Session.Run<int>([f.Snapshot],op=>f.Call<int>(op,_=>{f.Snapshot.Dispose();throw primary;})));Assert.Equal(new Exception[]{primary,cleanup},error.InnerExceptions);Assert.Equal(1,f.Input.Releases);}
    /// <summary>HC10 escaped frame and operation cannot replay callbacks after window/raw completion.</summary>
    [Fact] public void HC10_ExpiredWindows()
    {using var f=new Fixture();CapturedHostBinding.Frame? escaped=null;SchemaQueryOperation? operation=null;f.Session.Run([f.Snapshot],op=>{operation=op;f.Call(op,frame=>{escaped=frame;return 0;});int before=f.Input.Checks;Assert.Throws<ObjectDisposedException>(()=>escaped!.Check());Assert.Equal(before,f.Input.Checks);return 0;});int checks=f.Input.Checks;Assert.Throws<ObjectDisposedException>(()=>f.Call(operation!,_=>1));Assert.Equal(checks,f.Input.Checks);}
    /// <summary>HC12 overlapping and reentrant calls cannot enter a second host producer.</summary>
    [Fact] public void HC12_Reentry()
    {using var f=new Fixture();f.Session.Run([f.Snapshot],op=>f.Call(op,_=>{Assert.Throws<InvalidOperationException>(()=>f.Call(op,_=>1));return 0;}));}
    /// <summary>HC12 independent operations can execute host calls concurrently.</summary>
    /// <returns>A task completing after both independent operations.</returns>
    [Fact] public async Task HC12_IndependentOperations()
    {using var f=new Fixture();using var entered=new CountdownEvent(2);using var resume=new ManualResetEventSlim();int Run()=>f.Session.Run([f.Snapshot],op=>f.Call(op,_=>{entered.Signal();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));return 1;}));var a=Task.Run(Run);var b=Task.Run(Run);try{Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));}finally{resume.Set();}Assert.Equal(new[]{1,1},await Task.WhenAll(a,b));}
    /// <summary>HC13 frozen descriptor includes both context bounds and all session limits.</summary>
    [Fact] public void HC13_Descriptors()
    {using var f=new Fixture();using var bound=new Fixture(bindings:17);using var names=new Fixture(namespaces:17);Assert.NotEqual(f.Session.Environment.StaticContextFingerprint,bound.Session.Environment.StaticContextFingerprint);Assert.NotEqual(f.Session.Environment.StaticContextFingerprint,names.Session.Environment.StaticContextFingerprint);foreach(var l in new[]{new SchemaQueryLimits(5,8,8,128,128),new(4,9,8,128,128),new(4,8,9,128,128),new(4,8,8,129,128),new(4,8,8,128,127)}){using var other=new Fixture(limits:l);Assert.NotEqual(f.Session.Environment.StaticContextFingerprint,other.Session.Environment.StaticContextFingerprint);}}
    /// <summary>HC07 input callbacks that close the session cannot deliver root/context data.</summary>
    /// <param name="stage">Input callback boundary.</param>
    [Theory][InlineData("root")][InlineData("ownership")][InlineData("availability")]
    public void HC07_InputCallbackClosure(string stage)
    {using var f=new Fixture();Assert.Throws<ObjectDisposedException>(()=>f.Session.Run([f.Snapshot],op=>{if(stage=="root")f.Input.OnRoot=f.Session.Dispose;else if(stage=="ownership")f.Input.OnOwn=f.Session.Dispose;else f.Input.OnCheck=f.Session.Dispose;return f.Call(op,_=>1,match:f.Snapshot);}));Assert.Equal(1,f.Input.Releases);}
    /// <summary>HC12 host and optional provider modes cannot expand each other's exact membership.</summary>
    /// <param name="hostFirst">Host execution selected before provider execution.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void HC12_ModeConfinement(bool hostFirst)
    {using var f=new Fixture();f.Session.Run([f.Snapshot],op=>{var raw=op.RequireHost(f.Binding);if(hostFirst){f.Call(op,_=>1);Assert.Throws<ArgumentException>(()=>raw.CallProvider(f.Input,_=>1));}else{raw.CallProvider(f.Input,_=>1);Assert.Throws<ArgumentException>(()=>f.Call(op,_=>1));}return 0;});Assert.Equal(hostFirst?1:2,f.Input.Entries);}
    /// <summary>HC06 captured dynamic bindings are copied before producer mutation of caller input.</summary>
    [Fact] public void HC06_CopiedDynamicData()
    {using var f=new Fixture();var caller=new EvaluationContext().WithVariable("v",XdmValue.FromString("before"));Assert.Equal("before",f.Session.Run([],op=>f.Call(op,frame=>{caller.WithVariable("v",XdmValue.FromString("after"));Assert.True(frame.Context.TryGetVariable("v",out var value));return value.StringValue;},caller)));}
    /// <summary>HC02 reusing a binding in a second internal session cannot mint executable ownership.</summary>
    [Fact] public void HC02_BindingDoesNotMintSessionOwnership()
    {using var f=new Fixture();using var forged=Bosak.XPath.Api.Schema.SchemaQuerySession.CreateHostCandidate(f.Binding,new(4,8,8,128,128));Assert.Throws<ArgumentException>(()=>forged.Run([],op=>f.Call(op,_=>1)));}
    /// <summary>HC09 multiple input cleanups preserve reverse order and nested exception objects.</summary>
    [Fact] public void HC09_MultipleCleanupFailures()
    {using var f=new Fixture();using var other=new Input();using var second=f.Session.OpenSnapshot(other,other).Value;var work=new ApplicationException("work");var first=new AggregateException(new Exception("first"));var last=new ApplicationException("last");f.Input.OnRelease=()=>throw last;other.OnRelease=()=>throw first;var error=Assert.Throws<AggregateException>(()=>f.Session.Run<int>([f.Snapshot,second],op=>f.Call<int>(op,_=>{f.Snapshot.Dispose();second.Dispose();throw work;})));Assert.Equal(new Exception[]{work,first,last},error.InnerExceptions);}
    /// <summary>HC12 simultaneous calls in the same operation fail before entering another producer.</summary>
    /// <returns>A task completing after the serialized raw operation.</returns>
    [Fact] public async Task HC12_Overlap()
    {using var f=new Fixture();using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();await f.Session.RunAsync([f.Snapshot],async op=>{var first=Task.Run(()=>f.Call(op,_=>{entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));return 1;}));try{Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));Assert.Throws<InvalidOperationException>(()=>f.Call(op,_=>1));}finally{resume.Set();}Assert.Equal(1,await first);return 0;});}

    /// <summary>HR01 exact session/operation ownership delivers a successful internal host result.</summary>
    [Fact] public void HR01_ExactOutcomeRoute()
    {
        using var f = new Fixture(); var value = new object(); SchemaQueryOperation? retained = null;
        var result = f.Binding.RunOutcome<object>(f.Executable,[f.Snapshot],(operation,stage) =>
        { retained = operation; return f.Call(operation,frame => { Assert.Same(f.Input.Node,frame.Match); stage(value); return value; },match:f.Snapshot); });
        Assert.True(result.IsSuccess); Assert.True(result.HasResult); Assert.Same(value,result.Result);
        Assert.Null(result.AdmissionError); Assert.Null(result.WorkError); Assert.Empty(result.CleanupErrors);
        Assert.Throws<ObjectDisposedException>(() => f.Call(retained!,_ => value));
    }

    /// <summary>HR02 wrong executable or internal session refuses before source callbacks.</summary>
    /// <param name="mode">The invalid engine-owned identity.</param>
    [Theory][InlineData("executable")][InlineData("session")][InlineData("binding")]
    public void HR02_ExactEngineIdentity(string mode)
    {
        using var f = new Fixture(); using var other = new Fixture();
        using var forged = Bosak.XPath.Api.Schema.SchemaQuerySession.CreateHostCandidate(f.Binding,new(4,8,8,128,128));
        int checks = f.Input.Checks, roots = f.Input.Roots;
        Assert.Throws<ArgumentException>(() =>
        {
            if (mode=="executable") f.Binding.RunOutcome<object>(new object(),[f.Snapshot],(_,_) => throw new Exception("producer poison"));
            else if(mode=="session") forged.RunHostOutcome<object>(f.Binding,[f.Snapshot],_ => throw new Exception("producer poison"));
            else f.Session.RunHostOutcome<object>(other.Binding,[f.Snapshot],_ => throw new Exception("producer poison"));
        });
        Assert.Equal(checks,f.Input.Checks); Assert.Equal(roots,f.Input.Roots);
    }

    /// <summary>HR03 invalid snapshot membership is an admission report before producer/root access.</summary>
    /// <param name="mode">Invalid membership shape.</param>
    [Theory][InlineData("default")][InlineData("null")][InlineData("duplicate")][InlineData("foreign")]
    public void HR03_MembershipReport(string mode)
    {
        using var f = new Fixture(); using var other = new Fixture();
        ImmutableArray<SchemaQuerySnapshot> members = mode switch
        { "default" => default, "null" => [null!], "duplicate" => [f.Snapshot,f.Snapshot], _ => [other.Snapshot] };
        int roots = f.Input.Roots;
        var result = f.Binding.RunOutcome<object>(f.Executable,members,(_,_) => throw new Exception("producer poison"));
        Assert.False(result.HasResult); Assert.False(result.IsSuccess); Assert.Null(result.WorkError);
        Assert.IsAssignableFrom<ArgumentException>(result.AdmissionError); Assert.Empty(result.CleanupErrors);
        Assert.Equal(roots,f.Input.Roots);
    }

    /// <summary>HR04 pre-cancellation and closure are admission errors without producer execution.</summary>
    /// <param name="cancel">Whether to cancel rather than close the session.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void HR04_AdmissionRevocation(bool cancel)
    {
        using var f = new Fixture(); using var cancellation = new CancellationTokenSource();
        if(cancel)cancellation.Cancel();else f.Session.Dispose();
        var result = f.Binding.RunOutcome<object>(f.Executable,[],(_,_) => throw new Exception("producer poison"),cancellation.Token);
        Assert.False(result.IsSuccess); Assert.False(result.HasResult); Assert.Null(result.WorkError); Assert.Empty(result.CleanupErrors);
        if(cancel)Assert.IsAssignableFrom<OperationCanceledException>(result.AdmissionError);
        else Assert.IsType<ObjectDisposedException>(result.AdmissionError);
    }

    /// <summary>HR05 staged diagnostics survive final host-frame refusal and cannot imply success.</summary>
    /// <param name="mode">Revocation source after producing detached diagnostics.</param>
    [Theory][InlineData("session")][InlineData("snapshot")][InlineData("input")][InlineData("cancel")]
    public void HR05_StagedResultAndFinalRefusal(string mode)
    {
        using var f = new Fixture(); using var cancellation = new CancellationTokenSource(); var value = new object();
        var result = f.Binding.RunOutcome<object>(f.Executable,[f.Snapshot],(op,stage) => f.Call(op,_ =>
        {
            stage(value);
            if(mode=="session")f.Session.Dispose();else if(mode=="snapshot")f.Snapshot.Dispose();
            else if(mode=="input")f.Input.Dispose();else cancellation.Cancel();
            return value;
        }),cancellation.Token);
        Assert.True(result.HasResult); Assert.Same(value,result.Result); Assert.False(result.IsSuccess); Assert.Null(result.AdmissionError);
        if(mode=="cancel")Assert.IsAssignableFrom<OperationCanceledException>(result.WorkError);
        else Assert.IsType<ObjectDisposedException>(result.WorkError);
    }

    /// <summary>HR06 staging errors preserve the first diagnostic and fail the work.</summary>
    /// <param name="mode">Repeated, null or mismatched staged value.</param>
    [Theory][InlineData("repeat")][InlineData("null")][InlineData("different")]
    public void HR06_StagingDiscipline(string mode)
    {
        using var f = new Fixture(); var first = new object();
        var result = f.Binding.RunOutcome<object>(f.Executable,[],(op,stage) => f.Call(op,_ =>
        {
            if(mode=="null"){stage(null!);return first;}
            stage(first); if(mode=="repeat")stage(first); return new object();
        }));
        Assert.False(result.IsSuccess); Assert.Null(result.AdmissionError);
        if(mode=="null"){Assert.False(result.HasResult);Assert.IsType<ArgumentNullException>(result.WorkError);}
        else{Assert.True(result.HasResult);Assert.Same(first,result.Result);Assert.IsType<InvalidOperationException>(result.WorkError);}
    }

    /// <summary>HR07 a staging callback cannot mutate a report after its producer returns.</summary>
    [Fact] public void HR07_ExpiredStageCallback()
    {
        using var f = new Fixture(); Action<object>? escaped = null; var value = new object();
        var result = f.Binding.RunOutcome<object>(f.Executable,[],(_,stage) => { escaped=stage;return value; });
        Assert.True(result.IsSuccess); int checks = f.Input.Checks;
        Assert.Throws<ObjectDisposedException>(() => escaped!(new object()));
        Assert.Same(value,result.Result); Assert.Equal(checks,f.Input.Checks);
    }

    /// <summary>HR08 work and reverse cleanup failures retain every original exception object.</summary>
    [Fact] public void HR08_WorkAndCleanupIdentity()
    {
        using var f = new Fixture(); using var second = new Input(); using var snapshot = f.Session.OpenSnapshot(second,second).Value;
        var work = new AggregateException(new Exception("work")); var cleanup = new AggregateException(new Exception("cleanup"));
        f.Input.OnRelease = () => throw cleanup; second.OnRelease = () => throw cleanup; var value = new object();
        var result = f.Binding.RunOutcome<object>(f.Executable,[f.Snapshot,snapshot],(op,stage) => f.Call<object>(op,_ =>
        { stage(value); f.Snapshot.Dispose(); snapshot.Dispose(); throw work; }));
        Assert.Same(value,result.Result); Assert.True(result.HasResult); Assert.False(result.IsSuccess);
        Assert.Same(work,result.WorkError); Assert.Null(result.AdmissionError); Assert.Equal(2,result.CleanupErrors.Length);
        Assert.All(result.CleanupErrors,error => Assert.Same(cleanup,error)); Assert.Single(cleanup.InnerExceptions);
    }

    /// <summary>HR09 closure during provider cleanup fails the post-cleanup engine delivery guard.</summary>
    [Fact] public void HR09_PostCleanupDelivery()
    {
        using var f = new Fixture(); var value = new object();
        f.Input.OnRelease = f.Session.Dispose;
        var result = f.Binding.RunOutcome<object>(f.Executable,[],(op,_) =>
        { op.RequireHost(f.Binding).CallProvider(f.Input,_ => 1);return value; });
        Assert.True(result.HasResult); Assert.Same(value,result.Result); Assert.False(result.IsSuccess);
        Assert.IsType<ObjectDisposedException>(result.WorkError); Assert.Null(result.AdmissionError); Assert.Empty(result.CleanupErrors);
    }

    /// <summary>HR10 a cleanup error alone keeps its own category without an invented work error.</summary>
    [Fact] public void HR10_CleanupOnly()
    {
        using var f = new Fixture(); var cleanup = new AggregateException(new Exception("cleanup")); var value = new object();
        f.Input.OnRelease = () => throw cleanup;
        var result = f.Binding.RunOutcome<object>(f.Executable,[],(op,_) =>
        { op.RequireHost(f.Binding).CallProvider(f.Input,_ => 1);return value; });
        Assert.True(result.HasResult); Assert.Same(value,result.Result); Assert.False(result.IsSuccess);
        Assert.Null(result.WorkError); Assert.Null(result.AdmissionError); Assert.Same(cleanup,Assert.Single(result.CleanupErrors));
        f.Input.OnRelease = null;
    }

    /// <summary>HR11 producer failure without staging cannot create a successful result.</summary>
    [Fact] public void HR11_NoFabricatedResult()
    {
        using var f = new Fixture(); var work = new AggregateException(new Exception("work"));
        var result = f.Binding.RunOutcome<object>(f.Executable,[],(_,_) => throw work);
        Assert.False(result.HasResult); Assert.False(result.IsSuccess); Assert.Same(work,result.WorkError);
        Assert.Null(result.AdmissionError); Assert.Empty(result.CleanupErrors);
    }

    /// <summary>HR12 the API-owned report waits for an actual host tail and real source cleanup.</summary>
    /// <returns>The completed lifetime and detached diagnostic check.</returns>
    [Fact] public async Task HR12_ActualTailAndCanceledWait()
    {
        using var f = new Fixture();
        using var file = new FileStream(Path.GetTempFileName(),FileMode.Open,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        f.Input.Resource=file;
        using var entered = new ManualResetEventSlim(); using var resume = new ManualResetEventSlim();
        var value = new object(); Task<Exception?>? tail = null;
        var resultTask = Task.Run(() => f.Binding.RunOutcome<object>(f.Executable,[f.Snapshot],(op,_) =>
        {
            tail = Task.Run(() => Record.Exception(() => f.Call(op,_ =>
            { entered.Set(); Assert.True(resume.Wait(TimeSpan.FromSeconds(20))); Assert.True(file.CanWrite); file.WriteByte(42); return 1; })));
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10))); return value;
        }));
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));
            f.Session.Dispose(); f.Input.Dispose();
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => resultTask.WaitAsync(cancellation.Token));
            Assert.False(resultTask.IsCompleted); Assert.True(file.CanWrite); Assert.Equal(0,f.Input.Releases);
            Assert.False(f.Session.DrainAsync().IsCompleted);
            resume.Set();
            var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(result.HasResult); Assert.Same(value,result.Result); Assert.False(result.IsSuccess);
            Assert.IsType<ObjectDisposedException>(result.WorkError); Assert.Null(result.AdmissionError);
            Assert.IsType<ObjectDisposedException>(await tail!.WaitAsync(TimeSpan.FromSeconds(10)));
            await f.Session.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(file.CanWrite); Assert.Equal(1,f.Input.Releases);
        }
        finally { resume.Set(); await resultTask.WaitAsync(TimeSpan.FromSeconds(10)); if(tail is not null)await tail.WaitAsync(TimeSpan.FromSeconds(10)); }
    }
}
