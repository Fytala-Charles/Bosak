<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala integer modulo boundary correction">
  <h1>Integer modulo boundaries</h1>
  <p>25 October 2026 — source 1.25.2 unpublished; public 1.25.1</p>
</div>

**Integer division precision audit — 25 October 2026:** [Evidence and proposed scope](IDIV-PRECISION-AUDIT.md) add 48 result-range controls and 96 exact-rational comparisons, exposing 48 in-range one-unit discrepancies while documenting precision qualifications. Recommend exact truncation for existing decimal-backed operands with the current Int64 result bounds preserved. Engine/version unchanged; source 1.25.2 unpublished/public 1.25.1; REQ-126 partial.


This bounded REQ-126 correction follows the [division/modulo audit](DIVISION-MODULO-AUDIT.md), merged as PR446 at base `f69dc9c6a3de41b3e286dcaf158037ad12921ec9`. It repairs minimum-integer remainder overflow and preserves integer result identity when existing decimal backing is used. Source remains the existing unpublished patch 1.25.2; public 1.25.1 and independent Braid smoke/adoption remain separate.

| Expression | Before | Corrected result |
|---|---|---|
| `xs:integer('-9223372036854775808') mod -1` | FOAR0002 | integer zero |
| `-9223372036854775808 mod -1` | decimal zero | integer zero |
| `9223372036854775808 mod 3` | decimal 2 | integer 2 |
| `(0 - 9223372036854775808) mod 3` | decimal -2 | integer -2 |
| `9223372036854775808 mod 3.0` | decimal 2 | unchanged decimal 2 |

The Int64 branch checks a zero divisor first, then returns zero for minimum value modulo -1 without executing the overflowing CLR remainder instruction. All other Int64 inputs retain the existing remainder operation. Decimal-backed remainder uses the existing arithmetic result helper to retain integer identity only when both operands are integers. A genuine decimal operand keeps the result decimal, including an integral remainder. Float/double, untyped-atomic, empty-sequence and backwards-compatible dispatch branches are unchanged.

This follows the [F&O 3.1 modulo contract](https://www.w3.org/TR/xpath-functions-31/#func-numeric-mod), including the explicit minimum/-1 zero remainder case, and its [arithmetic type rules](https://www.w3.org/TR/xpath-functions-31/#op.numeric). The correction applies across XPath31, frozen XPath40 and XPath40Experimental as an existing numeric bug fix. It introduces no new API or newer-draft function contract.

## Verification

Adds 84 cross-profile cases, each checking direct evaluation and an inline function with runtime operands. They cover Int64 minimum and adjacent divisor signs, positive/negative wide operands and divisors, zero, unsignedLong maximum, both decimal-backed integer endpoints, genuine decimal/floating controls and zero-divisor errors. Before: 45 failed /39 passed. After: 84 passed, zero failures/skips, unchanged expectations.

Release build succeeds with zero errors and the existing CS8604 warning. All **11,879 solution +84 language-server tests pass (11,963 total)**. QT3 stays **31,142/0/679** and frozen qt4 stays **8,436/0/1,894** across 224 configured/executed gates, with 11 existing Experimental fallback passes. Current native Unix stays **22/2/0 (exit2)** under frozen evaluation. The independent 84-record probe changes exactly 21 modulo records; all 63 quotient/other controls remain identical. Skips/retries do not certify conformance. Eleven pages rendered as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance claimed.

[Machine-readable proof](INTEGER-MODULO.json) and [raw native/diagnostic/test receipts](evidence/integer-modulo/) retain original and corrected results. The same 84-record audit probe verifies the bounded change independently: corrected remainder records are identified separately; quotient and unrelated controls are compared with their original observations. Diagnostic completion is not a conformance assertion.

## Scope and next work

No version, public API, authoring source, fixture, gate membership, retry policy, upstream contribution or release change. `idiv` result range and precision, generic constructors/storage/error policy, double-negation/coercion and bounded decimal precision remain separate. No arbitrary-precision storage or complete arithmetic/current-draft conformance is claimed.

REQ-126/D2/D3/D4/AC-03 remain partial. Catalog totals stay 76 entries, 3,148 feature plus 24 chapter checks, 224 gates and 28 unavailable historical adopted pins; the 84 regressions are outside catalog totals. REQ-138 and Braid consumer adoption remain independent. Next: review this correction, then investigate exact `idiv` quotient computation and its explicit range policy separately. No release performed.
