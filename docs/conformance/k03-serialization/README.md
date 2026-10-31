# K03-C02: content-preserving name serialization — 27 October 2026

**K03 name-storage integration — 27 October 2026:** [Bounded core completion](../k03-name-storage/README.md) preserves Unicode map/provider names, literal values and namespace/copy metadata. All47 new regressions and22,007 solution tests pass. The original map-to-element-053 assertion now passes; qt4 improves to8,437/0/1,893 with exactly that skip removed and all1,893 retained reasons unchanged. QT3 remains31,145/0/676 with identical676 skips. No fixture, assertion, harness filter or gated-set change. Candidate1.26.11 unpublished/public1.26.3. Parser-edition/current-draft limits remain explicit; Bosak.Schema retains XSD1.1 ownership and REQ-126 remains Complete. Next: K04 dependency/routing applicability audit. Earlier open Unicode findings are historical checkpoints.



**K03-C02 is complete for serialization of existing stored node content.** All 40 focused regressions pass, including the 39 failures reproduced before correction. Full solution tests pass at **21,960**. Name-storage integration remains open, and the original qt4 supplementary-name skip stays in place.

## Change

`XDocumentNode` now restores sentinel-encoded **element and attribute name tokens** rather than decoding the entire emitted XML string. The existing XML serializer still performs escaping and namespace fixup. A scan of its emitted markup changes only encoded names; it skips quoted attribute values (including namespace URIs), text, comments, CDATA, processing instructions and complete declarations. DTD internal subsets retain quoted literals, comments, processing instructions and bracket depth.

Both document and standalone-element serialization use the same helper. It returns the original string for unannotated trees, strings without sentinels, or serialized output with no name token requiring decoding. The output builder is allocated only when a name changes. PI targets are preserved because the current loader/construction hook does not encode them.

Previously, an annotated element with XDM string value `ǀ0041ǁ` serialized that literal as `A`. The same whole-output replacement could change attribute values, namespace URIs, comments, PI content and DTD literals. These existing stored values now remain unchanged while supplementary and literal-sentinel element/attribute names decode once to their original names.

The [pre-implementation pin](evidence/IMPLEMENTATION-PIN.json) records source base `2d224d0a` and the [XML names/content contract](https://www.w3.org/TR/2008/REC-xml-20081126/). No new public API is introduced. Candidate **1.26.10 is unpublished**; public version remains **1.26.3**.

## Verification and boundaries

| Verification | Result |
| --- | --- |
| Focused baseline | 39 failed / 1 passed |
| Focused after correction | 40 passed |
| Complete solution tests | 21,960 passed |
| Repeated K03 diagnostics | 30 isolated processes / zero timeouts |
| Normal QT3 | 31,145 / 0 / 676 |
| Exact QT3 skip map | All 676 names/reasons unchanged |
| qt4 224-set gate | 8,436 / 0 / 1,894; 11 existing Experimental passes |

The 40 tests cross ASCII, supplementary and literal-sentinel name forms with element/document serialization and text, attributes, namespace declarations, comments, PIs and CDATA. Additional controls cover encoded attribute/prefix names, inherited namespace copying, DTD literals/comments/brackets and unannotated trees. They assert the exact emitted content without reparsing it through a loader that has independent decoding behavior.

The 30 original K03 probes retain all material outcomes except the two annotated-provider serialization results, which now preserve literal text. Their XDM string values and decoded local names are unchanged. The original `map-to-element-053` still skips at the `XName` storage boundary; this is not counted as a new conformance pass.

This correction preserves content **already present in XObject trees**. Loader decoding, XDM value accessors, provider name representation and XML 1.1 control-character support are unchanged. The evidence does not establish an end-to-end parse/copy/serialization guarantee. Name-storage integration must include those boundaries in its round-trip tests before admission is proposed.

Release solution build succeeds with zero errors and one existing nullable warning in the XSLT conformance runner. Full solution tests execute outside the restricted sandbox so local HTTP test stubs can bind. Fixtures, original assertions, harness routing, dependency/retry policy, skips and frozen baselines are unchanged.

[RESULTS.json](RESULTS.json) retains the deterministic review and diagnostic outcomes. [Evidence manifest](evidence/manifest.json) pins source/binary/fixture identities, all raw receipts and historical evidence.

```powershell
python docs/conformance/k03-serialization/build-review.py
python docs/conformance/k03-serialization/evidence/validate-evidence.py
```

Next: reversible map/provider name-storage integration with end-to-end tests for names, literal values, namespaces and copies, including review of loader/accessor value preservation. Keep the qt4 Unicode skip until that complete boundary is verified. Bosak.Schema retains XSD 1.1 ownership; REQ-126 remains Complete for its owner-accepted bounded catalog. No release, push, tag, upstream or Braid action occurs.
