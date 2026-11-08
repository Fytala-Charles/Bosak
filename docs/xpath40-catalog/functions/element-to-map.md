<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Element-to-map conversion">
  <br><br><h1>Element-to-map conversion</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. The published simple-content example does not certify every conversion-plan record, name collision, namespace, schema annotation or option; use only tested layouts/options. Current-draft escape-names decoding is unsupported; the new option is not adopted automatically into frozen or Experimental profiles. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Element-to-map conversion** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [fn:element-to-map](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-element-to-map), [fn:element-to-map-plan](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-element-to-map-plan). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #106](https://github.com/Fytala-Charles/Bosak/pull/106), [immutable mainline revision `1bb73147`](https://github.com/Fytala-Charles/Bosak/commit/1bb731474c6ba48dded66441183c676a0ebf9761) (2026-10-09). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Describe the conversion shape explicitly before passing data into a map-oriented pipeline.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, construct the desired map explicitly from XML selectors. Compare the [complete earlier-version baseline](../examples/element-to-map/before.xpath) with the [complete expression](../examples/element-to-map/main.xpath). Both yield `FILTER-A` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

An integration adapter needs an XML product code as a map field. Describe the conversion shape explicitly before passing data into a map-oriented pipeline. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Layout and typing options matter for attributes and repeated children. This example deliberately uses a single simple-content element.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Layout and typing options matter for attributes and repeated children. This example deliberately uses a single simple-content element.

## 8. Structure

```xquery
fn:element-to-map(parse-xml('<sku>FILTER-A</sku>')/sku)?sku
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Describe the conversion shape explicitly before passing data into a map-oriented pipeline. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Layout and typing options matter for attributes and repeated children. This example deliberately uses a single simple-content element.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Simple-content XML-to-map shape; Plans/layout/name policy are explicit conversion choices; Wrong input, option and keyword boundaries. **Boundaries:** The published simple-content example does not certify every conversion-plan record, name collision, namespace, schema annotation or option; use only tested layouts/options. Current-draft escape-names decoding is unsupported; the new option is not adopted automatically into frozen or Experimental profiles. Follow-up groups: F01, F14. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

**Supplementary contract review (2026-10-12):** Keyword drift: implemented input differs from pinned element; the [signature audit](../SIGNATURE-CONTRACT-AUDIT.md) publishes paired success/refusal files. Positional conversion remains separate evidence.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/element-to-map/main.xpath) yields `FILTER-A`. The [edge case](../examples/element-to-map/edge.xpath) yields `0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: tree-size dependent conversion. Selected layout and repeated-child representation affect allocations. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `element-to-map-main`, `element-to-map-edge`, `element-to-map-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Evaluate_ElementToMap_FrozenXPath40_Works`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Named promoted sets: `fn-element-to-map`, `fn-element-to-map-plan`. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
