# Handover — W3C QT3 Conformance Test Work

**Date:** 2026-05-22  
**Commit:** `TBD` on `main` (pushed to origin)  
**Baseline before this session:** `62ebb16` (50.57% pass rate)

---

## Current Status

| Metric | Before Session | After Session | Delta |
|--------|---------------|---------------|-------|
| **Total tests** | 31,821 | 31,821 | — |
| **Passed** | 16,093 | **TBD** | **+TBD** |
| **Failed** | 5,660 | **TBD** | **-TBD** |
| **Skipped** | 10,068 | 10,068 | — |
| **Pass rate** | 50.57% | **TBD** | **+TBD pp** |

**All 651 unit tests pass.** ✅

**Cast/CastableExpr subset:** 3322/4026 passed (82.51%), up from 3047/3896 (78.21%).

---

## What Was Done in This Session

### Cast Conformance Fixes

#### 1. Date → DateTime Cast
- `xs:date` cast as `xs:dateTime` now works (sets `T00:00:00`)
- `xs:time` to `xs:date`/`xs:dateTime` properly rejected

#### 2. Mixed-Duration Rejection
- `yearMonthDuration`/`dayTimeDuration` `TryCast` now rejects mixed strings (e.g. `-P1Y1M1DT1H1M1.123S`) via `IsMixedDuration`
- Cast from existing `Duration` values still allowed (extracts appropriate component)

#### 3. HexBinary Validation
- Added even-length check, whitespace stripping, uppercase output
- Fixed constructors to validate instead of passthrough

#### 4. Base64Binary Validation
- Fixed padding check (rejects `F===`)
- Strips whitespace before validation

#### 5. Float Negative Zero
- `FormatXPathDouble` preserves `-0`
- `xs:float("-0.0E0")` cast to `xs:string` now yields `-0`

#### 6. Double/Float Canonical Formatting
- `FormatXPathDouble` normalizes scientific notation (strips leading zeros in exponent, ensures `E` not `E+`)
- `ResultComparer.SerializeSingle` now uses canonical XPath formatting via `value.ToString()`

#### 7. Extended-Year g* Extraction
- Added `XPathDateTime` struct to replace `DateTimeOffset` for XPath date/time values
- Supports XML Schema extended years (negative, year 0000, >9999)
- `gYear`/`gYearMonth`/`gMonthDay`/`gDay`/`gMonth` extraction from `DateTime`/`Date` now uses `XPathDateTime` directly

#### 8. anyURI Validation
- Replaced strict `Uri.IsWellFormedUriString` with `IsValidAnyUri`
- Collapses whitespace, rejects invalid percent-encoding

#### 9. Derived String Types
- `normalizedString` replaces CR/LF/tab with space
- `token` collapses whitespace (trim + collapse runs)
- `NCName`/`Name`/`NMTOKEN`/`language` use Unicode-aware regex and collapse whitespace

#### 10. Constructor Functions
- Updated `XsGDay`, `XsGMonth`, `XsGYear`, `XsGYearMonth`, `XsGMonthDay`, `XsNCName`, `XsName`, `XsLanguage`, `XsNormalizedString`, `XsToken`, `XsID`, `XsIDREF`, `XsNMTOKEN`, `XsENTITY`, `XsDuration`, `XsHexBinary`, `XsAnyUri`, `XsDayTimeDuration`, `XsYearMonthDuration` to use `VmEngine.Cast` for shared validation logic

#### 11. Whitespace Handling
- `TryParseXPathDateTime`/`Date`/`Time` and duration parsers now strip surrounding whitespace before validation

---

## Remaining Top Failure Patterns (Next Steps)

| Pattern | Count | Recommendation |
|---------|-------|----------------|
| Lexical validations (`FORG0001` expect/succeed mismatches) | ~200+ | Scattered numeric/string/duration edge-case regex/parsing rules |
| Schema type tracking | ~40 | `gYear`/`gYearMonth` stored as `String` kind; casts to `anyURI`/`base64Binary`/`hexBinary` cannot be rejected without tracking original schema type |
| Parser errors (`XPST0080`/`XPST0003`) | ~40 | Invalid/unknown type names in cast expressions |
| Hex/B64 cross-casting | ~6 | `base64Binary`→`hexBinary` requires actual decode/re-encode |
| Date overflow | ~3 | Very large year values exceed `DateTimeOffset` range via legacy paths |
| Year 0000 vs -0000 | ~2 | Negative zero representation mismatch |
| Float formatting edge case | ~1 | `12678968f` serializes with wrong precision |
| QName formatting | ~2 | Expanded `Q{uri}local` vs prefixed serialization |
| List types | ~4 | `IDREFS`, `NMTOKENS`, `ENTITIES` constructors not implemented |

### Most Impactful Next Targets

1. **Lexical validations (~200+ failures)** — Many small regex/parsing fixes across numeric, string, and duration types. High volume, likely many easy wins.
2. **Schema type tracking (~40 failures)** — Requires storing original schema type in `XdmValue`. Medium architectural change.
3. **Parser gaps (~40 failures)** — Invalid type name errors in cast expressions.

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

# Run only Cast/CastableExpr tests (~2-3 min)
cd tests/Bosak.XPath.Conformance
dotnet run --configuration Release -- "D:/Development/Bosak/tests/qt3tests" "Cast"
```

### Files You Will Touch Most

| File | What to change |
|------|---------------|
| `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` | Add/modify `fn:*`, `map:*`, `array:*`, `xs:*` functions |
| `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` | Casting (`TryCast`), arithmetic, type checking, opcodes |
| `src/Bosak.XPath.Core/Xdm/XdmValue.cs` | Value representation, formatting, accessors |
| `tests/Bosak.XPath.Conformance/ResultComparer.cs` | Add new assertion types |
| `tests/Bosak.XPath.Conformance/ConformanceRunner.cs` | Runner behavior (filtering, limits, reporting) |

---

## Known Issues / Gotchas

1. **Stdout buffering** — When redirecting conformance output to a file, .NET buffers heavily. Use `Console.Error` for real-time progress, or just wait the full 12-13 minutes.
2. **File locks** — If a previous conformance run is still running (check `tasklist | grep -i conformance`), builds will fail with "process cannot access the file". Kill lingering processes before building.
3. **Date/time edge cases** — `XPathDateTime` now handles extended years, but legacy `DateTimeOffset` accessors (`DateTimeValue`, `DateValue`, `TimeValue`) still throw for out-of-range years.
4. **`array:sort`** deep-equal — `fo-test-array-sort-002/003` fail with "Expected: (array), Got: (array)". This is a deep-equal comparison issue for nested arrays, not a sort correctness issue.
5. **Negative zero** — `double.IsNegative(value)` is used to detect `-0`; `value == 0.0` alone is not sufficient.

---

## Branches

- `main` — all work is on `main`, pushed to `origin/main`
- No feature branches created in this session
