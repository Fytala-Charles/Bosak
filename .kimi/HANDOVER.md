# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`04e9f0f` — Clear use-when conformance cluster and add static-expression infrastructure  
`6e09b23` — Progress static cluster: +17 passes (40/49)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Committed use-when cluster clearance + static-expression infrastructure. | Multiple | Done |
| 2 | Fixed `_select` handling for global variables/parameters at runtime. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 3 | Made static attribute value validation case-sensitive; added `tunnel`/`visibility`/`required+select` validations for static variables/parameters. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 4 | Allowed static declarations without `select` to default to empty sequence (or undefined for required parameters). | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 5 | Added XTSE0090 for non-global `static="yes"` variables/parameters and for `visibility` on static variables/parameters. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 6 | Added XTSE3450 detection when a static variable and static parameter share the same expanded name, regardless of import precedence. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 7 | Reworked static-context merging so child-module `ShadowingNames` no longer incorrectly shadow parent declarations, and higher-precedence declarations override lower-precedence ones without spurious XTSE3450. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `use-when` cluster: **99/102 passing, 0 failed, 3 skipped** ✅
- [x] `type` cluster: **58/79 runnable passing, 0 failed, 21 skipped** ✅
- [x] `static` cluster: **44/49 passing, 5 failed** (was 40/49 at start of session)
- [ ] Full W3C XSLT 3.0 suite: **~4,699 passed / ~552 failed / ~9,349 skipped** (static cluster win reduces failures by 4)

## Remaining `static` Cluster Failures

| Test | Expected | Notes |
|---|---|---|
| static-003a | supplied static param avoids forward-ref error | External static params not wired into stylesheet loading |
| static-011 | `true--false` | Empty-sequence general comparison in TVT produces `true--` instead of `true--false` |
| static-013c | XTTE0590 | External static param type validation |
| static-027 | `p=0`, `q=11` | Static variable shadowed by non-static variable with higher import precedence |
| static-030 | complex assertion | Namespace nodes from static variable (`//namespace::*` returns 5, expected 10) |

## Next Recommended Work

1. Wire external static parameters from the conformance harness into stylesheet loading (fixes static-003a, static-013c).
2. Resolve static-027: static value must remain visible to other static expressions even when a non-static declaration shadows it at runtime.
3. Investigate static-011 expected result and static-030 namespace-axis coverage.
