# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-19

## What Was Done

- **QT3 Tier-2z `cbcl-castable` is complete.**
  - Fixed `VmEngine.Castable` to catch dynamic cast errors (`FOCA0003`, `FOAR0002`) and return `false` instead of propagating the error.
  - Fixed empty-sequence handling for `castable as`: `()` is not castable to an exactly-one type but is castable to optional (`?`) and zero-or-more (`*`) types.
  - Added regression tests for empty-sequence castability and overflow castability (`xs:decimal`, `xs:dayTimeDuration`, `xs:yearMonthDuration`).
  - Targeted `prod-CastableExpr` pool now **782 passed / 0 failed / 177 skipped** (was 772/10/177).
- **QT3 Tier-2z `prod-NamedFunctionRef` / `named-function-ref-reserved-function-names` is complete.**
  - Added `XPST0003` validation in `XPathParser.ParseNamedFunctionRef` for reserved function names.
  - Removed the reserved-name check from `ParseFunctionCall` because reserved names are valid as kind tests.
  - Targeted `prod-NamedFunctionRef` pool now **546 passed / 0 failed / 10 skipped** (was 534/12/10).
- **Updated QT3 baselines:** full suite **21,607 passed / 333 failed / 9,881 skipped (67.81%)**; runnable pass rate **98.48%**. Unit tests **1,343/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change histories** for `XPathParser.cs`, `ParserTests.cs`, `VmEngine.cs`, and `FunctionLibraryTests.cs`.
- **Build:** `dotnet build Bosak.sln` — 0 errors, 1 warning (pre-existing `XdmSequence.FromSource` nullability in `FunctionLibrary.cs`).

## Next Session Focus

**QT3 Tier-2z: `fn/format-number`** (5 failures: numberformat63/64 large-number precision, numberformat123 non-ASCII exponent digits, numberformat128 FODF1310, numberformat906InputErr XPTY0004). Alternate: `fn/matches` caseless-match cluster (3 failures).

## Remaining Tier-2 Pools

- `fn/matches` caseless-match (3)
- `op/numeric-less-than` (2)
- `RangeExpr` BigInteger cases (12 — known limitation)

## Notes

- Full QT3 run ~5 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
- All changes from this session are committed and pushed as `fd529e5`.
