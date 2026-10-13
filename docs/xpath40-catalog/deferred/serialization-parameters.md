<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: serialization parameter boundaries and invoice JSON Lines">
  <br><br><h1>Serialization 4.0 parameters</h1>
  <p>Deferred additions — existing serialization and portable invoice export</p>
</div>

## 1. Name and Classification

**Serialization 4.0 parameter additions — Deferred, with a supported XSLT solidus control.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) remains; it must not imply that all serialization is missing. Bosak engine 1.1.2 reviewed against [revision 23218073](https://github.com/Fytala-Charles/Bosak/commit/232180733358fc4239229267763854fda0fa2791).

Primary references: [Serialization 4.0 parameters](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#serparam), [Serialization 3.1](https://www.w3.org/TR/xslt-xquery-serialization-31/). Immutable review source: [serialization XML at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-xquery-serialization-40/src/xslt-xquery-serialization.xml), section `serparam`. This is a review pin, not recovery of the missing historical adopted snapshot.

## 2. Intent

Choose an invoice interchange format with predictable record boundaries, URI escaping and output layout. Record parameter-by-parameter behavior instead of equating accepted options with implemented serialization.

## 3. Also Known As

Canonical output, JSON Lines, configurable indentation/line endings and CSV serialization controls. These are serialization parameters, distinct from XPath CSV parsing functions.

## 4. History / Supersedes

The pinned source adds escape-solidus (issue 530/PR 534), json-lines (1471/1497), canonical (938/2259), layout controls (1234/2719) and CSV controls (2748/2794). This disposition covers those nine parameter additions, not every change to Serialization 4.0: GNodes, CSV data-model shapes, numeric/JSON rules, HTML defaults and other revisions require separate reviews.

## 5. Motivation

A partner imports one JSON invoice per physical line. Invoice identifiers, currency-labelled totals and URIs must survive serialization. A pretty JSON array cannot substitute for JSON Lines. Canonical output has a different purpose—stable representation for signing/digesting—and must use a verified canonicalization implementation.

## 6. Language Alignment

| Path | Verified boundary |
|---|---|
| `fn:serialize` maps in XPath 3.1/4.0 | Nine new keys leave tested output unchanged; map-valued invalid options are tolerated |
| `fn:serialize` escape-solidus false | Still emits escaped solidus; tested default also escapes it |
| XSLT 3.0/4.0 output escape-solidus yes/no | Controls URI string escaping in the verified JSON outputs |
| `fn:serialize` json-lines true, two maps | `SERE0023` |
| XSLT json-lines yes, two strings | JSON array `["INV-1042","INV-1043"]`, not separate lines |
| `fn:serialize` method csv | Tested scalar sequence equals XML-family serialization; CSV is not established |
| XSLT method csv | `XTSE1570` |
| Explicit per-record serialization + LF joining | Portable invoice JSON Lines; empty and reordered cases verified |

Successful XPath 4.0 probes also assert the same default 3.1 outcome. XSLT error fixtures assert markers from compilation or capture; no detection-stage promise follows.

## 7. Applicability

Use existing XML/JSON/text serialization for ordinary exports. Build JSON Lines explicitly by serializing each record and joining the strings. Avoid these ignored options when a consumer requires canonical bytes, a custom layout or CSV. Do not assume that parser support for CSV delivers a CSV serializer.

## 8. Structure

The pinned nine-parameter boundary is:

| Parameter | Pinned draft contract | Bosak scope and evidence |
|---|---|---|
| [canonical](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#XML_CANONICAL) | XML/XHTML Canonical XML 1.1; JSON canonicalization has its own rules | No handling in inspected option/property parsers. XML examples retain z-before-a attributes and an empty-element tag, so are not canonical XML |
| [json-lines](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#id-json-lines) | Serialize top-level items as successive JSON records separated by line endings | No option handling; XPath error and XSLT array behavior differ |
| [escape-solidus](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#JSON_ESCAPE-SOLIDUS) | Boolean controls escaping `/` as `\/` in JSON strings; adaptive has a separate parameter section | XSLT JSON yes/no verified; fn option ignored. Adaptive behavior not certified |
| [indent-unit](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#XML_INDENT-UNIT) | Spaces or tabs (including literal `\t` escape), when indent is true | No handling. Current equality probes use default indent=false and do not prove an indented-layout mismatch; source inspection establishes missing control |
| [indent-attributes](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#XML_INDENT-ATTRIBUTES) | Boolean refinement when indent=true; draft permits optional attribute line breaks | No handling. Absence of line breaks alone would not prove a draft violation |
| [line-ending](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#XML_LINE-ENDING) | LF, CR or CRLF; `\n`/`\r` escape notation is recognized; applies after mapping/normalization | No handling. XSLT text with requested CRLF retains LF in the tested capture |
| [csv-separator](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#CSV_SEPARATOR) | Single separator character for CSV fields | No handling; CSV method boundary tested separately |
| [csv-quote-character](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#CSV_QUOTE-CHARACTER) | CSV quoting control | No handling; cannot infer support from ignored JSON/text options |
| [csv-header](https://qt4cg.org/specifications/xslt-xquery-serialization-40/Overview.html#CSV_HEADER) | Boolean controls writing the header from the supplied CSV structure | No handling; neither method path provides verified CSV output |

XSLT text tolerance probes for canonical/layout/CSV keys deliberately test acceptance only. They do not show those parameters working in applicable methods. XML canonical and newline-bearing text probes provide stronger observed boundaries. Canonicalization entails more than sorting attributes; the examples are counterexamples, not a substitute implementation.

## 9. Participants

The invoice batch is a JSON object with an invoices array. Each record carries ID, currency, a precomputed numeric total and a URI. The JSON serializer escapes data; string-join supplies the record separator. The manifest records exact outputs/errors and compatibility metadata.

## 10. Collaborations

The portable expression parses the complete batch, selects its invoice members, serializes each separately with method=json and joins them using codepoints-to-string(10). Empty selection yields an empty string. Reordering invoices changes line order while retaining values. There is no terminal newline in this example; add one explicitly if the receiver requires it.

## 11. Consequences

The portable output is valid JSON Lines with escaped URI solidi and exactly two lines for the main input. An escaped solidus decodes to the same URI. Exact map-key order is observed fixture output, not a general stable-order or canonicalization guarantee. fn CSV fallback and XSLT JSON sequence-as-array are separate existing behavior; this slice records them without engine changes.

## 12. Implementation

Source evidence at the reviewed revision: [XdmSerializer.cs](https://github.com/Fytala-Charles/Bosak/blob/232180733358fc4239229267763854fda0fa2791/src/Bosak.XPath.Standard/Functions/XdmSerializer.cs), `ApplyParameter` ignores unrecognized map keys, and `Serialize` routes methods other than json/adaptive through XML-family serialization. Its JSON path retains its default escaping independently of the ignored new key. [OutputProperties.cs](https://github.com/Fytala-Charles/Bosak/blob/232180733358fc4239229267763854fda0fa2791/src/Bosak.Xslt/Stylesheet/OutputProperties.cs) stores and parses EscapeSolidus but does not store the other additions. [ResultTreeSerializer.cs](https://github.com/Fytala-Charles/Bosak/blob/232180733358fc4239229267763854fda0fa2791/src/Bosak.Xslt/Runtime/ResultTreeSerializer.cs) forwards that flag to [XdmJsonSerializer.cs](https://github.com/Fytala-Charles/Bosak/blob/232180733358fc4239229267763854fda0fa2791/src/Bosak.XPath.Standard/Json/XdmJsonSerializer.cs), whose sequence path can serialize multiple items as an array.

Named existing evidence: `Capture_MethodFidelity_XmlHtmlTextJson` in [XsltCaptureTests.cs](https://github.com/Fytala-Charles/Bosak/blob/232180733358fc4239229267763854fda0fa2791/tests/Bosak.Xslt.Tests/Capture/XsltCaptureTests.cs), and `Output_Text_Method_Serializes_Only_Text` in [StylesheetTests.cs](https://github.com/Fytala-Charles/Bosak/blob/232180733358fc4239229267763854fda0fa2791/tests/Bosak.Xslt.Tests/StylesheetTests.cs). Those tests establish ordinary paths, not the nine additions. The catalog adds 52 named cases without changing engine/API/harness or promoting manual gates.

## 13. Sample Code

Complete partner invoice input (totals are already calculated; no tax or currency conversion occurs here):

```json
{"invoices":[{"id":"INV-1042","currency":"EUR","total":30,"uri":"https://partner.example/invoices/1042"},{"id":"INV-1043","currency":"USD","total":12,"uri":"https://partner.example/invoices/1043"}]}
```

Portable XPath 3.1 JSON Lines export:

```xpath
(: Complete invoice batch; serialize each record separately and join with LF. :)
let $batch := parse-json('{"invoices":[{"id":"INV-1042","currency":"EUR","total":30,"uri":"https://partner.example/invoices/1042"},{"id":"INV-1043","currency":"USD","total":12,"uri":"https://partner.example/invoices/1043"}]}')
return string-join($batch?invoices?* ! serialize(., map{'method':'json'}), codepoints-to-string(10))
```

Exact output, including one LF between the records and no terminal LF:

```text
{"id":"INV-1042","currency":"EUR","total":30,"uri":"https:\/\/partner.example\/invoices\/1042"}
{"id":"INV-1043","currency":"USD","total":12,"uri":"https:\/\/partner.example\/invoices\/1043"}
```

Empty input:

```json
{"invoices":[]}
```

The complete [empty expression](../examples/serialization-parameters/portable-json-lines-empty.xpath) returns the empty string. Reordered input:

```json
{"invoices":[{"id":"INV-1043","currency":"USD","total":12,"uri":"https://partner.example/invoices/1043"},{"id":"INV-1042","currency":"EUR","total":30,"uri":"https://partner.example/invoices/1042"}]}
```

The [complete reordered expression](../examples/serialization-parameters/portable-json-lines-reordered.xpath) returns:

```text
{"id":"INV-1043","currency":"USD","total":12,"uri":"https:\/\/partner.example\/invoices\/1043"}
{"id":"INV-1042","currency":"EUR","total":30,"uri":"https:\/\/partner.example\/invoices\/1042"}
```

Observed fn:serialize solidus control boundary:

```xpath
serialize(map{'uri':'https://partner.example/invoices/1042'}, map{'method':'json','escape-solidus':false()})
```

Exact result (false is ignored):

```text
{"uri":"https:\/\/partner.example\/invoices\/1042"}
```

Shared XML input for all XSLT probes:

```xml
<invoices><invoice id="INV-1042" currency="EUR" total="30" uri="https://partner.example/invoices/1042"/><invoice id="INV-1043" currency="USD" total="12" uri="https://partner.example/invoices/1043"/></invoices>
```

Supported XSLT 3.0 solidus alternative:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify serialization parameter handling for invoice interchange.
SPECIAL NOTES: In-memory string capture; not octet or canonicalization conformance.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: declared exact text or refusal marker.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
  <xsl:output method="json" escape-solidus="no"/>
  <xsl:template match="/">
    <xsl:sequence select="string(invoices/invoice[1]/@uri)"/>
  </xsl:template>
</xsl:stylesheet>
```

Exact output:

```text
"https://partner.example/invoices/1042"
```

Observed XSLT JSON Lines boundary:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify serialization parameter handling for invoice interchange.
SPECIAL NOTES: In-memory string capture; not octet or canonicalization conformance.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: declared exact text or refusal marker.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:output method="json" json-lines="yes"/>
  <xsl:template match="/">
    <xsl:sequence select="invoices/invoice/@id ! string(.)"/>
  </xsl:template>
</xsl:stylesheet>
```

Exact output is an array:

```text
["INV-1042","INV-1043"]
```

Observed XML canonical boundary:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify serialization parameter handling for invoice interchange.
SPECIAL NOTES: In-memory string capture; not octet or canonicalization conformance.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: declared exact text or refusal marker.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:output method="xml" omit-xml-declaration="yes" canonical="yes"/>
  <xsl:template match="/">
    <invoice z="30" a="EUR">
      <line/>
    </invoice>
  </xsl:template>
</xsl:stylesheet>
```

Exact output:

```text
<invoice z="30" a="EUR"><line/></invoice>
```

Observed requested CRLF text boundary:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify serialization parameter handling for invoice interchange.
SPECIAL NOTES: In-memory string capture; not octet or canonicalization conformance.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice XML. Output: declared exact text or refusal marker.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
  <xsl:output method="text" line-ending="\r\n"/>
  <xsl:template match="/">
    <xsl:value-of select="invoices/invoice/@id" separator="&#10;"/>
  </xsl:template>
</xsl:stylesheet>
```

Exact output contains LF, not CRLF:

```text
INV-1042
INV-1043
```

All fixtures below are linked in full. Outcomes shown as JSON string literals make empty strings, backslashes and newline characters unambiguous. Boolean `true` outcomes assert equality with the same payload serialized without that new option; they do not establish draft conformance. Map-valued invalid options test ignored validation. Text tolerance probes use non-applicable output methods, as scoped above.

| Fixture | Mode | Exact result or marker |
|---|---|---|
| [fn-canonical](../examples/serialization-parameters/fn-canonical.xpath) | 40 | `"true"` |
| [fn-invalid-canonical](../examples/serialization-parameters/fn-invalid-canonical.xpath) | 40 | `"true"` |
| [fn-json-lines](../examples/serialization-parameters/fn-json-lines.xpath) | 40 | `"true"` |
| [fn-invalid-json-lines](../examples/serialization-parameters/fn-invalid-json-lines.xpath) | 40 | `"true"` |
| [fn-escape-solidus](../examples/serialization-parameters/fn-escape-solidus.xpath) | 40 | `"true"` |
| [fn-invalid-escape-solidus](../examples/serialization-parameters/fn-invalid-escape-solidus.xpath) | 40 | `"true"` |
| [fn-indent-unit](../examples/serialization-parameters/fn-indent-unit.xpath) | 40 | `"true"` |
| [fn-invalid-indent-unit](../examples/serialization-parameters/fn-invalid-indent-unit.xpath) | 40 | `"true"` |
| [fn-indent-attributes](../examples/serialization-parameters/fn-indent-attributes.xpath) | 40 | `"true"` |
| [fn-invalid-indent-attributes](../examples/serialization-parameters/fn-invalid-indent-attributes.xpath) | 40 | `"true"` |
| [fn-line-ending](../examples/serialization-parameters/fn-line-ending.xpath) | 40 | `"true"` |
| [fn-invalid-line-ending](../examples/serialization-parameters/fn-invalid-line-ending.xpath) | 40 | `"true"` |
| [fn-csv-separator](../examples/serialization-parameters/fn-csv-separator.xpath) | 40 | `"true"` |
| [fn-invalid-csv-separator](../examples/serialization-parameters/fn-invalid-csv-separator.xpath) | 40 | `"true"` |
| [fn-csv-quote-character](../examples/serialization-parameters/fn-csv-quote-character.xpath) | 40 | `"true"` |
| [fn-invalid-csv-quote-character](../examples/serialization-parameters/fn-invalid-csv-quote-character.xpath) | 40 | `"true"` |
| [fn-csv-header](../examples/serialization-parameters/fn-csv-header.xpath) | 40 | `"true"` |
| [fn-invalid-csv-header](../examples/serialization-parameters/fn-invalid-csv-header.xpath) | 40 | `"true"` |
| [fn-solidus-main](../examples/serialization-parameters/fn-solidus-main.xpath) | 31 | `"{\"uri\":\"https:\\/\\/partner.example\\/invoices\\/1042\"}"` |
| [fn-json-lines-multiple](../examples/serialization-parameters/fn-json-lines-multiple.xpath) | 31 | `SERE0023` |
| [fn-csv-method](../examples/serialization-parameters/fn-csv-method.xpath) | 31 | `"true"` |
| [fn-canonical-main](../examples/serialization-parameters/fn-canonical-main.xpath) | 31 | `"<invoice z=\"30\" a=\"EUR\"><line/></invoice>"` |
| [portable-json-lines-input](../examples/serialization-parameters/portable-json-lines-input.xpath) | 31 | `"{\"id\":\"INV-1042\",\"currency\":\"EUR\",\"total\":30,\"uri\":\"https:\\/\\/partner.example\\/invoices\\/1042\"}\n{\"id\":\"INV-1043\",\"currency\":\"USD\",\"total\":12,\"uri\":\"https:\\/\\/partner.example\\/invoices\\/1043\"}"` |
| [portable-json-lines-empty](../examples/serialization-parameters/portable-json-lines-empty.xpath) | 31 | `""` |
| [portable-json-lines-reordered](../examples/serialization-parameters/portable-json-lines-reordered.xpath) | 31 | `"{\"id\":\"INV-1043\",\"currency\":\"USD\",\"total\":12,\"uri\":\"https:\\/\\/partner.example\\/invoices\\/1043\"}\n{\"id\":\"INV-1042\",\"currency\":\"EUR\",\"total\":30,\"uri\":\"https:\\/\\/partner.example\\/invoices\\/1042\"}"` |
| [xslt-solidus-yes-xslt31](../examples/serialization-parameters/xslt-solidus-yes-xslt31.xsl) | xslt31 | `"\"https:\\/\\/partner.example\\/invoices\\/1042\""` |
| [xslt-solidus-no-xslt31](../examples/serialization-parameters/xslt-solidus-no-xslt31.xsl) | xslt31 | `"\"https://partner.example/invoices/1042\""` |
| [xslt-canonical-xslt31](../examples/serialization-parameters/xslt-canonical-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-indent-unit-xslt31](../examples/serialization-parameters/xslt-indent-unit-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-indent-attributes-xslt31](../examples/serialization-parameters/xslt-indent-attributes-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-line-ending-xslt31](../examples/serialization-parameters/xslt-line-ending-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-csv-separator-xslt31](../examples/serialization-parameters/xslt-csv-separator-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-csv-quote-character-xslt31](../examples/serialization-parameters/xslt-csv-quote-character-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-csv-header-xslt31](../examples/serialization-parameters/xslt-csv-header-xslt31.xsl) | xslt31 | `"INV-1042|INV-1043"` |
| [xslt-json-lines-xslt31](../examples/serialization-parameters/xslt-json-lines-xslt31.xsl) | xslt31 | `"[\"INV-1042\",\"INV-1043\"]"` |
| [xslt-csv-xslt31](../examples/serialization-parameters/xslt-csv-xslt31.xsl) | xslt31 | `XTSE1570` |
| [xslt-solidus-yes-xslt40](../examples/serialization-parameters/xslt-solidus-yes-xslt40.xsl) | xslt40 | `"\"https:\\/\\/partner.example\\/invoices\\/1042\""` |
| [xslt-solidus-no-xslt40](../examples/serialization-parameters/xslt-solidus-no-xslt40.xsl) | xslt40 | `"\"https://partner.example/invoices/1042\""` |
| [xslt-canonical-xslt40](../examples/serialization-parameters/xslt-canonical-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-indent-unit-xslt40](../examples/serialization-parameters/xslt-indent-unit-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-indent-attributes-xslt40](../examples/serialization-parameters/xslt-indent-attributes-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-line-ending-xslt40](../examples/serialization-parameters/xslt-line-ending-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-csv-separator-xslt40](../examples/serialization-parameters/xslt-csv-separator-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-csv-quote-character-xslt40](../examples/serialization-parameters/xslt-csv-quote-character-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-csv-header-xslt40](../examples/serialization-parameters/xslt-csv-header-xslt40.xsl) | xslt40 | `"INV-1042|INV-1043"` |
| [xslt-json-lines-xslt40](../examples/serialization-parameters/xslt-json-lines-xslt40.xsl) | xslt40 | `"[\"INV-1042\",\"INV-1043\"]"` |
| [xslt-csv-xslt40](../examples/serialization-parameters/xslt-csv-xslt40.xsl) | xslt40 | `XTSE1570` |
| [fn-solidus-false](../examples/serialization-parameters/fn-solidus-false.xpath) | 31 | `"{\"uri\":\"https:\\/\\/partner.example\\/invoices\\/1042\"}"` |
| [xslt-canonical-xml-xslt31](../examples/serialization-parameters/xslt-canonical-xml-xslt31.xsl) | xslt31 | `"<invoice z=\"30\" a=\"EUR\"><line/></invoice>"` |
| [xslt-line-ending-text-xslt31](../examples/serialization-parameters/xslt-line-ending-text-xslt31.xsl) | xslt31 | `"INV-1042\nINV-1043"` |
| [xslt-canonical-xml-xslt40](../examples/serialization-parameters/xslt-canonical-xml-xslt40.xsl) | xslt40 | `"<invoice z=\"30\" a=\"EUR\"><line/></invoice>"` |
| [xslt-line-ending-text-xslt40](../examples/serialization-parameters/xslt-line-ending-text-xslt40.xsl) | xslt40 | `"INV-1042\nINV-1043"` |


## 14. Performance

The portable expression materializes parsed input and one string per record before joining. These are in-memory string checks, not a streaming or bounded-memory export benchmark. The capture API returns text; octet encoding, BOM, canonical digest/signature correctness and transport framing need independent checks.

## 15. Known Uses

The partner import is a realistic invoice interchange pattern, not a claim of a named deployment. Canonical XML or JSON intended for cryptographic use requires a verified canonicalization component; none is supplied by this worked example.

## 16. Related Features

See [the function-composition disposition](fn-apply-templates.md), [the manifest](../examples/cases.json), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md). Existing serialization remains useful; the remaining draft controls are deferred.
