# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-18

## What Was Done

- **QT3 Tier-2t `fn:id` / `fn:idref` / `fn:element-with-id` DTD support is complete.**
  - Added DTD properties to `IXdmNode` (`HasDocumentType`, `DocumentTypeName`, `PublicId`, `SystemId`, `InternalSubset`) and implemented them on `XDocumentNode` via `XDocument.DocumentType`.
  - `FunctionLibrary` parses the DTD internal subset for `ID`/`IDREF`/`IDREFS` attribute declarations and caches the result per document node.
  - `fn:id`, `fn:idref`, and `fn:element-with-id` now match DTD-declared ID/IDREF attributes in addition to `id`/`xml:id`.
  - `fn:idref` returns the matching attribute node(s) per F+O.
  - Sequence arguments like `("id1", "id2")` are tokenized correctly via a new `ParseIdTokens(XdmValue)` overload.
  - All six arities now raise `XPTY0004` when the context item (one-argument forms) or second argument (two-argument forms) is not a node.
  - Added seven unit tests covering DTD-declared attributes, type-check errors, and DTD parser behavior.
- **Updated QT3 baselines:** full suite **21,535 passed / 405 failed / 9,881 skipped (67.68%)**; runnable pass rate **98.15%**. Targeted `fn-id`/`fn-idref` pool **54/0/61**.
- **Unit tests:** all passing **1,147/0**.
- **Updated documentation:** `docs/AGENT_HANDOVER.md`, `docs/FEATURE_REQUESTS.md`, `docs/INTEGRATION.md`, `docs/ARCHITECTURE.md`, `README.md`, `.kimi/HANDOVER.md`.
- **Updated file change histories** for all modified source files.
- **Build:** `dotnet build Bosak.sln` — 0 warnings, 0 errors.

## Next Session Focus

**QT3 Tier-2u: `xs-numeric`** (10 failures).
`xs:numeric` is a union type in XSD; Bosak currently lacks the `xs:numeric#1` constructor and casts to `xs:numeric`, causing failures in `xs-numeric-007` through `xs-numeric-018`. Likely touches `FunctionLibrary` constructor/cast handling and possibly the `XsBuiltInTypes` registry.

## Remaining Tier-2 Pools (after 2u)

- `K-NumericIntegerDivide` (9)
- `fn-has-children` (8)
- `K2-NumericMod` (6)
- `K-SeqIndexOfFunc` (6)
- `cbcl-*` scattered clusters (~8+)
- `named-function-ref-reserved-function-names` (12 — newly surfaced)
- `RangeExpr` BigInteger cases (12 — known limitation)

## Notes

- Full QT3 run ~5 min background (timeout 900). Exit code 2 = has failures (normal).
- Canonical state is in `docs/AGENT_HANDOVER.md`.
