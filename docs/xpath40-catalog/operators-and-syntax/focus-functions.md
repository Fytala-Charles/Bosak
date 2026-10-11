<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Focus-based constructors">
  <br><br><h1>Focus-based constructors</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**Focus-based constructors** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

## 2. Intent

Use the current item as the argument to a zero-arity constructor inside a simple map.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, call xs:integer(.) explicitly. Compare the [complete earlier-version baseline](../examples/focus-functions/before.xpath) with the [complete expression](../examples/focus-functions/main.xpath). Both yield `12,5` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

An XML extract supplies quantity strings that need integer values. Use the current item as the argument to a zero-arity constructor inside a simple map. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. There must be a suitable focus, and invalid lexical data still fails; this is conversion, not validation.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. There must be a suitable focus, and invalid lexical data still fails; this is conversion, not validation.

## 8. Structure

```xquery
string-join(('12', '5') ! xs:integer() ! string(.), ',')
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Use the current item as the argument to a zero-arity constructor inside a simple map. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. There must be a suitable focus, and invalid lexical data still fails; this is conversion, not validation.

## 12. Implementation

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/focus-functions/main.xpath) yields `12,5`. The [edge case](../examples/focus-functions/edge.xpath) yields `FORG0001 (expected error)`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: per-item conversion. Invalid values still require error classification rather than a successful result. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `focus-functions-main`, `focus-functions-edge`, `focus-functions-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
