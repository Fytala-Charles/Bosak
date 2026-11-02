# Release plan and acceptance checklist

Updated 1 November 2026. Current source version: **1.42.0**. Target publication version and date: **not selected**. Source version increments identify engineering checkpoints; they do not mean a package was published or an API frozen.

The [consolidated answers and execution plan](../foundation-r2-consolidation/README.md) is the current owner-disposition index: five submitted returns, three scoped design accepts, two revisions and two missing Schema returns. The internal foundation is qualified within its bounded private profile. The remaining critical path is **public contract disposition → public implementation → actual consumer/performance qualification → four-owner acceptance → release candidate → publication**. [Owner capability mapping](OWNER-CAPABILITIES.md) identifies concrete gaps from pinned source, rather than treating synthetic tests as consumer acceptance. [GATES.json](GATES.json) is the machine-readable gate registry.

| Gate | Status | Accountable role | Exit evidence |
|---|---|---|---|
| R0 Private engine foundation | Qualified | Core | 1.42.0 source/build, 24,390 unit passes, unchanged frozen gates; private capture, evidence and multi-source execution. [Qualification](../foundation-xslt-source-membership/README.md). |
| R1 Actual owner capability mapping | Audited | Core | Exact read-only source/requirement pins and explicit gaps. [Mapping](OWNER-CAPABILITIES.md). This is not runtime owner acceptance. |
| R2 Final public contract disposition | In review; Core ledger recorded | Core with Schema/XmlDb/Braid | Finite declaration/requirement decision ledger, supported/refused scope, immutable semantic basis and actual owner dispositions. |
| R3 Accepted public implementation | Pending R2 | Core | Selected public members implemented and documented; fresh API/ABI inventory and functional/lifetime/error tests. Existing private implementation is reusable groundwork. |
| R4 Real consumers and performance | Pending candidate; preparation parallel | Core/Schema/XmlDb/Braid | One immutable candidate; actual source and unchanged-binary consumer runs; representative PF-01–09 budgets; complete core/conformance qualification. |
| R5 C45 acceptance and API freeze | Pending R4 | All four roles | Dated acceptance of exact manifest/profile/candidate; no unresolved required signature gap. Intended final breaking foundation change. |
| R6 Release candidate packages/docs | Pending freeze; preparation parallel | Core | Selected SemVer, package provenance, migration/release notes, complete docs and fresh-cache package-only consumer tests. |
| R7 Publish and verify | Pending reviewed artifacts and release approval | Core | Approved package/tag/release identities, publication receipts, downloaded public artifact verification. Consumer pin adoption remains separate. |


Current revised-draft status (2 November): [Schema O1/O2 reviewed](../foundation-schema-response-review/README.md), both Revise. All five external role forms are submitted: two scoped accepts/three revisions; only Core O2/O4 remain unsubmitted. Next Core records its own reviews and clarifies existing explicit-limit policy; Schema source/profile and XmlDb native lifetime revisions remain open. R2/freeze/publication stay pending.

## What is already demonstrated

The latest full production qualification passed **24,390 unit tests**, QT3 **31,397/0/424**, gated qt4 **8,941/0/1,918** and 11 experimental passes. Public metadata remains **270 types / 5,770 declarations**. The private factories, same-session snapshot leases, typed input stamps, separate XSLT source roles and deferred cleanup are implemented. A further [57 host acquisition/rollback controls](../foundation-host-acquisition-rollback/README.md) passed twice without production changes. These receipts do not export or accept the proposed public SDK.

The [owner mapping](OWNER-CAPABILITIES.md) is completed as a read-only audit. Notable decisions still needed are ordinary XmlDb admission without schema authority, neutral authority/assessment handles, exact numeric and list/union representation, advisor/compatible-view semantics, real resource failure preservation and finite concurrency profiles.

## Immediate next work

- [x] Core ledger covers all proposed type/member/requirement rows; the isolated review draft compiles. Core O2/O4 and XmlDb O1 have scoped design accepts, not runtime acceptance.
- [x] [Consolidate five submitted returns and Schema checklist guidance](../foundation-r2-consolidation/README.md); independently verify XmlDb's newer b09c7341 source/evidence pins.
- [x] **Core P03 complete within its bounded profile:** qualification and cost/cancellation observations are recorded.
- [ ] **Core P04 coordination:** Braid scoped design acceptance recorded; Core O2/O4 revised returns, Schema O1/O2 revisions and XmlDb provider revision remain.
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
