# REQ-133 - Replace One Existing Literal Text Body for Bosak.Braid

Submitted: 2026-10-18. Requester: Bosak.Braid. Status: Pending. Owner: Unassigned. Target Version: TBD. Documentation-only consumer request; API acceptance, implementation, tests and release decisions remain Bosak-maintainer responsibilities. Current upstream main1eb91d1ceeb427a8097fe2db5b21c41e8a66d780 was checked for request-ID collisions before registration.

## User Outcome and First Profile

Select an existing `xsl:text`, review its current decoded logical text, replace a label or separator, review the exact candidate and explicitly Apply. This complements accepted text insertion and avoids adding a second instruction when the user intends to change the first.

The first profile replaces only the body of one engine-approved principal paired ordinary literal `xsl:text`. Input is exact logical Unicode, including empty text, spaces, tabs, physical newline code points, braces and markup-looking text. No trimming, Unicode normalization, XPath/AVT evaluation or user-supplied XML is permitted. XML escaping and character-reference emission belong to Bosak.

Preserve all bytes outside the original body range, including opening/closing tags, prefix spelling, namespace declarations, attributes, surrounding indentation, comments/PIs, declaration/BOM and source encoding. Empty new text retains the existing paired tags and yields an actual empty-body caret. Readable layout has no role in this operation: intentional payload whitespace must not become formatting.

Self-closing instruction expansion, arbitrary text nodes, literal-result element text, mixed/CDATA/comment/PI instruction bodies, dynamic text-value templates, output escaping, attribute/control edits, namespace changes, deletion and arbitrary subtree replacement are excluded. Bosak defines precise static/resource/profile eligibility and may conservatively refuse additional contexts. Effective non-expansion and no output escaping must be proven by the engine; Braid must not infer these from lexical names. Preserve relevant `xml:space` metadata rather than treating payload whitespace as indentation; unsupported contexts refuse explicitly.

## Capability Audit and Ownership

Published adopted1.20.0 at91f1af0d488bb2ed72b219118eeb1ab3570726bd and refreshed upstream main1eb91d1ceeb427a8097fe2db5b21c41e8a66d780 expose text insertion, not existing text-body replacement. Expression replacement edits attributes and does not authorize this operation. Current insertion target/proposal/identity evidence cannot be reused as permission to replace a body.

Bosak owns target eligibility, logical old/new text semantics, XML character legality/escaping, static context, exact body extent, source generation and actual descriptors. Braid owns session/inspection/license/revision checks, defensive review, explicit atomic source-owner Apply, history and navigation. No Braid parser, raw source splice, engine patch or friend/reflection bypass is allowed.

## Requested Public Contract

- Additive separate text-content replacement capability and snapshot-owned opaque target for an exact actual `xsl:text` descriptor. Exact public API names, bounds, owner, version and release are maintainer decisions.
- Discovery returns immutable engine-identified original instruction/parent/module, exact original body range, old logical text and actual old payload descriptor or explicit absence for an empty body. Name resemblance, numeric IDs from other inspections and source offsets cannot create a target.
- Proposal accepts this target and exact replacement logical text. Validate XML characters/Unicode using supported engine policy; classified invalid-text, unsupported-context/encoding, size, foreign-target and cancellation refusals retain the original snapshot. Existing authoring APIs and failure values remain compatible.
- If new logical text equals old logical text ordinally, classify an explicit **NoChange** outcome without a candidate that would rewrite lexical entity spellings, normalize source or create history. Braid preserves the bytes and current revision. No case folding, trim or Unicode normalization is part of this comparison.
- A changed proposal returns defensive exact original/candidate sources and ordinary inspection, accepted old/new logical text, original/replacement body ranges, complete single body SourceEdit, actual mapped instruction/parent, actual new payload descriptor or explicit absence, and exact encoded payload range/caret. Opening/closing tags and all outside bytes must replay unchanged; no implicit formatting or control-attribute rewrite.
- Define explicit disjoint descriptor partitions. Every original descriptor outside the old payload maps injectively to one actual candidate counterpart, preserving structure/names/attributes/logical content except enlarged owner/ancestor ranges. Account explicitly for the replaced old payload (if any) and actual new payload (if any); do not assume total ordinary-survivor mapping can include a disappearing text node. Empty-to-nonempty, nonempty-to-empty and nonempty-to-nonempty have distinct payload-presence cases. No synthetic child, arbitrary removed subtree, insertion/deletion merge relation or guessed cross-snapshot identity is accepted.
- Any extra, missing, split or merged descriptor outside that declared payload replacement must refuse alignment. Old/new payload evidence is actual inspection evidence, not a manufactured numeric-ID correspondence. Namespace/attribute descriptors retain existing metadata rules.
- Keep existing physical source bounds/encodings where compatible. Independently bound original and actual final source, including BOM, encoded escaping and replacement shrinkage; no hypothetical append-only size rejection. Exact limits and cancellation checkpoints must be documented. Deny unsupported preprocessing/resource acquisition before evaluation.

## Consumer Review, Apply and Selection

Future outline/graph commands **Check text edit target** / **Edit literal text** will be enabled only by a reviewed delivered public capability and exact current engine target. Show old logical text separately from lexical source and editable replacement intent. For NoChange, report that source is unchanged; do not enter history or reserialize entity spellings.

For a changed candidate, display the old/new logical values, exact original/replacement body bytes, whole resulting instruction and current source/revision/engine provenance. Any input/source change invalidates review. Oversized complete review refuses without truncating applicable evidence or silently truncating the user's input. Separate explicit Apply rechecks capabilities, current owner/inspection/revision, permissions, cancellation and overlap, then commits one exact-byte history step.

After Apply, rebuild outline, select the actual surviving instruction and highlight the new encoded payload or exact empty caret. Graph reopen uses that same current selection; no Expression slot or empty text descriptor is invented. Undo/redo restores exact bytes under new revisions with fresh inspection; old validation/preview/export receipts stay historical. Cancellation before commit retains source and retryable authority where valid; cancellation after committed acceptance reports success.

## Acceptance Matrix

| Gate | Required evidence |
|------|-------------------|
| TE-01 Eligibility | Imported/previously inserted paired `xsl:text`, prefix aliases/default XSLT namespace, principal-only scope; wrong node, dependency, self-close, unsupported body/static context refuse |
| TE-02 Logical values | Exact Unicode/decomposed sequences, markup-looking text, braces, spaces/tabs/CR/LF, XML references and empty body; invalid scalars/surrogates refuse |
| TE-03 Source fidelity | One exact body replacement; opening/closing tags, attributes, namespaces, comments/PIs and every outside gap unchanged in four encodings and newline variants |
| TE-04 NoChange | Same logical text preserves original entity/reference spelling and bytes/revision/history; distinct normalization forms or newline code points are not silently equated |
| TE-05 Provenance | Complete disjoint old-survivor/replaced-payload and candidate mapped/new-payload partitions; empty/nonempty transitions; malformed names/parents/map/ranges/payload evidence rejected |
| TE-06 Bounds | Physical original/final size, escaped expansion and shrink-to-fit, encoding declaration mismatch, overflow, cancellation and no lossy fallback |
| TE-07 Authority | Deep freeze, foreign/identical-byte reopened targets, stale inspection/revision, overlap, independent policy loss, cancellation/retry and committed late cancellation |
| TE-08 UI/history | Separate old logical/lexical review, changed-input invalidation, explicit Apply, complete bounded evidence, cancel/deferred close, exact instruction/payload/caret selection, graph handoff/reopen and one-step undo/redo |
| TE-09 Lifecycle | Insert text then edit; edit imported text repeatedly; empty-to-nonempty/nonempty-to-empty; fresh targets and current identities at every step; legacy operations unchanged |
| TE-10 Golden | Deliberately changed worker output, fresh validation and ExactBytes expected output, restricted new-file export/receipt/hash and independent destination reopen/validate/run across four encodings |
| TE-11 Native | Protected kit/manifests, explicit walkthrough and actual observed variants, immediate receipt/file hashes and reopened result confirmation; no inferred accessibility certification |

## Delivery Sequence

1. Registered with explicit product-owner authorization as REQ-133; maintainer API/profile acceptance remains pending.
2. Bosak owns API/profile acceptance, implementation, compatibility/source tests, CI and release decisions; independently review delivered source/package evidence.
3. Verify the actual published release and immutable pin before separate Braid typed adapter/review/Apply/history/selection/UI work.
4. Complete changed-output worker/export/reopen goldens and protected native preparation, then human review and formal acceptance when requested.

## Consumer References and Compatibility

- D:/Development/Bosak.Braid/docs/LITERAL_TEXT_EDITING.md: scope and eleven acceptance groups.
- D:/Development/Bosak.Braid/docs/ADR-019-reviewed-literal-text-replacement.md: proposed architectural decision.
- D:/Development/Bosak.Braid/docs/FEATURE_REQUESTS.md: Braid REQ-122 and upstream tracking.

Braid adopted public1.20.0 at91f1af0d and its accepted checkpoint is pushed through824307f. The text-editing design is an uncommitted follow-up. Existing expression/value-of/text insertion/deletion/literal-element APIs and their declared behavior must remain compatible; an existing instruction's runtime support is not replacement authority. No Braid parser, raw splice, reflection/friend bypass or engine edit is authorized by this request.
