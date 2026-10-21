# REQ-135 immutable merge verification

21 October 2026. The user authorized merging PR #392. Immutable merge **`9218a2b6f2339f923fd6d018ba988c8325b2c44d`** contains source1.23.0, unpublished. Public packages remain1.22.0. [Braid implementation head review](REQ-135-CONSUMER-IMPLEMENTATION-REVIEW.md) cleared f9e618dc with700 public-only passes and no blockers. Runtime source and tests are identical from that reviewed head through the approved documentation head2ea4aea5 to this merge. This does not substitute for independently checking merge-specific package identities.

Merged-main [CI39543201162](https://github.com/Fytala-Charles/Bosak/actions/runs/39543201162) succeeds: **10,214 solution +84 language-server tests**, all16 runner controls, four Windows wrapper controls and highlighting. All11 project summaries were checked from the full log. The136 new source cases (117 public/19 internal) are included in the solution count. Existing frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894** evidence remains valid for the unchanged runtime source; no new manual conformance run is claimed by this merge checkpoint.

A fresh isolated PackageReference-only consumer runs against a separate exact-merge local feed/cache: **117/0/0**. All ten candidate archive nuspec repository revisions equal9218a2b6; all nine consumed informational versions equal1.23.0 plus the full merge SHA. Archive/cache/output DLL bytes, cached archive bytes, SHA-512/contentHash/project.assets metadata and actual restore-feed receipts agree. These are unsigned local candidates, not public NuGet/signature verification. All previous head archives/caches/results/provenance and Braid review originals are retained separately; repeated suites are overlapping evidence.

Delivery assets:

- Packages: `D:/Development/Bosak/.guard-tmp/req135-merged-packages`
- Consumer: `D:/Development/Bosak/.guard-tmp/req135-merged-package-consumer`
- Fresh cache: `D:/Development/Bosak/.guard-tmp/req135-merged-package-cache`
- Provenance: `D:/Development/Bosak/.guard-tmp/req135-merged-public-provenance.json`
- Full CI: `D:/Development/Bosak/.guard-tmp/req135-merged-main-ci.log` and `.json`
- [Tracked merge verification](REQ-135-MERGED-VERIFICATION.json) retains counts and identities.

Next: [independent Braid exact-merge confirmation](REQ-135-CONSUMER-MERGE-HANDOFF.md). Publication requires a separate user decision afterward. No tag/release, public package push, Braid runtime/pin/UI change or native acceptance is performed. The current documentation checkpoint changes no source/tests/version, and prior merge-pending text is historical.
