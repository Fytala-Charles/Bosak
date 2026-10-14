<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.4.0 release preparation">
  <br><br><h1>Bosak 1.4.0</h1>
  <p>Private XSLT functions without a namespace</p>
</div>

## Release scope

**Prepared on 2026-10-14; unpublished.** This coordinated .NET 10 minor release adds private namespace-free XSLT 4.0 functions through [PR #284](https://github.com/Fytala-Charles/Bosak/pull/284). Published libraries remain 1.3.2 and the separately published VS Code extension remains 0.1.6. No public API or conformance gate membership is added.

- Accept unprefixed and `Q{}` function declarations under effective XSLT 4.0 with private visibility, including the default private visibility. Earlier declaration profiles and public/final/abstract visibility retain XTSE0740.
- Resolve unprefixed calls and named references to a matching no-namespace declaration by arity before falling back to fn. Explicit fn names bypass that preference; explicit Q{} names retain no namespace and never fall back.
- Support direct, arrow, keyword, named-reference, lookup and positional partial-application paths, including positional placeholders combined with keywords for the selected no-namespace host function.
- Retain optional/default argument behavior. Carry host signatures into runtime AVTs and match predicates, and captured signatures into authoring validation and supported expression edits. Original source bytes remain intact; conditional or ambiguous signatures report unsupported coverage.

For example, a private `sum#3` handles `sum(10,11,12)`, while the outer call in `sum(sum(10,11,12),0)` falls back to fn:sum#2 and returns 33. `divide(?, divisor := 3)` produces a function that returns 3 when called with 9. Includes share private scope; two focused non-optional used-package checks verify internal implementation isolation and absence from caller lookup.

The distinct availability rule is preserved: an unprefixed string passed to function-available names a standard fn function; use `'Q{}divide'` for the private function. See the [complete bounded contract](XSLT40-NO-NAMESPACE-FUNCTIONS.md).

## Verification

Integrated implementation `86c1547a62893fc5a555648d512ad7886e373f94` has successful [exact-head CI](https://github.com/Fytala-Charles/Bosak/actions/runs/38599962860). Solution tests pass **8,298/0/0**, including **5,412 XSLT tests**; the language server passes **77/0/0**. The 39 focused cases pass. Frozen QT3 remains exactly **31,142/0/679**, and qt4 gate-only remains **8,278/0/1,894**.

All ten local 1.4.0 packages pass version, dependency, repository revision, .NET 10 assets, XML documentation, README/license/icon and SHA-256 checks. A separate consumer with package references only and a fresh cache passes **120 checks**, verifies all ten assembly versions as **1.4.0.0**, and restores every Bosak package exclusively from the mapped local candidate feed. Cached archives match the candidate packages. This includes 79 previous-release checks and 41 checks for private names, arity fallback, explicit identities, partial keywords, visibility/profile errors, source-preserving edits, includes, package isolation, AVTs/predicates, conditional coverage and optional caller focus.

[Preparation evidence](RELEASE-1.4.0-PREPARATION.json) records exact revisions, package/content hashes, source/test trees, feed provenance and successful logs. This is local candidate verification. After the preparation documentation merges, repack and repeat consumer/content checks against the exact proposed release revision, comparing source/test trees with this receipt. Tagging and publication require the user's approval of that final revision.

## Boundaries and upstream status

General keyword placeholder handling for other functions is unchanged. Used-package optional ranges and optional xsl:override remain explicitly unsupported. Broader expose/accept/override linkage, schema constructor integration, conditional/shadow/ambiguous authoring graphs and general named-type/pattern/AVT replacement remain separate work. Existing decimal precision, bounded date/time conversion and remote HTTP limitations remain documented. REQ-126 remains In Progress.

The eight existing upstream cases function-40-014 through 021 fail against retained 1.3.2 and pass with the feature; their fixtures are unchanged from the recorded upstream base. The selected family improves from 28/8/363 to 36/0/363 on the pending PR #32 correction checkout. That sample includes the proposed 012/903 corrections and does not establish full conformance or upstream acceptance. No new test contribution is needed for these eight cases.

[PR #31](https://github.com/qt4cg/xslt40-test/pull/31) and [PR #32](https://github.com/qt4cg/xslt40-test/pull/32) are open without reviews at the preparation check; independent processor confirmation remains pending. Historical submission, acceptance and execution receipts are preserved. Any follow-up correction must use freshly fetched current upstream and be rebased if it advances, to avoid the merge conflicts Michael identified.
