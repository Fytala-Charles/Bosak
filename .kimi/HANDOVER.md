# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-05

## Commit

`cd67433` — `collations` cluster cleared and pushed to `origin/main`.

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Made `EvaluationContext.DefaultCollation` flow through all collation-aware standard functions: `fn:default-collation`, `fn:compare`, `fn:contains`, `fn:starts-with`, `fn:ends-with`, `fn:substring-before`, `fn:substring-after`, `fn:index-of`, `fn:distinct-values`, `fn:min`, `fn:max`, and `fn:deep-equal`. | `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` | Done |
| 2 | Propagated the effective default collation into XSLT instruction execution, template bodies, `xsl:if`/`xsl:choose` tests, `xsl:for-each-group`, `xsl:key` building/lookup, and `xsl:sort`. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 3 | Added per-key-name collation tracking to `KeyIndex` and `KeyDefinition`, with XTSE1220 detection when the same expanded key name uses conflicting effective collations. | `src/Bosak.Xslt/Runtime/KeyIndex.cs`, `src/Bosak.Xslt/Stylesheet/KeyDefinition.cs` | Done |
| 4 | Fixed `xsl:sort` `case-order="upper-first"`/`"lower-first"` to act as a tie-breaker after the primary collation comparison, clearing UCA secondary-strength sort tests. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 5 | Updated the W3C conformance harness to read the environment `<collation>` URI and set `EvaluationContext.DefaultCollation`. | `tests/Bosak.Xslt.Conformance/Program.cs` | Done |
| 6 | Documentation sync: updated `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, and `docs/AGENT_HANDOVER.md` with the new `collations` cluster status and conformance baseline. | `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `docs/AGENT_HANDOVER.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (913 tests across 8 projects — 0 failures)
- [x] `collations` cluster: **43/43 passing, 0 runnable failures, 0 skipped** ✅
- [x] `iterate` cluster remains **44/44 passing** ✅
- [x] Full W3C XSLT 3.0 suite: **5,097 passed / 153 failed / 9,350 skipped** (97.1%; +24/−24 vs. previous run)

## Remaining Active Clusters

| Cluster | Failures | Notes |
|---------|----------|-------|
| `xml-version` | 23 | XML 1.1 and version-specific parsing |
| `tunnel` | 22 | Tunnel parameter propagation |
| `normalize-unicode` | 14 | Unicode normalization forms |
| `version` | 13 | XSLT version handling |
| `backwards` | 13 | Backwards-compatibility mode |
| `avt` | 10 | Attribute value templates |
| `xpath-compat` | 9 | XPath 1.0 compatibility |
| `seqtor` | 8 | Sequence constructor edge cases |

## Next Recommended Work

1. Tackle the largest remaining clusters from the 153-failure baseline:
   - `xml-version` (23) and `tunnel` (22) are the biggest blocks.
   - `normalize-unicode` (14) may depend on adding ICU/normalization support.
   - Quick wins: `avt` (10) and `seqtor` (8).
