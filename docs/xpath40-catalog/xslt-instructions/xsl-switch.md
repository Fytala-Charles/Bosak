<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: xsl:switch">
  <br><br><h1>xsl:switch</h1>
  <p>Practical reference: Bosak XSLT 4.0 surface</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited selection; incomparable-comparison blocker**. C06-H02: both fresh host controls still return empty instead of XPTY0004. Ordinary dispatch success does not certify full general-comparison semantics. Final owner acceptance remains C13; earlier checkpoints are historical.

**Grammar/version review — 21 October 2026:** [Current pinned mapping](../SWITCH-ARRAY-SELECT-GRAMMAR.md) and [complete dispatch comparison](../examples/dispatch-grammar/README.md) distinguish known-instruction recognition, effective XPath version, raw result types and actual text delivery. Public1.23.2 is verified; historical provenance gaps remain.


## 1. Name and Classification

**xsl:switch** - **Bosak XSLT 4.0 surface**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S7/S8; corresponding local XSLT surface tests. [Primary implementation evidence](../../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XSLT 4.0: conditional processing with xsl:switch](https://qt4cg.org/specifications/xslt-40/Overview.html#xsl-switch). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #92](https://github.com/Fytala-Charles/Bosak/pull/92), [immutable mainline revision `2a62ad4d`](https://github.com/Fytala-Charles/Bosak/commit/2a62ad4d9f72d2614887c260e8856de0a6617350) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Match a selector against branch test values.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

Use xsl:choose and repeat the selector in each condition. Compare the [complete earlier-version baseline](../examples/xsl-switch/before.xsl) with the [complete stylesheet](../examples/xsl-switch/transform.xsl). Both yield `expedite` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

Route a task by a single priority selector. Match a selector against branch test values. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Tests are values compared with the selector using general comparison, not boolean guards. The otherwise branch handles absent/unrecognized values. Incomparable numeric/Boolean operands currently return empty instead of the required XPTY0004 in both hosts (C06-H02); the ordinary string-dispatch example does not cover that gap.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Tests are values compared with the selector using general comparison, not boolean guards. The otherwise branch handles absent/unrecognized values. Incomparable numeric/Boolean operands currently return empty instead of the required XPTY0004 in both hosts (C06-H02); the ordinary string-dispatch example does not cover that gap.

## 8. Structure

```xml
<xsl:switch select="string(/task/@priority)"><xsl:when test="'urgent'" select="'expedite'"/><xsl:when test="'normal'" select="'standard'"/><xsl:otherwise select="'review'"/></xsl:switch>
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

The stylesheet compiler and TransformEngine execute the instruction; the contained expressions use the existing XPath engine. [Primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Match a selector against branch test values. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Tests are values compared with the selector using general comparison, not boolean guards. The otherwise branch handles absent/unrecognized values. Incomparable numeric/Boolean operands currently return empty instead of the required XPTY0004 in both hosts (C06-H02); the ordinary string-dispatch example does not cover that gap.

## 12. Implementation

The complete draft-instruction file declares version="4.0". Bosak recognizes these implemented instruction surfaces; its instruction support is separate from XPathCompatibility. Embedded expressions use effective compatibility: under the default Legacy host, effective version4.0 selects frozen XPath40 and a local3.0 override selects XPath31. An explicit Xslt40Draft host selects frozen XPath40 independently of lower local version. Known-instruction recognition is separate from these expression gates. The companion baseline declares version="3.0". See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression, or in [main.xml](../examples/xsl-switch/main.xml) for an instruction example. The [complete stylesheet](../examples/xsl-switch/transform.xsl) yields `expedite`. The [edge case](../examples/xsl-switch/edge.xml) yields `review`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: selector conversion followed by branch-value comparisons. The number and cost of branch tests matter. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `xsl-switch-main`, `xsl-switch-edge`, `xsl-switch-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Switch_LookupTable_SelectBranches`](../../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs), [`Switch_TestSequences_DaysInMonth`](../../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs), [`Switch_GeneralComparison_NumericPromotion`](../../../tests/Bosak.Xslt.Tests/Xslt40ArraySwitchTests.cs). Not a qt4 expression-set feature. XSLT evidence uses the named instruction tests below; no upstream XSLT gate promotion is claimed. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*

Updated21 October2026: dispatch-grammar array/switch cases add exact source-node identity, empty-member and3.0/4.0 host/version evidence without changing engine semantics.
