# Bosak REQ-132 Consumer Design Review

Date: 2026-10-18. Disposition: **No blocking design findings within the proposed bounded profile. Suitable to proceed to the Bosak implementation workflow.** This is not implementation, public-package or native acceptance.

## Reviewed Immutable Material

Reviewed merge `e6f2183905739e2a303acc9ee1827004e1307041` (PR373), including the consumer handoff, proposed literal-element contract, 28 planned acceptance groups and original consumer request. All four local documents match the committed blobs after checkout newline conversion. Original request SHA-256 is exactly `632df632b35a162ff288f74dad8e8a494eedcff1e4081194b8823bf01597f600`, matching the handoff. Provenance is retained under `.acceptance/req132-design-review-20261018/provenance.json`.

Baseline stated by the handoff: public Bosak 1.19.1 at a9e954d75b3518340ce6da84c9ae06c50ab1f5a4, documentation cd5507afa2e2bfc98f20669b2941acb5b938a121. This review does not independently verify that publication or change Braid's adopted 1.19.0 pin. Four documents reviewed; 28 planned acceptance groups assessed; **zero new-operation runtime/source/package probes executed**. Last Braid full Release remains 1,079/0/0 from the previous checkpoint, not a rerun for this design.

## Decisions Assessed

| Handoff decision | Consumer conclusion |
|------------------|---------------------|
| Paired empty no-namespace element, local-name intent; local default reset, alias refusal | Fits the requested output-container slice. Namespace reset is contained in new markup and review evidence. Expanded identity, rather than labels, distinguishes literal `value-of`/`text` names from instructions. |
| Separate capability, typed target/proposal, PreserveSource default and optional Readable | Compatible with existing operation-specific ports and explicit mode/retry flow. No capability inference, silent fallback or source-splice authority is introduced. |
| Actual inserted node/parent, zero-length body, total old mapping and NewNodes partition | Sufficient to validate complete candidate provenance and restore exact instruction/body selection without a parser or fabricated child/Expression slot. Actual namespace attributes stay node metadata. |
| Fresh text/value-of targets on new nodes, including reset fixtures | Essential and explicitly covered. The empty container is useful only if the existing population operations can target it after fresh inspection/Apply. No requirement to narrow legacy behavior is accepted. |
| Engine scope/resource/name/cancellation authority and consumer session/history authority | Correct separation. Old immutable snapshots can still propose to themselves; Braid must refuse stale current-session acceptance. No engine wall-clock staleness or hard parser deadline is promised. |
| Public evidence sufficient for bounded review | Proposed LocalName/ElementName, source/edit ranges, BodyRange, InputSnapshot/Snapshot, parent, immutable exports, mapping and NewNodes support the required review/Apply checks without guessing IDs or ranges. |

## Acceptance Coverage and Implementation Review Focus

All twelve Braid LE groups are represented by E01-E28. The matrix separates engine contracts from UI/license/history/native work, exact source bytes from output expanded-name semantics, and implementation/package/release/native delivery gates. Counts are planned groups, not passed tests.

No contract correction is required before implementation. The following are explicit existing requirements to verify at that later review, not missing-design findings:

- Default XSLT namespace and ordinary default namespace fixtures must both create a no-namespace literal child and allow subsequent text/value-of proposals through fresh actual targets. Source inspection alone cannot establish runtime namespace semantics; the changed-output fixtures must assert them independently.
- Readable edits may shrink original whitespace. Enforce original/final physical byte budgets independently, exact replay/unchanged gaps, overflow-safe repeated-name encoding and the complete actual descriptor partition. Refuse impossible text merges/splits rather than synthesizing nodes.
- Literal names must remain exact, including legal Unicode and otherwise legal `xml`/`xmlns` spellings; no blanket consumer blacklist, trim or normalization. Engine parser/profile refusals remain explicit.
- PreserveSource and existing text/value-of/deletion behavior must retain their measured compatibility. Namespace-alias, preprocessing, resource and mixed-content refusals must occur without execution or unintended acquisition.
- Proposed public evidence must remain deeply immutable and accessible without friend/internal APIs. The review UI must refuse an oversized complete footprint rather than truncate applicable evidence.

The namespace-reset decision is consistent with [Namespaces in XML section 6.2](https://www.w3.org/TR/REC-xml-names/#defaulting). The XSLT 3.0 link in the contract was consulted but the browsing tool could not retrieve the oversized document; this review does not claim a complete independent XSLT standards audit.

## Scope and Next Gate

This review supports the bounded design: append one paired empty no-namespace literal result element, no user attributes or content, explicit eligible parent end tag, separately disclosed readable layout and later independent population commands. Broader namespace/attribute/move/wrap/self-closing-parent operations remain excluded.

Bosak retains implementation, API/version and release authority. Next: authorized engine implementation and regressions, exact-head/merge CI and independent public-API package evidence, then published release verification and separate Braid adapter/review/Apply/history/selection/UI plus worker/export/reopen/native acceptance. Neither this report nor the 28 planned groups establish those gates. No engine or upstream documentation changes, commit, push or external notification were performed by this review.

## References

- [Braid consumer scope](LITERAL_ELEMENT_INSERTION.md)
- [Proposed ADR-018](ADR-018-bounded-literal-element-insertion.md)
- Upstream handoff: D:/Development/Bosak/docs/REQ-132-CONSUMER-DESIGN-HANDOFF.md
- Upstream contract: D:/Development/Bosak/docs/REQ-132-LITERAL-ELEMENT-CONTRACT.md
- Upstream matrix: D:/Development/Bosak/docs/REQ-132-LITERAL-ELEMENT-ACCEPTANCE.md
