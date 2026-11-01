# Release plan and acceptance checklist

Updated 1 November 2026. Current source version: **1.42.0**. Target publication version and date: **not selected**. Source version increments identify engineering checkpoints; they do not mean a package was published or an API frozen.

The internal foundation is qualified within its bounded private profile. The remaining critical path is **public contract disposition → public implementation → actual consumer/performance qualification → four-owner acceptance → release candidate → publication**. [Owner capability mapping](OWNER-CAPABILITIES.md) identifies concrete gaps from pinned source, rather than treating synthetic tests as consumer acceptance. [GATES.json](GATES.json) is the machine-readable gate registry.

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

## What is already demonstrated

The latest full production qualification passed **24,390 unit tests**, QT3 **31,397/0/424**, gated qt4 **8,941/0/1,918** and 11 experimental passes. Public metadata remains **270 types / 5,770 declarations**. The private factories, same-session snapshot leases, typed input stamps, separate XSLT source roles and deferred cleanup are implemented. A further [57 host acquisition/rollback controls](../foundation-host-acquisition-rollback/README.md) passed twice without production changes. These receipts do not export or accept the proposed public SDK.

The [owner mapping](OWNER-CAPABILITIES.md) is completed as a read-only audit. Notable decisions still needed are ordinary XmlDb admission without schema authority, neutral authority/assessment handles, exact numeric and list/union representation, advisor/compatible-view semantics, real resource failure preservation and finite concurrency profiles.

## Immediate next task: close R2 owner dispositions

- [x] [Core ledger](../foundation-r2-ledger/README.md) maps all 45 proposed types, 615 baseline metadata/evidence rows, six numeric member intents and 43 requirement rows.
- [x] Four boundary method changes compile in four isolated assemblies; 45 revised proposed types remain, with 619 metadata/evidence rows. Production is unchanged.
- [x] Prepare [O1–O4 owner packets](../foundation-r2-owner-review/README.md), seven unanswered role-specific return forms and draft-bound completeness checks. Delivery and acceptance are not claimed.
- [x] Record [O2-Core and O4-Core scoped design returns](../foundation-r2-core-returns/README.md), authored by Codex on the Core engineering task; no external/runtime/C45 acceptance is inferred.
- [ ] Obtain five external role returns: O1-Schema, O1-XmlDb, O2-Schema, O3-Braid and O4-XmlDb. Resolve all demonstrated required gaps before R2 closes.
- [ ] Record accepted implementation scope and any newly affected immutable specification revisions/sections/deviations before semantic implementation. The Core ledger is not owner acceptance.
- [x] Publish a finite W1–W7 implementation/qualification queue, gated by the corresponding accepted R2 scope. Prepare fixtures in parallel; do not substitute more synthetic lifecycle experiments for these decisions.

## Work that can continue in parallel

Schema can advance its supported XSD 1.1 backend and provider fixtures; XmlDb can qualify acquisition failure/reader release and ordinary compatibility; Braid can settle worker/invocation/currentness design. Core can prepare packaging/provenance, migration guidance and ABI/performance harnesses. Final adapter binding depends on selected public signatures; broader Braid product validation does not block the Core/Schema contract queue.

Full XSD 1.1 backend completion is not a prerequisite for representing its known foundation API needs. Unsupported backend features must refuse explicitly. All known required signature needs and representative actual consumer/performance acceptance must be resolved before the intended final breaking API freeze.

## Evidence and planning limits

No percentage complete or release date is assigned: R2 and actual consumer returns can still change implementation scope. The target release number must be chosen after compatibility disposition; **1.42.0 is not automatically the publication target**. This plan does not authorize publication or approve a package upgrade for another project.

[INPUTS.json](INPUTS.json) preserves exact Core and owner revisions. The old roadmap is retained as [an archival text snapshot](inputs/ROADMAP-before-consolidation.txt); its relative links retain their original repository-root context. Earlier checkpoint evidence remains in its original dossiers and [development history](../DEVELOPMENT_HISTORY.md). Sealed acceptance templates/receipts are unchanged.

This step changes documentation only. Prior engine tests/gates are inherited receipts, not fresh runs here. Validation checks immutable source pins, gate dependencies/statuses, unchanged production files and rendered links. Run `python -B docs/release-planning/tools/verify.py` for read-only verification.
