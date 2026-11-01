# BCAP-01 isolated capture declaration candidate

Core P02 has started. This candidate adds one operation-bound `CaptureTransform` method and one two-property result envelope to the previous R2 declaration proposal. It reuses `XsltCaptureOptions`, `XsltCaptureResult` and actual `ControlledResourceReceipt` values. All proposed bodies deliberately refuse execution. Compilation is a shape check, not C01-C10 runtime qualification or Braid acceptance. Production remains 1.42.0.

## Contract selected for review

The exact compiled executable owns Session. Reject foreign/closed operations before node getters. Every match/global/dynamic node must belong to the operation; distinct, shared and absent source roles remain explicit. Invocation is the only template/mode/base-output source; options Context/InitialTemplate/InitialMode/BaseOutputUri must be absent, RawResult must be false. Context is the sole dynamic-input source and cannot replace captured resource authority. Copy accepted options before dispatch; retain no caller callbacks or mutable services.

All four output/message count/byte controls must be supplied and nonnegative; zero is meaningful. MaxEngineOperations and TimeLimit must be supplied and positive. Compose these with existing stricter gates and cancellation; do not weaken caller limits. No memory, compile-time or mid-expression interruption guarantee is added, and no workload defaults are selected.

Capture retains raw membership/leases through evaluation and serialization. Existing capture machinery supplies detached principal/secondary bytes, production order, effective properties, messages and classified outcomes. Actual per-run receipts are detached after capture without mixing compile or another invocation. Receipt labels/digests are evidence, never authority; the consumer must redact credential-bearing fields before publication/IPC. Partial/Cancelled stays diagnostic. Admission/unsupported errors use SchemaResult; execution failures use Capture's existing failures. Outer operation cleanup retains all errors separately and cannot turn capture failure into success; caller wait cancellation never proves cleanup.

## Remaining executable work

P03 must bind the real compiled stylesheet, session and operation to the existing capture engine/session, enforce the rules above and run C01-C10 from the consolidation cohort. The existing flat atomic session evaluator cannot carry this envelope; an explicit verified capture producer is required. No external-value wrapper or legacy callback bypass is accepted. Record immutable specification evidence before semantic implementation. No production API, numeric implementation, package pin, C45/freeze or release changes are made here.

Owner feature requests were updated locally in all three active checkouts; see [registration hashes](evidence/owner-registration.json). Owner commits/pushes and implementation remain with their owners. Prior O3 Revise and missing Schema returns remain open. Every impacted return must bind this new candidate manifest explicitly; no old acceptance is silently transferred.

Run `python docs/foundation-bcap-candidate/tools/build.py` for isolated compilation and repeat metadata inventory. See [API evidence](evidence/api-0.json) and [input pins](evidence/INPUTS.json).
