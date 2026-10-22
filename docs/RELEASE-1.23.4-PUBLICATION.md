<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.4 publication">
  <h1>Bosak 1.23.4 publication</h1>
  <p>22 October 2026</p>
</div>

**Published and verified.** [Bosak v1.23.4](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.23.4) uses immutable source `2b5ffc5076af0411dee606b010368a36d637863a` and annotated tag object `a044b1206c6d1f722a089a6eb37d20653b2ebfdd`. Preparation PR #407 merged separately at `4decc84b`; the release tag retains the verified implementation revision.

This release includes the unpublished 1.23.3 minimum node-union default-priority correction and the 1.23.4 EQName URI-punctuation priority correction. Effective 4.0 implicit priorities can now select a different template; explicit priorities and earlier-profile behavior are preserved. No separate 1.23.3 release was made. Read the [release notes](RELEASE-NOTES-1.23.4.md), [union audit](xpath40-catalog/UNION-MINIMUM-PRIORITY.md) and [EQName audit](xpath40-catalog/EQNAME-PATTERN-PRIORITY.md). Opening-bracket URI parsing remains separately limited.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39751206130) | 10,470 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed 1.23.4 packages; exact source/version/dependencies and approved Git README/license/icon/branding bytes |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; all ten assembly identities verified |
| Fresh nuget.org-only focused consumer | 270/0/0: 117 lifecycle +32 switch +27 index-of +94 EQName/union/set-priority checks; nine assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw archive SHA-512 and NuGet contentHash each match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerabilities |

Separate fresh caches and nuget.org-only source mapping were used; consumers have no engine ProjectReferences or friend-assembly identity. Consumer counts overlap source tests. Reviewed implementation and preparation CI passed 10,470 solution +84 language-server tests. Identical reviewed implementation gates remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; Release reruns the solution, not those manual harnesses. The existing conformance nullable warning is outside this release's corrections.

REQ-126 remains In Progress: 71 catalog entries, 2,743 feature +24 chapter checks =2,767; four worked chapters and117 assigned function names. D2/D3/D4 and AC-03 remain partial. No full XSLT4 conformance or upstream acceptance is claimed. [Completed Braid acceptance](REQ-135-BRAID-ACCEPTANCE.md) on1.23.0 remains recorded; a1.23.4 upgrade smoke check is separate. No Braid pins or VS Code extension release changed. Pending upstream notes and protected PR #132 remain untouched.

Original preparation, failed copied-consumer guard, preflight, workflow logs, propagation receipts, signatures, public TRXs and source/cache/hash proofs remain under `.guard-tmp/release1234-preparation` and `.guard-tmp/release1234-publication`. [Machine-readable publication proof](RELEASE-1.23.4-PUBLICATION.json). One annotated tag push and one Release run were used; no retag or duplicate dispatch.
