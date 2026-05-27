# Handover — Bosak XPath/XSLT Implementation

**Date:** 2026-05-26  
**Commit:** `<latest>` on `main` (uncommitted changes)  
**Current focus:** XSLT engine fixes (keyword names, xsl:mode, atomic for-each)

---

## Project Status Overview

### XSLT Feature Requests (REQ-001 — REQ-012)

| REQ | Feature | Status | Tests Added |
|-----|---------|--------|-------------|
| REQ-001 | `xsl:import` / `xsl:include` URI resolution | ✅ Implemented | 6 |
| REQ-002 | Named XSLT modes (`#all`, `#current`, `#default`) | ✅ Implemented | 6 |
| REQ-003 | `xsl:sort` + `fn:sort` comparator | ✅ Implemented | 4+ |
| REQ-004 | `xsl:number` (single/any/multiple, format, value) | ✅ Implemented | 5 |
| REQ-005 | `xsl:key` + `key()` function | ✅ Implemented | 5 |
| REQ-006 | `xsl:output` serialization | ✅ Implemented | 4 |
| REQ-007 | `fn:sort` mixed-type comparator fix | ✅ Implemented | 2 |
| REQ-008 | `fn:function-lookup` double-to-string precision | ⏳ Pending | — |
| REQ-009 | date/time ordering | ⏳ Pending | — |
| REQ-010 | JSON/XML functions | ⏳ Pending | — |
| REQ-011 | `fn:transform()` | ⏳ Pending | — |
| REQ-012 | tunnel parameters | ✅ Implemented | 4 |

**Phase 2 (sort, key, number) is COMPLETE.**  
**Phase 3 started:** REQ-012 (tunnel parameters) implemented.  
**Customer A blocker resolved:** Global `xsl:variable` / `xsl:param` now collected from main/import/include stylesheets.  
**QT3 fixes:** `fn:substring` rounding fixed (+~10 passes), `fn:replace` replacement string validation fixed (+~5 passes).  
**XSLT conformance runner:** Now supports `<assert>`, `<all-of>`, `<any-of>`, `<assert-eq>` XPath assertion evaluation; loads `assert-xml` from `file` attribute. Full catalog: **7.3% pass rate** (776 / 10,696 non-skipped).  
**Engine fixes:** `xsl:copy` now copies attributes per spec; `xsl:strip-space` / `xsl:preserve-space` parsing and application added; `initial-template` support added to `XsltExecutable`/`TransformEngine`.  
**Keyword variable names:** Parser now accepts `mod`, `div`, `and`, `or` etc. as variable/parameter names (e.g. `$mod`).  
**`xsl:mode` support:** `on-no-match` attribute parsed (`shallow-copy`, `shallow-skip`, `text-only-copy`, `deep-copy`, `fail`); built-in template rules are now mode-aware.  
**Atomic for-each:** `xsl:for-each select="1 to 4"` now iterates over atomic values; `ExecuteXsltInstruction` signature changed to accept `XdmValue` context item.  
**AVT fix:** Attribute Value Templates (`{expr}`) in literal result elements are now evaluated.

### Unit Test Status

- **744 unit tests pass** across 7 test projects (0 failures)
- XSLT-specific tests: **55 tests** in `Bosak.XPath.Xslt.Tests`

### QT3 Conformance Baseline

- **Passed:** ~18,529 / **Failed:** ~3,490 / **Skipped:** 9,802 (31,821 total)
- Pass rate: ~58.2%
- Note: Conformance runner is an `Exe` project, not a test project. Run with `dotnet run --project tests/Bosak.XPath.Conformance`.

---

## Architecture Reminder

### XSLT Execution Pipeline
```
XDocument source → Stylesheet.Load() → TransformEngine.Transform()
  └── Template match compilation (PatternCompiler)
  └── Key index building (KeyIndex.Build) if xsl:key present
  └── key() function registration on EvaluationContext
  └── ApplyTemplates / ExecuteTemplate loop
      └── ExecuteXsltInstruction handles: element, attribute, value-of,
          text, apply-templates, for-each, if, choose, variable, param,
          call-template, copy, copy-of, number, sort
```

### Key Files for XSLT Work

| File | Responsibility |
|------|---------------|
| `src/Bosak.XPath.Xslt/Runtime/TransformEngine.cs` | Main execution engine; add new instruction handlers here |
| `src/Bosak.XPath.Xslt/Stylesheet/Stylesheet.cs` | Parses xsl:stylesheet, resolves imports/includes, collects templates/keys/output/strip-space |
| `src/Bosak.XPath.Xslt/Stylesheet/TemplateRule.cs` | Single template rule with match pattern, modes, priority, import precedence |
| `src/Bosak.XPath.Xslt/Stylesheet/KeyDefinition.cs` | Parsed xsl:key declaration |
| `src/Bosak.XPath.Xslt/Runtime/KeyIndex.cs` | Per-document index for key() lookups; builds via document tree walk |
| `src/Bosak.XPath.Xslt/Patterns/PatternCompiler.cs` | Compiles match patterns (`item`, `@id`, `*`, `node()`, predicates) |
| `src/Bosak.XPath.Xslt/Runtime/ResultTreeSerializer.cs` | Serializes result tree with xsl:output properties |
| `tests/Bosak.XPath.Xslt.Tests/StylesheetTests.cs` | All XSLT unit tests |
| `tests/Bosak.XPath.Xslt.Conformance/Program.cs` | W3C XSLT 3.0 Test Suite runner |

### Standard Library Files (Often Touched)

| File | Responsibility |
|------|---------------|
| `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` | Standard `fn:*`, `map:*`, `array:*`, `math:*` functions |
| `src/Bosak.XPath.Standard/Functions/FormatIntegerEngine.cs` | `fn:format-integer` (now `public`, reused by `xsl:number`) |
| `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` | VM execution, casting, opcode dispatch |
| `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` | Context item, variables, functions, namespaces |
| `src/Bosak.XPath.Core/Xdm/XdmValue.cs` | XDM value representation, formatting, accessors |
| `src/Bosak.XPath.Core/Xdm/XdmValueComparer.cs` | Spec-compliant sort comparator (atomization, type promotion) |

---

## Recent Changes (This Session)

### REQ-005 — `xsl:key` + `key()`
- `KeyIndex.Build()` walks source document, evaluates `use` expressions, populates index
- `TransformEngine.Transform()` builds index and registers `key()` in `fn` namespace
- `key()` supports sequence second argument with deduplication via `HashSet<IXdmNode>`

### REQ-004 — `xsl:number`
- Added `"number"` case to `ExecuteXsltInstruction` with full `ExecuteXsltNumber` helper
- Supports `level="single"` (default), `"any"`, `"multiple"`
- Supports `count`/`from` patterns, `value` XPath expression, `format` attribute
- Format tokenization: prefix/tokens/separators/suffix parsed and passed to `FormatIntegerEngine.Format`
- Document-order tree walk (`WalkDocumentTree`) for `level="any"`

---

## How to Build & Test

```bash
# Build entire solution
dotnet build Bosak.sln

# Run all unit tests
dotnet test Bosak.sln

# Run only XSLT tests
dotnet test tests/Bosak.XPath.Xslt.Tests/Bosak.XPath.Xslt.Tests.csproj

# Run QT3 conformance suite (~5 min, Exe project)
cd tests/Bosak.XPath.Conformance
dotnet run --configuration Release -- "D:/Development/Bosak/tests/qt3tests"

# Run W3C XSLT 3.0 conformance suite (variable test set example)
cd tests/Bosak.XPath.Xslt.Conformance
dotnet run -- "D:/Development/Bosak/tests/xslt30-test/catalog.xml" "variable"
```

---

## Known Issues / Gotchas

1. **Conformance runner locks DLLs** — If a previous conformance run is still running, builds will fail. Kill with `taskkill /F /IM Bosak.XPath.Conformance.exe` before building.
2. **Empty element serialization** — `XmlWriter` outputs `<done />` (with space), not `<done/>`. Tests should use flexible assertions.
3. **`key()` namespace** — Registered under `http://www.w3.org/2005/xpath-functions` (not XSLT namespace) because the XPath compiler resolves unprefixed function names to the `fn` namespace.
4. **PatternCompiler limitations** — Predicates create a new `EvaluationContext` per evaluation; prefix resolution in QNames is limited (returns empty namespace for `prefix:local`).
5. **`from` pattern edge cases** — `level="any"` resets count at each `from` match during document-order walk. Nested `from` boundaries may differ from strict spec behavior.
6. **Negative zero** — `double.IsNegative(value)` is used to detect `-0`; `value == 0.0` alone is not sufficient.
7. **Global variable forward references** — Global variables are evaluated in import/include/local order. A local variable referencing an imported variable works, but forward references within the same stylesheet are not dependency-sorted.
8. **`xsl:for-each` over atomic values** — `EnumerateNodes()` filters out non-node items; `1 to 4` in `xsl:for-each/@select` produces zero iterations. Need to generalize to `EnumerateItems()`.
9. **Keyword variable names** — Parser rejects `mod`, `div`, `and`, `or` etc. as variable names (e.g. `<xsl:variable name="mod">`).
10. **`xsl:mode` unsupported** — `on-no-match="shallow-skip"`, `on-multiple-match`, `warning-on-no-match` not implemented.

---

## Recommended Next Steps

1. **`xsl:param` default values & `as` attribute** — `xsl:param` with `required="no"` and no default currently produces `Undefined`. Need to support sequence constructors as defaults and type coercion via `as`.
2. **`instance of` on `Undefined`** — `instance of` type check crashes when value is `Undefined`. Should return `false` gracefully.
3. **Literal whitespace text nodes in stylesheets** — Whitespace text nodes between XSLT instructions in literal result elements are copied to output. Need to strip insignificant whitespace from the stylesheet tree before execution.
4. **REQ-011 — `fn:transform()`**: XSLT 3.0 function that invokes a transformation from within XPath. Needs API surface changes, nested transform isolation, result document handling. Larger scope.
5. **REQ-008 — `fn:function-lookup` precision**: Numeric serialization precision mismatches in QT3 tests. Narrow, isolated fix in `FunctionLibrary` or `XdmValue`.

---

## Branches

- `main` — all work is on `main`, pushed to `origin/main`
- No feature branches
