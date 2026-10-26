<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala exact numeric rounding kernel verification">
  <h1>Exact rounding kernels</h1>
  <p>26 October 2026 — unpublished source 1.26.2; public 1.26.1 unchanged</p>
</div>

This REQ-126 slice follows the [extreme-precision correction](ROUNDING-PRECISION-BOUNDARIES.md), based on commit `b75f7172232f2696f0cfdfdae835261bc0f75d84`. It completes the sampled numerical-kernel work for `fn:round` and `fn:round-half-to-even`. It consolidates into unpublished patch **1.26.2**, with no new public API. The owner reports that Braid has finished its smoke test; its result and acceptance are not supplied here.

## Behavior and implementation

[fn:round](https://www.w3.org/TR/xpath-functions-31/#func-round) chooses the nearest multiple of ten to the negative precision, with ties toward positive infinity. [fn:round-half-to-even](https://www.w3.org/TR/xpath-functions-31/#func-round-half-to-even) chooses the even multiple at a tie. The primitive numeric type is retained, and floating zero retains the input sign. Floating decisions use the exact binary input value.

The former decimal path constructed a power of ten in `decimal`, which could overflow before any rounding was needed. The half-even floating fallback multiplied or divided by a floating power, producing spurious infinity or NaN. The decimal-backed integer path returned an unannotated decimal, while narrowing rounded Int64 values could overflow even when decimal-backed integer storage could hold the result.

The private kernels now operate on exact coefficients and rational quotients:

- Decimal rounding reads the 96-bit coefficient and decimal scale, removes digits with integer quotient/remainder arithmetic, and checks the final result against existing decimal storage. Precision already finer than the stored scale returns the input. No floating power or overflowing scale intermediate is required.
- Floating rounding decomposes the IEEE value into a signed mantissa and binary exponent, applies a bounded decimal precision using `BigInteger`, and rounds the exact rational. The exact decimal result is parsed once into the destination format; floats parse directly as floats to avoid double rounding. NaN, infinities and existing signed zeros return unchanged. A newly rounded zero keeps the original sign.
- Integer results remain integers. Supported results beyond Int64 retain Bosak's existing decimal-backed integer annotation; results beyond decimal storage still raise FOAR0002. This extends rounding result handling, not the general string-constructor policy or integer storage range.

Argument conversion, empty/backwards-compatible behavior, named-function metadata and the previous guards outside +/-1000 are retained. The existing internal `RoundDouble` caller also uses the exact kernel. No runtime, parser, compiler or public API changes are needed.

| Expression | Previous behavior | Corrected behavior |
|---|---|---|
| `round(2.5,29)` | FOAR0002 | decimal 2.5 |
| `round-half-to-even(2.5e0,309)` | NaN | double 2.5 |
| `round-half-to-even(1e300,28)` | infinity | unchanged finite double |
| `round(9223372036854775808,28) instance of xs:integer` | false | true |
| `round(9223372036854775807,-1)` | narrowing overflow | integer 9223372036854775810 |
| `1 div round(-2.5e0,-1000)` | positive infinity | negative infinity |
| `round-half-to-even(-2.5e0,-1000)` | NaN | negative zero |

## Independent expectations and preserved corrections

An independent Python `Fraction` oracle uses exact binary input ratios and floor/remainder arithmetic to generate **480 cases**. It covers decimals, Int64 boundaries and wider integer literals, floats, doubles, subnormals, large finite values, both signs, zero, midpoint rules, precision boundaries, deterministic random inputs and final storage overflow. All **2,880** direct/named cases across XPath31, XPath40 and XPath40Experimental pass. Identical corrected test bytes against the unchanged `b75f7172` engine produce **1,110 failed /1,770 passed**. The corresponding direct diagnostic matrix has **1,440 observations**, **555 corrected discrepancies**, and zero remaining discrepancies or sampled regressions.

The previous diagnostic matrix contained an expectation error: four input predicates, repeated over three profiles (**12 observations**), expected `1e300` to become zero at precision `-29`. Its rounded mathematical value instead converts back to the same double. The independent oracle covers that case correctly. Original inputs and observations remain untouched; [the correction receipt](evidence/rounding-kernel/historical-oracle-correction.json) records exact original and corrected expressions. The earlier **612** predicate discrepancies comprise **600 actual baseline rounding discrepancies and 12 invalid expectations**; they are not 612 independent defects. After correcting those expectations, all **3,036** historical-matrix observations pass, with **600 measured corrections**, no remaining discrepancy and no sampled regression. Earlier 1.26.2 evidence is retained as historical evidence, with this correction taking precedence over its defect interpretation.

The initial new-test draft also used exponent notation inside some `xs:decimal` strings, which is invalid lexical syntax, and an out-of-range generated decimal. Corrected inputs use plain decimal strings within supported storage. Original draft inputs/tests/receipts remain separate. Draft before/after counts (1,224/1,656 and 180/2,700) are superseded; the first corrected implementation's 36 integer-annotation failures are also retained separately. The final implementation uses the established local annotation `integer`, rather than `xs:integer`. A no-matching-test attempt is retained as an execution receipt, not a passing verification. Final corrected baseline and final corrected after receipts use the same test bytes and expectations.

Both diagnostic matrices run in externally bounded 30-second batches on each engine. All observations execute; no watchdog timeout or withheld baseline row occurs. This does not establish a per-expression timeout or a performance benchmark. The prior 120 hazardous pre-extreme-fix calls remain unexecuted in their original historical receipts; no result is inferred for them.

## Verification and scope

Release build and **18,737 solution +84 language-server tests (18,821 total)** pass. Fresh QT3 stays **31,142/0/679**; frozen qt4 stays **8,436/0/1,894**, with 11 existing Experimental fallback passes. Build has zero errors and the existing CS8604 conformance warning. Ten updated documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance. [Verification manifest](ROUNDING-KERNELS.json), [portable checker](evidence/rounding-kernel/validate-evidence.py), [diagnostic checker](evidence/rounding-kernel/analyze.py) and raw archived test receipts record source identities, pinned baseline, intermediate attempts and exact hashes.

The sampled rounding findings are closed. REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial; catalog inventory remains 76 entries /3,148 feature +24 chapter checks. This does not add arbitrary-precision decimal or integer storage, general wide string-constructor support, new conformance gates, fixtures or conversion policy. Results outside current decimal storage remain bounded. No full draft/native Unix rerun or Braid outcome is inferred. No package, tag, release, upstream submission or Braid change occurs.

Next: review both unpublished rounding slices together, then prepare a consolidated 1.26.2 package candidate and consumer checks before any publication decision. Braid's completed smoke test can be recorded with its actual receipt/outcome when supplied.
