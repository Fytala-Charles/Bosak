# K04-C13: remaining negative capability review ? 27 October 2026

The complete pinned catalogs contain 78,806 cases and 75 effective negative declarations (35 QT3, 40 qt4). Twelve original cases cover the remaining language, calendar and Unicode-normalization dependency kinds. The actual production filters reject all twelve; direct executor diagnostics preserve their original expressions, environments and assertions.

| Negative dependency | Cases across both profiles | Direct original result |
| --- | ---: | --- |
| language=xib | 4 | Passed: existing language fallback |
| calendar=CB | 4 | Failed: unexpected FOFD1340 |
| unicode-normalization-form=FULLY-NORMALIZED | 4 | Failed: expected FOCH0003 but succeeded |

These are diagnostic executions that bypass admission, not normal gate passes. Each process has a 20-second external watchdog; all complete with exit zero and no timeout. Assertion failures remain failures. The filter report separately records XPath admission, QT3 XQuery dependency admission and syntax routing; qt4 has no XQuery route. Do not infer support or non-support from rejection of an unknown capability predicate.

## Disposition and next slice

- **C13-F01:** Four language-fallback assertions pass. Keep admission unchanged until a bounded language-capability contract and positive/negative controls distinguish unsupported xib from supported languages.
- **C13-F02 / recommended next:** Review the calendar fallback contract for both format-date and format-dateTime. The original unsupported CB calendar assertions expect an AD indication and month 03; current execution raises FOFD1340. Pin the immutable specification and separate unknown-calendar fallback from malformed calendar arguments before implementing a correction.
- **C13-F03:** Review FULLY-NORMALIZED handling separately. Both originals per profile expect FOCH0003 but currently succeed. A successful call alone does not establish full normalization support. Preserve the exact assertions and characterize supported forms, empty inputs and invalid forms before deciding the engine correction or capability predicate.

Feature negatives, the adopted numbering-sequence predicate, and qt4 revalidation declarations remain outside this bounded execution sample. Schema predicates remain coordinated with Bosak.Schema's staged XSD 1.1 ownership; unsupported prolog and language routing guards stay in place. REQ-126 remains Complete under its existing bounded acceptance.

## Evidence and validation

[Inventory](inventory.json) records every effective negative declaration and all scanned test-set hashes. [Requests](requests.json), [filter decisions](filters.jsonl), [execution receipts](execution.json) and per-case raw outputs preserve the original observations. Executor source/project snapshots and binary hashes identify the reused read-only diagnostic driver and actual normal harness assemblies. No fixture or assertion is modified. The diagnostic driver is reused from K04-C11; it is not a new production component.

Source base and fixture revisions are in [RESULTS.json](RESULTS.json). Source-identical validation is inherited from fcdca743: 22,198 solution passes; QT3 **31,162/0/659**; qt4 **8,587/0/1,892**. No fresh full-suite or gate run is claimed. Candidate 1.28.0 remains unpublished; public version remains 1.26.3. No engine, filter, baseline, version, GatedSets or release change.

Run `python docs/conformance/k04-negative-capability-review/validate-review.py` to check preserved identities and results. The original execution can be reproduced with `run-review.py` while its recorded diagnostic driver is available; this replaces only this slice's generated records, so preserve the committed receipts before any later rerun.
