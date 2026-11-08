// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Authenticates optional original schema evidence on neutral source admissions.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
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
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Trusted selected advisor bound to immutable source and schema issuance state.</summary>
internal interface IQuerySchemaAdvisor : ISchemaEvidenceIssuer
{
    /// <summary>The exact compiler-issued immutable environment.</summary>
    SchemaQueryEnvironment Environment { get; }
    /// <summary>Checks the borrowed advisor resource.</summary>
    void ThrowIfUnavailable();
    /// <summary>Checks fact issuance before fact getters.</summary>
    /// <param name="facts">The purported issued facts.</param>
    /// <returns>Whether this advisor issued the exact object.</returns>
    bool OwnsNodeFacts(object facts);
    /// <summary>Checks exact original node association after source admission.</summary>
    /// <param name="node">The admitted node.</param>
    /// <param name="facts">The issued facts.</param>
    /// <param name="input">The exact input.</param>
    /// <param name="originalAssessment">The original assessment.</param>
    /// <returns>Whether the full association is owned.</returns>
    bool OwnsNode(IXdmNode node, object facts, IQuerySourceInput input, object originalAssessment);
    /// <summary>Checks source-specific scalar issuance before any payload getter.</summary>
    /// <param name="value">The purported immutable scalar.</param>
    /// <param name="input">The exact input.</param>
    /// <param name="originalAssessment">The original assessment.</param>
    /// <returns>Whether the exact container is issued for this input and origin.</returns>
    bool OwnsTypedValue(ISchemaIssuedScalar value, IQuerySourceInput input, object originalAssessment);
}

/// <summary>Optional bounded original-scalar association; source ownership remains neutral.</summary>
internal sealed class QuerySchemaAssociation : ISchemaEvidenceIssuer, IDisposable
{
    private readonly object _gate = new();
    private readonly SchemaQueryEnvironment _environment;
    private readonly IQuerySourceInput _input;
    private readonly IQuerySchemaAdvisor _advisor;
    private readonly object _origin;
    private readonly Action _checkSource;
    private readonly int _valueLimit;
    private readonly SchemaEvidenceKernel _kernel;
    private readonly SchemaEvidenceKernel.Snapshot _snapshot;
    private readonly Dictionary<ISchemaIssuedScalar, GuardedScalar> _values = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<SchemaEvidenceKernel.Evidence, ISchemaIssuedScalar> _issued = new(ReferenceEqualityComparer.Instance);
    private bool _closed;

    internal QuerySchemaAssociation(SchemaQueryEnvironment environment, IQuerySourceInput input,
        IQuerySchemaAdvisor advisor, object origin, int valueLimit, Action checkSource, CancellationToken cancellation)
    {
        _environment = environment; _input = input; _advisor = advisor; _origin = origin;
        _checkSource = checkSource; _valueLimit = valueLimit;
        _kernel = new SchemaEvidenceKernel(environment, this, 1, valueLimit);
        try
        {
            Check(); cancellation.ThrowIfCancellationRequested();
            _snapshot = _kernel.Open(environment, input, origin, () => { }, cancellation);
            Check();
        }
        catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, _kernel.Close); throw; }
    }
    internal void Check()
    {
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
        _checkSource(); _advisor.ThrowIfUnavailable();
        if (!ReferenceEquals(_advisor.Environment, _environment)) throw new ArgumentException("Advisor belongs to another compilation environment.");
        _checkSource(); _advisor.ThrowIfUnavailable();
        if (!_advisor.OwnsAssessment(_origin)) throw new ArgumentException("Original assessment is not owned by the selected advisor.");
        _checkSource(); _advisor.ThrowIfUnavailable();
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
    }
    private void CheckRaw(ISchemaIssuedScalar value)
    {
        Check();
        if (!_advisor.OwnsTypedValue(value, _input, _origin)) throw new ArgumentException("Typed scalar is not issued for this exact input and origin.", nameof(value));
        Check();
    }
    private T ReadRaw<T>(ISchemaIssuedScalar value, Func<T> read)
    { CheckRaw(value); var result = read(); CheckRaw(value); return result; }
    internal void ValidateNodeFacts(IXdmNode node, object facts)
    {
        Check(); ArgumentNullException.ThrowIfNull(facts);
        if (!_input.OwnsNode(node)) throw new ArgumentException("Node is foreign to the selected input.", nameof(node));
        Check();
        if (!_advisor.OwnsNodeFacts(facts)) throw new ArgumentException("Facts were not issued by the selected advisor.", nameof(facts));
        Check();
        if (!_advisor.OwnsNode(node, facts, _input, _origin)) throw new ArgumentException("Node/facts/input/origin association is not owned.", nameof(facts));
        Check();
    }
    internal XdmValue ReadScalar(ISchemaIssuedScalar value, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(value); cancellation.ThrowIfCancellationRequested(); CheckRaw(value);
        GuardedScalar guarded;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closed, this);
            if (!_values.TryGetValue(value, out guarded!))
            {
                if (_values.Count >= _valueLimit) throw new InvalidOperationException("Issued scalar registration limit reached.");
                _values.Add(value, guarded = new GuardedScalar(this, value));
            }
        }
        var evidence = _kernel.Read(_snapshot, guarded, cancellation);
        CheckTypes(evidence); CheckRaw(value); cancellation.ThrowIfCancellationRequested();
        lock (_gate) { ObjectDisposedException.ThrowIf(_closed, this); _issued[evidence] = value; }
        var result = _kernel.AttachValue(evidence); Check(); return result;
    }
    private void CheckTypes(SchemaEvidenceKernel.Evidence evidence)
    {
        Check();
        if (!((ISchemaEvidenceIssuer)this).OwnsType(evidence.DeclaredType) || !((ISchemaEvidenceIssuer)this).OwnsType(evidence.ActualType)
            || !ReferenceEquals(evidence.OriginalAssessment, _origin))
            throw new ArgumentException("Scalar type or original assessment evidence is no longer admitted.");
        Check();
    }
    internal bool TryValidateTransport(XdmValue value, out SchemaEvidenceKernel.Evidence? evidence)
    {
        evidence = null;
        if (XdmValue.SchemaTransportMarker(value) is not SchemaEvidenceKernel.Evidence marker || !ReferenceEquals(marker.Snapshot, _snapshot)) return false;
        Check(); ISchemaIssuedScalar scalar;
        lock (_gate) if (!_issued.TryGetValue(marker, out scalar!)) throw new ArgumentException("Unregistered scalar evidence.", nameof(value));
        CheckRaw(scalar); evidence = _kernel.ReadValue(value) ?? throw new ArgumentException("Missing scalar evidence.", nameof(value));
        CheckTypes(evidence); CheckRaw(scalar); return true;
    }
    bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value)
    {
        if (value is not GuardedScalar guarded || !ReferenceEquals(guarded.Owner, this)) return false;
        CheckRaw(guarded.Raw); return true;
    }
    bool ISchemaEvidenceIssuer.OwnsType(object type)
    { Check(); bool result = _advisor.OwnsType(type); Check(); return result; }
    bool ISchemaEvidenceIssuer.OwnsAssessment(object origin)
    { Check(); return ReferenceEquals(origin, _origin); }
    bool ISchemaEvidenceIssuer.IsSameType(object first, object second)
    { Check(); bool result = _advisor.IsSameType(first, second); Check(); return result; }
    /// <summary>Revokes cached evidence without disposing the borrowed advisor.</summary>
    public void Dispose()
    {
        lock (_gate) { if (_closed) return; _closed = true; _values.Clear(); _issued.Clear(); }
        _kernel.Close();
    }
    private sealed class GuardedScalar(QuerySchemaAssociation owner, ISchemaIssuedScalar raw) : ISchemaIssuedScalar
    {
        internal QuerySchemaAssociation Owner => owner;
        internal ISchemaIssuedScalar Raw => raw;
        XdmValue ISchemaIssuedScalar.Value => owner.ReadRaw(raw, () => raw.Value);
        object ISchemaIssuedScalar.DeclaredType => owner.ReadRaw(raw, () => raw.DeclaredType);
        object ISchemaIssuedScalar.ActualType => owner.ReadRaw(raw, () => raw.ActualType);
        object? ISchemaIssuedScalar.OriginalAssessment
        {
            get
            {
                var origin = owner.ReadRaw(raw, () => raw.OriginalAssessment);
                if (!ReferenceEquals(origin, owner._origin)) throw new ArgumentException("Scalar metadata has a different original assessment.");
                return origin;
            }
        }
    }
}
