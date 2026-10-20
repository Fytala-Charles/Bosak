# XSLT 4.0 baseline and harness audit â€” 20 October 2026

This is the first approved testing slice, independently of Braid integration. No engine behavior, upstream fixtures, assertions, skip lists, packages or gates changed. The source baseline is `53c098d1529343b41f763168df1daa3b6ce8bdfe` (1.22.0). The pre-existing untracked `.sweep-baselines/` directory is preserved.

## Fresh pins and execution

Freshly fetched authoritative upstream default branches remain unchanged from the previous audit: xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`, qtspecs `72f222929768b411d8e88d96a1a8740483290078` (both `master`). Tests ran from a clean detached worktree; the specification is a separate clean shallow clone. [Machine receipt](XSLT40-BASELINE-2026-10-20.json) retains arguments, exact pins, specification SHA-256 values, failures, exit codes, durations and raw-log hashes. Local raw evidence is retained under `.guard-tmp/xslt40-baseline-20261020/`.

The Release conformance project build passed with zero warnings/errors; all ten permanent runner profile/adapter controls passed. Each matrix process had a 300-second outer watchdog, isolated working directory and retained log. All ten completed without a timeout. Every summary total, failure-line count and exit status was checked. Expected exit 1 is a failing selected baseline, not an execution failure or a green gate.

| Profile | Family filter | Passed | Failed | Skipped |
|---|---|---:|---:|---:|
| legacy | array | 155 | 5 | 20 |
| legacy | select | 157 | 0 | 1 |
| legacy | switch | 9 | 1 | 2 |
| legacy | record | 38 | 26 | 1 |
| legacy | type-alias | 23 | 8 | 1 |
| xslt40-draft | array | 158 | 2 | 20 |
| xslt40-draft | select | 157 | 0 | 1 |
| xslt40-draft | switch | 9 | 1 | 2 |
| xslt40-draft | record | 38 | 26 | 1 |
| xslt40-draft | type-alias | 23 | 8 | 1 |

Draft aggregate: **385/37/25** across 447 catalog cases. Legacy: **382/40/25**. These are selected-family results, not full XSLT 4.0 conformance. Array/record/type-alias draft results remain **219/36/22**, matching the [previous checkpoint](XSLT40-RECORD-MAP-SUBTYPING.md).

## Failure disposition

The unchanged array/record/type-alias failure inventory retains the earlier classification: 33 catalog/draft differences and three unresolved contracts (`array-025`, `record-121`, `record-123`). No old expected errors were rewritten to match Bosak. The receipt preserves every current diagnostic so that the historical classification can be reassessed against future pins.

The additional failure is **switch-901**, in both profiles: expected `XTSE0010`, actual `<out/>`. The fixture puts `xsl:fallback` before a later `xsl:when`. The pinned `element-catalog.xml` switch grammar requires `when+`, `otherwise?`, `fallback*`, supporting the expected structural error. The prose says fallback children are ignored by a 4.0 processor, so the fix should enforce the grammar while continuing to ignore valid trailing fallbacks. This is a focused engine-defect candidate for a separate slice; the current baseline does not fix it or waive its error assertion.

## Harness findings

- Family selection is a case-insensitive substring filter. `array` selects `array` (37), `sx-SquareArrayConstructor` (56) and `arrays` (87); `record` selects `xsl-record` (18) and `record` (47). Select/switch/type-alias select 158/12/32 cases. The receipt lists exact files and counts.
- Case-filter mismatches contribute to `Skipped`; dependency skips can have no individual reason. Skip totals therefore cannot be reported as a pure unsupported-feature inventory. The displayed catalog test-set count also does not mean selected count.
- XSLT40 mode disables legacy error-code aliases, and an empty/all-skipped execution fails. Existing controls verify profile defaults, both explicit profiles, invalid/missing/duplicate arguments, empty selection and adapter behavior.
- Missing test-set files can be printed and omitted from summary counts. This audit parsed every selected catalog file successfully and reconciled all 447 cases per profile, so no such omission occurred in these runs.
- The normal wrapper and runner have no built-in watchdog; their receipt is produced only after process completion. This audit supplied an external process-tree timeout. Permanent timeout receipts and clearer selected/skipped accounting are worthwhile harness improvements before an unattended wider sweep.

## Recommended next slice

First make a bounded harness-hardening PR: retain timeout/interrupted receipts, distinguish unselected cases from dependency skips, and reconcile selected catalog counts. Use adversarial fixture controls for those behaviors, preserving the ten current controls. Separately prepare a small switch validation fix with legal trailing-fallback and illegal interleaved-fallback tests. Do not mix it with upstream expectation corrections.

Then review existing array/select/switch cases against pinned normative rules and add roughly 20â€“30 genuinely missing positive and negative cases. Existing switch-008/009 already cover first-match short-circuiting and an empty selected branch; avoid duplicating them. Records/type aliases remain a later batch while the three unresolved contracts are investigated. Fetch upstream again before creating any contribution branch and before submission; record the refreshed base. No upstream submission, gate promotion or release is performed by this audit.

Full solution tests and frozen engine gates were not rerun: this step adds evidence/documentation only and changes no engine or test source. Braid acceptance is a separate consumer checkpoint; this audit makes no adoption claim.

## Evidence correction after the post-fix rerun

The post-fix matrix rerun reused the baseline scratch log paths. The original manifest, diagnostics and hashes in the linked baseline JSON remain intact, but its original raw logs were overwritten. The post-fix evidence belongs to the separate switch validation PR. Do not identify post-fix raw files as the original baseline logs.
