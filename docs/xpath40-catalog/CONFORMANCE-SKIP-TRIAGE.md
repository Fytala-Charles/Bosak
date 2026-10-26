<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala QT3 and qt4 skipped conformance triage">
  <h1>Skipped conformance: applicability and addressable work</h1>
  <p>26 October 2026 — released Bosak 1.26.3; frozen baselines preserved</p>
</div>

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
| K01 | Schema-list stale skip/error-code pair | **First:** add targeted regressions, review schema/profile semantics, fix the remaining mismatch and remove only justified skips |
| K02 | Seven schema-loader cases | Separate duration built-in resolution from six multi-affiliation substitution cases; establish supported XSD subset/design before implementing |
| K03 | Supplementary XML-name construction | Trace QName/XDM/provider/serialization behavior; retain the actual Unicode name and assertion |
| K04 | Dependency/routing applicability | Review negative/unknown capabilities and version/routing policy; stage resource-bounded wider qt4 exploration separately |
| K05 | Billion-item range guard | Establish a spec-correct early bound and bounded regression before executing the original dangerous case |
| K06 | Heavy-suite policy | Design isolated opt-in runs; do not add all corpus-loading/demo/benchmark cases to the fast gate |

These six investigations are separate from the [13 REQ-126 catalog closure packages](CLOSURE-PLAN.md). One cluster may need multiple implementation PRs, especially K02/K04. The eight numeric/calendar boundary cases and the upstream/external cases are explicitly deferred dispositions, not promised quick fixes.

**Baseline constraint:** AGENTS.md still requires exact QT3 **31,142/0/679** and frozen qt4 **8,436/0/1,894**. This audit preserves both. Removing even a stale skip changes the expected totals and therefore needs a separately reviewed baseline-policy update alongside evidence. No count is revised silently and no criterion is weakened to produce more green results.

Before any upstream correction, fetch the current upstream default branch, prepare/rebase against it, record the exact base, refresh again immediately before submission and preserve original execution/submission receipts. No upstream submission occurs here. Current unit/CI success is inherited from the unchanged published source; fresh validation here is the two harness runs, inventories, actual filter checks, bounded diagnostics, reproducibility checks and documentation rendering.
