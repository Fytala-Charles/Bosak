<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.7 publication">
  <h1>Bosak 1.23.7 publication</h1>
  <p>23 October 2026</p>
</div>

**Published and verified.** [Bosak v1.23.7](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.23.7) uses immutable source `9dd5096104b9d08bfad2dd88130f04ed0c3807f8` and annotated tag object `6110c0c7c61a9a8d1a4ca6f2ee608db99ae81b86`. Preparation PR #413 merged separately at `ff2a9cd7`; the tag retains the tested implementation source.

This release includes the previously unpublished EQName bracket parsing (1.23.5), inline-record signature old-profile gate (1.23.6), and fractional/exponent separator boundary (1.23.7) fixes. No separate1.23.5/1.23.6 release was made. [Release notes](RELEASE-NOTES-1.23.7.md) explain profile boundaries, compatibility effects and remaining limitations. The corrections add no public API and do not establish full XPath/XSLT4.0 conformance or arbitrary precision.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39900443754) |10,720 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed 1.23.7 packages; exact source/version/dependencies and approved Git README/license/icon/branding bytes |
| Signatures | All ten dotnet nuget verify --all checks pass |
| Fresh nuget.org-only broad consumer |153 checks; all ten assembly identities verified |
| Fresh nuget.org-only focused consumer |394/0/0:117 lifecycle +32 switch +27 index-of +96 priority +44 EQName brackets +44 record gates +34 numeric boundaries; nine assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw archive SHA-512 and NuGet contentHash match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerabilities |

Consumers use separate fresh caches and nuget.org-only source mapping, with no engine ProjectReferences or friend-assembly identity. Their counts overlap source coverage. Implementation and preparation CI pass 10,720 solution +84 language-server tests. Manual gate receipts from the corrected implementation remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; Release reruns the solution, not those manual harnesses. The existing conformance nullable warning remains outside this release's scope.

REQ-126 stays In Progress: 74 entries, 2,869 feature +24 chapter checks =2,893; four worked chapters and 117 assigned function names. D2/D3/D4 and AC-03 remain partial; 28 historical adoption pins remain missing. No upstream acceptance is implied. Braid's last recorded native acceptance remains 1.23.0; its dependency 1.23.4 smoke is separate. No Braid pins or VS Code extension release change here. Pending upstream notes and protected PR132 remain preserved.

Preflight, one workflow's logs, propagation attempts, signatures, public TRXs and archive/cache/source/hash proofs remain under `.guard-tmp/release1237-preparation` and `.guard-tmp/release1237-publication`. [Machine-readable proof](RELEASE-1.23.7-PUBLICATION.json). One annotated tag push and one Release run were used, with no retag or duplicate dispatch.
