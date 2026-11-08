<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala xsl:select: XPath in stylesheet text">
  <h1>xsl:select: XPath in stylesheet text</h1>
  <p>21 October 2026</p>
</div>

**C12 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-4.md) records three contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Verified bounded XPath body instruction**. CDATA is an XML authoring mechanism; complete schema/static/package/resource combinations and global static-detection mode remain unaccepted. Final owner acceptance remains C13; earlier checkpoints are historical.

## 1. Name and Classification

**xsl:select — implemented Bosak XSLT4.0 surface.** [Body-expression delivery `69a7cfd3`](https://github.com/Fytala-Charles/Bosak/commit/69a7cfd3c84e405c037be41786464950d1c06696); [current source review](../SWITCH-ARRAY-SELECT-GRAMMAR.md). This entry documents shipped behavior under REQ-126, not a new engine feature. Historical adopted specification revision is not recovered; current review pin is3230573afe246f1b502ed2e22d187ce9928155b1 section `select-instruction`.

## 2. Intent

Return an XPath expression result while keeping a multiline expression in XML text or CDATA rather than a quoted XML attribute.

## 3. Also Known As

Text-body XPath evaluation. `xsl:sequence/@select` is the earlier alternative, not an alias for the new instruction.

## 4. History / Supersedes

The [3.0 dispatch report](../examples/dispatch-grammar/report-3.xsl) uses value-of/attribute expressions; the [4.0 report](../examples/dispatch-grammar/report-4.xsl) uses xsl:select for report fragments. Both explicitly create text. Arbitrary returned maps, arrays and nodes require their own receiving context; text conversion is not universal equivalence.

## 5. Motivation

Keep both quote characters, braces and formatting readable in dispatch/report expressions. XML escaping is still needed for ordinary text containing markup delimiters; CDATA avoids those escapes subject to XML's CDATA terminator rule.

## 6. Language Alignment

The body is XPath, not JavaScript or an XSLT sequence constructor. Literal braces belong to XPath maps/strings, not AVT or expand-text interpolation. Returning items does not imply writing text nodes.

## 7. Applicability

Use when the receiving Bosak host permits the implemented draft surface. Use portable3.0 instructions when portability is required. Validate structured results through their consumer rather than stringifying them unintentionally.

## 8. Structure

```xml
<xsl:select as="xs:string"><![CDATA[
  map { 'label': '{SKU}' }?label
]]></xsl:select>
```

Complete namespace/context declarations are in [select-cdata.xsl](../examples/dispatch-grammar/select-cdata.xsl). Optional as applies result conversion/type checks. Only permitted leading fallbacks precede expression text; this is a descriptive shape, not a complete grammar.

## 9. Participants

[Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) validates content and selects expression compatibility; [TransformEngine](../../../src/Bosak.Xslt/Runtime/TransformEngine.cs) evaluates it with the caller's context. No new public API is introduced.

## 10. Collaborations

Combine with variables/functions for structured values and value-of for explicit text output. The dispatch report uses arrays and switch without flattening away empty batch members.

## 11. Consequences

Improves expression readability but adds a draft instruction dependency. Adjacent atomic values can gain separators during tree construction; explicit text construction matters. Optional result types can raise XPTY0004.

## 12. Implementation

Default Legacy host uses effective version:3.0 expressions use XPath31,4.0 uses frozen XPath40, and local overrides matter. Known instruction recognition is separate: Bosak accepts the tested simple xsl:select forms under3.0, but this is not portable3.0 syntax. Explicit Xslt40Draft host selects frozen XPath40 independently of local lower version. [Version matrix](../SWITCH-ARRAY-SELECT-GRAMMAR.md#recognition-is-distinct-from-compatibility).

## 13. Sample Code

Run the complete [report](../examples/dispatch-grammar/README.md) against normal, empty and reordered input. The [focus](../examples/dispatch-grammar/select-focus.xsl) and [text-focus](../examples/dispatch-grammar/select-focus-text.xsl) files distinguish item results from explicit text. The [required-empty](../examples/dispatch-grammar/select-required-empty.xsl) file intentionally fails XPTY0004.

## 14. Performance Notes

Cost is dominated by the XPath expression and its result materialization. CDATA changes XML authoring, not an execution complexity guarantee. No measured speed or memory improvement is claimed.

## 15. Known Uses

New manifest IDs include dispatch-grammar-select-cdata, select-coercion, select-type-error, select-required-empty, select-focus/text and expression-root30/root40/local30/local40. [Xslt40SelectTests](../../../tests/Bosak.Xslt.Tests/Xslt40SelectTests.cs) and [ArraySelectNativeBoundaryTests](../../../tests/Bosak.Xslt.Tests/ArraySelectNativeBoundaryTests.cs) supply additional source/native evidence. No applicable XPath qt4tests expression set or new GatedSets member exists for this XSLT instruction; AC-03 acceptance remains partial.

## 16. Related Features

[xsl:switch](xsl-switch.md), [xsl:array](xsl-array.md), [catalog index](../README.md), [grammar/type review](../SWITCH-ARRAY-SELECT-GRAMMAR.md). Current draft reference: [select-instruction](https://qt4cg.org/specifications/xslt-40/Overview.html#select-instruction); immutable review source is linked in the review.
