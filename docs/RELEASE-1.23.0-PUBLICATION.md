<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.0 publication">
<h1>Bosak 1.23.0 publication</h1>
<p>21 October 2026</p>
</div>

**Published and verified.** [Bosak v1.23.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.23.0) uses immutable implementation source `9218a2b6f2339f923fd6d018ba988c8325b2c44d` and annotated tag object `8d202a8cb123a67ab0fac048ff94297fde3c037e`. Preparation PR #393 merged separately as `deaea957d9db4d5fa449107aff0403cadf61ac22`; the release tag remains pinned to the reviewed implementation merge.

The release adds REQ-135 literal attribute Create, Update and Delete together. It also delivers the previously unpublished switch fallback ordering correction, harness safeguards and array/xsl:select boundary regressions. Read the [release notes](RELEASE-NOTES-1.23.0.md) and [attribute contract](REQ-135-ATTRIBUTE-LIFECYCLE-CONTRACT.md).

| Verification | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39564362629) | 10,214 solution tests passed, zero failed/skipped; ten upload acknowledgments |
| Public archives | Ten signed 1.23.0 packages, exact source revision, aligned dependencies and approved Git README/license/icon/brand bytes |
| Repository signatures | All ten `dotnet nuget verify --all` checks passed |
| Fresh nuget.org-only broad consumer | 153 checks passed; all ten consumed assembly identities verified |
| Fresh nuget.org-only lifecycle consumer | 117/0/0; all nine consumed assembly identities verified |
| Provenance | Signed archive/cache/output DLL bytes match; raw signed archive SHA-512 and NuGet PackageArchiveReader contentHash independently match their respective restore records |
| Dependency audits | Both transitive vulnerability audits clear |

Both public consumers use separate fresh caches and PackageReferences without engine project references. The 19 internal lifecycle controls are excluded from the 117 public cases. Counts overlap source verification. Prior exact-merge CI passed10,214 solution +84 language-server tests,16 runner controls, four Windows wrapper controls and highlighting. Earlier unchanged-source manual gates remain QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**; the Release workflow reruns the solution, not those separate gates.

[Braid's preserved exact-merge review](REQ-135-CONSUMER-MERGE-REVIEW.md) independently passed700 checks (117 imported lifecycle +551 prior authoring/identity +32 probes) without blockers against unsigned local candidates at the same source. The signed public evidence above is separate. Braid remains on1.22.0 until its own adoption. LA17–LA22 adapter/session/review/atomic Apply/history/navigation and real worker/restricted export/reopen/native acceptance remain pending. The VS Code extension has a separate release cycle.

Original preflight, workflow logs, propagation snapshots, signatures, TRXs, metadata, restore sources and hashes remain under `.guard-tmp/release1230-publication`. [Machine-readable proof](RELEASE-1.23.0-PUBLICATION.json) records identities. One annotated tag push and one successful publication run were used; no retag or duplicate dispatch.
