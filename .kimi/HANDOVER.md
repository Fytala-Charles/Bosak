# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`18f53bd` — Cleared the remaining single-failure clusters (`attribute-0601`, `system-property-022`, `unparsed-text-lines-004`, `regex-026`); `call-template-0201` was already passing; full suite 4,647/604/9,349 (88.5%)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | `RegexHelper.TranslateDot`: `.` now matches Unicode code points (including surrogate pairs) instead of .NET 16-bit code units. Fixes `regex-026`. | `src/Bosak.XPath.Standard/Functions/RegexHelper.cs` | Done |
| 2 | `TransformEngine.RemoveXsltContextFunctions` now unregisters `fn:system-property#1` inside `xsl:evaluate`, so calling it in a dynamically evaluated expression raises `XTDE3160`. Fixes `system-property-022`. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 3 | `TransformEngine.GetErrorCode` now recognizes bare 8-character error codes (e.g. `FOUT1190`) with no colon, so `xsl:catch errors="*:FOUT1190"` matches. Fixes `unparsed-text-lines-004`. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 4 | `TransformEngine.CopyLiteralElement` now copies all in-scope namespace declarations from the stylesheet to root-level literal result elements (except excluded prefixes and the XSLT namespace). Fixes `attribute-0601`. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 5 | Documentation sync: updated `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, and `docs/INTEGRATION.md` with cleared cluster status and latest conformance baseline. | `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `call-template` cluster: **38/38 runnable passing, 0 failed, 6 skipped** ✅
- [x] `attribute` cluster: **67/67 runnable passing, 0 failed, 58 skipped** ✅ (was 66/1)
- [x] `system-property` cluster: **15/15 runnable passing, 0 failed, 12 skipped** ✅ (was 14/1)
- [x] `unparsed-text-lines` cluster: **6/6 passing, 0 failed, 0 skipped** ✅ (was 5/1)
- [x] `regex` cluster: **47/47 runnable passing, 0 failed, 2,115 skipped** ✅ (was 46/1)
- [x] Full W3C XSLT 3.0 suite: **4,647 passed / 604 failed / 9,349 skipped** (88.5%; +5 passed / −5 failed vs. previous run)

## Remaining Single-Failure Clusters

All listed single-failure targets are now clear for runnable tests.

## Next Recommended Work

1. Push the current commit to `origin/main`.
2. Tackle larger remaining clusters (e.g. `date` 32 failures, `namespace` 22 failures, `maps` 36 failures) or pick off additional quick wins.
