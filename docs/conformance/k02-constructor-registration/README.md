# K02-C01: imported constructor registration — 27 October 2026

**K02-C01 is complete.** Imported schema simple-type constructor functions are now registered on the first evaluation of a fresh XQuery context. All **68 constructor-registration regressions** from the wider audit pass. The four relative-import failures remain assigned to K02-C02; guarded schema loading stays default-off.

## Change and boundary

`FunctionLibrary.RegisterSchemaConstructors` extracts the existing registration loop into an internal helper, using the existing XQuery friend access. `Populate` retains its prior behavior. After query-owned schema compilation, `XQueryExecutable` invokes only that helper, guarded by `SkipStandardFunctionPopulation`. It does not repopulate the standard library. Existing host bindings are retained, and user declarations colliding with a schema constructor still raise XQST0034. No public API is added.

The correction operates on the current `XmlSchemaSet` implementation. **Bosak.Schema owns staged XSD 1.1 implementation under REQ-009.** Its S0 gap matrix and technical handover were reviewed and their hashes recorded in the [pre-implementation pin](evidence/IMPLEMENTATION-PIN.json). This slice introduces no XSD 1.1 backend or competing schema model. Any future public seam for that backend requires its separately approved compatibility ADR.

The contract is pinned to the 21 March 2017 recommendations: [XQuery schema import](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-schema-import) and [constructor functions](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#constructor-functions). Source base: `79942741`. Candidate **1.26.7 is unpublished**; public version remains **1.26.3**.

## Validation

| Check | Result |
| --- | --- |
| Focused regressions before correction | 9 failed, 2 passed |
| Final focused regressions | 11 passed |
| Complete solution projects | 21,836 passed |
| Original eligible paired cases | 324 cases / 648 isolated processes |
| Constructor regressions fixed | 68 |
| New paired regressions | 0 |
| Remaining guarded regressions | 4 relative-import cases |
| Declined boundaries | 17 cases / 34 processes, unchanged |
| External watchdogs | 682 processes, 20 seconds each, zero timeouts |
| QT3 normal gate | 31,144 / 0 / 677 |
| qt4 224-set gate | 8,436 / 0 / 1,894; 11 existing Experimental passes |

Tests cover direct calls, named references, function-lookup, list/union/QName types, empty input, invalid facets, host bindings, constructor collisions and context reuse. Public-API traces for original Castable-UnionType-7 and qischema030 now resolve both type and constructor on the first evaluation, rather than requiring a second evaluation.

The paired audit retains **319 Passed→Passed, four Passed→Failed and one HarnessException→Passed** (`validateexpr-28` through its original expected-error alternative). These are assertion outcomes, not byte-for-byte result equivalence. The original assertions, fixtures and normal-runner policy are unchanged.

Validation receipts preserve the unsuccessful attempts as well: the first fixed test run exposed an assertion-authoring mistake (`ToString()` represents a sequence as “(sequence)”), corrected to check `deep-equal` against the three list values; the normal solution build hit DLLs held by an existing XSLT conformance process, while the isolated-output solution build passed; the restricted solution test run could not bind the database provider's local HTTP listener, followed by a complete 47/47 green run of that project outside the sandbox. The full exploratory qt4 attempt was interrupted; only the completed gated run is accepted. No timeout or expected outcome was relaxed.

[RESULTS.json](RESULTS.json) consolidates paired and boundary results, unit-project receipts, traces and the four remaining failures. [Evidence manifest](evidence/manifest.json) protects raw logs, source/binary identities and prior receipts. The ZIP preserves all 682 process logs without line-ending conversion.

```powershell
python docs/conformance/k02-constructor-registration/build-review.py
python docs/conformance/k02-constructor-registration/evidence/validate-evidence.py
```

## Next slice

**K02-C02:** preserve source/base URI for relative schema imports, starting with file-backed resolver streams. Re-run the whole paired audit afterwards. Normal adoption and any skip/baseline proposal remain separate work; no enablement, release, upstream or Braid action is included here. REQ-126 remains Complete for its accepted bounded catalog.
