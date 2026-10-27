# K04-C05: CLDR format-integer review

27 October 2026. Source base `01e67cec`; candidate **1.26.11** unpublished, public **1.26.3**.

**Review complete; implementation and qt4 CLDR admission remain open.** The two original QT3 cases pass. Their qt4 counterparts also pass through direct diagnosis, but the two newer qt4 assertions fail with FODF1310. The normal qt4 runner continues to skip all four under its existing CLDR feature exclusion. No source, admission, baseline or fixture changes are justified by this review alone.

| Original fixture | Direct executor | Normal runner |
| --- | --- | --- |
| QT3 format-integer-065 / -066 | Two passed | Two passed |
| qt4 format-integer-065 / -066 | Two passed | Two dependency skips |
| qt4 format-integer-076 | Failed: FODF1310 for c(%spellout-cardinal) | Dependency skip |
| qt4 format-integer-077 | Failed: FODF1310, neither original any-of alternative matches | Dependency skip |

All original queries, environments and assertions are preserved. No exploratory set is promoted and no expected result is weakened. These six original diagnostic processes and six normal single-case processes run under external 20/30-second watchdogs.

## Two distinct implementation findings

1. **K04-CLDR-01: named cardinal modifiers.** FormatIntegerEngine.ParseModifier accepts plain `c` and parenthesized `o(...)`, but rejects `c(...)`. Original cases076/077 therefore fail before their requested numbering variations can be evaluated.
2. **K04-CLDR-02: German named ordinal endings.** GermanToOrdinalWords treats every percent-prefixed scheme name as ending `e`. Derived `o(%spellout-ordinal-s/-r/-n)` controls all return `Erste`, while the requested reference forms are `Erstes`, `Erster`, `Ersten`. Existing explicit `o(-es/-er/-en)` controls produce those forms correctly. Accepting cardinal parentheses alone would not establish the remaining named-scheme behavior.

The final modifier-control round runs 11 controls through each actual QT3/qt4 executor: **14 pass and eight fail**. The failures isolate named-cardinal handling and three named German endings in both compatibility paths. Plain cardinal/ordinal output, full explicit endings and malformed-picture FODF1310 controls pass. These are mechanism diagnostics derived from the original case's forms, not new claims of standards or complete CLDR compliance.

**Calibration receipt:** the initial explicit-ending controls used `-s/-r/-n`, incorrectly expecting full endings. They produced literal `Ersts/Erstr/Erstn`. Those six expected-value mistakes are not engine defects. The initial 22 executions, original inputs and results remain under [initial-controls](evidence/initial-controls/control-receipts.json); the accepted control round uses `-es/-er/-en`. Across both rounds and original/normal checks there are **56 bounded processes, zero timeouts**. Failed diagnoses are retained in [RESULTS.json](RESULTS.json).

## Next bounded implementation

Before semantic changes, pin exact immutable F&O and CLDR revisions for cardinal variations, named ordinal schemes, unsupported variation fallback and malformed-picture errors. Implement only the reviewed named-cardinal parsing and known German endings, reusing existing formatting mechanisms. Add happy-path and edge regressions, including existing ordinary/explicit output, malformed parentheses, zero/negative values and unknown schemes according to the pinned policy. Rerun all six originals and complete required gates. Consider qt4 feature admission only when the four original assertions pass; passing two historical cases never establishes complete CLDR support.

The review makes **no tracked source, harness, version, fixture, assertion or baseline change**. It inherits the source-identical accepted **22,007 solution passes**, normal QT3 **31,161/0/660**, qt4 gated **8,438/0/1,892** and 11 Experimental passes from source base01e67cec. These are explicitly inherited, not fresh full-suite/gate runs. The [manifest](evidence/manifest.json) protects this review and **1,539 previous artifacts**. Bosak.Schema retains staged XSD1.1 ownership; REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs.

```powershell
python docs/conformance/k04-cldr-review/build-review.py
python docs/conformance/k04-cldr-review/evidence/validate-evidence.py
```
