# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-04

## Commit

`d5a9c50` (typed function signature matching)
`29db567` (handover docs update)

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | `fn-sort-spec-6` fix — removed global `NormalizeSequence` from `Execute()` | `VmEngine.cs` | Committed |
| 2 | Sort key atomization via `Data()` in `Sort()`/`ArraySort()` | `FunctionLibrary.cs` | Committed |
| 3 | `fn-for-each-pair-017` fix — whitespace preservation + `function(*)` matching | `VmEngine.cs`, `FunctionLibrary.cs`, `XDocumentProvider.cs` | Committed |
| 4 | Typed function signature matching for `instance of function(T...) as R` | `VmEngine.cs` | Committed |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (867 tests across 7 projects — 0 failures)
- [x] QT3 sort cluster: 66/0/18 (100% of runnable)
- [x] QT3 for-each-pair cluster: 56/0/2 (100% of runnable)
- [x] QT3 filter cluster: 32/0/15 (100% of runnable)
- [x] QT3 HigherOrderFunctions cluster: 33/11/85 (was 26/18/85)
- [x] Full QT3 baseline: 18,758 / 3,112 / 9,951 (58.94%) — latest full run
- [x] Estimated current baseline (partial runs): ~18,765 / 3,105 / 9,951 (58.97%)

## Next Recommended Work

### Immediate quick wins (highest impact / lowest effort)

1. **`function-item-8`** — `fn:function-name` formatting:
   - Expression: `function-name(fn:abs#1)`
   - Expected: `fn:function-name`, Got: `function-name`
   - Root cause: `FunctionName_1` returns only local name, missing namespace prefix
   - File: `FunctionLibrary.cs` (~1 hour)

2. **`inline-function-12a`** — Duplicate parameter name error not raised:
   - Expected: `XQST0039`, but succeeded
   - Root cause: parser doesn't validate duplicate param names in inline functions
   - File: `XPathParser.cs` (~30 min)

3. **`inline-function-16`** — Closure variable scope issue:
   - Error: `Variable $foo is not defined`
   - Expression: `function($x as xs:integer) { function() { $x, $foo } }`
   - Root cause: nested inline function can't access outer inline function params
   - File: `IrLowerer.cs` or `VmEngine.cs` (complex, ~2–4 hours)

### Short-term cluster targets

4. **` HigherOrderFunctions` remaining** (11 failures):
   - `hof-025`, `function-item-1`: `concat#123456`, `concat#64` not found — arity limit issue
   - `hof-033`: `Cannot access FunctionValue on XDM value of kind 'Sequence'` — function lookup on sequence
   - `hof-916`, `hof-917`, `function-item-3`, `xqhof7`: missing `XPTY0004` type errors
   - `function-item-4`: missing `FOTY0013` error

5. **`number` cluster** (10 failures, 96.2% passing):
   - `number-0111`: large integer overflow — already fixed? verify
   - `number-0802/0812/0813/0828/0829/2506`: non-English word/ordinal formatting
   - `number-0807`: double `1e100` formatting — already fixed? verify
   - `number-1004`: `xsl:iterate` not implemented
   - `number-1501`: whitespace stripping in `level="any"` count

6. **`boolean` cluster** (2 failures, 98% passing):
   - `boolean-076`, `boolean-077`: node ordering `<<`/`>>` — `PrecedesNode`/`FollowsNode` unimplemented?

### Medium-term

7. **Sequence constructor batching** — Refactor `ExecuteSequenceConstructorDirect` for §5.7.1 complex content rules. Fixes 13 seqtor failures.
8. **`match` cluster** (78 failures) — pattern matching gaps.
9. **`for-each-group`** (62 failures) — `xsl:for-each-group` implementation gaps.
