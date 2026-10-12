<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Customer Text Cleanup">
  <br><br><h1>Customer Text Cleanup</h1>
  <p>Frozen 4.0 — real-world import text contracts</p>
</div>

## 1. Name and Classification

**Customer text cleanup — Frozen 4.0.** [fn:trim-space](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-trim-space), [fn:pad-string](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-pad-string), [fn:characters](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-characters), [fn:graphemes](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-graphemes) and [fn:char](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-char).
Initial char/characters delivery: [PR #82](https://github.com/Fytala-Charles/Bosak/pull/82), [40ebaa79](https://github.com/Fytala-Charles/Bosak/commit/40ebaa79032b17fc45d6d3bb3e58cb00e2fda742), REQ-118 S0/S1 part 1. Remaining string functions: [PR #83](https://github.com/Fytala-Charles/Bosak/pull/83), [532d04f4](https://github.com/Fytala-Charles/Bosak/commit/532d04f4672df430d93752e3158fc3cdfd4c7c39), S1 part 2.

Definitions reviewed **2026-10-12** against immutable [QT4CG source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). The eight reviewed function definitions are unchanged from the earlier a014e1b4 review pin. This does not recover unavailable historical adopted specification snapshots.

## 2. Intent

Trim import padding from a customer label while preserving its interior spacing and Unicode spelling. Report codepoint and grapheme counts separately, and pad an export value to a minimum codepoint length.

## 3. Also Known As

Boundary trimming, minimum-length padding and character segmentation are descriptive terms. No function aliases are registered.

## 4. History / Supersedes

XPath 3.1 can trim XML whitespace with two anchored regular-expression replacements, split codepoints with string-to-codepoints/codepoints-to-string, and append repeated padding explicitly. The complete comparison returns the same cleaned and padded label. It omits the grapheme count: XPath 3.1 has no direct standard grapheme-segmentation function. `normalize-space` would also collapse interior runs and therefore implements a different cleanup rule.

## 5. Motivation

A customer name can contain deliberately repeated interior spaces, a combining accent and a supplementary character. Treating UTF-16 storage units, Unicode codepoints and displayed clusters as interchangeable gives misleading length checks. The report makes the import/export contract explicit.

## 6. Language Alignment

`trim-space` removes XML whitespace (space, tab, CR and LF) at the selected start/end/both boundary. It preserves interior whitespace and does not remove NBSP. Empty input returns an empty string. `pad-string` defaults to end padding with spaces; options select side and padding. It never truncates. Both-side odd padding puts the extra character at the end.

`characters` returns one string per Unicode codepoint; `graphemes` groups supported combining/ZWJ sequences into clusters. Neither performs Unicode normalization. `char` constructs a string from a positive codepoint, recognized entity name or supported escape; some entity names return multiple codepoints. It is not an HTML document decoder.

Bosak's [Graphemes_1/GraphemeClusterClass implementation](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs) approximates segmentation: it does not distinguish Hangul jamo, Prepend, regional-indicator or full Indic/extended-pictographic classes. These verified combining-mark and ZWJ cases do not establish complete Unicode grapheme conformance.

## 7. Applicability

Use for import boundary cleanup and codepoint-length export rules. The fixture's target length is a minimum of ten codepoints, not bytes, screen columns or grapheme clusters. Padding does not make emoji and Latin characters equally wide in a font. A Unicode normalization policy or complete grapheme implementation is a separate requirement. The examples do not truncate customer names or claim to validate them.

## 8. Structure

Read a name element, trim its boundaries, count codepoints and supported grapheme clusters, then append dots to reach ten codepoints. Construct an em-dash separator with char(8212).

## 9. Participants

The customer element supplies unnormalized text. Cleanup preserves interior content. Segmentation describes length under two models. Padding produces the export field. A report shows all outputs.

## 10. Collaborations

Trim before padding. Keep the original combining sequence when preservation is required. Use count(characters(...)) for the specified codepoint contract; do not substitute grapheme count for that minimum-length rule.

## 11. Consequences

The fixture retains two interior spaces and a decomposed accent. Its eight codepoints form seven supported grapheme clusters; two dots reach ten codepoints. Oversized values remain unchanged. Invalid side/padding options and unsupported char values raise the demonstrated errors. Known grapheme limitations remain documented rather than being certified by these samples.

## 12. Implementation

Compile new calls with CompileOptions.Compatibility40; the default 3.1 profile rejects them. The comparison uses Compatibility31. The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) verifies exact text/error markers and default-profile refusal for successful 4.0 cases. Unicode inputs are inline XML character references so their intended values are explicit.

## 13. Sample Code

Complete synthetic input; the name contains e followed by combining acute, two interior spaces and a truck emoji:

```xml
<customer><name>  Jose&#x301;  &#x1F69A;  </name></customer>
```

### Complete XPath 4.0 expression

[Runnable file](../examples/customer-text-cleanup/main.xpath):

```xpath
let $name := string(parse-xml('<customer><name>  Jose&#x301;  &#x1F69A;  </name></customer>')/customer/name)
let $label := trim-space($name)
return concat('label=[', $label, ']; codepoints=', count(characters($label)), '; graphemes=', count(graphemes($label)), '; export=[', pad-string($label, 10, map {'padding': '.'}), ']; separator=', char(8212))
```

Exact output:

```text
label=[José  🚚]; codepoints=8; graphemes=7; export=[José  🚚..]; separator=—
```

### Complete XPath 3.1 comparison

[Runnable file](../examples/customer-text-cleanup/baseline.xpath):

```xpath
let $name := string(parse-xml('<customer><name>  Jose&#x301;  &#x1F69A;  </name></customer>')/customer/name)
return let $label := replace(replace($name, '^\s+', ''), '\s+$', '')
return concat('label=[', $label, ']; codepoints=', count(string-to-codepoints($label)), '; export=[', $label, string-join(for $i in 1 to max((0, 10 - string-length($label))) return '.', ''), ']; separator=', codepoints-to-string(8212))
```

Exact output:

```text
label=[José  🚚]; codepoints=8; export=[José  🚚..]; separator=—
```

### Boundary and error inventory

Each file embeds its complete input. Brackets make significant spaces visible.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/customer-text-cleanup/main.xpath) | `label=[José  🚚]; codepoints=8; graphemes=7; export=[José  🚚..]; separator=—` |
| [baseline](../examples/customer-text-cleanup/baseline.xpath) | `label=[José  🚚]; codepoints=8; export=[José  🚚..]; separator=—` |
| [preserve-interior](../examples/customer-text-cleanup/preserve-interior.xpath) | `[Ada  Lovelace]` |
| [nbsp-preserved](../examples/customer-text-cleanup/nbsp-preserved.xpath) | `160` |
| [trim-start](../examples/customer-text-cleanup/trim-start.xpath) | `[Ada  ]` |
| [empty-trim](../examples/customer-text-cleanup/empty-trim.xpath) | `0` |
| [pad-both](../examples/customer-text-cleanup/pad-both.xpath) | `**Ada***` |
| [never-truncate](../examples/customer-text-cleanup/never-truncate.xpath) | `Lovelace` |
| [empty-pad](../examples/customer-text-cleanup/empty-pad.xpath) | `...` |
| [codepoint-split](../examples/customer-text-cleanup/codepoint-split.xpath) | `101,769` |
| [combining-grapheme](../examples/customer-text-cleanup/combining-grapheme.xpath) | `1` |
| [emoji-zwj](../examples/customer-text-cleanup/emoji-zwj.xpath) | `1` |
| [empty-characters](../examples/customer-text-cleanup/empty-characters.xpath) | `0` |
| [empty-graphemes](../examples/customer-text-cleanup/empty-graphemes.xpath) | `0` |
| [entity-name](../examples/customer-text-cleanup/entity-name.xpath) | `á` |
| [multi-codepoint-entity](../examples/customer-text-cleanup/multi-codepoint-entity.xpath) | `2` |
| [invalid-pad-side](../examples/customer-text-cleanup/invalid-pad-side.xpath) | `FORG0001` |
| [empty-padding](../examples/customer-text-cleanup/empty-padding.xpath) | `FORG0001` |
| [invalid-trim-side](../examples/customer-text-cleanup/invalid-trim-side.xpath) | `FORG0001` |
| [unknown-entity](../examples/customer-text-cleanup/unknown-entity.xpath) | `FOCH0005` |
| [surrogate](../examples/customer-text-cleanup/surrogate.xpath) | `FOCH0005` |
| [nonstring-characters](../examples/customer-text-cleanup/nonstring-characters.xpath) | `XPTY0004` |

The 3.1 output deliberately excludes grapheme count; it verifies the same cleanup, codepoint padding and separator. NBSP is reported as codepoint 160 so an invisible preserved character is reviewable. The ZWJ case supplies woman + joiner + laptop via codepoints. Invalid values verify documented diagnostics.

## 14. Performance

Text segmentation and padding process and materialize the supplied text. No display measurement or byte encoding occurs. Very large inputs and padding targets require memory; no benchmark or allocation-free claim is implied.

## 15. Known Uses

This customer import is synthetic. Existing evidence includes [`TrimSpace_DefaultTrimsBothEnds`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`TrimSpace_XmlWhitespaceOnly`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`PadString_NeverTruncates`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`PadString_BothSidesSplitsWithExtraAtEnd`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Characters_NonAsciiString_SplitsByCodepoint`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Graphemes_CombiningMark_AttachesToBase`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Graphemes_EmojiZwjSequence_IsOneGrapheme`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), and [`Char_MultiCodepointEntity_ReturnsFullString`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).

The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) assigns exact fn-characters to GatedSets. No exact char/graphemes/pad-string/trim-space assignment is claimed. These 22 executable cases provide separate evidence; no gate is promoted.

## 16. Related Features

[Import file names](import-file-names.md) covers codepoint substring positions. [Configured label callbacks](configured-label-callbacks.md) covers reusable formatting policies. [The audit](../COMPLETENESS-AUDIT.md) records the grapheme limitation separately from reference-name coverage.
