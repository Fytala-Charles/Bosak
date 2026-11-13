# Scoped analyze-string error reporting — 13 November 2026

The reviewed correction maps regex preparation failures to `XTDE1140` and invalid flags to `XTDE1145` only within `xsl:analyze-string`. Select/regex/flags AVT evaluation and matching/non-matching child execution retain their original XPath errors. The shared XPath RegexHelper and the production legacy matcher are unchanged. Preparation, child validation and native regex construction keep their original order.

The [immutable adoption record](ADOPTION.json) precedes implementation and pins XSLT 3.0 Recommendation 8 June 2017, sections 2.14 and 17.1. Development Core/server metadata is 1.81.6, unpublished; published Core1.81.2 and extension0.2.6 remain unchanged.

Fourteen focused tests pass, including named `xsl:catch` patterns and the exact standard namespace/local name, native pattern construction, literal quote-mode behavior, both AVT error codes and both child callbacks in tree/typed evaluation. Identical sources against nine sealed1.81.5 assemblies produce four regression failures and ten protective/happy-path passes. The source of those tests is hash-bound to the baseline result.

Fresh solution, frozen-gate, targeted/oracle and complete standalone qualification are pending. No admission, fixture edit, new skip or gate-budget/ruleset change is proposed. The adopted required gate stays10,221/0/4,380; the579-case error set remains excluded. The diagnostic exact-identity audit will separately verify that the two original cases no longer use legacy aliases.
