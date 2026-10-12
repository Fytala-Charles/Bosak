<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: invoice API URI components and encoding">
  <br><br><h1>Invoice API URIs</h1>
  <p>Frozen 4.0 — parse components, build routes and decode values</p>
</div>

## 1. Name and Classification

**Invoice API URIs — Frozen 4.0.** [fn:parse-uri](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-parse-uri), [fn:build-uri](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-uri), and [fn:decode-from-uri](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-decode-from-uri).
Initial engine delivery: [PR #84](https://github.com/Fytala-Charles/Bosak/pull/84),
[revision f7f55b4e](https://github.com/Fytala-Charles/Bosak/commit/f7f55b4e3a6e92da26ac607e34f46aede695cb19), REQ-118 4.0-S2.
Definitions reviewed **2026-10-11** against immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.
General syntax references: [RFC 3986](https://www.rfc-editor.org/rfc/rfc3986) and
[RFC 3987](https://www.rfc-editor.org/rfc/rfc3987). The pinned function draft
also specifies convenient handling for some URI-adjacent strings.

## 2. Intent

Construct an invoice API route from a configured endpoint, an invoice identifier
and a search value. Preserve a slash inside the identifier as data in one path
segment and distinguish a literal plus from a query-parameter space.

## 3. Also Known As

URI decomposition, component assembly and percent decoding are descriptive
terms. No aliases are registered. Form decoding is a distinct convention used
for query parameters, not a synonym for generic URI decoding.

## 4. History / Supersedes

XPath 3.1 can assemble a fixed route with `concat` and `encode-for-uri` on each
identifier/query value. The complete comparison below constructs the same URI
for this configured HTTPS `/v1` endpoint. Its explicit removal of port 443 is
scoped to that fixture, not a general-purpose URI parser. XPath 3.1 has no direct
standard counterpart to the complete component-record parser or UTF-8 percent
decoder; a host implementation or dedicated helper is needed for general decoding.

## 5. Motivation

An invoice ID such as INV/2048 may contain a slash as part of its business key.
Concatenating it into the route without segment encoding creates two route
segments. Likewise, a plus in a search value must not turn into a space when
the server form-decodes query parameters. Component-based assembly makes those
boundaries explicit before the host uses the resulting address.

## 6. Language Alignment

`parse-uri` returns an optional map with raw and derived fields, including scheme,
host, integer port, path, decoded path-segments, query, decoded query-parameters
and fragment. Repeated query keys collect a sequence of values. Parsing query
parameters applies form decoding: plus becomes space before percent decoding.
An encoded `%2B` becomes a literal plus. Empty input returns empty.

`build-uri` takes a component map. Nonempty path-segments take precedence over
path in the demonstrated cases; a nonempty query-parameters map takes precedence
over query. A leading empty segment supplies the initial slash. Each hierarchical
segment is encoded independently, so a slash inside a segment becomes `%2F`.
Query values use their own delimiter-encoding rules. Non-hierarchical schemes
such as mailto have different assembly rules; this entry's business route is HTTPS.

`decode-from-uri` percent-decodes UTF-8 and leaves ordinary plus signs unchanged.
Empty input becomes a zero-length string, unlike parse-uri's empty result.
Malformed escapes, invalid UTF-8 and XML-invalid characters become U+FFFD
replacement characters in the specified cases, rather than throwing a strict
validation error. Do not decode the entire address before identifying its boundaries.

The functions have optional options maps for parsing/building. This case uses
`omit-default-ports` to omit HTTPS port 443; a nondefault port is preserved.
The entry verifies that option and default behavior, not every option/signature.

## 7. Applicability

Use for component-aware integration addresses and decoded values. The fixture
assumes a validated HTTPS endpoint with the known `/v1` route, a nonempty invoice
ID, and a supplied search value. The parser accepts relative references and
URI-adjacent strings; successful parsing is not proof that an address is an
approved API endpoint or a valid network destination. The host owns endpoint
validation, relative-reference resolution and actual network requests.

Do not infer a canonical signed-request spelling from a parse/build roundtrip.
Keep original wire text when a protocol requires its exact bytes or query order.
Generic decoding is permissive and may lose malformed input through replacement.

## 8. Structure

Read the request, parse its base endpoint with default-port omission, then build
scheme/host/port, separate route segments and a one-key query map. Parse the
built URI to verify that invoice ID and search text still have their original
values. No HTTP operation is performed.

## 9. Participants

The request supplies endpoint and business values. The parser exposes endpoint
components. The builder escapes component data and joins delimiters. The parsed
result and report demonstrate the roundtrip of the business values.

## 10. Collaborations

Pass raw component values to path-segments/query-parameters; do not pre-escape
them and then ask the builder to escape again. The fixed 3.1 comparison instead
calls `encode-for-uri` before concatenating delimiters. The decoder is useful
for a component already isolated from its address; query form handling requires
the plus-to-space step first. The form-decoding fixture illustrates that order.

## 11. Consequences

Slash, space, question mark, hash and plus are preserved as component data in
the verified cases. Parsing/building may change spelling: spaces become escaped,
query plus may rebuild as `%20`, and configured default ports may disappear.
Repeated query values remain sequences; map iteration does not promise a
portable query-key order. The exact builder fixtures use one query key so they
do not imply such an ordering contract. Semantic value preservation differs
from byte-for-byte roundtripping.

## 12. Implementation

Use `CompileOptions.Compatibility40` for the new functions. The default XPath
3.1 profile rejects them. The fixed route comparison uses `Compatibility31`.
The existing [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks exact results/errors and default-profile refusal for successful 4.0 cases.
All inputs are inline, and the example hostname is illustrative. No external
context item, variables, files or HTTP request are required.

**Supplementary contract review (2026-10-12):** [URI/import utility review](../URI-UTILITY-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed import/link README](../examples/utility-contracts/README.md) supply 105 checks across five families: keyword modes, component/options/escaping, one-pass UTF-8 decoding, collation availability, identity, NaN and type/cardinality boundaries, plus complete XPath 3.1/4.0 reports. Keywords remain unavailable. Boolean-option/record tolerance and empty query-map precedence remain follow-ups. All 37 absent-keyword families now have first targeted review, with metadata still absent; this does not close D2. Source/public version is 1.1.3; original delivery/unit/gate and historical adopted-pin limits remain.

## 13. Sample Code

Complete synthetic input:

```xml
<request endpoint="https://api.example.test:443/v1" invoice="INV/2048" query="Ada Lovelace + VAT"/>
```

[Complete XPath 4.0 expression](../examples/invoice-api-uris/main.xpath):

```xpath
let $request := parse-xml('<request endpoint="https://api.example.test:443/v1" invoice="INV/2048" query="Ada Lovelace + VAT"/>')/request
let $base := parse-uri(string($request/@endpoint), map {'omit-default-ports': true()})
let $uri := build-uri(map {'scheme': $base?scheme, 'host': $base?host, 'port': $base?port, 'path-segments': ('', 'v1', 'invoices', string($request/@invoice)), 'query-parameters': map {'q': string($request/@query)}})
let $parsed := parse-uri($uri)
return concat('uri=', $uri, '; invoice=', $parsed?path-segments[4], '; query=', $parsed?query-parameters?q)
```

Exact output:

```text
uri=https://api.example.test/v1/invoices/INV%2F2048?q=Ada%20Lovelace%20%2B%20VAT; invoice=INV/2048; query=Ada Lovelace + VAT
```

[Complete XPath 3.1 construction comparison](../examples/invoice-api-uris/baseline.xpath):

```xpath
let $request := parse-xml('<request endpoint="https://api.example.test:443/v1" invoice="INV/2048" query="Ada Lovelace + VAT"/>')/request
return let $origin := replace(substring-before(string($request/@endpoint), '/v1'), ':443$', '')
return let $uri := concat($origin, '/v1/invoices/', encode-for-uri(string($request/@invoice)), '?q=', encode-for-uri(string($request/@query)))
return $uri
```

Exact output (URI construction only):

```text
https://api.example.test/v1/invoices/INV%2F2048?q=Ada%20Lovelace%20%2B%20VAT
```

### Delimiters inside the identifier

Full input contains both a question mark and a hash in the invoice ID:

```xml
<request endpoint="https://api.example.test:443/v1" invoice="INV?2048#A" query="Ada Lovelace + VAT"/>
```

The [delimiter fixture](../examples/invoice-api-uris/delimiter-in-id.xpath) reports:

```text
uri=https://api.example.test/v1/invoices/INV%3F2048%23A?q=Ada%20Lovelace%20%2B%20VAT; invoice=INV?2048#A; query=Ada Lovelace + VAT
```

### Complete fixture inventory

Each file embeds its full input and exact result or diagnostic marker. `�` below
is the literal Unicode replacement character U+FFFD.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/invoice-api-uris/main.xpath) | `uri=https://api.example.test/v1/invoices/INV%2F2048?q=Ada%20Lovelace%20%2B%20VAT; invoice=INV/2048; query=Ada Lovelace + VAT` |
| [baseline](../examples/invoice-api-uris/baseline.xpath) | `https://api.example.test/v1/invoices/INV%2F2048?q=Ada%20Lovelace%20%2B%20VAT` |
| [space-in-id](../examples/invoice-api-uris/space-in-id.xpath) | `uri=https://api.example.test/v1/invoices/INV%202048?q=Ada%20Lovelace%20%2B%20VAT; invoice=INV 2048; query=Ada Lovelace + VAT` |
| [delimiter-in-id](../examples/invoice-api-uris/delimiter-in-id.xpath) | `uri=https://api.example.test/v1/invoices/INV%3F2048%23A?q=Ada%20Lovelace%20%2B%20VAT; invoice=INV?2048#A; query=Ada Lovelace + VAT` |
| [duplicate-query](../examples/invoice-api-uris/duplicate-query.xpath) | `ready,review` |
| [build-duplicate-query](../examples/invoice-api-uris/build-duplicate-query.xpath) | `https://api.example.test/?tag=ready&tag=review` |
| [query-plus](../examples/invoice-api-uris/query-plus.xpath) | `Ada Lovelace` |
| [literal-plus](../examples/invoice-api-uris/literal-plus.xpath) | `Ada+Lovelace` |
| [decode-plus](../examples/invoice-api-uris/decode-plus.xpath) | `Ada+Lovelace` |
| [decode-form](../examples/invoice-api-uris/decode-form.xpath) | `Ada Lovelace+` |
| [decode-utf8](../examples/invoice-api-uris/decode-utf8.xpath) | `café` |
| [encode-decode](../examples/invoice-api-uris/encode-decode.xpath) | `INV/2048 + VAT` |
| [malformed-escape](../examples/invoice-api-uris/malformed-escape.xpath) | `�!` |
| [invalid-utf8](../examples/invoice-api-uris/invalid-utf8.xpath) | `�A` |
| [decode-empty](../examples/invoice-api-uris/decode-empty.xpath) | `0` |
| [parse-empty](../examples/invoice-api-uris/parse-empty.xpath) | `0` |
| [default-port](../examples/invoice-api-uris/default-port.xpath) | `0` |
| [custom-port](../examples/invoice-api-uris/custom-port.xpath) | `8443` |
| [port-type](../examples/invoice-api-uris/port-type.xpath) | `true` |
| [relative-reference](../examples/invoice-api-uris/relative-reference.xpath) | `scheme=false; path=../invoices/INV2048; query=ready` |
| [fragment-only](../examples/invoice-api-uris/fragment-only.xpath) | `pathCount=0; fragment=summary view` |
| [ipv6-host](../examples/invoice-api-uris/ipv6-host.xpath) | `[2001:db8::7]` |
| [normalized-roundtrip](../examples/invoice-api-uris/normalized-roundtrip.xpath) | `https://api.example.test/p%20x?q=Ada%20Lovelace#summary%20view` |
| [segment-precedence](../examples/invoice-api-uris/segment-precedence.xpath) | `https://api.example.test/invoices/INV%2F2048` |
| [query-precedence](../examples/invoice-api-uris/query-precedence.xpath) | `https://api.example.test?q=ready` |
| [nonhierarchical](../examples/invoice-api-uris/nonhierarchical.xpath) | `mailto:accounts@example.test` |
| [unmatched-ipv6](../examples/invoice-api-uris/unmatched-ipv6.xpath) | `FOUR0001` |
| [build-nonmap](../examples/invoice-api-uris/build-nonmap.xpath) | `XPTY0004` |

The duplicate-key fixtures parse and rebuild ready/review tags in one key's
value sequence. Plus fixtures distinguish raw plus, encoded plus, generic
percent decoding and query form decoding. UTF-8 and malformed-input fixtures
verify decoding behavior. Relative/fragment references demonstrate that parsing
alone does not establish an absolute API address. Port and IPv6 fixtures verify
component types and authority structure. Precedence and normalized-roundtrip
fixtures explain how assembly chooses fields and changes textual spelling.

## 14. Performance

Parsing and assembly process supplied strings and component values synchronously.
Repeated query keys create sequence-valued map entries. These functions do not
resolve DNS, access files or fetch resources. No benchmark or universal complexity
claim is implied; large inputs still require memory for component strings/maps.

## 15. Known Uses

This invoice API route is a synthetic integration example. Existing evidence includes
[`ParseUri_HierarchicalHttp_AllFields`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ParseUri_PortIsInteger`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ParseUri_QueryParametersAccumulateDuplicateKeys`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ParseUri_OmitDefaultPorts`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ParseUri_UnmatchedBracketInAuthority_RaisesFour0001`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildUri_ParseRoundTrip_ReencodesDelimiters`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildUri_PathSegmentsEscapedOnlyWhenHierarchical`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildUri_QueryParameters_EscapeAndEmptyKey`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`DecodeFromUri_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`DecodeFromUri_InvalidEscapesAndInvalidUtf8_BecomeReplacementChar`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
has no exact fn-parse-uri, fn-build-uri or fn-decode-from-uri assignment in GatedSets.
Named tests and these 28 executable examples provide separate evidence. No gate
membership changes are included.

## 16. Related Features

[Archive and partner URI policy](uri-option-policy.md) extends this reference
with Boolean option contracts, UNC interpretation and raw/structured component
precedence, including observed option-validation and empty-query-map limits.

[Configured label callbacks](configured-label-callbacks.md) covers reusable
configuration values. [Timestamp deadlines](timestamp-deadlines.md) covers
other integration-unit conversions. [The completeness audit](../COMPLETENESS-AUDIT.md)
tracks remaining reference names, separate from exhaustive option/signature coverage.
