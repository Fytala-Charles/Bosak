<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Type, map and array template patterns">
  <h1>Type, map and array template patterns</h1>
  <p>21 October 2026</p>
</div>

## 1. Name and Classification

**Implemented bounded XSLT4.0 surface in public1.23.2.** This entry maps tested type, record, map-entry and array-member patterns; it is not the complete pattern grammar. [Pinned source/deliveries](../TYPE-PATTERN-GRAMMAR.md).

## 2. Intent

Dispatch structured values to template rules while keeping key presence, member cardinality, type annotation and nominal identity explicit.

## 3. Also Known As

Type pattern, string-key map pattern, array member pattern and named record pattern in their respective reviewed forms.

## 4. History / Supersedes

The intake [3.0 baseline](../examples/intake-types/report-3.xsl) uses a named template and explicit predicates. The [4.0 report](../examples/intake-types/report-4.xsl) uses structured-value template patterns. Same results for these fixtures do not establish universal equivalence or a general grammar migration.

## 5. Motivation

Route valid line maps and nonempty SKU arrays while making zero quantities and empty bundles visible as review cases.

## 6. Language Alignment

Pattern syntax is not XPath constructor syntax. `array(atomic(xs:integer)+)` in match differs from a sequence type in @as. Matching does not perform the annotation/conversion that an @as binding may apply.

## 7. Applicability

Use when the consumer supports the relevant XSLT host/effective-version contract. Prefer the portable named-template form when targeting3.0 processors.

## 8. Structure

Reviewed shapes: `~t:shape`, `record(t:point)`, `{'v':.?}`, `{'v':.,*}` and `array(atomic(xs:integer)+)`. Namespace prefixes, context and complete files are in the [example family](../examples/intake-types/README.md). These shapes do not replace the normative grammar.

## 9. Participants

[PatternCompiler](../../../src/Bosak.Xslt/Patterns/PatternCompiler.cs), [StringKeyMapPattern](../../../src/Bosak.Xslt/Patterns/StringKeyMapPattern.cs), [ArrayPattern](../../../src/Bosak.Xslt/Patterns/ArrayPattern.cs) and [RecordPattern](../../../src/Bosak.Xslt/Patterns/RecordPattern.cs).

## 10. Collaborations

Use named aliases for type tests, explicit conversion at bindings, and explicit template priorities where application policy needs them. The report uses disjoint map/array patterns and explicit priorities rather than claiming all default-priority interactions.

## 11. Consequences

A closed map pattern rejects extra keys unless opened; optional entry values still require the key. Member occurrence indicators constrain each sequence, not array size. Empty arrays can satisfy universal member constraints; business nonempty checks remain explicit.

## 12. Implementation

New manifest checks use default Legacy host and effective version4.0, hence frozen XPath40. Legacy3.0 declaration/pattern refusals are explicit. Host selection, module/package identity, imports and authoring contexts need their own applicable tests; no exhaustive cross-context claim follows. No engine behavior changes.

## 13. Sample Code

[map-empty-present](../examples/intake-types/map-empty-present.xsl) versus [map-empty-missing](../examples/intake-types/map-empty-missing.xsl); [array-vacuous](../examples/intake-types/array-vacuous.xsl) versus [array-empty-member](../examples/intake-types/array-empty-member.xsl); six full [reports](../examples/intake-types/README.md).

## 14. Performance Notes

Cost follows the value/type/pattern being checked and any conversion/materialization. Field/member walks and predicates can add work; no benchmark, universal speedup or measured memory claim is supplied.

## 15. Known Uses

New intake-types map/array/type rows; [map-entry tests](../../../tests/Bosak.Xslt.Tests/Xslt40MapEntryPatternTests.cs), [array tests](../../../tests/Bosak.Xslt.Tests/Xslt40ArrayPatternTests.cs) and [type tests](../../../tests/Bosak.Xslt.Tests/Xslt40TypePatternTests.cs). No new qt4tests gate membership; applicable-gate acceptance remains partial.

## 16. Related Features

[xsl:item-type](../deferred/xslt-item-type.md), [xsl:record-type](../deferred/xslt-record-type.md), [switch/array/select](../SWITCH-ARRAY-SELECT-GRAMMAR.md), [full bounded inventory](../TYPE-PATTERN-GRAMMAR.md).
