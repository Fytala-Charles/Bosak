<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala verified Bosak 1.19.0 publication">
<h1>Bosak 1.19.0 publication</h1>
<p>Published and verified - 18 October 2026</p>
</div>

User approved publication following [release preparation](RELEASE-1.19.0-READINESS.md) and [Braid's no-blocking implementation review](REQ-131-CONSUMER-IMPLEMENTATION-REVIEW.md). Immutable annotated `v1.19.0` points exactly to `27691fdb3708bb4ffd7c984abbf3196b7cde5667`, tag object 682f361ca25cd1c2f3cad09b275d3087b9137ca0. [Public GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.19.0). One tag push triggered Release; no retag or duplicate dispatch.

| Gate | Verified result |
|---|---|
| Single tag-triggered Release | [Run 39074829260](https://github.com/Fytala-Charles/Bosak/actions/runs/39074829260) succeeds: **9,726 solution tests**, ten library upload acknowledgments. |
| Public signed artifacts | All ten 1.19.0 archives downloaded; repository signatures verified; version/dependencies/repository revision/XML docs/branding and committed source asset bytes checked. |
| Fresh public-only consumers | **153 broad checks** and **282 focused public tests** pass, zero focused failures/skips. Separate fresh caches; only nuget.org configured for restore/test/audit. |
| Provenance | Ten broad/nine focused assemblies identify exact 1.19.0+27691fdb revision. Signed archive/cache/output DLL bytes, restored archive SHA-256/SHA-512, assets and source receipts agree. |
| Dependency audit | Zero vulnerable top-level/transitive dependencies reported. |

[Structured publication proof](RELEASE-1.19.0-PUBLICATION.json) retains archive hashes, source URLs, consumer identity and restore provenance. Local execution receipts are under `.guard-tmp/release1190-publication/`; they are observed publication checks, separate from imported Braid reports. The Release workflow runs solution tests; the 84 language-server checks and frozen gates were verified during source delivery, not rerun by this workflow.

## Delivered profile and consumer boundary

[Release notes](RELEASE-NOTES-1.19.0.md) describe additive bounded literal xsl:text insertion, typed targets/proposals/candidates, PreserveSource/explicit Readable, exact Unicode/non-expansion and complete source/new-subtree provenance. Existing insertion/deletion compatibility and conservative profile/resource/encoding limits remain. No arbitrary XML/subtree API or wider conformance claim.

REQ-131 is implemented and published; **Braid integration and independent functional/native acceptance remain open**. Braid's no-blocking review used local candidate archives and overlapping 291/282/298 suites, including 16 independent probes; it is not public adoption or native acceptance. Its accepted production pin remains public 1.18.0 at 3264fcfa until independent verified adoption. REQ-130 remains accepted in its delivered profile. VS Code 0.2.5 bundles 1.16.5 and is unchanged.

Next: Braid independently verifies this immutable tag and public packages, adopts the pin and implements text-specific adapter/capabilities, frozen review/Apply, selection/history, worker/restricted export/reopen and separate native acceptance. No Braid source/pin/UI changes are made by this publication.


## Signed-package hash cross-check

Raw signed archive SHA-512 matches the cached archive and its .sha512 file. NuGet signed-package content identity is checked separately using the SDK PackageArchiveReader.GetContentHash API; all ten computed content hashes match the restored assets/.nupkg.metadata values in both consumers. This distinction follows [NuGet?s implementation](https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Packaging/PackageArchiveReader.cs). An additional verifier initially equated these two hashes and failed; the incorrect assumption was corrected without changing packages or rerunning publication. The original correction receipt remains in extra-raw-hash-assumption-correction.txt beside the computed hashes.
