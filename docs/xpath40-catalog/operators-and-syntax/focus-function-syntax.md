<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala focus-function syntax and pipeline focus catalog entry">
  <h1>Focus-function syntax and pipeline focus</h1>
  <p>Frozen XPath 4.0 - bounded executable reference</p>
</div>

**C11 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-3.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified bounded focus-function syntax**. No arbitrary callback/constructor/package context acceptance; C04 mapping-extra-focus and C06 host/static-mode limits remain distinct. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Focus-function syntax** - **Frozen 4.0**, REQ-118 S4. `fn { E }` and `function { E }` omit the signature and take one `item()*` argument, bound as the whole context value. `fn($x) { E }` abbreviates an ordinary explicit-signature inline function; it is not a focus function. This entry is distinct from the [zero-argument focus constructors](focus-functions.md).

Delivery: [PR #87](https://github.com/Fytala-Charles/Bosak/pull/87), immutable `ec79355936a29ac70eeea3ae47e4c4e2ee00d813`; that revision already contains the named focus-function tests. Current review: [immutable draft expressions](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xquery-40/src/expressions.xml), sections `id-inline-func`, `id-focus-functions`, `id-pipeline-operator`, and [InlineFunctionExpr grammar](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/grammar-40/xpath-grammar.xml). Upstream master was refreshed on 22 October 2026. This is a current comparison pin, not recovery of a historically adopted specification revision. Source 1.23.5 remains unpublished; public 1.23.4 already contains this bounded syntax.

## 2. Intent

Write a callback whose argument supplies its context value, and distinguish per-item processing from whole-sequence reduction.

## 3. Also Known As

A focus function is an inline function without an explicit signature. Both `fn { ... }` and `function { ... }` are supplied forms. The keyword abbreviation `fn($x) { ... }` still has an explicit parameter list. A zero-argument constructor such as `xs:integer()` is a different feature.

## 4. History / Supersedes

In 3.1, write `function($line as element()) { xs:decimal($line/@qty) * xs:decimal($line/@unit) }`. The paired [3.0 stylesheet](../examples/focus-function-syntax/report-3.xsl) and [4.0 stylesheet](../examples/focus-function-syntax/report-4.xsl) produce the same order-line report. A parameterized 3.1 function does not implicitly bind its parameter as the focus; references use `$line` explicitly.

## 5. Motivation

An order feed contains line quantity and unit amount. The report computes one amount per line, preserves source line order, then reports the count and total over all amounts. A focus callback keeps the attribute calculation readable; the pipeline makes the whole-sequence summary distinct from the mapping stage. [The complete task and fixtures](../examples/focus-function-syntax/README.md) state assumptions, exact input/output and failure behavior.

## 6. Language Alignment

This is XPath focus, not ambient JavaScript `this` or an implicit item-by-item lambda. The callback receives a sequence as a single argument and sets position/size to 1/1, even for an empty or multiple-item argument. `=!>` calls its target per input item; `->` evaluates its right expression once with the complete left value as context. The 3.1 `=>` arrow inserts a first function argument; it does not establish the pipeline context. Similar results for a unary example do not make these operators interchangeable.

## 7. Applicability

Use a focus function for an attribute-oriented callback or a whole-value operation when its context contract is clear. Use explicit parameters when argument names/types improve clarity or caller position must be passed explicitly. In the report each amount callback receives exactly one line node. Empty input makes no mapping calls; the summary pipeline still runs once with an empty value.

## 8. Structure

```xpath
fn { concat(count(.), ':', position(), ':', last()) }((9,8))
```

This returns `2:1:1`. The runnable [whole-focus probe](../examples/focus-function-syntax/probes/whole-focus.xpath) returns `2:1:1` (count:position:size), and [empty-focus probe](../examples/focus-function-syntax/probes/empty-focus.xpath) returns `0:1:1`.

## 9. Participants

[XPathParser.ParseInlineFunction](../../../src/Bosak.XPath.Parser/Ast/XPathParser.cs) lowers an omitted signature into a single synthetic parameter plus a PipelineExprNode. [IrLowerer](../../../src/Bosak.XPath.Compiler/Ir/IrLowerer.cs) and [VmEngine](../../../src/Bosak.XPath.Runtime/Vm/VmEngine.cs) execute the existing function/focus path. Public callers use XPath31Expression/CompileOptions or ordinary XSLT stylesheets; no new API is introduced.

## 10. Collaborations

The report's `$amount` focus function reads the supplied line; `$format` uses the explicit `fn(...)` abbreviation and typed named arguments; the per-item label callback captures those variables. Mapping creates line labels and amounts. The pipeline consumes the amounts as one value for count and sum. Caller focus is restored after inner calls/pipelines, as verified by named probes.

## 11. Consequences

A callback argument containing two items is not two calls. A callback invoked from a simple map sees its own position/size 1/1, not the caller's 1/N, 2/N positions. An axis such as `@qty` requires an appropriate node context; the implicit signature alone does not validate the report's line schema. Wrong lexical decimals raise FORG0001 in both report versions. Values are formatted to two display decimals, without claiming a general accounting model.

## 12. Implementation

**C04 grammar review — 26 October 2026, published 1.26.3:** [Expression/call/focus reconciliation](../EXPRESSION-CALL-FOCUS-RECONCILIATION.md) covers S4-focus. Pipeline whole-value/fixed-focus, focus-function implicit/explicit signatures, lexical capture/restoration and accepted mapping syntax are mapped. Mapping arrow extra arguments incorrectly see mapped-item focus rather than the containing focus. Use for $u in ... return F($u,A...) when extra arguments depend on outer dot/position/size. C03 constructor capture/type coverage remains distinct. Host/profile propagation belongs to C06. This is bounded production/profile evidence, not full grammar or AC-04 acceptance.


Thirty new manifest cases use the existing CatalogExampleTests public execution path: 18 focused values/grammar/focus probes, four explicit 3.1 refusals and eight complete stylesheet reports. Sixteen successful XPath cases also assert a 3.1 refusal; two compatible forms retain baseline results. No engine, test runner, compatibility level or gate is changed. The reports use the default Legacy host with effective stylesheet versions 3.0/4.0, not an exhaustive host/module/package matrix.

## 13. Sample Code

Input: `<orders xmlns="urn:fytala:focus"><line sku="BOLT" qty="2" unit="12.50"/><line sku="NUT" qty="4" unit="2.50"/><line sku="FREE" qty="0" unit="99.00"/></orders>`.

Both complete stylesheets yield `BOLT=25.00|NUT=10.00|FREE=0.00|lines=3;total=35.00`. Empty input yields `lines=0;total=0.00`; reordered input changes label order while retaining count/total. An invalid quantity `oops` raises FORG0001. [Full examples](../examples/focus-function-syntax/README.md) include every fixture, expected result, context and runnable file.

## 14. Performance Notes

The report makes a bounded number of passes over N lines/amounts and emits O(N) labels. Function items and sequence materialization may allocate; no speed or memory advantage over the baseline is claimed. Use the benchmark harness for measured claims. Focus functions do not make the report streaming or parallel.

## 15. Known Uses

Applicable evidence under the owner-approved AC-03 policy: `focus-syntax-report-3-normal`, `focus-syntax-report-4-normal`, their empty/reordered/invalid counterparts, and the `focus-syntax-*` probes in [the manifest](../examples/cases.json), executed by [CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs). All 30 case IDs and receipt hashes are recorded in [the slice audit](../FOCUS-FUNCTION-SYNTAX.json).

Named existing tests in [VersionGateTests](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs): `Compile_FocusFunction_31Mode_ThrowsXpst0003`, `Evaluate_FocusFunction_SpecExamples`, `Evaluate_FocusFunction_BindsArgumentAsWholeContextValue`, `Evaluate_FocusFunction_AfterMappingArrow`, `Evaluate_FocusFunction_WrongArityThrows`, `Evaluate_Pipeline_SetsFixedFocusPositionOneSizeOne`, `Evaluate_Pipeline_FocusRestoredAfterEvaluation`. **No feature-specific qt4 gated-set assignment is made here**; named engine tests and runnable public examples are the explicit evidence. This does not assert that upstream lacks broader tests or claim whole-set conformance. The policy amendment does not automatically close acceptance of this or other entries.

## 16. Related Features

[Pipeline](pipeline-operator.md), [mapping arrow](mapping-arrow.md), [zero-argument focus constructors](focus-functions.md), [keyword arguments](keyword-arguments.md), [coverage reconciliation](../COVERAGE-RECONCILIATION.md). Record casts/coercion/updates remain the next separate D3 scope.
