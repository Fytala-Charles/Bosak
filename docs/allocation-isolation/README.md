# Isolated allocation regression measurements

PR493's first head864040e1 failed two allocation checks: comparison5512 bytes against0; conversion4,007,040 against4,001,024. The earlier main failure was4,000,120 against4,000,000. The1KiB allowance was insufficient and is withdrawn. The precise source of the extra test-host allocations remains unproven. Each failed run and original runtime TRX remains separately recorded; none was retried or reclassified.

Two existing tests now launch the non-packable Bosak.XPath.AllocationChecks fixture automatically. Cached strongly typed delegates call the same VM methods in a fresh process;10000warm-up and100000measured calls per row; setup, assertions, reflection, JSON and process launch are outside measured loops. Every row is reported once, with no retry/minimum selection. Parent tests validate protocol, PID, engine version, row names/results/counts and budgets. The child has a60-second watchdog and256MiB managed-heap cap scoped through its environment; this is not an RSS limit. Errors are caught in the worker; engine-bearing types resolve inside the entry-point guard. Build copies the worker automatically; ordinary build/test commands remain.

| Check | Result |
|---|---|
| Corrected full solution | 26,435 passed |
| Separate language-server suite | 90 passed |
| Final focused comparison/matcher tests | 9 passed |
| Current six comparisons | 0 bytes/call |
| Current five primitive matches | 0 bytes/call |
| Current five primitive conversions | 40 bytes/call; ceiling40, no allowance |
| Old1.81.9 relational comparisons | 1,008 bytes/call; rejected by exact0 checks |
| Old1.81.9 equality/inequality | Already0; not claimed as regressions |
| Old1.81.10 primitive matches/conversions | 72/112 bytes/call; rejected |
| Invalid worker workload | Caught argument error, exit2 |

The retained-engine controls compile exactly the production worker source against the oldest qualified references, then run the same compatible calibration binary against current and both retained DLL sets. Production worker and compatibility worker are separate builds; their source identity is bound. No new runtime binaries are exported. An initial calibration used newer references against older DLLs and failed before Main with an unhandled loader error; its stderr is retained. Startup was corrected to defer engine-bearing type resolution inside the catch boundary, and compatible references fixed the calibration. That incompatible launch was not repeated. The initial assumption that all old comparisons allocated was corrected from the recorded data: equality/inequality already allocated0, while four relational operators allocated1,008. Original data was not rerun to change it. All successful calibration launches were bounded by60s/512MiB Windows jobs with the same256MiB managed-heap cap and process-scoped error mode. No machine-wide WER/runtime setting change.

The first isolated implementation passed the full suite; after startup guarding changed, the complete suite passed again. Enabling XML documentation left the tested worker DLL hash unchanged; final focused tests passed. No engine, schema validation, public API, runtime version, production runner, fixture, exclusion, workflow or conformance watchdog change. The new console project is a test fixture in the solution, not a product layer. Development1.81.11 remains unpublished. This is calling-thread regression evidence, not a full-workload benchmark.

Prior allowance-head standalone43594586250 independently verifies10,221/0/4,380/all14,601 identities in1108.078s, original1200s/8GiB and hashes intact. It does not qualify this new head or replace its two unit failures. Required hosted checks and independent artifact replay remain required before merge.

See [review](REVIEW.json), [results](RESULTS.json), [compact receipts](EVIDENCE.zip), [manifest](MANIFEST.json), [offline verifier](verify.py) and [HTML receipt](DOCUMENTATION-CHECKS.json). Run `python docs/allocation-isolation/verify.py` to replay original failures, source binding, full counts, exact worker rows and old-engine controls without launching .NET. The archive contains text/JSON/TRX/source snapshots only. Full original artifact ZIPs/runtime/first-pass logs remain local under `.guard-tmp/allocation-isolation-20261114`; the earlier test-budget dossier is historical and remains unchanged.
