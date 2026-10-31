# K03: reversible name-storage integration — 27 October 2026

**K04 applicability audit — 27 October 2026:** [Current dependency/routing review](../k04-dependency-audit/README.md) reproduces all619 QT3 and1,893 gatedqt4 dependency skips through actual filters. Of619 QT3 cases,246 satisfy XQuery dependencies but fail syntax routing;16 are triple-quote candidates. Thirty-two bounded execution probes yield six public-XQuery assertion passes and ten retained failures;62 synthetic predicate assessments expose profile/polarity questions. No engine, filter or baseline change. Next K04-C01: quote-aware routing with complete candidate and rehearsal evidence before any QT3 admission proposal. Candidate1.26.11 unpublished/public1.26.3; inherited22,007 solution passes and gates31,145/0/676 and8,437/0/1,893. Bosak.Schema owns XSD1.1; REQ-126 remains Complete.



**The bounded K03 core correction is complete.** The original `map-to-element-053` Unicode-name assertion now passes normally. The full qt4 gate improves to **8,437 passed / 0 failed / 1,893 skipped**. Only that case leaves the skip set; all 1,893 retained names and reasons match. Exact QT3 remains **31,145/0/676**, with all 676 names/reasons unchanged.

## What changed

Map construction uses the existing reversible name codec for element/attribute local names and namespace prefixes, while preserving namespace URIs and values. The existing internal annotation marks encoded storage independently of XML-version feature validation. Standalone attributes, copied trees, XML-layout copies and constructed documents retain coherent name metadata. Raw parsed names are normalized exactly once when copied into encoded trees; already encoded names are not encoded again. Namespace-axis nodes obtain encoding metadata from their owner, and physical prefixes are decoded before re-encoding for cached namespace storage.

The XML reader's **existing** `CheckCharacters=false` setting already materializes numeric character references. The loader's extra sentinel encoding/decoding was redundant and could turn literal `ǀ0041ǁ` into `A`, including after entity expansion. Removing that pass preserves raw parsed values. Attribute string access likewise reads the already materialized value rather than decoding it again. Reader/resolver/network settings and XML-version feature validation remain unchanged.

No public API or assembly friend grant is added. Internal copy helpers reuse the existing provider/standard-layer seam. XML 1.1 storage annotation terminology now states that it also marks constructed encoded-name trees; it does not enable XSD 1.1 or relax explicit namespace/version checks.

## Validation

| Check | Result |
| --- | --- |
| Initial map baseline | 22 failed / 13 passed |
| Initial provider baseline | 7 failed / 0 passed |
| Final new round-trip regressions | 39 map + 8 provider = **47 passed** |
| Complete solution | **22,007 passed** |
| Original K03 diagnostic probes | 30 isolated processes / zero timeouts |
| Original qt4 Unicode assertion | Passed with its unchanged `assert-eq 119070` |
| Normal QT3 gate | **31,145 / 0 / 676**, exact skip map unchanged |
| qt4 224-set gate | **8,437 / 0 / 1,893**, 11 existing Experimental passes |

New tests cover ASCII, BMP range boundaries, supplementary characters and literal codec-like names; roots, children, attributes, Unicode/sentinel lexical prefixes, namespace axes, map round trips, XML-layout copies, parsed references/entity/CDATA/comment/PI values, free attributes and document/descendant copies. The native .NET reader controls separately compare eight configurations; they demonstrate the existing mechanism rather than certify every XML 1.1 validity rule.

The integration review added free-attribute and namespace regressions. The first namespace correction exposed a second encoding of physical prefixes and an extra namespace declaration; both were fixed without weakening assertions. Initial compile errors, intermediate test failures, an interrupted run and its output-lock build attempt are retained. Only the final complete solution/gate receipts count as accepted checks. The final header cleanup changes comments only and is followed by a successful build.

The 30 original K03 probes show the expected ten material changes: the original harness case passes; valid BMP/supplementary map names reach usable nodes; child/attribute/URI-qualified paths work; and default constructed supplementary names no longer expose encoded storage. Existing invalid-name errors, literal content and .NET platform controls retain their material outcomes. The underlying platform still rejects these names directly; the provider codec bridges that representation boundary.

The [qt4 skip delta](evidence/qt4-skip-delta.json) retains all names/reasons. Gated-set membership, dependency admission, assertions and fixtures are unchanged. AGENTS.md's exact QT3 requirement stays unchanged, and its qt4 rule remains zero gated failures. The qt4 score gain follows the engine correction; no harness admission policy is changed.

[RESULTS.json](RESULTS.json) consolidates receipts, probes, scope and limitations. The [pre-implementation pin](evidence/IMPLEMENTATION-PIN.json) records source base `1d6d421e`; the [manifest](evidence/manifest.json) protects source/binary identities and prior evidence.

```powershell
python docs/conformance/k03-name-storage/build-review.py
python docs/conformance/k03-name-storage/evidence/validate-evidence.py
```

## Limits and next work

This verifies bounded map/provider name and value round trips, not complete current-draft F&O behavior, every XML edition/parser input, or full XML 1.1 validity. XML-layout parsing retains its existing parser restrictions; current-draft `escape-names` remains unsupported. Existing reader character-checking policy is unchanged. These limits do not justify replacing the original Unicode name or weakening its assertion.

Candidate **1.26.11 remains unpublished**; public version **1.26.3**. Bosak.Schema retains XSD 1.1 ownership, and REQ-126 remains Complete for its accepted bounded catalog. No release, tag, push, upstream or Braid action occurs.

Next: **K04 dependency/routing applicability audit**, separating genuine core capability gaps from version/profile exclusions and Bosak.Schema work.
