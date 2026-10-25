<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala repeated unary operator conversion and error correction">
  <h1>Repeated unary operators: conversion and errors</h1>
  <p>25 October 2026 - source 1.26.1 unpublished; public 1.26.0</p>
</div>

This bounded REQ-126 slice follows the [verified 1.26.0 publication](../RELEASE-1.26.0-PUBLICATION.md) at base `352622aa807760fc27e446f4f3656eb00a1b956b`. It removes the optimizer's unconditional `-(-x) => x` rewrite. Cancelling signs must preserve atomization, conversion, numeric base-type normalization and errors. Source is the new unpublished patch **1.26.1**. Public installation pins remain **1.26.0** while Braid independently validates that release.

## Contract and correction

[XPath 3.1 arithmetic evaluation](https://www.w3.org/TR/xpath-31/#id-arithmetic) atomizes operands, handles empty/multiple values, converts untyped atomic values to double and checks accepted operand types. [Numeric unary minus](https://www.w3.org/TR/xpath-functions-31/#func-numeric-unary-minus) returns a numeric base type and reverses floating signs, including signed zero. Backward-compatible arithmetic performs its specified numeric conversion. These established rules apply to the tested shared paths in XPath31, frozen XPath40 and XPath40Experimental; no newer draft contract is adopted.

| Expression | Before | Corrected |
|---|---|---|
| `-(-xs:untypedAtomic('2'))` | untyped atomic 2 | double 2 |
| `-(-(parse-xml('<n>3</n>')/n))` | node | double 3 |
| `-(-'2')` in a modern profile | string 2 | XPTY0004 |
| `-(-(1,2))` | sequence (1,2) | XPTY0004 |
| `-(-xs:untypedAtomic('not-a-number'))` | untyped text | FORG0001 |
| `-(-xs:short('7')) instance of xs:short` | true | false: numeric result is integer |
| `-(-true())` with backward compatibility enabled | boolean true | double 1 |

The correction removes only the unconditional pair-elimination block. Existing literal constant folds remain, including minimum-value promotion safeguards. Nonliteral double negation reaches the existing runtime twice; there is no new static type inference or runtime conversion path. Keeping both operations preserves intermediate effects that simply returning the operand, or substituting unary plus, would lose.

The patch changes no public API, integer storage, constructor/cast limits, decimal precision, fixture, gate membership or retry policy. General unary-plus subtype handling, duration extensions and exhaustive backwards-compatible literal folding remain outside this slice. No broad arithmetic/coercion certification is inferred.

## Verification

Adds **79 cases**: 24 value/error rows across three modern profiles (72), three subtype/signed-zero cases and four backward-compatible conversion cases. Each modern row checks direct syntax, an inline-function parameter and two separately invoked unary operations against explicit expected values/types or error codes. Coverage includes untyped element/attribute atomization, singleton arrays, empty/multiple values, string/boolean/URI/date refusals, invalid lexical conversion, integer storage boundaries, decimal/float/double, NaN/infinities and signed zero.

Before: **46 failed / 33 passed / 0 skipped**. After: **79 passed / 0 failed / 0 skipped**, with the same expectations. The original failing TRX is retained separately from the corrected receipt.

Release build succeeds with zero errors and the existing CS8604 conformance-runner warning. All **12,195 solution + 84 language-server tests pass (12,279 total)**. Fresh QT3 remains **31,142/0/679**; frozen qt4 remains **8,436/0/1,894**, across 224 unchanged configured/executed sets with the existing 11 Experimental fallback passes. The separately retained current-native Unix diagnostic remains **22/2/0, exit 2**, including known negative-domain failures 120/121. Skips and retries are not conformance acceptance.

[Machine-readable proof](UNARY-COERCION.json) records exact receipt hashes and original/corrected outcomes. [Retained evidence](evidence/unary-coercion/) includes the original/corrected focused TRXs, build/test/gate logs and hashes/counters for all solution/LSP TRXs. Complete raw TRXs remain under `.guard-tmp/req126-unary-coercion-evidence` to avoid duplicating the full suite's multi-megabyte output in Git. Documentation is rendered as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial. The 79 regressions are outside the unchanged 76-entry / 3,148 feature + 24 chapter documentation-test inventory. No upstream contribution or public release is made here. Braid 1.26.0 integration and native acceptance remain separate.

Next: review this patch; keep the public 1.26.0 release stable during Braid validation. Further generic integer storage/error and coercion coverage can proceed as separately bounded slices.
