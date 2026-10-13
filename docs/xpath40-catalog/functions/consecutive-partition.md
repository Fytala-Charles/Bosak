<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Consecutive partitioning">
  <br><br><h1>Consecutive partitioning</h1>
  <p>Frozen 4.0</p>
</div>

## 1. Name and Classification

**Consecutive partitioning — Frozen 4.0.** [Bosak PR #88](https://github.com/Fytala-Charles/Bosak/pull/88) / [immutable revision `0a928bb8`](https://github.com/Fytala-Charles/Bosak/commit/0a928bb81b81d0631cc681f86bba83259c0ed790). This is the shipped engine delivery.

**External specification references (current drafts; links checked 2026-10-11):** [fn:partition](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-partition). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Split a dispatch queue into bounded batches while preserving arrival order.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

XPath 3.1 requires explicit recursion or stateful grouping logic to split at a boundary depending on the current partition. Grouping by route would merge nonadjacent orders, so it is not a semantics-preserving replacement for consecutive partitioning.

partition returns a sequence of arrays. Its split callback receives the current nonempty partition sequence, next item and one-based input position. A true result starts the next partition before that item. Empty input returns no partitions; a singleton returns one array and the callback is not invoked for its first item. The worked chapter combines capacity and route-change boundaries.

## 5. Motivation

Split a dispatch queue into bounded batches while preserving arrival order. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

This is consecutive splitting, not SQL GROUP BY and not a two-way accepted/rejected partition. Nonadjacent equal routes remain separate; filtering before splitting changes which boundaries are visible.

## 7. Applicability

Use when the consumer opts into XPath40 and needs the exact validation or ordering contract described here. Keep the XPath 3.1 baseline when that opt-in is unavailable.

## 8. Structure

```xquery
string-join(partition(('NORTH','NORTH','NORTH','SOUTH','NORTH'), function($current,$next,$position) { count($current) ge 2 or $current[1] ne $next }) ! string-join(?*, ','), '|')
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

partition returns a sequence of arrays. Its split callback receives the current nonempty partition sequence, next item and one-based input position. A true result starts the next partition before that item. Empty input returns no partitions; a singleton returns one array and the callback is not invoked for its first item. The worked chapter combines capacity and route-change boundaries. This is consecutive splitting, not SQL GROUP BY and not a two-way accepted/rejected partition. Nonadjacent equal routes remain separate; filtering before splitting changes which boundaries are visible.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[main.xpath](../examples/consecutive-partition/main.xpath): returns `NORTH,NORTH|NORTH|SOUTH|NORTH` at mode 40.

[edge.xpath](../examples/consecutive-partition/edge.xpath): returns `0` at mode 40.

[singleton.xpath](../examples/consecutive-partition/singleton.xpath): returns `1` at mode 40.

## 14. Performance Notes

Duplicate checks and index selection may traverse the input; some/every may finish early. Partitioning creates arrays and retains input items. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `consecutive-partition-main`, `consecutive-partition-edge`, `consecutive-partition-singleton`. [`Partition_FixedSizeGroups`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Partition_SplitsOnValueChange`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Partition_EmptyInput_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs) `fn-partition` is in GatedSets. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

