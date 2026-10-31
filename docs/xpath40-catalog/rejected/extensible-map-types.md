<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Extensible map type proposal">
  <br><br><h1>Extensible map type proposal</h1>
  <p>Rejected proposal; record alternatives shipped</p>
</div>

**C11 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-3.md) records 2 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified rejected proposal and record alternatives**. Keep structural membership, cast, coercion and annotation-aware lookup separate; C05 defects remain. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Extensible map type proposal — Rejected proposal; record alternatives shipped.** [Bosak PR #77](https://github.com/Fytala-Charles/Bosak/pull/77) / [immutable revision `27bc663b`](https://github.com/Fytala-Charles/Bosak/commit/27bc663b71b948318f69ee1f23173ac63046a525). This is a disposition dossier, not implementation of the refused/deferred shape.

**External specification references (current drafts; links checked 2026-10-11):** [XPath structural records](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-structural-record-types), [XPath record casts](https://qt4cg.org/specifications/xquery-40/xpath-40.html#casting-to-record). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Make surplus-field policy explicit when importing an order record.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

The dossier rejects the historical extensible-map-type proposal. This does not reject map(*) or typed maps. Shipped structural records provide field contracts, but record casts and coercion differ: casts discard surplus keys; coercion rejects them with XPTY0004. The dossier shorthand saying coercion discards entries is inaccurate for the delivered annotation model.

## 5. Motivation

Make surplus-field policy explicit when importing an order record. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

An open map and a closed record are not interchangeable. Silent removal of unknown fields is a cast policy, not the normal function-argument coercion policy.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
map:size(map {'id':'R1','extra':'ignored'} cast as record(id as xs:string))
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

The dossier rejects the historical extensible-map-type proposal. This does not reject map(*) or typed maps. Shipped structural records provide field contracts, but record casts and coercion differ: casts discard surplus keys; coercion rejects them with XPTY0004. The dossier shorthand saying coercion discards entries is inaccurate for the delivered annotation model. An open map and a closed record are not interchangeable. Silent removal of unknown fields is a cast policy, not the normal function-argument coercion policy.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[cast.xpath](../examples/extensible-map-types/cast.xpath): returns `1` at mode 40.

[coercion.xpath](../examples/extensible-map-types/coercion.xpath): raises `XPTY0004` at mode 40.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `extensible-map-types-cast`, `extensible-map-types-coercion`. [`Cast_DiscardsSurplusKeys`](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [`Coercion_SurplusKeys_XPTY0004`](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs). No additional gate promotion is claimed by this decision record. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

