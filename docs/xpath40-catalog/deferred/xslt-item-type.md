<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala xsl:item-type: published alias boundaries">
  <h1>xsl:item-type: published alias boundaries</h1>
  <p>21 October 2026</p>
</div>

**C09 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-1.md) records 3 contract groups with exact sample/context, applicable evidence and language-alignment limits. Status: **Limited implemented declaration**. No complete alias/schema/package/type-conversion acceptance; C03/C05 limits remain. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Implemented bounded XSLT4.0 surface in public1.23.2; complete declaration contract not certified.** This existing path is retained for links; deferred describes its historical dossier, not all present behavior. [Current pinned review](../TYPE-PATTERN-GRAMMAR.md).

## 2. Intent

Give an existing permitted type a stylesheet-context name without inventing a new nominal record identity.

## 3. Also Known As

Named item-type alias; enumeration or structural-record alias in the reviewed forms. It does not itself request a generated constructor.

## 4. History / Supersedes

The earlier REQ-118 deferral and1.2.0 implementation logs remain in the [historical snapshot](../../history/REQ126-ITEM-TYPE-SNAPSHOT-20261021.md). Enumeration-alias delivery isf53bc80f; structural-record aliases are669c862a. Current review provenance does not reconstruct the original adopted draft or promote every deferred declaration contract.

## 5. Motivation

An intake classifier can name an integer quantity or permitted status set, while keeping validation and routing policy visible.

## 6. Language Alignment

An alias is not a C# nominal class. Instance-of tests the supplied value; an XSLT @as binding can first perform conversion. Identical textual values or fields do not imply every type check agrees.

## 7. Applicability

Use only the declared/verified aliases in a supporting host. Use ordinary3.1 types and explicit predicates in a portable3.0 stylesheet.

## 8. Structure

Example declarations: `xsl:item-type name="t:status" as="enum('ready','queued')"` and `name="t:shape" as="record(x as xs:integer)"`. These are descriptive reviewed forms, not a complete grammar.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs), the XPath type system and the compiled stylesheet context resolve names. No new public registration API is introduced here.

## 10. Collaborations

Combine aliases with bindings and type/pattern checks; compare the [intake report](../examples/intake-types/README.md) and [record identity reference](xslt-record-type.md).

## 11. Consequences

Reduces repeated type syntax but adds a host/context dependency. Expanded names matter; enum treat mismatch raises XPDY0050. Do not treat successful coercion as evidence that an unconverted map already carried an annotation.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S8-patterns. Fresh version-4 stylesheets in both hosts cover array/member sequences, map missing-versus-empty/surplus keys, aliases, annotated records and nominal identity. Predicates can fail to match without propagating their dynamic errors in the tested forms. No claim of complete declaration/import/package visibility, every composite pattern, generic JNode dispatch or earlier-profile promotion. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


New manifest checks use default Legacy host and effective version4.0, hence frozen XPath40. Legacy3.0 declaration/pattern refusals are explicit. Host selection, module/package identity, imports and authoring contexts need their own applicable tests; no exhaustive cross-context claim follows. No engine behavior changes.

## 13. Sample Code

[structural-binding.xsl](../examples/intake-types/structural-binding.xsl) returns true|false for converted versus original map. [enum-allowed](../examples/intake-types/enum-allowed.xsl) and [enum-rejected](../examples/intake-types/enum-rejected.xsl) distinguish allowed values. The complete report has normal, empty and reordered cases.

## 14. Performance Notes

Cost follows the value/type/pattern being checked and any conversion/materialization. Field/member walks and predicates can add work; no benchmark, universal speedup or measured memory claim is supplied.

## 15. Known Uses

[Xslt40ItemTypeTests](../../../tests/Bosak.Xslt.Tests/Xslt40ItemTypeTests.cs) and [Xslt40StructuralRecordAliasTests](../../../tests/Bosak.Xslt.Tests/Xslt40StructuralRecordAliasTests.cs); new intake-types enum/alias/structural rows. Missing @as and duplicates are explicit diagnostic controls. No new GatedSets membership; AC-03 remains partial.

## 16. Related Features

[Structural records](../type-system/structural-records.md), [enums](../type-system/enum-types.md), [type/pattern grammar](../TYPE-PATTERN-GRAMMAR.md), [pattern entry](../xslt-instructions/structured-value-patterns.md).
