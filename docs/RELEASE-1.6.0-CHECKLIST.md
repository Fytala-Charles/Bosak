# Bosak 1.6.0 publication checklist

Prepared 2026-10-15. Candidate engine revision: `6dcc9671a7b28cd1b55ec80650e54249f3e44339`. [Release notes](RELEASE-NOTES-1.6.0.md) and [preparation evidence](RELEASE-1.6.0-PREPARATION.json) define the scope. The user approved final revision `3715b2f98bbf80883358d31234224cbb9cd94cad`; publication and public-feed verification are complete. [Publication receipt](RELEASE-1.6.0-PUBLICATION.json) records results. Candidate checks below are historical; final checks apply to the approved revision.

## Completed preparation

- [x] Merge reviewed optional overrides with successful exact-head and post-merge CI.
- [x] Verify 8,372 solution tests, 84 language-server tests and extension checks.
- [x] Run both frozen gates on the merge revision: 31,142/0/679 and 8,278/0/1,894.
- [x] Verify all ten local package archives and fresh-cache provenance; run 74 package-only tests and verify nine engine dependency DLLs.
- [x] Document scope, compatibility behavior, remaining boundaries and published versions.

## Before approval

- [x] Repeat the 153-check broad release consumer against 1.6.0, including all ten libraries; verify exact DLL bytes and revision metadata. New cache and receipts preserve 1.5.0 evidence; the 33 used-package checks use the current fixture.
- [x] Review and integrate these preparation documents after successful exact-head CI.
- [x] Record the final merged revision and compare its source/test trees with this candidate. Repack from that revision, including its packaged README; verify all ten archive contents, dependency versions and repository metadata.
- [x] Repeat both package consumers in fresh caches against the final archives; verify all ten assembly versions and exact informational revision metadata.
- [x] Check that the intended `v1.6.0` tag is unused and that a duplicate 1.6.0 publication will not occur. Present the final revision and evidence for user approval.

## After explicit publication approval

- [x] Tag only the approved revision and push `v1.6.0`. The numeric tag triggers `.github/workflows/release.yml`; do not also dispatch a duplicate workflow.
- [x] Verify the workflow's exact revision, full test success and ten NuGet uploads. Preserve the workflow receipt; investigate partial publication before retrying.
- [x] Publish the GitHub release with the reviewed notes. Record the immutable tag target.
- [x] Wait for NuGet indexing, download all ten public archives and verify version, dependencies, repository revision, assets, metadata and hashes. Public signed archive hashes may differ from local unsigned archives; record them separately.
- [x] Run the broad and feature consumers with nuget.org as their only source in fresh caches. Verify restored archive provenance and output assembly metadata; retain restore hashes and signed archive SHA-512 evidence.
- [x] Add a separate publication receipt and update release notes and canonical status documents. Preserve preparation receipts, historical logs and `.sweep-baselines/`.

The VS Code extension has its own release process; this checklist covers the ten NuGet libraries.
