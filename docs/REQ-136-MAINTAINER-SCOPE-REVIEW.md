<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-136 proposed literal element lifecycle contract">
  <h1>REQ-136 maintainer scope and API proposal</h1>
  <p>23 October 2026 — proposed contract for consumer review</p>
</div>

**Proposal only.** REQ-136 remains Pending; owner is Unassigned and target version is TBD. No API below exists in published 1.23.8. This document approves no implementation, compatibility expansion, release or Braid integration. The [original consumer request](REQ-136-literal-result-element-lifecycle.md) remains unchanged. [Intake/smoke receipt review](REQ-136-INTAKE-REVIEW.json).

## Recommendation and existing foundation

Deliver Rename and whole-subtree Delete together in one implementation PR, preserving the accepted REQ-132 creation API. One subsequent Braid integration PR should expose the coordinated lifecycle. A documentation/design review may precede that implementation; there is no separate rename-only or empty-delete-only feature acceptance.

The reviewed repository base is `2aa77d0c906edb790d686aeb989771f6031152da`. Published 1.23.8 has immutable package source `6d12bed35dac56a635acc7702645ef9085103c83`. Its [snapshot surface](../src/Bosak.Xslt/Authoring/AuthoringSnapshot.cs) and [capabilities](../src/Bosak.Xslt/Authoring/AuthoringCapabilities.cs) offer literal-element insertion, not rename/delete. Reuse patterns from [literal element creation](../src/Bosak.Xslt/Authoring/LiteralElementInsertionPipeline.cs), [literal attribute lifecycle](../src/Bosak.Xslt/Authoring/LiteralAttributeLifecyclePipeline.cs), [strict source scanning](../src/Bosak.Xslt/Authoring/SourceTagScanner.cs), and [deletion correspondence](../src/Bosak.Xslt/Authoring/ValueOfDeletionPipeline.cs). These are reuse candidates, not proof that their present eligibility, traversal or single-instruction removal logic handles arbitrary subtrees.

REQ-130/134 already distinguish ordinary survivors from bounded adjacent-whitespace merge provenance. Extend the proof to every removed descendant and attribute without changing the meaning of those existing public models. Text-payload or attribute authority cannot stand in for element authority.

## Proposed supported profile

| Dimension | Proposed initial contract |
|---|---|
| Module | One dependency-free principal module, explicit XSLT `stylesheet`/`transform` root; no simplified literal-root stylesheet or root deletion |
| Version/host | Explicit 3.0/4.0 effective versions in the existing authoring compiler profile (Legacy). The authoring surface does not currently capture a selectable Draft host; no Draft-host portability or new host-selection API is promised. |
| Target | Actual ordinary literal result element with empty namespace, imported or engine-created, paired or self-closing; identity is established from expanded names and engine context |
| Target ancestry | The existing creation-compatible constructor boundary: a top-level template or a literal-result-only ancestor chain beneath it. Selecting a nested literal below an intervening XSLT control instruction is outside this proposed first target profile. |
| Populated subtree | Proven supported literal descendants, constant result attributes, ordinary literal text, comments/PIs, and the already covered instruction/expression subset, including `xsl:text` and `xsl:value-of`. Empty-only deletion is insufficient. |
| Namespace cases | Alternate lexical prefixes for the XSLT namespace, prefix rebinding and explicit `xmlns=""` resets must be tested by actual expanded-name classification. Retain namespace declarations byte-exactly. `xsl:namespace-alias` instructions are excluded. |
| Attributes | Preserve all actual attributes in accepted input. Proposed first profile uses the existing whole-module constant-result-attribute recognizer, including escaped literal braces. Computed AVTs, unsupported qualified result attributes and other unproved static contexts refuse. |
| Text/markup | Ordinary XML 1.0 text, comments and PIs; preserve lexical form. CDATA, expanded-text and output-escaping forms outside the closed validator refuse. Effective `xml:space="preserve"` is outside the proposed common first profile. |
| Encoding | Strict UTF-8, with/without BOM; BOM-marked UTF-16LE/BE with matching declarations. Broader inspection encodings do not confer mutation eligibility. |
| Static/resources | Complete original and candidate restricted validation; deny dependency/resource/extension acquisition. Exclude include/import/use-package/schema, static evaluation, use-when, shadow attributes, unsupported type/mode/component/extension contexts and opaque instructions before compilation. |

The implementation must publish an explicit instruction/attribute/expression allow-list; the table does not authorize every instruction named in an old helper's set. Profile exclusions apply to the whole inspected module where needed for complete validation, not only to nodes being removed. Removing an invalid or unsupported construct cannot repair the original into eligibility. Existing creation eligibility and formatting remain unchanged; full lifecycle availability is the declared intersection of operation profiles, not a claim about every previously creatable element.

**Consumer decision needed before implementation:** confirm that this constant-attribute, creation-compatible target profile and whitespace-only boundary-merge policy cover the requested first delivery. If Braid needs computed AVTs, targets beneath control instructions, preserve-space or non-whitespace merge support, expand the reviewed design explicitly before advertising that support. Populated deletion remains part of the first delivery under either decision.

## Proposed additive API shape

Names below are concrete review candidates, not final public declarations:

| Surface | Proposed shape |
|---|---|
| Capability discovery | Separate `SupportsLiteralElementRename` and `SupportsLiteralElementDeletion`, with independent published bounds/profile descriptions; no reuse of insertion or attribute permission |
| Rename discovery | `snapshot.GetLiteralElementRenameTarget(node, cancellationToken)` returns an opaque snapshot-owned target or typed failure |
| Rename proposal | `snapshot.ProposeLiteralElementRename(new AuthoringLiteralElementRenameProposal(target, newLocalName), cancellationToken)` |
| Rename result | Exactly one of Changed candidate, NoChange evidence, or Failure |
| Delete discovery | `snapshot.GetLiteralElementDeletionTarget(node, cancellationToken)` returns its own operation-specific opaque target or typed failure |
| Delete proposal | `snapshot.ProposeLiteralElementDeletion(new AuthoringLiteralElementDeletionProposal(target), cancellationToken)`; no replacement text, layout or unwrap parameter |
| Delete result | Exactly one of Changed candidate or Failure; a real selected element always changes source when removed |
| Candidate | Operation, original/fresh snapshots and module/engine/profile provenance; immutable source edits, node/attribute correspondence, removal/merge partitions and actual candidate selection |

Targets and engine-created evidence have internal constructors. Discovery checks actual descriptor membership, not just ID/range/value equality. Proposals accept owned targets; public constructors follow existing null-argument conventions, while name legality is a typed proposal refusal. Equal-byte snapshots, stale objects and targets from the other operation never grant authority. Candidate ports are fresh ordinary-inspection ports. Old IDs, slots and validation/preview/export receipts remain historical.

Use operation-specific result/target types and a new additive failure vocabulary if existing enums cannot express this contract without changing old behavior. Expected failures include unsupported capability/module/profile/encoding/subtree/target/name, foreign authority, invalid original/candidate, source alignment, correspondence, size/depth/evidence and cancellation. Failure diagnostics remain owned by the applicable original/candidate module. Do not convert unexpected infrastructure/programming faults into ordinary unsupported-input refusals.

## Rename: exact tokens and NoChange

1. Re-prove authority, complete original coverage, bounds and supported context before deciding equal intent. Validate the exact supplied Unicode NCName without trimming, case folding or normalization. Colon/prefix/namespace changes are excluded; unpaired surrogates and illegal XML names refuse.
2. Scan retained source with quote/comment/PI awareness and align actual opening/closing tokens to the selected descriptor. A paired element has two name-token replacements; a self-closing element has one. Never search/replace coincidental text, descendant names or attribute values.
3. Preserve every byte outside those name tokens, including whitespace, attributes, quotes, namespace declarations, body, paired/self-closing form and closing punctuation. A rename must not serialize the whole element.
4. Equal ordinal logical local name returns NoChange only after the original is eligible. No candidate, changed edits, candidate ports or Apply authority are supplied. Equal output in a sample is not NoChange.
5. On Changed, re-inspect the one candidate, validate it under the same captured coverage and prove a complete injective graph/attribute correspondence. Only the addressed element's local name and translated extents change. Select the actual renamed candidate element.

## Source edits and replay convention

Every edit records its owning original/candidate snapshot and module, original range/bytes, replacement bytes and candidate range. Byte export methods return independent copies; read-only collections wrap private storage and do not expose mutable arrays/dictionaries through casts or memory extraction.

**All original ranges refer to the same original source.** Edits are sorted by increasing original offset and are non-overlapping. Replay by copying original untouched intervals plus replacements, or apply edits in descending original offset. Do not sequentially interpret an original offset against an already changed buffer. Candidate offsets include the checked sum of earlier replacement-length deltas. A paired rename's two edits replay into one candidate and one eventual Braid history step.

Strict encoding coordinate maps establish every byte/line/column boundary, including BOM bias and Unicode. A range inside a replaced name is not an ordinary survivor range. Verify the final replay against exported candidate bytes and expose all edit payloads for complete review. No adjacent indentation cleanup, namespace cleanup, pretty printing or shortening paired tags is performed.

## Delete: whole subtree and complete partitions

Delete removes exactly the selected full element extent, from its opening `<` through its matching closing delimiter (or complete self-closing form). It includes every owned attribute, instruction, literal text, comment/PI and descendant. It does not unwrap or retain children, and requires no proof that output would stay equal.

The candidate selects the actual surviving direct parent. Preserve siblings, parent delimiters and all outside bytes; an emptied paired parent remains paired. The removal edit has an empty replacement and a zero-length candidate anchor in the owned parent/module. Complete review includes the full removed bytes and all actual removed descriptors/attributes, not a truncated summary.

The proof must establish these disjoint, exhaustive partitions:

- Original nodes: removed selected subtree; one-to-one ordinary survivors; original text contributors in an explicitly supported boundary merge.
- Candidate nodes: injective ordinary-survivor images; actual merged-text descriptors with the declared original provenance. No extra or unexplained missing candidate nodes.
- Original attributes: attributes owned by removed nodes; complete per-owner survivor correspondences. Namespace declarations are accounted for wherever ordinary inspection represents them. No foreign, duplicate or synthetic owner/attribute identity.

For every survivor, verify kind, expanded name, logical value, attribute metadata/order, parent/child order and strictly translated extents. Root/document-level and unrelated descriptors participate too. Delete evidence cannot be synthesized from a text-payload partition or attribute token range.

### Adjacent text policy

Support at most one direct-parent boundary merge, of the two immediate original ordinary whitespace-only text siblings on either side of the removed element. Reuse the meaning of existing `AuthoringTextMergeGroup`/`AuthoringTextMergeFragment` where compatible: actual original descriptors and actual candidate descriptor; ordered full original extents mapped to contiguous candidate byte fragments that exhaust the candidate text range. Each raw source fragment must be unchanged.

Ordinary inspection owns the parsed candidate value, including CR/LF and character-reference normalization. Do not concatenate old logical text strings and invent a candidate descriptor. CDATA, non-whitespace contributors or any other unproved merge shape refuse explicitly. A nonempty selected subtree is still supported when this boundary policy is satisfied; merge refusal is not an empty-only deletion policy.

## Independent bounds, cancellation and ownership

Proposed initial limits for consumer review:

| Work/material | Independent bound |
|---|---|
| Original source and candidate source | 8,388,608 physical bytes each, checked independently |
| Caller local-name input | At most1,024 UTF-16 code units before validation and1,024 bytes in the retained strict encoding; paired repetition budget checked separately |
| Original/candidate descriptor traversal | 65,536 nodes and65,536 attributes each; maximum depth256, checked iteratively |
| Copied review payload | 16,777,216 physical bytes across the complete requested original/candidate element, removed source, edit payload and other copied byte evidence; graph bookkeeping bounded separately by descriptor counts |
| Operation edits/merge groups | Rename1/2 edits; Delete1 edit and at most1 supported merge group |

These are proposed operation bounds, not the broader inspection contract or a promise about total managed heap usage. Count copied material conservatively and refuse complete evidence that exceeds its bound; never truncate. Use checked arithmetic for every length, delta, extent, repetition and allocation. Check cheap limits before expensive name validation, copying, recursive inspection or evidence construction. Exported caller-owned copies do not give mutation rights over the retained candidate.

Observe cancellation during bounded traversal, scanning, encoding/copy and proof, and immediately before publishing success/NoChange. Every proposal is pure with respect to the original snapshot and competing proposals. Resource-denial/coverage checks precede any compilation that could acquire resources; instrument resolver/file/network/extension callbacks to prove no ambient acquisition. Proposal/discovery executes no transformation.

## Acceptance ownership and delivery gates

| Consumer group | Bosak evidence | Braid evidence |
|---|---|---|
| LE01-LE06 | Additive discovery; authority and refusal matrix; Unicode/ordinal names; paired/self-closing edits/replay; NoChange; complete rename identities | Held review invalidation, exact old/new naming disclosure, current selection and unchanged redo on NoChange |
| LE07-LE10 | Empty and populated/nested deletion; full removed-node/attribute sets; survivor maps; bounded merge partitions; forged/missing/duplicate evidence refusals | Explicit subtree-removal review, full evidence size refusal, cleared removed slots and surviving-parent navigation |
| LE11-LE13 | UTF-8/BOM/UTF-16 matrices, physical boundaries/overflow/depth, invalid original/candidate, resource denial, cancellation, immutable/concurrent results | Session/license/revision policy checks and complete frozen review |
| LE14-LE15 | Pure proposals and fresh owned ports; no engine history claim | Separate Propose/atomic Apply; exactly one Changed history step; NoChange creates none; exact Undo/Redo, late-cancellation and historical evidence behavior |
| LE16 | Full Create -> populate via accepted attributes/text/value-of -> Rename -> Delete -> Create using actual descriptors, not IDs replayed across snapshots | Both operations integrated together with the existing creation UI |
| LE17-LE18 | Candidate validation, synthetic transform goldens and public-only package/provenance tests; ordinary execution remains separate from proposal | Real worker previews/ExactBytes goldens; restricted export receipt/hash/reopen; native interaction acceptance and adoption of a separately published release |

Keep existing creation, attribute, expression, text/value-of insertion/deletion and replacement tests intact. Include no-namespace elements whose local names resemble instruction names, namespace-prefix rebinding, default-namespace resets, imported lexical variants, unchanged-output Changed operations and both supported/non-supported merge boundaries. No rename/delete test has run yet; these are future acceptance obligations.

Before implementation: Braid reviews the scope decisions and API/evidence shape above, then the owner approves the reviewed design. The implementation PR must deliver both operations, all declared bounds/refusals, public XML documentation and regression tests; run the full solution plus manual QT3/qt4 gates before merge. A later package publication and one Braid integration PR follow exact implementation review. Additive public functionality requires the repository's minor-version policy; no target version is assigned here.

## Smoke checkpoint and parallel work

The user reports Bosak1.23.8 smoke passed. Read-only review of Braid's `.acceptance/bosak-1.23.8-smoke/verification.json` confirms recorded **1,405 Release /710 public /36 release-regression passes**, no failures/skips, and nine consumed assembly hashes matching Bosak's [signed public release proof](RELEASE-1.23.8-PUBLICATION.json). Signature success is reported by that consumer receipt; Braid tests/signatures were not rerun here. Counts overlap source coverage.

Braid reports its adopted pin remains1.23.7. This smoke establishes neither1.23.8 adoption nor a new native acceptance. No Braid file, dependency, session or license state is changed by this review. [CSV draft PR420](https://github.com/Fytala-Charles/Bosak/pull/420) independently has green CI and remains open for review; it is not merged or incorporated into this design branch. Pending upstream notes, original intake/smoke evidence and protected PR132 remain preserved.
