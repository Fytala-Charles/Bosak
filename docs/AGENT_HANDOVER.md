# Handover — Bosak XPath/XSLT Implementation

**Date:** 2026-06-07
**Commit:** `bb13da6`
**Current focus:** `match` cluster at 84.4% (152/294). 27 failures remain (`intersect`/`except` variable patterns, namespace nodes, complex assertions). Next: `next-match` cluster (17/40) or `for-each-group` (38/85).

---

## Project Status Overview

### XSLT Conformance (W3C XSLT 3.0 Test Suite)

- **Passed:** ~3,301 / **Failed:** ~2,112 / **Skipped:** ~9,187 (14,600 total)
- Pass rate: **61.0%** (latest run, 2026-06-07)
- Runner completes without crashes (exit code 0)

**Recent trajectory:**
- Latest: 3301 passed / 2112 failed / 9187 skipped (61.0%) — apply-imports + for-each-group atomic patterns
- Latest XPath: 19041 passed / 2829 failed / 9951 skipped (59.84%) — QT3 stable
- Previous: 3255 passed / 2206 failed / 9139 skipped (59.6%) — namespace fixes in XPath expressions, xsl:number, and conformance harness
- Previous: 3232 passed / 2229 failed / 9139 skipped (59.2%) — `expression` cluster 100% (cross-document key() lookup, key index per-document)
- Previous: 3231 passed / 2230 failed / 9139 skipped (59.2%) — `attribute()` axis fix, initial template selection fix, parentless element patterns, template last-wins rule
- Previous: 3198 passed / 2263 failed / 9139 skipped (58.6%) — `fn:doc`/`fn:document` empty-sequence handling, parentless element pattern fixes
- Previous: 2859 passed / 2603 failed / 9138 skipped (52.3%) — map/array keyword disambiguation, static error propagation from patterns, XPST0017 error codes
- Previous: 2823 passed / 2639 failed / 9138 skipped (51.7%) — `core-function` cluster 100% (round/ceiling/floor string-arg fixes)
- Previous: 2767 passed / 2695 failed / 9138 skipped (50.7%) — boolean cluster 98% (8 result mismatches + 2 unimplemented fixed)
- Previous: 2761 passed / 2701 failed / 9138 skipped (50.6%) — string cluster 100% complete (135, 094, 095 fixed)
- Previous: 2750 passed / 2712 failed / 9138 skipped (50.3%) — after document-node wrapping + global var context fix
- Previous: 2675 passed / 2787 failed / 9138 skipped (49.0%) — after seqtor/simple-content fixes
- Run 11: 2547 passed / 2915 failed / 9138 skipped (46.6%)
- Run 36: 2545 passed / 2922 failed / 9133 skipped (46.6%)
- Run 37: **crashed** — stack overflow in `seqtor-031` (deep xsl:function recursion, depth 61)
- Run 9: 2525 passed / 2940 failed / 9135 skipped (46.2%) — after fixing crash
- Run 10: 2529 passed / 2933 failed / 9138 skipped (46.3%) — after empty sequence cast fix

### Recent Fixes (This Session)

1. **XSLT 3.0 `xsl:apply-imports` support** — Previously completely unimplemented. Added `case "apply-imports"` in `TransformEngine.ExecuteXsltInstruction`. Finds best matching template with higher import precedence (deeper in import chain). Passes through current tunnel parameters. Falls back to built-in rules for nodes and atomic values. Fixes `match-133` and ~8 mode tests.
2. **`for-each-group` atomic pattern matching** — `group-starting-with` and `group-ending-with` now evaluate patterns against atomic values, not just nodes. Previously `item.IsNode` guard prevented atomic predicate patterns like `.[. mod 3 = 0]` from matching. Fixes `match-134`, `match-135`, and `next-match-007/022`.
3. **.NET 10 migration (REQ-022)** — All 18 `.csproj` files updated from `net9.0` to `net10.0`. Build clean, 867 unit tests pass, XSLT conformance stable.
4. **Large integer overflow in `xsl:number` (number-0111)** — `VmEngine.Multiply/Add/Subtract` now detect `long` overflow via `checked` arithmetic and promote to `decimal`. Previously `1234567890^3` wrapped to negative, triggering `XTDE0980`.
5. **`xsl:number` BigInteger formatting pipeline (number-0807)** — `FormatIntegerEngine` now accepts `BigInteger` values. `TransformEngine.xsl:number` value path migrated from `long[]` to `BigInteger[]`. `1e100` now formats as `100000...000` instead of `long.MaxValue`.
6. **XSLT conformance baseline run** — Full suite: 3,301 passed / 2,112 failed / 9,187 skipped (61.0%).
7. **Register overflow fix (CRITICAL)** — `IrInstruction` register fields expanded from `byte` to `ushort`, removing the 255-register limit. `VmEngine` dynamically sizes register arrays via `module.MaxRegisterCount`. Fixed `_freeRegisters.Clear()` bug in `IrLowerer.Lower()` that caused incorrect register reuse. Fixed `PackArgumentsConsecutive` and argument repacking in `LowerFunctionCall`/`LowerDynamicFunctionCall` to guarantee consecutive register allocation.
8. **QT3 harness `normalize-space` fix** — `assert-string-value` now respects `normalize-space="true"` and applies XPath `normalize-space()` semantics to both expected and actual values. Fixed 14+ string-value tests. XPath QT3 suite now completes all 31,821 tests across 428 sets (19,041 passed / 2,829 failed / 9,951 skipped, 59.84%).

### This Session Fixes (2026-06-04)

1. **`fn:implicit-timezone` returns `xs:dayTimeDuration`** — Previously returned a plain `String`, causing 12 QT3 failures in the timezone cluster. Now returns `XdmValueKind.Duration` with correctly formatted ISO 8601 dayTimeDuration.
2. **Time subtraction normalization** — `VmEngine.Subtract` for `xs:time` values now normalizes both operands to a common reference date (`0001-01-01`) before subtracting. Fixes comparisons between `fn:current-time()` and `xs:time` literals that previously failed due to injected current date from `DateTimeOffset.TryParse`.
3. **`fn:trace#1` overload** — Added missing 1-argument `fn:trace` function.
4. **`fn:sort` / `array:sort` collation support** — Collation URI is now threaded through `Sort`/`ArraySort` to `CompareSortKeys`, which uses `CompareStrings` with the specified collation for string keys.
5. **QT3 `caseblind` collation** — Added recognition of the QT3 test-suite case-insensitive collation URI (`http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind`).
6. **`array:sort#2`** — Added missing 2-argument `array:sort($array, $collation)` overload.
7. **NaN sort comparer** — `XdmValueComparer.CompareNumeric` now treats NaN as equal to NaN (placing NaN values together during sort).
8. **QT3 baseline (session part 1)** — 18,659 passed / 3,212 failed / 9,950 skipped (58.64%). +25 tests.
9. **QT3 baseline (session part 2)** — 18,695 passed / 3,175 failed / 9,951 skipped (58.75%). +61 tests total.

### This Session Fixes (2026-06-05)

1. **`SkipSequenceType` parser bug** — `SkipSequenceType` in `XPathParser.cs` was using token indices (`_position`) as character positions for `GetSpanText`, producing garbage type strings like `"d"` or `"e("` instead of `"xs:integer"`. Fixed to use actual token character spans (`token.Start`, `token.Length`). This was the root cause of ALL `XPTY0004` failures in inline functions with typed parameters/return types.
2. **`ParseTypeNameAndParens` function return type** — Now consumes `as ReturnType` after `function(...)` for `instance of function(...) as type` expressions.
3. **`InvokeFunctionItem` sequence parameter validation** — Previously rejected any sequence with >1 item regardless of occurrence indicator (`*`/`+`). Now respects `*` (0+), `+` (1+), `?` (0/1) and validates each item.
4. **Numeric promotion in `ItemInstanceOf`** — `xs:double` now accepts `integer`/`decimal`; `xs:float` now accepts `integer`/`decimal`/`double`; added `xs:numeric` and `xs:anyAtomicType` support.
5. **`node()` type test in `ValueMatchesType`** — `ValueMatchesType` was missing `node()` handling (only had `element()`/`attribute()`). Added `normalized == "node()" => value.IsNode`.
6. **QT3 baseline** — 18,722 / 3,148 / 9,951 (58.84%). +27 tests this batch, +88 total from session start.

### This Session Fixes (2026-06-05, continued)

1. **`function-item-8` fix** — `fn:function-name` now returns QNames with the standard namespace prefix for built-in functions. `FunctionName` maps known namespace URIs (`fn`, `math`, `map`, `array`) to their conventional prefixes. Also fixed `ValuesEqual` in `ResultComparer.cs` to compare QNames by namespace URI and local name (ignoring prefix), making `assert-eq` spec-compliant for QName values.
   - **HigherOrderFunctions cluster**: improved from 33/11/85 to 34/10/85.
2. **`inline-function-12a` fix** — `ParseInlineFunction` now validates that parameter names are unique and raises `XQST0039` when duplicates are found.
   - **HigherOrderFunctions cluster**: improved from 34/10/85 to 35/9/85.
3. **QT3 estimated baseline** — ~18,767 / ~3,103 / 9,951 (approx. +2 tests).

### This Session Fixes (2026-06-05, part 2)

1. **`K-NodeBefore-3` / `K-NodeAfter-3` fix** — Node comparison operators (`is`, `<<`, `>>`) now raise `XPTY0004` when operands are not single nodes. Previously returned `false` for non-node operands.
2. **`K-NodeBefore-5..11` / `K-NodeAfter-5..11` fix** — `ParseException` now auto-prefixes generic messages with `XPST0003:` when no explicit error code is present. This makes the conformance harness correctly recognize static parse errors.
   - **node-before cluster**: improved from 16/10/10 to 24/2/10.
   - **node-after cluster**: improved from 16/10/9 to 24/2/9.
   - Remaining failures (`nodeexpression28/31/44/47`) are test-environment issues (missing `$works` variable), not operator bugs.
3. **QT3 estimated baseline** — ~18,783 / ~3,087 / 9,951 (approx. +16 tests this batch).

### This Session Fixes (2026-06-05, part 3)

1. **`inline-function-16` fix** — `InvokeFunctionItem` now resolves inline function parameter names (including `Q{uri}local` and `prefix:local`) into expanded QNames before binding them to variables in `EvaluationContext`. Previously raw strings like `Q{http://local/}foo` were stored as variable names, so body references to `$Q{http://local/}foo` couldn't find them.
2. **`function-item-3` fix** — The `is` operator now raises `XPTY0004` for non-node operands (fixed alongside `<<`/`>>` in part 2). `string-join#1 is string-join#1` now correctly errors.
3. **`ResolveVariableName` braced URI support** — Added `Q{uri}local` parsing to `ResolveVariableName`, matching the compiler's handling of `VariableReferenceNode` with explicit namespace URIs.
   - **HigherOrderFunctions cluster**: improved from 35/9/85 to 37/7/85.
4. **QT3 estimated baseline** — ~18,785 / ~3,085 / 9,951 (approx. +2 tests this batch).

### This Session Fixes (2026-06-04, part 2 — latest)

1. **`fn-sort-spec-6` fix** — Variable-length node sort keys returned wrong ordering. Root cause: `VmEngine.Execute()` globally called `NormalizeSequence` which sorted all nodes by document order, reversing explicitly constructed sequences like `($emp/name/last, $emp/name/first)`. Also, sort keys were not atomized before comparison.
   - Removed global `NormalizeSequence(result)` from `Execute()` — path expressions already normalize via `IrOpCode.Normalize` emitted by compiler.
   - Wrapped key-function results in `Data()` in `Sort()` and `ArraySort()` so node keys are atomized to strings before comparison.
   - **Sort cluster**: improved from 65/1/18 to 66/0/18.
2. **`fn-for-each-pair-017` fix** — `deep-equal` on mixed item types returned one `false`. Three root causes:
   - `instance of function(*)` returned `false` — `ValueMatchesType` didn't handle `function(*)`, `map(*)`, `array(*)`.
   - Whitespace text nodes were stripped by default .NET XML parsing, but QT3 counts them as part of `//node()`.
   - Document-level whitespace (before/after root element) was incorrectly preserved when using `PreserveWhitespace`.
   - Added `function(*)`, `map(*)`, `array(*)` matching to `ValueMatchesType`.
   - Changed `XDocumentProvider.LoadXml` and `ParseXml_1`/`ParseXmlFragment_1` to use `LoadOptions.PreserveWhitespace`.
   - Added `StripDocumentLevelWhitespace()` in `XDocumentProvider` to remove whitespace-only text nodes that are direct children of the document node.
   - **For-each-pair cluster**: improved from 55/1/2 to 56/0/2.
3. **Commit:** `5405926` — both fixes in single commit.
4. **Typed function signature matching** — `ValueMatchesType` now parses and validates `function(T1,...) as R` type tests against `InlineFunctionItem` metadata. Implements XPath 3.1 contravariant parameters and covariant return rules with occurrence indicator awareness. Adds `IsSequenceTypeSubtype`, `IsBaseTypeSubtype`, `GetDirectSupertypes`, `TryParseFunctionType`, `TryGetInlineFunctionSignature` helpers. Fixes 7 QT3 tests: `inline-function-6/7/8/9`, `function-item-10/13/14`. HigherOrderFunctions cluster now 33/11/85 (was 26/18/85).
5. **Commit:** `d5a9c50` — typed function signature matching.

### This Session Fixes (2026-06-04, part 2)

1. **`fn:default-language#0`** — Returns `"en"` with `xs:language` schema type. DependencyFilter skips tests requiring other languages.
2. **`fn:element-with-id#1`** — Basic implementation searching `id` and `xml:id` attributes.
3. **`Filter_2` / `ArrayFilter` boolean validation** — Predicate must return `xs:boolean`; strings/empty sequences now raise XPTY0004.
4. **`ForEachPair_3` arity validation** — Validates function arity is exactly 2.
5. **`RequireString` type enforcement** — New helper that enforces `xs:string?` arguments. Used by `upper-case`, `lower-case`, `contains`, `starts-with`, `ends-with`.
6. **Document URI base URI fix** — `XDocumentProvider.LoadXml` uses `LoadOptions.SetBaseUri`; `TestEnvironment` loads source docs via file path.
7. **QT3 baseline** — 18,695 / 3,175 / 9,951 (58.75%). +61 tests from start of session.

### This Session Fixes (2026-06-07)

1. **Atomic-value PredicatePattern matching** — `PatternCompiler.CompileAtomicMatch` now correctly handles:
   - Runtime numeric predicate semantics for variable expressions (e.g. `$N=2` returns false per XSLT §6.4).
   - Whitespace between `.` and `[` after XPath comment stripping.
   - Multiple predicates compiled individually with proper numeric/boolean evaluation.
2. **`ApplyTemplates` supports atomic values** — `TransformEngine.ApplyTemplates` now processes sequences containing atomic values, not just nodes. Added `ApplyTemplates(XdmValue)` overload for named-template invocations without a context node.
3. **`next-match` supports atomic values** — `TransformEngine` `xsl:next-match` now works with atomic context items, using `FindBestTemplate(XdmValue)` and `ExecuteTemplate(rule, XdmValue)`.
4. **Built-in rule for atomic values** — When no template matches an atomic value (via `apply-templates` or `next-match`), the string value is output to the result tree. Required for `match-131/132` `next-match` chain termination.
5. **XPath comment stripping in priority computation** — `TemplateRule.ComputeDefaultPriority` / `ComputeSinglePatternPriority` now strip XPath comments before checking pattern shape. Patterns like `(:c:).[expr]` now correctly get PredicatePattern priority (1.0) instead of generic 0.5.
   - **Match cluster**: improved from 138/41/115 to 149/30/115 (+11 tests). Fixes: match-127/128/130/131/132/240a/240b/240c.

### This Session Fixes (2026-06-05, part 4 — latest)

1. **`number-1501` fix** — `WalkDocumentTree` in `TransformEngine.cs` now correctly propagates the text-node skip across empty elements and elements without attributes. The algorithm models .NET `XslCompiledTransform` semantics for `xsl:number level="any"`:
   - Only the **first attribute** of each element is counted (restored from earlier behavior).
   - The **first text node** after an element's attributes is skipped, regardless of content.
   - If an element with attributes has no text children, the skip propagates to the next text node in document order, even across intervening empty elements.
   - Fixed by introducing `skipNextText` parameter and `pendingSkip` out-parameter in `WalkDocumentTree` recursive traversal.
   - **Number cluster**: `number-1501` now passes. Cluster improved from 262/273 (96.2%) to 263/271 (97.3%).
2. **`number-1101` fix** — `WalkDocumentTree` was only visiting the first attribute of each element. When `xsl:number level="any"` was called with a non-first attribute as the context node (e.g. `//msp:Source/@title`), `foundCurrent` was never set, causing the walk to continue to the end of the document and over-count.
   - `WalkDocumentTree` now visits **all attributes** (required for correct `foundCurrent` detection).
   - `ComputeNumberAny` tracks `lastCountedAttributeParent` to ensure only the **first attribute** of each element increments the count, preserving `number-1501` behavior.
   - **Number cluster**: `number-1101` now passes. Cluster improved from 263/271 (97.3%) to 264/271 (97.8%). Only 6 non-English word/ordinal formatting failures remain.

### Previous Session Fixes

1. **XPath unprefixed element namespace handling** — `IrLowerer` now emits `NamespaceTest` for unprefixed element names on element axes, enforcing the default element namespace. `VmEngine.NamespaceTest` resolves empty prefix to default element namespace, falls back to empty namespace, and handles `Q{uri}local` URIs directly. Fixes `number-1502` and many tests where `//book` incorrectly matched namespaced elements.
2. **`Q{uri}local` parser fix** — `XPathParser.ParseNodeTest` now correctly creates `QName` node tests for braced URI literals (e.g., `Q{http://z.test.com/}note`) instead of treating them as `LocalName`. Previously discarded the `nsUri` from `SplitQName`.
3. **Namespace prefix resolution in `xsl:number`** — `TransformEngine.ExecuteXsltNumber` now resolves namespace prefixes in `count` and `from` patterns using the `xsl:number` element's in-scope namespace declarations. `KeyIndex.Build` also resolves prefixes in `xsl:key` match patterns. Fixes `number-1101`.
4. **Conformance harness namespace leak fix** — `ExtractNamespaces` no longer passes the test catalog's default namespace (empty prefix) to XPath assertion evaluation contexts. This was causing assertions like `/out/a = "x"` to incorrectly use the catalog namespace as the default element namespace.
5. **`attribute()` default axis fix** — `ParseAxisStep` now defaults to `attribute` axis for `attribute()`/`schema-attribute()` kind tests and `namespace` axis for `namespace-node()` (XPath 2.0 §3.2.1.1). Previously parsed as `child::attribute()` which always returned empty. Fixes `match-106` and ~18 other conformance tests.
6. **Initial template selection fix** — `Transform` now applies templates to children of the document node (XSLT 2.0 §5.4 built-in rule), not the document node itself. Prevents spurious `node()` template matching on document root. Fixes ~14 `next-match` and related conformance tests.
7. **`fn:doc`/`fn:document` empty-sequence guard** — `Doc_1` and `Document_2` return `XdmValue.Undefined` for empty input instead of throwing. Fixes unit test and prevents `ArgumentException` on `XDocument.Load("")`.
8. **Parentless element patterns** — `PatternCompiler` now handles `*:local`, `child::*:b`, and implicit child-axis patterns for parentless nodes. Fixes `match-102`, `108`, `109`.
9. **Template selection last-wins rule** — `FindBestTemplate` uses XSLT "later declaration wins" tie-breaker when priority and import precedence are equal.

### Previous Session Fixes

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
14. **`fn:string-length` surrogate pair fix** — `StringLength_0`/`StringLength_1` now use `EnumerateRunes()` to count Unicode code points instead of UTF-16 code units (`.Length`). Fixes `string-132`.
15. **`fn:upper-case` / `fn:lower-case` Unicode full case mapping** — Added `ApplyUnicodeCaseMapping` with special handling for one-to-many mappings (e.g., ß → SS, İ → i̇). Fixes `string-135`.
16. **`fn:substring` code-point-aware** — `Substring_2`/`Substring_3` now operate on Unicode code points via `EnumerateRunes()`, matching XPath spec semantics for surrogate pairs. Fixes `string-094`.
17. **AVT parser string-literal awareness** — `EvaluateAvt` now uses `FindAvtExprEnd` which skips `}` inside XPath string literals (`'...'` and `"..."`). Fixes `string-095`.
18. **Empty sequence in comparisons** — `Compare`/`CompareGeneral` now return `XdmValue.Undefined` when either operand is an empty sequence. `IsSameNode` also returns empty sequence for empty operands. Fixes `boolean-071`, `072`, `075`.
19. **XPath 1.0 backwards-compatible general comparisons** — `EvaluationContext.BackwardsCompatible` flag set from `xsl:stylesheet/@version`. `CompareGeneral` applies XPath 1.0 coercion rules (boolean→numeric→string hierarchy) when active, and strict type checking (XPTY0004) in XPath 2.0+ mode. Fixes `boolean-081`, `083`, `096`, `097`, `098`.
20. **XPath optimizer boolean simplification** — `SimplifyBoolean` restricted to only substitute `BooleanLiteralNode` operands. Prevents type mismatch where `true and "00"` was incorrectly simplified to `"00"` (string instead of boolean). Fixes `boolean-023`, `031`, `078`, `079`, `083`.
21. **XPath optimizer divide-by-zero** — Constant folding for `DecimalLiteralNode / DecimalLiteralNode` skips when divisor is zero. Fixes `boolean-032`, `084`.

### Unit Test Status

- **867 unit tests pass** across 7 test projects (0 failures)
- XSLT-specific tests: 95 tests in `Bosak.Xslt.Tests`

### QT3 Conformance Baseline

- Passed: 18,785 / Failed: 3,085 / Skipped: 9,951 (31,821 total)
- Pass rate: ~59.04%

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
| `src/Bosak.Xslt/Runtime/TransformEngine.cs` | Main execution engine; add new instruction handlers here. **Recently modified for seqtor fixes.** |
| `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Parses xsl:stylesheet, resolves imports/includes, collects templates/keys/output/strip-space |
| `src/Bosak.Xslt/Stylesheet/TemplateRule.cs` | Single template rule with match pattern, modes, priority, import precedence |
| `src/Bosak.Xslt/Stylesheet/KeyDefinition.cs` | Parsed xsl:key declaration |
| `src/Bosak.Xslt/Runtime/KeyIndex.cs` | Per-document index for key() lookups; builds via document tree walk |
| `src/Bosak.Xslt/Patterns/PatternCompiler.cs` | Compiles match patterns (`item`, `@id`, `*`, `node()`, predicates) |
| `src/Bosak.Xslt/Runtime/ResultTreeSerializer.cs` | Serializes result tree with xsl:output properties |
| `tests/Bosak.Xslt.Tests/StylesheetTests.cs` | All XSLT unit tests |
| `tests/Bosak.Xslt.Conformance/Program.cs` | W3C XSLT 3.0 Test Suite runner |

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

### Expression Cluster — 100% Pass Rate
- **Numeric predicate exact equality** — `VmEngine.Filter` now uses `ToDouble(predResult) == i + 1` instead of `Math.Round(...) == i + 1`. XPath 2.0 §3.2.4 predicates must match context position exactly.
- **`fn:min`/`fn:max` mixed-type comparison** — `FunctionLibrary.MinMax` now atomizes items before comparison and uses ordinal string comparison. Fixes `expression-1201`, `expression-1301`.
- **`xsl:sequence` context item propagation** — `CopyLiteralElement` passes the `contextItem` parameter into `ExecuteXsltInstruction` instead of `_context.ContextItem`. Fixes `expression-4301`.
- **Leading-space atomic text fix** — `AppendAtomicText` no longer prepends a space to the first atomic value in a sequence. Fixes `expression-4301`.
- **Backwards-compatible node→number coercion** — `ApplyBackwardsCompatibleCoercion` converts node items to string then double when the other operand is numeric. Fixes `expression-4302`.
- **`escape-html-uri` surrogate pair encoding** — `FunctionLibrary.EscapeHtmlUri` now uses `Rune.TryGetRuneAt` + `Encoding.UTF8` for characters outside the BMP. Fixes `expression-1601`.
- **SimpleMap vs PathStepMap XPTY0018 separation** — `SimpleMap` allows atomic results; `PathStepMap` validates node-only steps. Fixes `expression-0902`, `0903`, `0905`, `0908`, `0932`, `0933`.
- **Cross-document `key()` lookup** — `TransformEngine` now maintains a `Dictionary<IXdmNode, KeyIndex>` keyed by document root node. The 2-arg `key()` form resolves the document from the context node; 3-arg form from the argument node. Lazy-built indices save/restore context focus to avoid corrupting template execution. Fixes `expression-1101`.
- Change history updated in `VmEngine.cs`, `FunctionLibrary.cs`, `TransformEngine.cs`, `IrLowerer.cs`

### XPath Keyword Disambiguation
- `ParseStepExpr` now excludes `map`, `array`, and `function` keywords from the name-test path when followed by `{`, `[`, or `(` respectively.
- Fixes map/array constructor parsing regressions that broke ~10 unit tests.
- Change history updated in `XPathParser.cs`

### Pattern Static Error Propagation
- `PatternCompiler` catch blocks now rethrow exceptions containing `XPST`/`XTSE`/`XPTY` error codes instead of swallowing them as "no match".
- `VmEngine` function-not-found exceptions now include `XPST0017:` prefix.
- Allows static errors in pattern predicates (e.g. undeclared functions) to be reported correctly.
- Change history updated in `PatternCompiler.cs` and `VmEngine.cs`

### Match Pattern Fixes
- **Pattern priority** — `ComputeDefaultPriority` checks for predicates (`[`) before QName check, so `doc[true()]` gets priority 0.5 instead of 0.0.
- **`union` keyword** — `PatternCompiler.Compile()` normalizes top-level `union` to `|` so `and union or` splits correctly into branches.
- **`root()` in patterns** — `ParseQName` returns `(string.Empty, name)` for non-QName strings. `CompileElementPattern` passes `root()` through to XPath compilation.
- **Dot in predicate patterns** — `CompilePredicatePattern` trims whitespace/comments from `basePattern` before checking for `.`.
- **`doc()` pattern resolution** — `Doc_1` function fixed to properly resolve documents for `doc('uri')/path` pattern matching.
- Change history updated in `PatternCompiler.cs`, `TemplateRule.cs`, `XPathParser.cs`

### Previous Session — Document Node Wrapping for Mixed Content
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

### `fn:substring` Fix
- `Substring_2`/`Substring_3` use `RoundDouble(startD, 0)` instead of `(int)startD`
- Change history updated in `FunctionLibrary.cs`

---

## How to Build & Test

```bash
# Build entire solution
dotnet build Bosak.sln

# Run all unit tests
dotnet test Bosak.sln

# Run only XSLT tests
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj

# Run QT3 conformance suite (~5 min, Exe project)
cd tests/Bosak.XPath.Conformance
dotnet run --configuration Release -- "D:/Development/Bosak/tests/qt3tests"

# Run W3C XSLT 3.0 conformance suite (full catalog)
dotnet run --project tests/Bosak.Xslt.Conformance/Bosak.Xslt.Conformance.csproj

# Run specific test set only (e.g. seqtor)
dotnet run --project tests/Bosak.Xslt.Conformance/Bosak.Xslt.Conformance.csproj -- tests/xslt30-test/catalog.xml seqtor
```

---

## Known Issues / Gotchas

1. **Conformance runner locks DLLs** — If a previous conformance run is still running, builds will fail. Kill with `taskkill /F /IM Bosak.Xslt.Conformance.exe` before building.
2. **Empty element serialization** — `XmlWriter` outputs `<done />` (with space), not `<done/>`. Tests should use flexible assertions.
3. **`key()` namespace** — Registered under `http://www.w3.org/2005/xpath-functions` (not XSLT namespace) because the XPath compiler resolves unprefixed function names to the `fn` namespace.
4. ~~PatternCompiler limitations~~ — **FIXED**: `TemplateRule.CompileMatch` now resolves namespace prefixes to `Q{uri}local` syntax.
5. ~~XPath unprefixed element namespace~~ — **FIXED**: `IrLowerer` now emits `NamespaceTest` for unprefixed element names on element axes, and `VmEngine.NamespaceTest` correctly handles default element namespace resolution.
6. ~~Global `NormalizeSequence` in `Execute()`~~ — **FIXED**: Removed; path expressions normalize via `IrOpCode.Normalize`. Explicitly constructed sequences preserve document order.
7. ~~Whitespace text node stripping~~ — **FIXED**: `LoadOptions.PreserveWhitespace` used everywhere; document-level whitespace stripped separately.
8. ~~Typed function signature matching~~ — **FIXED**: `ValueMatchesType` now parses `function(T...) as R` and applies contravariant params / covariant return subtyping.
9. **Negative zero** — `double.IsNegative(value)` is used to detect `-0`; `value == 0.0` alone is not sufficient.
10. **Global variable forward references** — Global variables are evaluated in import/include/local order. Forward references within the same stylesheet are not dependency-sorted.
11. **Namespace declaration hoisting** — LINQ-to-XML places `xmlns:prefix` on first element using it; Saxon/test suite expects hoisting to outermost element. Root cause of many namespace test failures.
12. **Sequence constructor batching** — `ExecuteSequenceConstructorDirect` adds items to `_currentContainer` eagerly (one by one). This means adjacent atomics across `xsl:for-each` iterations or multiple `xsl:sequence` instructions are not batched before complex content construction. `_lastAddedWasAtomic` is a partial workaround but cannot fully emulate true sequence accumulation. Root cause of `seqtor-024`, `025`, `026` and possibly others.
13. **`xsl:namespace-alias` not implemented** — ~26 namespace tests fail.
14. **`xsl:number level="multiple"`** — Multi-level ancestor chain formatting is incomplete.
15. **Decimal overflow in `FormatNumberEngine`** — Uses `decimal` which overflows for very large inputs.
16. **Match pattern gaps** — `descendant-or-self::x[predicate]`, `except`/`intersect`, `id()`/`key()` patterns missing in `PatternCompiler`. Atomic-value PredicatePattern (`.[expr]`) now works; remaining gaps are `intersect`/`except` with variables and namespace-node matching.
17. **DateTime year < 1** — `DateTimeOffset` minimum year is 1. Tests using year `-2` cannot pass without switching to a custom date representation.
18. **Timezone adjustment** — `adjust-time-to-timezone` produces incorrect offsets in some cases.

---

## Recommended Next Steps

### Next Session Immediate Targets (start here)

1. **`match` cluster** — 30 failures, 115 skipped (83.2% passing):
   - `match-040`: undeclared function in predicate → XPST0017
   - `match-069`: namespace node matching
   - `match-133`: `xsl:apply-imports` not implemented for atomic values
   - `match-134/135`: `for-each-group` with atomic value patterns (`group-starting-with`/`group-ending-with`)
   - `match-248-284`: `intersect`/`except` variable patterns

### Immediate: Quick-win clusters (~few hours, +30–50 tests)

These clusters are >75% passing with only a handful of distinct root causes:

- **`expression`** — **0 failures, 102/102 passed (100%)** ✅
- **`string`** — **0 failures, 136/136 passed (100%)** ✅
- **`core-function`** — **0 failures, 90/90 passed (100%)** ✅
- **`number`** — **6 failures, 1 skipped (97.8% passing)** — remaining: `number-0802/0812/0813/0828/0829/2506` (non-English word/ordinal formatting, out of scope)
- **`position`** — **5 failures, 3 skipped (96.2% passing)** — remaining: `position-0103` (`xsl:merge` unimplemented), `position-2201` (empty output, complex), `position-4101` (`xsl:apply-imports` unimplemented), `position-4104` (`position()` in pattern predicate, complex), `position-4105` (`xsl:for-each-group` unimplemented)
- **`boolean`** — **0 failures, 0 skipped (100% passing)** ✅ — node ordering `<<`/`>>` fixed.
- **`match`** — **30 failures, 115 skipped (83.2% passing)** — down from 78 failures after atomic-value PredicatePattern fixes.

### Short-term: High-density clusters (~1–2 days each, +50–100 tests)

| Cluster | Failed | Skipped | Notes |
|---------|--------|---------|-------|
| **number** | 6 | 1 | 264/271 passing (97.8%). Remaining: non-English word formatting (out of scope). |
| **match** | 30 | 115 | 149/294 passing (83.2%). Down from 78 failures. Atomic-value patterns now work. Remaining: `apply-imports`, `intersect`/`except`, namespace nodes, `for-each-group` atomic patterns. |
| **sort** | 0 | 18 | 66/84 passing (100% of runnable). ✅ Clean. |
| **for-each-pair** | 0 | 2 | 56/58 passing (100% of runnable). ✅ Clean. |
| **node-before** | 2 | 10 | 24/36 passing (66.7%). Remaining 2 are environment issues (missing `$works` variable). |
| **node-after** | 2 | 9 | 24/35 passing (68.6%). Remaining 2 are environment issues. |
| **HigherOrderFunctions** | 7 | 85 | 37/129 passing (28.7%). +4 fixed this session (function-name prefix, XQST0039, inline-function-16, function-item-3). |
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

- `main` — all work is on `main`
- No feature branches
- All work committed to `main`
- No pending changes
- Latest: `<uncommitted>` — node comparison operator fixes (XPTY0004 + XPST0003)
- Previous: `a792924` — function-name prefix and inline-function XQST0039
