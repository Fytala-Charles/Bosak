# Finite read-only Braid consumer adoption proposal — 29 October 2026

Propose an **additive, inspection-only engine-session component at Braid's unchanged reviewed 1.26.2 pin**. The first product candidate returns port-free data. It does not replace the current designer inspector or enable authoring, preview or export through the new session. This is a concrete proposal, with compiled draft API shapes and acceptance gates; no product implementation, dependency adoption or publication is performed.

## Selection and evidence

The reviewed lifecycle proof passes 65 controls over 13 cases; prior facade/typed/runtime proofs establish the relevant starting evidence. They also show why the component needs explicit ownership: raw ports remain callable after an unload request. The first production candidate therefore does not expose authoring ports or targets at all. Rename-target/NoChange remains useful diagnostic proof, not a feature silently admitted into this production scope.

Source inspection is pinned to clean Braid 0f1d949e758645f7944f617f2b4613e49161a195. Existing Desktop/Workbench composition supplies a general ISourceInspector that participates in editing/export; replacing it with a narrower facade would silently remove workflows or misreport capabilities. Keep that composition untouched in the first component slice. A later explicit read-only host/view may opt in after its own UI/capability tests. Do not label existing editing/preview/export as protected by the new session.

The [draft contract](evidence/DraftContracts.cs.txt) compiles independently on net10.0 against the actual existing Application/Core contracts, with XML documentation enabled and zero warnings/errors. It is not compiled into the real Application assembly. Compilation establishes API-shape feasibility only: no runtime implementation, invariants, source/binary compatibility or actual consumer adoption is proved by this new assembly.

## Proposed API and ownership

Draft namespace/type names are reviewable proposals, not adopted public symbols:

| Contract | Proposed role |
|---|---|
| IReadOnlyInspectionSessionFactory.OpenAsync | Accepts finite caller limits and the existing optional IResourceReader. Factory composition privately selects the trusted approved deployment; callers cannot approve DLLs by supplying a digest/manifest. Returns a fresh session or explicit refusal. |
| IReadOnlyInspectionSession.Identity | Immutable process-local session ID, reviewed source revision, verified artifact identity and finite profile ID as separate facts. Never deserialize a session ID into authority. |
| Status | Informational Ready/Quiescing/Closed, whether raw work is active and the first terminal cause. State snapshots do not grant permission. |
| EngineCapabilities | Informational original engine capability facts; not an admitted-operation vector. Profile identity explicitly admits inspection only. No mutation/preview/export operation exists on this interface. |
| InspectAsync | Single-lane read-only operation returning completed projected data or explicit admission refusal. Normal uninspectable source remains completed data with diagnostics, not a successful validation/conformance result. |
| ReadOnlyInspectionData | Source-inspectability, frozen InspectionModule bytes/extents and InspectionIssue diagnostics. No IExpressionEditor, ILiteralElementEditor, target, candidate, review or Apply field. Explicit projection, never a raw SourceInspectionResult passthrough. |
| BeginClose / Dispose | Idempotently stop admission without blocking an active callback. Dispose has the same close-initiation semantics; it does not promise completed drain. |
| DrainAsync | Explicit positive finite bound; returns Drained, Incomplete or Refused. Cancelling this wait does not cancel/disown underlying work. Closed/Drained mean release requested, not CLR collection. |

The draft uses IDisposable plus explicit bounded drain rather than implicit async disposal that could await its own callback. The owner must initiate close and externally drain when it needs shutdown evidence; keep incomplete drain visible and retain ownership until raw completion. A caller abandoning its Task/wait cannot release the operation early.

The interface declarations and record constructors do not enforce runtime mutual-exclusion/invariant rules by themselves. Production factory/publisher code must validate open/reply/drain combinations, finite options, frozen collection ownership and output shape. Client-created records are data, not authority. No arbitrary raw object, assembly, reflection Type or exception.Data payload is part of the projected result.

## Finite profile and refusal mapping

Caller limits may tighten, not expand, the first approved profile: per-module bytes 1–8 MiB, modules 1–64, total bytes 1–32 MiB, with per-module bound no greater than total. Preserve existing URI/resource/XML/DTD/aggregate restrictions. Null reader remains principal-only. Invalid limits/profile return configuration refusal before inspection. Resource refusal remains projected inspection diagnostics where the existing adapter supplies them.

| Trigger | Proposed observable result |
|---|---|
| Valid approved component open | Fresh session with no active call. |
| Artifact/shared-binding failure at open | No session, ArtifactMismatch/BindingMismatch refusal; no raw adapter handed to caller. |
| Overlap or callback reentrant engine call | Prompt Busy reply, no second dispatch and no queue. |
| Closing/closed/revoked entry | Refusal with preserved status/cause, no new data authority. |
| Actual source inspection/diagnostics | Completed port-free data; SourceWasInspectable does not mean compiled/valid XSLT. |
| Malformed/unsafe underlying result | InvalidResponse refusal and quiesce on lost session invariant; no nested capability leakage. |
| Unexpected nonfatal engine failure | Bounded EngineFailure refusal and terminal session fault; preserve diagnostics without raw state. Ordinary source diagnostics are not converted into fatal faults. |
| Standard cancellation observed before publication | OperationCanceledException; private lease remains owned until the raw operation ends. Do not swallow cancellation or fatal OOM conditions into success. |
| Callback attempts its own drain | Refused SelfDrain under the tested logical-operation context; exact product flow/scheduler limitations must be documented and tested. |
| Drain before close / invalid bound | NotClosing/InvalidConfiguration refusal, no ownership change. |
| Drain bound expires | Incomplete with active ownership/admission closed; no forced unload or successful-shutdown claim. |

Names are conceptual draft enum categories, not allocated Braid diagnostic codes. First terminal causes (owner close, artifact mismatch, binding failure, fatal session fault) are distinct from per-call refusals such as Busy. Preserve source pin, artifact identity and execution/resource policy separately. Entitlement/source freshness/resource authority is not granted by matching hashes or successful inspection.

## Composition and implementation candidate

Keep the proposed contracts in Braid.Application and implementation/factory ownership in consumer infrastructure. Share exact host Application/Core instances; isolate the approved engine-backed adapter/engine snapshot closure. Include actual new consumer artifact/contract hashes in the deployment approval—do not reuse the old prototype's Application hash after adding APIs/rebuilding. Existing source pin remains 6a3372fc3b91e733b71a426cf40f26feaaeb3bcb (1.26.2), not the qualified but unadopted 1.34.1 candidate.

Use the proven snapshot-backed resolver as the candidate binding strategy. The diagnostic source is evidence, not production code to transplant unchanged. Review trusted factory/bootstrap assembly ownership, framework/non-framework resolution, immutable snapshots, runtime observations, operation epochs and loader error handling in the actual consumer. No default-context engine fallback is allowed for this component. Do not infer that a default-loader disk precheck offers equivalent binding.

One operation lane per session remains the first concurrency policy. Ready→Quiescing→Closed ordering, no lock across verification I/O/raw work/await/callback, private operation lease, post-await publication checks and exactly-once end/release are required. Suppressed ExecutionContext/custom schedulers and self-drain limitations must be addressed or explicitly refused before claiming wider support. Do not force all engine members to load merely to compute identity; approved snapshots support lazy use.

Project only immutable data into ReadOnlyInspectionData and deep-freeze every nested mutable collection/byte alias. Failed or partial raw results can retain bounded read-only diagnostic data but no ports. Tests must prove all current nine raw port properties are absent from the new DTO and no nested authority-bearing value enters its graph. Contract evolution must fail the inventory tests until reviewed.

## Proposed consumer change set

1. Register the finite inspection-only component and an ADR in Braid, using the next real available consumer IDs when that work is actually authorized. No ID is invented/allocated in this core proposal. An engine/core request is unnecessary unless implementation finds a missing public capability.
2. Add the reviewed API/data projection and private trusted factory/session implementation at the unchanged pin. Implement disposal/drain semantics and explicit refusal mapping, with all non-inspection operations absent. Keep existing Desktop/Workbench default inspector, workers, editing/export and persistence untouched in this component slice.
3. Add actual consumer tests for admission, data projection, alias rejection, source fidelity, lifecycle/cancellation/reentry and all controlled negative schedules. Do not advertise the internal model counts as product coverage.
4. Update actual consumer architecture/integration/registry/handover and version under its MinVer policy (new public feature implies a minor change). No tag, package publication, product upgrade or automatic core merge follows from implementation.
5. Separately review host/UI opt-in, honest capability presentation, incomplete-drain UX and relevant native acceptance before any release or default composition change. Existing editor/export paths remain outside the new guard until explicitly migrated.

## Acceptance before adoption

| Gate | Required result |
|---|---|
| API/build | Actual consumer source and public XML documentation compile normally; no new engine dependency in Application/Core and no private/reflection engine bypass. |
| Fresh full consumer suite | Complete Braid solution passes at the unchanged reviewed pin; exact original and new test identities/counts and legitimate expectation changes are recorded. |
| Real component behavior | Actual factory/session tests exercise the new APIs, exact shared contracts/private snapshots, read-only source bytes/extents/modules and approved artifact refusal. |
| Data-only result/alias safety | No raw ports/targets/reviews/engine types, including failed/partial responses and future fields; caller collection/record copies cannot mutate retained source or grant session authority. |
| Lifecycle | The reviewed schedule matrix runs against the actual component, including overlap, reentry, post-admission cancellation, wait abandonment, close/revoke publication and incomplete drains. Report finite tested limits honestly. |
| Existing workflow regression | Existing authoring, export, preview/worker, report/history and recovery tests remain green. New component does not silently replace current default services. |
| Original binaries/package | Evaluate actual public ABI/package compatibility for touched consumer contracts separately; no blanket inference from isolated source/prototype compilation. |
| Native host/release | Any opted-in host path receives applicable native shutdown/cancellation/read-only UX/accessibility acceptance before release; new default UI is not part of this proposal. |

Core/Schema production is untouched, so their previous complete unit/conformance qualifications remain inherited unless the actual implementation changes those sources. The new proposal compile is fresh; full consumer execution and implementation acceptance have not occurred here.

## Evidence and disposition

Source inputs, standalone contract/project/XML documentation and hashes are retained under evidence/. All earlier sealed dossiers remain unchanged. The standalone draft builds with zero warnings/errors; no implementation, test run, core API change, credential/licensing flow, actual product pin change or production adoption is included. Current proposal/documentation remains uncommitted.

Next: review/checkpoint this concrete adoption proposal, then use it for a consumer-owned implementation task/ADR at the unchanged pin. Production implementation must occur in an explicitly scoped Braid workspace, not in core/Schema or by treating the diagnostic gate as already deployed.
