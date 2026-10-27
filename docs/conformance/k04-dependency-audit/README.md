# K04: dependency and routing applicability audit — 27 October 2026

**K04-C03 explicit feature admission - 27 October 2026:** [Capability inventory](../k04-feature-inventory/README.md) records all 78,806 catalog cases and 55 feature/profile rows. Both filters now reject unrecognized feature names for either polarity and preserve explicit legacy admissions (19 QT3/17 qt4), without certifying full support. All 222 before/after controls pass; current feature decisions and all skip names/reasons remain unchanged. Fresh solution validation passes **22,007 tests**. Fresh gates remain **31,161/0/660** and **8,437/0/1,893**. Bosak.Schema retains staged XSD 1.1 ownership. Candidate1.26.11 unpublished/public1.26.3; REQ-126 remains Complete. Next: original-assertion review of the single gated qt4 non-codepoint-collation skip. Earlier K04 notices are historical checkpoints.

**K04-C02 bounded polarity correction - 27 October 2026:** [Profile review](../k04-profile-review/README.md) makes negative spec/default-language predicates invert their existing positive predicates in both harnesses, with 74 before/after controls. An inventory of 31,821 QT3 and all 46,985 qt4 cases finds no current negative declaration of either kind. Fresh solution validation passes **22,007 tests**. Fresh gates remain **31,161/0/660** and **8,437/0/1,893**, retaining every skip name/reason. Schema comments now distinguish frozen admission from Bosak.Schema's staged XSD 1.1 implementation; admission is unchanged. F03 broader capabilities and F04 integration remain open. Candidate1.26.11 unpublished/public1.26.3; REQ-126 remains Complete. Next: explicit feature/capability inventory, beginning with unknown-positive fallback. Earlier K04 notices are historical checkpoints.

**K04-C01 adopted ? 27 October 2026:** Owner approved the exact 16-case quote-routing change proposed at `d0ca0741`. The complete normal QT3 gate passes **31,161/0/660**; exactly the 16 approved cases leave the skip set and all 660 retained names/reasons are unchanged. The qt4 gated run passes **8,437/0/1,893**, with 11 Experimental passes. All broader routing/schema/dependency guards remain unchanged. Candidate 1.26.11 remains unpublished; public version is 1.26.3. Bosak.Schema retains XSD 1.1 ownership; REQ-126 remains Complete. Next: the remaining K04 dependency-profile/polarity review. See [adoption receipt](../k04-quote-routing/adoption/BASELINE-ADOPTION.md) for approval and fresh validation. Earlier proposal notices are historical.

**K04-C01 quote-routing proposal — 27 October 2026:** [Exact adoption review](../k04-quote-routing/BASELINE-UPDATE-PROPOSAL.md) verifies all16 original candidate assertions in an isolated corrected harness. Full rehearsal passes31,161/0/660, with exactly those16 skips removed and all660 retained names/reasons unchanged. Eleven boundary cases and13 lexical controls preserve broader guards. Normal source/baseline stays31,145/0/676 pending owner approval; no tracked engine/harness/filter change. Candidate1.26.11 unpublished/public1.26.3;22,007 solution passes and qt4 8,437/0/1,893 are inherited unchanged-engine evidence. Bosak.Schema XSD1.1 ownership and REQ-126 Complete are unchanged.



**The initial K04 audit is complete; implementation and policy follow-ups remain open.** All **2,512 dependency skips** are reproduced through the actual current filters: **619 QT3** and **1,893 gated qt4**. No engine, harness, assertion or admission policy changes in this slice.

## What the skips mean

| QT3 actual filter/routing combination | Cases |
| --- | ---: |
| XQuery dependency predicates pass; syntax gate refuses | 246 |
| XQuery dependency predicates fail; syntax gate accepts | 367 |
| Both refuse | 6 |
| **Total current dependency skips** | **619** |

The audit reparses each original fixture through the actual `TestCase` parser and invokes actual filter methods for both the full dependency list and individual declarations. Syntax eligibility is recorded separately from semantic support. Multiple rejected predicates overlap; their mentions must not be added as distinct skipped cases.

Among the 246 routing-only cases, **16 contain triple double quotes**. `CanHandleAsXQuery` rejects that raw character sequence before considering doubled-quote escaping or surrounding lexical context. Five sampled quote cases pass their original assertions through public XQuery: four positive assertions and one malformed-constructor error assertion. This supports a narrow routing follow-up, not blanket removal of prolog guards.

Most gated qt4 dependency exclusions are XQuery/version-related: 1,608 mention rejected XQ10+ predicates. The qt4 runner remains XPath-only. Schema, language, static-typing and capability predicates can overlap those version exclusions. Passing a query through another language pipeline does not establish legal applicability.

## Bounded execution results

Sixteen selected original QT3 cases run in two independent diagnostic paths, each under a 20-second external watchdog. One invokes the actual direct executor; the other invokes public XQuery and the real assertion comparer against resource-free environments. Both bypass normal dependency admission only for diagnosis and preserve the original fixture/assertion. This selected public path uses no output-option overrides; it is not a universal substitute for the harness.

| Diagnostic path | Passed | Failed assertions | Executor skips |
| --- | ---: | ---: | ---: |
| Current direct executor | 2 | 2 | 12 |
| Public XQuery with original assertions | 6 | 10 | 0 |

The public XQuery successes are `K2-DirectConElemAttr-8`, `Constr-attr-syntax-5`, `DirectConElemAttr-1`, `K2-DirectConElemAttr-29`, `Literals069` and `K-CopyNamespacesProlog-5`. The first and last are negative syntax checks; they do not prove general constructor/prolog support. Only five successes concern the quote heuristic.

All seven sampled context declarations, construction stripping, a valid copy-namespace declaration and a pragma control still fail original assertions through public XQuery. Those ten failures remain failures in the review. An expected parse-error assertion can pass on an unsupported construct, so it is insufficient evidence for broad unskipping.

The 32 execution processes and four metadata processes complete without timeouts. Metadata covers 2,512 original cases plus **62 synthetic predicate assessments**; it does not execute those 2,512 queries. Heavy suites, remote resources and a full exploratory qt4 sweep are not run.

## Five open follow-ups

1. **K04-F01 / next K04-C01:** quote-aware routing. Audit all 16 candidates with positive/negative lexical controls, then rehearse the complete runner before proposing a QT3 baseline change. [Next-slice scope](NEXT-SLICE.md).
2. **K04-F02:** keep unsupported prolog/pragma guards until each construct's engine semantics are verified.
3. **K04-F03:** define explicit polarity and capability profiles. Synthetic negative mixed XP/XQ predicates can pass despite matching XPath tokens; contradictory positive/negative XQ31 predicates pass in admitted-XQuery mode. Negative fr-CA is rejected despite positive fr-CA rejection. Unknown positive features default to supported. These observations need profile review, not automatic unskips.
4. **K04-F04:** correct schema capability wording and coordinate profile predicates with Bosak.Schema. Both filters admit `xsd-version=1.1` and contain comments asserting implemented XSD 1.1, but their predicates do not establish that capability. The current core schema representation is `XmlSchemaSet`; Bosak.Schema owns staged XSD 1.1. This audit records the mismatch without changing the frozen profile.
5. **K04-F05:** keep qt4 XQuery/version exclusions distinct from core bugs; broader language routing requires its own design and evidence.

[RESULTS.json](RESULTS.json) records all findings, outcomes and overlapping counts. [Evidence](evidence/manifest.json) includes current filter records, synthetic controls, original requests/assertions, binaries, fixture pins and prior raw artifacts. The initial executor-driver compile error is preserved; all final diagnostic processes complete successfully.

```powershell
python docs/conformance/k04-dependency-audit/build-review.py
python docs/conformance/k04-dependency-audit/evidence/validate-evidence.py
```

Source is unchanged from `a707c90a`. Its **22,007 solution passes**, QT3 **31,145/0/676** and qt4 **8,437/0/1,893**, 224 gated sets and 11 existing Experimental passes are inherited, not new full-suite/gate runs. Candidate **1.26.11 remains unpublished**, public **1.26.3**. REQ-126 remains Complete for its owner-accepted bounded catalog. No fixture, assertion, filter, baseline, package, release, upstream or Braid change occurs.
