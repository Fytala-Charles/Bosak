<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak engine">
  <br><br>
  <h1>Basic Sweep Triage — the 26 XSLT 3.0 Conformance Failures</h1>
  <p>Root-cause analysis and 1.0 recommendation for the last basic-mode sweep failures</p>
</div>

> **Status:** Triaged (2026-10-05) — all 26 formally documented and skipped with reason in the harness (basic mode only) · **Basis:** main @ `4e66e92` (v0.12.3-beta) · **Baseline:** `.guard-tmp/work/slice3-basic-final.log` (2026-10-03) — Passed 10,250 / Failed 26 / Skipped 4,325 of 14,601

---

## 1. Summary

The newest basic sweep (2026-10-03, single chunk, `.guard-tmp/work/slice3-basic-final.log`) reports **10,250 passed / 26 failed / 4,325 skipped**. Every one of the 26 failures has the identical symptom:

```
FAIL <test>: XTSE1650: xsl:import-schema requires a schema-aware processor
```

All 26 were re-run in isolation on current main (`docs/basic-sweep-triage` @ `4e66e92`) and reproduce exactly. All 26 are **schema-aware tests whose catalog entries lack a `schema_aware`/`schema-import` dependency declaration**, so the harness's feature gate does not skip them in basic mode. On a non-schema-aware processor, XSLT 3.0 §27.2 (carried over from XSLT 2.0 §3.14.2) **requires** `xsl:import-schema` to be rejected statically with XTSE1650 — Bosak's behavior is spec-correct. No basic-mode processor can produce these tests' expected outcomes.

| Test | Catalog / test-set | Failure symptom | Root-cause group | Recommended action |
|------|--------------------|-----------------|------------------|--------------------|
| stream-107, stream-108, stream-109 | source-document | XTSE1650 (expected success: `/out = '9.06'` / `'true'`) | A — schema-aware test, missing catalog dependency | Document as known limitation — **done** (basic-only skip, harness 3.72) |
| non-stream-107, non-stream-108, non-stream-109 | source-document | XTSE1650 (same expectations via env `non-stream-B`) | A — schema-aware test, missing catalog dependency | Document as known limitation — **done** |
| si-fork-001 … si-fork-009 | si-fork (tests/strm) | XTSE1650 (expected success: assert-xml) | A — schema-aware test, missing catalog dependency | Document as known limitation — **done** |
| si-fork-901, si-fork-902 | si-fork (tests/strm) | Expected error XTSE3430, got XTSE1650 | B — expected static error unreachable behind XTSE1650 | Document as known limitation — **done** |
| si-map-001 … si-map-006, si-map-008 | si-map (tests/strm) | XTSE1650 (expected success: assert-xml) | A — schema-aware test, missing catalog dependency | Document as known limitation — **done** |
| si-map-007 | si-map (tests/strm) | Expected error XTDE3365, got XTSE1650 | B — expected dynamic error unreachable behind XTSE1650 | Document as known limitation — **done** |
| si-map-009 | si-map (tests/strm) | Expected error XTTE3375, got XTSE1650 | B — expected dynamic error unreachable behind XTSE1650 | Document as known limitation — **done** |

**Mapping to the AGENTS.md documented platform limitations:** none of the 26 map to the three known platform limits (DateTime year < 1, .NET decimal 28–29 digit precision, remote-HTTP/Cloudflare). Those limits account for other suite entries (e.g. the schema-aware tail's single failure, `type-functions-0401` — FODT0001 for year −12, already documented). The 26 are a **catalog annotation gap**, not an engine gap.

## 2. Root-cause group A — schema-aware test without a catalog dependency (22 tests)

**Tests:** stream-107/108/109, non-stream-107/108/109, si-fork-001…009, si-map-001…006, si-map-008.

**Evidence.**
- Each test's resolved stylesheet contains `xsl:import-schema`: `tests/insn/source-document/stream-B.xsl:11` (imported by both the `stream-B` and `non-stream-B` environments; the environment also declares `<schema role="stylesheet-import" file="books.xsd"/>`), `tests/strm/si-fork/si-fork-A.xsl:11` (imports `../docs/loans.xsd`), `tests/strm/si-map/si-map-A.xsl:11` (same).
- The catalog test cases declare **no** `<dependency type="feature" value="schema_aware"/>` (nor `schema-import`), so the harness `SkipFeatures` gate does not skip them in basic mode (catalog scan 2026-10-05, all 26 entries `deps=[]`).
- Basic-mode compile therefore raises XTSE1650 — mandated by XSLT 3.0 §27.2 — and the expected success result is unreachable. W3C submissions for this class concur (same precedent as the long-documented `json-to-xml-typed-010` skip).
- **Schema-aware cross-check:** in `--schema-aware` mode all 20 si-fork/si-map tests in this group **pass**; the 6 stream/non-stream tests are skipped there because `books.xsd` uses XSD 1.1 `xs:assert`/`xs:alternative` (REQ-112 documented limit, engine is XSD 1.0). So the engine demonstrably handles these tests correctly when schema-awareness is on.

## 3. Root-cause group B — expected error unreachable behind spec-mandated XTSE1650 (4 tests)

**Tests:** si-fork-901, si-fork-902 (expect static XTSE3430, non-streamable stylesheet), si-map-007 (expects dynamic XTDE3365), si-map-009 (expects dynamic XTTE3375).

**Evidence.**
- Same dependency-gap root cause as group A (stylesheets contain `xsl:import-schema`; catalog declares no schema feature dependency).
- XTSE1650 is a **compile-time** error; it fires before the stylesheet's streamability analysis (XTSE3430) or any runtime evaluation (XTDE3365/XTTE3375) can be reached. On a basic processor the expected error is unreachable by construction — the same spec-contradiction class as the already-skipped `json-to-xml-typed-010` (XTDE3245 expectation).
- **Schema-aware cross-check:** all 4 pass in `--schema-aware` mode.

## 4. Disposition taken (harness 3.72, 2026-10-05)

The 26 tests were added to a new `BasicOnlySkipTests` set in `tests/Bosak.Xslt.Conformance/Program.cs` (version 3.72), consulted only when `--schema-aware` is **off**, each with a `GetSkipReason` explanation citing this document. This follows the established `json-to-xml-typed-010` / `error-1160a` skip-with-reason precedent and is the harness's formal mechanism for documented known limitations. Being basic-mode-only, the closed schema-aware sweep (11,054/1/3,546) is untouched.

**Gate evidence (targeted re-runs, Release build of `docs/basic-sweep-triage` @ `4e66e92`):**

| Test-set | Mode | Before (slice3 baseline) | After (3.72) |
|----------|------|--------------------------|--------------|
| source-document | basic | 40 passed / **6 failed** / 13 skipped | 40 / 0 / 19 |
| si-fork | basic | 44 / **11** / 0 | 44 / 0 / 11 |
| si-map | basic | 3 / **9** / 0 | 3 / 0 / 9 |
| source-document | schema-aware | 40 / 0 / 19 | 40 / 0 / 19 (unchanged) |
| si-fork | schema-aware | 55 / 0 / 0 | 55 / 0 / 0 (unchanged) |
| si-map | schema-aware | 12 / 0 / 0 | 12 / 0 / 0 (unchanged) |

Zero pass→fail regressions; the 26 failures moved to documented skips. Expected full-sweep basic result after 3.72: **10,250 / 0 / 4,351**.

## 5. Recommendation for the 1.0 milestone

1. **Accept the 26 as formally documented known limitations** (upstream catalog annotation gap; engine behavior is spec-mandated). No engine work is warranted — fixing these "in" basic mode would require violating XTSE1650.
2. **Run one confirmation full basic sweep** (`.guard-tmp/work/resilient-sweep-slice3-basic.sh` pattern) on the commit that carries harness 3.72 to record the expected 10,250/0/4,351 before tagging 1.0. The full sweep was not re-run during this triage (hours-long); the gate above covers the three affected test sets in both modes.
3. **Optionally report the catalog gap upstream** (w3c/xslt30-test): add `schema_aware`/`schema-import` dependencies to these 26 test cases so other basic processors' harnesses skip them too.
4. The **schema-aware tail's single failure** (`type-functions-0401`, FODT0001 for year −12) is already covered by the documented DateTime platform limit in AGENTS.md — no new action.
5. No changes to AGENTS.md Known Limitations were needed: the 26 are harness/sweep bookkeeping, and the engine limits already documented (DateTime year < 1, decimal precision, remote HTTP/Cloudflare) were not implicated by any basic-sweep failure.
