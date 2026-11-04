# Release plan and acceptance checklist

Updated 4 November 2026. Current source version: **1.53.0**. Target publication version and date: **not selected**. Source version increments identify engineering checkpoints; they do not mean a package was published or an API frozen.

The [scoped R2 closure](../foundation-r2-closure/README.md) records all seven role returns as AcceptWithScope against the reviewed declaration manifest. R3 is in progress: internal prerequisites and the [ordinary-source public facade](../foundation-public-ordinary/README.md) are qualified. The ordinary-source public facade is qualified within its string/boolean profile. The bounded exact arithmetic profile is qualified for literals/variables, unary signs and +,-,*. Exact numeric arithmetic/scalars, map keys, terminating division and primitive numeric constructors are qualified and TA-01 internal containers are qualified at 1.53.0; public schema adapter activation, all-host routing and migration remain next; cache/typed-projection and wider casts/rounding/error policy remain open. The remaining critical path is public implementation, actual consumer/performance qualification, four-owner acceptance, release candidate and publication. [Owner capability mapping](OWNER-CAPABILITIES.md) and [GATES.json](GATES.json) retain the requirements and machine-readable gates.

| Gate | Status | Accountable role | Exit evidence |
|---|---|---|---|
| R0 Private engine foundation | Qualified | Core | 1.42.0 source/build, 24,390 unit passes, unchanged frozen gates; private capture, evidence and multi-source execution. [Qualification](../foundation-xslt-source-membership/README.md). |
| R1 Actual owner capability mapping | Audited | Core | Exact read-only source/requirement pins and explicit gaps. [Mapping](OWNER-CAPABILITIES.md). This is not runtime owner acceptance. |
| R2 Final public contract disposition | ClosedWithScope | Core with Schema/XmlDb/Braid | Seven validated scoped role returns and the immutable R2 closure; no runtime acceptance inferred. |
| R3 Accepted public implementation | InProgress | Core | Qualified ordinary facade, finite numeric profiles and TA-01 internal containers at 1.53.0. Public schema adapter/cache/typed-projection policy, wider numeric/error and all-host integration remain pending. |
| R4 Real consumers and performance | Pending candidate; preparation parallel | Core/Schema/XmlDb/Braid | One immutable candidate; actual source and unchanged-binary consumer runs; representative PF-01–09 budgets; complete core/conformance qualification. |
| R5 C45 acceptance and API freeze | Pending R4 | All four roles | Dated acceptance of exact manifest/profile/candidate; no unresolved required signature gap. Intended final breaking foundation change. |
| R6 Release candidate packages/docs | Pending freeze; preparation parallel | Core | Selected SemVer, package provenance, migration/release notes, complete docs and fresh-cache package-only consumer tests. |
| R7 Publish and verify | Pending reviewed artifacts and release approval | Core | Approved package/tag/release identities, publication receipts, downloaded public artifact verification. Consumer pin adoption remains separate. |



3 November: [public ordinary-source facade qualified at source 1.45.0](../foundation-public-ordinary/README.md), with 38 public controls and the reviewed additive signature subset. Continue numeric/schema transport and all-host implementation before owner qualification and freeze.


3 November: [exact-decimal representation qualified at source 1.46.0](../foundation-exact-decimal/README.md); 46 new controls and allocation-before-refusal checks pass. Next exact XdmValue transport and checked legacy accessors, then numeric/schema/all-host integration.


4 November: [exact XdmValue carrier transport qualified at source 1.47.0](../foundation-exact-value-transport/README.md); 32 new controls and all previous API fields/rows retained. Record typed-default source migration; next bounded numeric operation limits and exact execution routing.


4 November: [bounded exact arithmetic qualified at source 1.48.0](../foundation-bounded-numeric/README.md), with 44 new controls and unchanged public declarations. Next exact comparisons/map keys and bounded serialization, then broader numeric/schema/all-host/native qualification.

4 November: [exact comparisons, map keys and scalar output qualified at 1.49.0](../foundation-exact-scalars/README.md); 50 new controls, unchanged public metadata. Next wider numeric operators, schema/all-host/native qualification.

4 November: [exact quotient/remainder qualified at 1.50.0](../foundation-exact-quotient/README.md); 29 new controls, unchanged public metadata. Next decimal division policy and schema/all-host/native qualification.

4 November: [exact terminating division policy selected and reference-checked](../foundation-decimal-division-policy/README.md); source remains 1.50.0. Next implement/qualify the finite subset; general rounded decimal division remains unresolved.

4 November: [exact terminating division qualified at 1.51.0](../foundation-exact-division/README.md); 33 new controls, unchanged public metadata. Next typed/schema numeric and all-host/native qualification; wider rounding/error policy remains pending.

4 November: [primitive numeric constructors qualified at 1.52.0](../foundation-exact-constructors/README.md); 24 new controls, unchanged public metadata. Next issuer-backed typed/schema admission and all-host/native qualification; wider casts/rounding/error policy remain pending.

4 November: [issuer-backed typed admission audited; TA-01 prepared](../foundation-typed-admission-plan/README.md). Next internal uncached container reader, then explicit cache/typed-profile policy and public adapter qualification; source remains 1.52.0.

4 November: [TA-01 internal typed containers qualified at 1.53.0](../foundation-typed-container/README.md); 94 new controls and unchanged public API source/metadata. Next cache/evidence and typed numeric projection policy before public adapter activation.

## Earlier planning checkpoints

The dated entries below retain earlier dispositions; the summary and gate table above describe current status.


Current ecosystem (2 November): [four-project status](../foundation-ecosystem-status/README.md) records XmlDb native fix and fresh scoped O4 acceptance. All seven active returns: five scoped accepts/two Schema revisions. Next obtain Schema exact source/policy dispositions, with R3 scope preparation parallel; public implementation, actual candidate integration/budgets and freeze remain pending.


Schema decision handoff: [three precise questions and localized source evidence](../schema-precise-disposition/REQUEST.md) are available inside Schema's workspace. Updated owner dispositions remain pending; all current counts and gates are unchanged.


3 November: [Schema accepts O1/O2 design with scope](../schema-acceptance-preparation/README.md); versioned carry-forward confirmation remains pending. R2 stays InReview; the next neutral admission slice is prepared.


3 November: [W1-A is explicitly blocked pending gate evidence](../schema-w1-gate-review/README.md). Trusted ordinary-source membership, complete retained acceptance checklist and exact qualification commands are now specified; no implementation has started.


3 November: [R2 is ClosedWithScope; W1-A entry is Ready for implementation](../foundation-r2-closure/README.md). Seven exact scoped returns are validated; semantic adoption precedes source edits. Implementation, native qualification, freeze and publication remain pending.


3 November: [internal W1-A neutral admission is implemented and qualified at source 1.43.0](../foundation-w1-neutral-admission/README.md). R3 is InProgress; optional schema composition, public/all-host integration and native/performance qualification remain ahead.


3 November: [bounded internal W1-B schema association is qualified at source 1.44.0](../foundation-w1-schema-association/README.md). Ordinary inputs stay issuer-free; optimize context allocations before public/all-host integration and R4 budgets. Wider typed/numeric/native work remains pending.


3 November: [private allocation fix qualified at source 1.44.1](../foundation-function-table-sharing/README.md), saving 19,256 bytes per captured evaluation with public allocations/API unchanged. Continue accepted public/all-host integration; native and budget qualification remain pending.


3 November: [open-session drain qualified at source 1.44.2](../foundation-open-session-drain/README.md); active work can drain without closing admissions. Continue the accepted public facade and all-host integration.

## What is already demonstrated

The latest full production qualification passed **24,390 unit tests**, QT3 **31,397/0/424**, gated qt4 **8,941/0/1,918** and 11 experimental passes. Public metadata remains **270 types / 5,770 declarations**. The private factories, same-session snapshot leases, typed input stamps, separate XSLT source roles and deferred cleanup are implemented. A further [57 host acquisition/rollback controls](../foundation-host-acquisition-rollback/README.md) passed twice without production changes. These receipts do not export or accept the proposed public SDK.

The [owner mapping](OWNER-CAPABILITIES.md) is completed as a read-only audit. Notable decisions still needed are ordinary XmlDb admission without schema authority, neutral authority/assessment handles, exact numeric and list/union representation, advisor/compatible-view semantics, real resource failure preservation and finite concurrency profiles.

## Immediate next work

- [x] Core ledger covers all proposed type/member/requirement rows; the isolated review draft compiles. Core O2/O4 and XmlDb O1 have scoped design accepts, not runtime acceptance.
- [x] [Consolidate five submitted returns and Schema checklist guidance](../foundation-r2-consolidation/README.md); independently verify XmlDb's newer b09c7341 source/evidence pins.
- [x] **Core P03 complete within its bounded profile:** qualification and cost/cancellation observations are recorded.
- [ ] **Core P04 coordination:** Braid scoped design acceptance recorded; Core O2/O4 scoped accepts recorded; Schema O1/O2 revisions remain; XmlDb fresh O4 scoped acceptance recorded.
- [ ] **XmlDb P06/P07, parallel:** resolve actual cleanup/failure/profile revision XDB-LIFE-01 and implement/qualify provider-local membership. No additional required Core disposal signature has been demonstrated.
- [ ] **Schema P05, parallel:** submit actual O1/O2 dispositions using the eight checklist work items, with actual methods/profile/gaps/evidence. The issue comment is guidance, not those responses.
- [ ] **P08:** reconcile changed draft hashes and all owner dispositions, then close accepted R2 scope. R3 production implementation remains gated; W1–W7 and targeted follow-up controls are prepared, not completed.

See the [dependency queue](../foundation-r2-consolidation/PLAN.json), [Core capture plan](../foundation-r2-consolidation/CORE-CAPTURE-PLAN.md) and [parallel owner actions](../foundation-r2-consolidation/OWNER-ACTIONS.md). Do not replace these named deliverables with another generic lifecycle model or planning-only checkpoint.

## Work that can continue in parallel

Schema can advance its supported XSD 1.1 backend and provider fixtures; XmlDb can qualify acquisition failure/reader release and ordinary compatibility; Braid can settle worker/invocation/currentness design. Core can prepare packaging/provenance, migration guidance and ABI/performance harnesses. Final adapter binding depends on selected public signatures; broader Braid product validation does not block the Core/Schema contract queue.

Full XSD 1.1 backend completion is not a prerequisite for representing its known foundation API needs. Unsupported backend features must refuse explicitly. All known required signature needs and representative actual consumer/performance acceptance must be resolved before the intended final breaking API freeze.

## Evidence and planning limits

No percentage complete or release date is assigned: R2 and actual consumer returns can still change implementation scope. The target release number must be chosen after compatibility disposition; **1.42.0 is not automatically the publication target**. This plan does not authorize publication or approve a package upgrade for another project.

[INPUTS.json](INPUTS.json) preserves exact Core and owner revisions. The old roadmap is retained as [an archival text snapshot](inputs/ROADMAP-before-consolidation.txt); its relative links retain their original repository-root context. Earlier checkpoint evidence remains in its original dossiers and [development history](../DEVELOPMENT_HISTORY.md). Sealed acceptance templates/receipts are unchanged.

This step changes documentation only. Prior engine tests/gates are inherited receipts, not fresh runs here. Validation checks immutable source pins, gate dependencies/statuses, unchanged production files and rendered links. Run `python -B docs/release-planning/tools/verify.py` for read-only verification.
