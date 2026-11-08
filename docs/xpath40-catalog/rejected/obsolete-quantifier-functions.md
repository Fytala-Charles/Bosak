<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Never-adopted quantifier names">
  <br><br><h1>Never-adopted quantifier names</h1>
  <p>Rejected</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified rejected names and quantified alternative**. No cross-language alias or universal duplicate/partition contract is adopted. Final owner acceptance remains C13; earlier checkpoints are historical.

## 1. Name and Classification

**Never-adopted quantifier names — Rejected.** [Bosak PR #77](https://github.com/Fytala-Charles/Bosak/pull/77) / [immutable revision `27bc663b`](https://github.com/Fytala-Charles/Bosak/commit/27bc663b71b948318f69ee1f23173ac63046a525). This is a disposition dossier, not implementation of the refused/deferred shape.

**External specification references (current drafts; links checked 2026-10-11):** [fn:all-equal](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-all-equal), [fn:some](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-some), [fn:every](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-every). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Prevent guessed function names from becoming accidental compatibility promises.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

REQ-118 records fn:all and fn:some-of as names that never existed in the adopted surface. Use all-equal/all-different for sequence equality and some/every for boolean predicates. These are different contracts; there is no universal mechanical rename from fn:all.

## 5. Motivation

Prevent guessed function names from becoming accidental compatibility promises. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

Names borrowed from other languages are not XPath aliases. Equality of all values differs from every value satisfying a predicate.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
fn:all((1,1))
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

REQ-118 records fn:all and fn:some-of as names that never existed in the adopted surface. Use all-equal/all-different for sequence equality and some/every for boolean predicates. These are different contracts; there is no universal mechanical rename from fn:all. Names borrowed from other languages are not XPath aliases. Equality of all values differs from every value satisfying a predicate.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[all.xpath](../examples/obsolete-quantifier-functions/all.xpath): raises `XPST0017` at mode 40.

[some-of.xpath](../examples/obsolete-quantifier-functions/some-of.xpath): raises `XPST0017` at mode 40.

[alternative.xpath](../examples/obsolete-quantifier-functions/alternative.xpath): returns `true` at mode 40.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `obsolete-quantifier-functions-all`, `obsolete-quantifier-functions-some-of`, `obsolete-quantifier-functions-alternative`.  No additional gate promotion is claimed by this decision record. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

