<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.0 verified publication">
  <h1>Bosak 1.26.0 publication</h1>
  <p>25 October 2026</p>
</div>

**REQ-138 accepted by Braid - 25 October 2026:** [Formal acceptance](REQ-138-BRAID-ACCEPTANCE.md) pins pushed checkpoint `29acaac761206d6b316e8844930ce5d4e8043835`, green checkpoint CI, 1,518 Release /1,784 isolated public-package passes and the separate owner "Passed - accepted" native receipt. [Maintainer verification](req138-evidence/acceptance/verification.json) rechecks all seven TRX hashes/counters and correlates nine consumed archive identities with independently verified Bosak signatures. Acceptance covers the delivered conditional lifecycle. Optional/unreported native variants remain unspecified; the host is not represented as a clean-checkpoint build. Earlier consumer-pending notices are historical. The original publication-time pending statements below and machine-readable publication receipt are retained historical evidence.


**Published and independently verified.** [Bosak v1.26.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.26.0) uses approved immutable source `1f06efb4d938d3d8e12eb273e632a8fe15110041` and annotated tag object `3a33c705d54b7783bbbe8117acbe69d615d71b6c`. The candidate documentation merge is separate from package source. The owner authorized merging PR453, tagging this exact source and publishing before these actions.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/40185109290) | 12,116 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed 1.26.0 packages; exact source, versions, internal dependency pins, XML documentation and Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; ten assembly identities |
| Fresh nuget.org-only focused consumer | 1,493 tests; zero failures/skips; nine assembly identities |
| Provenance | All 19 assembly identities are 1.26.0.0 with informational revision `1f06efb4d938d3d8e12eb273e632a8fe15110041`; archive/cache/output DLL bytes match |
| Restore integrity | Raw signed archive SHA-512 and signature-normalized NuGet content hashes match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerable packages |

The focused consumer comprises 614 retained authoring/pattern checks, 815 numeric/Unix checks (431 previously published plus 384 carried forward from the unpublished 1.25.2 candidate), and 64 conditional checks. The conditional checks include 47 public source cases and 17 independent consumer probes; ten internal source cases remain in the 57-case source suite. Source and package counts overlap and are not summed as distinct conformance tests.

The [release notes](RELEASE-NOTES-1.26.0.md) and [REQ-138 API guide](REQ-138-IMPLEMENTATION.md) describe coordinated Wrap/Edit condition/Unwrap, bounded cancellation and complete provenance. The legacy expression overload and existing refusal values remain compatible. The numeric corrections preserve Int64 `idiv` bounds; arbitrary-precision decimal and wider generic integer storage remain separate.

[Candidate validation](RELEASE-1.26.0-CANDIDATE.md) also passed 84 language-server tests (12,200 solution/LSP total). Source manual gates remain QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894**, across 224 unchanged gated sets with 11 existing Experimental fallback passes. Current native Unix remains **22/2/0, exit 2**, including the two known negative-domain failures (120/121). Release reran the solution, not the manual harnesses. No full current-draft conformance or REQ-126 closure is claimed.

REQ-138 remains **In progress: implemented and published; consumer integration/native acceptance pending**. Braid's no-blocker source review at `41748242`, including its 1,364 authoring and 968 consumer passes, does not establish public-package adoption or native acceptance. Braid remains adopted on 1.25.1. Next: integrate 1.26.0 and validate conditional behavior, atomic Apply/history, worker/export and native smoke.

Original propagation observations, signature attempts, consumer TRXs and complete provenance receipts remain under `.guard-tmp/release1260-publication`. [Machine-readable publication proof](RELEASE-1.26.0-PUBLICATION.json) includes hashes of the retained core receipts. No retag or duplicate release dispatch occurred. Original candidate evidence is unchanged.
