<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.8 release readiness">
  <h1>Bosak 1.23.8 release readiness</h1>
  <p>23 October 2026</p>
</div>

**Prepared; publication not started.** Proposed annotated tag `v1.23.8` must point to **`6d12bed35dac56a635acc7702645ef9085103c83`**, the PR417 implementation/coverage merge. Later preparation documentation must not change this package source. Public version remains 1.23.7. [Release notes](RELEASE-NOTES-1.23.8.md), [machine readiness](RELEASE-1.23.8-READINESS.json).

| Gate | Evidence |
|---|---|
| Source | PR416 fix merged, PR417 refreshed onto main; complete source tree equals reviewed PR417 head `2d3fbec9282745e1e2faefa2bc3b32103d4f9455` |
| Exact-head CI | [39929015269](https://github.com/Fytala-Charles/Bosak/actions/runs/39929015269), all three checks pass; 10,936 solution +84 language-server tests |
| Exact-merge CI | [39929985143](https://github.com/Fytala-Charles/Bosak/actions/runs/39929985143), all three checks pass; 10,936 solution +84 language-server tests |
| Exact-source build/tests | Release build zero errors; existing CS8604 warning; 10,936 passed, no failures/skips |
| Candidate packages | Ten 1.23.8 unsigned packages;153 broad +430 focused package-only checks pass |
| Provenance | Exact repository revision, aligned dependencies, assembly version/informational revision, archive/cache/output SHA256 and NuGet SHA512 match; XML docs and source README/license/icon/branding verified |
| Restore/audits | Separate fresh caches; Bosak.* mapped only to candidate feed; external test dependencies from nuget.org; both transitive audits clear |
| Engine gates | PR416 fresh QT3 31,142/0/679 and qt4 gated8,278/0/1,894; no later engine change; retained, not rerun during preparation |
| Availability | Read-only check: no 1.23.8 tag/release or version in any of ten package indexes; repeat immediately before publication |

Fresh consumers copy the prior 1.23.7 verification sources, update version/revision guards and add the 36 default-mode controls. The initial broad run caught ten stale numeric assembly-version guards copied from 1.23.7. Those verifier guards were corrected and the broad run repeated with fresh source/output/cache; no engine/package correction was needed. Original consumers, failures, receipts, caches, stashes and worktrees are preserved. These are unsigned local candidates; no signed public verification is claimed.

## Publication sequence

1. Review/merge this preparation with green CI and approve publication of the immutable source above. Repeat live version/source/CI and tag/release/all-package availability checks. Reconcile partial artifacts if any appear.
2. Push one annotated `v1.23.8` at that immutable source. The existing tag-triggered Release workflow builds/tests/packs and publishes ten packages via NuGet OIDC. Do not also dispatch it manually.
3. Retain the workflow revision, tests and ten upload acknowledgments; create the GitHub release at the verified tag with these notes.
4. Download all signed public packages; verify signatures, source/version/dependencies/assets and assembly identities. Run fresh nuget.org-only 153 broad/430 focused checks and verify archive/cache/output/contentHash provenance plus transitive audits. Preserve failed propagation attempts without republishing.
5. Record verified publication and update public install links. Treat Braid upgrade smoke separately from native acceptance; no Braid files/pins change here.

Candidates: `.guard-tmp/release1238-packages`; immutable checkout: `.guard-tmp/release1238-source`; evidence: `.guard-tmp/release1238-preparation`. Root upstream notes, sweep baselines and protected PR132 remain preserved. README install/build/quick-start remains current for public1.23.7.
