# REQ-137: Same-parent child element Move up / Move down for Bosak.Braid

Prepared: 2026-10-23. Registered: **2026-10-24**. Requester: **Bosak.Braid**. Parent consumer: REQ-122. Status: **Pending maintainer scope/API review. Target version: TBD. Owner: Unassigned.** The owner explicitly authorizes this documentation-only registration. No implementation, API, release or runtime command is delivered by this proposal.

## Outcome and capability audit

Provide **Move up** and **Move down** together in one cohesive feature and implementation PR. A designer selects a supported output child element and exchanges it with the adjacent supported literal element under the same parent. Existing Create/Rename/Delete remain compatible; graph layout is not the transformation order.

Published Bosak 1.24.0 at `ff9f509461ce7f8e639d64d4b83024dcffe32d80` exposes element insertion, Rename and Delete but no public move/reorder capability or AuthoringSnapshot proposal. Textual audits at local upstream main `36410023` (23 October) and `525832a4` (24 October) also find no such API; these newer heads were not runtime-tested here. Attribute/text/expression or insertion/deletion authority cannot authorize a move. Braid will not emulate it by deleting and recreating a subtree or splicing XML.

## Proposed first profile

- One engine-approved, no-namespace ordinary literal result element and its adjacent supported no-namespace literal element sibling, within a supported ordinary literal result parent in the dependency-free principal module. Support imported and previously authored subtrees within declared closed static/resource coverage; preserve the existing accepted operation profiles.
- Move up exchanges with the previous eligible element; Move down exchanges with the next. The engine resolves the actual adjacent sibling, not a caller-supplied offset, list index or alternate destination. There is no search past an ineligible/intervening sibling, no cross-parent move, arbitrary placement, wrapping, namespace editing or drag/drop in this first request.
- Between the two elements, permit only a proven ordinary whitespace-only text separator, or no separator. Comments, PIs, literal non-whitespace text and XSLT instructions between them cause an explicit refusal. Do not silently attach comments/indentation to an element or jump over meaningful content. Preserve raw separator bytes without cleanup, normalization or pretty printing. Preserved-space, CDATA and other unproved contexts refuse under the declared profile.
- Move each complete element with all its owned attributes, children, instructions, text, comments/PIs and local namespace declarations unchanged. Reordering intentionally changes evaluation/output order; no semantic or sample-output equivalence is promised. Original and candidate whole-module restricted validation remain required. A same-parent move does not imply every lexical/static context is safe; Bosak owns eligibility and conservative refusals.
- Keep both directions together in one upstream implementation PR and one later Braid integration PR. Maintainers may propose different explicit bounds or a narrower supported vocabulary for consumer review, without silently changing this contract into arbitrary node editing.

## Exact source contract

For adjacent elements A and B with separator W, expose and prove this byte relationship:

```text
Original:  P + A + W + B + S
Candidate: P + B + W + A + S
```

P and S remain byte-identical; A and B are exact full element slices; W retains its exact bytes between them. Its absolute offset may shift when subtree lengths differ. Whitespace before/after the exchanged span stays outside the change. No regenerated start/end tags, namespace fixup, attribute escaping, indentation cleanup or implicit source normalization. Preserve BOM, strict encoding, entity spelling, Unicode and line endings.

One contiguous replacement covering both elements and the separator is sufficient. Bosak may choose another explicit edit representation if every edit has owned original/candidate ranges and payloads, a declared replay convention and a complete exact-byte proof of the same relationship. All original coordinates must refer to the same original source, with non-overlap/order rules and checked candidate-coordinate translation. The exchange is one eventual Braid history step regardless of edit count.

At the first/last boundary or with an unsupported neighbor, return a typed refusal/unavailable target, no candidate or history. Do not wrap around. If the exact proposed byte sequence equals the original (for example identical adjacent element slices), return explicit **NoChange**, with no changed candidate, fresh Apply authority, history or redo invalidation. Do not claim a new physical selection/order when bytes did not change. Equal sample output alone does not establish NoChange; different source bytes remain Changed.

## Authority, selection and complete correspondence

Discover an independently advertised reorder capability and direction-specific opaque snapshot-owned authority. The target must retain the selected original element, actual neighbor, common parent, direction, exact subtree/separator extents and declared coverage. Numeric IDs, equal-byte foreign snapshots, caller-provided ranges or another operation's target cannot grant authority. Exact public type names and failure precedence are Bosak design choices.

A Changed result includes original/candidate complete source/inspection, engine/profile/module identity, exact edits, explicit original/candidate selected and neighbor descriptors, common parent, and complete correspondence for **every actual descriptor and per-owner attribute**. No node or attribute is created, deleted, split or merged in this profile. The mapping is a total bijection; attributes are metadata rather than fabricated child nodes.

Global preorder changes intentionally. Prove that each exchanged subtree retains its complete internal preorder, parent relationships, kinds, lexical/expanded names, logical values, attribute metadata/order and raw bytes. Only the two sibling positions and their corresponding translated extents change. Map unchanged separator text to its actual candidate descriptor; retain its normalized logical value as supplied by ordinary inspection and its unchanged raw bytes. Account for parent/document/module/unrelated descriptors and all outside attributes. Do not reuse a global order-preserving insertion/deletion map without the explicit swapped-sibling exception.

The new selection is the actual moved selected element in its new location, with its complete extent and current module/node identity; the neighbor is not arbitrarily selected. Preserve this intent for either direction and when sibling subtrees have repeated names or equal-looking descendants. Candidate ports must be fresh and support subsequent moves and existing operations using actual new identities.

All targets/evidence/collections are immutable or defensively copied. Independently bound original/candidate source bytes, both subtree/separator scans, traversal depth/counts, copied review payload and edit arithmetic before allocations or recursive work. Refuse oversized complete evidence rather than truncating it. Discovery/proposal performs no transformation, file/network/extension acquisition or source mutation. Observe cooperative cancellation throughout bounded work and immediately before success/NoChange; concurrent proposals remain isolated. Distinguish typed input/profile/resource/authority/bounds/correspondence refusals from unexpected engine defects.

## Braid review, Apply and history

Show Move up/down intent, selected and neighbor names/identities, same parent, both complete subtrees, the separator policy, complete before/after changed span, exact edit payloads and source/session/revision/engine provenance. No drag/drop or immediate mutation. Changing selection/direction/source/policy invalidates held review. Complete review has a separate UI bound, with explicit refusal instead of partial display.

Separate Propose and Apply. Apply independently rechecks exact owner/source/inspection/port/operation/direction/revision/license, frozen full evidence, cancellation and overlap at the existing atomic source-owner boundary. Each Changed exchange is one exact-byte history step; NoChange creates none and preserves redo. Undo/Redo re-inspects and restores actual moved-element navigation, not old numeric IDs. Validation/preview/export results become historical after changed source or history navigation, even if bytes recur. Precommit refusal retains source and any still-current explicit retry; committed late cancellation reports success.

## Planned acceptance groups

These are proposed requirements, not executed move/reorder tests.

| Group | Required evidence |
|---|---|
| CR01 | Additive capability, both directions, opaque owned selected/neighbor/parent authority; foreign/equal-byte/stale/other-operation targets refuse |
| CR02 | Imported and engine-created supported elements, nested populated subtrees, aliases/rebinding/default resets and declared version/host eligibility |
| CR03 | First/last boundary, missing/ineligible neighbor and unsupported parent refuse; no wrap, skip or cross-parent move |
| CR04 | Exact P+A+W+B+S exchange, both directions, unequal subtree lengths, no separator and raw whitespace separators; outside bytes remain intact |
| CR05 | Intervening comments/PIs/instructions/non-whitespace text, preserve-space/CDATA and unsupported contexts refuse explicitly |
| CR06 | Exact byte-identical NoChange preserves source/revision/history/redo and original selection; equal-output Changed remains a real history step |
| CR07 | Total node/attribute bijection, moved-subtree internal order, parent/neighbor order, explicit piecewise extent translation and exact moved-element selection |
| CR08 | Repeated names/equal-looking descendants, empty/paired/self-closing elements, full attribute/text/value-of contents and namespace declarations retained |
| CR09 | UTF-8 with/without BOM and supported UTF-16 LE/BE; Unicode, entities, CR/LF and strict physical bounds/overflow/depth/evidence limits |
| CR10 | Complete original/candidate restricted coverage, denied resources/extensions/static contexts and invalid originals cannot be repaired by reordering |
| CR11 | Immutable defensive exports, concurrent isolation, cancellation and malformed/missing/duplicate/foreign correspondence/edit evidence |
| CR12 | Shared outline/graph review/Apply, currentness/license/operation invalidation, complete-evidence refusal, explicit retry and one-step Undo/Redo |
| CR13 | Move up/down interoperate with Create/populate/Rename/Delete and fresh ports; each changed stage validates, runs real-worker ExactBytes goldens, exports/hash-checks and independently reopens/executes |
| CR14 | Signed public-package provenance; focused native direction/review/selection/history acceptance, with encoding/export/hash/refusal matrices automated |

## Delivery and references

Bosak owns design acceptance, API details, engine implementation/tests, scheduling and release. The request is registered Pending in the feature registry; no engine work is authorized by registration. Braid reviews the design and exact delivery, verifies a separately published release, then integrates both directions together. REQ-137 is the actual assigned registry ID. No target version or implementation approval is assigned by this registration.

Related delivered Braid contracts: [accepted element lifecycle](https://github.com/Fytala-Charles/Bosak.Braid/blob/c9c130e8fbce6a498e20fda7f535aa53a017f326/docs/LITERAL_ELEMENT_LIFECYCLE.md), [source-backed authoring boundary](https://github.com/Fytala-Charles/Bosak.Braid/blob/c9c130e8fbce6a498e20fda7f535aa53a017f326/docs/ADR-002-source-preserving-authoring-boundary.md). New local Braid request: `CHILD_ELEMENT_REORDER_REQUEST.md`; proposed decision: `ADR-024-same-parent-child-element-reordering.md`, both in `D:/Development/Bosak.Braid/docs`. Those prepared documents are uncommitted/unpublished; this upstream request includes the complete scope and acceptance groups without relying on missing remote links.

## Registration evidence and limits

The owner answers "Yes, register the Bosak request" on 24 October 2026. Registry and this supporting document are the only upstream files changed by the Braid registration task. Existing unrelated Bosak working files are preserved. REQ-136 is already implemented, published and consumer-accepted in 1.24.0; PR425 is merged. Last Braid evidence is 1,444 Release /839 signed public checks and green latest main CI39988580154 at c9c130e. Those are existing lifecycle results, not new move/reorder tests. No engine API, tests, source, dependency, version, release, commit or push is performed by this documentation registration.
