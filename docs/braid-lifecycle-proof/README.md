# Deterministic facade lifecycle proof — 29 October 2026

An isolated single-lane facade passes **65 controls across 13 cases**, including actual Braid adapter/resource callbacks and explicitly labeled response/accounting injections. Every case ends with balanced leases and exactly one underlying release/unload request. This is new bounded schedule evidence, not arbitrary concurrency certification or a production Braid session implementation.

## Fresh schedule results

| Schedule | Observed result |
|---|---|
| Ordinary inspection/target/NoChange | Actual adapter succeeds through guarded shared contracts; exact same-name NoChange returns the wrapper. Changed candidates remain refused. |
| Engine facts versus admission | Original engine capability facts remain intact; a separate diagnostic admitted-operation/profile view describes the finite read-only scope and Busy state. No existing product UI/API is changed. |
| Failed/partial response | A fault-injected unsuccessful result retaining raw ports is sanitized to diagnostic data with no authority. This is an injection, not an engine defect. |
| Overlap | While a real callback is blocked, a second same-session call completes promptly with Busy, no queue or second raw dispatch. The original operation later completes and the lane is reusable. |
| Callback reentry | Actual resource callback reentry into inspection and a returned port gets Busy without waiting on itself. Cached target metadata can still be read without engine dispatch while Ready/busy; after close every target getter refuses. |
| Callback close/self-drain | Callback close is nonblocking and leaves the context owned while active. Logical operation context rejects self-drain. An external drain waits until callback return; the late result publishes no authority. |
| Pre-cancellation | Standard cancellation, no lease and no raw inspection dispatch. Session/adapter bootstrap has already occurred; this is not a claim of no assembly loading. |
| Cancellation after admission | The actual callback sees the token signal but remains deliberately barrier-blocked. The operation lease remains owned and new work stays Busy. After release, cancellation propagates and the lease ends once; nonterminal cancellation does not poison the session. |
| Close plus cancellation | Both are retained in trace evidence; no authority is published. Underlying release occurs only after the raw callback/operation ends. |
| Caller stops awaiting | Cancelling a separate waiter does not cancel or release the owned raw operation. It still excludes new work and completes after its barrier is released. |
| Bounded drain | With a proven blocked callback, drain reports incomplete without unloading. New admission remains closed; multiple later drain waiters complete after release, with one underlying release request. |
| Drift before publication | Actual generated engine-file replacement during a callback is detected on publication, quiesces/revokes the session and publishes no authority. Restoring files does not reopen it. |
| Independent sessions | Two separate contexts/readers can each hold a blocked actual call; one completes while the other remains owned. No shared-reader thread-safety guarantee follows. |
| Accounting/cleanup | A direct gate-control lease tolerates repeated Dispose; repeated close does not double-release. All 13 cases finish Closed with created/released lease counts equal and ReleaseRequests=1. Closed means unload requested, not GC completion. |

The initial 64-control receipt is retained; the final receipt adds explicit cancellation-plus-close trace reconciliation for 65. Host build has zero warnings/errors; final execution exits zero within an external 120-second bound. Callback/barrier waits have ten-second bounds. The incomplete-drain case uses a short bound only after an explicit barrier proves the callback remains blocked; elapsed timing is not used to infer race ordering.

## Implementation boundary demonstrated

The gate reserves a private operation lease under its ordering lock, verifies artifact binding outside that lock, then checks state again before dispatch/publication. Raw work, awaits, callbacks and verification I/O do not run under the lifecycle lock. Ready/Quiescing/Closed state and first terminal cause are explicit. Cancellation alone does not release ownership early. Context disposal/release requests occur outside the lock after the active lease ends.

Same-session calls refuse Busy immediately while an operation is active. Metadata is cached shared-contract data rather than a raw-engine getter or an engine-work lease; it remains non-authoritative and is checked against lifecycle state. Engine facts and the diagnostic admission profile are separate, unlike the earlier prototype's narrowed capability descriptor. These are experimental consumer-owned choices, not final public API/type/refusal-code adoption.

The actual callback self-drain check uses a private logical operation scope flowed through ExecutionContext, not thread identity. Normal Task.Run/await flow is exercised. Suppressed execution-context flow, custom schedulers and arbitrary host callback arrangements are not qualified. The session-lane Busy rule still rejects overlapping engine work independently of that scope; no universal self-drain detection claim is made.

## Evidence and limits

Raw control outcomes and ordered per-case state/lease/dispatch/publication events are retained with actual loaded snapshot ledgers. Source/project/compiled assembly and unchanged original factory hashes identify the tested diagnostic. The partial response is fault-injected; duplicate lease disposal is a direct gate check. Other schedules use the actual unchanged adapter and controlled host resource callbacks. The original 54-control facade proof remains distinct historical evidence and was not rerun or overwritten.

This does not implement Braid product composition, all authoring/edit/Apply paths, preview/export/real worker protocol, UI capability mapping, history migration, arbitrary concurrency/reentrancy, malicious in-process isolation, forced in-process termination or unload/GC certification. A drain timeout is explicitly incomplete shutdown. A caller abandoning its wait is not evidence that engine work stopped. The profile still admits only inspection, Rename-target discovery and exact same-name NoChange; no source edit is applied.

Original Braid/core/Schema source/pins and all sealed dossiers remain unchanged. Core 1.34.1 and Schema 0.9 full-unit/conformance results and earlier source/binary/model/runtime/typed evidence remain inherited. No new core API need, request ID or product release is established.

## Next step

Review/checkpoint this proof with its proposed lifecycle contract. Then prepare the consumer-owned adoption proposal for the finite read-only session facade: actual API/refusal/capability mapping, composition ownership and acceptance gates at the unchanged reviewed pin. Keep broader editing/preview/export disabled until separately integrated and tested. Current proof/scope documentation and evidence are uncommitted.
