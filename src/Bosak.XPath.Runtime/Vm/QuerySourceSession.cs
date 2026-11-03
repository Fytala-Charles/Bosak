// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Admits ordinary source snapshots without fabricating schema evidence.
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
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Trusted host-selected source capability; public node getters never prove membership.</summary>
internal interface IQuerySourceInput
{
    IXdmNode Root { get; }
    void ThrowIfUnavailable();
    bool OwnsNode(IXdmNode node);
}

/// <summary>Neutral internal admission layer; optional schema composition is a dependent slice.</summary>
internal sealed class QuerySourceSession : IDisposable
{
    private readonly object _gate = new();
    private readonly Func<EvaluationContext, XdmValue> _evaluate;
    private readonly int _snapshotLimit;
    private readonly Dictionary<IQuerySourceInput, Snapshot> _snapshots = new(ReferenceEqualityComparer.Instance);
    private readonly QuerySnapshotLifetime _lifetime = new(() => { });
    private bool _closing;
    private int _opening;
    private Task? _drain;

    internal QuerySourceSession(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate, int snapshotLimit)
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
            retained = enterOwner() ?? throw new InvalidOperationException("Owner returned no admission lease.");
            input.ThrowIfUnavailable(); cancellation.ThrowIfCancellationRequested();
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_closing, this);
                if (_snapshots.ContainsKey(input)) throw new ArgumentException("Input is already admitted.", nameof(input));
                var lease = retained;
                var snapshot = new Snapshot(this, input, () =>
                {
                    try { lease.Dispose(); }
                    finally { lock (_gate) _snapshots.Remove(input); }
                });
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
    private Operation OpenOperation(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, CancellationToken cancellation)
    {
        CheckEnvironment(environment);
        if (snapshots.IsDefault || snapshots.Length > _snapshotLimit) throw new ArgumentException("Explicit bounded membership required.", nameof(snapshots));
        var unique = new HashSet<Snapshot>(ReferenceEqualityComparer.Instance);
        foreach (var snapshot in snapshots)
            if (snapshot is null || !ReferenceEquals(snapshot.Owner, this) || !unique.Add(snapshot))
                throw new ArgumentException("Members must be unique admissions from this session.", nameof(snapshots));
        var members = ImmutableArray.CreateBuilder<SchemaOperationLeaseSet.Member>(snapshots.Length + 1);
        members.Add(new(this, () => CheckEnvironment(environment), _lifetime.Enter));
        foreach (var snapshot in snapshots) members.Add(new(this, snapshot.Check, snapshot.Lifetime.Enter));
        var leases = SchemaOperationLeaseSet.Acquire(this, members.ToImmutable(), _snapshotLimit == int.MaxValue ? int.MaxValue : _snapshotLimit + 1, () => CheckEnvironment(environment), cancellation);
        return new Operation(this, snapshots, leases);
    }
    internal T Run<T>(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, Func<Operation, T> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work); var operation = OpenOperation(environment, snapshots, cancellation);
        try { return work(operation); }
        catch (Exception primary) { operation.Complete(primary); throw; }
        finally { operation.Complete(); }
    }
    internal async Task<T> RunAsync<T>(SchemaQueryEnvironment environment, ImmutableArray<Snapshot> snapshots, Func<Operation, Task<T>> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work); var operation = OpenOperation(environment, snapshots, cancellation);
        try { return await work(operation).ConfigureAwait(false); }
        catch (Exception primary) { operation.Complete(primary); throw; }
        finally { operation.Complete(); }
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
        lock (_gate) return _drain ?? throw new InvalidOperationException("Close the session before draining.");
    }
    internal sealed class Snapshot : IDisposable
    {
        internal QuerySourceSession Owner { get; }
        internal IQuerySourceInput Input { get; }
        internal QuerySnapshotLifetime Lifetime { get; }
        internal Snapshot(QuerySourceSession owner, IQuerySourceInput input, Action release)
        { Owner = owner; Input = input; Lifetime = new QuerySnapshotLifetime(release); }
        internal void Check() { Lifetime.Check(); Input.ThrowIfUnavailable(); Lifetime.Check(); }
        /// <summary>Revokes access without releasing an active raw-operation resource.</summary>
        public void Dispose() => Lifetime.Dispose();
    }
    internal sealed class Operation
    {
        private readonly QuerySourceSession _owner;
        private readonly ImmutableArray<Snapshot> _snapshots;
        private readonly SchemaOperationLeaseSet _leases;
        internal Operation(QuerySourceSession owner, ImmutableArray<Snapshot> snapshots, SchemaOperationLeaseSet leases)
        { _owner = owner; _snapshots = snapshots; _leases = leases; }
        internal void Check() => _leases.Check();
        private bool OwnsNode(IXdmNode node)
        {
            Check(); int owners = 0;
            foreach (var snapshot in _snapshots) { if (snapshot.Input.OwnsNode(node)) owners++; Check(); }
            return owners == 1;
        }
        internal IXdmNode GetRoot(Snapshot snapshot)
        {
            Check();
            if (!_snapshots.Any(s => ReferenceEquals(s, snapshot))) throw new ArgumentException("Snapshot is not a member.", nameof(snapshot));
            snapshot.Check(); var root = snapshot.Input.Root;
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
        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller, int maxBindings, int maxNamespaces)
        {
            Check(); ArgumentNullException.ThrowIfNull(caller);
            foreach (var value in caller.SnapshotVariables().Values)
                if (XdmValue.SchemaTransportMarker(value) is not null) throw new NotSupportedException("Schema evidence requires an explicitly bound advisor.");
            var inputs = caller.CaptureSchemaInputs(OwnsNode, maxBindings, maxNamespaces).ForOperation(this, OwnsNode);
            Check(); return inputs;
        }
        internal XdmValue EvaluateAtomic(SchemaOperationInputs inputs)
        {
            Check(); ArgumentNullException.ThrowIfNull(inputs);
            if (!ReferenceEquals(inputs.OperationIdentity, this)) throw new ArgumentException("Inputs belong to another operation.", nameof(inputs));
            inputs.ValidateNodeInputs(this);
            var context = inputs.CreateContext(); context.ResourcePolicy = SchemaProviderSessionBinding.ClosedResources();
            var result = _owner._evaluate(context); Check();
            if (result.Kind is not (XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float or XdmValueKind.String or XdmValueKind.Boolean)
                || XdmValue.SchemaTransportMarker(result) is not null)
                throw new NotSupportedException("Only plain copied scalar results are supported by this slice.");
            return result;
        }
        internal void Complete(Exception? primary = null) => _leases.Complete(primary);
    }
}
