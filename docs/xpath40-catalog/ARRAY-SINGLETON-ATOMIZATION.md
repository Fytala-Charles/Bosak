<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala recursive array singleton atomization correction">
  <h1>Recursive array singleton atomization</h1>
  <p>25 October 2026 - source 1.26.1 unpublished; public 1.26.0</p>
</div>

**Merged follow-up:** PR459 passed all three CI checks (12,526 tests) and merged at `50cac81c47d751eac4b190e8250d664139259006`. The [numeric binary/range empty correction](BINARY-EMPTY-ATOMIZATION.md) reduces matrix discrepancies from 84 to 36. Original evidence below is unchanged; 1.26.1 is still unpublished.


This bounded REQ-126 correction implements the first recommendation from the [caller/cardinality audit](ARRAY-ATOMIZATION-AUDIT.md), merged as PR458 at `8263f689216ea8f8ad0a719dd3552490499d0a69` after all three CI checks passed (12,415 tests). The VM's singleton array path now counts atomic values after recursive member atomization. Source stays in the existing unpublished **1.26.1** patch; [public 1.26.0](../RELEASE-1.26.0-PUBLICATION.md) and independent Braid validation remain separate.

## Behavior and implementation

[XPath 3.1 atomization](https://www.w3.org/TR/xpath-31/#id-atomization) recursively obtains the atomic values of array members. Empty nested members add no atomic values; member count is not the final atomic cardinality. Singleton consumers retain their required zero/one/multiple handling.

| Expression | Before | Corrected |
|---|---|---|
| `-[[],2]` | XPTY0004 | integer -2 |
| `([[],2]) + 1` | XPTY0004 | integer 3 |
| `([[],2]) eq 2` | XPTY0004 | true |
| `string-join((([[],2]) to 3) ! string(.), ',')` | XPTY0004 | 2,3 |
| `empty(-[[],[]])` | XPTY0004 | true |
| `-[[],1,2]` | XPTY0004 | XPTY0004 |

The array branch calls a private accumulator for each member. It recursively visits arrays, member sequences and JNode values, and delegates node atomization to the existing node path, preserving typed values and FOTY0012. It retains at most one atomic result; a second raises XPTY0004. Empty values are ignored, and map/function items raise FOTY0013. There is no flattened-result list or new public API. Traversal state still grows with nesting depth; this is not a new unbounded-depth or resource-budget guarantee.

The strict helper still rejects genuine multiple atomic values. Its array-cardinality message now refers to multiple atomic values rather than an immediate-member count. Compound erroneous inputs may expose a different encountered error while recursion proceeds; the slice does not certify deterministic precedence among independent errors. Outer sequence cardinality behavior, cast-specific recursion, standard function conversion, caller empty guards and numeric storage remain unchanged.

## Verification

Adds **111 direct/runtime cases**, covering empty members before/after/between nested singleton arrays, member sequences containing arrays, unary and binary arithmetic, range and value equality, empty results, multiple values, map/function refusal, invalid lexical conversion, untyped nodes, decimal-backed wide integers, decimals, the existing duration extension, schema nodes without typed values and backward-compatible numeric conversion.

Before: **92 failed / 19 passed / 0 skipped**. After: **111 passed / 0 failed / 0 skipped**, with unchanged expectations. Original failing and corrected TRXs are retained separately.

The audit's identical 576-case matrix (1,152 raw actual/reference observations) improves from **207 to 84 discrepancies** against independent expectations: **123 corrected cases, no regressions**. All 576 flat-reference outcomes are unchanged, including the 12 known faulty `abs` cardinality references; they remain explicitly excluded as oracles. Unary plus/minus and value equality each match all 48 sampled expectations. Remaining array discrepancies are binary add/multiply/idiv and range empty results (12 each), plus function conversion (36). These diagnostics overlap source coverage and are not added to native conformance counts.

Release build succeeds with zero errors and the existing CS8604 conformance-runner warning. An early focused restore reported unavailable NuGet vulnerability metadata; the refreshed restore and final build have no NU1900 warning. All **12,442 solution + 84 language-server tests pass (12,526 total)**. Fresh QT3 remains **31,142/0/679** and frozen qt4 **8,436/0/1,894** across 224 unchanged configured/executed sets, with 11 existing Experimental fallback passes. Current-native Unix remains **22/2/0, exit 2**, retaining known negative-domain cases 120/121 under the frozen profile. No full current-draft conformance is claimed.

[Machine-readable proof](ARRAY-SINGLETON-ATOMIZATION.json) records validated source fingerprints, gate receipts and exact hashes of [original/corrected tests, raw matrix comparisons and logs](evidence/array-singleton/). Complete solution/LSP TRXs remain under `.guard-tmp/req126-array-singleton-evidence`, with hashes/counters committed. Nine documentation pages render as HTML with unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

## Remaining work

Binary arithmetic and ranges still need their own post-atomization empty-result checks. Standard-function conversion/cardinality, including `abs([2])` and `abs((1,2))`, remains separate. The singleton helper change does not resolve the 84 remaining matrix discrepancies or wider generic integer/coercion limits.

REQ-126/D2/D3/D4/applicable-evidence AC-03 remain partial. The 111 regressions are outside the unchanged 76-entry / 3,148 feature + 24 chapter documentation inventory. No version increment within the existing unpublished patch, fixture/gate/retry change, upstream contribution, release or Braid edit occurs here. Next: review this patch, then correct binary/range post-atomization empty handling as a bounded follow-up.
