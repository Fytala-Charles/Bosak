// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Verifies public ordinary-source evaluation, refusal, ownership and raw lifetime behavior.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; bounded public ordinary-source facade (1.45.0).                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Move newly supported string(arithmetic) case to exact scalar positive coverage (1.49.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 04-11-2026     | Promote finite division controls and retain unavailable grammar checks (1.51.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Session = Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Api.Tests;

/// <summary>Uses only public APIs to qualify the ordinary-source session facade.</summary>
public sealed class PublicOrdinarySessionTests
{
    private sealed class Source : ISchemaSourceBinding { }
    private sealed class Binding : ISchemaSnapshotBinding { }
    private sealed class Lease(Input input, Action release) : IQueryOperationLease
    {
        private Action? _release = release;
        internal bool Revoked;
        /// <inheritdoc/>
        public void ThrowIfUnavailable(CancellationToken token)
        { input.Tokens.Add(token); token.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(Revoked, this); }
        /// <inheritdoc/>
        public void Dispose() => Interlocked.Exchange(ref _release, null)?.Invoke();
    }
    private sealed class Lifetime(Input input) : IQueryOperationLifetime
    {
        internal Lease? Last;
        internal bool ReturnNull;
        /// <inheritdoc/>
        public IQueryOperationLease Enter(CancellationToken token)
        { token.ThrowIfCancellationRequested(); if(ReturnNull)return null!; return Last=(Lease)input.Enter(); }
    }
    private sealed class Input : ISchemaAssessmentInput
    {
        private readonly HashSet<Node> _nodes = new(ReferenceEqualityComparer.Instance);
        private readonly Node _root;
        internal bool Available = true, Poison, ClaimAll;
        internal int RootReads, GetterReads, Entries, Releases, Checks;
        internal Action? EnterAction, CheckAction, ReleaseAction;
        internal ISchemaSourceBinding SourceBinding = new Source();
        internal ISchemaSnapshotBinding SnapshotBinding = new Binding();
        internal readonly List<CancellationToken> Tokens = [];
        internal bool BadBinding;
        ISchemaSourceBinding ISchemaAssessmentInput.Source => SourceBinding;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot => BadBinding ? throw new InvalidOperationException("binding") : SnapshotBinding;
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token) { Tokens.Add(token); token.ThrowIfCancellationRequested(); ThrowIfUnavailable(); }
        internal Input() => _root = Project(new XDocumentNode(XDocument.Parse("<n a='x'>42</n>")));
        internal IXdmNode Candidate => _root;
        private Node Project(IXdmNode node) { var projection = new Node(this, node); _nodes.Add(projection); return projection; }
        IXdmNode ISchemaAssessmentInput.Root { get { RootReads++; ThrowIfUnavailable(); return _root; } }
        /// <summary>Checks the trusted fixture owner before source access.</summary>
        public void ThrowIfUnavailable() { Checks++; CheckAction?.Invoke(); ObjectDisposedException.ThrowIf(!Available, this); }
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node)
        { ThrowIfUnavailable(); return ClaimAll || node is Node known && ReferenceEquals(known.Owner, this) && _nodes.Contains(known); }
        internal IQueryOperationLease Enter() { Entries++; EnterAction?.Invoke(); return new Lease(this, () => { Releases++; ReleaseAction?.Invoke(); }); }
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

    private static SchemaQueryLimits Limits(int snapshots=4) => new(snapshots,8,8,32,8);
    private static Session Compile(string expression="string(.)", CompileOptions? options=null)
    { var result=Session.CompileXPath(expression,Limits(),options);Assert.Equal(SchemaResultKind.Supported,result.Kind);Assert.Null(result.DiagnosticCode);return result.Value; }
    private static SchemaQuerySnapshot Open(Session s,Input input,Lifetime? lifetime=null)
    { var result=s.OpenSnapshot(input,lifetime??new Lifetime(input));Assert.Equal(SchemaResultKind.Supported,result.Kind);return result.Value; }
    private static XdmValue Evaluate(Session s,SchemaQuerySnapshot a,Input i)
        =>s.Run([a],op=>op.Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(op.GetRoot(a)),1,1)));

    /// <summary>Validates every explicit limit and refuses the default struct.</summary>
    [Fact] public void LimitsAreExplicit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>new SchemaQueryLimits(0,1,1,1,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new SchemaQueryLimits(1,0,1,1,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new SchemaQueryLimits(1,1,0,1,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new SchemaQueryLimits(1,1,1,0,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new SchemaQueryLimits(1,1,1,1,-1));
        Assert.Throws<ArgumentException>(()=>Session.CompileXPath("true()",default));
        var l=new SchemaQueryLimits(1,2,3,4,0);Assert.Equal((1,2,3,4,0),(l.MaxOpenSnapshots,l.MaxCachedValuesPerSnapshot,l.MaxTypedValueAtoms,l.MaxNumericDigits,l.MaxDecimalScale));
    }
    /// <summary>Compiles and evaluates supported strings and booleans through the public facade.</summary>
    /// <param name="expression">A supported expression.</param>
    /// <param name="expected">The resulting string representation.</param>
    [Theory]
    [InlineData("string(.)","42")]
    [InlineData("string(/n/@a)","x")]
    [InlineData("string(/n[exists(@a)])","42")]
    [InlineData("if (exists(/n)) then 'yes' else 'no'","yes")]
    [InlineData("boolean(/n) and not(empty(/n))","true")]
    [InlineData("fn:true()","true")]
    [InlineData("false()","false")]
    [InlineData("local-name(/n)","n")]
    [InlineData("name(/n)","n")]
    [InlineData("namespace-uri(/n)","")]
    [InlineData("'999999999999999999999999999999999999999999'","999999999999999999999999999999999999999999")]
    public void SupportedQueries(string expression,string expected)
    { using var s=Compile(expression);var i=new Input();var a=Open(s,i);Assert.Equal(expected,Evaluate(s,a,i).ToString());Assert.Null(s.Advisor); }
    /// <summary>Unavailable syntax returns Unsupported before numeric parsing or resource dispatch.</summary>
    /// <param name="expression">A query outside the declared profile.</param>
    [Theory]
    [InlineData("999999999999999999999999999999999999999999999999999999999999999999999999999")]
    [InlineData("string(1 to 2)")]
    [InlineData("xs:integer('123')")]
    [InlineData("count(/n)")]
    [InlineData("string(position())")]
    [InlineData("string(doc('https://invalid.example/never'))")]
    [InlineData("string(map{'x':'y'})")]
    [InlineData("string((function(){'x'})())")]
    [InlineData("string(/n[@a = 'x'])")]
    [InlineData("/n")]
    public void UnsupportedCompilation(string expression)
    { var r=Session.CompileXPath(expression,Limits());Assert.Equal(SchemaResultKind.Unsupported,r.Kind);Assert.NotNull(r.DiagnosticCode);Assert.Throws<InvalidOperationException>(()=>r.Value); }
    /// <summary>Static context is copied and has an explicit profile and dated data-model revision.</summary>
    [Fact] public void EnvironmentCapturesActualNamespaces()
    {
        var namespaces=new Dictionary<string,string>(StringComparer.Ordinal){["f"]="http://www.w3.org/2005/xpath-functions"};
        using var s=Compile("f:true()",new CompileOptions{Namespaces=namespaces});namespaces["f"]="urn:changed";
        Assert.Equal("http://www.w3.org/2005/xpath-functions",s.Environment.StaticContext.Namespaces["f"]);
        Assert.Equal(QueryHostLanguage.XPath,s.Environment.HostLanguage);Assert.Equal("3.1",s.Environment.HostVersion);
        Assert.Equal("bosak-ordinary-string-boolean-v1",s.Environment.BehaviorProfile);Assert.Contains("20170321",s.Environment.XdmSpecificationRevision.ToString());
        Assert.NotEmpty(s.Environment.StaticContextFingerprint);Assert.Same(s.Environment,s.Environment);
        Assert.True(s.Run([],op=>op.Evaluate(new EvaluationContext())).BooleanValue);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("f:true()",Limits(),new CompileOptions{Namespaces=namespaces}).Kind);
    }
    /// <summary>Nonselected host and execution profiles refuse compilation explicitly.</summary>
    [Fact] public void UnavailableCompileOptionsRefuse()
    {
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("true()",Limits(),new CompileOptions{BackwardsCompatible=true}).Kind);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("true()",Limits(),new CompileOptions{Compatibility=XPathCompatibility.XPath40}).Kind);
    }
    /// <summary>Admission borrows resources without reading a root and enforces quota and duplicate identity.</summary>
    [Fact] public void AdmissionIsBoundedAndGetterFree()
    {
        using var s=Session.CompileXPath("true()",Limits(1)).Value;var i=new Input();var a=Open(s,i);
        Assert.Equal(0,i.RootReads);Assert.Equal(0,i.GetterReads);Assert.Same(i.SnapshotBinding,a.Binding);
        Assert.Throws<ArgumentException>(()=>Open(s,i));var j=new Input();Assert.Throws<InvalidOperationException>(()=>Open(s,j));Assert.Equal(0,j.Entries);
        a.Dispose();Assert.Equal(1,i.Releases);var b=Open(s,i);b.Dispose();Assert.Equal(2,i.Releases);
    }
    /// <summary>Foreign and ambiguous incoming nodes refuse before purported node getters.</summary>
    [Fact] public void MembershipPrecedesGetters()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);var foreign=new Input{Poison=true};
        Assert.Throws<NotSupportedException>(()=>s.Run([a],op=>op.Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(foreign.Candidate),1,1))));Assert.Equal(0,foreign.GetterReads);
        var j=new Input{ClaimAll=true};var b=Open(s,j);int before=i.GetterReads;
        Assert.Throws<ArgumentException>(()=>s.Run([a,b],op=>op.GetRoot(a)));Assert.Equal(before,i.GetterReads);
    }
    /// <summary>Tokens reach both trusted input and retained owner lease; revoked leases reject access.</summary>
    [Fact] public void RawCancellationAndRevocationReachOwners()
    {
        using var s=Compile();var i=new Input();var lifetime=new Lifetime(i);var a=Open(s,i,lifetime);
        using var cancel=new CancellationTokenSource();s.Run([a],op=>{op.GetRoot(a);return 0;},cancel.Token);
        Assert.Contains(cancel.Token,i.Tokens);lifetime.Last!.Revoked=true;int reads=i.RootReads;
        Assert.Throws<ObjectDisposedException>(()=>s.Run([a],op=>op.GetRoot(a)));Assert.Equal(reads,i.RootReads);
    }
    /// <summary>Cancellation before entry acquires nothing and does not invoke a callback.</summary>
    [Fact] public void PreCanceledWorkAcquiresNothing()
    {
        using var cancel=new CancellationTokenSource();cancel.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(()=>Session.CompileXPath("true()",Limits(),cancellationToken:cancel.Token));
        using var s=Compile();var i=new Input();Assert.ThrowsAny<OperationCanceledException>(()=>s.OpenSnapshot(i,new Lifetime(i),cancellationToken:cancel.Token));
        Assert.Equal(0,i.Entries);Assert.ThrowsAny<OperationCanceledException>(()=>s.Run<int>([],_=>throw new Exception("not called"),cancel.Token));
    }
    /// <summary>Changed source and snapshot identities revoke the admission before reading a root.</summary>
    [Fact] public void BindingDriftRefuses()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);i.SnapshotBinding=new Binding();
        Assert.Throws<InvalidOperationException>(()=>s.Run([a],op=>op.GetRoot(a)));Assert.Equal(0,i.RootReads);
    }
    /// <summary>Failed descriptor capture releases admission and preserves primary and cleanup faults.</summary>
    [Fact] public void BindingFailureRollsBack()
    {
        using var s=Compile();var cleanup=new Exception("cleanup");var i=new Input{BadBinding=true,ReleaseAction=()=>throw cleanup};
        var error=Assert.Throws<AggregateException>(()=>Open(s,i));Assert.Equal("binding",error.InnerExceptions[0].Message);Assert.Same(cleanup,error.InnerExceptions[1]);
        Assert.Equal(1,i.Releases);Assert.Equal(0,i.RootReads);i.BadBinding=false;i.ReleaseAction=null;Open(s,i);
    }
    /// <summary>Missing owner admission is refused before any input getter.</summary>
    [Fact] public void NullLeaseRefuses()
    { using var s=Compile();var i=new Input();Assert.Throws<InvalidOperationException>(()=>Open(s,i,new Lifetime(i){ReturnNull=true}));Assert.Equal(0,i.RootReads);Assert.Equal(0,i.Checks); }
    /// <summary>Callbacks and snapshots carry exact session and operation membership.</summary>
    [Fact] public void ForeignAndExpiredOperationsRefuse()
    {
        using var s=Compile();using var other=Compile();var i=new Input();var a=Open(s,i);SchemaQueryOperation? saved=null;
        s.Run([a],op=>{saved=op;return 0;});Assert.Throws<ObjectDisposedException>(()=>saved!.GetRoot(a));
        Assert.Throws<ArgumentException>(()=>other.Run([a],_=>0));Assert.Throws<ArgumentException>(()=>s.Run([a,a],_=>0));
        Assert.Throws<ArgumentException>(()=>s.Run(default,_=>0));a.Dispose();Assert.Throws<ObjectDisposedException>(()=>s.Run([a],_=>0));
    }
    /// <summary>String and boolean variables work, while numeric and node result channels refuse.</summary>
    [Fact] public void DynamicScalarChannelIsBounded()
    {
        using var s=Compile("$x");Assert.Equal("ok",s.Run([],op=>op.Evaluate(new EvaluationContext().WithVariable("x",XdmValue.FromString("ok")))).StringValue);
        Assert.True(s.Run([],op=>op.Evaluate(new EvaluationContext().WithVariable("x",XdmValue.FromBoolean(true)))).BooleanValue);
        Assert.Throws<NotSupportedException>(()=>s.Run([],op=>op.Evaluate(new EvaluationContext().WithVariable("x",XdmValue.FromInteger(1)))));
        var i=new Input();var a=Open(s,i);Assert.Throws<NotSupportedException>(()=>s.Run([a],op=>op.Evaluate(new EvaluationContext().WithVariable("x",XdmValue.FromNode(op.GetRoot(a))))));
    }
    /// <summary>Raw work survives cancellation of a wait and retains owner admissions through close.</summary>
    /// <returns>The completed lifetime verification.</returns>
    [Fact] public async Task AsyncCloseRetainsActualRawWork()
    {
        var s=Compile();var i=new Input();var a=Open(s,i);var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync([a],async op=>{await finish.Task;Assert.Throws<ObjectDisposedException>(()=>op.GetRoot(a));return 7;});
        var open=s.DrainAsync();using var cancel=new CancellationTokenSource();cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>s.DrainAsync(cancel.Token));Assert.False(open.IsCompleted);
        s.Dispose();Assert.Equal(0,i.Releases);var closed=s.DrainAsync();Assert.False(closed.IsCompleted);
        finish.SetResult(true);Assert.Equal(7,await raw);await open;await closed;Assert.Equal(1,i.Releases);s.Dispose();
    }
    /// <summary>Open-session drain leaves snapshots usable after the current raw callback completes.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task OpenDrainPreservesLaterUse()
    {
        using var s=Compile();await s.DrainAsync();var i=new Input();var a=Open(s,i);
        var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync([a],async _=>{await finish.Task;return 1;});var drain=s.DrainAsync();Assert.False(drain.IsCompleted);
        finish.SetResult(true);await raw;await drain;Assert.Equal("42",Evaluate(s,a,i).StringValue);Assert.Equal(0,i.Releases);
    }
    /// <summary>Close attempts every owner release and exposes cleanup failures through drain.</summary>
    /// <returns>The completed verification.</returns>
    [Fact] public async Task ClosePreservesAllCleanupFailures()
    {
        var s=Compile();var i=new Input{ReleaseAction=()=>throw new Exception("one")};var j=new Input{ReleaseAction=()=>throw new Exception("two")};Open(s,i);Open(s,j);
        Assert.Throws<AggregateException>(s.Dispose);await Assert.ThrowsAnyAsync<Exception>(()=>s.DrainAsync());Assert.Equal(1,i.Releases);Assert.Equal(1,j.Releases);
    }
    private sealed class UnavailableAssessor : ISchemaAssessor
    {
        ISchemaAuthority ISchemaAssessor.Authority => throw new Exception("assessor getter");
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage => throw new Exception("assessor getter");
        string ISchemaAssessor.AssessmentProfile => throw new Exception("assessor getter");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment environment,CancellationToken token) => throw new Exception("bind");
    }
    private sealed class UnavailableValue : ISchemaTypedValue
    {
        ISchemaComponentHandle ISchemaTypedValue.GoverningType => throw new Exception("typed getter");
        bool ISchemaTypedValue.IsList => throw new Exception("typed getter");
        IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems => throw new Exception("typed enumeration");
    }
    private sealed class UnavailableComponent : ISchemaComponentHandle
    { ISchemaAuthority ISchemaComponentHandle.Authority => throw new Exception("component getter"); }
    private sealed class UnavailableEpisode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => throw new Exception("episode getter");
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => throw new Exception("episode getter");
    }
    private sealed class UnavailableOrigin : ISchemaAssessmentOrigin
    { ISchemaSourceBinding ISchemaAssessmentOrigin.Source => throw new Exception("origin getter"); }
    /// <summary>Refused schema capabilities invoke no purported assessor, origin, typed or construction getter.</summary>
    [Fact] public void SchemaRefusalsDoNotInvokeProviders()
    {
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("true()",Limits(),assessor:new UnavailableAssessor()).Kind);
        using var s=Compile();var i=new Input();Assert.Equal(SchemaResultKind.Unsupported,s.OpenSnapshot(i,new Lifetime(i),new UnavailableOrigin()).Kind);Assert.Equal(0,i.Entries);
        var a=Open(s,i);s.Run([a],op=>
        {
            Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(a,new UnavailableValue()).Kind);
            Assert.Equal(SchemaResultKind.Unsupported,op.ConstructSimpleValue(default,new UnavailableComponent(),new UnavailableEpisode()).Kind);return 0;
        });Assert.Equal(0,i.RootReads);Assert.Equal(0,i.GetterReads);
        using var other=Compile();Assert.Throws<ArgumentException>(()=>other.Run([],op=>op.ReadTypedValue(a,new UnavailableValue())));
    }
    /// <summary>Cancellation during a trusted availability callback refuses before a root getter.</summary>
    [Fact] public void CancellationDuringAvailabilityRefusesBeforeRoot()
    {
        using var s=Compile();var i=new Input();var a=Open(s,i);using var cancel=new CancellationTokenSource();
        i.CheckAction=cancel.Cancel;Assert.ThrowsAny<OperationCanceledException>(()=>s.Run([a],op=>op.GetRoot(a),cancel.Token));Assert.Equal(0,i.RootReads);
    }

}
