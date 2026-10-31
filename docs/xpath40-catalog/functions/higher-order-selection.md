<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Higher-order selection">
  <br><br><h1>Higher-order selection</h1>
  <p>Frozen 4.0</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. Callable/subtyping combinations beyond the verified signatures remain unsupported in this consumer reference. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Higher-order selection — Frozen 4.0.** [Bosak PR #88](https://github.com/Fytala-Charles/Bosak/pull/88) / [immutable revision `0a928bb8`](https://github.com/Fytala-Charles/Bosak/commit/0a928bb81b81d0631cc681f86bba83259c0ed790). This is the shipped engine delivery.

**External specification references (current drafts; links checked 2026-10-11):** [fn:some](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-some), [fn:every](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-every), [fn:index-where](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-index-where). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Validate order lines and locate every rejected line without losing its original position.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

XPath 3.1 offers `some ... satisfies` and `every ... satisfies` quantified expressions. Positional `for` expressions can locate failures, as the linked baseline demonstrates. Function forms make the predicate a reusable function item; neither syntax nor callback contracts are interchangeable automatically.

some/every test whether any/all items satisfy a predicate; index-where returns one-based matching positions. The two-argument callbacks receive item and position. Empty predicate results mean false; arbitrary nonboolean callback results are errors. Empty input gives some=false, every=true and no positions. An explicit existence check is needed when an empty order is invalid.

## 5. Motivation

Validate order lines and locate every rejected line without losing its original position. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

Do not confuse fn:some/fn:every function calls with the XPath 3.1 quantified-expression grammar. Callback positions refer to the input sequence, not document sibling positions.

## 7. Applicability

Use when the consumer opts into XPath40 and needs the exact validation or ordering contract described here. Keep the XPath 3.1 baseline when that opt-in is unavailable.

## 8. Structure

```xquery
string-join(index-where((2, 0, 3), function($quantity, $position) { $quantity le 0 }) ! string(.), ',')
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

some/every test whether any/all items satisfy a predicate; index-where returns one-based matching positions. The two-argument callbacks receive item and position. Empty predicate results mean false; arbitrary nonboolean callback results are errors. Empty input gives some=false, every=true and no positions. An explicit existence check is needed when an empty order is invalid. Do not confuse fn:some/fn:every function calls with the XPath 3.1 quantified-expression grammar. Callback positions refer to the input sequence, not document sibling positions.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Any/all/position selection and vacuous empty results; Item and one-based position callbacks; Boolean/empty result conversion and invalid callback arity. **Boundaries:** Callable/subtyping combinations beyond the verified signatures remain unsupported in this consumer reference. Follow-up groups: F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

**Supplementary contract review (2026-10-12):** [Comparison/selection review](../SELECTION-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed warehouse README](../examples/selection-contracts/README.md) supply 68 checks: keyword modes, collation/default/key/predicate, atomization, result/cardinality boundaries and complete 3.1/4.0 reports. duplicate-values/highest/lowest keywords remain unavailable; index-where keywords work. Map tolerance and tested extrema NaN results remain explicit engine follow-ups. Extrema return boundary-equal items, not all distinct records sharing its key. Initial delivery/unit/gate and historical adopted-pin limits are unchanged.

## 13. Sample Code

[main.xpath](../examples/higher-order-selection/main.xpath): returns `2` at mode 40.

[edge.xpath](../examples/higher-order-selection/edge.xpath): returns `false/true` at mode 40.

[before.xpath](../examples/higher-order-selection/before.xpath): returns `2` at mode 31.

## 14. Performance Notes

Duplicate checks and index selection may traverse the input; some/every may finish early. Partitioning creates arrays and retains input items. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `higher-order-selection-main`, `higher-order-selection-edge`, `higher-order-selection-before`. [`Some_PositionArgument_IsOneBased`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Every_EmptyInput_ReturnsTrue`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`IndexWhere_ReturnsAscendingPositions`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs) The exact-name sets `fn-some`, `fn-every` and `fn-index-where` are not in the current GatedSets; named unit tests and published examples provide evidence. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

