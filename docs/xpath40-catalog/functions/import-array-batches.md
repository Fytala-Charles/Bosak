<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Import batches with arrays">
  <br><br><h1>Import batches with arrays</h1>
  <p>Frozen 4.0 — verified reference contracts</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. array:members/of-members are JNode functions owned by C05, not replacements for member/item concatenation. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Import batches with arrays — Frozen 4.0.** Shipped in [Bosak PR #84](https://github.com/Fytala-Charles/Bosak/pull/84) at immutable revision [`f7f55b4e`](https://github.com/Fytala-Charles/Bosak/commit/f7f55b4e3a6e92da26ac607e34f46aede695cb19).

Functions: [array:build](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-build), [array:empty](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-empty), [array:items](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-items), [array:slice](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-slice). Definitions reviewed 2026-10-11 against immutable [QT4CG function source `a014e1b4`](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml). This is a review pin, not evidence of the historical adopted snapshot, which remains unavailable.

## 2. Intent

Keep every order’s product-code sequence in one array member, select a two-order import batch, and concatenate the lines only at the export boundary. One order can contain several lines without becoming several orders.

## 3. Also Known As

Exact spellings are `array:build`, `array:empty`, `array:items` and `array:slice`. No aliases are introduced. `array:items` performs one-level concatenation; `array:flatten` is the recursive alternative.

## 4. History / Supersedes

XPath 3.1 uses `fold-left` and `array:append` to construct one member per order, then `array:subarray` for the illustrated forward batch and `?*` for one-level concatenation. `array:subarray` is not a general substitute for negative bounds, reverse slices, strides or forgiving out-of-range selection.

## 5. Motivation

Flattening line sequences before batching destroys order boundaries. Building one array member per callback invocation preserves them even when a callback returns several items or the empty sequence.

## 6. Language Alignment

Array members can be arbitrary XPath sequences, unlike a simple list of scalar JSON values. Addresses are one-based. Slice end positions are inclusive, unlike Python and JavaScript slices; negative values count from the end. `array:items` concatenates one level and retains nested arrays, while `array:flatten` is recursive.

## 7. Applicability

Use for bounded selections from an already materialized batch. `array:empty([()])` is false: there is one member even though it contains no items. Distinguish no orders from an order with no lines, and validate that business condition separately.

## 8. Structure

```xquery
array:build($orders, function($order,$position){$order?lines})
array:slice($members, $start, $end, $step)
array:empty($members)
array:items($members)
```

Positional and shorter callback forms are exercised by the exact files below. This entry covers the illustrated contracts rather than every arity or option.

## 9. Participants

Existing `FunctionLibrary` frozen registrations select these names only at `XPathCompatibility.XPath40` or higher. The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) compiles each file at its declared mode and checks default XPath31 refusal for successful 4.0 forms.

## 10. Collaborations

The map family indexes products while this array family preserves order boundaries. The [sequence-selection family](order-sequence-selection.md) uses the same slice position rules for ordinary sequences.

## 11. Consequences

Each callback result is one array member. `array:items` loses those boundaries, so call it only when flattening one level is intended. `[]`, `[()]` and `[[]]` have different member counts. Empty bounds or zero select defaults; a negative step defaults the omitted start/end to opposite sequence ends. An omitted/zero step chooses direction from the resolved bounds. An explicit incompatible direction returns an empty array, and out-of-range bounds do not raise the `array:subarray` bounds error.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles One callback result per member and one-level item concatenation; Zero members versus empty/nested members; Inclusive/negative slice bounds, default/zero step and empty direction result. **Boundaries:** array:members/of-members are JNode functions owned by C05, not replacements for member/item concatenation. Follow-up groups: F01, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Implementation: [FunctionLibrary.cs](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs); [named unit tests](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [manifest](../examples/cases.json). Names refuse XPST0017 in default XPath31; the comparisons remain valid at XPath31. Source/package version stays 1.1.1. This reference work changes no engine code.

**Supplementary contract review (2026-10-12):** [Container review](../CONTAINER-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed stock-feed README](../examples/container-contracts/README.md) supply 89 checks covering six families: keyword modes, empty/default/member/item shapes, nested values, node identity, callback arity/results and complete XPath 3.1/4.0 reports. Keywords remain unavailable. map:filter boolean-result enforcement remains an engine follow-up. Source/public version is 1.1.3; original delivery/unit/gate evidence and historical adopted-pin limits are unchanged. The map result-shape corrections already shipped in 1.1.2.

## 13. Sample Code

Input is declared inside each expression: reduced order/product records, no document focus, external variables or resource access. Namespace prefixes `map` and `array` use the standard public API bindings. Outputs are exact text projections of XDM results, not serialized supplier or ERP formats.

**Main — mode 40:** [main.xpath](../examples/import-array-batches/main.xpath)

```xquery
(: One array member holds all product codes for one order. :)
let $orders := (map{'id':'ORD-101','lines':('FILTER-A','SEAL-B')}, map{'id':'ORD-102','lines':('VALVE-C')}, map{'id':'ORD-103','lines':('HOSE-D','CLIP-E')}),
    $members := array:build($orders, function($order,$position){$order?lines}),
    $batch := array:slice($members, 1, 2)
return concat('orders=', array:size($batch), ';lines=', string-join(array:items($batch), ','))
```

Expected text:

```text
orders=2;lines=FILTER-A,SEAL-B,VALVE-C
```

**Baseline — mode 31:** [baseline.xpath](../examples/import-array-batches/baseline.xpath)

```xquery
let $orders := (map{'id':'ORD-101','lines':('FILTER-A','SEAL-B')}, map{'id':'ORD-102','lines':('VALVE-C')}, map{'id':'ORD-103','lines':('HOSE-D','CLIP-E')}),
    $members := fold-left($orders, [], function($array,$order){array:append($array,$order?lines)}),
    $batch := array:subarray($members, 1, 2)
return concat('orders=', array:size($batch), ';lines=', string-join($batch?*, ','))
```

Expected text:

```text
orders=2;lines=FILTER-A,SEAL-B,VALVE-C
```

**Edge — mode 40:** [edge.xpath](../examples/import-array-batches/edge.xpath)

```xquery
let $batch := array:slice(array:build(()), 1, 2) return concat(array:size($batch), '/', array:empty($batch), '/', count(array:items($batch)))
```

Expected text:

```text
0/true/0
```

**Additional executable contracts:**

| Case | Mode | Expected text or error |
|---|---|---|
| [empty-member](../examples/import-array-batches/empty-member.xpath) | 40 | `true/false/false/0` |
| [nested-array](../examples/import-array-batches/nested-array.xpath) | 40 | `3/true` |
| [positions](../examples/import-array-batches/positions.xpath) | 40 | `1:ORD-101&#124;2:ORD-102` |
| [reverse](../examples/import-array-batches/reverse.xpath) | 40 | `ORD-103,ORD-102,ORD-101` |
| [out-of-range](../examples/import-array-batches/out-of-range.xpath) | 40 | `true` |
| [negative-bounds](../examples/import-array-batches/negative-bounds.xpath) | 40 | `ORD-102,ORD-103` |
| [step](../examples/import-array-batches/step.xpath) | 40 | `ORD-101,ORD-103` |
| [incompatible-direction](../examples/import-array-batches/incompatible-direction.xpath) | 40 | `0` |


## 14. Performance Notes

Construction retains one member per input item; slicing creates a selected-member array. This implementation materializes input and positions. None of the examples establishes bounded-memory streaming.

## 15. Known Uses

[`ArrayBuild_ResultSequenceBecomesSingleMember`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ArrayBuild_ActionReceivesItemAndPosition`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ArrayEmpty_OnlyZeroMemberArrayIsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ArrayItems_ConcatenatesMembersNonRecursively`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ArraySlice_StartEndStep`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ArraySlice_OutOfBoundsYieldsEmptyArrayNoError`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).

Exact sets `array-build`, `array-items` and `array-slice` are in GatedSets. No exact `array-empty` set is assigned; its named unit and manifest checks remain separate evidence. No sets are promoted by this step. See the [actual gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs). Existing 1.1.1 release gate counts are historical evidence; no engine gates are rerun for this documentation slice.

## 16. Related Features

[Array member iteration](../operators-and-syntax/array-member-iteration.md), [product maps](product-map-index.md), [sequence selection](order-sequence-selection.md). [Completeness audit](../COMPLETENESS-AUDIT.md).
