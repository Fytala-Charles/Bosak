<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak URI and import utility contract reference">
  <br><br><h1>Inventory Import Links: Detailed Example</h1>
  <p>Component links, decoding, collations and import diagnostics</p>
</div>

## Business case and input policy

An inventory importer receives item codes in raw and percent-encoded forms plus reported quantities. It needs a diagnostic report that preserves source order, compares decoded codes with their raw counterparts under an available collation, flags failed numeric conversion, and builds one item route per valid quantity. R1's code contains a slash that belongs inside one path segment; R2's quantity 'unknown' is deliberately rejected; R3's zero quantity is valid. This example creates report text and performs no inventory update or HTTP request.

Each item has a stable id, rawCode, encodedCode and quantity. The raw/encoded pair is intentionally supplied so the XPath 3.1 comparison needs no general decoder. Validate unique/nonempty IDs, present single code values, code-pair agreement and a defined nonnegative finite quantity/range/unit policy before production use. The fixture exposes failed numeric conversion, but is-NaN alone will not reject infinity or negative/out-of-range values. Production ingestion must handle those conditions separately. Preserve raw encoding if audit fidelity matters; replacement characters from decoding do not prove valid source bytes. Decode exactly once at a defined boundary.

The host inventory.example is synthetic and fixed. XPath 4.0 builds a hierarchical HTTPS route, removes default port 443 and escapes each code as one path segment. XPath 3.1 constructs that fixed route with concat and encode-for-uri. This baseline agrees for the fixture codes A/B, C D and E; it is not a general URI-record serializer. build-uri's selective encoding differs from encode-for-uri for other characters, including literal plus and Unicode.

The requested collation is HTML ASCII case-insensitive. XPath 4.0 probes recognition and explicitly falls back to codepoint when unavailable. XPath 3.1 uses a configured allowlist containing only codepoint and HTML ASCII case-insensitive for these fixtures; it does not provide a general runtime availability query. The host must establish those allowed collations when configuring a different processor. compare then tests decoded/raw equality under the selected collation. identity passes nodes through without copying; the portable baseline simply retains the source sequence. number converts quantity text; 4.0 checks is-NaN, while 3.1 uses self-inequality for this numeric value.

| Report field | Meaning |
|---|---|
| items | Source item count, including rows flagged invalid. |
| collation | requested if recognized/allowed; fallback otherwise. |
| quantity | Converted valid sample quantity; zero remains valid. |
| match | compare result, zero for equal fixture code pairs. |
| uri | Synthetic HTTPS item route, slash in a code encoded as %2F. |
| invalid-quantity | Failed numeric conversion; no route emitted for that row. |

## Complete inputs, expressions and exact outputs

### Normal import

Complete input:

```xml
<imports><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R3" rawCode="E" encodedCode="E" quantity="0"/></imports>
```

**XPath 31 — [report-31.xpath](report-31.xpath):**

```xquery
let $source := parse-xml('<imports><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R3" rawCode="E" encodedCode="E" quantity="0"/></imports>')/imports/item
return let $candidate := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $available := $candidate = ('http://www.w3.org/2005/xpath-functions/collation/codepoint','http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := $source
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := string($r/@rawCode)
    return if ($quantity ne $quantity) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',concat('https://inventory.example/v1/items/',encode-for-uri($code)))
,'|'))
```

Exact output:

```text
items=3;collation=requested;details=R1:quantity=42.5,match=0,uri=https://inventory.example/v1/items/A%2FB|R2:invalid-quantity|R3:quantity=0,match=0,uri=https://inventory.example/v1/items/E
```

**XPath 40 — [report-40.xpath](report-40.xpath):**

```xquery
let $source := parse-xml('<imports><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R3" rawCode="E" encodedCode="E" quantity="0"/></imports>')/imports/item
return let $candidate := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $available := collation-available($candidate)
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := identity($source)
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := decode-from-uri(string($r/@encodedCode))
    return if (is-NaN($quantity)) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',build-uri(map{'scheme':'https','host':'inventory.example','port':443,'path-segments':('','v1','items',$code)},map{'omit-default-ports':true()}))
,'|'))
```

Exact output:

```text
items=3;collation=requested;details=R1:quantity=42.5,match=0,uri=https://inventory.example/v1/items/A%2FB|R2:invalid-quantity|R3:quantity=0,match=0,uri=https://inventory.example/v1/items/E
```

### Empty import

Complete input:

```xml
<imports/>
```

**XPath 31 — [report-empty-31.xpath](report-empty-31.xpath):**

```xquery
let $source := parse-xml('<imports/>')/imports/item
return let $candidate := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $available := $candidate = ('http://www.w3.org/2005/xpath-functions/collation/codepoint','http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := $source
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := string($r/@rawCode)
    return if ($quantity ne $quantity) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',concat('https://inventory.example/v1/items/',encode-for-uri($code)))
,'|'))
```

Exact output:

```text
items=0;collation=requested;details=
```

**XPath 40 — [report-empty-40.xpath](report-empty-40.xpath):**

```xquery
let $source := parse-xml('<imports/>')/imports/item
return let $candidate := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $available := collation-available($candidate)
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := identity($source)
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := decode-from-uri(string($r/@encodedCode))
    return if (is-NaN($quantity)) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',build-uri(map{'scheme':'https','host':'inventory.example','port':443,'path-segments':('','v1','items',$code)},map{'omit-default-ports':true()}))
,'|'))
```

Exact output:

```text
items=0;collation=requested;details=
```

### Reversed items

Complete input:

```xml
<imports><item id="R3" rawCode="E" encodedCode="E" quantity="0"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/></imports>
```

**XPath 31 — [report-reordered-31.xpath](report-reordered-31.xpath):**

```xquery
let $source := parse-xml('<imports><item id="R3" rawCode="E" encodedCode="E" quantity="0"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/></imports>')/imports/item
return let $candidate := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $available := $candidate = ('http://www.w3.org/2005/xpath-functions/collation/codepoint','http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := $source
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := string($r/@rawCode)
    return if ($quantity ne $quantity) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',concat('https://inventory.example/v1/items/',encode-for-uri($code)))
,'|'))
```

Exact output:

```text
items=3;collation=requested;details=R3:quantity=0,match=0,uri=https://inventory.example/v1/items/E|R2:invalid-quantity|R1:quantity=42.5,match=0,uri=https://inventory.example/v1/items/A%2FB
```

**XPath 40 — [report-reordered-40.xpath](report-reordered-40.xpath):**

```xquery
let $source := parse-xml('<imports><item id="R3" rawCode="E" encodedCode="E" quantity="0"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/></imports>')/imports/item
return let $candidate := 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'
return let $available := collation-available($candidate)
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := identity($source)
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := decode-from-uri(string($r/@encodedCode))
    return if (is-NaN($quantity)) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',build-uri(map{'scheme':'https','host':'inventory.example','port':443,'path-segments':('','v1','items',$code)},map{'omit-default-ports':true()}))
,'|'))
```

Exact output:

```text
items=3;collation=requested;details=R3:quantity=0,match=0,uri=https://inventory.example/v1/items/E|R2:invalid-quantity|R1:quantity=42.5,match=0,uri=https://inventory.example/v1/items/A%2FB
```

### Unavailable requested collation

Complete input:

```xml
<imports><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R3" rawCode="E" encodedCode="E" quantity="0"/></imports>
```

**XPath 31 — [report-fallback-31.xpath](report-fallback-31.xpath):**

```xquery
let $source := parse-xml('<imports><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R3" rawCode="E" encodedCode="E" quantity="0"/></imports>')/imports/item
return let $candidate := 'urn:inventory:unknown'
return let $available := $candidate = ('http://www.w3.org/2005/xpath-functions/collation/codepoint','http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := $source
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := string($r/@rawCode)
    return if ($quantity ne $quantity) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',concat('https://inventory.example/v1/items/',encode-for-uri($code)))
,'|'))
```

Exact output:

```text
items=3;collation=fallback;details=R1:quantity=42.5,match=0,uri=https://inventory.example/v1/items/A%2FB|R2:invalid-quantity|R3:quantity=0,match=0,uri=https://inventory.example/v1/items/E
```

**XPath 40 — [report-fallback-40.xpath](report-fallback-40.xpath):**

```xquery
let $source := parse-xml('<imports><item id="R1" rawCode="A/B" encodedCode="A%2FB" quantity="42.5"/><item id="R2" rawCode="C D" encodedCode="C%20D" quantity="unknown"/><item id="R3" rawCode="E" encodedCode="E" quantity="0"/></imports>')/imports/item
return let $candidate := 'urn:inventory:unknown'
return let $available := collation-available($candidate)
return let $effective := if ($available) then $candidate else 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $rows := identity($source)
return concat('items=',count($rows),';collation=',if ($available) then 'requested' else 'fallback',';details=',string-join(
  for $r in $rows return
    let $quantity := number($r/@quantity)
    return let $code := decode-from-uri(string($r/@encodedCode))
    return if (is-NaN($quantity)) then concat($r/@id,':invalid-quantity')
    else concat($r/@id,':quantity=',$quantity,',match=',compare($code,string($r/@rawCode),$effective),',uri=',build-uri(map{'scheme':'https','host':'inventory.example','port':443,'path-segments':('','v1','items',$code)},map{'omit-default-ports':true()}))
,'|'))
```

Exact output:

```text
items=3;collation=fallback;details=R1:quantity=42.5,match=0,uri=https://inventory.example/v1/items/A%2FB|R2:invalid-quantity|R3:quantity=0,match=0,uri=https://inventory.example/v1/items/E
```

The empty import still reports the configured collation choice but contains no details. Reversing items changes report order while stable IDs keep each outcome attributable. Requesting urn:inventory:unknown selects codepoint fallback without calling compare with an unrecognized URI. Neither branch changes code text or source nodes. Failed quantity conversion is reported per row; the complete document remains available to the caller for follow-up.

## Focused contract fixtures

The [contract review](../../URI-UTILITY-CONTRACT-REVIEW.md) links the immutable specification and explains absent keywords, boolean-option tolerance, empty-query precedence, one-pass decoding and NaN/type boundaries. The [manifest](../cases.json) records exact case IDs/results. The diagnostics below are separate from validated business policies; successful 4.0 fixtures also verify default 3.1 refusal.

| Runnable case | Mode | Exact result/error |
|---|---|---|
| [build-uri-keyword-31.xpath](build-uri-keyword-31.xpath) | 31 | `XPST0003` |
| [build-uri-keyword-40.xpath](build-uri-keyword-40.xpath) | 40 | `XPST0017` |
| [build-uri-keyword-experimental.xpath](build-uri-keyword-experimental.xpath) | experimental | `XPST0017` |
| [decode-from-uri-keyword-31.xpath](decode-from-uri-keyword-31.xpath) | 31 | `XPST0003` |
| [decode-from-uri-keyword-40.xpath](decode-from-uri-keyword-40.xpath) | 40 | `XPST0017` |
| [decode-from-uri-keyword-experimental.xpath](decode-from-uri-keyword-experimental.xpath) | experimental | `XPST0017` |
| [collation-available-keyword-31.xpath](collation-available-keyword-31.xpath) | 31 | `XPST0003` |
| [collation-available-keyword-40.xpath](collation-available-keyword-40.xpath) | 40 | `XPST0017` |
| [collation-available-keyword-experimental.xpath](collation-available-keyword-experimental.xpath) | experimental | `XPST0017` |
| [identity-keyword-31.xpath](identity-keyword-31.xpath) | 31 | `XPST0003` |
| [identity-keyword-40.xpath](identity-keyword-40.xpath) | 40 | `XPST0017` |
| [identity-keyword-experimental.xpath](identity-keyword-experimental.xpath) | experimental | `XPST0017` |
| [is-NaN-keyword-31.xpath](is-NaN-keyword-31.xpath) | 31 | `XPST0003` |
| [is-NaN-keyword-40.xpath](is-NaN-keyword-40.xpath) | 40 | `XPST0017` |
| [is-NaN-keyword-experimental.xpath](is-NaN-keyword-experimental.xpath) | experimental | `XPST0017` |
| [build-uri-omitted.xpath](build-uri-omitted.xpath) | 40 | `https://inventory.example:443/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-empty.xpath](build-uri-empty.xpath) | 40 | `https://inventory.example:443/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-empty-map.xpath](build-uri-empty-map.xpath) | 40 | `https://inventory.example:443/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-omit-port.xpath](build-uri-omit-port.xpath) | 40 | `https://inventory.example/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-string-false.xpath](build-uri-string-false.xpath) | 40 | `https://inventory.example/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-numeric-false.xpath](build-uri-numeric-false.xpath) | 40 | `https://inventory.example:443/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-empty-option.xpath](build-uri-empty-option.xpath) | 40 | `https://inventory.example:443/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-multiple-option.xpath](build-uri-multiple-option.xpath) | 40 | `https://inventory.example/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-unknown-option.xpath](build-uri-unknown-option.xpath) | 40 | `https://inventory.example:443/v1/items/A%2FB%20+?q=A%2BB%20C#part%201/?` |
| [build-uri-wrong-option.xpath](build-uri-wrong-option.xpath) | 40 | `XPTY0004` |
| [build-uri-empty-parts.xpath](build-uri-empty-parts.xpath) | 40 | `XPTY0004` |
| [build-uri-empty-record.xpath](build-uri-empty-record.xpath) | 40 | Empty string |
| [build-uri-wrong-array.xpath](build-uri-wrong-array.xpath) | 40 | `XPTY0004` |
| [build-uri-multiple-records.xpath](build-uri-multiple-records.xpath) | 40 | `XPTY0004` |
| [build-uri-raw-path-query.xpath](build-uri-raw-path-query.xpath) | 40 | `/raw%2Fid?q=A+B#A+B` |
| [build-uri-segments-over-path.xpath](build-uri-segments-over-path.xpath) | 40 | `/A%2FB` |
| [build-uri-parameters-over-query.xpath](build-uri-parameters-over-query.xpath) | 40 | `?q=A%2BB&q=C%20D` |
| [build-uri-empty-parameters.xpath](build-uri-empty-parameters.xpath) | 40 | `?raw=1` |
| [build-uri-password-default.xpath](build-uri-password-default.xpath) | 40 | `https://inventory.example/` |
| [build-uri-password-allowed.xpath](build-uri-password-allowed.xpath) | 40 | `https://user:secret@inventory.example/` |
| [build-uri-opaque.xpath](build-uri-opaque.xpath) | 40 | `mailto:a+b@example.org` |
| [build-uri-unc.xpath](build-uri-unc.xpath) | 40 | `file:////server/share` |
| [build-uri-unknown-field.xpath](build-uri-unknown-field.xpath) | 40 | `/items` |
| [build-uri-port-string.xpath](build-uri-port-string.xpath) | 40 | `https://inventory.example/` |
| [build-uri-unicode.xpath](build-uri-unicode.xpath) | 40 | `/café` |
| [build-uri-wrong-parameters.xpath](build-uri-wrong-parameters.xpath) | 40 | Empty string |
| [build-uri-empty-valued-parameters.xpath](build-uri-empty-valued-parameters.xpath) | 40 | `?` |
| [decode-from-uri-empty.xpath](decode-from-uri-empty.xpath) | 40 | Empty string |
| [decode-from-uri-plain.xpath](decode-from-uri-plain.xpath) | 40 | `A+B` |
| [decode-from-uri-space-plus.xpath](decode-from-uri-space-plus.xpath) | 40 | `A B+C` |
| [decode-from-uri-unicode.xpath](decode-from-uri-unicode.xpath) | 40 | `café` |
| [decode-from-uri-emoji.xpath](decode-from-uri-emoji.xpath) | 40 | `💡` |
| [decode-from-uri-malformed.xpath](decode-from-uri-malformed.xpath) | 40 | `�!` |
| [decode-from-uri-incomplete.xpath](decode-from-uri-incomplete.xpath) | 40 | `�` |
| [decode-from-uri-invalid-utf8.xpath](decode-from-uri-invalid-utf8.xpath) | 40 | `�A` |
| [decode-from-uri-xml-null.xpath](decode-from-uri-xml-null.xpath) | 40 | `�` |
| [decode-from-uri-one-pass.xpath](decode-from-uri-one-pass.xpath) | 40 | `%2F` |
| [decode-from-uri-anyURI.xpath](decode-from-uri-anyURI.xpath) | 40 | `A B` |
| [decode-from-uri-untyped.xpath](decode-from-uri-untyped.xpath) | 40 | `A B` |
| [decode-from-uri-node.xpath](decode-from-uri-node.xpath) | 40 | `A B` |
| [decode-from-uri-numeric.xpath](decode-from-uri-numeric.xpath) | 40 | `XPTY0004` |
| [decode-from-uri-multiple.xpath](decode-from-uri-multiple.xpath) | 40 | `XPTY0004` |
| [decode-from-uri-map.xpath](decode-from-uri-map.xpath) | 40 | `XPTY0004` |
| [collation-available-codepoint.xpath](collation-available-codepoint.xpath) | 40 | `true` |
| [collation-available-ascii.xpath](collation-available-ascii.xpath) | 40 | `true` |
| [collation-available-uca.xpath](collation-available-uca.xpath) | 40 | `true` |
| [collation-available-fallback.xpath](collation-available-fallback.xpath) | 40 | `true` |
| [collation-available-unknown.xpath](collation-available-unknown.xpath) | 40 | `false` |
| [collation-available-empty-string.xpath](collation-available-empty-string.xpath) | 40 | `true` |
| [collation-available-relative.xpath](collation-available-relative.xpath) | 40 | `false` |
| [collation-available-empty.xpath](collation-available-empty.xpath) | 40 | `XPTY0004` |
| [collation-available-numeric.xpath](collation-available-numeric.xpath) | 40 | `XPTY0004` |
| [collation-available-anyURI.xpath](collation-available-anyURI.xpath) | 40 | `true` |
| [collation-available-untyped.xpath](collation-available-untyped.xpath) | 40 | `true` |
| [collation-available-node.xpath](collation-available-node.xpath) | 40 | `true` |
| [collation-available-multiple.xpath](collation-available-multiple.xpath) | 40 | `XPTY0004` |
| [collation-available-map.xpath](collation-available-map.xpath) | 40 | `FOTY0013` |
| [identity-empty.xpath](identity-empty.xpath) | 40 | `0` |
| [identity-order.xpath](identity-order.xpath) | 40 | `B,A,B` |
| [identity-map.xpath](identity-map.xpath) | 40 | `A` |
| [identity-array.xpath](identity-array.xpath) | 40 | `2` |
| [identity-mixed-count.xpath](identity-mixed-count.xpath) | 40 | `3` |
| [identity-node-identity.xpath](identity-node-identity.xpath) | 40 | `true` |
| [identity-function.xpath](identity-function.xpath) | 40 | `3` |
| [identity-named-callback.xpath](identity-named-callback.xpath) | 40 | `B,A` |
| [is-NaN-double.xpath](is-NaN-double.xpath) | 40 | `true` |
| [is-NaN-float.xpath](is-NaN-float.xpath) | 40 | `true` |
| [is-NaN-number-failure.xpath](is-NaN-number-failure.xpath) | 40 | `true` |
| [is-NaN-positive-infinity.xpath](is-NaN-positive-infinity.xpath) | 40 | `false` |
| [is-NaN-negative-infinity.xpath](is-NaN-negative-infinity.xpath) | 40 | `false` |
| [is-NaN-integer.xpath](is-NaN-integer.xpath) | 40 | `false` |
| [is-NaN-decimal.xpath](is-NaN-decimal.xpath) | 40 | `false` |
| [is-NaN-string.xpath](is-NaN-string.xpath) | 40 | `false` |
| [is-NaN-untyped.xpath](is-NaN-untyped.xpath) | 40 | `false` |
| [is-NaN-node.xpath](is-NaN-node.xpath) | 40 | `false` |
| [is-NaN-boolean.xpath](is-NaN-boolean.xpath) | 40 | `false` |
| [is-NaN-empty.xpath](is-NaN-empty.xpath) | 40 | `XPTY0004` |
| [is-NaN-multiple.xpath](is-NaN-multiple.xpath) | 40 | `XPTY0004` |
| [is-NaN-map.xpath](is-NaN-map.xpath) | 40 | `FOTY0013` |
| [is-NaN-function.xpath](is-NaN-function.xpath) | 40 | `FOTY0013` |
| [is-NaN-array.xpath](is-NaN-array.xpath) | 40 | `false` |
| [is-NaN-qname.xpath](is-NaN-qname.xpath) | 40 | `false` |
