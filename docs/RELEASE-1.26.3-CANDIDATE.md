<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.3 source and package verification">
  <h1>Bosak 1.26.3 consolidated release candidate</h1>
  <p>26 October 2026 — unsigned local candidate; unpublished</p>
</div>

**Publication completed:** [Verified 1.26.3 publication](RELEASE-1.26.3-PUBLICATION.md). Candidate/unpublished statements below retain the pre-publication checkpoint; original candidate JSON and evidence remain unchanged.


**Pinned package source: `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`.** This candidate consolidates abs integer preservation, floor/ceiling integer preservation and the completed floor/ceiling argument audit. A clean isolated checkout supplies all builds, tests and ten packages. [Machine-readable proof](RELEASE-1.26.3-CANDIDATE.json) records its complete tree, RepositoryCommit and assembly informational revisions. Later candidate documentation is separate from package source. Remote CI is not verified by this preparation.

[Candidate release notes](RELEASE-NOTES-1.26.3.md) summarize the patch. Public installation remains **1.26.2**. This step creates no tag, release, upload or signed public verification, and does not establish Braid adoption of 1.26.3.

| Verification | Fresh result |
|---|---|
| Release build | Zero errors; existing CS8604 conformance warning |
| Solution tests | 21,798 passed /0 failed /0 skipped |
| Separate LSP tests | 84 passed /0 failed /0 skipped |
| QT3 frozen gate | 31,142/0/679 |
| qt4 frozen gate | 8,436/0/1,894; all 224 configured sets; 11 existing Experimental fallback passes |
| Current-native Unix diagnostic | 22/2/0; expected exit 2, known cases 120/121 |
| Broad PackageReference-only consumer | 153 checks; all ten libraries |
| Focused PackageReference-only consumer | 11,175 passed /0 failed /0 skipped; nine libraries |
| Fresh dependency audits | Neither consumer reports vulnerable packages; official vulnerability endpoint HTTP 200; no NU1900 |

## Consolidated patch coverage

Since public 1.26.2, three suites add **3,061 regressions and controls**. Each is copied byte-for-byte from the pinned source into the external consumer; source and package counts agree by class.

| Test class | Cases | Scope |
|---|---:|---|
| AbsIntegerPreservationTests | 1,323 | Int64.MinValue, supported wide integers, numeric types/signs/special values, conversion/error controls |
| FloorCeilingIntegerPreservationTests | 936 | Supported wide integer identity, decimal/floating values, signed zero and special values |
| FloorCeilingArgumentContractTests | 802 | Cardinality, recursive arrays, untyped/node conversion, required errors, call forms and bounded static BC |

The focused consumer retains **8,114** prior checks, including the nine earlier numeric regression files, 47 public conditional lifecycle cases and 17 independent Braid probes. Those nine files remain byte-identical to their pinned source. Internal-construction authoring tests remain source-only. Counts overlap source/package and diagnostic runs and are not summed into conformance totals.

[Abs](xpath40-catalog/ABS-INTEGER-PRESERVATION.md) widens Int64.MinValue before negation and preserves existing decimal-backed integer identity. [Floor and ceiling](xpath40-catalog/FLOOR-CEILING-INTEGER-PRESERVATION.md) return existing integer values unchanged, including their supported wide representation. Plain decimals and floating calculations retain their behavior. The [argument audit](xpath40-catalog/FLOOR-CEILING-ARGUMENT-CONTRACT.md) passed on the unchanged engine; it required no implementation correction. The 802 audit controls are not a defect tally. Numeric storage and general constructors/casts are not expanded.

## Provenance and repeatability

All ten archives are **unsigned local 1.26.3 packages**, with RepositoryCommit `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f` and internal Bosak dependencies at 1.26.3. Consumers use PackageReferences exclusively, with no project or binary references. Separate initially empty caches and package-source mapping resolve Bosak.* only from the candidate feed; external dependencies restore from nuget.org.

All **19 consumed assembly identities** (ten broad, nine focused) have version **1.26.3.0** and informational revision **1.26.3+08fbdc31ac79baac04cf00f4773ec6d0b2dd247f**. Archive/cache/output DLL bytes match. Archive SHA-512, restore contentHash, cache metadata/source and .sha512 receipts agree. XML documentation assembly names and packaged README/license/icon/brand assets match the pinned source. No public signatures are inferred.

All ten official package indexes contain 1.26.2 and omit 1.26.3 at the recorded preflight. Both fresh transitive audits report no vulnerable packages; this is a dated result rather than a promise about later advisories.

[Retained evidence](release-evidence/1.26.3/) includes source/consumer plans, per-class coverage, archive and assembly provenance, raw test receipts, logs, raw/normalized audits, official endpoint responses, consumer source bundle and execution scripts. Raw bytes are protected from line-ending conversion. Complete outputs/caches remain in `.guard-tmp/release1263-candidate`, packages in `.guard-tmp/release1263-packages`, and the clean checkout in `.guard-tmp/release1263-source`.

To repeat package checks, extract the consumer source bundle, update both NuGet.Config candidate paths to the retained feed, and use separate empty NUGET_PACKAGES directories. Restore with the matching config and `--no-http-cache`; run broad/focused checks in Release with `--no-restore`. Set BOSAK_RELEASE_RECEIPT_DIR for broad assembly receipts. The retained scripts and raw TRXs record the executed checks.

Run `python docs/release-evidence/1.26.3/validate-evidence.py` for the portable ledger/source/receipt checks; add `--local-artifacts` for retained nupkgs or `--staged` for index-byte verification.

## Limits and publication boundary

REQ-126/D2/D3/D4/applicable-evidence AC-03 stay partial/open. Catalog inventory stays 76 entries /3,148 feature +24 chapter checks. Fixed numeric storage, general wide string constructors/casts, Int64 idiv result bounds, schema-list/general coercion, dynamic backwards-compatible conversion and DateTimeOffset Unix limits remain explicit. The native Unix diagnostic and existing fallback passes do not establish full current-draft conformance.

XSLT authoring is byte-unchanged since public 1.26.2. Braid's [previously supplied local adoption report](xpath40-catalog/evidence/abs-integer/braid-1.26.2-adoption-report.md) identifies public 1.26.2, 1,876 Release passes and overlapping signed-package smoke checks. Its captured uncommitted status, lack of current-adoption CI and absence of new native UI/accessibility acceptance remain explicit. Original report/provenance and all earlier candidate/publication/diagnostic receipts are untouched. No Braid edits or 1.26.3 adoption are inferred.

Next: review and the owner's release decision for this pinned source. Any separately authorized publication must use normal CI/signing and verify fresh signed public packages/consumers. Do not substitute a later documentation/worktree commit for the package source.
