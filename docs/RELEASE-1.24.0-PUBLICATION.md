<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak1.24.0 publication">
  <h1>Bosak1.24.0 publication</h1>
  <p>23 October 2026</p>
</div>

**Published and verified.** [Bosak v1.24.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.24.0) uses immutable source `ff9f509461ce7f8e639d64d4b83024dcffe32d80` and annotated tag object `8cd426091d0ab3a4f13c3e7882756b45eb8ca7de`. Preparation PR423 merged separately at `a7a3dfba`; the tag retains the tested implementation source.

This release adds source-preserving literal result element Rename and populated whole-subtree Delete together, alongside existing Create. Exact name-token edits, exclusive Rename NoChange, complete removed/survivor/attribute/merge evidence and actual candidate selection preserve the reviewed authoring contract. [Release notes](RELEASE-NOTES-1.24.0.md), [API and exact supported profile](REQ-136-IMPLEMENTATION.md).

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39974479262) | 11,009 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed1.24.0 packages; exact source/version/dependencies and approved Git README/license/icon/brand assets |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; all ten assembly identities verified |
| Fresh nuget.org-only focused consumer | 503/0/0, including all73 new element lifecycle cases; nine assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw signed archive SHA-512 and NuGet contentHash match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerable packages |

Consumers use fresh isolated caches with nuget.org-only mapping, no engine ProjectReferences or friend access. Counts overlap source coverage. Implementation, merge and preparation CI also pass84 language-server tests. Fresh reviewed engine gate receipts remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; the merged tree is identical and no engine changes occurred during preparation/publication. Release reruns the solution, not the manual harnesses.

The first profile retains no-namespace targets, creation-compatible literal ancestry, constant attributes, explicit supported versions in the Legacy authoring host, strict retained encodings and at most one whitespace-only boundary merge. Unsupported contexts refuse. This release does not certify unrestricted element editing or full XPath/XSLT4.0 conformance. REQ-126 remains In Progress. REQ-136 engine implementation is published; Braid integration and consumer acceptance remain pending.

Braid's exact-head source review cleared839 unsigned-candidate public-consumer checks. That source review, prior1.23.8 smoke and adopted1.23.7 pin are distinct from signed1.24.0 adoption, UI/session/license/revision/atomic Apply/history, worker/export/hash/reopen and native acceptance. No Braid dependency or VS Code extension changed here.

All preflight, the single workflow's logs, propagation attempts, signature logs, public TRXs and archive/cache/source/hash proofs are retained under `.guard-tmp/release1240-preparation` and `.guard-tmp/release1240-publication`. [Machine-readable publication proof](RELEASE-1.24.0-PUBLICATION.json). No retagging or duplicate dispatch occurred. Pending upstream notes, retained stashes/worktrees/caches, sweep baselines and protected PR132 remain preserved.
