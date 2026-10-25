<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.0 source and package verification">
  <h1>Bosak 1.26.0 validated release candidate</h1>
  <p>25 October 2026 - unsigned local candidate; unpublished</p>
</div>

**Publication completed:** [Verified 1.26.0 publication](RELEASE-1.26.0-PUBLICATION.md). The unsigned/unpublished statements below describe the retained pre-publication checkpoint. Original candidate JSON and evidence are unchanged.


**Publication source: `1f06efb4d938d3d8e12eb273e632a8fe15110041`**, merge of PR452. Its complete tree equals the green corrected PR head `5fa82f8dace00ee379640d21d14bd7f2ebe575d5`. [Merge-source CI](https://github.com/Fytala-Charles/Bosak/actions/runs/40182817580) passes all checks. Braid reviewed engine/code at `41748242`; its two documentation corrections are included without changing engine/test/version/design bytes. GitHub also marks the included design PR451 merged. Candidate documentation commits are separate and must never replace this immutable package/tag source.

[Release notes](RELEASE-NOTES-1.26.0.md) describe the complete conditional lifecycle and the numeric corrections carried forward from the separately pinned unsigned 1.25.2 candidate. Source version 1.26.0 is a minor feature increment. Public packages remain 1.25.1; neither 1.25.2 nor 1.26.0 was published by this preparation.

| Check | Fresh result |
|---|---|
| Release build | Zero errors; existing CS8604 conformance-runner warning |
| Solution tests | 12,116 passed /0 failed /0 skipped |
| Separate LSP tests | 84 passed /0 failed /0 skipped |
| REQ-138 source cases | 57 passed; included in the solution count |
| QT3 | 31,142/0/679 |
| Frozen qt4 | 8,436/0/1,894;224 configured/executed gates |
| Current native Unix | 22/2/0, expected exit2; cases 120/121 remain explicit |
| Broad local-package consumer | 153 checks; all ten libraries |
| Focused local-package consumer | 1,493 passed /0 failed /0 skipped; nine libraries |
| Fresh transitive audits | Neither consumer reports vulnerable packages; no NU1900 |

The focused consumer retains 1,429 numeric/authoring checks and adds 47 public conditional cases plus17 independent Braid probes. The 10 internal-construction cancellation/forged-metadata cases are excluded from this external assembly and retained within the 57-case source suite. Additional source coverage since public 1.25.1 is 441 cases: 384 numeric and 57 conditional. These counts overlap; no aggregate conformance claim is made.

All ten archives have version 1.26.0, the exact merge RepositoryCommit and internal dependency pins. Fresh PackageReference-only consumers have no project references and map Bosak.* exclusively to the local feed. All 19 consumed assembly versions1.26.0.0/informational revisions 1.26.0+1f06efb4d938d3d8e12eb273e632a8fe15110041 and archive/cache/output DLL bytes agree. Unsigned archive SHA-512, restore assets, cache contentHash/source and .sha512 receipts agree. XML documentation names and README/license/icon/brand asset bytes match source. Archives contain no signature; no signed public verification is claimed.

The official vulnerability index was reachable with HTTP200 and the public Bosak.Xslt index did not list 1.26.0 at the recorded preflight. [Machine-readable proof](RELEASE-1.26.0-CANDIDATE.json) records counters/hashes, all package/dependency/assembly identities, consumer adaptations, cache/source provenance and gate receipts. [Retained evidence](release-evidence/1.26.0/) preserves raw receipt bytes and normalized UTF8 audits. Original full TRX/logs, fresh caches and consumers remain under `.guard-tmp/release1260-candidate`; archives under `.guard-tmp/release1260-packages`. Earlier 1.25.2 and consumer source-review receipts remain historical and immutable.

REQ-138 remains **In progress** until publication and independent consumer integration/acceptance. Its initial ancestry/namespace/lexical-boundary/resource/cancellation restrictions remain explicit in the [API guide](REQ-138-IMPLEMENTATION.md). Braid is adopted on public 1.25.1; source-reference tests and these unsigned package tests establish no Braid runtime/native acceptance. REQ-126/D2/D3/D4/AC-03 remain partial/open; decimal precision, Int64 idiv bounds, generic integer constructor/storage policy, years 1-9999 Unix calendar and 28 unavailable historical adopted pins remain separate.

## Publication boundary

The owner approved merge and candidate preparation, not publication. A later approved release must tag/build the immutable source above, publish/sign through the normal workflow, and independently verify signed public packages and fresh public consumers. Braid integration and native acceptance follow separately. No tag, GitHub release or NuGet upload was performed. Local main is synced to the merge; unrelated upstream notes, original registration snapshots/stashes, sweep baselines and protected PR132 are preserved.
