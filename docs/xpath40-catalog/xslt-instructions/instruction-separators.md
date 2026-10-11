<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Instruction separators">
  <br><br><h1>Instruction separators</h1>
  <p>Practical reference: Bosak XSLT 4.0 surface</p>
</div>

## 1. Name and Classification

**Instruction separators** - **Bosak XSLT 4.0 surface**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S7/S8; corresponding local XSLT surface tests. [Primary implementation evidence](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

## 2. Intent

Insert a delimiter between iterations without adding a trailing delimiter.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

Use string-join or explicit position checks in 3.0. Compare the [complete earlier-version baseline](../examples/instruction-separators/before.xsl) with the [complete stylesheet](../examples/instruction-separators/transform.xsl). Both yield `FILTER-A, BOLT-M8` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

Print a compact list of product codes for a picking label. Insert a delimiter between iterations without adding a trailing delimiter. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. The separator is between iterations; emitted text and empty iteration bodies need deliberate handling.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. The separator is between iterations; emitted text and empty iteration bodies need deliberate handling.

## 8. Structure

```xml
<xsl:for-each select="/task/sku" separator=", "><xsl:value-of select="."/></xsl:for-each>
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

The stylesheet compiler and TransformEngine execute the instruction; the contained expressions use the existing XPath engine. [Primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Insert a delimiter between iterations without adding a trailing delimiter. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. The separator is between iterations; emitted text and empty iteration bodies need deliberate handling.

## 12. Implementation

The complete draft-instruction file declares version="4.0". Bosak recognizes these implemented instruction surfaces; its instruction support is separate from XPathCompatibility. Embedded expressions in these files remain XPath 3.1. The companion baseline declares version="3.0". See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression, or in [main.xml](../examples/instruction-separators/main.xml) for an instruction example. The [complete stylesheet](../examples/instruction-separators/transform.xsl) yields `FILTER-A, BOLT-M8`. The [edge case](../examples/instruction-separators/edge.xml) yields an empty string. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: iteration output with separator insertion. Output volume remains proportional to selected labels and delimiters. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `instruction-separators-main`, `instruction-separators-edge`, `instruction-separators-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
