<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-134: literal text instruction deletion contract">
<h1>REQ-134: literal text instruction deletion contract</h1>
<p>Proposed design - 19 October 2026</p>
</div>

**REQ-134 source verified - 19 October 2026:** [Braid design review](REQ-134-CONSUMER-DESIGN-REVIEW.md) found no blocking findings; the user subsequently authorized implementation. Source 1.22.0 adds separate literal text deletion authority, complete instruction/payload removal, exact byte replay and bounded whitespace-merge provenance. Local verification passes 10,010 solution, 84 language-server, 110 new source and 531 initial package-public checks; frozen gates are unchanged. [Implementation record](REQ-134-IMPLEMENTATION.md), [verification receipt](REQ-134-VERIFICATION.json) and [consumer implementation handoff](REQ-134-CONSUMER-IMPLEMENTATION-HANDOFF.md) track exact delivery gates through [PR #383](https://github.com/Fytala-Charles/Bosak/pull/383). Public packages remain 1.21.0; consumer implementation review, publication and Braid integration/native acceptance remain separate gates. Earlier design-only restrictions below describe the historical design checkpoint.

## Historical reviewed design material

## Status and intended result

**Pending; contract proposed for consumer review. Target version TBD; implementation owner unassigned.** The user authorized REQ-134 registration and this design after [issue #381](https://github.com/Fytala-Charles/Bosak/issues/381) and closure of [REQ-133 consumer acceptance](REQ-133-BRAID-ACCEPTANCE.md). This authorizes no engine implementation, new capability, version reservation or publication. Design base `ecac021c5e34e620127251f750d7001ec49fe97a`; public Bosak 1.21.0 remains at `8929a1301d1846bb24b49bcc505877f05fd3395a`.

Remove exactly one engine-approved principal literal `xsl:text` instruction, including its actual payload when present. Preserve every byte outside its full instruction extent, including indentation and unused namespace declarations. Body replacement with empty text remains a different supported operation: it retains the instruction. Read the [unchanged inquiry](REQ-134-literal-text-instruction-deletion.md), [issue provenance](REQ-134-ISSUE-381.json), [planned acceptance matrix](REQ-134-TEXT-DELETION-ACCEPTANCE.md) and [consumer handoff](REQ-134-CONSUMER-DESIGN-HANDOFF.md).

## Proposed additive public surface

These proposed members do not exist in 1.21.0. Existing value-of deletion, text replacement/insertion, expression edits and literal element creation retain their types, capabilities, enum values, eligibility, defaults and cancellation behavior. Share proven internal primitives only where their semantics agree.

| Proposed surface | Contract |
|---|---|
| `AuthoringCapabilities.SupportsTextDeletion` | Separate flag, advertised only after verified implementation/delivery. No inference from value-of deletion or text replacement. |
| `MaxTextDeletionSourceBytes` / `TextDeletionSupportedEncodings` | Original and final each bounded to 8,388,608 physical bytes including BOM; immutable UTF-8 / UTF-16LE / UTF-16BE list. UTF-8 permits BOM/no-BOM; UTF-16 requires matching BOM/declaration. |
| `AuthoringSnapshot.GetTextDeletionTarget(instruction, cancellationToken)` | Exclusive Target/Failure result. Exact original descriptor from this owner; opaque `AuthoringTextDeletionTarget` has no public constructor. |
| Target properties | Actual Instruction, Parent, ModuleUri, OriginalRange, IsSelfClosing, nullable OriginalBodyRange, OriginalTextValue and nullable OriginalPayloadNode; private immutable owner/source/static authority. |
| `AuthoringTextDeletionProposal(target)` | Immutable typed authority only. No caller range, payload, formatting or cleanup options. |
| `AuthoringSnapshot.ProposeTextDeletion(proposal, cancellationToken)` | Exclusive `AuthoringTextDeletionResult`: complete Candidate or operation-specific Failure; IsSuccess means Candidate is present. |
| `AuthoringTextDeletionCandidate` | InputSnapshot, fresh Snapshot, ModuleUri, RemovedNode, RemovedPayloadNode, RemovedNodes, OriginalTextValue, nullable OriginalBodyRange, OriginalRange, CandidateRange, SurvivingParent, SourceEdits, NodeCorrespondence, TextMergeGroups and defensive ExportSource(). |
| Correspondence / reusable data | `IReadOnlyDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>` for ordinary survivors; existing immutable SourceRange, AuthoringInsertionSourceEdit, AuthoringTextMergeGroup and AuthoringTextMergeFragment may be reused with their existing meanings. A target/result/candidate from another operation is never deletion authority. |
| `AuthoringTextDeletionFailure` / `AuthoringTextDeletionFailureKind` | Separate typed classification, Message, optional diagnostic ErrorCode/Range and ModuleUri. No old enum renumbering or message parsing. |

Null reference arguments throw ArgumentNullException. This new API proposes classified Cancelled refusals for both discovery and proposal; legacy value-of cancellation continues to throw under its existing contract. Every result is exclusive; cancellation returns no target/candidate/partial evidence. All retained collections, handles and exported bytes are immutable or defensive copies. Failure.ModuleUri identifies the input principal module; a non-null Failure.Range always belongs to InputSnapshot source coordinates. Candidate-only diagnostic ranges are omitted rather than mislabeled or returned as unowned partial-candidate evidence.

### Deletion always changes source

There is no NoChange outcome. Removing an eligible empty paired or self-closing instruction still removes nonempty source bytes and returns a fresh candidate. Runtime output may happen to be unchanged; neither output equivalence nor difference grants deletion authority. A user who cancels review makes no source/history change. An already removed descriptor is foreign to a fresh snapshot and cannot be reused there.

OriginalBodyRange is the real body span for paired syntax, possibly empty. It is null for self-closing syntax, whose nonexistent body must not receive an invented caret. OriginalTextValue is actual inspected logical text or empty; OriginalPayloadNode is null exactly when no actual payload exists. The candidate's zero-length CandidateRange is the removal seam in the complete resulting module, not a fabricated text child or expression slot.

## Proposed first eligibility profile

Identify `{http://www.w3.org/1999/XSL/Transform}text` by expanded name, including alternate prefixes and default XSLT namespace syntax. A literal result element named text, arbitrary text descriptor, other instruction or wrong-owner descriptor refuses. Permit imported, previously inserted and previously replaced instructions through fresh owned targets.

Accept **paired empty, paired nonempty ordinary literal text, and self-closing text**. Self-closing support removes the whole existing instruction; it neither expands tags nor inherits body-replacement's paired-only restriction. A paired body has zero actual payload nodes or exactly one ordinary XText covering its complete lexical body. Whitespace inside xsl:text is intentional payload, even under default XML whitespace rules. It is removed with the instruction and never counted as a surrounding merge fragment.

Refuse CDATA (including empty), comments, PIs, elements, mixed/split or ambiguous body evidence. A public Kind=Text descriptor alone does not prove ordinary text; validate retained syntax/context and actual ranges. Refuse effective expand-text=yes/true/1, disable-output-escaping=yes and unsupported/invalid control forms even for empty/self-closing syntax. Effective non-expansion absent/no/false/0 and a supported local false override may be accepted only when the remaining module has complete captured coverage. Never repair attributes or preprocess dynamic content to create eligibility.

Use the dependency-free principal explicit XSLT 3.0/4.0 stylesheet/transform, top-level template and literal-result-only ancestor chain. Retain static/resource exclusions for dependencies, packages/schema imports, unsupported type/component annotations, static/use-when/shadow preprocessing, extension contexts and unsupported effective versions. Validate original and candidate under the same complete bounded profile; compilation alone does not establish coverage. Invalid original source cannot become eligible merely because removing it would repair the stylesheet.

### Surrounding whitespace, mixed content and xml:space

The first profile deliberately retains REQ-130's conservative containing-constructor boundaries: refuse significant direct parent text or direct CDATA, and any surviving ancestor xml:space=preserve, **including a nearer xml:space=default reset**. Ordinary direct whitespace, comments, PIs and literal result siblings may remain when complete evidence is provable. xml:space=default without a preserving ancestor is eligible. Supported xml:space metadata on the removed instruction itself is part of its removed bytes; it does not alone preserve the surrounding parent. Validate such attributes normally.

These are explicit initial scope limits, not a claim that exact deletion intrinsically requires layout or that no broader context could be proven. REQ-133 body replacement still supports its accepted preserved/mixed contexts; this new operation does not narrow it. Broader deletion contexts require a separately reviewed extension. No whitespace cleanup, comment/PI deletion, namespace cleanup or ancestor rewrite accompanies removal.

## Exact removal and source coordinates

For original B and proven instruction range [a,b), emit C = B[0:a] + B[b:length(B)]. Require a < b, exact retained scanner alignment and encoding boundaries. The single SourceEdits entry is OriginalRange in the input and CandidateRange of zero bytes at a in the candidate. Replay produces ExportSource exactly; all outside bytes are identical. Removing the last child retains a parent's existing paired spelling. Prefixes, attributes, namespaces, declarations, BOM, mixed physical newlines, comments/PIs and URI are unchanged outside the removed extent.

There is no payload emission, escaping, normalization, reserialization or caller-provided source splice. Original entity/Unicode spelling stays available in the complete removal review. Derive byte/UTF-16 line/column positions from each source's own coordinate map, including BOM bias; do not subtract byte counts from character coordinates. Empty body and seam ranges are valid evidence, never synthetic descriptors. The seam may fall inside the merged text run, including between a newly adjacent CR and LF; its exact source coordinates do not mandate a UI caret there. Navigation uses the actual SurvivingParent.

Each original and actual final source is at most 8,388,608 bytes. Candidate length is exactly original length minus the nonempty encoded instruction length. An oversized original refuses even if deletion would fit; count/validate bounds before allocating a candidate. Strict UTF-8 with/without BOM and BOM-marked UTF-16 LE/BE retain matching declaration rules and XML 1.0 inspection boundaries. Unsupported encodings or invalid bytes refuse at their documented inspection/operation boundary without conversion.

## Complete removed and surviving graph partitions

Use actual ordinary inspection of the emitted bytes at the same URI. Let O and C be all original/candidate principal descriptors, R the removed descriptors, M the ordinary one-to-one survivor map, F all original fragment nodes in merge groups, and D all actual merge destinations.

- **R is exactly the selected instruction plus its actual ordinary payload, when present**, in original preorder. RemovedNode is that instruction; RemovedPayloadNode equals its actual child or null. Paired nonempty removes two descriptors; paired empty/self-closing removes one. Attributes/namespaces remain instruction metadata, not invented removed node IDs.
- **O = R disjoint-union domain(M) disjoint-union F.** No removed payload can leak into ordinary correspondence or a merge group.
- **C = image(M) disjoint-union D.** M is injective; no unexplained candidate nodes or duplicate destinations exist. No arbitrary new-node set excuses missing proof.
- SurvivingParent is the actual candidate counterpart of target Parent through M. Preserve every ordinary survivor's kind, lexical/expanded name, full attribute metadata/ranges, outside logical text, translated source ranges, parent and ordered children after removal/declared merge.

References establish owned identity; integer IDs are snapshot-scoped and may be reused numerically by fresh inspection. No guessing by label, largest ID, traversal position or text equality. Every descriptor is accounted for exactly once. A missing/extra/foreign/duplicate/split/merged descriptor outside the declared relation refuses; no truncated or partial proof can succeed. Selecting the result means SurvivingParent, not a removed payload, surviving sibling or invented expression handoff.

### Zero or one bounded adjacent whitespace merge

Reuse the already reviewed REQ-130 source-fragment meaning, without widening its existing public behavior. TextMergeGroups is empty unless removing this instruction joins its two immediately bordering ordinary whitespace text siblings under the same direct parent. At most one group has exactly those two distinct actual original nodes and one actual candidate ordinary text node. Both siblings must be nonempty, XML-whitespace-only (U+0020, U+0009, U+000D, U+000A), non-CDATA and outside R. Significant text/CDATA or any larger/nonadjacent merge is outside this profile.

Each fragment's OriginalRange covers its complete actual original node. The translated CandidateRanges are ordered, nonoverlapping, contiguous and cover the whole actual destination range once, with exact original/candidate byte equality and encoding alignment. A fragment range is lexical provenance inside one node, not another candidate descriptor. Groups are disjoint from M and R; their parent is the mapped SurvivingParent. No surrounding whitespace is reclassified as deleted to make the map appear injective.

Fresh ordinary inspection owns the joined run's logical value. **Do not concatenate the two original parsed values to infer it.** A physical left CR and right LF join into CRLF after removal and may inspect as one LF; numeric character references have their existing distinct lexical/normalization behavior. Preserve source bytes exactly and prove the actual candidate value under engine-owned XML parsing. Braid verifies fragment bytes, graph partitions and authority without implementing another XML parser or normalization algorithm.

Comments, PIs and elements that separate text keep their actual distinct positions; one-sided ordinary whitespace stays an ordinary survivor where proven. The removed xsl:text payload is never one of these fragments, regardless of its content or equal IDs. Whole-module candidate validation and explicit expected-output cases still apply.

## Validation, failure and cancellation

Proposed FailureKind members: CapabilityUnavailable, ForeignSnapshot, UnknownTarget, UnsupportedModule, UnsupportedProfile, UnsupportedCoverage, MixedContent, UnsupportedEncoding, SizeLimit, IneligibleInstruction, InvalidOriginal, InvalidCandidate, ResourceDenied, SourceAlignmentFailure, UnsupportedCorrespondence and Cancelled. Wrong node kind/expanded name is IneligibleInstruction; excluded body/control syntax is UnsupportedCoverage; the declared surrounding mixed/preserved boundaries are MixedContent. Metadata/range alignment failures and incomplete map/merge partitions have distinct alignment/correspondence classifications. Stable single-cause cases are required; no universal precedence for multiple simultaneous faults is promised.

Order: arguments/cancellation and owner identity; original limits/module/encoding/profile/static-execution exclusions; retained instruction/body/parent/seam proof; original restricted validation; private exact emission; independent ordinary inspection and candidate restricted validation; complete source/partition/map/merge proof; final cancellation check; publish the isolated complete result. Proposal repeats authoritative discovery checks. Deny-by-default resource authority applies to both validations; no transformations, extension execution or ambient file/network acquisition occur in proposals. Refuse denied acquisition as ResourceDenied. Do not hide unexpected engine defects as user syntax failures.

Observe cooperative cancellation during traversal/count/copy/evidence work, before/after inspection/validation and immediately before every successful target/candidate. Parsing is not promised a preemptive wall-clock deadline. Cancellation observed before return gives classified Cancelled with no partial state. Shared immutable owners remain safe under concurrent proposals; no Refused/cancelled request mutates them. An old snapshot can still propose to itself; only Braid knows current application revision, license/permission, overlap and history authority.

## Consumer workflow and delivery gates

A compatible replacement-independent text-deletion adapter discovers the separate capability/target, freezes the complete instruction including old logical/lexical payload, exact source edit, R/M/F/D partitions, merge fragments and surviving parent, then verifies all evidence before explicit Apply. An adapter limited to value-of's one removed descriptor or a total plain map cannot Apply. Oversized complete review refuses without truncating evidence. Intent/source/policy changes invalidate review.

Apply is a separate atomic action under current owner/source/inspection/revision/license/cancellation/overlap checks, with actual SurvivingParent selection. One history step restores exact bytes on undo/redo under fresh revisions/inspection; old preview/validation/export receipts remain historical. Precommit cancellation retains source; cancellation after committed Apply reports the committed result. Proposal/cancel never creates history. Empty/self-closing deletion still needs review/history even when sample output is unchanged.

Implementation follows independent Braid design review and separate user authorization, then a minor increment from the then-current source version; none is reserved now. Gates include happy/edge/refusal tests, legacy compatibility, full solution and separate language-server checks, exact-head CI, exact-merge isolated PackageReference-only proof, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**. Later signed public-package verification, Braid adoption/adapter/UI/history, real worker/restricted-export/destination-reopen goldens across four encodings and native observations remain separate gates. No external message or implementation/publication is authorized by preparing this handoff.

## Design evidence and limits

The source/API audit confirms 1.21.0 value-of discovery requires a childless value-of with select, its RemovedNodes describes that one instruction, and text body replacement retains its owner. The new API therefore needs separate authority and complete payload removal. [Ten public 1.21.0 baseline cases](REQ-134-DESIGN-BASELINE.json) verify existing ordinary inspection/runtime shapes, old deletion refusals and source retention at exact 8929a130. The separately authored split/joined newline fixtures demonstrate existing parser behavior, not a deletion candidate or new API. **Zero text-deletion tests have run.** The [REQ-130 contract](REQ-130-DELETION-CONTRACT.md) supplies reviewed bounded merge/normalization precedent; its historical statuses do not override current accepted delivery.
