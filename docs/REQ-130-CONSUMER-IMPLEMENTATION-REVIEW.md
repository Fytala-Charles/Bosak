# Bosak REQ-130 Implementation Review

Date: 2026-10-17. PR: [#361](https://github.com/Fytala-Charles/Bosak/pull/361), merged. Exact implementation head `07e66dbb12c4c717c12364f03992a80d233c51c1`; integrated revision `3264fcfaf739e492770c882d2cbfc1f93f0172d2`. Source version 1.18.0, unpublished at this review. Review and independent tests only; Braid pin remains published 1.17.0/15eacde48.

## Verdict

**No blocking implementation finding within the delivered bounded profile.** The implementation follows the reviewed instruction-only deletion contract and separate two-whitespace-node merge relation. It is suitable to proceed through publication and independent Braid adoption work. This is not final Braid functional/native acceptance, release authorization, a posted GitHub approval or a dependency update.

Confirmed the PR is merged, both PR CI checks succeeded, the merged checkout is clean, and PR-head versus merge source/test trees are identical. Reviewed the new public target/proposal/result/failure/merge/candidate types, snapshot wrappers, full deletion pipeline and shared insertion-helper visibility changes. The existing insertion/expression defaults and failure enums remain intact.

## Independent Test Results

| Run | Result | Scope |
|-----|--------|-------|
| Exact merged source-focused tests | **226/0/0** | 49 deletion cases (including four internal malformed-proof controls) plus 177 insertion/readable controls |
| Unchanged independent public package contracts | **221/0/0** | 45 public deletion, 175 public insertion/readable and one nine-assembly provenance case |
| Public contracts plus new Braid probes | **241/0/0** | The same 221 plus 20 independently authored Braid cases |

Counts are passed/failed/skipped. These runs overlap; they are not 688 unique cases. Source tests were run on the exact clean merge checkout using RunAnalyzers=false and TreatWarningsAsErrors=false for inherited upstream build/test warnings; normal Braid settings were not changed. Copied upstream package tests use the same isolated overrides, not friend-assembly access. The full upstream solution/language-server suites and frozen gates were not independently rerun here; upstream reports 9,661 solution/84 language-server passes and unchanged QT3 31,142/0/679 plus qt4 8,278/0/1,894, with successful PR CI. No normal Braid runtime change was made; its last complete baseline remains 952/0/0.

Package tests restored local merged-revision 1.18.0 artifacts into a fresh `.dependencies/req130-review-package-cache`. All nine consumed engine assemblies identify exactly `1.18.0+3264fcfaf739e492770c882d2cbfc1f93f0172d2`. These are unpublished local artifacts, not public NuGet/release verification. GitHub v1.18.0 release lookup returned not found.

Receipts under `.acceptance/req130-implementation-3264fcfa`: `source-results/source-focused.trx`, `results/public-contracts.trx`, `results/independent-review-corrected.trx`, exact copied public fixtures, independent probe source and `package-identity-final.json`. The initial independent fixture run used an undeclared stylesheet prefix and failed before reaching deletion; the fixture prefix was corrected without an engine patch, and its original TRX is preserved. A probe build also used the wrong source-byte helper name before switching to the actual public snapshot export API; no engine source was modified.

## Independent Braid Probe Coverage

- Eight imported-source cases combine four encodings with compact/indented repeated identical value-of siblings. Repeated middle/last/sole deletion uses each fresh candidate, checks changed exact output after each step, retains explicit empty parent spelling and refuses the old descriptor in the new snapshot.
- Four encoding cases each exercise four lexical seams: physical CR plus LF, referenced CR plus physical LF, referenced CR/LF and tab/CRLF runs. Actual deletion merge Text values follow engine-owned joined-source semantics, with exact byte fragments and full source reconstruction.
- Eight insertion/deletion lifecycle cases combine four encodings with PreserveSource/Readable insertion. They delete the actual inserted descriptor, retain the original snapshot, verify changed exact output, write a new temporary ordinary XSLT file, verify its bytes/hash, independently reopen/inspect/run it at the destination URI and successfully insert again through the supported API. Temporary files are cleaned up.
- Each successful candidate independently verifies removal/empty replacement, exact unchanged-byte replay, complete disjoint original/candidate graph partitions by descriptor reference, injective ordinary survivors, translated survivor extents, the mapped surviving parent, bounded contiguous two-fragment merge coverage and immutable/defensive evidence collections/bytes.

The temporary-file checks use ordinary file storage and the engine public compiler. They do not claim Braid's restricted export coordinator, licensed preview worker, session-issued deletion review/Apply/history or native UI is implemented or accepted.

## Source Review Conclusions

The pipeline rejects foreign descriptors by exact snapshot-owned reference before eligibility; then enforces principal-only/module/byte/profile/body/static/resource exclusions before original validation. Targets require expanded XSLT value-of identity, supported select and an empty descriptor body. Any preserving ancestor and significant mixed/CDATA context refuse under the declared conservative profile. The actual scanner extent must match retained source.

Proposal rechecks the target, emits only original prefix plus suffix around the removed instruction, inspects privately with compilation disabled and denied module resolution, proves complete graph correspondence/merge evidence and independently validates the candidate before success. Parent selection is an actual mapped candidate descriptor. Numeric ID reuse across snapshots supplies no authority.

The recursive proof matches exact ranges, kinds, names, text, attributes and child ordering for ordinary survivors; the removed instruction is the only removed descriptor. At most one merge uses exactly the two adjacent ordinary nonempty whitespace nodes. Ordered full original/candidate ranges prove byte equality and complete contiguous coverage of the actual fresh text node. Fresh XML inspection owns parsed Text normalization; consumers do not concatenate original parsed values or invent split descriptors.

Null programmer inputs throw, known ineligible inputs return classified failure, and deletion cancellation throws OperationCanceledException rather than insertion's older classified cancellation result. Both discovery and proposal perform current work cancellation checks. Public engine snapshots remain immutable reusable inputs; only Braid can enforce current session identity/licensing/overlap at Apply.

## Remaining Adoption Gates

1. Verify a published 1.18.0 delivery (or separately authorize exact unpublished-revision adoption); keep the production pin unchanged until then.
2. Add deletion-specific capability/opaque target/port/DTOs for removed descriptors, descriptor-scoped ordinary survivors, merge groups/fragments, surviving parent and complete edits. Do not flatten merges or borrow insertion's new-node/select acceptance evidence.
3. Freeze and independently validate complete partitions/fragment coverage/replay, current source/inspection/permission/cancellation/overlap in a session-issued deletion review and source-owner Apply boundary. Add malformed consumer-adapter evidence and exact history/selection tests.
4. Add complete bounded deletion review and graph/outline parent selection; verify real worker/validation/restricted export/reopen changed-output golden and native acceptance. Engine success alone does not certify this consumer flow.

No engine edits, Braid runtime/pin changes, upstream comments/approval, commit or push were made in this review. See [design review](BOSAK_REQ130_CONSUMER_REVIEW.md), [consumer plan](VALUE_OF_DELETION.md) and [proposed ADR-016](ADR-016-reviewed-deletion-and-survivor-selection.md).
