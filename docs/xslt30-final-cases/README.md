# Final standalone XSLT failure corrections - 12 November 2026

PRs [#474](https://github.com/Fytala-Charles/Bosak/pull/474) and [#475](https://github.com/Fytala-Charles/Bosak/pull/475) are merged. This follow-up starts from `d86fc38e7ee83519e761afa16d960d7c74a4ad55`, with development Core/server metadata 1.81.5, unpublished. Published Core remains 1.81.2 and the extension remains 0.2.6. All preceding qualification dossiers and their original receipts are unchanged.

## Diagnoses and bounded corrections

The caught diagnostic against the unchanged 1.81.4 assemblies isolates both remaining failures. In `try-022`, all three principal-result assertions pass and the secondary document satisfies `/output`; its capture key is relative while assertion lookup uses an absolute host-resolved identity. The observer now canonicalizes only map lookup keys with that same existing resolver. It preserves the processor's dynamic output-URI behavior and the captured node's existing URI annotations; this does not claim general no-base-output-URI semantics.

In `output-0707`, a raw initial-template result uses the typed template's temporary container. The attribute instruction mistakes this for ordinary element content and raises XTDE0410 after preceding items. A top-level attribute in that existing typed/raw sequence now goes through its placeholder accumulator, preserving item position, duplicate names and parentlessness. Actual element content still rejects late attributes. The [adoption record](ADOPTION.json) identifies immutable XSLT and serialization revisions and sections before implementation.

The console summary now reports `Selected-case result: PASS`, `FAIL` or `NO EXECUTED CASES` with executed and excluded counts. It no longer prints a rounded percentage that could say 100.0% despite failures. Legacy exit behavior is unchanged; independent controllers still check counts and completeness.

## Verification

Five new unit cases pass, covering the nine-item adaptive sequence, mixed attribute/text order, duplicate attribute items, declared attribute sequence types preserved element-content errors and nested namespace simple-content evaluation. The identical sources against the sealed 1.81.4 baseline produce three regression failures and two protective passes. The first compile diagnostic is retained separately.

The complete output set passes267 cases with14 existing exclusions; the complete try set passes82 cases with7 existing exclusions. Fourteen assertion controls produce the expected nine passes and five explicit refusals. The nested no-base positive control fails on the baseline and passes on the candidate; wrong and missing nested outputs remain rejected, and all golden bytes remain unchanged. Sixteen existing profile/error controls pass. Wrapper controls retain successful, failed, empty and timed-out receipt behavior. An initial wrapper preflight rejected temporary gate-fixture junction metadata; after those verified junctions were removed, its repeat passed. Both attempts are retained.

The complete pinned catalog rerun records **10,221 passed / 0 failed / 4,380 excluded** in 829.117 seconds. All 234 sets / 14,601 cases at `fddf1cf920087e791f13315d68dfbe874d97dc56` are accounted for under basic/non-schema-aware Legacy/Xslt30 execution. All exclusion identities and explanations are unchanged; no corpus file is added, changed or removed. The original 13,229-file checkout and all 32 runtime files remain unchanged. All 26,359 solution tests (including 8,334 XSLT tests) and 90 separate language-server tests pass. Frozen QT3 remains 31,397/0/424 and selected qt4 remains 8,941/0/1,918, with exact named skip/reason maps.

The first full attempt records 10,220/1/4,380 and is rejected: namespace-2602 exposed a nested simple-content collector being mistaken for the typed template result. The corrected route also checks exact root-container identity; a fifth regression case protects this boundary. Both complete attempts and their source snapshots remain separate evidence.

The accepted packet is `D:/Development/Bosak/.guard-tmp/xslt30-final-retry-20261112`. Execution used the retained 1,200-second watchdog and 8 GiB job-memory limit; these are diagnostic controls, not performance acceptance. Four changed source files and runtime inputs are sealed separately from the base commit. The temporary idle reservation was released.

[Results](RESULTS.json), [sealed evidence](EVIDENCE.zip), [hash manifest](MANIFEST.json), [failure ledger](failures.csv), [named exclusions](skips.csv) and [review description](PR-DRAFT.md) are retained. Run `python docs/xslt30-final-cases/verify.py` for read-only verification. The retained selected profile qualifies; passing under 4,380 exclusions does not establish full conformance.

## Receipt integrity and scope

The earlier post-merge PR474 CI attempt failed an unchanged allocation-observation test (63 measured warm bytes against expected0). Its original failed log is retained; the failed job passed on a same-commit rerun. Latest merged-main CI and optional Schema integration pass. No allocation threshold or test was weakened.

No expected fixture edit, new skip admission, schema-coverage expansion, package publication or automatic standalone-gate promotion is part of this maintenance correction. Passing executed cases under retained exclusions does not establish complete XSLT conformance.

## Local reproduction

From this worktree:

```powershell
dotnet build Bosak.sln -c Release
dotnet test Bosak.sln -c Release --no-build
dotnet test tests/Bosak.LanguageServer.Tests -c Release
python docs/xslt30-final-cases/controls.py --output <fresh-control-directory> --assembly tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll
python docs/xslt30-final-cases/prepare.py <fresh-packet-with-LF-corpus>
python docs/xslt30-qualification/run.py --packet <fresh-packet-with-LF-corpus>
python docs/xslt30-final-cases/report.py <completed-packet>
python docs/xslt30-qualification/audit.py --packet <completed-packet> --original-corpus D:/Development/Bosak/tests/xslt30-test
```

Preparation/sealing helpers refer to this retained Windows workspace. Earlier dossiers keep their own qualified inputs, unsuccessful results, original diagnostics and historical exclusion explanations.
