# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-09

## Commit

`<uncommitted>`

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Match cluster 100% runnable — compile-time predicate validation catches XPST0017 | `PatternCompiler.cs`, `TransformEngine.cs` | Done |
| 2 | `apply-templates` default-mode resolution fix | `TransformEngine.cs` | Done |
| 3 | Initial mode existence validation (XTDE0045) | `TransformEngine.cs` | Done |
| 4 | Required parameter validation (XTDE0050) | `TransformEngine.cs` | Done |
| 5 | Conformance harness reads `<param>` inside `<initial-mode>` | `Program.cs` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (873 tests across 8 projects — 0 failures)
- [x] XSLT match cluster: **179/294 (100% of runnable)** ✅ — 0 failures, 115 skipped
- [x] XSLT mode cluster: 102/42/44 (70.8% of runnable)
- [x] XSLT full suite: 3,555 / 1,854 / 9,191 (65.7%)
- [x] QT3 baseline: 19,041 / 2,829 / 9,951 (59.84%) — stable

## Next Recommended Work

### Immediate quick wins

1. **`seqtor`** (13 failures) — Sequence constructor batching refactor for §5.7.1
2. **`copy`** (80 failures) — `xsl:copy`, `xsl:copy-of` behavior gaps
3. **`for-each-group`** (62 failures) — Implementation gaps

### Short-term cluster targets

4. **`key`** (34 failures) — `key()` / `xsl:key` behavior (57/99 passing)
5. **`use-when`** (61 failures) — Static evaluation of `use-when` expressions
6. **`as`** (60 failures) — Type declarations/casting

### Medium-term

7. **Sequence constructor batching** — Refactor `ExecuteSequenceConstructorDirect` to accumulate raw items and apply §5.7.1 rules on flush. Would fix seqtor, copy, variable clusters.
