# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-19

## What Was Done

- **QT3 Tier-2z `prod-NamedFunctionRef` / `named-function-ref-reserved-function-names` is complete.**
  - Added `XPST0003` validation in `XPathParser.ParseNamedFunctionRef` for unprefixed reserved function names (e.g., `attribute#0`, `element#0`, `node#0`, `switch#0`).
  - Removed the reserved-name check from `ParseFunctionCall` because reserved names like `attribute()` are valid as kind tests / node tests.
  - Added `NamedFunctionRef_ReservedName_RaisesXPST0003` `[Theory]` tests covering all reserved function names.
  - Removed the two function-call tests that incorrectly assumed reserved names would raise `XPST0003`.
  - Targeted `prod-NamedFunctionRef` pool now **546 passed / 0 failed / 10 skipped** (was 534/12/10).
- **Updated QT3 baselines:** full suite **21,597 passed / 343 failed / 9,881 skipped (67.79%)**; runnable pass rate **98.44%**. Unit tests **1,339/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change histories** for `XPathParser.cs` and `ParserTests.cs`.
- **Build:** `dotnet build Bosak.sln` — 0 errors, 1 warning (pre-existing `XdmSequence.FromSource` nullability in `FunctionLibrary.cs`).

## Next Session Focus

**QT3 Tier-3a: `cbcl-*` scattered clusters** (~8+ failures in the full QT3 suite; pick the largest individual cluster first).

## Remaining Tier-2 Pools

- `RangeExpr` BigInteger cases (12 — known limitation)

## Notes

- Full QT3 run ~5 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
- All changes from this session are committed and pushed as `33dfc94`.
