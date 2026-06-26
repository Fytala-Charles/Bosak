# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`TBD` — Cleared the entire XSLT `date` conformance cluster: `date-094/095` constructor/serialization fixes, `format-date`/`format-date-en` picture-string fixes, and `adjust-dateTime-to-timezone` timezone preservation; full suite 4,695/556/9,349 (89.4%)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | `TransformEngine` now evaluates `_select` AVTs on `xsl:value-of`, enabling static-parameter substitution stylesheets (e.g. `date-094`/`date-095`). | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 2 | `FormatDateTimeEngine` fixes: escaped brackets `[[`/`]]`, component default widths, roman/alphabetic presentations, non-BMP digit families, ISO week-of-month across year boundaries, and `[Z]`/`[z]` timezone semantics. | `src/Bosak.XPath.Standard/Functions/FormatDateTimeEngine.cs` | Done |
| 3 | `adjust-dateTime-to-timezone` now preserves the target timezone offset instead of returning a zero-offset value. | `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` | Done |
| 4 | Documentation sync: updated `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, and `docs/INTEGRATION.md` with the cleared `date` cluster status and latest conformance baseline. | `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `date` cluster: **200/211 passing, 0 runnable failures, 11 skipped** ✅ (was 154/46)
- [x] Full W3C XSLT 3.0 suite: **4,695 passed / 556 failed / 9,349 skipped** (89.4%; +48 passed / −48 failed vs. previous run)

## Remaining Active Clusters

| Cluster | Failures | Notes |
|---------|----------|-------|
| `namespace` | 22 | deferred |
| `maps` | 36 | deferred |

## Next Recommended Work

1. Push the current commit to `origin/main`.
2. Tackle the `namespace` (22 failures) or `maps` (36 failures) clusters, or pick off additional quick wins.
