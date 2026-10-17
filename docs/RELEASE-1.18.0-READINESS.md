<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.18.0 release readiness">
<h1>Bosak 1.18.0 release readiness</h1>
<p>Prepared; publication authorization outstanding — 17 October 2026</p>
</div>

**Publication completed after user approval — 17 October 2026:** Immutable v1.18.0 remains at `3264fcfaf739e492770c882d2cbfc1f93f0172d2`. [Public delivery and verification](RELEASE-1.18.0-PUBLICATION.md) supersedes the original preparation-only status below. Original execution boundary/preflight findings are retained as history; do not republish or move the tag.

## Decision and immutable target

**Ready for publication authorization, not published.** Proposed version 1.18.0, tag `v1.18.0`, exact source target `3264fcfaf739e492770c882d2cbfc1f93f0172d2` (merged PR #361). The documentation preparation branch may advance separately; do not tag its later documentation merge or rebuild the release from an unspecified main HEAD. The current ten library versions are already 1.18.0; no further source/version bump is needed. [Structured readiness](RELEASE-1.18.0-READINESS.json) and [prepared release notes](RELEASE-NOTES-1.18.0.md) record this decision.

The user authorized release preparation after Braid's no-blocking implementation review. Publication remains a separate action: this step must not create/push a release tag, dispatch Release, upload NuGet packages, or create a public GitHub release. GitHub draft/public release creation is not needed for this reviewable preparation.

## Evidence already available

| Evidence | Status |
|---|---|
| Reviewed implementation | PR #361 head 07e66dbb; exact integrated source 3264fcfa. Source/test/extension trees agree. |
| CI | Final-head run 39029229185 and merged-main run 39029719562 successful. 9,661 solution +84 language-server tests at final head; highlighting green. |
| Actual source-focused cases | 226 pass, including 49 deletion and 177 legacy insertion/readable controls. |
| Frozen conformance | QT3 31,142/0/679; qt4 gated 8,278/0/1,894. |
| Local-package proof | Exact final head and integrated revision each 221 public-only consumer passes; ten archive/nine assembly provenance and zero reported vulnerable dependencies. Original failure/correction receipts retained. |
| Independent Braid implementation review | No blocking finding at exact head/merge. Reported 226/221/241 runs overlap;241 includes 20 independently authored cases. Local artifacts only; no functional/native acceptance inferred. |
| Publication preflight — 17 October 2026 | No remote v1.18.0 tag; no GitHub release; all ten public NuGet flat-container version indexes lack1.18.0. Recheck immediately before authorized publication. |
| Original review | Verbatim [receipt](REQ-130-CONSUMER-IMPLEMENTATION-REVIEW.md), SHA-256 `f92f5648800e82f9b9507ffed142b824ca330f3114aa7e94331863437fd7e972`; source `D:/Development/Bosak.Braid/docs/BOSAK_REQ130_IMPLEMENTATION_REVIEW.md`. |

Receipt-relative links and `.acceptance` paths retain their original sibling Braid context; they are not copied Bosak artifacts. The review discloses inherited-warning overrides and initial fixture/helper corrections; preserve these limitations. Bosak imports the report without claiming new Braid runs. Its last full normal runtime baseline remains 952; no normal Braid suite rerun or pin change is claimed.

## Prepared execution sequence after authorization

1. Recheck remote tag/release absence and all ten NuGet 1.18.0 versions. Verify target commit, clean isolated checkout, version 1.18.0, source/test trees and successful source CI. Inspect unexpected existing artifacts before taking action; do not retag or silently skip a conflicting release.
2. Create an annotated `v1.18.0` pointing exactly to `3264fcfaf739e492770c882d2cbfc1f93f0172d2`, then push that one tag. This triggers existing `.github/workflows/release.yml`; do not also dispatch it. Keep the tag immutable. The workflow builds/tests, packs, uses existing NuGet OIDC and pushes library packages; it does not create GitHub release notes.
3. Observe the single tag-triggered Release run through success, including 9,661 solution tests and ten library upload acknowledgments. Retain original workflow/run/log receipts. If partial publication occurs, inspect the actual completed uploads and recover that same release; do not start a duplicate run or move the tag blindly.
4. Download all ten public signed packages, verify signatures and version/repository revision/dependency metadata, then restore an independent public-nuget.org-only consumer with a fresh cache. Keep restore, tests and audit on the same scoped cache. Compare downloaded signed archive/cache/output bytes and exact assembly informational revision `1.18.0+3264fcfaf739e492770c882d2cbfc1f93f0172d2`; preserve SHA-512/source receipts. Run focused public contracts and appropriate broader consumer checks. Local unsigned package proof is not public signed-artifact proof; platform/signing can change archive bytes.
5. Create the public GitHub release for the existing immutable tag with reviewed notes only after public artifacts verify. Record public links, package signatures/metadata, consumer results and remaining consumer/native limits. Synchronize canonical docs to Published; retain all original receipts/corrections.
6. Braid independently adopts the verified delivery, implements deletion-specific port/review/Apply/history/parent selection, real worker/restricted export/reopen checks and native acceptance. Keep REQ-130 consumer acceptance open until that evidence arrives. No Braid source/pin/UI or extension changes are authorized by the Bosak publication itself.

## Ten-package scope

- `Bosak.XPath.Api` 1.18.0
- `Bosak.XPath.Compiler` 1.18.0
- `Bosak.XPath.Core` 1.18.0
- `Bosak.XPath.Parser` 1.18.0
- `Bosak.XPath.Providers` 1.18.0
- `Bosak.XPath.Providers.Database` 1.18.0
- `Bosak.XPath.Runtime` 1.18.0
- `Bosak.XPath.Standard` 1.18.0
- `Bosak.XQuery` 1.18.0
- `Bosak.Xslt` 1.18.0

The language server executable is private and excluded from NuGet publication. VS Code 0.2.5 remains on engine 1.16.5. Release preparation introduces no engine changes, test/gate membership changes, workflow changes, credentials or external notifications.
