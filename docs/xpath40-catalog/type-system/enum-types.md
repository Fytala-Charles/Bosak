<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Enumeration types">
  <br><br><h1>Enumeration types</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**XSLT host/type review — 21 October 2026:** [Current mapping](../TYPE-PATTERN-GRAMMAR.md) distinguishes stylesheet aliases, conversion in bindings and noncoercing type/pattern tests. Named XSLT record identity is a separate bounded published contract; this does not add standalone XPath declarations.


## 1. Name and Classification

**Enumeration types** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S6a; XPath enum type evidence in VersionGateTests. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: enumeration types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-enumeration-types). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #89](https://github.com/Fytala-Charles/Bosak/pull/89), [immutable mainline revision `83d152ff`](https://github.com/Fytala-Charles/Bosak/commit/83d152ffdba55fe160f33366fa14ed2922fa2f9d) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Name a finite allowed set in the type test.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, test membership against a sequence of strings. Compare the [complete earlier-version baseline](../examples/enum-types/before.xpath) with the [complete expression](../examples/enum-types/main.xpath). Both yield `true` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

An order workflow only accepts ready and review-required status strings. Name a finite allowed set in the type test. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. Matching is case-sensitive; an allowed enum does not establish that the rest of a business record is valid.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. Matching is case-sensitive; an allowed enum does not establish that the rest of a business record is valid.

## 8. Structure

```xquery
 'review-required' instance of enum('ready', 'review-required')
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Name a finite allowed set in the type test. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. Matching is case-sensitive; an allowed enum does not establish that the rest of a business record is valid.

## 12. Implementation

**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies Case-sensitive string membership; Untyped input coercion to a matching enum and invalid binding refusal; 3.1 enumeration grammar refusal. **Boundary:** Enum membership validates one value, not a business record. Broader derived/string unions and all inline/host positions remain outside these bounded checks. Ownership: T03, T05. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/enum-types/main.xpath) yields `true`. The [edge case](../examples/enum-types/edge.xpath) yields `false`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: scalar membership testing against the declared allowed values. Large enumerations merit representative measurement. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `enum-types-main`, `enum-types-edge`, `enum-types-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_EnumType_31Mode_ThrowsXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Compile_EnumType_EmptyMemberList_ThrowsXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_Enum_InstanceOf_MembershipIsCodepointSensitive`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No feature-specific qt4 set assignment recorded here; unit evidence is listed explicitly. This is not a claim of absent upstream tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
