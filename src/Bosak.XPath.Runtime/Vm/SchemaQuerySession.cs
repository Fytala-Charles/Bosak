// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Composes private schema admission with whole evaluation operation ownership.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: private foundation admission (1.35.0)                                          |
//                      | Charles Korthout | 0.2   | 31-10-2026     | Compose issued-value reads and explicit conversion transport (1.36.0)                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3  | 01-11-2026     | Add typed isolated input and guarded atomic result operations. |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4  | 01-11-2026     | Check borrowed provider availability and require typed factory execution. |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5 | 01-11-2026     | Integrate same-session composite leases and operation-stamped typed inputs. |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6 | 01-11-2026     | Admit composite host node roles before child context and evaluation. |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7 | 01-11-2026     | Reuse the captured composite ownership delegate for role validation. |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Private compiler-issued immutable static context; equality never admits an environment.</summary>
internal sealed class SchemaQueryEnvironment
{
    internal const string DescriptorFormat = "bosak-private-static-v1";
    internal SchemaQueryEnvironment(ImmutableDictionary<string, string> namespaces, string descriptor)
    {
        Namespaces = namespaces;
        Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(descriptor)));
    }
    internal ImmutableDictionary<string, string> Namespaces { get; }
    internal string Fingerprint { get; }
}

/// <summary>Directly composes private evidence admission with a frozen compiled evaluator.</summary>
internal sealed class SchemaQuerySession : IDisposable
{
    private readonly SchemaEvidenceKernel _kernel;
    private readonly Func<EvaluationContext, XdmValue> _evaluate;
    private readonly Action? _ensureAvailable;
    private readonly bool _typedInputsOnly;
    private readonly bool _compositeEvaluation;
    private readonly Action<Operation, SchemaOperationInputs>? _compositeAdmission;
    private readonly int _snapshotLimit;

    internal SchemaQuerySession(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate,
        ISchemaEvidenceIssuer issuer, int snapshotLimit, int valueLimit)
        : this(environment, evaluate, issuer, snapshotLimit, valueLimit, null, false) { }

    internal SchemaQuerySession(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate,
        ISchemaEvidenceIssuer issuer, int snapshotLimit, int valueLimit, Action? ensureAvailable, bool typedInputsOnly, bool compositeEvaluation = true,
        Action<Operation, SchemaOperationInputs>? compositeAdmission = null)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(evaluate);
        Environment = environment;
        _evaluate = evaluate;
        _ensureAvailable = ensureAvailable;
        _typedInputsOnly = typedInputsOnly;
        _compositeEvaluation = compositeEvaluation;
        _compositeAdmission = compositeAdmission;
        _snapshotLimit = snapshotLimit;
        _kernel = new SchemaEvidenceKernel(environment, issuer, snapshotLimit, valueLimit);
    }

    internal SchemaQueryEnvironment Environment { get; }
    internal SchemaEvidenceKernel.Evidence? ReadEvidence(XdmValue value) { _ensureAvailable?.Invoke(); return _kernel.ReadValue(value); }
    internal SchemaEvidenceKernel.Snapshot OpenSnapshot(SchemaQueryEnvironment environment, object identity,
        object assessment, Action release, CancellationToken cancellationToken = default)
        { _ensureAvailable?.Invoke(); return _kernel.Open(environment, identity, assessment, release, cancellationToken); }

    internal T Run<T>(SchemaQueryEnvironment environment, SchemaEvidenceKernel.Snapshot snapshot,
        Func<Operation, T> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        _ensureAvailable?.Invoke();
        _kernel.AdmitEnvironment(environment);
        _kernel.CheckSnapshot(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        var operation = new Operation(this, snapshot, snapshot.Enter(), cancellationToken);
        try { return work(operation); }
        catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, operation.Complete); throw; }
        finally { operation.Complete(); }
    }

    internal async Task<T> RunAsync<T>(SchemaQueryEnvironment environment, SchemaEvidenceKernel.Snapshot snapshot,
        Func<Operation, Task<T>> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        _ensureAvailable?.Invoke();
        _kernel.AdmitEnvironment(environment);
        _kernel.CheckSnapshot(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        // This raw task, not any caller's cancellable wait, owns the operation until work completes.
        var operation = new Operation(this, snapshot, snapshot.Enter(), cancellationToken);
        try { return await work(operation).ConfigureAwait(false); }
        catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, operation.Complete); throw; }
        finally { operation.Complete(); }
    }

    private Operation OpenOperation(SchemaQueryEnvironment environment, ImmutableArray<SchemaEvidenceKernel.Snapshot> snapshots, CancellationToken cancellation)
    {
        _ensureAvailable?.Invoke();
        _kernel.AdmitEnvironment(environment);
        if (snapshots.IsDefault || snapshots.Length > _snapshotLimit)
            throw new ArgumentException("Explicit bounded membership is required.", nameof(snapshots));
        var unique = new HashSet<SchemaEvidenceKernel.Snapshot>(ReferenceEqualityComparer.Instance);
        foreach (var snapshot in snapshots)
        {
            _kernel.CheckSnapshot(snapshot);
            if (!unique.Add(snapshot)) throw new ArgumentException("Duplicate snapshot admission.", nameof(snapshots));
        }
        void CheckSession() { _ensureAvailable?.Invoke(); _kernel.AdmitEnvironment(environment); }
        var members = snapshots.Select(snapshot => new SchemaOperationLeaseSet.Member(this,
            () => _kernel.CheckSnapshot(snapshot), snapshot.Enter)).ToImmutableArray();
        var leases = SchemaOperationLeaseSet.Acquire(this, members, _snapshotLimit, CheckSession, cancellation);
        return new Operation(this, snapshots, leases, cancellation);
    }

    internal T Run<T>(SchemaQueryEnvironment environment, ImmutableArray<SchemaEvidenceKernel.Snapshot> snapshots,
        Func<Operation, T> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        var operation = OpenOperation(environment, snapshots, cancellationToken);
        try { return work(operation); }
        catch (Exception primary) { operation.Complete(primary); throw; }
        finally { operation.Complete(); }
    }

    internal async Task<T> RunAsync<T>(SchemaQueryEnvironment environment, ImmutableArray<SchemaEvidenceKernel.Snapshot> snapshots,
        Func<Operation, Task<T>> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        var operation = OpenOperation(environment, snapshots, cancellationToken);
        try { return await work(operation).ConfigureAwait(false); }
        catch (Exception primary) { operation.Complete(primary); throw; }
        finally { operation.Complete(); }
    }

    /// <summary>Closes admissions without disposing the borrowed issuer.</summary>
    public void Dispose() => _kernel.Close();

    /// <summary>Borrowed operation for the complete evaluation/drain or detachment callback.</summary>
    internal sealed class Operation
    {
        private readonly SchemaQuerySession _owner;
        private readonly SchemaEvidenceKernel.Snapshot _snapshot;
        private readonly CancellationToken _cancellationToken;
        private IDisposable? _lease;
        private readonly SchemaOperationLeaseSet? _leaseSet;
        private readonly ImmutableArray<SchemaEvidenceKernel.Snapshot> _snapshots;
        internal Operation(SchemaQuerySession owner, SchemaEvidenceKernel.Snapshot snapshot, IDisposable lease, CancellationToken cancellationToken)
        { _owner = owner; _snapshot = snapshot; _lease = lease; _cancellationToken = cancellationToken; }
        internal Operation(SchemaQuerySession owner, ImmutableArray<SchemaEvidenceKernel.Snapshot> snapshots,
            SchemaOperationLeaseSet leases, CancellationToken cancellation)
        { _owner = owner; _snapshot = null!; _snapshots = snapshots; _leaseSet = leases; _cancellationToken = cancellation; }
        private void RequireMember(SchemaEvidenceKernel.Snapshot snapshot)
        {
            Check();
            if (_leaseSet is null ? !ReferenceEquals(snapshot, _snapshot) : !_snapshots.Any(s => ReferenceEquals(s, snapshot)))
                throw new ArgumentException("Snapshot is not an operation member.", nameof(snapshot));
        }
        private void RequireSingle()
        { if (_leaseSet is not null) throw new NotSupportedException("Composite reads require an explicit source snapshot."); }
        internal XdmValue ReadValue(SchemaEvidenceKernel.Snapshot snapshot, ISchemaIssuedScalar value)
        { RequireMember(snapshot); return _owner._kernel.AttachValue(_owner._kernel.Read(snapshot, value, _cancellationToken)); }
        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller,
            Func<SchemaEvidenceKernel.Snapshot, IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces)
        {
            Check();
            if (_leaseSet is null) throw new NotSupportedException("Explicit source membership requires a composite operation.");
            ArgumentNullException.ThrowIfNull(caller);
            ArgumentNullException.ThrowIfNull(ownsNode);
            foreach (var value in caller.SnapshotVariables().Values)
            {
                if (XdmValue.SchemaTransportMarker(value) is null) continue;
                var evidence = _owner._kernel.ReadValue(value)
                    ?? throw new ArgumentException("Unadmitted scalar evidence.", nameof(caller));
                if (evidence.Snapshot is not null) RequireMember(evidence.Snapshot);
            }
            bool Owns(IXdmNode node)
            {
                Check(); int owners = 0;
                foreach (var snapshot in _snapshots) { if (ownsNode(snapshot, node)) owners++; Check(); }
                return owners == 1;
            }
            Func<IXdmNode, bool> owns = Owns;
            var inputs = caller.CaptureSchemaInputs(owns, maxBindings, maxNamespaces).ForOperation(this, owns);
            Check(); return inputs;
        }
        internal void RequireNodeInput(SchemaOperationInputs inputs, IXdmNode? node)
        {
            Check();
            ArgumentNullException.ThrowIfNull(inputs);
            if (_leaseSet is null || !ReferenceEquals(inputs.OperationIdentity, this))
                throw new ArgumentException("Host roles require this composite operation's inputs.", nameof(inputs));
            if (node is not null && !inputs.OwnsNode(this, node))
                throw new ArgumentException("Host input must belong to exactly one operation source.", nameof(node));
            Check();
        }
        private void Check()
        {
            if (_leaseSet is not null) { _leaseSet.Check(); return; }
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _lease) is null, this);
            _cancellationToken.ThrowIfCancellationRequested();
            _owner._ensureAvailable?.Invoke();
            _owner._kernel.CheckSnapshot(_snapshot);
        }
        internal void ValidateAccess() => Check();
        internal XdmValue ReadValue(ISchemaIssuedScalar value)
        { Check(); RequireSingle(); return _owner._kernel.AttachValue(_owner._kernel.Read(_snapshot, value, _cancellationToken)); }
        internal XdmValue AdmitConversionValue(XdmValue source, ISchemaIssuedScalar constructed)
        {
            Check();
            RequireSingle();
            var evidence = _owner._kernel.ReadValue(source)
                ?? throw new ArgumentException("An admitted source value is required.", nameof(source));
            if (evidence.Snapshot is not null && !ReferenceEquals(evidence.Snapshot, _snapshot))
                throw new ArgumentException("Source belongs to another operation snapshot.", nameof(source));
            return _owner._kernel.AttachValue(_owner._kernel.AdmitConversion(evidence, constructed, _cancellationToken));
        }
        internal SchemaOperationInputs CaptureInputs(EvaluationContext caller, Func<IXdmNode, bool> ownsNode, int maxBindings, int maxNamespaces)
        {
            Check();
            ArgumentNullException.ThrowIfNull(caller);
            RequireSingle();
            var inputs = caller.CaptureSchemaInputs(ownsNode, maxBindings, maxNamespaces, _snapshot);
            Check();
            return inputs;
        }
        internal SchemaAtomicResult EvaluateAtomic(SchemaOperationInputs inputs, int maxAtoms)
        {
            Check();
            ArgumentNullException.ThrowIfNull(inputs);
            if (maxAtoms <= 0) throw new ArgumentOutOfRangeException(nameof(maxAtoms));
            if (_leaseSet is not null && !_owner._compositeEvaluation && _owner._compositeAdmission is null)
                throw new NotSupportedException("This host has no composite source-role admission.");
            if (_leaseSet is not null ? !ReferenceEquals(inputs.OperationIdentity, this) : !ReferenceEquals(inputs.Snapshot, _snapshot))
                throw new ArgumentException("Captured inputs belong to another operation snapshot.", nameof(inputs));
            if (_leaseSet is not null) _owner._compositeAdmission?.Invoke(this, inputs);
            Check();
            return new SchemaAtomicResult(this, _owner._evaluate(inputs.CreateContext()), maxAtoms);
        }
        internal XdmValue Evaluate(EvaluationContext context)
        {
            Check();
            RequireSingle();
            if (_owner._typedInputsOnly) throw new NotSupportedException("Bound host sessions require typed isolated inputs.");
            return _owner._evaluate(context);
        }
        internal SchemaEvidenceKernel.Evidence Read(ISchemaIssuedScalar value)
        { Check(); RequireSingle(); return _owner._kernel.Read(_snapshot, value, _cancellationToken); }
        internal SchemaEvidenceKernel.Evidence AdmitConversion(SchemaEvidenceKernel.Evidence source, ISchemaIssuedScalar constructed)
        { Check(); RequireSingle(); return _owner._kernel.AdmitConversion(source, constructed, _cancellationToken); }
        internal void Complete() { if (_leaseSet is not null) _leaseSet.Complete(); else Interlocked.Exchange(ref _lease, null)?.Dispose(); }
        internal void Complete(Exception primary)
        {
            if (_leaseSet is not null) _leaseSet.Complete(primary);
            else SchemaEvidenceKernel.PreserveFailure(primary, Complete);
        }
    }
}
