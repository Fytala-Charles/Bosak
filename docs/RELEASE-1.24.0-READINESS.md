<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.24.0 release readiness">
  <h1>Bosak 1.24.0 release readiness</h1>
  <p>23 October 2026</p>
</div>

**1.24.0 published and verified — 23 October2026:** [Public delivery proof](RELEASE-1.24.0-PUBLICATION.md) records immutable source `ff9f5094`, one Release run with11,009 solution tests/ten uploads, ten verified signed packages and fresh nuget.org-only153 broad/503 focused checks with matching provenance and clear audits. Rename and populated-subtree Delete are delivered together. REQ-136 consumer integration/acceptance remains pending; Braid adoption/native acceptance and REQ-126 stay separate. Earlier preparation/unpublished checkpoints below are historical.

**Historical preparation record:** The checklist below predates completed publication.


**Prepared; publication not started.** Proposed annotated tag `v1.24.0` must point to **`ff9f509461ce7f8e639d64d4b83024dcffe32d80`**, the consumer-reviewed REQ-136 merge. Later documentation commits must not change the proposed package source. Public version remains1.23.8. [Release notes](RELEASE-NOTES-1.24.0.md), [machine readiness](RELEASE-1.24.0-READINESS.json).

| Gate | Evidence |
|---|---|
| Consumer implementation review | No blockers at `b535a56b`; exact review retained, 839/0/0 consumer TRX hash/counters independently checked; nine unsigned candidates |
| Merge identity | PR422 merged as `ff9f509461ce7f8e639d64d4b83024dcffe32d80`; full tree identical to reviewed head |
| CI | Reviewed-head [39967993478](https://github.com/Fytala-Charles/Bosak/actions/runs/39967993478) and merge [39971422336](https://github.com/Fytala-Charles/Bosak/actions/runs/39971422336) green; 11,009 solution +84 language-server tests |
| Exact-source build/tests | Release build zero errors/existing CS8604; 11,009 passed, no failures/skips |
| Candidate packages | Ten unsigned 1.24.0 packages; fresh153 broad +503 focused package-only checks pass |
| Provenance | Exact repository revision, assembly versions, aligned dependencies, archive/cache/output SHA-256 and NuGet SHA-512 match; XML docs and README/license/icon/brand assets verified |
| Restore/audits | Fresh separate caches; Bosak.* mapped only to candidate feed; test dependencies from nuget.org; both transitive audits clear |
| Engine gates | Fresh reviewed implementation receipts: QT3 31,142/0/679; qt4 gated8,278/0/1,894; identical merged tree, no later engine changes |
| Availability | Read-only check finds no 1.24.0 remote tag/release/version in any of ten package indexes; repeat immediately before publication |

## Publication sequence

1. Review this concrete preparation with green CI and approve publication at the immutable source above. Repeat live tag/release/all-package availability and source/CI checks; reconcile partial artifacts if any appear.
2. Push one annotated `v1.24.0` at that source. Use the existing tag-triggered Release workflow to build/test/pack and publish ten packages through NuGet OIDC. Do not also dispatch manually.
3. Retain the workflow revision, tests and ten upload acknowledgments; create the GitHub release at the verified tag.
4. Download all signed public packages; verify signatures, source/version/dependencies/assets and assembly identities. Run fresh nuget.org-only153 broad/503 focused checks, verify archive/cache/output/contentHash provenance and transitive audits. Preserve propagation failures without republishing.
5. Record verified publication and update public installation links. Braid then verifies the signed release and integrates Rename/Delete together in one PR, preserving Create and all session/history/worker/export/native gates.

Candidates: `.guard-tmp/release1240-packages`; exact source: `.guard-tmp/release1240-source`; evidence: `.guard-tmp/release1240-preparation`. No tag push, release dispatch, NuGet upload or Braid edit is performed by this preparation. Local main is synced to the implementation merge; original notes and registration snapshots remain in retained stash `6a43667ed06739e1bbd1c737c1cd717f1bf33e0f`. The duplicate request registration is absorbed; pending upstream notes, sweep baselines and protected PR132 remain preserved. README install/build/quick-start remains current for public1.23.8.
