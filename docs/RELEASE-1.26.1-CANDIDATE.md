<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.1 source and package verification">
  <h1>Bosak 1.26.1 consolidated release candidate</h1>
  <p>26 October 2026 — unsigned local candidate; unpublished</p>
</div>

**Pinned package source: `65417cdb3bff184ca9ff1f624582f55ca4681db9`.** This is the reviewed local rounding commit, containing all preceding unpublished 1.26.1 corrections. It was built and tested in a clean isolated checkout. Its complete Git tree, package RepositoryCommit and assembly informational revisions are recorded in [the machine-readable proof](RELEASE-1.26.1-CANDIDATE.json). Candidate documentation commits are separate and must not replace this package source. Remote CI was not verified by this preparation.

[Candidate release notes](RELEASE-NOTES-1.26.1.md) summarize the patch. Public installation remains **1.26.0**. This step creates no tag, GitHub release, NuGet upload or public signature verification.

| Verification | Fresh result |
|---|---|
| Release build | Zero errors; existing CS8604 conformance-runner warning |
| Solution tests | 14,536 passed /0 failed /0 skipped |
| Separate LSP tests | 84 passed /0 failed /0 skipped |
| QT3 frozen gate | 31,142/0/679 |
| qt4 frozen gate | 8,436/0/1,894; all 224 configured sets executed |
| Current-native Unix diagnostic | 22/2/0; expected exit 2, known cases 120/121 |
| Broad PackageReference-only consumer | 153 checks; all ten libraries |
| Focused PackageReference-only consumer | 3,913 passed /0 failed /0 skipped; nine libraries |
| Fresh dependency audits | Neither consumer reports vulnerable packages; official vulnerability index HTTP 200; no NU1900 |

## Consolidated patch coverage

Since public 1.26.0, the source adds **2,420 regressions and controls** in seven files. All seven are copied byte-for-byte from the pinned source into the external package consumer; each class has the same pass count in source and package runs.

| Test class | Cases | Scope |
|---|---:|---|
| UnaryCoercionTests | 79 | Repeated unary operators preserve conversion, normalization and required errors |
| UnaryEmptyAtomizationTests | 64 | Empty results after unary atomization |
| ArrayAtomizationAuditTests | 72 | Supported independent recursive-array controls |
| ArraySingletonAtomizationTests | 111 | Cardinality after recursively atomizing array members |
| BinaryEmptyAtomizationTests | 214 | Empty numeric binary operands and modern ranges |
| AtomicFunctionConversionTests | 366 | abs argument conversion, cardinality and coerced-name preservation |
| RoundingFunctionConversionTests | 1,514 | Rounding numeric/precision conversion, nodes, types and function identity |

The focused consumer retains the previous **1,493** checks, including **47** public conditional lifecycle cases and **17** independent Braid conditional probes, then adds the **2,420** new cases. The ten internal-construction conditional cases remain in source coverage and are not copied into the external consumer. Source, package and diagnostic counts overlap; they are not added into a conformance total.

## Package provenance

All ten archives are **unsigned local 1.26.1 packages** with RepositoryCommit `65417cdb3bff184ca9ff1f624582f55ca4681db9` and internal Bosak dependency versions 1.26.1. The consumers have no project or binary references. Bosak.* is mapped exclusively to the local candidate feed, with separate initially empty caches for broad and focused consumers. Other dependencies restore from nuget.org.

All **19 consumed assembly identities** (ten broad, nine focused) have version **1.26.1.0** and informational revision **1.26.1+65417cdb3bff184ca9ff1f624582f55ca4681db9**. Package/archive, cache and output DLL bytes agree. Archive SHA-512, restore contentHash, cache metadata/source and .sha512 receipts agree. XML documentation assembly names and packaged README/license/icon/brand asset bytes match the pinned source. No public signatures are inferred from these checks.

All ten official package indexes contained public 1.26.0 and omitted 1.26.1 at the recorded preflight. Both fresh transitive vulnerability audits report no vulnerable packages. This is a dated audit result, not a promise about later advisories.

[Retained evidence](release-evidence/1.26.1/) includes source/consumer plans, archive and assembly identities, per-class coverage, original logs, normalized audit JSON, official preflight responses, source/consumer receipt archives and a package-consumer source bundle. Raw files are protected from Git line-ending conversion. Full original TRXs, consumer outputs and caches remain under `.guard-tmp/release1261-candidate`; the ten archives are under `.guard-tmp/release1261-packages`.

To repeat the package checks, extract the consumer-source bundle, point both NuGet.Config candidate sources at the retained package directory, and use separate empty NUGET_PACKAGES directories. Restore with the respective config and `--no-http-cache`; run the broad console application and the focused test project in Release with `--no-restore`. Set BOSAK_RELEASE_RECEIPT_DIR to a receipt directory for the broad metadata output. The evidence retains the executed commands and verification scripts.

Run `python docs/release-evidence/1.26.1/validate-evidence.py` to verify the retained ledger, source tree, receipt/source bundles and counts. Add `--local-artifacts` to verify the ten retained local nupkgs too.

## Limits and publication boundary

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial/open. The catalog remains 76 entries /3,148 feature +24 chapter checks; these regressions are outside that inventory. Fixed-precision decimal arithmetic, generic wide-integer storage/casts, wide abs and extreme rounding precision, Int64 idiv/precision payload bounds, schema-list cardinality, generalized metadata/coercion and BC-dynamic conversion remain separate. The numeric empty-result correction does not certify early date/duration or backwards-compatible arithmetic dispatch. DateTimeOffset-based Unix operations retain years 1–9999; extended-year construction is a separate capability. Known current-native Unix failures and existing fallback passes do not certify full current-draft conformance.

The XSLT authoring implementation is byte-unchanged since public 1.26.0. REQ-138's bounded Braid acceptance remains valid on that public version; these candidate tests do not establish Braid 1.26.1 adoption or new native acceptance.

Next is review and the separate owner release decision for pinned source `65417cdb3bff184ca9ff1f624582f55ca4681db9`. A later authorized publication must use the normal signing/publishing workflow, verify signed public packages and fresh public consumers, and keep any consumer adoption/native acceptance separate. The current worktree is not substituted for the pinned source; no workflow dispatch, tag, release or upload is authorized by candidate preparation.
