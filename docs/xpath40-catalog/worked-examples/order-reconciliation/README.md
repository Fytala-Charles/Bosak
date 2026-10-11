<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak worked example: order reconciliation">
  <br><br><h1>Reconcile Orders and Shipments</h1>
  <p>Document-scoped joins, nested grouping, and an honest exception report</p>
</div>

## Reading this chapter

- [The business problem](#the-business-problem)
- [Files and output policy](#files-and-output-policy)
- [Source-to-target field guide](#source-to-target-field-guide)
- [Complete input and output documents](#complete-input-and-output-documents)
- [XSLT elements and XPath constructs used](#xslt-elements-and-xpath-constructs-used)
- [Join using stable identities](#join-using-stable-identities)
- [Keep validation separate from arithmetic](#keep-validation-separate-from-arithmetic)
- [Group by customer, then product](#group-by-customer-then-product)
- [Inspect the incomplete batch](#inspect-the-incomplete-batch)
- [Run and extend the chapter](#run-and-extend-the-chapter)

## The business problem

A workshop receives orders throughout the day and records shipments separately.
Its dispatch team needs a customer/product summary of ordered units, shipped
units, outstanding units, and over-shipments. One order line can have several
shipment records. Some records have missing reference data or a quantity still
marked `pending`; those must not silently count as zero without an explanation.

The synthetic input places customer/product reference tables, orders, and
shipments in one namespaced XML batch. This keeps the example self-contained.
It is not an ERP integration standard, inventory allocation system, or monetary
reconciliation. Quantities are positive integer units; returns, cancellations,
reservations, time windows, external lookups, and duplicate shipment-event
detection are not implemented.

## Files and output policy

| File | Purpose |
|---|---|
| [orders.xml](fixtures/orders.xml) | Complete batch with two customers, repeated product lines, partial shipments, and an over-shipment. |
| [order-reconciliation.xsl](order-reconciliation.xsl) | Complete annotated XSLT 3.0 / XPath 3.1 transformation. |
| [expected-report.xml](fixtures/expected-report.xml) | Customer/product reconciliation report. |
| [expected-exceptions.xml](fixtures/expected-exceptions.xml) | Empty exception document for the complete input. |
| [incomplete-orders.xml](fixtures/incomplete-orders.xml) | Unknown customer/product, non-numeric quantities, and an unmatched shipment. |
| [expected-incomplete-report.xml](fixtures/expected-incomplete-report.xml) | Accepted-data report marked `review-required`. |
| [expected-incomplete-exceptions.xml](fixtures/expected-incomplete-exceptions.xml) | Exact rejected line and shipment identities. |

The principal report groups accepted order lines. The secondary
`exceptions.xml` describes rejected business records. A report with any rejection
is marked `review-required`: its quantities reflect accepted records only.
The host must review exceptions before treating that report as complete.

An over-shipment is represented explicitly in the report rather than discarded:
it is not an invalid quantity. `ready` means no records were rejected, not that
there are no outstanding or over-shipped units. A dispatch workflow should inspect
those quantities according to its business rules.


## Source-to-target field guide

Source elements use `urn:example:orders` (`o`); report and exception elements use `urn:example:reconciliation` (`r`). Reference tables and transactions belong to the same input document.

| Source element or attribute | Join, validation, and target representation |
|---|---|
| `o:batch` | Expected root; produces principal `r:report` and secondary `r:exceptions`. This is selected validation, not a full schema check. |
| `o:customers/o:customer` with `@id`, `@name` | Customer key matches an order's `@customer-id`; exactly one match is required. ID and name populate `r:customer`. Name presence is not validated. |
| `o:products/o:product` with `@sku`, `@name` | Product key matches a line's `@sku`; exactly one match is required. SKU and name populate `r:product`. |
| `o:orders/o:order/@id` | Non-blank, unique, and free of `\|`; identifies the shipment's order. Invalid/ambiguous order identities terminate the batch. |
| Order `@customer-id` | Resolves customer reference data and becomes the outer grouping key. |
| `o:line/@id` | Unique within its order; missing attributes or duplicate IDs fail the root cardinality check. A present blank ID or one containing `\|` is rejected by the line helper. |
| Line `@sku`, `@quantity` | Known product and positive integer quantity required; quantities sum into product `@ordered`. |
| `o:shipments/o:shipment` with `@order-id`, `@line-id`, `@quantity` | Composite identity locates one accepted line; each positive integer shipment contributes to `@shipped`. Unmatched, rejected-line, or invalid-quantity shipments are reported. |
| Accepted customer/product groups | Emit `@ordered`, `@shipped`, `@outstanding`, and `@over-shipped`; customer IDs and SKUs sort ascending. |
| Rejected lines and shipments | Exceptions retain `@scope`, `@order-id`, `@line-id`, and `@reason`; report status becomes `review-required`. |

### Follow the quantities and rejection flow

O1 line 1 and O2 line 1 both belong to C1/BOLT, so their quantities combine: `10 + 5 = 15`. Their three shipment records contribute `6 + 2 + 5 = 13`. C1/FILTER's shipment quantity 3 exceeds the ordered 2; it remains valid data and yields over-shipped 1. C2/FILTER has no shipments, yielding shipped 0 and outstanding 4.

`f:error` checks a line in this order: `invalid-line-id`, `unknown-or-duplicate-customer`, `unknown-or-duplicate-product`, `invalid-quantity`. Only the first reason is emitted. `f:shipped` accepts an already valid line and totals its positive-integer shipment quantities, returning zero when none qualify. The root separately records excluded shipments as `unmatched-or-invalid-shipment`; excluding a bad quantity does not hide it.

The incomplete case accepts only O1 line 1. Its quantity 10 and valid shipment 4 yield outstanding 6. Three line exceptions appear first, followed by two shipment exceptions. Two shipment failures can share an order/line identity because the vocabulary has no shipment-event ID; production repair and deduplication need a stronger event contract.

## Complete input and output documents

These are the complete published fixtures. Formatting is expanded for reading; paragraph text and inline spacing are preserved. The linked files remain the executable source of truth.

### Complete order and shipment batch

[orders.xml](fixtures/orders.xml)

<!-- fixture: fixtures/orders.xml -->
```xml
<batch xmlns="urn:example:orders">
  <customers>
    <customer id="C1" name="North Workshop"/>
    <customer id="C2" name="South Workshop"/>
  </customers>
  <products>
    <product sku="BOLT" name="M8 bolt"/>
    <product sku="FILTER" name="Air filter"/>
  </products>
  <orders>
    <order id="O1" customer-id="C1">
      <line id="1" sku="BOLT" quantity="10"/>
      <line id="2" sku="FILTER" quantity="2"/>
    </order>
    <order id="O2" customer-id="C1">
      <line id="1" sku="BOLT" quantity="5"/>
    </order>
    <order id="O3" customer-id="C2">
      <line id="1" sku="FILTER" quantity="4"/>
    </order>
  </orders>
  <shipments>
    <shipment order-id="O1" line-id="1" quantity="6"/>
    <shipment order-id="O1" line-id="1" quantity="2"/>
    <shipment order-id="O2" line-id="1" quantity="5"/>
    <shipment order-id="O1" line-id="2" quantity="3"/>
  </shipments>
</batch>
```

### Principal reconciliation report

[expected-report.xml](fixtures/expected-report.xml)

<!-- fixture: fixtures/expected-report.xml -->
```xml
<r:report xmlns:r="urn:example:reconciliation" status="ready">
  <r:customer id="C1" name="North Workshop">
    <r:product sku="BOLT" name="M8 bolt" ordered="15" shipped="13" outstanding="2" over-shipped="0"/>
    <r:product sku="FILTER" name="Air filter" ordered="2" shipped="3" outstanding="0" over-shipped="1"/>
  </r:customer>
  <r:customer id="C2" name="South Workshop">
    <r:product sku="FILTER" name="Air filter" ordered="4" shipped="0" outstanding="4" over-shipped="0"/>
  </r:customer>
</r:report>
```

### Secondary exceptions for the complete batch

[expected-exceptions.xml](fixtures/expected-exceptions.xml)

<!-- fixture: fixtures/expected-exceptions.xml -->
```xml
<r:exceptions xmlns:r="urn:example:reconciliation"/>
```

### Incomplete batch

[incomplete-orders.xml](fixtures/incomplete-orders.xml)

<!-- fixture: fixtures/incomplete-orders.xml -->
```xml
<batch xmlns="urn:example:orders">
  <customers>
    <customer id="C1" name="North Workshop"/>
  </customers>
  <products>
    <product sku="BOLT" name="M8 bolt"/>
  </products>
  <orders>
    <order id="O1" customer-id="C1">
      <line id="1" sku="BOLT" quantity="10"/>
      <line id="2" sku="UNKNOWN" quantity="2"/>
      <line id="3" sku="BOLT" quantity="pending"/>
    </order>
    <order id="O2" customer-id="UNKNOWN">
      <line id="1" sku="BOLT" quantity="5"/>
    </order>
  </orders>
  <shipments>
    <shipment order-id="O1" line-id="1" quantity="4"/>
    <shipment order-id="O1" line-id="1" quantity="pending"/>
    <shipment order-id="MISSING" line-id="1" quantity="2"/>
  </shipments>
</batch>
```

### Principal accepted-data report

[expected-incomplete-report.xml](fixtures/expected-incomplete-report.xml)

<!-- fixture: fixtures/expected-incomplete-report.xml -->
```xml
<r:report xmlns:r="urn:example:reconciliation" status="review-required">
  <r:customer id="C1" name="North Workshop">
    <r:product sku="BOLT" name="M8 bolt" ordered="10" shipped="4" outstanding="6" over-shipped="0"/>
  </r:customer>
</r:report>
```

### Secondary rejected records

[expected-incomplete-exceptions.xml](fixtures/expected-incomplete-exceptions.xml)

<!-- fixture: fixtures/expected-incomplete-exceptions.xml -->
```xml
<r:exceptions xmlns:r="urn:example:reconciliation">
  <r:exception scope="line" order-id="O1" line-id="2" reason="unknown-or-duplicate-product"/>
  <r:exception scope="line" order-id="O1" line-id="3" reason="invalid-quantity"/>
  <r:exception scope="line" order-id="O2" line-id="1" reason="unknown-or-duplicate-customer"/>
  <r:exception scope="shipment" order-id="O1" line-id="1" reason="unmatched-or-invalid-shipment"/>
  <r:exception scope="shipment" order-id="MISSING" line-id="1" reason="unmatched-or-invalid-shipment"/>
</r:exceptions>
```

## XSLT elements and XPath constructs used

This inventory covers every XSLT element in the [complete stylesheet](order-reconciliation.xsl). Literal result elements build the target vocabulary described above; `{...}` in their attributes are attribute value templates that evaluate XPath. Expressions in `select`, `test`, and `group-by` are XPath directly.

| XSLT element | Responsibility |
|---|---|
| `xsl:choose` | Selects one branch, keeping rejection and accepted-data paths explicit. |
| `xsl:for-each` | Processes records or reason values in sequence order; its selected item becomes the current context. |
| `xsl:for-each-group` | Groups records by a computed value; body expressions use `current-group()` and `current-grouping-key()`. |
| `xsl:function` | Declares a typed, reusable validation or calculation helper. |
| `xsl:if` | Emits content or raises a message only when its test holds. |
| `xsl:key` | Declares a document-local index; the XPath `key()` function performs its lookup. |
| `xsl:message` | With `terminate="yes"`, refuses structurally unsuitable input rather than presenting a business report. |
| `xsl:output` | Chooses serialization: XML for business reports, HTML5 for the publishing chapter. |
| `xsl:param` | Declares the helper input and its required element type. |
| `xsl:result-document` | Constructs the secondary exception document; relative URI resolution belongs to the host output contract. |
| `xsl:sequence` | Returns atomic values or existing nodes without converting them to text. |
| `xsl:sort` | Orders groups deterministically before emitting their body. |
| `xsl:strip-space` | Removes indentation-only source text in business records; deliberately absent from the mixed-content manual. |
| `xsl:stylesheet` | Declares XSLT 3.0, source/result/helper namespace bindings, and excluded implementation prefixes. |
| `xsl:template` | Associates a source match with construction rules; `/` coordinates the whole transformation. |
| `xsl:variable` | Binds source nodes, validation results, or amounts without changing their values later. |
| `xsl:when` | Tests an ordered branch of `xsl:choose`. |

The main XPath building blocks are:

| Construct | Role in this chapter |
|---|---|
| `key`, `root`, `concat` | Lookup reference tables and shipments in the explicit source document; create the delimiter-based shipment identity. |
| Parent paths `../`, predicates, `@`, `.` | Join each line to its order and identify matching shipments without constructing duplicate records. |
| `distinct-values`, `count`, `normalize-space`, `contains` | Reject ambiguous identities and enforce the delimiter restriction. |
| `castable as`, `xs:integer`, `if/then/else` | Prevent numeric casts of `pending`; guard `f:error($matched[1])` so a missing match never reaches an element-required helper. |
| `empty`, `exists`, `string` | Select accepted records, set status, and supply string grouping keys. |
| `for/return`, simple map `!`, `sum` | Sum valid shipment units and accepted order quantities. |
| `current-group()`, `current-grouping-key()`, `max` | Form customer/product aggregates and separate positive outstanding/over-shipped differences. |

## Join using stable identities

Customer and product keys index their reference tables. A line is accepted only
when each lookup resolves exactly once, so missing and duplicate reference data
are visible. Order IDs must be unique; line IDs must be unique within an order.
Ambiguous identities terminate the entire transformation because they would make
shipment matching unreliable.

The shipment key combines order ID and line ID. This reduced contract prohibits
`\|` in accepted order/line identifiers, so the composite key cannot collide by
delimiter ambiguity. In a broader vocabulary, use a key design suited to its
identifier rules.

The helper `f:shipped` calls `key` with an explicit third argument, `root($line)`.
Function evaluation cannot rely on the caller's current document focus. Passing
the document explicitly keeps the lookup tied to the right batch.

## Keep validation separate from arithmetic

`f:error` returns the primary reason for rejecting an order line. It checks ID,
reference-table resolution, and integer quantity before the line is grouped.
Malformed or unmatched shipments also produce exceptions. Only positive-integer
shipments matched to accepted lines participate in the quantities.

A line without shipment records contributes zero shipped units. That is a
meaningful outstanding order, not a missing-reference error. A non-numeric
shipment quantity is different: it is excluded and reported, and the principal
report becomes `review-required`.

## Group by customer, then product

The outer `xsl:for-each-group` groups accepted lines by customer ID. The inner
grouping combines the customer's lines by SKU, even when they belong to different
orders. Both levels sort their keys for stable output. The grouping key is saved
before invoking calculation helpers.

For the complete fixture:

| Customer/product | Ordered | Shipped | Outstanding | Over-shipped |
|---|---:|---:|---:|---:|
| North / BOLT | 15 | 13 | 2 | 0 |
| North / FILTER | 2 | 3 | 0 | 1 |
| South / FILTER | 4 | 0 | 4 | 0 |

The outstanding amount is `max(0, ordered - shipped)`; over-shipped is the reverse
positive difference. The two separate columns keep exceptions to the expected
flow visible. No monetary rounding is involved.

## Inspect the incomplete batch

Only the known-customer BOLT line with quantity 10 is accepted. Its valid
shipment contributes 4, leaving 6 outstanding. Three rejected order lines and
two rejected shipment records appear in the expected exceptions document. The
malformed shipment is not silently accepted as a zero-unit delivery.

The exception ordering is deliberate: rejected lines in source order, then
rejected shipments in source order. Consumers can use the preserved order/line
identities to route repairs, then rerun the batch.

## Run and extend the chapter

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~WorkedExampleTests.Reconciliation
```

Run from the repository root. [WorkedExampleTests](../../../../tests/Bosak.Xslt.Tests/Documentation/WorkedExampleTests.cs)
compares both complete expected outputs for complete/incomplete input and verifies
that duplicate order IDs refuse the batch. Comparisons ignore formatting and
namespace-declaration placement while retaining expanded names, values, and
element order. The host uses public `XsltCompiler`/`CaptureTransform` APIs with a
source document adapter and explicit base output URI; both documents stay in
memory. Secondary identity is
`https://example.invalid/reference/exceptions.xml` in the verification harness.

Before adapting this to production, define shipment-event deduplication, returns,
cancellations, and external reference-data policy, then add fixtures for those
rules. The [map entry iteration](../../operators-and-syntax/map-entry-iteration.md)
and [comparator sorting](../../functions/sort-with.md) entries offer optional 4.0
techniques for map-based summaries; this XML grouping solution remains 3.0.
