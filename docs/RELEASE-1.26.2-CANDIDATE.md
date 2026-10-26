<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.2 source and package verification">
  <h1>Bosak 1.26.2 consolidated release candidate</h1>
  <p>26 October 2026 — unsigned local candidate; unpublished</p>
</div>

**Publication completed:** [Verified 1.26.2 publication](RELEASE-1.26.2-PUBLICATION.md). Candidate/unpublished statements below retain the pre-publication checkpoint; original candidate JSON and evidence remain unchanged.


**Pinned package source: `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`.** The candidate consolidates both unpublished rounding slices. A clean isolated checkout supplies the build, tests and ten packages. [Machine-readable proof](RELEASE-1.26.2-CANDIDATE.json) pins its complete tree, RepositoryCommit and assembly informational revision. Candidate documentation commits remain separate from the package source. Remote CI is not verified by this preparation.

[Candidate release notes](RELEASE-NOTES-1.26.2.md) summarize the patch. Public installation remains **1.26.1**. No tag, GitHub release, package upload or signed public verification occurs here. The owner reports that Braid's smoke test finished; its outcome/acceptance has not been supplied.

| Verification | Fresh result |
|---|---|
| Release build | Zero errors; existing CS8604 conformance warning |
| Solution tests | 18,737 passed /0 failed /0 skipped |
| Separate LSP tests | 84 passed /0 failed /0 skipped |
| QT3 frozen gate | 31,142/0/679 |
| qt4 frozen gate | 8,436/0/1,894; all 224 configured sets executed; 11 existing Experimental fallback passes |
| Current-native Unix diagnostic | 22/2/0; expected exit 2, known cases 120/121 |
| Broad PackageReference-only consumer | 153 checks; all ten libraries |
| Focused PackageReference-only consumer | 8,114 passed /0 failed /0 skipped; nine libraries |
| Fresh dependency audits | Neither consumer reports vulnerable packages; official vulnerability endpoint HTTP 200; no NU1900 |

## Consolidated patch coverage

Since public 1.26.1, the two rounding slices add **4,201 regressions and controls**. Both new test files are copied byte-for-byte from the pinned source into the external focused consumer. Source and package case counts agree by class.

| Test class | Cases | Scope |
|---|---:|---|
| RoundingPrecisionBoundaryTests | 1,321 | Extreme precision, supported wide integer payloads, numeric identity, signs, special values and required errors |
| RoundingKernelTests | 2,880 | Exact decimal/binary rounding, ties, subnormals, integer result widening and final storage overflow |

The focused consumer retains its previous **3,913** checks, including the prior 2,420 source regressions, 47 public conditional lifecycle cases and 17 independent Braid conditional probes, then adds all 4,201 new cases. The prior seven regression files remain byte-identical too. Existing internal-construction authoring tests remain source-only. Source, consumer and diagnostic counts overlap and must not be summed into conformance totals.

[Extreme precision](xpath40-catalog/ROUNDING-PRECISION-BOUNDARIES.md) now avoids narrowing/power hazards, preserving primitive numeric types and floating signs/special values. [Exact kernels](xpath40-catalog/ROUNDING-KERNELS.md) round decimal coefficients and exact IEEE binary rationals without overflowing intermediate scales, then convert once to the destination format. Integer results beyond Int64 use existing decimal-backed integer storage; results beyond supported storage retain FOAR0002.

The kernel evidence corrects an earlier interpretation: 612 historical predicate discrepancies comprised **600 actual rounding discrepancies** and **12 wrong expectations** for `1e300` at precision `-29`. Original receipts remain untouched and corrections are recorded separately. Both corrected diagnostic matrices are green: 1,440 independent-oracle observations (555 corrections) and 3,036 historical-matrix observations (600 corrections), with no remaining discrepancy or sampled regression. These matrices overlap each other and source/package tests.

## Package provenance and repeatability

All ten archives are **unsigned local 1.26.2 packages**. Their RepositoryCommit is the pinned revision and all internal Bosak dependencies use 1.26.2. Both consumers have exclusively PackageReferences, no project/binary references, separate initially empty package caches, and source mapping that resolves Bosak.* only from the local candidate feed. External test dependencies restore from nuget.org.

All **19 consumed assembly identities** (ten broad, nine focused) have version **1.26.2.0** and informational revision **1.26.2+6a3372fc3b91e733b71a426cf40f26feaaeb3bcb**. Archive/cache/output DLL bytes match. Archive SHA-512, restore contentHash, cache metadata/source and .sha512 receipts agree. XML documentation names and packaged README/license/icon/brand assets match pinned source bytes. These checks do not establish public package signatures.

All ten official package indexes contain public 1.26.1 and omit 1.26.2 at the recorded preflight. Neither fresh transitive audit reports vulnerable packages. This is a dated audit result. The initial network-restricted endpoint attempt is retained separately; successful endpoint receipts and actual fresh audits establish the reported result.

[Retained evidence](release-evidence/1.26.2/) contains plans, source/consumer test receipts, per-class coverage, archive/assembly provenance, original logs, normalized audit JSON, official endpoint responses and a consumer source bundle. Raw bytes are protected from line-ending conversion. Full outputs and caches remain in `.guard-tmp/release1262-candidate`; packages remain in `.guard-tmp/release1262-packages`; the clean pinned checkout is `.guard-tmp/release1262-source`.

To repeat package checks, extract the consumer source archive, update both NuGet.Config candidate paths to the retained package feed, and use separate empty NUGET_PACKAGES directories. Restore with the corresponding config and `--no-http-cache`, run broad/focused checks in Release with `--no-restore`, and set BOSAK_RELEASE_RECEIPT_DIR for broad assembly receipts. Executed source/consumer/gate scripts and raw TRXs are retained.

Run `python docs/release-evidence/1.26.2/validate-evidence.py` to verify the ledger, source tree, raw receipts, consumer sources and case counts; add `--local-artifacts` for retained nupkgs and `--staged` to compare evidence against the Git index.

## Remaining limits and next step

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial/open; catalog inventory remains 76 entries /3,148 feature +24 chapter checks. Fixed-precision decimal and decimal-backed integer storage, general wide string constructors/casts and wide abs, Int64 idiv result bounds, schema-list cardinality and broader metadata/coercion remain separate. DateTimeOffset-based Unix operations retain years 1–9999. The current-native Unix 22/2/0 and existing fallback passes do not certify full current-draft conformance.

The XSLT authoring implementation is byte-unchanged since public 1.26.1. Public conditional probes and the reported completion of Braid's smoke test do not establish a supplied outcome, new adoption or native acceptance.

Next is review and the owner's release decision for pinned source `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`. Any separately authorized publication must use normal signing/publishing and fresh signed public package consumers. No current documentation/worktree commit is substituted for the pinned package source.
