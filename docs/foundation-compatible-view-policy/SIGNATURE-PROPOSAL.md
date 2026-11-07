# CV-ENTRY-01: proposed engine-issued compatible admission

**Proposed, not adopted or implemented.** Current public metadata remains unchanged. This records a concrete signature need instead of declaring the existing raw-view parameter sufficient for future compatibility.

```csharp
// In Bosak.XPath.Api.Schema.SchemaQuerySession:
public SchemaQueryCompatibilityResult OpenCompatibleSnapshot(
    SchemaQuerySnapshot sourceSnapshot,
    CancellationToken cancellationToken = default);

// New API-owned outcome, constructor internal:
public sealed class SchemaQueryCompatibilityResult
{
    public SchemaCompatibilityDecision Decision { get; }
    public SchemaQuerySnapshot? Snapshot { get; }
    public string? DiagnosticCode { get; }
}
```

Compatible has exactly one new target-session snapshot. Incompatible and Unsupported have no snapshot and distinct fixed engine diagnostics. Required arguments, malformed provider answers, foreign/stale evidence and actual cancellation/disposal/cleanup failures retain exceptions. The result is immutable metadata, not another resource owner. Dispose a successful Snapshot using its existing contract; unsuccessful outcomes own no admission. No provider Diagnostics getter or unbounded new diagnostic copying is selected.

The original source snapshot supplies the exact already-admitted input, original assessment and captured source advisor. The target session supplies its actual compiled environment/advisor. Do not add public parameters for arbitrary advisor/view/binding/issuer factories. The exact target advisor optionally implements existing ISchemaCompatibilityAuthority and IQueryOperationLifetime; source advisor optionally implements IQueryOperationLifetime. Missing capability returns Unsupported. No mandatory new provider member is proposed.

Reasons for rejecting alternatives:

- Raw ISchemaCompatibilityView input only exposes self-claimed endpoint getters, not engine-selected issuance.
- SchemaResult<SchemaQuerySnapshot> has no Incompatible category: Unsupported would erase a supported negative proof and Supported-null violates the admitted-result invariant.
- A public standalone view factory cannot by itself express the admitted source lineage, target snapshot retention and release obligations.
- Same-session pointer equality cannot demonstrate compatible independent schema universes.

The proposed selector is exact `$compatible`, with distinct inactive profile/descriptor in POLICY.md. It consumes original singleton source type evidence through the newly admitted target snapshot; it does not silently alter `$value` or existing profiles. New public declarations require a scoped R2/C45 delta disposition; the existing historical closure is not acceptance of this proposal. Internal MC01-MC16 lifetime work can proceed before that disposition.
