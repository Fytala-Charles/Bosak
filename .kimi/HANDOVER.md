# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-09

## Commit

`c640b3d`

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | `apply-templates` default-mode resolution fix — respects `xsl:default-mode` scope | `TransformEngine.cs` | Done |
| 2 | Initial mode existence validation (XTDE0045) — `#all` templates don't count | `TransformEngine.cs` | Done |
| 3 | Required parameter validation (XTDE0050) — `xsl:param required="yes"` | `TransformEngine.cs` | Done |
| 4 | Conformance harness reads `<param>` inside `<initial-mode>` | `Program.cs` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (873 tests across 8 projects — 0 failures)
- [x] XSLT mode cluster: 102/42/44 (70.8% of runnable) — +3 tests (mode-1619, initial-mode-002/003)
- [x] XSLT full suite: 3,554 / 1,855 / 9,191 (65.7%) — +11 tests total
- [x] QT3 baseline: 19,041 / 2,829 / 9,951 (59.84%) — stable

## Next Recommended Work

### Immediate quick wins

1. **`match` cluster remaining** (19 failures) — see docs/AGENT_HANDOVER.md for detailed breakdown
2. **`seqtor`** (13 failures) — Sequence constructor batching refactor for §5.7.1
3. **`mode` cluster remaining** (36 failures) — mostly static validation, accumulators, packages

### Short-term cluster targets

4. **`copy`** (80 failures) — `xsl:copy`, `xsl:copy-of` behavior gaps
5. **`for-each-group`** (62 failures) — Implementation gaps
6. **`key`** (34 failures) — `key()` / `xsl:key` behavior (now 57/99 passing)

### Medium-term

7. **`use-when`** (61 failures) — Static evaluation of `use-when` expressions
8. **`as`** (60 failures) — Type declarations/casting
