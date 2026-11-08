// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Verifies ordinary source admission and shared raw-operation resource ownership.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; W1-A neutral admission (1.43.0)                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 03-11-2026     | Verify open-session drain epochs, cancellation and rollback (1.44.2).                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Exercises neutral admission, before-getter authentication and actual raw lifetime ownership.</summary>
public sealed class OrdinarySourceAdmissionTests
{
    private sealed class Lease(Action release) : IDisposable
    {
        /// <summary>Releases the injected owner obligation.</summary>
        public void Dispose() => release();
    }
    private sealed class Input : IQuerySourceInput
    {
        private readonly HashSet<Node> _nodes = new(ReferenceEqualityComparer.Instance);
        private readonly Node _root;
        internal bool Available = true, Poison, ClaimAll;
        internal int RootReads, GetterReads, Entries, Releases, Checks;
        internal Action? EnterAction, CheckAction, ReleaseAction;
        internal Input() => _root = Project(new XDocumentNode(XDocument.Parse("<n a='x'>42</n>")));
        internal IXdmNode Candidate => _root;
        private Node Project(IXdmNode node) { var projection = new Node(this, node); _nodes.Add(projection); return projection; }
        IXdmNode IQuerySourceInput.Root { get { RootReads++; ThrowIfUnavailable(); return _root; } }
        /// <summary>Checks the trusted fixture owner before source access.</summary>
        public void ThrowIfUnavailable() { Checks++; CheckAction?.Invoke(); ObjectDisposedException.ThrowIf(!Available, this); }
        bool IQuerySourceInput.OwnsNode(IXdmNode node)
        { ThrowIfUnavailable(); return ClaimAll || node is Node known && ReferenceEquals(known.Owner, this) && _nodes.Contains(known); }
        internal IDisposable Enter() { Entries++; EnterAction?.Invoke(); return new Lease(() => { Releases++; ReleaseAction?.Invoke(); }); }
        private sealed class Node(Input owner, IXdmNode raw) : IXdmNode
        {
            internal Input Owner => owner;
            private void Check() { owner.GetterReads++; if (owner.Poison) throw new InvalidOperationException("Foreign getter was touched."); owner.ThrowIfUnavailable(); }
            XdmNodeKind IXdmNode.NodeKind { get { Check(); return raw.NodeKind; } }
            string IXdmNode.LocalName { get { Check(); return raw.LocalName; } }
            string IXdmNode.NamespaceUri { get { Check(); return raw.NamespaceUri; } }
            string IXdmNode.Prefix { get { Check(); return raw.Prefix; } }
            string IXdmNode.StringValue { get { Check(); return raw.StringValue; } }
            XdmValue IXdmNode.TypedValue { get { Check(); return raw.TypedValue; } }
            (string NamespaceUri, string LocalName)? IXdmNode.SchemaTypeAnnotation => ("urn:descriptive-only", "not-authority");
            IXdmNode? IXdmNode.Parent { get { Check(); return raw.Parent is { } parent ? owner.Project(parent) : null; } }
            IXdmNode? IXdmNode.Document { get { Check(); return raw.Document is { } document ? owner.Project(document) : null; } }
            XdmSequence IXdmNode.Children(XdmNodeKind kind) { Check(); return Wrap(raw.Children(kind)); }
            XdmSequence IXdmNode.Attributes(string? localName, string? namespaceUri) { Check(); return Wrap(raw.Attributes(localName, namespaceUri)); }
            XdmSequence IXdmNode.Axis(XdmAxis axis) { Check(); return Wrap(raw.Axis(axis)); }
            bool IXdmNode.IsSameNode(IXdmNode other) { Check(); return other is Node n && ReferenceEquals(n.Owner, owner) && raw.IsSameNode(n.Raw); }
            private IXdmNode Raw => raw;
            long IXdmNode.DocumentOrder { get { Check(); return raw.DocumentOrder; } }
            string IXdmNode.BaseUri { get { Check(); return raw.BaseUri; } }
            string IXdmNode.DocumentUri { get { Check(); return raw.DocumentUri; } }
            string IXdmNode.ToXmlString() { Check(); return raw.ToXmlString(); }
            private XdmSequence Wrap(XdmSequence values) => XdmSequence.FromSource(new Projections(owner, values));
        }
        private sealed class Projections(Input owner, XdmSequence values) : IXdmSequence
        {
            bool IXdmSequence.TryGetLength(out int length) => values.TryGetLength(out length);
            IXdmSequenceEnumerator IXdmSequence.GetEnumerator() => new Enumerator(owner, values.GetEnumerator());
            private sealed class Enumerator(Input owner, XdmSequence.Enumerator inner) : IXdmSequenceEnumerator
            {
                private XdmSequence.Enumerator _inner = inner;
                XdmValue IXdmSequenceEnumerator.Current => XdmValue.FromNode(owner.Project(_inner.Current.NodeValue!));
                bool IXdmSequenceEnumerator.MoveNext() { owner.ThrowIfUnavailable(); return _inner.MoveNext(); }
            }
        }
    }
    private static QuerySourceSession Compile(string expression = "string(.)", int limit = 8)
    {
        Assert.True(SchemaSessionCompilation.TryCompileOrdinary(expression, new CompileOptions(), limit, out var session, out var reason), reason);
        return session!;
    }
    private static QuerySourceSession.Snapshot Open(QuerySourceSession s, Input i) => s.OpenSnapshot(s.Environment, i, i.Enter);
    private static string Evaluate(QuerySourceSession.Operation op, IXdmNode node)
    {
        var inputs = op.CaptureInputs(new EvaluationContext().WithFocus(XdmValue.FromNode(node), 1, 1), 16, 16);
        return op.EvaluateAtomic(inputs).StringValue;
    }
    /// <summary>Evaluates a compiled XPath with no issuer or assessment and no eager graph reads.</summary>
    [Fact] public void W1_01_OrdinaryWithoutSchema()
    {
        using var s = Compile(); var i = new Input(); var a = Open(s, i);
        Assert.Equal(0, i.RootReads); Assert.Equal(0, i.GetterReads);
        Assert.Equal("42", s.Run(s.Environment, [a], op => Evaluate(op, op.GetRoot(a))));
        Assert.Equal(0, i.Releases); a.Dispose(); Assert.Equal(1, i.Releases);
    }
    /// <summary>Refuses an equal-looking foreign compilation before owner entry or getters.</summary>
    [Fact] public void W1_02_ForeignEnvironment()
    {
        using var s = Compile(); var i = new Input(); var foreign = new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty, "same");
        Assert.Throws<ArgumentException>(() => s.OpenSnapshot(foreign, i, i.Enter)); Assert.Equal(0, i.Entries); Assert.Equal(0, i.GetterReads);
    }
    /// <summary>Refuses original evidence without an explicit advisor before provider callbacks.</summary>
    [Fact] public void W1_03_OriginWithoutAdvisor()
    { using var s=Compile();var i=new Input();Assert.Throws<NotSupportedException>(()=>s.OpenSnapshot(s.Environment,i,i.Enter,new object()));Assert.Equal(0,i.Entries); }
    /// <summary>A failing owner entry transfers no release obligation and reads no root.</summary>
    [Fact] public void W1_04_OwnerEntryFailure()
    { using var s=Compile();var error=new Exception("entry");var i=new Input{EnterAction=()=>throw error};Assert.Same(error,Assert.Throws<Exception>(()=>Open(s,i)));Assert.Equal(0,i.RootReads);Assert.Equal(0,i.Releases); }
    /// <summary>Unavailable input rolls back only its acquired owner lease without reading its root.</summary>
    [Fact] public void W1_05_AvailabilityBeforeRoot()
    { using var s=Compile();var i=new Input{Available=false};Assert.Throws<ObjectDisposedException>(()=>Open(s,i));Assert.Equal(1,i.Releases);Assert.Equal(0,i.RootReads); }
    /// <summary>Foreign poisoned projections refuse before any node getter.</summary>
    [Fact] public void W1_06_ForeignPoisonNode()
    { using var s=Compile();var i=new Input();var f=new Input{Poison=true};var a=Open(s,i);s.Run(s.Environment,[a],op=>{Assert.Throws<NotSupportedException>(()=>Evaluate(op,f.Candidate));return 0;});Assert.Equal(0,f.GetterReads); }
    /// <summary>Overlapping trusted test predicates refuse ambiguous nodes before dispatch or getters.</summary>
    [Fact] public void W1_07_AmbiguousPoisonNode()
    { using var s=Compile();var i=new Input{ClaimAll=true};var j=new Input{ClaimAll=true};var foreign=new Input{Poison=true};var a=Open(s,i);var b=Open(s,j);s.Run(s.Environment,[a,b],op=>{Assert.Throws<NotSupportedException>(()=>Evaluate(op,foreign.Candidate));return 0;});Assert.Equal(0,foreign.GetterReads); }
    /// <summary>Descriptive annotations do not become attached original evidence.</summary>
    [Fact] public void W1_08_AnnotationsAreNotAuthority()
    { using var s=Compile();var i=new Input();var a=Open(s,i);Assert.NotNull(i.Candidate.SchemaTypeAnnotation);var value=s.Run(s.Environment,[a],op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext().WithFocus(XdmValue.FromNode(i.Candidate),1,1),16,16)));Assert.Null(XdmValue.SchemaTransportMarker(value)); }
    /// <summary>Captured inputs cannot transfer between operations with identical member lists.</summary>
    [Fact] public void W1_11_ExactOperationToken()
    { using var s=Compile();var i=new Input();var a=Open(s,i);var input=s.Run(s.Environment,[a],op=>op.CaptureInputs(new EvaluationContext(),16,16));s.Run(s.Environment,[a],op=>{Assert.Throws<ArgumentException>(()=>op.EvaluateAtomic(input));return 0;}); }
    /// <summary>Duplicate admissions refuse without work or new provider callbacks.</summary>
    [Fact] public void W1_12_DuplicateMembership()
    { using var s=Compile();var i=new Input();var a=Open(s,i);int checks=i.Checks;Assert.Throws<ArgumentException>(()=>s.Run<int>(s.Environment,[a,a],_=>throw new Exception("work")));Assert.Equal(checks,i.Checks); }
    /// <summary>A late second-member failure rolls back acquired raw leases and preserves host admissions.</summary>
    [Fact] public void W1_13_PartialAcquisition()
    {
        using var s=Compile();var i=new Input();var j=new Input();var a=Open(s,i);var b=Open(s,j);var primary=new Exception("second check");j.CheckAction=()=>throw primary;
        Assert.Same(primary,Assert.Throws<Exception>(()=>s.Run(s.Environment,[a,b],_=>0)));Assert.Equal(0,i.Releases);a.Dispose();Assert.Equal(1,i.Releases);j.CheckAction=null;
    }
    /// <summary>Primary and all cleanup failures retain object identity, aggregate boundaries and reverse order.</summary>
    /// <returns>The asynchronous raw-work and drain verification.</returns>
    [Fact] public async Task W1_14_PrimaryAndEveryCleanup()
    {
        var s=Compile();var one=new Exception("one");var two=new AggregateException(new Exception("two"));var primary=new Exception("work");
        var i=new Input{ReleaseAction=()=>throw one};var j=new Input{ReleaseAction=()=>throw two};var a=Open(s,i);var b=Open(s,j);
        var error=Assert.Throws<AggregateException>(()=>s.Run<int>(s.Environment,[a,b],_=>{s.Dispose();throw primary;}));
        Assert.Equal(new Exception[]{primary,two,one},error.InnerExceptions);Assert.Same(two,error.InnerExceptions[1]);
        var drain=s.DrainAsync();await Assert.ThrowsAnyAsync<Exception>(()=>drain);Assert.NotNull(drain.Exception);Assert.Equal(1,i.Releases);Assert.Equal(1,j.Releases);
    }
    /// <summary>Cancelling an external wait retains the resources until actual callback completion.</summary>
    /// <returns>The asynchronous raw-work and drain verification.</returns>
    [Fact] public async Task W1_15_CancelledWait()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);var entered=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync(s.Environment,[a],async op=>{entered.SetResult(true);await finish.Task;return 7;});await entered.Task;
        using var cts=new CancellationTokenSource();cts.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>raw.WaitAsync(cts.Token));s.Dispose();Assert.Equal(0,i.Releases);Assert.False(s.DrainAsync().IsCompleted);finish.SetResult(true);Assert.Equal(7,await raw);await s.DrainAsync();Assert.Equal(1,i.Releases);
    }
    /// <summary>Session close revokes new guarded reads without releasing raw-owned resources.</summary>
    [Fact] public void W1_16_CloseDuringRawWork()
    { var s=Compile();var i=new Input();var a=Open(s,i);s.Run(s.Environment,[a],op=>{s.Dispose();Assert.Throws<ObjectDisposedException>(()=>op.GetRoot(a));Assert.Equal(0,i.Releases);return 0;});Assert.Equal(1,i.Releases);s.Dispose(); }
    /// <summary>Inner completion cannot release an overlapping outer raw lease.</summary>
    [Fact] public void W1_17_NestedOperations()
    { using var s=Compile();var i=new Input();var a=Open(s,i);s.Run(s.Environment,[a],outer=>{s.Run(s.Environment,[a],inner=>{a.Dispose();return 0;});Assert.Equal(0,i.Releases);Assert.Throws<ObjectDisposedException>(outer.Check);return 0;});Assert.Equal(1,i.Releases); }
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Origin=new(), Type=new();
        bool ISchemaEvidenceIssuer.OwnsAssessment(object x)=>ReferenceEquals(x,Origin);
        bool ISchemaEvidenceIssuer.OwnsType(object x)=>ReferenceEquals(x,Type);
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar x)=>x is Scalar;
        bool ISchemaEvidenceIssuer.IsSameType(object a,object b)=>ReferenceEquals(a,b);
    }
    private sealed class Scalar(Issuer issuer) : ISchemaIssuedScalar
    {
        XdmValue ISchemaIssuedScalar.Value=>XdmValue.FromInteger(7);
        object ISchemaIssuedScalar.DeclaredType=>issuer.Type;
        object ISchemaIssuedScalar.ActualType=>issuer.Type;
        object ISchemaIssuedScalar.OriginalAssessment=>issuer.Origin;
    }
    /// <summary>Shared lifetime preserves scalar provenance and ordinary inputs refuse foreign issued evidence.</summary>
    [Fact] public void W1_19_TypedEvidenceRemainsAuthenticated()
    {
        var issuer=new Issuer();using var typed=new SchemaQuerySession(new(ImmutableDictionary<string,string>.Empty,"typed"),_=>XdmValue.FromInteger(7),issuer,4,8);
        var snapshot=typed.OpenSnapshot(typed.Environment,new object(),issuer.Origin,()=>{});var value=typed.Run(typed.Environment,snapshot,op=>op.ReadValue(new Scalar(issuer)));
        Assert.Same(issuer.Origin,typed.ReadEvidence(value)!.OriginalAssessment);using var ordinary=Compile("$v");
        ordinary.Run(ordinary.Environment,[],op=>{Assert.Throws<NotSupportedException>(()=>op.CaptureInputs(new EvaluationContext().WithVariable("v",value),16,16));return 0;});
        snapshot.Dispose();Assert.Throws<ObjectDisposedException>(()=>typed.ReadEvidence(value));
    }
    /// <summary>Legacy evaluation stays available and neutral scalar results use the same actual XPath engine.</summary>
    [Fact] public void W1_20_LegacyAndCompiledOrdinaryAgree()
    { using var s=Compile("1+2");Assert.Equal(XPath31Expression.Compile("1+2").Evaluate(new EvaluationContext()).IntegerValue,s.Run(s.Environment,[],op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),16,16))).IntegerValue); }
    /// <summary>Drain waits through raw work, completes after release and repeated close does not double release.</summary>
    /// <returns>The asynchronous raw-work and drain verification.</returns>
    [Fact] public async Task W1_A_DRAIN_ExactlyOnce()
    {
        var s=Compile();var i=new Input();var a=Open(s,i);var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync(s.Environment,[a],async _=>{await finish.Task;return 0;});s.Dispose();var drain=s.DrainAsync();Assert.False(drain.IsCompleted);s.Dispose();finish.SetResult(true);await raw;await drain;await s.DrainAsync();Assert.Equal(1,i.Releases);
    }
    /// <summary>Registered parent/document/children/attribute/namespace routes preserve trusted ownership.</summary>
    [Fact] public void ProjectionRoutesRemainOwned()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);s.Run(s.Environment,[a],op=>
        {
            var inputs=op.CaptureInputs(new EvaluationContext(),16,16);var root=op.GetRoot(a);foreach(var value in root.Children())
            {var child=value.NodeValue!;op.RequireNodeInput(inputs,child);if(child.Parent is {} parent)op.RequireNodeInput(inputs,parent);if(child.Document is {} doc)op.RequireNodeInput(inputs,doc);foreach(var attribute in child.Attributes())op.RequireNodeInput(inputs,attribute.NodeValue!);foreach(var ns in child.Axis(XdmAxis.Namespace))op.RequireNodeInput(inputs,ns.NodeValue!);}return 0;
        });
    }
    /// <summary>Membership changes after input capture reject before the evaluator can access the node.</summary>
    [Fact] public void RechecksMembershipBeforeDispatch()
    {
        int calls=0;using var s=new QuerySourceSession(new(ImmutableDictionary<string,string>.Empty,"recheck"),_=>{calls++;return XdmValue.FromInteger(1);},4);
        var i=new Input();var j=new Input();var a=Open(s,i);var b=Open(s,j);s.Run(s.Environment,[a,b],op=>{var inputs=op.CaptureInputs(new EvaluationContext().WithFocus(XdmValue.FromNode(i.Candidate),1,1),16,16);int getters=i.GetterReads;j.ClaimAll=true;Assert.Throws<ArgumentException>(()=>op.EvaluateAtomic(inputs));Assert.Equal(getters,i.GetterReads);return 0;});Assert.Equal(0,calls);
    }
    /// <summary>Failed admission retains primary and rollback failures without leaking the quota reservation.</summary>
    [Fact] public void AdmissionFailurePreservesRollback()
    { using var s=Compile();var primary=new Exception("availability");var cleanup=new Exception("cleanup");var i=new Input{CheckAction=()=>throw primary,ReleaseAction=()=>throw cleanup};var error=Assert.Throws<AggregateException>(()=>Open(s,i));Assert.Equal(new[]{primary,cleanup},error.InnerExceptions);Assert.Equal(1,i.Releases); }
    /// <summary>Limits and unsupported returned graphs refuse explicitly rather than exposing borrowed results.</summary>
    [Fact] public void BoundsAndUnsupportedResults()
    { Assert.Throws<ArgumentOutOfRangeException>(()=>Compile(limit:0));using var s=Compile(".",1);var i=new Input();var a=Open(s,i);Assert.Throws<InvalidOperationException>(()=>Open(s,new Input()));s.Run(s.Environment,[a],op=>{Assert.Throws<NotSupportedException>(()=>Evaluate(op,i.Candidate));return 0;}); }
    /// <summary>Close during owner entry waits for admission rollback without reading a root.</summary>
    /// <returns>The asynchronous completion observation.</returns>
    [Fact] public async Task CloseDuringAdmissionDrainsRollback()
    {
        var s=Compile();var i=new Input();Task? drain=null;
        i.EnterAction=()=>{s.Dispose();drain=s.DrainAsync();Assert.False(drain.IsCompleted);};
        Assert.Throws<ObjectDisposedException>(()=>Open(s,i));Assert.Equal(1,i.Releases);Assert.Equal(0,i.RootReads);
        await drain!;s.Dispose();Assert.Equal(1,i.Releases);
    }
    /// <summary>Source-free raw work still blocks session drain until its actual completion.</summary>
    /// <returns>The asynchronous raw callback verification.</returns>
    [Fact] public async Task SourceFreeRawWorkParticipatesInDrain()
    {
        var s=Compile("1+2");var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync(s.Environment,[],async _=>{await finish.Task;return 3;});s.Dispose();Assert.False(s.DrainAsync().IsCompleted);
        finish.SetResult(true);Assert.Equal(3,await raw);await s.DrainAsync();s.Dispose();
    }

    /// <summary>Idle observation preserves later source admission and execution.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task OpenIdleDrainDoesNotCloseSession()
    {
        using var s=Compile();Assert.True(s.DrainAsync().IsCompletedSuccessfully);await s.DrainAsync();
        var i=new Input();var a=Open(s,i);Assert.True(s.DrainAsync().IsCompletedSuccessfully);
        s.Run(s.Environment,[a],op=>{Assert.Same(i.Candidate,op.GetRoot(a));return 0;});Assert.Equal(0,i.Releases);
    }
    /// <summary>Busy observation includes source-free callbacks and gives later work its own epoch.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task OpenDrainTracksSeparateBusyEpochs()
    {
        using var s=Compile();
        for(int epoch=0;epoch<2;epoch++)
        {
            var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var raw=s.RunAsync(s.Environment,[],async _=>{await finish.Task;return 7;});
            var drain=s.DrainAsync();Assert.False(drain.IsCompleted);Assert.Same(drain,s.DrainAsync());
            finish.SetResult(true);Assert.Equal(7,await raw);await drain;Assert.True(s.DrainAsync().IsCompletedSuccessfully);
        }
    }
    /// <summary>All overlapping operations must leave before an open busy epoch completes.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task OpenDrainWaitsForEveryRawOperation()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);
        var first=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var r1=s.RunAsync(s.Environment,[a],async _=>{await first.Task;return 1;});
        var r2=s.RunAsync(s.Environment,[a],async _=>{await second.Task;return 2;});var drain=s.DrainAsync();
        first.SetResult(true);await r1;Assert.False(drain.IsCompleted);Assert.Equal(0,i.Releases);
        second.SetResult(true);await r2;await drain;Assert.Equal(0,i.Releases);
    }
    /// <summary>Canceling one wait neither completes the shared observation nor releases retained resources.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task CanceledOpenDrainWaitRetainsRawWork()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);
        var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync(s.Environment,[a],async _=>{await finish.Task;return 1;});var drain=s.DrainAsync();
        using var cancel=new CancellationTokenSource();cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>drain.WaitAsync(cancel.Token));
        Assert.False(drain.IsCompleted);Assert.Equal(0,i.Releases);finish.SetResult(true);await raw;await drain;
        s.Run(s.Environment,[a],op=>{op.Check();return 0;});Assert.Equal(0,i.Releases);
    }
    /// <summary>Pending owner admission participates even before a snapshot is registered.</summary>
    [Fact] public void OpenDrainWaitsForAdmission()
    {
        using var s=Compile();var i=new Input();Task? drain=null;
        i.EnterAction=()=>{drain=s.DrainAsync();Assert.False(drain.IsCompleted);};
        var a=Open(s,i);Assert.True(drain!.IsCompletedSuccessfully);Assert.Equal(0,i.Releases);
        s.Run(s.Environment,[a],op=>{op.Check();return 0;});
    }
    /// <summary>Failed admission drains only after rollback finishes.</summary>
    [Fact] public void OpenDrainWaitsForAdmissionRollback()
    {
        using var s=Compile();var i=new Input();Task? drain=null;
        i.EnterAction=()=>{drain=s.DrainAsync();};i.CheckAction=()=>throw new InvalidOperationException("unavailable");
        i.ReleaseAction=()=>Assert.False(drain!.IsCompleted);
        Assert.Throws<InvalidOperationException>(()=>Open(s,i));Assert.True(drain!.IsCompletedSuccessfully);Assert.Equal(1,i.Releases);
    }
    /// <summary>Close preserves deferred cleanup failure for both the open and closed observations.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task ClosingDuringOpenDrainRetainsCleanupFailure()
    {
        var s=Compile();var i=new Input{ReleaseAction=()=>throw new InvalidOperationException("cleanup")};var a=Open(s,i);
        var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync(s.Environment,[a],async _=>{await finish.Task;return 1;});var open=s.DrainAsync();
        s.Dispose();var closed=s.DrainAsync();Assert.False(open.IsCompleted);Assert.False(closed.IsCompleted);
        finish.SetResult(true);await Assert.ThrowsAnyAsync<Exception>(()=>raw);
        // Session idle observes raw completion; closed drain additionally reports snapshot release errors.
        await open;await Assert.ThrowsAnyAsync<Exception>(()=>closed);Assert.Equal(1,i.Releases);
    }
    /// <summary>A failed callback ends the busy epoch without poisoning future open observations.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task OpenDrainCompletesAfterCallbackFailure()
    {
        using var s=Compile();var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync<int>(s.Environment,[],async _=>{await finish.Task;throw new InvalidOperationException("work");});
        var drain=s.DrainAsync();finish.SetResult(true);await Assert.ThrowsAsync<InvalidOperationException>(()=>raw);await drain;
        Assert.Equal(3,s.Run(s.Environment,[],_=>3));
    }

}
