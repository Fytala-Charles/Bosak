# Supplier CSV import: complete comparison task

Run these files through `CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome`; all99 `csv-contract-*` IDs are in [cases.json](../cases.json). XPath files are complete expressions in XPath40, with default3.1 refusal checked for successful cases. XSLT examples use public compiler paths and the existing Legacy host; root version4.0 selects frozen4.0 XPath. No runner metadata or global resource setup is added.

## Inputs, paths and output

Each named fixture has a UTF-8 `.csv` and matching already-parsed `.xml` file. Every scenario has four self-contained stylesheets: `report-before-NAME.xsl` (portable3.0 over the XML fixture), `report-parse-NAME.xsl`, `report-xml-NAME.xsl`, `report-doc-NAME.xsl` (root4.0). Use the matching `NAME.xml` as the initial document. The CSV paths resolve `NAME.csv` beside the stylesheet via `static-base-uri()`; use the actual stylesheet file URI as the compilation base. All inputs and names are synthetic; no external network input is used.

| Fixture | Logical rows | Exact result in all four paths |
|---|---|---|
| normal | quoted SKU `A,BLUE` qty2; B qty3 | `A,BLUE=2|B=3|lines=2;total=5` |
| empty | header only; no data rows | `lines=0;total=0` |
| reordered | B qty3; quoted SKU A,BLUE qty2 | `B=3|A,BLUE=2|lines=2;total=5` |
| duplicate | A qty2; A qty3 | `A=2|A=3|lines=2;total=5` |
| invalid | A qty `oops` | FORG0001 from business `xs:integer` conversion |

The CSV variants use `header:true()` and return string fields. They do not merge repeated SKUs or silently invent missing quantities. The baseline receives host-parsed XML; it does not parse CSV using tokenize. `report-parse-malformed.xsl`, `report-xml-malformed.xsl` and `report-doc-malformed.xsl` use [malformed.csv](malformed.csv) and refuse FOCV0001. No host-parser baseline is claimed for malformed lexical CSV.

## Focused checks and boundaries

The76 focused expressions cover omitted/empty options, empty input versus empty/blank rows, header policies, separators/quotes/comments, whitespace, ragged rows, column reorder/repetition/padding, selection-before-trimming, keyword calls and malformed/invalid input. Width-prefixed summaries distinguish a zero-member blank row from a row containing an empty field. The three `observed-*` checks retain published1.23.8 behavior that differs from the pinned current draft; they do not certify that draft. See [the contract review](../../CSV-CONTRACT-REVIEW.md) for option defaults, the frozen negative-index test conflict and scope exclusions.

Local resource API controls live in [CsvResourceContractTests](../../../../tests/Bosak.XPath.Standard.Tests/CsvResourceContractTests.cs); they additionally cover explicit relative/absolute base URIs, a UTF-8 BOM and a missing local file. No public API, engine, version, conformance fixture or gate membership is changed.
