<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala conditional wrapper authoring implementation">
  <h1>REQ-138 conditional wrapper implementation</h1>
  <p>25 October 2026 - published and consumer-accepted in 1.26.0 within the delivered profile</p>
</div>

**REQ-138 accepted by Braid - 25 October 2026:** [Formal acceptance](REQ-138-BRAID-ACCEPTANCE.md) pins pushed checkpoint `29acaac761206d6b316e8844930ce5d4e8043835`, green checkpoint CI, 1,518 Release /1,784 isolated public-package passes and the separate owner "Passed - accepted" native receipt. [Maintainer verification](req138-evidence/acceptance/verification.json) rechecks all seven TRX hashes/counters and correlates nine consumed archive identities with independently verified Bosak signatures. Acceptance covers the delivered conditional lifecycle. Optional/unreported native variants remain unspecified; the host is not represented as a clean-checkpoint build. Earlier consumer-pending notices are historical.


Implements the [exact reviewed design](REQ-138-MAINTAINER-SCOPE-REVIEW.md) at `35f259ff24bd43c8e5b9927e4f62e38c9fcfef23`, accepted without blocking consumer findings in the [retained Braid design review](req138-evidence/original-braid-consumer-design-review.md). Owner implementation approval was separately received on 25 October 2026. [Braid implementation review](req138-evidence/original-braid-consumer-implementation-review.md) found no blocking findings at PR head `417482421343d72496d0f2dedcd45cfb1f463207`; its registry/punctuation corrections are addressed in a documentation-only follow-up. [Verified publication](RELEASE-1.26.0-PUBLICATION.md) records completed merge/release authorization, tagging and signed public-package validation at source `1f06efb4d938d3d8e12eb273e632a8fe15110041`. Braid integration and the focused owner native procedure are accepted within the delivered profile. REQ-138 is **Implemented, published and consumer-accepted**; optional native variants remain unreported. The original numeric 1.25.2 checkpoint remains separately pinned; its corrections are included in public 1.26.0.

## Coordinated API

```csharp
var target = snapshot.GetConditionalWrapTarget(selectedElement, token);
if (!target.IsSuccess) return; // surface the classified refusal
var wrapped = snapshot.ProposeConditionalWrap(new(target.Target!, "true()"), token);
if (!wrapped.IsSuccess) return;
var candidate = wrapped.Candidate!;
// Review complete source, both edits and graph proof before the consumer's separate atomic Apply.
var test = candidate.CandidateTest!;
var edited = candidate.Snapshot.ProposeExpressionEdit(
    new(candidate.CandidateSelection.Id, test.Name, "false()"), token);
// Adopt only after independent source/slot/currentness checks and complete review.
// Byte-identical complete expression output is feedback: no Apply/history/redo invalidation.
var current = edited.Candidate!.Snapshot;
var currentIf = current.PrincipalModule.Root.FindDescendant(
    n => n.Id == edited.Candidate.NodeCorrespondence[candidate.CandidateSelection.Id])!;
var unwrapTarget = current.GetConditionalUnwrapTarget(currentIf, token);
var unwrapped = current.ProposeConditionalUnwrap(new(unwrapTarget.Target!), token);
// After reviewed Apply, select unwrapped.Candidate.CandidateSelection (the actual retained E).
```

Production consumers must check each result, source/session/revision/license and frozen proof before adoption. Discovery and proposal do not mutate source, Apply, history or ports. Distinct internally constructed target types capture exact snapshot and operation; foreign equal-byte snapshots confer no authority. Changed/Refused is exclusive; structural NoChange does not exist. A full-source export and original-range export provide complete old/new payloads for each source edit. Collections are read-only defensive copies; byte exports are independent.

The two structural operations and the additive `ProposeExpressionEdit(proposal, CancellationToken)` ship together. Independent capabilities are `SupportsConditionalWrapping`, `SupportsConditionalUnwrapping` and `SupportsBoundedExpressionSlotReplacement`; profiles are `ConditionalLiteralLifecycleV1` and `BoundedPrincipalExpressionV1`. The legacy one-argument overload retains its behavior and has no new default-argument ambiguity. Existing refusal enum values remain 0-4; new bounded refusals are appended.

## Eligibility and namespaces

One dependency-free explicit 3.0/4.0 principal stylesheet/transform under the existing Legacy authoring compiler and closed REQ-136 module vocabulary. Wrap accepts one no-namespace literal E below a top-level template or literal-only ancestry. It chooses the ordinal-first bounded nonempty XSLT prefix in P's actual inherited context. Declarations only on E cannot authorize the wrapper; no declarations or rebinding are emitted. Test compilation uses actual candidate I/test context inherited from P. Escaping preserves logical tabs/CR/LF with character references. No condition is evaluated during proposal.

Unwrap independently accepts paired expanded-XSLT if I with only its unqualified test Expression attribute, exactly one E and optional plain lexical whitespace. No local declarations, other wrapper attributes, CDATA, significant boundary text, sibling comments/PIs/instructions, conditional ancestors or preserved-space contexts. Supported descendants/comments/PIs/instructions/declarations inside E retain exact bytes. Only the approved if edge is removed from hypothetical ancestry; existing Create/Rename/Delete/Move predicates are unchanged.

Both original and candidate modules undergo closed coverage and restricted validation. Compilation uses deny authorities/resolvers; original ambient resolver callbacks do not become candidate authority. Dependencies, static preprocessing, unproved functions and unsupported contexts refuse rather than invoke fallback acquisition.

## Exact source and graph evidence

Wrap emits two zero-width insertions in ascending same-original coordinates: opening O before E and closing X after E. Unwrap deletes exactly O/X, retaining all C. Replay simultaneously or in descending original-offset order. Untouched bytes and BOM are preserved; no indentation or serialization is authored. Candidate ranges include physical bytes and actual line/column maps. E's endpoint before X and a suffix's start after X have explicit ownership-dependent translation.

`AuthoringConditionalCandidate` exposes input/fresh snapshot, operation/profile/build identity, actual selection/test, both source edits, simple node correspondence, per-owner attribute correspondence, created/removed nodes and attributes, `Reparenting` edges/depth deltas and `MergedTextGroups`. Wrap creates only I/test; Unwrap removes only I/test. Ordinary-inspection partitions exhaust both graphs. Ordered recursive matching proves parent/child order except the declared edge changes. Namespace declaration attributes are included in correspondence. Candidate selection is actual I after Wrap and retained E after Unwrap.

Unwrap supports two distinct boundary merges, each with exactly two ordered original fragments and their actual original/candidate ranges. Participants are excluded from simple survivor maps. Plain lexical spaces/tabs/CR/LF are required at optional inner/adjoining outer boundaries; entity/character references there refuse. Entity spellings inside E are preserved. Logical candidate whitespace is recomputed from the complete raw concatenation: a new CR+LF seam becomes one newline. Raw fragment concatenation and full candidate text bytes must agree. Whole-subtree deletion evidence cannot authorize Unwrap.

## Independent limits and cancellation

| Material | Bound |
|---|---:|
| Original and candidate source, each | 8,388,608 physical bytes |
| Logical condition | 65,536 UTF-16 units |
| Encoded escaped condition | 1,048,576 bytes |
| Chosen prefix | 256 UTF-16 units |
| Combined wrapper markup | 1,052,672 encoded bytes |
| Original/candidate descriptors and attributes, each | 65,536 |
| Maximum descriptor depth | 256 |
| Cumulative copied evidence | 33,554,432 bytes |
| Structural edits | Exactly two |

Limits are published as `MaxConditional*` properties and apply independently to the bounded expression overload. Encodings are strict UTF-8 (with/without BOM) and BOM-marked matching UTF-16LE/BE. Candidate growth/depth and escaped size are counted before output/inspection; conservative evidence accounting counts retained copies, complete exports, selected extents and full edit payloads. A source below 8 MiB may exceed the evidence budget. Parse/compile phases remain opaque cooperative boundaries, not hard-preemptible calls. Checks occur through traversal/scanning/copy work and before success; copies check at most every 64 KiB and descriptor work checks each descriptor. Cancellation produces typed refusal without candidate authority. Unexpected engine defects propagate.

## Engine verification and next gate

Engine source: `5e268552cfd241262ade266ab00fc716591b48cd`. Local validation covers **12,200 distinct solution/LSP tests**: the full solution run12115 and84 LSP passes, affected XSLT rerun8196 replacing8195, and final57/0/0 REQ-138 cases after early-bound ordering refinement. Exact final PR-head full CI is a separate receipt. QT3 **31,142/0/679**; frozen qt4 **8,436/0/1,894**,224 configured/executed sets and11 existing Experimental fallback passes; current native Unix **22/2/0, exit2**, with known negative-domain120/121 failures retained. Build zero errors/existing CS8604 only; fresh restore has no NU1900.

Verification receipts: [implementation evidence](req138-evidence/implementation-verification.json). Tests cover complete lifecycle and actual execution, strict encoding variants, exact paired replay, total partitions/per-owner attributes, parent-prefix shadowing, slot normalization, two CR/LF seams, refusal shapes, invalid originals, foreign/forged metadata, source/depth/evidence limits, deterministic traversal cancellation, defensive expression exports, hostile ambient resolver and legacy ancestry/editor controls. The solution and conformance receipt records actual run results; design review is not runtime acceptance.

Consumer-owned IF01-IF15 obligations remain independent: review/intent invalidation, disclosure and unconditional-content acknowledgement, complete source/slot/proof comparison, atomic Apply and late cancellation semantics, history/redo, fresh ports/navigation, license/currentness and historical validation/preview/export receipts. The consumer source review is cleared, with [verified receipt/counters](req138-evidence/consumer-implementation-review-verification.json). Merge, release and signed public-package verification are complete. [Formal acceptance](REQ-138-BRAID-ACCEPTANCE.md) records completed bounded integration, public-package checks and the focused native report. IF01-IF15 responsibilities and optional native-observation limits remain explicit.

## IF01-IF15 evidence ownership

| Requirement | Engine evidence in this PR | Consumer obligation still required |
|---|---|---|
| IF01 | Independent capabilities/types; foreign/equal-byte snapshot and operation ownership checks | Session/revision/currentness invalidation |
| IF02 | Closed original/candidate validation, template/literal ancestry and imported ordinary inspection | Consumer availability and retained authored/imported workflows |
| IF03 | Paired insertion/replay, raw retained bytes, parent-prefix/static-context and escaping tests | Complete review and execution-change disclosure |
| IF04 | Shared bounded editor; exact slot proof, whitespace preservation and byte-identical candidate controls | Full-byte equality feedback, no Apply/history/redo loss |
| IF05 | Markup-only removal, both boundary merges, actual retained E selection | Explicit unconditional-content acknowledgement |
| IF06 | Exhaustive graph/attribute partitions, ordered matching and parent/depth evidence | Independently reject malformed/missing/duplicate/foreign frozen proof |
| IF07 | Unsupported wrapper shapes, declarations, contexts and invalid originals refuse | Surface precise refusal without fallback |
| IF08 | Four strict encoding variants, Unicode/entities/quotes/CRLF, populated/self-closing E and exact replay | Independent reopen/export/worker checks across encodings |
| IF09 | Independent source/text/escaping/graph/depth/evidence bounds; pre-count added I/test | Independent complete review budget, no truncation |
| IF10 | Defensive exports, pure isolated proposals, deterministic traversal cancellation and hostile resolver tests | Cancel/Close/currentness handling; no adoption on refusal |
| IF11 | Complete independent source alignment, including removed I/test; forged metadata tests | Review/Apply source, edit, attribute and merge-proof verification |
| IF12 | Actual I/test or retained E supplied; proposal is separate from mutation | Outline/graph UI, intent/license/busy/currentness and atomic Apply |
| IF13 | Exact reversible two-edit payloads and original source preserved | One-step Undo/Redo and historical validation/preview/export receipts |
| IF14 | True/false execution lifecycle and existing editor/ancestry restriction controls | Complete consumer lifecycle integration and fresh ports |
| IF15 | Whole-module restricted validation and real engine execution tests | Real-worker ExactBytes goldens, signed public-package provenance and separate native acceptance |

These are evidence boundaries, not a declaration that every consumer acceptance criterion has passed. No Braid dependency, implementation file or native workflow was changed by this engine step.
