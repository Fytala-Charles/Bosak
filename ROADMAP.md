<div align="center">
  <img src="assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak roadmap">
  <br><br><h1>Bosak roadmap</h1>
  <p>Current direction for the open XML engine and schema foundation</p>
</div>

## Immediate foundation work

The bounded [streaming completion fix](docs/streaming-completion-cleanup/README.md) is qualified in unpublished source 1.34.2. The [external signature review](docs/foundation-external-dependencies/README.md) now proposes an explicit legacy compatibility ledger alongside the future neutral foundation. The [legacy/no-schema compatibility harness](docs/foundation-legacy-compatibility/README.md) now qualifies selected 1.34.1-to-1.34.2 source and retained-binary paths. The [immutable-source Core/API package candidate](docs/foundation-package-qualification/README.md) now passes selected package-only source/binary checks. The [package-backed full solution and frozen gates](docs/foundation-package-gates/README.md) now pass in the documented mixed assembly cohort. The [consolidated core readiness packet](docs/foundation-readiness/README.md) now pins exact artifacts and the remaining acceptance checklist. Owners execute their delegated qualification; Core reconciles returned needs into the finite C139-01/C45 contract decision and named stable member scope. Publication and shared API freeze remain separate dispositions. Continue joint member-scope, ownership and real consumer qualification as delegated owner requirements arrive. General schema API acceptance, package publication and the named shared API freeze remain pending.


Last updated: **30 October 2026**. This roadmap replaces the old beta/paired-commercial-launch checklist. Detailed measurements and historical milestones are in [implementation status](docs/IMPLEMENTATION_STATUS.md) and [development history](docs/DEVELOPMENT_HISTORY.md#roadmap-snapshot-2026-10-29).

## Where we are

| Area | Current position |
|---|---|
| Core engine | Published XPath 3.1, XSLT 3.0 and XQuery 3.1 packages for .NET 10; [1.26.3 is the verified published baseline](docs/RELEASE-1.26.3-PUBLICATION.md). |
| Source development | Candidate 1.33.1 is unpublished; source qualification does not imply a package release. |
| Bosak.Schema | [Public and Apache-2.0](https://github.com/Fytala-Charles/Bosak.Schema); activation-free schema services. Full XSD 1.1 structural validation and engine integration remain incomplete. |
| 4.0 language support | Explicit opt-in support for documented draft features; complete current-draft conformance is not claimed. |
| Authoring and tooling | VS Code integration and bounded source-preserving APIs support Braid's developing visual/source workflow. |

## Next priorities

1. **Integrate the open schema foundation.** The [XSD 1.0 example](integrations/Schema/README.md) and [native bounded XSD 1.1 assessment candidate](docs/schema-assessment/README.md) now provide real typed-value paths. Next, qualify the smallest whole-document/anonymous-root bridge while retaining original component state and explicit unsupported coverage. Production session-API acceptance remains pending.
2. **Qualify real providers and consumers.** Build and test the actual Schema, XmlDb and Braid integrations, including compatible-schema handoff, lazy results and unchanged-consumer ABI checks, before freezing or releasing the new integration.
3. **Complete staged XSD 1.1 coverage.** Implement or explicitly refuse remaining structural/assessment features; publish precise support profiles rather than broad conformance claims.
4. **Reconcile 4.0 draft behavior.** Triage the recorded JNode, record, map and comparison gaps against immutable adopted specification revisions. Keep each semantic change and conformance admission separately reviewed.

The [feature registry](docs/FEATURE_REQUESTS.md) owns individual request status and acceptance criteria. Implementation work does not automatically approve an API, alter a frozen gate or authorize publication.

## Continuing work

- Improve source-preserving authoring through bounded, reviewed operation contracts; see the [authoring roadmap](docs/STRUCTURAL-AUTHORING-ROADMAP.md).
- Extend streaming support within documented streamability and resource limits.
- Measure runtime and transformation performance before selecting optimizations.
- Preserve default-language behavior, source compatibility and clear consumer migration paths as newer profiles develop.

## Open-source and commercial boundaries

Bosak and **Bosak.Schema are Apache-2.0**. Schema compilation and validation need no activation, trial or subscription. The schema implementation remains a separate package; using one license does not require introducing a core dependency cycle.

Braid and Folio retain their separate commercial/proprietary terms. Billing and key-generation infrastructure remains in the private Bosak.Licensing repository. The Schema transition did not publish a new NuGet package, change live billing or alter another product's license.

Commercial support and consulting can fund development without restricting access to the open engine or schema services. See [commercial offerings](COMMERCIAL.md) and the [ecosystem overview](docs/ECOSYSTEM.md).

## Compatibility, limitations and evidence

Changes follow [Semantic Versioning](https://semver.org/) and documented adoption/migration decisions. Extended-year construction does not imply every date operation supports that range; .NET decimal precision and external HTTP behavior also impose documented limits. See [implementation status](docs/IMPLEMENTATION_STATUS.md) and [integration](docs/INTEGRATION.md) for technical details.

Historical launch, release, test and pricing statements remain preserved as dated evidence in [development history](docs/DEVELOPMENT_HISTORY.md), not as current roadmap gates.

The bounded [whole-document assessment bridge candidate](docs/schema-document-bridge/README.md) is under local qualification; broader schema coverage and consumer acceptance remain future work.

The [bounded bridge API review checkpoint](docs/schema-document-bridge/api-review/README.md) records current local qualification and the next structural-coverage boundary.

The next bounded native Schema slice is [optional decimal children](docs/schema-optional-children/README.md). Its specification adoption and verification plan are prepared; implementation and the proposed Schema 0.6 development line remain future work.

Scope refinement: a thirteenth isolated observation exposes ignorable-whitespace removal being refused for an empty element-only assessed root. Supporting all-optional-absent documents therefore needs a bounded provider binding correction. The future implementation proposes core patch 1.34.1 plus fresh complete core tests and exact conformance gates; current versions and production source stay unchanged. Both the initial twelve and extended thirteen-observation receipts are preserved in the optional-child dossier; they overlap.

The [optional-child scope delivery](docs/schema-optional-children/delivery/README.md) records the paired review/backup branches. Optional support and the proposed version changes remain future implementation work.

The bounded [optional decimal implementation](docs/schema-optional-implementation/README.md) is now a locally qualified candidate (Schema 0.6/core 1.34.1). Further structural coverage, broader schema/session acceptance and product adoption remain future work.

The [optional assessment implementation](docs/schema-optional-review/README.md) is reviewed for checkpoint delivery. Bounded repeated-child coverage is the next scope; general schema/session acceptance and product adoption remain future work.

The next [bounded repetition slice](docs/schema-repeated-children/README.md) is specified but not implemented. Current optional support remains Schema 0.6/core 1.34.1; Schema 0.7/v3 is a future candidate target. General schema/session acceptance and product adoption remain separate.

The [bounded repeated-decimal implementation](docs/schema-repeated-implementation/README.md) is a local Schema 0.7 candidate with unchanged core 1.34.1. Review/checkpoint precedes further structural or assertion-grammar expansion; general schema/session acceptance and product adoption remain future work.

The [bounded repetition implementation](docs/schema-repetition-review/README.md) is reviewed for checkpoint delivery. A separately adopted fn:count assertion slice is next; aggregation and broader grammar remain outside that first function scope.

The next [count assertion slice](docs/schema-count-assertions/README.md) is adopted/planned but unimplemented. Schema 0.7/core 1.34.1 remain current; Schema 0.8/v4 would mark the future count feature. Other aggregates and wider namespace/function grammar remain separate.

The [bounded count implementation](docs/schema-count-implementation/README.md) is now a local Schema 0.8 candidate with unchanged core 1.34.1. Review/checkpoint comes before wider aggregation, namespaces/functions or general schema/session acceptance.


**29 October 2026 review:** Schema 0.8 count assertions reviewed: fresh 397/397 Schema tests pass; core 1.34.1 source and inherited engine gates remain unchanged. Next is the separately scoped presence-predicate implementation; exists/empty remain unsupported in this checkpoint. [Review and next scope](docs/schema-count-review/README.md).


Current presence implementation — 29 October 2026: Schema 0.9/v5 supports bounded unprefixed exists(child)/empty(child) Boolean predicates, retaining count-only v4 and no-call v1–v3. Complete Schema qualification: 451 passes (54 new); build zero warnings/errors; native example and real preserve/strip consumer tests pass. Two prior refusal expectations are explicitly adopted, 395 unchanged. Core 1.34.1 and its source-identical engine qualification remain inherited. Current implementation is uncommitted/unpublished; next is review and checkpoint. Earlier scope-only/presence-unsupported notices are historical capture state. [Implementation and evidence](docs/schema-presence-implementation/README.md).


**29 October 2026 review:** Schema 0.9/v5 presence predicates reviewed: fresh 451/451 Schema tests pass and all seven tested implementation source identities match. Core 1.34.1 remains unchanged with inherited engine/gate qualification. No further production correction was needed. Earlier uncommitted notices are historical capture state; checkpoint delivery resolves through the review document Git history. [Review](docs/schema-presence-review/README.md).


Consumer-priority assessment (29 October 2026): qualify the actual XmlDb persistent-node adapter in an isolated source workspace next, then Braid authoring/preview compatibility. No inspected dependency requires more native assertion functions now. Native structural expansion needs a concrete schema/instance use case. Schema 0.9/core 1.34.1 and all prior qualifications remain unchanged; no consumer execution or dependency adoption is claimed. [Assessment and acceptance plan](docs/schema-consumer-priorities/README.md).


Isolated XmlDb qualification (29 October 2026): committed adapter source builds unchanged against current core; 27/27 existing Xdm tests pass on both 1.26.2 and 1.34.1. All 44 bounded diagnostic observations agree, including 26 query pairs, seven persisted kind mappings and nested snapshot disposal. Unrelated consumer dirty work is excluded. No dependency adoption, binary/native/API acceptance or production fix. Next: isolated Braid source compatibility. [Qualification and limits](docs/xmldb-compatibility/README.md).


Isolated Braid qualification (29 October 2026): all actual consumer source builds against current core without a production fix. Original full suites on 1.26.2/current 1.34.1 each report 1,872 passes/four exact-version failures; separate corrected/adapted identity follow-ups pass all four. No single green unchanged full-suite claim, actual dependency adoption or native desktop acceptance. Next: review/checkpoint the consumer-priority and both compatibility dossiers. [Qualification and limits](docs/braid-compatibility/README.md).


Consumer qualification reviewed (29 October 2026): exact sealed receipts and test identities verified; XmlDb 27 passes per version/44 matching observations, Braid 1,872 passes/four version failures plus separate four passing identity follow-ups per version. No further production fix, dependency adoption or binary/native acceptance. Earlier uncommitted notices are historical; delivery resolves through the review document Git history. Next: separately scope unchanged-binary qualification. [Review checkpoint](docs/consumer-qualification-review/README.md).


Unchanged-binary qualification scoped (29 October 2026): exact retained baseline/candidate assembly hashes and identities are inventoried. Next: frozen baseline controls then engine-only replacement, preserving consumer DLLs, hosts, manifests and original version assertions. No replacement or new consumer execution yet; source qualification is not binary compatibility. Core 1.34.1/Schema 0.9 and actual product pins remain unchanged. [Execution plan](docs/binary-compatibility-scope/README.md).


Unchanged-binary qualification (29 October 2026): baseline controls pass XmlDb 27/Braid 1,876; engine-only replacement passes XmlDb 27 with identical 44 observations and Braid 1,872/four original version failures. All consumer/host/manifest bytes stay frozen; actual candidate loads and preview workers are verified. Additional public-contract probe shows Braid inspector/preview identity stays old pin despite loaded 1.34.1; transparent product adoption is not qualified. No production fix or dependency adoption. Next: review/checkpoint and separately resolve consumer runtime-identity policy before upgrade. [Results and limits](docs/binary-compatibility-execution/README.md).


Unchanged-binary qualification reviewed (29 October 2026): raw counts/identities, frozen layouts and actual replacement loads verified. XmlDb 27/27 plus identical 44 observations; Braid baseline 1,876 passes, replacement 1,872/four original version failures. Braid advertised runtime identity remains old pin; transparent adoption is not qualified. No production fix/pin/release change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: separately scope consumer identity/adoption policy. [Review checkpoint](docs/binary-compatibility-review/README.md).


Braid identity/adoption policy scoped (29 October 2026): recommend reviewed rebuild at an explicit pin plus runtime mismatch refusal using an approved artifact manifest, consistent parent/worker identity and preserved historical results. Dynamic automatic replacement is deferred. Next: isolated contract prototype with admission/refusal controls and explicit runtime-verification limits. No Braid/core implementation, actual pin, protocol, storage or release change; scope only. [Policy and acceptance scope](docs/braid-engine-identity-policy/README.md).


Isolated identity-policy prototype (29 October 2026): 47 modeled contract controls pass, six enforcement mutants detected, 18 actual PE/hash observations match. Recorded Braid trace observes eight/nine engine members; complete-runtime evidence therefore refuses. Model flags/envelopes are not an actual runtime/worker guard. No product/pin/core API change. Next: review/checkpoint, then resolve a real lazy-loading/binding admission design in isolation. [Prototype evidence and limits](docs/braid-identity-prototype/README.md).


Identity-policy prototype reviewed (29 October 2026): 47 controls/six detected mutants/18 real file observations reconciled; tested source/project/assembly hashes match. All prior evidence preserved. Runtime-binding flags and worker envelopes remain modeled; recorded eight/nine loaded members expose an unresolved lazy-admission boundary. No actual product guard, pin, protocol or core API change. Earlier uncommitted notices are capture history; delivery resolves through review Git history. Next: isolated actual runtime/lazy-binding experiment. [Review checkpoint](docs/braid-identity-review/README.md).


Actual-runtime admission experiment (29 October 2026): 34 real controls pass with nine independent worker exchanges. Approved snapshots allow eight-member XSLT then deferred ninth-member XQuery binding; actual byte/identity/MVID evidence, drift revocation and worker mismatch rejection are recorded. This is a standalone custom-load-context host, not Braid integration or a product protocol/pin/core API change. Next: review/checkpoint, then consumer-owned boundary/type-identity design. [Runtime evidence and limits](docs/braid-runtime-admission/README.md).


Actual-runtime experiment reviewed (29 October 2026): 34 real controls/nine worker exchanges, tested source/assembly hashes and approved snapshot ledgers verified; earlier evidence preserved. Lazy eight-to-nine-member binding works in the standalone host. Actual Braid typed adapters/default loader/protocol are not integrated; no product pin/core API change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: consumer-owned integration-boundary design. [Review checkpoint](docs/braid-runtime-review/README.md).


Braid integration design (29 October 2026): prefer approved snapshot-backed adapter sessions sharing exact host Application/Core contracts. Real typed casts, opaque ownership and callback/lifecycle behavior remain to prove; default-loader hash prechecks do not establish the same binding. Next: isolated real read-only inspector typed-port proof. No actual Braid implementation, dependency pin, protocol, storage or core API change. [Design and vertical-slice acceptance](docs/braid-integration-design/README.md).


Real typed-inspector proof (29 October 2026): 41 controls pass; four encodings match a separate direct baseline across shared Application/Core types, callbacks, source bytes/extents and actual target ownership. Raw port still returns NoChange after unload request; diagnostic lease closure alone is insufficient for a product guard. No edits/product pin/core API change. Next: review/checkpoint, then real exported-port facade admission proof. [Typed proof and lifetime boundary](docs/braid-typed-inspector/README.md).


Typed-inspector/integration design reviewed (29 October 2026): 41 controls, four exact direct/isolated projections and private/shared assembly hashes verified; earlier evidence preserved. Raw-port NoChange remains callable after unload request, so outer lease/unload is not revocation. No actual production guard, pin or core API change. Earlier uncommitted notices are historical; delivery resolves through review Git history. Next: isolated guarded exported-port/target facade proof. [Review checkpoint](docs/braid-typed-inspector-review/README.md).


Guarded-port facade proof (29 October 2026): 54 controls pass over the actual inspector. Supported public result ports/targets are wrapped or removed; closed/foreign/drift admission and controlled in-flight close refuse raw authority publication. Profile is inspection/rename-target/exact NoChange only; broader authoring remains refused. Raw adapter control still reproduces the old lifetime gap. No actual product guard/pin/core API change. Next: review/checkpoint, then consumer-owned facade/lifecycle scope. [Facade evidence and limits](docs/braid-guarded-ports/README.md).


Guarded-port facade reviewed (29 October 2026): 54 controls, nine result ports/seven target properties and source/assembly/artifact hashes verified; prior evidence preserved. Profile remains inspection/rename-target/exact NoChange only; controlled close is qualified, arbitrary concurrency is not. Raw adapter gap remains unchanged in product. No actual guard/pin/core API change. Earlier uncommitted notices are history; delivery resolves through review Git history. Next: consumer-owned facade/lifecycle contract scope. [Review checkpoint](docs/braid-guarded-ports-review/README.md).


Consumer facade/lifecycle contract scoped (29 October 2026): propose one active engine call per session with immediate overlapping/reentrant refusal, nonblocking close initiation and retained in-flight ownership until completion. Separate engine capability facts from admitted operations; guard every returned authority surface. Post-admission cancellation, callback/drain ordering and broader schedules still need proof. No product/core API/pin/protocol change. Next: isolated deterministic lifecycle proof. [Contract scope and schedule matrix](docs/braid-facade-lifecycle-contract/README.md).


Deterministic facade lifecycle proof (29 October 2026): 65 controls across 13 isolated cases pass, including real callback overlap/reentry, self-drain refusal, post-admission cancellation, caller-abandoned waits and incomplete drain. All leases balance; one underlying release request per case, not GC proof. Engine facts/admission profile are separate. Arbitrary concurrency and actual Braid product integration remain unqualified. No product pin/core API change. Next: review/checkpoint, then finite consumer adoption proposal. [Schedule evidence and limits](docs/braid-lifecycle-proof/README.md).


Lifecycle contract/proof reviewed (29 October 2026): 65 controls and 13 ordered state/lease traces verified, with balanced leases and one idle release request per case; source/assembly/snapshot hashes match. Earlier evidence preserved. Finite callback/cancellation schedules do not certify arbitrary concurrency or actual product integration. No pin/core API change. Earlier uncommitted notices are history; delivery resolves through review Git history. Next: finite consumer-owned read-only adoption proposal. [Review checkpoint](docs/braid-lifecycle-review/README.md).


Finite read-only adoption proposal (29 October 2026): additive inspection-only session component at Braid current 1.26.2 pin, with port-free data, explicit lifecycle/refusals and bounded drain. Draft API shape compiles with XML docs and zero warnings/errors; no implementation or consumer test run. Existing designer editing/preview/export defaults remain outside this component. No actual pin/protocol/core API change. Next: review/checkpoint, then consumer-owned implementation/ADR scope. [API proposal and adoption gates](docs/braid-readonly-adoption/README.md).


Read-only adoption proposal reviewed (29 October 2026): compiled draft source/project/assembly/XML hashes and port-free data shape verified; zero-warning/error build receipt preserved. This is an additive inspection-only proposal at Braid 1.26.2, not implementation or actual public API adoption. Existing default editing/preview/export stays untouched; no product pin/core API/ID change. Earlier uncommitted notices are history; delivery resolves through review Git history. Next: consumer-owned implementation task/ADR at the unchanged pin. [Proposal review checkpoint](docs/braid-readonly-adoption-review/README.md).


Owner-ready integration queue: [Bosak REQ-139 C139-01–04](docs/REQ-139-OWNER-WORK-PACKAGES.md) and [parallel public Schema work](docs/PARALLEL-OWNER-WORK.md). Start contract disposition and provider/coverage preparation independently of Braid product gates; C45 implementation and release require their own owner decisions.

## Shared foundation goal — 30 October 2026

Bosak REQ-140 now registers the shared foundation for both Bosak.Schema and Bosak.XmlDb. XmlDb is a required acceptance consumer alongside Schema. [The coordinated plan](docs/FOUNDATION-API-FREEZE-PLAN.md) defines F140-01 through F140-04: joint contract inventory, accepted gap implementation, actual dual-consumer qualification, then publication and a named versioned API freeze. Core REQ-139 supplies the neutral schema seam; Schema owns full XSD 1.1 implementation. The bounded assessment is not full support. Freeze and C45 acceptance remain pending; existing package pins, storage formats and Braid product gates remain unchanged.


## F140-01 core API baseline — 30 October 2026

The [compiled API inventory and proposed freeze scope](docs/foundation-api-baseline/README.md) records source `f758a29f` / candidate 1.34.1: 11 assemblies, 270 exported types and 5,766 metadata entries. Core inventory is complete; member-level scope, joint Schema/XmlDb requirements, actual consumer qualification and freeze acceptance remain pending. Contract observations distinguish required/default interfaces, XML enum values, value-type field shape, lazy ownership and the existing bounded schema bridge. Nine open decisions are recorded. Fresh Release build passed with five existing nullable warnings; no runtime suites or conformance gates were rerun for this metadata audit. Production APIs, package pins and external owner work are unchanged.


## F140-01 member-level draft — 30 October 2026

The [proposed contract manifest and compatibility matrix](docs/foundation-api-contract-draft/README.md) cover all 5,766 baseline entries with exact overload IDs, interface-slot distinctions and planned case links. Conservative Core/query signature reachability includes 11 additional Runtime types (40 total); these require explicit review before final member selection. Twenty behavioral/package/consumer cases remain NotRun. Ten audit-tool tests pass; engine source, public APIs, external owner work and pins are unchanged. Joint requirements, FG-01/04/07 decisions, C45 acceptance and freeze remain pending. Next: focused lazy/escaped-result lifetime and ownership decision note.


## F140-01 result ownership review — 30 October 2026

The [proposed ownership disposition](docs/foundation-result-ownership/DECISION.md) traces lazy, nested and escaped result paths. Ten bounded memory-only observations confirm no universal early-release protocol and reproduce an owned-reader cleanup gap when StreamCompleted throws. Existing interfaces/layouts remain unchanged; explicit provider/host resource ownership is the proposed compatibility baseline, with an optional scope subject to actual consumer needs. Next core slice: exception-safe owned streaming completion with failure-precedence tests. No engine fix, consumer qualification, C45 acceptance or API freeze occurs here; the previous twenty compatibility gates remain NotRun.
