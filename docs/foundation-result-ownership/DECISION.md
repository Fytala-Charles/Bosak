# FG-04 — Proposed result ownership disposition

30 October 2026. **Proposed core disposition; joint owner acceptance pending.** Production source is pinned at `4427cde5` (full revision/file hashes in [source-pins.json](evidence/source-pins.json)). This is an existing-behavior assessment, not a new normative adoption or API signature acceptance. It advances [F140-01](../FOUNDATION-API-FREEZE-PLAN.md) and compatibility cases FC-07/08/09/11/16 without editing delegated owner projects.

## Recommended baseline

Keep existing node/value/sequence contracts and layouts unchanged. Treat an evaluation result as a potentially lazy graph of borrowed provider values, not as an implicitly disposable owner or a detached snapshot. The host/provider must retain and explicitly release the actual resource owner for the entire supported use of that graph, including nested and deferred access. A strong CLR reference retains an object; it does not revive a disposed resource or certify concurrent use.

This is a proposed foundation contract, not a claim that core currently checks every access. An actual provider must state and enforce its own post-disposal behavior. XmlDb owns snapshot/reader/generation lease guarantees; Schema owns assessment/session/provenance lifetime. Their independent qualification remains necessary.

Do not add a required Dispose/cancellation member to IXdmNode, IXdmSequence or IXdmSequenceEnumerator as a cleanup shortcut. The existing binary surface has no universal owner-transfer protocol. Do not make a materialized list or result-wrapper Dispose recursively close arbitrary caller nodes, functions or external objects. Materialization alone does not detach contained nodes or deep-copy their resources/provenance.

## Observed engine paths

| Path | Exact existing behavior | Ownership implication |
|---|---|---|
| [XdmSequence](../../src/Bosak.XPath.Core/Xdm/XdmSequence.cs) and [interfaces](../../src/Bosak.XPath.Core/Xdm/IXdmSequence.cs) | Struct holds a source reference. Public enumerator delays source GetEnumerator until its first MoveNext. Neither interface nor struct enumerator declares Dispose/cancellation. | Creating, copying, enumerating or abandoning this value does not establish a resource-transfer/release contract. |
| [Enumerable adapter](../../src/Bosak.XPath.Core/Xdm/EnumerableXdmSequence.cs), [VM single-pass adapter](../../src/Bosak.XPath.Runtime/Vm/SinglePassXdmSequence.cs), [provider adapter](../../src/Bosak.XPath.Providers/Streaming/StreamingSinglePassSequence.cs) | Adapters retain an IEnumerator and forward Current/MoveNext without exposing Dispose. The single-pass factories guard a second enumeration. | foreach break and cancellation/fault unwinding cannot be assumed to dispose the underlying iterator through this interface. Underlying iterators may finish their own finally paths when advanced to completion; that is not a universal early-release guarantee. |
| [Evaluate/EvaluateNodes](../../src/Bosak.XPath.Api/XPath31Expression.cs) | Evaluate returns the VM value while restoring selected context settings in finally. EvaluateNodes wraps the returned sequence/node; it does not drain every result or attach a result owner. | Return from Evaluate is not proof that evaluation has finished pulling input or that input resources can be released. Context restoration is distinct from deferred semantic capture (FG-06 still open). |
| [VM lazy paths](../../src/Bosak.XPath.Runtime/Vm/VmEngine.cs) | Marked stream inputs/results can flow through lazy path/filter/map/type processing; some tests/operations return after inspecting a prefix. | Internal short-circuiting as well as application break can leave a source unfinished. Do not promise deterministic release from MoveNext=false on one outer wrapper alone. |
| [Streaming loader](../../src/Bosak.XPath.Providers/Streaming/XmlStreamingProvider.cs) / [source](../../src/Bosak.XPath.Providers/Streaming/StreamingSource.cs) | Stream overload owns the created reader; CloseInput controls stream closure. Supplied XmlReader stays caller-owned. FinishPump marks Done, invokes StreamCompleted, then disposes an owned reader. | Successful completion and failure/early exit have different cleanup paths. Draining is real work, invokes callbacks and may be unavailable mid-flight; it is not a general safe Dispose substitute. |
| [Maps](../../src/Bosak.XPath.Core/Xdm/XdmMap.cs), [arrays](../../src/Bosak.XPath.Core/Xdm/XdmArray.cs), [value carriers](../../src/Bosak.XPath.Core/Xdm/XdmValue.cs) and functions | Carriers/containers retain XdmValue/object references; function items may retain execution state. No universal recursive lifetime rebinding is performed by EvaluateNodes. | A scalar/map/array/function result can still depend on an owner; top-level IsSequence checks do not discover all borrowed resources. |
| [Bounded assessment annotation](../../src/Bosak.XPath.Providers/XDocument/XdmElementAssessment.cs) | Owner and State objects are retained with supported assessment facts. | Metadata retention is not transfer of ownership of the source snapshot or a general compatible-universe/session protocol. |

## Bounded probe results

Ten [observations](evidence/observations.json) passed against the unchanged engine using only owned in-memory fixtures. The [probe](tools/Program.cs) uses explicit caller cleanup on every fixture and prohibits external XML resources.

| Probe | Observed result |
|---|---|
| LO-01 | Creating the public sequence enumerator does not start the underlying source. |
| LO-02 | Breaking a sequence does not call a concrete enumerator's optional IDisposable; the fixture releases it explicitly. |
| LO-03 | Normal streaming completion closes an owned reader/input with CloseInput=true. |
| LO-04 | Breaking a direct streaming axis leaves input open; caller fixture cleanup closes it. |
| LO-05 | Normal completion with CloseInput=false leaves the caller stream open. |
| LO-06 | A supplied XmlReader remains caller-owned at normal completion. |
| LO-07 | A throwing StreamCompleted handler runs before owned-reader disposal; input remains open at the observation boundary. |
| LO-08 | A throwing record post-processor leaves input unfinished/open at the observation boundary. |
| LO-09 | A VM lazy path result remains single-pass after EvaluateNodes; breaking it leaves input open. |
| LO-10 | A map retains the same materialized in-memory record node after normal reader completion. This does not qualify persistent snapshot or Schema lifetime. |

Matching existing behavior is not acceptance of every outcome. In particular LO-07 reproduces a **core-owned completion cleanup gap**: an owned reader's disposal is skipped when a completion callback throws. It needs a focused fix/test slice before relying on exception-safe completion. LO-04/08/09 also show that a general early-exit/cancellation release guarantee is not supplied by the current surface; their lifecycle design and provider facts remain open. These probes do not measure GC, operating-system handles, cancellation races, thread safety or real consumer leases.

## Compatible design options

| Option | Assessment / proposed disposition |
|---|---|
| Explicit provider/host owner, existing borrowed results | Recommended compatibility baseline. The owner remains visible at the host boundary and outlives supported uses; provider checks determine behavior after disposal. No new required member or value layout. Owners must still qualify abandoned results, nested access and lease release. |
| Optional result/evaluation scope on an opt-in path | Prepare only if actual consumer requirements need core-coordinated early release. Define ownership creation/transfer, idempotent release, partial evaluation, failure/cancellation and escaped-result validity before choosing signatures. It must release only resources it owns, not recursively dispose arbitrary values. No signature accepted here. |
| IDisposable added to existing enumerator/interface or carrier layout | Do not select for this scope without separate compatibility/migration disposition. It changes implementer/ABI expectations and still does not solve nested escapes or undeclared external owners by itself. |
| Mandatory eager materialization | Not selected as a general ownership solution: it changes streaming/performance/termination behavior and retains borrowed nodes unless a separately defined detach/copy operation is used. |

## Owner requirements and remaining gates

Schema should report whether retained assessment metadata is immutable/shareable, which operations need a live provider/session, and how escaped/copy-preserved values retain original PSVI without owning caller trees. XmlDb should report its exact BindResult/node guards, early-abandonment and snapshot/lease release behavior, including nested results. These are owner deliverables under existing S009/FX009 packages; no external implementation or notification is performed here.

Core must separately establish exception-safe owned completion, optional-scope necessity, lazy context capture and supported cancellation/reentry boundaries. FC-07/08/09/11/16 remain **NotRun as full compatibility gates**; these ten narrow observations supplement their preparation and do not overwrite the previous planned-case receipts. C45, general API scope, ABI/package qualification, full engine gates and joint freeze stay pending.

## Next core implementation slice

Fix the reproduced FinishPump completion-callback cleanup gap in a bounded slice: always attempt disposal of a reader owned by the streaming provider even if StreamCompleted throws, keep supplied readers/CloseInput semantics, and decide/test original-callback versus disposal-failure precedence. Record exact existing ownership/error contracts and applicable adoption evidence before changing behavior; add happy-path and throwing-callback cases, then run required engine tests and frozen gates. This work is core-owned and does not wait for the external projects or Braid product validation.
