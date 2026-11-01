# Composite operation ownership decision

1 November 2026. Selected Core implementation direction following [independent-owner qualification](../foundation-independent-owners/README.md). This is a design checkpoint: production remains 1.39.0 at `f72db224`; no new runtime behavior, public signatures, owner acceptance or API freeze is established. The [contract](CONTRACT.md) makes the lifetime and failure rules concrete. [Acceptance cases](ACCEPTANCE.json) remain **NotRun**.

Choose a **session-owned immutable operation lease set over host-owned resources**. The host acquires source resources and opens individual admissions. The session takes one operation lease per admitted snapshot before calling work, rolls back partial lease acquisition, and retains the complete set until raw work finishes. The host still owns providers, stores and workers. This follows the existing proposed same-session `Run`/`RunAsync` admission array rather than adding a new public resource group.

| Option | Benefit | Cost / disposition |
|---|---|---|
| Host coordinates every raw-operation lease | No internal group implementation | Every host must reproduce atomic admission, rollback and deferred cleanup; rejected for the common engine boundary |
| Session acquires and owns provider/store resources | Central orchestration | Needs new heterogeneous acquisition/disposal capabilities and worker policy; deferred |
| Host acquires resources; session retains the operation lease set | Common admission and failure rules; provider ownership stays explicit | Requires bounded membership and acquisition rollback inside the session; selected |

The next implementation slice is a private lease-set primitive with failure injection, followed by integration into typed operation capture and all three hosts. Keep the existing production single-snapshot path until those tests prove membership, rollback, revocation and complete failure preservation. Production multi-source execution is not implemented by this document.

## Scope and performance

The first slice admits existing same-session snapshots under the session's bound provider. Multiple original assessments remain separate and evidence retains its exact originating snapshot. Two separately compiled sessions cannot be combined. Independent issuer routing and compatible-universe views remain unsupported until explicitly designed and qualified; equal descriptors, QNames or hashes confer no authority.

Copy membership once into a bounded array at operation entry; use reference identity for duplicate detection. Retain one lease per unique snapshot, including snapshots used only by global context. Do not clone node graphs, eagerly materialize outputs, enumerate complete persistent stores, or allocate a fresh membership set per atomic result. Checking all owners costs O(number of admitted snapshots) per guarded access; characterize that cost before selecting an optimization. Limits must be supplied, not inferred from synthetic component allocations.

Schema N5/N7/N8 require original assessment identity and borrowed value lifetime. XmlDb FXG-02/03/05 require deferred access, reader/generation retention and supported cancellation/concurrency. Braid BF140-02/03 require distinct input provenance and explicit worker ownership. These [recorded requirements](../foundation-c45-handoff136/README.md) motivate the contract; they do not constitute owner acceptance. No external checkout or owner request was changed or sent.

The current private XSLT factory captures match/global nodes at compilation. Joint execution must additionally prove both inputs belong to the current operation's admission set before reading them. That is a concrete integration gap, not a claim that the existing trusted predicate already supplies snapshot membership.

## Review evidence

[Input pins](INPUTS.json) hash the exact source, prior proposal and requirement records reviewed. The source baseline already passed 24,341 unit tests and frozen QT3/qt4 gates in [its sealed qualification](../foundation-private-host-factories/README.md). Those are historical receipts; no full solution build or conformance runner was repeated for this documentation-only decision. [Basis](SPEC-BASIS.md) separates private lifetime policy from language specification adoption.

Read-only verification: `python -B docs/foundation-composite-ownership/tools/verify.py`. It checks input hashes, unchanged production files, the selected decision identifier and unexecuted acceptance status. It does not execute the proposed contract.
