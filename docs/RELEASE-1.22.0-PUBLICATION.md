<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.22.0 publication">
<h1>Bosak 1.22.0 publication</h1>
<p>19 October 2026</p>
</div>

**Published and independently verified.** [Release v1.22.0](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.22.0) targets reviewed source `3b00938875490d1997b09f53b80f2ee2986a666b`. Annotated tag `8a96e8ed966c379e8596977d76a8f2f069a7f485` remains immutable.

REQ-134 adds deletion of one eligible ordinary literal `xsl:text` instruction, including its actual optional payload. Separate opaque authority, typed cancellation, nullable self-closing body span, exact outside-byte preservation and complete removed/surviving/bounded-whitespace-merge partitions follow the reviewed contract. Every success changes source, including empty/self-closing instructions with unchanged output; navigation uses the actual mapped parent. Read the [release notes](RELEASE-NOTES-1.22.0.md), [contract](REQ-134-TEXT-DELETION-CONTRACT.md) and [implementation record](REQ-134-IMPLEMENTATION.md).

| Gate | Observed result |
|---|---|
| Single tag-triggered [Release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/39290775983) | Success at `3b00938875490d1997b09f53b80f2ee2986a666b`; **10,010** solution tests pass, zero failed/skipped; ten upload acknowledgments |
| Public archives | Ten signed 1.22.0 archives; exact repository revisions, aligned Bosak dependencies, XML docs and approved Git README/license/icon/brand bytes |
| Signatures | All ten `dotnet nuget verify --all` checks pass |
| Fresh nuget.org-only broad consumer | **153** checks pass; all ten actual assembly/archive/cache/output identities verified |
| Fresh nuget.org-only focused consumer | **531/0/0**, including 94 new deletion checks plus437 legacy/identity checks; nine consumed assemblies verified |
| Signed content and dependencies | Raw signed archive/cache hashes match; NuGet PackageArchiveReader hashes match restored assets/metadata; both transitive vulnerability audits clear |

The consumers use separate fresh caches, no engine project references, a non-friend test identity and nuget.org only. Sixteen new internal controls and thirteen prior source controls are excluded from public tests. Counts overlap. Earlier source verification passes110 new cases,10,010 solution/84 LS, exact-head and merge CI10,094, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**. Release reruns the solution, not the separate language-server/manual gates.

[Braid's unchanged implementation review](REQ-134-CONSUMER-IMPLEMENTATION-REVIEW.md) closes the exact-merge delivery gate with no blocker and independently passed **551/0/0** (531 imported +20 probes) against fresh unsigned local merge packages. The prior head rerun is overlapping historical evidence. Review SHA-256 `96a76332f7f2a04e3d211a772633ccdcec6735459a4d143b675bd9fd8d2e9b2d`. This is separate from the signed public verification above and from Braid adoption/UI/history/real worker/restricted export/reopen/native acceptance. Braid's production dependency remains1.21.0 until its own integration.

Original workflow logs, propagation snapshots, signature results, TRXs, archive/hash/assembly metadata and restore-source receipts remain under `.guard-tmp/release1220-publication`. The [machine-readable proof](RELEASE-1.22.0-PUBLICATION.json) records exact identities. One tag push and one successful publication run were used; no retag, duplicate publish or reupload. The package README is the approved immutable source snapshot; main's quick-start links are updated separately. The VS Code extension retains its separate release cycle.
