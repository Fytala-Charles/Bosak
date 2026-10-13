<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Directional scan names">
  <br><br><h1>Directional scan names</h1>
  <p>Rejected / superseded</p>
</div>

## 1. Name and Classification

**Directional scan names — Rejected / superseded.** [Bosak PR #77](https://github.com/Fytala-Charles/Bosak/pull/77) / [immutable revision `27bc663b`](https://github.com/Fytala-Charles/Bosak/commit/27bc663b71b948318f69ee1f23173ac63046a525). This is a disposition dossier, not implementation of the refused/deferred shape.

**External specification references (current drafts; links checked 2026-10-11):** [fn:scan](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-scan), [QT4CG proposal history](https://github.com/qt4cg/qtspecs/pull/2824). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Explain why running stock balances use the experimental scan contract.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

The recorded scan-left/scan-right proposals consolidated into fn:scan, a left-fold scan. Bosak fn:scan remains experimental even though its conformance set is promoted. Do not promise a right-fold alias: reversing input and reversing results is not generally equivalent for noncommutative operations. The adopted scan returns array-wrapped states; consult its entry for that contract.

## 5. Motivation

Explain why running stock balances use the experimental scan contract. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

A fold returning only the final state and a scan returning intermediate states have different results. Direction matters for subtraction and string construction.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
fn:scan-left((1,2), 0, function($a,$b){$a+$b})
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

The recorded scan-left/scan-right proposals consolidated into fn:scan, a left-fold scan. Bosak fn:scan remains experimental even though its conformance set is promoted. Do not promise a right-fold alias: reversing input and reversing results is not generally equivalent for noncommutative operations. The adopted scan returns array-wrapped states; consult its entry for that contract. A fold returning only the final state and a scan returning intermediate states have different results. Direction matters for subtraction and string construction.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[left.xpath](../examples/directional-scan/left.xpath): raises `XPST0017` at mode experimental.

[right.xpath](../examples/directional-scan/right.xpath): raises `XPST0017` at mode experimental.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `directional-scan-left`, `directional-scan-right`.  No additional gate promotion is claimed by this decision record. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

