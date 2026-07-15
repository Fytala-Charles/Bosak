# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-14

## What Was Done

- **unicode-90 conformance set enabled: 1,365 passed / 0 failed / 95 skipped** (1,460 tests;
  all skips are documented upstream test/data defects). Full W3C XSLT 3.0 suite:
  **7,109 / 0 / 7,491** — 100% of runnable tests.
- New `XsdCharClasses` engine + generated `UnicodeData90` (pinned Unicode 9.0.0): all 38
  categories incl. LC, `\p{IsBlock}`, `\d\w\s\i\c` + complements, ranges/negation/union/
  subtraction `[A-[B]]`; astral ranges as surrogate-pair alternations; `\w` = `[^\p{P}\p{Z}\p{C}]`.
- Regex translation + compiled-Regex caches keyed by original pattern. **Compiled only** —
  NonBacktracking silently mis-matched U+000A on big alternations (suite found it).
- fn:codepoints-to-string = exact XML 1.0 Char production; fn:translate Rune-based;
  fn:concat registered to arity 32; VmEngine CompareGeneral integer-HashSet fast path.
- Harness: charclass param injection for Gen tests, 54MB doc cache, drop empty-@c entries
  (U+FFFE/U+FFFF placeholders), skips + real reasons.
- Unit tests: 961/0 across 8 projects (Standard 326).

## Next Session Pointers

- Canonical state: `docs/AGENT_HANDOVER.md` (top section).
- Skip pools: error test-set (~385), import-schema (~185), streaming, principal
  xsl:package/use-package.
- QT3 XPath conformance ~59% — separate frontier.
- Latent bugs: EscapeUriAttribute astral chars; raw-XPath @select sites (~2421/2459/2587);
  VM regex opcodes MatchesRegex/ReplaceRegex/TokenizeRegex bypass XSD translation (dead code).
- Debug runner: `tmp/hofdbg` (never commit tmp/; tmp/, tmpdebug/, mult/ now gitignored).
- Conformance runs: rebuild Release first; log to file; ~2h for unicode-90 alone.
