# Handover — W3C QT3 Conformance Test Work

**Date:** 2026-05-22  
**Commit:** `62ebb16` on `main` (pushed to origin)  
**Baseline before this session:** `620a33f` (48.02% pass rate)

---

## Current Status

| Metric | Before Session | After Session | Delta |
|--------|---------------|---------------|-------|
| **Total tests** | 31,821 | 31,821 | — |
| **Passed** | 15,279 | **16,093** | **+814** |
| **Failed** | 6,341 | **5,660** | **-681** |
| **Skipped** | 10,201 | 10,068 | — |
| **Pass rate** | 48.02% | **50.57%** | **+2.55 pp** |

**All 653 unit tests pass.** ✅

---

## What Was Done in This Session

### 1. Fixed Build Regression
- `VmEngine.cs:1651` — `IReadOnlyList.Length` → `.Count`

### 2. Removed NamedFunctionItem Over-Strict Validation
- Recent inline-function work added `ValueMatchesXdmKind` checks on **all** named function calls
- This broke valid XPath semantics where functions like `fn:concat` accept any atomic type (via atomization/casting)
- **Fix:** Removed the strict parameter-type gate for `NamedFunctionItem`; implementations handle their own coercion
- Fixed `PartialApplicationTests.Apply_Concat_ViaLookup` and similar HOF regressions

### 3. Removed 50-Set Cap on Conformance Runner
- `ConformanceRunner.cs` had `if (processedSets >= 50) break;`
- Now runs all **428 test sets** (~31,800 tests)
- Run time: ~12-13 minutes in Release mode

### 4. Added `assert-xml` Support to Conformance Runner
- `ResultComparer.cs` now handles `<assert-xml>` assertions
- Serializes actual result via `IXdmNode.ToXmlString()`
- Supports `ignore-prefixes="true"` by stripping `xmlns` declarations before comparison
- Unlocks `analyze-string` and other XML-comparison tests

### 5. Added Missing `xs:*` Constructors
- `xs:gYear#1`, `xs:gYearMonth#1`, `xs:gMonthDay#1`
- String passthrough pattern (same as existing `xs:gDay` / `xs:gMonth`)

### 6. Added Missing Map/Array Functions
- `map:entry#2`
- `map:for-each#2`
- `array:append#2`, `array:insert-before#3`, `array:subarray#2/#3`, `array:reverse#1`, `array:join#1`
- `array:filter#2`, `array:fold-left#3`, `array:fold-right#3`, `array:for-each#2`, `array:for-each-pair#3`
- `array:sort#1/#3`, `array:flatten#1`

### 7. Added Missing `fn:*` Functions
- `fn:tokenize#1` (default pattern `\s+`)
- `fn:sort#1/#2/#3` (sequence sort with optional collation and key function)
- `fn:innermost#1`, `fn:outermost#1`
- `fn:resolve-uri#1/#2`
- `fn:lang#1/#2`
- `fn:parse-ietf-date#1`
- `fn:format-integer#2/#3` (alphabetic, roman, words, zero-padded decimal)

### 8. Fixed Duration Arithmetic
- `Add` / `Subtract` — `Duration + Duration`, `Duration - Duration`
- `Multiply` — `Duration * number`, `number * Duration`
- `Divide` — `Duration div number`, `Duration div Duration` (ratio as decimal)
- `Negate` — `-Duration`
- Added `FormatYearMonthDuration` helper

### 9. Fixed `fn:min` / `fn:max` for Non-Numeric Types
- Now handles `xs:date`, `xs:time`, `xs:dateTime`, `xs:duration`, and `xs:string` comparisons
- Added `CompareDateTimeValues` helper

### 10. Fixed `fn:reverse` Array Handling
- `Materialize` was auto-unwrapping arrays (treating `[1,2,3]` as sequence `(1,2,3)`)
- `fn:reverse` now uses `AsSequence` which preserves arrays as single items

### 11. Added FORG0001 Validation
- `xs:decimal` — rejects exponent notation (`-0.0E0`)
- `xs:base64Binary` — validates length is multiple of 4, valid chars, correct padding
- `xs:anyURI` — validates via `Uri.IsWellFormedUriString`

---

## Remaining Top Failure Patterns (Next Steps)

| Pattern | Count | Recommendation |
|---------|-------|----------------|
| `assert-true` got `false` / `assert-false` got `true` | 731 | Scattered function bugs; investigate by test-set |
| Parser errors (`Unexpected token LParen`, `Name`, `LBrace`, `LessThan`, `KeywordAs`) | ~730 | **Structural** — missing syntax constructs |
| Expected `XPTY0004` but succeeded | 316 | Type-checking gaps (inline func params, sequence cardinality) |
| `fn:format-number#2` not found | 226 | **Biggest single win left** — needs picture-string engine |
| Expected `FORX0002` but succeeded | 187 | Regex validation gaps |
| Expected `FORG0001` but succeeded | ~180 | More constructor validation (integer whitespace, dateTime leniency, etc.) |
| `fn:json-to-xml`, `fn:parse-json` missing | ~90 | JSON parsing / XML conversion |

### Most Impactful Next Targets

1. **`fn:format-number#2`** (~150 recoverable of 226) — Implement XPath picture strings (`#`, `0`, grouping, percent, per-mille, decimal separator)
2. **Parser gaps** (~500+ recoverable of 730) — Likely 2-3 missing constructs (element constructors `<elem>`, `typeswitch`, `group by`, arrow edge cases)
3. **`assert-true`/`assert-false` failures** (731) — Many may be root-caused by the same underlying bugs (e.g., collation handling, timezone edges)

---

## How to Run the Conformance Suite

```bash
# Build
dotnet build Bosak.sln

# Run all unit tests
dotnet test Bosak.sln

# Run full W3C QT3 suite (~12-13 min in Release)
cd tests/Bosak.XPath.Conformance
dotnet run --configuration Release -- "D:/Development/Bosak/tests/qt3tests"
```

### Files You Will Touch Most

| File | What to change |
|------|---------------|
| `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` | Add/modify `fn:*`, `map:*`, `array:*`, `xs:*` functions |
| `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` | Casting (`TryCast`), arithmetic, type checking, opcodes |
| `tests/Bosak.XPath.Conformance/ResultComparer.cs` | Add new assertion types |
| `tests/Bosak.XPath.Conformance/ConformanceRunner.cs` | Runner behavior (filtering, limits, reporting) |

---

## Known Issues / Gotchas

1. **Stdout buffering** — When redirecting conformance output to a file, .NET buffers heavily. Use `Console.Error` for real-time progress, or just wait the full 12-13 minutes.
2. **File locks** — If a previous conformance run is still running (check `tasklist | grep -i conformance`), builds will fail with "process cannot access the file". Kill lingering processes before building.
3. **50-set cap removed** — The full suite is now ~31,800 tests. Don't panic if it seems slow; it's just big.
4. **Date/time edge cases** — `24:00:00` and negative years (`-0002`) are still rejected by our `DateTimeOffset.TryParse`-based cast. These need custom handling.
5. **`array:sort`** deep-equal — `fo-test-array-sort-002/003` fail with "Expected: (array), Got: (array)". This is a deep-equal comparison issue for nested arrays, not a sort correctness issue.

---

## Branches

- `main` — all work is on `main`, pushed to `origin/main`
- No feature branches created in this session
