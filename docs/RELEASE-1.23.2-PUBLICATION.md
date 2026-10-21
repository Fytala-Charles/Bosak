<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.2 publication">
  <h1>Bosak 1.23.2 publication</h1>
  <p>21 October 2026</p>
</div>

**Published and verified.** [Bosak v1.23.2](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.23.2) uses immutable implementation source `7832b785925d2491e0ceac0dd9faa60261619bc4` and annotated tag object `34fe2692f97e1fd07c9474a2aa01089b2d4e8b50`. Preparation PR #400 merged separately at245f243e; the version tag retains the reviewed implementation revision.

This release includes the switch selected-node identity correction from unpublished source1.23.1 and the XPath4.0 `index-of(..., ())` default-collation correction. It also delivers the REQ-126 customer-reference equality/collation example. No separate1.23.1 release was made. Read the [release notes](RELEASE-NOTES-1.23.2.md), [switch evidence](XSLT40-SWITCH-BOUNDARIES.md) and [index-of evidence](xpath40-catalog/INDEX-OF-EMPTY-COLLATION.md).

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39627053490) |10,331 solution tests; zero failures/skips; ten upload acknowledgments |
| Public archives | Ten signed1.23.2 packages; exact source/version, aligned dependencies and approved Git README/license/icon/branding bytes |
| Repository signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer |153 checks; all ten assembly identities verified |
| Fresh nuget.org-only focused consumer |176/0/0:117 existing attribute-lifecycle +32 switch +27 index-of; all nine assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes and informational revisions match; raw signed archive SHA-512 and NuGet PackageArchiveReader contentHash each match their respective restore records |
| Dependency audits | Both transitive vulnerability audits report no vulnerabilities |

Separate fresh caches and nuget.org-only source mapping were used; consumers have no engine ProjectReferences or friend-assembly identity. Consumer counts overlap source tests. Exact-head and merged-main CI pass10,331 solution +84 separate language-server tests. Previously verified identical reviewed implementation gates remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; the publication workflow reruns the solution, not these separate harnesses. The known conformance CS8604 warning remains outside this correction.

REQ-126 remains In Progress with68 catalog entries and2,660 checks. Upstream PRs #33/#34 remain separately pending; no upstream acceptance or full XSLT4 conformance is claimed. [REQ-135's completed Braid acceptance](REQ-135-BRAID-ACCEPTANCE.md) applies to public1.23.0 and remains recorded. The1.23.2 upgrade merits a normal Braid smoke check; these engine proofs do not claim Braid has upgraded. No Braid files/pins or VS Code extension release changed.

Original approval/preflight, workflow logs, propagation receipts, signatures, TRX, source/cache metadata and hash checks remain under `.guard-tmp/release1232-publication`. Local unsigned candidate evidence remains separate under `.guard-tmp/release1232-preparation`. [Machine-readable publication proof](RELEASE-1.23.2-PUBLICATION.json). One annotated tag push and one successful Release run were used; no retag or duplicate dispatch.

Verification tooling corrections are recorded separately from library behavior: PowerShell JSON enumeration initially duplicated signature receipt rows; the final set contains one success per exact archive. An initial .NET10 audit implicitly restored from a different cache and changed assets metadata; both audit stages now use `--no-restore`. The corrected fresh public attempt passes153/176 with complete provenance. Original duplicate rows, initial restore metadata and logs remain preserved. Neither correction changed source, packages or the release tag.
