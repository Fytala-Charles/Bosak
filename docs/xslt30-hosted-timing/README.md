# Hosted XSLT gate timing investigation — 13 November 2026

Two retained comparisons show a successful run and a timeout on the **same checkout commit with identical SHA-256 manifests for all 32 application-payload files**, including nine engine assemblies. A semantic code change is therefore not necessary for a timeout to occur. This establishes execution-time variation in the retained sample; it does **not establish its cause**.

This is a selected historical sample, not a randomized experiment or a failure-rate estimate. Three incomplete attempts remain failed. No diagnostic or benchmark execution was launched to produce this analysis; ordinary post-merge/PR CI is separate. Engine, runner, workflow, versions, limits, exclusions and assertions are unchanged.

## Retained attempts

All seven use the same immutable `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`, 234-set/14,601-case inventory, Legacy/Xslt30 basic/non-schema-aware profile and 1,200-second/8 GiB limits. The four completed artifacts independently replay **10,221 passed / 0 failed / 4,380 excluded**. Corpus and runtime-integrity receipts match in all seven. No FAIL lines are observed in the incomplete transcripts; this does not establish that their unexecuted work would pass.

| Attempt | Observed elapsed seconds | Outcome | Last recorded case on timeout | Reported region |
|---|---:|---|---|---|
| [df16 original main](https://github.com/Fytala-Charles/Bosak/actions/runs/43300842122/attempts/1) | 1,200.250 | Timed out; incomplete, not a pass | date-020 | centralus |
| [df16 fresh same-commit run](https://github.com/Fytala-Charles/Bosak/actions/runs/43305315323/attempts/1) | 1,079.563 | Completed: 10,221 / 0 / 4,380 | — | westus3 |
| [PR481 attempt 1](https://github.com/Fytala-Charles/Bosak/actions/runs/43374636539/attempts/1) | 1,200.218 | Timed out; incomplete, not a pass | sx-treat-103 | centralus |
| [PR481 attempt 2](https://github.com/Fytala-Charles/Bosak/actions/runs/43374636539/attempts/2) | 1,133.657 | Completed: 10,221 / 0 / 4,380 | — | eastus2 |
| [PR482](https://github.com/Fytala-Charles/Bosak/actions/runs/43408032043/attempts/1) | 632.735 | Completed: 10,221 / 0 / 4,380 | — | centralus |
| [PR482 post-merge main](https://github.com/Fytala-Charles/Bosak/actions/runs/43416044519/attempts/1) | 1,200.219 | Timed out; incomplete, not a pass | sx-instance-of-101 | westcentralus |
| [PR483](https://github.com/Fytala-Charles/Bosak/actions/runs/43422693039/attempts/1) | 1,086.360 | Completed: 10,221 / 0 / 4,380 | — | westus3 |

Timeout elapsed values are **right-censored observations**: the controller stopped the process, so its eventual completion time is unknown. They are not complete durations and must not be included as successful 1,200-second benchmark results. `last_test.txt` names the last recorded case; it is not a stack trace or proof that the named test hung.

## Controlled comparisons and limits

| Comparison | Source/payload relationship | What can be concluded |
|---|---|---|
| df16 original timeout vs fresh success | Same checkout `df16de7f`; identical 32-file application manifest and nine-engine-assembly manifest | One payload completed in 1,079.563s; another attempt exceeded the 1,200s cap. |
| PR481 attempts 1 vs 2 | Same checkout `dc477572`; identical 32-file application manifest and nine-engine-assembly manifest | One payload completed in 1,133.657s; another attempt exceeded the cap. |
| PR482 vs its post-merge main | Same source tree `6331a223`; different checkout commits and application manifests | Context only. This is not an exact-binary A/B comparison and cannot attribute a timing difference to source changes. |

“Application payload” means the 32 files listed by the retained manifest. It does **not include the external dotnet host, shared runtime/JIT, OS, CPU allocation, process state or I/O/cache conditions**. The logged image revision is `windows-2025-vs2026` / `20261103.298.1` throughout, but regions vary. Central US appears in both a timeout and the fastest retained success; region alone is not an established explanation.

## Sampled progress

These are the first **observed** `sf-*`, `si-*` and `sx-*` case prefixes in approximately 20-second watchdog samples, not exact phase starts or individual test durations. No missing sample is interpreted as zero time. Full case names and samples are retained in [the report](REPORT.json).

| Attempt | First sampled sf-* (s) | First sampled si-* (s) | First sampled sx-* (s) |
|---|---:|---:|---:|
| df16 original main | 280 | 681 | 861 |
| df16 fresh same-commit run | 240 | 581 | 761 |
| PR481 attempt 1 | 280 | 681 | 861 |
| PR481 attempt 2 | 260 | 621 | 801 |
| PR482 | 140 | 381 | 481 |
| PR482 post-merge main | 280 | 681 | 881 |
| PR483 | 240 | 601 | 781 |

The two identical-payload timeout comparisons show slower sampled progress before their terminal recorded cases. That is consistent with accumulated slower execution rather than evidence for one universal final-test hang; it is not a causal CPU, GC or I/O diagnosis. The three timeouts end at different recorded cases. The original date-020 fixture deliberately loops one million times to test stable current-dateTime, but these data do not justify blaming, shortening or excluding that case.

## Proposed next diagnostic step

Keep the production gate unchanged. Prepare a separate, explicitly labelled diagnostic lane before another semantic slice:

1. Capture host `dotnet --info`, OS/image, CPU count/affinity and job allocation, then sample the unchanged child's elapsed/CPU time, working/committed memory, process I/O and last-test marker. Bind the exact 32-file payload and immutable full corpus. Keep the same bounds and retain every attempt, including incomplete ones.
2. If case-level attribution is still needed, use a diagnostic-only runner copy to record per-case wall/CPU and compile/execute boundaries. Retain unchanged nine-engine DLL hashes and the observer diff; prove case-status and exclusion parity. Declare the different runner binary and instrumentation overhead rather than treating it as an untouched performance measurement.
3. Use those measurements to separate host/runtime/I/O effects from expensive engine paths before proposing optimization or a reviewed budget change. No automatic retry, timeout increase, shortened fixture, exclusion or gate admission is proposed here.

No new lane or execution is implemented by this PR. Existing artifacts do not contain per-case durations, process CPU, GC/heap or I/O measurements, so root cause remains open. SDK10.0.301 is the recorded qualification pin; the exact external runtime/JIT binary is not hash-bound by the application manifest.

## Provenance and replay

[SOURCES.json](SOURCES.json) identifies every original artifact by digest and run/attempt. Existing sealed repository archives are referenced without rewriting or duplicating them; three previously local original artifacts are retained under `artifacts/`. [EVIDENCE.zip](EVIDENCE.zip) contains the raw workflow logs, attempt metadata, Git commit/tree identities and original receipts. [MANIFEST.json](MANIFEST.json) seals every new file and evidence entry. The source tree must match the corresponding workflow head before a record is compared.

Run `python docs/xslt30-hosted-timing/verify.py` for offline replay of the seven original artifacts, all four complete case ledgers and exclusion inventories, exact-payload vs equivalent-tree comparisons, sampled progress/platform fields and thirteen synthetic refusal/comparison controls. [Renderer receipt](DOCUMENTATION-CHECKS.json). No .NET qualification is rerun for this documentation-only analysis.

PR #483 was merged at `04eea7317ab2f71f7f3644b1ebcc30076b9dbdaf` after its six checks and independent hosted replay passed. Its successful receipt does not replace any earlier timeout. Core/server1.81.8 remains unpublished; published Core1.81.2 and extension0.2.6 stay unchanged. No complete-conformance or performance-acceptance claim follows.
