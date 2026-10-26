<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: XPath switch expression">
  <br><br><h1>XPath switch expression</h1>
  <p>Rejected in XPath</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified XPath refusal and conditional alternative**. Host grammar acceptance does not transfer to standalone XPath; XQuery4 remains outside this contract. Final owner acceptance remains C13; earlier checkpoints are historical.

## 1. Name and Classification

**XPath switch expression — Rejected in XPath.** [Bosak PR #77](https://github.com/Fytala-Charles/Bosak/pull/77) / [immutable revision `27bc663b`](https://github.com/Fytala-Charles/Bosak/commit/27bc663b71b948318f69ee1f23173ac63046a525). This is a disposition dossier, not implementation of the refused/deferred shape.

**External specification references (current drafts; links checked 2026-10-11):** [XPath conditional expressions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-conditionals), [XSLT switch instruction](https://qt4cg.org/specifications/xslt-40/Overview.html#element-switch). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Choose a routing branch using a construct in the actual host language.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

XPath has no switch expression in the adopted grammar. Use if/then/else or map lookup for XPath dispatch. xsl:switch is an XSLT instruction; XQuery switch is a separate language construct and is not rejected by this XPath decision.

## 5. Motivation

Choose a routing branch using a construct in the actual host language. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

Language alignment is local to the host grammar. XQuery or XSLT acceptance does not make an expression legal in XPath.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
switch ('NORTH') case 'NORTH' return 1 default return 0
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

XPath has no switch expression in the adopted grammar. Use if/then/else or map lookup for XPath dispatch. xsl:switch is an XSLT instruction; XQuery switch is a separate language construct and is not rejected by this XPath decision. Language alignment is local to the host grammar. XQuery or XSLT acceptance does not make an expression legal in XPath.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[refusal.xpath](../examples/xpath-switch-expression/refusal.xpath): raises `XPST0003` at mode 40.

[alternative.xpath](../examples/xpath-switch-expression/alternative.xpath): returns `dispatch` at mode 31.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `xpath-switch-expression-refusal`, `xpath-switch-expression-alternative`.  No additional gate promotion is claimed by this decision record. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

