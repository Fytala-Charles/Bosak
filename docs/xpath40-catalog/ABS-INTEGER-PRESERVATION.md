<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala abs integer preservation verification">
  <h1>Absolute values preserve integer identity</h1>
  <p>26 October 2026 — source 1.26.3 unpublished; public 1.26.2 unchanged</p>
</div>

This REQ-126 slice starts from main `06abb7c794f0f572675bf359130f5e8b98b37311`, after [verified 1.26.2 publication](../RELEASE-1.26.2-PUBLICATION.md). It corrects `fn:abs` within the existing supported numeric storage range and begins unpublished patch **1.26.3**.

## Contract and bounded correction

[fn:abs](https://www.w3.org/TR/xpath-functions-31/#func-abs) returns the absolute value while preserving the argument's primitive numeric type. Float/double zeros become positive zero, and either infinity becomes positive infinity. Derived numeric types may return their base primitive numeric type.

The existing Int64 implementation called `Math.Abs(long.MinValue)`, which overflows even though Bosak's existing decimal-backed integer representation can hold positive 9223372036854775808. Decimal-backed integers used the general decimal branch, which discarded their integer annotation. Argument conversion was already corrected in 1.26.1 and is retained here.

The integer branch now widens Int64.MinValue to decimal before negation, retaining the local `integer` annotation. Other Int64 values retain their normal integer representation. The decimal branch recognizes existing integer-family annotations and returns an integer-annotated absolute value; plain decimals remain decimals. Float/double handling, empty behavior, argument conversion and named-function metadata are unchanged. No public API or runtime/parser/compiler change is required.

| Expression | Previous result | Corrected result |
|---|---|---|
| `abs(xs:long('-9223372036854775808'))` | FOAR0002 | integer 9223372036854775808 |
| `abs(-9223372036854775809) instance of xs:integer` | false | true |
| `abs(9223372036854775808) instance of xs:integer` | false | true |
| `abs(xs:unsignedLong('18446744073709551615')) instance of xs:integer` | false | true |
| `abs(xs:decimal('-9223372036854775808')) instance of xs:integer` | false | false; remains a decimal |
| `1 div abs(xs:double('-0'))` | positive infinity | positive infinity; retained control |

Tests use supported integer literals and supported derived constructors. They do not encode general wide string-constructor refusals as desired behavior. Values outside current decimal-backed integer storage and general constructor/cast changes remain separate. `idiv` result bounds are also outside this slice.

## Verification

The same **1,323** test cases produce **288 failed /1,035 passed** against the unchanged 1.26.2 source, then **1,323 passed /0 failed** after correction. Inputs and expectations are byte-identical before and after. The baseline source fingerprint matches the pinned main commit; the baseline probe executes before any engine modification. This verification uses that unchanged working source, rather than claiming a separate isolated checkout.

Coverage includes Int64 boundaries, supported wider signed integers up to the decimal-backed limit, derived integer constructors, plain decimals with integral values, floats/doubles, positive/negative zero, both infinities, NaN, empty values, recursive arrays, untyped conversion and required errors. Direct, named, lookup and inline calls run under XPath31, XPath40 and XPath40Experimental. A coerced-function-name control retains `abs` identity. Integer expectations use exact Python integer magnitudes; predicates separately verify numeric values, primitive type and floating zero sign.

The **1,323 before/after diagnostic observations** repeat the focused inputs through public API evaluation, with **288 corrected discrepancies**, zero remaining discrepancy and no sampled regression. These counts overlap tests; they are not additional conformance coverage or 288 independent defects. Both diagnostic batches have a 30-second external process watchdog; all rows execute and neither times out. No per-expression timeout guarantee or performance benchmark is claimed.

Release build succeeds with zero errors and the existing CS8604 conformance warning. All **20,060 solution +84 language-server tests pass (20,144 total)**. Fresh frozen gates remain **QT3 31,142/0/679** and **qt4 8,436/0/1,894**, with 11 existing Experimental fallback passes. No current-native Unix rerun is claimed. Nine changed documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance. [Machine-readable proof](ABS-INTEGER-PRESERVATION.json), [independent checker](evidence/abs-integer/analyze.py) and [portable evidence verifier](evidence/abs-integer/validate-evidence.py) retain raw receipts, source identities and hashes.

## Braid adoption evidence and remaining scope

The owner supplied [Braid's local 1.26.2 adoption report](evidence/abs-integer/braid-1.26.2-adoption-report.md). Its [provenance receipt](evidence/abs-integer/braid-report-provenance.json) records the source path, repository HEAD, document status and exact snapshot hash. The report identifies released source `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb` and tag `fb01b31412fd1f689228bd2f32285b955642b3f8`, ten verified signed packages and matching source/archive/cache/output provenance. It reports **1,876 Release tests**, including **246 public XPath API and 72 Braid preview rounding cases**, plus **280 overlapping signed-public-package checks**, and clean 113-file desktop publish hygiene. Those totals must not be added together.

The report explicitly states that adoption is uncommitted, claims no current-adoption CI result, and adds no native UI/accessibility acceptance. Existing native functional acceptance remains the 1.26.0 conditional workflow. Braid sources, its report, dependency checkouts and upstream records are read only in this task. This evidence establishes reported local automated adoption of public **1.26.2**, not adoption or acceptance of unpublished **1.26.3**.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial; inventory stays 76 entries /3,148 feature +24 chapter checks. Numeric storage, general wide constructors/casts, Int64 idiv, schema-list/general coercion and DateTimeOffset Unix limits remain explicit. No fixture/gate/retry/kit/workflow/upstream/Braid or publication change occurs. Original release/candidate/rounding evidence is untouched.

Next: review this bounded correction; keep integer constructor/cast and `idiv` work in separate slices. Consolidate package and consumer validation before any 1.26.3 publication decision.
