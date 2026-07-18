# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-18

## What Was Done

- **QT3 Tier-2r `fn:collection` / `fn:uri-collection` support is complete.**
  - Added `EvaluationContext.Collections` dictionary populated by the QT3 harness.
  - Implemented `<collection>` parsing in `TestEnvironment`.
  - Rewrote `Collection_0/1` and `UriCollection_0/1` to use `ResolveCollection` with registered collection lookup, directory fallback, and FODC error codes.
  - Restored `XdmMap` insertion-order iteration via `_keyOrder` / `_keyIndices` helpers.
  - Updated `Collection_EmptyArg` unit-test expectation to expect FODC error.
- **Committed and pushed to origin/main:** commit `7c44257`.
- **Updated documentation:** `docs/AGENT_HANDOVER.md` (commit hash), `.kimi/HANDOVER.md`.
- **Build:** `dotnet build Bosak.sln` — 0 warnings, 0 errors.
- **Unit tests:** all passing (1,282/0).

## Current QT3 Status

Full QT3 suite: **21,511 passed / 482 failed / 9,828 skipped (67.60%)**; runnable pass rate **97.81%**.

## Next Session Focus

**QT3 Tier-2s: `fn:function-lookup`** (7 tests).
Likely touches the function library and dynamic function-call dispatch path in `Bosak.XPath.Standard` / `Bosak.XPath.Runtime`.

## Remaining Tier-2 Pools (after 2s)

- `fn-load-xquery-module` (31)
- `fn-id` / `fn-idref` with DTD (27)
- `xs-numeric` (10)
- `K-NumericIntegerDivide` (9)
- `cbcl-*` (8)
- `K2-SeqIDFunc` (6)
- `K2-NumericMod` (6)
- `K-SeqIndexOfFunc` (6)

## Notes

- Full QT3 run ~5-6 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
