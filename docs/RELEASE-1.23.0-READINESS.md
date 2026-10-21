<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.0 release readiness">
<h1>Bosak 1.23.0 release readiness</h1>
<p>21 October 2026</p>
</div>

**1.23.0 published and verified - 21 October 2026:** [Public delivery proof](RELEASE-1.23.0-PUBLICATION.md) records immutable `v1.23.0` at `9218a2b6`, one successful Release run (10,214 solution tests; ten uploads), ten verified signed archives and fresh nuget.org-only **153 broad /117 lifecycle** checks with exact provenance and clear audits. REQ-135 Create/Update/Delete is published after [Braid exact-merge review](REQ-135-CONSUMER-MERGE-REVIEW.md) cleared700 checks without blockers. Braid adoption and LA17–LA22 integration/native acceptance remain pending. Earlier source/preparation/review-pending checkpoints below are historical.

**Historical checkpoint:** The original readiness/source statements below predate approved publication.


**Prepared; publication approval pending.** Pin proposed annotated tag `v1.23.0` to **`9218a2b6f2339f923fd6d018ba988c8325b2c44d`**, the independently reviewed implementation merge. Do not tag a later documentation merge or moving main. Read the [notes](RELEASE-NOTES-1.23.0.md), [machine-readable plan](RELEASE-1.23.0-READINESS.json), [merge evidence](REQ-135-MERGED-READINESS.md) and [unchanged updated consumer review](REQ-135-CONSUMER-MERGE-REVIEW.md).

| Gate | Observed result |
|---|---|
| Implementation merge | PR392 at9218a2b6; source 1.23.0; runtime/tests identical to reviewed f9e618dc |
| Merged-main CI 39543201162 |10,214 solution +84 LS,16 runner controls, four Windows wrappers and highlighting pass |
| Exact-merge local packages |117 public-only checks; ten archive/nine consumed assembly revisions and archive/cache/output hashes match |
| Independent Braid exact merge |700/0/0 with a fresh mapped local feed/cache; no blockers or corrections;19 source controls excluded |
| Documentation review | Braid clears PR393 head932ce591; its three CI checks pass39543856078; this later release-preparation update remains documentation only |
| Frozen gates | Prior verified unchanged runtime: QT3 31,142/0/679 and qt4 gated8,278/0/1,894; not rerun by this preparation |
| Read-only preflight 21 October | No remote1.23.0 tag or GitHub release; all ten public NuGet indexes lack1.23.0; source version matches |

The original head review remains preserved separately. The updated review SHA-256 is `1a9434b348407596dcbb9cbac77997ec771033084957e9dca436ab17621c5896`. Public1.22.0 remains the latest observed release, and Braid has not adopted1.23.0. Local candidate proofs are not signed public-package proof. The existing conformance nullable warning documented in the implementation record is not declared fixed by this preparation.

## Publication sequence after approval

1. Recheck source/version, merged CI, exact-merge review and all live tag/release/NuGet preflight conditions immediately before publication. If any1.23.0 artifact already exists, reconcile its identity/partial state; do not retag, overwrite or blindly republish.
2. Merge the cleared release-preparation documentation when its checks pass, while retaining9218a2b6 as the immutable package/tag source. Create one annotated `v1.23.0` there and push it once. The existing tag-triggered Release workflow performs restore/build, full solution tests, pack and OIDC publication of ten library packages. Do not also use workflow_dispatch.
3. Monitor the single Release run at the exact tag/source; retain all logs and ten upload acknowledgments. Do not infer success from tag creation or partial feed propagation.
4. Create the GitHub release using the prepared notes and verified tag as part of the separately authorized publication workflow. Download all ten public archives as they propagate, verify signatures/source/versions/dependencies/packaged docs and brand/license assets.
5. Run the prepared fresh nuget.org-only broad153-case and lifecycle117-case consumers with no local feed/cache reuse. Verify assembly/archive/cache/output identity, signed raw archive SHA-512 and NuGet contentHash/assets metadata, actual restore sources and transitive vulnerability audits. Preserve partial/failed/corrected receipts separately.
6. Record verified public delivery, then hand off to Braid for public adoption and its distinct LA17–LA22 UI/history/worker/export/reopen/native acceptance. No prior head/merge review certifies those gates.

Prepared local assets/plan: `D:/Development/Bosak/.guard-tmp/release1230-preparation`; read-only preflight: `.guard-tmp/release1230-preflight.json`. No tag, workflow, upload or GitHub release was created during preparation. No release authorization or consumer acceptance is inferred from the no-blocking review.
