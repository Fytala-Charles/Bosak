<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala xsl:record-type: construction and identity">
  <h1>xsl:record-type: construction and identity</h1>
  <p>21 October 2026</p>
</div>

**C09 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-1.md) records 3 contract groups with exact sample/context, applicable evidence and language-alignment limits. Status: **Limited implemented nominal declaration**. Broader constructors/defaults/static/package/type conversion remain C03/C05/C06 scope limits. Review completion is not final owner acceptance; earlier checkpoints remain historical.

**REQ-126 C03 complete — 26 October 2026:** [Constructor/type review](../CONSTRUCTOR-TYPE-RECONCILIATION.md) reconciles eight entries and all 49 focus constructors. All 1,333 asserted controls match across 1,357 observations; 245 focused schema/API/host tests pass. Bounded nominal defaults/static fields are now evidenced. Generic wide casts, decimal precision, schema/host context and broader conversion remain explicit boundaries; C02's atomic-annotation defect remains open. Ten closure workpackages remain; C04 is next. No engine/test/example/version/gate change or acceptance waiver; skips/Braid/extension work stays deferred. Earlier checkpoint counts are historical.

## 1. Name and Classification

**Implemented bounded XSLT4.0 named-record behavior in public1.23.2; broader contracts remain separately scoped.** The historical deferred URL is retained. [Current review](../TYPE-PATTERN-GRAMMAR.md) separates constructor identity, structural annotation and noncoercing type/pattern tests.

## 2. Intent

Establish a stylesheet-declared record identity and, when requested, a generated constructor.

## 3. Also Known As

Named or nominative record type. This is distinct from an anonymous structural record alias and from ordinary map key equality.

## 4. History / Supersedes

The old deferral/source1.2.0 snapshot and accumulated implementation notes are [preserved separately](../../history/REQ126-RECORD-TYPE-SNAPSHOT-20261021.md). Current source evidence includes named record pattern delivery2244c1d7. No complete draft adoption or feature-tier promotion is inferred from this bounded update.

## 5. Motivation

Two intake interfaces can use the same field names while needing different record identities. Constructors and type checks must distinguish them.

## 6. Language Alignment

Matching fields do not make t:point and t:other nominally identical. A plain map is not automatically a t:point in an instance-of or record pattern test. Conversion in a binding/function call is a separate contract, not a contradiction of that result.

## 7. Applicability

Use the declared constructor and tested compiled stylesheet context when named identity is required. Use portable maps and explicit predicates when the receiving processor lacks these declarations.

## 8. Structure

`<xsl:record-type name="t:point" constructor="yes"><xsl:field name="x" as="xs:integer"/></xsl:record-type>`; the complete namespaces are in [nominal-own.xsl](../examples/intake-types/nominal-own.xsl). constructor="no" does not generate the called function.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) registers the declarations/constructors; [RecordPattern](../../../src/Bosak.Xslt/Patterns/RecordPattern.cs) tests existing values. Keep declaration/type/pattern positions distinct.

## 10. Collaborations

Named records can be tested by named type/record patterns. [xsl:item-type](xslt-item-type.md) may name a structural shape without creating this nominal identity.

## 11. Consequences

Preserves an identity boundary beyond fields but adds declaration/context coupling. This review does not certify every recursive/default/static/visibility/import/package/error edge, nor a standalone XPath named-declaration language.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S8-patterns. Fresh version-4 stylesheets in both hosts cover array/member sequences, map missing-versus-empty/surplus keys, aliases, annotated records and nominal identity. Predicates can fail to match without propagating their dynamic errors in the tested forms. No claim of complete declaration/import/package visibility, every composite pattern, generic JNode dispatch or earlier-profile promotion. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


New manifest checks use default Legacy host and effective version4.0, hence frozen XPath40. Legacy3.0 declaration/pattern refusals are explicit. Host selection, module/package identity, imports and authoring contexts need their own applicable tests; no exhaustive cross-context claim follows. No engine behavior changes.

## 13. Sample Code

[nominal-own](../examples/intake-types/nominal-own.xsl) returns true, [nominal-other](../examples/intake-types/nominal-other.xsl) and [nominal-plain-map](../examples/intake-types/nominal-plain-map.xsl) return false. [nominal-no-constructor](../examples/intake-types/nominal-no-constructor.xsl) intentionally raises XPST0017. No external variables/resources are required.

## 14. Performance Notes

Cost follows the value/type/pattern being checked and any conversion/materialization. Field/member walks and predicates can add work; no benchmark, universal speedup or measured memory claim is supplied.

## 15. Known Uses

[Xslt40RecordPatternTests](../../../tests/Bosak.Xslt.Tests/Xslt40RecordPatternTests.cs) and [Xslt40TypePatternTests](../../../tests/Bosak.Xslt.Tests/Xslt40TypePatternTests.cs); new intake-types nominal/type-nominal rows. Applicable XSLT evidence is separate from XPath qt4tests gate membership; AC-03 is not waived.

## 16. Related Features

[Nominative scope](../type-system/nominative-records.md), [structural records](../type-system/structural-records.md), [pattern grammar](../TYPE-PATTERN-GRAMMAR.md).
