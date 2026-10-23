<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.7 release readiness">
  <h1>Bosak 1.23.7 release readiness</h1>
  <p>23 October 2026</p>
</div>

**1.23.7 published and verified - 23 October 2026:** [Public delivery proof](RELEASE-1.23.7-PUBLICATION.md) records immutable v1.23.7 at `9dd50961`, one successful Release run (10,720 solution tests; ten uploads), ten verified signed packages and fresh nuget.org-only153 broad/394 focused checks with matching provenance and clear audits. Includes EQName bracket parsing, inline-record old-profile refusal and numeric separator boundaries. REQ-126 remains In Progress. Braid dependency1.23.4 and existing acceptance remain unchanged; a1.23.7 upgrade smoke is separate. Earlier preparation/unpublished statements below are historical.

**Historical preparation record:** The checklist below predates completed publication.


**Prepared; publication not started.** Proposed annotated tag `v1.23.7` must point to **`9dd5096104b9d08bfad2dd88130f04ed0c3807f8`**, the immutable implementation merge. Later documentation commits must not move this package source. Public/Braid dependency remains 1.23.4. [Release notes](RELEASE-NOTES-1.23.7.md), [machine readiness](RELEASE-1.23.7-READINESS.json).

| Gate | Evidence |
|---|---|
| Reviewed implementation | PR412 head0556278d and implementation merge9dd50961 have identical complete trees; includes PR409/411 and documentation PR410 |
| Exact-head CI | [39896793845](https://github.com/Fytala-Charles/Bosak/actions/runs/39896793845), all three checks pass: 10,720 solution +84 language-server tests |
| Exact-merge CI | [39897457866](https://github.com/Fytala-Charles/Bosak/actions/runs/39897457866), completed successfully at9dd50961 |
| Exact-merge Release build | Zero errors; existing CS8604 conformance-runner warning; ten 1.23.7 libraries packed |
| Exact-merge local tests |10,720 passed, no failures/skips, across ten projects with required schema fixtures |
| Package-only consumers |153 broad +394 focused pass; no engine ProjectReferences or friend-assembly identity |
| Focused composition |117 attribute lifecycle +32 switch +27 index-of +96 priority +44 EQName brackets +44 record gates +34 numeric boundaries; overlaps source tests |
| Provenance | Ten package versions/source revisions, aligned Bosak dependencies, assembly versions/informational revisions and archive/cache/output hashes match; XML docs and exact README/license/icon/branding present |
| Fresh restore and audits | Separate caches; Bosak.* mapped exclusively to local candidate feed; external test dependencies from nuget.org. Both transitive audits report no vulnerable packages; no unavailable-audit warning |
| Engine gates | Latest corrected implementation receipts: QT3 31,142/0/679; qt4 gated8,278/0/1,894. Gates retained from numeric correction, not rerun during release preparation; no intervening engine change |
| Read-only availability preflight |1.23.5/1.23.6/1.23.7 tags and GitHub releases absent; all three versions absent from all ten public package indexes at preflight |

Existing successful1.23.4 consumer sources were copied into fresh directories and version/revision guards updated before execution. The candidate consumer adds the new bounded regression classes and retains the older package checks. Original sources, failures, caches and receipts remain separate. No library change was needed during release preparation.

These are **unsigned local candidates**. No tag, manual Release dispatch, NuGet upload or GitHub release has been created. Public signature/provenance and fresh nuget.org-only consumers remain post-publication gates. Completed Braid acceptance, upstream contributions and protected PR132 are separate and unchanged.

## Publication sequence

1. Review and merge the preparation PR with green CI, then approve publication of this exact source. Immediately before publishing, recheck version/source, CI, tag/release and all-ten-package availability. If partial artifacts exist, reconcile them without retagging or duplicate publication.
2. Create one annotated `v1.23.7` at `9dd5096104b9d08bfad2dd88130f04ed0c3807f8` and push it once. The existing tag-triggered Release workflow restores/builds/tests/packs and publishes ten libraries through NuGet OIDC. Do not also dispatch manually. No separate1.23.5/1.23.6 release is planned.
3. Retain the single workflow's revision, test results and ten upload acknowledgments. Create the GitHub release at the verified tag using these prepared notes.
4. Download every public package, verify NuGet signatures and exact version/source/dependencies/assets, and compare immutable source and library output identities. Signed archives differ from unsigned candidates; compare the appropriate content separately.
5. Run fresh nuget.org-only153 broad/394 focused package consumers; verify signed archive/cache/output/contentHash/restore-source metadata and transitive dependency audits. Preserve failed/partial attempts separately; account for NuGet propagation without republishing.
6. Record verified publication and update public install/status links. Recommend a normal Braid upgrade smoke check separately from its completed native acceptance; no Braid files/pins change in this preparation.

Candidates: `.guard-tmp/release1237-packages`; source: `.guard-tmp/release1237-source` at the immutable revision; detailed logs/TRXs/metadata/audits/consumer sources: `.guard-tmp/release1237-preparation`. Root upstream notes, sweep baselines, stashes and worktrees are preserved. Root README install/build commands remain current for public1.23.4; no CLI/structural change requires an overview rewrite.
