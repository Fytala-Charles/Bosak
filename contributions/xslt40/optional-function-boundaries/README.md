# Optional function argument boundaries

**Submitted 2026-10-14:** [Upstream PR #31](https://github.com/qt4cg/xslt40-test/pull/31) adds three XSLT 4.0 cases. Acceptance and independent processor confirmation are pending. Bosak source candidate 1.3.0 is unpublished; published libraries remain 1.2.1 and Marketplace extension 0.1.6.

## Coverage and specification basis

Upstream already covers ordinary defaults, context-dependent static calls, named references, lookup and partial application (`function-40-011` through `013`, `022`/`023`, and `higher-order-functions-081`). This batch adds the following combinations without changing an existing fixture or assertion.

| Case | Boundary | Expected result |
|---|---|---|
| function-40-024 | Reordered keywords omit the middle optional argument; explicit `()` supplies it; an arrow call also omits it | <code>omitted="A&#124;B&#124;Y"</code>, <code>empty="A&#124;&#124;Y"</code>, <code>arrow="A&#124;B&#124;Y"</code> |
| function-40-025 | Select and sequence-constructor defaults resolve a global shadowed by an earlier parameter and a caller local | <code>defaults="SUPPLIED&#124;GLOBAL&#124;GLOBAL"</code>, <code>supplied="CALLER&#124;B&#124;C"</code> |
| function-40-026 | Named reference, lookup and partial application capture creation item/position/size for both default forms; invocation uses another focus | Each form returns <code>X:1/2&#124;X:1/2</code>, then <code>Y:2/2&#124;Y:2/2</code>; invocation focus is `END:3/3` |

The [XSLT 4.0 function parameter rules](https://qt4cg.org/specifications/xslt-40/Overview.html#function-arguments), sections 10.3.1–10.3.3, define optional defaults and their evaluation context. Defaults use global variable scope and the focus of the static call or function-item creation operation. The expected outputs are derived from those rules. `SPECIFICATION.json` retains the implementation step's fetched-draft SHA-256 pin; the current 13 October 2026 editor's draft was checked again for this contribution.

## Verification

- Each submitted case: **1 pass / 0 fail / 401 unselected or unsupported skips** in the native catalog harness. Together: **3 pass / 0 fail**. Catalog schema validation passes.
- Negative controls: change the middle default in 024, the global value in 025, or the creation item in 026. Each unchanged result assertion reports a result mismatch. Original fixtures were restored byte-for-byte before final verification and submission.
- Pre-feature replay: **0 pass / 3 fail**, all `XTSE0020`, using retained 1.2.1 Bosak assemblies and a compatible retained conformance harness. Assembly hashes and logs are preserved. This is a retained-binary check, not a fresh source checkout baseline. An initial attempt to mix the current harness with the old assembly failed assembly loading and is not used as evidence.
- Broader `function-40-` sample: **29 pass / 10 fail / 363 skips**. The same sample using the unmodified upstream catalog has **26 pass / 10 fail / 363 skips**. Failure identities and diagnostics are identical: eight no-namespace cases (`014`–`021`) remain unsupported; `012` returns an empty sequence, and `904` expects `XPTY0004` where Bosak reports `XTTE0600`. These existing observations are separate follow-up candidates, not corrected in this PR.
- Existing `higher-order-functions-081`: **1 pass / 0 fail / 84 skips**.

Engine source, public APIs and gate membership are unchanged by this contribution. The merged optional-function implementation at `b6951cfa960628bf6ace6b5b7b765b57880a6bb0` passed [CI](https://github.com/Fytala-Charles/Bosak/actions/runs/38549161397): solution **8,219/0/0**, separate language-server **77/0/0**. Its QT3 **31,142/0/679** and qt4 gate-only **8,278/0/1,894** receipts remain authoritative. See [implementation evidence](../../../docs/XSLT40-OPTIONAL-FUNCTION-ARGUMENTS.md).

## Current upstream base and reproduction

Upstream `master` was fetched before creating the contribution branch and again immediately before submission. It remained `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Branch `bosak/optional-function-boundaries` at `f78438899ec32d502501019fa0b7c213aac6d51e` adds three stylesheets and 23 catalog lines: four files, 86 additions, zero deletions. The final diff passed whitespace checks against the refreshed base. `UPSTREAM_SUBMISSION.json` records the submitted PR's exact base/head.

Apply the portable patch to that upstream revision, or use the submitted branch, then run from Bosak:

```powershell
powershell -ExecutionPolicy Bypass -File ./validate-xslt40-catalog.ps1 -Files tests/decl/function/_function-test-set.xml
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet function -TestCase function-40-024 -NoBuild
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet function -TestCase function-40-025 -NoBuild
powershell -ExecutionPolicy Bypass -File ./run-xslt40-tests.ps1 -TestSet function -TestCase function-40-026 -NoBuild
```

Build the conformance project in Release first if its binaries are absent. The `function` set filter also selects other function-related sets, which explains the unselected skip counts. Saved receipts retain original paths and checkout metadata; local wrapper receipts are separate from acceptance or independent processor confirmation. Existing contribution receipts and PRs #29/#30 remain separate.
