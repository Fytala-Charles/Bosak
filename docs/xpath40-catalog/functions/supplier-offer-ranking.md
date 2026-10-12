<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Supplier Offer Ranking">
  <br><br><h1>Supplier Offer Ranking</h1>
  <p>Frozen 4.0 — verified integration contracts and limitations</p>
</div>

## 1. Name and Classification

Functions: [fn:highest](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-highest), [fn:lowest](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-lowest), [fn:sort-by](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-sort-by), [fn:collation-available](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-collation-available). Frozen 4.0.
Original ranking delivery [PR #83](https://github.com/Fytala-Charles/Bosak/pull/83), [532d04f4](https://github.com/Fytala-Charles/Bosak/commit/532d04f4672df430d93752e3158fc3cdfd4c7c39); collation availability [PR #114](https://github.com/Fytala-Charles/Bosak/pull/114), [33894747](https://github.com/Fytala-Charles/Bosak/commit/33894747f04da970a1f3c0f17fa960fa1f620605).

Reviewed 2026-10-12 against immutable [QT4CG function source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). This is a current review pin, not recovery of the unavailable historical adopted snapshot.

## 2. Intent

Rank supplier offers by ascending price and descending quality score, report all cheapest offers, and inspect whether a requested string collation is available.

## 3. Also Known As

Boundary-item selection, multi-key ranking and collation preflight are descriptive terms; no aliases are registered.

## 4. History / Supersedes

XPath 3.1 uses min/max plus explicit filters for boundary-key matches. Stable sorting by the minor score key first, then the major price key, implements this two-key ranking. The comparison omits collation availability: standard 3.1 has no direct availability function. A host must preflight or supply a known supported collation.

## 5. Motivation

Two suppliers can quote the same price but have different scores. A procurement report needs both tied cheapest quotes while ranking them by score. Selecting one boundary record alone does not implement that business rule.

## 6. Language Alignment

highest/lowest sort by an optional key and return boundary-related input items. Untyped keys are treated numerically unless an explicit string key changes that rule. In Bosak, the final membership comparison is against the whole boundary item (contextual/deep equality), not just its computed key. The observed two-node price-tie case returns B, although B and C share price 8. The report therefore filters all offers by the selected boundary price explicitly.

The pinned draft's input-item contextual-equality rules and its computed-key tie examples need follow-up reconciliation with this engine behavior. This reference records the observed contract, not a certification of all computed-key ties.

sort-by accepts key-definition maps in major-to-minor order, including key, order and collation fields. Equal full sort keys retain input order. collation-available reports whether a URI is recognized; it does not fetch that URI. Numeric comparisons do not require a string collation.

## 7. Applicability

Use when numeric prices and scores are present, valid and comparable; the fixture uses decimal values and no NaN/missing keys. Lowest price is the primary rule and highest score the secondary. No purchasing action is taken. If every equal-price offer must be retained, use the explicit filter shown here. String ordering policy needs a deliberate collation or fallback.

## 8. Structure

Read offers, define numeric price/score callbacks, select a lowest boundary, filter all equal-price records, and sort with price ascending then score descending.

## 9. Participants

Offer nodes supply identifiers and metrics. Key callbacks supply comparable decimals. Boundary selection provides a reference price. Filtering retains all tied quotes. Multi-key sort orders the report.

## 10. Collaborations

Use simple-map projection `! string(@id)` after sorting nodes. A path such as `$ranked/@id` normalizes node results to document order and can discard the ranking. Empty offers bypass the boundary-price callback. The fallback fixture selects codepoint collation when the requested URI is unavailable.

## 11. Consequences

Ties are explicit and the report preserves computed ranking. Lowest/highest are not a substitute for an all-equal-key selection policy on arbitrary records. An invalid key or order raises the demonstrated errors. Checking collation availability does not certify every collation feature.

## 12. Implementation

Compile new functions with CompileOptions.Compatibility40; default 3.1 rejects them. Earlier-version comparisons use Compatibility31. The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) verifies exact text/error markers and default-profile refusal for successful 4.0 cases. All values are inline and deterministic.

**Supplementary contract review (2026-10-12):** [Comparison/selection review](../SELECTION-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed warehouse README](../examples/selection-contracts/README.md) supply 68 checks: keyword modes, collation/default/key/predicate, atomization, result/cardinality boundaries and complete 3.1/4.0 reports. duplicate-values/highest/lowest keywords remain unavailable; index-where keywords work. Map tolerance and tested extrema NaN results remain explicit engine follow-ups. Extrema return boundary-equal items, not all distinct records sharing its key. Initial delivery/unit/gate and historical adopted-pin limits are unchanged.

**Supplementary contract review (2026-10-12):** [URI/import utility review](../URI-UTILITY-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed import/link README](../examples/utility-contracts/README.md) supply 105 checks across five families: keyword modes, component/options/escaping, one-pass UTF-8 decoding, collation availability, identity, NaN and type/cardinality boundaries, plus complete XPath 3.1/4.0 reports. Keywords remain unavailable. Boolean-option/record tolerance and empty query-map precedence remain follow-ups. All 37 absent-keyword families now have first targeted review, with metadata still absent; this does not close D2. Source/public version is 1.1.3; original delivery/unit/gate and historical adopted-pin limits remain.

**Existing-function review:** The [complete baseline-name inventory](../CHANGED-31-CONTRACT-INVENTORY.md) records unavailable three-argument map/array getters and empty sort-key/map:merge-option boundaries. The [stock examples](../examples/changed31-contracts/README.md) show explicit portable fallback policy. This review does not implement draft defaults or close D2.

## 13. Sample Code

Complete synthetic input:

```xml
<offers><offer id="A" price="10" score="90"/><offer id="B" price="8" score="90"/><offer id="C" price="8" score="85"/><offer id="D" price="12" score="92"/></offers>
```

### XPath 4.0 expression

[Runnable file](../examples/supplier-offer-ranking/main.xpath):

```xpath
let $offers := parse-xml('<offers><offer id="A" price="10" score="90"/><offer id="B" price="8" score="90"/><offer id="C" price="8" score="85"/><offer id="D" price="12" score="92"/></offers>')/offers/offer
let $price := function($o) { xs:decimal($o/@price) }
let $score := function($o) { xs:decimal($o/@score) }
let $boundary := lowest($offers, (), $price)
let $cheapest := if (empty($boundary)) then () else $offers[$price(.) eq $price($boundary[1])]
let $collation := 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return concat('cheapest=', string-join($cheapest/@id, ','), '; bestScore=', string-join(highest($offers, (), $score)/@id, ','), '; ranked=', string-join(sort-by($offers, (map {'key': $price}, map {'key': $score, 'order': 'descending'})) ! string(@id), ','), '; collationAvailable=', collation-available($collation))
```

Exact output:

```text
cheapest=B,C; bestScore=D; ranked=B,C,A,D; collationAvailable=true
```

### XPath 3.1 comparison

[Runnable file](../examples/supplier-offer-ranking/baseline.xpath):

```xpath
let $offers := parse-xml('<offers><offer id="A" price="10" score="90"/><offer id="B" price="8" score="90"/><offer id="C" price="8" score="85"/><offer id="D" price="12" score="92"/></offers>')/offers/offer
return let $cheap := min($offers/@price ! xs:decimal(.))
return let $best := max($offers/@score ! xs:decimal(.))
return let $byScore := sort($offers, (), function($o) { -xs:decimal($o/@score) })
return let $ranked := sort($byScore, (), function($o) { xs:decimal($o/@price) })
return concat('cheapest=', string-join($offers[xs:decimal(@price) eq $cheap]/@id, ','), '; bestScore=', string-join($offers[xs:decimal(@score) eq $best]/@id, ','), '; ranked=', string-join($ranked ! string(@id), ','))
```

Exact output:

```text
cheapest=B,C; bestScore=D; ranked=B,C,A,D
```

### Complete fixture inventory

Each file embeds its full input and exact result or diagnostic marker.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/supplier-offer-ranking/main.xpath) | `cheapest=B,C; bestScore=D; ranked=B,C,A,D; collationAvailable=true` |
| [baseline](../examples/supplier-offer-ranking/baseline.xpath) | `cheapest=B,C; bestScore=D; ranked=B,C,A,D` |
| [empty-offers](../examples/supplier-offer-ranking/empty-offers.xpath) | `cheapest=; bestScore=; ranked=; collationAvailable=true` |
| [stable-ties](../examples/supplier-offer-ranking/stable-ties.xpath) | `B,A,C` |
| [all-boundary-ties](../examples/supplier-offer-ranking/all-boundary-ties.xpath) | `2` |
| [default-sort](../examples/supplier-offer-ranking/default-sort.xpath) | `1,2,3` |
| [descending](../examples/supplier-offer-ranking/descending.xpath) | `3,2,1` |
| [numeric-untyped](../examples/supplier-offer-ranking/numeric-untyped.xpath) | `a` |
| [string-key](../examples/supplier-offer-ranking/string-key.xpath) | `b` |
| [available-ascii](../examples/supplier-offer-ranking/available-ascii.xpath) | `true` |
| [unavailable](../examples/supplier-offer-ranking/unavailable.xpath) | `false` |
| [fallback](../examples/supplier-offer-ranking/fallback.xpath) | `A,b` |
| [invalid-order](../examples/supplier-offer-ranking/invalid-order.xpath) | `FORG0001` |
| [invalid-record](../examples/supplier-offer-ranking/invalid-record.xpath) | `XPTY0004` |
| [bad-untyped](../examples/supplier-offer-ranking/bad-untyped.xpath) | `FORG0001` |
| [computed-key-tie-observed](../examples/supplier-offer-ranking/computed-key-tie-observed.xpath) | `B` |

The 3.1 report verifies the same business ranking and cheapest-price set; it omits availability preflight. The computed-key-tie-observed fixture intentionally records B, not an asserted all-price-ties result. Stable-ties verifies equal sort keys separately.

## 14. Performance

Key computation, sorting and boundary filtering materialize these small inputs. The 3.1 comparison performs two stable sorts. No benchmark or universal complexity claim is implied.

## 15. Known Uses

Named evidence: [`Highest_EqualBoundaryItems_AllRetainedInInputOrder`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Highest_WithComputedKey`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Lowest_SelectsMinimum`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`SortBy_WithKeyFunction_StableForEqualKeys`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`SortBy_MultipleKeyDefinitions_MajorToMinor`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`CollationAvailable_ReportsRecognizedCollations`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs). This supplier scenario is synthetic.
The [gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) assigns exact fn-collation-available, not exact highest/lowest/sort-by sets. These 16 fixtures provide separate evidence. No promotion is included.

## 16. Related Features

[Numeric import diagnostics](numeric-import-diagnostics.md) covers invalid numeric data before aggregation. [The audit](../COMPLETENESS-AUDIT.md) records the computed-key tie follow-up and partial conformance.
