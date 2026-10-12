<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: external-input function scope and partner invoices">
  <br><br><h1>External-input function boundaries</h1>
  <p>Out-of-scope proposals — verified narrower processing paths</p>
</div>

## 1. Name and Classification

**External-input function proposals — Out of scope in the original dossier; absent in the reviewed Bosak paths.** Covers `fn:parse-html`, `fn:html-doc`, `fn:invisible-xml`, `fn:unparsed-binary` and `fn:xsd-validator`. The [REQ-118 decision](../../REQ-118-xpath-xslt-40.md) is preserved. Engine 1.1.2 reviewed against [revision bce0d9b1](https://github.com/Fytala-Charles/Bosak/commit/bce0d9b1e21ce2ed875821e592f64bf2ed69ee55).

Primary function documentation: [parse-html](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-parse-html), [html-doc](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-html-doc), [invisible-xml](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-invisible-xml), [unparsed-binary](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-unparsed-binary), [xsd-validator](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-xsd-validator). Immutable review source: [QT4CG function catalog at e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml), the five corresponding `fos:function` elements. The pin is a review source, not recovery of the missing adopted snapshot.

## 2. Intent

Define a practical boundary between acquisition, format-specific parsing, schema validation and ordinary XPath processing of normalized partner invoice data.

## 3. Also Known As

HTML parsing/loading, Invisible XML parser factories, binary resource loading and schema-validator factories. These are different services; accepting XML, binary atomic values or guarded records does not implement them.

## 4. History / Supersedes

REQ-118 excluded these functions from its implementation schedule. This review checks current absence rather than repeating the exclusion as a timeless claim. It does not remove existing XML, schema-aware XSLT/XQuery or binary-value support. No external library is installed and no engine feature is added.

## 5. Motivation

Partner invoices may arrive as an HTML fragment, an HTML URL, an attached PDF, a fixed text record or XML requiring schema validation. A consumer needs to know which work happens before Bosak evaluates XPath, and what remains a distinct service. The examples normalize invoice identity, currency and precomputed total; they do not calculate tax or exchange rates.

## 6. Language Alignment

| Function | Draft call arities reviewed | Verified Bosak boundary |
|---|---|---|
| parse-html | 1, 2 | Calls refuse `XPST0017`; lookup empty |
| html-doc | 1, 2 | Calls refuse `XPST0017`; lookup empty |
| invisible-xml | 0, 1, 2 | Calls refuse `XPST0017`; lookup empty |
| unparsed-binary | 1 | Call refuses `XPST0017`; lookup empty |
| xsd-validator | 0, 1 | Calls refuse `XPST0017`; lookup empty |

All listed calls are tested in default 3.1, frozen 4.0 and experimental modes. Lookup is tested in 3.1 and 4.0. These are missing-name results, not draft runtime errors. The factory's returned function is never invoked. Resource URLs use the reserved example domain; no network acquisition occurs in these checks.

## 7. Applicability

Use this reference when planning a partner import boundary. Keep acquisition and non-XML parsing in an explicitly selected host component, then pass supported values or normalized XML into Bosak. For XSD requirements use the existing schema-aware integration paths where applicable or an external validator with a defined contract; the business guard here is only an example of application validation.

## 8. Structure

| Function | Pinned contract | Dependency, ownership and supported alternative |
|---|---|---|
| parse-html | Accepts optional string/hexBinary/base64Binary and options; returns an HTML document node or empty | Requires an HTML parsing/recovery component and decisions about options, encoding and DOM-to-XDM mapping. The integration host owns normalization today; parse-xml accepts only well-formed XML |
| html-doc | Optional resource string and options; specified using unparsed-binary followed by parse-html | Combines resource acquisition and HTML parsing. The host owns credentials, URI resolution and acquisition policy plus the HTML component. Supplying normalized XML avoids neither of those responsibilities |
| invisible-xml | Optional grammar and options; returns a parsing function taking a string and returning an item | Needs an Invisible XML processor, grammar lifecycle and error-result mapping. The host owns the chosen processor. A fixed pipe-delimited record expression is only a narrow alternative for the declared format |
| unparsed-binary | Optional resource string; returns optional base64Binary content | Does not inherently require a third-party parser. Integration needs binary acquisition, URI/base handling, resource policy and error semantics. The host acquires bytes; existing binary casts work on values already supplied |
| xsd-validator | Optional options; returns a validator function producing an optional validation-result record | Existing schema machinery is relevant, but not this factory/result-record contract. Ownership must cover schema resolution, options, annotations and validation reporting. Existing schema-aware layers or a host validator are distinct paths |

Absence of these names is not a promise that proposed dependency work must occur in a particular layer. Any future implementation needs its own design and tests. The original HTML note about blocked remote HTTP is an environment observation, not a reason to infer that local HTML parsing would fail.

The pinned Invisible XML contract specifies FOIX0003 when no processor is available; Bosak instead refuses the unregistered name with XPST0017. This review does not certify the other functions' draft error matrices or option maps.

## 9. Participants

The integration host acquires source content and selects the format parser/validator. Bosak receives normalized XML or atomic values. An invoice report expression reads ID, currency and total. A manifest records exact refused calls and supported narrower alternatives.

## 10. Collaborations

The normalized-XML path parses a complete supplied batch and produces currency-labelled scalar rows. The fixed-record path splits exactly three unquoted fields. The binary path converts a supplied hex value to base64. The invoice guard checks explicit business predicates and reports a named application error. None invokes an unavailable draft function.

## 11. Consequences

The examples establish useful processing paths without substituting for HTML recovery, arbitrary grammar parsing, binary IO or XSD validation. The guard supports only EUR/USD and nonnegative decimal totals with a nonempty ID; it does not check uniqueness, arbitrary XML structure, schema defaults or type annotations. The fixed record permits no embedded pipe, quoted/escaped field or multiline input. Those formats need a parser designed for them.

## 12. Implementation

The inspected [FunctionLibrary.cs](https://github.com/Fytala-Charles/Bosak/blob/bce0d9b1e21ce2ed875821e592f64bf2ed69ee55/src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs) contains no registration of the five names. [CatalogExampleTests.cs](https://github.com/Fytala-Charles/Bosak/blob/bce0d9b1e21ce2ed875821e592f64bf2ed69ee55/tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) executes the manifest's name/arity refusals and alternative outcomes. This slice changes documentation/fixtures only.

Existing named evidence: `ParseXml` in [FunctionLibraryTests.cs](https://github.com/Fytala-Charles/Bosak/blob/bce0d9b1e21ce2ed875821e592f64bf2ed69ee55/tests/Bosak.XPath.Standard.Tests/FunctionLibraryTests.cs), `Binary_HexAndBase64ComparableByOctets` in [AtomicEqualTests.cs](https://github.com/Fytala-Charles/Bosak/blob/bce0d9b1e21ce2ed875821e592f64bf2ed69ee55/tests/Bosak.XPath.Standard.Tests/AtomicEqualTests.cs), and `Copy_ValidationStrict_Valid_Succeeds` / `Copy_ValidationStrict_Invalid_Xtte1510` in [SchemaAwareValidationTests.cs](https://github.com/Fytala-Charles/Bosak/blob/bce0d9b1e21ce2ed875821e592f64bf2ed69ee55/tests/Bosak.Xslt.Tests/SchemaAwareValidationTests.cs). Those establish existing narrower paths; they do not prove these factory functions. Read [integration](../../INTEGRATION.md) for the schema/resource API contract. No route or extension function is advertised for the absent names.

55 new checks comprise 30 calls, ten lookup outcomes and fifteen alternative/edge cases. The 117 registered-name audit remains unchanged: these absent proposals are outside that scoped inventory.

## 13. Sample Code

The partner's fragment can contain HTML that is not XML:

```html
<p>Invoice<br>INV-1042</p>
```

The complete [XML refusal expression](../examples/external-input-functions/xml-is-not-html.xpath) raises `FODC0006`; parse-xml is not an HTML5 parser. The integration host instead supplies a well-formed normalized invoice batch:

```xml
<invoices><invoice id="INV-1042" currency="EUR" total="30.00"/><invoice id="INV-1043" currency="USD" total="12.00"/></invoices>
```

Complete portable XPath 3.1 report:

```xpath
(: Host supplies well-formed invoice XML after acquisition/HTML normalization. :)
let $batch := parse-xml('<invoices><invoice id="INV-1042" currency="EUR" total="30.00"/><invoice id="INV-1043" currency="USD" total="12.00"/></invoices>')
return string-join($batch/invoices/invoice ! concat(@id, '/', @currency, ':', format-number(xs:decimal(@total), '0.00')), ';')
```

Exact output:

```text
INV-1042/EUR:30.00;INV-1043/USD:12.00
```

Empty input:

```xml
<invoices/>
```

The [complete empty expression](../examples/external-input-functions/normalized-xml-empty.xpath) returns the empty string. Reordered input:

```xml
<invoices><invoice id="INV-1043" currency="USD" total="12.00"/><invoice id="INV-1042" currency="EUR" total="30.00"/></invoices>
```

The [complete reordered expression](../examples/external-input-functions/normalized-xml-reordered.xpath) returns:

```text
INV-1043/USD:12.00;INV-1042/EUR:30.00
```

Fixed partner text record:

```text
INV-1042|EUR|30.00
```

Complete narrow alternative to a grammar-parser factory:

```xpath
(: Fixed three-field partner record; no quotes, escapes, embedded pipes or grammar parser. :)
let $record := 'INV-1042|EUR|30.00'
return if ($record eq '') then '' else let $fields := tokenize($record, '\|') return concat($fields[1], '/', $fields[2], ':', format-number(xs:decimal($fields[3]), '0.00'))
```

Exact output:

```text
INV-1042/EUR:30.00
```

Already-acquired attachment bytes (the eight-byte `%PDF-1.7` prefix only, not a complete PDF or PDF validation):

```xpath
(: Bytes already acquired by the host; this cast does not fetch a resource. :)
string(xs:base64Binary(xs:hexBinary('255044462D312E37')))
```

Exact base64 output:

```text
JVBERi0xLjc=
```

Explicit business guard for an already parsed invoice:

```xml
<invoice id="INV-1042" currency="EUR" total="30.00"/>
```

Complete guard expression:

```xpath
(: Business guard only; does not validate XSD or annotate nodes. :)
let $invoice := parse-xml('<invoice id="INV-1042" currency="EUR" total="30.00"/>')/invoice
return if (string-length(string($invoice/@id)) gt 0 and $invoice/@currency = ('EUR','USD') and $invoice/@total castable as xs:decimal) then
  if (xs:decimal($invoice/@total) ge 0) then concat($invoice/@id, '/', $invoice/@currency, ':', format-number(xs:decimal($invoice/@total), '0.00'))
  else error(QName('urn:invoice', 'INVALID-INVOICE'))
else error(QName('urn:invoice', 'INVALID-INVOICE'))
```

Exact output:

```text
INV-1042/EUR:30.00
```

A zero total remains valid:

```xml
<invoice id="INV-1043" currency="USD" total="0"/>
```

Exact [zero guard](../examples/external-input-functions/invoice-guard-zero.xpath) output:

```text
INV-1043/USD:0.00
```

Invalid and missing business fields:

```xml
<invoice id="" currency="EUR" total="-1"/>
```

```xml
<invoice/>
```

Their complete [invalid](../examples/external-input-functions/invoice-guard-invalid.xpath) and [missing-field](../examples/external-input-functions/invoice-guard-empty.xpath) expressions report `INVALID-INVOICE`. This is an application error, not an XSD error.

All fixtures below are linked in full. Lookup output 0 counts the absent functions across the listed arities. Outcomes use JSON string notation to show empty values unambiguously. Refusal URLs are never fetched; complete inputs are embedded in the alternatives.

| Fixture | Mode | Exact output or error marker |
|---|---|---|
| [parse-html-31-0](../examples/external-input-functions/parse-html-31-0.xpath) | 31 | `XPST0017` |
| [parse-html-31-1](../examples/external-input-functions/parse-html-31-1.xpath) | 31 | `XPST0017` |
| [parse-html-40-0](../examples/external-input-functions/parse-html-40-0.xpath) | 40 | `XPST0017` |
| [parse-html-40-1](../examples/external-input-functions/parse-html-40-1.xpath) | 40 | `XPST0017` |
| [parse-html-experimental-0](../examples/external-input-functions/parse-html-experimental-0.xpath) | experimental | `XPST0017` |
| [parse-html-experimental-1](../examples/external-input-functions/parse-html-experimental-1.xpath) | experimental | `XPST0017` |
| [parse-html-lookup-31](../examples/external-input-functions/parse-html-lookup-31.xpath) | 31 | `"0"` |
| [parse-html-lookup-40](../examples/external-input-functions/parse-html-lookup-40.xpath) | 40 | `"0"` |
| [html-doc-31-0](../examples/external-input-functions/html-doc-31-0.xpath) | 31 | `XPST0017` |
| [html-doc-31-1](../examples/external-input-functions/html-doc-31-1.xpath) | 31 | `XPST0017` |
| [html-doc-40-0](../examples/external-input-functions/html-doc-40-0.xpath) | 40 | `XPST0017` |
| [html-doc-40-1](../examples/external-input-functions/html-doc-40-1.xpath) | 40 | `XPST0017` |
| [html-doc-experimental-0](../examples/external-input-functions/html-doc-experimental-0.xpath) | experimental | `XPST0017` |
| [html-doc-experimental-1](../examples/external-input-functions/html-doc-experimental-1.xpath) | experimental | `XPST0017` |
| [html-doc-lookup-31](../examples/external-input-functions/html-doc-lookup-31.xpath) | 31 | `"0"` |
| [html-doc-lookup-40](../examples/external-input-functions/html-doc-lookup-40.xpath) | 40 | `"0"` |
| [unparsed-binary-31-0](../examples/external-input-functions/unparsed-binary-31-0.xpath) | 31 | `XPST0017` |
| [unparsed-binary-40-0](../examples/external-input-functions/unparsed-binary-40-0.xpath) | 40 | `XPST0017` |
| [unparsed-binary-experimental-0](../examples/external-input-functions/unparsed-binary-experimental-0.xpath) | experimental | `XPST0017` |
| [unparsed-binary-lookup-31](../examples/external-input-functions/unparsed-binary-lookup-31.xpath) | 31 | `"0"` |
| [unparsed-binary-lookup-40](../examples/external-input-functions/unparsed-binary-lookup-40.xpath) | 40 | `"0"` |
| [invisible-xml-31-0](../examples/external-input-functions/invisible-xml-31-0.xpath) | 31 | `XPST0017` |
| [invisible-xml-31-1](../examples/external-input-functions/invisible-xml-31-1.xpath) | 31 | `XPST0017` |
| [invisible-xml-31-2](../examples/external-input-functions/invisible-xml-31-2.xpath) | 31 | `XPST0017` |
| [invisible-xml-40-0](../examples/external-input-functions/invisible-xml-40-0.xpath) | 40 | `XPST0017` |
| [invisible-xml-40-1](../examples/external-input-functions/invisible-xml-40-1.xpath) | 40 | `XPST0017` |
| [invisible-xml-40-2](../examples/external-input-functions/invisible-xml-40-2.xpath) | 40 | `XPST0017` |
| [invisible-xml-experimental-0](../examples/external-input-functions/invisible-xml-experimental-0.xpath) | experimental | `XPST0017` |
| [invisible-xml-experimental-1](../examples/external-input-functions/invisible-xml-experimental-1.xpath) | experimental | `XPST0017` |
| [invisible-xml-experimental-2](../examples/external-input-functions/invisible-xml-experimental-2.xpath) | experimental | `XPST0017` |
| [invisible-xml-lookup-31](../examples/external-input-functions/invisible-xml-lookup-31.xpath) | 31 | `"0"` |
| [invisible-xml-lookup-40](../examples/external-input-functions/invisible-xml-lookup-40.xpath) | 40 | `"0"` |
| [xsd-validator-31-0](../examples/external-input-functions/xsd-validator-31-0.xpath) | 31 | `XPST0017` |
| [xsd-validator-31-1](../examples/external-input-functions/xsd-validator-31-1.xpath) | 31 | `XPST0017` |
| [xsd-validator-40-0](../examples/external-input-functions/xsd-validator-40-0.xpath) | 40 | `XPST0017` |
| [xsd-validator-40-1](../examples/external-input-functions/xsd-validator-40-1.xpath) | 40 | `XPST0017` |
| [xsd-validator-experimental-0](../examples/external-input-functions/xsd-validator-experimental-0.xpath) | experimental | `XPST0017` |
| [xsd-validator-experimental-1](../examples/external-input-functions/xsd-validator-experimental-1.xpath) | experimental | `XPST0017` |
| [xsd-validator-lookup-31](../examples/external-input-functions/xsd-validator-lookup-31.xpath) | 31 | `"0"` |
| [xsd-validator-lookup-40](../examples/external-input-functions/xsd-validator-lookup-40.xpath) | 40 | `"0"` |
| [normalized-xml-main](../examples/external-input-functions/normalized-xml-main.xpath) | 31 | `"INV-1042/EUR:30.00;INV-1043/USD:12.00"` |
| [normalized-xml-empty](../examples/external-input-functions/normalized-xml-empty.xpath) | 31 | `""` |
| [normalized-xml-reordered](../examples/external-input-functions/normalized-xml-reordered.xpath) | 31 | `"INV-1043/USD:12.00;INV-1042/EUR:30.00"` |
| [xml-is-not-html](../examples/external-input-functions/xml-is-not-html.xpath) | 31 | `FODC0006` |
| [binary-main](../examples/external-input-functions/binary-main.xpath) | 31 | `"JVBERi0xLjc="` |
| [binary-empty](../examples/external-input-functions/binary-empty.xpath) | 31 | `""` |
| [binary-zero-octet](../examples/external-input-functions/binary-zero-octet.xpath) | 31 | `"AA=="` |
| [binary-invalid](../examples/external-input-functions/binary-invalid.xpath) | 31 | `FORG0001` |
| [fixed-record-main](../examples/external-input-functions/fixed-record-main.xpath) | 31 | `"INV-1042/EUR:30.00"` |
| [fixed-record-empty](../examples/external-input-functions/fixed-record-empty.xpath) | 31 | `""` |
| [fixed-record-second](../examples/external-input-functions/fixed-record-second.xpath) | 31 | `"INV-1043/USD:12.00"` |
| [invoice-guard-main](../examples/external-input-functions/invoice-guard-main.xpath) | 31 | `"INV-1042/EUR:30.00"` |
| [invoice-guard-zero](../examples/external-input-functions/invoice-guard-zero.xpath) | 31 | `"INV-1043/USD:0.00"` |
| [invoice-guard-invalid](../examples/external-input-functions/invoice-guard-invalid.xpath) | 31 | `INVALID-INVOICE` |
| [invoice-guard-empty](../examples/external-input-functions/invoice-guard-empty.xpath) | 31 | `INVALID-INVOICE` |


## 14. Performance

The checks operate entirely on in-memory literals. No network availability, parser throughput, schema-cache performance or bounded-memory claim is made. Production hosts should define acquisition and parser lifecycles separately from expression compilation.

## 15. Known Uses

The examples model partner invoice normalization, attachment transport and business validation. They are not evidence of a named production deployment or of a selected external dependency. No third-party package is recommended or installed by this slice.

## 16. Related Features

See [Serialization parameter boundaries](serialization-parameters.md), [supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md), [completeness audit](../COMPLETENESS-AUDIT.md) and [integration](../../INTEGRATION.md). The separate GNode/JNode and language-boundary dispositions remain to be reviewed.
