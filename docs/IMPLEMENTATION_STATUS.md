<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak implementation status">
  <br><br><h1>Bosak implementation status</h1>
  <p>Technical qualification, source candidates and open integration boundaries</p>
</div>

Last updated: **30 October 2026**. This page holds current engineering detail; the [README](../README.md) introduces the product and the [roadmap](../ROADMAP.md) describes direction. Dated checkpoints moved out of the README are preserved in [development history](DEVELOPMENT_HISTORY.md#readme-snapshot-2026-10-29). Original evidence dossiers remain unchanged.

## Releases and source state

| Scope | Recorded state | Evidence |
|---|---|---|
| Published Bosak packages | Verified 1.26.3, source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f` | [Publication receipt](RELEASE-1.26.3-PUBLICATION.md) |
| Current core source | Unpublished candidate 1.33.1 | `src/Directory.Build.props`; [C42 repair](conformance/k04-schema-element-fix/README.md) |
| Public schema engine | Apache-2.0, activation-free, 0.4 development line; no new NuGet release from the transition | [Schema publication](SCHEMA-APACHE-PUBLICATION-2026-10-29.md) |

## Recorded qualification

| Scope | Recorded result | Interpretation |
|---|---|---|
| Complete core solution | 24,203 passing tests | Inherited source-identical engine qualification; not rerun for this documentation cleanup. |
| Frozen QT3 gate | 31,397 passed / 0 failed / 424 skipped | Preserve exact skip names/reasons and adopted baseline. |
| qt4 gated run | 8,941 passed / 0 failed / 1,918 skipped; 230 gated sets, 11 experimental passes | Bounded adopted coverage, not complete current-draft certification. |
| Revised isolated schema prototype | 123 controls; twelve detected bad variants; 32 compiled proposed neutral types | [C44-R2 dossier](conformance/k04-schema-compatibility-prototype/README.md); no production API/ABI acceptance. |
| Public Schema solution | 114 passing tests and public CI | Separate schema-only suite; not a complete XSD 1.1 or core-provider certification. |

The [cross-system review](compatibility/xdm40-2026-10-29/README.md) and [revised prototype qualification](conformance/k04-schema-compatibility-prototype/QUALIFICATION.md) distinguish real observations, fake-provider controls, BCL witnesses and inherited results. Counts from different scopes must not be added together as one suite.

## Schema and XDM integration

The unpublished [Schema 0.5 native assessment candidate](schema-assessment/README.md) now checks one complete finite XSD 1.1 structure/decimal/assertion profile before exposing an owned document and original component state. Its 191-case suite (114 original plus 77 new) passes with published 1.26.3 and current core source; 114 unchanged legacy test binaries also pass with only Schema replaced. The API adds seven exported types without removing existing public members. These scopes overlap. Full typed anonymous-root projection remains explicitly unsupported; no general session API, product adoption or full XSD 1.1 claim follows.

Development is isolated at `D:/Development/Bosak/.worktrees/Bosak.Schema.Assessment`, branch `feature/bounded-document-assessment`, with uncommitted changes on public base `044d2cb`. The existing public checkout and its 1.26.3 dependency pins are unchanged. Core source/gates remain source-identical; their complete earlier qualification is inherited for this step. The candidate guide and receipts own the exact supported profile and refusal boundaries.

Schema is now open source; the earlier proprietary-provider boundary no longer determines the public API shape. Reassess the smallest useful contract before C45. Keep host/XDM profile, XSD version, component authority and source/snapshot identity distinct, retain original PSVI across compatible universes and preserve caller ownership/cancellation.

Full XSD 1.1 structural validation, complete real component/provider coverage, engine atomization/copy integration and actual consumer source/ABI acceptance remain pending. The proposed API is unaccepted. REQ-139 remains pending; REQ-126 retains its bounded completion and all 48 historical observations.

The first [real open Schema integration](schema-integration/README.md) now provides an XSD 1.0 validation/typed-transformation example and a separate optional CI job. Fresh qualification passes 24,203 core tests, 122 source-integration cases (114 existing plus eight new), 114 unchanged Schema binary-replacement cases and both exact conformance gates, including unchanged skip identities/reasons. Braid passes 1,872 unchanged cases plus four separately rerun cases with its expected engine identity updated only in an isolated copy. These are bounded observations, not full XSD 1.1/session interoperability or consumer adoption; see the results receipt for the complete consumer disposition. The recorded qualification table above remains its earlier checkpoint.

A local unsigned preview combines the existing nine core candidate packages with Schema `0.4.0-alpha.1` using isolated dependency-pin overrides. Its package-only typed transformation and all ten package/output identities verify. This local preparation does not publish a release, change the public source pins or adopt an API.

XmlDb's corrected candidate route passes all 27 adapter source tests. All 593 unchanged integration binary cases have passing evidence across the initial run and explicit layout/timeout reruns; this is not one uninterrupted pass. The independent storage baseline was cancelled after 1,258 observed passes and is not certified as complete. Exact routing, package/output identities and original failures are retained in the qualification dossier.

Use `D:/Development/Bosak.Schema.Public` for active public Schema source. `D:/Development/Bosak.Licensing` contains private infrastructure; the original locked `D:/Development/Bosak.Schema` directory also points to that private repository. This local migration detail belongs here and in the handover, not in the user quick start.

## Authoring, tooling and newer profiles

XPath, XSLT and XQuery share the core data model/runtime. Source-preserving inspection and bounded edit operations underpin Braid's visual/source workflow; full live bidirectional editing is still a product goal, not a blanket delivered capability. See the [authoring roadmap](STRUCTURAL-AUTHORING-ROADMAP.md), [authoring implementation](REQ-135-IMPLEMENTATION.md), [4.0 coverage](REQ-118-xpath-xslt-40.md) and [integration guide](INTEGRATION.md).

The current-draft JNode/record/map/comparison gaps are recorded in the [immutable-draft review](compatibility/xdm40-2026-10-29/SPEC-REVIEW.md). A later repair requires its own adoption scope, regressions and unchanged-profile checks. No baseline promotion follows from a diagnostic pass alone.

## Known limits and development commands

- Extended-year date/dateTime construction is supported in documented cases; operations using `DateTimeOffset` remain bounded to years 1–9999 and can raise FODT0001 outside that range.
- Decimal arithmetic uses .NET's fixed 28–29 significant-digit representation.
- Some remote HTTP conformance resources are unavailable from this environment; the recorded Cloudflare challenge is separate from an engine assertion failure.

Normal source verification is `dotnet build Bosak.sln` and `dotnet test Bosak.sln`. [AGENTS.md](../AGENTS.md) supplies manual conformance commands and the Windows XSLT test workaround. The [XSLT harness guide](XSLT40-HARNESS-HARDENING.md) documents watchdogs and retained receipts.

New engineering checkpoints belong in this page, request-specific dossiers and [agent handover](AGENT_HANDOVER.md); dated historical summaries belong in [development history](DEVELOPMENT_HISTORY.md). Keep receipt provenance, source/publication identity and owner decisions separate.

## Whole-document assessment candidate — 29 October 2026

The [bounded document bridge](schema-document-bridge/README.md) supersedes the earlier decimal-projection-only boundary for this unpublished candidate. Core 1.34.0 adds an optional XML-provider assessment carrier; native Schema 0.5 exports original anonymous-root/decimal states, preserving/stripping copies and explicit mutation/reassessment refusal. The general session proposal and broader REQ-139/REQ-009 acceptance remain pending. Source changes remain uncommitted; public release and consumer pins are unchanged.

Fresh bridge qualification: Release build has zero warnings/errors; complete core aggregate **24,207 passed**, native Schema **204 passed** (13 whole-document cases), QT3 **31,397/0/424**, qt4 gated **8,941/0/1,918**, 230 sets and 11 experimental passes. Exact skip mappings match the sealed integration checkpoint. The initial sandbox database failures are retained separately; the local-HTTP-authorized rerun passes all 47 cases. Whole-document example and strict Schema documentation checks pass. See [bridge evidence](schema-document-bridge/README.md).

## Bounded bridge API review — 29 October 2026

The [API review](schema-document-bridge/API-REVIEW.md) found and corrected failed-attachment exception safety: a replacement is checked completely before the previous binding is removed. Two regressions cover failed replacement and failed initial attachment. The trusted-host API carries immutable original state supplied by the host; it does not validate schemas or certify arbitrary host claims. Existing finite semantics and candidate version 1.34.0 remain unchanged. Shape-verification allocation cost is documented for later measurement. General API freeze and broader schema/session acceptance remain pending.

Current qualification and local checkpoint disposition are recorded in the [review checkpoint](schema-document-bridge/api-review/README.md). Earlier bridge/assessment receipts remain unchanged. Optional-child support is a proposed future slice, not implemented by this review.

Post-review verification: **24,209 core tests pass in one complete solution run**, **204 Schema tests pass**, and all six focused provider attachment cases pass. Release build: zero warnings/errors. QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 gated sets, 11 experimental passes), with exact skip mappings unchanged. The full-document example retains original root state and passes typed-value and preserve/strip checks. Prior sealed assessment, bridge and integration receipts remain unchanged.

## Optional decimal children: scope prepared — 29 October 2026

The [pre-implementation adoption/scope checkpoint](schema-optional-children/README.md) pins the next bounded native Schema slice: optional local decimals, maximum occurrence one, ordered matching, present-instance state and assertion effective Boolean conversion. Optional support is not implemented; no current API, version, REQ completion, consumer pin or gate change. Fresh Schema baseline: 204 passes; 13 unique isolated diagnostic observations (the initial twelve are preserved separately). Core 24,209-pass and exact gate results are inherited from source-identical `a163ff11`, not rerun. Next: implement the separately recorded plan in the Schema feature worktree. General sessions and full XSD 1.1 remain pending.

Scope refinement: a thirteenth isolated observation exposes ignorable-whitespace removal being refused for an empty element-only assessed root. Supporting all-optional-absent documents therefore needs a bounded provider binding correction. The future implementation proposes core patch 1.34.1 plus fresh complete core tests and exact conformance gates; current versions and production source stay unchanged. Both the initial twelve and extended thirteen-observation receipts are preserved in the optional-child dossier; they overlap.

## Optional decimal scope delivery — 29 October 2026

Current delivery: [synchronized scope checkpoint](schema-optional-children/delivery/README.md). The user requested commit and push of the documentation in both repositories. Optional children remain planned, with 13 diagnostic observations and 204 fresh Schema baseline passes; current Schema 0.5/core 1.34.0 and all product source/pins are unchanged. The future implementation targets Schema 0.6 and the bounded core 1.34.1 whitespace fix, requiring fresh full core/gate qualification. Earlier uncommitted notices describe capture time, not this delivery. No REQ completion, merge or release is inferred.

## Optional decimal implementation candidate — 29 October 2026

The [bounded implementation](schema-optional-implementation/README.md) supersedes the earlier optional-scope next-action notices. Schema 0.6 adds local minimum-zero decimal particles, ordered matching, effective Boolean assertion conversion and present-only original state. Required-only v1 remains; optional schemas use v2. Core 1.34.1 corrects whitespace binding for empty element-only roots, retaining real-content/decimal edit refusal. Current Schema tests: 260 passes, 56 new cases; core adds five provider cases. Current candidate dependency references change only in the feature worktree; product pins, gates/fixtures and broader REQ acceptance remain unchanged. Full qualification is recorded separately in the new dossier; earlier sealed evidence remains unchanged. No commit/push/tag/publication in this step.

Fresh final qualification: **24,214 complete core tests pass** (including 8,196 XSLT and 212 provider cases), **260 complete Schema tests pass** (56 new optional cases), normal QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. Exact skip names/reasons match the sealed integration checkpoint. Core build has zero errors and five warnings in unchanged Standard test/XSLT conformance sources; no new warning source is introduced. The optional-gap whole-document example and strict Schema documentation check pass. Original 45-case before evidence is 9 passed/36 failed; all 45 now pass alongside eleven added boundary cases. Five new core cases cover the root binding correction.

## Optional decimal review checkpoint — 29 October 2026

Current delivery: [reviewed optional implementation](schema-optional-review/README.md). Review found no further production defect; all changed source hashes match the qualified 24,214-core/260-Schema candidate and exact unchanged gates. Schema 0.6/core 1.34.1 remain unpublished. User-authorized checkpoint commits/pushes use the existing core checkpoint and Schema feature branches. Earlier uncommitted implementation notices are capture-time history. Next: separately scope bounded repeated decimal children; no repetition, full schema/session acceptance, consumer adoption or release is inferred.

## Bounded repeated decimal scope — 29 October 2026

The reviewed optional implementation is checkpointed at core `5e277c6a` / Schema `773fd69`. The [next bounded repetition scope](schema-repeated-children/README.md) is prepared before implementation: local minimum zero/one, maximum up to sixteen and total maximum budget sixteen, ordered distinct instances sharing original declarations, narrow cardinality-error handling and ambiguous singular-export refusal. Current Schema 0.6/core 1.34.1 and tested source are unchanged. Sixteen scratch observations support scoping; 260 Schema/24,214 core and exact gate results are inherited from the reviewed source-identical candidate. Schema 0.7/v3 is a proposed future implementation only. No general API/session acceptance, merge or release.

## Bounded repeated decimal implementation — 29 October 2026

The [Schema 0.7 repetition candidate](schema-repeated-implementation/README.md) implements the recorded finite scope: maximum/aggregate budget sixteen, contiguous matching, distinct occurrence states with shared declaration authority, narrow cardinality assertion failure and ambiguous singular projection refusal. Required-only v1/optional v2 remain; v3 marks repeated schemas. Complete Schema tests: 334 passes, 74 new cases; source-override build/example pass. Core stays source-identical 1.34.1; 24,214 core passes and exact normal gate evidence are inherited only after identity checks, not fresh full runs. No core/harness/fixture/consumer pin changes, general API freeze or release. Implementation changes remain uncommitted; next is bounded review/checkpoint.

Final repeated-candidate qualification: **334 complete Schema cases pass**, including **74 new repetition/error-boundary cases**. The original 57-case before cohort (17 passed/40 failed) now all passes; seventeen further boundary/safety cases also pass. The prior 260-case suite retains 259 unchanged passing expectations and one explicitly adopted maxOccurs-two coverage/profile update. Schema source-override Release build: **zero warnings/errors**. The repeated-document XPath/XSLT example and strict Schema docs check pass. Core source/tests/harness/fixture references and build configuration are unchanged from the reviewed source, so **24,214 core passes**, QT3 **31,397/0/424** and qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. Exact raw source/receipt identities and all earlier sealed dossiers are preserved.

## Repetition implementation review checkpoint — 29 October 2026

Current delivery: [reviewed Schema 0.7 repetition implementation](schema-repetition-review/README.md), core 1.34.1 unchanged. No additional defect was found; tested source hashes and original qualification match. User-authorized commits/pushes checkpoint this source before a separate fn:count assertion scope. Earlier uncommitted implementation notices describe capture time. Full schema/session/API acceptance, consumer adoption, merge and release remain pending.

## Count assertion scope — 29 October 2026

The [fn:count adoption and plan](schema-count-assertions/README.md) prepare the next bounded assertion operand: unprefixed count(DeclaredChildName), without broader grammar, aliases/prefixed calls or altered native validation. Bare count child names are preserved. Twelve diagnostic observations support scoping; native count assertions remain unimplemented. Current reviewed source is Schema 0.7/core 1.34.1; proposed Schema 0.8/v4 is future implementation. Core/Schema qualification is inherited from unchanged reviewed source, not new full runs. Earlier sealed repetition evidence remains unchanged; no general API/session acceptance, merge or release.

## Count assertion implementation candidate — 29 October 2026

The [Schema 0.8/v4 count candidate](schema-count-implementation/README.md) implements bounded unprefixed count operands with complete-parse feature metadata. Bare count child names, legacy profiles, native validation/state/copy/error behavior and bounds remain; broader function/namespace grammar is still refused. Fresh complete Schema qualification: 397 passes, 63 new tests; build/example/docs pass. Core 1.34.1 source/configuration and product pins are unchanged, so 24,214 core passes and exact gate evidence are inherited, not fresh runs. The one prior count refusal is explicitly adopted with preserved source; 333 other expectations remain. Earlier sealed scope/review evidence is unchanged. Source/doc changes remain uncommitted; next is bounded review/checkpoint, not release or general API acceptance.

Final count-candidate qualification: **397 complete Schema cases pass**, including **63 new count/grammar/validation-order/copy cases**. The original 57-case cohort (23 passed/34 failed) now all passes; six further profile/optimization-boundary cases pass. The prior 334-case suite retains **333 unchanged passing expectations** plus the explicitly adopted count capability check. Source-override build has **zero warnings/errors**. The actual native count/absence example and strict documentation checks pass. Core source/tests/harness/fixture references and build configuration remain identical, so **24,214 core passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 sets) are inherited, not fresh full runs. All earlier sealed evidence and original refusal source/receipts remain unchanged.


**29 October 2026 review:** Schema 0.8 count assertions reviewed: fresh 397/397 Schema tests pass; core 1.34.1 source and inherited engine gates remain unchanged. Next is the separately scoped presence-predicate implementation; exists/empty remain unsupported in this checkpoint. [Review and next scope](schema-count-review/README.md).


Current presence implementation — 29 October 2026: Schema 0.9/v5 supports bounded unprefixed exists(child)/empty(child) Boolean predicates, retaining count-only v4 and no-call v1–v3. Complete Schema qualification: 451 passes (54 new); build zero warnings/errors; native example and real preserve/strip consumer tests pass. Two prior refusal expectations are explicitly adopted, 395 unchanged. Core 1.34.1 and its source-identical engine qualification remain inherited. Current implementation is uncommitted/unpublished; next is review and checkpoint. Earlier scope-only/presence-unsupported notices are historical capture state. [Implementation and evidence](schema-presence-implementation/README.md).


**29 October 2026 review:** Schema 0.9/v5 presence predicates reviewed: fresh 451/451 Schema tests pass and all seven tested implementation source identities match. Core 1.34.1 remains unchanged with inherited engine/gate qualification. No further production correction was needed. Earlier uncommitted notices are historical capture state; checkpoint delivery resolves through the review document Git history. [Review](schema-presence-review/README.md).


Consumer-priority assessment (29 October 2026): qualify the actual XmlDb persistent-node adapter in an isolated source workspace next, then Braid authoring/preview compatibility. No inspected dependency requires more native assertion functions now. Native structural expansion needs a concrete schema/instance use case. Schema 0.9/core 1.34.1 and all prior qualifications remain unchanged; no consumer execution or dependency adoption is claimed. [Assessment and acceptance plan](schema-consumer-priorities/README.md).


Isolated XmlDb qualification (29 October 2026): committed adapter source builds unchanged against current core; 27/27 existing Xdm tests pass on both 1.26.2 and 1.34.1. All 44 bounded diagnostic observations agree, including 26 query pairs, seven persisted kind mappings and nested snapshot disposal. Unrelated consumer dirty work is excluded. No dependency adoption, binary/native/API acceptance or production fix. Next: isolated Braid source compatibility. [Qualification and limits](xmldb-compatibility/README.md).


Isolated Braid qualification (29 October 2026): all actual consumer source builds against current core without a production fix. Original full suites on 1.26.2/current 1.34.1 each report 1,872 passes/four exact-version failures; separate corrected/adapted identity follow-ups pass all four. No single green unchanged full-suite claim, actual dependency adoption or native desktop acceptance. Next: review/checkpoint the consumer-priority and both compatibility dossiers. [Qualification and limits](braid-compatibility/README.md).


Consumer qualification reviewed (29 October 2026): exact sealed receipts and test identities verified; XmlDb 27 passes per version/44 matching observations, Braid 1,872 passes/four version failures plus separate four passing identity follow-ups per version. No further production fix, dependency adoption or binary/native acceptance. Earlier uncommitted notices are historical; delivery resolves through the review document Git history. Next: separately scope unchanged-binary qualification. [Review checkpoint](consumer-qualification-review/README.md).


Unchanged-binary qualification scoped (29 October 2026): exact retained baseline/candidate assembly hashes and identities are inventoried. Next: frozen baseline controls then engine-only replacement, preserving consumer DLLs, hosts, manifests and original version assertions. No replacement or new consumer execution yet; source qualification is not binary compatibility. Core 1.34.1/Schema 0.9 and actual product pins remain unchanged. [Execution plan](binary-compatibility-scope/README.md).


Unchanged-binary qualification (29 October 2026): baseline controls pass XmlDb 27/Braid 1,876; engine-only replacement passes XmlDb 27 with identical 44 observations and Braid 1,872/four original version failures. All consumer/host/manifest bytes stay frozen; actual candidate loads and preview workers are verified. Additional public-contract probe shows Braid inspector/preview identity stays old pin despite loaded 1.34.1; transparent product adoption is not qualified. No production fix or dependency adoption. Next: review/checkpoint and separately resolve consumer runtime-identity policy before upgrade. [Results and limits](binary-compatibility-execution/README.md).


Unchanged-binary qualification reviewed (29 October 2026): raw counts/identities, frozen layouts and actual replacement loads verified. XmlDb 27/27 plus identical 44 observations; Braid baseline 1,876 passes, replacement 1,872/four original version failures. Braid advertised runtime identity remains old pin; transparent adoption is not qualified. No production fix/pin/release change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: separately scope consumer identity/adoption policy. [Review checkpoint](binary-compatibility-review/README.md).


Braid identity/adoption policy scoped (29 October 2026): recommend reviewed rebuild at an explicit pin plus runtime mismatch refusal using an approved artifact manifest, consistent parent/worker identity and preserved historical results. Dynamic automatic replacement is deferred. Next: isolated contract prototype with admission/refusal controls and explicit runtime-verification limits. No Braid/core implementation, actual pin, protocol, storage or release change; scope only. [Policy and acceptance scope](braid-engine-identity-policy/README.md).


Isolated identity-policy prototype (29 October 2026): 47 modeled contract controls pass, six enforcement mutants detected, 18 actual PE/hash observations match. Recorded Braid trace observes eight/nine engine members; complete-runtime evidence therefore refuses. Model flags/envelopes are not an actual runtime/worker guard. No product/pin/core API change. Next: review/checkpoint, then resolve a real lazy-loading/binding admission design in isolation. [Prototype evidence and limits](braid-identity-prototype/README.md).


Identity-policy prototype reviewed (29 October 2026): 47 controls/six detected mutants/18 real file observations reconciled; tested source/project/assembly hashes match. All prior evidence preserved. Runtime-binding flags and worker envelopes remain modeled; recorded eight/nine loaded members expose an unresolved lazy-admission boundary. No actual product guard, pin, protocol or core API change. Earlier uncommitted notices are capture history; delivery resolves through review Git history. Next: isolated actual runtime/lazy-binding experiment. [Review checkpoint](braid-identity-review/README.md).


Actual-runtime admission experiment (29 October 2026): 34 real controls pass with nine independent worker exchanges. Approved snapshots allow eight-member XSLT then deferred ninth-member XQuery binding; actual byte/identity/MVID evidence, drift revocation and worker mismatch rejection are recorded. This is a standalone custom-load-context host, not Braid integration or a product protocol/pin/core API change. Next: review/checkpoint, then consumer-owned boundary/type-identity design. [Runtime evidence and limits](braid-runtime-admission/README.md).


Actual-runtime experiment reviewed (29 October 2026): 34 real controls/nine worker exchanges, tested source/assembly hashes and approved snapshot ledgers verified; earlier evidence preserved. Lazy eight-to-nine-member binding works in the standalone host. Actual Braid typed adapters/default loader/protocol are not integrated; no product pin/core API change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: consumer-owned integration-boundary design. [Review checkpoint](braid-runtime-review/README.md).


Braid integration design (29 October 2026): prefer approved snapshot-backed adapter sessions sharing exact host Application/Core contracts. Real typed casts, opaque ownership and callback/lifecycle behavior remain to prove; default-loader hash prechecks do not establish the same binding. Next: isolated real read-only inspector typed-port proof. No actual Braid implementation, dependency pin, protocol, storage or core API change. [Design and vertical-slice acceptance](braid-integration-design/README.md).


Real typed-inspector proof (29 October 2026): 41 controls pass; four encodings match a separate direct baseline across shared Application/Core types, callbacks, source bytes/extents and actual target ownership. Raw port still returns NoChange after unload request; diagnostic lease closure alone is insufficient for a product guard. No edits/product pin/core API change. Next: review/checkpoint, then real exported-port facade admission proof. [Typed proof and lifetime boundary](braid-typed-inspector/README.md).


Typed-inspector/integration design reviewed (29 October 2026): 41 controls, four exact direct/isolated projections and private/shared assembly hashes verified; earlier evidence preserved. Raw-port NoChange remains callable after unload request, so outer lease/unload is not revocation. No actual production guard, pin or core API change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: isolated guarded exported-port/target facade proof. [Review checkpoint](braid-typed-inspector-review/README.md).


Guarded-port facade proof (29 October 2026): 54 controls pass over the actual inspector. Supported public result ports/targets are wrapped or removed; closed/foreign/drift admission and controlled in-flight close refuse raw authority publication. Profile is inspection/rename-target/exact NoChange only; broader authoring remains refused. Raw adapter control still reproduces the old lifetime gap. No actual product guard/pin/core API change. Next: review/checkpoint, then consumer-owned facade/lifecycle scope. [Facade evidence and limits](braid-guarded-ports/README.md).


Guarded-port facade reviewed (29 October 2026): 54 controls, nine result ports/seven target properties and source/assembly/artifact hashes verified; prior evidence preserved. Profile remains inspection/rename-target/exact NoChange only; controlled close is qualified, arbitrary concurrency is not. Raw adapter gap remains unchanged in product. No actual guard/pin/core API change. Earlier uncommitted notices are history; delivery resolves through review Git history. Next: consumer-owned facade/lifecycle contract scope. [Review checkpoint](braid-guarded-ports-review/README.md).


Consumer facade/lifecycle contract scoped (29 October 2026): propose one active engine call per session with immediate overlapping/reentrant refusal, nonblocking close initiation and retained in-flight ownership until completion. Separate engine capability facts from admitted operations; guard every returned authority surface. Post-admission cancellation, callback/drain ordering and broader schedules still need proof. No product/core API/pin/protocol change. Next: isolated deterministic lifecycle proof. [Contract scope and schedule matrix](braid-facade-lifecycle-contract/README.md).


Deterministic facade lifecycle proof (29 October 2026): 65 controls across 13 isolated cases pass, including real callback overlap/reentry, self-drain refusal, post-admission cancellation, caller-abandoned waits and incomplete drain. All leases balance; one underlying release request per case, not GC proof. Engine facts/admission profile are separate. Arbitrary concurrency and actual Braid product integration remain unqualified. No product pin/core API change. Next: review/checkpoint, then finite consumer adoption proposal. [Schedule evidence and limits](braid-lifecycle-proof/README.md).


Lifecycle contract/proof reviewed (29 October 2026): 65 controls and 13 ordered state/lease traces verified, with balanced leases and one idle release request per case; source/assembly/snapshot hashes match. Earlier evidence preserved. Finite callback/cancellation schedules do not certify arbitrary concurrency or actual product integration. No pin/core API change. Earlier uncommitted notices are history; delivery resolves through review Git history. Next: finite consumer-owned read-only adoption proposal. [Review checkpoint](braid-lifecycle-review/README.md).


Finite read-only adoption proposal (29 October 2026): additive inspection-only session component at Braid current 1.26.2 pin, with port-free data, explicit lifecycle/refusals and bounded drain. Draft API shape compiles with XML docs and zero warnings/errors; no implementation or consumer test run. Existing designer editing/preview/export defaults remain outside this component. No actual pin/protocol/core API change. Next: review/checkpoint, then consumer-owned implementation/ADR scope. [API proposal and adoption gates](braid-readonly-adoption/README.md).


Read-only adoption proposal reviewed (29 October 2026): compiled draft source/project/assembly/XML hashes and port-free data shape verified; zero-warning/error build receipt preserved. This is an additive inspection-only proposal at Braid 1.26.2, not implementation or actual public API adoption. Existing default editing/preview/export stays untouched; no product pin/core API/ID change. Earlier uncommitted notices are history; delivery resolves through review Git history. Next: consumer-owned implementation task/ADR at the unchanged pin. [Proposal review checkpoint](braid-readonly-adoption-review/README.md).


## Registered owner work — 30 October 2026

[REQ-139 C139-01–04](REQ-139-OWNER-WORK-PACKAGES.md) requests minimal public-transition-aware contract disposition, guarded implementation after approval, real consumer/API/gate qualification and immutable delivery. [Parallel handoff](PARALLEL-OWNER-WORK.md) links Schema REQ-009 work. Current bounded 1.34.1 integration is preserved; C45/general API acceptance is not inferred. Documentation only; no fresh engine tests/gates or new behavior.
