<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred apply-templates function and invoice export">
  <br><br><h1>fn:apply-templates</h1>
  <p>Deferred — instruction and stylesheet-function alternatives</p>
</div>

## 1. Name and Classification

**fn:apply-templates — Deferred.** The [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) remains in force. Bosak engine 1.1.2 reviewed against [revision 7758c568](https://github.com/Fytala-Charles/Bosak/commit/7758c568612a19781c36bb1ca30c01e973a6246a).

Primary documentation: [XSLT 4.0 function](https://qt4cg.org/specifications/xslt-40/Overview.html#func-apply-templates) and [XSLT 3.0 instruction](https://www.w3.org/TR/xslt-30/#element-apply-templates). Immutable review sources: [XSLT function catalog at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/function-catalog.xml), function `apply-templates`, and [xslt.xml at the same pin](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml), section `func-apply-templates`. This review pin does not recover the historical adopted snapshot.

## 2. Intent

Invoke template rules inside an expression, retaining their returned values for construction of a report, map or array.

## 3. Also Known As

Expression-based template dispatch. The function in the `fn` namespace is distinct from the supported `xsl:apply-templates` instruction and a user-defined stylesheet function.

## 4. History / Supersedes

The pinned draft records introduction under issues 2005/2038 and PR 2006 (May 2025). The function supplements the instruction; it does not replace existing template dispatch. Bosak retains the deferral.

## 5. Motivation

An ERP integration exports invoice totals while sharing template rules with other report builders. Expression composition is convenient when each invoice becomes a map or array member. The worked example keeps currency with each amount and does not sum across currencies or claim tax, discounts or currency conversion.

## 6. Language Alignment

| Form | Verified Bosak result |
|---|---|
| Standalone XPath calls, arities 1/2, modes 3.1, frozen 4.0 and experimental | `XPST0017`; standalone XPath also lacks the XSLT rule environment required by the draft |
| XSLT versions 3.0/4.0: arities 1/2 and `fn:apply-templates#1` | `XPST0017` |
| XPath `function-lookup`, arities 1/2, modes 3.1/4.0 | Both absent; count `0` |
| XSLT availability | Instruction `true`, function arities 1/2 `false` |
| Instruction and `f:export` wrapper | Verified with named mode, ordinary/tunnel parameters, empty/reordered input |

Error fixtures assert a reported marker from compilation or capture, without promising a detection stage.

## 7. Applicability

Use an explicit instruction today. Use a stylesheet-function wrapper when an expression needs the result sequence. This example fixes the export mode and parameter names; it does not implement dynamic mode/parameter-map dispatch.

## 8. Structure

The draft signature is `fn:apply-templates($select as item()*, $options as map(*)? := {}) as item()*`. It dispatches each selected item and concatenates the rule results in selection order. The options are `params` and `tunnel-params` (QName-keyed maps), and `mode` (QName, `#current` or `#unnamed`, default `#unnamed`). The pinned draft requires a declared/accepted public mode and specifies `XTDE0565` for unavailable/private modes.

These draft contracts are reference requirements, not verified Bosak function behavior. The one-argument prose equates the call with an instruction omitting mode, while the options table defaults to `#unnamed`; reconcile this against the implementation target before adding support. The alternatives use an explicit named mode.

## 9. Participants

Invoice nodes provide ID, currency, quantities and decimal prices. The export rule calculates a currency-labelled amount. The caller supplies an ordinary label and a tunnel batch identifier. The wrapper returns strings; the output step joins them explicitly.

## 10. Collaborations

The wrapper accepts nodes and values explicitly, executes the instruction in `f:export`, passes `label` and tunnel `batch`, and returns the atomic sequence. The caller uses `string-join` rather than relying on serialization spacing.

## 11. Consequences

The wrapper preserves a useful composition path with existing XSLT support. It is not an implementation of the draft function: dynamic options, public-mode validation, package acceptance and function-specific errors remain deferred. Empty input returns no rows; reordered input retains invoice order. Invoice line order does not change the decimal sum.

## 12. Implementation

Bosak [FunctionLibrary.cs](https://github.com/Fytala-Charles/Bosak/blob/7758c568612a19781c36bb1ca30c01e973a6246a/src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs) mentions `apply-templates` in the instruction-availability list, not a registration of this function. [TransformEngine.cs](https://github.com/Fytala-Charles/Bosak/blob/7758c568612a19781c36bb1ca30c01e973a6246a/src/Bosak.Xslt/Runtime/TransformEngine.cs) implements instruction dispatch and function-body sequence construction. Executable refusals and availability probes are the evidence for missing function support.

Existing named regression evidence in [StylesheetTests.cs](https://github.com/Fytala-Charles/Bosak/blob/7758c568612a19781c36bb1ca30c01e973a6246a/tests/Bosak.Xslt.Tests/StylesheetTests.cs): `Function_ApplyTemplates_Inside_Function_Matches_Name_Pattern`, `Function_ApplyTemplates_Returns_Atomic_Sequence`, `Function_ApplyTemplates_Inside_Function_Matches_Variable_Reference_Pattern`. The catalog adds 28 manifest cases; it makes no engine change or gate promotion.

## 13. Sample Code

The source represents two invoices in different currencies. INV-1042 totals `2 × 12.50 + 1 × 5 = 30.00 EUR`; INV-1043 totals `3 × 4 = 12.00 USD`.

```xml
<invoices><invoice id="INV-1042" currency="EUR"><line sku="FILTER" quantity="2" price="12.50"/><line sku="BLUE" quantity="1" price="5"/></invoice><invoice id="INV-1043" currency="USD"><line sku="SEAL" quantity="3" price="4"/></invoice></invoices>
```

Refused XSLT 4.0 expression (result `XPST0017`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify invoice template dispatch and the deferred fn:apply-templates boundary.
SPECIAL NOTES: In-memory catalog example; no streaming or draft function conformance claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice batch. Output: exact text report or declared refusal marker.
Empty input returns an empty report; reordered input retains selection order.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:invoice" version="4.0" exclude-result-prefixes="xs f">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:sequence xmlns:fn="http://www.w3.org/2005/xpath-functions" select="fn:apply-templates(invoices/invoice, map{'mode': QName('urn:invoice','export')})"/>
  </xsl:template>
</xsl:stylesheet>
```

Portable XSLT 3.0 instruction alternative:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify invoice template dispatch and the deferred fn:apply-templates boundary.
SPECIAL NOTES: In-memory catalog example; no streaming or draft function conformance claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice batch. Output: exact text report or declared refusal marker.
Empty input returns an empty report; reordered input retains selection order.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:invoice" version="3.0" exclude-result-prefixes="xs f">
  <xsl:output method="text"/>
  <!-- Explicit export mode keeps dispatch independent of the caller's mode. -->
  <xsl:mode name="f:export"/>
  <xsl:template match="invoice" mode="f:export">
    <xsl:param name="label" as="xs:string"/>
    <xsl:param name="batch" as="xs:string" tunnel="yes"/>
    <xsl:sequence select="concat($batch, '/', $label, '/', @id, '/', @currency, ':', format-number(sum(line ! (xs:decimal(@quantity) * xs:decimal(@price))), '0.00'), ';')"/>
  </xsl:template>
  <xsl:template match="/">
    <xsl:variable name="rows" as="xs:string*">
      <xsl:apply-templates select="invoices/invoice" mode="f:export">
        <xsl:with-param name="label" select="'EXPORT'"/>
        <xsl:with-param name="batch" select="'BATCH-7'" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:value-of select="string-join($rows, '')"/>
  </xsl:template>
</xsl:stylesheet>
```

Portable stylesheet-function alternative, usable from XPath expressions inside the stylesheet:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify invoice template dispatch and the deferred fn:apply-templates boundary.
SPECIAL NOTES: In-memory catalog example; no streaming or draft function conformance claim.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: invoice batch. Output: exact text report or declared refusal marker.
Empty input returns an empty report; reordered input retains selection order.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:invoice" version="3.0" exclude-result-prefixes="xs f">
  <xsl:output method="text"/>
  <!-- Explicit export mode keeps dispatch independent of the caller's mode. -->
  <xsl:mode name="f:export"/>
  <xsl:template match="invoice" mode="f:export">
    <xsl:param name="label" as="xs:string"/>
    <xsl:param name="batch" as="xs:string" tunnel="yes"/>
    <xsl:sequence select="concat($batch, '/', $label, '/', @id, '/', @currency, ':', format-number(sum(line ! (xs:decimal(@quantity) * xs:decimal(@price))), '0.00'), ';')"/>
  </xsl:template>
  <!-- The wrapper is a user function, not registration of the draft fn function. -->
  <xsl:function name="f:export" as="xs:string*">
    <xsl:param name="nodes" as="element(invoice)*"/>
    <xsl:param name="label" as="xs:string"/>
    <xsl:param name="batch" as="xs:string"/>
    <xsl:apply-templates select="$nodes" mode="f:export">
      <xsl:with-param name="label" select="$label"/>
      <xsl:with-param name="batch" select="$batch" tunnel="yes"/>
    </xsl:apply-templates>
  </xsl:function>
  <xsl:template match="/">
    <xsl:value-of select="string-join(f:export(invoices/invoice, 'EXPORT', 'BATCH-7'), '')"/>
  </xsl:template>
</xsl:stylesheet>
```

Both alternatives return exactly:

```text
BATCH-7/EXPORT/INV-1042/EUR:30.00;BATCH-7/EXPORT/INV-1043/USD:12.00;
```

Empty input (output is the empty string):

```xml
<invoices/>
```

Reordered invoices and lines:

```xml
<invoices><invoice id="INV-1043" currency="USD"><line sku="SEAL" quantity="3" price="4"/></invoice><invoice id="INV-1042" currency="EUR"><line sku="BLUE" quantity="1" price="5"/><line sku="FILTER" quantity="2" price="12.50"/></invoice></invoices>
```

Exact reordered output:

```text
BATCH-7/EXPORT/INV-1043/USD:12.00;BATCH-7/EXPORT/INV-1042/EUR:30.00;
```

All executable fixtures are listed below. XPath refusal probes deliberately use empty selections; their outcomes establish name/arity absence rather than invoice processing. XSLT probes and alternatives use the linked complete XML inputs. The two 4.0 lookup probes also verify the same 3.1 result.

| Fixture | Mode | Input | Exact output or error marker |
|---|---|---|---|
| [xpath-31-arity1](../examples/fn-apply-templates/xpath-31-arity1.xpath) | 31 | Expression | `XPST0017` |
| [xpath-31-arity2](../examples/fn-apply-templates/xpath-31-arity2.xpath) | 31 | Expression | `XPST0017` |
| [lookup-31](../examples/fn-apply-templates/lookup-31.xpath) | 31 | Expression | `0` |
| [xpath-40-arity1](../examples/fn-apply-templates/xpath-40-arity1.xpath) | 40 | Expression | `XPST0017` |
| [xpath-40-arity2](../examples/fn-apply-templates/xpath-40-arity2.xpath) | 40 | Expression | `XPST0017` |
| [lookup-40](../examples/fn-apply-templates/lookup-40.xpath) | 40 | Expression | `0` |
| [xpath-experimental-arity1](../examples/fn-apply-templates/xpath-experimental-arity1.xpath) | experimental | Expression | `XPST0017` |
| [xpath-experimental-arity2](../examples/fn-apply-templates/xpath-experimental-arity2.xpath) | experimental | Expression | `XPST0017` |
| [call1-xslt31](../examples/fn-apply-templates/call1-xslt31.xsl) | xslt31 | [XML](../examples/fn-apply-templates/input.xml) | `XPST0017` |
| [call2-xslt31](../examples/fn-apply-templates/call2-xslt31.xsl) | xslt31 | [XML](../examples/fn-apply-templates/input.xml) | `XPST0017` |
| [reference-xslt31](../examples/fn-apply-templates/reference-xslt31.xsl) | xslt31 | [XML](../examples/fn-apply-templates/input.xml) | `XPST0017` |
| [availability-xslt31](../examples/fn-apply-templates/availability-xslt31.xsl) | xslt31 | [XML](../examples/fn-apply-templates/input.xml) | `true|false|false` |
| [call1-xslt40](../examples/fn-apply-templates/call1-xslt40.xsl) | xslt40 | [XML](../examples/fn-apply-templates/input.xml) | `XPST0017` |
| [call2-xslt40](../examples/fn-apply-templates/call2-xslt40.xsl) | xslt40 | [XML](../examples/fn-apply-templates/input.xml) | `XPST0017` |
| [reference-xslt40](../examples/fn-apply-templates/reference-xslt40.xsl) | xslt40 | [XML](../examples/fn-apply-templates/input.xml) | `XPST0017` |
| [availability-xslt40](../examples/fn-apply-templates/availability-xslt40.xsl) | xslt40 | [XML](../examples/fn-apply-templates/input.xml) | `true|false|false` |
| [instruction-xslt31-input](../examples/fn-apply-templates/instruction-xslt31-input.xsl) | xslt31 | [XML](../examples/fn-apply-templates/input.xml) | `BATCH-7/EXPORT/INV-1042/EUR:30.00;BATCH-7/EXPORT/INV-1043/USD:12.00;` |
| [instruction-xslt31-empty](../examples/fn-apply-templates/instruction-xslt31-empty.xsl) | xslt31 | [XML](../examples/fn-apply-templates/empty.xml) | `(empty string)` |
| [instruction-xslt31-reordered](../examples/fn-apply-templates/instruction-xslt31-reordered.xsl) | xslt31 | [XML](../examples/fn-apply-templates/reordered.xml) | `BATCH-7/EXPORT/INV-1043/USD:12.00;BATCH-7/EXPORT/INV-1042/EUR:30.00;` |
| [wrapper-xslt31-input](../examples/fn-apply-templates/wrapper-xslt31-input.xsl) | xslt31 | [XML](../examples/fn-apply-templates/input.xml) | `BATCH-7/EXPORT/INV-1042/EUR:30.00;BATCH-7/EXPORT/INV-1043/USD:12.00;` |
| [wrapper-xslt31-empty](../examples/fn-apply-templates/wrapper-xslt31-empty.xsl) | xslt31 | [XML](../examples/fn-apply-templates/empty.xml) | `(empty string)` |
| [wrapper-xslt31-reordered](../examples/fn-apply-templates/wrapper-xslt31-reordered.xsl) | xslt31 | [XML](../examples/fn-apply-templates/reordered.xml) | `BATCH-7/EXPORT/INV-1043/USD:12.00;BATCH-7/EXPORT/INV-1042/EUR:30.00;` |
| [instruction-xslt40-input](../examples/fn-apply-templates/instruction-xslt40-input.xsl) | xslt40 | [XML](../examples/fn-apply-templates/input.xml) | `BATCH-7/EXPORT/INV-1042/EUR:30.00;BATCH-7/EXPORT/INV-1043/USD:12.00;` |
| [instruction-xslt40-empty](../examples/fn-apply-templates/instruction-xslt40-empty.xsl) | xslt40 | [XML](../examples/fn-apply-templates/empty.xml) | `(empty string)` |
| [instruction-xslt40-reordered](../examples/fn-apply-templates/instruction-xslt40-reordered.xsl) | xslt40 | [XML](../examples/fn-apply-templates/reordered.xml) | `BATCH-7/EXPORT/INV-1043/USD:12.00;BATCH-7/EXPORT/INV-1042/EUR:30.00;` |
| [wrapper-xslt40-input](../examples/fn-apply-templates/wrapper-xslt40-input.xsl) | xslt40 | [XML](../examples/fn-apply-templates/input.xml) | `BATCH-7/EXPORT/INV-1042/EUR:30.00;BATCH-7/EXPORT/INV-1043/USD:12.00;` |
| [wrapper-xslt40-empty](../examples/fn-apply-templates/wrapper-xslt40-empty.xsl) | xslt40 | [XML](../examples/fn-apply-templates/empty.xml) | `(empty string)` |
| [wrapper-xslt40-reordered](../examples/fn-apply-templates/wrapper-xslt40-reordered.xsl) | xslt40 | [XML](../examples/fn-apply-templates/reordered.xml) | `BATCH-7/EXPORT/INV-1043/USD:12.00;BATCH-7/EXPORT/INV-1042/EUR:30.00;` |


## 14. Performance

The examples use in-memory capture. Both alternatives visit each selected invoice and sum its lines. No timing, streaming, bounded-memory or optimizer-equivalence claim is made. Materialize only the returned scalar rows needed by the consumer.

## 15. Known Uses

The executable invoice export is a realistic ERP reporting pattern, not evidence of a named production deployment. Other uses include applying normalization rules before building JSON records; those constructions need their own fixtures.

## 16. Related Features

See [enclosing modes](enclosing-modes.md), [capturing accumulators](capturing-accumulators.md), [the examples manifest](../examples/cases.json) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md). These entries keep ordinary dispatch, deferred enclosure/capture and function composition separate.
