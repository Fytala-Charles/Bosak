# REQ-138: Conditional wrapper authoring for Bosak.Braid

Prepared and registered: 2026-10-24. Requester: **Bosak.Braid**. Status: **Pending / target TBD / owner Unassigned**. Actual upstream request: **REQ-138**. The owner explicitly approves documentation-only registration; implementation/API design, target release and engine changes remain Bosak decisions.

## Outcome and delivery

Deliver one bounded conditional-authoring feature: **Wrap in xsl:if**, **Edit condition**, and **Remove condition wrapper, keep contents**. Keep both new structural operations in one Bosak implementation PR, require compatibility with the existing condition-edit API, and integrate the complete workflow in one later Braid PR. Removing a condition wrapper preserves its contents; deleting its entire content is a separate, unrequested operation. No xsl:choose/otherwise, arbitrary node wrapping, cross-parent movement or drag/drop.

Design and implementation must remain engine-owned. Braid must not parse/generate/splice XML, manufacture wrapper authority from insertion/deletion, or use reflection/friend access as a bypass.

## Capability audit and reuse

Published Bosak1.25.0 at `c005054bb096559368df985359e038e254814d12` exposes generic `ProposeExpressionEdit` and Expression inspection for an existing expanded XSLT if element's unqualified test attribute. Six independent public-package probes pass: four encoding variants edit true() to false(), compile/execute, retain original bytes and prove exact unchanged bytes outside the slot; two malformed conditions refuse. This is existing support, not a new conditional-authoring implementation. Initial audit assertions mistakenly required recorded compilation after setting AttemptCompilation=false; corrected probes enable compilation, with initial/final logs retained.

Braid already routes principal Expression slots through ProposeExpressionAsync and separate ApplyExpression; byte-identical candidate feedback grants no Apply/history. Reuse that path where it meets the reviewed lifecycle contract. Do not silently strengthen it to logical-expression equivalence or introduce a duplicate engine condition editor. Textual audit of the published capability/snapshot declarations and current upstream2580823a finds no public conditional Wrap/Unwrap API. The newer main audit is textual, not a runtime test or dependency adoption.

## Proposed first profile

- One engine-approved no-namespace ordinary literal result element E, imported or engine-authored, in the dependency-free principal module. Its supported parent is a top-level template or an ordinary no-namespace literal result parent reached through a creation-compatible literal-only ancestor chain. No implicit support for arbitrary sequence constructors or conditional ancestors.
- Keep supported explicit3.0/4.0 Legacy-host, closed static/instruction/attribute/resource coverage consistent with accepted element lifecycle capabilities; retain all E's attributes, descendants, instructions, comments/PIs and namespace declarations byte-exactly. Original and candidate whole-module restricted validation must pass. An operation cannot repair an invalid original into eligibility.
- Wrap inserts one engine-spelled expanded-XSLT if wrapper with one unqualified test attribute. The engine owns QName/prefix choice and escaping. Initially use an existing nonempty prefix correctly bound to XSLT at the wrapper location; if no safe prefix exists, refuse explicitly. Do not invent/rebind namespaces, change default namespace/static context, or take a caller-supplied prefix/range. Alias/default-reset/rebinding cases must be proved, not guessed from lexical names.
- Unwrap targets an actual expanded-XSLT if element I with exactly one unqualified Expression test attribute and no other attributes or local namespace declarations. Contents C contain exactly one supported E, optionally surrounded by ordinary whitespace-only text descriptors. Refuse significant sibling text, comments/PIs/instructions alongside E, multiple/zero literal children, self-closing empty if, wrapper namespace declarations, xml:base/lang/space, version/expand-text and other context-affecting attributes. Interior content inside E remains intact. Maintainers may propose a safely narrower or broader profile for explicit consumer review.
- Exclude CDATA, preserved-space, unsupported lexical/static contexts and unproved namespace relocation. Prefix/static bindings and all retained descendants' semantics must remain valid after reparenting; wrapper removal must not drop a declaration that retained content depended on.

## Exact source and semantics

For wrapping, expose/prove `P + E + S` becomes `P + O + E + X + S`, with O/X the complete engine-owned opening/closing wrapper spellings and O containing the properly escaped test value. E/P/S remain byte-identical. Do not generate interior indentation or normalize original XML. For unwrapping, prove `P + O + C + X + S` becomes `P + C + S`: remove only wrapper markup and its test metadata, preserving every retained inner and outside byte. Preserve BOM, strict encoding, Unicode, entities, quote spellings outside newly authored test bytes, CR/LF and all local namespace declarations on retained E.

Expose all exact source edits with original/candidate ownership, complete old/new payload availability, same-original coordinate/replay convention, nonoverlap/order and checked translations. One or two markup edits are acceptable if proof is complete; each operation is one eventual Braid history step. No off-record formatting, serialization or whitespace cleanup.

Wrapping intentionally gates evaluation/output, and unwrapping makes retained contents unconditional. Neither promises equivalent sampled output. Evaluate the test as an ordinary XSLT if in its actual static/dynamic context; the operation itself must not run a transformation or evaluate caller input. Condition editing uses the actual owned test Expression slot, contextual compilation and the existing exact-byte NoChange policy. Equal output or logically equivalent XPath with different source bytes is not NoChange. Already wrapped/ineligible targets refuse rather than implicitly nesting or unwrapping another node.

## Complete identity and reparenting proof

Independent capability/profile discovery and opaque operation-specific snapshot-owned targets must bind selected E or I, actual parent/child/optional boundary-text descriptors, extents, engine/profile/module and operation. Equal-byte foreign snapshots, old numeric IDs, another operation's authority or caller-provided markup cannot authorize a candidate. Expected failures remain typed; unexpected defects remain defects.

Changed evidence includes complete original/candidate source/ordinary inspection, exact edits, explicit original/candidate wrapper/retained-child/parent selection and exhaustive descriptor/per-owner-attribute correspondence:

- Wrap creates exactly I and its test attribute; every actual original node/attribute survives, all mappings are total/injective and candidate created/mapped partitions exhaust inspection. E moves under I; the common parent's child position is replaced by I, and E's subtree internal order/kinds/names/values/attributes/raw bytes remain intact. Only that subtree's depth increases by one; all other parent/depth/order changes refuse. Wrapper-owned namespace metadata, if later proposed, requires explicit new proof rather than silently classifying it as preexisting.
- Unwrap removes exactly I and its test attribute; every other actual descendant/attribute survives or is explicitly accounted for by a supported text merge. E replaces I under the original parent, its subtree depth decreases by one and internal order is retained. Never reuse whole-subtree Delete evidence: no retained E content is removed.
- Preserve C's optional inner ordinary whitespace verbatim. It may become adjacent to outside ordinary whitespace when tags disappear. Either refuse such boundaries explicitly in a reviewed narrower first profile, or expose bounded complete text-merge fragment provenance at **both** wrapper boundaries (up to two merges): original owned text descriptors/ranges, candidate fragment ranges, exact byte concatenation, normalized logical values and resulting parent/order. Removed old text descriptors must be accounted for in merge partitions; no unreported split/merge or trivia association.
- Root/document/module/container/outside nodes and all attributes are included. Check exact byte and strict character/line coordinates with piecewise translations for insertions/deletions and explicit parent/depth exceptions. Missing/duplicate/foreign/unaccounted descriptors or inability to prove context preservation refuse.

After Wrap select actual candidate I and offer its actual test slot for later separate editing. After Edit condition select the current I/test extent through existing correspondence. After Unwrap select the actual retained candidate E. Repeated names, equal-looking subtrees and whitespace merges must not cause selection by name/stale ID. Fresh candidate ports must support subsequent operations.

## Bounds, review, Apply and history

Targets/results/byte exports/collections must be immutable or defensive. Declare independent bounds for original/candidate bytes, test text/escaped output, wrapper/subtree/separator scans, graph nodes/attributes/depth, complete copied evidence, edits and checked coordinate arithmetic before allocations or recursive work. Observe cooperative cancellation during bounded traversal/copy/validation/proof and immediately before any success/NoChange. Proposals are pure/concurrently isolated, invoke no ambient file/network/extension acquisition and never execute the transformation.

Braid shows operation, complete selected subtree/wrapper and resulting retained content, exact before/after spans/edits, condition text, changed execution disclosure, namespace/boundary policy, actual selection and full source/session/revision/engine/profile provenance. Unwrap requires explicit acknowledgement that the condition is removed while contents remain. Review has its own complete-evidence size refusal, not truncation. Changing intent/condition/selection/source/policy invalidates held input or review as appropriate.

Separate Propose and Apply. The atomic source-owner boundary rechecks exact owner/source/inspection/port/module/operation/revision/license, frozen complete evidence, cancellation and overlap. Changed creates one exact-byte history step; byte-identical condition feedback creates none and preserves redo. Undo/Redo re-inspects and restores actual wrapper/retained-element navigation with fresh identities. Prior validation/preview/export remains historical even if bytes recur. Precommit refusal preserves current source and still-current explicit retry; committed late cancellation reports success.

Existing Create/Rename/Delete/Move eligibility must not silently broaden through a newly introduced if ancestor. Preserve original operation contracts; explicitly test refusals where those operations require literal-only ancestry. Unwrap restores the accepted original context. Where operations are already independently supported inside the preserved subtree, prove fresh-port interoperability.

## Planned acceptance groups

These are requirements, not executed Wrap/Unwrap results.

| Group | Required evidence |
|---|---|
| IF01 | Independent capabilities; opaque Wrap/Unwrap authority; stale/foreign/equal-byte/other-operation targets refuse |
| IF02 | Imported/authored supported E, top-level-template/literal parent eligibility, closed original/candidate validation and explicit host/version boundaries |
| IF03 | Exact Wrap insertion, full E/outside preservation, proper condition escaping, safe XSLT prefix alias/rebinding/default-reset refusal or proof |
| IF04 | Existing public test-slot Edit condition, contextual valid/invalid XPath, exact-byte NoChange/redo versus equal-output Changed |
| IF05 | Exact Unwrap markup-only removal, retained C/E, declared both-boundary whitespace/merge policy and actual selected E |
| IF06 | Total created/mapped/removed/merge node/attribute partitions, explicit reparent/depth/order exceptions and root/module/outside accounting |
| IF07 | Refuse wrapper declarations/context attributes, absent/duplicate test, unsupported/multiple/empty/significant contents and invalid originals |
| IF08 | Four strict encodings, Unicode/entities/quotes/CRLF, paired/self-closing E, nested populated subtrees, repeated names and exact physical ranges |
| IF09 | Independent bytes/text/escaping/scan/graph/depth/evidence/edit/overflow limits before work; complete refusal, no truncation |
| IF10 | Defensive exports, concurrent isolation, cancellation during bounded work/before success, denied ambient acquisition and no transform evaluation |
| IF11 | Malformed/duplicate/missing/foreign source/edit/correspondence/attribute/merge proof refused before Braid review or Apply |
| IF12 | Shared outline/graph review, explicit unconditional-content acknowledgement, correct selection, intent/currentness/license/busy/Cancel/Close/retry gates |
| IF13 | One-step exact Undo/Redo; NoChange retains redo; original files intact and old validation/preview/export evidence historical |
| IF14 | Wrap/Edit/Unwrap and existing lifecycle compatibility with declared ancestor restrictions; true/false sample inputs and error/refusal cases |
| IF15 | Every changed stage validates, real-worker ExactBytes true/false goldens, export bytes/hash, independent reopen/execute across encodings; signed public-package provenance and short separate native review |

## Delivery ownership and next gate

Bosak owns acceptance, API names/design, implementation/engine tests and release decisions. Braid requests one coherent lifecycle delivery, reusing existing test-slot editing rather than assuming a new API is needed. Register the need before engine work, retain the actual upstream ID, review the scope/API and exact implementation, independently verify signed publication, then integrate/accept the whole workflow together. No implementation/version/merge/publication approval is granted by this prepared request.

Braid local contract: `CONDITIONAL_WRAPPER_REQUEST.md` and proposed `ADR-025-conditional-wrapper-authoring.md`, under `D:/Development/Bosak.Braid/docs`. These planning files are not yet pushed, so no invented remote link is supplied. Existing public references at immutable Braid56f2c18: [accepted reordering](https://github.com/Fytala-Charles/Bosak.Braid/blob/56f2c18e4b3483b38698bf2b2054264e6ba14270/docs/LITERAL_ELEMENT_REORDER.md), [existing expression review](https://github.com/Fytala-Charles/Bosak.Braid/blob/56f2c18e4b3483b38698bf2b2054264e6ba14270/docs/EXPRESSION_EDITING.md), and [source ownership boundary](https://github.com/Fytala-Charles/Bosak.Braid/blob/56f2c18e4b3483b38698bf2b2054264e6ba14270/docs/ADR-002-source-preserving-authoring-boundary.md).

Registration textual re-audit of current local upstreamfa5c7ec0 finds no public conditional Wrap/Unwrap entry point in snapshot/capability declarations. No new runtime test, upstream source/API edit, version/release change, commit or push is performed by registration. Existing six public1.25.0 audit probes remain baseline evidence, not Wrap/Unwrap tests. Existing REQ-137 acceptance and unrelated working changes are preserved.
