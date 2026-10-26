<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala extreme rounding precision audit and correction">
  <h1>Extreme rounding precision and retained kernel findings</h1>
  <p>26 October 2026 — source 1.26.2 unpublished; public 1.26.1 unchanged</p>
</div>

**Subsequent correction — 26 October 2026:** [Exact rounding kernels and expectation correction](ROUNDING-KERNELS.md) supersede the remaining-work interpretation below: the 612 recorded predicate discrepancies comprise 600 actual rounding discrepancies (now corrected) and 12 invalid expectations for `1e300` at precision `-29`. Original raw evidence and this step's receipts remain unchanged. Braid smoke has since finished per owner; no outcome supplied.

This REQ-126 slice starts from main `d09b3c571117f58bc7e8aa9f48ce91708d3b51de`, after [verified 1.26.1 publication](../RELEASE-1.26.1-PUBLICATION.md). Braid is conducting its 1.26.1 smoke test, as reported by the owner; no result or new native acceptance is inferred here. The new correction begins unpublished **1.26.2**.

## Contract and independent expectations

[fn:round](https://www.w3.org/TR/xpath-functions-31/#func-round) and [fn:round-half-to-even](https://www.w3.org/TR/xpath-functions-31/#func-round-half-to-even) round to multiples of ten to the power of minus the integer precision. Both preserve the primitive numeric type, NaN, infinities and floating zero; a result rounded to zero keeps the original floating sign. These rules apply after the [argument conversion correction](ROUNDING-FUNCTION-CONVERSION.md).

All currently represented finite numeric magnitudes are below 10^309. At precision below -1000, zero is unambiguously nearest, independently of the tie rule. Precision above 1000 is already an identity case: the decimal quantum is much smaller than the minimum binary64 spacing, and exceeds the fractional scale of supported decimal/integer values. These conservative bounds avoid dependence on a numerical kernel or an enormous power calculation. They do not establish correctness throughout the intervening window.

The predeclared diagnostic matrix has **1,012 rows across three profiles (3,036 observations)**: two rounding functions, direct/named calls, 23 precision choices, and 11 value/type shapes. Expected identity/zero/special-value predicates come from magnitude and representable-spacing arguments, not observed Bosak output. The matrix includes decimal scale, floating scaling, Int32/Int64 narrowing and supported wide integer literal boundaries.

## Findings and bounded correction

The old wrappers accessed `IntegerValue` even when the converted precision was a decimal-backed integer. They also narrowed a negative Int64 precision to Int32 before the kernel; wraparound could select an unrelated precision or request a prohibitively large BigInteger power.

The correction reads an already converted integer payload, retaining ordinary Int64 values and saturating integral decimal-backed values outside the conservative window to +/-1001. Both numeric helpers keep argument conversion and empty handling first, then return identity above 1000 and type-correct zero below -1000. Floating NaN/infinity/zero are returned unchanged; finite floating signs use `Math.CopySign`; decimal-backed integer arguments produce integer zero. No precision outside the window reaches the Int32 casts or numerical power calculations. The original kernels and registered function metadata are unchanged.

| Expression | Before | Corrected |
|---|---|---|
| `round(2.5,-1001)` | FOAR0002 | decimal zero |
| `round(2.5e0,-2147483648)` | 2.5 | double positive zero |
| `round-half-to-even(2.5,9223372036854775808)` | integer payload access error | decimal 2.5 |
| `round(-2.5e0,-1001)` | positive zero | negative zero |
| `round(9223372036854775808,-1001)` | FOAR0002 | integer zero |
| `round(xs:double('INF'),-9223372036854775809)` | integer payload access error | positive infinity |

The use of supported literal values matters. Representation controls confirm `9223372036854775808 instance of xs:integer` is true while `xs:integer('9223372036854775808')` raises FORG0001 in all three profiles. This existing string-constructor limitation is separate. The first test draft confounded it with rounding; original inputs, test source, observations and receipts are retained under `constructor-draft-*`. The corrected inputs use admitted integer literals, and identical corrected tests were rerun against a separate unchanged 1.26.1 engine checkout. No constructor refusal is promoted into a desired passing assertion. Constructor/cast storage beyond existing support remains out of scope.

## Verification and remaining work

The corrected **1,321-case** suite has **1,027 failed /294 passed** against pinned baseline `d09b3c57`, then **1,321 passed /0 failed** after correction with the same test bytes and expectations. It covers all three modern profiles, direct/named/lookup/inline calls, integer/decimal/float/double types, subnormal/large values, both infinities, NaN, negative zero, wide/derived integer precision, empty values, required errors and four supported static backwards-compatible cases. Original constructor-confounded draft receipts are kept separately (967/354 before; 722/599 after) and are not the correction's defect tally.

The matrix has **1,452 extreme-precision observations**. Of 1,332 baseline observations actually executed, **924 discrepancies are corrected**. Another **120** floating-point baseline calls were conservatively withheld because source inspection identified potentially enormous power allocations; their records explicitly say not executed, not pass. All 1,452 extreme observations are executed and correct after the fix. A 30-second external process watchdog bounds each diagnostic batch; no timeout occurred. This is not a performance benchmark, a per-expression hard timeout guarantee, or a claim that withheld baseline calls were observed to time out.

The other **1,584 observations** cover the retained kernel window. **612 discrepancies remain**, with every normalized before/after outcome unchanged and no sampled regression. Seven additional concrete example expressions (21 observations across profiles) confirm the unchanged findings:

| Existing kernel expression | Still observed | Independent expectation |
|---|---|---|
| `round(2.5,29)` | FOAR0002 | decimal 2.5 |
| `round-half-to-even(2.5e0,309)` | NaN | double 2.5 |
| `round-half-to-even(1e300,28)` | INF | unchanged finite double |
| `round(9223372036854775808,28) instance of xs:integer` | false | true |
| `round(9223372036854775808,-28) instance of xs:integer` | false | true |
| `1 div round(-2.5e0,-1000)` | INF | -INF |
| `round-half-to-even(-2.5e0,-1000)` | NaN | negative zero |

These are diagnostic findings, not passing tests that encode defects. They overlap profile/call coverage and are not native conformance totals. [Independent checker](evidence/rounding-precision/analyze.py), [summary](evidence/rounding-precision/diagnostic-summary.json), [remaining findings](evidence/rounding-precision/remaining-kernel-findings.json) and [verification manifest](ROUNDING-PRECISION-BOUNDARIES.json) retain inputs, normalized changes, corrected/draft receipts, watchdog records, source identities and raw evidence hashes.

Final Release build succeeds with zero errors and the existing CS8604 warning. All **15,857 solution +84 language-server tests pass (15,941 total)**. Fresh QT3 stays **31,142/0/679**, frozen qt4 **8,436/0/1,894**, including 11 existing Experimental fallback passes. No current-native Unix rerun is claimed. Nine changed documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance.

REQ-126/D2/D3/D4/applicable-evidence AC-03 stay partial. Catalog inventory remains 76 entries /3,148 feature +24 chapter checks; these source regressions are separate. No public API, general constructor policy, arbitrary-precision decimal, metadata/coercion policy, fixture, gate, retry, kit, upstream, Braid or publication change. Public 1.26.1 remains current; 1.26.2 is unpublished. No error-precedence guarantee is added for multiple independently invalid inputs.

Next: review this bounded correction, then audit/correct retained-window scaling, zero signs and wide-integer result identity as a separate numerical-kernel slice. Keep string-constructor bounds separate. Braid's ongoing 1.26.1 smoke test remains independent.
