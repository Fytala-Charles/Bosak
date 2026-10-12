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
| 2 | Options | `hash#3` accepts an options map, but the implementation currently only validates its shape; named option behavior is not established. Hash examples cover algorithm/default/input cases, not a complete options contract. [URI option policy](functions/uri-option-policy.md) now documents `allow-deprecated-features`, `omit-default-ports` and `unc-path`, raw/structured component precedence and 28 fixtures. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps; raw-authority bypass is draft-defined. Audit sorting records and JSON options separately. |
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
| Computed XPath node constructors | [Deferred record supplied](deferred/computed-node-constructors.md) | Current pinned draft includes them; Bosak refuses the seven braced-name forms at 31/40/experimental. Existing XQuery 3.1 named tests and complete XSLT receipt alternative provide separate evidence. Historical adopted pin remains unavailable. |
| `xsl:item-type` / `xsl:record-type` | [Item-type](deferred/xslt-item-type.md) / [record-type](deferred/xslt-record-type.md) records supplied | Deferred named types; 28 fixtures distinguish unused tolerance from binding/function failure and supported anonymous/portable alternatives. Structural-record XSLT @as conversion remains an observed gap with explicit-cast workaround. |
| Enclosing modes | [Deferred record supplied](deferred/enclosing-modes.md) | 20 fixtures record XTSE0260 refusal and supported top-level named-mode alternatives; no enclosing-rule isolation or draft diagnostic matrix claimed. |
| Capturing accumulators; `fn:apply-templates` | Deferred in tier 4 | Dedicated scope/rationale, compiler/runtime evidence and supported alternatives remain to be supplied. |
| Serialization 4.0 parameters | Deferred in tier 4 | Parameter-by-parameter boundary against existing serialization behavior. |
| `parse-html`, `html-doc`, `invisible-xml`, `unparsed-binary`, `xsd-validator` | Out of scope in the original dossier | Recheck actual registration before asserting current refusal; record dependency and ownership rationale per function. |
| Full GNode/JNode model | Original out-of-scope decision, later partial delivery | Keep the existing partial-model boundary; reconcile historical wording with shipped navigation rather than repeating the original exclusion. |
| XQuery 4.0 and XSLT Streaming 4.0 deltas | Separate/out-of-scope work | Record project boundary and residual streaming prerequisite; do not infer support from XPath features. |

## Next delivery and acceptance

The [typed measurement comparison](functions/measurement-comparison.md) now supplies that first delivery. Its default-mode outcomes and keyword drift are explicit. The [batch extrema reference](functions/batch-extrema.md) supplies 29 cases and records numeric promotion/result-type and duration gaps. The [warehouse sorting reference](functions/warehouse-pick-sorting.md) now supplies 28 cases and records numeric/NaN/default-key/collation limits. Next review URI option contracts and component precedence before expanding grammar inventories. Keyword spelling and legacy binary type validation remain separate engine/specification follow-ups.

AC-01 through AC-04 remain partial. AC-05 remains met for the current scope.
Closure requires reviewed contracts and disposition records, executable examples,
exact gate assignments or explicit absence, and feature-specific drift warnings.
Neither the 117-name milestone nor this registration inventory closes REQ-126.


Measurement comparison delivery (2026-10-12): coverage was 55 entries and 442 checks (418 feature plus 24 chapter). The opening 54-entry/415-check count records the preceding audit delivery. Registered-name and arity inventories keep their original scopes.


Batch extrema delivery (2026-10-12): coverage was 56 entries and 471 checks (447 feature plus 24 chapter). QName extrema have a 3.1 fold comparison; mixed numeric and plain-duration behavior remain incomplete against the pinned draft. Exact fn-min/fn-max gate assignment is absent.


Historical warehouse sorting delivery (2026-10-12): coverage was 57 entries and 499 checks (475 feature plus 24 chapter). Portable composite-key sorting and array member boundaries were verified; exact numeric and NaN ordering remain incomplete against the pinned draft. The subsequent URI option review is supplied below.


**URI option review — 2026-10-12:** [Archive and partner URI policy](functions/uri-option-policy.md) adds complete input/output, a scoped 3.1 comparison and 28 executable cases. Boolean options, UNC spelling, raw/structured authority and path/query precedence are explicit. Non-Boolean option truthiness and empty-query-map fallback remain observed draft gaps. Coverage: 62 entries, 581 feature plus 24 chapter checks (605 total); all 117 names remain assigned. REQ-126 stays In Progress; source/public version remains 1.1.2. Next: capturing-accumulator disposition.

**Computed XPath constructor disposition — 2026-10-12:** [Deferred node construction](deferred/computed-node-constructors.md) separates the draft XPath feature from Bosak XQuery 3.1 support. Complete invoice input, refused XPath, 3.1 report and XSLT receipt output accompany 30 executable checks: 25 refusals across compatibility modes, three reports and two XML outputs. Current coverage: 62 entries, 581 feature plus 24 chapter checks (605 total); all 117 names remain assigned. Version stays 1.1.2; REQ-126 remains In Progress. Named XSLT declaration dispositions are now supplied; enclosing modes are now supplied; next: capturing accumulators.

**Named XSLT declaration review — 2026-10-12:** [xsl:item-type](deferred/xslt-item-type.md) and [xsl:record-type](deferred/xslt-record-type.md) add full invoice cases, pinned disposition evidence and 28 executable boundary/alternative checks. Unused 4.0 declarations are ignored, not implemented; named bindings and constructors fail. Portable map validation and inline enums are verified. Structural-record XSLT `@as` raises XTTE0570; explicit XPath cast in `select` plus `as="map(*)"` is the verified workaround. Catalog: 62 entries, 581 feature plus 24 chapter checks (605 total); all 117 names assigned. REQ-126 remains In Progress, version 1.1.2. Next: capturing-accumulator disposition.

**Enclosing-mode review — 2026-10-12:** [Deferred enclosing modes](deferred/enclosing-modes.md) supplies a full invoice export/audit case, pinned draft/source evidence and 20 executable checks. Nested mode templates refuse with XTSE0260; top-level named rules provide supported dispatch, without enclosing-rule isolation. Explicit mode, #current and lexical defaults, empty/reordered input and namespace identity are verified. Catalog: 62 entries, 581 feature plus 24 chapter checks (605 total); all 117 names remain assigned. Version remains 1.1.2; REQ-126 In Progress. Next: capturing-accumulator disposition.
