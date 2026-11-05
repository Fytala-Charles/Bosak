# Private production admission ? 31 October 2026

Core **1.35.0** now contains production-source building blocks for private scalar evidence admission, explicitly opted-in immutable XPath compilation context, and whole-operation lifetime. This replaces the corresponding harness-only work with directly composed Core/Runtime/API code. No diagnostic hook, public V5 signature, public schema entry point or XdmValue carrier is promoted.

## Implementation and boundary

[SchemaEvidenceKernel](../../src/Bosak.XPath.Core/Xdm/SchemaEvidenceKernel.cs) verifies exact issued containers before reading getters, provider-owned type components, and original assessment identity for the admitted snapshot. Scalar values must be atomic. Evidence caches use reference identity and explicit per-snapshot entry limits; snapshot registrations also have an explicit capacity. Cached metadata assumes the issuer keeps issued values and component membership immutable for the admitted lifetime. The caller's snapshot identity is a trusted host lifetime key, not independent proof that arbitrary XML was assessed.

Conversion admission accepts only an issuer-owned, already validated constructed scalar with no invented original assessment. It does not perform casting/facet validation. A verified exact declared/actual-type and unchanged-value/type-field no-op may reuse source evidence. Changed values/types produce fresh boundary evidence without the source snapshot; closing the source does not invalidate that detached evidence while its owning session remains open. There is no attachment to VM values and no assertion of metadata propagation through arbitrary VM opcodes, functions or constructors.

[SchemaQuerySession](../../src/Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs) composes the private kernel with a frozen compiled evaluator directly. Sync/async callbacks own their raw operation until completion. Callbacks must evaluate and drain/detach borrowed results within that operation; arbitrary escaped nested lazy values are not automatically detached. Canceling a wait does not dispose raw work. Closure rejects new access and defers owned cleanup until every admitted raw lease ends. Completed operation objects cannot evaluate again. Provider and cleanup failures are both retained, and one failing cleanup does not skip other snapshots. The borrowed issuer is not disposed by the session.

[SchemaSessionCompilation](../../src/Bosak.XPath.Api/SchemaSessionCompilation.cs) is an internal, opt-in path. It freezes supported ordinal dictionaries/sets before compilation and records the effective inline namespace context. All supported CompileOptions fields, including private aliases/record names, contribute to the versioned deterministic descriptor. Null versus empty contexts remain distinct. Matching fingerprints never authenticate another environment instance.

Known ordinal comparer identities are checked by reference; a custom comparer cannot gain admission by lying through its own Equals implementation. Unversioned function callbacks, mutable BCL SchemaSet inputs and unknown/non-ordinal collection semantics cause **private admission refusal**, before issuer dispatch. They are not shallow-copied or represented by delegate strings. Ordinary public Compile/Evaluate follows its existing path with no new per-compilation snapshot allocation or per-opcode branch. Its existing namespace-map behavior is retained by a regression test. Dynamic evaluation context, broader XQuery/XSLT host declarations and actual provider binding need separate qualification; this is not the complete public semantic environment contract.

## Verification

[RESULTS.json](RESULTS.json) and raw [evidence](evidence/build.txt) record:

| Check | Result |
| --- | --- |
| Final Release build | 0 errors / 0 warnings (incremental); earlier full builds report five warnings in unchanged test/conformance files. |
| Unit coverage, ten projects | **24,245 passed / 0 remaining failures / 0 skipped**, including 20 new Core/Runtime regressions; full solution run plus complete serial database-project rerun. |
| Frozen QT3 | **31,397 / 0 / 424**. |
| Gated qt4 | **8,941 / 0 / 1,918**, eleven experimental-level passes. |
| Skip identities/reasons | Both maps match the retained 1.34.2 package-gate maps after line-ending normalization. |
| Retained 1.34.1 caller, unchanged binary | **20 / 0**, identical case observations with seven new source-built DLLs; actual loaded hashes match. |
| Exported metadata | Same 78 exported types and 1,199 declared reflection members across seven assemblies. This count excludes inherited-member duplication. |
| XdmValue x64 layout | **48 bytes**, unchanged. Its source, payload accessors and equality implementation are untouched. |

New tests cover wrong environments/snapshots, unissued/invalid/foreign/wrong-origin values, no-op versus type/value changes, detached conversions, capacity/cache reuse, closure during provider callbacks, canceled waits, actual VM evaluation/draining, escaped operation objects and combined cleanup failures. They are at [Core tests](../../tests/Bosak.XPath.Core.Tests/SchemaEvidenceKernelTests.cs) and [Runtime tests](../../tests/Bosak.XPath.Runtime.Tests/SchemaQuerySessionTests.cs).

The [recorded commands](evidence/COMMANDS.json) retain the final runner settings and exit statuses. The initial sandboxed unit run could not start local HttpListener servers in 45 database-provider tests; its original receipt remains. A subsequent full-suite run with localhost access passed all projects. The final refreshed parallel run hit two existing fixture port-reservation collisions before engine assertions; rerunning the complete database project serially passes 47/0. All ten projects now have passing final coverage. The original failed parallel receipt is retained; it is not relabeled as a clean single full-solution run. Serialization uses the documented [xUnit RunSettings](https://xunit.net/docs/config-runsettings#parallelizetestcollections) and changes no assertions or source. XSLT passed normally; no Application Control workaround was needed. Earlier pre-cleanup-review and pre-comparer-review receipts stay separate from the final gate runs. The retained caller was initially invoked without its required output-file argument; that usage/watchdog correction is recorded separately, with the corrected run proving all 20 cases. No compatibility expectation was changed.

[SOURCE.json](SOURCE.json) records normalized source hashes and the base revision before delivery; [source-overlay.zip](source-overlay.zip) retains the exact changed sources, unchanged XdmValue anchor and licenses for replay over that base. Tests were built before the delivery commit; assembly informational revisions may therefore name that base. These are source-built qualification artifacts, not packages rebuilt from an immutable release commit. The earlier 1.34.2 package feed/receipts remain unchanged historical evidence and do not qualify this new version automatically.

## Remaining work

No public schema API, full advisor/assessor composition, real native Schema provider adapter, atomic VM carrier, general constructor provenance transport, list/union/facet implementation, persistent database lifetime or worker termination is claimed. Snapshot/cache entry limits are not byte budgets or final performance acceptance. There are no new hot-path changes in ordinary evaluation, but the private boundary's allocations, arbitrary concurrency and complete consumer workloads still need qualification.

Next: choose and qualify the private atomic transport, connect actual provider admission through the intended full environment/advisor path, and validate Schema/XmlDb/Braid against an immutable package cohort. C45, final manifest, performance/compatibility owner acceptance, publication and four-owner API freeze remain pending. No owner checkout, store, package pin or external notification is changed.

[Pre-implementation basis](SPEC-BASIS.md) records exact immutable specification sources and the deliberately internal scope. This is not new XPath language semantics or a new normative deviation. Delivery resolves with `git log -1 --format=%H -- docs/foundation-production-admission/README.md`.
