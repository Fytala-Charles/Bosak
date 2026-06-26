# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`defefde` — Precedence-aware XTSE3450 conflict detection for static variables; clears `use-when-0137/0138`; full suite 4,640/611/9,349 (88.4%)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Precedence-aware static variable conflict detection: `Stylesheet.BuildStaticContext` now evaluates top-level `use-when` in document order and tracks import precedence; same-precedence conflicting values and higher-precedence overrides that change the effective value raise `XTSE3450`. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 2 | Static variable/parameter kind mismatch handling: a static variable and static parameter with the same expanded name raise `XTSE3450` when the higher-precedence declaration is processed second; a higher-precedence declaration processed first shadows lower-precedence ones. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 3 | Documentation sync: updated `docs/FEATURE_REQUESTS.md` and `docs/INTEGRATION.md` with `use-when` cluster status and latest conformance baseline. | `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `static` cluster: **49/49 passing, 0 failed, 0 skipped** ✅
- [x] `use-when` cluster: **99/99 runnable passing, 0 failed, 3 skipped** ✅
- [x] Full W3C XSLT 3.0 suite: **4,640 passed / 611 failed / 9,349 skipped** (88.4%; +2 passed / −2 failed vs. previous run)

## Remaining `use-when` / `static` Failures

None — both clusters are fully green for runnable tests.

## Next Recommended Work

1. Push the current commit to `origin/main`.
2. Continue with remaining quick-win clusters (e.g. `whitespace`, `unparsed-text`, `lre`, `xml-to-json`, single-failure clusters).
