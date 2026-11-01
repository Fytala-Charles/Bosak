# Production typed private host inputs and results

1 November 2026. Production source **1.38.0** adds nonreflective bounded input capture, snapshot-bound requests, guarded atomic visitation and direct XQuery/XSLT host binders. Public declarations and original friend grants remain unchanged. [Adoption basis](SPEC-BASIS.md) preceded source edits; no package publication, C45 acceptance or API freeze occurs here.

| Fresh production qualification | Result |
|---|---:|
| Full solution rebuild | 0 errors; same 5 existing warnings |
| Ten rebuilt unit projects | 24,315 passed, zero failed/skipped |
| New regression cases within that total | 31: Runtime 25, XSLT 6 |
| Runtime / XSLT project totals | 306 / 8,219 passed |
| Database project | 47 passed with loopback access |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Real private-session integration | 81 checks passed twice |
| Full public declaration inventory | 11 assemblies / 270 types / 5,770 rows, unchanged |

Both frozen skip maps and clean corpus pins remain exact. Actual load-time reports verify the rebuilt cohort and test/gate entry assemblies, including XSLT in its suite and both gates. Eight named fn:transform/nested-transform controls pass within the XSLT suite. [Final counters](evidence/RESULTS.json), [unit TRX](evidence/unit-trx.zip), [gate exits](evidence/GATES.json), [execution layouts](evidence/INPUTS.json) and [repeated API inventory](evidence/public-api-0.json) identify the exact production bytes. All eleven product assembly identities are 1.38.0.0; the language server receives a separate no-dependencies build.

## Internal boundary

EvaluationContext.CaptureSchemaInputs explicitly checks advanced state without reflection. It copies focus/position/size, direct scalar or owner-admitted node bindings, namespaces, base URI/default namespaces and collation into SchemaOperationInputs. Nondefault services/registries/formats, unsupported binding kinds, invalid focus/counts, capacity overflow and custom alias-table semantics refuse. A regression checks all 69 context field names so additions require policy review. The caller must provide a stable view and a trusted ownership predicate; copied bindings do not freeze node graphs, admit scalar provenance or issue authority.

Operation.CaptureInputs performs real availability checks and stamps the actual snapshot. EvaluateAtomic rejects unbound or foreign-snapshot requests before constructing a child or invoking the host. Captures may be reused across operations only on the same live snapshot. Operation.Evaluate(EvaluationContext) remains the existing private raw path. Typed evaluation always creates a fresh child; caller context remains unchanged under the tested bounded profile.

SchemaAtomicResult.ForEachAtomic checks actual operation availability before enumeration and each delivered value, enforces a visitation limit and refuses structured results. Empty output visits no values. Visitors consume borrowed atomics inside the callback; evidence is not detached, and arbitrary result graphs are not recursively guarded. Limits apply during consumption and do not bound all allocations/evaluation work in an unknown-length producer. Closing still rejects further access immediately while resource release waits for the raw callback.

The existing public XQuery evaluator delegates to an internal EvaluationContext core. SchemaHostExecution, in the existing upper XSLT layer, binds that core directly and binds explicit-context XSLT with per-invocation root membership checks before node getters. The typed XQuery path creates no XQueryContext wrapper/copy. Runtime/API acquire no concrete host dependency, and no new friend grant is needed. These internal binders are execution bridges, not complete production compilation/assessment factories.

## Allocation evidence

Repeated warmed single-thread measurements, averaged over 1,000 iterations on the same 1.38.0 cohort:

| Component | Allocated bytes/op |
|---|---:|
| Earlier reflective fixture capture, replayed for comparison | 33,461 |
| Typed capture, including actual operation/snapshot checks | 448 |
| Fresh populated EvaluationContext | 2,920 |
| Old additional XQueryContext wrapper/copy, removed from typed execution | 3,744 |
| Guarded single-atomic visitation | 0 |

The 448-byte measurement includes the snapshot identity slot. It excludes creation of the ownership delegate; measurements reuse that delegate. The fresh child cost remains. Compilation, full host evaluation, result-processing callbacks, worker-thread allocation and XSLT stack memory are outside these component measurements; no throughput or consumer workload budget is selected. [Probe receipts](evidence/probe-0.json) and [repeat](evidence/probe-1.json) retain exact values.

The integration fixture uses actual production capture/result code and direct delegates returned by the production host binders. Reflection binds those internal factories once at fixture setup through an existing test identity; it is absent from per-evaluation capture/dispatch. Reflection also remains in diagnostic observations and policy-coverage tests. A test issuer and one composite snapshot remain fixture boundaries: this is not native authenticated PSVI, actual consumer resources or independent multi-source acquisition.

## Review receipts and remaining work

Review added snapshot stamping after the first draft; preliminary source/build/probe receipts remain under pre-snapshot-draft. Final qualification rebuilds and executes the corrected source cohort. The first stamped probe exposed fixture reuse across independent snapshots; it now explicitly recaptures under each owning operation, while new controls verify implicit transfer is refused. Initial compile diagnostics, the corrected incremental build and the final full rebuild are retained separately. No failing tests remain in final qualification.

Next implement complete private compilation/binding factories with effective static-context capture and actual provider/owner contracts. Keep unsupported service profiles explicit and qualify independent multi-source ownership before exposing the public facade. Exact numeric execution, Schema/XmlDb/Braid acceptance, C45 and API freeze remain pending. Source/binary archives retain the tested production cohort with license/NOTICE; informational source revisions may name the pre-commit baseline, so use archive hashes and the packet delivery commit for exact identity.

The [phase runner](tools/run.py) records rebuild, tests and gates; [probe runner](tools/probe.py) records bounded host/cost controls. Replay into a fresh evidence destination. Read-only verification: `python -B docs/foundation-typed-host-inputs/tools/verify.py`. Negative cases are caught with scoped child error mode and bounded waits; no deliberate unhandled crash or machine-wide error setting change occurred.
