# Proposed K02 guarded-runner adoption

**Pending owner approval — 27 October 2026. Current normal runner and baseline are unchanged.**

Decision ID: `K02-guarded-runner-adoption-v1`. Source candidate **1.26.8**, published version **1.26.3**.

Approve enabling the existing guarded query-owned schema initialization switch on the **normal QT3 runner**, and update the exact QT3 requirement in [AGENTS.md](../../../AGENTS.md) from **31,144/0/677** to **31,145/0/676**.

## Concrete change

The [proposed patch](PROPOSED-ADOPTION.patch) changes only:

1. `ConformanceRunner._executor` initialization to `new() { UseQueryOwnedSchemasForDiagnostics = true }`, plus its dated history row. The internal switch keeps its existing name; `TestExecutor` itself stays default-off for other callers. The existing structural/profile guards are retained verbatim.
2. AGENTS.md's exact QT3 totals and supersession note, preserving the historical K01 approval.

No `KnownSkips` or `DocumentedSkips` entry is removed. `validateexpr-28` currently becomes a skip when eager environment preparation throws `XmlSchemaException`. Guarded query-owned preparation lets its actual query report XQST0012, which its unchanged original `any-of` assertion explicitly permits. This is an assertion pass through the error alternative; it does not certify successful duration-derived schema validation.

The proposal enables the existing guard for structurally eligible cases, rather than adding a test-name exception. It affects 324 directly compared schema cases in the pinned catalog. It preserves eager handling for validated sources, environment expressions, explicit schema profiles, module imports, missing/ambiguous mappings and unavailable import metadata. XPath routing, dependency filtering, heavy-set policy, retry/timeout settings, fixtures and assertions stay unchanged.

## Complete rehearsal evidence

| Full actual-runner mode | Passed | Failed | Skipped | Total |
| --- | ---: | ---: | ---: | ---: |
| Current unchanged mode | 31,144 | 0 | 677 | 31,821 |
| Guarded rehearsal | 31,145 | 0 | 676 | 31,821 |

Two independent processes execute the actual runner against the original catalog under external 300-second watchdogs. Each starts from the unchanged default-off runner; the guarded process selects its existing flag through harness-only reflection. No engine fields, source or binaries are patched. The dedicated 512-MB worker stack and QTTEST environment variables match the normal entry point.

The exact skip delta is **only `validateexpr-28`**. No new skip appears, and all **676 retained names and reasons are identical**. Both processes finish successfully without timeouts. The earlier 324-case paired audit is all green, with all 17 declined-boundary cases unchanged after the two core corrections.

The source-identical K02-C02 validation is inherited: **21,842 solution tests pass** across complete project runs (nine isolated projects plus the 269-case normal-layout Runtime rerun), and the qt4 224-set gate is **8,436/0/1,894**, including 11 existing Experimental passes. This review does not claim new unit or qt4 runs. The [review results](RESULTS.json) and [evidence manifest](evidence/manifest.json) retain all receipts and identities.

## Approval scope and acceptance

Approval authorizes applying this scoped harness/baseline change, synchronizing current documentation, rebuilding the normal harness and rerunning the **complete normal QT3 gate** against the revised requirement. Adoption is complete only if the result is exactly **31,145/0/676**, the full skip delta remains only the named case, and relevant regression checks pass. Any unexpected outcome stops adoption; no assertion or guard may be relaxed to reach the target.

Approval does not authorize a release, push, tag, external merge, fixture correction, XSD 1.1 backend, new public API, broader dependency admission or removal of another skip. Candidate 1.26.8 remains unpublished. REQ-126's owner-accepted bounded catalog remains Complete with its original technical limits.

**Bosak.Schema REQ-009 owns staged XSD 1.1 implementation.** Explicit profile exclusions and all six substitution-group preparation exceptions remain unchanged. This proposal does not cover the 108 schema-bearing inventory exclusions or establish applicability beyond the pinned catalog. Richer identity for non-file/wrapped resolver streams is separate work. Any future public seam for the add-on follows its separate compatibility ADR.

Explicit approval is required because AGENTS.md currently mandates the exact frozen **31,144/0/677** totals. The successful rehearsal does not change that policy by itself.
