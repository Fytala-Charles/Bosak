# Bosak REQ-134 Consumer Implementation Review

## Final Exact-Merge Review - 2026-10-19

Disposition: **No blocking implementation findings within the declared bounded profile; exact-merge delivery review complete.** PR383 is merged at **3b00938875490d1997b09f53b80f2ee2986a666b**. Its complete tree is identical to reviewed head e0eba6de7c30bd24f31150c87767329ef45afdea, not merely its src/tests subtrees. Live GitHub merge run39289205795 and both merge checks report success at this exact SHA.

Independently reran the same public consumer suite against the merge's unsigned local1.22.0 packages with a new isolated cache: **551 passed, zero failed/skipped** (531 imported checks +20 new Braid probes). This rerun overlaps the prior head coverage and is not 551 additional unique checks. All ten local archive nuspec revisions and all nine consumed assembly informational revisions match the merge. Independently verified archive/cache/output DLL bytes, cached archive bytes, SHA-512 project.assets values and Bosak-exclusive merged-feed restore receipts. Complete source/package/TRX provenance is retained in `.acceptance/req134-implementation-review-3b009388`; the original head receipts remain separate and unchanged.

No source-only/full-solution/language-server/conformance/broad153 consumer rerun is claimed. No blocking change was found between reviewed head and immutable delivery. The supplied merge receipt and delivery JSON are retained as snapshots. These results close the previously reserved exact-merge review gate; they do not authorize publication or certify signed public packages, Braid integration/UI/history/worker/export/native behavior. Braid's production dependency remains published1.21.0.

Next: Bosak's separately authorized publication, then independent signed public-package verification, Braid text-deletion adapter/session/UI/history implementation and workflow/native acceptance. No engine edits, upstream messages, commit or push were performed. Documentation and whitespace checks pass.

## Historical PR-Head Review

The following records the earlier open-PR review and its then-pending merge gate; that gate is now closed by the evidence above.

Date: 2026-10-19. Disposition: **No blocking implementation finding at the reviewed PR head within the declared bounded profile. Final merge-delivery review remains pending.** [PR383](https://github.com/Fytala-Charles/Bosak/pull/383) is OPEN, not merged. Its exact head is **e0eba6de7c30bd24f31150c87767329ef45afdea**. Source version 1.22.0 is an unsigned local candidate; Braid remains on published 1.21.0.

## Revision and Delivery Evidence

Reviewed the operation-specific public types, target/proposal entry points, complete TextDeletionPipeline, shared validation/pattern refusal-filter changes and source tests against the [reviewed design](BOSAK_REQ134_CONSUMER_DESIGN_REVIEW.md). Initial implementation 16c71179ecc1b79bfa2c36fcd884437128e71440 and final head have identical src/tests trees. Five handoff/implementation/contract/acceptance/verification documents match the exact final head after checkout newline conversion.

Both exact-head CI checks pass: Build & test (net10.0) and VS Code highlighting, run39282961861. The delivery receipt still reports FINAL_HEAD_VERIFIED_CI_QUEUED and merged=false; live CI supersedes its queued status, but does not supply a merge. The handoff requests exact-merge packages in req134-merged-packages; that directory and merge proof were unavailable during review. Final-head proof cannot substitute for final merge proof. No merge was performed by this review.

## Independent Execution and Package Provenance

Fresh isolated PackageReference-only consumer: **551 passed, zero failed/skipped**, consisting of **531 imported public checks plus 20 new Braid probes**. Imported coverage is 94 new-operation checks plus 437 legacy/identity checks; overlapping upstream runs are not additive unique coverage. Source-only internal controls are excluded. No engine ProjectReference, friend assembly identity or access to engine internals is used. Reflection in the inherited metadata check reads public assembly identity only.

The 20 Braid probes cover all three instruction spellings across four encodings (12), repeated fresh-snapshot deletion across four encodings (4), identical-byte foreign ownership, new typed cancellation, conservative mixed/preserved/body refusals and separation from legacy value-of deletion (4). Checks verify whole-instruction replay, actual one/two-node removal, complete disjoint partitions, mapped parent, actual whitespace merge destination/fragments, physical CR/LF seam behavior, defensive exports, original retention, fresh ordinary reinspection and direct real-engine output. Empty/self-closing deletions change source even when output remains unchanged. Repeated deletion retains explicit paired parent tags.

All **ten local candidate archive nuspec revisions** match e0eba6de. All **nine consumed assembly informational revisions**, archive/cache/output DLL bytes, cached archive bytes, SHA-512 assets and mapped candidate restore sources match. Restore used a new isolated cache, with Bosak packages mapped exclusively to the final-head local feed. This is not signed public-package verification. Evidence, retained handoff snapshots, harness, TRX and verified-provenance.json: `.acceptance/req134-implementation-review-e0eba6de`.

No independent full upstream/Braid solution, language-server, source-only controls, broad 153-check consumer, QT3 or qt4 run is claimed. Upstream reports 10,010 solution,84 LS,110 new source (94 public/16 internal),153 broad compatibility and frozen conformance gates; these remain reported upstream evidence. Previous Braid 1,242 Release/460 public-package passes and 20 design baseline checks remain historical separate evidence. No new consumer adapter/UI/history/worker/restricted-export/native acceptance is inferred from these engine package checks.

## Contract Assessment

| Area | Review result |
|------|---------------|
| Separate authority and outcomes | Separate capability, opaque target/proposal and exclusive Candidate/Failure; no NoChange. Null guards and typed cancellation preserve existing API defaults. |
| Syntax and payload | Paired empty/nonempty and self-closing supported; self-closing body span null. Ordinary syntax proof excludes CDATA and body markup. Actual payload removal is explicit. |
| Fidelity and coordinates | Exact one-instruction source removal, sole empty replacement seam, unchanged outside bytes and independent source coordinate maps. No cleanup or synthetic result node. |
| Graph proof | Recursive source/metadata correspondence and explicit original/candidate uniqueness/partition checks; removed payload excluded from map/merge. Actual mapped parent returned. |
| Whitespace seam | At most one two-fragment ordinary whitespace merge with full contiguous lexical byte coverage. Fresh engine inspection owns parsed joined-run value. |
| Scope and validation | Conservative parent/ancestor-preserve rules, controls, original/candidate validation, physical original bound, encoding and static/resource exclusions. Legacy callers retain default exception handling; stricter filter is passed only for the new operation. |
| Ownership and immutability | Targets remain snapshot-owned, exports defensive and collections read-only. Old owner remains valid on itself; application revision/currentness remains Braid's responsibility. |

No required source correction identified in these boundaries. Review does not claim unrestricted text deletion, broader mixed-content support or production readiness.

## Next Gates

1. Bosak merges the verified head and supplies immutable merge SHA plus exact-merge local package/provenance proof. Recheck source/test equality, merged CI and package identities; rerun the public consumer checks against that merge before completing the requested delivery review.
2. Separate publication authorization and signed public 1.22.0 verification.
3. Braid's [dedicated operation design](LITERAL_TEXT_DELETION.md) and [proposed ADR-020](ADR-020-reviewed-literal-text-deletion.md) guide adapter/session/evidence validation, complete review, explicit atomic Apply, exact parent navigation/history and real worker/export/reopen/native acceptance.

No engine edits, Braid runtime/pin/UI changes, upstream messages, commit or push were made by this review.
