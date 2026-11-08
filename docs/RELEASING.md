<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak release preparation guide">
  <br><br><h1>Preparing and publishing Bosak releases</h1>
  <p>Separate artifact preparation, review and exact-artifact publication</p>
</div>

## Current scope and prerequisites

The tracked candidate versions are Core/language server **1.81.2** and VS Code extension **0.2.6**. [The release configuration](../.github/release-config.json) lists exactly ten NuGet libraries, including the existing Database REST-loader package. The language server is a bundled executable, not an eleventh NuGet library. Proprietary XmlDb/Braid integrations are separate delivery work.

These pins do not announce a public release. [The release checklist](RELEASE-READINESS-1.81.1.md) still requires applicable owner workload/baseline/budget decisions, disposition of deferred requirements, C45/API acceptance, signing/provenance review and approval over concrete final artifact hashes. Preparing an artifact, pushing a commit or creating a tag does not grant those decisions.

## Preparation

Both release workflows now default to preparation only. A matching version tag also prepares artifacts; it does not publish them. A mismatched tag fails before building. NuGet uses `v<Core version>`; VS Code uses `vscode-v<extension version>`. Versions come from tracked metadata, never from silently rewriting that metadata to fit a tag.

1. Commit reviewed source, version and recipe changes. Preparation requires a clean Git checkout root: tracked changes and untracked source inputs are refused before sealing. SDK/npm outputs stay ignored or outside the checkout. Gitlinks are checked out by the workflows where required; the frozen gate check independently verifies conformance totals and skip names/reasons.
2. Run the appropriate workflow with `publish=false`, or use the corresponding approved preparation tag. **Do not dispatch either workflow as part of ordinary local verification without explicit instruction.**
3. NuGet preparation builds/tests the solution, runs frozen QT3/gated qt4 with exact totals and skip names/reasons, then packs the explicit ten-project set. NOTICE is included by normal MSBuild metadata. Packages must have the selected version, source revision, dependency closure, XML docs and legal assets. Unicode license and NOTICE/license bytes are compared with the source checkout.
4. Extension preparation publishes a framework-dependent server from the same source/version, installs the dependency lock, compiles/tests, runs server tests and packages with pinned vsce4.0.0. Node24.16.0 satisfies its Node22-or-newer requirement. .NET SDK10.0.301 matches the qualified local toolchain. License/NOTICE/Unicode files and the server dependency cohort are verified.
5. Download `nuget-<source SHA>` or `vscode-<source SHA>` from the successful preparation run. Each contains the artifact files and `release-manifest.json`. Retained qualification artifacts include raw TRX and gate receipts. Review the artifacts and record the SHA-256 of **the manifest file**, not the enclosing download ZIP or a prior locally built candidate.

The previous local candidates remain historical. Changed source/version/recipe/build outputs require their own qualification and manifest; do not relabel earlier hashes. Hosted Linux preparation and publisher authentication remain separate from local Windows validation.

## Publication after approval

Publication is an explicit second manual dispatch, not a rebuild:

- Set `publish=true`.
- Supply the successful preparation `source_run_id` and its reviewed `expected_manifest_sha256`.
- The workflow checks the source run's success, event, workflow path, source repository and commit, then downloads only that run's matching artifact. It checks out the artifact's source commit and verifies the supplied manifest hash, complete artifact set, byte hashes, versions, dependencies and required legal assets.
- NuGet exchanges its existing OIDC identity only after verification, then pushes those existing package files. The registered `release.yml` filename and `publish` job ID remain unchanged. Existing duplicate-skip behavior remains; registry verification after publication is still required and must not infer content identity merely from a skipped duplicate.
- Marketplace publication uses `vsce publish --packagePath` with the verified VSIX. It does not run another package build. The existing repository secret stays named `VCSE_PAT`; only the publish step exposes it as the packager's `VSCE_PAT` environment variable. Missing credentials fail without displaying their value.

Preparation has no publisher credential step. Publication checks require a reviewed manifest and successful source run; they do not fabricate owner acceptance or replace the checklist. No environment/protection rules or secret state were changed by this recipe update. The artifact download and provenance checks use the [download-artifact inputs](https://github.com/actions/download-artifact/blob/v4/README.md) and [GitHub workflow-run API](https://docs.github.com/en/rest/actions/workflow-runs#get-a-workflow-run).

After an explicitly approved publication, retain service-side package/extension verification and final source/tag/artifact identities separately. Do not claim service publication from a local package, uploaded preparation artifact or workflow configuration alone.

## Local recipe checks

```powershell
python .github/scripts/test_release_artifacts.py
node .github/scripts/test-source-run.cjs
python .github/scripts/check_workflows.py
```

The YAML checker requires pinned `PyYAML==6.0.3`; install it in a task-local environment, or use the CI recipe-control job. The tests exercise valid artifact sets and refusal of changed bytes, source/version mismatch, missing legal assets, unexpected packages, unsafe ZIP entries, changed skip reasons, wrong experimental counts and invalid preparation metadata. Synthetic controls are not engine or hosted publication tests.

The read-only artifact helper prints pinned versions/projects, checks tag correspondence, seals fresh manifests and verifies supplied reviewed hashes. It contains no publish or network operation. Real release artifacts must be produced and checked from the matching clean immutable checkout.
