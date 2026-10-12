# Supplementary coverage audit — 2026-10-12

The catalog assigns all 117 registered 4.0-only names, but REQ-126 remains
**In Progress**. This audit separates registration evidence from documented and
tested contracts. At the preceding audit delivery, coverage was 54 entries and
415 executable documentation checks. The follow-up comparison delivery is
recorded below; neither delivery changes engine behavior.

Reviewed Bosak baseline: `37457bfd` after PR #169. Published engine: **1.1.2**.
The accompanying [registration inventory](audit-registration-arities.json) records
literal dictionary registrations for the 68 non-constructor names. Its arities
are implementation metadata, not proof of full draft signatures or conformance.
The 49 `xs:*` focus constructors remain a separate family requiring verification
of generated registrations, focus conversion, missing focus and invalid values.

## Sources and reproducible evidence

- [Registered name inventory](audit-function-inventory.json) defines the 117-name scope.
- [Function library](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs)
  contains registrations, keyword/default metadata and implementations.
- [Parser](../../src/Bosak.XPath.Parser/Ast/XPathParser.cs) supplies grammar evidence.
- [Function tests](../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs)
  supply named behavior evidence; catalog coverage additionally requires a case in
  [the example manifest](examples/cases.json).
- [Pinned F&O draft source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml)
  is the current function-contract review reference. This does not recover the
  historically adopted draft for earlier deliveries.
- [REQ-118 dossier](../REQ-118-xpath-xslt-40.md) records the original tier and
  disposition decisions. Live [XPath](https://qt4cg.org/specifications/xquery-40/xpath-40.html),
  [XSLT](https://qt4cg.org/specifications/xslt-40/Overview.html) and
  [specification index](https://qt4cg.org/specifications/) links are navigation,
  not immutable evidence of adoption.

## Contract gaps to close

| Priority | Surface | Evidence and remaining work |
|---|---|---|
| 1 | Changed existing functions | `fn:compare` already existed in 3.1 and is absent from the 4.0-only name inventory. Tests `Compare40_CrossNumericExact_DoubleVsDecimal`, `Compare40_NanOrdersBelowNegativeInfinity_AndEqualsItself`, `Compare40_BooleansAndBinariesAndQName`, `Compare40_MixedIncomparable_RaisesXpty0004` and `Compare40_KeywordFormAndEmptyCollation` establish implementation evidence. The [measurement reference](functions/measurement-comparison.md) now supplies 27 executable cases and a distinct 3.1 normalization policy. This is scoped coverage, not every atomic-family contract. Next inspect min/max, sorting, collation and changed map/array overloads individually. |
| 2 | Arity, keyword and default contracts | For every registration, reconcile parameter names, sequence types, omitted arguments and keyword calls against the pinned draft. A registered arity alone cannot establish any of these. Add success, empty and invalid-argument examples for uncovered forms. |
| 2 | Options | `hash#3` accepts an options map, but the implementation currently only validates its shape; named option behavior is not established. Hash examples cover algorithm/default/input cases, not a complete options contract. URI code reads `allow-deprecated-features`, `omit-default-ports` and `unc-path`; reconcile each option and conflicting component precedence against draft rules and published fixtures. Audit sorting records and JSON options separately. |
| 2 | Constructors and types | One focus-constructor family entry does not verify all 49 names. Reconcile constructor-specific lexical/value constraints and error paths, then structural records, enum/choice coercion and function-signature conversion. |
| 3 | Grammar and patterns | Existing syntax entries do not constitute a grammar inventory. Reconcile REQ-118 S3/S4 and S7/S8 against parser/compiler tests: keyword calls, guarded/otherwise expressions, templates, collection iteration, arrows, focus functions, binary literals and type/map/array patterns. Record omitted productions and version-gate behavior explicitly. |
| 3 | Drift and lifecycle | The 28-entry provenance ledger retains delivery evidence but lacks historical adopted draft pins. Keep that limitation. For each changed or superseded form, link an immutable current review and explain the difference; do not substitute a live page for historical adoption. |

`highest`/`lowest` computed-key tie behavior and approximate Unicode grapheme
segmentation remain separate engine/specification follow-ups. Existing observed
fixtures do not prove complete conformance. The 1.1.2 map result-shape correction
is delivered; it is no longer an outstanding implementation defect.

The pinned hash draft requires BLAKE3 and CRC-32; Bosak's implementation explicitly
reports these algorithms as unsupported (`FOHA0001`). Preserve that documented
limitation when reviewing the options contract; a passing SHA-256 example cannot
establish complete hash conformance.

## Wider disposition backlog

The section-1.2 decisions already have dedicated records in the
[completeness audit](COMPLETENESS-AUDIT.md#deferred-and-rejected-decisions).
The following wider dossier decisions still need dedicated records with exact
scope, rationale, delivery/refusal evidence where available, alternatives and
links. These are documentation tasks, not authorization to implement proposals.

| Dossier scope | Recorded decision | Evidence needed before catalog completion |
|---|---|---|
| Computed XPath node constructors | Deferred in tier 2 | Separate XPath refusal/partial support from existing XQuery constructors; pin draft production and test evidence. |
| `xsl:item-type` / `xsl:record-type` | Deferred in tier 4 | Separate XSLT declarations from shipped XPath structural types. |
| Enclosing modes; capturing accumulators; `fn:apply-templates` | Deferred in tier 4 | Individual scope and rationale records, compiler/runtime evidence and supported alternatives. |
| Serialization 4.0 parameters | Deferred in tier 4 | Parameter-by-parameter boundary against existing serialization behavior. |
| `parse-html`, `html-doc`, `invisible-xml`, `unparsed-binary`, `xsd-validator` | Out of scope in the original dossier | Recheck actual registration before asserting current refusal; record dependency and ownership rationale per function. |
| Full GNode/JNode model | Original out-of-scope decision, later partial delivery | Keep the existing partial-model boundary; reconcile historical wording with shipped navigation rather than repeating the original exclusion. |
| XQuery 4.0 and XSLT Streaming 4.0 deltas | Separate/out-of-scope work | Record project boundary and residual streaming prerequisite; do not infer support from XPath features. |

## Next delivery and acceptance

The [typed measurement comparison](functions/measurement-comparison.md) now supplies that first delivery. Its default-mode outcomes and keyword drift are explicit. Next review changed `fn:min`/`fn:max` contracts before expanding option and grammar inventories. Keyword spelling and legacy binary type validation remain separate engine/specification follow-ups.

AC-01 through AC-04 remain partial. AC-05 remains met for the current scope.
Closure requires reviewed contracts and disposition records, executable examples,
exact gate assignments or explicit absence, and feature-specific drift warnings.
Neither the 117-name milestone nor this registration inventory closes REQ-126.


Follow-up delivery (2026-10-12): current coverage is 55 entries and 442 checks (418 feature plus 24 chapter). The opening 54-entry/415-check count records the preceding audit delivery. Registered-name and arity inventories keep their original scopes.
