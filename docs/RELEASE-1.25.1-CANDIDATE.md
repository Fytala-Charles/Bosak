<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.1 release-candidate evidence">
  <h1>Bosak 1.25.1 validated release candidate</h1>
  <p>24 October 2026 — unsigned local packages; unpublished</p>
</div>

**Publication completed:** [Verified 1.25.1 publication](RELEASE-1.25.1-PUBLICATION.md). The unsigned/unpublished statements below describe the retained pre-publication checkpoint.


**Candidate source: `6081e70daa8a49ef3d522b124a18d8de68e0b4d5`**, PR442 merge. Reviewed head `d46f9d2fb8d80393f775c7600dc5189ca2d49649` passed [all CI checks](https://github.com/Fytala-Charles/Bosak/actions/runs/40115516013); the complete candidate tree matches that reviewed head. No candidate tag, GitHub release or NuGet upload was created. Public/Braid1.25.0 remains accepted within REQ-137's delivered scope.

## Consolidated behavior

[Release notes](RELEASE-NOTES-1.25.1.md) cover Experimental signed Unix epochs, valid integer Unix calendar errors, checked folding/integer-preserving arithmetic, exact floating thresholds/unsigned magnitudes and unsigned numeric truncation before facets. Implementation PRs434/437/438/440/441/442 are merged; test/evidence PRs433/436/439 support the contracts. No new public API, layer, fixture, gate or retry policy is introduced. Version1.25.1 is the existing unpublished patch.

The XSLT authoring source is byte-unchanged relative to1.25.0 source `c005054bb096559368df985359e038e254814d12`. The shared-checkout REQ-138 request/registry additions and unrelated upstream notes remain outside this candidate. REQ-126/D2/D3/D4/AC-03 remain partial, with generic wide storage/error policy, unary/division audit work, decimal precision, bounded calendar and28 missing historical adopted pins explicit.

## Fresh validation at the exact candidate revision

| Check | Result |
|---|---|
| Release build | Passed; zero errors, existing CS8604 warning |
| Full solution |11,675 passed /0 failed /0 skipped |
| Separate language-server project |84 passed /0 failed /0 skipped |
| QT3 frozen gate |31,142 passed /0 failed /679 skipped |
| qt4 frozen gate |8,436 passed /0 failed /1,894 skipped;224 configured/executed sets |
| Current native Unix-time set |22 passed /2 failed /0 skipped; expected exit2 under frozen semantics |
| Fresh local-package broad consumer |153 checks passed; all ten libraries |
| Fresh local-package focused consumer |1,045 passed /0 failed /0 skipped; nine transitive libraries |
| Transitive vulnerability audits |Both fresh consumers report no vulnerable packages |

The431 added source cases are independently counted by class in the package consumer:52 Unix contract,25 Experimental Unix,4 host,24 wide-boundary,42 promotion,32 wide-range,72 constructor/cast controls,72 floating boundaries,48 unsigned Decimal and60 unsigned floating cases. They join614 retained focused checks. Counts overlap source tests and are not additive conformance totals. Skips and existing Experimental retries do not establish full4.0 support. The current native Unix failures are the two newer negative-domain cases120/121; direct Experimental tests verify the opt-in behavior separately.

## Package identity and provenance

All ten local1.25.1 archives carry the exact candidate repository commit and1.25.1 internal dependency versions. Both consumers use PackageReference only, fresh separate caches and source mapping that resolves Bosak.* exclusively from the local candidate feed. Version/informational revision and archive/cache/output DLL hashes agree for all19 consumed assembly identities. XML documentation and packaged README/license/icon/brand assets match source bytes. These archives are unsigned local candidates; no public repository-signature or public package-consumption verification is claimed.

The official NuGet vulnerability index was reachable with HTTP200, and the public Bosak.Xslt version index did not list1.25.1. An initial preflight could not decode the compressed vulnerability response; gzip decoding corrected that capture, with the original receipt retained. Neither test expectations nor dependency audit requirements were weakened.

[Machine-readable proof](RELEASE-1.25.1-CANDIDATE.json) records package hashes, dependency graphs, test counters, exact source and gate receipts. [Retained evidence](release-evidence/1.25.1/) includes per-project TRX hashes and native logs. Original local TRX, build/pack/restore/test/audit logs and fresh caches remain under `.guard-tmp/release1251-candidate`; archives are under `.guard-tmp/release1251-packages`.

## Publication boundary

This preparation changes documentation only after validating the immutable source. It does not tag or publish. Candidate documentation commits must not be mistaken for the package source above. A later authorized release should publish from this exact candidate revision, then independently verify signed public packages, provenance and fresh public consumption. Braid adoption/smoke for1.25.1 remains a later consumer step;1.25.0 acceptance is not reused as proof for this candidate.
