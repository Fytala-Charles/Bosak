// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Admits neutral source snapshots with optional original schema evidence.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
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
//                      | Charles Korthout | 0.2   | 03-11-2026     | Compose optional exact-input original schema evidence (1.44.0).                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 03-11-2026     | Allow open-session drain to observe current raw work (1.44.2).                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 03-11-2026     | Propagate raw cancellation to public source and owner availability (1.45.0).             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 03-11-2026     | Check exact public snapshot membership before unsupported typed reads (1.45.0).          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 04-11-2026     | Retain raw availability checks inside selected compiled arithmetic dispatch (1.48.0).    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 04-11-2026     | Admit typed containers; remove repeated membership closure allocation (1.53.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 04-11-2026     | Own lazy transactional scalar registry and close it with admission (1.54.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 04-11-2026     | Validate private transport through an explicit admitted snapshot (1.55.0).                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 04-11-2026     | Guard an explicitly selected compiled typed identity VM route (1.57.0).                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 04-11-2026     | Share immutable standard functions in fresh identity contexts (1.57.1).                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10   | 05-11-2026     | Activate bounded public typed identity with exact ownership (1.58.0).                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11   | 05-11-2026     | Authorize numeric projection through owned actual type relations (1.59.0).                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12   | 05-11-2026     | Execute bounded public typed numeric operations with live authorization (1.60.0).          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13   | 05-11-2026     | Add uncached original-node facts and exact selected ownership (1.61.0).                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.14   | 05-11-2026     | Allow internal original facts metadata-only reads (1.62.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.15   | 05-11-2026     | Select exactly one raw node owner before public nilled getters (1.63.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.16   | 05-11-2026     | Bridge original node atomization to the scalar registry (1.64.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.17   | 05-11-2026     | Retain raw leases through optional provider call tails (1.65.0). |
//                      | Charles Korthout | 0.18 | 05-11-2026     | Guard source-free construction and operation-local capture (1.66.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.19 | 05-11-2026     | Add internal paired calls and revocable neutral source retention (1.67.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.20 | 05-11-2026     | Separate stable payload scope from current compatible issuer (1.69.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.21 | 05-11-2026     | Bind private host windows to current neutral source operations (1.71.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.22 | 06-11-2026     | Report internal source admission/work/cleanup outcomes without flattening (1.74.0). |
//                      | Charles Korthout | 0.23 | 06-11-2026     | Select inherited gate on explicit host capture only (1.76.0). |
//                      | Charles Korthout | 0.24 | 06-11-2026     | Expose callback-free observation expiry with original admission cancellation (1.77.0). |
//                      | Charles Korthout | 0.25 | 06-11-2026     | Offer opt-in complete stored-metadata validation for borrowed reads (1.79.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Trusted host-selected source capability; public node getters never prove membership.</summary>
internal interface IQuerySourceInput
{
    IXdmNode Root { get; }
    void ThrowIfUnavailable();
    void ThrowIfUnavailable(CancellationToken cancellation)
    { cancellation.ThrowIfCancellationRequested(); ThrowIfUnavailable(); cancellation.ThrowIfCancellationRequested(); }
    bool OwnsNode(IXdmNode node);
}

/// <summary>Neutral internal admission layer with optional original schema evidence.</summary>
internal sealed class QuerySourceSession : IDisposable
{
    private readonly object _gate = new();
    private readonly object _evaluate;
    private readonly int _snapshotLimit;
    private readonly bool _typedIdentity;
    private readonly bool _typedNumeric;
    internal bool TypedNumericProfile=>_typedNumeric;
    private readonly Dictionary<IQuerySourceInput, Snapshot> _snapshots = new(ReferenceEqualityComparer.Instance);
    private readonly QuerySnapshotLifetime _lifetime = new(() => { });
    private bool _closing;
    private int _opening;
    private Task? _drain;

    internal QuerySourceSession(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate, int snapshotLimit)
        : this(environment,evaluate,snapshotLimit,false) { }
    internal QuerySourceSession(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate, int snapshotLimit,bool typedIdentity)
        : this(environment,(object)evaluate,snapshotLimit) { _typedIdentity=typedIdentity; }
    internal QuerySourceSession(SchemaQueryEnvironment environment, Func<EvaluationContext,Action,XdmValue> evaluate, int snapshotLimit)
        : this(environment,evaluate,snapshotLimit,false) { }
    internal QuerySourceSession(SchemaQueryEnvironment environment, Func<EvaluationContext,Action,XdmValue> evaluate, int snapshotLimit,bool typedNumeric)
        : this(environment,(object)evaluate,snapshotLimit) { _typedNumeric=typedNumeric; }
    private QuerySourceSession(SchemaQueryEnvironment environment, object evaluate, int snapshotLimit)
    {
        ArgumentNullException.ThrowIfNull(environment); ArgumentNullException.ThrowIfNull(evaluate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(snapshotLimit);
        Environment = environment; _evaluate = evaluate; _snapshotLimit = snapshotLimit;
    }
    internal SchemaQueryEnvironment Environment { get; }
    private void CheckEnvironment(SchemaQueryEnvironment environment)
    {
        if (!ReferenceEquals(environment, Environment)) throw new ArgumentException("Foreign compilation environment.", nameof(environment));
        lock (_gate) ObjectDisposedException.ThrowIf(_closing, this);
    }
    internal Snapshot OpenSnapshot(SchemaQueryEnvironment environment, IQuerySourceInput input,
        Func<IDisposable> enterOwner, object? originalAssessment = null, CancellationToken cancellation = default)
        => OpenSnapshotCore(environment, input, enterOwner, originalAssessment, cancellation, null);

    // Mechanical retention only: no schema proof, original assessment or compatible view is minted.
    internal Snapshot OpenRetainedSourceSnapshot(SchemaQueryEnvironment environment, Snapshot source, CancellationToken cancellation = default)
    {
        CheckEnvironment(environment); ArgumentNullException.ThrowIfNull(source);
        if (ReferenceEquals(source.Owner, this) || source.RetainedSource is not null)
            throw new NotSupportedException("Retained source admission requires a distinct original source session.");
        return OpenSnapshotCore(environment, source.Input, source.Lifetime.Enter, null, cancellation, source);
    }

    private Snapshot OpenSnapshotCore(SchemaQueryEnvironment environment, IQuerySourceInput input,
        Func<IDisposable> enterOwner, object? originalAssessment, CancellationToken cancellation, Snapshot? retainedSource)
    {
        CheckEnvironment(environment); ArgumentNullException.ThrowIfNull(input); ArgumentNullException.ThrowIfNull(enterOwner);
        if (originalAssessment is not null) throw new NotSupportedException("Original assessment requires an explicitly bound schema advisor.");
        cancellation.ThrowIfCancellationRequested();
        IDisposable openingLease;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closing, this);
            if (_snapshots.ContainsKey(input)) throw new ArgumentException("Input is already admitted.", nameof(input));
            if (_snapshots.Count + _opening >= _snapshotLimit) throw new InvalidOperationException("Snapshot admission limit reached.");
            openingLease = _lifetime.Enter(); _opening++;
        }
        IDisposable? retained = null;
        try
        {
            if (retainedSource is not null)
            {
                // Acquire the real source lifetime before any source input callback can be on stack.
                retainedSource.Owner.CheckEnvironment(retainedSource.Owner.Environment);
                retainedSource.Lifetime.Check();
                CheckEnvironment(environment); cancellation.ThrowIfCancellationRequested();
            }
            retained = enterOwner() ?? throw new InvalidOperationException("Owner returned no admission lease.");
            if (retainedSource is null) input.ThrowIfUnavailable(cancellation);
            else
            {
                CheckEnvironment(environment); cancellation.ThrowIfCancellationRequested();
                CheckRetainedSource(retainedSource, cancellation);
                CheckEnvironment(environment);
            }
            cancellation.ThrowIfCancellationRequested();
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_closing, this);
                if (_snapshots.ContainsKey(input)) throw new ArgumentException("Input is already admitted.", nameof(input));
                var lease = retained;
                var snapshot = new Snapshot(this, input, () =>
                {
                    try { lease.Dispose(); }
                    finally { lock (_gate) _snapshots.Remove(input); }
                }, retainedSource,cancellation);
                _snapshots.Add(input, snapshot); retained = null; return snapshot;
            }
        }
        catch (Exception primary)
        {
            if (retained is not null) SchemaEvidenceKernel.PreserveFailure(primary, retained.Dispose);
            throw;
        }
        finally { lock (_gate) _opening--; openingLease.Dispose(); }
    }
    private static void CheckRetainedSource(Snapshot? source, CancellationToken cancellation)
    {
        if (source is null) return;
        source.Owner.CheckEnvironment(source.Owner.Environment);
        source.CheckSource(cancellation);
        source.Owner.CheckEnvironment(source.Owner.Environment);
    }

    internal Snapshot OpenAssessedSnapshot(SchemaQueryEnvironment environment, IQuerySourceInput input,
        Func<IDisposable> enterOwner, IQuerySchemaAdvisor advisor, object originalAssessment, int valueLimit,
        CancellationToken cancellation = default)
    {
        CheckEnvironment(environment); ArgumentNullException.ThrowIfNull(advisor); ArgumentNullException.ThrowIfNull(originalAssessment);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valueLimit);
        var snapshot = OpenSnapshot(environment, input, enterOwner, cancellation: cancellation);
        try
        {
            Run(environment, [snapshot], operation =>
            {
                var association=new QuerySchemaAssociation(environment,input,advisor,originalAssessment,valueLimit,snapshot.CheckSource,cancellation);
                try{snapshot.Schema=association;}
                catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,association.Dispose);throw;}
                operation.Check(); return 0;
            }, cancellation);
            return snapshot;
        }
        catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, snapshot.Dispose); throw; }
    }

    private ImmutableArray<SchemaOperationLeaseSet.Member> PrepareOperation(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, CancellationToken cancellation)
    {
        CheckEnvironment(environment);
        if (snapshots.IsDefault || snapshots.Length > _snapshotLimit) throw new ArgumentException("Explicit bounded membership required.", nameof(snapshots));
        var unique = new HashSet<Snapshot>(ReferenceEqualityComparer.Instance);
        foreach (var snapshot in snapshots)
            if (snapshot is null || !ReferenceEquals(snapshot.Owner, this) || !unique.Add(snapshot))
                throw new ArgumentException("Members must be unique admissions from this session.", nameof(snapshots));
        var members = ImmutableArray.CreateBuilder<SchemaOperationLeaseSet.Member>(snapshots.Length + 1);
        members.Add(new(this, () => CheckEnvironment(environment), _lifetime.Enter));
        foreach (var snapshot in snapshots) members.Add(new(this, () => snapshot.Check(cancellation), snapshot.Lifetime.Enter));
        return members.ToImmutable();
    }

    private Operation OpenOperation(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, CancellationToken cancellation)
    {
        var members = PrepareOperation(environment, snapshots, cancellation);
        var leases = SchemaOperationLeaseSet.Acquire(this, members, _snapshotLimit == int.MaxValue ? int.MaxValue : _snapshotLimit + 1, () => CheckEnvironment(environment), cancellation);
        return new Operation(this, snapshots, leases, cancellation);
    }
    internal sealed class RunCompletion<T>(bool hasResult, T result, Exception? admissionError,
        Exception? workError, ImmutableArray<Exception> cleanupErrors)
    {
        internal bool HasResult { get; } = hasResult;
        internal T Result { get; } = result;
        internal Exception? AdmissionError { get; } = admissionError;
        internal Exception? WorkError { get; } = workError;
        internal ImmutableArray<Exception> CleanupErrors { get; } = cleanupErrors;
        internal bool IsSuccess => HasResult && AdmissionError is null && WorkError is null && CleanupErrors.IsEmpty;
    }

    internal RunCompletion<T> RunOutcome<T>(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots,
        Func<Operation, T> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        SchemaOperationLeaseSet.Acquisition admission;
        try
        {
            var members = PrepareOperation(environment, snapshots, cancellation);
            admission = SchemaOperationLeaseSet.AcquireOutcome(this, members,
                _snapshotLimit == int.MaxValue ? int.MaxValue : _snapshotLimit + 1,
                () => CheckEnvironment(environment), cancellation);
        }
        catch (Exception error) { return new(false, default!, error, null, []); }
        if (admission.AdmissionError is not null)
            return new(false, default!, admission.AdmissionError, null, admission.CleanupErrors);
        Operation operation;
        try { operation = new Operation(this, snapshots, admission.Leases!, cancellation); }
        catch (Exception error)
        { return new(false, default!, error, null, [.. admission.Leases!.CompleteFailures()]); }
        bool hasResult = false;
        T result = default!;
        Exception? failure = null;
        try { operation.Check(); result = work(operation); hasResult = true; operation.Check(); }
        catch (Exception error) { failure = error; }
        var completion = operation.CompleteOutcomeAsync(failure).GetAwaiter().GetResult();
        return new(hasResult, result, null, completion.Primary, completion.CleanupErrors);
    }

    internal T Run<T>(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, Func<Operation, T> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work); var operation = OpenOperation(environment, snapshots, cancellation);
        T result = default!; Exception? failure = null;
        try { result = work(operation); } catch (Exception error) { failure = error; }
        operation.CompleteAsync(failure).GetAwaiter().GetResult();
        return result;
    }
    internal async Task<T> RunAsync<T>(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, Func<Operation, Task<T>> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work); var operation = OpenOperation(environment, snapshots, cancellation);
        T result = default!; Exception? failure = null;
        try { result = await work(operation).ConfigureAwait(false); } catch (Exception error) { failure = error; }
        await operation.CompleteAsync(failure).ConfigureAwait(false);
        return result;
    }
    /// <summary>Revokes admissions and attempts every snapshot release; raw work retains its leases.</summary>
    public void Dispose()
    {
        Snapshot[] snapshots;
        lock (_gate)
        {
            if (_closing) return;
            _closing = true; snapshots = _snapshots.Values.ToArray();
            _drain = Task.WhenAll(snapshots.Select(s => s.Lifetime.DrainAsync()).Append(_lifetime.DrainAsync()));
        }
        List<Exception>? errors = null;
        for (int i = snapshots.Length - 1; i >= 0; i--)
            try { snapshots[i].Dispose(); } catch (Exception error) { (errors ??= []).Add(error); }
        _lifetime.Dispose();
        if (errors is not null) throw new AggregateException("Snapshot cleanup failed.", errors);
    }
    internal Task DrainAsync()
    {
        lock (_gate) return _drain ?? _lifetime.WaitForIdleAsync();
    }
    internal sealed class Snapshot : IDisposable
    {
        internal QuerySourceSession Owner { get; }
        internal IQuerySourceInput Input { get; }
        internal QuerySnapshotLifetime Lifetime { get; }
        private QuerySchemaAssociation? _schema;
        private int _scalarMode; // 0 undecided, 1 legacy association, 2 transactional registry.
        internal QuerySchemaAssociation? Schema
        {
            get=>Volatile.Read(ref _schema);
            set
            {
                if(value is not null&&Interlocked.CompareExchange(ref _scalarMode,1,0)==2)throw new NotSupportedException("Legacy association cannot share a typed registry.");
                Volatile.Write(ref _schema,value);
            }
        }
        private QueryTypedScalarCache? _typedScalars;
        internal int TypedScalarCount=>Volatile.Read(ref _typedScalars)?.Count??0;
        internal QueryTypedScalarCache RequireTypedScalars()=>Volatile.Read(ref _typedScalars)??throw new ArgumentException("Snapshot has no typed scalar registry.");
        internal bool ContainsTypedTransport(XdmValue value)=>Volatile.Read(ref _typedScalars)?.ContainsTransport(value)==true;
        internal QueryTypedScalarCache GetTypedScalars(IQueryTypedContainerIssuer issuer,Bosak.XPath.Core.Schema.ISchemaAssessmentInput originalInput,Bosak.XPath.Core.Schema.ISchemaAuthority authority,int limit)
        {
            Lifetime.Check();
            if(Interlocked.CompareExchange(ref _scalarMode,2,0)==1)throw new NotSupportedException("Typed registry cannot share a legacy scalar association.");
            var cache=Volatile.Read(ref _typedScalars);
            if(cache is null)
            {
                var candidate=new QueryTypedScalarCache(this,issuer,originalInput,authority,limit);
                cache=Interlocked.CompareExchange(ref _typedScalars,candidate,null)??candidate;
            }
            try{Lifetime.Check();}catch{cache.Close();throw;}
            cache.RequireScope(issuer,originalInput,authority,limit);return cache;
        }
        internal Snapshot? RetainedSource { get; }
        private readonly CancellationToken _observationCancellation;
        internal void CheckObservationActive()
        {
            _observationCancellation.ThrowIfCancellationRequested();Owner.CheckEnvironment(Owner.Environment);Lifetime.Check();
            RetainedSource?.CheckObservationActive();
            _observationCancellation.ThrowIfCancellationRequested();Owner.CheckEnvironment(Owner.Environment);Lifetime.Check();
        }
        internal Snapshot(QuerySourceSession owner, IQuerySourceInput input, Action release, Snapshot? retainedSource = null,CancellationToken observationCancellation=default)
        { Owner = owner; Input = input; Lifetime = new QuerySnapshotLifetime(release); RetainedSource = retainedSource; _observationCancellation=observationCancellation; }
        internal void CheckSource() => CheckSource(default);
        internal void CheckSource(CancellationToken cancellation)
        {
            Lifetime.Check();
            if (RetainedSource is null) Input.ThrowIfUnavailable(cancellation);
            else CheckRetainedSource(RetainedSource, cancellation);
            cancellation.ThrowIfCancellationRequested(); Lifetime.Check();
        }
        internal void Check() => Check(default);
        internal void Check(CancellationToken cancellation) { CheckSource(cancellation); Schema?.Check(); CheckSource(cancellation); }
        /// <summary>Revokes access without releasing an active raw-operation resource.</summary>
        public void Dispose()
        {
            Volatile.Read(ref _typedScalars)?.Close();
            try{Lifetime.Dispose();}
            finally{Volatile.Read(ref _typedScalars)?.Close();Schema?.Dispose();}
        }
    }
    internal sealed class Operation
    {
        private readonly QuerySourceSession _owner;
        private readonly ImmutableArray<Snapshot> _snapshots;
        private readonly SchemaOperationLeaseSet _leases;
        private readonly CancellationToken _cancellation;
        private readonly QueryProviderOperationScope _providerCalls;
        internal Operation(QuerySourceSession owner, ImmutableArray<Snapshot> snapshots, SchemaOperationLeaseSet leases, CancellationToken cancellation)
        { _owner = owner; _snapshots = snapshots; _leases = leases; _cancellation = cancellation;
            _providerCalls = new QueryProviderOperationScope(_leases.Check, cancellation); }
        internal void Check() { _providerCalls.CheckActive(); _leases.Check(); _providerCalls.CheckActive(); }
        internal void RequireOwner(QuerySourceSession owner){Check();if(!ReferenceEquals(owner,_owner))throw new ArgumentException("Operation belongs to another runtime session.");}
        internal T CallHost<T>(object host,Func<Action,T> work)=>_providerCalls.CallHost(host,work);
        internal T CallProvider<T>(Bosak.XPath.Core.Schema.IQueryOperationLifetime provider, Func<Action,T> work, Action<Action>? beforeEnter=null)
            => _providerCalls.Call(provider, work, beforeEnter);
        internal T CallProviderPair<T>(Bosak.XPath.Core.Schema.IQueryOperationLifetime source,
            Bosak.XPath.Core.Schema.IQueryOperationLifetime target, Func<Action,T> work, Action<Action>? beforeEnter=null)
            => _providerCalls.CallPair(source, target, work, beforeEnter);
        internal QueryTypedContainer ReadTypedContainer(Snapshot snapshot,IQueryTypedContainerIssuer issuer,
            Bosak.XPath.Core.Schema.ISchemaAssessmentInput originalInput,Bosak.XPath.Core.Schema.ISchemaAssessmentOrigin? origin,
            Bosak.XPath.Core.Schema.ISchemaTypedValue value,int maxAtoms,int digits,int scale,bool validateStored=false)
        {
            RequireMember(snapshot);
            var reader=new QueryTypedContainerReader(this,snapshot,_owner.Environment,issuer,originalInput,origin,value,maxAtoms,digits,scale,_cancellation);
            var result=reader.Read();
            if(!validateStored)return result;
            var captured=result.Capture();
            void CheckComplete()
            {
                CheckObservationActive();reader.ValidateStored(captured.Governing,captured.Cells);CheckObservationActive();
            }
            CheckComplete();return new QueryTypedContainer(CheckComplete,captured.Governing,captured.List,captured.Cells);
        }
        internal QueryNodeFacts ReadNodeFacts(Snapshot snapshot,IQueryNodeFactsIssuer issuer,
            Bosak.XPath.Core.Schema.ISchemaAssessmentInput input,Bosak.XPath.Core.Schema.ISchemaAssessmentOrigin origin,
            IXdmNode node,Bosak.XPath.Core.Schema.ISchemaNodeFacts facts,int atoms,int digits,int scale,bool metadataOnly=false)
            =>new QueryNodeFactsReader(this,snapshot,_owner.Environment,issuer,input,origin,node,facts,atoms,digits,scale,_cancellation).Read(metadataOnly);
        internal void RequireNodeOwner(Snapshot selected,IXdmNode node)
        {
            RequireMember(selected);
            if(!ReferenceEquals(FindNodeOwner(node),selected))throw new ArgumentException("Node requires exactly the selected admitted owner.",nameof(node));
        }
        internal XdmValue AtomizeOriginalNode(Snapshot snapshot,IQueryNodeFactsIssuer issuer,
            Bosak.XPath.Core.Schema.ISchemaAssessmentInput input,Bosak.XPath.Core.Schema.ISchemaAssessmentOrigin origin,
            IXdmNode node,Bosak.XPath.Core.Schema.ISchemaNodeFacts facts,XdmNodeKind kind,int maxCached,int atoms,int digits,int scale,int faultAfter=-1)
            =>new QueryNodeFactsReader(this,snapshot,_owner.Environment,issuer,input,origin,node,facts,atoms,digits,scale,_cancellation).Atomize(kind,maxCached,faultAfter);
        internal CancellationToken Cancellation=>_cancellation;
        internal Snapshot FindNodeOwner(IXdmNode node)
        {
            Check(); int owners=0; Snapshot? owner=null;
            foreach(var snapshot in _snapshots)
            {
                snapshot.Check(_cancellation); bool owns=snapshot.Input.OwnsNode(node); Check(); snapshot.Check(_cancellation);
                if(owns){owners++;owner=snapshot;}
            }
            if(owners!=1)throw new ArgumentException("Node requires exactly one admitted owner.",nameof(node));
            Check();return owner!;
        }
        internal void CheckObservationActive()
        {
            _providerCalls.CheckActive();_leases.CheckActive();_owner.CheckEnvironment(_owner.Environment);
            _providerCalls.CheckActive();_leases.CheckActive();
        }
        internal void CheckRegistrationActive(Snapshot snapshot)
        {
            _leases.CheckActive();_owner.CheckEnvironment(_owner.Environment);
            bool member=false;foreach(var candidate in _snapshots)if(ReferenceEquals(candidate,snapshot)){member=true;break;}
            if(!ReferenceEquals(snapshot.Owner,_owner)||!member)throw new ArgumentException("Foreign registration snapshot.",nameof(snapshot));
            snapshot.Lifetime.Check();
        }
        internal QueryRegisteredTypedContainer ValidateTypedTransport(Snapshot snapshot,XdmValue value,int maxAtoms,int digits,int scale)
        {
            RequireMember(snapshot);
            return snapshot.RequireTypedScalars().ValidateTransport(this,value,maxAtoms,digits,scale,_cancellation);
        }
        internal QueryRegisteredTypedContainer ValidateTypedTransport(Snapshot snapshot,XdmValue value,IQueryTypedContainerIssuer issuer,int maxAtoms,int digits,int scale)
        {
            RequireMember(snapshot);return snapshot.RequireTypedScalars().ValidateTransport(this,value,maxAtoms,digits,scale,_cancellation,issuer);
        }
        internal XdmValue ProjectTypedNumeric(Snapshot snapshot,XdmValue value,int maxAtoms,int digits,int scale)
        {
            var view=ValidateTypedTransport(snapshot,value,maxAtoms,digits,scale);
            var result=view.ProjectNumeric(value);Check();return result;
        }
        internal XdmValue EvaluateTypedIdentity(Snapshot snapshot,XdmValue value,int maxAtoms,int digits,int scale)
        {
            Check();
            if(!_owner._typedIdentity)throw new NotSupportedException("This session has no typed identity profile.");
            ValidateTypedTransport(snapshot,value,maxAtoms,digits,scale);
            var context=EvaluationContext.CreateIsolated().WithVariable("value",value);
            context.ResourcePolicy=SchemaProviderSessionBinding.ClosedResources();
            Check();var result=((Func<EvaluationContext,XdmValue>)_owner._evaluate)(context);Check();
            ValidateTypedTransport(snapshot,result,maxAtoms,digits,scale);
            if(!ReferenceEquals(XdmValue.SchemaTransportMarker(value),XdmValue.SchemaTransportMarker(result))||!value.SchemaTransportMatches(result))
                throw new ArgumentException("Typed identity changed scalar evidence or representation.");
            Check();return result;
        }
        private Snapshot TypedOwner(XdmValue value)
        {
            Check();Snapshot? owner=null;
            foreach(var snapshot in _snapshots)
                if(snapshot.ContainsTypedTransport(value)){if(owner is not null)throw new ArgumentException("Ambiguous typed scalar owner.");owner=snapshot;}
            if(owner is null)throw new ArgumentException("Typed identity requires exactly one registered member owner.",nameof(value));
            return owner;
        }
        internal XdmValue EvaluatePublicTypedIdentity(XdmValue value,int maxAtoms,int digits,int scale)
            =>EvaluateTypedIdentity(TypedOwner(value),value,maxAtoms,digits,scale);
        internal bool TypedNumericProfile=>_owner._typedNumeric;
        internal XdmValue EvaluatePublicTypedNumeric(XdmValue value,int maxAtoms,int digits,int scale)
        {
            Check();if(!_owner._typedNumeric)throw new NotSupportedException("This session has no typed numeric profile.");
            var owner=TypedOwner(value);var projected=ProjectTypedNumeric(owner,value,maxAtoms,digits,scale);
            void Guard(){_ = ProjectTypedNumeric(owner,value,maxAtoms,digits,scale);}
            var context=EvaluationContext.CreateIsolated().WithVariable("value",projected);context.ResourcePolicy=SchemaProviderSessionBinding.ClosedResources();
            Guard();var result=((Func<EvaluationContext,Action,XdmValue>)_owner._evaluate)(context,Guard);Guard();
            if(XdmValue.SchemaTransportMarker(result) is not null||result.UserSchemaTypeName is not null||result.Kind==XdmValueKind.Boolean&&result.SchemaTypeName is not(null or "boolean"))throw new ArgumentException("Computed numeric results must be plain primitives.");
            if(result.Kind!=XdmValueKind.Boolean)_=new ExactArithmeticLimits(digits,scale).Read(result);
            Guard();return result;
        }
        internal QueryTypedScalarRegistration PrepareTypedRegistration(Snapshot snapshot,IQueryTypedContainerIssuer issuer,
            Bosak.XPath.Core.Schema.ISchemaAssessmentInput originalInput,Bosak.XPath.Core.Schema.ISchemaAssessmentOrigin? origin,
            Bosak.XPath.Core.Schema.ISchemaTypedValue value,int maxCached,int maxAtoms,int digits,int scale)
        {
            RequireMember(snapshot);ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCached);
            var reader=new QueryTypedContainerReader(this,snapshot,_owner.Environment,issuer,originalInput,origin,value,maxAtoms,digits,scale,_cancellation);
            var cache=snapshot.GetTypedScalars(issuer,originalInput,reader.SelectedAuthority,maxCached);
            var existing=cache.Find(value,origin);
            QueryTypedScalarCache.Group group;
            if(existing is not null)group=existing;
            else{var data=reader.Read().Capture();group=new QueryTypedScalarCache.Group(cache,value,origin,data.Governing,data.List,data.Cells);}
            reader.ValidateStored(group.Governing,group.Cells);
            return new QueryTypedScalarRegistration(cache,this,snapshot,reader,group);
        }
        internal QueryRegisteredTypedContainer ReadRegisteredTypedContainer(Snapshot snapshot,IQueryTypedContainerIssuer issuer,
            Bosak.XPath.Core.Schema.ISchemaAssessmentInput originalInput,Bosak.XPath.Core.Schema.ISchemaAssessmentOrigin? origin,
            Bosak.XPath.Core.Schema.ISchemaTypedValue value,int maxCached,int maxAtoms,int digits,int scale)
            =>PrepareTypedRegistration(snapshot,issuer,originalInput,origin,value,maxCached,maxAtoms,digits,scale).Commit();
        private bool OwnsNode(IXdmNode node)
        {
            Check(); int owners = 0;
            foreach (var snapshot in _snapshots) { if (snapshot.Input.OwnsNode(node)) owners++; Check(); }
            return owners == 1;
        }
        internal void RequireMember(Snapshot snapshot)
        {
            Check();
            bool member=false;
            foreach(var candidate in _snapshots)if(ReferenceEquals(candidate,snapshot)){member=true;break;}
            if(!member)throw new ArgumentException("Snapshot is not a member.",nameof(snapshot));
            snapshot.Check(_cancellation);
        }
        internal IXdmNode GetRoot(Snapshot snapshot)
        {
            Check();
            if (!_snapshots.Any(s => ReferenceEquals(s, snapshot))) throw new ArgumentException("Snapshot is not a member.", nameof(snapshot));
            snapshot.Check(_cancellation); var root = snapshot.Input.Root;
            if (root is null || !OwnsNode(root)) throw new ArgumentException("Root must have exactly one admitted owner.", nameof(snapshot));
            Check(); return root;
        }
        internal void RequireNodeInput(SchemaOperationInputs inputs, IXdmNode node)
        {
            Check(); ArgumentNullException.ThrowIfNull(inputs); ArgumentNullException.ThrowIfNull(node);
            if (!ReferenceEquals(inputs.OperationIdentity, this) || !inputs.OwnsNode(this, node))
                throw new ArgumentException("Input requires this exact operation and one owner.", nameof(inputs));
            Check();
        }
        private QuerySchemaAssociation RequireSchema(Snapshot snapshot)
        {
            Check();
            if (!_snapshots.Any(s => ReferenceEquals(s, snapshot))) throw new ArgumentException("Snapshot is not an operation member.", nameof(snapshot));
            return snapshot.Schema ?? throw new NotSupportedException("This ordinary admission has no selected schema advisor.");
        }
        internal void ValidateNodeFacts(Snapshot snapshot, IXdmNode node, object facts)
        {
            var schema = RequireSchema(snapshot);
            if (!OwnsNode(node)) throw new ArgumentException("Node requires exactly one admitted input.", nameof(node));
            schema.ValidateNodeFacts(node, facts); Check();
        }
        internal XdmValue ReadScalar(Snapshot snapshot, ISchemaIssuedScalar value)
        {
            var result = RequireSchema(snapshot).ReadScalar(value, _cancellation); Check(); return result;
        }
        private void ValidateTransport(XdmValue value)
        {
            if (XdmValue.SchemaTransportMarker(value) is null) return;
            if (!_snapshots.Any(s => s.Schema is not null)) throw new NotSupportedException("Schema evidence requires an explicitly bound advisor.");
            int owners = 0;
            foreach (var snapshot in _snapshots)
            {
                if (snapshot.Schema?.TryValidateTransport(value, out _) == true) owners++;
                Check();
            }
            if (owners != 1) throw new ArgumentException("Scalar evidence requires exactly one admitted schema association.", nameof(value));
        }
        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller, int maxBindings, int maxNamespaces,bool inheritExecutionGate=false)
        {
            Check(); ArgumentNullException.ThrowIfNull(caller);
            if(_owner._typedIdentity||_owner._typedNumeric)throw new NotSupportedException("Typed execution requires an explicit authenticated operand route.");
            foreach (var value in caller.SnapshotVariables().Values)
                ValidateTransport(value);
            var inputs = caller.CaptureSchemaInputs(OwnsNode, maxBindings, maxNamespaces,allowExecutionGate:inheritExecutionGate).ForOperation(this, OwnsNode);
            Check(); return inputs;
        }
        internal XdmValue EvaluateAtomic(SchemaOperationInputs inputs)
        {
            Check(); ArgumentNullException.ThrowIfNull(inputs);
            if(_owner._typedIdentity||_owner._typedNumeric)throw new NotSupportedException("Typed execution does not admit general evaluation inputs.");
            if (!ReferenceEquals(inputs.OperationIdentity, this)) throw new ArgumentException("Inputs belong to another operation.", nameof(inputs));
            inputs.ValidateNodeInputs(this);
            var context = inputs.CreateContext();
            foreach (var pair in inputs.VariableBindings) ValidateTransport(pair.Value);
            context.ResourcePolicy = SchemaProviderSessionBinding.ClosedResources();
            var result = _owner._evaluate is Func<EvaluationContext,Action,XdmValue> guarded
                ? guarded(context,Check) : ((Func<EvaluationContext,XdmValue>)_owner._evaluate)(context); Check();
            if (result.Kind is not (XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float or XdmValueKind.String or XdmValueKind.Boolean))
                throw new NotSupportedException("Only copied scalar results are supported by this slice.");
            ValidateTransport(result); Check(); return result;
        }
        internal Task<QueryProviderOperationScope.Completion> CompleteOutcomeAsync(Exception? primary = null)
            => _providerCalls.CompleteOutcomeAsync(_leases.CompleteFailures, primary);
        internal Task CompleteAsync(Exception? primary = null) => _providerCalls.CompleteAsync(_leases.CompleteFailures, primary);
    }
}
