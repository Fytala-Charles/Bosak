# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-15 (night → morning)

## What Was Done

- **Tier-2a QT3: 20,684 → 21,081 passed (+397), skips 9,776 → 9,286 (−490), zero regressions.**
  Now **21,081 / 1,454 / 9,286 (66.25%)**. The 93 new failures are all previously-skipped
  tests exposing genuine engine gaps (verified: −490 skips = 397 pass + 93 fail exactly).
- **T2.1 OverflowException→FOAR0002** (~90 skips → 4): `VmEngine.Execute` wraps ExecuteBlock
  in try/catch. No constant folding exists, so one wrap point suffices.
- **T2.2 External `<param select>` binding** (460 skips → 0): deleted skip block in
  ConformanceRunner; `TestExecutor.BindExternalParameters` evaluates select via
  XPath31Expression on post-ApplyTo ctx (can use `$var` sources), binds via WithVariable
  (prefix stripped — engine vars are local-name keyed); empty select → unbind;
  bind failure → skip.
- Unit tests 1,010/0.

## Tier-2a-exposed gaps (93, documented in AGENT_HANDOVER)

format-date/time picture+locale (~50); BigInteger ranges (12 — deferred, major change);
duration arith FODT0002/±INF (8); cbcl-castable out-of-range (8); collection/fn-doc invalid
URIs → IOException instead of FODC000x (11); fn-transform XSLT gaps (13); misc (3).

## Next Session Pointers

- Canonical state: `docs/AGENT_HANDOVER.md` (top section).
- Tier 2 remaining: (3) function-item registry (function-lookup 37, function-literal 26,
  map:find#2 — root cause found: wrong ParameterTypes in registry for dynamic calls, e.g.
  fn:not=[Boolean]→item()*, *-from-duration=[String]→[Duration], map:* key=[String]→any;
  empty-seq () passed to String params needs pass-through in ConvertArgToKind;
  fn:load-xquery-module stub registration for exists() tests); (4) `?` lookup operator
  (~65); (5) xml-to-json options (43); (6) serialize-xml (37).
- Static calls bypass ParameterTypes (VmEngine.cs:184) — registry kind fixes only affect
  dynamic invocation; safe to correct kinds to spec values.
- Probe trick: replicate harness heuristics in tmp/testrd to enumerate affected tests
  without a full run. Watch CRLF in comm diffs (sed -i 's/\r$//').
- Full QT3 ~5-6 min background (timeout 900). Exit code 2 = has failures (normal).
