<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.4 release readiness">
  <h1>Bosak 1.23.4 release readiness</h1>
  <p>22 October 2026</p>
</div>

**1.23.4 published and verified — 22 October 2026:** [Public delivery proof](RELEASE-1.23.4-PUBLICATION.md) records immutable `v1.23.4` at `2b5ffc50`, one successful Release run (10,470 solution tests; ten uploads), ten verified signed packages and fresh nuget.org-only153 broad /270 focused checks with matching provenance and clear audits. Includes the union-minimum and EQName-priority corrections; opening-bracket URI parsing remains separately limited. REQ-126 stays In Progress; completed Braid acceptance on1.23.0 is unchanged. Braid1.23.4 adoption remains a separate upgrade smoke check. Earlier preparation/unpublished checkpoints are historical.

**Historical preparation record:** The checklist below predates completed publication.


**Prepared; publication not started.** Proposed annotated tag `v1.23.4` must point to **`2b5ffc5076af0411dee606b010368a36d637863a`**, the PR #406 implementation merge, even after preparation documents merge later. Public release remains 1.23.2. Read the [release notes](RELEASE-NOTES-1.23.4.md) and [machine-readable readiness record](RELEASE-1.23.4-READINESS.json).

| Gate | Evidence |
|---|---|
| Reviewed implementation | PR #406 head `d4164e88`; merge `2b5ffc50` has the identical complete tree; includes PR #405 minimum-union correction |
| Exact-head CI | [39733650315](https://github.com/Fytala-Charles/Bosak/actions/runs/39733650315): all three jobs pass; 10,470 solution +84 language-server tests |
| Merged-main CI | [39739617913](https://github.com/Fytala-Charles/Bosak/actions/runs/39739617913); final conclusion recorded in the readiness JSON |
| Exact-merge Release build | Succeeds; ten 1.23.4 libraries packed; existing CS8604 conformance warning |
| Exact-merge solution tests | 10,470/0/0 across ten projects; pinned QT3 schema fixtures initialized |
| Package-only consumers | 153 broad +270 focused pass; no engine ProjectReferences or friend-assembly identity |
| Focused composition | 117 attribute lifecycle +32 switch +27 index-of +94 EQName/union/set-priority checks; overlaps source coverage |
| Provenance | Ten archive versions/repository commits, aligned Bosak dependencies, XML docs and exact README/license/icon/brand assets; archive/cache/output hashes and informational revisions match |
| Restore and audit | Separate fresh caches; Bosak.* mapped only to the local candidate feed; external test dependencies from nuget.org; both transitive vulnerability audits clear |
| Engine gates | Reviewed identical implementation: QT3 31,142/0/679 and qt4 gated 8,278/0/1,894; receipts retained from the implementation slice; not rerun during preparation |
| Read-only preflight | No 1.23.3/1.23.4 tag or GitHub release; neither version appears in any of the ten public NuGet indexes |

The copied broad consumer initially retained numeric assembly-version guards for 1.23.2.0. Original consumer source and its failed run are preserved; updating only those candidate-consumer guards to 1.23.4.0 makes all 153 checks pass. No library correction was needed.

These are unsigned local candidate proofs. No tag, Release dispatch, package upload or GitHub release has been created. Signed archives, public restore sources, content hashes and public consumers remain post-publication gates. The opening-bracket URI parser limitation remains documented; REQ-126 stays In Progress. Completed Braid acceptance and upstream PRs #33/#34 are separate; old PR #132 is untouched.

## Publication sequence

1. Complete review of this preparation and obtain publication approval. Immediately before publishing, recheck exact source/version, green CI and live tag/release/all-ten-package availability. Reconcile partial artifacts instead of overwriting or retagging them.
2. Create one annotated `v1.23.4` at `2b5ffc5076af0411dee606b010368a36d637863a` and push it once. The existing tag-triggered Release workflow runs the full solution, packs and publishes ten libraries via NuGet OIDC. Do not additionally dispatch the workflow manually. No separate 1.23.3 release is planned.
3. Retain the single workflow's exact revision, test results and all ten upload acknowledgments. Create the GitHub release from these prepared notes at the verified tag.
4. Download every public package, verify NuGet signatures and exact version/source/dependencies, and compare packaged README/license/branding with immutable source.
5. Run fresh nuget.org-only consumers covering 153 broad and 270 focused checks. Verify signed archive/cache/output identities, informational revisions, NuGet contentHash/assets metadata, restore-source receipts and transitive audits. Preserve partial and failed attempts separately.
6. Record verified publication and update public package status/links. Recommend a normal Braid public-package upgrade smoke check without reopening or claiming new verification of its completed 1.23.0 acceptance.

Retained candidates: `.guard-tmp/release1234-packages`; preparation logs, original failed consumer guard, restored caches and receipts: `.guard-tmp/release1234-preparation`. Root upstream notes, sweep data, stashes and worktrees remain preserved. Later documentation commits must not move the immutable package source.
