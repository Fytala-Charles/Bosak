<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala exact-source adjacent literal element reordering">
  <h1>REQ-137 implementation review</h1>
  <p>24 October 2026 — coordinated Move up and Move down</p>
</div>

Both directions are implemented together in the unpublished source feature candidate **1.25.0**. Public packages remain **1.24.0**. The owner approved implementation after [Braid's nonblocking design review](REQ-137-BRAID-CONSUMER-DESIGN-REVIEW.md) of exact proposal head `cf66f2be3a8d5dce0b694fdd5c410d2f19adaca4`. The [reviewed proposal](REQ-137-MAINTAINER-SCOPE-REVIEW.md) and [original request](REQ-137-same-parent-child-element-reordering.md) remain unchanged. A release version is not allocated by this implementation candidate; publication and Braid integration/acceptance remain separate.

## Public use and exact result

```csharp
var discovered = snapshot.GetLiteralElementReorderTarget(
    selectedNode, AuthoringLiteralElementMoveDirection.Down, cancellationToken);
if (discovered.Target is { } target)
{
    var result = snapshot.ProposeLiteralElementReorder(
        new AuthoringLiteralElementReorderProposal(target), cancellationToken);
    // Changed: review result.Candidate in full before a separate consumer Apply.
    // NoChange: retain original selection; create no history or Apply authority.
    // Refused: show result.Failure; no partial candidate exists.
}
```

Discovery binds direction, actual selected/neighbor/common-parent references and source extents. The proposal accepts only that opaque target. IDs, ranges, equal-byte foreign snapshots, old revisions and other operations' targets cannot authorize an exchange. `AuthoringLiteralElementReorderResult` has exactly one Changed candidate, NoChange or typed refusal. Candidate selection is the actual moved selected element, including repeated names and equal-looking descendants.

`AuthoringCapabilities.SupportsLiteralElementReordering`, `LiteralElementReorderProfile` (`LiteralSiblingExchangeV1`), `LiteralElementReorderSupportedEncodings` and the independent `MaxLiteralElementReorder*` bounds describe availability. Target/candidate `Profile` and `EngineVersion`, the owning module URI, original/fresh snapshots and typed operation/direction carry provenance. Capability discovery is not permission to edit arbitrary XML.

## Initial coverage and refusal

The selected element, neighbor and common parent are supported no-namespace literal result elements, with a literal-only ancestor chain under a principal top-level template. Imported and engine-authored populated subtrees are supported. Keep the dependency-free explicit 3.0/4.0 Legacy authoring host and complete REQ-136 closed instruction/attribute/expression coverage. All original and candidate module content must validate. Original invalidity cannot be repaired by moving.

The immediately adjacent supported element may be separated by no text or one ordinary whitespace-only text descriptor. No skip, wrap, cross-parent movement, caller destination, implicit trivia attachment or Delete/Create emulation. Intervening comments/PIs/instructions/significant text refuse. Interior comments/PIs/instructions move with their owning subtree. CDATA, preserved-space, expanded-text, computed AVTs, unsupported qualified result attributes, opaque instructions/static contexts/dependencies and unsupported encodings refuse.

Ordinary XML numeric references to whitespace in the separator retain their exact spelling. Both the inspected logical whitespace and the aligned raw numeric-reference/whitespace lexical form are checked. This is ordinary text, not CDATA or preserved-space support. Prefix aliases, local rebinding/default namespace resets and Unicode/entities/line endings remain byte-exact.

The complete closed XML vocabulary is the unchanged `LiteralElementLifecyclePipeline.Instructions`/`CheckXml` recognizer, combined with `LiteralAttributeLifecyclePipeline.Scope` and restricted `Validate`. Existing Rename/Delete eligibility and behavior are unchanged. Compiler validation denies ambient resources; discovery/proposal runs no transformations and does not invoke the captured module resolver.

## One edit and total reordered correspondence

`LiteralElementReorderPipeline` copies retained original byte slices directly:

```text
P + A + W + B + S  ->  P + B + W + A + S
```

Exactly one `SourceEdits` replacement covers the pair and separator. Its original range belongs to `InputSnapshot`, replacement range to `Snapshot`; recover old payload with `ExportOriginalRange` and replacement payload from the exported candidate at `ReplacementRange`. Both complete sources remain available. Replay once against the original source. BOM, prefix, suffix and total length stay exact; no serialization, escaping, indentation or namespace fixup occurs.

`Exchange` exposes original A/B/common parent/optional separator and their actual candidate descriptors. `CandidateOfEarlier` means the image of original A, now later; `CandidateOfLater` means the image of original B, now earlier. It accompanies a total `NodeCorrespondence` bijection and complete per-owner `AttributeCorrespondence`, including namespace declarations. No node or attribute is created, removed, split or merged.

The iterative proof maps both endpoints by character-coordinate translation and independently checks byte extents/raw content. A moves by `LB+LW` bytes, W by `LB-LA`, B by `-(LA+LW)`. Containing ancestors and outside nodes retain extents. Internal subtree preorder, parent ownership, kinds/names/values, all attribute metadata/order and bytes are preserved. The only child-order exception is the declared A/B exchange under the common parent. W maps to the actual candidate text descriptor with its ordinary XML-normalized logical value. Both node inventories are exhausted; duplicate/foreign correspondence is refused. Whole-source component/replay checks reconcile the edit with the graph proof.

Complete candidate byte equality, after original eligibility/authority/bounds checks, produces NoChange without a candidate, fresh ports or Apply authority. Its target retains original selection. Paired versus self-closing spelling or equal sample output cannot turn differing bytes into NoChange.

## Bounds and ownership

| Independent work/material | Implemented bound |
|---|---|
| Original and candidate complete source | 8,388,608 bytes each |
| Complete A+W+B exchange, including both subtrees and separator | 8,388,608 bytes |
| Each original/candidate graph | 65,536 nodes, 65,536 attributes, depth 256 |
| Copied complete review evidence | 16,777,216 bytes; conservative budget `2 * sourceBytes + 4 * exchangeBytes` |
| Changed edits | Exactly one, equal original/candidate span length |

The evidence budget accounts for before/after source envelopes, old/new span payloads and full component review bytes, including repetition. Oversized complete evidence refuses even below the source maximum; nothing is truncated. Counts/depth and streaming XML bounds precede recursive candidate inspection, and checked arithmetic/budget checks precede candidate allocation. These are work/copy bounds, not a total heap guarantee.

Targets/results/evidence have internal constructors. Collections wrap private storage and all byte exports are independent copies. Proposals are isolated and do not mutate original snapshots. Cancellation is observed at traversal/scanning/validation/copy/proof boundaries and immediately before Changed/NoChange. Existing bounded inspection/compiler/scanner calls are cooperative work units; this is not preemptive cancellation inside every library call. Unexpected defects are not caught as ordinary unsupported-input refusals.

## Validation and consumer gates

**Executed local checks:** Release build succeeds; all 11,244 solution +84 language-server tests pass, zero failures/skips. Fresh QT3 is 31,142/0/679; frozen qt4 is 8,434/0/1,896 across 224 gated sets (11 existing Experimental fallback passes). Native skips remain explicit. Build warnings are the existing CS8604 conformance warning and two NU1900 warnings from unavailable NuGet vulnerability metadata; no fresh vulnerability-audit result is claimed.

The [machine-readable verification receipt](REQ-137-IMPLEMENTATION-VERIFICATION.json) records executed checks and retained log/TRX hashes. The new focused suite has 71 cases: 24 encoding/direction/separator exchanges with inverse movement and exhaustive correspondence checks; eight byte-identical NoChange variants; adjacency/profile/boundary/authority/cancellation/defensive export/concurrency/refusal cases; equal-output Changed; Create/populate/Rename/Delete composition; resource-resolver observations; and negative internal proof candidates with wrong order, missing/extra nodes or altered attributes.

Initial failures and later corrections are retained in ignored local evidence. Eight initial numeric-whitespace-separator failures prompted exact numeric-reference recognition. Five negative-test reflection-constructor lookup failures were corrected in the test harness; two subsequently exposed missing proof guards for shortened candidates, now refused before range translation. Unicode test generation was corrected to explicit UTF-8 before final verification. No original receipt was overwritten.

Engine tests do not establish Braid review/atomic Apply/history/redo, worker/export/reopen or native acceptance. Braid must review this exact implementation revision against CR01–CR14, especially complete UI evidence, direction/currentness/license rechecks, moved selection, one-step Undo/Redo and NoChange redo preservation. Then prepare a separately approved release, verify signed public packages, and integrate both directions together. No merge, package publication, Braid dependency update or runtime acceptance is inferred from the owner's implementation approval.
