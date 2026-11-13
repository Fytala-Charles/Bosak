# Copied-runner case-cost diagnostic - 13 November 2026

This independent diagnostic branch is based on main `04eea731`, not blocked PR485. The [retained qualification](QUALIFIED-LOCAL.json) binds its exact 32-file application payload and nine engine assemblies; [baseline](BASELINE.json) pins the original archive digest. Production source, runner, required gate/workflow, skip reasons, budgets and versions stay unchanged. Core/server1.81.8 remains unpublished. PR485 final required43453704094 and earlier delivery43452912032 timed out; their originals are retained without retry or merge. PR484 also remains blocked.

The copied runner wraps only the existing RunTestCase call and adds a guarded observer initialization. Removing the additions exactly restores the original source, including its copyright/SPDX notices and line endings. Generated caller/helper sources are diagnostic-only. The bounded controller differs only in its command: exact dotnet path, `--fx-version 10.0.11 --roll-forward Disable`. The 1200-second/8GiB watchdog, owned job and process-scoped error mode remain the original implementation. No global runtime, environment, WER or power settings are changed.

The [API review](API-REVIEW.json) was recorded before implementation against immutable dotnet/dotnet-api-docs revision `a010100852b71f33d67d913d7a9ed69f83a639b1`. It covers Stopwatch elapsed counters and whole-process user+kernel CPU counters. CPU includes background threads, does not establish VM entitlement, and is not exclusive case CPU. Short-case CPU deltas can be zero because counter units do not guarantee effective resolution. Counter probes and ledger writes perturb the copied runner; total overhead is unmeasured. This is neither a production qualification nor a controlled benchmark or timeout diagnosis.

## Run

Use a full repository checkout containing build-source04eea731, Windows x64, installed .NET10.0.11 and the exact retained native identities. Prepare fresh output outside the original runtime and corpus, then run bounded contained controls and one diagnostic sweep:

```powershell
python -m unittest discover -s tools -p test_xslt30_case_cost.py
python tools/xslt30_case_cost.py prepare --qualified D:/retained/gate --corpus D:/pinned/corpus --native D:/retained/NATIVE-MODULES.json --output D:/fresh/diagnostic
python tools/xslt30_case_cost.py controls --packet D:/fresh/diagnostic
python tools/xslt30_case_cost.py run --packet D:/fresh/diagnostic
```

Preparation refuses an existing output, altered corpus, arbitrary self-declared qualified binaries, source-anchor drift or changed native runtime. Every installed framework file is hashed before/after execution; loaded native module paths/hashes and Environment.Version are checked inside the copied caller. The generated helper catches pin refusals. Controls use a harmless computation, a fast skipped case and a wrong-version refusal caught inside the diagnostic; no unhandled .NET negative case is launched.

The complete catalog's outcomes and original exclusions are reconciled independently. Cases excluded before the case loop have no invented zero timing: `unmeasured-skips.csv` preserves each identity and original reason. Every executable case must have exactly one timing row and unchanged result. The case interval includes its dependency, environment, compilation, evaluation and assertion work; it does not distinguish those phases. Partial/failed sweeps retain raw data and failed receipts without becoming successful qualification.

Fifteen Python controls and three bounded live helper controls pass. The copied caller builds successfully and completes the full diagnostic in 850.084 seconds. The unchanged original outcomes are 10,221 passed, zero failed and 4,380 skipped. All 14,601 identities and original exclusion reasons reconcile. The exact nine engine assemblies, all application files and the full 189-file native framework inventory remain unchanged after execution. Original hosted CI at source99c7e288 separately passed 26,422 solution and 90 language-server tests. The final delivery adds these fifteen fast Python controls to normal CI; the required standalone workflow remains unchanged. No hosted copied-runner diagnostic was requested.

## Observed coverage and costs

[Case report](REPORT.json), [aggregate findings](FINDINGS.json), [original evidence](EVIDENCE.zip), [seal](MANIFEST.json) and [renderer receipt](DOCUMENTATION-CHECKS.json). Offline replay: `python docs/xslt30-case-cost/verify.py`. Diagnostic source checkpoint `99c7e288acaabfd588dc0477acff1a0f5873df01`; later changes synchronize evidence/docs and wire the existing Python controls into CI. Runtime and engine observations come from this one copied-runner execution, not a new adopted gate.

| Original outcome | Cases | Timing treatment |
| --- | ---: | --- |
| Passed | 10,221 | One measured interval each |
| Skipped inside case evaluation | 1,003 | One measured interval each, including fast/zero-CPU deltas |
| Skipped before case loop | 3,377 | Unmeasured; original identities/reasons in `unmeasured-skips.csv` |
| Total | 14,601 | 11,224 measured intervals and 3,377 explained exclusions |

| Largest individual interval | Wall seconds | Whole-process CPU seconds |
| --- | ---: | ---: |
| catalog/catalog-007 | 54.219 | 50.609 |
| sf-deep-equal/sf-deep-equal-018 | 25.328 | 24.984 |
| sf-fold-left/sf-fold-left-021 | 20.306 | 18.109 |
| catalog/catalog-006 | 18.695 | 17.703 |
| catalog/catalog-004 | 16.672 | 2.562 |

Measured case wall intervals sum to 848.569 seconds and CPU deltas to 793.156 seconds. The remaining 1.515 seconds of controller wall time includes startup, test-set work, observer work and other gaps; it is not an overhead estimate. Case scopes also include the original caller's console/file I/O. Background-thread CPU and counter resolution can make CPU/wall ratios misleading for individual short cases.

The six `sx-GeneralComp-*` sets contain 325 timed cases, totalling 186.826 wall seconds and 183.672 process CPU seconds: about 22.0% of measured case wall time. Catalog self-tests total 120.081 wall seconds; fold-left totals 75.559. The largest individual case, catalog-007, checks stylesheet element/attribute coverage across the catalog. Timings alone do not separate its loading, compilation, evaluation, assertion or scheduling costs.

A useful next step is a bounded CPU-stack profile of `sx-GeneralComp-gt` in this separately identified copied-runner setup with the same engine/native pins, before proposing any engine optimization. The combined comparison-family cost supports selecting that workload; it does not establish the cause of hosted timeouts. Native/runtime/host differences and uncontrolled state prevent a controlled comparison with earlier observations. The 850.084-second diagnostic versus the retained 817.285-second original qualification is not an observer-overhead measurement.

## Retained required-gate failures

PR485 remains OPEN/BLOCKED and was not merged. Earlier delivery run43452912032 timed out at1200.063 seconds (last marker sx-treat-015); final-head run43453704094 timed out at1200.235 seconds (last marker date-020). Both original artifacts, digests and logs are retained here separately from successful measurements. No retry was launched. PR484 also remains blocked by its original timeout. No budgets, case selection, skip policies, gate requirements, production native runtime or package versions changed.

PR486 original required run43459327822 also timed out at1200.063 seconds, last marker sx-instance-of-007. Its original artifact/digest/logs are retained separately from the successful diagnostic; no retry or merge. Final delivery normal checks are independent new-head checks and remain pending until reported. The copied-runner observation does not resolve this hosted qualification failure.
