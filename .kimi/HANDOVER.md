# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-04

## Commit

`4ebe44b` — context-item changes committed and pushed.

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Added `ContextItemDeclaration` to parse and statically validate `xsl:context-item` (`@use`, `@as`, position, duplicates, disallowed `@select`). | `src/Bosak.Xslt/Stylesheet/ContextItemDeclaration.cs` (new), `src/Bosak.Xslt/Stylesheet/TemplateRule.cs` | Done |
| 2 | Enforce `xsl:context-item` at runtime: `use="absent"`, `use="required"` with `XTTE3090`, `@as` type check with `XTTE0590`, and skip the declaration during sequence-constructor execution. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 3 | Reject `xsl:context-item` inside `xsl:function` with `XTSE0010`. | `src/Bosak.Xslt/Stylesheet/XsltFunctionDefinition.cs` | Done |
| 4 | Strip whitespace text nodes immediately before `xsl:param`/`xsl:sort`/`xsl:context-item` regardless of `xml:space="preserve"` (XSLT 3.0 §4.3). | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 5 | Preserve atomic spacing across `xsl:call-template`/`xsl:apply-templates` boundaries by not restoring `_lastAddedWasAtomic` after `ExecuteTemplate`. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 6 | Documentation sync: updated `docs/AGENT_HANDOVER.md` with cleared `context-item` cluster status and latest conformance baseline. | `docs/AGENT_HANDOVER.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (911 tests across 8 projects — 0 failures)
- [x] `context-item` cluster: **31/31 passing, 0 runnable failures, 0 skipped** ✅
- [x] Full W3C XSLT 3.0 suite: **5,048 passed / 202 failed / 9,350 skipped** (96.2%; +21/−21 vs. previous run)

## Remaining Active Clusters

| Cluster | Failures | Notes |
|---------|----------|-------|
| `iterate` | 25 | deferred |
| `collations` | 25 | deferred |
| `xml-version` | 23 | deferred |
| `tunnel` | 22 | deferred |
| `normalize-unicode` | 14 | deferred |
| `version` | 13 | deferred |
| `backwards` | 13 | deferred |
| `avt` | 10 | deferred |

## Next Recommended Work

1. Tackle the largest remaining clusters, or pick off quick wins from the 202-failure baseline:
   - `iterate` / `collations` / `xml-version` / `tunnel` are the biggest remaining blocks.
   - `avt` (10 failures) may be a smaller quick win.
