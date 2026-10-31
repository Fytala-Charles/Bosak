<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Comparator-based sorting">
  <br><br><h1>Comparator-based sorting</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 2 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. Comparator consistency and arbitrary result/coercion combinations are not certified; avoid NaN/mixed-family business order. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Comparator-based sorting** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [fn:sort-with](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-sort-with), [array:sort-with](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-array-sort-with). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #112](https://github.com/Fytala-Charles/Bosak/pull/112), [immutable mainline revision `89c90b4d`](https://github.com/Fytala-Charles/Bosak/commit/89c90b4d5b6a216a4a37223e169d36e3a1a832bd) (2026-10-10). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

A comparator expresses descending priority directly; zero means equal priority.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, sort by a transformed key such as negative priority, or use xsl:sort. Compare the [complete earlier-version baseline](../examples/sort-with/before.xpath) with the [complete expression](../examples/sort-with/main.xpath). Both yield `5,2,1` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A dispatch queue must show highest priority first. A comparator expresses descending priority directly; zero means equal priority. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. A comparator must return a consistent ordering. This example sorts numeric priorities and does not define a secondary order for equal records.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. A comparator must return a consistent ordering. This example sorts numeric priorities and does not define a secondary order for equal records.

## 8. Structure

```xquery
string-join(fn:sort-with((2, 5, 1), function($a, $b) { if ($a gt $b) then -1 else if ($a lt $b) then 1 else 0 }) ! string(.), ',')
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: A comparator expresses descending priority directly; zero means equal priority. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. A comparator must return a consistent ordering. This example sorts numeric priorities and does not define a secondary order for equal records.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Comparator-driven numeric ordering and array member boundaries; Empty input, comparator result/arity and consistency requirements. **Boundaries:** Comparator consistency and arbitrary result/coercion combinations are not certified; avoid NaN/mixed-family business order. Follow-up groups: F06, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/sort-with/main.xpath) yields `5,2,1`. The [edge case](../examples/sort-with/edge.xpath) yields `0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: hot-path relevant sorting. Comparator callbacks may execute many times, so expensive work inside a comparator can dominate. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `sort-with-main`, `sort-with-edge`, `sort-with-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_SortWithCluster_DefaultOptions_ThrowXpst0017`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Compile_SortWithCluster_3Point1Functions_StillWork`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_SortWithCluster_FrozenXPath40_Works`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Named promoted sets: `array-sort-with`, `fn-sort-with`. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
