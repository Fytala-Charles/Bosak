<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.8 publication">
  <h1>Bosak 1.23.8 publication</h1>
  <p>23 October 2026</p>
</div>

**Published and verified.** [Bosak v1.23.8](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.23.8) uses immutable source `6d12bed35dac56a635acc7702645ef9085103c83` and annotated tag object `0bc4bf9260a4a9dc5fae88baec5678a691570102`. Preparation PR #418 merged separately at `2028dca4`; the tag retains the tested implementation source.

This release fixes omitted `mode` on `xsl:apply-templates` inside stylesheet functions, avoiding empty-QName XTSE0020 and preserving named defaults. Explicit modes, empty selections and explicitly empty mode errors remain covered. It also includes ordinary and direct module/package host-profile documentation. [Release notes](RELEASE-NOTES-1.23.8.md), [bounded fix](XSLT-FUNCTION-DEFAULT-MODE.md). No public API is added; this does not certify every lexical/default/static/package combination or full XPath/XSLT 4.0 conformance.

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39932304238) | 10,936 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed 1.23.8 packages; exact source/version/dependencies and approved Git README/license/icon/branding bytes |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | 153 checks; all ten assembly identities verified |
| Fresh nuget.org-only focused consumer | 430/0/0: 117 lifecycle +32 switch +27 index-of +96 priority +44 EQName brackets +44 record gates +34 numeric boundaries +36 function default-mode checks; nine assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw signed archive SHA-512 and NuGet contentHash match their respective restore records |
| Dependency audits | Both transitive audits report no vulnerabilities |

Consumers use separate fresh caches with nuget.org-only mapping, no engine ProjectReferences or friend-assembly identity. Counts overlap source coverage. Implementation and preparation CI each pass 10,936 solution +84 language-server tests. Corrected-engine manual gate receipts remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; Release reruns the solution, not the manual harnesses. The existing conformance nullable warning remains outside this release's scope.

REQ-126 remains In Progress: 76 entries, 3,049 feature +24 chapter checks =3,073; four worked chapters and117 assigned names. D2/D3/D4 and AC-03 stay partial; 28 historical adoption pins remain unavailable. Braid's1.23.7 smoke result has not been received; no dependency change or new native acceptance is claimed. The VS Code extension keeps its separate release cycle. Pending upstream notes and protected PR132 are preserved.

Preflight, the one workflow's logs, propagation attempts, signatures, public TRXs and archive/cache/source/hash proofs are retained under `.guard-tmp/release1238-preparation` and `.guard-tmp/release1238-publication`. [Machine-readable proof](RELEASE-1.23.8-PUBLICATION.json). One annotated tag push and one Release run were used, without retagging or duplicate dispatch.
