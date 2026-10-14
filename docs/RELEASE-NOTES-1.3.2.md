<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.3.2 release preparation">
  <br><br><h1>Bosak 1.3.2</h1>
  <p>Optional XSLT function arguments and bounded numeric coercion</p>
</div>

## Release scope

**Prepared on 2026-10-14; unpublished.** This coordinated .NET 10 release carries the optional/default feature introduced in source 1.3.0 and its 1.3.1/1.3.2 corrections. Published libraries remain 1.2.1; the separately published VS Code extension remains 0.1.6. No new public API or conformance gate membership is introduced.

- Ordinary XSLT 4.0 functions accept trailing `required="no"` parameters with select, sequence-constructor or empty defaults. Direct, keyword and arrow calls support omissions; reduced-arity references, lookup and positional partial application retain the documented creation focus. Explicit `()` remains a supplied argument. Supplied values skip defaults.
- Defaults use declaration namespaces, base URI and globals with caller focus. Function bodies retain absent focus. Authoring signatures, validation and supported expression edits agree with execution and preserve original source bytes.
- Invalid default conversion reports `XPTY0004`; errors raised while evaluating defaults retain their identity.
- XPath 4.0 and ordinary XSLT 4.0 function arguments allow the specified primitive decimal/float/double casts, including defaults. Double-to-integer and combined promotion/relabeling remain invalid; earlier profiles keep their established behavior. Non-finite decimal casts retain `FOCA0002`.

The detailed contracts are [optional arguments](XSLT40-OPTIONAL-FUNCTION-ARGUMENTS.md), [default errors](XSLT40-DEFAULT-ARGUMENT-ERRORS.md) and [primitive numeric coercion](XSLT40-PRIMITIVE-NUMERIC-COERCION.md).

## Verification

Implementation revision `67644ba38dce654356c81aae1fdf60266bca87fc` has successful exact-head implementation CI. Solution tests pass 8,259/0/0, including 5,373 XSLT tests; language-server tests pass 77/0/0. Frozen QT3 remains 31,142/0/679 and qt4 gate-only remains 8,278/0/1,894. The optional/default family passes 61 cases and primitive numeric coverage passes 29 cases, with retained pre-fix baselines.

All ten local 1.3.2 packages pass version, dependency, repository revision, .NET 10 assets, XML documentation, README/license/icon and SHA-256 checks. A separate consumer with package references only and a fresh package cache passes 79 checks, verifies ten assembly versions as 1.3.2.0, and restores every Bosak package exclusively from the candidate feed. This includes the 60 earlier release checks plus 19 optional/numeric/error compatibility checks. No publication has been performed. [Preparation evidence](RELEASE-1.3.2-PREPARATION.json) identifies the exact candidate, package hashes, feed provenance and excluded preliminary runs.

Before tagging a reviewed documentation merge, rebuild and repeat package/consumer verification for that exact revision; compare source/test trees with this receipt. A successful local consumer does not establish marketplace or nuget.org publication.

## Boundaries and upstream status

Optional ranges in used packages and optional `xsl:override` remain explicit unsupported profiles. Broader package linkage, keyword placeholders, conditional/shadow/ambiguous authoring graphs, named-type expression-edit compilation and general pattern/AVT replacement remain separate work. General host variable/return conversion and derived numeric relabeling are outside the bounded numeric correction. Fixed decimal precision, bounded date/time conversions and remote HTTP test limitations remain documented.

[Upstream PR #31](https://github.com/qt4cg/xslt40-test/pull/31) and [PR #32](https://github.com/qt4cg/xslt40-test/pull/32) are open with no reviews at the preparation check. Acceptance and independent processor confirmation remain pending. PR #32 retains the original 903 fixture and corrects its error expectation; historical evidence is preserved. Corrections must be submitted against freshly fetched current upstream, rebasing if it advances, to avoid the merge conflicts Michael identified.

Tagging and publication require the user's approval of the final verified release revision.
