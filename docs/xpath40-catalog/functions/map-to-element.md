<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Map-to-element conversion">
  <br><br><h1>Map-to-element conversion</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

**K03-C02 serialization — 27 October 2026:** [Content-preserving name decoding](../../conformance/k03-serialization/README.md) restores encoded element/attribute name tokens while preserving existing literal node content, namespace URIs and declarations. All40 focused regressions and21,960 solution tests pass; QT3 remains31,145/0/676 with identical676 skips, and qt4 remains8,436/0/1,894. The original supplementary-name skip remains. Candidate1.26.10 unpublished/public1.26.3. Next: reversible name-storage integration with loader/accessor/copy round-trip verification; no public API, baseline or XSD1.1 ownership change. REQ-126 remains Complete.



**K03-C01 XML-name validation — 27 October 2026:** [Scalar validation correction](../../conformance/k03-name-validation/README.md) replaces category/surrogate-pair approximation with explicit XML name ranges and rejects malformed UTF-16. All78 focused tests pass (baseline18 failures), all21,920 solution tests pass, and gates stay QT3 31,145/0/676 and qt4 8,436/0/1,894. All676 QT3 skip names/reasons are unchanged. Original supplementary construction and literal-content serialization findings remain open; next is K03-C02 serialization preservation before provider storage integration. Candidate1.26.9 unpublished/public1.26.3; no baseline or XSD1.1 capability change. REQ-126 remains Complete.



**K03 Unicode-name trace — 27 October 2026:** [Bounded investigation](../../conformance/k03-unicode-names/README.md) reproduces map-to-element-053 and separates three open corrections: scalar NCName validation, reversible provider name storage, and serialization that preserves literal sentinel text. All 30 isolated diagnostics complete without timeouts; no engine or baseline change. Next: K03-C01 name-validation regressions/correction, followed by serialization preservation before map storage integration. QT3 remains31,145/0/676 and qt4 remains8,436/0/1,894; solution21,842 and gates are inherited unchanged-source evidence. Candidate1.26.8 unpublished/public1.26.3; REQ-126 remains Complete. Bosak.Schema XSD1.1 ownership is unchanged.



**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 2 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. The reduced example does not certify the whole conversion-plan/record/namespaces space; supplementary provider Unicode limits belong to the deferred skip queue. Current-draft escape-names encoding is unsupported; use XML-compatible field names and verified conversion options. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Map-to-element conversion** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [fn:map-to-element](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-map-to-element). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #106](https://github.com/Fytala-Charles/Bosak/pull/106), [immutable mainline revision `1bb73147`](https://github.com/Fytala-Charles/Bosak/commit/1bb731474c6ba48dded66441183c676a0ebf9761) (2026-10-09). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Convert an agreed map shape back into XML and inspect its text value.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, an XSLT template or XQuery constructor builds the target XML explicitly. Compare the [complete earlier-version baseline](../examples/map-to-element/before.xsl) with the [complete expression](../examples/map-to-element/main.xpath). Both yield `FILTER-A` for this synthetic input. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A map-based integration step must deliver a small XML product element. Convert an agreed map shape back into XML and inspect its text value. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. A conversion is not automatically a lossless round trip for arbitrary mixed-content XML; agree on namespaces and layout.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. A conversion is not automatically a lossless round trip for arbitrary mixed-content XML; agree on namespaces and layout.

## 8. Structure

```xquery
string(fn:map-to-element(map { 'sku': 'FILTER-A' }))
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Convert an agreed map shape back into XML and inspect its text value. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. A conversion is not automatically a lossless round trip for arbitrary mixed-content XML; agree on namespaces and layout.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Single tested map-to-XML conversion shape; QName/name/layout, nested content and option boundaries are distinct. **Boundaries:** The reduced example does not certify the whole conversion-plan/record/namespaces space; supplementary provider Unicode limits belong to the deferred skip queue. Current-draft escape-names encoding is unsupported; use XML-compatible field names and verified conversion options. Follow-up groups: F01, F14. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

**Supplementary contract review (2026-10-12):** Keyword drift: implemented input differs from pinned map; the [signature audit](../SIGNATURE-CONTRACT-AUDIT.md) publishes paired success/refusal files. Positional conversion remains separate evidence.

## 13. Sample Code

Input is synthetic and declared directly in the complete expression. The [complete expression](../examples/map-to-element/main.xpath) yields `FILTER-A`. The [edge case](../examples/map-to-element/edge.xpath) yields `0`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.

## 14. Performance Notes

Expected class: output-size dependent construction. The resulting element tree occupies memory in addition to its source map. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `map-to-element-main`, `map-to-element-edge`, `map-to-element-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs). Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`MapToElement_SingleEntryMap_BuildsElement`](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs), [`MapToElement_EmptySequence_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs), [`MapToElement_NonMapInput_Xpty0004`](../../../tests/Bosak.XPath.Standard.Tests/ElementMapTests.cs). Named frozen gate: `fn-map-to-element`, enforced after the [gate-name repair](../MAP-TO-ELEMENT-GATE-REPAIR.md). The native set contributes156 passed /0 failed /2 skipped; the complete expanded gate passes8,434/0/1,896. Synthetic controls verify that a failing assertion now causes exit2. This is bounded frozen-revision evidence; current-upstream map/element contract changes remain separate.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
