<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala bounded value-of deletion implementation">
<h1>REQ-130: bounded value-of deletion</h1>
<p>Source implementation — 17 October 2026</p>
</div>

**REQ-130 accepted ? 17 October 2026:** Braid formally accepts bounded value-of deletion in published **1.18.0**, source `3264fcfaf739e492770c882d2cbfc1f93f0172d2`. The [unchanged consumer acceptance receipt](REQ-130-BRAID-ACCEPTANCE.md) reports 1,015 normal Release tests and 241 fresh public-package checks (overlapping evidence), exact nine-assembly provenance, and product-owner native completion with matching hashes. These are reported consumer results, not engine reruns. Exact literal hashes/native encoding variants were not supplied. Acceptance is limited to the delivered profile; REQ-131 is separate. Earlier pending/adoption checkpoints below are historical.

## Delivery state

Published version **1.18.0** at immutable tag `v1.18.0`, source 3264fcfa. Branch `feat/req130-value-of-deletion`, based on reviewed design merge `5c8513d6cbad3ea9f2f6c5d55918d3a730fff070`. User requested continuation after Braid reported no blocking design findings. Historical source delivery checkpoint: engine verification at code head `10a3b5e3ef59074ba803b63e13d29a7d4eaae989` passes; no Braid pin/UI/source changes or native acceptance is claimed. Public libraries are 1.18.0; Braid production remains 1.17.0; extension 0.2.5 still bundles 1.16.5.

## Implemented boundary

One empty-body XSLT value-of with a supported select, directly in a principal top-level template or its literal-result-only descendant constructor chain. Expanded names identify instructions regardless of prefix/default binding. Imported instructions and previously inserted candidates are supported. Self-closing or explicit empty spellings work; instruction-body text, whitespace, CDATA, comments, PIs or elements refuse. All original bytes outside the exact instruction extent remain intact, including blank indentation lines and explicit now-empty parent tags.

Both original and candidate are bounded to 8,388,608 bytes including BOM. Strict UTF-8 BOM/no-BOM and BOM-marked UTF-16 LE/BE retain existing declaration rules. Reuse source-derived scope exclusions and restricted validation from the insertion pipeline through internal helpers; existing insertion behavior/enum values remain unchanged. Single-module/static/resource/type/expression/profile limits remain in force. Significant mixed text/CDATA and any preserving ancestor, including nearer xml:space=default reset, refuse. Neither original nor candidate evaluation occurs during deletion.

## Public API and ownership

`AuthoringCapabilities.SupportsValueOfDeletion` independently advertises this operation. `MaxValueOfDeletionSourceBytes` gives its bound. Obtain `snapshot.GetValueOfDeletionTarget(node, token)` and pass its opaque target to `new AuthoringValueOfDeletionProposal(target)`, then `snapshot.ProposeValueOfDeletion(proposal, token)`. Both discovery/proposal return an exclusive success or `AuthoringDeletionFailure` carrying kind/message/optional error code/module URI/optional range. Null programmer arguments throw; observed cancellation throws OperationCanceledException, unlike the older insertion operation's classified cancellation. Immutable older snapshots remain valid on their own; the consumer enforces current session revision.

The candidate has InputSnapshot, fresh compiled-valid Snapshot, ModuleUri, RemovedNode/RemovedNodes, SurvivingParent, OriginalRange, zero-length CandidateRange, one immutable SourceEdits removal and defensive ExportSource(). SourceEdits reuses the existing immutable AuthoringInsertionSourceEdit range carrier: OriginalRange is removed; ReplacementRange is zero bytes in the candidate. No caller replacement or formatting mode exists.

**Deletion NodeCorrespondence maps actual original descriptors to actual candidate descriptors**, not integer IDs. Both are scoped to their respective snapshots. It is total and injective outside the removed instruction and merge participants. A candidate can reuse an old numeric ID without reusing identity. The surviving parent is an actual mapped candidate element; selection never uses a nearby sibling heuristic.

TextMergeGroups is empty or contains exactly one AuthoringTextMergeGroup for the selected instruction's two immediately neighboring ordinary nonempty whitespace text siblings. Its actual CandidateNode and two immutable AuthoringTextMergeFragments record full OriginalNode/OriginalRange and exact ordered contiguous CandidateRanges. These fragments cover the entire ordinary inspected candidate text node; they do not create new descriptor identities. Original graph partitions into removed, mapped and merge participants; candidate graph partitions into map image and merge destinations. Recursive range/name/attribute/text/child-order proof rejects missing/extra/altered/ambiguous evidence.

Original source bytes are spliced once, then ordinary inspection creates the candidate. XML parsed text is engine-owned: physical CR + LF across the removal seam normalizes as a joined CRLF, while numeric whitespace references retain their distinct semantics. Braid must verify opaque ownership, complete graph partitions and byte replay without adding a parallel parser/normalizer. A plain dictionary-only adapter cannot consume this operation safely.

## Verification evidence and limits

Initial focused run: 32 passed / 3 failed. One fixture expected serializer output `<out />` instead of actual `<out/>`; two fixtures requested readable insertion from an ambiguous source indentation layout. Corrected expected output and used an eligible readable source layout; no engine fix was needed for these three failures. Original log/TRX retained in `.guard-tmp/req130-focused-results`. Corrected first 35 deletion fixtures passed. Supplemented 45 deletion plus 177 existing insertion/readable tests passed (222 total) before four additional internal malformed-candidate proof controls and the final dependency-classification guard. These earlier results are not claimed as final-head validation.

The final actual API cases cover imported first/middle/last instructions, compact/indented source, prior preserve/readable insertion, ordinary compilation options, aliases/default XSLT namespace, exact source replay, graph partitions and fragment coverage, four encodings and four lexical whitespace seams, explicit parent retention, metadata/astral Unicode/namespace provenance, exact/over-limit originals, foreign ownership, profile/body/mixed/preserved/resource refusals, cancellation/nulls/concurrency and changed-output goldens. Internal proof controls exercise changed/missing/extra candidate graphs and remain source-only tests; independent package consumers use only public APIs.

Final build, focused/full tests, language-server suite, frozen QT3/qt4 results, exact-head CI and independent package-only proof are recorded below once complete. No D01–D27 consumer/native gate is inferred from engine tests. Braid still needs compatible port/review/Apply/history/selection integration, malformed consumer-evidence controls, worker/export/independent reopen goldens and native owner acceptance after delivery. Preserve [Braid's design-only review](REQ-130-CONSUMER-DESIGN-REVIEW.md) and the [original request](REQ-130-source-preserving-value-of-deletion.md) unchanged.

An added internal malformed-candidate control exposed a range-bound exception when the supplied candidate was shorter than the translated root extent. The proof now explicitly rejects translated endpoints outside candidate text before constructing coordinates. This is a robustness correction in the engine proof (distinct from the earlier fixture corrections); original failed full-run receipt is retained in `.guard-tmp/req130-final-solution-results`. Corrected-source focused 226 and exact-head CI full 9,661 solution plus 84 language-server tests pass. The earlier failed run remains preserved.

## Verified code-head delivery gates

Code revision `10a3b5e3ef59074ba803b63e13d29a7d4eaae989`, [implementation PR #361](https://github.com/Fytala-Charles/Bosak/pull/361). Subsequent documentation-only updates retain the same source/test trees; final-head CI/package proof and integrated revision are retained in the delivery receipt.

| Gate | Observed evidence |
|---|---|
| Release build | Successful, zero errors; existing unrelated documentation/test warnings retained. |
| Focused source | **226 passed**, zero failed/skipped: 49 deletion cases (including four internal proof controls) plus 177 insertion/readable cases. |
| Exact code-head CI | Run **39028847729** succeeds: **9,661 solution +84 language-server tests**, zero failed/skipped; highlighting green. |
| QT3 frozen | **31,142 /0 /679**, matching baseline. |
| qt4 gated | **8,278 /0 /1,894**, matching baseline, zero gated/exploratory failures. |
| Independent local-package consumer | **221 passed**, zero failed/skipped: 45 public deletion +175 public insertion +one assembly-identity case. No project references or friend-assembly identity. |
| Package provenance | Ten local 1.18.0 archives, nine consumed assemblies; exact repository/informational revision and archive/cache/output bytes, SHA-512 restore hashes and local source receipts agree. Dependency audit reports zero vulnerable dependencies. |
| Documentation and retained evidence | Nine changed pages render using unchanged canonical CSS; original consumer dossiers/review remain byte-identical Git blobs. |

Four new proof controls and two older insertion traversal controls stay in source tests because they use internals. They are not relabeled as public package cases. Receipts are in `.guard-tmp/req130-*`, including `req130-head-package-provenance.json`, `req130-corrected-focused-results`, `req130-corrected-qt3.log`, `req130-corrected-qt4.log` and `req130-head-ci.log`. Preserve every original failure and corrected receipt separately.

These engine gates do not implement Braid's review/Apply/history/worker/export/reopen workflows or native acceptance. Source version 1.18.0 is published; Braid remains on accepted public 1.17.0. Release and consumer adoption follow their own review and authorization.

## Independent implementation review and release preparation

Braid [reports no blocking implementation findings](REQ-130-CONSUMER-IMPLEMENTATION-REVIEW.md) at the exact reviewed head and integrated revision. Its 226 source,221 unchanged package and 241 combined package/probe counts overlap;241 includes 20 independent cases. These use local unpublished artifacts, not public NuGet. Its normal runtime baseline remains 952, and restricted worker/export/Apply/history/native gates remain open. The receipt is preserved verbatim, including warning overrides and initial fixture/helper corrections. [Release readiness](RELEASE-1.18.0-READINESS.md) and prepared notes target 3264fcfa; publication requires separate authorization. No engine change is needed from this review.
