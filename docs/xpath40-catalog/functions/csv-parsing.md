<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: CSV parsing">
  <br><br><h1>CSV parsing</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**CSV parsing** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-123 CSV cluster; local FunctionLibrary CSV implementation. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

## 2. Intent

Use the parsed row array rather than splitting on commas, which would split a quoted product name.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, use a dedicated CSV reader or an explicitly tested parser; tokenize is not a general CSV parser. Compare the [complete earlier-version baseline](../examples/csv-parsing/before.xpath) with the [complete expression](../examples/csv-parsing/main.xpath). Both yield `FILTER,BLUE / 12` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A supplier exports product codes and quantities as quoted CSV cells. Use the parsed row array rather than splitting on commas, which would split a quoted product name. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. This reduced example has one data row and no header. Validate numeric quantities separately; parsing CSV is not business validation.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. This reduced example has one data row and no header. Validate numeric quantities separately; parsing CSV is not business validation.

## 8. Structure

```xquery
let $row := fn:parse-csv('"FILTER,BLUE",12')?rows[1]
return $row?1 || ' / ' || $row?2
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Use the parsed row array rather than splitting on commas, which would split a quoted product name. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. This reduced example has one data row and no header. Validate numeric quantities separately; parsing CSV is not business validation.

## 12. Implementation

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. The baseline starts with a host-parsed array; it is not a replacement CSV parser. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/csv-parsing/main.xpath) yields `FILTER,BLUE / 12`. The [edge case](../examples/csv-parsing/edge.xpath) yields `FILTER,BLUE / 0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: input-size dependent. Parsing constructs row/cell values; the quantity cast and downstream business checks are separate costs. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `csv-parsing-main`, `csv-parsing-edge`, `csv-parsing-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
