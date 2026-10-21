# REQ-135: Literal Result Attribute Create, Update and Delete for Bosak.Braid

Date: 2026-10-20. Requester: **Bosak.Braid**, parent consumer REQ-122. Status: **Pending maintainer scope/API review; implementation owner Unassigned; target version TBD.** The product owner explicitly requests create, update and delete as one cohesive feature in a single implementation PR. This documentation PR registers that complete consumer need; it does not authorize engine implementation, reserve a release or split the feature into separate operation deliveries.

## User Outcome and Current Gap

Braid can add output elements and text/value-of instructions, edit literal text and delete supported instructions. A designer should also manage an output element's literal attributes as one lifecycle: create `status` with literal value `ready`, update it to `done`, then remove it. One coordinated implementation PR should supply all three operations, shared invariants and lifecycle/compatibility coverage.

Published Bosak1.22.0 is reviewed/adopted at3b00938875490d1997b09f53b80f2ee2986a666b. API audit of current main53c098d1529343b41f763168df1daa3b6ce8bdfe finds no literal-result-attribute create/update/delete capability or proposal in AuthoringCapabilities/AuthoringSnapshot. Existing expression replacement, read-only AVT attributes and element/text authoring do not grant this authority; SupportsAvtSlotReplacement remains false. No engine patch, reflection/friend access or Braid XML splice is proposed.

## Requested First Profile

- One engine-approved literal result element in the dependency-free principal module and the declared supported authoring/static/resource profile. A no-namespace literal result owner is sufficient for the first profile; Bosak must explicitly document any broader qualified-owner support. Preserve self-closing versus paired owner syntax and all children.
- One **unqualified attribute in no namespace**, with a valid XML NCName, exact case and Unicode spelling. No rename, namespace/prefix/declaration creation, xml:* metadata, XSLT control attributes, xsl:attribute instructions or arbitrary node editing. The reserved `xmlns` name is not an ordinary attribute; reject it. Bosak owns name legality and duplicate detection by expanded name.
- Values are exact logical XML1.0 Unicode strings, including empty, whitespace/newlines, quotes, ampersands, markup-looking text and braces. No trim, Unicode normalization, XPath or AVT expression input. Bosak must emit literal attribute semantics: double literal AVT braces as required and escape XML/quote/newline characters so actual output preserves the requested logical value. A request for `{name}` means literal braces/text, never evaluation.
- Update/delete target only an existing engine-proven **constant literal attribute**, including supported entity/reference spelling and escaped literal braces. Refuse dynamic AVTs, ambiguous/unproven lexical forms and unsupported static/resource contexts. Classification must come from Bosak's retained syntax/semantics, not Braid scanning for braces. Other attributes/context require declared captured coverage; target constancy alone is not whole-module validation.
- Invalid original modules are not repaired through update/delete. Original/candidate inspection, complete restricted static coverage, resource denial, strict supported encodings and independent physical bounds remain necessary. Bosak may conservatively narrow scope with explicit refusals, without changing previously accepted operation profiles.

## Three Explicit Operations, One Delivery

| Operation | Authority and intent | Outcome |
|-----------|----------------------|---------|
| Create | Owned supported element target plus proposed absent unqualified name and exact logical value | Changed candidate or typed refusal; duplicate name refuses, never overwrites |
| Update | Owned existing constant-attribute target plus exact new logical value; name is fixed | Changed / NoChange / Refused, exclusive; missing or foreign attribute refuses, never creates |
| Delete | Owned existing constant-attribute target; no replacement/name/layout input | Changed candidate or typed refusal; absent target refuses, never silently succeeds |

A shared family/capability and evidence primitives are welcome; explicit operation intent and operation-bound authority must remain distinguishable. Public class names, capability shape, flags and cancellation signatures belong to Bosak's design. Do not advertise full lifecycle support until all three are delivered and verified. No generic upsert or reuse of Expression/AVT authoring permission is requested. Existing public names, enums, capabilities, eligibility, defaults and legacy cancellation behavior remain compatible.

For Update, logical equality is ordinal after engine interpretation of a constant literal attribute. NoChange preserves every original byte (entity/reference spelling, quote style and escaped braces), source owner/snapshot and history; it supplies no candidate, edit or Apply authority. Same output in one sample does not establish NoChange. Names are not case-folded; normalization-distinct values remain distinct. Original authority/coverage/bounds/cancellation still apply to equal intent.

## Source-Preservation Footprint

1. **Create:** one engine-owned insertion inside the existing opening tag. Add only the required attribute token and minimal XML separator; preserve all pre-existing bytes, attribute order/spelling/quotes, whitespace, namespace declarations, closing punctuation, owner body and paired closing tag. Bosak chooses and documents the exact legal insertion boundary. No caller offsets or layout mode.
2. **Update:** replace only the exact existing lexical value span inside its retained quotes. Keep name, equality spacing, quote delimiters and every outside byte. Escape the retained delimiter rather than silently changing quote style. Empty values retain the quoted attribute. No canonical rewrite on equal logical value.
3. **Delete:** remove only the existing complete attribute token from name through closing quote. Retain adjacent whitespace/trivia, unrelated attributes and original tag punctuation; extra spaces may remain. If inspection's FullRange includes trivia, expose the exact removal range separately rather than silently broadening deletion. Do not shorten a paired element or remove now-unused namespaces.

Every Changed result has one complete source edit linking its own original/candidate coordinate spaces. Exact byte replay must equal defensive ExportSource; every gap outside the declared edit is byte-identical. Create uses a zero-length original span; Delete a zero-length candidate seam; empty Update values use real zero-length value spans. Empty span is evidence, not a synthetic node. Use each source's strict byte/UTF-16 coordinate map, including BOM bias and supplementary characters.

Count original and actual final physical bytes independently, including escaping and BOM. Retain existing supported four physical variants (UTF-8 BOM/no-BOM, UTF-16 LE/BE with matching declarations) where compatible. An original over the declared bound refuses even if deletion/shrinkage would fit. Escaping expansion and exact-bound creation/update cases must be checked before publishing a result; no unchecked arithmetic or hypothetical append-only sizing.

## Actual Attribute and Descriptor Provenance

Return opaque snapshot-owned element and existing-attribute targets; numeric node IDs, names/indexes alone, caller ranges and equal-byte foreign snapshots cannot authorize edits. Attribute descriptors currently belong to element metadata rather than standalone graph nodes: **do not invent attribute child nodes, fabricated IDs or reuse text payload partitions**.

A Changed result needs complete original/candidate source/inspection, actual mapped owner element and explicit old/new addressed attribute metadata or absence according to the operation, exact name/namespace, original/new logical value when present, whole attribute/value ranges and the complete source edit. Existing attribute authority must identify the actual attribute within its actual owner, even if collection indexes/ranges shift. New/updated selection must identify the actual candidate attribute/value; deletion selects the mapped owner and clears the deleted attribute/slot.

Require complete injective correspondence for every actual graph descriptor: elements, descendants, text, comments/PIs and document/module identities remain accounted for. For attributes, explicitly partition ordinary survivors from the addressed old/new attribute:

- Create: every old attribute survives; exactly one actual candidate attribute is new.
- Update: exactly the selected old/new attribute pair has permitted logical/lexical value changes; all other attributes survive unchanged.
- Delete: exactly the selected old attribute is removed; every remaining candidate attribute has an ordinary original counterpart.

Preserve owner/descendant identity, expanded and lexical names, unedited attribute metadata/values, order, strict translated ranges and all children. No unexplained extra/missing/duplicate/foreign attribute, descriptor split/merge or partial correspondence may succeed. Attribute range/identity selection is engine-issued metadata, not Braid parsing. Emit fresh candidate ports so subsequent create/update/delete and existing operations use current identities. Returned collections/handles are immutable or defensively copied.

## Authority, Failures and Consumer Workflow

Use additive capability discovery and typed outcomes/refusals for unavailable capability, unsupported owner/attribute/profile/coverage/encoding, invalid name/value/Unicode, duplicate create, missing attribute, foreign/stale authority, bounds, denied resource, invalid original/candidate, alignment/correspondence and cooperative cancellation. Exact failure names/precedence are maintainer choices. No partial candidate grants authority; unexpected engine defects must remain distinguishable from user syntax refusals. Non-null diagnostic ranges must be owned by their declared source; omit unowned candidate offsets.

Discovery/proposal performs no transformation or ambient file/network/extension acquisition. Deny unsupported preprocessing/resource routes before execution. Observe cancellation through traversal/encoding/copy/proof and immediately before success; do not promise parsing is preemptive. Concurrent proposals must not mutate shared owners. Engine snapshots can remain valid on themselves; only Braid can assert current session/revision/license/history authority.

Braid will provide a shared lifecycle UI with explicit Create/Update/Delete intent. Changing operation, name, value, source or target invalidates held review. Show exact original/new logical values separately from lexical source, the whole affected opening tag, complete edit and source/engine/revision provenance. NoChange has feedback only, with Apply disabled and no history/redo/evidence invalidation caused by a fabricated candidate. Oversized complete review refuses rather than truncating applicable evidence or user input. Policy loss hides source-derived evidence.

Changed Apply independently rechecks exact owner/source/inspection/port/revision/license/cancellation/overlap and complete frozen evidence at the atomic source-owner boundary. Each accepted operation is one exact-byte history step; undo/redo refreshes inspection and current attribute/owner navigation. Precommit cancellation/refusal retains source and any still-current explicit retry authority; committed late cancellation reports success. Historical validation/preview/export evidence remains historical even if bytes recur.

## Planned Acceptance Matrix

These are **planned groups, not executed tests**. No new attribute-authoring tests or runtime changes are claimed by this request.

| Group | Required evidence |
|-------|-------------------|
| LA01 | Additive public capability and opaque element/attribute authority; unavailable/foreign/equal-byte-other-owner inputs refuse; legacy contracts unchanged |
| LA02 | Actual supported imported/previously authored literal result owners, self-closing/paired forms, aliases/default/rebound namespaces and explicit supported XSLT versions; unsupported owners/control/dependency contexts refuse |
| LA03 | Valid Unicode NCNames/case distinction and unqualified no-namespace semantics; empty/colon/invalid/reserved xmlns names refuse; no namespace/control/rename edits |
| LA04 | Create absent attribute; duplicate existing constant or dynamic attribute refuses with no overwrite or history |
| LA05 | Update existing constant attribute only, including empty value and entity/reference/escaped-brace spelling; absent/dynamic/unproven targets refuse |
| LA06 | Equal logical Update yields exclusive NoChange, preserves exact quote/entity/brace spelling and current source/revision/inspection/history/redo; unequal normalization forms are not silently equated |
| LA07 | Delete selected existing attribute token only; absent/dynamic/foreign targets refuse; adjacent whitespace, other attribute ordering and tag spelling survive |
| LA08 | Exact logical empty/Unicode/decomposed/markup/quotes/braces/tab/CR/LF values become intended runtime attribute values; no AVT execution or normalization; invalid XML scalars/surrogates refuse |
| LA09 | One exact edit, full unchanged-gap replay, strict zero/nonzero spans, retained quote style and no whitespace/namespace/owner/body cleanup across all operations |
| LA10 | Complete graph-descriptor correspondence plus actual per-owner attribute survivor/created/updated/removed partitions; no fabricated attribute nodes/IDs |
| LA11 | Malformed missing/extra/duplicate/foreign/reordered attribute metadata, wrong owner/name/value/range, partial maps/edit replay and swapped operation evidence refuse at engine/consumer boundary |
| LA12 | Four physical encodings, BOM/declaration matching, Unicode before/in/after opening tag, physical/reference newlines and exact byte/UTF-16 coordinates; invalid/unsupported encodings refuse |
| LA13 | Independent original/final physical bounds, escaped growth, empty spans, shrinkage, oversized originals, checked arithmetic and complete review/input bounds |
| LA14 | Original/candidate restricted coverage and validity; invalid originals are not repaired; instrumentation proves denied preprocessing/resource/extension access before side effects |
| LA15 | Cancellation at discovery/proposal/traversal/encoding/copy/proof/pre-return, exclusive outcomes, immutable exports/collections and concurrent independent proposals |
| LA16 | Create -> Update -> Delete -> Create on fresh candidate identities; imported attributes; unrelated attributes/children stay intact; existing expression/text/element/value-of operations compose unchanged |
| LA17 | Separate frozen operation review, current session/source/revision/port/license/overlap gates and independent policy loss; operation/input changes invalidate review |
| LA18 | Explicit atomic Apply per operation, exact history/redo branching and parent/attribute/value focus; NoChange creates no history; precommit retry and committed late cancellation |
| LA19 | Shared headless UI for all three operations, logical/lexical/full-opening-tag review, cancel/deferred close/queued Apply, policy hiding and oversized-evidence refusal |
| LA20 | Independent changed expected-output goldens for each operation and full lifecycle, fresh validation and real worker ExactBytes preview across four encodings; attributes containing braces/newlines verified in actual output |
| LA21 | Restricted new-file export/receipt/file hash, independent destination reopen/inspect/validate/run and retained originals/history for each operation and lifecycle |
| LA22 | Native lifecycle walkthrough, actual variants/platform/accessibility observations and immediate receipt/file/manifest hashes; no inferred unobserved acceptance |
| LA23 | One coordinated implementation PR delivering all three operations, source/compatibility/conformance/CI gates, exact-head/merge public-only local-package review, separately authorized signed public release and Braid adoption |

Source-only malformed-evidence instrumentation must stay separate from public-package consumers; do not add public mutation/friend/reflection bypasses merely to forge evidence. Upstream engine tests cannot certify Braid-only LA17-LA22. Preserve initial failures, corrections, exact revisions/toolchain, provenance and overlapping counts honestly.

## Delivery and Ownership

1. Maintainers review/choose the complete family contract, eligibility, public API/evidence and acceptance coverage for this request. Confirm exclusions and single implementation PR delivery explicitly.
2. After separate implementation authorization, implement create/update/delete together with shared proofs and focused happy/edge/refusal plus legacy tests. Complete normal upstream solution/LS, frozen conformance gates and exact-head CI without changing unrelated gate membership/assertions.
3. Supply immutable merge SHA, contract/implementation handoffs and isolated PackageReference-only candidate packages/provenance for independent Braid review. This is not a public release.
4. After separate publication approval, verify signed public packages against exact source/tag and a fresh feed-only consumer. Braid then adopts and integrates shared lifecycle review/Apply/history/navigation and real worker/export/reopen/native acceptance.

Bosak owns API design, engine semantics, implementation/tests and release decisions. Braid owns designer sessions/UI/projections and consumer fidelity checks. **This PR changes documentation only.** No engine edits, version reservation, release, live service, new native acceptance or implementation authorization is supplied.

## Consumer References

- Parent requirement and ownership: [Braid feature registry](https://github.com/Fytala-Charles/Bosak.Braid/blob/5d45734b8677033c981f4239af4c73b8f09e11d4/docs/FEATURE_REQUESTS.md).
- Existing element authoring: [Braid literal element contract](https://github.com/Fytala-Charles/Bosak.Braid/blob/5d45734b8677033c981f4239af4c73b8f09e11d4/docs/LITERAL_ELEMENT_INSERTION.md).
- NoChange and review precedent: [Braid literal text editing](https://github.com/Fytala-Charles/Bosak.Braid/blob/5d45734b8677033c981f4239af4c73b8f09e11d4/docs/LITERAL_TEXT_EDITING.md).
- Local companion design: D:/Development/Bosak.Braid/docs/LITERAL_ATTRIBUTES.md and proposed ADR-021-reviewed-literal-attribute-lifecycle.md (planning documents, not yet committed/pushed). This self-contained request does not depend on unpublished files for its criteria.
