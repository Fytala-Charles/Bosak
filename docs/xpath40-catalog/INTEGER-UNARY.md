<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala integer unary negation boundary correction">
  <h1>Integer unary negation boundaries</h1>
  <p>25 October 2026 — source 1.25.2 unpublished; public 1.25.1</p>
</div>

**Division/modulo audit — 25 October 2026:** [Evidence and next correction](DIVISION-MODULO-AUDIT.md) add 72 supported controls and 84 observed diagnostics. Minimum/-1 modulo overflow and wide remainder type loss are the recommended next bounded correction; quotient narrowing and precision remain separate. Engine/version unchanged: source 1.25.2 unpublished, public 1.25.1. REQ-126 remains partial.


This bounded REQ-126 slice follows the published [1.25.1 numeric corrections](../RELEASE-1.25.1-PUBLICATION.md), from base `3b419f1c259097cccc3cc9bfa46abe9ad59de211`. It fixes unary negation of Int64 minimum and preserves integer identity for existing decimal-backed integer values. Patch source is 1.25.2, unpublished. Braid's independent 1.25.1 smoke/adoption remains separate; no consumer acceptance is inferred.

| Expression | Before | Corrected result |
|---|---|---|
| `-xs:integer('-9223372036854775808')` | wraps to the negative minimum | `9223372036854775808`, integer |
| `-(-9223372036854775807 - 1)` | folded negation wraps | `9223372036854775808`, integer |
| `-9223372036854775809` | magnitude preserved, integer identity lost | `-9223372036854775809`, integer |
| `-xs:unsignedLong('18446744073709551615')` | decimal result | `-18446744073709551615`, integer |
| `-xs:decimal('9223372036854775808')` | genuine decimal | unchanged genuine decimal |

The optimizer leaves Int64-minimum negation for runtime, which promotes it to a decimal-backed integer. Negating a wide integer literal retains its `IsIntegerLiteral` marker. Runtime decimal-backed integers return an integer-tagged result; genuine decimals remain decimal. Integer-derived input results use the numeric base type rather than retaining an unsigned subtype after sign reversal. Existing double/float, untyped-atomic, empty-sequence and backwards-compatible dispatch remain unchanged.

The type and sign contract follows [F&O 3.1 numeric unary minus](https://www.w3.org/TR/xpath-functions-31/#func-numeric-unary-minus) and the [arithmetic type rules](https://www.w3.org/TR/xpath-functions-31/#op.numeric). This is an existing arithmetic correction across XPath31, frozen XPath40 and XPath40Experimental, not adoption of a newer draft-only function contract.

## Validation and limits

The 48-case regression matrix compares literal/folding-eligible and runtime expressions across all three modern profiles. It covers Int64 minimum/maximum, promoted negative/positive values, unsignedLong maximum, both decimal-backed integer endpoints, chained arithmetic, unary-plus controls, zero and genuine decimal/float/double controls. Original run: 24 failed /24 passed. Corrected run: 48 passed, zero failures/skips, unchanged expectations. Each passing case exercises both expression paths.

Initial build: zero errors and the existing CS8604 warning; final incremental build: zero warnings/errors. All **11,723 solution +84 language-server tests pass (11,807 total)**. QT3 stays **31,142/0/679**; frozen qt4 stays **8,436/0/1,894**, with 224 configured/executed gates and 11 existing Experimental fallback passes. Native Unix stays **22/2/0 (exit2)** under the frozen runner; only the newer negative-domain cases remain unresolved. Skips/retries do not certify conformance. Ten documentation pages rendered as HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

The first full-suite wrapper stopped on stderr from an older test that intentionally pinned the minimum-literal defect. Its log/interruption receipt is retained. That test now requires corrected integer identity, frozen negative-domain FOCA0002 and Experimental calendar FODT0001. Function policy is unchanged; no expectation was weakened. The final full suite completes successfully.

[Machine-readable evidence](INTEGER-UNARY.json) and [raw receipts](evidence/integer-unary/) retain before/after TRX counters and hashes, full test receipts and native gate logs. Counts overlap source coverage. No new public API, arbitrary-precision representation, fixture, gate membership, retry policy or upstream contribution change is introduced. No package release is performed.

Division/idiv/mod, double-negation elimination/coercion, general constructors/storage/error policy and bounded decimal precision remain separate work. This slice does not certify every unary operation or full current-draft conformance. REQ-126/D2/D3/D4/AC-03 remain partial: 76 catalog entries, 3,148 feature plus 24 chapter checks, 224 configured gates and 28 unavailable historical adopted pins. These 48 source regressions are outside the catalog check total. REQ-138 and Braid authoring source are outside this slice.

Next: review this correction, then audit integer division/modulo boundaries separately. Public/Braid dependency adoption remains on 1.25.1 until a separately authorized release.
