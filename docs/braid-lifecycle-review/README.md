# Facade lifecycle contract and schedule-proof review — 29 October 2026

The proposed consumer lifecycle contract and isolated proof are reviewed for checkpoint. No correction was needed. Raw receipts reconcile 65 passing controls across 13 cases. Each case has ordered events, monotonic Ready→Quiescing→Closed state, balanced reservation/end counts and exactly one release request with no active lease. Dispatch/publication events occur only while Ready with an active lease. Source/project/assembly/factory hashes and loaded snapshot approvals match. All thirty-four earlier dossier manifests retain their exact bytes; completed execution was not rerun for this documentation review.

The evidence covers actual adapter/callback schedules for same-session Busy refusal, callback reentry and self-drain, cancellation after admission, separate waiter cancellation, controlled close/drain, drift before publication and two independent sessions/readers. The failed/partial response is fault-injected and duplicate lease disposal is a direct gate check. Those categories remain distinct. Engine capability facts and admitted-operation profile are separate; broader editing remains disabled.

This is finite schedule evidence under normal flowed ExecutionContext, not arbitrary concurrency/reentrancy/UI scheduler safety or universal self-drain detection. Closed means underlying release/unload requested, not garbage collection. Drain timeout means incomplete shutdown with ownership retained; cancelling a waiter does not stop the raw operation. No real Braid product facade, composition, protocol/history/UI migration or native acceptance is included.

Original Braid/core/Schema source/pins and earlier complete qualification remain unchanged. No new core API, request ID, source edit, release or product adoption is inferred. The original raw-port lifetime gap and earlier source/binary/runtime findings remain preserved separately.

Earlier uncommitted/pending-review notices in sealed scope/proof receipts describe capture-time state. [Checkpoint identities](CHECKPOINT.json) record Schema delivery; core delivery resolves through `git log -1 --format=%H -- docs/braid-lifecycle-review/README.md`.

Next: prepare the consumer-owned finite read-only adoption proposal, specifying actual facade/lifetime API and refusal mapping, honest engine/profile presentation, composition ownership and complete consumer/native acceptance gates at the unchanged reviewed pin. Do not enable broader authoring, preview or export from this proof alone.
