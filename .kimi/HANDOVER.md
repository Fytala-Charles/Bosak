# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-15 (night)

## What Was Done

- **Tier-1 QT3 harness cluster: 20,294 → 20,684 passed (+390), failures 1,985 → 1,361 (−624).**
  Now **20,684 / 1,361 / 9,776 (65.0%)**. 641 fixed; 17 new failures, all genuine
  newly-exposed engine gaps (documented in AGENT_HANDOVER).
- assert-count: harness read nonexistent `count` attribute → reads element text (189 skips→0).
- `<source role="$var">` binds docs to variables — cleared generalexpression cluster (77).
- XQuery detection: string/comment stripping + constructors/switch/try/FLWOR patterns;
  **parse-error exemption** — XPST0003/`*`-expecting tests still run (52 passes preserved),
  XQTY/XQST-expecting tests skip (avoids ~300 false failures). Lesson: wildcard
  `<error code="*">` needed ParseException handling in CompareError.
- assert-permutation implemented (multiset DeepEqual); `<assert>` uses EBV.
- Unit tests 1,010/0 (harness-only changes).

## Next Session Pointers

- Canonical state: `docs/AGENT_HANDOVER.md` (top section).
- Tier 2 order: (1) OverflowException→FOAR0002 (~90); (2) external `<param select>`
  binding (460 — biggest skip pool); (3) function-item registry (function-lookup 37,
  function-literal 26, map:find#2); (4) `?` lookup operator (~65); (5) xml-to-json
  options (43); (6) serialize-xml (37).
- Newly-exposed gaps (17): distinct-values coercion, innermost/outermost namespace axis,
  transform result-docs, `?` lookup, json-to-xml keys, map:merge duplicates, serialize
  param types.
- Probe trick: replicate harness heuristics in tmp/testrd to enumerate affected tests
  without a full run. Watch CRLF in comm diffs (sed -i 's/\r$//').
- Full QT3 ~6-7 min background (timeout 900). Exit code 2 = has failures (normal).
