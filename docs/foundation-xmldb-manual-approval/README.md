# Exact-action approval follow-up

Current status (8 November 2026): process-probe execution is unblocked. Integration coverage is 862 full-run passes plus one layout-corrected focused pass. The original full-run failure is retained. Aggregate qualification is 4,482 distinct passing cases across runs. Release acceptance remains pending.

## Historical first attempt

The VS Code picker approval was recorded and consumed for the Integration-only command. VSTest exited 1 before test discovery because the launcher requested `Bosak.XmlDb.Integration.Tests.dll`; the pinned file is `Bosak.XmlDb.IntegrationTests.dll`. This launcher error is corrected. No Integration cases ran in this attempt, and Storage was not rerun. The one-time receipt remains consumed; no automatic second attempt was made.

Prior results remain 3,688 distinct passing cases, with 794 Integration cases unqualified. Original restriction and result snapshots remain historical. `before/` preserves the prior launcher and manifest. The captured failed execution log is retained here. No release, tag or publication occurred.

## Fresh review, 8 November 2026

At the user's request, `tools/retry.py --lane integration` was submitted separately with `require_escalated`, explicitly disclosing the intentional child-process kill/fault probes. Automatic review allowed the request and the run started. The historical consumed approval was not reset or reused. The fresh launcher retains the complete layout hash check, authorization constraints, binding hook, watchdog and process-scoped error-mode wrapper. It uses the corrected DLL name and separate execution, binding and TRX output paths. Storage is excluded. Final results will be recorded after completion.

## Final result

The fresh full run completed in 12.58 minutes: **862 passed, one failed** out of the exact 863-case retained Integration catalog. The failure was `ReopenTests.Store_ReopenedInNewProcess_ReturnsHeaderWithoutLinePayloads`: the flattened copied layout lacked `Bosak.XmlDb.slnx`, so its repository-search assertion failed before launching the sample or exercising the database.

A separate pinned repository-shaped diagnostic layout supplies the unchanged marker and already built quick-start sample. All original guarded test payload bytes and seven preview engine hashes match. A test-name-filtered rerun passed that one case. No production or test source changed. Full-run failure, focused rerun, binding reports and exact layout hashes are retained.

There are now **863 distinct Integration cases passed across the two runs**, and **4,482 distinct cases** including the previously completed Storage3572 and XDM47. This is split-run coverage, not a claim of a single wholly green full run. All intentional process kill/fault cases passed in the full run. The approval blocker is resolved. Owner acceptance, baseline/budget adoption, production adapter and artifact-provenance delivery decisions remain separate; no tag or publication is authorized by this result.
