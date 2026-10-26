<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: Focus-based constructors">
  <br><br><h1>Focus-based constructors</h1>
  <p>Practical reference: Frozen 4.0</p>
</div>

## 1. Name and Classification

**Focus-based constructors** - **Frozen 4.0**. Delivery family: [REQ-118](../../REQ-118-xpath-xslt-40.md) / [REQ-123 registry](../../FEATURE_REQUESTS.md). Section provenance: See the linked REQ dossiers and implementation evidence. [Primary implementation evidence](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) records the shipped behavior; this entry adds executable user-facing examples under REQ-126, rather than announcing a new engine feature.

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: constructor functions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-constructor-functions), [F&O 4.0: XML Schema constructors](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#constructor-functions-for-xsd-types). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #110](https://github.com/Fytala-Charles/Bosak/pull/110), [immutable mainline revision `2871c0d6`](https://github.com/Fytala-Charles/Bosak/commit/2871c0d6ecb76c5099a4f682a71c5da49fb10ecf) (2026-10-10). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

This page covers zero-argument constructors. For `fn { ... }`, `function { ... }`, the `fn(...)` abbreviation and pipeline focus, use the separate [focus-function syntax entry](focus-function-syntax.md). Constructor coverage does not establish that grammar.

## 2. Intent

Use the current item as the argument to a zero-arity constructor inside a simple map.

## 3. Also Known As

Use the name and spelling in this entry and its executable files. Related functions or instruction forms are described in the primary test evidence; no additional alias is adopted here.

## 4. History / Supersedes

In 3.1, call xs:integer(.) explicitly. Compare the [complete earlier-version baseline](../examples/focus-functions/before.xpath) with the [complete expression](../examples/focus-functions/main.xpath). Both yield `12,5` for this introductory input. The [delivery import](../examples/focus-functions/README.md) now supplies a complete business case and all 49 constructor names. They demonstrate this task, not universal equivalence of the two mechanisms.

## 5. Motivation

A supplier delivery extract supplies quantities, prices, promised dates and active flags as XML attributes. The receiving system requires typed values; the import report exposes canonical values before accepting the delivery. Use the current item as the argument to a zero-arity constructor inside a simple map. Keeping this task explicit makes it possible to choose the feature for a practical reason and verify what the receiving application will see.

## 6. Language Alignment

Read the behavior as XPath/XSLT semantics. Similar vocabulary in another programming language is not a compatibility promise. There must be a suitable focus, and invalid lexical data still fails; this is conversion, not validation.

## 7. Applicability

Use this form when solving the stated task and the consumer permits the declared compatibility level. Keep the earlier baseline when the application requires 3.1 expressions or portable 3.0 stylesheets. There must be a suitable focus, and invalid lexical data still fails; this is conversion, not validation.

## 8. Structure

```xquery
string-join(('12', '5') ! xs:integer() ! string(.), ',')
```

This is the worked case's syntax, not a replacement for the full language grammar. Required namespaces and surrounding context are supplied in the linked complete files.

## 9. Participants

XPath31Expression, CompileOptions, the parser/compiler, and the shared runtime/function library provide the public execution path. [Primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs) identify the feature's implementation contract. The examples use public APIs only.

## 10. Collaborations

The case combines source values, the named feature, and explicit output formatting. [Worked chapters](../worked-examples/README.md) show how selection, construction, and business rejection fit into larger transformations.

## 11. Consequences

Benefit: Use the current item as the argument to a zero-arity constructor inside a simple map. Tradeoff: the declared compatibility level and edge rules become part of the consumer contract. There must be a suitable focus, and invalid lexical data still fails; this is conversion, not validation.

## 12. Implementation

**C04 grammar review — 26 October 2026, published 1.26.3:** [Expression/call/focus reconciliation](../EXPRESSION-CALL-FOCUS-RECONCILIATION.md) covers S4-focus. Pipeline whole-value/fixed-focus, focus-function implicit/explicit signatures, lexical capture/restoration and accepted mapping syntax are mapped. Mapping arrow extra arguments incorrectly see mapped-item focus rather than the containing focus. Use for $u in ... return F($u,A...) when extra arguments depend on outer dot/position/size. C03 constructor capture/type coverage remains distinct. Host/profile propagation belongs to C06. This is bounded production/profile evidence, not full grammar or AC-04 acceptance.


**C03 review — 26 October 2026, published 1.26.3:** [Constructor/type reconciliation](../CONSTRUCTOR-TYPE-RECONCILIATION.md) verifies All 49 registered zero-arity names with representative lexical conversion, type/error and context checks; Direct/reference/lookup/node paths, explicit one-argument comparison, empty iteration and missing focus; QName static namespace and captured-focus boundaries. **Boundary:** A zero-arity constructor requires suitable focus; empty simple-map input invokes nothing. xs:error has no nonempty successful value, and its empty/type fixture is not proof of a constructed value. Representative inputs do not certify all facets, namespaces, schema types or arbitrary casts. Ownership: T03, T05. Per-path controls and named schema/API/host evidence are retained in the [review matrix](../CONSTRUCTOR-TYPE-RECONCILIATION.json); no whole-type or AC-04 acceptance follows.

Compile the after/edge expressions at XPathCompatibility.XPath40. The baseline uses XPath31. The example harness checks default-mode refusal for successful 4.0 cases; negative cases pin the declared error. See [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs): it executes the published files from the [case manifest](../examples/cases.json). This documentation slice changes no engine source.

## 13. Sample Code

The introductory input is declared directly in the complete expression. The [complete expression](../examples/focus-functions/main.xpath) yields `12,5`. The [edge case](../examples/focus-functions/edge.xpath) yields `FORG0001 (expected error)`. No external resources or application variables are required. See the [example runner](../examples/README.md) for commands and host context.


### Supplier delivery import and constructor audit

The [detailed example README](../examples/focus-functions/README.md) explains full input/output, field responsibilities, failures and compatibility. It links every published file and the [49-name audit inventory](../audit-focus-constructors.json). The new audit adds **508 executable checks** (including representative types, context/error/cardinality cases, 3.1 alternatives and experimental outcomes); the original three checks remain.

Input:

```xml
<deliveries><line id="A" quantity="12" price="2.50" date="2026-10-12" active="true"/><line id="B" quantity="5" price="4.00" date="2026-10-13" active="false"/></deliveries>
```

Complete [4.0 report](../examples/focus-functions/audit/import-40.xpath):

```xquery
string-join(for $line in parse-xml('<deliveries><line id="A" quantity="12" price="2.50" date="2026-10-12" active="true"/><line id="B" quantity="5" price="4.00" date="2026-10-13" active="false"/></deliveries>')/deliveries/line return concat($line/@id, '|', $line/@quantity ! xs:integer(), '|', $line/@price ! xs:decimal(), '|', $line/@date ! xs:date(), '|', $line/@active ! xs:boolean()), ';')
```

Output (id, quantity, price, promised date, active flag):

```text
A|12|2.5|2026-10-12|true;B|5|4|2026-10-13|false
```

The [3.1 report](../examples/focus-functions/audit/import-31.xpath) uses explicit `(.)` constructor arguments and produces identical output. Both empty-input reports return an empty string; reordered input retains document order. Quantity `twelve` refuses FORG0001 in both versions. These reports normalize values; they do not apply required-field, schema or business-total validation.

Empty simple-map input means no call occurs. It differs from a missing context item (XPDY0002 for every zero-arity constructor), explicit empty argument (empty result for every one-argument constructor), and a multi-item argument (tested integer refusal XPTY0004). Mapping two lexical inputs invokes the constructor once per item: scalar types produce two values; ENTITIES/IDREFS/NMTOKENS produce four for two two-token inputs.

All 48 inhabited types pass representative value/type checks. `xs:error` is registered but has no non-empty value space; its non-empty calls refuse FORG0001 and its explicit empty argument returns empty. This is not a missing constructor implementation. String-like types have no generic invalid lexical string; their invalid-input checks use a map and report FOTY0013. Additional numeric bounds, timezone, QName binding, array atomization and wrong-source-type cases are documented in the README. Testing one lexical sample per type does not establish exhaustive XML Schema casting conformance.

**Pinned contract review:** [F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/xpath-functions.xml), sections `constructor-functions-for-xsd-types`, `constructor-functions-for-xsd-list-types`, `constructor-functions-for-xsd-union-types`, plus QName rules. Default argument is the context item; list/union contracts remain distinct. This current comparison does not recover the historically adopted pin. Source: [generated registrations and XsFocusCtor](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs); coercion: [VM engine](../../../src/Bosak.XPath.Runtime/Vm/VmEngine.cs).

## 14. Performance Notes

Expected class: per-item conversion. Invalid values still require error classification rather than a successful result. These are implementation expectations, not measured deltas. No benchmark is claimed; use the repository benchmark harness before making workload-specific speed or memory claims.

## 15. Known Uses

Executable example IDs: `focus-functions-main`, `focus-functions-edge`, `focus-functions-before` in `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`. Existing unit evidence: [primary tests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). New IDs use `focus-audit-<name>-<scenario>`, with valid, invalid, missing, empty, multiple, before, type, experimental and gate31 variants; argument-empty cases cover all names. Business/boundary IDs are enumerated in the manifest. Chapter-level evidence does not promote a qt4tests set into GatedSets; conformance status remains owned by the harness and delivery records.

**Named implementation evidence:** [`Compile_FocusConstructors_DefaultOptions_ThrowXpst0017`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_FocusConstructors_FrozenXPath40_Works`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Compile_ArityOneConstructors_DefaultOptions_StillWork`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Existing promoted sets include `misc-FocusConstructors`, `xs-normalizedString`, `xs-numeric`, `xs-token`, verified in [GatedSets](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs). These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

See the [catalog index](../README.md), [worked chapters](../worked-examples/README.md), and the alternatives in section 4. The case may be used independently of the larger chapters.

*Expanded 2026-10-12: all-name focus constructor and typed delivery audit; no engine change.*

*Created 2026-10-11: verified practical catalog example and earlier-version baseline.*
