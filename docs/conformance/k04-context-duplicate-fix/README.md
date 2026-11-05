# K04-C25 - Duplicate library context declaration error

28 October 2026. Implemented against `98cce43e`. Candidate **1.29.3** remains unpublished; public version is **1.26.3**.

The library-module parser now reports **XQST0099** for a second context-item declaration, as required by the adopted [XQuery 3.1 REC-20170321 section 4.17](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-context-item-declarations). Previously it reported XQST0113. The [immutable pre-implementation pin](evidence/IMPLEMENTATION-PIN.json) records the exact source revision/hash and bounded scope.

Only the duplicate-declaration error and its comment change in the parser body. Library initializers and external default values still raise **XQST0113**. Valid typed/untyped external declarations and supplied-context type checks retain their behavior. Dynamic loading retains its **FOQM0003** wrapper around invalid-library parse errors. Source history is 1.17; the candidate version advances by one patch.

## Verification

- **24 new public API regressions pass**; before implementation, eight failed and 16 passed. They cover imports and `fn:load-xquery-module`, typed/untyped and commented duplicate declarations, valid modules, forbidden initializers/defaults, wrong external types and the retained main-module parser boundary. Existing tests and all original catalog assertions remain unchanged.
- The complete solution build and a fresh complete solution test run pass: **23,770 tests**, including **371 XQuery**, **13,797 Standard** and **8,196 XSLT** tests.
- Replaying C24 preserves all **80 original/public-control results** and all **97 actual-filter assessments**. The 31 resource-free QT3 originals still produce 1 syntax-error pass/30 public-XQuery failures and 1 pass/30 actual-executor skips; 18 independent controls retain 3 passes/15 failures. These main-module limitations remain open.
- All **five library parser controls now pass**. Exactly `library-duplicate` changes from failure to pass, with XQST0099; the other four results/messages are unchanged.
- Complete normal context-declaration sets stay **4/0/40** (QT3) and **0/0/53** (qt4), retaining all skip names/reasons.
- Fresh complete frozen gates stay QT3 **31,165/0/656**, qt4 **8,939/0/1,920**, with **230 gated sets** and 11 Experimental passes. Every retained skip name/reason is identical to the approved C23 baseline.

The 90 final bounded gate/diagnostic processes finish without timeout: 80 execution probes, five library controls, two context-declaration sets, one filter inventory and two complete gates. Failed assertions in the preserved C24 main-module cohort remain failed assertions; successful process completion is not a conformance pass claim. C24's initial diagnostic setup failure and all its historical artifacts are preserved byte-for-byte rather than rewritten.

[Raw evidence](evidence/manifest.json), [source identities](evidence/source-identities.json), [review deltas](evidence/review-deltas.json), [gate skip comparisons](evidence/skip-verification.json), [results](RESULTS.json) and [validator](evidence/validate-evidence.py) establish the exact scope. Nine Markdown pages render with unchanged Fytala CSS; HTML rendering is not screenshot/PDF acceptance.

## Next bounded slice

Implement main-module context-item declarations with explicit pre-implementation adoption, parser/compiled representation and runtime initialization. Preserve typed singleton validation, external supplied/default behavior, focus, forward global-variable dependencies, cycle detection and library restrictions. Use C24's original cohort without weakening any accepted error alternative. Audit its 13 resource/module-bearing QT3 boundaries before changing the context routing guard. Qt4 XQuery-only cases do not become applicable through diagnostic execution.

No public API, fixture/assertion, routing/dependency guard, KnownSkips, GatedSets, AGENTS baseline, schema/backend, release, publication, push, tag, upstream or Braid change is included. Bosak.Schema retains staged XSD 1.1 ownership. REQ-126 stays Complete under bounded owner acceptance; all original dispositions and 48 historical unmet batch observations are preserved.
