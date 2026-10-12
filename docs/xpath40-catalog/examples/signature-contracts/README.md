<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak invoice fingerprint and keyword contract examples">
  <br><br><h1>Invoice Fingerprints and Keyword Contracts</h1>
  <p>Full input/output, defaults, option policy and observed boundaries</p>
</div>

## Business case and data contract

A receiving service constructs a stable fingerprint from an already parsed invoice. It concatenates the invoice ID, currency and supplied total text with `|`, then hashes the UTF-8 bytes. This example deliberately preserves 125.00 text; it does not canonicalize arbitrary XML, sign an invoice or normalize equivalent decimal spellings. Reordering/adding fields, whitespace and encoding policies must be agreed by the consumer before treating fingerprints as business identities.

Complete input, embedded in every business report:

```xml
<invoice id="INV-42" currency="EUR"><total>125.00</total></invoice>
```

The id and currency attributes plus total element provide the three payload fields. parse-xml builds the document; string-join atomizes the selected nodes. hash receives the resulting string, SHA-256 and an empty options map; concat appends its hexBinary result in the report.

Complete runnable [positional report](hash-business-positional.xpath):

```xquery
let $invoice := parse-xml('<invoice id="INV-42" currency="EUR"><total>125.00</total></invoice>')/invoice let $payload := string-join(($invoice/@id, $invoice/@currency, $invoice/total), '|') return concat($payload, ';SHA-256=', hash($payload, 'SHA-256', map{}))
```

Exact output:

```text
INV-42|EUR|125.00;SHA-256=8D8F32B72196A44EF9860012FA1B67B3BC651435F52D9AC9C9BF3EA5A11A5176
```

Independent host-side comparison for a 3.1 consumer:

```python
import hashlib
payload = 'INV-42|EUR|125.00'
print(payload + ';SHA-256=' + hashlib.sha256(payload.encode('utf-8')).hexdigest().upper())
```

It produces the same output. XPath 3.1 has no standard hash function; this host calculation does not reproduce Bosak's result-type or version checks.

## Keyword and default boundary

Keyword spelling allows parameters to be supplied out of order. Simple hash keyword calls work, including the omitted algorithm default MD5, empty algorithm default, empty options, and reordered arguments. Missing required, unknown and duplicate keyword arguments refuse XPST0017. Examples use MD5 only to verify the declared default, not to choose an invoice security policy.

The complete [keyword business report](hash-business-report.xpath) currently refuses XPST0017 for hash#0:

```xquery
let $invoice := parse-xml('<invoice id="INV-42" currency="EUR"><total>125.00</total></invoice>')/invoice let $payload := string-join(($invoice/@id, $invoice/@currency, $invoice/total), '|') return concat($payload, ';SHA-256=', hash(value := $payload, algorithm := 'SHA-256', options := map{}))
```

[One-let](hash-keyword-let.xpath) and [concat-only](hash-keyword-concat.xpath) forms succeed. [Two-let](two-let-keyword.xpath) and [separate digest binding](hash-business-keyword-binding.xpath) reproduce refusal. This narrows the consumer scenario without claiming a diagnosed compiler cause. Use the positional business report while that boundary is unresolved.

element-to-map/map-to-element/parse-uri/compare use names differing from the pinned draft. Paired files below demonstrate current names succeeding and draft names refusing. fn:char has positional availability but no keyword metadata. fn:replace has keyword defaults but no exact #2 registration; direct/reference/lookup evidence is separate from its successful keyword call.

## Hash input and option policy

An empty input sequence returns empty; an empty string hashes zero UTF-8 bytes. Hex/base64 inputs hash their decoded octets and match text abc. Empty algorithm defaults to MD5. Empty sequence and empty-map options are accepted; the opaque partner-policy key leaves the digest unchanged. No meaning is assigned to that key. Non-map options refuse XPTY0004, also when the tested value is empty.

The pinned F&O contract leaves additional options implementation-defined; it defines no standard partner-policy option. Correctly distinguish unavailable named extensions from mandatory algorithm support: BLAKE3 and CRC-32 remain unsupported FOHA0001 despite being required in the pinned draft.

## Complete behavior fixtures

| Runnable case | Current output/error |
|---|---|
| [hash-default](hash-default.xpath) | `900150983CD24FB0D6963F7D28E17F72` |
| [hash-empty-algorithm](hash-empty-algorithm.xpath) | `900150983CD24FB0D6963F7D28E17F72` |
| [hash-empty-options](hash-empty-options.xpath) | `BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD` |
| [hash-map-options](hash-map-options.xpath) | `BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD` |
| [hash-unknown-option](hash-unknown-option.xpath) | `BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD` |
| [hash-keyword-options](hash-keyword-options.xpath) | `900150983CD24FB0D6963F7D28E17F72` |
| [hash-keyword-reordered](hash-keyword-reordered.xpath) | `BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD` |
| [hash-empty-value](hash-empty-value.xpath) | `0` |
| [hash-empty-text](hash-empty-text.xpath) | `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855` |
| [hash-invalid-options](hash-invalid-options.xpath) | `XPTY0004` |
| [hash-invalid-options-empty](hash-invalid-options-empty.xpath) | `XPTY0004` |
| [hash-invalid-algorithm](hash-invalid-algorithm.xpath) | `FOHA0001` |
| [hash-blake3](hash-blake3.xpath) | `FOHA0001` |
| [hash-crc32](hash-crc32.xpath) | `FOHA0001` |
| [hash-binary-equivalence](hash-binary-equivalence.xpath) | `true` |
| [hash-base64-equivalence](hash-base64-equivalence.xpath) | `true` |
| [parse-uri-implemented-keyword](parse-uri-implemented-keyword.xpath) | `https` |
| [parse-uri-draft-keyword](parse-uri-draft-keyword.xpath) | `XPST0017` |
| [element-map-implemented-keyword](element-map-implemented-keyword.xpath) | `1` |
| [element-map-draft-keyword](element-map-draft-keyword.xpath) | `XPST0017` |
| [map-element-implemented-keyword](map-element-implemented-keyword.xpath) | `1` |
| [map-element-draft-keyword](map-element-draft-keyword.xpath) | `XPST0017` |
| [compare-implemented-keyword](compare-implemented-keyword.xpath) | `-1` |
| [compare-draft-keyword](compare-draft-keyword.xpath) | `XPST0017` |
| [char-no-keyword-metadata](char-no-keyword-metadata.xpath) | `XPST0017` |
| [char-positional](char-positional.xpath) | `A` |
| [hash-missing-required](hash-missing-required.xpath) | `XPST0017` |
| [hash-unknown-keyword](hash-unknown-keyword.xpath) | `XPST0017` |
| [hash-duplicate-keyword](hash-duplicate-keyword.xpath) | `XPST0017` |
| [hash-business-report](hash-business-report.xpath) | `XPST0017` |
| [hash-business-positional](hash-business-positional.xpath) | `INV-42\|EUR\|125.00;SHA-256=8D8F32B72196A44EF9860012FA1B67B3BC651435F52D9AC9C9BF3EA5A11A5176` |
| [hash-keyword-let](hash-keyword-let.xpath) | `BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD` |
| [hash-keyword-concat](hash-keyword-concat.xpath) | `sha256=BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD` |
| [hash-business-keyword-binding](hash-business-keyword-binding.xpath) | `XPST0017` |
| [replace-positional-default](replace-positional-default.xpath) | `XPST0017` |
| [replace-keyword-default](replace-keyword-default.xpath) | `42` |
| [replace-reference2](replace-reference2.xpath) | `XPST0017` |
| [replace-lookup2](replace-lookup2.xpath) | `0` |
| [two-let-keyword](two-let-keyword.xpath) | `XPST0017` |

## Availability matrix and execution

The [signature audit](../../SIGNATURE-CONTRACT-AUDIT.md) and [JSON comparison](../../audit-signature-contracts.json) link 148 exact registration lookup files for 68 new-name families and 20 selected existing functions. They run at frozen 4.0 except experimental scan, with explicit 3.1 visibility and frozen-scan absence. These probes establish callable arity availability, not complete parameter coercion or option semantics.

Run `dotnet test tests/Bosak.Xslt.Tests -c Release --filter FullyQualifiedName~CatalogExampleTests`. The existing harness executes all files via public APIs with fresh contexts; outputs/errors and applicable compatibility boundaries are exact manifest values. No host resolver, external variables or engine/harness change is required.

This delivery adds 187 checks (148 availability, 39 behavior); REQ-126 remains In Progress and version remains 1.1.2. Continue the remaining D2 work listed in the audit before claiming complete signature coverage.
