<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.20.0 publication">
<h1>Bosak 1.20.0 publication</h1>
<p>18 October 2026</p>
</div>

**Published and verified.** [Release v1.20.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.20.0) targets explicitly approved source `91f1af0d488bb2ed72b219118eeb1ab3570726bd`. Annotated tag object `3def4bc7e6aecbcdbb81c4b64f0b50a33a73beae` remains immutable.

Adds the bounded REQ-132 paired empty no-namespace literal result element operation: engine-owned target/local-name validation, local namespace reset, explicit PreserveSource/Readable layout, complete edit/descriptor partition and actual empty-body selection. Fresh text/value-of targets can populate the new element. See [release notes](RELEASE-NOTES-1.20.0.md), [implementation](REQ-132-IMPLEMENTATION.md) and [source proof](REQ-132-SOURCE-VERIFICATION.json).

| Check | Observed result |
|---|---|
| Single tag-triggered [Release run](https://github.com/Fytala-Charles/Bosak/actions/runs/39122930266) | Success at approved `91f1af0d`; **9,824** solution tests pass, zero failed/skipped, ten upload acknowledgments. |
| Public archives | All ten signed NuGet 1.20.0 packages downloaded; exact repository revisions, aligned dependencies, README/license/icon/assets and XML documentation verified against the approved Git blobs. |
| Signatures | All ten `dotnet nuget verify --all` checks pass. |
| Fresh public broad consumer | **153** checks pass; all ten assembly identities and archive/cache/output bytes verify. |
| Fresh public focused consumer | **374/0/0**: 85 public literal-element cases plus 289 existing authoring/regression/identity checks; nine assemblies verify. Six source-only controls excluded. |
| Signed content and audit | Raw signed archive/cache hashes match; NuGet PackageArchiveReader content hashes match restored assets/metadata. Both dependency audits clear. |

Consumers use separate fresh caches, nuget.org as the only feed, distinct non-friend assembly identity and no project references. Counts overlap source coverage and are not additive. These public signed archives are separate evidence from local unsigned candidates. The upstream source record includes 91 focused tests, 9,824 solution tests, 84 language-server tests, frozen QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894** and 9,908 tests in CI. The Release workflow reruns the solution; the separate gates and language-server results retain their source-verification receipts.

[Braid implementation review](REQ-132-CONSUMER-IMPLEMENTATION-REVIEW.md) reports no blocking findings at `91f1af0d`, with independently observed 91 source tests and 399 local-package checks (374 imported plus 25 Braid probes). Its original build/fixture corrections and scope limits remain unchanged in that report. It did not independently rerun the full upstream suite/gates or certify public packages, Braid UI adoption, restricted export/worker workflow, destination relocation or native acceptance. The review hash is `58c80bac435dc1a3cdd375f4a0513aecf1d21cd50bd9cae9f7985d641649ac5c`.

NuGet propagation was checked after successful upload; original availability snapshots are retained. No duplicate publication workflow, reupload or retag was used. Raw receipts live under `.guard-tmp/release1200-publication/`; [machine-readable publication proof](RELEASE-1.20.0-PUBLICATION.json) records literal hashes and identities.

This publishes the ten NuGet libraries. Braid remains on its independently adopted version until its own verified integration/acceptance; no pin/UI/history/native change is claimed. The VS Code extension retains its separate release cycle. The package README is the immutable approved source snapshot; the main-branch README reflects the now-published capability.
