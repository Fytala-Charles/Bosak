# Bosak REQ-133 Consumer Design Review

Date: 2026-10-18. Disposition: **No blocking design findings within the proposed bounded literal-text body replacement profile.** No required contract correction identified. Implementation remains pending and requires separate upstream authorization; this review grants no implementation, release or native acceptance.

## Immutable Material and Evidence

Reviewed merged design commit **00b5e01e9749e6c11e3dfa8279dda76ff8a429df** (PR377). Five local documents match committed content after checkout newline conversion: consumer handoff, replacement contract,28 planned acceptance groups, original consumer request and existing-operation baseline JSON. The unchanged request SHA-256 matches the handoff: `ba50ab5c3761f0c193e72ffc3467a950b08744d5a7b30c41bf9ba57db987a22e`.

Independently executed **seven existing public inspection/runtime baseline cases:7 passed, zero failed/skipped**, using cached public Bosak1.20.0 packages at exact assembly revision91f1af0d488bb2ed72b219118eeb1ab3570726bd. Cases cover empty/reference/numeric-newline/physical-newline/preserved/mixed-parent/local-expansion-override text. Actual payload counts/logical values, runtime output and unchanged source agree with the stated assumptions. This harness has no engine project/friend access and no replacement operation. It reuses previously verified public packages; no fresh release-verification claim is made.

**Replacement tests executed: zero.** Twenty-eight acceptance groups were assessed as plans, not passed tests. No full Braid/upstream solution, language-server or conformance gate rerun was performed. Prior Braid Release1,157/0/0 remains historical integration evidence. Provenance and independent baseline TRX/harness are retained under `.acceptance/req133-design-review-20261018`.

## Seven Proposed Decisions

| Handoff decision | Consumer conclusion |
|------------------|---------------------|
| Separate capability/opaque target/proposal; no formatting or handle reuse | Fits operation-specific authority. Replacing intentional payload content must not inherit append placement/layout semantics or authorize tag/control edits. |
| Changed / NoChange / Refused, exclusive evidence | Sufficient and clearer than insertion-style success. NoChange is validated against original authority/coverage/size/Unicode/cancellation, before hypothetical canonical-output sizing; it supplies no candidate to Apply. |
| Paired ordinary text; effective non-expansion and preserved/mixed surroundings | Fits a body-only operation. Preserved or significant surrounding text need not trigger append-layout refusal when original/candidate validation supports the context. Local false overrides retain attributes rather than silently repairing control state. |
| One body edit with unchanged outside bytes and exact encoded range/caret | Supports complete byte review without a Braid parser/splice. Zero original or replacement length is meaningful. Numeric CR/LF/TAB preserves replacement logical code points while NoChange preserves existing lexical spelling. |
| Survivor map excludes replaced payload in all changed cases | Correctly handles disappearing/appearing text descriptors. Actual zero-or-one ReplacedNodes/NewNodes and complete disjoint partitions avoid treating a changed payload as an ordinary survivor or fabricating an empty node. |
| Original/final physical bounds and replacement shrinkage | Original size remains gated; Changed subtracts old body before counting encoded replacement. NoChange cannot fail merely because a hypothetical canonical spelling would be larger. |
| Separate failure enum and engine/consumer responsibilities | Maintains prior API behavior and clear authority boundaries. Engine targets belong to immutable owners; Braid independently enforces latest revision, permission, frozen review, atomic history and late-cancellation behavior. |

## Acceptance Coverage and Later Review Focus

R01-R28 cover the eleven Braid TE groups. No correction is required before seeking separate implementation authorization. Retain these existing requirements as implementation review priorities:

- Prove source syntax and captured context for CDATA/mixed-body/control exclusions; a generic Text descriptor alone is insufficient. Check exclusions and original validity before equal-intent NoChange.
- Exercise exact logical equality across entity spellings, physical XML newline normalization and explicit CR/LF/CRLF intent. Preserve differing Unicode normalization forms; literal entity-looking replacement text must not be decoded as user markup.
- Require both complete graph partitions and actual payload references for empty/nonempty transitions. Attribute raw/expanded metadata, namespace context and all outside parent/order/source evidence must stay unchanged.
- Use a replacement-specific adapter/consumer validator. Existing insertion checks can require positive replacement length or total old-node correspondence, which are invalid assumptions here. Reuse only primitives whose semantics agree.
- Test original/final physical byte boundaries independently, escaped-source growth, replacement shrinkage, and equal-intent original-size/cancellation refusal. NoChange must not allocate a hypothetical candidate, refresh revision or create history.
- Keep old logical values and lexical source separate in review; changed input invalidates authority. Changed Apply must restore the actual edited instruction plus encoded payload or empty caret through outline/graph/history.
- Preserve public implementation/package/release and worker/export/reopen/native gate separation. Engine tests cannot certify consumer licensing, UI, destination export or native observations.

## Scope and Next Gate

The no-blocking conclusion covers only one engine-approved principal paired ordinary literal `xsl:text` body, exact logical replacement input, unchanged tags/attributes/outside bytes, supported static/resource profile and explicit classified refusals. Self-closing expansion, generic text/subtree editing, formatting, dynamic text-value templates and control/namespace changes remain excluded.

Next: Bosak maintainer implementation authorization, then source/compatibility tests, CI and independent package review. Public release verification precedes Braid adapter/session/UI/history and real worker/restricted-export/reopen/native acceptance. Braid's production pin/runtime remain unchanged. No upstream file edits, external messages, commit or push were made by this review.

## References

- [Consumer scope and TE acceptance groups](LITERAL_TEXT_EDITING.md)
- [Proposed ADR-019](ADR-019-reviewed-literal-text-replacement.md)
- Upstream handoff: D:/Development/Bosak/docs/REQ-133-CONSUMER-DESIGN-HANDOFF.md
- Upstream contract: D:/Development/Bosak/docs/REQ-133-TEXT-BODY-REPLACEMENT-CONTRACT.md
- Upstream matrix: D:/Development/Bosak/docs/REQ-133-TEXT-BODY-REPLACEMENT-ACCEPTANCE.md
