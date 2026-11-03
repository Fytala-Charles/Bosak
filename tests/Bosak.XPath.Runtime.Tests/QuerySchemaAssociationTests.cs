// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Verifies optional exact-source schema association and guarded scalar evidence.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; W1-B optional association (1.44.0)                                                |
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

/// <summary>Checks optional exact-source schema association and before-getter authentication.</summary>
public sealed class QuerySchemaAssociationTests
{
    private sealed class Lease(Action release) : IDisposable
    {
        /// <summary>Releases the host-owned admission obligation.</summary>
        public void Dispose() => release();
    }
    private sealed class Input : IQuerySourceInput
    {
        internal readonly IXdmNode Node = new XDocumentNode(XDocument.Parse("<n>42</n>"));
        internal int Entries, Releases, RootReads;
        internal bool Available = true;
        internal Action? ReleaseAction;
        IXdmNode IQuerySourceInput.Root { get { RootReads++; Check(); return Node; } }
        void IQuerySourceInput.ThrowIfUnavailable() => Check();
        bool IQuerySourceInput.OwnsNode(IXdmNode node) { Check(); return ReferenceEquals(node, Node); }
        internal void Check() => ObjectDisposedException.ThrowIf(!Available, this);
        internal IDisposable Enter() { Entries++; return new Lease(() => { Releases++; ReleaseAction?.Invoke(); }); }
    }
    private sealed class Scalar(object origin, object type) : ISchemaIssuedScalar
    {
        internal object Origin = origin, Declared = type, Actual = type;
        internal readonly List<string> Reads = [];
        internal bool Poison;
        internal Action<string>? OnRead;
        private void Read(string name) { Reads.Add(name); if (Poison) throw new InvalidOperationException("Foreign scalar getter."); OnRead?.Invoke(name); }
        XdmValue ISchemaIssuedScalar.Value { get { Read("value"); return XdmValue.FromInteger(7); } }
        object ISchemaIssuedScalar.DeclaredType { get { Read("declared"); return Declared; } }
        object ISchemaIssuedScalar.ActualType { get { Read("actual"); return Actual; } }
        object ISchemaIssuedScalar.OriginalAssessment { get { Read("origin"); return Origin; } }
    }
    private sealed class Facts
    {
        internal int Reads;
        internal object Poison { get { Reads++; throw new InvalidOperationException("Unadmitted fact getter."); } }
    }
    private sealed class Advisor(SchemaQueryEnvironment environment) : IQuerySchemaAdvisor, IDisposable
    {
        internal SchemaQueryEnvironment Environment = environment;
        internal readonly object Origin = new(), Type = new();
        internal bool Available = true, OriginLive = true, TypesLive = true;
        internal int Checks, FactChecks, TupleChecks, ScalarChecks, DisposeCalls;
        internal Exception? AvailabilityError;
        internal Action? OnCheck;
        internal readonly Dictionary<ISchemaIssuedScalar, (IQuerySourceInput Input, object Origin)> Scalars = new(ReferenceEqualityComparer.Instance);
        internal readonly HashSet<object> Facts = new(ReferenceEqualityComparer.Instance);
        internal readonly List<(IXdmNode Node, object Facts, IQuerySourceInput Input, object Origin)> Tuples = [];
        SchemaQueryEnvironment IQuerySchemaAdvisor.Environment => Environment;
        void IQuerySchemaAdvisor.ThrowIfUnavailable()
        { Checks++; OnCheck?.Invoke(); if (AvailabilityError is not null) throw AvailabilityError; ObjectDisposedException.ThrowIf(!Available, this); }
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin) => OriginLive && ReferenceEquals(origin, Origin);
        bool ISchemaEvidenceIssuer.OwnsType(object type) => TypesLive && ReferenceEquals(type, Type);
        bool ISchemaEvidenceIssuer.IsSameType(object first, object second) => ReferenceEquals(first, second);
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) => Scalars.ContainsKey(value);
        bool IQuerySchemaAdvisor.OwnsTypedValue(ISchemaIssuedScalar value, IQuerySourceInput input, object origin)
        { ScalarChecks++; return Scalars.TryGetValue(value, out var binding) && ReferenceEquals(input, binding.Input) && ReferenceEquals(origin, binding.Origin); }
        bool IQuerySchemaAdvisor.OwnsNodeFacts(object facts) { FactChecks++; return Facts.Contains(facts); }
        bool IQuerySchemaAdvisor.OwnsNode(IXdmNode node, object facts, IQuerySourceInput input, object origin)
        { TupleChecks++; return Tuples.Any(t => ReferenceEquals(node,t.Node) && ReferenceEquals(facts,t.Facts) && ReferenceEquals(input,t.Input) && ReferenceEquals(origin,t.Origin)); }
        void IDisposable.Dispose() => DisposeCalls++;
        internal Scalar Issue(Input input) { var value = new Scalar(Origin, Type); Scalars.Add(value, (input, Origin)); return value; }
        internal Facts IssueFacts(Input input, Facts? original = null)
        { var facts = original ?? new Facts(); Facts.Add(facts); Tuples.Add((input.Node, facts, input, Origin)); return facts; }
    }
    private static QuerySourceSession Compile(string source = "$v")
    { Assert.True(SchemaSessionCompilation.TryCompileOrdinary(source, new CompileOptions(), 8, out var session, out var reason), reason); return session!; }
    private static QuerySourceSession.Snapshot Open(QuerySourceSession session, Input input, Advisor advisor, int limit = 8)
        => session.OpenAssessedSnapshot(session.Environment, input, input.Enter, advisor, advisor.Origin, limit);
    private static XdmValue Read(QuerySourceSession session, QuerySourceSession.Snapshot snapshot, Scalar value)
        => session.Run(session.Environment, [snapshot], op => op.ReadScalar(snapshot, value));

    /// <summary>Mixes ordinary XML focus and an assessed scalar without fabricating ordinary evidence.</summary>
    [Fact] public void MixedOrdinaryAndAssessedEvaluation()
    {
        using var s=Compile("$v + xs:integer(string(.))");var ordinary=new Input();var assessed=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(assessed);
        var a=s.OpenSnapshot(s.Environment,ordinary,ordinary.Enter);var b=Open(s,assessed,advisor);var facts=advisor.IssueFacts(assessed);
        var result=s.Run(s.Environment,[a,b],op=>{op.ValidateNodeFacts(b,assessed.Node,facts);var v=op.ReadScalar(b,raw);return op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext().WithFocus(XdmValue.FromNode(ordinary.Node),1,1).WithVariable("v",v),16,16));});
        Assert.Equal(49,result.IntegerValue);Assert.Null(XdmValue.SchemaTransportMarker(result));Assert.Equal(0,facts.Reads);
    }
    /// <summary>Ordinary-only operations invoke no unselected schema capability.</summary>
    [Fact] public void OrdinaryRouteDoesNotInvokeAdvisor()
    { using var s=Compile("1+2");var unused=new Advisor(s.Environment){Available=false};var input=new Input();var a=s.OpenSnapshot(s.Environment,input,input.Enter);Assert.Equal(3,s.Run(s.Environment,[a],op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),16,16))).IntegerValue);Assert.Equal(0,unused.Checks); }
    /// <summary>Repeated scalar reads reuse bounded immutable evidence without re-reading payload getters.</summary>
    [Fact] public void WarmReadRetainsOriginAndAvoidsGetters()
    {
        using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);
        var first=Read(s,a,raw);int reads=raw.Reads.Count;var second=Read(s,a,raw);Assert.Equal(reads,raw.Reads.Count);Assert.True(reads>0);
        var evidence=Assert.IsType<SchemaEvidenceKernel.Evidence>(XdmValue.SchemaTransportMarker(first));Assert.Same(advisor.Origin,evidence.OriginalAssessment);Assert.Same(XdmValue.SchemaTransportMarker(first),XdmValue.SchemaTransportMarker(second));
    }
    /// <summary>Nonmember admissions refuse before purported scalar payload access.</summary>
    [Fact] public void NonmemberRefusesBeforeScalarGetters()
    { using var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor);var b=Open(s,j,advisor);var raw=new Scalar(advisor.Origin,advisor.Type){Poison=true};s.Run(s.Environment,[a],op=>{Assert.Throws<ArgumentException>(()=>op.ReadScalar(b,raw));return 0;});Assert.Empty(raw.Reads); }
    /// <summary>Another snapshot with the same origin cannot borrow a container issued to the first input.</summary>
    [Fact] public void ForeignInputRefusesBeforeScalarGetters()
    { using var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);raw.Poison=true;var a=Open(s,i,advisor);var b=Open(s,j,advisor);Assert.Throws<ArgumentException>(()=>Read(s,b,raw));Assert.Empty(raw.Reads); }
    /// <summary>A container registered under a foreign original assessment is refused without getters.</summary>
    [Fact] public void WrongIssuedOriginRefusesBeforeGetters()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);advisor.Scalars[raw]=(i,new object());raw.Poison=true;var a=Open(s,i,advisor);Assert.Throws<ArgumentException>(()=>Read(s,a,raw));Assert.Empty(raw.Reads); }
    /// <summary>Matching fields on a forged container do not establish issuance.</summary>
    [Fact] public void ForgedContainerRefusesBeforeGetters()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=new Scalar(advisor.Origin,advisor.Type){Poison=true};var a=Open(s,i,advisor);Assert.Throws<ArgumentException>(()=>Read(s,a,raw));Assert.Empty(raw.Reads); }
    /// <summary>Inconsistent original metadata is rejected before the atomic payload is accessed.</summary>
    [Fact] public void WrongMetadataOriginRefusesBeforeValue()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);raw.Origin=new object();var a=Open(s,i,advisor);Assert.Throws<ArgumentException>(()=>Read(s,a,raw));Assert.DoesNotContain("value",raw.Reads); }
    /// <summary>Unowned actual types never become admitted atomic provenance.</summary>
    [Fact] public void ForeignActualTypeRefusesBeforeValue()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);raw.Actual=new object();var a=Open(s,i,advisor);Assert.Throws<ArgumentException>(()=>Read(s,a,raw));Assert.DoesNotContain("value",raw.Reads); }
    /// <summary>Revocation by a getter prevents every following provider getter.</summary>
    /// <param name="source">Whether the source rather than the advisor is revoked.</param>
    [Theory][InlineData(false)][InlineData(true)] public void GetterRevocationStopsFurtherReads(bool source)
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);raw.OnRead=_=>{if(source)i.Available=false;else advisor.Available=false;};Assert.Throws<ObjectDisposedException>(()=>Read(s,a,raw));Assert.Single(raw.Reads);Assert.Equal(0,i.Releases); }
    /// <summary>Close in a provider getter keeps owner resources until actual raw read completion.</summary>
    /// <returns>The actual raw completion and asynchronous drain observation.</returns>
    [Fact] public async Task CloseDuringGetterDefersHostRelease()
    { var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);raw.OnRead=_=>{s.Dispose();Assert.Equal(0,i.Releases);};Assert.Throws<ObjectDisposedException>(()=>Read(s,a,raw));Assert.Equal(1,i.Releases);await s.DrainAsync();Assert.Equal(0,advisor.DisposeCalls); }
    /// <summary>Foreign fact objects and false node/fact associations do not authenticate.</summary>
    /// <param name="owned">Whether the fact object is issued but paired with another node.</param>
    [Theory][InlineData(false)][InlineData(true)] public void FactsRequireExactNodeTuple(bool owned)
    {
        using var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor);var facts=owned?advisor.IssueFacts(j):new Facts();
        s.Run(s.Environment,[a],op=>{Assert.Throws<ArgumentException>(()=>op.ValidateNodeFacts(a,i.Node,facts));return 0;});Assert.Equal(0,facts.Reads);Assert.Equal(owned?1:0,advisor.TupleChecks);
    }
    /// <summary>Source ownership is checked before advisor fact issuance callbacks.</summary>
    [Fact] public void ForeignNodeRefusesBeforeFactCallbacks()
    { using var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor);var facts=advisor.IssueFacts(i);s.Run(s.Environment,[a],op=>{Assert.Throws<ArgumentException>(()=>op.ValidateNodeFacts(a,j.Node,facts));return 0;});Assert.Equal(0,advisor.FactChecks);Assert.Equal(0,facts.Reads); }
    /// <summary>Cached type membership is revalidated without re-reading mutable payloads.</summary>
    [Fact] public void CachedTypeRevocationRefuses()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);Read(s,a,raw);int reads=raw.Reads.Count;advisor.TypesLive=false;Assert.Throws<ArgumentException>(()=>Read(s,a,raw));Assert.Equal(reads,raw.Reads.Count); }
    /// <summary>Cached scalar issuance is revalidated both on read and input capture.</summary>
    [Fact] public void CachedContainerRevocationRefuses()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);var value=Read(s,a,raw);int reads=raw.Reads.Count;advisor.Scalars.Remove(raw);Assert.Throws<ArgumentException>(()=>Read(s,a,raw));s.Run(s.Environment,[a],op=>{Assert.Throws<ArgumentException>(()=>op.CaptureInputs(new EvaluationContext().WithVariable("v",value),16,16));return 0;});Assert.Equal(reads,raw.Reads.Count); }
    /// <summary>Transport cannot cross to another admitted input even with equal origin and types.</summary>
    [Fact] public void TransportRequiresExactSnapshotMembership()
    { using var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);var b=Open(s,j,advisor);var value=Read(s,a,raw);s.Run(s.Environment,[b],op=>{Assert.Throws<ArgumentException>(()=>op.CaptureInputs(new EvaluationContext().WithVariable("v",value),16,16));return 0;}); }
    /// <summary>Successful typed values retain their original evidence through same-source identity evaluation.</summary>
    [Fact] public void IdentityEvaluationKeepsOriginalEvidence()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);s.Run(s.Environment,[a],op=>{var value=op.ReadScalar(a,raw);var result=op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext().WithVariable("v",value),16,16));Assert.Same(XdmValue.SchemaTransportMarker(value),XdmValue.SchemaTransportMarker(result));return 0;}); }
    /// <summary>Finite cache registration refuses excess containers before their payload getters.</summary>
    [Fact] public void CacheRegistrationIsBounded()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor,1);var first=advisor.Issue(i);var second=advisor.Issue(i);Read(s,a,first);Assert.Throws<InvalidOperationException>(()=>Read(s,a,second));Assert.Empty(second.Reads); }
    /// <summary>Rejected unissued containers do not consume the finite registration quota.</summary>
    [Fact] public void UnissuedContainerDoesNotConsumeQuota()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor,1);Assert.Throws<ArgumentException>(()=>Read(s,a,new Scalar(advisor.Origin,advisor.Type)));Assert.Equal(7,Read(s,a,advisor.Issue(i)).IntegerValue); }
    /// <summary>Environment and origin binding failures roll back the host obligation without root reads.</summary>
    /// <param name="environment">Whether the environment rather than the origin is invalid.</param>
    [Theory][InlineData(false)][InlineData(true)] public void InvalidBindingRollsBack(bool environment)
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);if(environment)advisor.Environment=new(ImmutableDictionary<string,string>.Empty,"foreign");else advisor.OriginLive=false;Assert.Throws<ArgumentException>(()=>Open(s,i,advisor));Assert.Equal(1,i.Releases);Assert.Equal(0,i.RootReads); }
    /// <summary>Invalid finite policy refuses before host owner entry.</summary>
    [Fact] public void InvalidBoundsRefuseBeforeEntry()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);Assert.Throws<ArgumentOutOfRangeException>(()=>Open(s,i,advisor,0));Assert.Equal(0,i.Entries); }
    /// <summary>Binding failures preserve original primary and actual rollback cleanup exceptions.</summary>
    [Fact] public void BindingFailurePreservesRollbackErrors()
    { using var s=Compile();var i=new Input();var primary=new Exception("advisor");var cleanup=new Exception("owner release");i.ReleaseAction=()=>throw cleanup;var advisor=new Advisor(s.Environment){AvailabilityError=primary};var error=Assert.Throws<AggregateException>(()=>Open(s,i,advisor));Assert.Equal(new[]{primary,cleanup},error.InnerExceptions);Assert.Equal(1,i.Releases); }
    /// <summary>Close while binding retains actual resources through the raw initialization callback.</summary>
    /// <returns>The actual initialization rollback and drain observation.</returns>
    [Fact] public async Task CloseDuringBindingDefersRelease()
    { var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);advisor.OnCheck=()=>{s.Dispose();Assert.Equal(0,i.Releases);};Assert.Throws<ObjectDisposedException>(()=>Open(s,i,advisor));Assert.Equal(1,i.Releases);await s.DrainAsync(); }
    /// <summary>Ordinary admissions cannot acquire schema authority from another operation member.</summary>
    [Fact] public void OrdinaryAdmissionCannotReadAssessedScalar()
    { using var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var ordinary=s.OpenSnapshot(s.Environment,i,i.Enter);var assessed=Open(s,j,advisor);var raw=advisor.Issue(j);raw.Poison=true;s.Run(s.Environment,[ordinary,assessed],op=>{Assert.Throws<NotSupportedException>(()=>op.ReadScalar(ordinary,raw));return 0;});Assert.Empty(raw.Reads); }
    /// <summary>Trusted copies retain original facts only when exact new-node association is registered.</summary>
    [Fact] public void PreserveCopyUsesExplicitMapping()
    {
        using var s=Compile();var original=new Input();var copy=new Input();var advisor=new Advisor(s.Environment);var facts=advisor.IssueFacts(original);advisor.IssueFacts(copy,facts);
        var a=Open(s,original,advisor);var b=Open(s,copy,advisor);var av=advisor.Issue(original);var bv=advisor.Issue(copy);
        s.Run(s.Environment,[a,b],op=>{op.ValidateNodeFacts(a,original.Node,facts);op.ValidateNodeFacts(b,copy.Node,facts);var x=Assert.IsType<SchemaEvidenceKernel.Evidence>(XdmValue.SchemaTransportMarker(op.ReadScalar(a,av)));var y=Assert.IsType<SchemaEvidenceKernel.Evidence>(XdmValue.SchemaTransportMarker(op.ReadScalar(b,bv)));Assert.Same(x.OriginalAssessment,y.OriginalAssessment);Assert.NotSame(x.Snapshot,y.Snapshot);return 0;});
    }
    /// <summary>Strip copies retain ordinary ownership without original fact authority.</summary>
    [Fact] public void StripCopyHasNoSchemaAuthority()
    { using var s=Compile();var original=new Input();var copy=new Input();var advisor=new Advisor(s.Environment);var facts=advisor.IssueFacts(original);var a=Open(s,original,advisor);var b=s.OpenSnapshot(s.Environment,copy,copy.Enter);s.Run(s.Environment,[a,b],op=>{Assert.Throws<NotSupportedException>(()=>op.ValidateNodeFacts(b,copy.Node,facts));return 0;}); }
    /// <summary>Environment changes after capture refuse before engine dispatch.</summary>
    [Fact] public void ChangedAdvisorEnvironmentRefusesDispatch()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor);var raw=advisor.Issue(i);s.Run(s.Environment,[a],op=>{var inputs=op.CaptureInputs(new EvaluationContext().WithVariable("v",op.ReadScalar(a,raw)),16,16);advisor.Environment=new(ImmutableDictionary<string,string>.Empty,"other");Assert.Throws<ArgumentException>(()=>op.EvaluateAtomic(inputs));advisor.Environment=s.Environment;return 0;}); }
    /// <summary>Revoked original assessment refuses cached evidence before any further scalar getter.</summary>
    [Fact] public void RevokedOriginRefusesCachedEvidence()
    { using var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var raw=advisor.Issue(i);var a=Open(s,i,advisor);Read(s,a,raw);int reads=raw.Reads.Count;advisor.OriginLive=false;Assert.Throws<ArgumentException>(()=>Read(s,a,raw));Assert.Equal(reads,raw.Reads.Count); }
    /// <summary>Mixed assessed and ordinary cleanup retains every original failure in reverse member order.</summary>
    /// <returns>The asynchronous drain observation after failed raw work.</returns>
    [Fact] public async Task MixedPrimaryAndCleanupFailuresRemainDistinct()
    {
        var s=Compile();var i=new Input();var j=new Input();var advisor=new Advisor(s.Environment);var first=new Exception("ordinary cleanup");var second=new AggregateException(new Exception("assessed cleanup"));
        i.ReleaseAction=()=>throw first;j.ReleaseAction=()=>throw second;var a=s.OpenSnapshot(s.Environment,i,i.Enter);var b=Open(s,j,advisor);var primary=new Exception("raw work");
        var error=Assert.Throws<AggregateException>(()=>s.Run<int>(s.Environment,[a,b],_=>{s.Dispose();throw primary;}));Assert.Equal(new Exception[]{primary,second,first},error.InnerExceptions);
        await Assert.ThrowsAnyAsync<Exception>(()=>s.DrainAsync());Assert.Equal(1,i.Releases);Assert.Equal(1,j.Releases);Assert.Equal(0,advisor.DisposeCalls);
    }
    /// <summary>Cancelled waits and close cannot release assessed resources while the raw async callback is pending.</summary>
    /// <returns>The actual raw callback and drain completion.</returns>
    [Fact] public async Task AssessedAsyncRawWorkRetainsOwner()
    {
        var s=Compile();var i=new Input();var advisor=new Advisor(s.Environment);var a=Open(s,i,advisor);var scalar=advisor.Issue(i);var finish=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=s.RunAsync(s.Environment,[a],async op=>{op.ReadScalar(a,scalar);await finish.Task;int reads=scalar.Reads.Count;Assert.Throws<ObjectDisposedException>(()=>op.ReadScalar(a,scalar));Assert.Equal(reads,scalar.Reads.Count);return 7;});
        using var cancel=new CancellationTokenSource();cancel.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>raw.WaitAsync(cancel.Token));s.Dispose();Assert.Equal(0,i.Releases);Assert.False(s.DrainAsync().IsCompleted);
        finish.SetResult(true);Assert.Equal(7,await raw);await s.DrainAsync();Assert.Equal(1,i.Releases);Assert.Equal(0,advisor.DisposeCalls);
    }

}
