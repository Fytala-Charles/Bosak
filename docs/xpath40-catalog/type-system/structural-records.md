<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Structural record types">
  <br><br><h1>Structural record types</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**XSLT host/type review — 21 October 2026:** [Current mapping](../TYPE-PATTERN-GRAMMAR.md) distinguishes stylesheet aliases, conversion in bindings and noncoercing type/pattern tests. Named XSLT record identity is a separate bounded published contract; this does not add standalone XPath declarations.


## 1. Name and Classification

**Structural record types** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: REQ-118 S6b; structural record evidence in RecordTypeTests. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: structural record types](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-structural-record-types), [item coercion](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-item-coercion-rules), [but with operator](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-with-expression). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #90](https://github.com/Fytala-Charles/Bosak/pull/90), [immutable mainline revision `ca6e9c27`](https://github.com/Fytala-Charles/Bosak/commit/ca6e9c27cf4c62953763f800bdef2610ff64789a) (2026-10-08). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Cast the agreed shape to an annotated record before using the record type contract.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, validate the map keys and field values explicitly. Compare the [complete earlier-version baseline](../examples/structural-records/before.xpath) with the [complete expression](../examples/structural-records/main.xpath). Both yield `EUR` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

An adapter receives a small map with invoice ID and currency fields. Cast the agreed shape to an annotated record before using the record type contract. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. A plain map is not an annotated record in Bosak. Coercion policy and undeclared fields must be understood before adapting broad payloads.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. A plain map is not an annotated record in Bosak. Coercion policy and undeclared fields must be understood before adapting broad payloads.

## 8. Structure

```xquery
(map { 'invoiceId': 'INV-1042', 'currency': 'EUR' } cast as record(invoiceId as xs:string, currency as xs:string))?currency
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Cast the agreed shape to an annotated record before using the record type contract. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. A plain map is not an annotated record in Bosak. Coercion policy and undeclared fields must be understood before adapting broad payloads.

The [record-contract delivery](../RECORD-CONTRACTS.md) and [complete stock task](../examples/record-contracts/README.md) add bounded value/error comparisons and the separate [record-update entry](record-updates.md).

| Contract | Cast | Coercion / record update |
|---|---|---|
| Extra source key | Discarded when outside the target record fields | Rejected with XPTY0004. |
| String quantity to integer | Cast when required; invalid lexical value raises FORG0001 | A string is not automatically an integer; supplied parameter/update probes raise XPTY0004. |
| Missing optional field | Create a key whose value is empty | Create that present empty entry; it is not absence. |
| Missing/empty required field | XPTY0004 | XPTY0004. |
| Numeric/nested conversion | Keep matching values or apply the field cast | Apply field coercion recursively; tested integer-to-double promotion. |
| Field order | Constrained by the target record | Coercion/recoercion follows field declaration order in the probes. |
| record(*) | Assertion of an already annotated record | A plain map is not itself an annotated record. |
| Changed value | Construct a record | but with reconstructs under the existing annotation; original value remains available. |

These are tested structural cases, not exhaustive casts, nominal static/default fields or host declarations. Source1.23.6 also fixes an earlier-profile inline-signature leak; [the finding](../INLINE-RECORD-TYPE-GATE.md) preserves the original failing3.1 refusals instead of treating them as supported syntax.

## 12. Implementation

**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies Cast constructs an annotated record and discards surplus fields; Coercion rejects surplus fields and recursively coerces declared fields; Required/optional emptiness, declaration order, record(*) and undeclared lookup/map:get distinctions. **Boundary:** A plain map is not an annotated record. Structural matches do not require annotation identity with the tested target, while named identity remains host-defined. Complete record/nominal/schema conversion combinations are not inferred. Ownership: T04, T05. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/structural-records/main.xpath) yields `EUR`. The [edge case](../examples/structural-records/edge.xpath) yields `false`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: field-dependent validation/coercion and record construction. Nested field contracts can add further work. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `structural-records-main`, `structural-records-edge`, `structural-records-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Coercion_BuildsAnnotatedRecord`](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [`Coercion_MissingFieldBecomesEmptyEntry`](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs), [`Coercion_SurplusKeys_XPTY0004`](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs). Hand-written tests pin annotated-record semantics; the delivery explicitly identified upstream `prod-RecordType` as an older draft. Do not infer promotion from these tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

[XSLT named-record boundary](../deferred/xslt-record-type.md) separates named
stylesheet declarations from this shipped XPath structural contract. It also separates supported structural bindings from named identity; older deferral and source snapshots remain historical.

See [record updates](record-updates.md), the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
