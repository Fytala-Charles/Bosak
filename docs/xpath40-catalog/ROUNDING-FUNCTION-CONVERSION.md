<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala rounding function argument conversion">
  <h1>Rounding argument conversion and precision cardinality</h1>
  <p>26 October 2026 — source 1.26.1 unpublished; public 1.26.0 accepted</p>
</div>

This bounded REQ-126 slice follows the [atomic-function audit and abs correction](ATOMIC-FUNCTION-CONVERSION.md), reviewed and committed at `ef33fab9`. It corrects argument contracts for two-argument `round` and both forms of `round-half-to-even`, with one-argument `round` retained as a neighboring control. It does not change the numerical rounding algorithms or certify every representable precision.

## Contract and audit

[fn:round](https://www.w3.org/TR/xpath-functions-31/#func-round) and [fn:round-half-to-even](https://www.w3.org/TR/xpath-functions-31/#func-round-half-to-even) accept an optional singleton numeric argument and, when supplied, exactly one integer precision. [Function conversion](https://www.w3.org/TR/xpath-31/#id-function-conversion) recursively atomizes atomic-typed arguments, converts untyped atomic values, and checks the resulting type/cardinality. Numeric input retains its numeric kind; untyped numeric input becomes double. Multiple atomic values are XPTY0004; maps/function items cannot be atomized (FOTY0013); invalid lexical conversions are FORG0001.

Before this correction, the internal rounding helpers used `AtomizeValue`, which selects only the first sequence item and leaves arrays intact. The two-argument wrappers read `IntegerValue` directly. Static dispatch passed the precision unchanged; named/dynamic dispatch attempted kind-only conversion first. This produced different failures for equivalent static and dynamic arguments, including raw payload-access failures for static untyped precision and refusal of singleton precision arrays in named calls. The `precision > 1000` shortcut also returned invalid or unconverted numeric arguments unchanged.

The retained [498 independently expected rows](evidence/rounding-function-conversion/inputs.json) cover four signatures, four public call forms and three modern profiles, producing **1,494 observations per run**. The expectations were declared before the engine change. Baseline discrepancies are 396 numeric-contract cases, 204 precision cases, 36 type-identity cases and 18 large-positive-precision contract cases. Twelve coerced-name and 24 NaN/infinity observations already pass and remain unchanged. These are overlapping diagnostics, not native conformance totals.

## Correction and compatibility

Both internal numeric helpers now call the existing `xs:numeric?` conversion before empty handling, numerical dispatch or the large-positive-precision shortcut. The existing handling of undefined and materialized empty sequences remains explicit. This preserves the established backwards-compatible `round` NaN behavior and the empty result for `round-half-to-even`.

The two-argument registrations add partial parameter metadata `[null, "xs:integer"]`. The first null entry leaves numeric validation at the implementation boundary; the precision entry makes static and dynamic dispatch use the same full conversion rules before accessing the integer payload. This metadata describes exactly the preexisting coarse function types (`item()*`, `xs:integer`), preserving their subtyping/coercion behavior. Return metadata is unchanged. Full precise numeric registration metadata remains separate because the earlier abs prototype showed that it changes function-item coercion identity.

The runtime's explicit numeric atomization-error guard also covers `xs:integer`, retaining FOTY0013 for a map/function item used as precision. This affects other callers of integer-target function conversion too; non-integer/non-numeric targets and the general atomization algorithm are unchanged.

| Expression | Before | Corrected |
|---|---|---|
| `round([[],2.5,[]],0)` | XPTY0004 | 3 |
| `round-half-to-even([2.5])` | XPTY0004 | 2 |
| `round-half-to-even((2.5,3.5))` | 2 | XPTY0004 |
| `round-half-to-even([])` | XPTY0004 | empty |
| `round(12.5,[1])` | payload access failure | 12.5 |
| `round#2(12.5,[1])` | XPTY0004 | 12.5 |
| `round(12.5,xs:untypedAtomic('1'))` | payload access failure | 12.5 |
| `round('bad',1001)` | string `bad` | XPTY0004 |
| `round-half-to-even(xs:untypedAtomic('2.5'),1001)` | untyped atomic | double 2.5 |

## Scope and evidence

The new suite has **1,514 passing cases**. The unchanged 1,502-case suite is **654 failed /848 passed** before and **1,502 passed /0 failed** after. Twelve additional node controls exercise untyped attributes, recursive array members, multiple numeric nodes and multiple precision nodes through all four call forms. Eight static backwards-compatible controls preserve supported first-item and empty-input behavior. The modern cases cover integer/decimal/float/double identity, NaN/infinity, coerced names, required errors, recursive empty/singleton/multiple arrays, and high-positive-precision contract checks.

The [checker](evidence/rounding-function-conversion/analyze.py) verifies **654 corrected observations, zero remaining sampled discrepancies and zero regressions**, against the original expectations. It retains the [summary](evidence/rounding-function-conversion/diagnostic-summary.json), [changes](evidence/rounding-function-conversion/diagnostic-changes.json), and raw [before](evidence/rounding-function-conversion/observations-before.json) / [after](evidence/rounding-function-conversion/observations-final.json) results. Baseline, first correction, compact-test and node-expanded TRXs remain separate. Compacting the test data did not alter the expressions or expectations.

Release build succeeds with zero errors and the existing CS8604 conformance-runner warning. All **14,536 solution +84 language-server tests pass (14,620 total)**. Fresh QT3 remains **31,142/0/679** and frozen qt4 **8,436/0/1,894**, including 11 existing Experimental fallback passes. Nine changed documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

Complete build/test/gate receipts, source fingerprints, commands and evidence hashes are recorded in [the verification manifest](ROUNDING-FUNCTION-CONVERSION.json). The full solution runs outside the sandbox so the existing database-provider HTTP stubs can start. No test expectation, frozen fixture, gated set, or retry policy is changed. No current-native Unix rerun is claimed. No error-precedence guarantee is added for inputs with multiple independent errors.

The numerical kernels, Int64 precision payload limit, extreme-negative-precision narrowing, schema-list typed-value cardinality and generalized function conversion remain separate. The eight supported static backwards-compatible cases do not certify every backwards-compatible or dynamic-call conversion rule. Decimal precision and wide-integer limitations remain in force. No public API, package version, publication, upstream contribution, or Braid edit occurs. REQ-126 stays partial and the catalog inventory stays 76 entries /3,148 feature +24 chapter checks.

**Raw-evidence retention correction — 26 October 2026:** the prior abs commit `ef33fab9` normalized line endings in 18 receipt files and omitted nine logs because of repository ignore rules. The separate archive commit `4163e846` stores those receipts' original raw bytes and explicitly retains the nine logs. Every file matches the SHA-256 already recorded in the original abs manifest. Both evidence directories now disable Git line-ending conversion. Original execution receipts and outcomes are preserved; this archive correction is recorded separately from the original validation and does not constitute a rerun or a change in acceptance.

Next: review this bounded slice, then prepare a consolidated unpublished 1.26.1 candidate with consumer/package regression validation. Publication requires its separate owner authorization; broader numerical and coercion issues remain explicit limitations.
