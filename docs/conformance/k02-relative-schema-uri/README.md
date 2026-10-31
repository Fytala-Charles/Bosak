# K02-C02: relative schema import URI — 27 October 2026

**K02 adopted — 27 October 2026:** [Owner-approved normal-runner adoption](../k02-admission-review/BASELINE-ADOPTION.md) is complete. The fresh normal QT3 gate passes **31,145/0/676**; only validateexpr-28 leaves the skip set through its original XQST0012 error alternative, and all 676 retained names/reasons match. AGENTS now requires this exact baseline. The normal runner selects the existing guard; standalone TestExecutor defaults and all profile/structural exclusions remain unchanged. Engine/candidate1.26.8 unchanged and unpublished; public1.26.3. Solution21,842 and qt4 8,436/0/1,894 are inherited source-identical validation. Bosak.Schema owns XSD1.1. REQ-126 remains Complete; earlier pending/default-off notices are historical checkpoints.



**K02 admission review — 27 October 2026:** [Guarded-runner proposal](../k02-admission-review/BASELINE-UPDATE-PROPOSAL.md) is ready for owner approval. Two full actual-runner rehearsals verify current 31,144/0/677 and guarded 31,145/0/676, with only validateexpr-28 leaving the skip set through its existing XQST0012 alternative; all 676 retained names/reasons match. Normal runner, AGENTS policy and baseline remain unchanged pending approval. Candidate 1.26.8 is unpublished/public 1.26.3. Unit21,842 and qt4 8,436/0/1,894 receipts are inherited from unchanged source; no new XSD1.1 capability. Bosak.Schema REQ-009 owns that track; REQ-126 remains Complete.



**K02-C02 is complete.** All four remaining guarded-loader regressions (`qischema032` and its a/b/c variants) now pass. The complete bounded audit has **324/324 guarded passes**, with no new failures. Normal schema initialization and the known-skip list remain unchanged; guarded mode stays default-off.

## File-backed source identity

`XQueryExecutable.BuildSchemaSet` now supplies the schema document's absolute file URI to its `XmlReader`. A resolver-returned `FileStream` uses its fully qualified `Name`; the existing file-location fallback retains its selected path. Relative imports and includes therefore resolve against the schema that contains them, including nested child schemas, independently of the query base URI or unselected location hints.

A memory stream or wrapped/non-file stream does not expose this file identity. It retains the previous empty source URI; guessing its base from a query or hint could resolve the wrong resource. A richer non-file stream identity contract remains separate work. Existing resolver, network and DTD settings are unchanged. No new public API or schema backend is added.

**Bosak.Schema owns staged XSD 1.1 implementation under REQ-009.** This correction applies to the existing `XmlSchemaSet` path. Future integration with the add-on's compiled model follows its separate compatibility ADR. Today's .NET representation limit does not describe the project's long-term schema roadmap.

The [pre-implementation pin](evidence/IMPLEMENTATION-PIN.json) records source base `6b8eefbc` and the [XQuery 3.1 schema-import contract](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-schema-import). Source candidate **1.26.8 is unpublished**; public version stays **1.26.3**.

## Validation

| Check | Result |
| --- | --- |
| New regressions before correction | 3 failed / 3 passed |
| New regressions after correction | 6 passed |
| Complete solution tests | 21,842 passed |
| Original eligible paired cases | 324 / 648 isolated processes |
| Guarded assertion outcomes | 324 passed / zero failed |
| Existing current-mode outcomes | 323 passed / one environment exception, unchanged |
| Declined boundaries | 17 cases / 34 processes, unchanged |
| External watchdogs | 682 processes at 20 seconds each, zero timeouts |
| Normal QT3 gate | 31,144 / 0 / 677 |
| Exact QT3 skip names and reasons | All 677 unchanged |
| qt4 224-set gate | 8,436 / 0 / 1,894; 11 existing Experimental passes |

The six public API cases exercise resolver file identity with misleading query/hint metadata, absolute-path and file-URI fallbacks, nested imports/includes, a non-file resolver that must not borrow a hint, self-contained non-file schemas and missing-child diagnostics. No fixture or existing assertion was modified.

The paired sweep compares original assertion outcomes, not byte-for-byte values or full schema capability. It retains 323 Passed→Passed cases and one HarnessException→Passed case (`validateexpr-28`, through its original XQST0012 error alternative). The earlier 68 constructor fixes remain green. All 17 boundary cases retain their actual decisions, exceptions and outcomes, including explicit XSD-profile exclusions.

Build and test the whole solution using isolated outputs to avoid the existing XSLT conformance process's normal-output file locks. The solution test run executes outside the restricted sandbox so the database provider tests can bind their local HTTP stub. Its Runtime project has 51 fixture lookup failures because of hard-coded relative paths in the isolated layout; a complete normal-layout rerun passes all 269 Runtime tests. The accepted total combines that run with the other nine complete green projects. No test is excluded or fixture path patched. Build has zero errors and one existing nullable warning in the XSLT conformance runner. Full gated receipts, unsuccessful pre-fix tests, binaries and original process output remain recorded.

[RESULTS.json](RESULTS.json) consolidates all outcomes. The [manifest](evidence/manifest.json) pins source/binary identities, original fixture resources, prior evidence, support logs and the lossless ZIP containing all 682 process logs.

```powershell
python docs/conformance/k02-relative-schema-uri/build-review.py
python docs/conformance/k02-relative-schema-uri/evidence/validate-evidence.py
```

## Next review

Review normal-mode applicability and the exact admission/baseline proposal separately, using this green bounded audit and the preserved profile exclusions. No normal-mode enablement, skip removal or baseline increase is authorized by these diagnostic results. The 108 inventory exclusions remain outside this evidence, and explicit XSD 1.1 profiles remain for the Bosak.Schema track. REQ-126 remains Complete for its owner-accepted bounded catalog. No release, push, tag, upstream or Braid action occurs.
