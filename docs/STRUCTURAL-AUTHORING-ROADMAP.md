<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala operation-specific structural authoring roadmap">
  <br><br><h1>Structural authoring operation roadmap</h1>
  <p>Planning catalogue — 15 October 2026</p>
</div>

**Consumer design clearance — 20 October 2026:** [Braid review](REQ-135-CONSUMER-DESIGN-REVIEW.md) reports no blocking findings or required contract correction at `c01537066cb991b1eea2b1e51a046215a70ed46f`. Braid explicitly approves preparing one implementation PR containing all three operations within this profile. Its 24 passing published-1.22.0 inspection/runtime checks are baseline evidence, not new attribute-authoring tests; initial harness version failures and provenance limits are retained. All three exact-head CI checks passed. Product-owner implementation authorization remains the next agreed gate; no implementation, version or publication is performed by this import.

**20 October 2026:** REQ-134 is [consumer-accepted in 1.22.0](REQ-134-BRAID-ACCEPTANCE.md). [REQ-135 literal attribute lifecycle](REQ-135-ATTRIBUTE-LIFECYCLE-CONTRACT.md) is proposed for Braid design review: Create, Update and Delete together in one implementation PR, 23 planned groups, no implementation or version authorized. [Review handoff](REQ-135-CONSUMER-DESIGN-HANDOFF.md). Earlier pending acceptance/planning notes below are historical.

**1.22.0 published and verified - 19 October 2026:** [Public delivery proof](RELEASE-1.22.0-PUBLICATION.md) records immutable `v1.22.0` at `3b009388`, one successful Release run (10,010 solution tests and ten uploads), ten verified signed archives and fresh nuget.org-only **153 broad /531 focused** checks with exact provenance and clear audits. REQ-134 literal text instruction deletion is published after [Braid final implementation review](REQ-134-CONSUMER-IMPLEMENTATION-REVIEW.md) found no blockers. Braid adoption/native acceptance remain pending; REQ-133 remains consumer-accepted. Earlier source/review-pending checkpoints below are historical.

**REQ-134 design proposed - 19 October 2026:** [Literal text instruction deletion contract](REQ-134-TEXT-DELETION-CONTRACT.md), [28 planned acceptance groups](REQ-134-TEXT-DELETION-ACCEPTANCE.md) and [Braid design handoff](REQ-134-CONSUMER-DESIGN-HANDOFF.md) propose separate whole-instruction authority, complete removed payload evidence and bounded whitespace-merge provenance. [Issue #381](https://github.com/Fytala-Charles/Bosak/issues/381) is registered for design only; ten existing public inspection/runtime probes inform the plan, zero text-deletion tests or new capabilities are delivered. Version TBD, owner unassigned; implementation/publication require later authorization. REQ-133 remains consumer-accepted in public 1.21.0.


## Historical planning checkpoints

**Current consumer checkpoint - 18 October 2026:** Braid accepts bounded text insertion in public1.19.0; see the [unchanged receipt](REQ-131-BRAID-ACCEPTANCE.md). Expression/value-of/text operations and navigation are delivered foundations for the larger bidirectional builder. Free-form editing and full live graph/source synchronization remain in development. [Public ecosystem overview](ECOSYSTEM.md). Historical planning checkpoints below retain their original context.


**1.19.0 published and verified - 18 October 2026:** Immutable `v1.19.0` at27691fdb supplies bounded REQ-131 literal-text insertion. [Public delivery proof](RELEASE-1.19.0-PUBLICATION.md) records one successful Release run (9,726 tests/ten uploads), ten verified signed archives and fresh nuget.org-only153 broad/282 focused checks with exact provenance and clear audits. REQ-131 Braid adoption/functional/native acceptance remains pending; accepted1.18.0 pin and extension are unchanged. Earlier preparation-only statements below are historical.

**REQ-130 accepted ? 17 October 2026:** Braid formally accepts bounded value-of deletion in published **1.18.0**, source `3264fcfaf739e492770c882d2cbfc1f93f0172d2`. The [unchanged consumer acceptance receipt](REQ-130-BRAID-ACCEPTANCE.md) reports 1,015 normal Release tests and 241 fresh public-package checks (overlapping evidence), exact nine-assembly provenance, and product-owner native completion with matching hashes. These are reported consumer results, not engine reruns. Exact literal hashes/native encoding variants were not supplied. Acceptance is limited to the delivered profile; REQ-131 is separate. Earlier pending/adoption checkpoints below are historical.

**REQ-131 source delivery complete ? 17 October 2026:** [PR #364](https://github.com/Fytala-Charles/Bosak/pull/364) merged as `27691fdb3708bb4ffd7c984abbf3196b7cde5667`; final-head and integrated CI each pass 9,810 checks. Local focused 291 and frozen QT3 31,142/0/679 / qt4 gated 8,278/0/1,894 pass. Independent exact-merge local packages pass 282 public-API checks with exact ten-archive/nine-assembly provenance and no vulnerable dependencies. Source **1.19.0 remains unpublished**. [Independent consumer review handoff](REQ-131-CONSUMER-REVIEW-HANDOFF.md) is next; no Braid/native acceptance is inferred.

**1.18.0 published — 17 October 2026:** [Verified public delivery](RELEASE-1.18.0-PUBLICATION.md) at immutable source 3264fcfa supplies bounded REQ-130 value-of deletion. Ten signed public packages verify; fresh nuget.org-only 153 broad/221 focused checks pass with exact provenance and no reported vulnerable dependencies. Braid adoption/functional/native acceptance remains pending; its production pin 1.17.0 and extension 0.2.5/engine 1.16.5 are unchanged. Earlier preparation/unpublished statements below describe historical checkpoints.



**1.18.0 release preparation — 17 October 2026:** [Braid's implementation review](REQ-130-CONSUMER-IMPLEMENTATION-REVIEW.md) reports no blocking findings within the delivered REQ-130 profile, with overlapping reported 226/221/241 test runs. [Release readiness](RELEASE-1.18.0-READINESS.md) and [prepared notes](RELEASE-NOTES-1.18.0.md) pin proposed publication to 3264fcfa. Publication is not authorized or performed; public 1.17.0, Braid's production pin and extension remain unchanged. REQ-130 consumer/native acceptance remains open.

**REQ-130 implementation — 17 October 2026:** Source 1.18.0 implements bounded instruction-only value-of deletion, with engine-issued targets, isolated candidates, complete descriptor correspondence and explicit adjacent-whitespace merge fragments. Source verification passes; public packages remain 1.17.0 and Braid adoption/native acceptance are pending. See the [implementation record](REQ-130-IMPLEMENTATION.md). No whitespace cleanup, dependency deletion, arbitrary subtree edit or automatic publication is included.

**REQ-130 design update — 17 October 2026:** The [bounded deletion contract](REQ-130-DELETION-CONTRACT.md) and [27 planned acceptance groups](REQ-130-DELETION-ACCEPTANCE.md) are proposed. The product owner selected explicit merged-text provenance: preserve one-to-one mapping outside a bounded whitespace merge and prove every surviving fragment in the actual candidate text node. This supports ordinary indented/readable cases without deleting surrounding whitespace. Concrete schema/consumer review precedes engine implementation. [Braid design review](REQ-130-CONSUMER-DESIGN-REVIEW.md) reports no blockers at c6db469c; six reported inspection probes are not deletion tests or functional acceptance. REQ-130 remains Pending/TBD/Unassigned; public 1.17.0 and all engine capabilities are unchanged.

**17 October 2026:** REQ-129 is implemented, published and accepted by Braid within its delivered bounded profile. The user-requested text design is now [REQ-131](REQ-131-TEXT-INSERTION-CONTRACT.md), with [25 planned acceptance groups](REQ-131-TEXT-INSERTION-ACCEPTANCE.md). During this design step, Braid registered [REQ-130 value-of deletion](REQ-130-source-preserving-value-of-deletion.md); that existing consumer identifier is preserved. Neither operation is implemented here. Next implementation priority should be reviewed against the actual deletion request before scheduling the internal text proposal. Earlier planning/adoption wording below describes the original checkpoint.



REQ-128 delivers one bounded creation operation: append one `xsl:value-of` with a contextual select expression. Public 1.8.0 delivers the consumer-cleared implementation from PR #312, merged and tagged at `20a5d9be`; independent Braid adoption remains separate. The wider instruction list accepted in existing source describes validation coverage, not instructions that callers may create.

Each later operation needs an engine-owned eligibility and payload contract before implementation. Shared review, source-revision checks, Apply/history and selection orchestration can remain reusable in the consumer. That reuse must not become arbitrary XML-fragment insertion or UI assumptions that every instruction has the same legal positions and static context.

| Planning candidate | Contract required before implementation |
|---|---|
| Append `xsl:sequence` | Expression result-item semantics, adjacent sequence behavior, actual slot scope and candidate identity |
| Append `xsl:text` | Literal payload and XML escaping, whitespace preservation, expand-text handling and exact output semantics |
| Add a literal result element | Expanded names, attribute payloads, namespace bindings, constructor ownership and fresh node identities |
| Add `xsl:if` | Test context, container-body ownership, legal sequence position and an atomic candidate |
| Add `xsl:for-each` | Select context, body focus/scope changes, container ownership and identity mapping |
| Add `xsl:choose` branches | Atomic multi-node changes, required when/otherwise ordering and structural validity |
| Add a variable or parameter | QName uniqueness, legal declaration position, following-sibling scope and visibility |
| Add a template | Top-level placement, match/name/mode identity, body context and whole-module static validation |

The first planning comparison after value-of delivery should consider sequence and text separately: one carries an XPath expression and can produce arbitrary result items; the other carries literal text. Neither is authorized or scheduled by this catalogue. Final priority should follow an explicit consumer request and its acceptance matrix.

For every proposed operation, define snapshot ownership, payload validation, namespace/scope rules, byte ranges and trivia behavior, surviving/new node identities, size/encoding limits, cooperative cancellation, resource restrictions and classified refusals. Add positive and edge-case fixtures plus independent consumer review. Advertise support individually only after the complete declared slice is implemented and verified.

No new public API, capability flag, target version, accepted REQ item or Braid command is introduced by this document. See the current [value-of contract](REQ-128-INSERTION-CONTRACT.md) and [baseline readiness](REQ-128-RELEASE-READINESS.md).
