# Release readiness checklist ? 8 November 2026

**Status: preparation scope selected - bounded Core packages plus the VS Code extension; publication remains blocked by acceptance and delivery gates.** The process-probe restriction is resolved. This checklist does not accept budgets, waive requirements or authorize publication.

Current final local source: `1d69d059c14888c1c43264efa8668909292af858`; alignment and generated-server correction commits remain local. [Final stock qualification](release-1.81.2-final/README.md) passes 26,342 solution tests in one invocation, server90 and all retained gates/editor checks, with both artifact sets sealed. Historical source candidate: `c0b926de34824ceb2010d5f814a4450b28b31dcc`. Local preview: **1.81.1-preview.c0b926de**, nine-library Bosak.Xslt closure. Latest committed code/evidence checkpoint: `e992d90b0efa01d45224771a9f8e53dee05096a6`. Existing preview artifacts retain their C0 identity. [Selected candidate decisions](release-final-candidate-plan/README.md) approve local preparation of Core/server1.81.2, extension0.2.6 and the existing Database REST-loader package as the tenth NuGet library. [The rebuilt local cohort](release-1.81.2-candidate/README.md) is technically qualified; owner/API and publication gates remain open.

The user selected this preparation scope on 8 November 2026, including the extension explicitly. [Core/extension preparation](release-core-vscode/README.md) records deliverables, initial checks and remaining VSIX/install/upgrade work. Schema/XmlDb/Braid production adoption is deferred from delivery; existing requirements and owner acceptance need explicit disposition rather than an assumed waiver.

## Completed evidence

- [x] [Local package artifacts and consumer checks](foundation-preview-packages/README.md): nine packages, fresh-cache package-only consumer and retained caller; source/package/cache/output/load hashes.
- [x] [Package-backed qualification](foundation-preview-qualification/README.md): solution26,342; separate language-server84, native451, adapter59 and consumer10 lanes; frozen QT3/gated qt4 results; API318 types/6,465 rows.
- [x] [Exact-package bounded workload](foundation-package-workload/README.md): six candidate-only decimal sessions. These do not supply accepted baselines or budgets.
- [x] [XmlDb qualification across runs](foundation-xmldb-manual-approval/README.md): retained Storage3,572 and XDM47; Integration862 full-run passes plus one layout-corrected focused pass. Aggregate **4,482 distinct passing cases**. The original failure remains; no single wholly green full run is claimed.
- [x] [Rebuilt1.81.2 / extension0.2.6 cohort](release-1.81.2-candidate/README.md): ten packages, fresh consumer7, retained20, solution26,342 across fixture retry, server90, native451/adapter59/consumer10, exact frozen gates/API and installed editor command/diagnostic checks. No owner/budget/API acceptance is inferred.
- [x] Fresh automatic review allowed the guarded process probes. Earlier denials and the consumed approval remain historical receipts.

## Blockers and closure evidence

These block the currently documented broader foundation release. An explicit scope revision can change applicability; this checklist does not do so.

| ID | Responsible role | Required decision or work | Evidence that closes it |
|---|---|---|---|
| B1 | Release owner, Core/Schema/XmlDb/Braid | Preparation scope selected: bounded Core packages plus VS Code extension. Finalize profiles, exclusions and migration scope. | User direction recorded in the preparation packet; remaining acceptance must explicitly dispose of deferred requirements over exact artifacts. |
| B2 | Core, Schema, XmlDb, Braid | Accept or request revisions to the exact preview cohort for assigned scenarios and source/unchanged-binary lanes. Historical design agreement is insufficient. | Completed [owner returns](foundation-package-workload/OWNER-REVIEW.md), reviewer identity/date, exact hashes, supporting receipts and separately reviewed acceptance. Structural preflight alone is insufficient. |
| B3 | Scenario owners, coordinated by Core | Select representative PF-01?PF-09 workloads, comparable independent baselines, budgets and finite limits; execute remaining accepted workloads. All nine accepted-budget slots remain open. | Approved fixture/configuration/profile and baseline pins; numerical limits with units/statistics/tolerances; raw correctness/performance/lifetime/binding receipts and explicit dispositions. [Draft proposals](foundation-owner-workload-draft/README.md) remain review inputs. |
| B4 | Schema, XmlDb, Braid; Core for contracts | Deliver production provider routes required by B1: Schema placement/profile, XmlDb actual snapshot/operation lifetime adapter, Braid worker/IPC/configuration route. | Reviewed production changes; exact source/binary/end-to-end qualification; deterministic lifetime/refusal evidence and applicable budgets. [Adoption proposals](foundation-provider-adoption/README.md) and compatibility tests do not prove delivery. |
| B5 | All four API reviewers | Complete C45/public API acceptance and freeze for the chosen scope after applicable workload/provider findings. | Dated acceptance over the exact API inventory and supported/refused profile. Resulting semantic/API changes need adoption evidence and qualification before freeze. |
| B6 | Release owner and artifact maintainer | Local target versions/package set selected: Core/server1.81.2, extension0.2.6, ten NuGet packages. [Tracked metadata and workflows are aligned](release-workflow-alignment/README.md); [Final immutable stock-pack/VSIX qualification passes](release-1.81.2-final/README.md); complete hosted preparation and signing/provenance review. | Immutable source/recipe pins, version decision, metadata/assets/dependency checks and signing/provenance disposition. Changed version, source or rebuilt bytes require new hashes and applicable qualification; preview receipts cannot be relabeled. |
| B7 | Release owner | Authorize the concrete final tag and publication after applicable gates close. | Final review links B1?B6 dispositions to exact package hashes, source commit, tag and destination. Evidence commit/push does not grant publication approval. |

Scenario ownership follows the [existing decision matrix](foundation-owner-gap-review/OWNER-DECISIONS.md): Core PF-01 and shared scenarios; Schema PF-02/03 and shared PF-06/08/09; XmlDb PF-04/05 and shared PF-06/08/09; Braid PF-06/07 and shared PF-08/09. Shared scenarios need the relevant owner dispositions.

## Proposed deferrals ? none accepted yet

| Work | Recommendation | Condition |
|---|---|---|
| Additional generic Core APIs | Defer until a consumer demonstrates a missing capability. | Preserve current contracts and explicit supported/refused behavior. |
| Coverage beyond the selected bounded numeric/schema/provider profile | Keep outside release claims. | B1/B5 must explicitly state the profile and dispose of affected existing requirements. |
| Shipping XmlDb/Braid binaries or language-server artifacts in the nine Core packages | Keep outside the current package set. | Retain separate compatibility evidence and licensing boundaries; any expansion needs scope and artifact qualification. |
| Correctly stamped XmlDb delivery artifacts | Defer only if no XmlDb artifact is shipped or represented as owner-delivery accepted. | Diagnostic builds carry parent-Core informational stamps: retain [the identity note](foundation-xmldb-full-qualification/evidence/OWNER-IDENTITY-NOTE.json). Shipping needs correctly stamped artifacts and separate qualification. |
| Production provider adoption | Consider deferral only for an explicitly narrowed Core-only release. | B1 must identify affected requirements and obtain relevant owner dispositions; B2/B3/B5 and performance claims are not automatically waived. |
| A single wholly green XmlDb full run | Review whether split-run coverage satisfies acceptance criteria. | If a single run is required, qualify a complete repository-shaped layout and use fresh logs. Storage need not be rerun merely to repeat Integration. |

## Extension delivery gate

- [ ] Build and inventory a fresh VSIX with the selected language-server/engine cohort; align its independent version and lock; verify packaged hashes, real editor diagnostics/commands, disposable-profile fresh install and upgrade, release notes and exact-artifact publication. [Local candidate](release-core-vscode-candidate/README.md) build, nine-engine hash inventory, server84, protocol3 and fresh/upgrade installation pass. [Live editor-host checks now pass](release-vscode-editor-unblock/README.md) for XPath/XQuery/XSLT diagnostics and correction after a reviewed launch outside Codex restriction, with the normal sandbox and unchanged machine ACL. The [rebuilt1.81.2 VSIX](release-1.81.2-candidate/README.md) and earlier [URI correction](release-vscode-uri-fix/README.md) pass all five registered commands and live diagnostic/correction checks; nine preview engine hashes remain unchanged. Earlier [command failures](release-vscode-command-smoke/README.md) remain historical. Final release acceptance, broader runtime/platform and file-picker/cancellation coverage remain open. These partial checks do not close this gate. NuGet and Marketplace approval/actions remain separate.

## Execution order

1. **Complete the selected B1 scope:** bounded Core packages plus the VS Code extension. Record explicit dispositions for deferred provider requirements and prepare the isolated language-server/VSIX candidate.
2. Collect B2/B3 decisions using the prepared owner returns. Use those inputs to bound B4 work; avoid repeated Storage sweeps or speculative APIs.
3. Complete applicable B4 work and B5 freeze. Qualify changes against their resulting exact cohort and preserve earlier receipts.
4. Review and commit/push the local Integration evidence and checklist as an evidence checkpoint. This can precede final release acceptance; it does not alter the C0 preview identity. Screen retained logs; keep proprietary source/binaries in ignored diagnostics.
5. Prepare B6 artifacts, perform applicable final-cohort checks, and present the exact release for B7 approval. Only then tag and publish.

No publication date is promised while scope, owner decisions and resulting implementation remain unresolved. This is a documentation step; qualification is retained, not a fresh engine test claim. No owner message, external repository edit, commit, push, tag or publication is performed here.

The [exact-artifact review packet](release-1.81.2-review/README.md) pins the final local cohort and lists outstanding owner, budget, API, signing/provenance and hosted preparation decisions. No approval is inferred from its consistency check.

Authorized preparation-only runs are recorded in the [hosted dossier](release-1.81.2-hosted/README.md), separately from the local candidate and its manifest hashes. Publication remains unapproved.

Hosted preparation exposes six Linux QT3 collection fixture failures; NuGet packing is blocked. Extension preparation and exact-byte verification pass. See the hosted dossier for raw receipts and the required file-path/URI fix.

The Linux collection fixture harness correction passes local regression, solution coverage across the Database listener retry, and exact frozen gates. Fresh hosted preparation remains required; see release-linux-collection-fix/README.md.
