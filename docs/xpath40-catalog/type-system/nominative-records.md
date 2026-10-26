<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Nominative records: host-defined identity">
  <h1>Nominative records: host-defined identity</h1>
  <p>21 October 2026</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified bounded XSLT nominal identity**. Package visibility, transitive imports, complete static/default field combinations and arbitrary host registration remain bounded by C03/C05/C06. Final owner acceptance remains C13; earlier checkpoints are historical.

## 1. Name and Classification

**Bounded XSLT-defined identity is implemented in public1.23.2; this is not a standalone XPath declaration facility or a complete promotion of the historical deferred proposal.** [Current mapping](../TYPE-PATTERN-GRAMMAR.md).

## 2. Intent

Explain why a host-declared name can matter beyond the field shape of a record.

## 3. Also Known As

Named/nominal/nominative record identity in the tested XSLT declaration context.

## 4. History / Supersedes

The original PR77 disposition at27bc663b and its earlier blanket deferral prose are retained in the [historical snapshot](../../history/REQ126-NOMINATIVE-RECORDS-SNAPSHOT-20261021.md). Later XSLT source supplies bounded named identity. This does not recover historical adopted specification pins or claim a new standalone XPath feature.

## 5. Motivation

Distinct application interfaces may share a field named x while requiring different declared identities.

## 6. Language Alignment

Unlike a structural alias, nominal identity is not established by matching fields alone. A C# or TypeScript name does not register a type in Bosak; the compiled XSLT declaration provides the tested context.

## 7. Applicability

Use the tested XSLT declaration/constructor path. A bare XPath expression without the host-defined type context cannot assume these names are registered.

## 8. Structure

Declare t:point and t:other with identical x fields; compare constructor results using instance-of or `record(t:point)` pattern. See complete fixtures rather than treating the name as a globally available XPath keyword.

## 9. Participants

[xsl:record-type](../deferred/xslt-record-type.md) supplies the context and constructor; [RecordPattern](../../../src/Bosak.Xslt/Patterns/RecordPattern.cs) tests the value without coercing it.

## 10. Collaborations

Structural record aliases can describe field shapes separately; map patterns can inspect keys/entry sequences. None is a synonym for nominal identity.

## 11. Consequences

Equal fields need not satisfy a different named type. Binding/function conversion rules require separate evidence; an identity check does not establish every coercion/recursive/package rule.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S8-patterns. Fresh version-4 stylesheets in both hosts cover array/member sequences, map missing-versus-empty/surplus keys, aliases, annotated records and nominal identity. Predicates can fail to match without propagating their dynamic errors in the tested forms. No claim of complete declaration/import/package visibility, every composite pattern, generic JNode dispatch or earlier-profile promotion. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies Host-defined nominal identity differs from plain map shape; Constructor and typed-map coercion paths; Tested select/body/trailing defaults and static constructor/coercion values in Legacy and Xslt40Draft version-4 stylesheets. **Boundary:** Bounded defaults/static fields are verified in both named hosts; no blanket claim of missing support remains for those forms. Import/package identity, visibility, recursion, global-initializer timing and all static override/cast/update combinations still require separate evidence. Ownership: T04. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

New manifest checks use default Legacy host and effective version4.0, hence frozen XPath40. Legacy3.0 declaration/pattern refusals are explicit. Host selection, module/package identity, imports and authoring contexts need their own applicable tests; no exhaustive cross-context claim follows. No engine behavior changes.

## 13. Sample Code

[nominal-own](../examples/intake-types/nominal-own.xsl), [nominal-other](../examples/intake-types/nominal-other.xsl), [type-nominal-own](../examples/intake-types/type-nominal-own.xsl) and [type-nominal-other](../examples/intake-types/type-nominal-other.xsl) give positive and negative examples. The older standalone [structural alternative](../examples/nominative-records/alternative.xpath) remains unchanged.

## 14. Performance Notes

Cost follows the value/type/pattern being checked and any conversion/materialization. Field/member walks and predicates can add work; no benchmark, universal speedup or measured memory claim is supplied.

## 15. Known Uses

New nominal and record-pattern rows plus [Xslt40RecordPatternTests](../../../tests/Bosak.Xslt.Tests/Xslt40RecordPatternTests.cs). No new XPath GatedSets member or upstream acceptance is inferred from host-specific XSLT examples.

## 16. Related Features

[Structural records](structural-records.md), [item aliases](../deferred/xslt-item-type.md), [grammar inventory](../TYPE-PATTERN-GRAMMAR.md).
