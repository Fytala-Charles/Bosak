# Public original-node nilled profile (TA-14)

5 November 2026. Candidate **1.63.0**, based on ec6daa0e plus the exact uncommitted [TA-13 prerequisite](../foundation-node-metadata-reader/README.md) pinned in INPUTS.json. Explicit assessor-bound `SchemaQuerySession.CompileXPath` now supports the exact `nilled($node)` / `fn:nilled($node)` grammar under `bosak-original-node-nilled-v1` and its distinct descriptor.

[Adopted basis](SPEC-BASIS.md) | [Machine status](STATUS.json) | [NP01-NP24 mapping](CASE-MAPPING.json) | [Exact candidate](evidence/SOURCE-AND-BINARY-PINS.json) | [API comparison](evidence/API-COMPARISON.json)

Evaluate with exactly one unprefixed `node` variable, bound to a direct owned node or explicitly to Undefined, absent focus and the supported closed dynamic context. Empty/known non-element input returns Undefined after raw/advisor checks; an original element returns a plain Boolean. Source ownership precedes NodeKind or annotation getters. Original facts must be issued by the bound advisor for the exact original node/input/assessment, with owned type/declaration handles. Missing original element capabilities refuse; malformed evidence rejects. No typed-value/container/iterator, legacy schema getter or type-family lookup is used. Each query rereads authenticated metadata; no facts cache is introduced.

The original acceptance selection is historical. CASE-MAPPING.json records current tests for all 24 cases, including preserved full-reader/metadata-mode controls and existing ordinary/identity/numeric regressions. Original commands, failures, TRX, allocations and loaded engine identities are retained. The first build found an incorrect test-only sequence constructor; it was corrected before the successful focused run. No intentional unhandled negative caller was launched.

Fresh qualification passes **25,306 tests**, including **76 new public cases** and the 116 retained original-reader cases. QT3 **31,397/0/424** and gated qt4 **8,941/0/1,918** retain exact skip maps. Final solution and LanguageServer builds have zero warnings/errors; exported API remains 11 assemblies / 310 types / 6,369 declarations.

Synthetic public cost is **31,848 bytes per warm query**, **36,128 bytes per separate raw operation**, **15,257 provider fixture callbacks** and **five metadata getters per query**. The callback count includes provider availability and binding/issuance checks, not just metadata getters. The measurement exposes required guard-cost attribution/reduction before publication; it does not establish native latency or an accepted owner budget. All queries intentionally revalidate metadata rather than reuse a facts cache.

No public signatures, enum/layout or friend grants change. This is bounded public execution with controlled fixtures, not native provider or owner performance acceptance. Data atomization, source-provenance output policy/FOTY0012, construction, compatibility, all-host/migration, R4 qualification, C45/freeze and publication remain separate work.

Next attribute/reduce the measured public guard fanout and allocations while preserving revocation/error semantics. Then select typed atomization output/provenance and finite quota/cache/lifetime policy before implementing its exact separate profile. [Release readiness](../release-planning/README.md) distinguishes a checkpoint push and preview package preparation from stable publication. Candidate version 1.63.0 is not automatically the release target.

Verify the exact candidate with `python docs/foundation-public-node-nilled/tools/verify.py`.
