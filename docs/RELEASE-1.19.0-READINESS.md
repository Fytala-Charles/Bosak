<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.19.0 release readiness">
<h1>Bosak 1.19.0 release readiness</h1>
<p>Prepared; publication approval outstanding ? 17 October 2026</p>
</div>

**Publication completed after user approval - 18 October 2026:** [Verified public delivery](RELEASE-1.19.0-PUBLICATION.md) supersedes the original preparation-only status below. v1.19.0 remains immutable at27691fdb; do not republish or move it. Original preflight and execution boundary retained as history.

## Decision and immutable target

**Ready for publication approval; not published.** Proposed version **1.19.0**, annotated tag `v1.19.0`, exact source target **`27691fdb3708bb4ffd7c984abbf3196b7cde5667`**, merged [PR #364](https://github.com/Fytala-Charles/Bosak/pull/364). Do not tag the later documentation-preparation merge. Ten coordinated libraries already use 1.19.0; no source/version bump is needed. [Structured readiness](RELEASE-1.19.0-READINESS.json), [prepared release notes](RELEASE-NOTES-1.19.0.md).

This preparation creates no tag, workflow dispatch, package upload or GitHub release. Publication requires a separate user decision. Braid's no-blocking review does not itself authorize publication or accept its future integration.

## Evidence

| Evidence | Observed status |
|---|---|
| Exact source target | Reviewed final head 189405f2 and merge 27691fdb have identical source/test trees. |
| Upstream CI | Final-head39035190436 and integrated 39035670638 pass 9,810 checks (9,726 solution +84 language-server), zero failed/skipped; highlighting green. |
| Focused / frozen gates | 291 focused (65 text +177 legacy insertion/readable +49 deletion); QT3 31,142/0/679; qt4 gated 8,278/0/1,894. |
| Exact local package proof | Source-head/final-head/merge consumers each pass the same 282 public cases; ten archives/nine assemblies match each exact revision and archive/cache/output/restore provenance; zero vulnerable dependencies. Local candidates are unsigned and unpublished. |
| Independent Braid review | No blockers; reported 291 source-focused, 282 public contracts, 298 combined (16 new probes), and 1,015 accepted 1.18 baseline. Suites overlap; counts are not additive. |
| Publication preflight | All ten public NuGet version indexes lack1.19.0; no remote v1.19.0 tag or GitHub release. Timestamp and queries in structured readiness. Recheck immediately before publication. |

The unchanged [Braid review](REQ-131-CONSUMER-IMPLEMENTATION-REVIEW.md) is imported from `D:/Development/Bosak.Braid/docs/BOSAK_REQ131_CONSUMER_REVIEW.md`, SHA-256 **83d8a912f8748462f86e735d8a5153d62aef4c35c3ad2a53ab6046c3ebc45732**. It records analyzer/warning overrides for isolated upstream tests, an initial surrogate InlineData fixture failure and corrected runtime construction with original receipts retained. No engine correction was needed. These are reported independent runs, not new Bosak reruns. Braid consumed existing local exact-merge archives, not public NuGet packages or a new build. It did not independently rerun the full upstream solution/language-server/frozen gates.

REQ-130 remains implemented, published and consumer-accepted in1.18.0. Braid's accepted immutable production pin remains 3264fcfa. REQ-131 integration, worker/restricted-export/session/UI and native acceptance remain separate; review probes do not establish those gates. VS Code 0.2.5/engine 1.16.5 is unchanged.

## Execution sequence after approval

1. Recheck remote tag/release and all ten public version indexes. Verify a clean isolated checkout at 27691fdb, version 1.19.0 and successful exact-source CI. Investigate unexpected existing artifacts; do not retag or silently skip a conflicting release.
2. Create one annotated `v1.19.0` pointing exactly to 27691fdb and push it once. This triggers existing `.github/workflows/release.yml`; do not also dispatch Release. Keep the tag immutable. The workflow builds/tests/packs and uses existing NuGet OIDC; no credentials/workflow changes are needed.
3. Observe the single tag-triggered Release run, including **9,726 solution tests** and ten package upload acknowledgments. The Release workflow does not run the 84 language-server checks or create GitHub release notes. Preserve logs and any correction receipts. On partial publication, inspect completed uploads and recover the same release rather than moving the tag or starting duplicate publication.
4. Download all ten signed public packages. Verify signatures, version/repository revision/dependencies and complete package set. Run fresh-cache **nuget.org-only** focused public contracts (282) and appropriate broader checks, with restore/test/audit sharing one NUGET_PACKAGES. Verify exact informational revision `1.19.0+27691fdb3708bb4ffd7c984abbf3196b7cde5667`, signed archive/cache/output bytes, SHA-512 assets and restore-source receipts, plus dependency audit. Local unsigned bytes are not evidence of public signed artifacts.
5. After public artifacts verify, create the public GitHub release for that immutable tag using the reviewed notes. Record publication/signature/public-consumer evidence and synchronize canonical status; preserve original receipts.
6. Braid independently verifies the public delivery before changing its immutable pin, then implements separate text capability/adapter/review/Apply/history/selection and worker/export/reopen tests, followed by independent native acceptance. No Braid source/pin/UI or extension update is included in Bosak publication.

## Package scope

Ten .NET 10 libraries: Bosak.XPath.Api, Bosak.XPath.Compiler, Bosak.XPath.Core, Bosak.XPath.Parser, Bosak.XPath.Providers, Bosak.XPath.Providers.Database, Bosak.XPath.Runtime, Bosak.XPath.Standard, Bosak.XQuery and Bosak.Xslt, all 1.19.0. The language-server executable remains private and excluded from NuGet publication. No new conformance claims, gate membership changes or external notifications are included.
