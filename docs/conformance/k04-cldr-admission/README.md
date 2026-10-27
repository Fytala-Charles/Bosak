# K04-C07: bounded qt4 CLDR admission

27 October 2026. Source base `7387170f`; candidate **1.26.12** unpublished, public **1.26.3**.

The qt4 dependency filter now admits `fn-format-integer-CLDR` for the reviewed bounded word-numbering implementation from K04-C06. The feature moves from the unsupported set to the explicit admitted set; other feature/profile predicates and unknown-name refusal stay unchanged. All four original qt4 assertions pass through normal admission, as do their two existing QT3 counterparts. This does not certify complete CLDR/RBNF implementation, every numbering scheme or full language support.

## Original cases and full exploratory set

The newly admitted cases are **format-integer-065, -066, -076 and -077**. Their original fixtures, environments and assertions are unchanged. Six normal single-case checks produce six passes under external30-second watchdogs.

The complete **102-case fn-format-integer exploratory set** runs before/after through the normal runner under external60-second watchdogs:

| Normal exploratory set | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Before CLDR admission | 82 | 15 | 5 |
| After CLDR admission | 86 | 15 | 1 |

Only the four named CLDR skips are removed. The remaining skip is format-integer-042, with its original dependency reason. All **15 failure names and messages are identical**. The set stays outside GatedSets; successful process exit does not mean those exploratory failures passed. [Remaining radix findings](evidence/remaining-radix-findings.json) preserves their original expressions/assertions for the next review. No set is promoted to hide or overlook them.

## Profile and validation evidence

**242 filter controls** run before/after through the production filters in four externally bounded30-second processes. They include the prior222 controls, all six original dependency lists with/without the feature, and combined CLDR requirements with XQuery4, default-language fr-CA, typedData or an unknown feature. Only six qt4 outcomes change: the feature's positive/negative assessment and four complete original lists. QT3 outcomes, the other rejected combined requirements, unknown-name refusal, schema/PSVI predicates and language/version policy remain unchanged.

The `language` dependency type retains its existing admission behavior; this change does not add a language whitelist. The existing `default-language` guard remains a separate predicate. Known German aliases, legacy feminine alias, unknown variation fallback and implementation limits remain those pinned and documented in [K04-C06](../k04-cldr-fix/README.md); no engine semantics are changed here.

Fresh complete validation: solution build succeeds; **22,043 solution tests pass**. Normal QT3 stays **31,161/0/660** and qt4 gated stays **8,438/0/1,892**, with every skip name/reason identical, 224 GatedSets and11 Experimental passes unchanged. The four new passes are exploratory; the frozen gate totals do not change. There are **12 bounded small processes**, zero timeouts: four filter runs, six normal original-case checks and two exploratory-set runs.

[RESULTS.json](RESULTS.json) and the [manifest](evidence/manifest.json) protect all current results and **1,772 previous artifacts**. No engine source, version, fixture, assertion, GatedSets, public API, schema profile or QT3 baseline change. Bosak.Schema retains staged XSD1.1 ownership; REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs.

Next recommended slice: review the 15 retained XPath4 radix-picture errors against immutable specification evidence, then choose a bounded formatter implementation. The current CLDR admission is complete; radix formatting is separate engine work.

```powershell
python docs/conformance/k04-cldr-admission/build-review.py
python docs/conformance/k04-cldr-admission/evidence/validate-evidence.py
```
