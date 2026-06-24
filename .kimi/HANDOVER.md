# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-24

## Commit

`1510f3f`

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Housekeeping: added REQ-033 for `format-date-en` cluster to `docs/FEATURE_REQUESTS.md` and bumped "Last updated" date | `docs/FEATURE_REQUESTS.md` | Done |
| 2 | Housekeeping: removed untracked scratch files (`merge_fails*.txt`, `mode_fails*.txt`, leftover `tmpdebug/*.xsl`) | — | Done |
| 3 | Parse stylesheets with DTD processing enabled in the conformance harness so external entity definitions resolve (fixes `copy-1201` / `copy-1202`) | `tests/Bosak.Xslt.Conformance/Program.cs` | Done |
| 4 | Correct default `use-accumulators` for an undeclared initial mode from `#all` to empty list per XSLT 3.0 spec (fixes `copy-3002` XTDE3362) | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 5 | Distinguish named-template entry points so the source tree is treated as the global context item, not the initial match selection, preserving `mode-1511`–`mode-1514` | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (883 tests across 8 projects — 0 failures)
- [x] XSLT `copy` cluster: **128/128 runnable passing** ✅ (100%; was 125/128)
- [x] XSLT `mode` cluster: **117/117 runnable passing** ✅ (100%; restored after accumulator refinement)
- [x] Full W3C XSLT 3.0 suite: **4,490 / 778 / 9,332 (85.2%)** — up from 4,487/781

## Next Recommended Work

1. Commit the copy-cluster fixes and updated documentation.
2. Continue clearing remaining small failure clusters (e.g. `evaluate`, `function`, `xpath-default-namespace` each have 1 failure).
