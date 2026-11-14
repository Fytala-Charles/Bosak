# Scalar materialization and call-site observation — 14 November 2026

This local diagnostic observes unchanged qualified 1.81.9 binaries, checkpoint `af52e427ea122cdeef70ad125a90492ed585cc9e`, with engine/runner source identical to main `cef6df3720cc30cda98bf96ac7a7f6d8c3975a7e`. It makes no engine, API, conformance, fixture, exclusion, version or production runner change. Raw binaries remain in the ignored local packet; this dossier is not published.

PR [490](https://github.com/Fytala-Charles/Bosak/pull/490) merged as `cef6df37` after all six checks and independent required-artifact verification: 10,221/0/4,380, all 14,601 identities/exclusions, 818.969 seconds, original 1200-second/8-GiB limits. The post-merge standalone run 43573964047 remains pending at this checkpoint; other five checks succeeded.

## Method and observations

Review was recorded before diagnostic implementation/execution. A newly compiled private diagnostic binds actual VM methods through cached strongly typed delegates against the exact nine qualified DLLs; no reflection invocation or result boxing occurs inside measured loops. This is not the retained foundation LegacyCaller. One launch completed in 0.904 seconds under a 60-second watchdog and 512-MiB Windows job limit, .NET 10.0.11 with roll-forward disabled. All nine DLL hashes agree before/after. Build: zero warnings/errors. All 22 protective checks pass; expected lazy failures are caught inside the diagnostic, with catch-all Main and scoped process error mode.

Each row measures calling-thread allocated bytes across 100,000 calls after 10,000 warm-up calls; inputs/delegates are prepared outside the loops. These are separate helper/whole-call observations, not exclusive CPU, throughput, full-workload allocation or a causal explanation of hosted timing. First successful launch is retained; no unchanged rerun.

| Route | Bytes per call |
|---|---:|
| array/scalar/integer | 72 |
| view/scalar/integer | 72 |
| array/scalar/string | 72 |
| view/scalar/string | 72 |
| array/scalar/boolean | 72 |
| view/scalar/boolean | 72 |
| array/scalar/decimal | 72 |
| view/scalar/decimal | 72 |
| array/scalar/double | 72 |
| view/scalar/double | 72 |
| array/undefined | 0 |
| view/undefined | 0 |
| array/materialized-array | 0 |
| view/materialized-array | 0 |
| array/materialized-list | 120 |
| view/materialized-list | 0 |
| array/singleton-sequence | 392 |
| view/singleton-sequence | 320 |
| array/empty-sequence | 32 |
| view/empty-sequence | 32 |
| strict-comparison/primitive-integers | 0 |
| general-comparison/primitive-integers | 1008 |
| function-conversion/matching-integer | 112 |
| function-conversion/integer-to-double | 888 |
| strict-comparison/materialized-singletons | 0 |
| function-conversion/materialized-singleton | 496 |

## Source interpretation and next slice

`VmEngine.MaterializeSequence` (line 4003) and `MaterializeSequenceView` (line 5973) both create a singleton array for primitive scalars. A blanket view substitution therefore saves no scalar allocation. For materialized lists the view reuses mutable backing storage, while the array helper copies it: the mutation check proves that replacing the copy changes snapshot behavior. Array-backed sequences are reused by both. Lazy sources are consumed once with the same recorded MoveNext/fault order in these examples. Typed-node identity and list-valued atomization/conversion/general comparison are preserved in four positive checks; these bounded checks do not prove every semantic edge.

Strict primitive comparison already bypasses materialization and allocates zero here. General primitive comparison (`CompareGeneral`, line 8558) builds enumeration/atomization machinery and two temporary buffers; its whole-call allocation is 1,008 bytes. This does not assign all bytes to those buffers. Matching integer conversion returns through its existing type-match path yet still allocates 112 bytes, so no zero-allocation conversion claim is warranted. Promotion and sequence conversion have different costs.

Recommended next implementation proposal: a narrowly guarded primitive-atomic general-comparison route before sequence enumeration/buffers, with both inputs already primitive and existing coercion/error behavior preserved. First record immutable normative specification sections and deviations, then add happy/edge regressions for operator/coercion/empty/NaN/untyped behavior and prove node/list/lazy/function/array inputs keep the existing route. No such implementation is included here. Avoid altering general materialization, list aliasing, node atomization or lazy error order. Require full build/tests, exact frozen QT3, gated qt4 and pinned standalone qualification for an engine change; no suite-wide speed claim without a comparable measurement.

## Retained evidence

REVIEW.json predates Probe.cs and execution. RUN.json binds source/binary and nine engine hashes, command and limits. RESULT.json has every allocation row and protective check. Probe.cs, project, setup/controller and build/stdout/stderr are copied verbatim from the ignored local packet `.guard-tmp/scalar-materialization-20261114`. Setup/controller retain their actual local paths and are not advertised as a portable runner. MANIFEST.json binds these files. No package export or new publication authorization is assumed from the earlier PR490 approval.
