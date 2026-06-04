# Handover — Bosak XPath/XSLT Implementation

**Date:** 2026-06-04
**Commit:** `cb67d7c`
**Current focus:** QT3 conformance quick-wins (timezone, sort collation, trace, implicit-timezone).

---

## Project Status Overview

### QT3 Conformance (W3C XPath 3.1 Test Suite)

- **Passed:** 18,695 / **Failed:** 3,175 / **Skipped:** 9,951 (31,821 total)
- Pass rate: **58.75%**
- Runner completes all 428 test sets without crashes

### XSLT Conformance (W3C XSLT 3.0 Test Suite)

- **Passed:** 3,257 / **Failed:** 2,204 / **Skipped:** 9,139 (14,600 total)
- Pass rate: **59.6%**

### Unit Test Status

- **867 unit tests pass** across 7 test projects (0 failures)

---

## Recommended Next Steps

### Immediate: QT3 quick-wins (~1 hour, +15–25 tests)

These are clusters with clear, related root causes:

1. **`default-language`** — 6 failures. `fn:default-language#0` not implemented. Stub to return `"en"`.
2. **`element-with-id`** — 5 failures. `fn:element-with-id#1` not implemented. Similar to existing `id#1`.
3. **`filter` / `for-each` / `for-each-pair`** — ~15 failures. Missing `XPTY0004` type-checking when function argument has wrong arity/return type.
4. **`contains-token`** — 2 failures. Token matching edge cases.
5. **`document-uri`** — 4 failures. Returning boolean-as-string (`'fals'` / `'tru'`) instead of proper substrings.

### Short-term: Sort cluster deep dive (~2–3 hours)

The remaining 5 sort failures need investigation:
- `fn-sort-22` / `array-sort-023`: Sorting maps with `map:get(?, "key")` returns empty sequence
- `fn-sort-17`: NaN in array sorting (may need stable sort implementation)
- `fn-sort2-str-2`: Inline function `as` keyword (`function($x as type) as type { ... }`) not supported by parser
- `fn-sort-spec-6`: Variable-length sequence keys ordering wrong

### Medium-term: High-density XSLT clusters

| Cluster | Failed | Notes |
|---------|--------|-------|
| **match** | 78 | Pattern matching gaps |
| **mode** | 88 | Template mode dispatch |
| **copy** | 80 | `xsl:copy`, `xsl:copy-of` behavior |
| **date** | 68 | Known `DateTimeOffset` limitation, but many others fixable |
| **for-each-group** | 62 | Implementation gaps |
| **key** | 63 | `key()` / `xsl:key` behavior |

---

## How to Build & Test

```bash
# Build entire solution
dotnet build Bosak.sln

# Run all unit tests
dotnet test Bosak.sln

# Run QT3 conformance suite (~3 min, Exe project)
cd tests/Bosak.XPath.Conformance
dotnet run --configuration Release -- "../../tests/qt3tests"

# Run specific QT3 cluster only
dotnet run --configuration Release -- "../../tests/qt3tests" "timezone"

# Run W3C XSLT 3.0 conformance suite (full catalog)
dotnet run --project tests/Bosak.XPath.Xslt.Conformance/Bosak.XPath.Xslt.Conformance.csproj
```

---

## Known Issues / Gotchas

1. **Inline function `as` keyword** — Parser doesn't support type declarations in inline functions (`function($x as xs:int) as xs:int { $x + 1 }`). Root cause of `fn-sort2-str-2` and ~12 other QT3 failures.
2. **Map sorting returns empty** — `fn:sort` / `array:sort` with `map:get(?, "key")` key function return empty sequences. Needs investigation.
3. **Stable sort** — `List<T>.Sort` is unstable. XPath `fn:sort` requires stable sort for equal keys.
4. **`key()` namespace** — Registered under `http://www.w3.org/2005/xpath-functions` (not XSLT namespace).
5. **DateTime year < 1** — `DateTimeOffset` minimum year is 1. Tests using year `-2` cannot pass without switching to a custom date representation.
