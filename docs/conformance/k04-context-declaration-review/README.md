# K04-C24 - XQuery context-item declaration review

28 October 2026. Review complete against `bf732468`; **no engine, harness, guard, fixture/assertion, baseline, version or release change**. Candidate **1.29.2** remains unpublished; public **1.26.3**.

K04-F02 retains prolog/pragma guards until semantics are verified. This slice reviews context-item declarations as the next bounded target. The parser recognizes these declarations only for library modules; main-module declarations reach the body parser and fail with XPST0003. The routing guard is therefore still necessary. Removing it would expose genuine unsupported behavior.

## Evidence and coverage

| Observation | Result | Meaning |
|---|---|---|
| Static original inventory | 44 QT3 + 53 qt4 cases | Every expression, assertion, dependency and environment/module boundary recorded |
| Selected resource-free, no-module QT3 originals | 31 | Actual XQuery dependency predicates pass; actual router refuses all 31 |
| Direct actual executor | 1 pass / 30 skips | A bypass diagnostic, not normal admission |
| Public XQuery with unchanged original assertions | 1 pass / 30 failures | All 31 raise parser errors; one original expects XPST0003 |
| Independent public XQuery controls | 3 passes / 15 failures | Arithmetic, global variable and supplied focus baselines pass; declaration controls fail at parsing |
| Library parser boundary controls | 4 passes / 1 failure | Typed/external declarations and initializer/default refusal work; duplicate declaration reports the wrong error code |
| Fresh normal QT3 context-declaration set | **4/0/40** | Existing admission preserved |
| Fresh normal qt4 context-declaration set | **0/0/53** | XQuery-only applicability exclusions preserved; no qt4 XQuery execution claim |

The only passing selected original is `contextDecl-023`, whose invalid occurrence indicator expects XPST0003. Its parser-error pass does not demonstrate declaration support or justify unskipping. Thirty positive/error-alternative originals still fail their unchanged assertions. Public diagnostics preserve supplied literal context items through the actual environment adapter and reject resource-bearing environments.

Thirteen QT3 originals are declined: ten have module mappings and three require resource-bearing environments. All 53 qt4 cases are inventoried and filter-assessed, without routing them through a different language to claim applicability. All original fixtures and environment boundaries remain unchanged.

All **88 final bounded diagnostic processes** complete successfully without timeout: 80 original/control executions, five library-parser controls, two normal sets and one actual-filter inventory. These successful process exits include the documented failed assertions above. An initial library diagnostic used the wrong reflection namespace, raised TypeLoadException and was ended by its 20-second watchdog. Its source/log and separate disposition are retained; the corrected five library probes finish normally. This setup failure is not an engine timeout finding.

## Confirmed adjacent defect

Duplicate context-item declarations in a library module return **XQST0113**, whereas the pinned XQuery 3.1 section 4.17 requires **XQST0099**. The existing XQST0113 refusals for library initializers and external defaults pass their independent controls and must be preserved. This is a small, separately testable parser correction before the larger main-module work.

The immutable [review pin](evidence/REVIEW-PIN.json) records [XQuery 3.1 REC-20170321](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-context-item-declarations), its SHA-256 and affected sections. This is review evidence, not new semantic adoption. [Next-slice scope](NEXT-SLICE.md) separates the duplicate error fix, main-module implementation and later admission audit.

## Validation boundary

Production source, AGENTS policy and both fixture revisions are unchanged. The **23,746 solution passes**, QT3 gate **31,165/0/656**, qt4 gate **8,939/0/1,920**, 230 gated sets and 11 Experimental passes are inherited source-identical evidence from the completed C23 adoption. No fresh full-solution or full-gate run is claimed in this review.

[Inventory](evidence/inventory.json), [actual filter summary](evidence/filter-summary.json), [execution receipts](evidence/execution-receipts.json), [library receipts](evidence/library-receipts.json), [results](RESULTS.json), [validator](evidence/validate-evidence.py) and [manifest](evidence/manifest.json) retain the original failures, exact source/binary identities and prior C23 adoption artifacts. The final driver adds only a library-parser dispatch branch; its original execution paths are preserved separately. HTML receipts use unchanged Fytala CSS and are not screenshot/PDF acceptance.

REQ-126 stays Complete under bounded owner acceptance. All original dispositions and 48 historical unmet batch observations remain preserved. Bosak.Schema owns staged XSD 1.1 work. No baseline proposal, publication, upstream submission or Braid change is included.
