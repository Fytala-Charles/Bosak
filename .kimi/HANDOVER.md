# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-15 (late evening)

## What Was Done

- **URI-mapping cluster: QT3 18,698 → 20,294 passed (+1,596), skips 11,381 → 9,542, zero regressions.**
  Now **20,294 / 1,985 / 9,542 (63.78%)**. All new failures are previously-skipped tests
  exposing genuine gaps (verified by per-run name-level diffs).
- UriFormatException root cause: `XDocumentProvider.LoadXml` did `new Uri(relativePath)` →
  throws. Now absolutizes; harness Program also absolutizes suite path. 2,106 skips → 0.
- New `EvaluationContext.ResourceUriMapper` (Func<string,string?>): maps published http:
  URIs to local files; consulted by LoadDocument/json-doc/unparsed-text*/fn:transform
  stylesheet-location. Harness parses `<source uri=>`/`<resource uri=>` (existing files only).
- JSON: parse failures → FOJS0001 (incl. STJ surrogate InvalidOperationException); U+FEFF
  stripped; fallback option now handles unpaired \uXXXX (manual unescaper, json-doc-039).
- unparsed-text strict decoding: explicit→FOUT1200, inferred→FOUT1190; unknown encoding
  name→FOUT1200; unparsed-text-available reads+validates local files.
- `#UNDEFINED` static-base-uri sentinel no longer poisons ctx.BaseUri.
- Unit tests 1,010/0 (+11). XSLT smoke green (transform 9/9, json 10/0, analyze-string 53/0).

## Next Session Pointers

- Canonical state: `docs/AGENT_HANDOVER.md` (top section).
- Remaining skip pools: ~90 OverflowException→FOAR0002 (numeric range, engine-wide),
  189 invalid assert-count + 72 assert-permutation (harness assert support),
  460 external-variable binding, 13 harness InvalidOperationException.
- Newly-exposed failure clusters: json-doc options (escape/duplicates semantics, FOJS0005,
  XPTY0004 ~8), map:find#2 function items (2), d1e* assert-xml serialization diffs (3),
  fn:transform XSLT gaps (54), XQuery constructors (Constr-*) failing instead of skipping.
- w3.org rate-limits (403) — tests needing repo-missing files (text-plain-utf-8-invalid.txt)
  are flaky when falling through to network.
- fn-unparsed-text-040: harness assert-string-value newline→space comparator quirk (like
  cbcl-normalizedstring-002b). Deprioritized.
- Background tasks need explicit timeout (default 60s kills 7-min QT3 runs; use 900s).
- Harness exit code 2 = has failures (normal). Full QT3 ~6-7 min.
