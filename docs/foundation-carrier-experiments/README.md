# E-01/E-02 isolated carrier results ? 31 October 2026

**Neither representation is ready to freeze.** The accessor-aware reference-slot overlay retains the current 48-byte XdmValue layout and preserves diagnostic markers in the tested Core containers, but allocates a holder per annotated item and adds an unwrapping branch to reference-backed reads. The separate struct wrapper is cheaper to construct in this probe but cannot pass its marker through existing XdmValue-only APIs after unwrapping. A deliberately unsafe slot replacement fails four reference-backed access controls.

This advances the first two [planned experiments](../foundation-contract-design/EXPERIMENTS.json) with **partial engineering evidence**. It is not full E-01/E-02 completion, general provenance semantics, real provider qualification, ABI acceptance or performance-budget acceptance. Historical design/experiment plans remain unchanged. [RESULTS.json](RESULTS.json) is the current returned evidence.

## Reproducible isolation

The runner archives exact Core source `c88eea4308c0e49a81e7bfe2f481319f2e4d208c`, then rebuilds four standalone cohorts under ignored `.guard-tmp/carrier-experiments`. No production source or external owner checkout is edited. These source-only builds use custom minimal project metadata; they are not the qualified seven-package binaries and do not establish released assembly compatibility.

- **baseline:** unchanged archived Core source.
- **naive:** diagnostic holder in the existing reference slot, with original accessors. This is the intentional bad variant.
- **overlay:** the same holder plus payload-unwrapping reads throughout the cloned XdmValue. No instance field is added. Two diagnostic public helper methods and a private holder/property exist only in this clone.
- **wrapper:** unchanged Core plus a 56-byte `(XdmValue, marker)` struct in the probe.

Markers are shared plain objects identifying the diagnostic origin. They are not Schema component authority, PSVI or a security mechanism. List markers illustrate empty/singleton list-boundary retention only; no actual Schema list/union assessment or validation is executed. A CLR function carrier test is not VM function dispatch.

`python docs/foundation-carrier-experiments/tools/run.py` rebuilds and executes the experiment, replacing this dossier?s raw unsealed outputs; run it in a copy to preserve the sealed receipt. The source archive, exact patches, generator, runner, build logs, environment overrides and raw JSON are retained. All four final builds pass with zero warnings/errors. Fourteen processes cover two reversed cohort orders and sizes 1,000/100,000 (the intentional bad variant runs only at 1,000). Every measured operation has three warmups and seven samples. Tiered compilation and ReadyToRun are disabled for this controlled comparison; that is not production configuration advice.

## Deterministic observations

| Observation | Result |
|---|---|
| Outer XdmValue size | 48 bytes in all four x64 builds; the separate wrapper is 56 bytes. This runtime observation is not a portable ABI guarantee. |
| Naive reference-slot access | String, decimal, QName and dateTime controls fail as expected; boolean/integer/double inline reads still work. |
| Overlay access and direct transport | Seven sampled atomic access/string forms work; the original marker survives array, map-value, singleton sequence and CLR function-carrier return. A different marker is not mistaken for the original. |
| XPath equality/hash/map keys | Sampled integer/string/decimal/QName/dateTime values compare/hash and retrieve keys like the unannotated controls using XdmValueEqualityComparer. |
| CLR default equality | Annotated integer default ValueType.Equals differs from the original due to its reference field. XPath comparer success does not waive .NET equality or consumer compatibility; audit actual usages before acceptance. |
| Separate wrapper at existing boundaries | The wrapper retains its marker locally; passing its unwrapped XdmValue through existing arrays/maps loses the marker. A complete wrapper-based API/VM design is still missing. |
| Newly constructed integer | No diagnostic provenance appears automatically. This is an observation, not adoption of arithmetic/cast provenance laws. |
| List boundaries | Explicit empty/singleton list markers distinguish them from an atomic view. Exporting only the atomic items loses the singleton-list distinction. |

There are 49 distinct diagnostic control IDs in the normal cohorts, reused across modes/sizes/rounds; the bad variant executes the 21 accessor/marker controls. These repeated observations must not be added up as independent test coverage. Expected losses and the intentional bad variant are controls, not claimed production regressions.

## Allocation and pilot timing

The construction operation copies a prebuilt integer into a fresh array; each annotation reuses the same existing marker. Value construction and marker allocation are outside the timed region. GC counters and allocated bytes are recorded per sample.

| Entries | Unchanged XdmValue array | Annotated overlay array + holders | Separate wrapper array |
|---:|---:|---:|---:|
| 1,000 | 48,024 bytes | 80,024 bytes | 56,024 bytes |
| 100,000 | 4,800,024 bytes | 8,000,024?8,000,048 bytes | 5,600,024 bytes |

The small case isolates roughly **32 extra allocated bytes per annotated item** for this holder, versus **8 extra bytes per wrapper entry**. Both coexist with existing payload allocations where those payloads are reference-backed; these figures are not total query memory or retained-memory measurements. Small variation in the large overlay allocation counter is preserved in raw results.

Raw samples and per-process medians cover construction, annotated/ordinary string access and integer access. The overlay construction is slower in this pilot, and reference-backed default paths now test for a holder. Do not infer production latency ratios from these synthetic loops: repeated scalar reads may be optimized/hoisted, the wrapper read path accesses its already unwrapped payload, samples are few, the host is uncontrolled, and GC/scheduling varies. No p95 service target, statistical significance, peak-memory result or default-path regression budget is claimed. Allocation and representation losses are the stronger findings here.

## Decision and next work

Do not choose the unsafe slot replacement. Keep the accessor-aware overlay as an experiment only; do not accept a per-item allocation or ordinary-path branch without representative PF-01?09 budgets. Investigate shared metadata and more selective holder use without merging component authority, list boundaries or resource lifetime. Any batching/cache alternative must be bounded and justified by actual owner workloads.

Before selecting a carrier, run an actual isolated VM vertical slice through atomization, casts, comparison/map keys, nested values, function dispatch and explicit serialization/copy boundaries, with immutable adoption for any new semantic preservation rules. Audit .NET default equality and all direct payload consumers, not only XdmValue's public accessors. Compare allocation alternatives and default paths on actual Schema/XmlDb/Braid workloads. E-03 authority granularity and E-04 operation/session lifetime remain unrun.

Initial harness corrections are retained: a syntax error in the JSON reporting line, and an array control using zero instead of the API?s one-based index. Neither was a product defect. A handled failure exit and 120-second process timeout prevent failed probes hanging on this host. The first timing pilot lacked per-sample GC counters; its source/raw observations remain in `initial-pilot-without-gc-counters.zip`, separately from the final runs. No product assertion was changed to make a result pass.

Run `python docs/foundation-carrier-experiments/tools/verify.py` for data-only receipt verification. Production tests/gates were not rerun: production source is unchanged, and the private clone is not qualified as an engine release. C139-01/C45 acceptance, production implementation, performance qualification and API freeze remain Pending.
