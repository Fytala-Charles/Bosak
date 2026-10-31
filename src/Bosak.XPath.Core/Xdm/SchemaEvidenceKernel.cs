// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 October 2026
// PURPOSE              : Admits private scalar evidence and manages bounded snapshot lifetimes.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-10-2026     | Creation: private foundation admission (1.35.0)                                          |
//                      | Charles Korthout | 0.2   | 31-10-2026     | Bind private scalar transport and retain stack-owned cached-read leases (1.36.0)         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>Trusted provider operations for the private scalar admission boundary.</summary>
internal interface ISchemaEvidenceIssuer
{
    /// <summary>Checks issuance without accessing purported value contents.</summary>
    /// <param name="value">The purported immutable scalar.</param>
    /// <returns>Whether the exact container was issued.</returns>
    bool OwnsValue(ISchemaIssuedScalar value);
    /// <summary>Checks actual component membership.</summary>
    /// <param name="component">The purported type component.</param>
    /// <returns>Whether the component belongs to this issuer.</returns>
    bool OwnsType(object component);
    /// <summary>Checks original assessment issuance.</summary>
    /// <param name="assessment">The purported original assessment.</param>
    /// <returns>Whether the assessment was issued.</returns>
    bool OwnsAssessment(object assessment);
    /// <summary>Compares two already admitted type components.</summary>
    /// <param name="first">The first admitted component.</param>
    /// <param name="second">The second admitted component.</param>
    /// <returns>Whether the components identify the same type.</returns>
    bool IsSameType(object first, object second);
}

/// <summary>An issuer-owned immutable scalar; never a VM carrier.</summary>
internal interface ISchemaIssuedScalar
{
    /// <summary>The plain atomic value.</summary>
    XdmValue Value { get; }
    /// <summary>The original governing type.</summary>
    object DeclaredType { get; }
    /// <summary>The actual atomic type.</summary>
    object ActualType { get; }
    /// <summary>The original assessment, or null for construction.</summary>
    object? OriginalAssessment { get; }
}

/// <summary>Admits private evidence and owns bounded snapshot registrations.</summary>
internal sealed class SchemaEvidenceKernel
{
    private readonly object _gate = new();
    private readonly object _environment;
    private readonly object _seal = new();
    private readonly ISchemaEvidenceIssuer _issuer;
    private readonly int _snapshotLimit;
    private readonly int _valueLimit;
    private readonly Dictionary<object, Snapshot> _snapshots = new(ReferenceEqualityComparer.Instance);
    private bool _closed;

    internal SchemaEvidenceKernel(object environment, ISchemaEvidenceIssuer issuer, int snapshotLimit, int valueLimit)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(snapshotLimit);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valueLimit);
        _environment = environment;
        _issuer = issuer;
        _snapshotLimit = snapshotLimit;
        _valueLimit = valueLimit;
    }

    internal void AdmitEnvironment(object environment)
    {
        if (!ReferenceEquals(environment, _environment))
            throw new ArgumentException("The compilation environment was not admitted.", nameof(environment));
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
    }

    internal Snapshot Open(object environment, object identity, object assessment, Action release, CancellationToken cancellationToken)
    {
        AdmitEnvironment(environment);
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(release);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_issuer.OwnsAssessment(assessment)) throw new ArgumentException("Unissued assessment.", nameof(assessment));
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closed, this);
            if (_snapshots.ContainsKey(identity)) throw new ArgumentException("Snapshot identity is already admitted.", nameof(identity));
            if (_snapshots.Count >= _snapshotLimit) throw new InvalidOperationException("Snapshot admission limit reached.");
            var snapshot = new Snapshot(this, assessment, _valueLimit, () =>
            {
                lock (_gate) _snapshots.Remove(identity);
                release();
            });
            _snapshots.Add(identity, snapshot);
            return snapshot;
        }
    }

    internal Evidence Read(Snapshot snapshot, ISchemaIssuedScalar value, CancellationToken cancellationToken)
    {
        CheckSnapshot(snapshot);
        var lease = snapshot.EnterRead();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_issuer.OwnsValue(value)) throw new ArgumentException("Unissued scalar container.", nameof(value));
            cancellationToken.ThrowIfCancellationRequested();
            var cached = snapshot.ReadCached(value);
            return cached ?? ReadUncached(snapshot, value, cancellationToken);
        }
        catch (Exception primary)
        {
            try { lease.Dispose(); }
            catch (Exception cleanup) { throw new AggregateException("Operation and owned cleanup both failed.", primary, cleanup); }
            throw;
        }
        finally { lease.Dispose(); }
    }

    private Evidence ReadUncached(Snapshot snapshot, ISchemaIssuedScalar value, CancellationToken cancellationToken)
    {
        CheckSnapshot(snapshot);
        var operation = snapshot.Enter();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Never read a caller's getters until its exact container has been admitted.
            if (!_issuer.OwnsValue(value)) throw new ArgumentException("Unissued scalar container.", nameof(value));
            return snapshot.GetOrCreate(value, () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var data = ReadIssued(value);
                if (!ReferenceEquals(data.Origin, snapshot.Assessment)) throw new ArgumentException("Assessment does not belong to this snapshot.", nameof(value));
                return new Evidence(_seal, data.Value, data.Declared, data.Actual, data.Origin, snapshot);
            });
        }
        catch (Exception primary) { PreserveFailure(primary, operation.Dispose); throw; }
        finally { operation.Dispose(); }
    }

    internal Evidence AdmitConversion(Evidence source, ISchemaIssuedScalar constructed, CancellationToken cancellationToken)
    {
        CheckEvidence(source);
        var operation = source.Snapshot?.Enter();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_issuer.OwnsValue(constructed)) throw new ArgumentException("Unissued constructed value.", nameof(constructed));
            var target = ReadIssued(constructed);
            if (target.Origin is not null) throw new ArgumentException("Construction cannot issue an original assessment.", nameof(constructed));
            cancellationToken.ThrowIfCancellationRequested();
            if (!_issuer.OwnsType(source.DeclaredType) || !_issuer.OwnsType(source.ActualType))
                throw new ArgumentException("Source type evidence is no longer owned.", nameof(source));
            bool same = _issuer.IsSameType(source.DeclaredType, target.Declared)
                && _issuer.IsSameType(source.ActualType, target.Actual)
                && source.Value.Kind == target.Value.Kind
                && source.Value.SchemaTypeName == target.Value.SchemaTypeName
                && source.Value.UserSchemaTypeName == target.Value.UserSchemaTypeName
                && source.Value.SchemaTransportMatches(target.Value);
            source.Snapshot?.Check();
            return same ? source : new Evidence(_seal, target.Value, target.Declared, target.Actual, null, null);
        }
        catch (Exception primary) { if (operation is not null) PreserveFailure(primary, operation.Dispose); throw; }
        finally { operation?.Dispose(); }
    }

    internal static void PreserveFailure(Exception primary, Action release)
    {
        try { release(); }
        catch (Exception cleanup) { throw new AggregateException("Operation and owned cleanup both failed.", primary, cleanup); }
    }

    private (XdmValue Value, object Declared, object Actual, object? Origin) ReadIssued(ISchemaIssuedScalar scalar)
    {
        var declared = scalar.DeclaredType;
        var actual = scalar.ActualType;
        if (declared is null || actual is null || !_issuer.OwnsType(declared) || !_issuer.OwnsType(actual))
            throw new ArgumentException("Unissued type evidence.", nameof(scalar));
        var origin = scalar.OriginalAssessment;
        if (origin is not null && !_issuer.OwnsAssessment(origin)) throw new ArgumentException("Unissued assessment evidence.", nameof(scalar));
        var value = scalar.Value;
        if (XdmValue.SchemaTransportMarker(value) is not null)
            throw new ArgumentException("Issuer values must be plain.", nameof(scalar));
        if (value.Kind is < XdmValueKind.String or > XdmValueKind.Binary)
            throw new ArgumentException("A scalar atomic value is required.", nameof(scalar));
        return (value, declared, actual, origin);
    }

    // Private scalar transport; no public attachment factory.
    internal XdmValue AttachValue(Evidence evidence)
    {
        CheckEvidence(evidence);
        return XdmValue.SchemaTransportAttach(evidence.Value, evidence);
    }

    internal Evidence? ReadValue(XdmValue value)
    {
        AdmitEnvironment(_environment);
        var marker = XdmValue.SchemaTransportMarker(value);
        if (marker is null) return null;
        if (marker is not Evidence evidence) throw new ArgumentException("Unrecognized evidence marker.", nameof(value));
        CheckEvidence(evidence);
        var plain = XdmValue.WithoutSchemaEvidence(value);
        var expected = evidence.Value;
        if (plain.Kind != expected.Kind || plain.SchemaTypeName != expected.SchemaTypeName
            || plain.UserSchemaTypeName != expected.UserSchemaTypeName
            || !plain.SchemaTransportMatches(expected))
            throw new ArgumentException("Rebound evidence payload.", nameof(value));
        return evidence;
    }

    internal void CheckEvidence(Evidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
        if (!evidence.IsIssuedBy(_seal)) throw new ArgumentException("Foreign evidence.", nameof(evidence));
        evidence.Snapshot?.Check();
    }

    internal void CheckSnapshot(Snapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
        if (!ReferenceEquals(snapshot.Owner, this)) throw new ArgumentException("Foreign snapshot.", nameof(snapshot));
        snapshot.Check();
    }

    internal void Close()
    {
        Snapshot[] snapshots;
        lock (_gate)
        {
            if (_closed) return;
            _closed = true;
            snapshots = _snapshots.Values.ToArray();
        }
        List<Exception>? failures = null;
        foreach (var snapshot in snapshots)
            try { snapshot.Dispose(); } catch (Exception error) { (failures ??= []).Add(error); }
        if (failures is not null) throw new AggregateException("Snapshot cleanup failed.", failures);
    }

    /// <summary>Immutable admitted evidence bound to private scalar transport.</summary>
    internal sealed class Evidence
    {
        private readonly object _seal;
        internal Evidence(object seal, XdmValue value, object declared, object actual, object? origin, Snapshot? snapshot)
        { _seal = seal; Value = value; DeclaredType = declared; ActualType = actual; OriginalAssessment = origin; Snapshot = snapshot; }
        internal bool IsIssuedBy(object seal) => ReferenceEquals(_seal, seal);
        internal Snapshot? Snapshot { get; }
        internal XdmValue Value { get; }
        internal object DeclaredType { get; }
        internal object ActualType { get; }
        internal object? OriginalAssessment { get; }
    }

    /// <summary>Revocable snapshot admission; raw operations defer cleanup until completion.</summary>
    internal sealed class Snapshot : IDisposable
    {
        private readonly object _gate = new();
        private readonly int _valueLimit;
        private readonly Dictionary<ISchemaIssuedScalar, Evidence> _values = new(ReferenceEqualityComparer.Instance);
        private Action? _release;
        private int _operations;
        private bool _closing;

        internal Snapshot(SchemaEvidenceKernel owner, object assessment, int valueLimit, Action release)
        { Owner = owner; Assessment = assessment; _valueLimit = valueLimit; _release = release; }
        internal SchemaEvidenceKernel Owner { get; }
        internal object Assessment { get; }
        internal int CachedValues { get { lock (_gate) return _values.Count; } }
        internal void Check() { lock (_gate) ObjectDisposedException.ThrowIf(_closing, this); }
        internal IDisposable Enter()
        {
            lock (_gate) { ObjectDisposedException.ThrowIf(_closing, this); _operations++; return new Lease(this); }
        }
        internal ReadLease EnterRead()
        {
            lock (_gate) { ObjectDisposedException.ThrowIf(_closing, this); _operations++; return new ReadLease(this); }
        }
        // Synchronous stack-owned local only; no interface boxing or async escape.
        internal ref struct ReadLease
        {
            private Snapshot? _snapshot;
            internal ReadLease(Snapshot snapshot) => _snapshot = snapshot;
            internal void Dispose() { var snapshot = _snapshot; _snapshot = null; snapshot?.Leave(); }
        }
        internal Evidence? ReadCached(ISchemaIssuedScalar value)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_closing, this);
                return _values.TryGetValue(value, out var existing) ? existing : null;
            }
        }
        internal Evidence GetOrCreate(ISchemaIssuedScalar value, Func<Evidence> create)
        {
            using var lease = Enter();
            lock (_gate) if (_values.TryGetValue(value, out var existing)) return existing;
            var evidence = create();
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_closing, this);
                if (_values.TryGetValue(value, out var existing)) return existing;
                if (_values.Count >= _valueLimit) throw new InvalidOperationException("Snapshot evidence cache limit reached.");
                _values.Add(value, evidence);
                return evidence;
            }
        }
        private Action? TakeRelease()
        {
            if (!_closing || _operations != 0 || _release is null) return null;
            var release = _release; _release = null; _values.Clear(); return release;
        }
        /// <summary>Revokes new access and schedules owned cleanup after raw operations finish.</summary>
        public void Dispose()
        {
            Action? release;
            lock (_gate) { _closing = true; release = TakeRelease(); }
            release?.Invoke();
        }
        private void Leave()
        {
            Action? release;
            lock (_gate) { _operations--; release = TakeRelease(); }
            release?.Invoke();
        }
        private sealed class Lease(Snapshot snapshot) : IDisposable
        {
            private int _disposed;
            /// <summary>Releases this raw operation once.</summary>
            public void Dispose() { if (Interlocked.Exchange(ref _disposed, 1) == 0) snapshot.Leave(); }
        }
    }
}
