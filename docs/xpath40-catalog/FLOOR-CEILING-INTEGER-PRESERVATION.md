<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala floor and ceiling integer preservation verification">
  <h1>Floor and ceiling preserve integer identity</h1>
  <p>26 October 2026 — source 1.26.3 unpublished; public 1.26.2 unchanged</p>
</div>

This bounded REQ-126 slice follows [abs integer preservation](ABS-INTEGER-PRESERVATION.md), based on commit `85ad1335dff6e294fdf25e22e68d17a753156203`. It consolidates into the existing unpublished **1.26.3** patch. No new package, API or publication is introduced.

## Correction

[fn:floor](https://www.w3.org/TR/xpath-functions-31/#func-floor) and [fn:ceiling](https://www.w3.org/TR/xpath-functions-31/#func-ceiling) preserve integer type for integer arguments. Every supported integer is already a whole number, so either operation can return it unchanged. Floating signed-zero and special-value rules remain applicable to floating inputs.

The implementation already returned Int64-backed integers unchanged, but decimal-backed integers passed through the ordinary decimal branch, losing their integer annotation. Each function now returns a decimal-backed integer argument unchanged when the existing runtime type matcher recognizes it as `xs:integer`. This preserves its value and existing integer-family annotation without narrowing or arithmetic.

| Expression | Before | After |
|---|---|---|
| `floor(9223372036854775808) instance of xs:integer` | false | true |
| `ceiling(-9223372036854775809) instance of xs:integer` | false | true |
| `floor(xs:unsignedLong('18446744073709551615')) instance of xs:integer` | false | true |
| `ceiling(xs:decimal('9223372036854775808')) instance of xs:integer` | false | false; retained plain-decimal behavior |
| `1 div ceiling(xs:double('-0.25'))` | negative infinity | negative infinity; retained sign control |

The source change is two guarded switch arms plus a dated header history entry. Decimal/floating calculations, atomization, argument conversion, empty/backwards-compatible behavior, function metadata and registrations are unchanged. General wide constructors/casts and `idiv` limits remain separate. This slice does not certify the broader floor/ceiling argument-conversion contract; that remains an independent audit.

## Verification

The same **936 cases** produce **168 failed /768 passed** on the unchanged baseline, then **936 passed /0 failed** after correction. Predeclared expectations use exact integer identity, Python decimal floor/ceiling for exactly represented decimal inputs, and exactly represented floating controls. Direct, named, lookup and inline calls run under XPath31, XPath40 and XPath40Experimental. Cases include both signs and supported wide integer limits, derived integers, integral/fractional plain decimals, floats/doubles, signed zero, infinities, NaN and modern empty input. No baseline failure is converted into a passing expected defect.

Baseline source fingerprints were captured before modification and match `85ad1335`; test bytes and expectations are identical before/after. The baseline is the unchanged tracked working source, not a separately claimed isolated checkout. Diagnostic before/after batches repeat the focused inputs: **936 observations**, **168 corrected discrepancies**, zero remaining discrepancy and no sampled regression. These counts overlap tests and do not represent 168 independent defects. Both batches execute every row within a 30-second external process watchdog; no timeout occurs. No per-expression timeout guarantee or performance benchmark is claimed.

Release build has zero errors and the existing CS8604 conformance warning. All **20,996 solution +84 language-server tests pass (21,080 total)**. Fresh frozen gates remain **QT3 31,142/0/679** and **qt4 8,436/0/1,894**, with 11 existing Experimental fallback passes. No current-native Unix rerun is claimed. Nine changed documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance. [Proof/source identities](FLOOR-CEILING-INTEGER-PRESERVATION.json), [diagnostic checker](evidence/floor-ceiling-integer/analyze.py) and [portable verifier](evidence/floor-ceiling-integer/validate-evidence.py) retain raw receipts and hashes.

## State and next work

Public **1.26.2** remains current. Its [Braid local automated adoption report and scope](ABS-INTEGER-PRESERVATION.md#braid-adoption-evidence-and-remaining-scope) remain as previously recorded: uncommitted adoption at capture, no current-adoption CI or added native UI/accessibility acceptance. This task makes no Braid edits and infers no 1.26.3 adoption. Original publication, candidate, rounding, abs and Braid snapshot evidence is untouched.

REQ-126/D2/D3/D4/applicable-evidence AC-03 stay partial; catalog inventory remains 76 entries /3,148 feature +24 chapter checks. Numeric storage, general constructors/casts, `idiv`, schema-list/coercion and DateTimeOffset Unix limits remain explicit. No fixture/gate/retry/workflow/kit/upstream/publication changes occur.

Next: audit floor/ceiling argument conversion separately, or consolidate the two integer-preservation slices for package/consumer verification before a later release decision. Do not infer broader numeric conformance or constructor support from this identity correction.
