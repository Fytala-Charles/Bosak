# BCAP-01 real capture feasibility — first cohort

Core P03 is in progress. A persistent isolated source copy now binds the **same compiled stylesheet, session and live operation** to the actual `TransformEngine`, `XsltCaptureSession` and serializer. Production stays 1.42.0. This is a private feasibility adapter, not an implementation or acceptance of the proposed public SDK.

The producer checks exact session ownership and operation-stamped input membership before either source role's getters. Distinct/shared document roots and source-free named templates work. Copied finite capture controls feed the existing capture machinery; raw serialization stays inside the operation callback. A fresh closed resource policy supplies actual per-run immutable receipt snapshots. Unsupported acquisition is reported as `RefusedUnsupported`, rather than fabricated authorization/denial evidence. Credential userinfo is stripped by the existing policy; further consumer publication/IPC redaction remains the owner's responsibility.

## Executed evidence

- **26 real capture checks pass twice**, with identical case results and verified loaded binary hashes. They cover identity/closed/foreign input refusal, distinct/shared/empty source roles, UTF-8 principal/secondary bytes, ordered secondary URIs, effective text output properties, message order, four production bounds, partial/error output, pre-cancelled capture, operation budget, actual resource receipts, separate-run isolation, option conflicts and cancelled-wait retention.
- **366 existing Runtime tests and 265 focused XSLT capture/explicit-context tests pass** against the isolated candidate binaries. These are selected regressions, not a fresh full-solution qualification. Test assemblies are pinned separately from the replaced candidate engines.
- Two observations measure 100 warmed source-free real captures, including operation/context/policy, dedicated worker startup and serialization. Raw process-wide allocation and elapsed time are in the probe receipts; they are neither native workload measurements nor accepted budgets. Compilation/provider acquisition, cold performance and cancellation latency remain unmeasured.
- [Source changes](evidence/SOURCE-DELTA.json), replayable patches, original source hashes, [binary pins](evidence/BINARY-PINS.json), source/binary archive hashes and [cohort status](evidence/STATUS.json) retain the exact experiment. The existing Runtime test identity is reused; reflection occurs once to bind the upper factory delegate at setup, with no new production friend grants.

## Limits and next work

The [case map](evidence/STATUS.json) explicitly leaves C06/C08/C09/C10 partially exercised. The other controls pass only in the bounded synthetic-provider profile. No native Schema or XmlDb authority is established, and none of the ten controls is owner acceptance.

Caller execution gates and resource policies are currently **refused by input capture**, not composed or replaced. Options carry their own capture limits; the producer links capture and operation cancellation. Mid-run operation cancellation/time schedules, inherited stricter gate composition, initial modes, mutation schedules, serialization/provider-release fault injection and all-error ordering need further work. The private tuple adapter has no RawResult input; it cannot establish the complete public invocation/result contract. It also does not supply a public result-plus-cleanup receipt orchestration API.

Capture completes synchronously before the operation callback can end. The lease tests observe resources retained after serialization and through a paused raw callback; cancelling a wait does not release them. They do not inject a pause inside the serializer or prove native thread handoff. Closing a snapshot blocks **new** work even while its raw operation's lease remains retained. Do not equate retained resources with permission to start another capture after close.

Before promotion, finish these cases, actual consumers and the full engine/conformance qualification, then obtain Braid's exact candidate disposition and reconcile revised public declarations/owner hashes. BCAP-01, R2, C45/freeze and publication remain open. External owner registries and product pins are unchanged in this step.

## Reproduction and retained failures

Read [specification/profile basis](SPEC-BASIS.md). On a new candidate workspace run `python docs/foundation-bcap-runtime/tools/prepare.py` (refuses an existing candidate), then `tools/run.py`, `tools/regressions.py`, `tools/seal-inputs.py` and `tools/verify.py` from the same directory prefix. The persistent candidate is under `.guard-tmp/bcap-runtime-v4`; archives retain sources and binaries without modifying production files.

Initial failures are retained separately: an interrupted preparation caused a probe build against incomplete sources; an incorrect receipt enum name was a probe compile error; an eight-namespace ceiling was below the context's actual defaults; and an attempted new capture after snapshot close correctly refused. An initial regression layout omitted QT3 fixtures (315 passed/51 fixture failures); the corrected test layout supplies the original corpus via a junction and all 366 pass. Negative .NET probes use the scoped child error-mode wrapper and catch exceptions; no deliberately unhandled crash or machine-wide setting is used.
