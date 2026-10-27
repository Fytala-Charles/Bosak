<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala QT3 and qt4 skipped conformance triage">
  <h1>Skipped conformance: applicability and addressable work</h1>
  <p>26 October 2026 — released Bosak 1.26.3; frozen baselines preserved</p>
</div>

**K04-C06 formatter correction - 27 October 2026:** [Bounded integer-variation fix](../conformance/k04-cldr-fix/README.md) accepts cardinal variations and preserves known German named ordinal endings using pinned F&O/CLDR semantics. All six original assertions and22 diagnostic controls pass. The new36 regression rows pass in both compatibility paths (21failed before). Fresh solution validation passes **22,043 tests**; gates remain **31,161/0/660** and **8,438/0/1,892**, with every skip name/reason unchanged. Candidate **1.26.12** remains unpublished/public1.26.3. No qt4 CLDR admission, fixture/assertion, GatedSets, schema or QT3 baseline change. Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Next: separate bounded qt4 CLDR admission review. Earlier K04 notices are historical checkpoints.

**K04-C05 CLDR review - 27 October 2026:** [CLDR diagnosis](../conformance/k04-cldr-review/README.md) preserves the original two QT3 passes, two qt4 direct passes and two qt4 FODF1310 failures. Normal qt4 still dependency-skips all four cases. Focused controls isolate rejected named-cardinal modifiers and lost German named-ordinal endings; initial control-expectation mistakes are preserved separately. No source/admission/baseline change. Inherited source-identical results:22,007 solution passes, QT3 **31,161/0/660**, qt4 gated **8,438/0/1,892**; no fresh full-suite claim. Next: pin immutable specification/CLDR semantics and implement a bounded formatter correction before reviewing admission. Candidate1.26.11 unpublished/public1.26.3; Bosak.Schema owns staged XSD1.1; REQ-126 remains Complete. Earlier K04 notices are historical checkpoints.

**K04-C04 bounded collation admission - 27 October 2026:** [Collation review](../conformance/k04-collation-admission/README.md) admits existing non-codepoint collation routes in qt4. The original compare-010 assertion passes; the normal qt4 gate improves to **8,438/0/1,892**, with only that gated skip removed and all retained reasons unchanged. Four exploratory original assertions also pass; eight XQuery-only cases remain dependency-skipped, with their direct diagnostic failures preserved. Seven expression controls and248 before/after filter controls pass. Fresh solution validation passes **22,007 tests**. Normal QT3 remains **31,161/0/660**. No engine, fixture/assertion, GatedSets, schema or QT3 baseline change. Candidate1.26.11 unpublished/public1.26.3; Bosak.Schema retains XSD1.1 ownership; REQ-126 remains Complete. Next: bounded review of four qt4 CLDR format-integer declarations. Earlier K04 notices are historical checkpoints.

**K04-C03 explicit feature admission - 27 October 2026:** [Capability inventory](../conformance/k04-feature-inventory/README.md) records all 78,806 catalog cases and 55 feature/profile rows. Both filters now reject unrecognized feature names for either polarity and preserve explicit legacy admissions (19 QT3/17 qt4), without certifying full support. All 222 before/after controls pass; current feature decisions and all skip names/reasons remain unchanged. Fresh solution validation passes **22,007 tests**. Fresh gates remain **31,161/0/660** and **8,437/0/1,893**. Bosak.Schema retains staged XSD 1.1 ownership. Candidate1.26.11 unpublished/public1.26.3; REQ-126 remains Complete. Next: original-assertion review of the single gated qt4 non-codepoint-collation skip. Earlier K04 notices are historical checkpoints.

**K04-C02 bounded polarity correction - 27 October 2026:** [Profile review](../conformance/k04-profile-review/README.md) makes negative spec/default-language predicates invert their existing positive predicates in both harnesses, with 74 before/after controls. An inventory of 31,821 QT3 and all 46,985 qt4 cases finds no current negative declaration of either kind. Fresh solution validation passes **22,007 tests**. Fresh gates remain **31,161/0/660** and **8,437/0/1,893**, retaining every skip name/reason. Schema comments now distinguish frozen admission from Bosak.Schema's staged XSD 1.1 implementation; admission is unchanged. F03 broader capabilities and F04 integration remain open. Candidate1.26.11 unpublished/public1.26.3; REQ-126 remains Complete. Next: explicit feature/capability inventory, beginning with unknown-positive fallback. Earlier K04 notices are historical checkpoints.

**K04-C01 adopted ? 27 October 2026:** Owner approved the exact 16-case quote-routing change proposed at `d0ca0741`. The complete normal QT3 gate passes **31,161/0/660**; exactly the 16 approved cases leave the skip set and all 660 retained names/reasons are unchanged. The qt4 gated run passes **8,437/0/1,893**, with 11 Experimental passes. All broader routing/schema/dependency guards remain unchanged. Candidate 1.26.11 remains unpublished; public version is 1.26.3. Bosak.Schema retains XSD 1.1 ownership; REQ-126 remains Complete. Next: the remaining K04 dependency-profile/polarity review. See [adoption receipt](../conformance/k04-quote-routing/adoption/BASELINE-ADOPTION.md) for approval and fresh validation. Earlier proposal notices are historical.

**K04-C01 quote-routing proposal — 27 October 2026:** [Exact adoption review](../conformance/k04-quote-routing/BASELINE-UPDATE-PROPOSAL.md) verifies all16 original candidate assertions in an isolated corrected harness. Full rehearsal passes31,161/0/660, with exactly those16 skips removed and all660 retained names/reasons unchanged. Eleven boundary cases and13 lexical controls preserve broader guards. Normal source/baseline stays31,145/0/676 pending owner approval; no tracked engine/harness/filter change. Candidate1.26.11 unpublished/public1.26.3;22,007 solution passes and qt4 8,437/0/1,893 are inherited unchanged-engine evidence. Bosak.Schema XSD1.1 ownership and REQ-126 Complete are unchanged.



**K04 applicability audit — 27 October 2026:** [Current dependency/routing review](../conformance/k04-dependency-audit/README.md) reproduces all619 QT3 and1,893 gatedqt4 dependency skips through actual filters. Of619 QT3 cases,246 satisfy XQuery dependencies but fail syntax routing;16 are triple-quote candidates. Thirty-two bounded execution probes yield six public-XQuery assertion passes and ten retained failures;62 synthetic predicate assessments expose profile/polarity questions. No engine, filter or baseline change. Next K04-C01: quote-aware routing with complete candidate and rehearsal evidence before any QT3 admission proposal. Candidate1.26.11 unpublished/public1.26.3; inherited22,007 solution passes and gates31,145/0/676 and8,437/0/1,893. Bosak.Schema owns XSD1.1; REQ-126 remains Complete.



**K03 name-storage integration — 27 October 2026:** [Bounded core completion](../conformance/k03-name-storage/README.md) preserves Unicode map/provider names, literal values and namespace/copy metadata. All47 new regressions and22,007 solution tests pass. The original map-to-element-053 assertion now passes; qt4 improves to8,437/0/1,893 with exactly that skip removed and all1,893 retained reasons unchanged. QT3 remains31,145/0/676 with identical676 skips. No fixture, assertion, harness filter or gated-set change. Candidate1.26.11 unpublished/public1.26.3. Parser-edition/current-draft limits remain explicit; Bosak.Schema retains XSD1.1 ownership and REQ-126 remains Complete. Next: K04 dependency/routing applicability audit. Earlier open Unicode findings are historical checkpoints.



**K03-C02 serialization — 27 October 2026:** [Content-preserving name decoding](../conformance/k03-serialization/README.md) restores encoded element/attribute name tokens while preserving existing literal node content, namespace URIs and declarations. All40 focused regressions and21,960 solution tests pass; QT3 remains31,145/0/676 with identical676 skips, and qt4 remains8,436/0/1,894. The original supplementary-name skip remains. Candidate1.26.10 unpublished/public1.26.3. Next: reversible name-storage integration with loader/accessor/copy round-trip verification; no public API, baseline or XSD1.1 ownership change. REQ-126 remains Complete.



**K03-C01 XML-name validation — 27 October 2026:** [Scalar validation correction](../conformance/k03-name-validation/README.md) replaces category/surrogate-pair approximation with explicit XML name ranges and rejects malformed UTF-16. All78 focused tests pass (baseline18 failures), all21,920 solution tests pass, and gates stay QT3 31,145/0/676 and qt4 8,436/0/1,894. All676 QT3 skip names/reasons are unchanged. Original supplementary construction and literal-content serialization findings remain open; next is K03-C02 serialization preservation before provider storage integration. Candidate1.26.9 unpublished/public1.26.3; no baseline or XSD1.1 capability change. REQ-126 remains Complete.



**K03 Unicode-name trace — 27 October 2026:** [Bounded investigation](../conformance/k03-unicode-names/README.md) reproduces map-to-element-053 and separates three open corrections: scalar NCName validation, reversible provider name storage, and serialization that preserves literal sentinel text. All 30 isolated diagnostics complete without timeouts; no engine or baseline change. Next: K03-C01 name-validation regressions/correction, followed by serialization preservation before map storage integration. QT3 remains31,145/0/676 and qt4 remains8,436/0/1,894; solution21,842 and gates are inherited unchanged-source evidence. Candidate1.26.8 unpublished/public1.26.3; REQ-126 remains Complete. Bosak.Schema XSD1.1 ownership is unchanged.



**K02 adopted — 27 October 2026:** [Owner-approved normal-runner adoption](../conformance/k02-admission-review/BASELINE-ADOPTION.md) is complete. The fresh normal QT3 gate passes **31,145/0/676**; only validateexpr-28 leaves the skip set through its original XQST0012 error alternative, and all 676 retained names/reasons match. AGENTS now requires this exact baseline. The normal runner selects the existing guard; standalone TestExecutor defaults and all profile/structural exclusions remain unchanged. Engine/candidate1.26.8 unchanged and unpublished; public1.26.3. Solution21,842 and qt4 8,436/0/1,894 are inherited source-identical validation. Bosak.Schema owns XSD1.1. REQ-126 remains Complete; earlier pending/default-off notices are historical checkpoints.



**K02 admission review — 27 October 2026:** [Guarded-runner proposal](../conformance/k02-admission-review/BASELINE-UPDATE-PROPOSAL.md) is ready for owner approval. Two full actual-runner rehearsals verify current 31,144/0/677 and guarded 31,145/0/676, with only validateexpr-28 leaving the skip set through its existing XQST0012 alternative; all 676 retained names/reasons match. Normal runner, AGENTS policy and baseline remain unchanged pending approval. Candidate 1.26.8 is unpublished/public 1.26.3. Unit21,842 and qt4 8,436/0/1,894 receipts are inherited from unchanged source; no new XSD1.1 capability. Bosak.Schema REQ-009 owns that track; REQ-126 remains Complete.



**K02-C02 relative schema imports — 27 October 2026:** [File-backed source URI correction](../conformance/k02-relative-schema-uri/README.md) fixes the four remaining guarded regressions. All 324 eligible original cases now pass in guarded diagnostics; 17 declined-boundary cases remain unchanged. All 21,842 solution tests and both required gates pass: QT3 31,144/0/677 (all 677 skip names/reasons unchanged), qt4 8,436/0/1,894. Guarded loading stays default-off; admission/baseline review is separate. Candidate 1.26.8 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; this slice adds no public API or backend. REQ-126 remains Complete. Earlier failure counts are historical evidence.



**K02-C01 constructor registration — 27 October 2026:** [First-evaluation correction](../conformance/k02-constructor-registration/README.md) fixes all 68 constructor regressions in the wider audit. All 11 focused tests and 21,836 solution tests pass; QT3 remains 31,144/0/677 and the qt4 gate 8,436/0/1,894. The 324-case paired sweep leaves four relative-import failures for K02-C02, with no new failures; 17 declined boundaries are unchanged. Guarded loading stays default-off. Candidate 1.26.7 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; no new public schema seam or capability is introduced. REQ-126 remains Complete. Earlier audit counts are historical evidence.



**K02 wider audit — 27 October 2026:** [Schema/import inventory](../conformance/k02-schema-inventory/README.md) blocks normal adoption: 324 eligible original cases yield 251 retained passes, 72 regressions and one environment-exception-to-pass transition. All 1,584 metadata/paired/boundary processes complete without timeouts. Two corrections remain open: schema constructor registration after import (68 observed regressions), then relative schema-import base URI (four). Guarded loading stays default-off; no source, skip, baseline or version change. Next: K02-C01 constructor registration. Candidate 1.26.6 unpublished/public 1.26.3; REQ-126 remains Complete. Earlier narrow-sample results remain historical evidence.



**K02 guarded loader diagnostics — 27 October 2026:** [Implemented guarded mode](../conformance/k02-schema-loader/README.md) delegates eligible schemas to actual query imports only when explicitly enabled for diagnostics; normal eager loading stays the default. All 66 actual-harness diagnostic runs match independent expectations, including wrong-assertion failures and preserved source/profile boundaries. All 21,825 solution tests and both normal gates pass at 31,144/0/677 and 8,436/0/1,894; all 677 QT3 skips/reasons are unchanged. Candidate 1.26.6 is unpublished. K02 profile/capability and wider applicability work remain open; no admission or baseline change. Earlier checkpoints are historical.


**K02 bounded diagnostic correction — 27 October 2026:** [Duration-schema investigation](../conformance/k02-schema-duration/README.md) reproduces the unavailable schema duration types and corrects the public aggregated schema-import error to XQST0012. All 9 regressions pass (baseline 4 failed/5 passed). The original public query reaches its declared error alternative, but the QT3 environment still fails before query execution; validateexpr-28 remains skipped. No XSD 1.1 capability, skip, fixture or baseline change. Candidate 1.26.5 is unpublished; public 1.26.3 and REQ-126 acceptance are unchanged. Earlier checkpoints are historical.


**K01 complete; approved QT3 baseline — 27 October 2026:** [Exact two-case adoption](../conformance/k01-schema-list/BASELINE-ADOPTION.md) removes only CastAs-ListType-20 and -28 from known skips. The fresh full QT3 gate passes **31,144/0/677**; qt4 remains 8,436/0/1,894. The owner approval and original receipts are preserved; no fixture/assertion/filter change. Source candidate 1.26.4 remains unpublished and REQ-126 remains Complete. Earlier totals and pending-approval notices are historical.


**K01 schema-list correction — 27 October 2026:** [Correction and evidence](../conformance/k01-schema-list/README.md) fixes legacy typed bindings reporting XPTY0004 instead of XPST0051 for invalid list item types. All 18 regressions pass (baseline 6 failed/12 passed); direct original QT3 cases CastAs-ListType-20 and -28 now both pass. Both skips and frozen totals stay unchanged pending a separate baseline-policy approval. Source candidate 1.26.4 is unpublished; public 1.26.3 and REQ-126 bounded acceptance are unchanged. Earlier checkpoints are historical.


**Historical initial triage — 26 October 2026; K01 is superseded by the correction above.**

**Yes, some skipped cases merit follow-up.** The strongest immediate lead is the QT3 schema-list pair: `CastAs-ListType-28` already passes in the released executor, while `CastAs-ListType-20` still returns XPTY0004 where its assertion expects XPST0051. This task investigates and plans work; it changes no skip, assertion, fixture, capability declaration, retry policy or frozen expected count.

## Scope and counts

| Run | Passed /failed /skipped | Scope |
|---|---|---|
| Fresh QT3 | 31,142 /0 /679 | 428 sets, all 31,821 catalog cases |
| Fresh qt4 frozen gate | 8,436 /0 /1,894 | 224 gated sets, 10,330 cases; 11 existing Experimental fallback passes |
| Full frozen qt4 catalog | Not executed in full | Static inventory: 738 sets /46,985 cases |

The **36,655 cases in 514 qt4 sets outside the gate were not executed here**. The 1,894 skip count is the gated subset, not a full-qt4-suite skip total. A wider exploratory sweep needs an explicitly bounded process/resource plan because some evaluations can run away. Configured set membership is distinct from executed coverage and draft conformance.

| Combined frozen-scope group | Cases | Initial disposition |
|---|---:|---|
| Dependency-filtered | 2,512 | 619 QT3 +1,893 qt4; applicability/routing must be reviewed before considering implementation |
| Heavy suite policy | 40 | 14 catalog consistency, 5 demos, 21 benchmarks; retain opt-in/resource boundaries |
| Schema-loader exceptions | 7 | Six multi-QName substitution-group cases; one missing schema duration type |
| Documented schema-list gaps | 2 | One stale-skip lead, one reproduced error-code mismatch |
| Supplementary XML-name construction | 1 | Reproduced XML-name exception |
| Numeric/calendar/range boundaries | 8 | Fixed decimal, extended dates/years and huge duration/year inputs; larger capability work |
| Dangerous range guard | 1 | Potential billion-item enumeration; deliberately not executed |
| Upstream assertion | 1 | Retained leading-space defect record; refresh upstream before proposing any correction |
| External service | 1 | Retained Cloudflare-blocked HTTP test; not an engine fix |
| **Total** | **2,573** | Every skip has a pinned set/case/dependency record |

[Machine-readable triage](CONFORMANCE-SKIP-TRIAGE.json), [case inventory](evidence/closure-triage/skip-cases.json), [suite inventory](evidence/closure-triage/suite-inventory.json) and [actual filter results](evidence/closure-triage/filter-results.jsonl) retain exact fixture revisions, set hashes, declarations, reasons and routing assessments. The filter diagnostic invokes the actual released internal filter methods through a generated read-only utility; it does not infer rejection from a reimplemented Python filter.

These are local Release harness binaries compiled from the clean approved `08fbdc31` source checkout, with binary hashes retained. Conformance harnesses are not signed public NuGet packages; this investigation does not claim otherwise.

## What “unsupported dependency” conceals

For qt4, **1,873 of 1,893 filtered cases contain a positive XQuery-only specification clause**, including **106 XQ40+ clauses**. Some have additional blockers. There is no XQuery 4.0 pipeline in this harness. Routing them through XQuery 3.1 and calling the result XPath 4.0 conformance would be invalid. Three cases mention `typedData` as a rejected feature, but **none is blocked only by typedData**; removing that exclusion alone would admit none of these cases.

For QT3, **246 cases have dependencies admitted by the XQuery-aware filter but are rejected by syntax/routing checks**. Others involve exact-version semantics, static typing, XML/XSD versions, Unicode/language/calendar assumptions, or multiple conditions. Negative dependencies matter: a test for a feature's absence can correctly remain excluded when the implementation supports it. The JSON preserves all rejected predicates; primary inventory labels and overlapping predicate mentions are explicitly distinguished.

## Bounded executor probes

Twenty-six selected cases were evaluated in separate released-executor processes with a 20-second external watchdog per case. Results: **6 passed, 8 failed assertions, 7 schema exceptions, 5 executor skips; no timeout**. These are diagnostic outcomes outside normal admission and gate accounting. Failures in an intentionally inapplicable test are not automatically engine defects, and passes are not automatically legal unskips.

| Group | Observation | Meaning |
|---|---|---|
| QT3 schema-list | `CastAs-ListType-28` passed; `-20` failed with XPTY0004 versus XPST0051 | First bounded regression/skip review candidate |
| Schema imports | Six `substitution-020`–`025` exceptions and one `validateexpr-28` exception reproduced | Loader/capability work, not seven ordinary XPath arithmetic bugs |
| qt4 XML names | `map-to-element-053` remains skipped on U+1D11E name construction | Provider/XML representation boundary |
| QT3 routing sample | One pass, three assertion failures, four executor skips | Routing exclusions include both stale-looking and genuine unsupported paths |
| Conditional-dependency sample | Four passes and four failures | Validate capability/profile predicates before changing admission |

The six passes are `CastAs-ListType-28`, `K2-DirectConElemAttr-8`, `collection-004`, `collection-008`, `format-date-en151` and `format-dateTime-en151`. Only the first is a documented-skip case without the sampled dependency/routing complications. The malformed-constructor error pass does not establish general constructor support; conditional collection/language passes do not waive their dependencies. All original observations and requests are retained in [diagnostic receipts](evidence/closure-triage/diagnostic-receipts.json).

The observed multi-QName schema failure is a real capability boundary: XSD 1.1 specifies `substitutionGroup` as a list of QNames. [W3C XSD 1.1 Structures](https://www.w3.org/TR/xmlschema11-1/#element-element). XML 1.1's name-start range includes supplementary characters such as U+1D11E, so replacing the test's name with ASCII would not resolve the stated contract. [W3C XML 1.1](https://www.w3.org/TR/xml11/#NT-NameStartChar).

## Six separately tracked investigation clusters

| ID | Follow-up | Recommendation |
|---|---|---|
| K01 | Schema-list stale skip/error-code pair | Complete: correction verified and exactly two skips removed with explicit approval; fresh QT3 31,144/0/677. Original frozen triage below remains historical. |
| K02 | Seven schema-loader cases | Guarded diagnostic mode verified in candidate1.26.6; duration error alternative passes only when opted in. Normal loading, explicit-profile/substitution capability and wider applicability remain open; all677 normal skips unchanged. |
| K03 | Supplementary XML-name construction | Trace QName/XDM/provider/serialization behavior; retain the actual Unicode name and assertion |
| K04 | Dependency/routing applicability | Review negative/unknown capabilities and version/routing policy; stage resource-bounded wider qt4 exploration separately |
| K05 | Billion-item range guard | Establish a spec-correct early bound and bounded regression before executing the original dangerous case |
| K06 | Heavy-suite policy | Design isolated opt-in runs; do not add all corpus-loading/demo/benchmark cases to the fast gate |

These six investigations are separate from the [13 REQ-126 catalog closure packages](CLOSURE-PLAN.md). One cluster may need multiple implementation PRs, especially K02/K04. The eight numeric/calendar boundary cases and the upstream/external cases are explicitly deferred dispositions, not promised quick fixes.

**Baseline constraint:** AGENTS.md still requires exact QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894**. This audit preserves both. Removing even a stale skip changes the expected totals and therefore needs a separately reviewed baseline-policy update alongside evidence. No count is revised silently and no criterion is weakened to produce more green results.

Before any upstream correction, fetch the current upstream default branch, prepare/rebase against it, record the exact base, refresh again immediately before submission and preserve original execution/submission receipts. No upstream submission occurs here. Current unit/CI success is inherited from the unchanged published source; fresh validation here is the two harness runs, inventories, actual filter checks, bounded diagnostics, reproducibility checks and documentation rendering.
