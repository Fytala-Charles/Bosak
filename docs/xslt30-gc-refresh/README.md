# GC/allocation PR delivery refresh

PR #488 incorporates qualified main `728f092063fb2865ab1813e0b942ba8751aaec03` after PR #487 passed all six checks and independent artifact replay. Original commits are preserved by a merge, without rebase or force-push. The five original GC-analysis/extraction tool, test and reader-template files are byte-identical to head `116b8245ea7c5a2c54c345624415d0f900e51bea`. CI retains case-cost and sampled-stack controls and appends the original two GC control commands exactly once. Production engine, tests, runner and required gate match main.

| Retained attempt | Result | Interpretation |
|---|---|---|
| Original PR488, 43484674916 | Timed out at 1,200.234 seconds; last started namespace-3152 | Failed and retained; original source tree independently checked |
| Qualified PR487, 43509508532 | 10,221 / 0 / 4,380 in 1,179.485 seconds | All 14,601 identities/exclusions replayed; six checks green before merge |
| Prior main, 43508680966 | 10,221 / 0 / 4,380 in 726.125 seconds | Complete independent artifact replay |
| Refreshed PR488 | Required hosted qualification pending | Actual 1.81.9 engine fix inherited from main; original 1,200-second / 8 GiB limits |

The [original GC/allocation dossier](../xslt30-gc-correlation/README.md) stays byte-for-byte sealed. Original sampled-stack/informational-GC data, the failed broad capture, the separately reviewed narrower GC-keyword/Verbose capture, raw hashes, profiler/native/reader identities and all analysis corrections remain historical 1.81.8 evidence. The broad raw file remains locally retained by hash; its bytes were checked with the existing optional verifier and are not added to this repository. It stays failed/incomplete. No capture, reader extraction or .NET execution is performed here; verification replays retained JSON data and hashes. Default extraction still enforces the original raw-trace whitelist.

GC collection elapsed, request windows, fully suspended phases, finalizer windows and sampled thread weights remain separate domains. Suspension requests are paired by process/CLR/requesting thread. Allocation intervals are approximate; the named threshold-crossing object does not own all interval bytes. No per-type byte totals, exact object counts, exclusive CPU attribution, failure rate, suite-wide speedup or timeout cause is claimed. A future current-engine measurement needs its own reviewed identities and scope; original observations are not relabeled as current-engine results.

Twelve GC-analysis, two extractor, sixteen inherited stack and fifteen inherited case-cost controls pass. Release workflow validation passes using the existing workspace-isolated CI-pinned PyYAML 6.0.3 dependency. No global package/environment, WER, runtime, fixture, exclusion/admission, required workflow, budget, ruleset, schema/private licensing or publication change. Core/server 1.81.9 remains unpublished. The narrowed capture remains distinct from the original failed experiment, not its retry.

[verify.py](verify.py) replays original and successor evidence without .NET. Historical checkout-specific boundaries are checked at their recorded commits, separately from current scope. Sealed original verifiers remain unchanged. See the [manifest](MANIFEST.json), [controls](CONTROLS.json), [original source proof](ORIGINAL-SOURCE.json), [merge receipt](MERGE487.json), [workflow validation](WORKFLOWS.json) and [HTML receipt](DOCUMENTATION-CHECKS.json).
