# K04-C31 - Module-scoped XQuery namespace copying

28 October 2026. Implemented against `3a5017df`; candidate **1.31.0** is unpublished. Public version remains **1.26.3**. **The semantic slice is qualified; normal routing and baselines are unchanged.**

`declare copy-namespaces` now supports all preserve/no-preserve and inherit/no-inherit combinations. Malformed declarations raise XPST0003, duplicates raise XQST0055, and phase-one ordering is enforced. Immutable static contexts preserve the options across later declarations. Each direct/computed/document constructor carries its declaring module's options in IR, including imported functions/variables, callbacks, inline closures and dynamically loaded functions. No mutable caller option is used.

No-preserve retains bindings required by copied element/attribute names and removes unused source bindings. No-inherit excludes constructor namespaces from copied elements; preserved source bindings override conflicting parent bindings. Original and host-supplied nodes remain unchanged. Namespace absence and inheritance boundaries use provider annotations, consumed by the XDM namespace axis and prefix-inspection functions; QName resolution uses that same axis. Default namespace absence is retained without redundant serialized declarations. Literal child markup retains lexical namespace declarations and is distinguished from enclosed-expression node copying.

The original replay exposed prerequisite lexical-context issues: lazy main-module globals could initialize under a caller constructor's default/prefix namespace frame, and literal child origin was erased in the AST. The [pre-implementation module-scope addendum](evidence/MODULE-SCOPE-ADDENDUM-PIN.json) pins those rules separately. Named declarations now isolate and restore namespace/default/constructor-local frames; IR captures lexical constructor declarations. The base URI, schema and collation policies remain governed by existing module initialization.

## Qualified verification

| Check | Outcome |
|---|---|
| Identical 76 public regressions against preserved 1.30.1 DLLs | **9 passed / 67 failed** |
| Same regressions against candidate 1.31.0 | **76 passed** |
| Full solution | **24,010 passed**, ten projects; XQuery **611** |
| All 49 unchanged copy-only originals, isolated actual executor | **49 passed** |
| Same originals, normal direct executor | **2 passed / 47 skipped**; runner admission stays guarded |
| Complete normal QT3 gate | **31,208 / 0 / 613** |
| Complete isolated copy-only QT3 gate | **31,257 / 0 / 564** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920**, 230 sets, 11 experimental passes |
| Previously admitted C29 original cohort | **43 passed**, unchanged result dictionaries |
| Complete normal/isolated QT3 context sets | Both **44 / 0 / 0** |
| Normal qt4 context set | **0 / 0 / 53**, applicability exclusions retained |

All normal skip names/reasons exactly match C29's approved adoption. The isolated rehearsal removes exactly the 49 copy-only skips and preserves all 564 retained names/reasons. Its copied harness changes only the copy-namespaces refusal and diagnostic history; construction and pragma refusals remain. All **216 final bounded diagnostic processes** finish successfully without timeout. Normal TestExecutor, dependency filters, KnownSkips, GatedSets, AGENTS baseline, fixtures and assertions remain untouched.

The 20 frozen C30 public controls now give **16 passes / four failures**. The four retained failures are construction-preserve/strip, duplicate construction and commented construction declarations; they remain unimplemented. This slice does not implement construction type-annotation modes or their namespace-sensitive typed-value/XQTY0086 interactions. The 75 construction-only and 12 mixed C30 cases remain separately tracked. Bosak.Schema retains XSD 1.1 ownership.

## Evidence and integration

The [immutable adoption pin](evidence/IMPLEMENTATION-PIN.json), preserved C30 evidence, exact original assertions, source/binary identities and [validator](evidence/validate-evidence.py) establish this bounded scope. Earlier failed implementation, baseline-discovery, original-replay and full-gate attempts are retained separately. Initial redundant xmlns declarations caused serialization regressions; a later comparison caught four new skips from namespace undeclarations/reserved xml handling. Final full gates restore the exact approved maps. No historical failure was removed or reclassified as a qualified run. See [baseline disposition](evidence/baseline-disposition.json).

Provider hooks receive additive `XdmContentItem.PreserveNamespaces`, `InheritNamespaces` and `IsLiteralElement` properties without changing the record constructor or deconstruction signatures. Defaults preserve existing preserve/inherit behavior; `IsLiteralElement` identifies lexical child markup. The XDocument provider implements the copying rules. Custom element/document hooks must honor these inputs when copying node content. The implementation does not broaden unrelated non-XDocument cloning or typed-construction capability claims.

REQ-126 remains Complete under bounded owner acceptance; original dispositions and 48 historical unmet observations remain recorded. No release, publication, push, tag, upstream submission or Braid change is included.

## Next slice

[C32 should review exact copy-only routing admission](NEXT-SLICE.md). The semantic engine change keeps both normal refusals. Reconcile the complete catalog and lexical guard boundaries, then present the exact baseline proposal for owner disposition separately.

## Reproduction

Use the pinned source base and fixture revisions. `prepare-review.py` creates the isolated harness/actual-executor driver; build its two generated Release projects and run `run-originals.py`, `run-controls.py`, `run-context.py`, `run-sets.py` and `run-gates.py`. `prepare-baseline.py` checks and copies the preserved old DLL witnesses and the exact qualified regression source into the isolated baseline project. The old engine test run must fail with 67 failures/9 passes; the current focused tests must pass all 76. Complete normal solution/gate commands remain those in AGENTS.md. Regenerating execution receipts requires regenerating their identities; use the preserved manifest for audit.
