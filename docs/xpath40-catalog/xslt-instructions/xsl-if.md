<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: xsl:if then/else">
  <br><br><h1>xsl:if then/else</h1>
  <p>Practical reference: Bosak XSLT 4.0 surface</p>
</div>

## 1. Name and Classification

**xsl:if then/else** - **Bosak XSLT 4.0 surface**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S7/S8; corresponding local XSLT surface tests. [Primary implementation evidence](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XSLT 4.0: conditional processing with xsl:if](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-if). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

## 2. Intent

Express two small scalar branches in one instruction.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

Use xsl:choose with xsl:when/xsl:otherwise in 3.0. Compare the [complete earlier-version baseline](../examples/xsl-if/before.xsl) with the [complete stylesheet](../examples/xsl-if/transform.xsl). Both yield `expedite` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

Choose the dispatch queue label from the task priority. Express two small scalar branches in one instruction. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. This demonstration treats absent or non-urgent priorities as normal; it does not validate a priority vocabulary.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. This demonstration treats absent or non-urgent priorities as normal; it does not validate a priority vocabulary.

## 8. Structure

```xml
<xsl:if test="/task/@priority = 'urgent'" then="'expedite'" else="'normal'"/>
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

The stylesheet compiler and TransformEngine execute the instruction; the contained expressions use the existing XPath engine. [Primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Express two small scalar branches in one instruction. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. This demonstration treats absent or non-urgent priorities as normal; it does not validate a priority vocabulary.

## 12. Implementation

The complete draft-instruction file declares version="4.0". Bosak recognizes these implemented instruction surfaces; its instruction support is separate from XPathCompatibility. Embedded expressions in these files remain XPath 3.1. The companion baseline declares version="3.0". See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression, or in [main.xml](../examples/xsl-if/main.xml) for an instruction example. The [complete stylesheet](../examples/xsl-if/transform.xsl) yields `expedite`. The [edge case](../examples/xsl-if/edge.xml) yields `normal`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: effective-boolean-value evaluation and one selected branch. Work in an unselected branch is not required by this example. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `xsl-if-main`, `xsl-if-edge`, `xsl-if-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
