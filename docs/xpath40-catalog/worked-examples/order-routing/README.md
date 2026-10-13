<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak order routing chapter">
  <br><br><h1>Validate Orders and Build Dispatch Batches</h1>
  <p>Higher-order predicates, identity checks and consecutive partitioning</p>
</div>

A fulfillment service receives orders in a committed processing order. It must send
valid orders to dispatch, retain rejected orders for repair, and limit each dispatch
batch to two orders for the same route. This chapter keeps the original order;
NORTH orders separated by a SOUTH order do not merge into one group.

The vocabulary and data are synthetic. This is a transformation contract, not an
ERP schema, stock reservation system or shipping API. The capacity is two **orders**,
not two lines or units. Production capacity, supported routes and quantities should
be governed by the receiving application. The stylesheet reads no external resources.

## Self-contained XSLT 4.0 variant (source 1.1.1)

[order-routing-40.xsl](order-routing-40.xsl) performs validation and routing directly
inside the stylesheet. It declares `version="4.0"`; the corrected runtime selects
frozen XPath40 for its expressions. This requires the **1.1.1 source correction**;
published 1.1.0 consumers should use the two-stage variant below until the fix is
released. See the [compatibility correction](../../../REQ-118-XSLT-XPATH40-COMPATIBILITY.md).

The host only compiles and captures:

```csharp
var source = new XDocumentNode(XDocument.Load(inputPath));
var executable = new XsltCompiler().Compile(File.ReadAllText(stylesheetPath),
    new Uri(Path.GetFullPath(stylesheetPath)).AbsoluteUri);
var result = executable.CaptureTransform(source, new XsltCaptureOptions
{
    BaseOutputUri = "https://example.invalid/order-routing/dispatch.xml",
});
```

Use the same fixtures and expected outputs shown below. This variant needs no
external `plan` parameter. The host must still check `IsSuccess` before publishing.
Missing/duplicate IDs produce an unsuccessful capture with ROUTING-ID before
either output is constructed. The two-stage version raises that refusal during
XPath planning instead. Successful results have the same source-order, capacity,
route and repair contracts in both variants.

`f:valid-line` and `f:valid-order` are stylesheet functions in this version. Their
comments document argument types, guarded casts and the empty-line rule.
`every` calls the line predicate; `partition` splits the accepted node sequence;
`index-where` locates invalid lines. The two template loops serialize batch arrays
and rejected orders directly. Five added tests compare both complete result trees
for normal/all-invalid/empty input and ensure both identity refusals produce no
outputs. The chapter now has **11 checks** across both integration variants.

| Additional XSLT element | Role in the self-contained variant |
|---|---|
| `xsl:function` | Declare the typed line/order validation helpers. |
| `xsl:sequence` | Return boolean helper results. |
| `xsl:if` | Guard batch-wide identity refusal before constructing results. |
| `xsl:message` | Terminate with the ROUTING-ID diagnostic. |

The `xsl:param` elements in this variant supply helper arguments; the two-stage
stylesheet uses a required global plan parameter instead. Other XSLT elements
have the roles listed below. Earlier-version local overrides still restrict their
expressions to XPath31, and stylesheet version never enables experimental scan.

## Two-stage compatibility and running

This chapter uses two explicit stages. [routing-plan-40.xpath](routing-plan-40.xpath)
is compiled with `CompileOptions.Compatibility = XPathCompatibility.XPath40`.
Its result is an XDM map containing source nodes, accepted nodes, batch arrays and
exception maps. [order-routing.xsl](order-routing.xsl) declares `version="3.0"`
and receives that map as a required parameter. There are no experimental functions.

**Published Bosak 1.1.0 runtime boundary:** a stylesheet's `version="4.0"` declaration does
not opt transformation-time XPath compilation into XPath40. Calling `partition`
directly from such a stylesheet fails XPST0017 in that release. The source 1.1.1 correction resolves this boundary; the two-stage version remains a supported alternative. The public XPath API
supports the explicit opt-in used here. This chapter demonstrates the supported
two-stage integration; its earlier-release behavior is retained for migration comparison.

The complete host sequence is:

```csharp
var source = new XDocumentNode(XDocument.Load(inputPath));
var plan = XPath31Expression.Compile(File.ReadAllText(planPath),
    new CompileOptions { Compatibility = XPathCompatibility.XPath40 })
    .Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(source), 1, 1));
var executable = new XsltCompiler().Compile(File.ReadAllText(stylesheetPath),
    new Uri(Path.GetFullPath(stylesheetPath)).AbsoluteUri);
var result = executable.CaptureTransform(source, new XsltCaptureOptions
{
    BaseOutputUri = "https://example.invalid/order-routing/dispatch.xml",
    Context = new EvaluationContext().WithVariable("plan", plan),
});
```

The types come from `Bosak.XPath.Api`, `Bosak.XPath.Core.Xdm`,
`Bosak.XPath.Runtime.Vm`, `Bosak.XPath.Providers.Xml`, `Bosak.Xslt.Api` and
`Bosak.Xslt.Capture`, together with `System.Xml.Linq`. The three path variables
identify this chapter's source fixture, planning expression and stylesheet.
If planning raises an error, the host must stop before invoking XSLT. It must not
substitute an empty plan. The supplied plan must belong to the same source document.
`WithVariable` supplies the global `plan` parameter through the capture context.

The principal result is dispatch XML. The secondary URI resolves to
`https://example.invalid/order-routing/exceptions.xml`. The host checks `IsSuccess`
before publishing either result and maps captured outputs to application storage.
See [capture integration](../../../INTEGRATION.md) for the API contract.

Run the published fixtures with:

```powershell
dotnet test tests/Bosak.Xslt.Tests -c Release --filter FullyQualifiedName~OrderRoutingReferenceGuideTests
```

## Input and output contract

The document root is `orders` in `urn:example:routing`; only its direct `order`
children and their direct `line` children are processed. Upstream schema validation
must establish the root and vocabulary; this example does not validate arbitrary XML.
IDs must be nonblank and unique across all orders, including rejected orders.
Uniqueness compares the original ID strings case-sensitively using codepoint
comparison. Trimming checks only blankness; it does not rewrite identity spelling.
Routes are exactly NORTH or SOUTH. An order needs at least one line, and every line
needs a nonblank SKU and a quantity castable to a positive `xs:integer`.
SKU blankness is checked without changing the original source. Quantities accept
XML Schema integer lexical forms; this is not a digits-only validation rule.

| Source or rule | Dispatch or exception result |
|---|---|
| Valid order | Exactly one dispatch `order`, with original ID and sum of integer quantities. |
| Route change or two orders already in current batch | Start a new batch before the next accepted order. |
| Invalid content | One exception with missing-lines, unsupported-route and one-based invalid line positions. |
| Missing or duplicate ID | Terminate the entire run with ROUTING-ID; publish no outputs. |
| Empty input | Successful empty dispatch and empty exceptions. |

Filtering occurs **before** partitioning. A rejected order therefore does not
create a route boundary or consume capacity. Invalid-line positions are relative
to that order's original line sequence, not the filtered accepted-order sequence.

## Walk through the solution

1. `some` checks blank IDs; `all-different` checks identity uniqueness before any
   output construction. Bad identity makes a repair report ambiguous, so it refuses
   the entire batch instead of guessing which order to reference.
2. `$valid-line` uses a guarded cast. A quantity such as `many` becomes a validation
   failure rather than a conversion exception. `$valid-order` requires lines and
   calls `every` with a two-argument predicate. `every` of an empty sequence is true;
   the explicit existence check prevents an empty order from passing.
3. `partition` receives accepted order nodes and a three-argument split predicate.
   Its first argument is the current partition sequence, its second the next order,
   and its third that order's one-based position in the accepted sequence. Returning
   true starts a new partition **before** the next order. The result is a sequence
   of arrays, not an array of arrays and not a pair of accepted/rejected sequences.
4. `?*` exposes each array's members for output construction. `position()` numbers
   batches in their result sequence; it is independent of predicate positions.
5. `index-where` returns every invalid line's one-based position. Independent flags
   explain empty orders and unsupported routes; several reasons may apply together.

The predicates return booleans. This differs from general effective-boolean-value
filtering: callback contracts, arity adaptation and error rules matter. These
functions support more forms than this chapter demonstrates; consult their entries
and specification links for supplementary forms. No throughput claim is made.

## All XSLT elements used

| Element | Role |
|---|---|
| `xsl:stylesheet` | Declare portable XSLT 3.0, namespaces and excluded source prefixes. |
| `xsl:output` | Serialize XML without an XML declaration; indentation is cosmetic. |
| `xsl:param` | Require the host-supplied routing plan map. |
| `xsl:template` | Transform the source document into the two results. |
| `xsl:variable` | Bind original orders, accepted orders, batches, members and exception maps. |
| `xsl:for-each` | Iterate batches, their members and rejected orders in source order. |
| `xsl:result-document` | Construct the repair report at a URI relative to the host base. |

Literal result elements construct dispatch, batch, order and exceptions. Attribute
value templates `{...}` produce scalar output values. The planning expression uses XPath predicates to select
accepted/rejected orders, inline functions for callbacks, `!` for per-item
conversion and `castable` for guarded validation. The stylesheet uses map lookups
to read the plan, `?*` to expose array members and `!` for quantity conversion.
The expression and stylesheet headers and template preconditions make it usable
without this README. See [XSLT style guide](../../../XSLT_STYLE_GUIDE.md).

## Complete fixtures and expected results

The normal fixture demonstrates a capacity split, both route changes, and rejected
lines that consume no capacity. The invalid fixture covers empty lines, zero quantity
and unsupported route. The empty fixture covers absence of orders. Refusal fixtures
have no expected outputs because planning fails before transformation is invoked.

### orders.xml

[orders.xml](fixtures/orders.xml)

<!-- fixture: fixtures/orders.xml -->
```xml
<orders xmlns="urn:example:routing">
  <order id="R1" route="NORTH"><line sku="FILTER-A" quantity="2"/></order>
  <order id="R2" route="NORTH"><line sku="VALVE-B" quantity="1"/></order>
  <order id="BAD1" route="NORTH"><line sku="FILTER-A" quantity="many"/><line sku="" quantity="2"/></order>
  <order id="R3" route="NORTH"><line sku="GASKET-C" quantity="3"/></order>
  <order id="R4" route="SOUTH"><line sku="FILTER-A" quantity="1"/></order>
  <order id="R5" route="NORTH"><line sku="VALVE-B" quantity="2"/></order>
</orders>
```

### expected-dispatch.xml

[expected-dispatch.xml](fixtures/expected-dispatch.xml)

<!-- fixture: fixtures/expected-dispatch.xml -->
```xml
<dispatch accepted="5" rejected="1">
  <batch number="1" route="NORTH"><order id="R1" units="2"/><order id="R2" units="1"/></batch>
  <batch number="2" route="NORTH"><order id="R3" units="3"/></batch>
  <batch number="3" route="SOUTH"><order id="R4" units="1"/></batch>
  <batch number="4" route="NORTH"><order id="R5" units="2"/></batch>
</dispatch>
```

### expected-exceptions.xml

[expected-exceptions.xml](fixtures/expected-exceptions.xml)

<!-- fixture: fixtures/expected-exceptions.xml -->
```xml
<exceptions><order id="BAD1" route="NORTH" missing-lines="false" unsupported-route="false" invalid-line-positions="1 2"/></exceptions>
```

### invalid-orders.xml

[invalid-orders.xml](fixtures/invalid-orders.xml)

<!-- fixture: fixtures/invalid-orders.xml -->
```xml
<orders xmlns="urn:example:routing">
  <order id="EMPTY" route="NORTH"/>
  <order id="ZERO" route="SOUTH"><line sku="FILTER-A" quantity="0"/></order>
  <order id="UNKNOWN" route="EAST"><line sku="FILTER-A" quantity="1"/></order>
</orders>
```

### expected-invalid-dispatch.xml

[expected-invalid-dispatch.xml](fixtures/expected-invalid-dispatch.xml)

<!-- fixture: fixtures/expected-invalid-dispatch.xml -->
```xml
<dispatch accepted="0" rejected="3"/>
```

### expected-invalid-exceptions.xml

[expected-invalid-exceptions.xml](fixtures/expected-invalid-exceptions.xml)

<!-- fixture: fixtures/expected-invalid-exceptions.xml -->
```xml
<exceptions>
  <order id="EMPTY" route="NORTH" missing-lines="true" unsupported-route="false" invalid-line-positions=""/>
  <order id="ZERO" route="SOUTH" missing-lines="false" unsupported-route="false" invalid-line-positions="1"/>
  <order id="UNKNOWN" route="EAST" missing-lines="false" unsupported-route="true" invalid-line-positions=""/>
</exceptions>
```

### empty-orders.xml

[empty-orders.xml](fixtures/empty-orders.xml)

<!-- fixture: fixtures/empty-orders.xml -->
```xml
<orders xmlns="urn:example:routing"/>
```

### expected-empty-dispatch.xml

[expected-empty-dispatch.xml](fixtures/expected-empty-dispatch.xml)

<!-- fixture: fixtures/expected-empty-dispatch.xml -->
```xml
<dispatch accepted="0" rejected="0"/>
```

### expected-empty-exceptions.xml

[expected-empty-exceptions.xml](fixtures/expected-empty-exceptions.xml)

<!-- fixture: fixtures/expected-empty-exceptions.xml -->
```xml
<exceptions/>
```

### duplicate-orders.xml

[duplicate-orders.xml](fixtures/duplicate-orders.xml)

<!-- fixture: fixtures/duplicate-orders.xml -->
```xml
<orders xmlns="urn:example:routing">
  <order id="R1" route="NORTH"><line sku="FILTER-A" quantity="1"/></order>
  <order id="R1" route="SOUTH"><line sku="FILTER-A" quantity="1"/></order>
</orders>
```

### missing-id-orders.xml

[missing-id-orders.xml](fixtures/missing-id-orders.xml)

<!-- fixture: fixtures/missing-id-orders.xml -->
```xml
<orders xmlns="urn:example:routing">
  <order route="NORTH"><line sku="FILTER-A" quantity="1"/></order>
</orders>
```

## Verification, alternatives and next reading

`OrderRoutingReferenceGuideTests` compares complete principal and secondary trees
for normal, all-invalid and empty input, checks the secondary URI, and verifies that
both identity refusals raise ROUTING-ID before XSLT is invoked, plus default-3.1 refusal of the planning expression. XML formatting and namespace
declaration placement are ignored; element/attribute values and order are checked.

An XPath 3.1 implementation can use `some ... satisfies`, `every ... satisfies`,
positional `for` expressions, distinct-values and explicit recursive batching.
Those are valid alternatives but require separate grouping logic. `xsl:for-each-group`
with group-by route would combine nonadjacent routes, changing this contract.
`partition` splits consecutive runs; it does not perform arbitrary group-by or
boolean bipartitioning.

Read [higher-order selection](../../functions/higher-order-selection.md),
[consecutive partitioning](../../functions/consecutive-partition.md), and
[sequence identity checks](../../functions/sequence-distinctness.md) next.
For joins and grouping, return to [order reconciliation](../order-reconciliation/README.md).
For maintained documentation gaps, see the [completeness audit](../../COMPLETENESS-AUDIT.md).

## Complete XPath 4.0 planning expression

```xquery
(: Author: Charles Korthout; 11 October 2026; Apache-2.0; Copyright Fytala.
   Context: document with urn:example:routing orders. Compile at XPath40.
   Result: map containing original orders, accepted orders, arrays of batches,
   and exception maps. No outputs or external resource access.
   Missing/duplicate identity raises Q{urn:example:routing}ROUTING-ID before XSLT.
   Predicates are boolean; castable guards bad quantities; empty orders are invalid. :)
let $orders := Q{urn:example:routing}orders/Q{urn:example:routing}order,
    $valid-line := function($line) {
      normalize-space($line/@sku) ne '' and
      (if ($line/@quantity castable as xs:integer)
       then xs:integer($line/@quantity) gt 0 else false())
    },
    $valid-order := function($order) {
      $order/@route = ('NORTH', 'SOUTH') and exists($order/Q{urn:example:routing}line)
      and every($order/Q{urn:example:routing}line,
        function($line, $position) { $valid-line($line) })
    }
return if (some($orders, function($order, $position) { normalize-space($order/@id) = '' })
           or not(all-different($orders/@id ! string(.))))
then error(QName('urn:example:routing', 'ROUTING-ID'), 'ROUTING-ID: order IDs must be nonblank and unique.')
else let $accepted := $orders[$valid-order(.)]
return map {
  'orders': $orders,
  'accepted': $accepted,
  'batches': partition($accepted, function($current, $next, $position) {
    count($current) ge 2 or $current[1]/@route ne $next/@route
  }),
  'exceptions': for $order in $orders[not($valid-order(.))] return map {
    'order': $order,
    'missing-lines': empty($order/Q{urn:example:routing}line),
    'unsupported-route': not($order/@route = ('NORTH', 'SOUTH')),
    'invalid-lines': index-where($order/Q{urn:example:routing}line,
      function($line, $position) { not($valid-line($line)) })
  }
}
```
