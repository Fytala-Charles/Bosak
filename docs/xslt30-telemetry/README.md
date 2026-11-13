# Standalone XSLT telemetry - 13 November 2026

This diagnostic lane observes exactly one invocation of the unchanged [production gate](../../tools/xslt30_gate.py). It is separate from the required standalone check and cannot replace it. The [timing study PR](https://github.com/Fytala-Charles/Bosak/pull/484) remains blocked by its original required-check timeout (run 43437283485, 1200.156 seconds). No retry or merge of that PR is authorized by telemetry capture.

## Run

Build the unmodified Release conformance runner in a clean committed checkout. Use a fresh output directory outside the corpus and runtime:

```powershell
python -m unittest discover -s tools -p test_xslt30_telemetry.py
python tools/xslt30_telemetry.py --corpus D:/path/to/pinned-corpus --output D:/path/to/fresh-packet
```

The corpus must match the production [baseline](../xslt30-gate/BASELINE.json). The full Legacy/Xslt30/basic catalog remains 14,601 cases under 1,200 seconds and 8 GiB; the adopted result remains 10,221/0/4,380. This change alters no case, skip reason, runtime assembly, engine, required workflow, ruleset or package version. Core/server 1.81.8 remains unpublished. No new engine or specification semantics are adopted.

The separate workflow runs for PR changes to its own workflow/tool/control files and supports manual dispatch after reaching the default branch. Documentation-only pushes do not trigger another diagnostic attempt. Each invocation launches once; it does not retry. Its 35-minute job ceiling leaves time for preparation, a 1,800-second observer/controller lifetime safeguard and artifact upload. The original child qualification ceiling stays 1,200 seconds. A kill-on-close observer job owns the process tree without an additional CPU or memory limit.

## Receipts and interpretation

| Receipt | Meaning and limits |
| --- | --- |
| `TELEMETRY.json` | Separate `gateSuccess` and `telemetryCaptured`; a timeout always fails qualification even with useful telemetry. Exit 0 requires both, 1 means qualification failed, 2 means capture/preflight incomplete. |
| `samples.jsonl` | Approximately one-second cumulative process CPU, working set/private commit, page faults and I/O counters. Collection gaps/errors remain visible. Cumulative I/O is not latency; page faults are not specifically disk faults; memory is not allocation or GC activity. |
| `CHILD.json` | Child identity, creation time and affinity. Affinity does not establish VM entitlement. |
| `NATIVE-MODULES.json` | SHA-256 identities of observed loaded dotnet/CLR/JIT/host modules. Enumeration races can leave gaps. CoreLib beside CLR is marked as a sibling candidate, not as observed loaded. |
| `HOST.json` and `dotnet-*.log` | Allowlisted metadata, OS/Python identity and installed host/runtime/SDK inventory. NULL-job CPU data applies only to the observer's immediate job, never the child's nested job. |
| `OBSERVER.json` | Observer source hash, sampling interval, unchanged production bounds and zero retries. |
| `APPLICATION-BEFORE.json` | All 32 application files, compared with the gate's copied runtime manifest and source files after execution. |
| `FINAL-TIMES.json` | Final cumulative CPU counters if the process handle remains readable. |
| `gate/` | Original gate receipts, case/skip reconciliation, logs, generated-fixture audit and retained work, including failed attempts. |

Last-case markers are sampled observations, not precise case/phase timings or stack traces. GC pause/allocation counters, CPU scheduling delay and actual VM entitlement are unavailable in this first observer. Sampling and native-file hashing overhead is unmeasured; observed executions are not untouched performance benchmarks. Capture does not establish a root cause or justify a higher budget, rerun, new admission or a conformance percentage.

## API review and validation

The [API review](API-REVIEW.json) was recorded before implementation against MicrosoftDocs/sdk-api revision `656e9656bfaef34dd29cd7ee9e1146aec17e924c`. It is operational evidence, separate from engine specification adoption. The primary API references are [GetProcessTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesstimes), [PROCESS_MEMORY_COUNTERS_EX](https://learn.microsoft.com/en-us/windows/win32/api/psapi/ns-psapi-process_memory_counters_ex), [GetProcessIoCounters](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprocessiocounters), [EnumProcessModulesEx](https://learn.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-enumprocessmodulesex) and [QueryInformationJobObject](https://learn.microsoft.com/en-us/windows/win32/api/jobapi2/nf-jobapi2-queryinformationjobobject).

Seventeen observer controls and fourteen existing gate controls pass locally, including a harmless live Python child. Negative qualification/refusal cases use mocked data; no unhandled negative .NET program is launched. A real complete qualification and hosted telemetry are pending; the earlier engine qualification receipts remain historical and are not presented as fresh runs for this tool.
