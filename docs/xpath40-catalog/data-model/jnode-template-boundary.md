<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: JNode model and template boundary">
  <br><br><h1>JNode model and template boundary</h1>
  <p>Deferred remainder; partial Frozen 4.0</p>
</div>

## 1. Name and Classification

**JNode model and template boundary — Deferred remainder; partial Frozen 4.0.** [Bosak PR #116](https://github.com/Fytala-Charles/Bosak/pull/116) / [immutable revision `345ae58e`](https://github.com/Fytala-Charles/Bosak/commit/345ae58e5f8e35d1b504aa0a7df250e3eaf88dd0). This is the shipped engine delivery.

**External specification references (current drafts; links checked 2026-10-11):** [XSLT built-in template rules](https://qt4cg.org/specifications/xslt-40/Overview.html#built-in-rule), [XDM node kinds](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html#node-kinds). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Avoid confusing JSON navigation support with complete stylesheet processing of JSON nodes.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

REQ-123 shipped XdmJNode, jtree/jkey/jvalue, jnode type tests and navigation. That supersedes a blanket deferral of all JNode support. The full GNode/XNode model and JNode-specific XSLT built-in template rules are not certified by that delivery. TransformEngine built-in dispatch and public source APIs remain oriented to IXdmNode XML sources. Use explicit JSON map/array traversal and XML construction for stylesheet integration; audit each extra model/template contract before claiming it.

## 5. Motivation

Avoid confusing JSON navigation support with complete stylesheet processing of JSON nodes. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

A JSON map, a JNode wrapper and an XML node adapter are distinct values. A navigation test passing does not prove default XSLT template behavior, serialization or every node-kind operation.

## 7. Applicability

Use within the declared compatibility and business contract. Deferred/rejected contracts require explicit alternatives; they must not be advertised as implemented.

## 8. Structure

```xquery
jvalue(jtree(map {'id':'R1'})/child::id)
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

REQ-123 shipped XdmJNode, jtree/jkey/jvalue, jnode type tests and navigation. That supersedes a blanket deferral of all JNode support. The full GNode/XNode model and JNode-specific XSLT built-in template rules are not certified by that delivery. TransformEngine built-in dispatch and public source APIs remain oriented to IXdmNode XML sources. Use explicit JSON map/array traversal and XML construction for stylesheet integration; audit each extra model/template contract before claiming it. A JSON map, a JNode wrapper and an XML node adapter are distinct values. A navigation test passing does not prove default XSLT template behavior, serialization or every node-kind operation.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

## 13. Sample Code

[navigation.xpath](../examples/jnode-template-boundary/navigation.xpath): returns `R1` at mode 40.

## 14. Performance Notes

Predicates and duplicate checks scan their inputs; partitioning allocates arrays and retains accepted nodes. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `jnode-template-boundary-navigation`.  No additional gate promotion is claimed by this decision record. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

