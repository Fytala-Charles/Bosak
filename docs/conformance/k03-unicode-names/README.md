# K03: Unicode XML-name boundary trace — 27 October 2026

**K03-C01 XML-name validation — 27 October 2026:** [Scalar validation correction](../k03-name-validation/README.md) replaces category/surrogate-pair approximation with explicit XML name ranges and rejects malformed UTF-16. All78 focused tests pass (baseline18 failures), all21,920 solution tests pass, and gates stay QT3 31,145/0/676 and qt4 8,436/0/1,894. All676 QT3 skip names/reasons are unchanged. Original supplementary construction and literal-content serialization findings remain open; next is K03-C02 serialization preservation before provider storage integration. Candidate1.26.9 unpublished/public1.26.3; no baseline or XSD1.1 capability change. REQ-126 remains Complete.



**The bounded trace is complete; K03 implementation remains open.** The original qt4 `map-to-element-053` still skips on U+1D11E construction. Thirty independent diagnostics identify three separate corrections. No engine source, original assertion, skip or baseline changes in this slice.

## What fails

| Finding | Observed boundary | Correction sequence |
| --- | --- | --- |
| K03-F01 | Map NCName validation rejects valid U+200C/U+3001, while admitting supplementary scalars above the XML name ceiling | **K03-C01:** scalar-aware NCName validation and error classification |
| K03-F02 | Accepted supplementary root/child/attribute/URI-qualified names fail at `XName`; default-mode provider construction can expose encoded local names | Integrate reversible name storage after validation and serialization preservation |
| K03-F03 | Existing annotated provider serialization changes literal text `ǀ0041ǁ` into `A`, despite preserving it in the XDM string value | **K03-C02:** decode name tokens without decoding node content |

The original test's expression and `<assert-eq>119070</assert-eq>` are unchanged. The actual `TestExecutor40` returns its existing `Skipped` outcome with an XmlException at U+1D11E. The same platform boundary appears in the direct public API for root, child, attribute and URI-qualified element construction.

`ElementMap.IsNcName` uses Unicode categories for BMP characters and accepts every surrogate pair. The XML name grammar instead uses explicit scalar ranges. U+200C and U+3001 are valid name starts; the supplementary name range ends at U+EFFFF. Scalars U+F0000 and U+10FFFF should therefore fail map name validation before provider construction. These ranges apply in both [XML 1.0 Fifth Edition](https://www.w3.org/TR/2008/REC-xml-20081126/#NT-NameStartChar) and [XML 1.1 Second Edition](https://www.w3.org/TR/2006/REC-xml11-20060816/#NT-NameStartChar). NCNames additionally exclude colon.

The platform controls reproduce rejection in both `XmlConvert.VerifyNCName` and `XName.Get`. Bosak already has a reversible `Xml11NameCodec`, but applying that codec and an XML 1.1 annotation to the map builder alone is insufficient. Public `XDocumentProvider.ConstructElement` controls show why: default mode leaks the encoded supplementary local name, while annotated mode restores it but `ToXmlString()` also decodes literal sentinel text. Both ASCII and supplementary-name annotated trees serialize the literal `ǀ0041ǁ` as `A`. That is a content-preservation defect, independently of the original skipped assertion.

## Diagnostic scope

| Probe family | Processes |
| --- | ---: |
| Original actual-harness case | 1 |
| Public XPath query controls | 19 |
| .NET name-validation/storage controls | 5 |
| Existing public provider-construction controls | 4 |
| Existing codec round trip | 1 |
| **Total** | **30** |

Every process completes under a 20-second external watchdog, with zero timeouts or process failures. The 19 public query controls return six values, eight XmlExceptions and five FOJS0009 errors. These are diagnostic observations, not 30 conformance passes. Three FOJS0009 controls deliberately use invalid ASCII names; two demonstrate rejection of valid BMP names. Sequence-valued diagnostic `ToString()` output is recorded as “(sequence)” and is not used as a codepoint assertion; the original harness assertion is retained separately.

Fresh controls preserve ASCII/BMP ordinary names, invalid digit/empty/space cases, literal sentinel names/content, supplementary boundaries and current serialization behavior. The initial driver setup/build authoring failures are retained; they occurred before these 30 executions and did not affect engine source.

[RESULTS.json](RESULTS.json) contains all requests, outcomes, exception stacks and the three open findings. The [evidence manifest](evidence/manifest.json) pins source/fixture/binary identities, raw receipts and prior evidence. The fixture remains at its recorded original revision. Reflection is limited to invoking the original conformance harness; platform/provider probes use their public APIs.

```powershell
python docs/conformance/k03-unicode-names/build-review.py
python docs/conformance/k03-unicode-names/evidence/validate-evidence.py
```

## Next implementation

Start **K03-C01** with scalar-aware name validation and tests for the valid name ranges, excluded scalars, malformed surrogate sequences, NCName colon rules and FOJS0009 classification. Valid-name acceptance alone will not remove the original skip while provider storage still rejects the name. Next preserve serialized text/attribute/comment/PI values, namespace identity and escaping, then integrate reversible name storage across map roots, children, attributes, prefixes, copies and round trips. Do not use a case-name bypass, ASCII replacement or blanket whole-output decoding to make the single assertion pass.

The exact gates remain **QT3 31,145/0/676** and **qt4 8,436/0/1,894**. The latter still includes this one XML-name skip. Source is unchanged from `9950e680`; its 21,842 solution passes and prior complete gate receipts are inherited, with no new full-suite/gate claim. Candidate **1.26.8 remains unpublished**, public **1.26.3**. REQ-126 remains Complete for its accepted bounded catalog. This is core XML-name work; **Bosak.Schema retains XSD 1.1 ownership**. No release, upstream submission or Braid action occurs.
