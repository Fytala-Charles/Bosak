# K04-C04: bounded qt4 collation admission
**K04-C05 CLDR review - 27 October 2026:** [CLDR diagnosis](../k04-cldr-review/README.md) preserves the original two QT3 passes, two qt4 direct passes and two qt4 FODF1310 failures. Normal qt4 still dependency-skips all four cases. Focused controls isolate rejected named-cardinal modifiers and lost German named-ordinal endings; initial control-expectation mistakes are preserved separately. No source/admission/baseline change. Inherited source-identical results:22,007 solution passes, QT3 **31,161/0/660**, qt4 gated **8,438/0/1,892**; no fresh full-suite claim. Next: pin immutable specification/CLDR semantics and implement a bounded formatter correction before reviewing admission. Candidate1.26.11 unpublished/public1.26.3; Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Earlier K04 notices are historical checkpoints.


27 October 2026. Source base `f797cb13`; candidate **1.26.11** unpublished, public **1.26.3**.

The qt4 filter now admits `non_unicode_codepoint_collation` using the existing HTML ASCII case-insensitive and UCA routes, matching the bounded mechanism already available to QT3. The name moves from `UnsupportedFeatures` to `FrozenAdmittedFeatures`; other feature/language predicates, unknown-name refusal and URI validation remain unchanged. This does not promise support for arbitrary collation URIs or every UCA option. No engine behavior changes.

The original gated **compare-010** assertion evaluates the HTML ASCII case-insensitive comparison of `"a"` and `"A"` and expects zero. It passes through the existing TestExecutor40 and through the corrected normal runner. The fresh qt4 gate improves to **8,438 passed / 0 failed / 1,892 skipped**, with exactly this skip removed and all 1,892 retained names/reasons unchanged. All 224 existing GatedSets and 11 Experimental passes stay unchanged. The frozen normal QT3 baseline remains **31,161/0/660**, with every skip name/reason unchanged; AGENTS.md is unchanged.

## All affected original declarations

The fixture inventory contains 13 positive declarations and no negative declaration of this feature. The isolated direct-executor diagnosis retains every original assertion/environment: five pass, while eight fail because they contain XQuery grammar unsupported by the XPath-only qt4 executor. Those failures remain recorded. Actual full dependency checks and separate normal runner invocations prove that the original language guards continue to exclude all eight; no XQuery admission or routing is added.

| Original cases | Direct diagnosis | Corrected normal runner |
| --- | --- | --- |
| compare-010 | Passed | Passed, gated |
| K-SeqDeepEqualFunc-56 / -57 | Passed | Passed, exploratory |
| fn-substring-after-24 / fn-substring-before-24 | Passed | Passed, exploratory |
| K-SeqDeepEqualFunc-58 through -63, plus -70 | Seven failed assertions due to unsupported XQuery constructors | Seven dependency skips retained |
| group-017 | Failed assertion due to unsupported XQuery group-by grammar | Dependency skip retained |

Seven expression controls pass through actual TestExecutor40/ResultComparer: ASCII folding, non-ASCII distinction, codepoint distinction, UCA primary folding, UCA ordering, unknown URI error FOCH0002 and ASCII case-insensitive substring matching. The non-ASCII control uses codepoints so the synthetic fixture has no encoding ambiguity.

The full prior 222 controls plus 26 original/full-without-feature assessments give **248 filter controls**, run before/after in four externally bounded 30-second processes. Only seven qt4 outcomes change: the feature's positive/negative assessment and five admitted original cases. QT3 filter outcomes, unknown-feature refusal, other capability/profile checks and all eight blocked language cases remain unchanged. There are **37 bounded small processes**: 13 direct diagnostics, seven expression controls, 13 normal case runs and four filter runs. Direct/expression watchdogs are 20 seconds; normal/filter watchdogs are 30 seconds. No timeout occurs.

Fresh solution build succeeds and all **22,007 solution tests pass**. [RESULTS.json](RESULTS.json) records the exact gate delta and retained failure outcomes. The [manifest](evidence/manifest.json) protects all current receipts and **1,445 previous pinned artifacts**. Fixtures, assertions, engine sources, public APIs, schema admission and version remain unchanged. Bosak.Schema retains staged XSD 1.1 ownership; REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs.

Next recommended slice: review the four original qt4 CLDR format-integer feature declarations against their original assertions and the two existing QT3 observations. Two passing assertions do not certify complete CLDR numbering support.

```powershell
python docs/conformance/k04-collation-admission/build-review.py
python docs/conformance/k04-collation-admission/evidence/validate-evidence.py
```
