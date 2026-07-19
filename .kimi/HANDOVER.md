# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-19

## What Was Done

- **QT3 Tier-2z `fn/matches` caseless-match cluster is complete.**
  - `RegexHelper.ParseRegexFlags` now maps the XPath `i` flag to `RegexOptions.IgnoreCase`.
  - `XsdCharClasses` wraps category escapes `\p{}`/`\P{}` in `(?-i:...)` so .NET does not expand them.
  - Bracketed class expressions are case-folded during translation (single code points and escaped atoms) and ranges are completed by `IgnoreCase` (including special Unicode foldings like U+212A Kelvin sign).
  - Back-references and quote mode now match case-insensitively.
  - Fixed `CaseFoldSet` to emit singleton ranges `[cp, cp]` instead of flattening code points, preventing empty/never-matching character classes.
  - Targeted `fn-matches` pool (including `fn-matches.re`) now **1,117 passed / 0 failed / 58 skipped** (5 previously failing tests now pass).
- **Updated QT3 baselines:** full suite **21,610 passed / 330 failed / 9,881 skipped (67.91%)**; runnable pass rate **98.49%**. Unit tests **1,343/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change histories** for `RegexHelper.cs`, `XsdCharClasses.cs`, `FunctionLibrary.cs`, and `TransformEngine.cs`.
- **Build:** `dotnet build Bosak.sln` — 0 errors, 1 warning (pre-existing `XdmSequence.FromSource` nullability in `FunctionLibrary.cs`).

## Next Session Focus

**QT3 Tier-2z: `fn/format-number`** (5 failures: numberformat63/64 large-number precision, numberformat123 non-ASCII exponent digits, numberformat128 FODF1310, numberformat906InputErr XPTY0004). Alternate: `fn/contains` collation/whitespace cluster (5).

## Remaining Tier-2 Pools

- `op/numeric-less-than` (2)
- `RangeExpr` BigInteger cases (12 — known limitation)
- `fn/contains` collation/whitespace cluster (5)

## Notes

- Full QT3 run ~5 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
- All changes from this session are committed and pushed as `4a5370b` (code) and a follow-up docs update.
