<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Product indexing with maps">
  <br><br><h1>Product indexing with maps</h1>
  <p>Frozen 4.0 — verified reference contracts</p>
</div>

## 1. Name and Classification

**Product indexing with maps — Frozen 4.0.** Shipped in [Bosak PR #84](https://github.com/Fytala-Charles/Bosak/pull/84) at immutable revision [`f7f55b4e`](https://github.com/Fytala-Charles/Bosak/commit/f7f55b4e3a6e92da26ac607e34f46aede695cb19).

Functions: [map:build](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-build), [map:entries](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-entries), [map:filter](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-filter), [map:items](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-items). Definitions reviewed 2026-10-11 against immutable [QT4CG function source `a014e1b4`](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml). This is a review pin, not evidence of the historical adopted snapshot, which remains unavailable.

## 2. Intent

Turn a supplier stock snapshot into a SKU index, apply an explicit duplicate policy, and retain available products. The input deliberately contains a later correction for FILTER-A.

## 3. Also Known As

Exact spellings are `map:build`, `map:entries`, `map:filter` and `map:items`. No aliases are introduced. `map:entries` is a function; map-entry iteration syntax is a separate feature.

## 4. History / Supersedes

The XPath 3.1 comparison uses `map:merge` over single-entry maps and explicitly requests `use-last`. It sorts keys for presentation because a portable 3.1 map does not promise entry order. This agrees on the one surviving SKU, not on every 4.0 ordering contract.

## 5. Motivation

A duplicate SKU can mean a corrected snapshot, an additive delivery, or invalid input. Those meanings require different policies. The main case chooses the last snapshot value (8), while the combiner probe totals two deliveries (20). Neither policy is a substitute for business validation.

## 6. Language Alignment

Do not assume JavaScript Map overwrite semantics or SQL grouping: `map:build` defaults to sequence concatenation (`combine`), not use-last. Keys use XPath same-key rules. A key callback may return several keys or no keys. Callbacks receive the item and one-based input position; `map:filter` receives key, value and one-based entry position. Lower-arity callbacks can ignore surplus arguments.

## 7. Applicability

Use for deterministic indexing of already parsed records. Reject duplicates when identities must be unique. Do not use a sum combiner for a snapshot feed, or silently drop missing SKU keys when the application must report them. These fixtures simplify the vocabulary and do not implement supplier-feed validation.

## 8. Structure

```xquery
map:build($rows, $key, $value, $options)
map:filter($index, function($key,$value,$position){...})
map:entries($index)
map:items($index)
```

Positional and shorter callback forms are exercised by the exact files below. This entry covers the illustrated contracts rather than every arity or option.

## 9. Participants

Existing `FunctionLibrary` frozen registrations select these names only at `XPathCompatibility.XPath40` or higher. The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) compiles each file at its declared mode and checks default XPath31 refusal for successful 4.0 forms.

## 10. Collaborations

Maps can be supplied as stylesheet parameters; see the [order-routing chapter](../worked-examples/order-routing/README.md). Preserve source rows separately if discarded corrections or zero-stock items must appear in an exception report.

## 11. Consequences

**Published 1.1.2 correction:** `map:entries` now returns a sequence of
single-entry maps in entry order. Empty input returns the empty sequence; a
multi-item value remains whole inside its single-entry map. `map:items`
concatenates the item sequences associated with entries, omitting empty values
and retaining nested maps/arrays as items rather than recursively flattening them.
The `entries-boundary` result is `false/2`; `items-sequence-boundary` is `3`,
matching the portable comparisons. No `?*` adapter is needed for `map:entries`.

**Migration from published 1.1.1:** that version returned an array from
`map:entries` and retained grouped sequence values from `map:items`. Replace
`map:entries($map)?*` with `map:entries($map)`, and array indexing of the entry
result with sequence predicates such as `[2]`. Item counts now reflect the
concatenated values. Review code that relied on the old grouped cardinality;
portable `map:for-each` alternatives remain valid. This is a correction of the
supported 4.0 draft contracts; default XPath31 still rejects these new names.
Version 1.1.2 is published with the corrected contracts. See the
[correction dossier](../../REQ-118-MAP-RESULT-SHAPES.md).

`use-any` leaves the choice implementation-defined; Bosak currently retains the first value. It is unsuitable when the business requires a specific correction to win.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Duplicate combine/use-first/use-last/reject and custom combining policies; Single-entry-map sequence and concatenated item result shapes; Entry-position callbacks, empty values, nesting and node identity. **Boundaries:** map:filter currently accepts nonboolean truthy predicate results; do not use them. Map enumeration is not portable business order. Follow-up groups: F01, F07, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Implementation: [FunctionLibrary.cs](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs); [named unit tests](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [manifest](../examples/cases.json). Names refuse XPST0017 in default XPath31; the comparisons remain valid at XPath31. The original reference delivery changed no engine code. The later result-shape correction targets source 1.1.2; see the linked correction dossier for its regressions and gates.

**Supplementary contract review (2026-10-12):** [Container review](../CONTAINER-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed stock-feed README](../examples/container-contracts/README.md) supply 89 checks covering six families: keyword modes, empty/default/member/item shapes, nested values, node identity, callback arity/results and complete XPath 3.1/4.0 reports. Keywords remain unavailable. map:filter boolean-result enforcement remains an engine follow-up. Source/public version is 1.1.3; original delivery/unit/gate evidence and historical adopted-pin limits are unchanged. The map result-shape corrections already shipped in 1.1.2.

**Existing-function review:** The [complete baseline-name inventory](../CHANGED-31-CONTRACT-INVENTORY.md) records unavailable three-argument map/array getters and empty sort-key/map:merge-option boundaries. The [stock examples](../examples/changed31-contracts/README.md) show explicit portable fallback policy. This review does not implement draft defaults or close D2.

**Existing-function default review (2026-10-12):** The [ordering/default review](../ORDER-DEFAULT-CONTRACT-REVIEW.md) and [detailed supplier README](../examples/order-contracts/README.md) add 132 checks across sort/array:sort/min/max/map:merge, explicit key/options and collation/NaN/cardinality boundaries, and six full 3.1/4.0 reports. Supplier price-plus-ID keys preserve business tie-breaking under source reordering. Use simple mapping to project sorted node IDs; a path restores document order. This is targeted evidence, not an engine fix or full D2 completion.

## 13. Sample Code

Input is declared inside each expression: reduced order/product records, no document focus, external variables or resource access. Namespace prefixes `map` and `array` use the standard public API bindings. Outputs are exact text projections of XDM results, not serialized supplier or ERP formats.

**Main — mode 40:** [main.xpath](../examples/product-map-index/main.xpath)

```xquery
(: A supplier snapshot contains a later correction for FILTER-A. :)
let $rows := (map{'sku':'FILTER-A','stock':12}, map{'sku':'SEAL-B','stock':0}, map{'sku':'FILTER-A','stock':8}),
    $index := map:build($rows, function($row){$row?sku}, function($row){$row?stock}, map{'duplicates':'use-last'}),
    $available := map:filter($index, function($sku,$stock,$position){$stock gt 0})
return string-join(map:entries($available) ! (let $entry := . return let $sku := map:keys($entry)[1] return concat($sku, '=', $entry($sku))), '|')
```

Expected text:

```text
FILTER-A=8
```

**Baseline — mode 31:** [baseline.xpath](../examples/product-map-index/baseline.xpath)

```xquery
let $rows := (map{'sku':'FILTER-A','stock':12}, map{'sku':'SEAL-B','stock':0}, map{'sku':'FILTER-A','stock':8}),
    $index := map:merge($rows ! map:entry(?sku, ?stock), map{'duplicates':'use-last'})
return string-join(for $sku in sort(map:keys($index)) return if ($index($sku) gt 0) then concat($sku, '=', $index($sku)) else (), '|')
```

Expected text:

```text
FILTER-A=8
```

**Edge — mode 40:** [edge.xpath](../examples/product-map-index/edge.xpath)

```xquery
concat(map:size(map:build((), function($r){$r?sku})), '/', map:size(map:filter(map{'SEAL-B':0}, function($k,$v){$v gt 0})), '/', count(map:items(map{})))
```

Expected text:

```text
0/0/0
```

**Additional executable contracts:**

| Case | Mode | Expected text or error |
|---|---|---|
| [combine](../examples/product-map-index/combine.xpath) | 40 | `12,8` |
| [reject](../examples/product-map-index/reject.xpath) | 40 | `FOJS0003` |
| [invalid-policy](../examples/product-map-index/invalid-policy.xpath) | 40 | `FOJS0005` |
| [combiner](../examples/product-map-index/combiner.xpath) | 40 | `20` |
| [multi-key](../examples/product-map-index/multi-key.xpath) | 40 | `2/FILTER-A` |
| [empty-key](../examples/product-map-index/empty-key.xpath) | 40 | `0` |
| [positions](../examples/product-map-index/positions.xpath) | 40 | `1:FILTER-A&#124;2:SEAL-B` |
| [empty-predicate](../examples/product-map-index/empty-predicate.xpath) | 40 | `0` |
| [items-sequence-boundary](../examples/product-map-index/items-sequence-boundary.xpath) | 40 | `3` |
| [items-baseline](../examples/product-map-index/items-baseline.xpath) | 31 | `3` |
| [entries-boundary](../examples/product-map-index/entries-boundary.xpath) | 40 | `false/2` |
| [entries-baseline](../examples/product-map-index/entries-baseline.xpath) | 31 | `2` |


## 14. Performance Notes

Build/filter traverse input records or map entries and retain output maps. Duplicate combination may retain all contributing values. The example constructs a report string after indexing; no streaming or constant-memory claim is made.

## 15. Known Uses

[`MapBuild_DuplicatesUseLast_KeepsLastValue`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`MapBuild_DuplicatesReject_RaisesFojs0003`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`MapBuild_DuplicatesCombinerFunction_AppliesCombiner`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`MapEntries_ReturnsSingleEntryMapsInOrder`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`MapFilter_EmptyPredicateResultMeansFalse`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`MapItems_ReturnsValuesInEntryOrder`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).

No exact-name map-build, map-entries, map-filter or map-items set is in the current GatedSets. Named unit tests and manifest cases provide the evidence here; mapping the names does not establish conformance. See the [actual gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs). The original 1.1.1 release gate counts remain historical evidence. The subsequent source-1.1.2 correction reruns engine gates separately; see its dossier. No map test set is promoted by that correction.

## 16. Related Features

[Map entry iteration](../operators-and-syntax/map-entry-iteration.md), [array batches](import-array-batches.md), [structural records](../type-system/structural-records.md). [Completeness audit](../COMPLETENESS-AUDIT.md).
