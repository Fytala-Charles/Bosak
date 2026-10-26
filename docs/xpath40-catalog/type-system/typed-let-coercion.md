<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Typed let coercion">
  <br><br><h1>Typed let coercion</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited typed binding; choice blocker retained**. C05-K02 remains for choice-typed let/argument/return; passing scalar examples do not certify all declared types or error precedence. Final owner acceptance remains C13; earlier checkpoints are historical.

## 1. Name and Classification

**Typed let coercion** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-123 destructuring/typed-let cluster; local coercion tests. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: let expressions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-let-expressions), [item coercion](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-item-coercion-rules). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #129](https://github.com/Fytala-Charles/Bosak/pull/129), [immutable mainline revision `b8e4b9f5`](https://github.com/Fytala-Charles/Bosak/commit/b8e4b9f510543c04d88a7c10ee2af28808a0fd5c) (2026-10-10). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

State the type requirement at the binding; integral in-range values can be relabeled.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, an explicit constructor and separate validation express the boundary. Compare the [complete earlier-version baseline](../examples/typed-let-coercion/before.xpath) with the [complete expression](../examples/typed-let-coercion/main.xpath). Both yield `true` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A small sensor code must fit the receiving signed-short value space. State the type requirement at the binding; integral in-range values can be relabeled. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Coercion is not truncation. A fractional number cannot be silently relabeled as an integer.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Coercion is not truncation. A fractional number cannot be silently relabeled as an integer.

## 8. Structure

```xquery
let $code as xs:short := 42 return $code instance of xs:short
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: State the type requirement at the binding; integral in-range values can be relabeled. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Coercion is not truncation. A fractional number cannot be silently relabeled as an integer.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S6-record-conversion, REQ123-destructuring. C03 cast/coercion/lookup boundaries remain: casts filter surplus fields, coercion rejects them, optional keys become present empty entries, and ordinary maps are not annotated records. Full composite/nominal/static/default/package and error-eagerness combinations remain unaccepted; bounded nominal defaults from C03 remain supported. Sequence rest/short inputs, array member groups/surplus/short errors, map missing keys, per-variable/whole-pattern types, duplicates and lexical shadowing are mapped. Undeclared annotated-record fields incorrectly bind empty when destructured. K03 retains the required record-lookup XPTY0004 expectation. Plain-map missing-key emptiness remains a different supported contract. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies Typed binding/coercion differs from instance-of and cast; Record and numeric promotion with surplus/invalid value refusal; Inline function parameter/result conversion and modern versus paired static/runtime BC controls. **Boundary:** The probe runner explicitly sets both compile-time and runtime BC flags. These controls do not certify all XSLT 1.0/3.0 host propagation, schema-aware constructors or arbitrary function subtyping. C02 atomic-type-annotation empty-input defect remains open. Ownership: T03, T05, F08-atomic-annotation-empty. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

The [record-contract slice](../RECORD-CONTRACTS.md) adds typed-let record promotion and surplus-field refusal probes. A typed binding uses coercion, not an import cast; extra fields are rejected. See [casts versus coercion](structural-records.md) and [record updates](record-updates.md) for the bounded record-specific matrix.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/typed-let-coercion/main.xpath) yields `true`. The [edge case](../examples/typed-let-coercion/edge.xpath) yields `XPTY0004 (expected error)`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: small per-binding type/coercion overhead; complex declared types add their own checks. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `typed-let-coercion-main`, `typed-let-coercion-edge`, `typed-let-coercion-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Evaluate_TypedLet_FrozenXPath40_Coerces`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No feature-specific qt4 set assignment recorded here; unit evidence is listed explicitly. This is not a claim of absent upstream tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
