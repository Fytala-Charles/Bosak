# Bosak REQ-134 Consumer Design Review

Date: 2026-10-19. Disposition: **No blocking design findings within the proposed bounded literal-text instruction deletion profile.** No required contract correction identified. This is design review, not engine implementation authorization, release approval or native acceptance.

## Immutable Material and Evidence

Reviewed merged design **bdefbfbfcc2f3654708e8cfd200416377bd623e3** (PR382), including final design head 31187718. All six REQ-134 documents match committed content after checkout newline conversion: handoff, contract, 28 planned acceptance groups, original inquiry, issue provenance and baseline JSON. Design base ecac021c is context, not the reviewed design commit. Local copies and SHA-256 receipts are retained under `.acceptance/req134-design-review-20261019`.

Independently executed **20 public baseline checks: 20 passed, zero failed/skipped**. Sixteen cover self-closing, paired empty, paired Unicode/reference and paired whitespace text across UTF-8 BOM/no-BOM and UTF-16 LE/BE. Actual payload cardinality/logical values, existing value-of deletion refusal, runtime output and original-byte retention agree. Four independently authored split/joined physical/reference newline fixtures confirm why joined-run parsing belongs to Bosak: physical CR/LF may normalize differently after adjacency; reference spellings retain their own semantics. These fixtures are not emitted deletion candidates.

The PackageReference-only harness uses cached public Bosak 1.21.0 packages and asserts the exact XSLT assembly informational revision 8929a1301d1846bb24b49bcc505877f05fd3395a. No engine ProjectReference, internal/friend access or reflection into engine internals. No fresh package provenance/signature verification is claimed. The first build stopped on missing XML documentation in the probe harness; comments were added before the successful run. No engine or product fix was required.

**New text-deletion tests executed: zero.** The 28 upstream groups are plans, not passing tests. Bosak's ten baseline cases are reported upstream evidence, not additional independently executed checks. No full Braid/upstream solution, language-server, QT3 or qt4 gate rerun. Last Braid full Release 1,242/0/0 and public-package 460/0/0 remain prior integration evidence.

## Assessment of the Eight Proposed Choices

| Choice | Consumer conclusion |
|--------|---------------------|
| Separate capability, owned target/proposal and Candidate/Failure | Fits operation-specific authority. Deleting empty/self-closing text still removes source and needs review/history; no NoChange outcome is appropriate. |
| Paired empty/nonempty and self-closing syntax | Accept self-closing inclusion and null body span; paired empty retains its actual empty span. Ordinary syntax proof cannot rely on Kind=Text alone. Excluded body/control forms must refuse. |
| Conservative parent and xml:space profile | Accept initial refusal of significant direct parent text, direct CDATA and any surviving preserving ancestor, including nearer default resets. Local metadata removed with the instruction does not imply parent preservation. Replacement's broader accepted contexts remain unchanged. |
| One full removal and zero-byte seam | Sufficient exact replay; no formatting/cleanup or output-equivalence requirement. Seam coordinates are evidence, not a mandate to highlight a caret inside a newly joined CRLF. Select actual surviving parent. |
| One/two-node removal and complete partitions | Required. Removed instruction plus actual payload are distinct from ordinary survivors and merge originals. Existing value-of's one-node assumptions must not be reused. |
| Zero/one two-fragment whitespace merge | Existing REQ-130 provenance model fits the bounded scope. Braid validates bytes, ownership and partitions; Bosak owns actual candidate parsed text, including physical/reference newline semantics. |
| Bounds, encodings, validation and cancellation | Independent original/final physical limits and captured original/candidate coverage are sufficient. Accept new typed Cancelled refusals; retain legacy throwing behavior unchanged. |
| Independent adapter and staged delivery | Separate frozen review, explicit atomic Apply, exact history and parent selection fit Braid. Source/package/public delivery, consumer integration and native acceptance remain independent gates. |

## Consumer Obligations and Next Gate

The [dedicated operation design](LITERAL_TEXT_DELETION.md) and [proposed ADR-020](ADR-020-reviewed-literal-text-deletion.md) define consumer boundaries. Validate complete removed/map/merge partitions; deep-freeze every retained collection and source edit. Review must show the whole removed instruction, logical/lexical payload and complete merge/provenance evidence or refuse oversized evidence without truncation. Policy/source changes invalidate authority and hide source-derived review where required.

Apply must independently recheck current session, inspection, source revision, issuing port, permissions, cancellation and overlap at the source-owner boundary. Restore only the actual mapped parent. Test stale/foreign/malformed evidence, policy loss, precommit retry and committed late cancellation. Exact undo/redo must refresh inspection and leave old validation/preview/export evidence historical.

Next gate: separate upstream implementation authorization, then engine/compatibility/conformance/CI and exact-merge package review. Public release verification precedes Braid integration and real worker/export/reopen/native acceptance. No Braid runtime/pin change, upstream edit/message, commit or push is supplied by this review.
