<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: JNode navigation">
  <br><br><h1>JNode navigation</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**JNode navigation** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-123 JNode cluster; JNodeTests. [Primary implementation evidence](../../../tests/Bosak.XPath.Standard.Tests/JNodeTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: JNode types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-jnode-types), [fn:jtree](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-jtree), [fn:jkey](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-jkey), [fn:jvalue](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-jvalue), [XDM 4.0](https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #116](https://github.com/Fytala-Charles/Bosak/pull/116), [immutable mainline revision `345ae58e`](https://github.com/Fytala-Charles/Bosak/commit/345ae58e5f8e35d1b504aa0a7df250e3eaf88dd0) (2026-10-10). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Navigate JSON keys with path syntax, then unwrap the selected JNode for its value.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, use chained map lookup over parse-json output. Compare the [complete earlier-version baseline](../examples/jnode-navigation/before.xpath) with the [complete expression](../examples/jnode-navigation/main.xpath). Both yield `Utrecht` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A JSON order API sends a nested shipping address. Navigate JSON keys with path syntax, then unwrap the selected JNode for its value. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. A missing key selects nothing. JNodes retain navigation context; plain map and array values have different identity semantics.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. A missing key selects nothing. JNodes retain navigation context; plain map and array values have different identity semantics.

## 8. Structure

```xquery
fn:jvalue(fn:jtree(map { 'shipping': map { 'city': 'Utrecht' } })/"shipping"/"city")
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Standard.Tests/JNodeTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Navigate JSON keys with path syntax, then unwrap the selected JNode for its value. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. A missing key selects nothing. JNodes retain navigation context; plain map and array values have different identity semantics.

## 12. Implementation

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/jnode-navigation/main.xpath) yields `Utrecht`. The [edge case](../examples/jnode-navigation/edge.xpath) yields `0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: JNode construction plus path traversal. Building a navigation tree has a different allocation profile from direct map lookup. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `jnode-navigation-main`, `jnode-navigation-edge`, `jnode-navigation-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Standard.Tests/JNodeTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_JNodeFunctions_DefaultOptions_ThrowXpst0017`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_JNodeFunctions_FrozenXPath40_Work`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Named promoted sets: `fn-jkey`, `fn-jtree`, `fn-jvalue`. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*

**Scope review — 2026-10-12:** The [partial-model disposition](jnode-template-boundary.md) reconciles the original full-model exclusion with this shipped navigation. Eighteen additional checks supply full invoice input/output and explicit node()/xnode()/identity boundaries; navigation does not certify the complete generic model or stylesheet built-ins.
