<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala proposed conditional wrapper authoring contract">
  <h1>REQ-138 maintainer scope and API proposal</h1>
  <p>25 October 2026 — proposed design for Bosak.Braid review</p>
</div>

**Design proposal only.** REQ-138 remains **Pending — maintainer design proposed; consumer review required**. Design owner: **Bosak**. Target: **TBD, a subsequent feature release**. The owner authorized preparing this design, independently of the numeric 1.25.2 candidate. No conditional Wrap/Unwrap API, implementation approval, feature version, package publication or Braid integration is delivered here. Reviewed source: `3785672d695cbc8467e1c027d5ce4304e9f92152` (PR449 merge); public packages remain 1.25.1.

The [registered request](REQ-138-conditional-wrapper-authoring.md) is retained byte-exact as the original planning record, including its historical Pending/Unassigned labels. [Original Braid request](req138-evidence/original-braid-request.md), [proposed ADR-025](req138-evidence/original-braid-adr.md) and [source hashes/provenance](req138-evidence/provenance.json) are retained. Relative citations in those original Braid files refer to their original Braid documentation context. The six public 1.25.0 condition-edit probes are historical baseline evidence; this step is a document/current-source review and does not rerun them or claim Wrap/Unwrap acceptance.

## Delivery and initial profile

Deliver **Wrap in xsl:if**, **Edit condition**, and **Remove condition wrapper, keep contents** as one coherent lifecycle. The two structural operations and the necessary bounded/cancellable bridge to the existing expression editor belong in one engine implementation PR. Braid integrates the complete lifecycle in one subsequent PR after independent signed-package verification. Whole-subtree deletion, choose/otherwise, arbitrary wrapping, cross-parent movement, drag/drop and automatic formatting are outside this profile.

Wrap targets exactly one ordinary no-namespace literal result element E, imported or engine-authored. Its immediate parent P is a supported top-level template or ordinary no-namespace literal parent with a creation-compatible literal-only ancestor chain. An existing conditional ancestor is ineligible; Wrap does not implicitly nest conditions. Keep the dependency-free principal module, explicit 3.0/4.0 stylesheet/transform root, captured Legacy authoring host, closed instruction/static/resource coverage and supported retained element vocabulary established by REQ-136/137. Original and candidate whole modules must both pass restricted validation; an invalid original cannot become eligible through this operation.

Unwrap independently targets an actual expanded-XSLT if element I under the same supported P ancestry. I has exactly one unqualified `test` Expression attribute, no other attributes and no local namespace declarations. Its content C is exactly one supported E, optionally preceded/followed by ordinary whitespace-only text descriptors. Reject empty/self-closing wrappers, multiple literal children, significant sibling text, comments/PIs/instructions alongside E, CDATA, effective preserved space and unproved contexts. Retain all supported attributes, descendants, instructions, comments/PIs and namespace declarations *inside E* byte-exactly. I's `xml:base`, `xml:lang`, `xml:space`, `version`, `expand-text`, extension/static context attributes and declarations are ineligible.

Unwrap checks E's hypothetical retained `P -> E` ancestry with precisely the one approved I edge removed. It must not globally relax the existing literal-only ancestry predicates, which would silently broaden Create/Rename/Delete/Move through conditional ancestors. Generic condition editing and structural Unwrap have independent discovery/eligibility: an editable test slot does not establish Unwrap authority.

## QName, namespace and expression context

Wrap uses a nonempty prefix already bound to XSLT in **P's context at the insertion position**. A binding declared only on E cannot authorize the wrapper's QName. Choose deterministically from eligible in-scope aliases using ordinal prefix order; default-only XSLT bindings or lack of a proven prefix refuse. No caller-supplied prefix, namespace declaration, rebinding or default-namespace change is allowed. Preserve E's local declarations and prove retained prefix/default/static bindings through the added or removed edge, including shadowed aliases and default resets.

Create paired opening/closing markup with exactly one unqualified test attribute. The engine owns quote spelling and escaping. Validate/compile the supplied condition in the **actual candidate I/test static context**, which inherits P rather than E's local bindings. Reuse the expression validator internally; do not simulate insertion by calling a consumer edit/delete API. Preserve the expanded condition text through attribute normalization, including tab/CR/LF by appropriate character references. Reject malformed syntax, unavailable prefixes, invalid XML characters, unsupported static/function coverage and unrepresentable text. Validation performs no transformation or dynamic evaluation of the caller's condition.

Restricted inspection/validation denies ambient file, network, resolver and extension acquisition before compilation. Merely retaining an original snapshot's resource callback is not sufficient proof of this boundary. Test explicit hostile callbacks. Wrapping gates execution/output and unwrapping makes retained content unconditional; sampled-output equivalence is not an eligibility or NoChange rule.

## Proposed additive API

These are review names, not current declarations. Existing authoring members/capabilities remain intact.

| Surface | Proposed contract |
|---|---|
| Capabilities | Independent `SupportsConditionalWrapping`, `SupportsConditionalUnwrapping`, profile identifier, supported encodings and published limits |
| Wrap discovery | `snapshot.GetConditionalWrapTarget(element, cancellationToken)` -> opaque `AuthoringConditionalWrapTarget` or typed refusal |
| Wrap proposal | `snapshot.ProposeConditionalWrap(new AuthoringConditionalWrapProposal(target, conditionText), cancellationToken)` |
| Unwrap discovery | `snapshot.GetConditionalUnwrapTarget(wrapper, cancellationToken)` -> distinct opaque `AuthoringConditionalUnwrapTarget` or typed refusal |
| Unwrap proposal | `snapshot.ProposeConditionalUnwrap(new AuthoringConditionalUnwrapProposal(target), cancellationToken)` |
| Structural outcome | Exactly one Changed candidate or Refused result, with operation Wrap/Unwrap bound into the evidence |
| Target ownership | Internal construction, exact snapshot/module/profile/operation, actual E/I/P/optional boundary descriptors and checked source extents |
| Changed candidate | Original/fresh candidate inspections and source, complete edits, created/removed/survivor/merge partitions, explicit reparenting and actual selection/test slot |

Wrap/Unwrap necessarily change source length. They have no structural NoChange outcome: already wrapped, ineligible or foreign targets refuse. No operation enum override can convert one target's authority into another operation. Numeric IDs, equal-looking nodes, equal-byte foreign snapshots, caller ranges/markup and stale/other-operation targets confer no authority. Proposal rechecks all captured ownership, profile, source alignment and eligibility. Null/programmer misuse follows existing constructor conventions; expected input/profile/cancellation failures are typed; unexpected defects remain defects.

Proposed structural failure categories: ForeignSnapshot, IneligibleTarget, UnsupportedModule/Profile/Encoding/Coverage, NoSafeXsltPrefix, UnsupportedNamespaceContext, InvalidCondition, InvalidOriginal, InvalidCandidate, SourceAlignmentFailure, UnsupportedCorrespondence, SizeLimit and Cancelled. Actual enum/type names are subject to consumer review. Refusal carries no candidate/Apply authority. Arrays, collections, byte exports and evidence are immutable or defensive.

## Reusing condition editing: explicit compatibility bridge

Current `AuthoringSnapshot.ProposeExpressionEdit(AuthoringEditProposal)` returns candidate-or-failure and has **no CancellationToken overload**. Existing candidates expose reinspection/node correspondence and changed-slot metadata; the current result does not itself express the new structural outcome model. These facts must remain explicit.

Propose an additive overload of the **same editor**, `ProposeExpressionEdit(AuthoringEditProposal, CancellationToken)`, with a separately advertised bounded principal-module profile (`SupportsBoundedExpressionSlotReplacement` and its limits). Preserve the existing one-argument behavior and binary compatibility; no overload default argument should make existing calls ambiguous. The token overload shares validation/escaping/splicing logic, checks independent source/text/graph/evidence limits before work, observes cooperative cancellation during bounded stages and before returning a candidate, and internally proves complete one-to-one node/per-owner-attribute correspondence with exactly the declared test slot changed. Keep candidate/result shapes compatible; append necessary Cancelled/SizeLimit/UnsupportedProfile/InvalidOriginal/InvalidCandidate/UnsupportedCorrespondence refusal values without renumbering existing enum members. This bridge is proposed implementation work, not a current guarantee.

Braid derives the proposal's ID/name from the actual owned I/test descriptor and verifies the source/session/revision/slot owner before using this existing numeric-slot interface. It rechecks ordinary candidate correspondence and exact test-byte ownership. A byte-identical complete candidate is original-selection feedback: no Apply, fresh-port adoption, history step or redo invalidation. Different bytes remain Changed even if expressions are logically equivalent or yield the same output. The bounded conditional workflow uses the new capability/overload; unrelated existing expression-edit routes retain their current contracts. Braid must confirm this additive bridge in design review.

After Wrap, select actual candidate I and expose its actual test slot. After condition editing, map the current I/test using the editor's owned correspondence. After Unwrap, select the actual retained candidate E. All adopted candidates use fresh ordinary-inspection ports. Do not find nodes by names, text equality or stale IDs.

## Exact source edits and coordinate convention

For original E at physical byte interval [a,b), Wrap proves:

```text
P + E + S  ->  P + O + E + X + S
```

O/X are engine-owned opening/closing markup; E/P/S and BOM are byte-identical. Emit two original-coordinate zero-width edits at a and b with complete new payloads and explicit candidate ranges; old payloads are empty. Define the list in ascending original order, replay simultaneously against the same original (or apply descending offset order). Candidate opening range is [a,a+|O|), retained E is [a+|O|,b+|O|), and closing range begins at b+|O|. No interior indentation is authored.

Unwrap proves `P + O + C + X + S -> P + C + S`. Emit two nonoverlapping original-coordinate deletions for the exact opening/closing markup, with full old payloads, empty new payloads and checked zero-width candidate positions. C includes all optional inner ordinary whitespace verbatim. Outside P/S and every retained C byte are unchanged. Each operation is one eventual history step even though it contains two edits.

Translation is component/ownership-aware at insertion boundaries: retained E ranges translate by |O|, prefix ranges keep their positions, suffix ranges translate by |O|+|X|, and containing ancestors retain starts while their ends include both insertions. A retained E endpoint at b precedes X; a suffix start at b follows X. A single unqualified point-delta function cannot assign both correctly. Unwrap uses the inverse component rules and explicit merge-fragment ranges. Prove strict encoded boundaries, physical bytes, UTF-16 character offsets and line/column coordinates against actual original/candidate coordinate maps, including container and ancestor extents. Checked arithmetic precedes allocation/copy. No off-record serialization or formatting is permitted.

## Total graph and two-boundary merge proof

Wrap creates exactly I and its test attribute. Every actual original descriptor/attribute survives in a total injective mapping; the mapped/created candidate partitions exhaust ordinary inspection. Only E's root changes parent to I; its subtree depth increases by one, with internal order/kinds/names/values/attribute ownership and source intact. P's E position is replaced by I, whose sole child is E. Other parents/order/depth remain mapped equivalents. Include document, module, containers, ancestors, outside nodes and every per-owner attribute.

Unwrap removes exactly I and test. Every other original descriptor/attribute survives either in the bijective survivor map or an explicit merge partition. E replaces I under P; E and its subtree depths decrease by one. Optional retained inner whitespace also moves to P at the corresponding depth. P's child sequence is proved after those replacements and merges; all other child orders are preserved. Created candidate nodes are forbidden except merged text descriptors with complete fragment provenance.

**Support up to two distinct ordinary whitespace merges in the first profile.** At the opening boundary, outside-left and inner-left whitespace may coalesce; at the closing boundary, inner-right and outside-right may coalesce. E separates these groups. Each group has the actual candidate text descriptor and exactly its ordered original fragments, their owned source descriptors/ranges and translated candidate fragment ranges. Merge participants are excluded from the simple survivor map, exhaust the original/candidate merge partitions and cannot be reused across groups. Reuse `AuthoringTextMergeGroup`/fragment data where compatible; the existing deletion validator's single-group rule is insufficient and must not be reused unchanged.

Prove raw fragment byte concatenation exactly equals candidate text bytes. Recompute the group's logical value using ordinary XML newline normalization across the complete concatenated bytes; do not simply concatenate independently normalized old Text values. A CR at the left fragment end followed by LF at the right start becomes one logical newline after the markup is removed. Explicitly test this seam and physical coordinates for all encodings. Missing fragments, duplicates, foreign descriptors, unexplained splits/merges, unsupported lexical forms and inability to prove normalization/ranges refuse. Whitespace cleanup and trivia reassociation are forbidden. Whole-subtree Delete evidence never authorizes Unwrap.

## Proposed independent limits and cooperative cancellation

| Material/work | Proposed bound |
|---|---|
| Original / complete candidate source | 8,388,608 physical bytes each, independently checked |
| Condition text | 65,536 UTF-16 code units; reject invalid Unicode/XML characters |
| Encoded escaped condition | 1,048,576 bytes, pre-counted before output allocation |
| Selected in-scope prefix | 256 UTF-16 code units; over-limit aliases cannot be used |
| Complete opening + closing markup | 1,052,672 encoded bytes |
| Each retained subtree / boundary scan | Within source bound; at most two boundary groups, two fragments each |
| Original / candidate graph | 65,536 nodes, 65,536 attributes, maximum depth 256 each; bound candidate depth before insertion |
| Complete copied evidence | 33,554,432 bytes cumulatively across actual retained source copies, edit payloads, selected spans and repeated evidence |
| Structural edits | Exactly two; same-original coordinates, nonoverlap and checked replay |

The bounded expression overload adopts independent source/condition/escaped/graph/evidence limits from this profile; its one attribute-value edit remains an expression edit. Limits count actual duplication conservatively and can refuse a source smaller than the source maximum. They bound proposal work/material and a complete defensive export, not total process heap or arbitrary repeated caller exports. Braid has its own complete-review bound and refuses rather than truncating evidence.

Check cheap limits before scans, allocations and recursive inspection. Use bounded/iterative graph traversal. Observe tokens during scanner/proof/traversal/copy batches (proposed maximum 64 KiB copied bytes or 128 descriptors between checks), before/after opaque parse/compile phases and immediately before Changed/byte-identical feedback. Cancellation is cooperative; this proposal makes no hard preemption guarantee inside an opaque compiler call. A requested cancellation yields typed Cancelled with no candidate authority; unrelated unexpected cancellation defects remain defects. Concurrent proposals remain isolated and do not mutate original snapshots. Test cancellation deterministically during each bounded phase, not through timing assumptions.

## Consumer review, Apply and history

Braid reviews complete before/after source and selected content, both edits/payloads, condition text and changed-execution disclosure, namespace/merge policy, actual selection and full source/session/revision/module/engine/profile provenance. Unwrap requires explicit acknowledgement that the condition is removed and content remains. Changes in intent/condition/selection/source/policy invalidate held input or review according to the existing ownership boundary.

Propose and Apply stay separate. Before atomic source-owner commit, Braid rechecks exact owner/source/inspection/port/module/operation/revision/license, frozen complete evidence, cancellation and edit overlap/replay. Changed creates one exact-byte history step. Precommit refusal preserves source and a still-current explicit retry; cancellation arriving after commit reports committed success. Byte-identical condition feedback creates no step and preserves redo. Undo/Redo re-inspects and restores actual I/E navigation with fresh identities. Validation, preview, worker and export receipts remain historical even if bytes recur.

Verify that existing lifecycle operations retain their ancestry restrictions inside a newly introduced if. Independently supported operations can continue only with their own fresh-port eligibility; this feature grants no implicit Create/Rename/Delete/Move authority. After Unwrap the supported original context is restored. No Braid parsing/splicing, manufactured wrapper authority, reflection or friend access bypass is permitted.

## Planned evidence ownership (not executed acceptance)

| Groups | Bosak engine/design evidence | Braid consumer evidence |
|---|---|---|
| IF01–IF03 | Independent operation capabilities/opaque ownership; parent-context QName selection; exact paired edits and whole-module eligibility | Availability, intent/currentness and correct refusal/review presentation |
| IF04 | Additive bounded/cancellable existing editor, contextual condition compile, exact byte correspondence | Existing expression route; no-change selection/history/redo and equal-output Changed |
| IF05–IF06 | Both boundary merges, CR/LF seam, complete survivor/created/removed/merge partitions and reparent/depth/order proof | Independent evidence rejection and actual selected I/E/test navigation |
| IF07–IF08 | Namespace/context/content refusals; four encoding variants, entities/Unicode/quotes/CRLF, paired/self-closing E and repeated names | Encoding/refusal matrices and intact imported files |
| IF09–IF11 | Independent allocation/work/evidence limits, cancellation/concurrency, defensive exports, denied acquisition and malformed/foreign/duplicate proof refusal | Independent complete-review bound and refusal before review/Apply |
| IF12–IF14 | Fresh-port lifecycle compatibility and unchanged ancestry gates | Shared outline/graph review, unconditional-content acknowledgement, atomic Apply/retry, exact one-step Undo/Redo and historical receipts |
| IF15 | Reviewed implementation, full tests and frozen gates; signed package/source provenance after authorized release | Independent signed-package adoption, true/false real-worker ExactBytes goldens, export/hash, reopen/execute and short separate native acceptance |

Implementation must run full solution/LSP tests and fresh QT3 exactly 31,142/0/679 and frozen qt4 8,436/0/1,894 across 224 configured sets unless a separately reviewed change explains a difference. Native current Unix 22/2/0 and existing Experimental fallbacks remain explicit. These numbers are the current engine baselines, not conditional-wrapper test results.

## Design review gate and next sequence

Braid should review: both structural operations together; the bounded parent/child profile; two original-coordinate edits; parent-context prefix selection without declarations; both boundary merges and CR/LF normalization; exhaustive reparenting partitions; additive bounded/cancellable expression overload with legacy compatibility; limits/cancellation guarantees; and selection/Apply/history semantics. Record the exact reviewed design revision and findings in a separate consumer design review.

After scope/API review and owner implementation approval, implement the whole lifecycle in one engine PR and obtain exact-head consumer implementation review. A subsequent feature release requires independent publication authorization and signed-package verification. Then Braid integrates and accepts the workflow together. The numeric 1.25.2 candidate stays separate; this document does not assign REQ-138 a version or report implementation/native acceptance.
