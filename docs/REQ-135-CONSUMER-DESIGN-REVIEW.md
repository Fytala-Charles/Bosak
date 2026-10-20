# Bosak REQ-135 Consumer Design Review

Date: 2026-10-20. Disposition: **No blocking design findings at requested head c01537066cb991b1eea2b1e51a046215a70ed46f. No required contract correction identified within the declared profile.** Create, Update and Delete remain one cohesive feature and one implementation PR. The review covers design readiness, not implemented attribute functionality or release acceptance.

## Immutable Material and Independent Evidence

Reviewed [PR391](https://github.com/Fytala-Charles/Bosak/pull/391) at exact head **c01537066cb991b1eea2b1e51a046215a70ed46f**, including the contract,23 planned acceptance groups, consumer handoff, unchanged request and source audit JSON from that commit. The current upstream working tree lacks these design files, so review used immutable Git blobs, not moving working files/main. All five snapshots and SHA-256 receipts are retained under `.acceptance/req135-design-review-c0153706`.

Original consumer request blob is unchanged from794ac98dab8921b16da0a2351a9f86afd5a868ba and matches SHA-256176b583319659acd60b1e7c7188b9424ea3c5fcd2d2b19ac5ed71a5aee7a103e. All eight audited source files match the recorded engine base fbfb7ea4764a6cb9c6d3a368053a4234daac6d1d and are unchanged at the design head. Three exact-head CI checks pass: Build & test, XSLT wrapper controls (Windows), and VS Code highlighting, run39447152358. Their full-suite evidence is upstream CI, not an independently executed Braid full run.

Independently executed **24 existing-public inspection/runtime baseline checks:24 passed, zero failed/skipped** on published Bosak1.22.0/3b00938875490d1997b09f53b80f2ee2986a666b. Six lexical cases across four encodings cover empty attributes, doubled braces, numeric-reference-introduced dynamic braces, numeric-reference doubled constant braces, referenced tab/CR/LF and quotes/ampersand/markup/supplementary/decomposed Unicode. Actual runtime attribute values, original source bytes, attribute metadata and sibling attributes agree with the design assumptions.

The baseline confirms that ExpandedValue can retain doubled opening/closing braces around `name`, while the runtime literal value contains single braces (`{name}`). It also confirms a raw attribute with numeric-reference introduced braces can be classified Plain yet execute an AVT; therefore SlotKind/raw brace heuristics must not grant constant-attribute authority. This is an existing-inspection observation, not a defect finding against the proposed design, which explicitly requires proper constancy proof.

The public-only PackageReference harness reuses the previously verified signed1.22.0 cache; all nine output engine assembly bytes match the retained signed-public provenance and the XSLT informational revision is asserted. No engine ProjectReference, internal/friend access or reflection into engine internals. Public assembly metadata inspection is used only for identity. No fresh package/signature proof or new authoring operation is claimed. Initial24 failures were a copied harness project still referencing1.21.0; its exact-version assertion exposed that setup error. Only the project reference version was corrected, original failed TRX retained, then24/0/0 passed on1.22.0. No engine/product change was required.

**New attribute-authoring tests executed: zero.** The23 groups remain plans. No full source/Braid/LS, conformance or attribute candidate tests were independently run. Prior1,316 Braid Release/551 public-package results remain historical separate evidence.

## Assessment of the Nine Handoff Choices

| Choice | Consumer conclusion |
|--------|---------------------|
| One lifecycle capability and distinct operation authority/results | Fits the requested coordinated delivery. Create requires absence, Update/Delete require actual owned existing attributes, and only Update exposes NoChange. Shared candidate Operation must be verified, not used to erase target intent. |
| Conservative no-namespace owner/attribute/module profile | Accept qualified-owner/attribute and module-wide dynamic-result-AVT exclusions initially. Ordinary mixed/preserved children may remain where full unchanged graph proof holds; do not import text-deletion merge exclusions. Broader CDATA/expanded-text coverage is not required. |
| Engine constant logical values after XML and AVT interpretation | Required. Numeric references and escaped braces demonstrate why ExpandedValue/SlotKind/raw spelling are insufficient. Input remains exact literal Unicode, including logical whitespace, quotes and braces. |
| Create first / Update quoted value / Delete token-only | Accept insertion of one space plus double-quoted token immediately after owner lexical name, before existing trivia. New attribute first preserves all original relative ordering/bytes. Update retains quote/equality/name; Delete retains all adjacent whitespace. No formatting/namespace cleanup. |
| Logical-equal Update NoChange | Fits source/history preservation. Check original validity/coverage/physical bounds first; do not reject equal intent based on hypothetical canonical escaped growth. Empty value and absence stay distinct. |
| Total graph bijection and per-owner attribute partitions | Sufficient and necessary. Actual descriptor/attribute references, full ordinary survivor maps and explicit addressed pair/absence avoid fake nodes/IDs or name/index guessing. Addressed Update may reclassify SlotKind; unchanged survivors may not. |
| Actual resulting selection and full review context | Fits Braid: actual new/updated attribute/value, including empty span, or mapped owner after Delete; no fabricated seam/attribute port. Whole opening-tag evidence must remain complete and bounded. |
| Bounds/encodings/resource/cancellation/diagnostics | Existing8,388,608 physical limits, checked counting before allocation, four strict encoding variants and explicit typed new cancellation fit the consumer. Legacy behavior remains unchanged. Failure ranges must belong to input coordinates. |
| Independent consumer authority/Apply/history/workflows | Fits current session/source/port/revision/license/overlap gates and atomic source owner. Full runtime/export/reopen/native acceptance follows implementation/public delivery; upstream unit tests cannot certify it. |

## Scoped Implementation Approval

Following the requested design review and authorization gate, **Braid's consumer approval is given for Bosak to prepare one implementation PR containing Create, Update and Delete together, strictly within this reviewed contract/profile and LA01-LA23 gates**. This is a separately recorded scoped approval, not an inference that any no-blocking review automatically authorizes code. Bosak maintainers retain implementation/API/release ownership; no engine edits are performed by this review.

Do not expand to qualified attributes/owners, dynamic AVT editing, rename, namespace/control edits, generic upsert or arbitrary source editing without a separate scope review. Do not advertise complete lifecycle capability with only a subset implemented. Select the implementation version from the then-current source according to Bosak policy; no version is reserved by Braid.

Before delivery, require focused happy/edge/refusal/legacy tests, complete graph/attribute proof and malformed source controls, full solution/LS, frozen conformance gates, exact-head CI and immutable merge PackageReference-only proofs. Source-only instrumentation must remain separate from public consumers. Braid will independently review implementation/evidence against exact head/merge. **Publication/release authorization, signed public verification, Braid integration and native acceptance remain separate and are not approved here.**

## Consumer Obligations

The [Braid design](LITERAL_ATTRIBUTES.md) and [ADR-021](ADR-021-reviewed-literal-attribute-lifecycle.md) retain distinct operation authority and complete frozen evidence. Consumer validation must account for every ordinary attribute and actual addressed create/update/remove pair across owners, including namespace/control metadata that survives. Do not require addressed Update SlotKind equality or XML-expanded/final-logical equality. Use source-specific coordinates and well-tested insertion boundary affinity; never subtract byte counts from columns or parse AVTs in Braid.

Shared UI must invalidate held review on operation/name/value/owner/source changes, distinguish empty from absent, hide source-derived data when denied and refuse oversized complete review/input without truncation. NoChange must preserve source/revision/inspection/history/redo and existing evidence under current independent authority. Changed Apply creates one exact reversible step with actual attribute/value or parent navigation.

End-to-end acceptance must include all three operations and full lifecycle, literal brace/newline actual output, fresh worker ExactBytes goldens across four encodings, restricted export/receipt/hash, independent destination reopen and observed native variants. A metadata baseline does not substitute for any of those gates. No runtime/pin change, upstream message, commit or push was made by this review.
