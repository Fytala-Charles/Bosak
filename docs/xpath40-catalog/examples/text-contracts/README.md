<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak customer label and import filename examples">
  <br><br><h1>Customer Labels and Import File Names</h1>
  <p>Detailed text-contract examples with full input/output</p>
</div>

## Business case

A batch importer receives a customer label and lexical file path. The export contract preserves interior spaces and the original combining accent, removes XML whitespace at label boundaries, pads to a minimum of 24 Unicode codepoints with dots, and reports the filename stem and last extension. This is not a display-column, byte-width or grapheme-width contract. The filename convention uses slash and final dot; it does not normalize Windows/URI paths or access the filesystem.

Complete input (the accent after e is U+0301, not a precomposed replacement):

```xml
<import><customer>  Café  Supplies  </customer><file>/incoming/2026.10/invoice.batch.XML</file></import>
```

customer supplies the label; file supplies the path string. parse-xml constructs the document. trim-space atomizes customer and trims only its boundaries. characters supplies codepoint count. pad-string adds nine dots to the 15-codepoint cleaned label. substring-after-last first isolates the filename; before/after-last on dot preserve invoice.batch and identify XML.

Complete [positional 4.0 expression](import-40.xpath):

```xquery
let $doc := parse-xml('<import><customer>  Café  Supplies  </customer><file>/incoming/2026.10/invoice.batch.XML</file></import>')/import let $clean := trim-space($doc/customer) let $filename := substring-after-last(string($doc/file),'/') return concat('label=[',pad-string($clean,24,map{'padding':'.'}),'];codepoints=',count(characters($clean)),';stem=',substring-before-last($filename,'.'),';extension=',substring-after-last($filename,'.'))
```

Exact output:

```text
label=[Café  Supplies.........];codepoints=15;stem=invoice.batch;extension=XML
```

Complete [portable 3.1 expression](import-31.xpath):

```xquery
let $doc := parse-xml('<import><customer>  Café  Supplies  </customer><file>/incoming/2026.10/invoice.batch.XML</file></import>')/import return let $clean := replace(replace(string($doc/customer),'^\s+',''),'\s+$','') return let $parts := tokenize(string($doc/file),'/') return let $filename := $parts[last()] return let $dots := (for $i in 1 to string-length($filename) return if (substring($filename,$i,1)='.') then $i else ()) return concat('label=[',$clean,string-join(for $i in 1 to max((0,24-string-length($clean))) return '.', ''),'];codepoints=',string-length($clean),';stem=',if (exists($dots)) then substring($filename,1,$dots[last()]-1) else '', ';extension=',if (exists($dots)) then substring($filename,$dots[last()]+1) else '')
```

This gives the same output for the stated case. Explicit return between nested let bindings keeps the expression within XPath 3.1 grammar. Anchored regular expressions trim XML whitespace, string-length counts codepoints, repeated dots implement end padding and a codepoint-position loop locates the final dot. The baseline does not reproduce a general collation-aware search or Unicode grapheme segmentation.

## Empty input and invalid data

Empty-element input, embedded in [both versions](import-empty-40.xpath):

```xml
<import><customer/><file/></import>
```

Output:

```text
label=[........................];codepoints=0;stem=;extension=
```

[The 3.1 empty report](import-empty-31.xpath) agrees. Blank values are deliberately visible; this example does not decide whether a receiving system should accept an empty customer or missing filename. Required-field/business validation must precede an export that forbids them. Separate files below show invalid typed arguments, map options, sides, padding and collations.

## Keywords, defaults and Unicode boundaries

All eight reviewed functions have positional registrations but no keyword metadata. Each published keyword file has explicit modes: 3.1 refuses XPST0003 grammar, 4.0/experimental refuse XPST0017 binding. Adding experimental mode does not make those keywords available.

Default/empty/map options, start/end/both padding and trimming, no truncation, repeated padding and source types are separately checked. Numeric source for trim/pad is allowed by anyAtomicType; numeric source for characters/graphemes/index is rejected. The observed numeric substring/padding-option tolerance and skipped side validation require precise coercion/option review, rather than an assertion of conformance.

characters('Café😀') gives six codepoints; graphemes gives five supported clusters. This distinguishes representations without promising complete Unicode segmentation. Neither operation normalizes the combining accent. An empty needle in an empty string matches one boundary; an empty value with a nonempty needle matches none.

## Complete fixture inventory

| Exact runnable case | Result/error |
|---|---|
| [char-keyword-31](char-keyword-31.xpath) | `XPST0003` |
| [char-keyword-40](char-keyword-40.xpath) | `XPST0017` |
| [char-keyword-experimental](char-keyword-experimental.xpath) | `XPST0017` |
| [characters-keyword-31](characters-keyword-31.xpath) | `XPST0003` |
| [characters-keyword-40](characters-keyword-40.xpath) | `XPST0017` |
| [characters-keyword-experimental](characters-keyword-experimental.xpath) | `XPST0017` |
| [graphemes-keyword-31](graphemes-keyword-31.xpath) | `XPST0003` |
| [graphemes-keyword-40](graphemes-keyword-40.xpath) | `XPST0017` |
| [graphemes-keyword-experimental](graphemes-keyword-experimental.xpath) | `XPST0017` |
| [pad-string-keyword-31](pad-string-keyword-31.xpath) | `XPST0003` |
| [pad-string-keyword-40](pad-string-keyword-40.xpath) | `XPST0017` |
| [pad-string-keyword-experimental](pad-string-keyword-experimental.xpath) | `XPST0017` |
| [trim-space-keyword-31](trim-space-keyword-31.xpath) | `XPST0003` |
| [trim-space-keyword-40](trim-space-keyword-40.xpath) | `XPST0017` |
| [trim-space-keyword-experimental](trim-space-keyword-experimental.xpath) | `XPST0017` |
| [index-of-substring-keyword-31](index-of-substring-keyword-31.xpath) | `XPST0003` |
| [index-of-substring-keyword-40](index-of-substring-keyword-40.xpath) | `XPST0017` |
| [index-of-substring-keyword-experimental](index-of-substring-keyword-experimental.xpath) | `XPST0017` |
| [substring-before-last-keyword-31](substring-before-last-keyword-31.xpath) | `XPST0003` |
| [substring-before-last-keyword-40](substring-before-last-keyword-40.xpath) | `XPST0017` |
| [substring-before-last-keyword-experimental](substring-before-last-keyword-experimental.xpath) | `XPST0017` |
| [substring-after-last-keyword-31](substring-after-last-keyword-31.xpath) | `XPST0003` |
| [substring-after-last-keyword-40](substring-after-last-keyword-40.xpath) | `XPST0017` |
| [substring-after-last-keyword-experimental](substring-after-last-keyword-experimental.xpath) | `XPST0017` |
| [char-codepoint](char-codepoint.xpath) | `—` |
| [char-entity](char-entity.xpath) | `&` |
| [char-empty](char-empty.xpath) | `XPTY0004` |
| [char-zero](char-zero.xpath) | `XPTY0004` |
| [char-unknown](char-unknown.xpath) | `FOCH0005` |
| [characters-codepoints](characters-codepoints.xpath) | `C\|a\|f\|e\|́\|😀` |
| [characters-empty](characters-empty.xpath) | `0` |
| [characters-number](characters-number.xpath) | `XPTY0004` |
| [graphemes-clusters](graphemes-clusters.xpath) | `C\|a\|f\|é\|😀` |
| [graphemes-empty](graphemes-empty.xpath) | `0` |
| [graphemes-number](graphemes-number.xpath) | `XPTY0004` |
| [pad-default](pad-default.xpath) | `[INV-42    ]` |
| [pad-empty-options](pad-empty-options.xpath) | `[INV-42    ]` |
| [pad-map-defaults](pad-map-defaults.xpath) | `[INV-42    ]` |
| [pad-both](pad-both.xpath) | `..INV-42...` |
| [pad-start-repeat](pad-start-repeat.xpath) | `ababaINV` |
| [pad-no-truncation](pad-no-truncation.xpath) | `invoice` |
| [pad-empty-input](pad-empty-input.xpath) | `...` |
| [pad-integer-input](pad-integer-input.xpath) | `00042` |
| [pad-invalid-options](pad-invalid-options.xpath) | `XPTY0004` |
| [pad-empty-padding](pad-empty-padding.xpath) | `FORG0001` |
| [pad-invalid-side](pad-invalid-side.xpath) | `FORG0001` |
| [pad-invalid-side-not-needed](pad-invalid-side-not-needed.xpath) | `invoice` |
| [pad-padding-number](pad-padding-number.xpath) | `INV77` |
| [pad-length-string](pad-length-string.xpath) | `XPTY0004` |
| [trim-default](trim-default.xpath) | `[INV  42]` |
| [trim-empty-options](trim-empty-options.xpath) | `[INV  42]` |
| [trim-start](trim-start.xpath) | `[INV  42  ]` |
| [trim-end](trim-end.xpath) | `[  INV  42]` |
| [trim-empty-value](trim-empty-value.xpath) | `[]` |
| [trim-integer](trim-integer.xpath) | `42` |
| [trim-invalid-options](trim-invalid-options.xpath) | `XPTY0004` |
| [trim-invalid-side](trim-invalid-side.xpath) | `FORG0001` |
| [trim-map-source](trim-map-source.xpath) | `FOTY0013` |
| [index-overlap](index-overlap.xpath) | `1,2,3` |
| [index-empty-needle](index-empty-needle.xpath) | `1,2,3,4` |
| [index-empty-source](index-empty-source.xpath) | `0` |
| [index-both-empty](index-both-empty.xpath) | `1` |
| [index-number](index-number.xpath) | `XPTY0004` |
| [before-default](before-default.xpath) | `invoice.batch` |
| [before-codepoint](before-codepoint.xpath) | `invoice.batch` |
| [before-empty-collation](before-empty-collation.xpath) | `invoice.batch` |
| [before-unknown-collation](before-unknown-collation.xpath) | `FOCH0002` |
| [before-empty-value](before-empty-value.xpath) | `[]` |
| [before-empty-needle](before-empty-needle.xpath) | `INV` |
| [before-number](before-number.xpath) | `42` |
| [after-default](after-default.xpath) | `XML` |
| [after-codepoint](after-codepoint.xpath) | `XML` |
| [after-empty-collation](after-empty-collation.xpath) | `XML` |
| [after-unknown-collation](after-unknown-collation.xpath) | `FOCH0002` |
| [after-empty-value](after-empty-value.xpath) | `[]` |
| [after-empty-needle](after-empty-needle.xpath) | `[]` |
| [after-number](after-number.xpath) | `` |
| [pad-unknown-option](pad-unknown-option.xpath) | `INV  ` |
| [trim-unknown-option](trim-unknown-option.xpath) | `INV` |
| [import-40](import-40.xpath) | `label=[Café  Supplies.........];codepoints=15;stem=invoice.batch;extension=XML` |
| [import-31](import-31.xpath) | `label=[Café  Supplies.........];codepoints=15;stem=invoice.batch;extension=XML` |
| [import-empty-40](import-empty-40.xpath) | `label=[........................];codepoints=0;stem=;extension=` |
| [import-empty-31](import-empty-31.xpath) | `label=[........................];codepoints=0;stem=;extension=` |

## Run and interpret

Run `dotnet test tests/Bosak.Xslt.Tests -c Release --filter FullyQualifiedName~CatalogExampleTests`. The existing harness executes exact files with fresh EvaluationContext via public APIs and verifies output/error and applicable default compatibility. No external variables, resolver or host context item are needed.

Read the [text contract review](../../TEXT-CONTRACT-REVIEW.md) and [raw signature matrix](../../audit-signature-contracts.json) alongside [customer cleanup](../../functions/customer-text-cleanup.md) and [filename extraction](../../functions/import-file-names.md). 83 new checks establish scoped behavior, not complete conformance. Version remains 1.1.2; REQ-126 and D2 remain incomplete.
