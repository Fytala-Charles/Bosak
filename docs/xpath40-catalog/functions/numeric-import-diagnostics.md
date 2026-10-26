<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Numeric Import Diagnostics">
  <br><br><h1>Numeric Import Diagnostics</h1>
  <p>Frozen 4.0 — verified integration contracts and limitations</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Blocked empty-annotation contract**. No complete derived-type reflection/function-conversion audit; C03 owns constructor/type/coercion coverage. atomic-type-annotation(()) incorrectly returns a type record in both 4.0 profiles. Required empty-input refusal is explicitly unsupported; bounded correction F08-atomic-annotation-empty retains the independent XPTY0004 expectation. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

Functions: [fn:is-NaN](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-is-NaN), [fn:atomic-type-annotation](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-atomic-type-annotation), [fn:op](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-op). Frozen 4.0.
Type/NaN delivery [PR #112](https://github.com/Fytala-Charles/Bosak/pull/112), [89c90b4d](https://github.com/Fytala-Charles/Bosak/commit/89c90b4d5b6a216a4a37223e169d36e3a1a832bd); operator accessor [PR #102](https://github.com/Fytala-Charles/Bosak/pull/102), [2c71d1ea](https://github.com/Fytala-Charles/Bosak/commit/2c71d1ea544ec2d5ab94c40c7bfabb0bc94e63c5).

Reviewed 2026-10-12 against immutable [QT4CG function source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). This is a current review pin, not recovery of the unavailable historical adopted snapshot.

## 2. Intent

Report accepted and rejected measurement rows, total their numeric values with an addition callback, and inspect the aggregate atomic type for diagnostics.

## 3. Also Known As

NaN detection, type reflection and operator callbacks are descriptive terms; no aliases are registered.

## 4. History / Supersedes

XPath 3.1 detects a numeric NaN because it is not equal to itself, and uses function($x,$y){$x+$y} as an addition callback. The comparison tests instance of xs:double rather than reproducing the full type-annotation record, for which standard 3.1 has no direct equivalent.

## 5. Motivation

A measurement import can contain malformed numeric text. Silently adding NaN would spoil the total, while losing rejected identifiers would make correction difficult. This report keeps row classification and aggregate type visible.

## 6. Language Alignment

is-NaN atomizes a single supplied value and tests whether it is numeric NaN. The string 'NaN' itself is not numeric NaN; number('invalid') is. Empty/multiple values are type errors and maps/functions cannot be atomized in the demonstrated cases.

atomic-type-annotation exposes a type record with fields/callables such as name, base-type, matches and constructor. It describes an atomic value, not the input node's annotation or a complete schema validation. The precise reported annotation is processor-dependent; use instance of for application type requirements.

op returns a two-argument callable for a supported operator spelling. It preserves that operator's conversion and error behavior. An arithmetic exception is not converted into a diagnostic result automatically.

## 7. Applicability

This is measurement data, not money: number() produces floating-point values, and NaN screening alone does not reject infinity or values outside a business range. Monetary totals require a decimal conversion/range policy instead. Present row IDs and supplied values are assumed. The host owns acquisition and remediation.

## 8. Structure

Convert value text with number, split rows using NaN detection, fold accepted values with op(+), and inspect the resulting atomic type name.

## 9. Participants

Readings provide text and IDs. The conversion creates doubles/NaN. Predicates classify rows. The operator callback adds values. Reflection reports the aggregate annotation.

## 10. Collaborations

Check the converted numeric value, not the source string spelling. Start the fold with 0e0 so empty/all-invalid input has the declared double zero result. Keep rejected rows visible instead of treating them as numeric zero.

## 11. Consequences

Invalid numeric text is reported rather than poisoning this total. Type reflection does not establish business validity. Floating-point precision and the untested infinity/range cases remain outside this sample contract.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Numeric NaN versus lexical NaN and single-value atomization; Atomic annotation record and callable operator reflection; Operator errors propagate; annotation is not business or schema validation. **Boundaries:** No complete derived-type reflection/function-conversion audit; C03 owns constructor/type/coercion coverage. atomic-type-annotation(()) incorrectly returns a type record in both 4.0 profiles. Required empty-input refusal is explicitly unsupported; bounded correction F08-atomic-annotation-empty retains the independent XPTY0004 expectation. Follow-up groups: F01, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile new functions with CompileOptions.Compatibility40; default 3.1 rejects them. Earlier-version comparisons use Compatibility31. The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) verifies exact text/error markers and default-profile refusal for successful 4.0 cases. All values are inline and deterministic.

**Supplementary contract review (2026-10-12):** [URI/import utility review](../URI-UTILITY-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed import/link README](../examples/utility-contracts/README.md) supply 105 checks across five families: keyword modes, component/options/escaping, one-pass UTF-8 decoding, collation availability, identity, NaN and type/cardinality boundaries, plus complete XPath 3.1/4.0 reports. Keywords remain unavailable. Boolean-option/record tolerance and empty query-map precedence remain follow-ups. All 37 absent-keyword families now have first targeted review, with metadata still absent; this does not close D2. Source/public version is 1.1.3; original delivery/unit/gate and historical adopted-pin limits remain.

## 13. Sample Code

Complete synthetic input:

```xml
<readings><reading id="A" value="12.5"/><reading id="B" value="invalid"/><reading id="C" value="7.5"/></readings>
```

### XPath 4.0 expression

[Runnable file](../examples/numeric-import-diagnostics/main.xpath):

```xpath
let $rows := parse-xml('<readings><reading id="A" value="12.5"/><reading id="B" value="invalid"/><reading id="C" value="7.5"/></readings>')/readings/reading
let $valid := $rows[not(is-NaN(number(@value)))]
let $invalid := $rows[is-NaN(number(@value))]
let $total := fold-left($valid/@value ! number(.), 0e0, op('+'))
return concat('accepted=', string-join($valid/@id, ','), '; rejected=', string-join($invalid/@id, ','), '; total=', $total, '; type=', local-name-from-QName(atomic-type-annotation($total)?name))
```

Exact output:

```text
accepted=A,C; rejected=B; total=20; type=double
```

### XPath 3.1 comparison

[Runnable file](../examples/numeric-import-diagnostics/baseline.xpath):

```xpath
let $rows := parse-xml('<readings><reading id="A" value="12.5"/><reading id="B" value="invalid"/><reading id="C" value="7.5"/></readings>')/readings/reading
return let $valid := $rows[number(@value) eq number(@value)]
return let $invalid := $rows[not(number(@value) eq number(@value))]
return let $total := fold-left($valid/@value ! number(.), 0e0, function($x, $y) { $x + $y })
return concat('accepted=', string-join($valid/@id, ','), '; rejected=', string-join($invalid/@id, ','), '; total=', $total, '; double=', $total instance of xs:double)
```

Exact output:

```text
accepted=A,C; rejected=B; total=20; double=true
```

### Complete fixture inventory

Each file embeds its full input and exact result or diagnostic marker.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/numeric-import-diagnostics/main.xpath) | `accepted=A,C; rejected=B; total=20; type=double` |
| [baseline](../examples/numeric-import-diagnostics/baseline.xpath) | `accepted=A,C; rejected=B; total=20; double=true` |
| [all-invalid](../examples/numeric-import-diagnostics/all-invalid.xpath) | `accepted=; rejected=A,B,C; total=0; type=double` |
| [empty](../examples/numeric-import-diagnostics/empty.xpath) | `accepted=; rejected=; total=0; type=double` |
| [nan-number](../examples/numeric-import-diagnostics/nan-number.xpath) | `true` |
| [nan-string](../examples/numeric-import-diagnostics/nan-string.xpath) | `false` |
| [ordinary-number](../examples/numeric-import-diagnostics/ordinary-number.xpath) | `false` |
| [type-not-validator](../examples/numeric-import-diagnostics/type-not-validator.xpath) | `false` |
| [derived-type](../examples/numeric-import-diagnostics/derived-type.xpath) | `duration` |
| [operator-pairs](../examples/numeric-import-diagnostics/operator-pairs.xpath) | `11,22` |
| [operator-arity](../examples/numeric-import-diagnostics/operator-arity.xpath) | `2` |
| [empty-nan](../examples/numeric-import-diagnostics/empty-nan.xpath) | `XPTY0004` |
| [map-nan](../examples/numeric-import-diagnostics/map-nan.xpath) | `FOTY0013` |
| [division-zero](../examples/numeric-import-diagnostics/division-zero.xpath) | `FOAR0001` |
| [unknown-operator](../examples/numeric-import-diagnostics/unknown-operator.xpath) | `XPTY0004` |

The earlier report verifies a double type test, not full reflection. All-invalid and empty cases return zero with rejected/accepted identifiers still explicit. Operator examples verify callable arity and propagated division errors.

## 14. Performance

Conversion and classification scan the small input, and fold-left accumulates its accepted values. Type reflection returns a record with function members. No benchmark or exact arbitrary-precision claim is implied.

## 15. Known Uses

Synthetic workflow. Named evidence: [`IsNan_DetectsDoubleAndFloatNaN`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`IsNan_EmptyMultiMapFunction_Raise`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`AtomicTypeAnnotation_PrimitiveBoolean_RecordShape`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`AtomicTypeAnnotation_DerivedDayTimeDuration_HierarchyAndMatches`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), and [`Op_ForEachPair_SpecExample`](../../../tests/Bosak.XPath.Standard.Tests/FnOpTests.cs).
The [runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) assigns exact fn-op, not exact is-NaN/atomic-type-annotation sets. These 15 fixtures provide separate evidence; membership is unchanged.

**C13 applicable-evidence placement:** The current XPath gate assignment is `fn-op`; is-NaN and atomic-type-annotation are explicitly unassigned and use the named engine tests and examples. The empty atomic-type-annotation argument remains a known XPTY0004 mismatch (C02-F08). [39 exact case receipts](../SEMANTIC-REVIEW-BATCH-2.json) retain input, mode/host, expected output/error and alternatives. Named evidence: [Op_ForEachPair_SpecExample](../../../tests/Bosak.XPath.Standard.Tests/FnOpTests.cs); [IsNan_DetectsDoubleAndFloatNaN](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs). The public [CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) executes the linked published fixtures.

## 16. Related Features

[Supplier offer ranking](supplier-offer-ranking.md) uses validated decimal prices. [Payload fingerprints](payload-fingerprints.md) covers another import diagnostic. [The audit](../COMPLETENESS-AUDIT.md) tracks reference coverage separately from exhaustive types/operators.
