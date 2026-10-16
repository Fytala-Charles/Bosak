# Completed Bosak 1.16.5 publication — 2026-10-16

All preparation, explicit approval, tag/workflow, ten public package/signature/consumer checks and GitHub release are complete at`09940e90f702566107bb2eed521b7f8d789a6410`. [Publication verification](RELEASE-1.16.5-PUBLICATION.md). Historical stage descriptions and immutable preparation receipts follow.

# Bosak 1.16.5 publication checklist

Prepared 16 October 2026. This preparation PR integrates the record-subtyping checkpoint and candidate release notes without changing engine or test source. [Release notes](RELEASE-NOTES-1.16.5-CANDIDATE.md); [preliminary validation](RELEASE-1.16.5-CANDIDATE-VALIDATION.md); [preparation receipt](RELEASE-1.16.5-PREPARATION.json).

## Reviewed engineering baseline

- [x] Integrate engine PRs #350 and #351 after exact-head CI success.
- [x] Verify 9,577 solution tests, 84 language-server tests, ten CLI controls and unchanged frozen gates.
- [x] Verify preliminary ten-library archives at `c95b56c0b18e4493c31f2285bbe98621c1027655`, with 153 broad checks and 338 public focused tests.
- [x] Record all 36 remaining selected draft failures: 33 catalog/draft mismatches and three unresolved contracts. Retain experimental status and original receipts.
- [x] Finalize release notes and repair the saved banner markup. Keep canonical documents synchronized.

## Final candidate verification before approval

- [x] Merge this single preparation PR after successful exact-head CI.
- [x] Record its exact merge revision and prove unchanged `src` and `tests` trees against the engineering baseline.
- [x] Repack all ten libraries from that exact merged revision, preserving the preliminary archives separately.
- [x] Run fresh package-only broad and public focused consumers in new caches, with local source mapping for Bosak packages.
- [x] Verify archive/cache/output DLL bytes, restore hashes, package dependencies, XML documentation, branding assets and exact assembly/repository revisions.
- [x] Repeat dependency advisory checks and read-only public tag/package availability checks.
- [x] Present the exact candidate revision, release notes and final verification receipts for explicit publication approval.

These final checks describe the preparation stage represented by this commit. Completion receipts are recorded separately under `.guard-tmp/release1165-final`, including a final approval report identifying the exact merge revision. The final packaging step must not relabel the earlier PR-head archives as merged-revision builds.

## Only after explicit publication approval

- [x] Create and push only the approved immutable `v1.16.5` tag. Its push triggers `release.yml`; do not also dispatch that workflow.
- [x] Verify the release workflow's exact revision, tests and all ten package uploads. Preserve partial-delivery evidence before any retry.
- [x] Create the GitHub release using the approved notes.
- [x] Download and verify public packages through fresh nuget.org-only consumers, then record a separate publication receipt.

No tag, release dispatch, upload, extension publication, installed-extension change, upstream submission or Braid pin change is part of preparation.
