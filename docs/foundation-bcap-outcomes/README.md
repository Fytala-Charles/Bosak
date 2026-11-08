# BCAP-01 capture and cleanup outcomes

The isolated P03 candidate now retains a completed capture when operation cleanup fails and preserves original execution/serialization exceptions alongside classified capture diagnostics. **70 real-engine controls pass twice**, including the prior 48; **366 Runtime and 265 focused XSLT regressions pass** on the final isolated binaries. Candidate/probe builds and the revised 47-type declaration build have zero warnings/errors. Production remains unchanged at 1.42.0.

## Proposed contract and executed behavior

`RunCaptureTransform` is the proposed capture-specific owning entry. It enters one bounded snapshot set, captures and serializes under that exact executable/session/operation, attempts every admission release once, then returns a final local result. Existing operation-borrowing `CaptureTransform` and throwing session `Run` remain available. The prototype uses an internal capture-specific wrapper and private outcome tuples; the public declarations still refuse execution and are not a shipping SDK.

The final outcome separates:

| Channel | Meaning |
|---|---|
| Result | Existing capture bytes/properties/messages/classifications and per-run receipts, when produced; retained even if cleanup failed. |
| ExecutionErrors | Original exceptions observed during execution/serialization, including principal serialization faults swallowed by the old classification-only path; reference-deduplicated in observation order. |
| AdmissionError | Original entry/rollback diagnostic when operation admission fails; existing entry aggregate boundaries remain intact. |
| WorkError | Original error if the operation callback cannot return a capture envelope. |
| CleanupErrors | Every original release exception, in reverse acquisition attempt order. A provider AggregateException remains one original exception object. |
| IsSuccess | True only if capture succeeded and every execution/admission/work/cleanup error channel is empty. Partial and Cancelled are never overall success. |

The owned wrapper has no arbitrary public callback after capture. Its result becomes observable only after the raw synchronous capture and cleanup attempts complete; cancelling a caller wait never produces an early outcome or releases the admitted work. Failed cleanup is release uncertainty, not proof of native disposal, and is never retried automatically. Admission rollback diagnostics remain in AdmissionError rather than being flattened into the later cleanup list.

Exception identity is **local diagnostic identity**. Exceptions and their Data/InnerException graphs can retain provider state or sensitive information; they are not recursively detached, sanitized, newly authorized node access or safe IPC payloads. Captured output bytes and immutable receipt snapshots retain their existing contract. Consumers must explicitly project/redact diagnostic data before export; preserving an original exception object cannot also promise to sanitize its arbitrary graph.

## Evidence and exact API delta

The 22 added controls cover successful completion; original principal serialization exception identity; identity deduplication; capture success or secondary serialization failure combined with two throwing cleanup callbacks; provider aggregate boundaries; closed-admission refusal; original work error before poison-node getters; raw work failure plus a single provider aggregate; and cancelled-wait retention during actual serialization followed by a final drained outcome. Synthetic provider release callbacks exercise actual Core snapshot/lease paths, not native XmlDb failures. [Two runs](evidence/probe-0.json), [regressions](evidence/REGRESSIONS.json), [status](evidence/STATUS.json).

Compared with the previous 46-type capture declaration candidate, this [47-type review candidate](declarations/proposal/Xslt.cs.txt) adds `SchemaXsltExecutable.RunCaptureTransform`, `SchemaXsltCaptureResult.ExecutionErrors`, and one five-property `SchemaXsltRunResult`. All prior declarations remain. [Repeat compiled inventory](declarations/evidence/api-0.json) verifies the additive delta; it does not implement those bodies, accept a public ABI or waive owner review. Previous owner returns retain their original proposal hash and do not automatically cover this revision.

Original-exception collection is opt-in to the private explicit capture path and allocated lazily only after an error. The ordinary capture entry does not enable it. Original internal method/constructor overloads and throwing Run semantics remain; the existing regression binaries bind successfully. The internal nonthrowing completion path preserves exactly-once invalidation and all reverse-order release attempts. The old gate, receipt, source membership and partial/cancelled controls rerun on this same candidate.

## Remaining work and reproduction

P03 remains InProgress for initial-mode/profile coverage, native provider/worker qualification, cold/workload cost and cancellation latency, full solution/conformance qualification before promotion, and Braid's exact revised-contract disposition. The runtime wrapper supports a bounded explicit argument profile; it does not yet implement the public invocation object or arbitrary resource authority. Concurrent/reentrant gate reuse remains unsupported. BCAP-01, R2, freeze and publication stay open.

Read [specification/profile basis](SPEC-BASIS.md), [source delta](evidence/SOURCE-DELTA.json), [loaded binary pins](evidence/BINARY-PINS.json) and [input/archive hashes](evidence/INPUTS.json). Prior cohort receipts are unchanged. `tools/prepare.py` requires the retained controls candidate, verifies its source hashes, creates `.guard-tmp/bcap-outcomes-v3` and refuses overwrite. Run each tool using its full `docs/foundation-bcap-outcomes/` prefix: `tools/run.py`, `tools/regressions.py`, `declarations/tools/build.py`, `tools/seal-inputs.py`, `tools/verify.py`. Source/binary archives are retained and sealing refuses to replace them. No owner files, package pins or production sources were changed.
