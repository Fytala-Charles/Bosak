# Bosak REQ-131 Consumer Implementation Review

Date: 2026-10-17. Delivery: [PR #364](https://github.com/Fytala-Charles/Bosak/pull/364), merged. Exact head `189405f2b7277b32a416323dbb45bb15fbf974c4`; exact integrated revision `27691fdb3708bb4ffd7c984abbf3196b7cde5667`. Source 1.19.0 remains unpublished; latest public release verified as 1.18.0. Braid's production pin remains 3264fcfaf739e492770c882d2cbfc1f93f0172d2.

## Verdict

**No blocking implementation finding within the declared bounded literal-text insertion profile.** Suitable to proceed to publication through Bosak's release workflow and then separately verified Braid integration. This review does not implement Braid text review/Apply/history/selection, certify the native UI or authorize release/publication. Existing deletion acceptance cannot be reused as text-insertion acceptance.

Verified merged/head source and test trees are identical, exact merged checkout is clean, and both PR-head CI checks succeeded. Reviewed the typed target/proposal/result/candidate, scalar validation/escaping and byte accounting, namespace selection, explicit non-expansion, candidate inspection/validation, complete ordinary-survivor/new-subtree provenance, cancellation and shared layout/correspondence changes against adopted 1.18.0. Changes are additive; legacy layout limits/defaults remain unchanged.

## Independently Executed Verification

| Run | Passed/failed/skipped | Scope |
|-----|-----------------------|-------|
| Exact merged source-focused suite | **291/0/0** | 65 text, 177 value-of/readable and 49 deletion cases, including source-only internal controls |
| Unchanged package-only public contracts | **282/0/0** | 61 public text, 175 legacy insertion/readable, 45 deletion and one nine-assembly provenance case |
| Public contracts plus new Braid probes | **298/0/0** | The same 282 plus 16 independent Braid public-API cases |
| Accepted Braid checkpoint revalidation | **1,015/0/0** | Normal production settings and unchanged 1.18.0 pin, before committing/pushing accepted REQ-130 |

These suites overlap; their counts are not additive unique coverage. Source-focused tests used RunAnalyzers=false/TreatWarningsAsErrors=false for inherited upstream warnings. The isolated imported package project also disables those analyzers/documentation generation; normal Braid settings remain unchanged. No source modification or internal/friend access was used in the distinct Bosak.TextPackageConsumer.Tests package assembly. Full upstream solution, language-server and frozen conformance gates were not independently rerun. Upstream reports 9,726 solution plus 84 language-server tests, with unchanged frozen QT3/qt4 gates; these are upstream reports, not new independent results here.

Fresh independent restore uses exact merged local 1.19.0 archives from the upstream delivery into .dependencies/req131-independent-package-cache. All nine consumed assemblies identify `1.19.0+27691fdb3708bb4ffd7c984abbf3196b7cde5667`. Archive DLL, fresh cache DLL and output DLL SHA-256 values match the original exact-merge provenance receipt; fresh cached archives also match the input archives. Assets contain nine Bosak package libraries and no project references. These are unpublished local artifacts, not public NuGet verification or a newly produced package build.

Receipts: .acceptance/req131-review-27691fdb/source-results, public-results, independent-corrected, package-identity-independent.json, independent-provenance.json and input-archives.json. Independent probe source is retained there as BraidTextProbes.cs. The first independent fixture passed unpaired surrogate strings through InlineData; test serialization replaced them and produced a duplicate identity plus one misleading failure before engine invocation. Corrected probes construct invalid UTF-16 at runtime from integer variants; the original failing receipt is retained in independent-results. No engine correction was needed.

## Additional Braid Probe Coverage

Eight mixed lifecycle cases combine four encodings and two layout modes. Empty text, hostile-looking XPath/markup, literal CR/LF/TAB, XML escaping, non-BMP/combining/U+FEFF characters and repeated text insertion remain exact. Text insertion, dynamic value-of insertion, another text insertion and value-of deletion use fresh snapshots. Source edits replay exactly, all old descriptors map injectively outside the new subtree, attributes/child order remain intact, and returned bytes/evidence are immutable. Ordinary-file export/hash/reopen/reinspection/run and subsequent authoring preserve the dedicated changed logical output and original bytes.

Three namespace/expansion cases cover inherited expansion, default XSLT namespace, explicit aliases and collisions. Generated expand-text=no keeps strings such as `{unbound:call()}` and markup literal. One concurrent ownership case runs eight proposals from the same immutable original target: each is isolated; the target refuses when supplied to a different snapshot, while remaining valid with its original snapshot. Four invalid-Unicode/cancellation cases retain original bytes and return no candidate. These are public-engine consumer probes, not Braid worker/restricted-export/session/UI integration tests.

## Contract Details Braid Must Preserve

- Text uses separate SupportsTextInsertion/SupportsReadableTextInsertion and typed target/proposal APIs. A visual role or an insertion/deletion capability grants no text-authoring authority.
- TextValue is exact literal Unicode, never XPath/AVT/TVT or markup. Preserve tabs, CR/LF pairs, leading/trailing whitespace, empty strings and normalization distinctions; do not trim or normalize in the editor. Bosak owns escaping and explicit expand-text=no. No DOE switch is offered.
- TextPayloadRange identifies encoded source content. Nonempty text has one actual payload descriptor; empty text has an empty payload range and null PayloadNode, while its new instruction remains real and explicitly closed. Do not invent a select slot or a synthetic payload node.
- NodeCorrespondence is a total injective integer-ID map scoped to InputSnapshot/Snapshot, unlike deletion's descriptor map and merge groups. Exclude the entire new instruction subtree; every additional unmapped candidate descriptor must be edit-covered formatting whitespace. Check exact source edit replay and complete old/new partitions before issuing Apply authority.
- PreserveSource remains the engine default. Explicit Readable has complete source edits, its own byte limit and no silent fallback; it may change only supported constructor layout, never payload bytes. Source-size and escaping-amplification accounting includes BOM and encoded candidate footprint.
- Cancellation is an insertion-style classified Cancelled result, unlike deletion's throwing contract. Braid must still check cancellation/current source/license around awaits and final source-owner acceptance; old immutable engine targets do not know Braid's current revision.
- The delivered text operation restricts existing xsl:text independently: ordinary text-only body, no effective expansion and absent/no DOE. Published 1.18.0 already permitted several existing-text cases for value-of insertion; their compatibility is retained rather than retroactively narrowed. Existing mixed/preserved/CDATA/dependency/version/static/resource exclusions remain boundaries.

## Adoption Gates and Disposition

Await a published delivery and verify its exact tag/release, public packages and assembly provenance before changing Braid's immutable pin. Then implement the separate adapter/capability/port, frozen complete review and explicit source-owner Apply, exact instruction/payload selection including empty-text behavior, one-step history, graph/outline handoff and bounded desktop review. Add Braid malformed-evidence/race/policy/cancellation cases and dedicated changed-output worker/restricted-export/reopen goldens; prepare separate native acceptance. No Braid runtime/API/pin or upstream engine source was changed by this review.

Accepted REQ-130 is pushed in Braid commit `6d89fcd5e53d65560c4004b18b98264aa20e1fee`. Its formal upstream notice is pushed at be307647b0fcc80b027f9e8f4905bb2acd3f4229 and ready for maintainer review in [PR #366](https://github.com/Fytala-Charles/Bosak/pull/366). That documentation PR is not merged at this checkpoint; highlighting CI passes and the build/test check remains running. The historical Bosak root and unrelated edits remain preserved. No merge, release/tag or new native acceptance was performed.
