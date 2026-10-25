<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala integer division precision and result-range investigation">
  <h1>Integer division precision and result range</h1>
  <p>25 October 2026 — source 1.25.2 unpublished; public 1.25.1</p>
</div>

**Exact idiv truncation — 25 October 2026:** [Correction and evidence](EXACT-IDIV.md) compute exact decimal-backed quotients before applying the existing Int64 bounds. Adds 132 direct/runtime regressions (48 failed before, all pass after); all 96 oracle observations now match the exact reference/range policy. FOAR0001/FOAR0002 codes, other dispatch and storage bounds remain intact. Source 1.25.2 unpublished/public 1.25.1; REQ-126 partial.


This REQ-126 investigation follows the [modulo correction](INTEGER-MODULO.md), merged as PR447 at `40555175ef59a826994bc8f6d29c35018837eeaf`. It adds 48 supported result-range controls and 96 observations compared with an independent exact rational reference. Engine, version, APIs, fixtures, gates, retry policy and public packages are unchanged. Braid 1.25.1 smoke/adoption remains separate.

## Findings

The decimal-backed `idiv` path currently computes a rounded .NET decimal division, then casts the result to Int64. A quotient very close to an integer can therefore cross the truncation boundary before the cast. This is independent of whether the exact quotient would fit Int64.

| Operands | Exact truncation | Observed |
|---|---|---|
| `79228162514264337593543950334 idiv 79228162514264337593543950335` | 0 | 1 |
| negative first operand / positive second operand for the same magnitudes | 0 | -1 |
| `79228162514264337593543950333 idiv 39614081257132168796771975167` | 1 | 2 |
| `79228162514264337593543950334 idiv 26409387504754779197847983445` | 2 | 3 |
| `xs:decimal('7.9228162514264337593543950334') idiv xs:decimal('7.9228162514264337593543950335')` | 0 | 1 |

All eight positive operand pairs are evaluated with four sign combinations and three modern profiles (96 observations). The exact reference uses Python standard-library `Fraction`, then integer division of absolute numerator/denominator and sign restoration to truncate toward zero. Every supplied operand is exactly representable by the existing decimal backing. The reference applies the existing Int64 result bounds separately: an exact quotient outside [-2^63, 2^63-1] maps to the existing FOAR0002 policy rather than assuming storage expansion.

Forty-eight observations match that reference/policy; 48 in-range observations differ by one from exact truncation. All three profiles agree. The first run uses reviewed PR447 head `08fd77d06cb41b9c46c04ff4881762e59cceea85`; the second uses the identical merged engine source. Results and reference comparisons are retained independently. These are mathematical comparisons, not a conformance pass/fail tally.

The [F&O 3.1 integer division rules](https://www.w3.org/TR/xpath-functions-31/#func-numeric-integer-divide) explicitly qualify behavior by implementation precision and overflow/underflow. Accordingly this evidence does not classify every precision discrepancy as a specification violation. It identifies a bounded consistency improvement that can be reviewed separately from general decimal precision and representation.

## Existing range controls

The 48 passing controls cover signed Int64 endpoints, decimal fractional quotients that truncate into range, out-of-range refusal, negative-one division, wide dividends with an in-range quotient, zero with a tiny decimal divisor, and float/double thresholds at signed 2^63. Negative 2^63 remains included; positive 2^63 refuses. The immediately smaller representable positive float/double values remain accepted. Values check exact text and integer identity; errors require FOAR0002. These controls preserve the current bounded result policy while the precision observations remain diagnostic.

## Recommended next bounded contract

1. Compute a quotient by exact truncation for the existing finite decimal/integer operands represented by .NET decimal, using their integer coefficients and scales.
2. Apply the existing inclusive Int64 minimum / inclusive maximum check to that exact truncated quotient; retain FOAR0002 outside it.
3. Retain zero-divisor FOAR0001 precedence and the existing float/double, untyped-atomic and backwards-compatible dispatch behavior.
4. Keep generic constructors, XDM representation, public APIs, ordinary `div`, modulo and general precision policy outside the correction.

Temporary bounded coefficient/scale arithmetic would leave XDM storage unchanged: decimal coefficients are 96 bits and scales are at most 28. An internal BigInteger calculation is one implementation option. Exact computation would be an explicit improvement over the current rounded intermediate; source 1.25.2 remains unpublished until a separately authorized release. This audit supplies evidence and the proposed scope; it makes no engine change.

## Verification and disposition

Release build succeeds with zero errors and the existing CS8604 warning. All **11,927 solution +84 language-server tests pass (12,011 total)**, including **48/0/0** result-range controls. Both 96-record observation runs match, each with 48 exact-reference matches and 48 in-range one-unit discrepancies. Eleven pages rendered as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance claimed.

[Machine-readable proof](IDIV-PRECISION-AUDIT.json) and [raw inputs, observations, oracle comparisons and test receipts](evidence/idiv-precision-audit/) preserve reproducible evidence. This test-only audit inherits PR447's exact unchanged engine/harness evidence: QT3 31,142/0/679 and frozen qt4 8,436/0/1,894 across 224 configured/executed gates, with 11 existing Experimental fallback passes. Manual gates are not rerun here; the source comparison is explicit. Current native Unix remains 22/2/0 under frozen evaluation. Skips/retries and diagnostic counts do not certify conformance.

REQ-126/D2/D3/D4/AC-03 stay partial. Catalog totals remain 76 entries, 3,148 feature plus 24 chapter checks, 224 gates and 28 unavailable historical adopted pins; these 48 controls are outside catalog totals. General constructors/storage/errors, double-negation/coercion, arbitrary-precision support and full current-draft adoption remain separate. REQ-138 and Braid authoring are outside this slice. Next: review this bounded exact-quotient proposal, then implement it with failing-before/passing-after regressions and fresh engine gates. No release performed.
