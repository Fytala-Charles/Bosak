<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak reference guide: supplier invoices to ERP XML">
  <br><br>
  <h1>From Supplier Invoices to ERP XML</h1>
  <p>A worked XSLT 3.0 chapter: map business documents, group tax lines, and make exceptions visible</p>
</div>

## Reading this chapter

- [The problem](#the-problem)
- [What you will build](#what-you-will-build)
- [Start with the source and target](#start-with-the-source-and-target)
- [Source-to-target field guide](#source-to-target-field-guide)
- [Complete input and output documents](#complete-input-and-output-documents)
- [XSLT elements and XPath constructs used](#xslt-elements-and-xpath-constructs-used)
- [Walk through the transformation](#walk-through-the-transformation)
- [Handle incomplete records deliberately](#handle-incomplete-records-deliberately)
- [Run and verify the chapter](#run-and-verify-the-chapter)
- [Where XPath 4.0 can improve readability](#where-xpath-40-can-improve-readability)
- [Extend the case](#extend-the-case)

## The problem

A supplier sends invoices as XML. Your ERP import expects another vocabulary: different element names, ISO dates, normalized identifiers, line totals, and a tax summary. Some invoices lack a purchase-order reference; some lines arrive with missing product codes or prices that are still marked `pending`. The transformation must distinguish usable data from data that needs attention.

This chapter uses synthetic supplier and ERP vocabularies. It demonstrates a realistic mapping task without claiming UBL, tax-law, or ERP-product compliance. The fixture has one currency per invoice, three allowed currency codes, non-negative prices, positive quantities, and no discounts, credit notes, exchange rates, duplicate-ID detection, or schema validation. Actual import contracts must define those rules separately.

## What you will build

The [complete stylesheet](invoice-to-erp.xsl) produces two documents:

| Output | Purpose |
|---|---|
| Principal `import.xml` | Mapped invoices, accepted lines, tax groups, and totals. An invoice with rejected lines is marked `review-required`. |
| Secondary `exceptions.xml` | Invoice/header rejection reasons and the primary reason for each rejected line. |

**Compatibility:** the complete mapping uses XSLT 3.0 and XPath 3.1. It needs no 4.0 opt-in. A separate, verified XPath 4.0 comparison later in the chapter illustrates map destructuring without changing the mapping's requirements.

**Host decision:** transformation success means that both documents were produced. It does not mean every invoice is ready to post. A consumer must inspect the exceptions and `status`; `review-required` invoices contain partial totals and must be held for review. An invalid header excludes the entire invoice from the import document.

## Start with the source and target

Open the [complete supplier input](fixtures/invoices.xml). It uses the default namespace `urn:example:supplier:invoice`. The stylesheet binds that URI to `s`, so `s:invoice` selects supplier invoices even though the source does not spell that prefix. An unprefixed `invoice` name in XPath would select a different vocabulary.

One source line looks like this (excerpt):

```xml
<line xmlns="urn:example:supplier:invoice" id="1">
  <sku>BRACKET-A</sku>
  <quantity>4</quantity>
  <unit-price>25.00</unit-price>
  <tax-rate>21</tax-rate>
</line>
```

Its target line is:

```xml
<erp:line xmlns:erp="urn:example:erp:import"
          id="1" sku="BRACKET-A" quantity="4" unit-price="25.00" net="100.00"/>
```

The [complete expected import](fixtures/expected-import.xml) also converts `11/10/2026` into `2026-10-11`, trims the purchase-order reference, and groups the `21` and `21.00` rates together. The [expected exception document](fixtures/expected-exceptions.xml) is empty for this input.


## Source-to-target field guide

The source prefix `s` and target prefix `erp` identify different namespace URIs; prefix spelling itself is not the contract. Fields are single-valued. The selected business checks do not validate the entire document structure.

| Source element or attribute | Rule and target representation |
|---|---|
| `s:batch` / `s:invoice` | Batch becomes `erp:import`; each header-valid invoice becomes `erp:invoice`. |
| Invoice `@id` | Required non-blank value; copied unchanged into target `@id` and exception `@invoice-id`. Whitespace is checked but the ID is not trimmed on output. |
| `s:supplier-id` | Required; normalized into invoice `@supplier`. |
| Invoice `@currency` | Normalized value must be exactly EUR, USD, or GBP; emitted as `@currency`. |
| `s:issue-date` | DD/MM/YYYY becomes a validated `xs:date`, serialized as ISO `@issue-date`. |
| `s:purchase-order` | Optional; normalized text becomes `erp:purchase-order`, omitted when absent or blank. |
| `s:line/@id` and `s:sku` | Required; line ID is copied unchanged, SKU normalized into `erp:line/@sku`. No duplicate-ID check is performed. |
| `s:quantity` | Positive decimal becomes line `@quantity`; fractional quantities are permitted. |
| `s:unit-price` | Non-negative decimal is displayed with two places as `@unit-price`; net calculation uses the source decimal before line-net rounding. Extra source price precision can therefore make displayed price times quantity differ from displayed net. |
| `s:tax-rate` | Decimal percentage in [0,100]; numeric grouping produces `erp:tax/@rate`, group `@net`, and `@amount`. |
| Accepted/rejected line sequences | `erp:lines` contains accepted lines only. `erp:totals` exposes both counts, net, tax, and gross; invoice `@status` signals partial data. |
| Rejected header/line | `erp:exceptions/erp:exception` preserves identity, `@scope`, and `@reason`; a line exception also has `@line-id`. |

### Helper contracts and rejection reasons

| Helper | Result and callers' responsibility |
|---|---|
| `f:issue-date` | One date or `()`; checks both spelling and calendar validity. |
| `f:header-errors` | All applicable reasons, in order: `missing-invoice-id`, `missing-supplier-id`, `unsupported-currency`, `invalid-issue-date`, `missing-lines`. A nonempty sequence excludes the invoice and suppresses its line checks. |
| `f:line-error` | First applicable reason: `missing-line-id`, `missing-sku`, `invalid-quantity`, `invalid-unit-price`, `invalid-tax-rate`; `()` means accepted. |
| `f:net` | Rounded decimal net; call only after line acceptance. |
| `f:tax-total` | Sum of rounded rate-group taxes for accepted lines; zero for an empty sequence. |

Header errors are emitted in check order, invoices in source order, and line errors in source order. Exception IDs retain source spelling so a repair tool can identify the original record. The root check enforces the batch namespace; it is not an XML Schema validator.

## Complete input and output documents

These are the complete published fixtures. Formatting is expanded for reading; paragraph text and inline spacing are preserved. The linked files remain the executable source of truth.

### Complete supplier input

[invoices.xml](fixtures/invoices.xml)

<!-- fixture: fixtures/invoices.xml -->
```xml
<!-- Synthetic learning fixture. Copyright Fytala; Apache-2.0; see license.md. -->
<batch xmlns="urn:example:supplier:invoice">
  <invoice id="INV-2026-1042" currency="EUR">
    <supplier-id>SUP-0042</supplier-id>
    <issue-date>11/10/2026</issue-date>
    <purchase-order> PO-8801 </purchase-order>
    <line id="1">
      <sku>BRACKET-A</sku>
      <quantity>4</quantity>
      <unit-price>25.00</unit-price>
      <tax-rate>21</tax-rate>
    </line>
    <line id="2">
      <sku>BOLT-M8</sku>
      <quantity>30</quantity>
      <unit-price>2.00</unit-price>
      <tax-rate>21.00</tax-rate>
    </line>
    <line id="3">
      <sku>MANUAL-PRINT</sku>
      <quantity>1</quantity>
      <unit-price>20.00</unit-price>
      <tax-rate>0</tax-rate>
    </line>
  </invoice>
</batch>
```

### Principal ERP import

[expected-import.xml](fixtures/expected-import.xml)

<!-- fixture: fixtures/expected-import.xml -->
```xml
<erp:import xmlns:erp="urn:example:erp:import">
  <erp:invoice id="INV-2026-1042" supplier="SUP-0042" currency="EUR" issue-date="2026-10-11" status="ready">
    <erp:purchase-order>PO-8801</erp:purchase-order>
    <erp:lines>
      <erp:line id="1" sku="BRACKET-A" quantity="4" unit-price="25.00" net="100.00"/>
      <erp:line id="2" sku="BOLT-M8" quantity="30" unit-price="2.00" net="60.00"/>
      <erp:line id="3" sku="MANUAL-PRINT" quantity="1" unit-price="20.00" net="20.00"/>
    </erp:lines>
    <erp:tax-summary>
      <erp:tax rate="0" net="20.00" amount="0.00"/>
      <erp:tax rate="21" net="160.00" amount="33.60"/>
    </erp:tax-summary>
    <erp:totals accepted-lines="3" rejected-lines="0" net="180.00" tax="33.60" gross="213.60"/>
  </erp:invoice>
</erp:import>
```

### Secondary exceptions for the complete invoice

[expected-exceptions.xml](fixtures/expected-exceptions.xml)

<!-- fixture: fixtures/expected-exceptions.xml -->
```xml
<erp:exceptions xmlns:erp="urn:example:erp:import"/>
```

### Incomplete supplier input

[incomplete-invoices.xml](fixtures/incomplete-invoices.xml)

<!-- fixture: fixtures/incomplete-invoices.xml -->
```xml
<!-- Synthetic learning fixture. Copyright Fytala; Apache-2.0; see license.md. -->
<batch xmlns="urn:example:supplier:invoice">
  <invoice id="INV-2026-1043" currency="EUR">
    <supplier-id>SUP-0042</supplier-id>
    <issue-date>12/10/2026</issue-date>
    <line id="1">
      <sku>FILTER-A</sku>
      <quantity>2</quantity>
      <unit-price>12.50</unit-price>
      <tax-rate>9</tax-rate>
    </line>
    <line id="2">
      <quantity>1</quantity>
      <unit-price>5.00</unit-price>
      <tax-rate>9</tax-rate>
    </line>
    <line id="3">
      <sku>FILTER-B</sku>
      <quantity>1</quantity>
      <unit-price>pending</unit-price>
      <tax-rate>9</tax-rate>
    </line>
    <line id="4">
      <sku>FILTER-C</sku>
      <quantity>0</quantity>
      <unit-price>5.00</unit-price>
      <tax-rate>9</tax-rate>
    </line>
  </invoice>
</batch>
```

### Principal partial import

[expected-incomplete-import.xml](fixtures/expected-incomplete-import.xml)

<!-- fixture: fixtures/expected-incomplete-import.xml -->
```xml
<erp:import xmlns:erp="urn:example:erp:import">
  <erp:invoice id="INV-2026-1043" supplier="SUP-0042" currency="EUR" issue-date="2026-10-12" status="review-required">
    <erp:lines>
      <erp:line id="1" sku="FILTER-A" quantity="2" unit-price="12.50" net="25.00"/>
    </erp:lines>
    <erp:tax-summary>
      <erp:tax rate="9" net="25.00" amount="2.25"/>
    </erp:tax-summary>
    <erp:totals accepted-lines="1" rejected-lines="3" net="25.00" tax="2.25" gross="27.25"/>
  </erp:invoice>
</erp:import>
```

### Secondary line exceptions

[expected-incomplete-exceptions.xml](fixtures/expected-incomplete-exceptions.xml)

<!-- fixture: fixtures/expected-incomplete-exceptions.xml -->
```xml
<erp:exceptions xmlns:erp="urn:example:erp:import">
  <erp:exception invoice-id="INV-2026-1043" scope="line" line-id="2" reason="missing-sku"/>
  <erp:exception invoice-id="INV-2026-1043" scope="line" line-id="3" reason="invalid-unit-price"/>
  <erp:exception invoice-id="INV-2026-1043" scope="line" line-id="4" reason="invalid-quantity"/>
</erp:exceptions>
```

## XSLT elements and XPath constructs used

This inventory covers every XSLT element in the [complete stylesheet](invoice-to-erp.xsl). Literal result elements build the target vocabulary described above; `{...}` in their attributes are attribute value templates that evaluate XPath. Expressions in `select`, `test`, and `group-by` are XPath directly.

| XSLT element | Responsibility |
|---|---|
| `xsl:apply-templates` | Dispatches selected source nodes to matching templates, optionally in a named mode. |
| `xsl:choose` | Selects one branch, keeping rejection and accepted-data paths explicit. |
| `xsl:for-each` | Processes records or reason values in sequence order; its selected item becomes the current context. |
| `xsl:for-each-group` | Groups records by a computed value; body expressions use `current-group()` and `current-grouping-key()`. |
| `xsl:function` | Declares a typed, reusable validation or calculation helper. |
| `xsl:if` | Emits content or raises a message only when its test holds. |
| `xsl:message` | With `terminate="yes"`, refuses structurally unsuitable input rather than presenting a business report. |
| `xsl:otherwise` | Supplies the fallback branch when no `xsl:when` matches. |
| `xsl:output` | Chooses serialization: XML for business reports, HTML5 for the publishing chapter. |
| `xsl:param` | Declares the helper input and its required element type. |
| `xsl:result-document` | Constructs the secondary exception document; relative URI resolution belongs to the host output contract. |
| `xsl:sequence` | Returns atomic values or existing nodes without converting them to text. |
| `xsl:sort` | Orders groups deterministically before emitting their body. |
| `xsl:strip-space` | Removes indentation-only source text in business records; deliberately absent from the mixed-content manual. |
| `xsl:stylesheet` | Declares XSLT 3.0, source/result/helper namespace bindings, and excluded implementation prefixes. |
| `xsl:template` | Associates a source match with construction rules; `/` coordinates the whole transformation. |
| `xsl:value-of` | Inserts the string value of an expression into result text. |
| `xsl:variable` | Binds source nodes, validation results, or amounts without changing their values later. |
| `xsl:when` | Tests an ordered branch of `xsl:choose`. |

The main XPath building blocks are:

| Construct | Role in this chapter |
|---|---|
| Namespaced paths, predicates, `@`, `.` | Select supplier records and filter accepted/rejected lines without losing source identity. |
| `normalize-space`, `tokenize`, `string-join`, `matches` | Normalize selected fields and construct/check the declared date spelling. |
| `castable as`, `xs:date`, `xs:decimal`, `if/then/else` | Test before casting; return `()` for valid/no-error outcomes or invalid/no-date outcomes as defined by each helper. |
| `empty`, `exists`, `not`, general comparison to a sequence | Drive acceptance and the EUR/USD/GBP allow-list. |
| Simple map `!`, `sum`, `count` | Apply net calculation to accepted lines, aggregate amounts, and count accepted/rejected records. |
| `round-half-to-even`, `div`, `format-number` | Implement the stated decimal calculation and two-place display policy. |
| `current-group()`, `current-grouping-key()` | Access each numeric tax-rate group's members and rate. |

## Walk through the transformation

The stylesheet is also intended to be read on its own. Its file header records
the input/output contracts, host responsibilities, calculation policy, license,
and change history. Each helper documents its parameters, return value, and edge
posture; each template documents its context, result, and preconditions. These
comments explain decisions such as header rejection taking precedence over line
checks and totals covering accepted lines only. They follow the applicable
conventions in the [copied XSLT style guide](../../../XSLT_STYLE_GUIDE.md).

### 1. Select invoices and validate their headers

The root template checks for the expected supplier batch, produces the exceptions document, then applies templates to invoices with valid headers. `f:header-errors` checks invoice ID, supplier ID, the currency allow-list, issue date, and presence of lines. `f:issue-date` converts the declared `DD/MM/YYYY` format only after confirming that the constructed ISO value is castable as `xs:date`. `31/02/2026` is rejected rather than silently normalized.

A wrong root namespace terminates the transformation. A malformed invoice inside a correctly namespaced batch is business data that the exception document can describe. This distinction keeps a wrongly configured input pipeline visible.

### 2. Separate accepted and rejected lines

`f:line-error` checks line ID, SKU, quantity, price, and tax rate in a declared order. Numeric casts follow castability checks through conditional branches, so the text `pending` never reaches multiplication. Each rejected line gets its first applicable reason; a repair workflow may need another run to expose a second defect.

The invoice template keeps accepted lines in source order and counts rejected lines. A missing or blank optional purchase-order reference emits no target purchase-order element. Missing required information follows the explicit rejection path.

### 3. Calculate line amounts and group tax rates

`f:net` multiplies decimal quantity and price and rounds the line net to two decimal places using `round-half-to-even`. The invoice sums those rounded line amounts. `xsl:for-each-group` groups accepted lines by the **numeric** tax rate, so `21` and `21.00` belong to one group. Each group's tax is calculated from its summed net amount and rounded once; groups are sorted numerically for stable output.

This is an example accounting policy, not a universal rule. Two lines with net amounts `0.03` at 21% yield group tax `0.01`: rounding each line's tax separately would yield `0.02`. The executable tests pin the group-rounding decision so a later refactor cannot quietly change it.

For the complete fixture:

| Rate | Net amount | Tax amount |
|---|---:|---:|
| 0% | 20.00 | 0.00 |
| 21% | 160.00 | 33.60 |
| Total | 180.00 | 33.60 |

Gross is **213.60**. Amounts are formatted to two decimal places for this simplified target; quantities retain their decimal value.

### 4. Use templates for output structure

The `s:invoice` template owns the invoice header, optional reference, tax summary, and totals. The `s:line` template owns one target line. `xsl:apply-templates` connects those responsibilities. The `erp` namespace makes the target vocabulary explicit and keeps source selectors separate from target construction.

This division lets you change line mapping without rewriting the entire invoice template. The named functions centralize validation and amount calculations shared by the principal and exception paths.

## Handle incomplete records deliberately

Run the [incomplete input](fixtures/incomplete-invoices.xml). It contains one usable line, one missing SKU, one non-numeric price, and one zero quantity. Its missing purchase-order reference is allowed.

The [expected partial import](fixtures/expected-incomplete-import.xml) contains only the usable line: net **25.00**, tax **2.25**, gross **27.25**, accepted count **1**, rejected count **3**, and `status="review-required"`. The [expected exceptions](fixtures/expected-incomplete-exceptions.xml) identify lines 2–4 and their reasons. Those totals describe accepted data, not the amount of the original supplier invoice.

The tests also cover an invalid calendar date, an unsupported currency, and an invoice whose every line is rejected. An all-rejected invoice remains `review-required` with zero accepted totals. Invalid headers produce invoice-level exceptions and no mapped invoice. A production host must decide whether to reject the whole batch or route individual invoices for repair.

## Run and verify the chapter

From the repository root:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~InvoiceReferenceGuideTests
```

The test project copies the published XML, XSLT, and XPath files to its output directory and executes those exact files. [InvoiceReferenceGuideTests](../../../../tests/Bosak.Xslt.Tests/Documentation/InvoiceReferenceGuideTests.cs) compares both complete expected XML trees, checks output identities, and verifies the edge cases and migration pair. XML comparisons ignore indentation and redundant namespace-declaration placement but retain expanded element/attribute names, attribute values, content, and element order; this chapter does not prescribe byte-identical serialization formatting. Use the repository's [Application Control workaround](../../../../AGENTS.md#build--test) if the local Windows policy blocks the rebuilt test assembly.

The host compiles `invoice-to-erp.xsl` with its absolute file URI and wraps the parsed source in `XDocumentNode`. It calls `CaptureTransform` with `BaseOutputUri` set to `https://example.invalid/invoice-preview/import.xml`. This makes the principal identity explicit and resolves `exceptions.xml` to `https://example.invalid/invoice-preview/exceptions.xml`. Capture returns both serialized outputs in memory; it does not write to or fetch those example URLs. The test harness uses public APIs only.

The sample inputs are deliberately small. This chapter does not exercise streaming or set execution/byte limits. Hosts accepting untrusted stylesheets need the [controlled resource and execution APIs](../../../INTEGRATION.md#req-125-consumer-review-corrections-2026-10-11) and appropriate process controls.

## Where XPath 4.0 can improve readability

Suppose surrounding application logic represents invoice metadata as a map and needs a display label. The [complete XPath 3.1 expression](invoice-label-31.xpath) binds the fields individually:

```xquery
let $invoice := map { 'invoiceId': 'INV-2026-1042', 'currency': 'EUR' }
return
  let $invoiceId := $invoice?invoiceId,
      $currency := $invoice?currency
  return $invoiceId || ' / ' || $currency
```

The [complete XPath 4.0 expression](invoice-label-40.xpath) puts the requested map fields in one binding:

```xquery
let $invoice := map { 'invoiceId': 'INV-2026-1042', 'currency': 'EUR' }
return
  let ${$invoiceId, $currency} := $invoice
  return $invoiceId || ' / ' || $currency
```

Both return `INV-2026-1042 / EUR`. The latter must be compiled through `XPath31Expression.Compile` with `CompileOptions.Compatibility = XPathCompatibility.XPath40`; the default mode rejects it with XPST0003. These are separate XPath expressions, not replacements to paste into the 3.0 stylesheet. Destructuring improves readability when several fields are unpacked together; it does not validate required keys. A missing map key binds an empty sequence. See the [destructuring entry](../../operators-and-syntax/destructuring-let.md) for shape rules and further compatibility details.

## Extend the case

Before adapting this to an ERP, define duplicate-ID handling, supported currencies and amount scales, discount/credit-note semantics, schema validation, and the required rounding policy. Add a fixture for each agreed rule and pin the expected output. The stylesheet should express that contract, and the host should enforce how rejected data is routed.

Continue with [the XML-manual publishing chapter](../manual-to-html/README.md), which preserves mixed content, links, and a table of contents, then [order reconciliation](../order-reconciliation/README.md). See the [chapter plan](../README.md) and [catalog index](../../README.md).
