<div align="center">
 <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala inline record signature version-gate correction">
 <h1>Inline record signature version gate</h1>
 <p>22 October 2026 - source 1.23.6, unpublished</p>
</div>

`function($r as record(a)) { $r }` was accepted in 3.0/3.1 because inline signatures retain type text through XPathParser.SkipSequenceType, bypassing the record gate in ParseSequenceType. Record syntax is a4.0 addition; [XPath3.1 InlineFunctionExpr](https://www.w3.org/TR/xpath-31/#doc-xpath31-InlineFunctionExpr) uses the [3.1 SequenceType/ItemType grammar](https://www.w3.org/TR/xpath-31/#doc-xpath31-ItemType), which does not admit record types. Current4.0 review sources and sections are pinned in [the record delivery](RECORD-CONTRACTS.md).

The new catalog run had41 passes and5 failures: four successful parameter-coercion examples failed their asserted3.1 refusal, and the explicit 3.1 parameter control also failed. The4.0 results and paired business reports passed. These expected refusals are retained unchanged.

[InlineRecordTypeVersionGateTests](../../tests/Bosak.XPath.Api.Tests/InlineRecordTypeVersionGateTests.cs) reproduces16 old-profile failures and28 passes before the correction. It covers direct/any-record parameters and returns, nested map/array/function signature positions, frozen/experimental 4.0, and ordinary element/attribute/PI names `record`.

The bounded fix checks an unprefixed record token followed by `(` while retaining sequence-type text when4.0 is disabled. It reuses the existing XPST0003 diagnostic and leaves4.0 conversion behavior and ordinary node names intact. Both old profiles reject the signature at compilation. All 44 API controls and 46 catalog examples then pass. This does not validate every possible inline type syntax, other4.0 type gate, or host declaration.

This is a compatibility bug fix, so source moves1.23.5 to1.23.6. Public and Braid dependency remain1.23.4. All 10,636 solution tests and both manual engine gates pass (QT3 31,142/0/679; qt4 8,278/0/1,894). Final receipts and original failures are in [RECORD-CONTRACTS.json](RECORD-CONTRACTS.json). No release or upstream submission is included.
