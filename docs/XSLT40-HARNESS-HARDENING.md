# XSLT 4.0 harness hardening — 20 October 2026

The Windows wrapper now accepts `-TimeoutSeconds` (default 300, range 1–86400), retains stdout/stderr and a receipt before execution, and terminates the process tree on timeout. Timeout receipts retain status `TimedOut`, exit code 124 and any partial summary. Start failures/interruption retain an unsuccessful receipt. Forced termination of the wrapper itself can leave the initial `Running` receipt; it is never a success. Each run has an isolated working directory for `last_test.txt`.

`SuiteRoot` must be the actual root of an independent Git checkout; a nested directory that resolves to a parent repository is rejected. This avoids misidentifying the suite revision. Logs and receipts remain under `.artifacts/xslt40/`.

The runner adds `Unselected`, `Selected`, `Selected skipped` and `Missing test sets` summary fields. Historical `Total` and `Skipped` remain compatible: `Skipped` still includes case-filter mismatches; `Selected skipped` excludes them. Filtering happens before set-level dependency skips, so only selected cases contribute to selected skip counts. Resumed sets remain outside the current execution summary. A missing selected test-set file fails XSLT40/draft execution, even when other selected tests pass. The wrapper reconciles all summary counts and rejects incomplete or zero-executed results.

Verification adds four permanent process controls (filtered accounting, missing selected set, missing unselected set and filtered set-level dependency skip) to the existing ten. Four Windows wrapper controls cover successful, failing, empty and timed-out execution using a temporary local Git fixture repository with spaces in its path. These fixtures are not upstream contributions. The timeout control's process-tree cleanup passed outside the development sandbox; the first sandbox-denied attempt and receipt remain retained separately. The Windows CI job runs these controls and uploads `.artifacts` even on failure.

Run after the Release build:

```powershell
./tests/Bosak.Xslt.Conformance/verify-runner-profile.ps1
./tests/Bosak.Xslt.Conformance/verify-wrapper.ps1
./run-xslt40-tests.ps1 -TestSet select -ProcessorProfile Xslt40Draft -NoBuild -TimeoutSeconds 300
```

This change preserves exact error assertions, native suite fixtures and all engine behavior. The switch validation fix and version increment belong to a separate PR. [Baseline](XSLT40-BASELINE-2026-10-20.md) records the pre-change results.

[Harness verification receipt](XSLT40-HARNESS-PR-VERIFICATION.json) records the 14 CLI and four Windows wrapper controls. The prior combined local run passed 10,030 solution tests and unchanged frozen gates, including 20 tests belonging to the separate switch PR; that count is not the isolated harness branch count. CI validates the isolated harness head, whose solution inventory remains 10,010 tests. Source and public package versions remain 1.22.0.
