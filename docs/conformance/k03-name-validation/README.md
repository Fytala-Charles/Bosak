# K03-C01: scalar XML-name validation — 27 October 2026

**K03-C02 serialization — 27 October 2026:** [Content-preserving name decoding](../k03-serialization/README.md) restores encoded element/attribute name tokens while preserving existing literal node content, namespace URIs and declarations. All40 focused regressions and21,960 solution tests pass; QT3 remains31,145/0/676 with identical676 skips, and qt4 remains8,436/0/1,894. The original supplementary-name skip remains. Candidate1.26.10 unpublished/public1.26.3. Next: reversible name-storage integration with loader/accessor/copy round-trip verification; no public API, baseline or XSD1.1 ownership change. REQ-126 remains Complete.



**K03-C01 is complete.** All 78 focused regressions pass, including the 18 failures reproduced before correction. The complete solution has 21,920 passing tests. The original supplementary-name construction skip remains: provider name storage and content-preserving serialization are still separate work.

## Change

`ElementMap.IsNcName` now decodes UTF-16 scalars with `Rune.DecodeFromUtf16`, rejects malformed input, and checks explicit XML NameStartChar/NameChar ranges. NCNames exclude colon; continuation characters such as digits, hyphen, middle dot and combining marks retain their position rules. Name-format parsing and FOJS0009 classification remain in their existing paths. No public API is added.

The [pre-implementation pin](evidence/IMPLEMENTATION-PIN.json) records source base `0d89801c` and [XML 1.0 Fifth Edition](https://www.w3.org/TR/2008/REC-xml-20081126/#NT-NameStartChar). Its scalar name ranges also apply in XML 1.1. This fixes language-level name validation; it does not add XSD 1.1 support or change Bosak.Schema ownership.

Unicode categories do not fully describe XML's allowed names. The old validator rejected valid BMP range points such as U+200C/U+3001 and accepted every supplementary surrogate pair, including U+F0000/U+10FFFF outside the name range. Invalid supplementary names now produce **FOJS0009** before provider construction. Valid characters that the current .NET `XName` representation cannot store still encounter that later boundary.

## Scope of the tests

The 78 public API regressions cover XML name-range endpoints, continuation-only scalars, excluded gaps, malformed UTF-16, ordinary invalid names, lexical namespace prefixes, root/child/attribute error paths and ordinary construction. Valid parsing beyond the current provider's character repertoire is exercised through the existing empty-valued-attribute omission path: the name is parsed and validated, then no attribute is physically constructed. This separates validation correctness from the remaining storage limitation.

Malformed UTF-16 values are constructed inside uniquely indexed theory cases. The first baseline attempt supplied those strings via MemberData and xUnit deduplicated two case IDs; its receipt is retained. The corrected baseline and final run execute all **78** cases without duplicate IDs. No production expectation was relaxed.

| Verification | Result |
| --- | --- |
| Corrected failing baseline | 18 failed / 60 passed |
| Focused regressions after correction | 78 passed |
| Complete solution tests | 21,920 passed |
| Original K03 diagnostics repeated | 30 isolated processes / zero timeouts |
| QT3 gate | 31,145 / 0 / 676 |
| Exact QT3 skips | All 676 names and reasons unchanged |
| qt4 224-set gate | 8,436 / 0 / 1,894; 11 existing Experimental passes |

The repeated diagnostics show precisely four changed query paths: U+200C/U+3001 now pass map validation and reach the still-limited `XName` construction; U+F0000/U+10FFFF now report FOJS0009. All other material outcomes are unchanged, including original `map-to-element-053`, provider encoded-name leakage and literal-sentinel serialization corruption. Those observations remain open in K03; they are not counted as conformance passes.

The normal Release solution build passes with zero errors. It reports one existing nullable warning in the XSLT conformance runner and two NuGet vulnerability-feed availability warnings; no successful vulnerability-feed audit is claimed. The full solution tests run outside the restricted sandbox so local HTTP test stubs can bind.

[RESULTS.json](RESULTS.json) retains the outcome review. [Evidence manifest](evidence/manifest.json) pins source/fixture/binary identities, all raw receipts and prior evidence. Rendered documentation uses the unchanged Fytala CSS.

```powershell
python docs/conformance/k03-name-validation/build-review.py
python docs/conformance/k03-name-validation/evidence/validate-evidence.py
```

Candidate **1.26.9 is unpublished**; public version remains **1.26.3**. No fixture, assertion, harness policy, dependency filter, retry, skip, baseline or REQ-126 acceptance changes. REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, upstream or Braid action occurs.

Next: **K03-C02**, serialization that decodes name tokens while preserving literal text, attribute values, comments, PIs, escaping and namespace identity. Then integrate reversible map/provider name storage with round-trip tests before proposing any qt4 skip adoption.
