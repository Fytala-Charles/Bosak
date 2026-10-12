<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred computed XPath node constructors">
  <br><br><h1>Computed XPath node constructors</h1>
  <p>Deferred in Bosak XPath — use the appropriate XML construction host</p>
</div>

## 1. Name and Classification

**Computed XPath node constructors — Deferred.** [REQ-118 tier-2 decision](../../REQ-118-xpath-xslt-40.md) records the original deferral: February-2026 draft movement and overlap with existing XQuery construction.
Bosak source reviewed 2026-10-12 at [revision 28d8dccc](https://github.com/Fytala-Charles/Bosak/commit/28d8dccc74ab35e27db92584a7bc3e1a202b7cb5) (URI documentation delivery; engine version 1.1.2). This is a new disposition record, not a constructor implementation or tier promotion.

External reference: [XPath 4.0 node constructors](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-constructors). Immutable review source: [QT4CG expressions.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xquery-40/src/expressions.xml), IDs `id-constructors` and `id-computedConstructors`.
The source records addition to XPath on 2026-02-07 (issue 2427 / PR 2446), and name-coercion changes on 2026-05-23. This current review pin does not recover the unavailable historical adopted snapshot. The draft includes the feature; Bosak retains its deferral.

## 2. Intent

Create an ERP receipt from a supplier invoice, computing element/attribute content from the source. Keep XPath evaluation separate from the host that constructs XML nodes.

## 3. Also Known As

Computed node construction; element, attribute, document, text, comment, processing-instruction and namespace constructors. These are syntax productions, distinct from `xs:*` atomic constructor functions and map/array constructors.

## 4. History / Supersedes

XQuery 3.1 already has [computed constructors](https://www.w3.org/TR/xquery-31/#id-computedConstructors). The reviewed XPath 4.0 draft adds them to XPath. Bosak's shared parser already supports XQuery construction, but its `_allowFullFlwor` host flag controls recognition independently of XPath 4.0 compatibility. The historical deferral has not been superseded by enabling the 4.0 flag.

## 5. Motivation

A receipt export needs real XML node construction, escaping and attribute ordering. A successful XQuery constructor test does not establish that the same source compiles through `XPath31Expression`. The complete XSLT alternative below constructs the receipt without manually escaping source strings.

## 6. Language Alignment

| Host / mode | Verified boundary |
|---|---|
| XPath31, XPath40, XPath40Experimental | Seven computed node-kind forms and direct XML element syntax refuse with `XPST0003` |
| XQuery 3.1 | Existing computed-constructor tests exercise all seven kinds and selected dynamic errors |
| XSLT 3.0 | Node instructions/literal result elements construct the complete receipt |

The pinned draft requires named XPath constructors to use a QName literal such as `element #receipt { ... }`, or a name expression such as `element { 'receipt' } { ... }`. Bare names in earlier XQuery syntax, such as `element receipt { ... }`, are not the reviewed XPath grammar. Refusal fixtures use braced names valid in that draft. Direct `<receipt/>` syntax remains a separate XQuery-only alternative, not a promised XPath 4.0 form. No full XQuery 4.0 grammar claim follows from the 3.1 tests.

## 7. Applicability

Use the disposition when selecting an XML export architecture. Use XSLT for document transformation, or the XQuery API for existing XQuery construction contracts. XPath can select source values and construct maps/arrays; that does not create XML nodes. `parse-xml` parses existing XML text, and is not a substitute for arbitrary safe node construction by string concatenation.

## 8. Structure

Invoice XML → select customer/amount → XSLT element/attribute/value instructions → receipt XML. The refused XPath expression shows the same intended projection, but produces no output in Bosak.

## 9. Participants

`XPath31Expression` is the refusing host. The shared [XPathParser](../../../src/Bosak.XPath.Parser/Ast/XPathParser.cs) recognizes constructors only under `_allowFullFlwor`. [XQueryParser](../../../src/Bosak.XQuery/Parser/XQueryParser.cs) supplies that host flag. [XQueryCompiler](../../../src/Bosak.XQuery/Api/XQueryCompiler.cs) is the supported query entry point; XSLT supplies the transformation alternative.

## 10. Collaborations

`parse-xml`, `string` and `concat` support the 3.1 text report. XSLT `xsl:element`, `xsl:attribute`, literal result elements and `xsl:value-of` produce XML. Attribute instructions execute before child content. The serializer handles the ampersand in `Ada & Co`.

## 11. Consequences

Opting into XPath40Experimental does not unlock this feature. A consumer must choose a construction host; do not retry the same XPath expression with a higher mode. The 3.1 report preserves business values but is a text report, not equivalent XML output. The example projects supplied strings without validating money/currency. Empty customer content yields an empty element; the exact tested serializer spelling is `<customer/>`.

## 12. Implementation

Parser evidence: `Parse` sets `allowConstructors: allowFullFlwor` in the lexer; `ParsePrimaryExpr` and the axis-step recognition guard `IsComputedConstructorForm` with `_allowFullFlwor`. `xpath40` alone does not satisfy it. XQueryParser calls the shared parser with `allowFullFlwor: true`. No parser/runtime changes or conformance promotions are part of this record.

## 13. Sample Code

Complete [invoice input](../examples/computed-xpath-constructors/input.xml):

```xml
<invoice id="INV-2048"><customer>Ada &amp; Co</customer><amount currency="EUR">125.50</amount></invoice>
```

Complete [refused XPath 4.0 expression](../examples/computed-xpath-constructors/main-refusal.xpath):

```xpath
let $i := parse-xml('<invoice id="INV-2048"><customer>Ada &amp; Co</customer><amount currency="EUR">125.50</amount></invoice>')/invoice
return element { 'receipt' } { attribute { 'id' } { string($i/@id) }, element { 'customer' } { string($i/customer) }, element { 'total' } { attribute { 'currency' } { string($i/amount/@currency) }, string($i/amount) } }
```

Observed outcome: `XPST0003`; no constructed receipt is returned.

### XPath 3.1 report alternative

Complete [text-report fixture](../examples/computed-xpath-constructors/baseline.xpath):

```xpath
let $i := parse-xml('<invoice id="INV-2048"><customer>Ada &amp; Co</customer><amount currency="EUR">125.50</amount></invoice>')/invoice return concat('id=', $i/@id, '; customer=', $i/customer, '; total=', $i/amount, ' ', $i/amount/@currency)
```

Output:

```text
id=INV-2048; customer=Ada & Co; total=125.50 EUR
```

### Complete XSLT 3.0 construction alternative

The [stylesheet](../examples/computed-xpath-constructors/xslt-main.xsl) consumes the input above, using `/invoice` as its template context. It documents its projection contract and empty-content boundary inside the file.

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Project a supplier invoice into an ERP receipt using portable XSLT 3.0.
SPECIAL NOTES: Alternative to deferred computed XPath node constructors; no external I/O.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation

Input contract: invoice/@id, customer text, amount text and amount/@currency.
The stylesheet projects supplied strings; it does not validate currency or totals.
XSLT node construction escapes XML characters through the serializer.
Empty customer content creates an empty customer element. Attribute instructions
precede child nodes. No computed XPath constructor expression is required.
-->
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" omit-xml-declaration="yes" indent="no"/>
  <xsl:template match="/invoice">
    <xsl:element name="receipt">
      <xsl:attribute name="id" select="string(@id)"/>
      <customer><xsl:value-of select="customer"/></customer>
      <total><xsl:attribute name="currency" select="string(amount/@currency)"/><xsl:value-of select="amount"/></total>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
```

Principal output (no secondary output):

```xml
<receipt id="INV-2048"><customer>Ada &amp; Co</customer><total currency="EUR">125.50</total></receipt>
```

Complete [empty-customer input](../examples/computed-xpath-constructors/empty.xml):

```xml
<invoice id="INV-EMPTY"><customer/><amount currency="EUR">0.00</amount></invoice>
```

Output:

```xml
<receipt id="INV-EMPTY"><customer/><total currency="EUR">0.00</total></receipt>
```

### Existing XQuery evidence

The existing `XQuery_ComputedElement_ComputedNameAndAtomicContent` test evaluates:

```xquery
(name(element { "dyn" } { 1, 2, 3 }), string(element { "dyn" } { 1, 2, 3 }))
```

It asserts two string items, `dyn` and `1 2 3`, through `XQueryCompiler` and `XQueryContext`. This is named unit-test evidence, not a new XQuery manifest fixture or proof that XPath constructors work.

### Complete fixture inventory

The [manifest](../examples/cases.json) and [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) execute 30 new cases: 25 XPath refusals, three 3.1 reports and two XSLT outputs. Error markers assert the host boundary, not full draft constructor semantics.

| Fixture | Mode | Exact output/error marker |
|---|---|---|
| [element-31](../examples/computed-xpath-constructors/element-31.xpath) | 31 | `XPST0003` |
| [element-40](../examples/computed-xpath-constructors/element-40.xpath) | 40 | `XPST0003` |
| [element-experimental](../examples/computed-xpath-constructors/element-experimental.xpath) | experimental | `XPST0003` |
| [attribute-31](../examples/computed-xpath-constructors/attribute-31.xpath) | 31 | `XPST0003` |
| [attribute-40](../examples/computed-xpath-constructors/attribute-40.xpath) | 40 | `XPST0003` |
| [attribute-experimental](../examples/computed-xpath-constructors/attribute-experimental.xpath) | experimental | `XPST0003` |
| [document-31](../examples/computed-xpath-constructors/document-31.xpath) | 31 | `XPST0003` |
| [document-40](../examples/computed-xpath-constructors/document-40.xpath) | 40 | `XPST0003` |
| [document-experimental](../examples/computed-xpath-constructors/document-experimental.xpath) | experimental | `XPST0003` |
| [text-31](../examples/computed-xpath-constructors/text-31.xpath) | 31 | `XPST0003` |
| [text-40](../examples/computed-xpath-constructors/text-40.xpath) | 40 | `XPST0003` |
| [text-experimental](../examples/computed-xpath-constructors/text-experimental.xpath) | experimental | `XPST0003` |
| [comment-31](../examples/computed-xpath-constructors/comment-31.xpath) | 31 | `XPST0003` |
| [comment-40](../examples/computed-xpath-constructors/comment-40.xpath) | 40 | `XPST0003` |
| [comment-experimental](../examples/computed-xpath-constructors/comment-experimental.xpath) | experimental | `XPST0003` |
| [processing-instruction-31](../examples/computed-xpath-constructors/processing-instruction-31.xpath) | 31 | `XPST0003` |
| [processing-instruction-40](../examples/computed-xpath-constructors/processing-instruction-40.xpath) | 40 | `XPST0003` |
| [processing-instruction-experimental](../examples/computed-xpath-constructors/processing-instruction-experimental.xpath) | experimental | `XPST0003` |
| [namespace-31](../examples/computed-xpath-constructors/namespace-31.xpath) | 31 | `XPST0003` |
| [namespace-40](../examples/computed-xpath-constructors/namespace-40.xpath) | 40 | `XPST0003` |
| [namespace-experimental](../examples/computed-xpath-constructors/namespace-experimental.xpath) | experimental | `XPST0003` |
| [direct-element-31](../examples/computed-xpath-constructors/direct-element-31.xpath) | 31 | `XPST0003` |
| [direct-element-40](../examples/computed-xpath-constructors/direct-element-40.xpath) | 40 | `XPST0003` |
| [direct-element-experimental](../examples/computed-xpath-constructors/direct-element-experimental.xpath) | experimental | `XPST0003` |
| [main-refusal](../examples/computed-xpath-constructors/main-refusal.xpath) | 40 | `XPST0003` |
| [baseline](../examples/computed-xpath-constructors/baseline.xpath) | 31 | `id=INV-2048; customer=Ada & Co; total=125.50 EUR` |
| [baseline-empty](../examples/computed-xpath-constructors/baseline-empty.xpath) | 31 | `id=INV-EMPTY; customer=; total=0.00 EUR` |
| [baseline-escaped](../examples/computed-xpath-constructors/baseline-escaped.xpath) | 31 | `id=INV-SPECIAL; customer=A < B & C; total=7.00 EUR` |
| [xslt-main](../examples/computed-xpath-constructors/xslt-main.xsl) | 31 | `<receipt id="INV-2048"><customer>Ada &amp; Co</customer><total currency="EUR">125.50</total></receipt>` |
| [xslt-empty](../examples/computed-xpath-constructors/xslt-empty.xsl) | 31 | `<receipt id="INV-EMPTY"><customer/><total currency="EUR">0.00</total></receipt>` |

## 14. Performance

Refusal happens during XPath compilation. XSLT constructs and serializes an output tree; costs depend on the input and transformation. No benchmark or performance comparison with deferred XPath constructors is claimed.

## 15. Known Uses

The receipt projection is a synthetic ERP integration case. Existing [XQuery tests](../../../tests/Bosak.XQuery.Tests/PlaceholderTests.cs) include `XQuery_ComputedElement_StaticName`, `XQuery_ComputedElement_ComputedNameAndAtomicContent`, `XQuery_ComputedElement_EmptyContent`, `XQuery_ComputedAttribute_AddsToElement`, `XQuery_ComputedTextCommentAndPI`, `XQuery_ComputedDocument_WrapsContent`, `XQuery_ComputedNamespace_BecomesDeclaration`, `XQuery_ComputedComment_DoubleHyphen_Rejected`, `XQuery_ComputedPI_NestedTerminator_Rejected` and `XQuery_ComputedAttribute_AfterContent_Rejected`.
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) has no computed-node-constructor assignment in GatedSets. Its `misc-FocusConstructors` gate covers atomic focus constructor functions, a separate feature. Existing tests and refusal fixtures do not certify XPath 4.0 node construction.

## 16. Related Features

[Invoice-to-ERP worked chapter](../worked-examples/invoice-to-erp/README.md), [XPath switch disposition](../rejected/xpath-switch-expression.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md).
