<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: deferred enclosing modes and invoice dispatch">
  <br><br><h1>Enclosing modes</h1>
  <p>Deferred — portable named-mode dispatch remains supported</p>
</div>

## 1. Name and Classification

**Enclosing modes — Deferred.** [REQ-118 tier-4 decision](../../REQ-118-xpath-xslt-40.md) records the deferral. This record documents the boundary without enabling nested declarations. Bosak baseline reviewed 2026-10-12: [revision d7cc935a](https://github.com/Fytala-Charles/Bosak/commit/d7cc935a3690c767fc17b01b90de4105a7140bed), engine 1.1.2.
External reference: [XSLT 4.0 enclosing modes](https://qt4cg.org/specifications/xslt-40/Overview.html#enclosing-modes). Immutable review source: [QT4CG xslt.xml at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml), section ID `enclosing-modes`. The source records issue 82 / PR 542 (2023-06-20). This review does not recover the historical adopted specification snapshot.

## 2. Intent

Keep invoice export rules together and ensure line processing uses the export rule set, while an audit pass uses a different set of rules.

## 3. Also Known As

Nested template-rule declarations within `xsl:mode`. This is not the same as an empty mode declaration, ordinary named modes, the dynamic current mode, or a lexical default mode.

## 4. History / Supersedes

The draft groups matched rules inside a named mode declaration and makes that mode the local default. It restricts contained templates and external applicable rules. Bosak retains its deferral; ordinary top-level named-mode rules already provide dispatch without that enclosure/isolation contract.

## 5. Motivation

An ERP export and an internal audit can process the same invoice nodes differently. A nested rule group would improve locality, but a consumer needs a supported way to retain the export mode when descending to invoice lines. The alternative makes the mode explicit and tests the consequences of omitting it.

## 6. Language Alignment

| Form | Verified Bosak boundary |
|---|---|
| `xsl:mode` containing templates, version 3.0 or 4.0 | `XTSE0260` |
| Empty named `xsl:mode` plus top-level matched rules | Supported in the complete 3.0 and 4.0 examples |
| Explicit recursive mode | Retains export dispatch |
| Recursive `mode="#current"` | Retains the dynamic active mode |
| Omitted recursive mode | Uses lexical default; selects the unnamed line rule in the test |
| Root lexical `default-mode` set to export | Omitted recursion uses export in the tested stylesheet |

The draft's invalid enclosing forms have specific static diagnostics (XTSE4005, XTSE4010 and XTSE4015, among others). Bosak's probes encounter the earlier generic empty-element restriction instead. These fixtures are refusal evidence, not implementation of the draft's diagnostic matrix.

## 7. Applicability

Use ordinary modes for separate export/audit passes. Keep mode declarations empty and matched rules at top level. Ordinary modes do not enforce that all applicable rules are defined in one enclosed group; imports, packages, precedence and additional rules need separate review.

## 8. Structure

The unnamed entry rule applies invoice nodes in `erp:export`. The invoice rule emits id/currency, applies lines in export mode, then emits a separator. The line rule emits each SKU. A separate audit rule reports line counts. The two prefixes used in the namespace probe identify the same expanded mode name.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) requires `xsl:mode` to be empty through `EmptyXsltElementNames`, and collects top-level template rules with `root.Elements(...)`. [TransformEngine](../../../src/Bosak.Xslt/Runtime/TransformEngine.cs) maintains the active mode stack. [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) executes exact stylesheets and asserts output or error markers.

## 10. Collaborations

`xsl:apply-templates`, named modes, `#current`, lexical `default-mode`, `count` and `reverse` support the complete report. Selection order and line order are preserved in the fixtures. No external I/O or secondary outputs are required.

## 11. Consequences

Raising the stylesheet version does not make enclosing modes work in Bosak. Moving templates to top level is a practical dispatch alternative, not equivalent rule isolation. `#current` is dynamic; an omitted mode is lexical, so they are not interchangeable. The deliberate unnamed line rule exposes this distinction through `wrong/` output. Input IDs, currencies and SKUs are projected strings; this report is not a data validator.

## 12. Implementation

The must-be-empty validation raises `XTSE0260` before nested rule collection or draft-specific enclosure checks. The nine refusal fixtures cover the ordinary enclosing shape, unnamed declaration, named/mode-bearing/missing-match child, conflicting default, external applicable rule and duplicate enclosure. They do not certify unused nested bodies or package override behavior. No compiler/runtime changes or gate promotions are included.

## 13. Sample Code

Complete synthetic [batch input](../examples/enclosing-modes/input.xml):

```xml
<batch><invoice id="INV-2048" currency="EUR"><line sku="P100"/><line sku="P200"/></invoice><invoice id="INV-2049" currency="USD"><line sku="P300"/></invoice></batch>
```

Complete [refused 4.0 stylesheet](../examples/enclosing-modes/main-refusal4.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred enclosing modes and portable invoice export/audit dispatch.
SPECIAL NOTES: Named top-level rules are the supported alternative; no external I/O.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice attributes and ordered line/@sku values. Output: text report.
Invoice and line rules share the export mode. Explicit mode or #current retains
export dispatch during recursion; omitted mode uses the lexical default instead.
These projections do not validate invoice data or enforce enclosing-mode isolation.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:erp="urn:example:erp" xmlns:q="urn:example:erp" version="4.0">
  <xsl:output method="text"/>
  <xsl:mode name="erp:export">
    <xsl:template match="invoice">
      <xsl:value-of select="concat(@id, ':', @currency, ':')"/>
      <xsl:apply-templates select="line"/>
      <xsl:text>;</xsl:text>
    </xsl:template>
    <xsl:template match="line">
      <xsl:value-of select="concat(@sku, '/')"/>
    </xsl:template>
  </xsl:mode>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice" mode="erp:export"/>
  </xsl:template>
</xsl:stylesheet>
```

Observed outcome: `XTSE0260`. No export result is asserted for this failing case.

### Complete portable XSLT 3.0 alternative

[Top-level named rules](../examples/enclosing-modes/portable3.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred enclosing modes and portable invoice export/audit dispatch.
SPECIAL NOTES: Named top-level rules are the supported alternative; no external I/O.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice attributes and ordered line/@sku values. Output: text report.
Invoice and line rules share the export mode. Explicit mode or #current retains
export dispatch during recursion; omitted mode uses the lexical default instead.
These projections do not validate invoice data or enforce enclosing-mode isolation.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:erp="urn:example:erp" xmlns:q="urn:example:erp" version="3.0">
  <xsl:output method="text"/>
  <xsl:mode name="erp:export"/>
  <xsl:template match="invoice" mode="erp:export">
    <xsl:value-of select="concat(@id, ':', @currency, ':')"/>
    <xsl:apply-templates select="line" mode="erp:export"/>
    <xsl:text>;</xsl:text>
  </xsl:template>
  <xsl:template match="line" mode="erp:export">
    <xsl:value-of select="concat(@sku, '/')"/>
  </xsl:template>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice" mode="erp:export"/>
  </xsl:template>
</xsl:stylesheet>
```

Principal text output:

```text
INV-2048:EUR:P100/P200/;INV-2049:USD:P300/;
```

The [version 4.0 comparison](../examples/enclosing-modes/portable4.xsl) returns the same report. The [current-mode fixture](../examples/enclosing-modes/current-mode.xsl) uses `#current` for line recursion and returns the same report despite a competing unnamed line rule.

### Omitted recursion mode boundary

Complete [omitted-mode fixture](../examples/enclosing-modes/omitted-recursion-mode.xsl):

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
AUTHOR: Charles Korthout
CREATE DATE: 12 October 2026
PURPOSE: Verify deferred enclosing modes and portable invoice export/audit dispatch.
SPECIAL NOTES: Named top-level rules are the supported alternative; no external I/O.
COPYRIGHT: Fytala
LICENSE: license.md (Apache-2.0)
SPDX-License-Identifier: Apache-2.0
Change History: Charles Korthout | 0.1 | 12-10-2026 | Creation
Input: batch/invoice attributes and ordered line/@sku values. Output: text report.
Invoice and line rules share the export mode. Explicit mode or #current retains
export dispatch during recursion; omitted mode uses the lexical default instead.
These projections do not validate invoice data or enforce enclosing-mode isolation.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:erp="urn:example:erp" xmlns:q="urn:example:erp" version="3.0">
  <xsl:output method="text"/>
  <xsl:mode name="erp:export"/>
  <xsl:template match="invoice" mode="erp:export">
    <xsl:value-of select="concat(@id, ':', @currency, ':')"/>
    <xsl:apply-templates select="line"/>
    <xsl:text>;</xsl:text>
  </xsl:template>
  <xsl:template match="line" mode="erp:export">
    <xsl:value-of select="concat(@sku, '/')"/>
  </xsl:template>
  <xsl:template match="line">
    <xsl:text>wrong/</xsl:text>
  </xsl:template>
  <xsl:template match="/">
    <xsl:apply-templates select="batch/invoice" mode="erp:export"/>
  </xsl:template>
</xsl:stylesheet>
```

Output:

```text
INV-2048:EUR:wrong/wrong/;INV-2049:USD:wrong/;
```

The [lexical default fixture](../examples/enclosing-modes/lexical-default-mode.xsl) sets the root `default-mode` to export and obtains the export report with omitted recursion. That is an explicit lexical policy, not enclosing-mode support.

### Empty and reordered input

Complete empty input:

```xml
<batch/>
```

Output is the empty string (zero principal text bytes). Complete reordered input:

```xml
<batch><invoice id="INV-2049" currency="USD"><line sku="P300"/></invoice><invoice id="INV-2048" currency="EUR"><line sku="P200"/><line sku="P100"/></invoice></batch>
```

Output:

```text
INV-2049:USD:P300/;INV-2048:EUR:P200/P100/;
```

The reverse-selection fixture reverses invoices without reversing their line sequence. The audit fixture reports `audit:INV-2048:2;audit:INV-2049:1;`, using its own top-level mode rules.

### Complete fixture inventory

The [manifest](../examples/cases.json) executes 20 cases, including nine generic refusals and eleven successful mode/ordering comparisons. Errors assert compile/capture markers without promising a universal detection stage.

| Fixture | Mode | Exact output/error marker |
|---|---|---|
| [main-refusal4](../examples/enclosing-modes/main-refusal4.xsl) | xslt40 | `XTSE0260` |
| [main-refusal3](../examples/enclosing-modes/main-refusal3.xsl) | xslt31 | `XTSE0260` |
| [unnamed](../examples/enclosing-modes/unnamed.xsl) | xslt40 | `XTSE0260` |
| [named-child](../examples/enclosing-modes/named-child.xsl) | xslt40 | `XTSE0260` |
| [explicit-child-mode](../examples/enclosing-modes/explicit-child-mode.xsl) | xslt40 | `XTSE0260` |
| [missing-child-match](../examples/enclosing-modes/missing-child-match.xsl) | xslt40 | `XTSE0260` |
| [default-conflict](../examples/enclosing-modes/default-conflict.xsl) | xslt40 | `XTSE0260` |
| [external-rule](../examples/enclosing-modes/external-rule.xsl) | xslt40 | `XTSE0260` |
| [duplicate](../examples/enclosing-modes/duplicate.xsl) | xslt40 | `XTSE0260` |
| [portable3](../examples/enclosing-modes/portable3.xsl) | xslt31 | `INV-2048:EUR:P100/P200/;INV-2049:USD:P300/;` |
| [portable4](../examples/enclosing-modes/portable4.xsl) | xslt40 | `INV-2048:EUR:P100/P200/;INV-2049:USD:P300/;` |
| [portable-empty](../examples/enclosing-modes/portable-empty.xsl) | xslt31 | `` |
| [portable-reordered](../examples/enclosing-modes/portable-reordered.xsl) | xslt31 | `INV-2049:USD:P300/;INV-2048:EUR:P200/P100/;` |
| [portable-reverse](../examples/enclosing-modes/portable-reverse.xsl) | xslt31 | `INV-2049:USD:P300/;INV-2048:EUR:P100/P200/;` |
| [audit](../examples/enclosing-modes/audit.xsl) | xslt31 | `audit:INV-2048:2;audit:INV-2049:1;` |
| [current-mode](../examples/enclosing-modes/current-mode.xsl) | xslt31 | `INV-2048:EUR:P100/P200/;INV-2049:USD:P300/;` |
| [omitted-recursion-mode](../examples/enclosing-modes/omitted-recursion-mode.xsl) | xslt31 | `INV-2048:EUR:wrong/wrong/;INV-2049:USD:wrong/;` |
| [lexical-default-mode](../examples/enclosing-modes/lexical-default-mode.xsl) | xslt31 | `INV-2048:EUR:P100/P200/;INV-2049:USD:P300/;` |
| [namespace-alias](../examples/enclosing-modes/namespace-alias.xsl) | xslt31 | `INV-2048:EUR:P100/P200/;INV-2049:USD:P300/;` |
| [empty-declaration](../examples/enclosing-modes/empty-declaration.xsl) | xslt40 | `ready` |

## 14. Performance

Export visits the selected invoices and their lines; audit counts selected line children. No benchmark or performance claim for deferred enclosing modes is made.

## 15. Known Uses

This ERP export/audit is synthetic integration evidence. [StylesheetTests](../../../tests/Bosak.Xslt.Tests/StylesheetTests.cs) includes `Hash_Current_Resolves_To_Current_Mode`, `Mode_All_Matches_All_Modes`, `Mode_OnNoMatch_ShallowSkip` and `Initial_Mode_With_Source_Succeeds` for ordinary mode behavior. [ModeConflictResolutionTests](../../../tests/Bosak.Xslt.Tests/ModeConflictResolutionTests.cs) includes `SamePrecedenceConflict_UnresolvedByHigherPrecedence_ThrowsXtse0545` and `SamePrecedenceDisjointAttributes_MergeWithoutError`. These tests are not enclosing-mode conformance evidence. No conformance gate membership changes are included.

## 16. Related Features

[Computed XPath constructors](computed-node-constructors.md), [named XSLT item types](xslt-item-type.md), [named XSLT record types](xslt-record-type.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md).
