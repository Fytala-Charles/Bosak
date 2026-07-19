# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-19

## What Was Done

- **QT3 Tier-2z `fn/contains` collation/whitespace cluster is complete.**
  - Fixed UCA collation strength mapping in `FunctionLibrary.TryParseUca`: `primary` ignores case and non-space accents, `secondary` ignores only case, and `tertiary`/`quaternary` use no ignore flags.
  - Implemented true ASCII-only case folding for the HTML ASCII case-insensitive collation (`http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive`), so only `A-Z`/`a-z` are folded; non-ASCII characters such as `ô`/`Ô` are compared exactly.
  - `fn:contains-token` now tokenizes on XPath whitespace only (`#x20`, `#x9`, `#xD`, `#xA`); non-breaking space (`U+00A0`) is no longer treated as a token separator.
  - `fn:substring-after` now delegates its non-UCA search to `StringIndexOf`, so HTML-ASCII collation is honored consistently.
  - Targeted `fn-contains` and `fn-contains-token` pools now **0 failed** (6 previously failing tests now pass).
- **Updated QT3 baselines:** full suite **21,618 passed / 319 failed / 9,884 skipped (67.93%)**; runnable pass rate **98.54%**. Unit tests **1,343/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change history** for `FunctionLibrary.cs`.
- **Build:** `dotnet build Bosak.sln` — 0 errors, 1 warning (pre-existing `XdmSequence.FromSource` nullability in `FunctionLibrary.cs`).
- **Tests:** `dotnet test Bosak.sln --configuration Release --no-build` — all 1,343 unit tests passed.

## Next Session Focus

**QT3 Tier-2z: `op/numeric-less-than`** (2 failures). Alternate: `RangeExpr` BigInteger cases (12 — known limitation), `cbcl-*` residual clusters.

## Remaining Tier-2 Pools

- `op/numeric-less-than` (2)
- `RangeExpr` BigInteger cases (12 — known limitation)
- `cbcl-*` residual clusters

## Notes

- Full QT3 run ~5 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
- All changes from this session are committed and pushed as `c3c76a2` (code) and the following docs commit.
