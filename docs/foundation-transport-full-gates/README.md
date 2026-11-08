# Full qualification of the composed transport candidate

31 October 2026. The isolated candidate passes the complete solution unit suite and both required conformance gates. Production remains unchanged at 1.35.0.

| Check | Fresh result |
|---|---|
| Full Release solution build | Pass; zero errors, five warnings in unchanged files |
| All ten solution unit-test projects | 24,245 passed, zero remaining failures/skips |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Skip names and reasons | Both maps unchanged from the sealed production baseline |
| Unchanged retained binary caller | 20 / 0; exact seven loaded hashes |
| Native integration probe | 121 / 0; 14 actual VM expressions |
| Atomic/lifecycle probe | 376 / 0; 39 fixtures across 11 public-factory atomic kinds |

The source snapshot is immutable checkpoint 83341091, whose src/tests match production semantic baseline 8080b18b. A separate materialization applies only the three previously sealed source overlays: XdmValue, SchemaEvidenceKernel and SchemaQuerySession. The full solution is rebuilt together; this is a new ten-assembly cohort, separate from earlier mixed-DLL trials. The seven foundation DLLs are identified separately for metadata and retained-caller comparison. The original production build outputs and source files are untouched.

All ten solution test projects execute via vstest against the rebuilt outputs. XSLT runs from a unique temporary copy to avoid the documented Application Control issue. Database collections are serialized. Its first sandbox run failed 45 fixture startups with HttpListenerException (two tests passed); the complete 47-case rerun with local listener access passes. Both original and rerun TRX/logs are retained, and final counts use the successful complete database run once. The full suite is therefore the ten-project successful coverage, not a claim that the original sandbox run was entirely green. The XSLT suite passes all 8,196 tests; its measured duration here is 4m31s, not a controlled performance comparison.

A read-only startup observer records actual loaded Bosak assembly hashes/paths/contexts for each unit-test host and both conformance processes. Every observed engine DLL matches the canonical rebuilt layout. Test output hashes also match before and after execution. All seven foundation assemblies are observed in both gates, the retained caller and both integration probes. Empty binding files from vstest launcher processes are preserved separately from actual test hosts. The observer changes no engine inputs; these instrumented runs are correctness evidence, not latency benchmarks.

Both corpus checkouts retain their clean status and exact revisions. They are read through isolated junctions or explicit original paths; no corpus files or owners' checkouts are changed. The full source archive, overlay digests and materialized source inventory identify what was compiled. The native host's inherited scope string predates the kernel overlays; the actual kernel is established by these manifests rather than that historical label.

## Public metadata review

There are still 78 exported types across the seven foundation assemblies. Declared public reflection members rise from 1,199 to 1,201: only XdmValue.Equals(object) and GetHashCode() are newly declared. The full inherited public signature lists and both base virtual slots remain unchanged; XdmValue remains 48 bytes. DeclaringType changes from System.ValueType to XdmValue, so this must be disclosed as a declared-manifest change. The [compatibility review](API-REVIEW.md) recommends retaining both overrides together but does not infer owner acceptance or final API freeze.

The fresh atomic probe rechecks this rebuilt cohort's allocations: cached kernel reads and evidence lookup allocate zero bytes per operation; issued-value reads allocate zero for integer and 32 for string/decimal. Explicit no-op admission remains 32 for integer and 64 for string/decimal. Its seven samples per group are recorded separately from the earlier paired performance trial. No latency, memory, I/O, worker or owner performance budget is accepted by these results.

Verify with `python -B docs/foundation-transport-full-gates/tools/verify.py`; add `--check-current` while the ignored qualification directory is present to compare every materialized input with its archived/overlay bytes. The [reproduction runner](tools/run.py) has separate prepare/units/gates/review/database-retry phases and requires fresh directories/retained-caller output paths. All child processes have watchdogs and scoped Windows error mode. Raw unit TRX files are compressed without changing their bytes.

Next: prepare the production promotion patch with explicit disposition of the two reflection-visible overrides. C45/public Schema provider API acceptance, consumer qualification, owner-approved performance budgets and API freeze remain pending; the full gate success does not close them.
