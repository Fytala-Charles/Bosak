<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak worked example: order reconciliation">
  <br><br><h1>Reconcile Orders and Shipments</h1>
  <p>Document-scoped joins, nested grouping, and an honest exception report</p>
</div>

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

## Join using stable identities

Customer and product keys index their reference tables. A line is accepted only
when each lookup resolves exactly once, so missing and duplicate reference data
are visible. Order IDs must be unique; line IDs must be unique within an order.
Ambiguous identities terminate the entire transformation because they would make
shipment matching unreliable.

The shipment key combines order ID and line ID. This reduced contract prohibits
`|` in accepted order/line identifiers, so the composite key cannot collide by
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
