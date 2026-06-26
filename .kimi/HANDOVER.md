# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-06-26

## Commit

`712730a` (HEAD) + uncommitted WIP on `use-when` / static-expression infrastructure

## What Was Built

| # | Change | Files | Status |
|---|--------|-------|--------|
| 1 | Fixed unit-test regression: XTSE1660 check in `ValidateInstructionTree` now only applies to XSLT elements, not literal result elements with a normal `type` output attribute. | `src/Bosak.Xslt/Stylesheet/Stylesheet.cs` | Done |
| 2 | Fixed `type` cluster regressions: `InstanceOf` now recognizes parameterised and unprefixed sequence type names (`item()`, `element(*, xs:anyType)`, `attribute(*, T)`, etc.) without throwing XPST0051. | `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` | Done |
| 3 | Verified `use-when` cluster remains clear and unit tests pass. | — | Done |

## Current Branch

`main`

## Test Status

- [x] All unit tests pass (894 tests across 8 projects — 0 failures)
- [x] `use-when` cluster: **99/102 passing, 0 failed, 3 skipped** ✅
- [x] `type` cluster: **58/79 runnable passing, 0 failed, 21 skipped** ✅
- [ ] `static` cluster: **23/49 passing, 26 failed** — still work-in-progress
- [ ] Full W3C XSLT 3.0 suite: **4,619 passed / 632 failed / 9,349 skipped** (88.0%)

## Notes

- The uncommitted WIP completely cleared the `use-when` cluster (was 73 passed / 26 failed on HEAD).
- It also regressed the `static` cluster (was 27 passed / 22 failed on HEAD) and left the full suite ~47 passes below the committed baseline of 4,666/585.
- The immediate regressions caused by the WIP have been fixed; the remaining `static` cluster failures are part of the original incomplete static-variable work.

## Next Recommended Work

Choose one of the following:

1. **Commit the `use-when` win separately** — tease apart the use-when improvements from the static-variable work, revert/disable the incomplete static processing, and commit a clean use-when-clearing change.
2. **Continue the `static` cluster** — debug static variable / import-precedence propagation so the 26 remaining failures clear.
3. **Pivot to a different cluster** — e.g. `available-system-properties` (26 failures) or `xml-version` (27 failures) — after stashing or reverting the current WIP.

Recommended pick: **option 1** (use-when is already green and self-contained).
