# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`49a562a` — Static cluster cleared (49/49) + use-attribute-sets quick win; full suite 4,638/613/9,349 (88.3%)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | External static parameter wiring: added `XsltCompiler.StaticParameters` and threaded caller-supplied values through `Stylesheet` so they override defaults during `BuildStaticContext()`. | `src/Bosak.Xslt/Api/XsltCompiler.cs`, `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 2 | Static value runtime binding: eagerly bind static variables/parameters from `Stylesheet.StaticVariables` before lazy non-static globals, so static values remain available even when a non-static declaration shadows the name. | `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 3 | XTSE3450 conflict detection: static variable vs static parameter with the same expanded name now raises XTSE3450 across import precedence; same-kind declarations at different precedences override. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 4 | XTSE0090 validations: reject `static="yes"` on non-global `xsl:param`/`xsl:variable`, and reject `visibility` on any static declaration. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 5 | Implicit empty-sequence defaults: required static parameters without a value default to undefined (XTDE0050 at runtime); optional declarations default to empty sequence. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 6 | Static `@as` type coercion: `ProcessStaticVariable` validates computed values against `@as` using `ConvertVariableValue` with XTTE0590 for parameters. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs`, `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Done |
| 7 | Conformance harness static parameters: evaluates `<param static="yes">` and passes values to `XsltCompiler.StaticParameters` instead of only substituting into `_select`. | `tests/Bosak.Xslt.Conformance/Program.cs` | Done |
| 8 | General comparison empty-sequence fix: `VmEngine.CompareGeneral` now returns `false` (not empty sequence) when one operand is empty, per XPath 3.1 §17.3. | `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` | Done |
| 9 | Namespace-axis coverage for implied namespaces: `XDocumentNode.GetNamespaceAxis` now includes namespaces implied by the element name (e.g. `json-to-xml` output with no explicit `xmlns` attribute on every element). | `src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs` | Done |
| 10 | Quick win: allow `xsl:use-attribute-sets` on literal result elements (XTSE0805 whitelist). Clears `attribute-set`, `xsl-document`, `analyze-string`, `next-match`, and `mode-1402`. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `static` cluster: **49/49 passing, 0 failed, 0 skipped** ✅
- [x] Full W3C XSLT 3.0 suite: **4,638 passed / 613 failed / 9,349 skipped** (88.3%; +39 passed / −39 failed vs. previous run)

## Remaining `static` Cluster Failures

None — cluster is fully green.

## Next Recommended Work

1. Commit and push the use-attribute-sets quick win.
2. Continue with remaining quick-win clusters (e.g. `whitespace`, `unparsed-text`, `lre`, `xml-to-json`, single-failure clusters).
