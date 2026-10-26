<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: XQuery and streaming invoice scope">
  <br><br><h1>XQuery and streaming scope</h1>
  <p>Existing APIs — separate 4.0 language and module deltas</p>
</div>

**C09 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-1.md) records 4 contract groups with exact sample/context, applicable evidence and language-alignment limits. Status: **Limited host and streaming contract**. No XQuery4/full4.0 streaming conformance or general bounded-memory guarantee. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**XQuery and streaming language boundaries — Existing support; 4.0 deltas outside the original schedule.** Reviewed engine 1.1.2 at [revision 2184e920](https://github.com/Fytala-Charles/Bosak/commit/2184e9200394ca2e06f720bb9cdb95afd927aa51) on 2026-10-12. The [REQ-118 exclusion](../../REQ-118-xpath-xslt-40.md) does not mean that XQuery FLWOR or streaming input is absent.

Primary references: [XQuery 4.0 version declaration](https://qt4cg.org/specifications/xquery-40/xquery-40.html#id-version-declaration), [XQuery 3.1](https://www.w3.org/TR/xquery-31/), [XSLT Streaming 4.0](https://qt4cg.org/specifications/xslt-streaming-40/Overview.html), [XSLT 3.0 streaming](https://www.w3.org/TR/xslt-30/#streaming). Immutable review sources: [query-prolog.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xquery-40/src/query-prolog.xml), section `id-version-declaration`, and [streaming XML at the same pin](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-streaming-40/src/xslt-streaming.xml), sections `intro`, `initiating-streaming` and `streaming-non-xml`. The review pin does not recover missing historical adopted snapshots.

## 2. Intent

Select the correct query or transformation API for partner invoice processing and distinguish existing capabilities from a complete 4.0 language/module implementation.

## 3. Also Known As

XQuery FLWOR processing, XPath clause boundaries, burst-mode XML streaming and Streaming 4.0 module scope. A stylesheet version number or a few 4.0 functions do not certify a complete language version.

## 4. History / Supersedes

REQ-118 excluded XQuery 4.0-only changes and Streaming 4.0 deltas. Existing XQuery 3.1 and streamed XML transformations predate this review. REQ-087 Phase D is marked Done in the registry; its dated residual reports are historical evidence, not a newly verified current failure count or a reason to call all streaming absent. Any remaining 4.0 delta needs a separately defined implementation/conformance slice.

## 5. Motivation

An ERP report needs positive invoices sorted by currency and ID, which fits XQuery FLWOR. A large invoice feed can instead be processed in arrival order by a streamed transformation. Sorting the entire feed and reporting it in a forward pass have different resource contracts; this reference keeps those choices concrete.

## 6. Language Alignment

| Path | Verified behavior |
|---|---|
| XQueryCompiler, xquery version 3.1 | Full invoice where/order-by query succeeds |
| XQueryCompiler, xquery version 4.0 | `XQST0031` |
| Simple XPath for/where, modes 3.1/4.0/experimental | Bosak accepts the tested extension and returns `3|2` |
| Tested XPath order-by/group-by forms, same modes | `XPST0003` |
| Portable XPath 3.1 predicate + sort | Same sorted invoice rows for main/empty/reordered input |
| Ordinary XSLT capture, stylesheet 3.0/4.0 | Arrival-order invoice reports succeed |
| TransformStreamingToString, same stylesheets | Same exact reports through the actual streaming input API |

The XPath where acceptance corrects older blanket dossier wording. It is observed Bosak behavior, not a claim that XPath 3.1's standard grammar includes a full FLWOR. XQuery version rejection does not enumerate all 4.0 feature gaps.

## 7. Applicability

Use XQueryCompiler for full FLWOR, constructors, modules and the existing XQuery surface. Use XPath31Expression for expressions with declared compatibility. Choose TransformStreamingToString when the host supplies a stream and the transformation fits its provider constraints; use ordinary transforms for grounded input. Treat Streaming 4.0 delta planning as a separate effort.

## 8. Structure

The pinned XQuery version declaration identifies module intent; Bosak's parser accepts 1.0/3.0/3.1 and rejects the tested 4.0 declaration. Shared parsing/runtime code does not make both public APIs the same language.

The Streaming 4.0 module separates optional streaming from the main XSLT specification. Its pinned status text says streamability of all new 4.0 constructs is still work in progress. It includes XML/non-XML, streamability, maps/arrays and other contracts; a successful XML forward pass cannot certify them all.

Our two business outputs deliberately differ: the query filters zero totals and sorts; the streamed stylesheet emits all invoices in arrival order. The same-output comparison is between ordinary and streamed execution of the same stylesheet, not between these different business tasks.

## 9. Participants

XQueryCompiler/XQueryContext compile and evaluate the query. XPath31Expression handles the portable alternative and clause probes. XsltCompiler/XsltExecutable compile the stylesheet and execute ordinary or streamed input. The test manifest selects the API explicitly; the integration host owns the source stream.

## 10. Collaborations

The query parses the complete invoice XML, filters positive totals, orders by currency/ID and joins scalar rows. The portable XPath uses a predicate and sort key for this data contract. The stylesheet makes one report row per invoice. The harness runs it through capture and through File.OpenRead plus TransformStreamingToString, disposing the stream after use.

## 11. Consequences

The existing query and streaming APIs are useful without promising XQuery 4.0 or complete Streaming 4.0 conformance. The sort-key alternative assumes the shown simple currency/ID strings; concatenated keys are not a universal replacement for multi-key collation ordering. The streamed report retains output text and does not establish a total-memory bound. Generic-node, capturing-accumulator and other separately recorded gaps remain relevant to future streaming work.

## 12. Implementation

Reviewed source: [XQueryParser.cs](https://github.com/Fytala-Charles/Bosak/blob/2184e9200394ca2e06f720bb9cdb95afd927aa51/src/Bosak.XQuery/Parser/XQueryParser.cs) enforces supported version declarations; [XPathParser.cs](https://github.com/Fytala-Charles/Bosak/blob/2184e9200394ca2e06f720bb9cdb95afd927aa51/src/Bosak.XPath.Parser/Ast/XPathParser.cs) separates full-FLWOR branches while accepting the tested where clause. [XsltExecutable.cs](https://github.com/Fytala-Charles/Bosak/blob/2184e9200394ca2e06f720bb9cdb95afd927aa51/src/Bosak.Xslt/Api/XsltExecutable.cs) exposes actual streaming transforms, distinct from CaptureTransform.

Existing named tests: `XQuery_OrderBy_WithWhere` and `XQuery_GroupBy_AggregatesNonGroupingVariables` in [PlaceholderTests.cs](https://github.com/Fytala-Charles/Bosak/blob/2184e9200394ca2e06f720bb9cdb95afd927aa51/tests/Bosak.XQuery.Tests/PlaceholderTests.cs); `ApplyTemplates_Parity`, `LastFunction_OverStream_RaisesStreamingError`, `SecondPassOverStream_RaisesForwardOnlyError` and `BoundedMemoryOverLargeStream` in [StreamingTransformTests.cs](https://github.com/Fytala-Charles/Bosak/blob/2184e9200394ca2e06f720bb9cdb95afd927aa51/tests/Bosak.Xslt.Tests/StreamingTransformTests.cs). The bounded-memory test has its own 500,000-record/selective-output contract, not a guarantee for arbitrary output or sorting. Read [integration](../../INTEGRATION.md) and [REQ-087 registry](../../FEATURE_REQUESTS.md) for source/runtime constraints and dated history.

The catalog harness adds xquery rows and stream31/stream40 dispatch metadata; these select actual public APIs. It also accepts an explicit frozenExpected outcome for an experimental probe already accepted by frozen 4.0. This is test infrastructure, not engine or compatibility-policy change. No manual gate promotion or new sweep is claimed.

## 13. Sample Code

Complete invoice input; totals are precomputed and each amount retains its currency:

```xml
<invoices><invoice id="INV-1043" currency="USD" total="12"/><invoice id="INV-1042" currency="EUR" total="30"/><invoice id="INV-1044" currency="EUR" total="0"/></invoices>
```

Complete XQuery 3.1 query:

```xquery
(: XQuery 3.1: select positive invoices and sort by currency then identifier. :)
xquery version "3.1";
let $batch := parse-xml('<invoices><invoice id="INV-1043" currency="USD" total="12"/><invoice id="INV-1042" currency="EUR" total="30"/><invoice id="INV-1044" currency="EUR" total="0"/></invoices>')
return string-join((for $invoice in $batch/invoices/invoice
where xs:decimal($invoice/@total) gt 0
order by string($invoice/@currency), string($invoice/@id)
return concat($invoice/@id, '/', $invoice/@currency, ':', format-number(xs:decimal($invoice/@total), '0.00'))), ';')
```

Complete portable XPath 3.1 alternative:

```xpath
(: XPath 3.1: predicate filtering plus explicit sort key. :)
let $batch := parse-xml('<invoices><invoice id="INV-1043" currency="USD" total="12"/><invoice id="INV-1042" currency="EUR" total="30"/><invoice id="INV-1044" currency="EUR" total="0"/></invoices>')
return string-join(sort($batch/invoices/invoice[xs:decimal(@total) gt 0], (), function($invoice){concat($invoice/@currency, '/', $invoice/@id)}) ! concat(@id, '/', @currency, ':', format-number(xs:decimal(@total), '0.00')), ';')
```

Both sorted positive-invoice reports return exactly:

```text
INV-1042/EUR:30.00;INV-1043/USD:12.00
```

The version boundary is a separate complete query:

```xquery
xquery version "4.0"; 1
```

Result: `XQST0031`. Observed XPath where extension:

```xpath
string-join((for $i in (3,1,2) where $i gt 1 return $i) ! string(.), '|')
```

Result: `3|2`, also checked at 4.0 and experimental. Complete forward-report stylesheet:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify invoice processing through ordinary and streaming XSLT paths.
SPECIAL NOTES: Exact report parity does not certify Streaming 4.0 or memory bounds.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: one currency-labelled row per invoice, or the declared error.
Empty batches return empty text; reordered batches retain input order.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" version="3.0" exclude-result-prefixes="xs">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <!-- A forward pass reports every invoice; sorting is a separate buffered task. -->
    <xsl:for-each select="invoices/invoice">
      <xsl:value-of select="concat(@id, '/', @currency, ':', format-number(xs:decimal(@total), '0.00'), ';')"/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
```

Compile it with XsltCompiler and call TransformStreamingToString with a readable stream containing the XML above. The manifest also executes the same business stylesheet through ordinary capture. Exact forward output:

```text
INV-1043/USD:12.00;INV-1042/EUR:30.00;INV-1044/EUR:0.00;
```

Empty input (all report paths return empty text):

```xml
<invoices/>
```

Reordered input:

```xml
<invoices><invoice id="INV-1042" currency="EUR" total="30"/><invoice id="INV-1043" currency="USD" total="12"/><invoice id="INV-1044" currency="EUR" total="0"/></invoices>
```

The query/portable reports retain the same sorted output. Ordinary/streamed stylesheets preserve reordered arrival order:

```text
INV-1042/EUR:30.00;INV-1043/USD:12.00;INV-1044/EUR:0.00;
```

All fixtures are linked below. xquery31/xquery40 selects XQueryCompiler; stream31/stream40 selects TransformStreamingToString; xslt31/xslt40 selects CaptureTransform. The stylesheet version is contained in the file. Outcomes use JSON string notation, including empty values. Error markers may come from compilation/evaluation; no detection-stage guarantee is added.

| Fixture | API/mode | Exact output or marker |
|---|---|---|
| [xquery-main](../examples/language-streaming-scope/xquery-main.xq) | xquery/xquery31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [portable-main](../examples/language-streaming-scope/portable-main.xpath) | xpath/31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [xquery-empty](../examples/language-streaming-scope/xquery-empty.xq) | xquery/xquery31 | `""` |
| [portable-empty](../examples/language-streaming-scope/portable-empty.xpath) | xpath/31 | `""` |
| [xquery-reordered](../examples/language-streaming-scope/xquery-reordered.xq) | xquery/xquery31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [portable-reordered](../examples/language-streaming-scope/portable-reordered.xpath) | xpath/31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [xquery-version40](../examples/language-streaming-scope/xquery-version40.xq) | xquery/xquery40 | `XQST0031` |
| [xpath-where-31](../examples/language-streaming-scope/xpath-where-31.xpath) | xpath/31 | `"3|2"` |
| [xpath-where-40](../examples/language-streaming-scope/xpath-where-40.xpath) | xpath/40 | `"3|2"` |
| [xpath-where-experimental](../examples/language-streaming-scope/xpath-where-experimental.xpath) | xpath/experimental | `"3|2"` |
| [xpath-order-31](../examples/language-streaming-scope/xpath-order-31.xpath) | xpath/31 | `XPST0003` |
| [xpath-order-40](../examples/language-streaming-scope/xpath-order-40.xpath) | xpath/40 | `XPST0003` |
| [xpath-order-experimental](../examples/language-streaming-scope/xpath-order-experimental.xpath) | xpath/experimental | `XPST0003` |
| [xpath-group-31](../examples/language-streaming-scope/xpath-group-31.xpath) | xpath/31 | `XPST0003` |
| [xpath-group-40](../examples/language-streaming-scope/xpath-group-40.xpath) | xpath/40 | `XPST0003` |
| [xpath-group-experimental](../examples/language-streaming-scope/xpath-group-experimental.xpath) | xpath/experimental | `XPST0003` |
| [report-xslt31-main](../examples/language-streaming-scope/report-xslt31-main.xsl) | xslt/xslt31 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00;INV-1044/EUR:0.00;"` |
| [report-stream31-main](../examples/language-streaming-scope/report-stream31-main.xsl) | xslt/stream31 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00;INV-1044/EUR:0.00;"` |
| [report-xslt31-empty](../examples/language-streaming-scope/report-xslt31-empty.xsl) | xslt/xslt31 | `""` |
| [report-stream31-empty](../examples/language-streaming-scope/report-stream31-empty.xsl) | xslt/stream31 | `""` |
| [report-xslt31-reordered](../examples/language-streaming-scope/report-xslt31-reordered.xsl) | xslt/xslt31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00;INV-1044/EUR:0.00;"` |
| [report-stream31-reordered](../examples/language-streaming-scope/report-stream31-reordered.xsl) | xslt/stream31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00;INV-1044/EUR:0.00;"` |
| [report-xslt40-main](../examples/language-streaming-scope/report-xslt40-main.xsl) | xslt/xslt40 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00;INV-1044/EUR:0.00;"` |
| [report-stream40-main](../examples/language-streaming-scope/report-stream40-main.xsl) | xslt/stream40 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00;INV-1044/EUR:0.00;"` |
| [report-xslt40-empty](../examples/language-streaming-scope/report-xslt40-empty.xsl) | xslt/xslt40 | `""` |
| [report-stream40-empty](../examples/language-streaming-scope/report-stream40-empty.xsl) | xslt/stream40 | `""` |
| [report-xslt40-reordered](../examples/language-streaming-scope/report-xslt40-reordered.xsl) | xslt/xslt40 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00;INV-1044/EUR:0.00;"` |
| [report-stream40-reordered](../examples/language-streaming-scope/report-stream40-reordered.xsl) | xslt/stream40 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00;INV-1044/EUR:0.00;"` |


## 14. Performance

XQuery and portable XPath materialize parsed XML and sorting input. Streaming input is forward-oriented, but the returned report string grows with output. The catalog streaming branch still loads the small XML fixture for the ordinary-case harness setup; it is not a memory benchmark. Existing selective-output bounded-memory tests are separate evidence. Sorting/grouping or replay retention can ground data; do not infer document-size-independent memory from this report.

## 15. Known Uses

The invoice report is a realistic ERP task, not a named deployment. No complete language-conformance claim follows from the 28 new fixtures or from a version="4.0" stylesheet executing successfully.

## 16. Related Features

[JNode boundary](../data-model/jnode-template-boundary.md), [capturing accumulators](capturing-accumulators.md), [computed constructors](computed-node-constructors.md), [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md) and [integration](../../INTEGRATION.md).
