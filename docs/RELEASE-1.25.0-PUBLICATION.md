<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.0 verified publication">
  <h1>Bosak 1.25.0 publication</h1>
  <p>24 October 2026</p>
</div>

**Published and verified.** [Bosak v1.25.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.25.0) uses immutable source `c005054bb096559368df985359e038e254814d12` and annotated tag object `8a128ae5beb8c7112fdb49a48423495dab9f14a2`. PR431 merged the exact consumer-reviewed tree; PR430's design commits were included and it is merged too. The owner's explicit merge/release instruction authorizes this publication.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/40071431156) | 11,244 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed 1.25.0 packages; exact source/version/dependencies and approved Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; all ten assembly identities verified |
| Fresh nuget.org-only focused consumer | 614/0/0, including 71 reorder and 40 substring/host checks; nine assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw signed archive SHA-512 and NuGet contentHash match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerable packages |

The [original Braid implementation review](REQ-137-BRAID-CONSUMER-IMPLEMENTATION-REVIEW.md) is byte-exact, clearing the bounded profile and independently reporting 235 literal-element /8,135 full XSLT checks (overlapping counts). Relative links inside the preserved review belong to the original Braid documentation context. The original request and reviewed design remain unchanged. Maintainer source checks and reviewed-head CI pass 11,244 solution +84 language-server tests. Fresh manual engine receipts remain QT3 **31,142/0/679** and frozen qt4 **8,434/0/1,896** across224 configured/executed sets. Release reruns the solution; it does not rerun those manual harnesses. Native gate skips and the 11 existing Experimental fallback passes remain explicit. No fixture/gate/kit or upstream adoption change accompanies this release.

The coordinated reorder API, exact supported profile, proof and limits are described in the [implementation guide](REQ-137-IMPLEMENTATION.md). [Release notes](RELEASE-NOTES-1.25.0.md) also identify the Experimental-only substring correction. Public consumer tests use isolated caches and nuget.org-only mapping without engine ProjectReferences or friend access; five negative reorder cases inspect the internal proof via reflection, distinct from ordinary public API calls. Counts overlap source coverage.

REQ-137 is implemented and published; Braid signed-package adoption, coordinated integration, atomic Apply/history/redo/selection, worker/export/hash/reopen and native acceptance remain pending. REQ-136's accepted 1.24.0 lifecycle and REQ-126's partial status retain their separate evidence. No Braid runtime/dependency or VS Code extension is changed here.

All preflight, one workflow's logs, propagation attempts, signature logs, public TRXs and archive/cache/source/hash proofs are retained under `.guard-tmp/release1250-publication`. [Machine-readable proof](RELEASE-1.25.0-PUBLICATION.json). No retagging or duplicate dispatch occurred. Pending local upstream notes, retained stashes/worktrees/caches, sweep baselines and protected PR132 are preserved.

The first broad public verifier still asserted the previous release version/revision and correctly stopped; only those local verifier expectations were corrected. The original failed attempt and restored packages remain retained. A new isolated attempt passed all checks; no engine or published archive was changed.
