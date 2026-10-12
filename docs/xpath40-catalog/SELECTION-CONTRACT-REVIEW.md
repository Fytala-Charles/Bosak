<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak warehouse comparison and selection reference">
  <br><br><h1>Comparison and Selection Contracts</h1>
  <p>Warehouse exceptions, extrema and position-aware selection</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against Bosak `211ba1ce88ad62b4a5bfb2cfbb1b91c0b8c09cb7` (PR #190 delivery branch), version 1.1.2. Four Frozen 4.0 families have representative keyword, collation/default, key/predicate, empty and typed-value evidence. Three extend the absent-keyword review; index-where already has metadata. **21 of the original 37 absent-keyword families are reviewed; 16 remain.** Metadata and engine behavior are unchanged. REQ-126 stays In Progress, D2 partial.

68 new checks: 12 explicit keyword/mode cases, 50 focused positional/default/type/callback/keyword-validation checks and six complete normal/empty/reordered warehouse reports in 3.1/4.0. Catalog: 67 entries, 1,791 feature + 24 chapter checks = 1,815, four worked chapters. No engine/API/harness changes, gate promotion or release.

Primary contract: [immutable QT4CG F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). [Raw comparison](audit-signature-contracts.json) links exact cases. This review pin does not recover historical adopted specification snapshots. Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), DuplicateValues, HighestLowest/ModifiedKey and IndexWhere_2.

## Declared signatures and keywords

| Function | Pinned parameters/defaults | Result |
|---|---|---|
| `fn:duplicate-values` | `values: xs:anyAtomicType*, collation: xs:string? := fn:default-collation()` | `xs:anyAtomicType*` |
| `fn:highest` | `input: item()*, collation: xs:string? := fn:default-collation(), key: (fn($item as item()) as xs:anyAtomicType*)? := fn:data#1` | `item()*` |
| `fn:index-where` | `input: item()*, predicate: fn($item as item(), $pos as xs:integer) as xs:boolean?` | `xs:integer*` |
| `fn:lowest` | `input: item()*, collation: xs:string? := fn:default-collation(), key: (fn($item as item()) as xs:anyAtomicType*)? := fn:data#1` | `item()*` |

All four keyword expressions refuse 3.1 grammar XPST0003. duplicate-values/highest/lowest refuse 4.0/experimental binding XPST0017. index-where keywords succeed in both modes; reversed argument order succeeds and missing predicate refuses XPST0017. Experimental mode does not add the other families' missing metadata.

## Positional/default and typed contracts

duplicate-values returns the second occurrence from each equality class, in second-occurrence order: A,B,B,A,B,A gives B,A. It does not return every repeated occurrence or first-occurrence order. Numeric equivalents group together; multiple NaNs produce one NaN. XML elements atomize to text. Explicit () collation selects the default; HTML ASCII case-insensitive A,a,B,b returns a,b. Unsupported collation refuses FOCH0002. A two-map input is accepted and yields count zero despite the atomic values signature: preserve as a type/atomization follow-up, not a supported business idiom.

highest/lowest default their key to data#1, including explicit (). Untyped numeric node text converts to double: 2/10 orders numerically, with original source nodes retained (identity verified). Nonnumeric untyped A/B refuses FORG0001. Repeated numeric maxima retain source order and multiplicity. Tuple keys are accepted; arity-two and nonfunction keys refuse XPTY0004. Empty input returns empty in the tested invalid-collation case before collation validation. This is observed evaluation behavior, not a promise that invalid arguments are generally accepted.

The pinned rule sorts by modified keys, then returns original items contextually equal to the boundary item, in source order. It is not a generic “return every record with the same maximum key” helper: the tested A/B map records share score 9, but highest returns only B, the selected boundary map. Distinct records with equal keys remain distinct. Case-insensitive A/a atomic input returns both. A callback returning an empty key yields boundary 9 for highest and 2 for lowest in the tested (2,9) input, rather than selecting every item.

The tested (1,NaN,2) input returns 1 for highest and 2 for lowest. These counterintuitive outcomes are recorded for a dedicated NaN/sort-order review; do not infer usable measurement extrema or certify full ordering from this sample. The business workflow uses validated integers without NaN.

index-where returns ascending one-based positions, accepts item-only and item/position predicates, and treats empty result as false. Odd-position selection of A/B/C gives 1,3. String 'yes', multiple booleans, nonfunction and arity-three predicates refuse XPTY0004 in the tested cases; untypedAtomic('true') converts to true. Full function coercion and all callable forms remain unclaimed.

## Workflow and remaining work

The [detailed warehouse README](examples/selection-contracts/README.md) supplies complete XML, paired code, exact outputs and every focused fixture. Existing [sequence identity](functions/sequence-distinctness.md), [supplier ranking](functions/supplier-offer-ranking.md) and [higher-order selection](functions/higher-order-selection.md) entries retain their unit/gate and historical delivery evidence. These diagnostic examples do not create another worked chapter.

Next review absent-keyword map/array container families: map:entries/map:items/map:filter and array:build/array:items/array:empty. Use a stock-feed normalization example with explicit sequence/member/result-shape and callback cases. Other missing metadata, complete coercion/options, exhaustive changed-3.1 inventory and D3/D4 acceptance remain. Historical pins and AC-03 applicability are unresolved; no closure or release based on sample coverage.
