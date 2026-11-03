# W1-A internal neutral source admission

3 November 2026. Source version **1.43.0** implements and qualifies the internal neutral admission slice after [scoped R2 closure](../foundation-r2-closure/README.md). This is actual production-source integration, with no new public facade or owner dependency upgrade. [Status](STATUS.json), [adoption basis](SPEC-BASIS.md), [acceptance overlay](ACCEPTANCE.json).

## Implemented behavior

`QuerySourceSession` admits a trusted host-selected ordinary input without any Schema issuer or original assessment. It retains an already-acquired owner lease, checks availability before obtaining a root, and requires exactly one admitted owner before purported node getters or evaluation dispatch. Captured inputs retain the exact operation token and recheck node membership before dispatch. Foreign environments, duplicate admissions, ambiguous/foreign nodes, unbound original evidence, schema-marked scalars and unsupported graphs refuse explicitly.

`QuerySnapshotLifetime` extracts the existing revocation/raw-operation lifetime into a neutral shared primitive. The typed evidence kernel reuses it with unchanged issuer/type/origin/cache checks. Session close revokes guarded access, attempts every cleanup, and raw callbacks retain resources until actual completion; cancelling an external wait does not drain or dispose. Drain waits through pending owner admission rollback and source-free raw work as well. Primary and cleanup errors retain identity and aggregate boundaries. The internal XPath factory reuses existing immutable static capture and closed external-resource policy; copied scalar results are the bounded initial result profile.

This is W1-A only. Optional advisor composition, public accepted declarations and XQuery/XSLT host routing remain dependent slices. Raw IXdmNode escape is not universally revocable; providers retain their own contracts. The test input uses privately registered sealed projections and preserves owner association through parent/document/child/axis/attribute/namespace routes; it does not qualify native Schema or XmlDb adapters.

## Qualification

| Evidence | Result |
|---|---|
| Full solution Release build | Pass; final build zero errors/warnings (the earlier full rebuild retained five unchanged test/conformance warnings). |
| All ten unit projects | **24,414 passed**, zero selected-cohort failures: baseline 24,390 plus 24 new controls. |
| W1-A acceptance | All **18 required prefixes passed**; 17 original cases plus drain, with six additional controls. |
| QT3 | **31,397/0/424**, unchanged frozen gate. |
| Gated qt4 | **8,941/0/1,918**, zero gated/exploratory failures; 11 experimental passes. |
| Existing shipping public metadata | All **11 assemblies / 270 types / 5,770 rows**, original declarations and friend grants unchanged. |
| Private capture regression | **83 controls twice** in a fresh private-extension cohort carrying the new shared lifetime; its Runtime/XSLT prototype methods remain separate from production public SDK support. |

[Unit results](evidence/UNIT-RESULTS.json) retain exact project/TRX hashes. The original full solution run had 45 Database HttpListener fixture failures under the sandbox; the same Database DLL passed all 47 tests with authorized loopback. Other nine projects passed, including all 8,234 XSLT tests in their normal layout; no XSLT Application Control workaround was needed. The final Runtime project has 390 passes after two additional lifetime controls. The [original execution archive](evidence/EXECUTION-RECEIPTS.zip) preserves failed development attempts as well as final build/test/gate/probe receipts; qualification selects the explicitly named final receipts rather than counting all archived executions as passed.

[Exact source/binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [loaded-binding observations](evidence/LOADED-BINDINGS.json), [public metadata comparison](evidence/API-COMPARISON.json), [changed source archive](evidence/CHANGED-SOURCE.zip) and [all eleven compiled engines](evidence/ENGINE-BINARIES.zip) retain the qualification cohort. Changed sources apply to the adoption record’s exact base commit. Gate results and loads refer to production 1.43 binaries. The distinct private capture regression has [its own source map](evidence/BCAP-REGRESSION-SOURCE.json) and [binary bindings](evidence/BCAP-REGRESSION-BINDINGS.json); it retains nonshipping prototype methods and is not an unchanged-binary native consumer proof.

## Measured allocation tradeoffs

[Three fresh-process observations per lane](evidence/COST-OBSERVATIONS.json), each with 200 warmups and 4,000 iterations per scenario, show typed snapshot open/close rising from **304 to 440 bytes** (+136 bytes for neutral lifetime/drain state), while warm typed reads fall from **96 to 88 bytes**. Ordinary open/close observes **272 bytes** and the captured ordinary XPath operation **24,672 bytes**, including fresh bounded context, closed policy and function dispatch. Timing samples are observations, not latency guarantees or accepted budgets.

The ordinary per-operation context cost is a concrete private implementation optimization target before representative R4 budgets. No new public batching/cursor API is justified by these numbers. Legacy/default APIs do not activate the new opt-in layer. Owner PF-01–09 workloads, native backing resources and actual cancellation/recovery envelopes remain R4 obligations.

## Next slice

W1-B composes optional authenticated schema evidence with neutral admission, preserving exact input/node/facts/origin/container checks. W1-C/D introduce the accepted thin public facade and all-host integration with supported/refused profiles. Original W1-09/10 remain NotRun for host integration; W1-18 remains NotRun for actual XmlDb R4 qualification. No C45 acceptance, API freeze, publication authorization or target release version/date is selected by this internal source bump.
