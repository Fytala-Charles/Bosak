# K02 wider schema-import audit — 27 October 2026

**K02-C02 relative schema imports — 27 October 2026:** [File-backed source URI correction](../k02-relative-schema-uri/README.md) fixes the four remaining guarded regressions. All 324 eligible original cases now pass in guarded diagnostics; 17 declined-boundary cases remain unchanged. All 21,842 solution tests and both required gates pass: QT3 31,144/0/677 (all 677 skip names/reasons unchanged), qt4 8,436/0/1,894. Guarded loading stays default-off; admission/baseline review is separate. Candidate 1.26.8 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; this slice adds no public API or backend. REQ-126 remains Complete. Earlier failure counts are historical evidence.



**K02-C01 constructor registration — 27 October 2026:** [First-evaluation correction](../k02-constructor-registration/README.md) fixes all 68 constructor regressions in the wider audit. All 11 focused tests and 21,836 solution tests pass; QT3 remains 31,144/0/677 and the qt4 gate 8,436/0/1,894. The 324-case paired sweep leaves four relative-import failures for K02-C02, with no new failures; 17 declined boundaries are unchanged. Guarded loading stays default-off. Candidate 1.26.7 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; no new public schema seam or capability is introduced. REQ-126 remains Complete. Earlier audit counts are historical evidence.



**Normal adoption is blocked.** The actual guarded loader breaks 72 cases that pass with normal eager loading. Keep `UseQueryOwnedSchemasForDiagnostics` off by default. This audit changes documentation and evidence only; REQ-126 remains Complete for its accepted bounded catalog.

## Scope and results

The pinned QT3 catalog contains 31,821 cases. Of 1,010 schema-bearing cases, 895 pass the existing baseline and 115 are skipped. Inspect the actual guard for those 895 plus seven known schema-preparation exceptions: 902 cases. Exclude 108 remaining skips (93 unsupported dependencies, 14 heavy CatalogCheck cases and one heavy Demo case). No skipped test is admitted.

| Actual guard decision | Cases |
| --- | ---: |
| Query-owned import subset | 324 |
| Declared source validation | 418 |
| XPath path | 78 |
| No query imports | 54 |
| Modules not reviewed | 19 |
| Explicit XSD profile not reviewed | 6 |
| Compile metadata unavailable | 3 |

Run each of the 324 eligible original tests independently in both modes, preserving its original assertion, routing and environment. Reproduce shared-environment base URI initialization and the runner's environment variables. All 323 baseline-passing eligible tests pass in current mode; `validateexpr-28` retains its environment exception.

| Current → guarded assertion outcome | Cases |
| --- | ---: |
| Passed → Passed | 251 |
| Passed → Failed | 72 |
| HarnessException → Passed | 1 |

The one gain is `validateexpr-28`, through its existing XQST0012 error alternative. It cannot justify adopting a mode with 72 regressions. These are assertion outcomes, not byte-for-byte result equivalence or a claim of 648 conformance passes.

Seventeen declined-boundary cases (34 processes) preserve their outcomes and actual guard decisions, including all six explicit-profile cases and all three unavailable-compile-metadata cases. In total, 902 metadata, 648 paired and 34 boundary processes complete under external 20-second watchdogs: **1,584 processes, zero timeouts or process failures**. Query failures remain recorded as failures. Four separate mechanism controls complete successfully; they are not included in that watchdog total.

## Two open corrections

**K02-C01 — constructor registration order (68 observed regressions).** `XQueryExecutable.ApplyStaticContext` populates functions before loading imported schemas. `FunctionLibrary` registers user-defined simple-type constructors only when a schema set already exists. Eager environment preparation masks this order dependency. A fresh public context for original `Castable-UnionType-7` and `qischema030` has the type after its first failed evaluation but lacks its constructor function; a second evaluation registers the constructor and succeeds without error. The trace reports registration and execution state, not independent XML assertion acceptance.

The 68 cases use unionListDefined.xsd (44), userdefined.xsd (20), notation.xsd (3), and hats.xsd (1). Fix registration after schema compilation through a dedicated reusable registration path. Preserve host bindings, duplicate/collision diagnostics and function-population policy. Cover direct calls, named references, function-lookup, list/union and namespace-sensitive types with fresh-context tests. Re-run the entire paired inventory; the audit does not pre-certify that one correction fixes every case.

**K02-C02 — relative import base URI (four observed regressions).** `qischema032` and its a/b/c variants import a schema that references sibling `qischema032a.xsd`. `BuildSchemaSet` creates a reader from a stream without its source URI. With the original schema bytes, a .NET schema-set control fails compilation without that URI and compiles with two global types and no validation events when the file URI is supplied. Both controls use the same resolver. Preserve file-backed resolver identity first; define non-file stream identity separately. Do not rewrite fixtures or schemaLocation values.

Recommended next slice: **K02-C01**, followed by **K02-C02**, then repeat this full paired audit before any normal-adoption or baseline-policy proposal.

## Evidence and verification

[RESULTS.json](RESULTS.json) retains all 72 regressions, original requests, both outcomes, boundaries, mechanism traces and open corrections. [Evidence manifest](evidence/manifest.json) pins catalog/set/resource bytes, driver snapshots, process receipts, original raw process logs in a lossless ZIP, prior evidence and source identities. The complete 902 guard decisions and 648 execution results remain separately inspectable.

Run from the repository root:

```powershell
python docs/conformance/k02-schema-inventory/build-review.py
python docs/conformance/k02-schema-inventory/evidence/validate-evidence.py
```

Engine, harness, tests, AGENTS and Fytala assets are unchanged from `ff1fd574`. Its 21,825 solution passes, QT3 **31,144/0/677**, qt4 **8,436/0/1,894**, 224 gated sets and 11 existing Experimental passes are inherited, not fresh full-suite runs. Candidate **1.26.6 remains unpublished**; public version **1.26.3**. All 677 skip names/reasons, profile limits, accepted REQ-126 dispositions and historical evidence remain unchanged. No push, tag, release, upstream or Braid action occurs.
