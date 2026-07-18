# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-18

## What Was Done

- **QT3 Tier-2s `fn:function-lookup` support is complete.**
  - `function-lookup` now captures the creation focus in the returned `NamedFunctionItem`.
  - Compiler-generated named function references also capture the focus.
  - `InvokeFunctionItemCore` uses the captured focus for context-dependent functions (`fn:base-uri#0`, `fn:document-uri#0`), fixing `fn-function-lookup-018` and `fn-function-lookup-022`.
  - Declared `fn-load-xquery-module` unsupported in `DependencyFilter`, correctly skipping tests that assert the feature (e.g. `fn-function-lookup-760`).
  - Added `FunctionLibraryTests.FunctionLookup_ContextDependentBaseUri` unit test.
- **Updated QT3 baselines:** full suite **21,494 passed / 446 failed / 9,881 skipped (67.55%)**; runnable pass rate **97.97%**. `fn-function-lookup` test set now **660/0/14**.
- **Unit tests:** all passing **1,283/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change histories** for all modified source files.
- **Build:** `dotnet build Bosak.sln` — 0 warnings, 0 errors (1 pre-existing nullable warning).

## Next Session Focus

**QT3 Tier-2t: `fn-id` / `fn-idref` with DTD** (27 tests).
Likely touches the `fn:id` and `fn:element-with-id` implementations in `Bosak.XPath.Standard/Functions/FunctionLibrary.cs` and the DTD-aware document parsing in the providers.

## Remaining Tier-2 Pools (after 2t)

- `xs-numeric` (10)
- `K-NumericIntegerDivide` (9)
- `cbcl-*` (8)
- `K2-SeqIDFunc` (6)
- `K2-NumericMod` (6)
- `K-SeqIndexOfFunc` (6)

## Notes

- Full QT3 run ~5-6 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
