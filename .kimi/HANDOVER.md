# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-25

## Commit

`b807e32`

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Cleared the `on-empty` and `on-non-empty` conformance clusters by rewriting sequence-constructor evaluation as an item-based pipeline with deferred conditional instruction processing. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 2 | XPath parser fix: keyword tokens (`for`, `in`, `return`, etc.) can now act as names in variable-binding contexts. | `src/Bosak.XPath.Parser/Ast/XPathParser.cs` | Done |
| 3 | Static validation: `xsl:on-empty` must be the last significant child of its sequence constructor (XTSE0010). | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 4 | Conformance harness: assertions can now use `$result/child::...` because the harness binds `$result` to a document node. | `tests/Bosak.Xslt.Conformance/Program.cs` | Done |
| 5 | Updated agent handover, integration guide, and feature request registry. | `docs/AGENT_HANDOVER.md`, `docs/INTEGRATION.md`, `docs/FEATURE_REQUESTS.md` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] Full W3C XSLT 3.0 suite: **4,666 passed / 585 failed / 9,349 skipped** (~88.9%)
- [x] `on-empty` cluster: **72/72 passing** ✅
- [x] `on-non-empty` cluster: **14/14 passing** ✅

## Next Recommended Work

Top remaining XSLT conformance clusters by failure count:

| Cluster | Failed | Runnable | Notes |
|---------|--------|----------|-------|
| `xml-version` | 27 | 42 | XML version serialization / parsing |
| `use-when` | 26 | 99 | Static evaluation of `use-when` expressions |
| `available-system-properties` | 26 | 26 | System-property availability and values |
| `namespace-alias` | 25 | — | Namespace alias transformation |
| `iterate` | 25 | — | `xsl:iterate` / `xsl:break` |
| `collations` | 25 | — | Collation URI handling |
| `tunnel` | 22 | 58 | Tunnel parameter propagation |
| `static` | 22 | 49 | Static errors / `xsl:static-error` |
| `try` | 21 | 35 | Many failures are static errors caught dynamically due to lazy compilation inside `xsl:try` |

Recommended pick: **`use-when`** (static-evaluation infrastructure with broad payoff) or **`try`** (error-handling correctness).
