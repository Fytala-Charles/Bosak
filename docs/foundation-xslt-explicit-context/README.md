# Explicit XSLT context bridge candidate

1 November 2026. An isolated XSLT-only candidate implements the context-presence gap found by the [host adapter](../foundation-host-operation-adapter/README.md). Canonical source, qualified production DLLs, public declarations, version, owner pins and frozen baselines are unchanged. The [specification basis](SPEC-BASIS.md) preceded candidate generation and records the later bounded-profile adjustment.

The candidate builds with zero warnings/errors. An unchanged caller runs twice with original XSLT and twice with candidate XSLT, while all other dependencies retain qualified hashes. All seven legacy controls are identical and pass. All 22 candidate cases pass, including those seven controls and 15 explicit-path controls. [Results](evidence/RESULTS.json), [candidate receipt](evidence/candidate-0.json), [repeat](evidence/candidate-1.json), [legacy receipt](evidence/legacy-0.json) and [source/binary pins](evidence/INPUTS.json) identify the actual executed cohorts.

## Candidate change

Only two copied source files change: [TransformEngine patch](evidence/TransformEngine.patch) and [XsltExecutable patch](evidence/XsltExecutable.patch). The original Transform signature delegates to an internal core with legacy fallback enabled. A new internal TransformWithExplicitContext bridge disables fallback. Null explicit global input means absent; required-context checks use that input; inherited dynamic focus is cleared before globals; named-template focus defaults to effective global input. Legacy named-template rule dispatch remains confined to the legacy branch. The bridge uses the existing stack runner and captures result documents without publishing a new public entry point.

| Explicit-path controls | Observed result |
|---|---|
| Match document A, global document B | A\|B |
| Named template, match A, global B | B\|B from template focus/global variable |
| Named template with only element global B | B |
| Match A with absent global input | A\|absent; direct global-context reference raises caught XPDY0002 |
| Named template with absent global input | Caught XPDY0002 for context access |
| Source-free constant template | 42 |
| Required global with no match but global B | B |
| Required global with only match A | Existing implementation diagnostic XTDE3086 |
| Stylesheet-declared absent global, supplied B | B ignored; A\|absent |
| Caller dynamic focus C, absent explicit global | C does not leak; A\|absent |
| Template declares absent context | Caught XPDY0002 for context access |
| Named template also declares a match pattern | Uses explicit global B for named invocation |
| Non-document initial-match input | Explicit NotSupportedException in this bounded bridge |

The first candidate passed 21/22: detached element initial matching reached a caught NullReferenceException. Its [receipt](evidence/first-candidate-21-of-22.json), input identity and bridge patch remain separate. The final bridge keeps initial match restricted to document roots and refuses other match node kinds before execution. Element global input passes. This does not fix or qualify general element matching and does not extend the proposed public profile automatically. The initial compile error for an omitted stack-size argument is retained separately.

## Scope and acceptance

The pinned source interpretation is an opt-in policy. Legacy behavior remains intentionally unchanged, including the existing missing-required diagnostic; the basis explicitly distinguishes that implementation code from normative specification evidence. The legacy document/element focus observations remain as recorded in the [earlier correction](../foundation-host-operation-adapter/FOCUS-CORRECTION.md).

Full metadata comparison against the qualified eleven-assembly inventory finds identical XSLT declarations: 157 exported types and 2,961 declaration/evidence rows, with zero new public exports. See [comparison](evidence/API-COMPARISON.json) and [candidate metadata](evidence/candidate-api.json). This does not prove binary equivalence, caller performance or full engine correctness.

This candidate is not promoted. No public C45 facade, authenticated schema binding, owner resource lease, dynamic input graph admission, exact numeric implementation or recursive detachment is supplied. Atomic global input, arbitrary initial item sequences, separate named-template context overrides and streaming remain outside this node-only bridge. Caller dynamic-context restoration and complete resolver/package/resource behavior need qualification. C45, consumer acceptance and API freeze remain pending.

Next qualify this candidate with the existing XSLT tests, fn:transform regressions and required frozen gates before preparing a production promotion. Preserve the exact candidate/source cohort and distinguish legacy checks from explicit-path tests. Existing full unit/conformance receipts remain historical; they were not rerun for this bounded experiment and are not attributed to this candidate.

Replay in order: `python -B docs/foundation-xslt-explicit-context/tools/build.py`, `python -B docs/foundation-xslt-explicit-context/tools/run.py`, `python -B docs/foundation-xslt-explicit-context/tools/audit.py`. Read-only verification: `python -B docs/foundation-xslt-explicit-context/tools/verify.py`. Negative cases are caught; subprocesses and stack execution are bounded by the runner timeout with scoped process error mode. No machine-wide error-reporting change or deliberate unhandled crash occurred.
