# Private evidence kernel and constructor bridge ? 31 October 2026

The isolated integration trial now implements a private issuer/evidence kernel, routes successful real VM casts through its guard, and gives snapshots explicit admission and cleanup. **205 controls pass in each of two guarded runs**; baseline and prior-holder cohorts each retain 162 passing controls in two runs. All six builds report zero warnings/errors. [RESULTS.json](RESULTS.json) records 1,058 successful control evaluations. Production sources and V5 signatures remain unchanged.

## What is implemented

The internal [kernel](tools/Kernel.cs.txt) runs in the trusted diagnostic host assembly. It checks reference-based container issuance before touching container getters, verifies the associated snapshot, authority-issued type handles and exact original assessment binding, and creates a private carrier. Its reader accepts only that carrier class and checks kind, payload value and both legacy type-name fields before exposing evidence. Rebinding a legitimate marker onto changed payload/type data is refused. A caller object copying valid-looking provenance fields is not an issued carrier.

An immutable issued scalar is cached per snapshot/container. Repeated reads reuse its carrier. A verified no-op constructor returns the existing value, preserving evidence without a new holder. Cache creation holds an operation lease and refuses publication if closure starts meanwhile. This is private caching policy, not a new public scalar/batch API.

The guarded Runtime wraps successful TryCast results and invokes a temporary diagnostic Core hook. Actual public XPath constructor calls exercise that route; no reflection or friend access is used to call VmEngine from the runner. The bridge covers the native decimal target and built-in integer target:

- An unchanged value with exact authoritative declared/actual type and unchanged legacy type fields retains its existing evidence.
- Integer conversion creates fresh engine-owned built-in target evidence with no OriginalAssessment. It detaches from the input owner/snapshot and remains readable after that snapshot closes. A later integer-to-decimal conversion also produces detached fresh built-in evidence.
- Unknown target spellings/types conservatively strip diagnostic provenance. They are not treated as supported provider conversions or allowed to inherit authority. The existing VM result/type fields remain intact.
- Unknown marker objects are stripped without reading their getters. Modified payload/kind/type annotations under an issued carrier are refused.

The built-in authority implements ownership and identity only; broader lookup/derivation/affiliation/closure remain Unsupported. This is a bounded target bridge, not generic authoritative constructor resolution, facet/union/list validation or full current-type evidence coverage for every function result.

## Snapshot ownership and caches

Registrations admit at most four snapshots in the normal fixture; a dedicated two-snapshot fixture tests refusal at capacity. These are diagnostic settings, not production defaults. Closing blocks new operations, typed-value/evidence reads and navigation. It retains the snapshot/evidence cache while an admitted raw operation is outstanding; cancellation of a wait does not release that operation's lease. Raw completion clears the evidence cache, removes adapter views/trees/issued-value records, frees admission capacity and clears the cleanup callback reference. Registration disposal closes remaining snapshots. Original immutable component facts can remain as metadata.

The reference count and registration bookkeeping use locks, and lease disposal is idempotent. Adapter cache administration is serialized by this host fixture; arbitrary concurrent provider operations, production eviction and every shutdown race are not qualified. The raw-work test uses a controlled asynchronous task holding a memory-snapshot lease. It is not a whole evaluation/drain request, physical database/file lease or actual worker-termination test.

The wrapper now checks deferred source MoveNext/Current and node content access. XdmSequence itself caches its Current value; returning that cached node reference cannot be revoked. Accessing that closed node's typed contents still fails. Detached built-in scalar evidence remains usable. No global per-item revocation overhead is added to the production XdmSequence API.

## Controls and exact cohorts

The [prior whole-snapshot trial](../foundation-snapshot-navigation/README.md)'s 162 controls run in all three modes, including all thirteen axes and 23 compiled path expressions. Forty-three guarded controls add unissued/default/non-atomic/wrong-origin refusal, real constructor reuse/freshness, forged/rebound marker refusal, conservative unknown-target handling, bounded admission, canceled waits, deferred access after close, cleanup/capacity reuse, independent snapshots, detached results and evidence-cache reuse/cleanup.

[INPUTS.json](evidence/INPUTS.json) records the three generated layouts and project text. Baseline loads all seven exact retained Core/API 1.34.2 package DLLs. Holder changes Core only. Guarded kernel changes Core and Runtime only; the other five DLLs match the package cohort exactly. Raw round reports verify all seven actual loaded hashes, Default contexts and noncollectible status. Each mode runs twice, with reversed order. Numeric/string/Boolean observations match across cohorts, while marker expectations match their declared mode.

Runtime source is archived from exact c88eea4308c0e49a81e7bfe2f481319f2e4d208c with license/NOTICE in [runtime-source.zip](evidence/runtime-source.zip); [RUNTIME-SOURCE.json](evidence/RUNTIME-SOURCE.json) records every file hash. Only the successful TryCast wrapper changes Runtime source in the guarded build. Core adds temporary post-cast/strip shims to the prior holder. Full modified diagnostic source and zero-warning build logs are retained. Native assessment source remains the exact six-file parent archive, compiled in the diagnostic assembly; no Schema owner checkout is edited or built.

The kernel is internal to the harness. The Core hook/strip accessors are **public diagnostic shims in scratch**, not proposed product APIs, a secure external plug-in admission mechanism, or a production placement decision. The mutable process-wide hook is unsuitable as a public foundation contract and must be replaced by direct private Core/runtime composition. SchemaAtomics remains an unimplemented public proposal helper. Bound advisor/environment admission and actual compiler capture remain pending.

## Limits and next implementation

Snapshot-count limits and cache cleanup do not prove byte budgets, bounded total caller-held memory or accepted performance. First attachment and fresh conversion still allocate; no timing/allocation/GC acceptance is claimed. The holder's known ABI/allocation/equality limitations remain, and it is not selected as the production carrier. Detached-result tests cover these two built-in targets only. Borrowed native-origin evidence requires an open snapshot in this diagnostic policy; it is not a general persistent-value lifetime promise.

[Pre-execution specification basis](SPEC-BASIS.md) records immutable XPath/XDM/F&O revisions and the candidate no-op disposition. No production semantics, package pin, external repository, original store or earlier sealed evidence is changed. No new full unit/conformance gates are claimed: engine edits exist only in isolated diagnostic assemblies. C45, real consumer qualification, budgets and four-owner API freeze remain pending.

Next: replace the diagnostic hook with a directly composed private Core/runtime kernel and qualify full advisor/environment admission and whole-operation lifetime. Select the internal carrier only with the required compatibility/performance evidence, then run full product gates before production acceptance.

Reproduce with `python docs/foundation-evidence-kernel/tools/run.py`; verify retained evidence with `python docs/foundation-evidence-kernel/tools/verify.py`. Exceptions are caught and builds/processes are bounded. An initial cached-Current expectation failed because XdmSequence caches the node value; its caught receipt is preserved separately. The final control checks access to the closed node's contents.
