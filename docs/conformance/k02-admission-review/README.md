# K02 guarded-runner admission review — 27 October 2026

**K02 adopted — 27 October 2026:** [Owner-approved normal-runner adoption](BASELINE-ADOPTION.md) is complete. The fresh normal QT3 gate passes **31,145/0/676**; only validateexpr-28 leaves the skip set through its original XQST0012 error alternative, and all 676 retained names/reasons match. AGENTS now requires this exact baseline. The normal runner selects the existing guard; standalone TestExecutor defaults and all profile/structural exclusions remain unchanged. Engine/candidate1.26.8 unchanged and unpublished; public1.26.3. Solution21,842 and qt4 8,436/0/1,894 are inherited source-identical validation. Bosak.Schema owns XSD1.1. REQ-126 remains Complete; earlier pending/default-off notices are historical checkpoints.



**Review ready; owner approval pending. Normal QT3 remains 31,144/0/677 and guarded loading remains default-off.**

The [concrete adoption proposal](BASELINE-UPDATE-PROPOSAL.md) would enable the existing guarded loader on the normal runner and adopt **31,145/0/676**. A complete original-catalog rehearsal verifies exactly one gain: `validateexpr-28` reaches its existing XQST0012 error alternative. All other 676 skip names and reasons are unchanged, with zero failures or timeouts. There is no known-skip entry to remove.

The two full actual-runner processes preserve the entry point's stack and environment configuration. Only the guarded process uses reflection into the harness to select the existing switch; engine fields, source and binaries remain untouched. The normal runner remains unchanged on disk. Earlier K02-C01/C02 corrections supply 324/324 guarded assertion passes and unchanged declined-profile boundaries. Those bounded diagnostic results do not certify wider XSD capability.

[RESULTS.json](RESULTS.json) deterministically consolidates full-run outcomes and the exact skip delta. [PROPOSED-ADOPTION.patch](PROPOSED-ADOPTION.patch) is a review-only patch, checked against the unchanged Git index without application. [Evidence manifest](evidence/manifest.json) pins both full receipts, original fixtures, binaries, current source identities and prior raw evidence.

```powershell
python docs/conformance/k02-admission-review/build-review.py
python docs/conformance/k02-admission-review/evidence/validate-evidence.py
git apply --cached --check docs/conformance/k02-admission-review/PROPOSED-ADOPTION.patch
```

AGENTS.md requires the current exact baseline, so explicit owner approval must precede normal adoption. After approval, build and run the real normal harness and require the proposed exact totals and one-case skip delta. This review changes no engine, harness, test, fixture, AGENTS policy, package version or release state.

Source base `55905ddc`; candidate **1.26.8 unpublished**, public **1.26.3**. Inherited source-identical validation: 21,842 solution passes and qt4 **8,436/0/1,894**, 224 gated sets and 11 existing Experimental passes. No new unit/qt4 run is claimed. **Bosak.Schema REQ-009 owns XSD 1.1**, including separately governed future schema seams; no competing implementation or capability claim is introduced. REQ-126 remains Complete for its accepted bounded catalog. No release, tag, push, upstream or Braid action occurs.
