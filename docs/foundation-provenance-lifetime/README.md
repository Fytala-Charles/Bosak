# Provenance policy and operation ownership ? 31 October 2026

**Proposed resolution:** original assessment authority is not inherited automatically by casts or computed values. Current type evidence, original assessment and optional audit lineage are separate concepts. Raw work owns its admission until completion; a canceled wait must not dispose that admission. These are design proposals and bounded diagnostic findings, not production adoption or complete E-04 qualification.

## Immutable specification basis and private extensions

[SPEC-BASIS.json](SPEC-BASIS.json) records exact dated W3C Recommendations: [XPath 3.1, 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-31-20170321/) ?2.4.2 and ?3.14.2, and [XDM 3.1, 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/) ?3.3.1.1?3 and ?5.14. Atomization uses typed values; casts produce values of the target type. PSVI supplies node/type facts and the selected union member contributes to typed-value determination.

The ten [proposed laws](PROPOSED-LAWS.json) are Bosak design rules for representing those boundaries and provider authority; W3C does not define these host capability/lease interfaces. No intentional normative deviation is selected, no full XSD 1.1 support is claimed, and no new production semantic adoption occurs. This step implements a standalone host-composition model only. Final semantic propagation changes require explicit adoption and real-provider cases.

## Concrete signature amendment

The [V2 isolated draft](SIGNATURES-V2.cs.txt) preserves the 13-type shape of the [earlier proposal](../foundation-contract-design/README.md), with one atomic metadata amendment:

```csharp
public interface ISchemaAtomicProvenance
{
    ISchemaComponentHandle DeclaredType { get; }
    ISchemaComponentHandle ActualType { get; }
    ISchemaAssessmentOrigin? OriginalAssessment { get; }
}
```

The two handles describe **current** authority-backed type evidence. A built-in cast must not pretend its output still carries the input's validation claim. An authority-checked user-defined conversion can issue fresh target-type evidence without inventing a document assessment; its construction/validation entry point is still part of the pending advisor/assessment design. Optional audit lineage could describe where a computation came from, but must not authorize a type, revive a resource or become an unbounded mandatory per-value graph.

This replaces the atomic draft's required `Origin` member with nullable `OriginalAssessment`. Original **node** facts retain their required `Origin`; casts never rewrite the original node assessment. Reflection verifies the nullable atomic property and nonnullable node property in the fresh compiled draft. This is a change to an unaccepted proposal, not a change to production APIs or historical sealed evidence.

## Proposed transition rules

- Selection, identity functions, unchanged container transport and ordering retain the selected original value's evidence. Equality of contents does not recreate or merge component authority.
- Atomization reads actual provider facts. No typed value, nilled emptiness, list boundaries and actual union-member type stay distinct; a list's atomics alone cannot represent all governing metadata.
- Casts/constructors and newly computed values have their resulting current type. Input original-assessment authority is not copied automatically, including same-type/equal-value casts. A provider-checked target may issue new evidence; ordinary numeric conversion cannot issue a user-defined type claim.
- Original node PSVI survives accepted preserve/copy/compatible views unchanged. Strip, reassess or refuse intentionally where required. Serialization transfers a representation, not live authority or resource ownership.
- XPath comparison/hash/map-key behavior must remain correct. Representative selection carries only its actual retained value's evidence. CLR default equality remains a separate unresolved compatibility decision.

These rules are proposed, not mechanically enforced in the VM. The prior [27-expression probe](../foundation-vm-carrier-probe/README.md) is observation evidence; existing marker drops at constructors are not automatically regressions. Type-evidence issuance, list/union construction, compatibility admission and exact error mapping still need full C139-01 disposition.

## Controlled lifetime experiment

The diagnostic implements the V2 draft's unchanged `IQueryOperationLifetime` / `IQueryOperationLease` signatures, with a private single-lane host owner, an explicit close/drain coordinator and caller-owned file resources. It uses the exact 1.34.2 package Core DLL for declaration/reference identity, but executes no Core provider or VM lifecycle implementation.

Three runs repeat **12 scenarios and 46 checks**, with eight fresh private file fixtures per run. These counts overlap; they are not 138 independent cases or actual database-generation leases. All runs pass, all expected exceptions are observed, all final stderr records are empty, fixture bytes remain unchanged after creation, and fixture directories are removed. The build succeeds with warnings treated as errors and zero warnings/errors.

| Scenario family | Actual observation in this model |
|---|---|
| Borrowed ownership / idempotence | Admission entry/release and drained state never close the caller's file. Explicit caller disposal releases its exclusive handle once; repeated operation release preserves accounting and the first close reason. |
| Canceled active wait | Raw work is deliberately blocked. Canceling the registered wait leaves one active admission and the actual file locked. Close refuses new work; raw private completion releases admission in finally and does not publish in this controlled closed-before-completion schedule. |
| Deferred/nested values | A dictionary-held callback can read while admitted; new deferred dispatch after close and access through a disposed admission refuse. A copied scalar remains usable. Materializing the container did not detach the file owner. |
| Caller closes early | An operation reference cannot revive a disposed FileStream. Actual reads throw; no implicit reopen occurs. |
| Fault / callback / drain cancellation | Observed raw fault releases admission without closing caller resources. Single-lane reentry refuses, callback close does not deadlock in this schedule, and canceling a registered drain wait retains the active admission/resource. |
| Abandonment | An unconsumed graph does not implicitly drain or release its caller resource. Explicit host release is required; no GC-based release is asserted. |
| Unsafe waiter-owned lease (negative control) | Disposing the raw lease from a canceled waiting scope makes active count zero while raw work is still blocked. Apparent drain permits the host to close the real file prematurely; the resumed raw read then throws ObjectDisposedException. This deliberately incorrect pattern is reproduced and caught. |

The private state model refuses new admission after close and gates new deferred dispatch. Already-started raw work retains its resource until it finishes; data publication is suppressed in the tested schedule. The simple publication check is **not a linearizable arbitrary close/publication-race proof**. Likewise, single-lane refusal is a model policy, not a requirement that all Schema assessors serialize their work.

## Implication for final API placement

Keep raw-operation ownership inside a host/provider operation controller. The waiting caller must not own the only IDisposable that protects running work. The current low-level lease signatures can express a correct pattern but cannot prevent an incorrectly scoped waiter from disposing the lease. Therefore, do not freeze a general public Core operation-owner/close/drain API from this diagnostic alone.

Prefer reusing Braid/provider-owned orchestration behind engine-free ports, exposing only the neutral availability/lifetime boundary real call sites require. If a public operation facade is demonstrated necessary, specify raw completion, separate wait cancellation, borrowed-result use, idempotent close, drain/self-drain and publication authority before choosing its signature. Braid's existing component lifecycle is related input, not replaced or accepted for a new cohort here.

E-04 remains **partial**: real Schema/XmlDb owners, generation leases, worker termination/IPC/brokers, arbitrary races, concurrent disposal, suppressed ExecutionContext/custom schedulers, self-drain, cleanup failure and out-of-memory recovery are not qualified. No physical durability, native UI, general leak or performance guarantee follows from these small file fixtures.

## Replay and next step

[RESULTS.json](RESULTS.json) and raw event traces preserve exact outcomes. `python docs/foundation-provenance-lifetime/tools/run.py` rebuilds a private scratch project from the V2 declarations and exact Core DLL, runs with bounded watchdogs and catches failures. Run it in a copy to preserve sealed outputs. `tools/verify.py` performs data-only verification without rerunning the experiment.

Next reconcile PL-01?10 and the V2 atomic amendment with Schema/XmlDb/Braid requirements, then finish the neutral assessment/advisor and compatible-view signatures. Decide lifetime boundary placement from actual consumer call sites; do not add generic Core ownership APIs solely to mirror this model. Production implementation, normative adoption, budgets, joint signatures and shared freeze remain Pending. Existing production source/pins and sealed receipts are unchanged.
