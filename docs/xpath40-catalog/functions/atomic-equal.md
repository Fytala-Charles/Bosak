<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Atomic equality">
  <br><br><h1>Atomic equality</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**Atomic equality** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [fn:atomic-equal](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-atomic-equal). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

## 2. Intent

Make the equality policy for individual atomic values explicit, including NaN.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, use type-specific comparisons and deliberate NaN handling. Compare the [complete earlier-version baseline](../examples/atomic-equal/before.xpath) with the [complete expression](../examples/atomic-equal/main.xpath). Both yield `true` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A telemetry feed uses NaN for unavailable measurements and compares two snapshots. Make the equality policy for individual atomic values explicit, including NaN. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. This is atomic-value equality, not document or map equality. Do not silently replace every general comparison with it.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. This is atomic-value equality, not document or map equality. Do not silently replace every general comparison with it.

## 8. Structure

```xquery
fn:atomic-equal(xs:double('NaN'), xs:double('NaN'))
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Make the equality policy for individual atomic values explicit, including NaN. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. This is atomic-value equality, not document or map equality. Do not silently replace every general comparison with it.

## 12. Implementation

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/atomic-equal/main.xpath) yields `true`. The [edge case](../examples/atomic-equal/edge.xpath) yields `false`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: small scalar comparison overhead, including atomic type classification. It does not traverse a document tree. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `atomic-equal-main`, `atomic-equal-edge`, `atomic-equal-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
