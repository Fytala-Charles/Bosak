# Braid engine-session integration design — 29 October 2026

Recommended candidate: **a consumer-owned engine session behind Braid's existing engine-free Application ports, using approved assembly snapshots**. Prove the real read-only inspector across a shared-contract load-context boundary first. Do not select a full production integration until that typed-port proof passes. The standalone string-returning runtime experiment proves a loader technique, not compatibility of Braid's actual objects or lifecycle.

Status: design and next-slice acceptance scope only. Braid remains at its current reviewed 1.26.2 pin, core remains 1.34.1, Schema remains 0.9. No consumer/core implementation, new request ID, public API, protocol, storage migration or product adoption is included.

## Source-backed boundaries

Inspection uses clean Braid 0f1d949e758645f7944f617f2b4613e49161a195. [Selected source identities](evidence/consumer-source-identities.json) pin the contracts. Application references only Braid.Core; Core has no engine project dependency. ISourceInspector returns projected Application records and snapshot-owned ports; opaque authoring target wrappers retain owner/snapshot/port identity. IPreviewExecutor returns byte envelopes, diagnostics and resource receipts without engine types. Desktop currently constructs BosakSourceInspector and WorkerProcessPreviewExecutor directly. PreviewWorkerServer currently constructs BosakPreviewExecutor internally.

PreviewWorkerProtocol version 1 carries request ID, execution profile, retained SourceDocument bytes, engine identity and limits, with a 64 MiB frame bound, 8 MiB document bound, depth bound and strict unknown-member handling. Those existing bounds and refusal semantics must be preserved. This design does not adopt the runtime experiment's diagnostic JSON protocol into Braid.

## Comparison and decision

| Candidate | Benefit | Boundary still requiring proof | Decision |
|---|---|---|---|
| Snapshot-backed adapter session, shared Braid contracts | Fixes the approved engine bytes before lazy use and supports coherent identity across operations. Prior independent experiment demonstrates that binding. | Real adapter/contract casts, opaque target ownership, callbacks, async lifecycle and host composition; actual Braid worker integration. | Preferred candidate for a typed read-only proof. |
| Snapshot-backed preview worker only | Existing process transport already carries projected data, so engine objects need not cross processes. | Leaves in-process inspection/validation/export identity unresolved; different closures could govern the same source workflow. | Useful later vertical slice, insufficient as the whole identity policy. |
| Existing default loader plus deployment/hash mismatch refusal | Smaller composition change, retains current typed objects and packaging. | A disk precheck is not an immutable runtime binding; observation after loading cannot be assumed to prevent unapproved module code. Requires separately bounded deployment/loader assumptions and proof. | Keep as a narrower alternative; do not give it snapshot-equivalent guarantees. |

The chosen next step is not arbitrary DLL sidegrade support. Normal product adoption remains a reviewed rebuild with a selected pin and approved artifact manifest. Runtime mismatch detection is a separate admission requirement. A source revision string and an execution/resource profile must not be conflated with the verified artifact identity.

## Proposed type and ownership boundary

| Payload/assembly | Placement and rule |
|---|---|
| Desktop/Workbench | Remain host-owned. Compose an engine session/factory rather than directly constructing a default-context engine-backed inspector. No UI object enters the engine context. |
| Braid.Application and Braid.Core | Share the already-loaded host assembly instances into the isolated context, after exact approved assembly identity/hash checks. Never load duplicate contract assemblies that merely have the same names. |
| Braid.Infrastructure engine-backed adapter | Candidate isolated component, approved alongside the engine closure. Its public ports implement the shared host interfaces; private engine values remain inside the session. Its inclusion/closure must be verified, not assumed from the prior nine-engine-only experiment. |
| Engine assemblies | Approved snapshot-bound closure, lazy loaded by the session-owned resolver. No engine fallback into the default context and no engine-typed objects serialized or returned across the port. |
| Framework/other dependencies | Framework sharing is explicit; any non-framework dependency needs an approved identity/source decision before admission. Unused licensing/storage components are not force-loaded to compute identity. |
| Projected records/retained bytes | Use shared Application/Core types, immutable copies and original byte/extent semantics. No AST reparsing or replacement parser is introduced. |
| Opaque authoring ports/targets | Retain their original session/snapshot/issuer identity. Never serialize, clone, rebind or equate them by a QName/hash. Refuse cross-session or closed-session use before engine work. |
| Resource readers/cancellation | Shared contract interfaces and framework tokens can cross only through the declared adapter. Preserve existing URI/policy/size limits, cancellation behavior and reentrancy rules. |

The type split is a candidate design: sharing contracts is plausible from the source graph but has not yet been executed with the actual inspector. The prior plugin returned strings and therefore cannot prove this table's real casts/target lifetime. No reflection/private-engine bypass is proposed; a diagnostic may invoke its own public factory compiled against the actual public consumer API.

## Session lifecycle and operation admission

An approved engine session owns its adapter context, artifact identity and operation admission state. Readiness requires the approved engine plus adapter closure and exact shared contract instances. Every engine-backed port entry must check active session, artifact policy and source/snapshot ownership. Disk drift revokes future admission; a successful earlier inspection is not a continuing authority. A revoked or closing session cannot silently readmit after files are restored.

Accepted operations retain an explicit session lifetime until completion; cancellation/closing must prevent new work and define what happens to in-flight work before disposing the context. Port/target wrappers must refuse use after closure without depending on the CLR having collected the context. Requesting unload does not revoke authority by itself and does not prove GC/unload completion. Parallel calls, callbacks/reentrancy and close-versus-dispatch are separate tests, not implied by the single-threaded runtime experiment.

A baseline-only currentness flag or caller-provided Boolean must not become the implementation of this authority. A future consumer-owned capability/lease may express verified admission, but no new public type or final API signature is adopted by this design. Keep validation, entitlement, source freshness, resource controls and artifact admission independent.

## Worker transport and retained results

The real worker must independently admit its artifact/adapter closure before engine dispatch. Its executor composition must use that admitted session instead of unconditionally constructing the existing default-context BosakPreviewExecutor. Parent and response checks must bind the verified artifact identity, request ID, protocol version and execution profile. A matching stamped source pin is insufficient. Preserve process deadlines, cancellation, result freezing, byte bounds and resource denial.

Whether a new identity fits existing string fields or requires a protocol version change is an explicit compatibility decision. Do not add fields to the strict version-1 decoder without designing mixed-version refusal. Keep old/new worker combinations, malformed/mismatched replies and child exit versus result admission separate. No final protocol schema is changed here.

Legacy reports/cases keep their original bytes and recorded identity. They may remain readable as historical evidence under existing supported paths; a legacy or different artifact identity cannot silently authorize current result reuse, export or preview. Define fresh validation/re-execution and an explicit restore/currentness result. Do not rewrite old identities, delete history or implicitly migrate on open. Restricted export must retain original owner, source freshness, one-use and entitlement rules in addition to verified engine identity.

## Bounded next slice: actual read-only typed-port proof

Build an ignored independent diagnostic against the existing baseline Braid public contracts. Keep original Braid/core/Schema source/pins unchanged. Freeze the actual qualified baseline Infrastructure, Application/Core and engine bytes. Share the host's exact Application/Core instances and isolate approved Infrastructure/engine snapshots. Invoke the real BosakSourceInspector through a diagnostic public factory returning shared ISourceInspector, then run InspectAsync on retained source bytes.

Required controls:

1. The returned object casts to the host ISourceInspector and its records are the host Application/Core types, with no duplicate contract copies.
2. Real read-only inspection preserves source bytes, UTF-8/UTF-16 extents, module identity and relevant projected diagnostics compared with the existing direct baseline adapter.
3. Engine assemblies and the adapter bind only approved bytes; no engine is observed in the default load context. The experiment may include a direct control in a separate process to avoid contaminating this check.
4. Duplicated/mismatched shared contracts, changed adapter bytes and mixed/unapproved engine bytes refuse before an admitted inspection.
5. A real module/resource callback uses the existing shared reader contract and limits, cancellation remains observable, and the callback does not transfer engine objects.
6. Demonstrate at least one returned opaque authoring port/target remains session-owned. Keep operations read-only: obtaining/inspecting a target must not apply an edit. Refuse cross-session/closed-session target use through a separately explicit wrapper proof; do not infer this from a successful interface cast.
7. Report in-flight closure, concurrent calls, export, real worker transport, report migration and full production admission as pending unless actually exercised. Preserve every initial refusal/failure and distinguish harness correction from product defects.

Read-only inspection is the first candidate vertical slice. Actual preview/worker and mutating authoring integration follow only after typed-boundary/lifetime evidence. A bounded production slice would then add consumer-owned session composition and read-only inspection admission at the unchanged current pin, with its own Braid registry/ADR, complete consumer tests and applicable native acceptance. It must not imply all editing/export/preview paths have migrated. No core request is needed unless the actual proof reveals a missing public engine capability; use Braid's upstream request workflow if that occurs.

## Evidence and state

The reviewed model (47 controls/six mutants/18 real files), actual runtime experiment (34 controls/nine workers), and source/binary qualifications remain inherited evidence. This integration design adds source inspection and acceptance criteria only: no new consumer execution, implementation or standard-language semantics. All prior sealed dossiers remain unchanged. Core 1.34.1/Schema 0.9 and their complete test/conformance results are unchanged and inherited. Current design/documentation is uncommitted.

Next: execute the isolated real-inspector typed-port proof above, preserving the original product checkout and dependency pin.
