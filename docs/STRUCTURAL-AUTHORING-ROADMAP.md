<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala operation-specific structural authoring roadmap">
  <br><br><h1>Structural authoring operation roadmap</h1>
  <p>Planning catalogue — 15 October 2026</p>
</div>

**REQ-130 design update — 17 October 2026:** The [bounded deletion contract](REQ-130-DELETION-CONTRACT.md) and [27 planned acceptance groups](REQ-130-DELETION-ACCEPTANCE.md) are proposed. The product owner selected explicit merged-text provenance: preserve one-to-one mapping outside a bounded whitespace merge and prove every surviving fragment in the actual candidate text node. This supports ordinary indented/readable cases without deleting surrounding whitespace. Concrete schema/consumer review precedes engine implementation. REQ-130 remains Pending/TBD/Unassigned; public 1.17.0 and all engine capabilities are unchanged.

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
