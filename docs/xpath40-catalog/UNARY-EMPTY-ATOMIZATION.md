<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala empty atomized unary operand correction">
  <h1>Unary minus after empty atomization</h1>
  <p>25 October 2026 - source 1.26.1 unpublished; public 1.26.0</p>
</div>

This bounded REQ-126 slice follows [PR455's unary conversion correction](UNARY-COERCION.md), merged at `d89b50e8223cec9913a8c41c9cef8c870bb6228a` after all three CI checks passed (12,279 solution/LSP tests). It adds the missing empty-result check immediately after unary-minus atomization. Source remains the existing unpublished **1.26.1** patch; [public 1.26.0](../RELEASE-1.26.0-PUBLICATION.md) and Braid validation are unchanged.

## Contract and behavior

[XPath 3.1 arithmetic evaluation](https://www.w3.org/TR/xpath-31/#id-arithmetic) applies atomization before handling an empty operand. [Array atomization](https://www.w3.org/TR/xpath-31/#id-atomization) obtains the atomized values of array members. An empty array remains an item before that step, but contributes no atomic values. Unary minus must return empty when the resulting operand is empty, rather than attempting integer conversion.

| Expression | Before | Corrected |
|---|---|---|
| `count(-[])` | XPTY0004: Cannot convert Undefined to integer | 0 |
| `count(-[()])` | XPTY0004 | 0 |
| `count(-[[]])` | XPTY0004 | 0 |
| `count(-[(),()])` | XPTY0004 | 0 |
| `count(-(-[]))` | XPTY0004 | 0 |
| `count(+[])` | 0 | 0 |
| `-[[2]]` | integer -2 | integer -2 |

`Negate` already handles an initially empty operand and backward-compatible numeric conversion. The correction adds `IsEmptySequence` after the existing modern-mode atomization and returns the existing undefined/empty representation before the duration/numeric branches. Shared atomization, unary plus, optimizer, conversion helpers, duration handling and backward-compatible dispatch are unchanged. No acquisition, public API, storage, decimal precision or result-type expansion is introduced.

## Verification

Adds **64 cases**: 24 modern empty-operand rows, 18 nonempty value controls, 18 nonempty error controls and four backward-compatible empty controls. The three modern profiles are XPath31, frozen XPath40 and XPath40Experimental. Empty rows exercise direct and inline-function runtime operands with minus, plus and repeated/mixed signs. Nonempty controls preserve untyped-to-double conversion, element atomization, wide integer/decimal magnitudes, cardinality/type errors, map/function atomization errors and invalid lexical conversion. Backward-compatible empty operands remain NaN.

Before: **21 failed / 43 passed / 0 skipped**. After: **64 passed / 0 failed / 0 skipped**, with unchanged expectations. A separate 36-observation probe changes exactly 21 formerly erroneous empty results to count zero; 15 controls and separate findings remain identical. The original failing TRX and observations are retained independently.

Release build succeeds with zero errors and the existing CS8604 conformance-runner warning. All **12,259 solution + 84 language-server tests pass (12,343 total)**. Fresh QT3 remains **31,142/0/679** and frozen qt4 **8,436/0/1,894** across 224 unchanged configured/executed sets, with 11 existing Experimental fallback passes. Current-native Unix remains **22/2/0, exit 2**, with known negative-domain cases 120/121 unresolved under the frozen profile. No full current-draft conformance is claimed.

[Machine-readable proof](UNARY-EMPTY-ATOMIZATION.json) retains source fingerprints and evidence hashes. [Evidence](evidence/unary-empty/) includes original/corrected focused TRXs, diagnostic comparison, build/test/gate logs and all solution/LSP TRX hashes/counters. Complete raw suite TRXs remain under `.guard-tmp/req126-unary-empty-evidence`. Nine updated pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

## Remaining boundary and next slice

The shared atomizer still counts some array members before recursively atomizing them. The separate observed expression `-[[],2]` still raises XPTY0004 although recursive member atomization yields one atomic value. This finding is retained as a diagnostic, not asserted as desired behavior. Correcting a helper shared by arithmetic, comparisons, casts and other consumers needs its own caller/cardinality audit and regression controls; it is not folded into this unary-minus patch.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial. The 64 regressions are outside the unchanged 76-entry / 3,148 feature + 24 chapter documentation inventory. Fixtures, gates, retry policy, public packages, upstream contributions and Braid code are unchanged. Next: review this patch, then audit shared recursive-array atomization before expanding its behavior.
