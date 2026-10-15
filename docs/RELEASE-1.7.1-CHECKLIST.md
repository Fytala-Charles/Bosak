# Bosak 1.7.1 publication checklist

Prepared 2026-10-15. Engine merge `0c0e2a3a8aab42da6ab536dca51e5e8442824f8e`. [Release notes](RELEASE-NOTES-1.7.1.md); [preparation receipt](RELEASE-1.7.1-PREPARATION.json). No publication or tag creation is authorized by preparation.

## Completed preparation

- [x] Review PR #305, reconcile current main and merge after successful exact-head CI.
- [x] Verify unchanged reviewed source/test trees, full CI suite and both frozen gates.
- [x] Verify ten full-revision 1.7.1 archives, 153 broad checks and 185 focused tests in fresh mapped caches.
- [x] Verify exact archive/cache/output DLL bytes, dependency versions, assembly metadata and restore hashes.
- [x] Verify isolated Braid compatibility: 843 passing tests; original checkout/pin unchanged.
- [x] Check that v1.7.1 and all ten public 1.7.1 package versions are unused at initial preparation.

## Before publication approval

- [ ] Integrate preparation documents after successful exact-head CI.
- [ ] Repack the final merged documentation revision and repeat broad/focused consumers with fresh caches; verify exact revision, README, assets and DLL provenance.
- [ ] Recheck unused tag/public versions and present the concrete revision for explicit publication approval.

## After explicit publication approval

- [ ] Create/push only the approved immutable v1.7.1 tag; its push triggers release.yml. Do not also dispatch the workflow.
- [ ] Verify exact workflow revision, complete tests and ten package uploads; preserve any partial-delivery evidence before retrying.
- [ ] Create the GitHub release using reviewed notes.
- [ ] Verify all ten public packages, signed archive hashes and fresh nuget.org-only broad/focused consumers with exact output metadata.
- [ ] Record a separate publication receipt and synchronize canonical documentation; preserve original receipts.

VS Code alignment, Braid production adoption, REQ-128 acceptance and upstream submissions remain separate. Preserve concurrent work, .sweep-baselines/ and immutable existing release tags.
