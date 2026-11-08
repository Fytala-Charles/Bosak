# Hosted preparation started - 8 November 2026

User authorized evidence commit/push and both preparation-only workflows. Evidence checkpoint `e2893d1aa36c2a68545a2d0eed34f103ab65632d` is pushed. [Hosted dossier](release-1.81.2-hosted/README.md) records NuGet run42477093485 and extension run42477095428; both publish jobs are skipped. Extension preparation and exact-byte verification pass with90 server tests. NuGet remains pending at this entry; no success is inferred. Owner/API/budget/signing and publication decisions remain open. The prior review packet remains a historical unfilled decision packet.

---

# Exact-artifact decision packet - 8 November 2026

[Current release review](release-1.81.2-review/README.md) pins the final local NuGet, VSIX, API and qualification inputs without transferring older owner approvals. All owner/budget/API/signing/hosted/publication decisions remain unfilled. Inspection found no NuGet signature entry in any of the ten local packages; this is not certificate or service trust validation. The packet consistency check passes. Next technical check is explicitly authorized push/hosted preparation only; no workflow was dispatched. Source remains 1d69d059; this packet and final evidence are uncommitted.

---

# Final local release qualification - 8 November 2026

Current source checkpoint is `1d69d059c14888c1c43264efa8668909292af858` on `checkpoint/foundation-1.81.1`, following alignment commit `f0f3e9c892fb9afb6b6b6417e4d707ebf338d599`. Both commits remain local; the last pushed checkpoint is e992d90b. [Final qualification](release-1.81.2-final/README.md) seals ten stock NuGet packages at 1.81.2 and extension 0.2.6 from this immutable source. No metadata override or NOTICE overlay was used. Seventy generated server files were removed from tracking; all local files were preserved.

The final cohort passes 26,342 solution tests in one invocation, 90 language-server tests, consumer7, retained20, native451/adapter59/consumer10, unchanged frozen gates and API inventory, five installed editor commands and live diagnostic/correction checks. Both artifact manifests and payloads verify. The earlier f0 stock packet remains historical and contains no VSIX. Final evidence and this documentation update are local and uncommitted.

Next: review exact-artifact owner workload/budget and C45/API dispositions, signing/provenance, and qualify the hosted preparation workflows under the [release guide](RELEASING.md). Hosted workflows, publisher authentication, push, tags and publication were not performed. Publication remains unauthorized. Earlier entries below retain their original identities.

---

# Qualified foundation checkpoint — 7 November 2026

The owner authorized committing and pushing the cumulative qualified work on
`checkpoint/foundation-1.81.1`. This checkpoint preserves the existing ancestry;
it does not authorize a release tag or package publication.

The [production qualification](foundation-captured-identity-production/README.md)
records development version 1.81.1, 26,342 passing solution tests, 84 separate
language-server tests, and the native, adapter, consumer and retained-caller
lanes. Frozen QT3 and gated qt4 results remain unchanged. Its verifier passed
again before staging this checkpoint. Historical handover entries describe their
original uncommitted state; this checkpoint records the subsequent delivery step.

Two sealed execution-layout archives use Git LFS because each exceeds GitHub's
ordinary file-size limit. Install Git LFS and run `git lfs pull` to retrieve their
full evidence bytes after checkout. Source builds do not require these archives.
Exact-path attributes preserve the qualified source and documentation bytes.
Generated Python bytecode is excluded.

Focused screening found no strong credential patterns in reviewed text, no
sensitive archive paths, and no missing source headers. This was a targeted
accidental-disclosure check, not a complete security audit.

Next: PV-Q05 review of exact-cohort owner workloads, comparable baselines,
performance budgets and public-provider adoption gaps. C45, freeze and release
acceptance remain open. Prepare and inspect package artifacts after those gates;
tag and publish only after release approval.

The subsequent [PV-Q05 gap review](foundation-owner-gap-review/README.md) records
the unchanged owner source inventory, all nine unfilled performance scenarios
and concrete owner decisions. It advances preparation to PV-Q06 without changing
the sealed qualification or granting release acceptance.

[PV-Q06](foundation-workload-1811/README.md) now records the separate exact-1.81.1
workload cohort: six provider sessions, 30 measured cycles, 135 checked reads,
14 method controls and a candidate-as-baseline refusal. Loaded binary identities
match the qualified cohort. This is candidate evidence, not accepted budgets or
production-provider adoption; next is PV-Q07 owner/adoption review.

[PV-Q07](foundation-provider-adoption/README.md) prepares the production-provider
adoption proposal and four unfilled owner returns over the exact 1.81.1 cohort.
The Schema production project still references 1.26.3; no dependency upgrade or
external source change was performed. Structural screening refuses unfilled
returns; 18 checker controls pass. Next is local preview/package-only consumer
preparation (PV-Q08), with R4/R5 acceptance and publication still pending.

[PV-Q08](foundation-preview-packages/README.md) now preserves nine local
`1.81.1-preview.c0b926de` packages built from the immutable checkpoint and checked
against qualified source/asset hashes. Fresh private-cache package-only consumers
pass six behavior controls; the unchanged retained caller passes 20/0. Package,
cache, runtime output and loaded DLL hashes agree. Next is PV-Q09 full qualification
of these distinct package binaries; owner acceptance/freeze/publication stay open.

[PV-Q09](foundation-preview-qualification/README.md) qualifies the exact preview
package overlay against the 26,342 solution cases, 84 separate language-server,
451 native, 59 adapter and 10 consumer cases, frozen conformance gates/skip maps
and unchanged API inventory. Initial fixture-path and metadata-dependency setup
failures remain separate from corrected passing runs. Next is PV-Q10 bounded
workload measurement and owner review over these package hashes; no owner budget,
production adoption, freeze or publication acceptance is inferred.

[PV-Q10](foundation-package-workload/README.md) now records the bounded workload
over the exact preview packages: fresh private-cache restore, six sessions,
30 measured cycles/135 reads, 14 method controls and baseline refusal. The
[owner review](foundation-package-workload/OWNER-REVIEW.md) supplies concrete
artifact/qualification references and refreshed return templates; all nine PF
workload/baseline/budget slots and production adoption decisions remain open.
Next is explicit owner review and the implementation/qualification it requires;
repeated candidate measurements cannot supply owner approval.

The subsequent [live owner repository review](foundation-owner-repo-review/README.md)
resolves the actual public Schema, Bosal.XmlDb and Bosak.Braid directories supplied
by the owner. Existing design/current-pin replies explicitly leave new-candidate
budgets/adoption/C45 open; no exact-preview acceptance was found in the reviewed
current documentation. Original repository edits remain untouched.

[Draft workload proposals](foundation-owner-workload-draft/README.md) now map all
nine PF scenarios to actual owner constraints. Two measured decimal fixtures have
explicitly provisional diagnostic guardrails; accepted budgets, baselines and
approvals remain unfilled. Next independent preparation is the live XmlDb
ordinary snapshot/operation route and an isolated exact-package qualification recipe.

[XmlDb package readiness](foundation-xmldb-package-readiness/README.md) pins the
clean live owner source and runs the same existing XDM test binary against baseline
and preview engines: 47/0 in each lane, exact retained case names and matching
loaded hashes. This is binary compatibility only. A six-step source/adapter/native/
worker qualification recipe retains the actual internal membership/raw-lifetime
gap; owner product pins and all storage bytes remain unchanged.

[XmlDb isolated source candidate](foundation-xmldb-source-candidate/README.md)
builds against the preview dependencies with zero warnings/errors and no source
migration edit. Rebuilt XDM47 and focused native cleanup139 pass. Full Storage
was cancelled after discovering its hour-long retained timing; Integration863
was not launched because automatic approval review rejected the kill/fault-probe
method without explicit authorization. Full source qualification remains incomplete;
the native staging-rename failure and all partial/cancelled receipts are preserved.

[The authorized full-run follow-up](foundation-xmldb-full-qualification/README.md)
records the owner's explicit approval for Storage and Integration process probes.
The full Storage sweep passes 3,572/0 in the pinned isolated source candidate. Safe
Integration selections pass 39 raw-work/lifetime plus 30 controlled-pilot cases;
the full Integration kill/fault launch was again rejected by automatic review
despite user approval and was not launched or bypassed. Final Storage results
and the remaining Integration restriction are tracked in that separate packet.
Together with the prior XDM47, 3,688 distinct XmlDb cases are verified; 794
Integration cases remain unqualified. The diagnostic owner-assembly build-context
labels and 13 private Debug/Release attribute differences are explicitly recorded;
owner artifact provenance needs a separate correctly stamped build before delivery.

The owner subsequently authorized a commit/push of this accumulated evidence.
[Checkpoint review and restriction handoff](QUALIFICATION-CHECKPOINT-REVIEW.md)
records the concrete completed work, screening scope, execution restriction and
remaining delivery gates. Packages retain their original C0 candidate identity;
this evidence checkpoint does not select a release version, tag or publication.

Manual approval follow-up: the exact Integration retry was allowed but stopped before discovery due to a launcher DLL filename error. The filename is corrected; the single-use approval remains consumed. See [the retained follow-up](foundation-xmldb-manual-approval/README.md). Qualification totals are unchanged.

8 November 2026: fresh automatic review allowed the corrected guarded Integration run. Full run: 862 passes / one layout-dependent reopen failure; focused rerun with unchanged payloads in a repository-shaped diagnostic layout: one pass. All 863 distinct Integration cases have passed across these two runs; aggregate with retained Storage3572 and XDM47 is 4,482 distinct passes. The original failure remains retained and this is not a single wholly green full run. See [the follow-up results](foundation-xmldb-manual-approval/README.md). Approval restriction is resolved; owner/release acceptance remains pending.

Release planning, 8 November 2026: [the release checklist](RELEASE-READINESS-1.81.1.md) separates completed qualification, seven remaining decision/delivery gates, proposed conditional deferrals and execution order. All owner acceptance fields remain unchanged. First decision: broader production-provider delivery versus an explicitly bounded Core-only release.

8 November 2026: user selected bounded Core packages plus VS Code extension preparation. [The preparation packet](release-core-vscode/README.md) pins existing extension0.2.5 inputs and records successful compile/highlighting/mocked launch checks. The ignored server cohort does not match the preview; fresh VSIX and real editor/install/upgrade qualification remain. Provider delivery is deferred from scope; owner requirements/acceptance are not waived. No version bump, tag or publication.

8 November 2026: [isolated extension0.2.6 candidate](release-core-vscode-candidate/README.md) locally built against exact1.81.1-preview.c0b926de engine bytes. Final VSIX includes explicit Unicode notice; server84, protocol3 and disposable-profile fresh/0.2.5 upgrade checks pass. Installed content verifies except installer-added manifest metadata. Real editor host failed on VS Code installation ACL after child Electron CLI environment correction; no system setting, active profile, tracked version, tag or publication changed. Live editor/command and final owner/release acceptance remain open.

8 November 2026: [editor execution blocker resolved](release-vscode-editor-unblock/README.md). A reviewed launch outside Codex restricted process context passed five live XPath/XQuery/XSLT diagnostic/correction checks on the installed final0.2.6 candidate; host exit0 and installed payload verified. Chromium sandbox remained enabled; installation ACL and active profile unchanged. Earlier failure remains historical. Representative commands, platform/runtime coverage and owner/final release acceptance remain open.

8 November 2026: [registered extension command smoke tests](release-vscode-command-smoke/README.md) complete: XSLT transform/initial-template pass; XPath evaluation and both XQuery commands fail. Deterministic protocol reproduction identifies raw escaped Windows URI lookup versus typed DocumentUri synchronization. All original attempts preserved. No production implementation changed; fix plus new-cohort qualification required before extension release.

8 November 2026: [URI command fix qualified](release-vscode-uri-fix/README.md). ExecuteCommandHandler now parses command arguments through DocumentUri; six new regressions bring language-server tests to90. Source and packaged90 lanes, five actual commands and five live correction checks pass; separate fixed-server VSIX retains exact nine preview engines. Solution build0 errors/eight existing warnings;26,342 distinct passing cases across main run and local-listener database retry47, with initial45 failures retained. Core version and tracked extension version remain unchanged; diagnostic server1.81.2-preview.urifix1 is a hash-pinned modified snapshot, not an approved release. No tag/publication.

8 November 2026: [reviewed checkpoint](RELEASE-CHECKPOINT-REVIEW-2026-11-08.md) prepares the URI fix and accumulated XmlDb/editor evidence for user-authorized commit/push on the checkpoint branch. Six evidence verifiers pass; targeted disclosure/size/archive screening passes. The evidence commit does not relabel immutable C0 preview or modified diagnostic server artifacts; final acceptance/tag/publication remain open.

8 November 2026: checkpoint e992d90b is pushed and remote-verified. [Final candidate decision packet](release-final-candidate-plan/README.md) proposes Core/server1.81.2 and extension0.2.6 after public registry observations. The existing REST-loader Database NuGet shipping contract was identified outside the nine-package preview; inclusion as a tenth package needs explicit selection and qualification. Version/package questions are pending. Owner budgets/API acceptance, workflow recipe corrections and final publication remain open; no version or workflow changed.

8 November 2026: user selected Core/server1.81.2, extension0.2.6 and existing Database library as tenth NuGet package. [New local cohort](release-1.81.2-candidate/README.md) built from archived e992d90b with explicit metadata overrides; consumer7/retained20, solution26,342 across fixture retry, server90, native451/adapter59/consumer10, exact frozen gates/API and installed command/live correction checks pass. Initial restricted-build and missing-fixture failures preserved. Tracked version/workflow files and owner/API acceptance remain unchanged; no tag/publication.

8 November 2026: [tracked release workflow alignment](release-workflow-alignment/README.md) pins Core/server1.81.2 and extension0.2.6, adds stock NOTICE and ten-project configuration, and separates preparation from exact reviewed prior-run publication.12 artifact/gate tests, ten source-run cases, YAML checks, real prior-artifact and metadata checks pass. Source alignment is to be committed before a new stock-pack qualification cohort. No engine body, tag, dispatch, secret state or publication changed.
