# TA-26 remaining production scope

5 November 2026. Source stays **1.70.0**. This is an exact-candidate reconciliation, not a new engine implementation, semantic adoption, requirement waiver or release acceptance. The full **25,831-test** qualification and conformance/API evidence remain [TA-25 evidence](../foundation-public-compatible/README.md); this audit does not relabel them as a new run.

The sealed accepted ledger has **43 owner/performance requirements, 14 Core decisions and six closure obligations**. Every row is mapped to current bounded behavior, remaining work and an exit condition. No row is waived or marked fully implemented on the strength of a subset or a historical provider receipt. [Requirement matrix](REQUIREMENTS.json), [finite plan](PLAN.json).

The compiled inventory contains **40 of the 47 proposed type names**. Seven remain absent: SchemaAtomics, SchemaXQueryCompiler, SchemaXsltCompiler, SchemaXsltExecutable, SchemaXsltInvocation, SchemaXsltCaptureResult and SchemaXsltRunResult. Presence of the other forty types is declaration evidence, not signature/semantic/backend acceptance. The added compatible result is recorded separately. [Exact declaration inventory](DECLARATIONS.json).

All six proposed XdmValue numeric member intents are declared in production. That does not imply exact legacy parsing/arithmetic/casts/map-key/serialization or all-host behavior. The [compiled witnesses](evidence/NUMERIC-INTENTS.json) distinguish declaration completion from remaining numeric coverage and the recorded default-literal source migration.

## Finite implementation queue

| Work | Concrete remaining boundary |
|---|---|
| AH-01, next | Shared engine-owned host/executable/operation bridge using the existing public session and raw lifetime. |
| AH-02 | Public XQuery compilation/evaluation entry with captured effective prolog/settings. |
| AH-03 | Five public XSLT types and actual BCAP transform/capture/outcome routing. |
| PV-01 | Missing public provenance observation and complete selected list/union/container semantics. |
| NU-01 | Remaining exact numeric pipeline coverage; preserve explicit bounded profiles and refusals. |
| SC-01 | Remaining assessment/authority/CTA/node-copy/predicate integration and supported provider coverage. |
| R4-01/02 | One immutable actual consumer candidate and representative agreed workload budgets. |
| R5-01, PK-01 | Exact-candidate owner/freeze disposition, concrete packages/consumer checks and release authorization. |

The [next host bridge](NEXT-HOST-BRIDGE.md) records a concrete bounded implementation target and HC01-HC16, all NotRun. Existing isolated BCAP source/control receipts inform the implementation; their compiled binaries and old whole-source replacements are not production integration. No sibling repositories, product pins, owner messages or private Schema material are changed.

A preview can be prepared before final freeze once its immutable checkpoint/version is selected. Current metadata alone is not an artifact or package-consumer check. Stable publication remains gated by remaining Core scope, actual consumer/performance evidence and exact owner/artifact decisions. No reliable date is inferred. [Status](STATUS.json), [input pins](INPUTS.json), [verifier](tools/verify.py).
