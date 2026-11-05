# Next bounded slice: XPath4 radix formatting

Review base:65d1a731. K04-C08 is a read-only review. This plan is not an implementation receipt or a request to change the frozen QT3 baseline.

1. Record explicit semantic adoption before code changes using the reviewed immutable qt4cg revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e, exact sections and intentional numeric/profile limits. Keep the review pin, historical REQ-126 acceptance and future adoption distinct.
2. Add a radix path only when the active context is XPath4 and the primary picture starts with an eligible ASCII radix2..36, without leading zero, followed by caret and later x/X. Preserve decimal caret grouping when no radix is recognized. Split the format modifier before interpreting the picture.
3. Convert the existing Int64 input via safe magnitude handling (including Int64.MinValue), honor uppercase/lowercase digit families, mandatory zero padding and optional digits. Reuse reviewed regular/irregular grouping mechanics while validating radix digit patterns, Unicode separator placement and caret separators.
4. Use the original integer to select numeric ordinal suffixes; rendered hexadecimal letters must not select the word-ordinal branch. Retain existing explicit modifier/fallback behavior in lower profiles.
5. Add positive and edge regressions: bases2..36, zero/negative/min/max values, case, padding, grouping, optional digits, bad ranges/mixedcase/separators, empty input, original-integer ordinals, preserved3.1 refusals and both existing caret-grouping cases.
6. Recheck all17 original assertions. Target14 formatter-only failures and preserve014/015. Keep012 open because parse-integer#2 is independently unavailable; do not weaken its assertion. Its failure may expose XPST0017 after the earlier formatter failure is corrected.
7. Run complete solution tests and both conformance gates, comparing all frozen skip names/reasons. The exploratory set must stay ungated until every failure is resolved. With exactly14 fixes and no other changes, its expected checkpoint is100passed/1failed/1skipped,not a green set.

Treat radix picture syntax as new functionality under repository versioning (a minor candidate bump), not as another CLDR patch. Preserve public1.26.3 and do not release automatically. No request to change AGENTS.md, existing gates, REQ-126 status, XSD1.1 ownership or upstream fixtures is included.

Follow with a separate parse-integer contract slice, including arities, profile gating, radix validation, invalid lexical input and round-trip semantics. Wide-integer conversion remains separate unless explicitly brought into scope with its own evidence.
