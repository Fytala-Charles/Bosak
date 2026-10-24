<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.1 verified publication">
  <h1>Bosak 1.25.1 publication</h1>
  <p>24 October 2026</p>
</div>

**Published and verified.** [Bosak v1.25.1](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.25.1) uses immutable source `6081e70daa8a49ef3d522b124a18d8de68e0b4d5` (PR442) and annotated tag object `8ac666ece27b7290cef7dc860b222fd7912f1982`. The owner explicitly authorized tagging and publication after the [consolidated candidate validation](RELEASE-1.25.1-CANDIDATE.md).

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/40129073797) | 11,675 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed 1.25.1 packages; exact source/version/dependencies and approved Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; ten assembly identities |
| Fresh nuget.org-only focused consumer | 1,045/0/0, including 431 added numeric/Unix regressions; nine assembly identities |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw signed SHA-512 and NuGet contentHash match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerable packages |

The 431 added package checks cover signed Unix-time and host boundaries (81), wide Unix epochs (56), integer promotion (42), signed integer casts (72), floating integer boundaries (72), unsigned decimal casts (48) and unsigned floating casts (60). All 614 retained focused checks remain green, including 71 reorder, 40 substring and 73 literal-element lifecycle cases. Consumer counts overlap source coverage.

Candidate validation also passes 84 language-server tests. Fresh source manual gates are QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894** across 224 unchanged configured/executed gates. Two former compile-overflow skips now reach accepted runtime errors. The 11 existing Experimental fallback passes and native gate skips remain explicit. Current native Unix tests remain **22/2/0, exit2** under the frozen runner. Release reruns the solution; it does not rerun the manual harnesses. No gate/fixture adoption or full 4.0 conformance is claimed.

[Release notes](RELEASE-NOTES-1.25.1.md) explain the bounded numeric corrections and unchanged limits. No new public API or layer is added. The XSLT authoring source is byte-unchanged from 1.25.0; Braid's 1.25.0 acceptance does not establish 1.25.1 smoke/adoption acceptance. REQ-126 remains partial, and REQ-138 is outside this release.

Original workflow logs, propagation/signature attempts, public TRXs and archive/cache/source/hash receipts are retained under `.guard-tmp/release1251-publication`. [Machine-readable proof](RELEASE-1.25.1-PUBLICATION.json). The immutable source/tag are unchanged by this documentation follow-up. No retag or duplicate dispatch occurred. Pending REQ-138/upstream notes, retained stashes/worktrees/caches, sweep baselines and protected PR132 are preserved.
