<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Choice item types">
  <br><br><h1>Choice item types</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited membership/cast; matching-coercion blocker**. C05-K02: six fresh controls still convert an already-matching string to an earlier integer alternative; explicit-cast success does not resolve this. Final owner acceptance remains C13; earlier checkpoints are historical.

## 1. Name and Classification

**Choice item types** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S6a; choice item type evidence in VersionGateTests. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: choice item types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-choice-item-types). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #89](https://github.com/Fytala-Charles/Bosak/pull/89), [immutable mainline revision `83d152ff`](https://github.com/Fytala-Charles/Bosak/commit/83d152ffdba55fe160f33366fa14ed2922fa2f9d) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Express the permitted alternatives in one item type.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, combine individual instance-of tests with or. Compare the [complete earlier-version baseline](../examples/choice-types/before.xpath) with the [complete expression](../examples/choice-types/main.xpath). Both yield `true` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A legacy order identifier can be an integer or a string, but not a fractional amount. Express the permitted alternatives in one item type. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Explicit cast tries alternatives in order. Coercion must preserve an already-matching alternative, but Bosak currently converts matching strings to an earlier integer alternative (C05-K02). The instance-of examples only test membership and do not perform conversion.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Explicit cast tries alternatives in order. Coercion must preserve an already-matching alternative, but Bosak currently converts matching strings to an earlier integer alternative (C05-K02). The instance-of examples only test membership and do not perform conversion.

## 8. Structure

```xquery
 'PO-8801' instance of (xs:integer|xs:string)
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Express the permitted alternatives in one item type. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Explicit cast tries alternatives in order. Coercion must preserve an already-matching alternative, but Bosak currently converts matching strings to an earlier integer alternative (C05-K02). The instance-of examples only test membership and do not perform conversion.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S6-types. Membership, explicit casts and coercion are separate. Cast alternatives are attempted in order; already-matching choice coercion currently converts a string to an earlier integer alternative, contrary to the current matched-alternative rule. K02 retains typed let/argument/return failures with independent expectations. Namespace/type aliases are host-defined, not standalone global registrations. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies Membership is distinct from coercion; Already-matching alternatives and ordered coercion of unmatched untyped values; 3.1 choice grammar refusal. **Boundary:** The tested integer/string alternatives do not certify every schema union, function/record alternative or error-precedence combination. Schema union/list context has separate named evidence. Ownership: T03, T05. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/choice-types/main.xpath) yields `true`. The [edge case](../examples/choice-types/edge.xpath) yields `false`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: alternative type matching/coercion. The number and ordering of alternatives affect the work performed. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `choice-types-main`, `choice-types-edge`, `choice-types-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_ChoiceItemType_31Mode_ThrowsXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_Choice_InstanceOf_AnyAlternativeMatches`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_Choice_Cast_FirstCoercibleAlternativeWins`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No feature-specific qt4 set assignment recorded here; unit evidence is listed explicitly. This is not a claim of absent upstream tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
