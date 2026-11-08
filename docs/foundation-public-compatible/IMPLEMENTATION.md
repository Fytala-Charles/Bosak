# Public handoff and bounded consumption

The public target session is compiled with an explicit assessor and exact trimmed `$compatible`. Unsupported target optional capabilities/options return Unsupported before an admission; missing source lifetime/original assessment and transitive compatible admissions return the distinct Unsupported decision. Null, disposed or malformed source/provider evidence keeps its exception behavior. Incompatible is an owned negative proof rather than Unsupported. Neither negative outcome holds a snapshot or reads provider View/Diagnostics.

```csharp
using var source = SchemaQuerySession.CompileXPath("$value", limits, assessor: sourceAssessor).Value;
using var original = source.OpenSnapshot(input, inputLifetime, originalAssessment).Value;
var compiled = SchemaQuerySession.CompileXPath("$compatible", limits, assessor: targetAssessor);
if (compiled.Kind != SchemaResultKind.Supported)
    throw new NotSupportedException("Target does not support this compatible profile.");
using var target = compiled.Value;
var outcome = target.OpenCompatibleSnapshot(original);
if (outcome.Decision != SchemaCompatibilityDecision.Compatible)
    throw new NotSupportedException(outcome.DiagnosticCode);
using var compatible = outcome.Snapshot!;
var result = target.Run([compatible], operation =>
{
    var read = operation.ReadTypedValue(compatible, sourceTypedValue);
    if (read.Kind != SchemaResultKind.Supported)
        throw new NotSupportedException("Value is outside the selected singleton profile.");
    return operation.Evaluate(new EvaluationContext().WithVariable("compatible", read.Value));
});
```

The example assumes the original source supports typed identity and owns the supplied input/origin/container. Source binding and retained resources remain original. Target proof supplies directional permission for the bounded profile; it does not validate an instance or turn target authority into owner of source type handles. Dispose successful target snapshots using their normal contract; result objects themselves need no disposal.

Each raw operation selects exactly one target-compatible snapshot, authenticates a fresh proof, then creates an ephemeral frame. Public ReadTypedValue/Evaluate and final validation enter the existing paired call serializer. The proof/issuer never enters persistent cache state. Sequential calls can span awaits. Simultaneous/reentrant provider calls within one raw operation fail before invoking provider work; independent raw operations have separate real paired admissions. Source/target admissions remain held until raw completion and cleanup, even while a caller cancels its wait or closes sessions. Actual blocked callbacks/iterators keep owned resources through their complete tail. Final post-cleanup checks use engine session/snapshot lifetimes rather than provider callbacks.

Context capture is closed: one unprefixed compatible binding, absent focus/default runtime namespaces and no services or populated execution state. String/Boolean/Integer/Decimal non-list singleton representation and descriptive annotations survive unchanged. Stored source governing/declared/actual ownership and source container/input/origin issuance are checked under the current raw proof. Lists, empty/multi and other carrier families refuse before scalar commit; existing atom/digit/scale/excess-Current controls apply. Original cache residency is untouched. Transport from the source, another target, a forged marker or an altered payload cannot substitute for this target's resident identity.

TA-24 cache transaction limits remain: fixed insertion failures roll back prefixes; the pre-delivery seam follows staging/group allocation. Revocation after successful installation can leave a complete inaccessible resident group until snapshot close, but cannot authorize delivery or later use without a fresh proof. No aggregate byte/time/parse budget or every-allocation-failure guarantee is claimed.

The selected public delta is limited to the entry and sealed immutable result; no required provider members or new Core value fields were added. The compiler relocates nullable context metadata on SchemaQuerySnapshot while explicitly retaining its public Binding non-null annotation. Exact declarations/attributes are retained in API-DELTA.json; all other exported contracts/references/friends/value layouts are unchanged. Source1.70.0 is a development version; final release SemVer must also account for the previously recorded default-literal overload migration.

CV31 records exclusions rather than native/performance/all-host acceptance. General compatible type/declaration predicates, numeric computation, node traversal/atomization, constructor import, transitive chains, persistent proof and XQuery/XSLT routing are unavailable. BCAP-01, actual XmlDb/Braid/Schema integration and workload-budget evidence remain separate R3/R4 needs. R2 historical returns do not automatically accept this added public delta; C45/freeze and publication need their own exact-candidate dispositions.
