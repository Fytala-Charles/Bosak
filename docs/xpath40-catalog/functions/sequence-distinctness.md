<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Sequence identity checks">
  <br><br><h1>Sequence identity checks</h1>
  <p>Frozen 4.0</p>
</div>

## 1. Name and Classification

**Sequence identity checks — Frozen 4.0.** [Bosak PR #83](https://github.com/Fytala-Charles/Bosak/pull/83) / [immutable revision `532d04f4`](https://github.com/Fytala-Charles/Bosak/commit/532d04f4672df430d93752e3158fc3cdfd4c7c39). This is the shipped engine delivery.

**External specification references (current drafts; links checked 2026-10-11):** [fn:all-different](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-all-different), [fn:duplicate-values](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-duplicate-values). Historical adopted specification source revision was not retained; this remains an explicit provenance gap.

## 2. Intent

Refuse ambiguous order identities and identify duplicates for repair.

## 3. Also Known As

Use the exact spellings in the linked examples; no undocumented aliases are adopted.

## 4. History / Supersedes

For this string-ID contract, XPath 3.1 can compare `count(distinct-values($ids))` with `count($ids)`, as the published baseline demonstrates. That detects existence of duplicates but does not return their second-appearance order. Broader atomic inputs need a separately reviewed equality policy.

all-different reports whether atomic values are pairwise distinct. duplicate-values reports each duplicated value once, in second-appearance order. Equality follows the function collation and atomic comparison rules; this chapter restricts identity to strings and the default codepoint collation. Both empty and singleton inputs are all-different. Blankness is a separate business rule.

## 5. Motivation

Refuse ambiguous order identities and identify duplicates for repair. The order-routing chapter places validation before dispatch so rejected records stay available for repair.

## 6. Language Alignment

Duplicate detection is not document identity, structural map equality or automatic Unicode normalization. Do not trim or case-fold identifiers implicitly; choose that policy before checking uniqueness.

## 7. Applicability

Use when the consumer opts into XPath40 and needs the exact validation or ordering contract described here. Keep the XPath 3.1 baseline when that opt-in is unavailable.

## 8. Structure

```xquery
string(all-different(('R1','R2','R1'))) || '/' || string-join(duplicate-values(('R1','R2','R1','R2')), ',')
```

This complete expression demonstrates the stated contract, not the full grammar.

## 9. Participants

The public XPath API compiles at the manifest mode and evaluates without application variables. The routing chapter additionally supplies an XML document context.

## 10. Collaborations

The routing planner passes an XDM map to portable XSLT through a required parameter. See [the full chapter](../worked-examples/order-routing/README.md).

## 11. Consequences

all-different reports whether atomic values are pairwise distinct. duplicate-values reports each duplicated value once, in second-appearance order. Equality follows the function collation and atomic comparison rules; this chapter restricts identity to strings and the default codepoint collation. Both empty and singleton inputs are all-different. Blankness is a separate business rule. Duplicate detection is not document identity, structural map equality or automatic Unicode normalization. Do not trim or case-fold identifiers implicitly; choose that policy before checking uniqueness.

## 12. Implementation

The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) runs the exact files, asserts declared results/errors, and checks default-3.1 refusal for successful 4.0 forms. No engine code changes or conformance promotions are included.

**Supplementary contract review (2026-10-12):** [Five-family predicate review](../PREDICATE-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed shipment acceptance README](../examples/predicate-contracts/README.md) add 102 exact checks. They cover explicit keyword modes, collation/defaults, numeric/NaN/node/map boundaries, callback direction/result conversion and eight complete 3.1/4.0 reports. All five keyword families remain unavailable. Equality functions tolerate the tested singleton map despite atomic signatures; numeric and empty-string collation processing remain explicit engine follow-ups. The string-valued workflow uses supported positional forms. Initial delivery/unit/gate evidence and historical adopted-pin limits are unchanged.

## 13. Sample Code

[main.xpath](../examples/sequence-distinctness/main.xpath): returns `false/R1,R2` at mode 40.

[edge.xpath](../examples/sequence-distinctness/edge.xpath): returns `true/0` at mode 40.

[before.xpath](../examples/sequence-distinctness/before.xpath): returns `false` at mode 31.

## 14. Performance Notes

Duplicate checks and index selection may traverse the input; some/every may finish early. Partitioning creates arrays and retains input items. These are qualitative expectations, not benchmark results. Stateful or expensive callbacks change workload costs.

## 15. Known Uses

Executable IDs: `sequence-distinctness-main`, `sequence-distinctness-edge`, `sequence-distinctness-before`. [`AllDifferent_SpecSemantics`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`DuplicateValues_ResultInSecondAppearanceOrder`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`DuplicateValues_NoDuplicates_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs) Neither exact-name set is currently listed in GatedSets; named unit tests and published examples are the evidence here. [Gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).

## 16. Related Features

[Catalog index](../README.md), [routing chapter](../worked-examples/order-routing/README.md), [structural records](../type-system/structural-records.md), [experimental scan](../experimental/fn-scan.md).

