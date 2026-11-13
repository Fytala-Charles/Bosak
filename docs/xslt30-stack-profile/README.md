# Sampled managed-stack investigation - 13 November 2026

This independent diagnostic profiles the 54 original `sx-GeneralComp-gt` cases using the exact copied caller and nine engine assemblies measured in PR486, with target .NET10.0.11 and its retained native/file pins. The [baseline](BASELINE.json) and [sealed reference](REFERENCE.zip) identify every selected case/result and the caller binaries. The other14,547 catalog cases are outside this diagnostic selection, not new skips. No full-catalog qualification is claimed.

The [API review](API-REVIEW.json) predates implementation. Profiler10.0.745401 is bound to official NuGet SHA-512 metadata and dotnet/diagnostics commit `cef304c50763bf24f99566cb31d55540842e7ae9`; documentation revision `94b7dca39d8dca4374f326176874381661f4dd59` records sampling semantics. [Profiler file closure](PROFILER-FILES.json) pins all53 package files used by the collector. The profiler is installed only inside the workspace; no global tool/runtime/security settings are changed.

`dotnet-sampled-thread-time` estimates sampled thread wall-time and includes threads regardless of CPU usage. Stack percentages are not on-CPU percentages. The pinned tool help and later documentation describe different nominal sampling rates; neither rate nor statistical accuracy is assumed. Whole-process CPU counters remain separate. Resolved RunTestCase ancestry selects case stacks for ranking, removing unrelated waiting-thread stacks from that denominator. Inclusive method shares overlap and deduplicate recursive frames; do not sum them.

The collector runs under exact .NET8.0.28/Disable; the workload under exact .NET10.0.11/Disable. The 180-second watchdog and8GiB aggregate owned job cover collector and target, with a256MiB trace-size ceiling. These are diagnostic limits; production's1200-second/8GiB qualification limits and required workflow are unchanged. Source/runtime/corpus integrity, target-native identities, complete selected outcomes and caller footer are checked independently of collector exit code. Analysis commands are separately bounded. Raw failures are retained without automatic retry; no intentional unhandled negative.NET cases are launched.

## Run

Use the exact retained caller packet, complete pinned corpus, and the verified NuGet package installed into a private tool directory:

```powershell
python -m unittest discover -s tools -p test_xslt30_stack_profile.py
python tools/xslt30_stack_profile.py prepare --retained D:/retained/case-cost --corpus D:/pinned/corpus --profiler D:/private-tool/dotnet-trace.dll --output D:/fresh/profile
python tools/xslt30_stack_profile.py run --packet D:/fresh/profile
```

Preparation refuses existing outputs, changed binaries/corpus/profiler files or native pins. No engine or production caller is rebuilt or edited. The actual collection uses explicit `dotnet-common,dotnet-sampled-thread-time`, a64MiB buffer, child startup tracing and natural completion; timeout/trace-size refusal never becomes successful capture. Trace, tool reports, converted stacks, all ledgers, signatures and before/after inventories belong to a fresh attempt.

Fifteen Python controls pass. One real selected-set collection and its analysis are pending at this source checkpoint. PR486 final required43463788504 timed out1200.250s at square-array-141; original artifact/digest/logs are retained without retry or merge. Earlier failed PR484/485/486 attempts remain historical failures. No case admissions, exclusions, production budgets, rulesets, engine version, publication or optimization changes are part of this step. Core/server1.81.8 remains unpublished.
