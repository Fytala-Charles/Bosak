<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala numeric function argument conversion audit">
  <h1>Atomic standard-function argument conversion and cardinality</h1>
  <p>25 October 2026 — source 1.26.1 unpublished; public 1.26.0 accepted</p>
</div>

**Archive correction — 26 October 2026:** implementation and validation were committed at `ef33fab9`. Git normalized line endings in 18 evidence files and ignored nine raw logs in that commit. The [separate retention correction](evidence/atomic-function-conversion/retention-correction-2026-10-26.json) restores the original raw bytes, adds the nine logs and protects the evidence directory from line-ending conversion. Every original evidence hash still matches the original manifest; execution outcomes and acceptance are unchanged.

This REQ-126 slice starts at local main `ffe3f2a6`, the PR460 merge. It follows the [binary/range empty-result correction](BINARY-EMPTY-ATOMIZATION.md) and closes the sampled `abs` findings from the [recursive-array audit](ARRAY-ATOMIZATION-AUDIT.md). It does not establish correctness of every standard-function conversion path.

## Contract and audit

[XPath 3.1 function conversion](https://www.w3.org/TR/xpath-31/#id-function-conversion) atomizes atomic-typed arguments, performs the permitted conversions, and checks the declared sequence type. [fn:abs](https://www.w3.org/TR/xpath-functions-31/#func-abs) accepts and returns `xs:numeric?`. Empty arrays therefore contribute no atomic values, recursively nested singleton arrays contribute one value, and multiple numeric values violate the parameter cardinality. Atomizing maps or other function items raises FOTY0013. Numeric values retain their numeric type; untyped atomic input converts to double.

The registration for `abs` lacked `ParameterTypeNames` and `ReturnTypeName`. Static calls consequently took the raw-span dispatch path, while named/dynamically looked-up calls used unconstrained kind metadata. `Abs` then called `AtomizeValue`, which selects only the first sequence item and leaves arrays intact. This explains both array refusal and silent acceptance of `abs((1,2))`.

The [120 retained audit observations](evidence/atomic-function-conversion/audit-before.json) cover five one-argument numeric functions, four cardinality shapes, two call forms and three profiles. `floor`, `ceiling`, and one-argument `round` already declare `xs:numeric?` and handle the sampled array/cardinality cases correctly. `round-half-to-even` has the same missing-metadata symptoms as `abs`. Source inspection also finds missing precise metadata in two-argument `round` and both `round-half-to-even` registrations; those remain follow-up scope.

## Bounded correction

`Abs` now applies the existing `xs:numeric?` function conversion at its implementation boundary. Both static and dynamic dispatch therefore use the existing recursive array conversion path. It normalizes both undefined and materialized empty-sequence results before numeric dispatch. No shared array-flattening algorithm or public API changes.

A precise-registration prototype passed its focused cases but changed function-item coercion identity: QT3 `fn-function-name-024` lost the name `fn:abs` after coercion to `function(item()*) as item()*`. The final correction retains the existing registration metadata and named-function identity, while enforcing the argument contract in `Abs`. A regression explicitly checks that identity in all three profiles. Registration metadata and general function coercion remain separate follow-up work; neither frozen fixtures nor expected gate counts were changed.

The first signature-only run exposed a separate error-code fallback: numeric-union conversion passed maps/function items through `Atomize` and eventually raised generic XPTY0004. The correction explicitly raises FOTY0013 for those items when the target item type is `xs:numeric`. This also applies to the existing `floor`, `ceiling`, and one-argument `round` signatures; it does not change non-numeric conversion targets.

| Expression | Before | Corrected |
|---|---|---|
| `abs([2])` | XPTY0004 | integer 2 |
| `abs([[],[-2],[]])` | XPTY0004 | integer 2 |
| `abs([])` | XPTY0004 | empty |
| `abs((1,2))` | integer 1 | XPTY0004 |
| `abs([1,2])` | XPTY0004 | XPTY0004 |
| `abs([xs:untypedAtomic('-2')])` | XPTY0004 | double 2 |
| `abs([map{}])` | XPTY0004 | FOTY0013 |

Backward-compatible calls retain the existing conversion policy, including first-item truncation. Early date/duration arithmetic dispatch is unchanged. Wide-integer `Math.Abs` bounds, schema-list typed-value cardinality, generalized function conversion, and the remaining rounding signatures are not certified by this slice.

## Verification

The focused suite adds **366 cases**: 300 argument cases across direct calls, named references, `function-lookup`, and an untyped inline-function wrapper; 63 numeric identity/special-value/neighbor-function/coerced-name controls; three backwards-compatible controls. All three modern profiles are covered.

The unchanged 300-case baseline has **168 failures /132 passes**; the signature-only attempt has **48 failures /252 passes**. The signature plus numeric atomization-error prototype passes **300/0/0**, then **363/0/0**, but its frozen QT3 result is **31,141/1/679** and it is superseded. The final implementation passes **366/0/0**, including coerced-name preservation. Original TRXs are retained separately. The first draft used an incorrect empty-result display expectation (`""` instead of `"()"`); its 180/120 receipt is superseded by the corrected baseline and is not used as the defect tally. The first identity-preserving attempt also retained a materialized empty sequence; its 13/353 receipt is superseded after adding explicit empty-sequence normalization.

The independent 576-pair matrix improves from **36 to zero array discrepancies** and from **12 to zero flat-reference discrepancies**. All 48 changed outcomes match independently flattened integer expectations; no other normalized outcome changes. These diagnostic counts overlap test coverage and are not native conformance totals. [Checker](evidence/atomic-function-conversion/analyze.py), [summary](evidence/atomic-function-conversion/matrix-summary.json), [changes](evidence/atomic-function-conversion/matrix-changes.json), and [raw observations](evidence/atomic-function-conversion/array-matrix-final.json) are retained. The matrix baseline is the prior binary-empty slice's retained observations, whose engine matches this slice's base; the final observations were freshly executed.

Final Release build succeeds with zero errors and zero warnings in the incremental build. All **13,022 solution +84 language-server tests pass (13,106 total)**. Fresh QT3 remains **31,142/0/679** and frozen qt4 **8,436/0/1,894**, including 11 existing Experimental fallback passes. The final complete solution run executes outside the sandbox so the database-provider local HTTP stubs can start. Earlier sandbox failures and the superseded prototype receipts remain separate. No current-native Unix rerun or full current-draft conformance is claimed.

Full validation receipts and source fingerprints are recorded in [the verification manifest](ATOMIC-FUNCTION-CONVERSION.json). The eight changed documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

REQ-126 remains partial. The unchanged 76-entry /3,148 feature +24 chapter catalog inventory does not count these regression cases. Source stays within the existing unpublished 1.26.1 patch; public 1.26.0 and bounded Braid REQ-138 acceptance remain current. No publication, fixture/gate change, upstream submission, or Braid edit occurs.

Next: review this bounded correction, then audit remaining rounding argument contracts and precision conversion, including named-function coercion compatibility, before extending the fix. A consolidated 1.26.1 release candidate remains a separate step.
