# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`04e9f0f` — Clear use-when conformance cluster and add static-expression infrastructure  
`TBD` — Static cluster progress (in progress, not yet committed)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Committed use-when cluster clearance + static-expression infrastructure. | Multiple | Done |
| 2 | Fixed `_select` handling for global variables/parameters at runtime. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 3 | Made static attribute value validation case-sensitive; added `tunnel`/`visibility`/`required+select` validations for static variables/parameters. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 4 | Allowed static declarations without `select` to default to empty sequence (or undefined for required parameters). | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `use-when` cluster: **99/102 passing, 0 failed, 3 skipped** ✅
- [x] `type` cluster: **58/79 runnable passing, 0 failed, 21 skipped** ✅
- [ ] `static` cluster: **40/49 passing, 9 failed** (was 23/49 after use-when commit)
- [ ] Full W3C XSLT 3.0 suite: **4,655 passed / 596 failed / 9,349 skipped** (88.6%)

## Remaining `static` Cluster Failures

| Test | Expected | Notes |
|---|---|---|
| static-003a | supplied static param avoids forward-ref error | External static params not wired into stylesheet loading |
| static-011 | `true--false` | Empty-sequence general comparison in TVT |
| static-013c | XTTE0590 | External static param type validation |
| static-020 | XTSE3450 | Import-precedence static conflict detection |
| static-023 | XTSE3450 | Variable vs param static conflict detection |
| static-025/026 | complex assertions | Package/streaming/static-error tests |
| static-027 | `p=0`, `q=11` | Static variable shadowed by non-static variable |
| static-030 | complex assertion | Namespace nodes from static variable |

## Next Recommended Work

1. Fix import-precedence conflict detection for static variables (XTSE3450).
2. Wire external static parameters from the conformance harness into stylesheet loading.
3. Resolve remaining edge cases (static-011 comparison, static-027 shadowing, static-030 namespace nodes).
