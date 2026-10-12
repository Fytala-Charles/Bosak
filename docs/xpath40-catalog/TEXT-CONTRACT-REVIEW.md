<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak text and Unicode function contract review">
  <br><br><h1>Text and Unicode Contract Review</h1>
  <p>Keywords, positional defaults, options and typed input boundaries</p>
</div>

## Scope and outcome

Reviewed 2026-10-12 against Bosak `f53dd8ff9ca055eef36e2cdd69c469653b0a1ae5`, version 1.1.2. Eight functions from the 37 absent-keyword-metadata families now have representative behavioral review. Metadata remains absent: the review does not implement it. **29 absent-metadata families still need this targeted review.** D2 and REQ-126 remain partial/In Progress.

83 new executable checks comprise 24 explicit keyword/mode refusals, 55 positional/default/empty/invalid/option cases and four full import reports (3.1/4.0, normal/empty input). Catalog: 67 entries, 1,556 feature + 24 chapter checks = 1,580. No engine/API/harness change or gate promotion.

Pinned contracts: [F&O function catalog e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml), and [option-parameter conventions in F&O source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/xpath-functions.xml). Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs). The [raw signature matrix](audit-signature-contracts.json) now links all eight families' exact case IDs. These current pins do not recover historical adoption.

## Declared contracts and keywords

| Function | Pinned parameters/defaults | Result type | Current keyword outcome |
|---|---|---|---|
| [`fn:char`](functions/customer-text-cleanup.md) | `value: (xs:string \| xs:positiveInteger)` | `xs:string` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:characters`](functions/customer-text-cleanup.md) | `value: xs:string?` | `xs:string*` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:graphemes`](functions/customer-text-cleanup.md) | `value: xs:string?` | `xs:string*` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:index-of-substring`](functions/import-file-names.md) | `value: xs:string?, substring: xs:string?` | `xs:integer*` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:pad-string`](functions/customer-text-cleanup.md) | `value: xs:anyAtomicType?, length: xs:integer, options: map(*)? := {}` | `xs:string` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:substring-after-last`](functions/import-file-names.md) | `value: xs:string?, substring: xs:string?, collation: xs:string? := fn:default-collation()` | `xs:string` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:substring-before-last`](functions/import-file-names.md) | `value: xs:string?, substring: xs:string?, collation: xs:string? := fn:default-collation()` | `xs:string` | 3.1 XPST0003; 4.0/experimental XPST0017 |
| [`fn:trim-space`](functions/customer-text-cleanup.md) | `value: xs:anyAtomicType?, options: map(*)? := {}` | `xs:string` | 3.1 XPST0003; 4.0/experimental XPST0017 |

3.1 rejects keyword grammar before function binding (XPST0003). Frozen 4.0 and experimental accept keyword grammar but reject the eight calls because keyword metadata is missing (XPST0017). Explicit experimental refusals do not change each function's Frozen 4.0 tier. Existing positional forms remain available.

## Positional/default evidence

pad-string omitted, empty-sequence and empty-map options produce the same end-space padding. Both-side odd padding places the extra codepoint at the end; repeated padding is truncated to the required run; long input is retained. Integer source 42 is permitted by the declared anyAtomicType input and yields 00042 with start-zero padding. A string length argument refuses XPTY0004.

trim-space omitted/empty options trim both boundaries, preserving interior repeated spaces. Start/end options act independently. Empty input yields empty text; integer input is permitted by anyAtomicType and yields 42. Non-map options refuse XPTY0004; invalid side reports FORG0001; a map source refuses FOTY0013.

characters splits Unicode codepoints; graphemes groups the tested combining sequence and emoji as supported clusters. Typed numeric input refuses XPTY0004 in both. Grapheme segmentation remains approximate; this review does not add Unicode classes or establish complete normalization/segmentation conformance. char accepts the tested positive codepoint/entity and refuses empty/non-positive input or unknown entity with the published errors.

substring-before-last/after-last omitted, explicit codepoint and empty-sequence collation agree in the tested filename case. Unknown collation refuses FOCH0002. Empty needle gives the full source before-last and empty after-last. Overlapping index-of-substring occurrences and codepoint positions are explicit. **Empty source with a nonempty needle produces no positions, but empty source with empty needle produces one boundary.** This corrects the earlier blanket empty-source wording.

## Type and option boundaries needing follow-up

| Published case | Observed behavior | Interpretation |
|---|---|---|
| [Numeric before-last](examples/text-contracts/before-number.xpath), [numeric after-last](examples/text-contracts/after-number.xpath) | Typed 4242 is treated as text; before returns 42, after empty | Pinned inputs declare xs:string?; review precise 4.0 coercion/enforcement. index-of-substring rejects its numeric source. |
| [Numeric padding](examples/text-contracts/pad-padding-number.xpath) | padding 7 produces INV77 | Declared option type is xs:string; verify full coercion-rule contract before classifying/fixing. |
| [Invalid side without padding](examples/text-contracts/pad-invalid-side-not-needed.xpath) | Returns invoice unchanged | The tested early-return path does not validate the side enum. Do not infer complete option validation from the padding-needed refusal. |
| [Unknown padding key](examples/text-contracts/pad-unknown-option.xpath), [unknown trimming key](examples/text-contracts/trim-unknown-option.xpath) | Unqualified sideways key is ignored | Pinned option conventions require rejection of an unrecognized key unless recognized by the implementation or a QName with namespace. No meaning for sideways is documented or handled in these implementations; retain this as an option-convention review gap. |

Tests preserve current observed outcomes. They do not turn tolerated input into a recommended consumer contract. Use declared argument/option types and recognized names in production examples. The engine/specification follow-ups remain separate from documentary coverage.

## Complete consumer example and next work

The [customer label and filename README](examples/text-contracts/README.md) shows complete source XML, positional 4.0 code, portable nested-let 3.1 code and exact outputs. It separates codepoint-width padding from display columns/graphemes, preserves combining spelling and interior spaces, and extracts only the final extension. No file IO or path authorization is implied.

Next continue D2 with sequence selection/repetition contracts: replicate, items-at, foot, trunk, insert-separator and slice. Review missing keywords, defaults/empty arguments and typed sources using a realistic import/export batch. The remaining changed-3.1 inventory, complete coercion/options coverage, D3 grammar/pattern matrix and D4 provenance/acceptance still prevent closure. Existing source/tests/gates are linked in the two family entries; this documentation slice does not rerun manual engine gates.
