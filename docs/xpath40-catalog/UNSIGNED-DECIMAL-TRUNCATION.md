<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala unsigned decimal truncation and range facets">
  <h1>Unsigned decimal truncation and range facets</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 accepted</p>
</div>

**Unsigned floating truncation — 24 October 2026:** [Correction and limits](UNSIGNED-FLOATING-TRUNCATION.md) truncate finite float/double operands before unsigned range checks, allowing(-1,0) to become unsigned zero. Adds60 constructor/cast/reference/castable cases; exact overflow thresholds and nonfinite errors remain intact. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



This bounded REQ-126 fix follows the [floating integer correction](FLOATING-INTEGER-BOUNDARIES.md), reviewed at `d0b8f35b2a8b6cea2ef768b9bbf4e74f76010f9b` (PR440 merged at `acc56ae1a5962f2361825d9b6d0ad470a598163c`). It corrects the unsignedLong Decimal conversion branch across XPath31, XPath40 and Experimental, without expanding storage or changing lexical conversion. Source remains the existing unpublished1.25.1 patch; public/Braid1.25.0 remains accepted within REQ-137's delivered scope.

## Conversion order

The branch now computes `decimal.Truncate(value.DecimalValue)` before testing the unsignedLong0–18446744073709551615 range and choosing Int64 or decimal backing. Numeric values lose their fraction toward zero; the range checks and returned magnitude use that same truncated value. This prevents fractional values carrying an integer tag and permits values whose truncated result lies inside the range.

| Numeric Decimal input | Before | After |
|---|---|---|
| -0.5 | rejected | 0 |
| 9223372036854775807.9 | fractional decimal-backed result | exact9223372036854775807 |
| 9223372036854775808.9 | fractional integer-tagged result | exact9223372036854775808 |
| 18446744073709551614.5 | fraction retained with integer identity | exact18446744073709551614 |
| 18446744073709551615.5 | rejected before truncation | exact18446744073709551615 |
| -1.1 or18446744073709551616 | rejected | still rejected |

Numeric conversion order follows [F&O3.1 integer conversion](https://www.w3.org/TR/xpath-functions-31/#casting-to-integer) and [derived-type casting](https://www.w3.org/TR/xpath-functions-31/#casting-to-derived-types). Lexical strings/untyped values follow their existing validation: `'1.9'` remains an invalid unsignedLong lexical form, and numeric truncation is not applied to it. Rejected derived-range and lexical values keep their existing FORG0001 classification. [Castability](https://www.w3.org/TR/xpath-31/#id-castable) agrees with the conversion result.

## Verification and remaining work

Adds48 cases across three explicit profiles, each exercising constructor, `cast as`, named function reference and `castable`. Exact outputs and integer identity cover zero, fractions near zero, signed-backing transition, maximum unsigned value and just-outside bounds; lexical controls remain separate. Before15 failed/33 passed; after48/0 with unchanged expected results.

Release build succeeded with zero errors and the existing CS8604 warning. All **11,615 solution plus84 language-server tests pass (11,699 total)**. Fresh QT3 stays **31,142/0/679** and frozen qt4 stays **8,436/0/1,894** across224 configured/executed sets. Eleven documentation pages rendered as HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

The [machine-readable proof](UNSIGNED-DECIMAL-TRUNCATION.json) and [raw gate/diagnostic receipts](evidence/unsigned-decimal-truncation/) retain full validation and a36-record comparison. The prior floating-point thresholds and accepted large floating magnitudes remain corrected by PR440.

The separate floating branch still rejects float/double values in(-1,0) before their fractions can be discarded; the probe explicitly records decimal-0.5 returning0 while float/double-0.5 refuse. This is a remaining conversion inconsistency, not a newly passing assertion. General wide constructors/storage/error classification and unary/division audits remain separate; no arbitrary-precision support is claimed.

No public API, source version, function metadata, compatibility enum, fixture, gate membership, retry or release change. Current native Unix stays22/2/0 under frozen execution, with only the newer negative-domain cases unresolved there. Skips and existing Experimental retries do not certify full conformance.

Catalog totals remain76 entries,3,148 feature plus24 chapter checks,224 gates and28 missing historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. Next: review this correction, then align unsigned floating fractional conversion near zero in a separate slice before broader integer storage/error work. Shared-checkout REQ-138 draft/registry and upstream notes remain untouched.
