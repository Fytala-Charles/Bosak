<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Import File Names">
  <br><br><h1>Import File Names</h1>
  <p>Frozen 4.0 — real-world import text contracts</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. Do not rely on tolerated typed-numeric string arguments; no grapheme-position or path-authorization contract. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Import file names — Frozen 4.0.** [fn:substring-before-last](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-substring-before-last), [fn:substring-after-last](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-substring-after-last), and [fn:index-of-substring](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-index-of-substring).
Initial delivery: [PR #83](https://github.com/Fytala-Charles/Bosak/pull/83), [532d04f4](https://github.com/Fytala-Charles/Bosak/commit/532d04f4672df430d93752e3158fc3cdfd4c7c39), REQ-118 S1 part 2.

Definitions reviewed **2026-10-12** against immutable [QT4CG source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). The eight reviewed function definitions are unchanged from the earlier a014e1b4 review pin. This does not recover unavailable historical adopted specification snapshots.

## 2. Intent

Split an import path at its final slash and a file name at its final dot, preserving earlier dots. Report dot positions to make the chosen extension boundary explicit.

## 3. Also Known As

Last-delimiter extraction and substring-position search are descriptive terms; no aliases are registered.

## 4. History / Supersedes

XPath 3.1 can enumerate matching codepoint positions, select the last one and slice with substring. The complete comparison implements the illustrated slash/dot contract. It is scoped to this path with present delimiters; a general helper must also handle missing and empty delimiters explicitly.

## 5. Motivation

An import directory can contain dots and a file can have multiple suffixes. Splitting at the first dot would mistake invoice.batch.xml for extension batch.xml. A final-boundary rule gives stem invoice.batch and extension xml, matching the stated importer policy.

## 6. Language Alignment

Last-substring functions return the parts before or after the last match under the chosen collation. If no match exists, both return an empty string. With an empty delimiter, before-last returns the input and after-last returns empty. They accept an optional collation; the case uses the default codepoint collation.

index-of-substring reports one-based codepoint positions, includes overlapping matches, and has no collation argument. An empty substring matches each boundary through length+1. Empty source with a nonempty needle produces no positions; an empty source and empty needle produce one boundary. These are codepoint positions, not UTF-16 storage indices or grapheme-cluster indices.

## 7. Applicability

Use for a declared lexical slash/dot import convention. The main fixture assumes a nonempty slash-separated path with a final filename and a dot extension. It does not access files, normalize Windows paths, parse URIs or classify compound archives. For archive.tar.gz the final extension is gz; recognizing tar.gz as a compound format requires a separate policy. URI percent escapes and query/fragment boundaries belong to [URI component parsing](invoice-api-uris.md).

## 8. Structure

Select the text path, derive directory and final filename, then derive stem/extension and all dot positions from that filename. Dots in the directory do not affect extension selection.

## 9. Participants

The file record supplies text. Last-match functions select boundaries. Position search exposes filename delimiters. The report shows the exact import classification.

## 10. Collaborations

Extract the filename before searching its dots. Apply presence guards in a production importer if a missing delimiter must preserve the whole filename rather than produce empty. Use an explicit collation for case-insensitive text delimiters; the index function remains codepoint matching.

## 11. Consequences

Earlier separators and dots remain in the preceding parts. Overlapping delimiters can select a later start than a non-overlapping tokenizer would. No delimiter is an empty-result contract, not an automatic whole-filename fallback. No path authorization or file operation occurs.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Last delimiter, overlap and one-based codepoint positions; Empty needle/source, default collation and malformed collation; Experimental absent-pattern correction with frozen refusal retained. **Boundaries:** Do not rely on tolerated typed-numeric string arguments; no grapheme-position or path-authorization contract. Follow-up groups: F01, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Use CompileOptions.Compatibility40 for the new calls and Compatibility31 for the comparison. Default 3.1 rejects new names. The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) verifies exact report/error markers and default-profile refusal for successful 4.0 cases. All inputs are inline.

**Supplementary text-contract review (2026-10-12):** [Eight-family contract review](../TEXT-CONTRACT-REVIEW.md) and [detailed label/filename README](../examples/text-contracts/README.md) supply explicit keyword refusals at 3.1/4.0/experimental, positional defaults/empty/invalid cases and complete paired reports. Numeric substring/option tolerance and validation boundaries remain observed follow-ups; the review does not implement keywords or certify full Unicode/coercion conformance.

## 13. Sample Code

Complete synthetic file record:

```xml
<file path="incoming/2026.10/invoice.batch.xml"/>
```

### Complete XPath 4.0 expression

[Runnable file](../examples/import-file-names/main.xpath):

```xpath
let $path := string(parse-xml('<file path="incoming/2026.10/invoice.batch.xml"/>')/file/@path)
let $directory := substring-before-last($path, '/')
let $name := substring-after-last($path, '/')
return concat('directory=', $directory, '; file=', $name, '; stem=', substring-before-last($name, '.'), '; extension=', substring-after-last($name, '.'), '; dots=', string-join(index-of-substring($name, '.') ! string(.), ','))
```

Exact output:

```text
directory=incoming/2026.10; file=invoice.batch.xml; stem=invoice.batch; extension=xml; dots=8,14
```

### Complete XPath 3.1 comparison

[Runnable file](../examples/import-file-names/baseline.xpath):

```xpath
let $path := string(parse-xml('<file path="incoming/2026.10/invoice.batch.xml"/>')/file/@path)
return let $positions := function($value, $needle) { for $p in 1 to string-length($value) return if (substring($value, $p, string-length($needle)) eq $needle) then $p else () }
return let $slash := $positions($path, '/')[last()]
return let $directory := substring($path, 1, $slash - 1)
return let $name := substring($path, $slash + 1)
return let $dots := $positions($name, '.')
return let $dot := $dots[last()]
return concat('directory=', $directory, '; file=', $name, '; stem=', substring($name, 1, $dot - 1), '; extension=', substring($name, $dot + 1), '; dots=', string-join($dots ! string(.), ','))
```

Exact output:

```text
directory=incoming/2026.10; file=invoice.batch.xml; stem=invoice.batch; extension=xml; dots=8,14
```

### Boundary and error inventory

Each file embeds its complete input. Brackets make significant spaces visible.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/import-file-names/main.xpath) | `directory=incoming/2026.10; file=invoice.batch.xml; stem=invoice.batch; extension=xml; dots=8,14` |
| [baseline](../examples/import-file-names/baseline.xpath) | `directory=incoming/2026.10; file=invoice.batch.xml; stem=invoice.batch; extension=xml; dots=8,14` |
| [compound-extension](../examples/import-file-names/compound-extension.xpath) | `gz` |
| [missing-delimiter](../examples/import-file-names/missing-delimiter.xpath) | `[]|[]` |
| [empty-delimiter](../examples/import-file-names/empty-delimiter.xpath) | `[invoice]|[]` |
| [overlapping](../examples/import-file-names/overlapping.xpath) | `before=a; after=[]; positions=1,2` |
| [no-matches](../examples/import-file-names/no-matches.xpath) | `0` |
| [empty-search](../examples/import-file-names/empty-search.xpath) | `1,2,3,4` |
| [empty-source](../examples/import-file-names/empty-source.xpath) | `0` |
| [empty-both](../examples/import-file-names/empty-both.xpath) | `[]|[]` |
| [supplementary-position](../examples/import-file-names/supplementary-position.xpath) | `1,3` |
| [explicit-collation](../examples/import-file-names/explicit-collation.xpath) | `-c` |
| [multi-item-source](../examples/import-file-names/multi-item-source.xpath) | `XPTY0004` |

The overlapping fixture reports matches at positions one and two in aaa for aa. Supplementary-character positions verify codepoints. The case-insensitive collation fixture tests a textual delimiter separately from the path rule. Missing/empty delimiters and empty source inputs are distinct cases.

## 14. Performance

Position search materializes match positions and text extraction works on supplied strings. The 3.1 comparison scans candidate positions explicitly. No benchmark, filesystem access or universal complexity claim follows.

## 15. Known Uses

**Experimental empty-argument correction (source1.24.1):** [Contract](../INDEX-OF-SUBSTRING-CONTRACT.md), [38 public API controls](../../../tests/Bosak.XPath.Standard.Tests/SubstringExperimentalContractTests.cs) and [two host controls](../../../tests/Bosak.Xslt.Tests/SubstringExperimentalHostBoundaryTests.cs) verify the newer absent-pattern result only under `XPath40Experimental`. Frozen `fn-index-of-substring` case021 still requires XPTY0004; current upstream case021 remains a diagnostic failure in the frozen runner. No current-gate promotion or historical provenance recovery is claimed.


This importer rule is synthetic. Existing evidence includes [`IndexOfSubstring_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`IndexOfSubstring_EmptySubstring_MatchesEveryPosition`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`IndexOfSubstring_PositionsInCodepoints`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`SubstringBeforeLast_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`SubstringAfterLast_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), and [`SubstringAfterLast_WithCollation`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).

The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) assigns exact fn-index-of-substring to GatedSets. No exact before-last/after-last assignment is claimed. These 13 executable cases provide separate evidence. Gate membership is unchanged.

## 16. Related Features

[Customer text cleanup](customer-text-cleanup.md) distinguishes codepoints and supported grapheme clusters. [Invoice API URIs](invoice-api-uris.md) handles URI boundaries. [The audit](../COMPLETENESS-AUDIT.md) separates reference coverage from exhaustive collation/signature conformance.
