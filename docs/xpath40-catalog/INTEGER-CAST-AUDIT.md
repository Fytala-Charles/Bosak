<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala integer constructor and cast boundary audit">
  <h1>Integer constructor and cast boundary audit</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0 accepted</p>
</div>

**Unsigned Decimal truncation — 24 October 2026:** [Correction and limits](UNSIGNED-DECIMAL-TRUNCATION.md) discard numeric fractions before unsignedLong range facets/backing selection, with48 constructor/cast/reference/castable cases. Fractional lexical forms still refuse; floating near-zero consistency remains separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



**Exact floating integer boundaries — 24 October 2026:** [Correction and limits](FLOATING-INTEGER-BOUNDARIES.md) reject the signed2⁶³/unsigned2⁶⁴ overflow thresholds and preserve accepted large unsigned magnitudes, with72 constructor/cast/reference/castable cases. Unsigned Decimal truncation and generic storage remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 partial.



This bounded REQ-126 audit follows the [Unix-time correction](UNIX-DATETIME-WIDE-RANGE.md), merged as PR438 at base `fadaa44631a0416f24406113e7ab6b3946feb13d`. It adds72 supported conversion tests and retains150 observed diagnostics across XPath31, XPath40 and XPath40Experimental. Engine/API/version, fixtures, gates and retry policy are unchanged. Source1.25.1 remains unpublished; public/Braid1.25.0 is consumer-accepted within REQ-137's delivered profile.

## Contract distinctions

[F&O3.1 §19.1.2.4](https://www.w3.org/TR/xpath-functions-31/#casting-to-integer) preserves integer values, truncates finite noninteger numerics toward zero and specifies FOCA0003 when a numeric integer result exceeds implementation limits. NaN and infinities use FOCA0002. [§19.2](https://www.w3.org/TR/xpath-functions-31/#casting-from-strings) distinguishes lexical validation and numeric representation limits; derived facets require separate analysis. [XPath3.1 §3.14.3](https://www.w3.org/TR/xpath-31/#id-castable) makes castability a boolean test of cast success, while operand evaluation/atomization errors still propagate. These established3.1 rules guide the shared audit; no blanket adoption of a newer draft is inferred.

The implementation currently has different effective limits: wide literals and unsignedLong can use decimal backing, whereas generic integer constructors/casts narrow to Int64. An implementation storage limit, a malformed lexical value and a derived-type facet failure are separate concerns. Not every observed FORG0001 is therefore the same bug, and no global error-code replacement is proposed.

## Observed boundaries

The132-record main probe and18-record unsigned follow-up give identical values/errors across the three profiles for each expression. They are observations, not passing conformance assertions.

| Boundary | Observed result |
|---|---|
| `9223372036854775808 instance of xs:integer` | true |
| Constructor/string cast of that value | FORG0001; generic parser uses Int64 |
| Numeric cast of that valid wide integer | FORG0001 on decimal-to-Int64 narrowing |
| The same numeric/string inputs with `castable` | false; no overflow exception escapes |
| `xs:integer(xs:double('9223372036854775808'))` | **9223372036854775807**, silently wrong at2⁶³ on this .NET10 host |
| Next representable double,9223372036854777856 | FOCA0003; castable false |
| Exact negative double minimum,-9223372036854775808 | accepted exactly |
| `xs:unsignedLong(xs:double('18446744073709551616'))` | **18446744073709600000**, above the permitted maximum; castable true |
| Next representable double,18446744073709555712 | FORG0001; rejected |
| `xs:unsignedLong(18446744073709551614.5)` | fractional magnitude retained while `instance of xs:integer` is true |
| `xs:unsignedLong(18446744073709551615.5)` | rejected before discarding fraction |

The numeric narrowing branches compare doubles with `long.MaxValue` or `ulong.MaxValue`; converting those maxima to double rounds them to2⁶³ or2⁶⁴. A strict greater-than comparison therefore admits the exact out-of-range threshold. The subsequent .NET conversion result is host-observed, not a guaranteed portable saturation rule. This is a source-backed explanation of the reproduced wrong values.

The special unsignedLong Decimal branch neither discards fractional parts before applying facets nor truncates the returned decimal-backed result. This is a separate defect from floating-point representability guards. Generic string/untyped conversion uses `long.TryParse`; generic Decimal conversion narrows to long before its derived-range check. The broad `Cast` overflow handler reports FORG0001 for integer-family overflow. `Castable` already catches both dynamic InvalidOperationException and OverflowException and returns false; no new broad catch is needed.

The general wide storage policy remains unresolved: supporting decimal-backed literals and arithmetic does not establish full arbitrary-precision constructors or support in every integer consumer. Narrowing failures must not be disguised by the earlier Unix-time boundary correction, which intentionally leaves constructors/casts separate.

## Validation and next slice

The72 new cases cover exact Int64 extrema, signed lexical whitespace, numeric truncation, boolean/untyped conversion, narrow derived-type maxima, unsignedLong identity, malformed lexical forms, nonfinite errors and castability/cardinality controls. They do not encode saturation, fractional integer tagging or disputed wide errors as desired behavior. [Machine-readable audit](INTEGER-CAST-AUDIT.json) and [diagnostic/native receipts](evidence/integer-cast-audit/) retain the evidence.

Release build succeeded with zero errors and the existing CS8604 warning. All **11,495 solution plus84 language-server tests pass (11,579 total)**. Nine documentation pages rendered as HTML with unchanged Fytala CSS; no visual/PDF acceptance claimed.

Fresh Unix native sets remain frozen16/0/0 and current22/2/0 exit2. Only newer negative-domain cases remain under frozen execution. Full manual gates were not rerun for this tests/docs-only slice; the engine-identical [PR438 receipts](UNIX-DATETIME-WIDE-RANGE.json) retainQT3 31,142/0/679 and frozen qt4 8,436/0/1,894 across224 sets. Skips/retries do not certify full conformance.

Recommended next implementation: correct the floating-point-to-integer boundary guards for signed Int64 and unsignedLong using exact binary thresholds, test their adjacent representable values and castability, and run both frozen gates. Keep unsigned decimal truncation/facet order and wider constructor/storage support as separate follow-ups. Correcting silent wrong numeric results comes before expanding storage or changing shared error classification.

Catalog totals remain76 entries,3,148 feature plus24 chapter checks,224 gates and28 missing historical adopted pins. REQ-126/D2/D3/D4/AC-03 remain partial. No engine/API/version/release change or new upstream contribution.
