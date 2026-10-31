<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Iteration across collection sequences">
  <br><br><h1>Iteration across collection sequences</h1>
  <p>Frozen 4.0</p>
</div>

**C11 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-3.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified bounded multi-container iteration**. Not a full generic collection/type-model promotion. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Iteration across collection sequences — Frozen 4.0.** [Bosak PR #87](https://github.com/Fytala-Charles/Bosak/pull/87) / [immutable revision `ec793559`](https://github.com/Fytala-Charles/Bosak/commit/ec79355936a29ac70eeea3ae47e4c4e2ee00d813). This is the shipped engine delivery.

**External specification references (current drafts; links checked 2026-10-11):** [XPath collection iteration](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-for-expressions). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Process parcels or inventory updates spread across several arrays or maps.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

The initial REQ-118 stability table deferred sequences of arrays/maps, but S4 shipped this extension. for member binds complete array members, including multi-item sequences; for key/value binds map entries. Positional counting continues across the outer collection sequence. Non-array/non-map input is XPTY0141. Map entry ordering is not an application sorting contract.

## 5. Motivation

Process parcels or inventory updates spread across several arrays or maps. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

Nested loops that flatten array members can destroy a parcel boundary. Map entry order should not be treated as stable business order; explicitly sort when presentation order matters.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
string-join(for member $parcel in ([('FILTER-A','BOLT-M8')], ['MANUAL']) return string(count($parcel)), ',')
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

The initial REQ-118 stability table deferred sequences of arrays/maps, but S4 shipped this extension. for member binds complete array members, including multi-item sequences; for key/value binds map entries. Positional counting continues across the outer collection sequence. Non-array/non-map input is XPTY0141. Map entry ordering is not an application sorting contract. Nested loops that flatten array members can destroy a parcel boundary. Map entry order should not be treated as stable business order; explicitly sort when presentation order matters.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S4-iteration. Members and map-entry values remain whole sequences, positions continue across container sequences, empty collections bind nothing, and invalid container items refuse XPTY0141. Container iteration preserves containing focus. Do not flatten member boundaries or infer portable map entry order. Complete typed iteration combinations and host bindings remain outside bounded controls. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[arrays.xpath](../examples/sequence-collection-iteration/arrays.xpath): returns `2,1` at mode 40.

[maps.xpath](../examples/sequence-collection-iteration/maps.xpath): returns `BOLT-M8=5,FILTER-A=2` at mode 40.

[invalid.xpath](../examples/sequence-collection-iteration/invalid.xpath): raises `XPTY0141` at mode 40.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `sequence-collection-iteration-arrays`, `sequence-collection-iteration-maps`, `sequence-collection-iteration-invalid`. [`Evaluate_ForMember_SequenceOfArraysCountsPositionsAcrossArrays`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_ForKeyValue_PositionalCountsAcrossMaps`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No additional gate promotion is claimed by this decision record. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

