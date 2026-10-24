<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala proposed exact-source child element reordering">
  <h1>REQ-137 maintainer scope and API proposal</h1>
  <p>24 October 2026 — proposed contract for consumer review</p>
</div>

**Proposal only.** REQ-137 remains Pending / TBD / Unassigned. The [original request](REQ-137-same-parent-child-element-reordering.md) is retained unchanged. No move API, implementation approval, target release or Braid integration is delivered by this document. Reviewed Bosak base: `4de6968cfb6248b2aa553d930bcf3a63bace9ef4`. Public packages remain 1.24.0; source 1.24.1 is unpublished and contains a separate Experimental substring correction.

## Recommended delivery and profile

Deliver Move up and Move down together in one engine implementation PR, followed by one Braid integration PR after signed public-package verification. Reuse the accepted element lifecycle's strict scanner, coordinate maps and closed validation where applicable. Introduce a separate reordered-graph proof: the existing globally ordered insertion/deletion correspondence is insufficient.

The initial target is an actual no-namespace ordinary literal element under a supported no-namespace literal element parent, within the creation-compatible literal-only ancestor chain beneath a top-level template. Both exchanged subtrees must satisfy the current REQ-136 supported vocabulary, including populated literal elements, constant attributes, literal text, comments/PIs and covered instructions such as `xsl:text` and `xsl:value-of`. Instructions/comments/PIs may be retained *inside* either subtree; they cannot be crossed between the two siblings.

Use a dependency-free principal module with an explicit `stylesheet`/`transform` root and effective version 3.0/4.0 under the captured Legacy authoring host. No new selectable Draft host is proposed. Validate actual expanded names, alternate XSLT prefixes, rebinding and default namespace resets. Retain local namespace declarations exactly. No computed AVTs, unsupported qualified result attributes, namespace aliases, expanded text, CDATA, effective preserved-space or unproved static contexts. Original and candidate must both pass the complete restricted module validation; a move cannot repair an invalid original into eligibility. Deny file/network/extension acquisition before compilation and execute no transformation during discovery/proposal.

The neighbor is the immediately adjacent eligible element in the requested direction, separated by either no node or exactly one ordinary whitespace-only text descriptor. No skipping past comments, PIs, instructions, significant text or unsupported elements. Boundaries and unsupported parents refuse. Whitespace is determined from ordinary inspected XML text and independently proved against its retained lexical source; unsupported lexical markup refuses. Outside whitespace remains outside the exchange. Strict UTF-8 with/without BOM and BOM-marked UTF-16LE/BE with matching declarations are the initial mutation encodings.

## Proposed additive surface

These names are concrete review candidates, not public declarations.

| Surface | Proposed contract |
|---|---|
| Capability | `SupportsLiteralElementReordering` with separate profile and published reorder bounds |
| Direction | `AuthoringLiteralElementMoveDirection.Up` / `.Down`; reject invalid enum values |
| Discovery | `snapshot.GetLiteralElementReorderTarget(node, direction, cancellationToken)` returns an opaque owned target or typed failure |
| Proposal | `snapshot.ProposeLiteralElementReorder(new AuthoringLiteralElementReorderProposal(target), cancellationToken)` |
| Result | Exactly one Changed candidate, NoChange evidence or Failure |
| Target | Internal construction; original snapshot/module, actual selected/neighbor/parent descriptors, captured direction, exact ranges and coverage |
| Changed | Original/fresh candidate snapshots, operation/direction, engine/profile/module identity, one exact edit, selected/neighbor/parent pairs and exhaustive node/attribute correspondence |
| NoChange | Original owned target/selection and checked equality evidence; no candidate, fresh ports or Apply authority |

Direction is bound at discovery, with no proposal override or caller destination. Actual descriptor reference membership is required: IDs, equal bytes, foreign snapshots, stale descriptors, ranges and other operations' targets never grant authority. Recheck the target's identity, direction, adjacency and eligibility during proposal. Engine-created targets/candidates have internal constructors; collections and byte exports are immutable or defensive. Candidate ports are fresh ordinary-inspection ports for subsequent moves, Create, populate, Rename and Delete.

Expected typed failures include invalid direction/input, unavailable neighbor, ineligible parent/target/profile/encoding/coverage, foreign authority, invalid original/candidate, lexical alignment, correspondence, bounds and cancellation. Keep operation-specific models rather than broadening existing lifecycle outcomes. Follow established constructor null conventions. Unexpected programming or infrastructure defects remain defects, not unsupported-input results.

## Exact edit and selection

Let A be the earlier subtree, B the later subtree and W the separator, regardless of which element is selected:

```text
Original:  P + A + W + B + S
Candidate: P + B + W + A + S
```

Propose exactly one contiguous replacement of `A+W+B` with `B+W+A`. It owns original and candidate ranges and full old/new payloads. Replay once against the original bytes. Length is unchanged; prefix, suffix and BOM stay exact. Do not serialize, normalize, reindent, escape or fix namespaces. Both payloads and replay are checked against complete exported source. The separator remains between the subtrees even though its offset may change.

If complete candidate bytes equal original bytes, return NoChange only after original eligibility, bounds, authority and cancellation checks. Preserve original selection; do not claim the selected physical subtree moved. Distinct source bytes are Changed even when sampled output is equal. Reinspect and validate every Changed candidate, then select the actual mapped selected element: Up selects the moved B; Down selects the moved A. Never locate selection by name or stale numeric ID.

## Piecewise correspondence proof

Let the original pair start at byte `a`, and lengths be `LA`, `LW`, `LB`. Candidate starts are B at `a`, W at `a+LB`, A at `a+LB+LW`. For complete ranges contained in a component, translate both endpoints by:

| Original component | Candidate offset delta |
|---|---|
| A subtree, including its owned attributes | `LB + LW` |
| W text descriptor, when present | `LB - LA` |
| B subtree, including its owned attributes | `-(LA + LW)` |
| Outside pair; containing ancestors | `0` (ancestor extents unchanged) |

Check all arithmetic and encoded boundaries before copying. Ancestor nodes keep extents but have changed child ordering; document/module containers are included. A generic rule for a range wholly inside a component must not be applied to a containing ancestor.

Construct a total bijection over every actual original/candidate node and every per-owner attribute. No creation, deletion, split or merge partitions exist. Explicitly exhaust both graphs, reject missing/duplicate/foreign entries, and prove parent ownership, kind, lexical/expanded name, logical value, attribute metadata/order and exact moved source. Preserve each subtree's internal preorder. The only child-order exception is exchanging A and B under their common parent while W stays between them. All other parents retain mapped child order. Map W to the actual candidate ordinary text descriptor, with its ordinary XML-normalized logical value and unchanged raw bytes. This distinguishes repeated names and equal-looking descendants by proved positions and ownership, not value matching.

Expose this exception as immutable exchange evidence: both original/candidate subtree ranges, optional original/candidate separator descriptor/range and common-parent pair, alongside exhaustive correspondences. Reconcile the evidence against the one edit and whole-source replay before returning Changed. Equal-looking nodes must not make the proof ambiguous; inability to prove the actual correspondence is a refusal.

## Independent limits and cancellation

| Material/work | Proposed limit |
|---|---|
| Original and candidate source | 8,388,608 physical bytes each, independently checked |
| Both subtree scans and separator | Individually within source bounds; combined exchange at most 8,388,608 bytes; checked end/start arithmetic |
| Each original/candidate graph | 65,536 nodes, 65,536 attributes, maximum depth 256; iterative bounded traversal |
| Complete copied review evidence | 16,777,216 bytes cumulatively across retained source copies, before/after span, edit payloads and any repeated byte evidence; never truncated |
| Edit count | Exactly one for Changed; none for NoChange |

These limits bound work and copied material, not total managed heap. Account for actual duplication conservatively; limits may refuse a source smaller than the source maximum when its complete evidence exceeds the separate budget. Graph bookkeeping is independently bounded. Check cheap limits before scans, allocations and recursive inspection; prevent overflow in lengths, endpoint translations and evidence sums. Discovery/proposal observe cancellation during traversal, scanning, copying, validation and proof, and immediately before Changed/NoChange. Concurrent proposals do not mutate shared snapshots. Resource callback probes must establish no ambient acquisition.

## Acceptance ownership

| Request groups | Engine evidence planned | Consumer evidence planned |
|---|---|---|
| CR01–CR03 | Both directions, real authority/adjacency, profiles, boundary and no-skip refusals | Discover availability; stale/foreign review invalidation |
| CR04–CR06 | Exact replay, unequal lengths, absent/raw W, outside bytes and byte-identical NoChange; equal-output Changed | Complete review; no history/redo invalidation on NoChange |
| CR07–CR08 | Exhaustive graph/attribute bijection, piecewise extents, repeated names and populated nested subtrees | Actual moved selection in outline/graph |
| CR09–CR11 | Four encodings, Unicode/entities/CRLF, independent bounds/overflow/depth, denied acquisition, invalid originals, defensive exports, cancellation and concurrency | Separate complete-review bound and refusal |
| CR12–CR13 | Fresh candidate ports; Create/populate/Rename/Delete interoperability | Atomic Apply rechecks source/session/revision/operation/direction/license/evidence/cancellation; precommit retry, late-commit success; one history step; Undo/Redo reinspection; validation/preview/export historical after changes; real-worker ExactBytes/export hash/reopen at each stage |
| CR14 | Signed published package/source provenance after implementation | Independent public-package verification and focused native direction/review/selection/history acceptance; automated encoding/refusal/export matrices kept distinct |

Future implementation must include happy-path and edge/refusal checks, full solution tests, frozen QT3 exactly 31,142/0/679 and frozen qt4 gate 8,434/0/1,896 across all 224 configured sets unless a separately reviewed gate change explains any difference. These are existing baselines, not executed reorder results. Braid automation/native acceptance remains separate from engine validation.

## Review decision and sequence

Ask Braid to confirm the initial profile, one-replacement exchange, direction-bound target, explicit reordered-graph proof, NoChange selection semantics and conservative complete-evidence budget. Record any requested changes and an exact reviewed design revision. Then obtain owner approval for the coordinated implementation. Only after implementation review, green checks and approval should a feature release be prepared; assign its version at that point. Braid integrates both directions after independently verifying the signed publication.

The local Braid request and proposed ADR-024 were inspected read-only; their documentation-registration approval does not authorize engine work. No implementation or move tests have run in this documentation step. Existing REQ-136 acceptance and REQ-126 conformance work retain their separate status.
