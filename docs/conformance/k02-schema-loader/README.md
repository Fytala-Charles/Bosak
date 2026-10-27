# K02: guarded query-owned schema diagnostics

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
