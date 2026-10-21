<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.2 release readiness">
  <h1>Bosak 1.23.2 release readiness</h1>
  <p>21 October 2026</p>
</div>

**1.23.2 published and verified — 21 October 2026:** [Public delivery proof](RELEASE-1.23.2-PUBLICATION.md) records immutable `v1.23.2` at `7832b785`, one successful Release run (10,331 solution tests; ten uploads), ten verified signed packages and fresh nuget.org-only153 broad /176 focused checks with matching provenance and clear audits. Includes the unpublished1.23.1 switch identity and4.0 index-of empty-collation fixes. REQ-126 stays In Progress; completed Braid REQ-135 acceptance on1.23.0 is unchanged. Braid1.23.2 adoption is a separate upgrade smoke check. Earlier preparation/unpublished checkpoints are historical.

**Historical preparation record:** The original approval-pending checklist below predates completed publication.


**Prepared; publication approval pending.** Pin proposed annotated tag `v1.23.2` to **`7832b785925d2491e0ceac0dd9faa60261619bc4`**, the PR #399 implementation merge. A later documentation merge must not change the package source. Read the [release notes](RELEASE-NOTES-1.23.2.md) and [machine-readable readiness record](RELEASE-1.23.2-READINESS.json).

| Gate | Evidence |
|---|---|
| Reviewed source | PR #399 head aee00a1d; merge 7832b785 has the identical complete tree |
| Exact-head CI | [39614088983](https://github.com/Fytala-Charles/Bosak/actions/runs/39614088983): all three jobs pass; 10,331 solution +84 language-server tests |
| Merged-main CI | [39615240217](https://github.com/Fytala-Charles/Bosak/actions/runs/39615240217); build/test/language-server steps succeed; final job/workflow status recorded in the machine-readable plan |
| Local exact-merge build | Succeeds; ten 1.23.2 libraries packed; one existing CS8604 conformance warning |
| Local exact-merge solution verification | 10,331/0/0 across ten projects after restoring the required pinned QT3 fixtures; original missing-fixture run retained separately |
| Package-only consumers | 153 broad +176 focused pass; no engine ProjectReferences or friend-assembly identity |
| Package provenance | Ten nuspec repository commits and versions, aligned Bosak dependencies, XML docs, exact README/icon/license/brand asset bytes, archive/cache/output hashes and informational revisions match |
| Restore and audit | Fresh separate caches; Bosak.* mapped only to the candidate feed; both transitive vulnerability audits report no vulnerabilities |
| Frozen gates | Previously verified identical reviewed runtime: QT3 31,142/0/679; qt4 gated 8,278/0/1,894; no additional gate run during preparation |
| Read-only availability preflight | No 1.23.1/1.23.2 tag or GitHub release; neither version appears in any of the ten public NuGet indexes |

The copied broad consumer initially retained a 1.23.0 assembly-version guard. Its failed receipt is preserved; changing that candidate-only guard to 1.23.2 makes all 153 checks pass without changing the library. The first full solution attempt lacked QT3 schema fixtures in the new worktree; the affected runtime suite was rerun after initializing the pinned submodule. Original results are not overwritten.

These are **unsigned local candidate** proofs. Signed public archives, actual NuGet restore sources, public cache/output identity and post-publication consumer runs remain future gates. Preparation creates no release tag, Release workflow, package upload or GitHub release. Public 1.23.0 and the completed bounded REQ-135 Braid acceptance remain unchanged. No Braid files or package pins are changed.

## Publication sequence after approval

1. Recheck the immutable revision/version, green CI and live tag/release/all-ten-package availability immediately before publishing. Reconcile any existing or partial artifacts; never retag or overwrite them.
2. Integrate the reviewed preparation documents after their checks pass. Create one annotated `v1.23.2` at 7832b785 and push it once. The existing tag-triggered Release workflow builds, runs the full solution tests, packs and publishes the ten libraries through NuGet OIDC. Do not also dispatch it manually.
3. Monitor that single workflow at the pinned revision; retain logs, the 10,331-test result and ten package-upload acknowledgments. Partial feed propagation is not completion.
4. Create the GitHub release from the prepared notes at the verified tag. Download all ten public archives; verify NuGet signatures, source/version/dependencies and packaged documentation/license/branding.
5. Run fresh nuget.org-only public consumers covering the prepared 153 broad and 176 focused checks. Verify archive/cache/output DLL identity, exact informational revisions, raw signed archive SHA-512 versus NuGet contentHash/assets metadata, restore-source receipts and transitive vulnerability audits. Keep failed/partial/corrected attempts separately.
6. Record verified delivery and update public package links/status. Recommend Braid's normal 1.23.2 package-upgrade smoke check; its accepted 1.23.0 attribute lifecycle is not reopened or claimed revalidated by this release.

Local retained preparation: `.guard-tmp/release1232-preparation`; ten unsigned archives: `.guard-tmp/release1232-packages`. The committed JSON contains package hashes and exact revision provenance. Root pending upstream PR #33 notes and sweep data remain preserved; old REQ-126 PR #132 is untouched. Upstream PRs #33/#34 remain outside this publication decision.
