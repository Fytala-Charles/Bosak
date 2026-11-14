# GC and sampled-allocation correlation - 13 November 2026

This independent diagnostic follows PR487's stack investigation without editing or rebuilding production engines or callers. It first reads the completed original trace, then performs separately reviewed GC-provider experiments using the same retained copied caller, nine engine assemblies, target .NET10.0.11 pins and54 original sx-GeneralComp-gt cases. The two schema_aware exclusions remain unchanged;14,547 other catalog cases are unselected. No full-catalog qualification or engine optimization is claimed.

[Event contract review](API-REVIEW.json), [broad experiment review](EXPERIMENT-REVIEW.json), [narrow provider review](NARROW-EXPERIMENT-REVIEW.json), [original report](ORIGINAL-REPORT.json), [allocation report](ALLOCATION-REPORT.json), [evidence](EVIDENCE.zip), [seal](MANIFEST.json) and [renderer receipt](DOCUMENTATION-CHECKS.json). Offline replay: `python docs/xslt30-gc-correlation/verify.py`.

## Measurements and boundaries

| Observation | Original sampled-stack trace | Narrow GC-keyword trace |
| --- | ---: | ---: |
| Completed selected outcomes | 52 passed / 0 failed / 2 original exclusions | 52 passed / 0 failed / 2 original exclusions |
| GC collections | 240 | 239 |
| Generation0 /1 /2 collections | 160 /58 /22 | 158 /58 /23 |
| GC suspension request-window union | 14,297.551 ms | 14,436.532 ms |
| Fully suspended phase union | 14,295.363 ms | 14,429.018 ms |
| Other suspension requests | 3,923 | 0 |
| Allocation ticks | Not requested at informational level | 39,308 |
| Approximate reported allocation-interval bytes | Unavailable, not zero allocation | 5,192,467,272 |

Collection start/stop elapsed includes background GC and is not pause time. Suspension windows include rendezvous/resumption; fully suspended phases use SuspendEEStop through RestartEEStart. Requests can overlap on different initiating threads, so pairing uses process, CLR instance and requesting thread before taking interval unions. All collection/suspension/finalizer pairs match in both complete captures. The initial single-CLR pairing assumption refused observed overlapping requests and was corrected before conclusions; the regression case is covered by tests.

The original thread-time trace has14.297 seconds of GC suspension request windows. About5.124 of7.419 seconds of GC-poll sample weight and6.478 of8.640 seconds of monitor sample weight temporally overlap those windows. These are multi-thread sample weights in the same TraceLog-relative timebase, not CPU time, exclusive responsibility or a causal proof. Finalizer event-window union is 388.004 ms in the original and 513.467 ms in the narrow capture. These event windows differ from the earlier sampled finalizer-thread weights, so those weights cannot be interpreted as finalizer CPU. The narrow capture has substantial GC suspension without the stack sampler; separate fresh-process state and provider overhead prevent a controlled A/B or profiler-overhead estimate.

AllocationTick describes an approximate interval of allocation activity. Its TypeName identifies the object crossing the threshold, not every byte in that interval. Do not assign the5.192GB total to sampled types or interpret tick counts as exact object counts. The narrow data contains39,300 resolved engine contexts; candidate frames with generic BCL signatures are filtered by a strict Bosak namespace prefix before choosing the nearest context.

| Frequently observed threshold-crossing types | Tick observations |
| --- | ---: |
| XdmValue arrays | 3,812 |
| WrapEach iterator | 3,687 |
| FilterNodesLazyIterator | 3,543 |
| Strings | 2,432 |
| EnumerateAttributes iterator | 2,091 |
| StreamingNode wrappers | 1,541 |
| XDocumentNode wrappers | 1,495 |

Resolved contexts repeatedly reach StreamingSource.Pump, StreamingNode.WrapEach/RecordAxis, VM filtering and XML wrapper creation. GetElementTreeSequence remains a relevant weak-table/registration path from the earlier stack trace. The current StreamingSource.Wrap source creates a capturing value-factory expression around ConditionalWeakTable.GetValue. A focused next candidate is an allocation-free cache-hit route with the miss factory kept separate, but neither hit rate, saved bytes nor benefit is established here. Any candidate needs exact adoption evidence, cache-hit/miss identity/index/concurrency/weak-lifetime tests, and a bounded comparison before acceptance. No engine change is part of this PR.

## Failed broad experiment retained

The pinned profiler's gc-verbose preset enables GC, GCHandle and Exception keywords. That broad capture crossed the unchanged256MiB threshold and was terminated after15.165 seconds; status is TraceSizeExceeded, exit124, incomplete and failed. Its272,139,284-byte partial raw trace remains locally retained by SHA-256 in BROAD-TRACE.json. Polling permits a small overshoot before termination. The public evidence archive contains failure receipts, source, logs, ledger and fingerprint; it does not contain the272MB partial trace. Optional offline verification can check that retained local file explicitly. It is never treated as a completed selection.

A separate review selected only `Microsoft-Windows-DotNETRuntime:0x1:5`, excluding GCHandle, exceptions and thread sampling, with the same54 cases,180-second watchdog,8GiB owned-tree limit and256MiB threshold. That experiment completed with52/0/2 and a10,124,170-byte trace. All application/corpus/framework/profiler inventories match. No budgets were raised, cases dropped, original attempts retried or global settings changed. The broad failed trace was not parsed as a valid allocation dataset.

## Read-only extraction and controls

The caught-error generated reader uses the existing pinned TraceEvent3.1.23 libraries from profiler10.0.745401 and exact .NET8.0.28. Its120-second/8GiB owned job only reads reviewed complete traces. [Input whitelist](BASELINE.json) rejects changed, unreviewed or partial traces before .NET launch. The original and narrow reader results retain separate source/binary hashes; source provenance limitations of the earlier TraceEvent version-tag review remain unchanged.

```powershell
python -m unittest discover -s tools -p test_xslt30_gc_analysis.py
python -m unittest discover -s tools -p test_xslt30_gc_extract.py
python tools/xslt30_gc_extract.py --trace D:/retained/raw.nettrace --profiler D:/pinned/tool-directory --output D:/fresh/reader
python tools/xslt30_gc_analysis.py --events D:/fresh/reader/gc-events.jsonl --output D:/fresh/report.json
```

Twelve analysis controls and two pre-launch refusal controls pass. The reusable reader reproduces the original GC report exactly without running the workload. Every diagnostic error is caught; no intentional unhandled negative.NET process is launched. Normal CI gains the14 fast Python controls. All existing engine/runner source, standalone required workflow, corpus/exclusions, production1200-second/8GiB limits, rulesets and unpublished Core/server1.81.8 remain unchanged; no fresh local QT3/qt4 qualification is claimed for this tooling-only slice.

PR487's required run43475843856 timed out1200.234s at sx-treat-017; its original artifact/digest/logs are retained without retry. PR484/485/486/487 remain open/blocked and unmerged. New review checks run independently for this delivery. No publication or performance acceptance is included.
