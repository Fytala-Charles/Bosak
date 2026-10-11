<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: xsl:map selection and duplicate policy">
  <br><br><h1>xsl:map selection and duplicate policy</h1>
  <p>Practical reference: Bosak XSLT 4.0 surface</p>
</div>

## 1. Name and Classification

**xsl:map selection and duplicate policy** - **Bosak XSLT 4.0 surface**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S7/S8; corresponding local XSLT surface tests. [Primary implementation evidence](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XSLT 4.0: xsl:map and xsl:map-entry](https://qt4cg.org/specifications/xslt-40/Overview.html#id-xsl-map-and-entry). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

## 2. Intent

Declare duplicate-key policy while combining maps.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

Use map:merge with the matching duplicates option in 3.1. Compare the [complete earlier-version baseline](../examples/xsl-map/before.xsl) with the [complete stylesheet](../examples/xsl-map/transform.xsl). Both yield `urgent` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

Combine carrier settings with a task-specific override. Declare duplicate-key policy while combining maps. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. use-last is an intentional override policy, not a safe default for every payload.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. use-last is an intentional override policy, not a safe default for every payload.

## 8. Structure

```xml
<xsl:variable name="settings" as="map(*)"><xsl:map select="(map{'queue':'normal'}, map{'queue':string(/task/@priority)})" duplicates="'use-last'"/></xsl:variable><xsl:value-of select="$settings?queue"/>
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

The stylesheet compiler and TransformEngine execute the instruction; the contained expressions use the existing XPath engine. [Primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Declare duplicate-key policy while combining maps. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. use-last is an intentional override policy, not a safe default for every payload.

## 12. Implementation

The complete draft-instruction file declares version="4.0". Bosak recognizes these implemented instruction surfaces; its instruction support is separate from XPathCompatibility. Embedded expressions in these files remain XPath 3.1. The companion baseline declares version="3.0". See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression, or in [main.xml](../examples/xsl-map/main.xml) for an instruction example. The [complete stylesheet](../examples/xsl-map/transform.xsl) yields `urgent`. The [edge case](../examples/xsl-map/edge.xml) yields `normal`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: input-map merge with duplicate-key handling. Map size and the chosen duplicate policy affect work and storage. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `xsl-map-main`, `xsl-map-edge`, `xsl-map-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
