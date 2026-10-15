# Bosak 1.7.0 publication checklist

Prepared 2026-10-15. Candidate engine revision `7978e5d7cc0a3092f466bc4ac70d5deacb339764`. [Release notes](RELEASE-NOTES-1.7.0.md) and [preparation evidence](RELEASE-1.7.0-PREPARATION.json). Publication has not been approved or performed.

## Completed preparation

- [x] Review and merge PR #301; exact-head and post-merge CI succeed.
- [x] Verify 8,465 solution tests, 84 language-server tests and frozen gates 31,142/0/679 and 8,278/0/1,894 on the merge revision.
- [x] Verify all ten 1.7.0 package archives and exact merge-revision metadata.
- [x] Run 153 broad checks and 167 focused tests in fresh mapped caches; verify archive/cache/output bytes, SHA-512, restore content hashes and assembly metadata separately.
- [x] Verify the isolated current Braid snapshot: 843/0/0; original source, status and pin unchanged.
- [x] Confirm `v1.7.0` is unused; no tag is created.

## Before publication approval

- [ ] Integrate preparation documents after successful exact-head CI.
- [ ] Record the final merged revision; confirm its source/test trees match this candidate.
- [ ] Repack that revision, including its committed README; repeat both consumers in fresh caches and verify ten archive/dependency/assembly revisions and exact output DLL bytes.
- [ ] Recheck tag and public feed availability to prevent duplicate publication; present the final revision and evidence for explicit approval.

## After explicit publication approval

- [ ] Tag only the approved revision with `v1.7.0`; push once. The tag triggers `.github/workflows/release.yml`; do not also dispatch it.
- [ ] Verify exact workflow revision, complete test success and ten NuGet uploads; preserve partial-publication evidence before any retry.
- [ ] Publish the GitHub release with reviewed notes and preserve the immutable tag target.
- [ ] Download all ten public packages after indexing; verify metadata, assets, dependencies, hashes and revision. Record signed archive hashes separately from unsigned local candidates.
- [ ] Repeat broad/focused consumers using fresh nuget.org-only caches; verify restore provenance and exact public archive/output DLL bytes and assembly metadata.
- [ ] Record a separate publication receipt and synchronize canonical documentation, preserving preparation and historical evidence.

VS Code publication and Braid production adoption are separate workflows. Preserve `.sweep-baselines/`, concurrent REQ-128 edits, original Braid work and all historical receipts.
