<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak reference guide: supplier invoices to ERP XML">
  <br><br>
  <h1>From Supplier Invoices to ERP XML</h1>
  <p>A worked XSLT 3.0 chapter: map business documents, group tax lines, and make exceptions visible</p>
</div>

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

Next planned chapter: publish an XML manual as HTML while preserving mixed content, links, and a table of contents. See the [chapter plan](../README.md) and [catalog index](../../README.md).
