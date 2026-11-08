# Repeated decimal implementation review checkpoint — 29 October 2026

The [Schema 0.7 repetition candidate](../schema-repeated-implementation/README.md) has been reviewed before delivery. No additional production defect was found. Every changed implementation/test/project source hash matches the qualified 334-case source. The original implementation dossier is sealed capture-time evidence; its uncommitted notices do not describe the later delivery here.

## Reviewed boundaries

- Unsigned bounded occurrence parsing cannot overflow; per-particle and aggregate maxima remain sixteen. Required/optional schemas retain v1/v2 and repetitions select v3.
- Matching checks all contiguous counts and order before values/assertions. Every occurrence gets distinct state referencing the correct original declaration; no instance is deduplicated by name/value.
- The closed-grammar cardinality filter requires a standard structured namespace/code or exact plain InvalidOperationException type with anchored XPTY0004 prefix. Foreign namespaces, misleading text, other codes and CLR failures do not become assertion failures. Cancellation/out-of-memory propagation and actual short-circuit behavior remain intact.
- Singular projection requires one actual match and refuses ambiguity before choosing XML/value state. Native/XQuery/XSLT copies preserve occurrence order, ownership and original authority; stripped copies remove it.
- Public overloads/layouts, core production code and product pins remain unchanged. The one prior maxOccurs-two coverage expectation update is explicitly adopted and preserved; 259 other prior expectations still pass.

Existing qualification: 334 complete Schema passes (74 new), zero-warning/error Schema source-override build and repeated-document example. Core 24,214 passes and exact QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets) remain inherited after full source/configuration checks. No fresh core/gate run is claimed for this review. Earlier raw receipts and sealed dossiers remain unchanged.

## Paired checkpoint and next scope

The user requested continuation of review, commit/push and count-scoping recommendations. Candidates remain Schema 0.7/core 1.34.1. CHECKPOINT.json records paired delivery identities and targets; core resolver: `git log -1 --format=%H -- docs/schema-repetition-review/README.md`. Core goes to existing `checkpoint/restart-2026-10-29` for draft PR #467; Schema goes to `feature/bounded-document-assessment`. No merge, tag, package publication, general API freeze or product adoption.

Next: separately adopt and plan the standard fn:count function in schema assertions. No count assertion grammar is implemented by this review or its checkpoint.

Reviewed Schema 0.7 delivery: `a462022a852575f84f0cc504f86771deaa118e0e`, remote-verified on `origin/feature/bounded-document-assessment` with a clean tree. Core delivery resolver: `git log -1 --format=%H -- docs/schema-repetition-review/README.md`; target `origin/checkpoint/restart-2026-10-29`. No merge or release.
