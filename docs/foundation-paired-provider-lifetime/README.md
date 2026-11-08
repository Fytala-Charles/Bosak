# TA-22 paired-provider lifetime prerequisite

5 November 2026. Development candidate **1.67.0**, uncommitted on ec6daa0e538fe3c4e2ebc54177a1daeda5834b38. This implements the private resource mechanics selected by [TA-21](../foundation-compatible-view-policy/README.md). **Public compatible views remain inactive**, CV01-CV32 remain NotRun and CV-ENTRY-01 remains proposed, not adopted.

## Implemented boundary

`Operation.CallProviderPair` selects the exact ordered source/target lifetime objects. Immutable single and paired modes share one serialized call window, completion task and raw drain. A constructor-selected single mode cannot expand into a pair, and a pair cannot change order/members or switch to single mode. Identical references acquire once; this is resource deduplication, not schema compatibility evidence. Acquisition order is source then target; release order is target then source, followed by the existing raw leases.

A first paired acquisition/retention failure rolls back every successful admission before returning to user code. The failed pair cannot reopen within that raw operation, including after cleanup failure; a fresh raw operation is required to retry. This conservative internal refinement avoids reopening uncertain resources and bounds successful retained membership to two. Existing constructor single-advisor failure behavior remains unchanged. Work failures after a successfully retained pair preserve admissions until raw completion.

Raw callback closure immediately denies new work/delivery. Actual Enter, delegated work and mandatory cleanup tails retain the shared call ticket and raw resources until they finish. Wait cancellation does not release actual work; raw cancellation is cooperative. Raw availability is checked between individual endpoint lease callbacks, so closure from a source callback stops further target callbacks. No provider callback or release executes under the scope state gate. Original primary and cleanup exceptions keep their identities, order and supplied aggregate boundaries.

The internal neutral `OpenRetainedSourceSnapshot` derives the exact input from an engine-created original snapshot in a distinct session. It acquires the original snapshot's real lifetime before invoking source input availability. This protects a source check blocked while its owner closes. Source session/snapshot closure revokes target access; target closure does not dispose a still-live original host owner. Active target raw work retains the original real resource through provider and raw cleanup. Duplicate input/quotas use existing units and rollback reservations; retained-source chains/same-session imports refuse. The target receives no schema association, typed registry, copied original assessment or compatibility grant. This helper is not a public admission API.

## Qualification

**25,650/0** unit tests, including **40 new internal controls** and **157 legacy lifetime/construction controls**, pass. MC01-MC16 have [internal prerequisite coverage](CASE-MAPPING.json). Separate exclusive file streams prove source/target admission retention, and an end-to-end internal paired call plus neutral source retention releases target advisor, source advisor and original owner in order. Blocked source input checks, acquisition rollback, cancellation/closure, quotas and original cleanup failures are exercised on actual raw operations/admissions.

MC06 covers the shared delegated-call mechanism; actual ISchemaCompatibilityAuthority.Establish/view callbacks are not integrated. MC15 covers neutral source-retainer transactions/composition; future public proof admission and publication are still CV cases, not qualified by this slice. No native backend/threading, actual owner budgets or public compatible semantics are claimed.

QT3 **31,397/0/424**, gated qt4 **8,941/0/1,918** plus 11 experimental passes retain exact skip maps. Eleven-assembly public metadata remains **310 exported types / 6,369 declarations**, with unchanged declarations, references, friend grants and recorded value-field shape. Release and separate LanguageServer builds pass; actual unit/gate bindings match the sealed candidate. Source/configuration/corpus/test/binary identities, original failures and retries are retained in [evidence](evidence/SOURCE-AND-BINARY-PINS.json).

The initial fixture build failed with CS9105; its following no-build run had zero matching tests. That zero-test success is explicitly rejected as qualification. The reviewed 39-case run was intermediate; the final 197-control focused run covers all 40 new and 157 legacy cases. Complete unit/gate runs supply final qualification. No historical failure is overwritten.

## Package and next step

The package metadata preflight is refreshed for **1.67.0**: eleven projects, exact seven-library XPath API closure and the explicit TA-21 NOTICE overlay. Twenty-two qualified source/package inputs differ from HEAD. This is metadata evaluation, not a package build/restore/signature or publication result. The [preview recipe/migration inventory](../foundation-compatible-view-policy/packaging/RECIPE.md) still needs an immutable source/assets/recipe checkpoint and an explicitly selected preview version before artifact qualification.

Next record a scoped disposition of [CV-ENTRY-01](../foundation-compatible-view-policy/SIGNATURE-PROPOSAL.md) before public handoff implementation and CV01-CV32 qualification. Broader retained/persistent/list/general/parse-budget/all-host, actual native/consumer workloads and C45/freeze/publication gates remain open. No public signatures/mandatory provider slots, schema authority, package, owner acceptance, external pin change, commit or push is introduced.

Run `python -B docs/foundation-paired-provider-lifetime/tools/verify.py` for exact qualification verification. Earlier dossiers remain sealed historical evidence. The new metadata preflight never builds, packs, restores or publishes.
