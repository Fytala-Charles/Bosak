<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: xsl:note">
  <br><br><h1>xsl:note</h1>
  <p>Practical reference: Bosak XSLT 4.0 surface</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified inert authoring content**. No arbitrary extension instruction or complete forwards-compatible authoring validation is inferred. Final owner acceptance remains C13; earlier checkpoints are historical.

**C06 host/profile review — 26 October 2026:** [Bounded reconciliation](../HOST-PROFILE-RECONCILIATION.md) maps this entry to current pinned instruction/host rules and named tests. Seven workpackages remain; instruction comparison, complete static-mode evidence and host/adoption limits remain explicit. No acceptance or profile promotion. Earlier notices remain historical.

## 1. Name and Classification

**xsl:note** - **Bosak XSLT 4.0 surface**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S7/S8; corresponding local XSLT surface tests. [Primary implementation evidence](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XSLT 4.0: xsl:note](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-note-elements). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #91](https://github.com/Fytala-Charles/Bosak/pull/91), [immutable mainline revision `c0427907`](https://github.com/Fytala-Charles/Bosak/commit/c0427907002a355f6662ddcdc7412ad8f4a18846) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Keep authoring notes adjacent to the template.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

Use an XML comment in a 3.0 stylesheet. Compare the [complete earlier-version baseline](../examples/xsl-note/before.xsl) with the [complete stylesheet](../examples/xsl-note/transform.xsl). Both yield `urgent` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

Record a dispatch-rule explanation without emitting it. Keep authoring notes adjacent to the template. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Notes do not contribute result text and do not execute their content.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Notes do not contribute result text and do not execute their content.

## 8. Structure

```xml
<xsl:note>Dispatch policy: priority is advisory until reviewed.</xsl:note><xsl:value-of select="/task/@priority"/>
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

The stylesheet compiler and TransformEngine execute the instruction; the contained expressions use the existing XPath engine. [Primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Keep authoring notes adjacent to the template. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Notes do not contribute result text and do not execute their content.

## 12. Implementation

The complete draft-instruction file declares version="4.0". Bosak recognizes these implemented instruction surfaces; its instruction support is separate from XPathCompatibility. Embedded expressions in these files remain XPath 3.1. The companion baseline declares version="3.0". See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression, or in [main.xml](../examples/xsl-note/main.xml) for an instruction example. The [complete stylesheet](../examples/xsl-note/transform.xsl) yields `urgent`. The [edge case](../examples/xsl-note/edge.xml) yields an empty string. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: discarded authoring content. Source parsing still reads the note, but it contributes no result-tree content. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `xsl-note-main`, `xsl-note-edge`, `xsl-note-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Note_InTemplate_IsDiscarded`](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs), [`Note_AtTopLevel_IsDiscarded`](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs), [`Note_ContentIsNotValidated`](../../../tests/Bosak.Xslt.Tests/Xslt40SurfaceTests.cs). Not a qt4 expression-set feature. XSLT evidence uses the named instruction tests below; no upstream XSLT gate promotion is claimed. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
