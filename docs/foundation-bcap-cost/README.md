# BCAP-01 bounded cost and cancellation evidence

2 November 2026. P03's bounded Core feasibility evidence is complete: cost and cancellation observations now complement the qualified 83-control capture/outcome cohort. The measurement host references the **unchanged engine DLLs** from [full isolated qualification](../foundation-bcap-profile/README.md). No engine rebuild, production change, accepted budget, native provider acceptance or public SDK implementation is implied.

## Workloads and observations

Each workload runs in three fresh processes. The first compile and first capture are measured separately; then 20 successful warmups precede 80 measured captures per process: **12 first-use observations and 960 measured warm captures**. Snapshot/provider acquisition and host stylesheet preparation are outside capture timings; operation admission, copied input context, dedicated transform worker, actual serialization, output validation and capture/drain outcome are included. Source authority is synthetic; output is bounded text with no external acquisition.

| Profile | Sources / outputs | Output bytes | First compile median ms | First capture median ms | Warm median / p95 ms | Managed B/capture* |
|---|---|---:|---:|---:|---:|---:|
| Source-free | 0 / principal | 16 | 140.024 | 151.550 | 0.406 / 0.682 | 78,877 |
| Shared | 1 / principal + 2 secondary | 49,155 | 178.611 | 331.429 | 0.749 / 2.833 | 323,103 |
| Distinct, matched payload | 2 / principal + 2 secondary | 49,155 | 180.379 | 353.987 | 0.777 / 2.313 | 323,359 |
| Distinct, larger payload | 2 / principal + 2 secondary | 196,611 | 187.456 | 321.369 | 0.824 / 2.472 | 913,639 |

*Median of three per-process allocation means. Precise process-wide managed allocation includes worker-thread allocations and validation. It does not measure live/retained peak memory, native allocations, reserved thread stacks or OS quotas. Cold measurements include first-use/JIT effects but exclude process launch; the sample is too small to infer cold tail latency. Warm timings aggregate 240 samples per profile, with nearest-rank p95. Environment/runtime/GC/frequency are recorded in each raw receipt.

The matched payload comparison observes 256 additional managed bytes per capture for the two-source profile. It includes real operation membership/lease paths with preopened synthetic inputs, not native acquisition, and is not a universal per-snapshot cost guarantee. Timing distributions overlap; no causal latency penalty or accepted threshold is inferred. The larger-payload row deliberately changes output size and must not be used to attribute all extra allocation to source count. Earlier unmatched exploratory receipts are retained under `evidence/initial-cohort` and excluded from the final summary.

## Controlled cancellation

Three fresh processes each run 12 observations for capture, operation and inherited tokens: **108 observations**, 36 per channel. The request occurs while actual second-output serialization is paused, after the first secondary output completed. Both synthetic snapshot admissions remain held; the measured raw outcome follows cancellation, unpause, serializer unwind, both release callbacks and caller scheduling. Every result stays Cancelled with the first output retained, and both releases occur before the final outcome.

| Token channel | Request to raw outcome median / p95 / max ms |
|---|---:|
| Capture | 0.247 / 4.132 / 6.981 |
| Operation | 0.242 / 0.455 / 0.633 |
| Inherited | 0.253 / 0.453 / 1.151 |

These are observations at a controlled serializer checkpoint, not worst-case cancellation bounds. Long XPath expressions between cooperative checkpoints, native cleanup, worker kill/recovery and real workloads remain outside this experiment. The observed maximum is not a service-level promise. No process-wide error setting or deliberately unhandled crash is used.

## Disposition and next work

Core P03 is **CompleteBoundedCoreFeasibility**: supported/refused invocation profiles, outputs, receipts, original errors, cleanup ordering, controlled raw lifetimes, full isolated regression/conformance qualification, and the named bounded cost observations are recorded. The ten original C01–C10 items are exercised within this private synthetic-provider profile, not universally accepted. Prior qualification remains valid because the actually loaded engine hashes match exactly; it was not rerun or rewritten for this measurement-only step.

BCAP-01 remains open for P04, Braid's disposition of the exact 47-type capture/outcome declaration candidate and qualified profile. Core next prepares those exact review inputs. Missing recorded Schema O1/O2 returns, XmlDb lifetime/membership revision, actual provider/worker runs and agreed R4 workload budgets remain parallel owner work. No prior response is rebound to a new manifest, no extra Core API signature is selected by these timings, and R2/public implementation/freeze/publication remain gated.

## Evidence and reproduction

[Raw run index](evidence/RUNS.json), [summary](evidence/SUMMARY.json), [qualified engine pins](evidence/ENGINE-INPUTS.json) and [measurement basis](SPEC-BASIS.md) specify the cohort. Final raw observations and host source/binary hashes are immutable review evidence; the original qualification dossier retains the engine source and binary archives. Original exception graphs remain local diagnostics rather than IPC payloads, as specified in the outcome candidate.

The unsealed workflow runs `python docs/foundation-bcap-cost/tools/prepare.py`, `tools/run.py`, `tools/summarize.py` and `tools/verify.py` with the same full directory prefix. Preparation generates a separate friend-identity test host from the qualified fixture and measurement methods; it verifies engine hashes and never rebuilds those engines. Mutating tools refuse a sealed cohort. Use `tools/verify.py` to verify this record; copy tools to a new dossier and select a new host/output directory for new measurements. Producer source, owner registries, product pins and prior dossiers are unchanged.
