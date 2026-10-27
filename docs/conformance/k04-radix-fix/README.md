# K04-C09: bounded XPath4 radix formatter

27 October 2026. Source base **a8b520fd**; new functionality advances the unpublished candidate to **1.27.0**. Public version remains **1.26.3**.

The formatter now handles radix2..36 pictures only in XPath4 contexts, including the experimental compatibility level. The **14 formatter-only failures pass**, and the two existing caret-grouping cases remain green. The complete102-case exploratory set improves from86/15/1 to **100passed/1failed/1skipped**. Its remaining failure, format-integer-40-012, now exposes **XPST0017 for missing parse-integer#2**, rather than the earlier FODF1310. The set remains ungated and the original assertion is unchanged.

## Semantics and implementation

[SPECIFICATION-ADOPTION.json](evidence/SPECIFICATION-ADOPTION.json) records explicit adoption before implementation, distinct from K04-C08's review pin and historical REQ-126 acceptance. It uses immutable qt4cg revision[e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml), SHA256409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5, for fn:format-integer radix recognition, digit-pattern conversion, grouping and ordinals. It does not adopt a newer draft or repin legacy provenance.

Recognition requires an ASCII radix2..36 without leading zero, followed by caret and a later x/X in the primary picture. The modifier is split first. Without a recognized radix, existing decimal caret grouping remains intact: original014/015 and cross-profile controls still pass. In3.1 the entire old formatting path remains active, including its refusal of new radix pictures.

The new private radix path validates consistent x/X mandatory signs, optional-sign order, grouping placement and Unicode separators. It performs safe magnitude conversion (including Int64.MinValue), zero padding and case selection, reusing the existing grouping-template/insertion mechanisms. The current public long input boundary is retained; internal BigInteger magnitude handling is not a claim of public arbitrary-precision formatting.

Numeric ordinal suffixes use the original value modulo100, so21 rendered as hexadecimal15 becomes **15st**; alphabetic radix digits also remain numeric rather than selecting word-ordinal processing. Existing literal/English numeric suffix conventions remain, and the traditional flag does not title-case radix digits. Other modifier grammar and unrelated language/word-numbering behavior are outside this slice.

## Verification

The new **68 regression rows** cover exact formatting in frozen/experimental4.0 contexts, every base2..36, zero/negative/Int64 endpoints, case, optional and mandatory padding, regular/irregular/caret/non-BMP grouping, ordinals, malformed pictures,3.1 refusal, empty values and unchanged decimal caret grouping. Before implementation: **53failed/15passed**. Final source: **68passed**. An initial green run before a parser-allocation cleanup is retained separately; final regression and complete solution runs use the final implementation.

All17 original radix assertions run unchanged through actual TestExecutor40/ResultComparer: **16pass, one retained dependency failure**. The34 review controls now have17 QT3 passes and14 qt4 passes, with only the three parse-integer call/lookup controls still failing. A fresh normal102-case exploratory run confirms100/1/1, preserving the single dependency skip format-integer-042. These are52 externally bounded small processes, zero timeouts; exploratory exit0 does not make the remaining failure pass.

Fresh solution build succeeds and **22,111 solution tests pass**, including all68 new rows. Normal QT3 stays **31,161/0/660**, qt4 gated stays **8,438/0/1,892**, with all skip names/reasons identical and11 Experimental passes retained. No fixture/assertion, harness admission, GatedSets, public API or QT3 baseline change. [RESULTS.json](RESULTS.json) and the [manifest](evidence/manifest.json) protect all current evidence and **1,942 prior artifacts**.

Bosak.Schema retains staged XSD1.1 ownership; REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs. Next: a separate parse-integer contract review/implementation, covering profile gating, arities, valid bases, lexical/range errors and the original round trip before considering set promotion.

```powershell
python docs/conformance/k04-radix-fix/build-review.py
python docs/conformance/k04-radix-fix/evidence/validate-evidence.py
```
