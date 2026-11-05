# K04-C08: radix-picture and dependent-function review
**K04-C09 radix formatter - 27 October 2026:** [Bounded radix implementation](../k04-radix-fix/README.md) fixes14 formatter-only originals and preserves the two caret-grouping cases. The exploratory set improves to **100/1/1**;case012 now exposes missing parse-integer#2 and stays failed/ungated. New68 regression rows pass (53failed before);all22,111 solution tests pass. Frozen gates/skips remain **31,161/0/660** and **8,438/0/1,892**. The4.0-only path preserves3.1 behavior and existing public Int64 input bounds. Explicit pre-implementation adoption uses the frozen qt4cg revision;candidate **1.27.0** unpublished/public1.26.3. No harness,fixture/assertion,GatedSets,schema or QT3 baseline change. Bosak.Schema owns staged XSD1.1;REQ-126 remains Complete. Next: separate parse-integer contract review. Earlier K04 notices are historical checkpoints.


27 October 2026. Source base **65d1a731**; candidate **1.26.12** unpublished, public **1.26.3**.

**Review complete; no implementation or admission change.** The 15 exploratory failures split into **14 formatter-only cases and one format/parse round-trip case**. A formatter implementation can address the first group, but cannot by itself supply the missing parse-integer function used by the round trip. [Next-slice scope](NEXT-SLICE.md) records the bounded implementation and its gates.

## Fresh original and profile evidence

All **17 original format-integer-40 cases** run through actual TestExecutor40/ResultComparer under separate20-second watchdogs:15fail with FODF1310; two existing decimal-caret cases014/015 pass. Every original query/environment/assertion is preserved. A fresh complete102-case normal exploratory-set run remains **86passed/15failed/1skipped**; process exit0 does not make the exploratory failures pass.

**34 derived controls** cover17 expressions in each QT3/qt4 profile. All17 QT3 controls pass their positive or refusal assertions. In qt4, eight pass and nine fail, preserving the missing radix behavior and the separately unavailable parse-integer#2. The controls include decimal caret grouping, case/grouped hex output, original-integer ordinal suffixes, empty input, Int64 endpoints, invalid-range/mixed-case errors, standalone parse calls and function lookup. There are **52 bounded processes**, zero timeouts:17 originals,34 controls and one exploratory run.

## Specification and implementation boundaries

[SPECIFICATION-REVIEW.json](evidence/SPECIFICATION-REVIEW.json) pins the exact frozen qt4cg [function catalog revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml), SHA256409221fb91752e0a174a00f3ef6aecd7bf69807bf203e15f91e4d420145e39e5. This matches the existing REQ-126 changed31 review source. It is a review pin, not new semantic adoption or current-draft certification.

- The reviewed radix prefix is ASCII2..36 without a leading zero. Caret marks a radix only with a valid preceding radix and a later x/X in the primary picture. Otherwise existing decimal grouping remains relevant. Cases014/015 already demonstrate this compatibility boundary.
- With a radix, x/X supply mandatory positions, case controls alphabetic digits, and existing optional/padding/grouping requirements remain. A second caret may be a grouping separator. These concerns belong to one bounded formatter path.
- The current FormatIntegerEngine has no such path: the leading digits enter its decimal-pattern validator, where x/X triggers FODF1310. The existing grouping helpers are reusable mechanisms, not proof that radix pattern validation is implemented.
- The existing ToOrdinal heuristic examines rendered digits. Original case017 requires21 formatted as hexadecimal15 to become **15st**, not15th. Future radix ordinals must retain original-value semantics and distinguish numeric output from words.
- Standalone parse-integer('ff',16) and parse-integer('101',2) raise **XPST0017** in the reviewed frozen4.0 profile; function-lookup#2 is empty. Case012 currently masks this dependency behind its earlier formatting error. The separate parse-integer catalog contract needs its own review/implementation.
- FunctionLibrary.FormatInteger currently narrows input to long even though its helper exposes a BigInteger overload. The first formatter slice must state that existing Int64 boundary and must not promise arbitrary-precision input support.

Invalid-range and mixed-case negative controls currently pass because the whole radix form is unsupported; they do not establish correct future radix validation. Derived ordinal/control expectations are review probes, not newly adopted conformance tests. All failed diagnoses remain in [RESULTS.json](RESULTS.json).

No source, harness, fixture/assertion, GatedSets, version or baseline changes. The source-identical **22,043 solution passes**, normal QT3 **31,161/0/660**, qt4 gated **8,438/0/1,892** and11 Experimental passes are explicitly inherited from the accepted base, not fresh full-suite/gate runs. The [manifest](evidence/manifest.json) protects this review and all pinned prior artifacts. Bosak.Schema retains staged XSD1.1 ownership; REQ-126 remains Complete for its accepted bounded catalog. No release, push, tag, Braid change or upstream submission occurs.

```powershell
python docs/conformance/k04-radix-review/build-review.py
python docs/conformance/k04-radix-review/evidence/validate-evidence.py
```
