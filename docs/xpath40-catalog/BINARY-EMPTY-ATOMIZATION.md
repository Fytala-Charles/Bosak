<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala empty numeric binary and range operand correction">
  <h1>Empty numeric binary and range operands after atomization</h1>
  <p>25 October 2026 - source 1.26.1 unpublished; public 1.26.0 accepted</p>
</div>

This bounded REQ-126 slice follows the [singleton-array correction](ARRAY-SINGLETON-ATOMIZATION.md), merged as PR459 at `50cac81c47d751eac4b190e8250d664139259006` after all three CI checks passed (12,526 tests). PR457 also records [formal Braid acceptance of REQ-138](../REQ-138-BRAID-ACCEPTANCE.md) on public 1.26.0. Source remains in the existing unpublished **1.26.1** patch; no release or Braid code change occurs here.

[XPath 3.1 arithmetic](https://www.w3.org/TR/xpath-31/#id-arithmetic) and [range expressions](https://www.w3.org/TR/xpath-31/#id-range-expressions) handle empty operands after atomization. An array can contribute no atomic values even though the original operand is an item. The numeric binary paths previously attempted conversion of that empty result; modern ranges rejected it as a non-integer.

The correction adds a post-atomization empty check to the numeric branches of Add, Subtract, Multiply, Divide, IntegerDivide and Modulo, before numeric conversion and zero-divisor handling. Modern ranges atomize both bounds and return the existing empty sequence before integer-range conversion. Direct syntax and runtime function parameters agree.

| Expression | Before | Corrected |
|---|---|---|
| `[] + 1` | XPTY0004 | empty |
| `1 - [()]` | XPTY0004 | empty |
| `[[],[]] * 1` | XPTY0004 | empty |
| `[] idiv 0` | XPTY0004 | empty |
| `0 mod []` | XPTY0004 | empty |
| `[] to 3` | XPTY0004 | empty |
| `1 idiv 0` | FOAR0001 | FOAR0001 |

Existing validation of nonempty operands, integer/range limits, numeric promotion, decimal-backed exact idiv and error codes are preserved. The change does not move early date/duration dispatch or backward-compatible dispatch. Empty-array handling in those early paths, including BC idiv/range, is not certified by this slice. Standard-function conversion/cardinality remains separate. Compound erroneous operands do not gain a deterministic error-precedence guarantee.

## Verification

Adds **214 cases**: 168 modern empty cases across seven operators, four empty array shapes, both operand sides and three profiles; 36 nonempty/error controls; ten backward-compatible NaN controls. Modern empty rows check both direct and inline-function runtime forms. Controls retain recursive singleton behavior, descending ranges, wide integer arithmetic, nonempty date/duration dispatch, cardinality/type/lexical errors and nonempty integer zero-divisor errors.

Before: **168 failed / 46 passed / 0 skipped**. After: **214 passed / 0 failed / 0 skipped**, with unchanged expectations. Original failing and corrected TRXs remain separate.

The identical independent 576-case matrix improves from **84 to 36 discrepancies**: 48 corrected, no regressions. All sampled numeric/range contexts now match their independent expectations. Remaining array discrepancies are standard-function conversion; all 576 flat-reference outcomes are unchanged, including 12 known faulty abs cardinality references. The diagnostic matrix is not a native conformance pass tally and overlaps source coverage.

Release build succeeds with zero errors and the existing CS8604 conformance-runner warning. All **12,656 solution + 84 language-server tests pass (12,740 total)**. Fresh QT3 stays **31,142/0/679** and frozen qt4 **8,436/0/1,894** across 224 unchanged configured/executed sets, with 11 existing Experimental fallback passes. Current-native Unix stays **22/2/0, exit 2**, retaining known negative-domain cases 120/121 under the frozen profile. No full current-draft conformance is claimed.

[Machine-readable proof](BINARY-EMPTY-ATOMIZATION.json) records validated source fingerprints and hashes for [before/after tests, matrices and logs](evidence/binary-empty/). Complete solution/LSP TRXs remain under `.guard-tmp/req126-binary-empty-evidence`; hashes/counters are committed. Documentation renders as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial. The 214 regressions are outside the unchanged 76-entry / 3,148 feature + 24 chapter documentation inventory. No public API, version increment within the unpublished patch, fixture/gate/retry change, upstream contribution or release. Next: review this patch, then audit standard-function atomic conversion/cardinality before its bounded correction.
