# Bounded current-implementation allocation observation

The diagnostics backlog is merged. PR #488 qualified10,221 passed/0 failed/4,380 excluded in1,186.500 seconds; merged main375f2a0158b3c2a0059f52a4ee0adcd5306a6c00 independently qualified the same14,601 identities in1,047.078 seconds. Artifact digests, source trees, exclusions and unchanged corpus/runtime hashes verify under the original1,200-second/8GiB policy. This study changes no engine code, fixture, gate, exclusion/admission, workflow, default historical baseline or raw-trace whitelist. Development Core/server1.81.9 is unpublished.

The [pre-implementation/pre-capture review](EXPERIMENT-REVIEW.json) selects the exact previously qualified1.81.9 DLLs from af52e427ea122cdeef70ad125a90492ed585cc9e; engine/runner source matches current main byte-for-byte after line-ending normalization. These are the retained qualified bits, not a new main-build binary claim. The same reversible copied caller is compiled against all nine exact DLLs. Target10.0.11/Disable, collector8.0.28/Disable, profiler10.0.745401/package closure, five loaded native modules and the complete target framework inventory stay pinned. Reader/API review pins remain separate from build-commit provenance gaps already recorded in the original dossiers.

| Scope / control | Result |
|---|---|
| Selected set | sx-GeneralComp-gt;54 identities;52 passed/0 failed/2 original schema-aware exclusions |
| Outside selected set |14,547 derived as full pinned catalog14,601 minus54; filtered runner prints Total54/Unselected0 |
| Capture |One narrow Microsoft-Windows-DotNETRuntime:0x1:5 trace;9,597,530 bytes |
| Capture bounds |180s /8GiB owned process tree /256MiB trace threshold; no retry |
| Read-only extraction |Pinned original reader source;120s /8GiB; only this complete producer/hash chain accepted |
| Guards |4 producer +3 reader tests pass; negative launcher mocked; no unhandled.NET negative case |
| Integrity |Application, corpus, target framework, collector and profiler unchanged before/after |

The two exclusions are sx-gc-gt-100/101. Their reason is derived from the original pinned catalog/profile (schema-aware feature unsupported); the runner does not print individual case skip explanations. No synthetic zero duration is assigned to14,547 outside-set cases. See [scope provenance](SCOPE.json). The producer's unselected field records derived outside-set scope, not its console counter; both meanings remain explicit.

| Observed GC domain | Historical1.81.8 narrow capture | Current1.81.9 capture |
|---|---:|---:|
| Collections |239 |227 |
| Fully suspended phase union |14,429.018ms |13,100.116ms |
| Allocation ticks |39,308 |36,353 |
| Reported allocation-interval bytes (approximate) |5,192,467,272 |4,874,098,336 |

These independent fresh-process captures have different caller reference metadata and heap/cache/timing histories. They are not a controlled benchmark, causal delta, overhead measurement or failure-rate estimate. Collection elapsed is not pause time; request windows, fully suspended phases, finalizer windows and sampled thread weights remain distinct. Threshold-crossing TypeName does not own all interval bytes, and tick/name frequencies are not exact object counts or per-type byte totals. Generated class names may renumber. See [comparison limitations](COMPARISON.json) and the complete [report](REPORT.json).

Current frequent threshold names include WrapEach iterators(3,786), XdmValue arrays(3,753), lazy node-filter iterators(3,581), strings(2,370), attribute iterators and streaming/XML wrappers. Real engine contexts include streaming pumping, date parsing and sequence materialization. These are investigation leads, not an allocation-byte ranking. No CreateValueCallback or DisplayClass50_0 threshold name was observed in this current trace; absence of samples is not proof of zero allocations. The prior exact warmed-hit regression remains direct proof of the wrapper-factory change:9,600,000 bytes removed per100,000 warmed hits.

The next bounded lead is primitive-scalar materialization at comparison/conversion call sites. First build an allocation reproduction and inspect existing sequence-view helpers. Any later fast path needs separate specification adoption and tests protecting typed-node/list atomization, lazy/error order, sequence repeatability and lifetime. No such engine change is implemented in this PR.

[EVIDENCE.zip](EVIDENCE.zip) retains the exact one-shot orchestration, copied source, raw trace, case/native ledgers, reports and guard controls. [REFERENCE.zip](REFERENCE.zip) binds the exact caller/engine/reader payloads. Original historical traces/baselines/whitelist stay sealed and unchanged. The archived orchestration uses the recorded workspace paths; it is an auditable one-off, not a new general-purpose CLI. [verify.py](verify.py) independently replays data, hashes and source boundaries without executing.NET. See [results](RESULTS.json), [manifest](MANIFEST.json), [HTML receipt](DOCUMENTATION-CHECKS.json), [main qualification](MAIN-488.json) and [merge receipt](MERGE488.json).
