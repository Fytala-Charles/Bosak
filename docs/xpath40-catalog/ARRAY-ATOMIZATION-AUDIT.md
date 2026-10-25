<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala recursive array atomization caller and cardinality audit">
  <h1>Recursive array atomization: caller and cardinality audit</h1>
  <p>25 October 2026 - source 1.26.1 unpublished; public 1.26.0</p>
</div>

**Implemented follow-up:** PR458 passed all three CI checks (12,415 tests) and merged at `8263f689216ea8f8ad0a719dd3552490499d0a69`. The [singleton-array correction](ARRAY-SINGLETON-ATOMIZATION.md) reduces matrix discrepancies from 207 to 84, while the original audit inputs/checker/observations remain unchanged below. Source 1.26.1 remains unpublished.


This REQ-126 investigation follows the [empty unary correction](UNARY-EMPTY-ATOMIZATION.md), merged as PR456 at `f646d9da6b97fab6816fca0c8fa8bd43ca94a79b` after all three CI checks passed (12,343 tests). It adds **72 supported regression controls** and **576 diagnostic array/reference pairs**, comprising 1,152 raw observations. It changes tests and documentation; the complete `src` tree is identical to PR456. Source version, public API, engine, fixtures, gates, retries and public packages are unchanged. Braid independently validates public 1.26.0.

## Contract and independent expectation

[XPath 3.1 atomization](https://www.w3.org/TR/xpath-31/#id-atomization) recursively obtains the atomic values of array members. The final empty/singleton/multiple cardinality is interpreted by the consumer: [arithmetic](https://www.w3.org/TR/xpath-31/#id-arithmetic), [value comparison](https://www.w3.org/TR/xpath-31/#id-value-comparisons), casts and function conversion have distinct contracts. [Function conversion](https://www.w3.org/TR/xpath-31/#id-function-conversion-rules) must respect the declared sequence type; [fn:abs](https://www.w3.org/TR/xpath-functions-31/#func-abs) accepts `xs:numeric?`, including empty but excluding multiple atomic values.

The matrix uses 16 explicit pure-integer/empty array shapes across 12 contexts and XPath31, frozen XPath40 and XPath40Experimental. A Python recursive flattening calculation supplies an independent integer sequence for each shape. Explicit context rules then calculate the expected integer/boolean/string result or XPTY0004 cardinality error. The processor also evaluates the flattened sequence as a reference, but that observed reference is not trusted as an oracle. No draft-only semantic adoption is inferred.

All three profiles agree on every actual/reference observation. **369 of 576 array cases match the independent expectations; 207 differ.** These are diagnostic counts, not unit/native conformance pass counts. Equal error codes do not establish complete error precedence or behavior for arbitrary node/function/type inputs.

| Context | Array cases | Array discrepancies against independent expectation | Flat-reference discrepancies |
|---|---:|---:|---:|
| data | 48 | 0 | 0 |
| unary-minus | 48 | 21 | 0 |
| unary-plus | 48 | 21 | 0 |
| add | 48 | 27 | 0 |
| multiply | 48 | 27 | 0 |
| idiv | 48 | 27 | 0 |
| range | 48 | 27 | 0 |
| value-eq | 48 | 21 | 0 |
| general-eq | 48 | 0 | 0 |
| cast | 48 | 0 | 0 |
| castable | 48 | 0 | 0 |
| function-argument | 48 | 36 | 12 |

The raw array/flat comparison alone gives 357 matches and 219 differences. The independent check identifies **12 incorrect flat-reference results**: `abs((1,2))` returns the first value instead of rejecting multiple atomic values in its single argument. Therefore the 219 pair differences cannot honestly be labeled 219 array failures; 12 of those array cases correctly raise XPTY0004 while their flat references do not.

## Separate findings

1. **Recursive member cardinality.** `-[[],2]` and `[[],2] eq 2` raise XPTY0004 even though recursive atomization yields one integer. The VM's singleton `Atomize` collects immediate members and checks their count before recursively atomizing the collected members. Nested empty arrays therefore inflate its cardinality. Singleton nesting such as `[[2]]` works; `fn:data`, general equality and the separate cast/castable paths match all 48 independently expected cases in each of those contexts.
2. **Empty results in callers.** `[] + 0`, `[] * 1`, `[] idiv 1` and `[] to 3` raise XPTY0004, while their independent empty-sequence inputs produce empty. The unary-minus correction handles its own post-atomization empty result; it does not fix binary arithmetic or range callers. Replacing one recursive helper cannot establish correct handling in every caller.
3. **Function-argument conversion/cardinality.** `abs([2])` raises XPTY0004 while `abs(2)` succeeds; `abs([])` refuses rather than returning empty. Conversely `abs((1,2))` silently returns 1. This is a separate standard-function conversion/cardinality path, not evidence that the VM singleton helper alone explains every discrepancy.

The [caller inventory](evidence/array-atomization-audit/caller-inventory.json) records 71 textual `Atomize` references, including its declaration and recursion. It records the nearest preceding static method, not a whole-program call graph. The distinct `AtomizeForCast`, `AtomizeItems`, general-comparison flattening and standard-library atomization/conversion routes must remain explicit during implementation review.

## Recommended implementation sequence

First correct the VM singleton array path so it recursively obtains zero or one atomic value before applying its caller's cardinality check. Preserve required multiple-value refusals, map/function errors, node typed-value behavior, profile gates and backward-compatible behavior. Add failing-before/passing-after direct/runtime regressions for empty members surrounding a singleton, nested singleton arrays and genuine multiple atomic values; run both frozen gates.

Keep post-atomization empty handling in binary arithmetic/ranges and standard-function conversion/cardinality as separate follow-ups. Do not claim that a singleton-helper correction resolves all 207 discrepancies. Shared helper reuse must be reviewed against the caller inventory and error behavior; no implementation or error-precedence expansion is delivered in this audit.

## Validation and evidence

The 72 source controls pass across all three modern profiles. They cover recursive `fn:data`, casts/castability, supported singleton unary/binary/comparison behavior, existing empty-unary behavior, genuine multiple-value errors, map/function atomization refusal and invalid lexical casts. They do not encode observed defects as passing assertions. The diagnostic matrix and independent checker are retained separately.

Release build succeeds with zero errors and the existing CS8604 conformance-runner warning. All **12,331 solution + 84 language-server tests pass (12,415 total)**. No manual conformance harness is rerun for this tests/docs-only audit. The engine-identical [PR456 baseline](evidence/array-atomization-audit/engine-identical-baseline-gates.json) retains QT3 **31,142/0/679**, frozen qt4 **8,436/0/1,894** across 224 unchanged configured/executed sets with 11 existing Experimental fallback passes, and separate current-native Unix **22/2/0, exit 2**. Neither skipped cases nor fallback retries certify full current-draft conformance.

[Machine-readable audit](ARRAY-ATOMIZATION-AUDIT.json) records source identity and hashes of [raw observations, inputs, comparisons, checker, probe and test receipts](evidence/array-atomization-audit/). Complete solution/LSP TRXs remain under `.guard-tmp/req126-array-atomization-audit-evidence`; hashes/counters are committed. Nine documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial. The 72 controls are outside the unchanged 76-entry / 3,148 feature + 24 chapter documentation inventory. Existing unpublished patch 1.26.1 and public/Braid-validation 1.26.0 remain separate. Next: review this audit, then implement the bounded VM singleton-array correction with fresh frozen gates.
