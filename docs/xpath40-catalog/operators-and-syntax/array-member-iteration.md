<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Array member iteration">
  <br><br><h1>Array member iteration</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**Array member iteration** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: for expressions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-for-expressions). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

## 2. Intent

Iterate members while preserving each member as one sequence binding.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, iterate 1 to array:size($parcels) and call array:get. Compare the [complete earlier-version baseline](../examples/array-member-iteration/before.xpath) with the [complete expression](../examples/array-member-iteration/main.xpath). Both yield `2,1` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A packing plan stores each parcel as an array member containing its item sequence. Iterate members while preserving each member as one sequence binding. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. A member can contain several items; flattening the array first loses parcel boundaries.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. A member can contain several items; flattening the array first loses parcel boundaries.

## 8. Structure

```xquery
string-join((for member $parcel in [('FILTER-A','BOLT-M8'), 'MANUAL'] return string(count($parcel))), ',')
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Iterate members while preserving each member as one sequence binding. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. A member can contain several items; flattening the array first loses parcel boundaries.

## 12. Implementation

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/array-member-iteration/main.xpath) yields `2,1`. The [edge case](../examples/array-member-iteration/edge.xpath) yields `0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: per-member iteration. Member sequences retain their boundaries, and processing those contents adds its own cost. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `array-member-iteration-main`, `array-member-iteration-edge`, `array-member-iteration-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
