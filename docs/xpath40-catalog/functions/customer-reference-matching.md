<div align="center">
<img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak Customer-reference Matching">
<h1>Customer-reference Matching</h1>
<p>21 October 2026</p>
</div>

## 1. Name and Classification

**Customer-reference equality and collation — existing 3.1 functions with reviewed 4.0 contracts.** Covers fn:distinct-values, fn:index-of, fn:deep-equal, fn:compare and fn:contains-token. Functions are not4.0-only names; this adds no name to the 117-name inventory. [REQ-126](../../FEATURE_REQUESTS.md) equality/collation documentation slice; [review and source pins](../EQUALITY-COLLATION-CONTRACT-REVIEW.md), source base `5de2cfc5213529cdef419b9b7dec3cf9b6c03f7f`. Keyword syntax requires4.0. Unsupported draft forms are explicitly separated from supported calls.

Documentation delivery: [Bosak PR #398](https://github.com/Fytala-Charles/Bosak/pull/398). The review and verification receipts preserve source/specification pins, named cases, original findings and test hashes.

## 2. Intent

Match synthetic customer references under an explicit business collation, retain every matching position, test whole tag tokens and verify report order. Start with the [complete customer example](../examples/equality-contracts/README.md).

## 3. Also Known As

Equivalence classes, positional lookup, structural equality, three-way comparison and token membership are different operations, not interchangeable function aliases. The current compare parameter-name drift is not an accepted rename of the specification.

## 4. History / Supersedes

All five names exist in 3.1. The [positional3.1 report](../examples/equality-contracts/report-normal-31.xpath) and [keyword4.0 report](../examples/equality-contracts/report-normal-40.xpath) give the same stated output.4.0 expands compare to atomic operands and deep-equal to optional options; this review does not claim full implementation of those declarations. Current pins do not reconstruct historical adoption.

## 5. Motivation

Case-insensitive matching does not imply accent folding, Unicode normalization, substring matching, unordered sequence equality or case-insensitive map keys. Make each import policy observable instead of conflating those decisions.

## 6. Language Alignment

JavaScript Set and .NET Distinct are only rough analogies. XPath distinct-value representative/order rules are not a promise of insertion order; index-of returns all one-based positions; deep-equal is sequence/order-sensitive and does not mean reference identity. NaN behavior differs between value equality and deep equality. Culture/default comparer behavior from host languages must not silently select a business collation.

## 7. Applicability

Use supported explicit string collations for a declared reference/tag contract. Use codepoint comparison for identifiers whose spelling must remain exact. Do not use this miniature report as identity verification, a production schema, or fuzzy customer matching. Validate required/unique IDs separately.

## 8. Structure

```xquery
distinct-values($references, $collation)
index-of($references, $target, $collation)
compare($reference, $target, $collation) eq 0
contains-token($tags, $token, $collation)
deep-equal($actualIds, $expectedIds)
```

Full argument types/defaults and options differences are in the [signature audit](../audit-equality-collation-contracts.json). These shapes show supported positional examples, not the complete grammar.

## 9. Participants

The parser/binder handles keyword syntax and compatibility gates; FunctionLibrary registers overloads and metadata; EvaluationContext supplies default collation/timezone; the standard functions implement equality and atomization. The unchanged [catalog runner](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) executes each published file.

## 10. Collaborations

parse-xml supplies controlled input; simple mapping projects strings without imposing path-node order; explicit NFC normalization is optional policy, not collation preprocessing. The report combines all five operations while keeping membership, positions, identity and order separate.

## 11. Consequences

Explicit policy makes reports reproducible for the supplied supported collations. Case folding can collapse distinct business identifiers if chosen incorrectly. ASCII collation does not fold Ä/ä or normalize combining marks. A distinct count is deliberately used without relying on which duplicate spelling or order is returned.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Explicit collations, duplicate representatives and one-based matching positions; Sequence-order-sensitive deep equality and NaN behavior; Empty index-of collation correction and compare keyword boundary. **Boundaries:** Deep-equal map options are unsupported; supported collations do not imply normalization or complete UCA behavior. Follow-up groups: F01, F09. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Original reviewed implementation: [FunctionLibrary at the immutable source base](https://github.com/Fytala-Charles/Bosak/blob/5de2cfc5213529cdef419b9b7dec3cf9b6c03f7f/src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), including DistinctValues_2, IndexOf_3, DeepEqual_3, Compare_2/3 and ContainsToken. This documentation slice changes no engine code. Current compare keywords differ from the draft, two families lack metadata, empty positional index collation is corrected in source1.23.2 only at4.0 while deep-equal options maps still refuse; see the separate findings. Keyword grammar refuses at XPath31; the five positional functions remain available there, with compare's earlier string contract.

**Index-of source correction:** [Empty-collation default evidence](../INDEX-OF-EMPTY-COLLATION.md) records27 new standard-library checks, unchanged3.1 refusal and the corrected manifest case. The original source/audit pins above remain historical evidence.

## 13. Sample Code

The [self-contained walkthrough](../examples/equality-contracts/README.md) contains complete input, both expressions and exact normal/empty/reordered output. Normal output is `references=3;positions=1,2;ids=C1,C2;vip=2;expected-order=true`.58 manifest checks include modes, empty/invalid arguments, NaN, Unicode, maps and six full reports. Observed-gap fixtures are labelled separately and retain normative expected outcomes.

## 14. Performance Notes

The report scans the supplied customer sequence several times. No benchmark, asymptotic guarantee for every collation, or comparison with host collections is claimed. Large/import-streaming design remains a separate task; ad-hoc timings are not performance evidence.

## 15. Known Uses

The [manifest](../examples/cases.json) and CatalogExampleTests verify the named equality-contract cases. QT3 sets: [distinct-values](https://github.com/w3c/qt3tests/blob/201a6e466940cdfc727f4babfedcde5332b9f578/fn/distinct-values.xml), [index-of](https://github.com/w3c/qt3tests/blob/201a6e466940cdfc727f4babfedcde5332b9f578/fn/index-of.xml), [deep-equal](https://github.com/w3c/qt3tests/blob/201a6e466940cdfc727f4babfedcde5332b9f578/fn/deep-equal.xml), [compare](https://github.com/w3c/qt3tests/blob/201a6e466940cdfc727f4babfedcde5332b9f578/fn/compare.xml), [contains-token](https://github.com/w3c/qt3tests/blob/201a6e466940cdfc727f4babfedcde5332b9f578/fn/contains-token.xml). Corresponding qt4 files exist at the same fn paths under [qt4tests](https://github.com/qt4cg/qt4tests/blob/68910080d2d4d4092a1c6a908998ceec64ffc6d4/catalog.xml). Current [GatedSets](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) includes fn-compare/fn-contains-token; fn-distinct-values/fn-index-of/fn-deep-equal are not gated. Membership is source evidence, not a new conformance run. No gate is invented or promoted; AC-03's catalog-wide acceptance remains unresolved.

## 16. Related Features

[Sequence distinctness](sequence-distinctness.md), [shipment-event sequence checks](shipment-event-sequences.md), [typed measurement comparison](measurement-comparison.md), [ordering/default contracts](../ORDER-DEFAULT-CONTRACT-REVIEW.md), [complete review](../EQUALITY-COLLATION-CONTRACT-REVIEW.md).


**Validation:** Release build succeeds with the pre-existing conformance Program.cs:1868 CS8604 warning. All **10,304 solution tests pass**, including 7,363 XSLT tests and2,636 manifest rows. The 58 new focused checks overlap these totals. [Verification receipt](../EQUALITY-COLLATION-VERIFICATION.json) preserves all ten TRX hashes. Manual engine gates were not rerun: engine/harness source, gate membership and package version are unchanged.
