<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Switch, array and select grammar review">
  <h1>Switch, array and select grammar review</h1>
  <p>21 October 2026</p>
</div>

This bounded REQ-126 D3 review maps implemented stylesheet grammar, types and version context to catalog entries and executable evidence. **Public/source version is 1.23.2**, with [signed delivery proof](../RELEASE-1.23.2-PUBLICATION.md). No engine, API, harness, profile default or package version changes are included.

## Business case and boundaries

A dispatch feed contains batches, each with a priority and zero or more SKU lines. The report sorts batches by stable ID, chooses a route, retains an empty batch as one empty array member, and emits an explicit review marker. The [complete example](examples/dispatch-grammar/README.md) pairs portable XSLT 3.0 with the implemented 4.0 forms. These are synthetic records, not an industry-standard logistics interchange. SKU line order within a batch is significant; batch reorder is normalized by ID. The example assumes unique IDs and does not validate a schema or implement fuzzy SKU matching.

Six full reports cover normal, empty and reordered input in both stylesheet versions. Another 31 checks isolate recognition, expression gates, content models, member shape, result typing, focus and node identity. The existing public manifest runner uses the **default Legacy host**; `xslt31`/`xslt40` labels describe stylesheet versions, not distinct host profiles.

## Pinned review sources

Freshly read qtspecs default HEAD is `3230573afe246f1b502ed2e22d187ce9928155b1`. The [immutable XSLT source](https://raw.githubusercontent.com/qt4cg/qtspecs/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xslt-40/src/xslt.xml) has SHA-256 `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`. Relevant section IDs are `xsl-switch`, `array-construction` and `select-instruction`. This is a current review pin, **not a recovered historical adoption pin**. The XML bytes match the earlier switch review's source hash even though the repository HEAD differs; identical file content does not establish historical adoption. Earlier original execution/submission receipts remain unchanged.

## Grammar and type mapping

The following are descriptive shapes for the reviewed forms, not a complete normative grammar. Required namespace is `http://www.w3.org/1999/XSL/Transform`.

| Surface | Syntax / result contract | New evidence (ID suffix after `dispatch-grammar-`) | Existing source evidence |
|---|---|---|---|
| `xsl:switch` | Required `select`; one or more `when`, optional single `otherwise`, trailing fallbacks. Selector must convert to one atomic item; branch tests are general-comparison values, not boolean guards. | `recognized-switch-3/4`, `switch-no-selector`, `switch-no-when`, `switch-duplicate-otherwise`, `switch-late-when`, `switch-empty-selector` | [Stylesheet static validation](../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs); [SwitchNativeBoundaryTests](../../tests/Bosak.Xslt.Tests/SwitchNativeBoundaryTests.cs) |
| Selected switch branch | Either `select` or sequence-constructor content; a selected empty result still terminates branch selection. Nodes retain identity in raw sequences. | `switch-select-content`, `switch-node-identity`; report route comparisons | [switch boundary record](../XSLT40-SWITCH-BOUNDARIES.md); `SelectedNodeKeepsIdentity` and native switch018 |
| `xsl:array/@select` | Without `for-each`, one member per selected item; empty selection gives no members. Array is an XDM value, not an XML element. | `array-per-item`, `array-empty`, `recognized-array-3/4` | `Xslt40ArraySwitchTests.Array_Select_OneSingletonMemberPerItem` |
| `xsl:array/@for-each` + `@select` | One member per focus item; the whole selected sequence is that member, including `()`. Focus follows each input item. | `array-per-sequence`; all report fixtures retain batch/member alignment | `Array_ForEachSelect_MemberIsWholeSequence`; [ArraySelectNativeBoundaryTests](../../tests/Bosak.Xslt.Tests/ArraySelectNativeBoundaryTests.cs) |
| `xsl:array-member` | A sequence-valued member; an empty member differs from an empty array. `select` and content cannot be combined. | `array-empty-member`, `member-select-content`, `array-select-content` | `ArrayMember_JNodeValueAndNestedEmptyMembers`; existing array/member native boundaries |
| `xsl:select` | XPath in text/CDATA rather than a select attribute. Optional `as` applies result conversion/type checks. Leading fallbacks are permitted; arbitrary child instructions or fallback after expression text are not. | `select-child`, `select-late-fallback`, `select-required-empty`, `select-type-error`, `select-coercion`, `select-cdata` | [Xslt40SelectTests](../../tests/Bosak.Xslt.Tests/Xslt40SelectTests.cs): `Select_EmptyAndCoercion`, `Select_InvalidResult_XPTY0004`, `Select_InvalidContent_XTSE0010` |
| Effective expression version | Legacy host uses effective stylesheet/instruction version to choose XPath31 versus frozen XPath40. | `expression-root30/root40/local30/local40` | `Stylesheet.GetXPathCompatibility`; [ProcessorProfileTests](../../tests/Bosak.Xslt.Tests/ProcessorProfileTests.cs) |
| Raw items versus text | `xsl:select` returns items. Adjacent atomic items can acquire separators during final tree construction; explicit `xsl:value-of` creates text deliberately. Caller focus is retained. | `select-focus` expects the atomic separator; `select-focus-text` emits exact joined text; report4 wraps each result in value-of | `Xslt40SelectTests.Select_CurrentFocus_PreservesWholeSequence`; [runtime](../../src/Bosak.Xslt/Runtime/TransformEngine.cs) |

## Recognition is distinct from compatibility

| Context | Observed / source-derived behavior | Evidence scope |
|---|---|---|
| Legacy host, stylesheet3.0 | These three known draft instructions are recognized by Bosak, with XPath31 expressions. This is **not portable XSLT3.0 syntax**. | Six new recognition checks cover versions3.0/4.0; only the separate before report is the portable baseline. |
| Legacy host, effective version4.0 | Embedded expressions use frozen XPath40. A local version3.0 restores XPath31; a local4.0 enables it under a3.0 root. | Four new expression tests use `for member`; failures are XPST0003. |
| Explicit Xslt40Draft host | Source selects frozen XPath40 even for a local lower stylesheet version. The host ceiling also governs forwards-compatible handling of unknown syntax. | Existing `ProcessorProfileTests.DraftXPathIndependentOfVersion` and `LocalVersion`, run by the full solution. Not asserted by the default-host manifest alone. |
| Experimental XPath40 | No experimental compatibility is selected by these XSLT manifest cases. | No experimental grammar promise or new profile/gate promotion. |

## Delivery and acceptance reconciliation

Switch/array delivery remains PR92 at2a62ad4d. `xsl:select` body-expression delivery is `69a7cfd3c84e405c037be41786464950d1c06696`; later namespace-declaration support is separate. The switch identity correction delivered by PR395 is now published in1.23.2; PR399 supplies the unrelated index-of correction. Native array/select and switch contribution receipts remain [separate](../../contributions/xslt40/array-select-boundaries/evidence.json). Upstream review/independent processor confirmation is not inferred from local success.

| REQ-126 area | This slice establishes | Remaining work |
|---|---|---|
| D3 grammar/type mapping | These three XSLT families, version/host distinctions and named error/shape evidence. | Other grammar, type and pattern productions remain; this is not an exhaustive inventory. |
| D2 signatures/options | No additional closure. | Deep-equal options, keyword drift and the recorded function/default gaps remain separate. |
| D4 history/provenance | Exact current review source and xsl:select delivery revision. | Historical adopted-specification pins remain absent where already recorded. |
| AC-01/AC-04 | One new16-field xsl:select entry and corrected existing array/switch fields;69 entries total. | Global traceability, drift and lifecycle review remain partial. |
| AC-03 | Applicable catalog/unit/native XSLT evidence, with explicit host scope. | These are not XPath qt4tests expression sets and have no GatedSets membership. The literal every-entry qt4tests requirement remains unresolved; no waiver or invented promotion. |

REQ-126 stays **In Progress**. Counts are69 entries,2,673 feature +24 chapter checks =2,697. Four worked chapters and117 assigned function names are unchanged. All2,636 original manifest rows remain unchanged;37 rows are appended. The next bounded D3 slice should inventory named/structural type and map/array pattern grammar against the existing tests, before deciding any further engine work. Deep-equal options need a separate contract/design review.

## Execution and fixture correction

First execution is33/3/0 across36 new rows. Two report failures and one focus failure expose an incorrect text-serialization assumption in these fixtures. Atomic items were separated during tree construction. The report now deliberately creates text with value-of; the raw focus case records the separator, and an additional text-focus case verifies the contrasting output. Original fixtures/manifest/TRX are preserved under `.guard-tmp/req126-grammar-evidence/first-execution-*`; an intermediate1-failure encoding-interrupted run is also retained. Final focused result is **37/0/0**. No engine behavior or historical expected result is changed. [Machine-readable audit](SWITCH-ARRAY-SELECT-GRAMMAR.json) retains original/final hashes and IDs.

**Full validation:** Release build passes with the existing conformance Program.cs:1868 CS8604 warning. All10,368 solution tests pass, including7,400 XSLT tests. The37 focused cases overlap this total. Original and corrected receipts, fixture hashes and specification pin are recorded in the audit. Engine/manual conformance gates are not rerun for this documentation-only diff; engine, parser, runtime, compiler, harness source and membership are unchanged.
