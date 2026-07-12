# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-12

## Commit

`1812d61` — commit on `main` (not yet pushed).

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Fixed character-map precedence: last map in `use-character-maps` now wins for duplicate characters. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs`, `src/Bosak.Xslt/Stylesheet/OutputProperties.cs` | Done |
| 2 | Applied character maps before XML/HTML escaping and encoding checks; preserved CDATA-section-element split-out characters. | `src/Bosak.Xslt/Runtime/ResultTreeSerializer.cs` | Done |
| 3 | Integrated character maps into JSON string serialization. | `src/Bosak.XPath.Standard/Json/XdmJsonSerializer.cs`, `src/Bosak.Xslt/Runtime/ResultTreeSerializer.cs` | Done |
| 4 | Combined UTF-16 surrogate-pair numeric character references into a single scalar NCR in XML/XHTML output. | `src/Bosak.Xslt/Runtime/ResultTreeSerializer.cs` | Done |
| 5 | Fixed XHTML empty-element handling: no-namespace HTML void elements self-close, non-void empty elements get explicit end tags. | `src/Bosak.Xslt/Runtime/ResultTreeSerializer.cs` | Done |
| 6 | Documentation sync: updated `docs/AGENT_HANDOVER.md`. | `docs/AGENT_HANDOVER.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (940 tests across 8 projects — 0 failures)
- [x] Full W3C XSLT 3.0 suite: **5,526 passed / 79 failed / 8,995 skipped** (98.6%, was 5,521/84/8,995)
- [x] `output` cluster: **16 failures remaining** (was 18)
- [x] `character-map` cluster: **12 failures remaining** (was 15)
- [ ] Known remaining: `maps-017` still fails on JSON output-method semantics.

## Remaining Active Clusters

- `output` (16): mostly error-handling, normalization-form, doctype placement, JSON/adaptive array flattening, and comment-CR edge cases.
- `character-map` (12): high-Unicode surrogate mapping, adaptive method map handling, NFD immune-replacement, and validation-error cases.
- `namespace` (10), `mode` (9), `xml-version` (8), `current-output-uri` (5), `bug` (4), plus a few scattered others.
