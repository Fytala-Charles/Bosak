<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala exact integer division truncation correction">
  <h1>Exact integer division truncation</h1>
  <p>25 October 2026 — source 1.25.2 unpublished; public 1.25.1</p>
</div>

**Follow-up:** The numeric corrections below are published in [1.26.0](../RELEASE-1.26.0-PUBLICATION.md). The next bounded [unary conversion correction](UNARY-COERCION.md) is implemented in unpublished 1.26.1. Earlier candidate/next-step statements below are historical.


This bounded REQ-126 correction implements the [precision/range investigation's proposal](IDIV-PRECISION-AUDIT.md), merged as PR448 at base `31ea265321107f94a2c8486218b3746d200d214b`. Decimal-backed `idiv` now truncates the exact quotient before applying the existing Int64 result bounds. Source remains the existing unpublished patch 1.25.2; public 1.25.1 and independent Braid smoke/adoption remain separate.

| Operands | Before | Corrected result |
|---|---|---|
| `79228162514264337593543950334 idiv 79228162514264337593543950335` | 1 | integer 0 |
| negative first operand / positive second operand at those magnitudes | -1 | integer 0 |
| `79228162514264337593543950333 idiv 39614081257132168796771975167` | 2 | integer 1 |
| `xs:decimal('7.9228162514264337593543950334') idiv xs:decimal('7.9228162514264337593543950335')` | 1 | integer 0 |
| `9223372036854775808 idiv 1` | FOAR0002 | FOAR0002 |

The decimal path validates the zero divisor, extracts signed coefficients/scales into stack-allocated bit buffers, aligns scales using bounded BigInteger temporaries, and divides those integers with truncation toward zero. The exact quotient must fit inclusive Int64 minimum/maximum before returning the existing integer representation. Decimal coefficients are 96 bits and scale differences are at most 28; temporary magnitudes stay below 2^190. XDM storage and accepted operand representation are unchanged.

Zero divisors retain FOAR0001 precedence. Out-of-range results retain FOAR0002 and the existing exception type; the decimal-backed overflow message is standardized to `Integer division overflow.`. Ordinary `div`, modulo, the Int64-only branch, float/double, untyped-atomic and backwards-compatible dispatch are unchanged. No API, arbitrary-precision result storage or broad decimal precision expansion is introduced.

The [F&O 3.1 integer division rules](https://www.w3.org/TR/xpath-functions-31/#func-numeric-integer-divide) retain their implementation-precision qualifications. This correction adopts the explicitly reviewed exact-truncation improvement for existing decimal-backed operands across XPath31, frozen XPath40 and XPath40Experimental. It does not certify every numeric operation or current-draft contract.

## Verification

Adds 132 cross-profile cases, each exercising direct evaluation and inline-function runtime operands. The first 96 use the investigation's exact rational reference and existing range policy. Additional cases cover scale differences of both signs through 28, trailing-zero scales, negative zero, tiny divisors, zero-divisor precedence, maximum decimal magnitude, floating/untyped dispatch controls and the retained Int64 minimum/-1 overflow. Before: 48 failed /84 controls passed. After: 132 passed, zero failures/skips, unchanged expectations. The 48 prior range controls remain intact.

The same independent 96-record oracle probe now matches the exact reference/range policy throughout: 48 precision records corrected, 36 accepted controls byte-identical and 12 overflow records retaining FOAR0002 with the standardized message. All three profiles agree. Diagnostic comparisons are separate from native conformance counts.

Release build succeeds with zero errors and the existing CS8604 warning. All **12,059 solution +84 language-server tests pass (12,143 total)**. QT3 stays **31,142/0/679** and frozen qt4 stays **8,436/0/1,894** across 224 configured/executed gates, with 11 existing Experimental fallback passes. Current native Unix stays **22/2/0 (exit2)** under frozen evaluation. All 96 oracle observations match exact truncation plus the retained range policy. Skips/retries do not certify conformance. Eleven pages rendered as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance claimed.

[Machine-readable proof](EXACT-IDIV.json) and [raw gate, oracle and test receipts](evidence/exact-idiv/) preserve original and corrected observations. Original failing TRX/logs remain retained under `.guard-tmp/req126-idiv-fix-evidence`.

## Remaining work

No version, public API, authoring source, fixture, gate membership, retry policy, upstream contribution or release change. Generic constructors/storage/error policy, double-negation/coercion, broader decimal precision and current-draft adoption remain separate. REQ-126/D2/D3/D4/AC-03 remain partial: 76 catalog entries, 3,148 feature plus 24 chapter checks, 224 gates and 28 unavailable historical adopted pins. These 132 regressions are outside catalog totals. REQ-138 and Braid adoption are independent.

Next: review this correction and consolidate the unpublished 1.25.2 regression/package candidate before expanding arithmetic scope. Publication requires a separate release step; no release performed here.
