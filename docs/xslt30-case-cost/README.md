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

Fifteen Python controls pass. Build, contained helper controls and a real full diagnostic are pending at this source checkpoint. No hosted diagnostic is automatically requested. A separate normal PR check run, if created, remains independent of these observations.
