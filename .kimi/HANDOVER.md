# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-15

## What Was Done

- **Committed yesterday's interrupted unicode-90 work** (`67a0a3d` + hash doc `c77e1d2`).
- **QT3 regex/string quick-wins cluster: +216 passed, −198 failed, zero regressions.**
  QT3 now **18,698 / 1,742 / 11,381 (58.76%)** (was 18,482/1,940/11,399).
- Strict XSD regex validation (~124 re00xxx tests): malformed quantifiers, bare `{`/`}`/`]`,
  non-`(?:` `(?x` constructs, octal/nonexistent backrefs, .NET-only escapes, trailing `\`,
  empty classes, unescaped `[` in classes, empty-base subtraction → FORX0002.
- Backrefs per F&O 5.6.1.4: digit-gobbling bounded by opened groups; unclosed-group
  reference → FORX0002 (FO.E24).
- `.` excludes #xD; `\S` fixed (unsorted `\s` literal broke Complement); flag `x` strips
  whitespace pre-translation; multiline `^` = `^(?!(?<=\n)\z)` (no match after trailing
  newline, still matches at 0 of "").
- fn:tokenize slices between Matches (no capture interleaving); 1-arg tokenize +
  normalize-space use XPath whitespace only (NBSP kept).
- XPTY0004 for bad args: translate/matches/normalize-unicode (RequireStringRequired).
- normalize-unicode: case-insensitive trimmed forms, "" = no-op, FULLY-NORMALIZED impl.
- QT3 harness `DocumentedSkips` (upstream defects + platform limitations, with reasons).
- Unit tests: **999/0** (Standard 364, +38 new).

## Next Session Pointers

- Canonical state: `docs/AGENT_HANDOVER.md` (top section).
- QT3 clusters by size: fn:transform (61), fn:unparsed-text (54), fn:parse (32),
  fn:load-xquery-module (31), fn:function-lookup (29), serialize (17), op/xs-numeric (22),
  map:find (10).
- Caseless 'i' flag: needs pinned CaseFolding-9.0.0 equivalence classes (Kelvin U+212A!);
  fold literals/class members incl. negation+subtraction, NOT `\p{}` escapes. caselessmatch12-14.
- Skip-drift mystery: skips 9,951 (June) → 11,381 (now); runner unchanged. Inventory skip
  reasons (NotSupportedException catch-all hides engine regressions as skips).
- Full QT3 run: ~5-7 min; log to tmp/ (gitignored).
