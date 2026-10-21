# Historical REQ-126 nominative-records snapshot

This is the pre-review catalog snapshot from `9ddf46b0b5232cc69ea845014eb252ef6a7ee925`, not the current supported contract. [Current reference](../xpath40-catalog/type-system/nominative-records.md). Original implementation logs and historical observations are retained below. Relative Markdown links have been rebased to the immutable original revision and the banner asset path adjusted; no behavior or old execution receipt is rewritten. [Original Git blob](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/type-system/nominative-records.md).

<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Nominative record types">
  <br><br><h1>Nominative record types</h1>
  <p>Deferred</p>
</div>

## 1. Name and Classification

**Nominative record types — Deferred.** [Bosak PR #77](https://github.com/Fytala-Charles/Bosak/pull/77) / [immutable revision `27bc663b`](https://github.com/Fytala-Charles/Bosak/commit/27bc663b71b948318f69ee1f23173ac63046a525). This is a disposition dossier, not implementation of the refused/deferred shape.

**External specification references (current drafts; links checked 2026-10-11):** [XPath nominative records](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-nominative-record-types), [QT4CG proposal history](https://github.com/qt4cg/qtspecs/pull/2934). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Record the boundary between structural data contracts and named record identity.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

Structural records are shipped; nominative records remain deferred while the specification evolves. A named type identity is a distinct contract, not an alias for a structural record. Do not advertise custom named record declarations or identity-preserving constructors as supported. QT4CG PR 2934 supplies the historical split; no Bosak engine delivery is claimed for nominative identity.

## 5. Motivation

Record the boundary between structural data contracts and named record identity. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

A C# record class name or TypeScript type alias does not establish XPath nominative identity. Structural matching, coercion and casts are separate behaviors.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
(map {'id':'R1'} cast as record(id as xs:string))?id
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/worked-examples/order-routing/README.md).

## 11. Consequences

Structural records are shipped; nominative records remain deferred while the specification evolves. A named type identity is a distinct contract, not an alias for a structural record. Do not advertise custom named record declarations or identity-preserving constructors as supported. QT4CG PR 2934 supplies the historical split; no Bosak engine delivery is claimed for nominative identity. A C# record class name or TypeScript type alias does not establish XPath nominative identity. Structural matching, coercion and casts are separate behaviors.

## 12. Implementation

The [manifest harness](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[alternative.xpath](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/examples/nominative-records/alternative.xpath): returns `R1` at mode 40.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `nominative-records-alternative`.  No additional gate promotion is claimed by this decision record. [Gate runner](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Deferred xsl:record-type declarations](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/deferred/xslt-record-type.md) supplies
complete invoice cases for named type/constructor use and supported alternatives.
[xsl:item-type](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/deferred/xslt-item-type.md) covers aliases separately.

[Catalog index](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/README.md), [routing chapter](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/worked-examples/order-routing/README.md), [structural records](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/type-system/structural-records.md), [experimental scan](https://github.com/Fytala-Charles/Bosak/blob/9ddf46b0b5232cc69ea845014eb252ef6a7ee925/docs/xpath40-catalog/experimental/fn-scan.md).

