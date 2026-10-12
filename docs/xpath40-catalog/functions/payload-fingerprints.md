<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Payload Fingerprints">
  <br><br><h1>Payload Fingerprints</h1>
  <p>Frozen 4.0 — verified integration contracts and limitations</p>
</div>

## 1. Name and Classification

Function: [fn:hash](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-hash). Frozen 4.0.
Original delivery [PR #83](https://github.com/Fytala-Charles/Bosak/pull/83), [532d04f4](https://github.com/Fytala-Charles/Bosak/commit/532d04f4672df430d93752e3158fc3cdfd4c7c39).

Reviewed 2026-10-12 against immutable [QT4CG function source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). This is a current review pin, not recovery of the unavailable historical adopted snapshot.

## 2. Intent

Compute an explicit SHA-256 fingerprint of a supplied invoice payload string so repeated or changed payload content can be compared.

## 3. Also Known As

Digest and content fingerprint describe the operation. No aliases are registered. A fingerprint is not an authenticated message signature.

## 4. History / Supersedes

XPath 3.1 has no standard hash function. Supply a verified digest from the host or an extension instead of claiming a pure standard-3.1 equivalent. The SHA-256 report below was independently computed with Python hashlib over the same UTF-8 payload.

## 5. Motivation

An import log can compare payload content across repeated receipts. Keeping the algorithm and exact input text explicit prevents a digest over normalized text from being mistaken for one over original transport bytes.

## 6. Language Alignment

hash accepts an optional string, hexBinary or base64Binary and returns optional hexBinary. Strings are hashed as UTF-8; binary values supply their octets. Empty sequence returns empty, whereas an empty string has a digest. Algorithm defaults to MD5; this case explicitly chooses SHA-256. Unsupported algorithms raise FOHA0001. The examples verify selected algorithm forms and default behavior, not every algorithm/options combination.

## 7. Applicability

The payload is a deliberately defined text representation INV-2048|EUR|125.00, not a serialization of arbitrary XML. The sample hashes the payload element string value, not XML markup. To fingerprint exact acquired bytes, pass those bytes as binary. Canonicalizing XML/JSON or authenticating a sender requires a separate protocol; no canonicalization or signature is performed here.

## 8. Structure

Read the text payload, call hash with SHA-256, then report the hex representation, its character length and result type.

## 9. Participants

The payload defines content. Algorithm selection defines the digest method. The binary result and its textual report provide comparison values.

## 10. Collaborations

Use the same representation and algorithm for comparisons. Trailing whitespace and a changed amount produce different digests in the fixtures. Binary abc and UTF-8 string abc yield the same bytes/digest.

## 11. Consequences

Content changes are observable without exposing a claim about sender identity. Empty versus absent payload remains distinct. The default-MD5 fixture records the API default; the business example chooses its algorithm explicitly.

## 12. Implementation

Compile new functions with CompileOptions.Compatibility40; default 3.1 rejects them. Earlier-version comparisons use Compatibility31. The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) verifies exact text/error markers and default-profile refusal for successful 4.0 cases. All values are inline and deterministic.

## 13. Sample Code

Complete synthetic input:

```xml
<payload>INV-2048|EUR|125.00</payload>
```

### XPath 4.0 expression

[Runnable file](../examples/payload-fingerprints/main.xpath):

```xpath
let $payload := string(parse-xml('<payload>INV-2048|EUR|125.00</payload>')/payload)
let $digest := hash($payload, 'SHA-256')
return concat('sha256=', $digest, '; hexCharacters=', string-length(string($digest)), '; hexBinary=', $digest instance of xs:hexBinary)
```

Exact output:

```text
sha256=612D86BAC9C1A9654CA5BFF669CB9B2C4CF8F5C077FB024FF443589AD02D5BF4; hexCharacters=64; hexBinary=true
```

### Complete fixture inventory

Each file embeds its full input and exact result or diagnostic marker.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/payload-fingerprints/main.xpath) | `sha256=612D86BAC9C1A9654CA5BFF669CB9B2C4CF8F5C077FB024FF443589AD02D5BF4; hexCharacters=64; hexBinary=true` |
| [binary-equivalent](../examples/payload-fingerprints/binary-equivalent.xpath) | `true` |
| [changed-payload](../examples/payload-fingerprints/changed-payload.xpath) | `true` |
| [whitespace-matters](../examples/payload-fingerprints/whitespace-matters.xpath) | `true` |
| [normalized-algorithm](../examples/payload-fingerprints/normalized-algorithm.xpath) | `true` |
| [empty-string](../examples/payload-fingerprints/empty-string.xpath) | `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855` |
| [empty-sequence](../examples/payload-fingerprints/empty-sequence.xpath) | `0` |
| [default-md5](../examples/payload-fingerprints/default-md5.xpath) | `900150983CD24FB0D6963F7D28E17F72` |
| [unsupported](../examples/payload-fingerprints/unsupported.xpath) | `FOHA0001` |
| [nonstring](../examples/payload-fingerprints/nonstring.xpath) | `XPTY0004` |

### Host-side comparison for a XPath 3.1 consumer

```python
import hashlib
print(hashlib.sha256(b"INV-2048|EUR|125.00").hexdigest().upper())
```

Exact output:

```text
612D86BAC9C1A9654CA5BFF669CB9B2C4CF8F5C077FB024FF443589AD02D5BF4
```

This independent comparison verifies the fingerprint bytes. It does not add a standard hashing function to XPath 3.1 or reproduce the Bosak result-type check.

## 14. Performance

Digest calculation processes supplied text/octets; it does not acquire a resource. Strings require UTF-8 encoding and digest state. No benchmark or streaming acquisition claim is implied.

## 15. Known Uses

Synthetic payload workflow. Named evidence: [`Hash_SpecVectors`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Hash_AlgorithmIsNormalized`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Hash_BinaryInputs_HashOctets`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Hash_EmptySequence_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs), [`Hash_UnsupportedAlgorithm_RaisesFoha0001`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs) has no exact fn-hash GatedSets assignment. These ten fixtures and the independent hashlib comparison provide separate evidence. No promotion is included.

## 16. Related Features

[Invoice API URIs](invoice-api-uris.md) distinguishes semantic values from original wire spelling. [Numeric import diagnostics](numeric-import-diagnostics.md) reports rejected input data. [The audit](../COMPLETENESS-AUDIT.md) retains algorithm/options coverage limits.
