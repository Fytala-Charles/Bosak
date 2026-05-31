# Handover — Bosak XPath/XSLT Implementation

**Date:** 2026-05-31
**Commit:** `bcb8b00` (pushed to origin)
**Current focus:** Customer A-driven feature gaps — REQ-020 (`exclude-result-prefixes`) just implemented; REQ-021 (`xsl:message`) remains.

---

## Project Status Overview

### XSLT Conformance (W3C XSLT 3.0 Test Suite)

- **Passed:** 2756 / **Failed:** 2706 / **Skipped:** 9138 (14,600 total)
- Pass rate: **50.5%** (latest run, 2026-05-30)
- Runner completes without crashes (exit code 0)

**Recent trajectory:**
- Latest: 2756 passed / 2706 failed / 9138 skipped (50.5%) — after optimizer boolean simplification fix
- Previous: 2752 passed / 2710 failed / 9138 skipped (50.4%) — after optimizer divide-by-zero fix
- Before that: 2750 passed / 2712 failed / 9138 skipped (50.3%) — after document-node wrapping + global var context fix
- Previous: 2675 passed / 2787 failed / 9138 skipped (49.0%) — after seqtor/simple-content fixes
- Run 11: 2547 passed / 2915 failed / 9138 skipped (46.6%)
- Run 36: 2545 passed / 2922 failed / 9133 skipped (46.6%)
- Run 37: **crashed** — stack overflow in `seqtor-031` (deep xsl:function recursion, depth 61)
- Run 9: 2525 passed / 2940 failed / 9135 skipped (46.2%) — after fixing crash
- Run 10: 2529 passed / 2933 failed / 9138 skipped (46.3%) — after empty sequence cast fix

### Recent Fixes (This Session)

1. **`fn:substring` rounding fix** — `Substring_2`/`Substring_3` now use `RoundDouble` (half-to-ceiling) matching XPath `fn:round` semantics. Fixed `string-021`, `string-090`, `string-093`.
2. **`xsl:number` fixes** — `ComputeNumberMultiple`/`ComputeNumberSingle` now correctly find nearest-ancestor `from` nodes and verify descendant-or-self relationship. `FormatNumberSequence` emits `prefix+suffix` for empty number arrays. +32 number tests (81→113 passed).
3. **Sequence constructor text node preservation** — `EvaluateSequenceConstructor` now preserves text nodes as `XdmValue` text nodes (not atomic strings), so `CopyToResult` can merge adjacent text nodes without inserting spaces.
4. **`CopyToResult` rewrite for §5.7.1** — Properly merges adjacent text nodes, joins consecutive atomic values with single space (#x20), discards zero-length text nodes, and handles sequences correctly.
5. **`ApplyComplexContentRules`** — New helper that merges adjacent text nodes and removes zero-length text nodes when wrapping sequence constructor output in a document node (for `xsl:variable` without `as`).
6. **Adjacent-atomic spacing tracking** — Added `_lastAddedWasAtomic` field and `AppendAtomicText()` method so that successive `xsl:sequence` instructions with single atomics are joined with spaces.
7. **TVT evaluation in `xsl:text`** — `xsl:text` now evaluates TVTs when `expand-text="yes"` is set. Fixes `seqtor-036b/c`, `037b/c`, `039b/c`, `040b/c`, `041`, `042`.
8. **`ContainsTvtExpression` guard** — `ProcessSequenceText` only evaluates TVTs when the text node actually contains `{...}`. Whitespace-only text nodes inside `expand-text="yes"` elements are now correctly stripped (unless they contain a TVT). Fixes `seqtor-020`, `026`.
9. **Empty sequence state preservation** — `CopyToResult` no longer resets `_lastAddedWasAtomic` when processing empty sequences. Fixes `seqtor-007`, `010`, `011`.
10. **`WhitespacePreserveElements` corrected** — Reduced to only `"text"` per XSLT 3.0 §3.3.1.1. Previously incorrectly included `comment`, `attribute`, `element`, `for-each`, etc.
11. **`xsl:processing-instruction` handler** — Added proper `xsl:processing-instruction` support using `EvaluateSimpleContent`.
12. **`xsl:namespace` handler** — Added basic `xsl:namespace` support.
13. **`EvaluateSimpleContent` adoption** — `xsl:attribute`, `xsl:comment`, `xsl:processing-instruction`, `xsl:value-of` (no select), and `xsl:text` now use `EvaluateSimpleContent` instead of naive text concatenation.

### Unit Test Status

- **734 unit tests pass** across 7 test projects (0 failures)
- XSLT-specific tests: 79 tests in `Bosak.XPath.Xslt.Tests`

### QT3 Conformance Baseline

- Passed: ~18,529 / Failed: ~3,490 / Skipped: 9,802 (31,821 total)
- Pass rate: ~58.2%

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
          call-template, copy, copy-of, number, sort, processing-instruction,
          namespace, sequence, next-match
```

### Key Files for XSLT Work

| File | Responsibility |
|------|---------------|
| `src/Bosak.XPath.Xslt/Runtime/TransformEngine.cs` | Main execution engine; add new instruction handlers here. **Recently modified for seqtor fixes.** |
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

### Document Node Wrapping for Mixed Content
- `EvaluateSequenceConstructor` now **always** wraps non-empty sequence constructor output in a document node when `wrapInDocumentNode=true`.
- For mixed content (text, multiple elements, comments, PIs), creates a synthetic `XDocument` with a hidden `__xdm_doc__` wrapper element.
- `XDocumentNode` transparently unwraps this wrapper: children, descendants, string value, parent navigation, document order, and serialization all skip the wrapper.
- Fixes `string-041`, `boolean-110`, `boolean-111`, and ~72 other tests across multiple clusters.
- Change history updated in `TransformEngine.cs` and `XDocumentNode.cs`

### Global Variable Context Item Fix
- `InitializeGlobalParametersAndVariables` now sets `_context.WithFocus(focus)` **before** evaluating global parameters and variables.
- `EvaluateSequenceConstructor` now saves/restores `_context.ContextItem` around execution, ensuring XPath expressions inside sequence constructors (e.g. `xsl:value-of/@select`) use the correct context item.
- This was the root cause of `string-041` and many other global-variable-related failures.

### Sequence Constructor & Simple Content Construction
- `CopyToResult` rewritten with proper §5.7.1 complex content rules
- `AppendAtomicText()` added for cross-instruction atomic joining
- `_lastAddedWasAtomic` field added to `TransformEngine`
- `ApplyComplexContentRules()` helper added for document-node wrapping
- `EvaluateSequenceConstructor` now preserves text nodes as nodes (not strings)
- `EvaluateSimpleContent` used by `attribute`, `comment`, `processing-instruction`, `value-of`, `text`
- Change history updated in `TransformEngine.cs`

### TVT & Whitespace
- `xsl:text` handler evaluates TVTs when `expand-text="yes"`
- `ContainsTvtExpression()` helper added
- `ProcessSequenceText` checks `ContainsTvtExpression` before TVT evaluation
- `WhitespacePreserveElements` reduced to only `"text"`
- Change history updated in `TransformEngine.cs`

### New Instruction Handlers
- `xsl:processing-instruction` added (with `EvaluateSimpleContent`)
- `xsl:namespace` added (basic support)
- Change history updated in `TransformEngine.cs`

### XPath Optimizer Constant Folding
- `XPathOptimizer.OptimizeBinary` now catches `DivideByZeroException` during constant folding for integer and decimal operations (divide, idiv, mod).
- Prevents compile-time crashes on expressions like `true() or 1 div 0` where short-circuit evaluation should skip the division.
- Fixes `boolean-032`, `boolean-084`.
- Change history updated in `XPathOptimizer.cs`

### XPath Optimizer Boolean Simplification
- `SimplifyBoolean` no longer returns non-boolean operands from `and`/`or` expressions.
- `and`/`or` always return `xs:boolean` in XPath; returning a string literal changed the result type and serialization.
- Fixes `boolean-023`, `boolean-031`, `boolean-078`, `boolean-079`.
- Change history updated in `XPathOptimizer.cs`

### `fn:substring` Fix
- `Substring_2`/`Substring_3` use `RoundDouble(startD, 0)` instead of `(int)startD`
- Change history updated in `FunctionLibrary.cs`

### Node Ordering Comparisons (`<<` / `>>`)
- `VmEngine` implements `PrecedesNode`/`FollowsNode` using `DocumentOrder` property.
- Previously threw `NotImplementedException`.
- Change history updated in `VmEngine.cs`

### `fn:format-number` Enhancements
- `FormatNumberEngine` now casts non-numeric atomic values to `double` before formatting.
- `IsNumeric`/`ConvertToDouble` helpers added.
- Grouping-separator parsing fixed in subpicture validation.
- Zero-digit sign detection fixed for surrogate pairs.
- Change history updated in `FormatNumberEngine.cs`

### Stylesheet Decimal-Format Inheritance
- `Stylesheet.GetAllDecimalFormats` now merges definitions from imported/included stylesheets with correct precedence.
- Descendant namespace declarations are collected for XPath expression resolution.
- Change history updated in `Stylesheet.cs`

### `xsl:try` / `xsl:catch` Support (REQ-019)
- `TransformEngine.ExecuteXsltInstruction` now handles `xsl:try` / `xsl:catch` in result-tree construction.
- `EvaluateFunctionBodyInstruction` now handles `xsl:try` / `xsl:catch` inside `xsl:function` bodies.
- Supports `select` attribute on `xsl:catch` as well as sequence-constructor children.
- Catches any `Exception` broadly (no error-code filtering yet).
- Change history updated in `TransformEngine.cs`

### `exclude-result-prefixes` Support (REQ-020)
- `Stylesheet.Load()` parses `exclude-result-prefixes` from the root element.
- `GetAllExcludedResultPrefixes()` merges excluded prefixes across imports/includes.
- `TransformEngine.CopyLiteralElement` filters out namespace declarations for excluded prefixes.
- Supports `#all` shorthand.
- Change history updated in `Stylesheet.cs` and `TransformEngine.cs`

### Customer A XSLT Usage Analysis
- Analyzed all 71 stylesheets in `D:\Development\Customer A\assets\xslt`.
- Identified 3 gaps: `xsl:try`/`xsl:catch` (P0), `exclude-result-prefixes` (P1), `xsl:message` (P2).
- Added REQ-019, REQ-020, REQ-021 to `docs/FEATURE_REQUESTS.md`.
- Notable non-usage: `xsl:for-each-group`, `xsl:key`/`key()`, `xsl:merge`, `xsl:apply-imports`, `xsl:result-document` — none used by Customer A.

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

# Run W3C XSLT 3.0 conformance suite (full catalog)
dotnet run --project tests/Bosak.XPath.Xslt.Conformance/Bosak.XPath.Xslt.Conformance.csproj

# Run specific test set only (e.g. seqtor)
dotnet run --project tests/Bosak.XPath.Xslt.Conformance/Bosak.XPath.Xslt.Conformance.csproj -- tests/xslt30-test/catalog.xml seqtor
```

---

## Known Issues / Gotchas

1. **Conformance runner locks DLLs** — If a previous conformance run is still running, builds will fail. Kill with `taskkill /F /IM Bosak.XPath.Xslt.Conformance.exe` before building.
2. **Empty element serialization** — `XmlWriter` outputs `<done />` (with space), not `<done/>`. Tests should use flexible assertions.
3. **`key()` namespace** — Registered under `http://www.w3.org/2005/xpath-functions` (not XSLT namespace) because the XPath compiler resolves unprefixed function names to the `fn` namespace.
4. **PatternCompiler limitations** — Predicates create a new `EvaluationContext` per evaluation; prefix resolution in QNames is limited (returns empty namespace for `prefix:local`).
5. **Negative zero** — `double.IsNegative(value)` is used to detect `-0`; `value == 0.0` alone is not sufficient.
6. **Global variable forward references** — Global variables are evaluated in import/include/local order. Forward references within the same stylesheet are not dependency-sorted.
7. **Namespace declaration hoisting** — LINQ-to-XML places `xmlns:prefix` on first element using it; Saxon/test suite expects hoisting to outermost element. Root cause of many namespace test failures.
8. **Sequence constructor batching** — `ExecuteSequenceConstructorDirect` adds items to `_currentContainer` eagerly (one by one). This means adjacent atomics across `xsl:for-each` iterations or multiple `xsl:sequence` instructions are not batched before complex content construction. `_lastAddedWasAtomic` is a partial workaround but cannot fully emulate true sequence accumulation. Root cause of `seqtor-024`, `025`, `026` and possibly others.
9. **`xsl:namespace-alias` not implemented** — ~26 namespace tests fail.
10. **`xsl:number level="multiple"`** — Multi-level ancestor chain formatting is incomplete.
11. **Decimal overflow in `FormatNumberEngine`** — Uses `decimal` which overflows for very large inputs.
12. **Match pattern gaps** — `descendant-or-self::x[predicate]`, `except`/`intersect`, `id()`/`key()` patterns missing in `PatternCompiler`.
13. **DateTime year < 1** — `DateTimeOffset` minimum year is 1. Tests using year `-2` cannot pass without switching to a custom date representation.
14. **Timezone adjustment** — `adjust-time-to-timezone` produces incorrect offsets in some cases.

---

## Recommended Next Steps

### Immediate: Quick-win clusters (~few hours, +30–50 tests)

These clusters are >75% passing with only a handful of distinct root causes:

- **`string`** — 4 failures, 0 skipped (97% passing)
- **`position`** — 21 failures, 3 skipped (90% passing)
- **`boolean`** — 10 failures, 0 skipped (91% passing)

### Short-term: High-density clusters (~1–2 days each, +50–100 tests)

| Cluster | Failed | Skipped | Notes |
|---------|--------|---------|-------|
| **number** | 152 | 1 | Already have context from earlier `xsl:number` fixes. Likely Unicode numbering, `lang`/`letter-value`, grouping, large numbers. |
| **match** | 106 | 107 | Pattern matching gaps. May overlap with `PatternCompiler` work. |
| **mode** | 88 | 44 | Template mode dispatch issues. |
| **copy** | 80 | 20 | `xsl:copy`, `xsl:copy-of` behavior gaps. |
| **date** | 68 | 0 | Known `DateTimeOffset` limitation, but many others may be fixable. |
| **for-each-group** | 62 | 3 | `xsl:for-each-group` implementation gaps. |
| **key** | 63 | 8 | `key()` / `xsl:key` behavior. |
| **use-when** | 61 | 0 | Static evaluation of `use-when` expressions. |

### Medium-term: Architectural refactor (~2–3 days, broad impact)

**Sequence constructor batching** — Refactor `ExecuteSequenceConstructorDirect` to accumulate raw `XdmValue` items in a list and apply complex content construction rules (§5.7.1) only when flushing. This would:
- Fix remaining 13 seqtor failures (`seqtor-016`, `017`, `024`, `025`, `026`, `027`, `028`, `034`, `035`, `036a`, `036d`, `037a`, `037d`)
- Likely improve many other clusters that depend on correct sequence construction (`sequence`, `copy`, `variable`, etc.)

---

## Branches

- `main` — all work is on `main`, pushed to `origin/main`
- No feature branches
- Uncommitted changes: none
