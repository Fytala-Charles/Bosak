# Consumer-owned read-only facade and lifecycle contract scope — 29 October 2026

Recommendation: a **finite read-only facade with one active engine-backed call per session**, nonblocking close initiation, explicit admission/publication checks and guarded returned authority. This proposal narrows concurrency deliberately. The reviewed 54-control proof is evidence for its starting point, not implementation of every rule below. No production API signature, diagnostic code or protocol format is finalized here.

Status: proposed Braid-owned contract/next-proof scope. No Braid/core/Schema source, actual pin, storage, worker protocol, release or public API changes. Original Braid remains clean at 0f1d949e758645f7944f617f2b4613e49161a195 and its 1.26.2 pin. [Selected contract hashes](evidence/consumer-source-identities.json) pin this inspection.

## Profile and capability truth

Admit structural read-only inspection, Rename-target discovery for a supported dependency-free principal, and exact same-name NoChange. Do not admit changed candidates, reviews/Apply, other authoring ports, compilation/validation/export or preview/worker execution through this first profile. Existing resource/source-byte limits and language restrictions remain in force.

Separate engine capability facts (what the approved engine reports) from facade admission (what this session/profile permits now). The original SourceAuthoringCapabilities documentation describes engine capabilities and configured limits; do not silently redefine it as session authority. The isolated prototype narrowed its returned descriptor to avoid advertising unsupported edits. An eventual consumer design must explicitly map an informational engine descriptor and a separate admitted-operation/profile view into existing UI/ports; no final property/type is assigned in this scope. Capability display never replaces admission at dispatch.

The profile may be unavailable while busy, closing, revoked or closed even when engine inspection support is true. Disabling UI controls is helpful feedback, not enforcement. Legacy display/version strings remain historical metadata; approved artifact identity, source pin, execution profile and current session state stay separate.

## Proposed lifecycle and linearization

Lifecycle states are Ready, Quiescing and Closed, with an optional active operation while Ready/Quiescing. Quiescing retains the first invalidating reason (explicit close, artifact/binding refusal or fatal session invariant) and records later faults without reopening. Closed means no active operation and underlying context release/unload has been requested; it does not assert garbage collection.

| Event | Required transition/result |
|---|---|
| Admit engine call | Check pre-cancellation, Ready state, no active call, exact owner/profile and verified artifact binding; atomically issue one private operation lease. |
| Overlapping call | Refuse promptly as Busy with no engine dispatch. No implicit queue or blocking wait is introduced. |
| Explicit close | Atomically stop new admission and enter Quiescing. If idle, request underlying release and become Closed; otherwise retain the context until the active lease ends. Close initiation is idempotent/nonblocking. |
| Detected artifact/binding drift | Enter Quiescing, refuse new admission/publication and retain evidence. Restoring files cannot readmit this session; use a fresh verified session. |
| Raw completion | Recheck cancellation, session/operation epoch, owner/source/profile and artifact admission before publishing supported data/guarded authority. A closing/revoked session cannot publish new capabilities. Release the private lease exactly once in finally. |
| Last active lease ends in Quiescing | Request underlying release and become Closed; retain the original reason. Never unload under an active callback/engine call merely because a timer expired. |

Admission, close/revocation and publication decisions need one explicit ordering mechanism. Do not hold that synchronization primitive across raw engine work, await, user callbacks or external I/O. A result admitted before a later close can reach the caller with already-revoked wrappers; those wrappers must refuse calls. Promise guarded authority/currentness, not perpetual validity of a successfully completed Task.

The earlier prototype allowed multiple leases and proved one controlled close schedule. The single-lane Busy policy and all ordering/cancellation/reentrancy rules here are new proposed acceptance work, not relabeled as already tested.

## Cancellation, callback reentry and drain

1. A token already cancelled before admission propagates standard cancellation and acquires no lease. For other entries, closed/revoked state is checked before Busy; neither path forwards engine work.
2. Cancellation after admission signals the existing raw operation/token. It does not release the lease until raw work actually ends, even if the caller stops waiting. Do not unload or publish raw ports while cancellation is pending.
3. If cancellation is observed before publication, publish no authority and report cancellation through the existing documented channel. Close/revocation also prevents authority; record both events when they race rather than treating a cancellation result as continued admission.
4. A callback that calls this same session's engine-backed facade gets immediate Busy (or the terminal refusal if closing), not a wait on itself. This works without relying on a thread ID remaining stable across Task.Run/await. No callback is granted the private active lease merely because it is on the same thread.
5. Close initiation from inside a callback is allowed and nonblocking. A drain await must be performed by an external owner after callback return. A future API must not let a callback synchronously/implicitly await its own active operation's drain; exact detection/refusal API is a consumer design decision requiring proof.
6. A drain timeout reports incomplete shutdown. It does not forcibly release a live in-process context or claim the underlying work stopped. Keep new admission closed and retain the dependency until actual completion. Hard worker termination remains a separate existing process-host concern, outside this profile.

Independent sessions may be investigated separately; this scope does not promise shared resource-reader thread safety, parallel engine execution, UI synchronization, arbitrary callback reentrancy or disposal concurrency. Pre-cancellation and the single controlled in-flight close already have prototype evidence; post-admission cancellation and reentrant/drain schedules still need execution.

## Authority-bearing surface inventory

| Surface | Required behavior |
|---|---|
| Composition/factory | Only the facade/session handle reaches product callers. Raw inspector, raw ports and engine context stay private to trusted composition. Do not offer a public raw escape hatch. |
| Inspection result | Preserve immutable bytes, extents, module identities and diagnostic projections. Guard or remove every Application port property; inventory changes must fail tests until reviewed. Partial/failed/invalid responses may preserve read-only diagnostic data but publish no new ports. |
| Unsupported ports | Explicit null/refusal and an honest profile. Do not reuse raw fields from a record clone by accident. |
| Supported port | Every method checks active session/owner/profile at entry and again before returning authority. Unknown/malformed underlying response fails closed. |
| Target | Wrap each actual target with nonforgeable session and issuer/snapshot ownership. No public raw target/port accessor. Metadata is immutable shared-contract data, not permission to dispatch; getters must refuse after closing/revocation. Prefer captured metadata so getters need not call raw engine objects or acquire an engine-work lane. Artifact freshness is re-established before engine dispatch, not inferred from cached metadata. |
| NoChange result | Return the exact facade target associated with the input, no raw target, review or candidate. Verify the raw result refers to the original underlying target before mapping. |
| Candidate/review/Apply | Not supported. Suppress any unexpected candidate/inspection/review instead of letting nested raw capabilities escape. Broader operations need recursive sanitation and ownership proofs before admission. |
| Retained history | Keep original bytes/identity and allow existing read-only display. A target/port must not be reconstructed from a serialized node ID, range, digest or QName. Legacy/different engine evidence is not current authority. |
| Callback/resource result | Accept only the existing declared reader/byte contracts and limits. Do not serialize arbitrary exception state or callbacks as capability substitutes. Source freshness/resource/entitlement checks remain independent. |

This is cooperative consumer admission, not a sandbox against hostile in-process code or privileged private reflection. Actual CLR unload cannot revoke a live raw reference; facade enforcement and preventing supported public escape are the relevant contract.

Metadata/currentness behavior and refusal mapping need explicit consumer documentation. Existing async port methods can return their typed refusal evidence; cancellation remains cancellation. Target getters have no result envelope, so a documented consumer admission exception/refusal mapping is needed rather than silent defaults. Conceptual names in this document are not new published Braid codes or core exceptions.

## Next bounded proof and acceptance matrix

Extend the ignored facade diagnostic over actual unchanged adapter binaries. Add a deterministic scheduler/blocked resource callback to order transitions; do not use timing alone to infer race safety. Preserve existing 54-control receipts and original raw-port escape as historical controls.

| Schedule/control | Required evidence |
|---|---|
| Ready→admit→publish | Real inspection/target/NoChange succeeds, only guarded authority leaves the facade. |
| Active→second call | Immediate Busy, no second raw dispatch and no queue. |
| Close before admission | Terminal refusal, no raw call. |
| Close during callback | Nonblocking close initiation, no new work, context retained until callback/engine ends. |
| Callback reentrant inspection/port call | Prompt Busy/terminal refusal; no deadlock or second raw dispatch. |
| Callback initiates close | Callback can return; external drain completes afterward. Test self-drain refusal/limitation explicitly. |
| Cancellation before admission | No lease/raw call. |
| Cancellation after admission | Raw operation/token observes cancellation where supported; lease retained to actual completion; no capabilities published. |
| Caller stops awaiting | Underlying operation remains owned; no early unload or new admission based on caller abandonment. |
| Close/revoke before publication | Late result exposes no ports/targets, including partial and fault-injected nested responses. |
| Publication then close | Previously returned wrappers and every target property refuse; immutable history stays readable. |
| Drift during active call | No new dispatch/publication after detection; restore does not reopen. |
| Repeated close/failure/finally | Exactly-once lease release/context-release request; no negative counters or double disposal. |
| Drain bound expires | Incomplete outcome and closed admission, retained active dependency, later completion handled once. |
| New interface/result surface | Explicit inventory failure or sanitized/refused new field, never implicit passthrough. |
| Raw baseline control | Still demonstrates the earlier unload/escaped-port gap independently of facade enforcement. |

Model-only transition tests, simulated faults and actual adapter/callback schedules must be reported separately. Do not claim arbitrary concurrency from a finite schedule matrix. Test engine capability facts and admitted-operation reporting independently; no changed source edits, Apply or preview/export integration are part of this proof.

## Consumer production gate

After proof review, a real Braid slice needs its own registry/ADR and API/refusal/profile documentation, composition/lifetime ownership design and concrete tests at the unchanged reviewed pin. It must cover all actual returned surfaces before exposing them, complete consumer suites, stored-result/worker compatibility for anything touched and applicable native acceptance. Do not transplant this diagnostic into core/Schema or assign a core request ID without an actual missing public engine capability. No such need is established now.

This scope only inspects contracts and defines acceptance. Prior facade/runtime/typed/model/source/binary/full-engine results remain inherited; no new prototype or consumer execution occurred here. Core 1.34.1, Schema 0.9, original consumer checkouts/pins and all sealed dossiers remain unchanged. Current scope/documentation is uncommitted.

Next: execute the deterministic single-lane lifecycle/callback/cancellation proof above in isolation.
