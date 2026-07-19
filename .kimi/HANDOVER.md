# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-19

## What Was Done

- **QT3 Tier-2z `op/numeric-less-than` cluster is complete.**
  - `xs:unsignedLong` lexical values that exceed `long.MaxValue` (e.g. `18446744073709551615`) are now represented as `XdmValueKind.Decimal` with the `unsignedLong` subtype annotation, so casts and comparisons work.
  - `VmEngine.TryCast` parses `xs:unsignedLong` strings as `ulong`, storing small values as `Integer` and large values as `Decimal`.
  - `ItemInstanceOf` now accepts decimal-backed values whose schema type is an integer subtype, so `instance of xs:unsignedLong` still matches the overflow representation.
  - Added `XdmValue.FromDecimal(decimal, schemaTypeName)` overload.
  - Targeted `op-numeric-less-than` pool now **154 passed / 0 failed / 29 skipped** (2 previously failing tests now pass).
- **Updated QT3 baselines:** full suite **21,620 passed / 317 failed / 9,884 skipped (67.93%)**; runnable pass rate **98.56%**. Unit tests **1,343/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change histories** for `XdmValue.cs` and `VmEngine.cs`.
- **Build:** `dotnet build Bosak.sln` — 0 errors, 1 warning (pre-existing `XdmSequence.FromSource` nullability in `FunctionLibrary.cs`).
- **Tests:** `dotnet test Bosak.sln --configuration Release --no-build` — all 1,343 unit tests passed.

## Next Session Focus

**QT3 Tier-2z: `RangeExpr` BigInteger cases** (12 failures in the full QT3 suite — known platform limitation because `XdmValue.IntegerValue` is `long`). Alternate: `cbcl-*` residual clusters.

## Remaining Tier-2 Pools

- `RangeExpr` BigInteger cases (12 — known limitation)
- `cbcl-*` residual clusters

## Notes

- Full QT3 run ~5 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
- All changes from this session are committed and pushed as `16484ae` (code) and the following docs commit.
