# Sampled managed-stack investigation - 13 November 2026

This independent diagnostic profiles the 54 original `sx-GeneralComp-gt` cases using the exact copied caller and nine engine assemblies measured in PR486, with target .NET10.0.11 and its retained native/file pins. The [baseline](BASELINE.json) and [sealed reference](REFERENCE.zip) identify every selected case/result and the caller binaries. The other14,547 catalog cases are outside this diagnostic selection, not new skips. No full-catalog qualification is claimed.

The [API review](API-REVIEW.json) predates implementation. Profiler10.0.745401 is bound to official NuGet SHA-512 metadata and dotnet/diagnostics commit `cef304c50763bf24f99566cb31d55540842e7ae9`; documentation revision `94b7dca39d8dca4374f326176874381661f4dd59` records sampling semantics. [Profiler file closure](PROFILER-FILES.json) pins all53 package files used by the collector. The profiler is installed only inside the workspace; no global tool/runtime/security settings are changed.

`dotnet-sampled-thread-time` estimates sampled thread wall-time and includes threads regardless of CPU usage. Stack percentages are not on-CPU percentages. The pinned tool help and later documentation describe different nominal sampling rates; neither rate nor statistical accuracy is assumed. Whole-process CPU counters remain separate. Caller ancestry is retained as a separate view: RunWithStack starts a dedicated transformation thread and the caller waits in Thread.Join. The corrected ranking follows engine-associated stacks across threads, retains the converted CPU_TIME category separately from UNMANAGED_CODE_TIME, and strips these virtual markers before identifying resolved managed leaves. The category name CPU_TIME is not an exact CPU measurement. Inclusive method shares overlap and deduplicate recursive frames; do not sum them.

The collector runs under exact .NET8.0.28/Disable; the workload under exact .NET10.0.11/Disable. The 180-second watchdog and8GiB aggregate owned job cover collector and target, with a256MiB trace-size ceiling. These are diagnostic limits; production's1200-second/8GiB qualification limits and required workflow are unchanged. Source/runtime/corpus integrity, target-native identities, complete selected outcomes and caller footer are checked independently of collector exit code. Analysis commands are separately bounded. Raw failures are retained without automatic retry; no intentional unhandled negative.NET cases are launched.

## Run

Use the exact retained caller packet, complete pinned corpus, and the verified NuGet package installed into a private tool directory:

```powershell
python -m unittest discover -s tools -p test_xslt30_stack_profile.py
python tools/xslt30_stack_profile.py prepare --retained D:/retained/case-cost --corpus D:/pinned/corpus --profiler D:/private-tool/dotnet-trace.dll --output D:/fresh/profile
python tools/xslt30_stack_profile.py run --packet D:/fresh/profile
```

Preparation refuses existing outputs, changed binaries/corpus/profiler files or native pins. No engine or production caller is rebuilt or edited. The actual collection uses explicit `dotnet-common,dotnet-sampled-thread-time`, a64MiB buffer, child startup tracing and natural completion; timeout/trace-size refusal never becomes successful capture. Trace, tool reports, converted stacks, all ledgers, signatures and before/after inventories belong to a fresh attempt.

Sixteen Python controls pass, including an execution-thread fixture without RunTestCase ancestry. One real collection completed successfully; the same retained trace was reanalyzed after correcting the caller-only assumption. The initial analysis report is preserved and no second workload was launched. PR486 final required43463788504 timed out1200.250s at square-array-141; original artifact/digest/logs are retained without retry or merge. Earlier failed PR484/485/486 attempts remain historical failures. No case admissions, exclusions, production budgets, rulesets, engine version, publication or optimization changes are part of this step. Core/server1.81.8 remains unpublished.

## Measured selection and interpretation

[Report](REPORT.json), [selected exclusions](SELECTED-SKIPS.json), [analysis review](ANALYSIS-REVIEW.json), [evidence](EVIDENCE.zip), [seal](MANIFEST.json) and [renderer receipt](DOCUMENTATION-CHECKS.json). Offline replay: `python docs/xslt30-stack-profile/verify.py`.

| Observation | Result |
| --- | --- |
| Selected cases | 54: 52 passed, zero failed, 2 original exclusions |
| Outside this selection | 14,547 unselected; no new skips or full-catalog result |
| Collector wall time | 21.281 seconds |
| Sum of selected case wall intervals | 20.841 seconds |
| Sum of process CPU deltas over case windows | 22.547 seconds; all target threads |
| Retained trace | 5,917,862 bytes; SHA-25626e7d71bf665a9b7c822d16278513200ac54decb8b17e4daebd1354f3d647b44 |
| Converted trace | 19 thread profiles, 785 frames |

Both original exclusions, sx-gc-gt-100 and sx-gc-gt-101, require schema_aware and remain outside the basic profile. Their original full reasons are retained. Native target identities, all application/corpus/framework inventories and all53 profiler package files match after capture/analysis. Collector, conversion and both original top-method reports completed under their separate bounds; no event-loss warning appears in their retained logs. This does not establish statistical accuracy or eliminate sampling bias.

The original caller-only view was mostly the waiting Thread.Join path. About97.57% of the corrected engine-category weight lacks a RunTestCase ancestor, explaining why that initial selection missed execution workers. The initial report and its source are preserved. No capture was repeated to correct this analysis.

The following shares use **converted CPU_TIME-category engine stack weight**, after removing virtual leaf markers. They are sampled thread-time shares, not on-CPU shares:

| Resolved leaf / associated context | Share or weight |
| --- | ---: |
| Monitor.Enter_Slowpath, engine-associated resolved leaf | 44.26% |
| Thread.PollGCWorker, engine-associated resolved leaf | 38.00% |
| RuntimeHelpers.GetHashCodeWorker, engine-associated resolved leaf | 5.35% |
| ConditionalWeakTable.Container.Resize, engine-associated resolved leaf | 4.99% |
| Nearest engine context GetElementTreeSequence in CWT/Monitor stacks | 3,806.666 ms of converted category weight |
| Nearest engine context StreamingSource.Pump in CWT/Monitor stacks | 3,256.649 ms of converted category weight |

The separate all-thread managed-category view also contains GC.RunFinalizers and ConditionalWeakTable container finalization. Shares from different views have different denominators; inclusive shares overlap. The raw tool reports retain their original all-thread denominators. This identifies weak-table/node-registration, locking and GC interaction as an investigation lead, without proving which table, allocation pattern or scheduling condition causes the hosted timeouts.

Source review at unchanged main04eea731 confirms GetElementTreeSequence uses ElementTreeSequences and SequenceLock, and RunWithStack creates/joins a dedicated thread. The next useful step is bounded GC/allocation correlation for this same selection and the node-registration/weak-table path before any optimization. Any later change must preserve node identity, creation order, weak lifetimes and existing provenance/state contracts.

This fresh-process subset does not reproduce the preceding cases, heap history, caches or scheduling of the full-catalog sweep. Its21.281-second observation versus the earlier36.652-second set total is not a speedup or profiler-overhead estimate. Target engines/native binaries are fixed; state/history and instrumentation still differ.

The installed TraceEvent3.1.23 DLL informational revision is ea4be383d3534033723c6c32f1b0add59f64f772; public tree lookup returned422. A separately identified version-tag review uses public v3.1.23 commit bc9c1c6811d2ac9a4ad1e97a7fd8de01bea09a26. That review pin is not asserted to be the binary build commit. Exact installed DLL bytes remain bound by the official profiler-package SHA-512 and file closure.
