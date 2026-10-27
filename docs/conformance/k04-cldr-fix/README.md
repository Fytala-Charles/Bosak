# K04-C06: bounded integer variation correction

27 October 2026. Source base `68e18be9`; candidate **1.26.12** unpublished, public **1.26.3**.

The existing format-integer implementation now accepts nonempty parenthesized cardinal variations and uses the known German ordinal ending aliases. The two original qt4 cases that previously raised FODF1310 now pass through the direct executor. All six original QT3/qt4 assertions and 22 derived controls pass. The normal qt4 runner still dependency-skips its four CLDR cases: admission is a separate follow-up.

## Pinned semantics and bounded behavior

[SPECIFICATION-ADOPTION.json](evidence/SPECIFICATION-ADOPTION.json) was recorded before implementation. It pins [F&O 3.1 Recommendation, 21 March 2017, section4.6.1](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-format-integer) by dated revision and full-content SHA256. CLDR release46 is resolved through tag object5cb9bb78b8beba93aaffbd8b84073ea3b21faf46 to exact commit [e1d37acce5dae468c414172be53b666d58d45be8](https://github.com/unicode-org/cldr/blob/e1d37acce5dae468c414172be53b666d58d45be8/common/rbnf/de.xml), with German rule content hashed separately. The record names affected sections and implementation limits; it does not repin historical REQ-126 adoption evidence or adopt a newer 4.0 draft.

FormatIntegerEngine.ParseModifier handles cardinal and ordinal variation syntax consistently, rejects empty or malformed parenthesized pictures and retains the existing balanced-parenthesis convention. Unknown cardinal variations use existing cardinal formatting. Existing unknown percent-prefixed ordinal fallback and title-case behavior remain. This is a correction within the existing function, not a complete rewrite of its modifier grammar.

Known German word-picture variations `%spellout-ordinal`, `%spellout-ordinal-feminine`, `%spellout-ordinal-s`, `%spellout-ordinal-r`, `%spellout-ordinal-n` select endings `e`, `e`, `es`, `er`, `en`. A known scheme may select ordinal word output even beneath `c(...)`, as required by original case077. The same existing ordinal conversion preserves ordinary and explicit `o(-es/-er/-en)` output. Other languages, numeric pictures and existing range behavior retain their previous formatting/fallback paths.

The pinned CLDR rules append s/r/n to the ordinary e-ending ordinal. The feminine name is an explicit existing Bosak alias, not a German rule name in that CLDR revision; generic `%spellout-cardinal` is handled through documented cardinal fallback. [Rule selection](evidence/CLDR-RULE-SELECTION.json) makes those distinctions explicit. This small mapping is not full CLDR/RBNF rule evaluation or certification of every locale/range/numbering system.

## Validation and admission boundary

The new 36 unit regression rows run through both XPath3.1 and XPath4.0 APIs. Before the fix: **21 failed / 15 passed**. After: **36 passed**. Cases cover named/ordinary/explicit forms, casing, zero/negative values, unknown schemes, numeric pictures, other languages, empty input and malformed pictures. Header history0.12 records the patch correction; the source candidate version advances to1.26.12.

Fresh complete validation: build succeeds; **22,043 solution tests pass**, including the 36 new rows. Normal QT3 remains **31,161/0/660**; qt4 gated remains **8,438/0/1,892**, with 11 Experimental passes. Every retained skip name/reason is identical. Six bounded original diagnoses and 22 modifier controls pass; six separate normal original-case runs confirm QT3two passes and qt4four dependency skips. Those 34 small processes have zero timeouts.

[RESULTS.json](RESULTS.json) records all outcomes; [evidence manifest](evidence/manifest.json) protects current receipts and **1,679 previous artifacts**, including the prior failed diagnoses/calibration receipts. No harness admission, fixture/assertion, GatedSets, public API or QT3 baseline change. Bosak.Schema retains staged XSD1.1 ownership; REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs.

Next recommended slice: evaluate qt4 CLDR feature admission with the four original cases and profile/language controls, preserving the documented bounded implementation and unsupported variations.

```powershell
python docs/conformance/k04-cldr-fix/build-review.py
python docs/conformance/k04-cldr-fix/evidence/validate-evidence.py
```
