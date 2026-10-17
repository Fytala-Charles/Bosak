# Bosak REQ-130 Consumer Design Review

Date: 2026-10-17. PR: [#360](https://github.com/Fytala-Charles/Bosak/pull/360), draft/open. Exact reviewed head: `c6db469c0dcc2eae0d290d1aaf0c33d22e02a88b`. Base: `731c94431a954a86cf771a6dbffee37eca92fa5a`. Scope: proposed deletion contract/acceptance matrix, not engine implementation or consumer runtime adoption.

## Verdict

**No blocking design finding at the reviewed head.** The bounded explicit merged-text provenance is a necessary and suitable refinement of the original injective-only consumer request. Recommend proceeding from schema review to a separately authorized engine implementation, preserving the contract's full delivery gates. This report is not a GitHub approval, merge authorization, implementation authorization or REQ-130 functional acceptance.

The PR changes eight documentation files only. It adds no engine source/API/capability, deletion tests or release. GitHub reports successful Build & test (net10.0) and VS Code highlighting checks for this head; those existing-suite results do not establish implemented deletion behavior. REQ-130 remains Pending / TBD / Unassigned. Braid continues to pin published 1.17.0 at 15eacde48b47cc8e48af29ddc95949801a2e3e5d.

## Reviewed Contract

The proposed operation independently advertises deletion, owns an opaque snapshot target, accepts only an empty-body existing xsl:value-of in the declared principal/template/literal-result profile, removes exactly its source extent, validates original/candidate with existing static/resource/encoding exclusions, and returns an isolated fresh candidate. Adjacent whitespace, comments/PI, namespaces and explicit parent tags remain unchanged. No cleanup, arbitrary subtree deletion, dependency mutation or existing insertion/expression API change is introduced.

The selected element is the sole removed descriptor. One-to-one survivors retain an injective NodeCorrespondence, while at most one separate TextMergeGroup accounts for exactly the two adjacent surviving ordinary whitespace text descriptors becoming one actual candidate text node. Complete disjoint original/candidate graph partitions, actual snapshot-owned descriptors, parent/child ordering, exact ordered contiguous full-source fragments and unchanged-byte replay prevent hidden removal, missing survivors, guessed identity or unexplained candidate nodes. The surviving parent remains a proven current element, including after sole-child removal.

The scope is deliberately narrow: merge participants are immediate neighboring nonempty ordinary whitespace nodes in the selected instruction's parent; significant text, CDATA, other parents, larger/arbitrary many-to-one changes and unsupported evidence refuse. The whole-module candidate is validated independently. Preserve-only removal can leave a blank indentation line; this matches the requested source footprint.

## Independent Public-API Design Probes

Using published Bosak.Xslt 1.17.0 from the existing public-only package cache, an isolated project inspected separately authored before/after fixture pairs. It does not implement deletion or construct an engine deletion candidate. Result: **6 passed, zero failed/skipped**.

Four encoding tests each check four whitespace seams (16 combinations total): ordinary indentation, physical CR before the removed instruction plus LF after it, a numeric CR reference plus physical LF, and numeric CR/LF references. Encodings are UTF-8 BOM/no-BOM and UTF-16 LE/BE. They verify actual descriptor counts/kinds, exact retained source fragments, original/candidate byte reconstruction and joined parsed text. Two controls verify a separating comment prevents merging and equal numeric IDs across snapshots can identify different elements.

The physical CR + LF seam parses to one LF after the element is absent; concatenating the two original parsed Text values would yield two LFs and is wrong. Numeric character references have different XML normalization behavior: a referenced CR followed by LF remains parsed CR/LF. The draft correctly requires joined lexical source and actual engine inspection rather than concatenating original parsed values or normalizing retained bytes.

Evidence: `.acceptance/req130-pr360-c6db469c/results/shape-probes.trx`, fixture source/project and exact PR document copies in the same ignored directory. A probe build initially called a nonexistent AuthoringSource byte-export helper; it was corrected to the public AuthoringSnapshot.ExportOriginal API. No engine fix was required. These six passing inspection-shape probes are **not passing REQ-130 deletion tests**. The last complete normal Braid baseline remains 952/0/0 from the preceding audit; no full runtime suite was rerun for this documentation/design review.

## Required Consumer Adaptation

The original [deletion plan](VALUE_OF_DELETION.md) and [proposed ADR-016](ADR-016-reviewed-deletion-and-survivor-selection.md) describe total injective survivor correspondence. Before implementation/adoption, the live consumer design must explicitly add the proposed bounded merge relation; the original upstream request remains historical evidence unchanged.

A compatible Braid adapter/Application review must freeze both NodeCorrespondence and TextMergeGroups, check the complete disjoint original/candidate partitions, exact parent/ordering and contiguous fragment byte coverage, then apply only a session-issued current/licensed reviewed candidate. It must never flatten two originals into a dictionary by selecting one participant, treat the other whitespace node as deleted, invent candidate descriptors or select by a numeric ID reused from another snapshot. Deletion's default current selection is the proven SurvivingParent; unprovable/unsupported selection clears navigation.

Engine owns XML parsing, character-reference/line-end semantics and the correctness of the fresh inspected Text value. Consumer fidelity checks verify opaque engine descriptors, ownership, partitions and byte replay; Braid must not add a parallel XML normalization/parser to implement this contract. Keep that boundary explicit in the eventual API documentation and consumer tests.

## Implementation Review Gates

- Prove all planned D01-D27 groups against actual deletion APIs, including positive ordinary indented/readable merge cases and malformed/foreign/duplicate/reordered/gapped/partial merge evidence refusals.
- Verify physical CR/LF joining and whitespace references across encodings, actual full-range coordinates and complete original/candidate graph partitions. Preserve exact source bytes and declared no-cleanup scope.
- Preserve insertion/expression/default APIs; run full engine/language-server/frozen gates plus exact-head and integrated public-only package proof. Passing documentation CI alone is insufficient.
- Independently review the implementation and published artifacts before Braid adopts it. Then implement deletion-specific capability/port/frozen review/Apply/history/survivor selection, changed-output worker/export/reopen goldens and native acceptance.

No upstream comment/review was posted, no engine source or dependency pin changed, and no commit/push was performed by this review.
