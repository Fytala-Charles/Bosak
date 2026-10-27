# K02: guarded query-owned schema diagnostics

**K02 adopted — 27 October 2026:** [Owner-approved normal-runner adoption](../k02-admission-review/BASELINE-ADOPTION.md) is complete. The fresh normal QT3 gate passes **31,145/0/676**; only validateexpr-28 leaves the skip set through its original XQST0012 error alternative, and all 676 retained names/reasons match. AGENTS now requires this exact baseline. The normal runner selects the existing guard; standalone TestExecutor defaults and all profile/structural exclusions remain unchanged. Engine/candidate1.26.8 unchanged and unpublished; public1.26.3. Solution21,842 and qt4 8,436/0/1,894 are inherited source-identical validation. Bosak.Schema owns XSD1.1. REQ-126 remains Complete; earlier pending/default-off notices are historical checkpoints.



**K02-C02 relative schema imports — 27 October 2026:** [File-backed source URI correction](../k02-relative-schema-uri/README.md) fixes the four remaining guarded regressions. All 324 eligible original cases now pass in guarded diagnostics; 17 declined-boundary cases remain unchanged. All 21,842 solution tests and both required gates pass: QT3 31,144/0/677 (all 677 skip names/reasons unchanged), qt4 8,436/0/1,894. Guarded loading stays default-off; admission/baseline review is separate. Candidate 1.26.8 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; this slice adds no public API or backend. REQ-126 remains Complete. Earlier failure counts are historical evidence.



**K02-C01 constructor registration — 27 October 2026:** [First-evaluation correction](../k02-constructor-registration/README.md) fixes all 68 constructor regressions in the wider audit. All 11 focused tests and 21,836 solution tests pass; QT3 remains 31,144/0/677 and the qt4 gate 8,436/0/1,894. The 324-case paired sweep leaves four relative-import failures for K02-C02, with no new failures; 17 declined boundaries are unchanged. Guarded loading stays default-off. Candidate 1.26.7 is unpublished/public 1.26.3. Bosak.Schema REQ-009 owns XSD 1.1; no new public schema seam or capability is introduced. REQ-126 remains Complete. Earlier audit counts are historical evidence.



**K02 wider audit — 27 October 2026:** [Schema/import inventory](../k02-schema-inventory/README.md) blocks normal adoption: 324 eligible original cases yield 251 retained passes, 72 regressions and one environment-exception-to-pass transition. All 1,584 metadata/paired/boundary processes complete without timeouts. Two corrections remain open: schema constructor registration after import (68 observed regressions), then relative schema-import base URI (four). Guarded loading stays default-off; no source, skip, baseline or version change. Next: K02-C01 constructor registration. Candidate 1.26.6 unpublished/public 1.26.3; REQ-126 remains Complete. Earlier narrow-sample results remain historical evidence.



**27 October 2026 — source candidate 1.26.6, unpublished; public 1.26.3 unchanged.**

The actual QT3 harness now has an internal diagnostic mode that delegates eligible environment schemas to the query's declared imports. The mode is **off by default**; the normal runner, known skips and baselines are unchanged. `validateexpr-28` passes in the guarded diagnostic through its original XQST0012 alternative. It remains skipped in the normal gate.

The implemented decision uses parsed main-module metadata and an internal compiled-import accessor, not private engine-field reflection. `Bosak.XQuery` grants the conformance assembly internal access; no public API was added. Schema-version declarations are retained in `SourceSchema`. The diagnostic requires a main-module XQuery with imports, exactly one existing environment file per associated/imported namespace, and no explicit XSD-version declaration, validated source, module dependency, parameter expression, inline context-item expression or collection query. Uncertainty falls back to current eager loading. Parsed module imports are checked before compilation can load module files. The executor clears its diagnostic state on reuse.

The [implementation pin](evidence/IMPLEMENTATION-PIN.json) records the fixed XQuery 3.1 Recommendation before source changes. The initial [design pin](evidence/DESIGN-PIN.json), first trial and [second revision](evidence/DESIGN-REVISION-2.json) remain preserved. The first prototype shifted six substitution errors into the runner's generic skip path; the tightened design preserves their original preparation failures. Two unsafe unconditional-deferral controls turn existing environment failures into passes. That unsafe option exists only in the isolated prototype and is not implemented in the real harness.

Verification covers 10 original cases, 21 synthetic prototype controls and two further module controls. There are 64 final prototype processes and 66 actual-harness processes under 20-second external watchdogs, plus 56 retained first-trial and 10 unchanged-harness baseline processes. Actual and prototype results match exactly for all shared controls. Two additional executor calls prove state reset after reuse. Deliberately wrong expected values and wrong error codes still fail comparison. These are diagnostic observations, not 196 additional conformance passes.

| Boundary | Guarded diagnostic result |
|---|---|
| Original validateexpr-28 | Passes its declared XQST0012 alternative |
| Three neighboring validated-source cases | Original passes preserved through eager validation |
| Six explicit-XSD 1.1 substitution cases | Original environment exceptions preserved; no pass claimed |
| Valid and invalid imported-schema controls | Original assertions evaluated by the actual ResultComparer |
| Wrong result and wrong error assertions | Both still fail |
| Missing imports, partial/duplicate mappings, missing files, profiles and environment expressions | Eager behavior retained |
| Module metadata and actual prolog module imports | Eager behavior retained; no module loading for the decision |
| Module keywords inside a string | Correctly treated as text, not an import |
| Executor reused without an environment | Deferred flag cleared; NoEnvironment decision |

Fresh validation: all 21,825 solution tests pass. Both normal gates remain **QT3 31,144/0/677** and **qt4 8,436/0/1,894**, including 11 existing Experimental passes. The final QT3 skipped-name/reason inventory matches all 677 entries of the approved baseline. The source revision remains the candidate's base plus the source hashes in [the manifest](evidence/manifest.json); raw builds, trial results, failures and assertions are retained.

This slice does not enable the mode for ordinary runs, remove a skip, change a baseline or add XSD 1.1 duration/substitution semantics. K02 remains open for the profile/capability boundary and wider applicability review. Recommended next: inventory and compare every structurally eligible schema/import case before proposing any normal-runner adoption. The current inaccurate broad XSD-profile assumption in the legacy dependency filter is recorded as a separate profile-policy issue; this mode does not silently rewrite it.

Validate: `python docs/conformance/k02-schema-loader/evidence/validate-evidence.py`. Reproducible diagnostic driver/project snapshots and synthetic inputs are retained in evidence; the private-reflection prototypes are historical experiment artifacts only. No tag, push, release, upstream or Braid change occurs. HTML rendering uses unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
