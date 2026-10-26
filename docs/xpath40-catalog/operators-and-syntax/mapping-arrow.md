<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Mapping arrow">
  <br><br><h1>Mapping arrow</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**Mapping arrow** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: mapping arrow expressions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-mapping-arrow-expression). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #87](https://github.com/Fytala-Charles/Bosak/pull/87), [immutable mainline revision `ec793559`](https://github.com/Fytala-Charles/Bosak/commit/ec79355936a29ac70eeea3ae47e4c4e2ee00d813) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Apply the target call to each item and return the mapped sequence.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, use the simple-map operator with normalize-space(.). Compare the [complete earlier-version baseline](../examples/mapping-arrow/before.xpath) with the [complete expression](../examples/mapping-arrow/main.xpath). Both yield `FILTER-A,BOLT-M8` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A batch of supplier product codes must be normalized individually. Apply the target call to each item and return the mapped sequence. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. The accepted spelling is =!>. Map each item, not the whole input sequence; =>> is rejected.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. The accepted spelling is =!>. Map each item, not the whole input sequence; =>> is rejected.

## 8. Structure

```xquery
string-join((' FILTER-A ', ' BOLT-M8 ') =!> normalize-space(), ',')
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Apply the target call to each item and return the mapped sequence. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. The accepted spelling is =!>. Map each item, not the whole input sequence; =>> is rejected.

## 12. Implementation

**C04 grammar review — 26 October 2026, published 1.26.3:** [Expression/call/focus reconciliation](../EXPRESSION-CALL-FOCUS-RECONCILIATION.md) covers S4-focus. Pipeline whole-value/fixed-focus, focus-function implicit/explicit signatures, lexical capture/restoration and accepted mapping syntax are mapped. Mapping arrow extra arguments incorrectly see mapped-item focus rather than the containing focus. Use for $u in ... return F($u,A...) when extra arguments depend on outer dot/position/size. C03 constructor capture/type coverage remains distinct. Host/profile propagation belongs to C06. This is bounded production/profile evidence, not full grammar or AC-04 acceptance.


Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/mapping-arrow/main.xpath) yields `FILTER-A,BOLT-M8`. The [edge case](../examples/mapping-arrow/edge.xpath) yields `0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: per-item mapping and result retention. Callback work and produced sequence size matter more than the spelling. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `mapping-arrow-main`, `mapping-arrow-edge`, `mapping-arrow-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_MappingArrow_31Mode_ThrowsXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_MappingArrow_AppliesFunctionPerItem`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_MappingArrow_WithArgumentsPrependsContextItem`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No feature-specific qt4 set assignment recorded here; unit evidence is listed explicitly. This is not a claim of absent upstream tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
