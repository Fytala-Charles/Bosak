# All-host context and operation adapter experiment

1 November 2026. Isolated executable adapter over the qualified 1.36.0 binaries, following the [revised declaration proposal](../foundation-c45-revised-shape/README.md). [Basis](SPEC-BASIS.md) preceded execution. Product sources, versions, API exports, owner pins and conformance baselines remain unchanged.

Two guarded processes produce identical [35-check receipts](evidence/run-0.json) and [repeat receipts](evidence/run-1.json), with zero build warnings/errors. Actual loaded assembly hashes match the qualified cohort; all ten canonical engine DLL hashes remain unchanged. [Inputs](evidence/INPUTS.json) pin the four inspected source files and both compiled adapter source files. Four earlier reduced/failed diagnostic receipts are retained separately; their logs are historical observations rather than independently replayable source snapshots.

| Evidence | What actually ran |
|---|---|
| Static context | Copied XPath namespaces, effective XQuery prolog and seeded namespaces after compiler mutation, and XSLT static parameters after compiler mutation. |
| Typed values | All three actual public hosts consume a test node whose lexical value is 999 and typed decimal is 10; results are consumed inside the modeled operation. This is ordinary atomization, not authenticated PSVI. |
| Separate XSLT inputs | Initial match document A and global document B yield A\|B; global-only named invocation yields B; source-free constant template yields 42. |
| Focus correction | Document and element inputs differ in the tested public named-template invocation; see the [explicit correction](FOCUS-CORRECTION.md). |
| Admission model | Foreign, duplicate, closed and unadmitted snapshots, foreign/ended operations, and unsupported invocation profiles refuse. Callback failure releases the modeled lease. |
| Raw work and drain | Snapshot/session close blocks new admission while already admitted work finishes. Canceling a drain wait preserves the pending task/lease. Drain also waits and completes in two successive cycles while the session remains open. |

The [adapter](tools/Adapter.cs.txt) supplies its own Session/Snapshot/Operation model. These are neither proposed public facade implementations nor the production private evidence kernel. Tests run on a controlled async schedule, not concurrently across threads; the model has no synchronization contract, real owner lease acquisition failures, schema assessment, authenticated evidence or recursive dynamic-context graph admission. Its counters demonstrate selected lifetime rules, not resource safety for actual XmlDb/Braid owners. No result graph is automatically detached.

XSLT uses reflection against the pinned internal ten-parameter TransformCaptured overload to pass a separate global context node. The exact reflected signature is recorded in each receipt. This is deliberately not a public API recommendation. The compiled proposal still throws and was not invoked by this experiment. The legacy bridge falls back to source when explicit global context is null, so the adapter rejects match-only invocation under the new independent-absence contract. A future explicit engine-owned bridge is required; no fallback is silently presented as separate absence.

The added focus matrix corrects the earlier broad statement without editing sealed historical evidence. The initial expanded attempt assumed document and element behavior matched and failed in a caught top-level handler. The corrected controls identify each source kind explicitly. All intentional negative cases are caught, with bounded subprocess and async waits and scoped child process error mode. No unhandled crash test or machine-wide setting change occurred.

## Disposition and next task

The tested qualified hosts can execute the selected captured settings and distinct document-input cases. This does not complete RS-05 through RS-10, the earlier 12 C45 cases, or the 43 owner requirements. Mutable package/resolver closure, complete effective-context capture, dynamic input admission, real multi-source leases, exact numeric execution and authenticated typed transforms remain unqualified. C45, consumer acceptance and API freeze remain pending.

Next specify the explicit XSLT context-presence policy against immutable XSLT source sections, then prototype its internal bridge in isolation with document/element, named/matched, absent/required and distinct-source controls. Preserve existing legacy fallback. Settle broader item/sequence/streaming invocation scope before accepting final public declarations. This concrete bridge gap takes precedence over promoting the throwing facade.

Replay with `python -B docs/foundation-host-operation-adapter/tools/run.py`; verify receipts using `python -B docs/foundation-host-operation-adapter/tools/verify.py`. Full product unit/conformance gates were not rerun because product sources/binaries did not change.
