<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak stock-feed container contract reference">
  <br><br><h1>Map and Array Container Contracts</h1>
  <p>Batch boundaries, product totals and callback contracts</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against published Bosak 1.1.3 at `43ed546bd1d61364fe3e61fee736433617473301`. Six Frozen 4.0 families have 89 new representative checks: 18 keyword/mode refusals, 65 focused positional/default/type/callback checks and six complete normal/empty/reordered stock-feed reports in XPath 3.1/4.0. **27 of the original 37 absent-keyword families are reviewed; 10 remain.** Metadata is unchanged. REQ-126 remains In Progress, D2 partial. Catalog: 67 entries, 1,880 feature + 24 chapter checks = 1,904; four worked chapters. No engine/API/harness change, gate promotion or release.

Primary contract: [immutable QT4CG F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). The [raw comparison](audit-signature-contracts.json) records exact signatures and case IDs. Review pins do not recover historical adopted snapshots. Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), ArrayBuild/ArrayEmpty/ArrayItems and MapEntries/MapFilter/MapItems.

## Signatures and keyword availability

| Function | Pinned parameters/defaults | Result |
|---|---|---|
| `array:build` | `input: item()*, action: (fn($item as item(), $pos as xs:integer) as item()*)? := fn:identity#1` | `array(*)` |
| `array:empty` | `array: array(*)` | `xs:boolean` |
| `array:items` | `array: array(*)` | `item()*` |
| `map:entries` | `map: map(*)` | `map(*)*` |
| `map:filter` | `map: map(*), predicate: fn($key as xs:anyAtomicType, $value as item()*, $pos as xs:integer) as xs:boolean?` | `map(*)` |
| `map:items` | `map: map(*)` | `item()*` |

All six keyword expressions refuse XPath 3.1 grammar with XPST0003 and refuse 4.0/experimental binding with XPST0017. Positional functions are available at Frozen 4.0; experimental mode does not supply their missing keyword metadata. Successful 4.0 fixtures also verify default 3.1 refusal in the existing manifest harness.

## Array members and concatenated items

array:build creates one member per input item, in input order, retaining duplicates. Omitted action and explicit () both use identity; (3,1,3) remains 3,1,3. Item-only and item/position callbacks work; positions start at one. Two empty callback results produce two members but zero concatenated items. Two two-item results produce two members, each containing two items, and four concatenated items. Input maps/arrays are single items and remain maps/arrays. XML node identity is preserved.

array:empty tests member count. [] is empty; [()] and [[]] are not. array:items([()]) contains zero items; array:items([[]]) contains one nested array. Concatenation removes one member boundary only and does not recursively flatten nested containers. Integer/map/empty-sequence/multiple-array arguments refuse XPTY0004 in the tested cases.

Zero-argument actions are adapted and return 7,7 for two inputs; arity-three actions refuse XPTY0004. A nonfunction action refuses XPTY0004 for nonempty input, but the tested empty input returns an empty array without invoking it. This is observed evaluation behavior, not a general invalid-argument guarantee. Full function coercion remains unclaimed.

## Map entries, values and filtering

map:entries returns an ordinary sequence of single-entry maps. map{'A':()} still yields one entry map; map:items returns no items for its empty value. Two entries with values (1,2) and 3 yield two entry maps and three value items. Nested arrays/maps remain individual items, and source node identity is preserved. Empty maps produce empty sequences. Wrong integer/array/empty/multiple-map arguments refuse XPTY0004 in the tested entries/items calls. The result-shape corrections already shipped in 1.1.2; this review adds evidence without reopening those fixed issues.

map:filter supports key-only, key/value and key/value/position callbacks. Values are complete entry sequences; sum((1,2)) can select A. An empty predicate result excludes the entry, while a true predicate can retain an entry whose value is empty. The tested position-two callback selects B from the constructed A/B map. Map iteration order is not a portable business ordering contract; sort keys explicitly when reporting. The workflow filters solely on quantity, then sorts keys.

**Boolean-result follow-up:** the pinned predicate result is xs:boolean?. Current MapFilter applies effective boolean value directly: 'yes', numeric 1 and untypedAtomic('true') each retain A/B. Multiple booleans refuse FORG0006. Preserve these observed boundaries for a focused function-conversion/result-enforcement review; do not advertise string/numeric predicates as a supported business pattern. Zero-argument predicates are adapted; arity-four/nonfunction predicates refuse XPTY0004 on nonempty maps. An empty map with a nonfunction predicate returns an empty map in the tested case without invocation.

## Worked workflow and next review

The [detailed stock-feed README](examples/container-contracts/README.md) supplies complete inputs, paired expressions, exact outputs, domain policies and every focused fixture. Existing [array batches](functions/import-array-batches.md) and [product map index](functions/product-map-index.md) retain original delivery/unit/gate evidence. This diagnostic workflow is not an additional worked chapter.

Next review date/time construction and utility contracts: build-dateTime, days-in-month, unix-dateTime, seconds and duration-to-seconds. The other five absent families are build-uri, decode-from-uri, collation-available, identity and is-NaN. Complete conversion/options, exhaustive changed-3.1 inventory, D3 grammar/pattern and D4 provenance/acceptance remain; historical adopted pins and AC-03 gate applicability are unresolved. No closure or release follows from sample coverage.

## Delivery validation

Release solution build passed (11 existing warnings, zero errors); 6,108 tests passed with no failures or skips across ten assemblies. Render checks verified 95 catalog pages and six implementation/canonical pages. Six pinned signatures, exact case IDs, independent full report outputs, complete README expressions, local links, 67 sixteen-field entries, 241 cached primary links/anchors, 28 historical delivery records and 117 assigned names were checked. No manual engine gates were rerun for this documentation/example scope.


The subsequent [date/time review](DATE-TIME-CONTRACT-REVIEW.md) brings targeted absent-keyword review to 32 of 37 families, with five remaining. Metadata is unchanged. Current catalog: 67 entries, 1,988 feature + 24 chapter checks = 2,012, version 1.1.3. Earlier counts and queues are historical. Next: URI/import utilities; D2 stays partial.
