<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala unsigned floating truncation near zero">
  <h1>Unsigned floating truncation near zero</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 accepted</p>
</div>

This bounded REQ-126 correction follows the [unsigned Decimal fix](UNSIGNED-DECIMAL-TRUNCATION.md), merged as PR441 at base `7838c40cafd098983932f149210fdcf22b69c314`. It aligns finite unsignedLong float/double conversion with truncation toward zero before range checks. Source remains the existing unpublished1.25.1 patch; public/Braid1.25.0 remains consumer-accepted within REQ-137's delivered scope.

## Corrected behavior

After the existing NaN/infinity refusal, the unsignedLong floating branch now applies `Math.Truncate` before its lower and upper range checks. Float values widen to double and follow the same path. Numeric values in(-1,0) therefore become unsigned zero; negative zero and tiny negative subnormal values also become zero. A value of-1 or below still refuses after truncation.

| Numeric input | Before | After |
|---|---|---|
| float/double-0.5 | FORG0001 | 0 |
| representable float/double immediately above-1 | refused | 0 |
| float/double-1 or immediately below-1 | refused | still refused |
| negative zero | 0 | 0 |
| negative floating subnormal near zero | refused | 0 |
| NaN/infinity | FOCA0002 | FOCA0002 |

The exact binary upper limit2⁶⁴, the signed-backing transition at2⁶³ and UInt64-backed conversion of accepted large values remain intact. No overflow is admitted by truncation: representable floating values at those magnitudes are already integral. Positive fractional inputs still truncate toward zero. Lexical strings/untyped values retain their existing validation; fractional lexical forms do not become valid unsigned integer strings.

This follows the established numeric conversion order in [F&O3.1 integer casting](https://www.w3.org/TR/xpath-functions-31/#casting-to-integer) and [derived-type casting](https://www.w3.org/TR/xpath-functions-31/#casting-to-derived-types). [Castability](https://www.w3.org/TR/xpath-31/#id-castable) agrees with the cast result. Existing derived-range FORG0001 and nonfinite FOCA0002 classifications are retained; no broad error rewrite is introduced.

## Verification and disposition

Adds60 cases across XPath31, XPath40 and Experimental, each checking constructor, `cast as`, named reference and `castable`. Accepted outputs check exact magnitude, integer identity and unsignedLong identity. Cases cover(-1,0), both sides of-1, negative zero, subnormal inputs, positive fractional controls and nonfinite refusal. Before18 failed/42 passed; after60/0 with unchanged expectations. A separate36-record probe compares Decimal/Float/Double near zero and retains upper-bound controls.

Release build succeeded with zero errors; existing CS8604 and NU1900 vulnerability-metadata availability warnings remain. All **11,675 solution plus84 language-server tests pass (11,759 total)**. Fresh QT3 stays **31,142/0/679** and frozen qt4 stays **8,436/0/1,894** across224 configured/executed sets. Eleven documentation pages rendered as HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

The [machine-readable proof](UNSIGNED-FLOATING-TRUNCATION.json) records full validation; [raw gate/probe receipts](evidence/unsigned-floating-truncation/) preserve the evidence. Generic wide integer constructors/storage/error policy and unary/division audit work remain separate; no arbitrary-precision or calendar expansion is claimed.

No public API, source version, function metadata, compatibility enum, fixture, gate membership, retry or release change. Current native Unix stays22/2/0 under frozen evaluation, with only the two newer negative-domain cases unresolved there. Skips and existing Experimental retries do not certify full conformance.

Catalog totals remain76 entries,3,148 feature plus24 chapter checks,224 gates and28 missing historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. Next: review/merge this correction, then consolidate1.25.1 release-candidate validation before expanding integer storage semantics. No package release performed. Shared-checkout REQ-138 draft/registry and pending upstream notes remain untouched.
