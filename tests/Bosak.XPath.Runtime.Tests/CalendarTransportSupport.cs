// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Provides public-contract test providers for exact calendar transport.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections;
using System.Collections.Immutable;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using SchemaQuerySession = Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Runtime.Tests;

// Provider fixtures reuse reviewed public-contract test shapes; no internal engine entry points.
internal static class CalendarTransportSupport
{
    private sealed class Source:ISchemaSourceBinding { }
    private sealed class Stamp:ISchemaSnapshotBinding { }
    internal sealed class Input:ISchemaAssessmentInput,IQueryOperationLifetime
    {
        internal bool Available=true;internal int Roots,Releases,Checks;internal Stream? Resource=null;
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
        internal Action<string>? OnRead=null;internal int ValueReads;
        /// <inheritdoc/>
        public override SchemaResultKind Kind{get{OnRead?.Invoke("kind");return kind;}}
        /// <inheritdoc/>
        public override T Value{get{ValueReads++;OnRead?.Invoke("value");if(kind!=SchemaResultKind.Supported)throw new Exception("unsupported value poison");return value;}}
        /// <inheritdoc/>
        public override string? DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    internal sealed class Assessor:ISchemaAssessor,IDisposable
    {
        internal ISchemaAuthority Selected=new Authority();internal Advisor? Advisor;internal IQuerySemanticEnvironment? Seen;
        internal Func<SchemaResult<ISchemaAssessmentAdvisor>>? Outcome=null;internal Action<string>? Hook=null;internal int Disposals,Binds;
        ISchemaAuthority ISchemaAssessor.Authority{get{Hook?.Invoke("authority");return Selected;}}
        SchemaLanguageVersion ISchemaAssessor.SchemaLanguage=>throw new Exception("descriptive language poison");
        string ISchemaAssessor.AssessmentProfile=>throw new Exception("descriptive profile poison");
        SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment environment,CancellationToken token)
        {Binds++;Seen=environment;Hook?.Invoke("bind");if(Advisor is not null)Advisor.Environment=environment;return Outcome is null?new Outcome<ISchemaAssessmentAdvisor>(SchemaResultKind.Supported,Advisor!):Outcome();}
        /// <inheritdoc/>
        public void Dispose()=>Disposals++;
    }
    internal sealed class Advisor(Assessor assessor):ISchemaAssessmentAdvisor,IDisposable
    {
        internal ISchemaAssessor Endpoint=assessor;internal IQuerySemanticEnvironment Environment=null!;internal bool Available=true;
        internal readonly Dictionary<ISchemaTypedValue,(ISchemaAssessmentInput Input,ISchemaAssessmentOrigin? Origin)> Values=new(ReferenceEqualityComparer.Instance);
        internal readonly HashSet<ISchemaAssessmentOrigin> Origins=new(ReferenceEqualityComparer.Instance);
        internal Action<string>? Hook=null;internal int Disposals,ValueChecks;internal ISchemaAssessmentInput? LastInput;
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
    internal sealed class Origin : ISchemaAssessmentOrigin
    {ISchemaSourceBinding ISchemaAssessmentOrigin.Source=>throw new Exception("origin source getter");}
    internal sealed class Handle : ISchemaComponentHandle
    {ISchemaAuthority ISchemaComponentHandle.Authority=>throw new Exception("self-claimed authority getter");}
    internal sealed class Authority : ISchemaAuthority
    {
        internal bool Available=true;
        internal Action<ISchemaComponentHandle>? OnOwn=null;
        internal readonly HashSet<ISchemaComponentHandle> Types=new(ReferenceEqualityComparer.Instance);
        bool ISchemaAuthority.Owns(ISchemaComponentHandle type){OnOwn?.Invoke(type);return Types.Contains(type);}
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveType(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveElementDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveAttributeDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<bool> ISchemaAuthority.IsSameComponent(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<bool> ISchemaAuthority.IsDerivedFrom(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<bool> ISchemaAuthority.IsAffiliatedWith(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("lookup");
        SchemaResult<ImmutableArray<ISchemaComponentHandle>> ISchemaAuthority.GetComponentClosure(ISchemaComponentHandle a,CancellationToken t)=>throw new Exception("lookup");
    }
    internal sealed class Provenance(Container container,ISchemaComponentHandle declared,ISchemaComponentHandle actual,ISchemaAssessmentOrigin? origin):ISchemaAtomicProvenance
    {
        internal ISchemaComponentHandle Declared=declared,Actual=actual;
        internal ISchemaAssessmentOrigin? Origin=origin;
        ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType {get{container.Trace("declared");return Declared;}}
        ISchemaComponentHandle ISchemaAtomicProvenance.ActualType {get{container.Trace("actual");return Actual;}}
        ISchemaAssessmentOrigin? ISchemaAtomicProvenance.OriginalAssessment {get{container.Trace("origin");return Origin;}}
    }
    internal sealed class Container : ISchemaTypedValue,IEnumerable<SchemaAtomicItem>
    {
        internal ISchemaComponentHandle Governing=null!;
        internal bool List=false,Fast=false,InvalidOut=false,Record=true;
        internal SchemaAtomicItem[] Items=[];
        internal readonly List<string> Reads=[];
        internal string? Fail=null,NullAt=null;
        internal Action<string>? OnRead=null;
        internal int Currents,Disposals,ForbiddenCurrent=int.MaxValue;
        internal void Trace(string stage){if(Record)Reads.Add(stage);OnRead?.Invoke(stage);if(Fail==stage)throw new InvalidOperationException(stage);}
        ISchemaComponentHandle ISchemaTypedValue.GoverningType {get{Trace("governing");return Governing;}}
        bool ISchemaTypedValue.IsList {get{Trace("list");return List;}}
        IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems {get{Trace("items");return NullAt=="items"?null!:this;}}
        bool ISchemaTypedValue.TryGetSingleAtomic(out SchemaAtomicItem item){Trace("fast");item=InvalidOut||Items.Length==0?default:Items[0];return Fast;}
        IEnumerator<SchemaAtomicItem> IEnumerable<SchemaAtomicItem>.GetEnumerator(){Trace("enumerator");return NullAt=="enumerator"?null!:new Enumerator(this);}
        IEnumerator IEnumerable.GetEnumerator()=>((IEnumerable<SchemaAtomicItem>)this).GetEnumerator();
        private sealed class Enumerator(Container owner):IEnumerator<SchemaAtomicItem>
        {
            private int _index=-1;
            /// <inheritdoc/>
            public SchemaAtomicItem Current {get{owner.Currents++;owner.Trace("current");if(_index>=owner.ForbiddenCurrent)throw new Exception("forbidden Current");return owner.Items[_index];}}
            object IEnumerator.Current=>Current;
            /// <inheritdoc/>
            public bool MoveNext(){owner.Trace("move");return ++_index<owner.Items.Length;}
            /// <inheritdoc/>
            public void Reset()=>throw new NotSupportedException();
            /// <inheritdoc/>
            public void Dispose(){owner.Disposals++;owner.Trace("dispose");}
        }
    }
    internal sealed class Fixture : IDisposable
    {
        internal readonly SchemaQuerySession Session;
        internal readonly Input Input = new();
        internal readonly Assessor Assessor = new();
        internal readonly Advisor Advisor;
        internal readonly Handle Governing = new(), Actual = new();
        internal readonly Origin Origin = new();
        internal readonly SchemaQuerySnapshot Snapshot;
        internal Fixture(int cache = 8, int atoms = 8, int digits = 64, int scale = 64)
        {
            Advisor = new(Assessor); Assessor.Advisor = Advisor;
            ((Authority)Assessor.Selected).Types.UnionWith([Governing, Actual]); Advisor.Origins.Add(Origin);
            Session = SchemaQuerySession.CompileXPath("$value", new SchemaQueryLimits(2, cache, atoms, digits, scale), assessor: Assessor).Value;
            Snapshot = Session.OpenSnapshot(Input, Input, Origin).Value;
        }
        internal Container Value(XdmValue value, int count = 1)
        {
            var c = new Container { Governing = Governing, Fast = count == 1, List = count != 1 };
            c.Items = Enumerable.Range(0, count).Select(_ => new SchemaAtomicItem(value, new Provenance(c, Governing, Actual, Origin))).ToArray();
            Advisor.Values.Add(c, (Input, Origin)); return c;
        }
        internal T Run<T>(Func<SchemaQueryOperation,T> work, CancellationToken token = default) => Session.Run([Snapshot],work,token);
        /// <inheritdoc/>
        public void Dispose() => Session.Dispose();
    }
}
