<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Keyword arguments">
  <br><br><h1>Keyword arguments</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**C11 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-3.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited keywords; placeholder blocker**. C04-G03 and C02-F01: keyword placeholders and per-function label/metadata gaps remain; not a universal dynamic-call keyword facility. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Keyword arguments** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: static function calls and keyword arguments](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-static-functions). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #86](https://github.com/Fytala-Charles/Bosak/pull/86), [immutable mainline revision `498ae8d7`](https://github.com/Fytala-Charles/Bosak/commit/498ae8d7a1e3b3a11bb404eceb19a29dba21cdc9) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Name start and length to clarify an otherwise positional call.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, use substring($value, 1, 3) and document the argument roles. Compare the [complete earlier-version baseline](../examples/keyword-arguments/before.xpath) with the [complete expression](../examples/keyword-arguments/main.xpath). Both yield `DHL` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A shipment label extracts a fixed carrier prefix from a tracking string. Name start and length to clarify an otherwise positional call. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Parameter names belong to the function signature. Positional arguments must not follow keyword arguments.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Parameter names belong to the function signature. Positional arguments must not follow keyword arguments.

## 8. Structure

```xquery
substring(value := 'DHL-2026-1042', start := 1, length := 3)
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Name start and length to clarify an otherwise positional call. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Parameter names belong to the function signature. Positional arguments must not follow keyword arguments.

## 12. Implementation

**C04 grammar review — 26 October 2026, published 1.26.3:** [Expression/call/focus reconciliation](../EXPRESSION-CALL-FOCUS-RECONCILIATION.md) covers S3-keywords. Reordered/mixed keywords, empty/default, Q{} EQNames and static rejection controls pass. Keyword partial application with ? fails parsing; positional partial application is a working alternative. Per-function metadata and spelling drift remain C02 F01. Dynamic calls have positional argument grammar; their rejection is not evidence of a missing universal keyword facility. This is bounded production/profile evidence, not full grammar or AC-04 acceptance.


Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/keyword-arguments/main.xpath) yields `DHL`. The [edge case](../examples/keyword-arguments/edge.xpath) yields `XPST0003 (expected error)`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: argument binding plus ordinary function execution. Named arguments improve clarity; no runtime speed claim is made. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `keyword-arguments-main`, `keyword-arguments-edge`, `keyword-arguments-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_KeywordArgs_31Mode_ThrowsXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_KeywordArgs_FullyKeyworded_Works`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_KeywordArgs_MixedWithPositionals_Works`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No feature-specific qt4 set assignment recorded here; unit evidence is listed explicitly. This is not a claim of absent upstream tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
