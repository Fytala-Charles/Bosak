<div align="center">
 <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala record update and recoercion catalog entry">
 <h1>Record updates with but with</h1>
 <p>Frozen XPath4.0 - structural record contract</p>
</div>

## 1. Name and Classification

**Record updates (`but with`)** - **Frozen 4.0**, REQ-118 S6b. The result is a new record with the original record's type annotation; merged values must satisfy that annotation through coercion. This entry maps existing structural behavior, not a new nominal declaration facility.

Delivery: [PR #90](https://github.com/Fytala-Charles/Bosak/pull/90), immutable `ca6e9c27cf4c62953763f800bdef2610ff64789a`; [RecordTypeTests](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs) pins source behavior. Current review pin, refreshed22 October 2026: [expressions XML](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xquery-40/src/expressions.xml), `id-with-expression`, `id-item-coercion-rules`, `casting-to-record`, `id-structural-record-types`. This is a current comparison, not a recovered historical adoption pin. The bounded signature-gate correction is in unpublished source 1.23.6; public 1.23.4 already supplies the documented4.0 record behavior.

## 2. Intent

Construct a changed record while keeping its declared field constraints and leaving the original value available.

## 3. Also Known As

A ButWithExpr performs a record update by construction. It is not an in-place assignment, a database mutation, or the discarded extensible-map proposal. The operands are record(*) and map(*); the record annotation supplies the result type.

## 4. History / Supersedes

A 3.1 adapter can select/validate fields, convert values and create a replacement map with map:merge/use-last. The [complete baseline](../examples/record-contracts/report-3.xsl) does that for the stock task. It has no record annotation and is not a universal replacement for record coercion. [The 4.0 report](../examples/record-contracts/report-4.xsl) casts the imported map, then constructs its amended record.

## 5. Motivation

A stock feed supplies quantity and delta. The report must show both the imported quantity and adjusted quantity, preserve row order and reject invalid lexical numbers. Record update makes a changed typed value explicit while retaining the before value. [Complete input/output and repair contract](../examples/record-contracts/README.md) makes the task independently runnable.

## 6. Language Alignment

This is XPath's annotation-driven reconstruction, not JavaScript object spread or an unrestricted immutable dictionary update. Undeclared fields and noncoercible values fail. A cast may discard surplus keys and convert a lexical string to an integer; update uses coercion and therefore does not automatically perform that lexical cast. Optional empty values are entries whose values are empty, not absent keys. Structural records are distinct from XSLT-defined nominal identity.

## 7. Applicability

Use it after establishing the target record annotation. At import boundaries, deliberately select cast versus coercion: cast filters extras; coercion rejects them. Supply correctly typed update values or convert them explicitly. The stock task casts @delta to integer before updating qty. This example permits negative adjustments and does not enforce a nonnegative-stock rule.

## 8. Structure

```xpath
((map{'a':1} cast as record(a as xs:integer)) but with map{'a':2})?a
```

This is the runnable `record-contract-update-value` probe and returns2. The original-preservation probe produces `1|2` from before/after lookups.

## 9. Participants

[XPathParser](../../../src/Bosak.XPath.Parser/Ast/XPathParser.cs) handles record syntax and but-with expressions. [VmEngine](../../../src/Bosak.XPath.Runtime/Vm/VmEngine.cs) applies the existing annotation/coercion path. [RecordTypeTests](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs) and the public [documentation runner](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) provide named evidence; no new public API is added.

## 10. Collaborations

Cast constructs the import record, adding a present-but-empty optional note and dropping import-only trace. Update merges the replacement quantity and recoerces to the left annotation. The output keeps the original quantity and adjusted value in a row map. Map access and record lookup are distinct: map:get can inspect a missing key without the record lookup's undeclared-field error.

## 11. Consequences

Unknown fields, a plain-map left operand, a non-map right operand and an empty required field raise XPTY0004 in the supplied probes. Integer-to-double promotion is checked; arbitrary string-to-integer update casting is not implied. Optional fields may be set to empty while retaining key presence. Nested structural conversion and declaration order are bounded tested cases, not proof of every composite/nominal type combination.

## 12. Implementation

Thirty-five XPath probes cover cast/coercion/update distinctions; three explicit 3.1 refusal cases and eight complete3.0/4.0 reports bring the slice to46 documentation checks. The first run exposed a record-signature gate leak in old profiles, separate from value semantics. [The parser finding](../INLINE-RECORD-TYPE-GATE.md) and 44 dedicated API regressions preserve the failures and bounded correction. The new examples do not invent or promote a qt4 gate.

## 13. Sample Code

The [complete report](../examples/record-contracts/README.md) uses `<stock xmlns="urn:fytala:stock"><item sku="BOLT" qty="10" delta="2"/><item sku="NUT" qty="4" delta="-1"/></stock>` and yields `BOLT:10->12|NUT:4->3|items=2;total=15` in both versions. Empty input yields `items=0;total=0`; reordered input changes line order; invalid delta `oops` raises FORG0001. No external variables or resources are needed. Report stylesheets specify namespaces and all context.

## 14. Performance Notes

Reconstruction checks declared fields and may recursively convert nested values; it can allocate a new record and entries. The report retains before/after values and is not a streaming/persistence benchmark. No speed or memory advantage over the baseline is claimed; use the benchmark harness for measured workloads.

## 15. Known Uses

Applicable AC-03 evidence: `record-contract-update-value`, `record-contract-update-original`, `record-contract-update-annotation`, `record-contract-update-extra`, `record-contract-update-promote`, `record-contract-update-empty-optional` and the report cases, all enumerated in [the slice audit](../RECORD-CONTRACTS.json) and [manifest](../examples/cases.json), run by CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome.

Named [RecordTypeTests](../../../tests/Bosak.XPath.Api.Tests/RecordTypeTests.cs): `ButWith_ResultKeepsRecordAnnotation`, `ButWith_UnknownField_XPTY0004`, `ButWith_PlainMapLhs_XPTY0004`, `ButWith_CoercesRhsValuesToFieldTypes`, `ButWith_ResultEntriesFollowDeclarationOrder`. [InlineRecordTypeVersionGateTests](../../../tests/Bosak.XPath.Api.Tests/InlineRecordTypeVersionGateTests.cs) independently verifies44 version/signature controls. No feature-specific qt4 gated-set assignment is made. Historical upstream RecordType fixtures reflect an older draft; named tests and executable examples are explicit applicable evidence, not whole-suite conformance or automatic AC-03 closure.

## 16. Related Features

[Structural record casts/coercion](structural-records.md), [typed-let coercion](typed-let-coercion.md), [XSLT named-record identity](nominative-records.md), [rejected extensible-map types](../rejected/extensible-map-types.md), [current grammar/acceptance reconciliation](../COVERAGE-RECONCILIATION.md).
