# Final candidate decisions and recipe review

8 November 2026. Base checkpoint: `e992d90b0efa01d45224771a9f8e53dee05096a6`. The user selected Core/server1.81.2, extension0.2.6 and inclusion of the existing Database library as the tenth NuGet package. [The local candidate](../release-1.81.2-candidate/README.md) records new-cohort preparation. This direction does not grant owner/API acceptance or authorize a tag/publication.

## Two decisions before the next build

| Decision | Recommendation | Reason |
|---|---|---|
| Final local candidate versions | Core libraries and language server1.81.2; extension0.2.6 | Keep one engine/server release line incorporating the editor fix. These are proposed shipping identities, separate from the already qualified preview/diagnostic snapshots. |
| Existing Database NuGet library | Include Bosak.XPath.Providers.Database as the tenth package | Its project explicitly records that it ships with the next Core tag. It is the existing REST-loader product, not the proprietary XmlDb provider adoption deferred from this release. A nine-package release would need an explicit disposition of this existing shipping contract. |

[Public registry observations](REGISTRY-PREFLIGHT.json) show none of the nine preview package IDs lists1.81.2 and the extension does not list0.2.6 at the observed time. That is not a reservation, and the Database package's1.81.2 availability was separately checked in DATABASE-REGISTRY-PREFLIGHT.json. Current local source remains1.81.1 and tracked extension0.2.5. No property or manifest version was changed.

[The decision packet](DECISIONS.json) pins existing evidence, workflows and project metadata. Both questions were answered with the recommended choices, recorded verbatim in the decision packet. All owner identities/dates/acceptances and all nine PF budget/baseline slots remain unfilled. The prior [owner review](../foundation-package-workload/OWNER-REVIEW.md) supplies scenario responsibilities and required receipts; no new form pretends those decisions have already been supplied.

## Concrete recipe corrections to implement

1. Select the final package list explicitly. The current NuGet workflow packs the entire solution, while the qualified preview contains only the nine-library XSLT closure. Include and qualify the Database artifact if selected; do not confuse its47 tests with the separate XmlDb4,482-case compatibility evidence.
2. Retain the [qualified NOTICE overlay](../foundation-compatible-view-policy/packaging/PreviewAssets.targets), or adopt an equivalent reviewed metadata correction. Stock pack omitted that asset in earlier preflight. Validate package licenses, README/icon/Unicode assets and exact dependency versions in resulting artifacts.
3. Build from a newly identified immutable source/recipe cohort containing the URI correction and selected versions. Record actual source revision, build inputs, package/cache/output/load hashes and exact server identity. Never relabel C0 preview packages or the modified diagnostic server snapshot with this new identity.
4. Pin VSIX packager4.0.0 or another explicitly reviewed version, and reproduce the extension dependency lock in an isolated candidate. The existing workflows invoke an unspecified latest packager. Include the explicit Unicode license already retained in the qualified VSIX.
5. Publish the exact reviewed VSIX. Cached vsce4.0.0 help confirms `publish --packagePath <path>` supports an existing VSIX. The current workflow runs `package` and then invokes `publish` without that path, so preparation must reconcile the publication step with exact-artifact review. No publish command or workflow dispatch ran here.
6. Retain separate final-artifact checks: fresh-cache package-only consumer and retained caller; package-backed regressions/conformance/API; scoped owner workloads; server unit/protocol and installed editor/command tests; fresh install/upgrade. A version or rebuilt-byte change requires applicable new-cohort qualification.

Both existing release workflows publish on manual dispatch. NuGet uses `v[0-9]*`; the extension uses `vscode-v*`. Neither is a preparation-only entry point. Do not dispatch either workflow or create tags to test a proposal. Credential/signing availability has not been checked; no secret state was read.

## Acceptance remaining after version/scope selection

Version selection only closes the candidate naming choice, not B2/B3/B5/B7 in the [release checklist](../RELEASE-READINESS-1.81.1.md). Representative owner workload/baseline/budget decisions, disposition of deferred provider requirements, C45/API acceptance, final provenance/signing and approval over concrete final artifact hashes remain required. Shared PF scenarios need the relevant owner dispositions. Candidate-only decimal measurements and proposed guardrails cannot become accepted SLAs by being copied into a release plan.

Versions and Database package scope are now selected; the new local artifact recipe uses immutable checkpoint e992d90b with explicit1.81.2 metadata overrides. No source behavior, workflow, version, owner repository, commit, push, tag or publication was changed in this planning step. This documentation/preflight step does not claim fresh engine tests.

Run `python docs/release-final-candidate-plan/tools/verify.py` for input-hash, registry observation, unchanged acceptance and local-link checks.
