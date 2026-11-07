# Preview recipe preparation, not a built package

The preflight evaluates actual installed MSBuild metadata for all eleven source projects and the seven-library transitive XPath API package closure. It does not build, pack, restore, publish or change source/version. Current source and qualified binaries remain 1.66.0. No preview/stable version or release date has been selected.

The stock pack items omit root NOTICE for the selected packages. The explicit PreviewAssets.targets overlay includes exact candidate NOTICE; metadata evaluation confirms this without editing production Directory.Build.props. License, README, icon/logo/brand assets and Standard's Unicode license remain in the inventory. Preserve this overlay in the eventual recipe or adopt an equivalent separately qualified stock metadata fix; do not call an unmodified stock pack command sufficient.

The current dirty source/package inputs are identified in evidence/PACKAGE-PREFLIGHT.json. HEAD alone cannot identify the candidate. Before an actual preview, checkpoint the selected source/assets/recipe into one exact immutable commit, reconcile its hashes with the qualified candidate, choose a preview SemVer explicitly and build/package from a fresh source archive. This step does not commit or publish. The seven-package closure is the proposed first preview scope, not automatic adoption of XQuery/XSLT/database/tooling products.

## Reviewable command recipe

The following parameters are deliberately unresolved. This is a template, not an execution receipt or a claim that the current dirty checkout is publishable.

1. Select QUALIFIED_COMMIT, PREVIEW_VERSION and a new empty WORKSPACE within the ignored task area. Archive exact src, README.md, license.md, NOTICE and assets from that commit. Record the recipe revision/hash separately and verify every qualified source and asset hash.
2. Build the archived source under net10.0/Release with explicit Version/PackageVersion/SourceRevisionId/RepositoryCommit consistently across all selected projects. Do not pack old assemblies under an unrelated new version. Capture actual compiled identities and full source configuration.
3. Pack each of the seven selected projects with the explicit overlay and CandidateSourceRoot pointing at the archive. Retain nupkg/nuspec, XML docs, complete dependency versions, license/NOTICE/README/icon/Unicode assets and SHA-256/NuGet hashes. Validate archive paths/content before accepting any package.
4. Restore a PackageReference-only API consumer from only the fresh local feed, with a fresh private cache and cleared fallback folders. No ProjectReference/HintPath or global-cache engine dependency. Verify package/archive/cache/output/actual loaded hashes and normal consumer behavior. Use a separately scoped new fixture for bounded construction; do not pretend a fixture proves native facets.
5. Replay the retained unchanged binary caller through tools/run-retained-foundation-caller.py with the exact seven-assembly candidate evidence and caller/configuration hashes. Catch negative diagnostics; use the existing popup-safe runbook. Do not copy the old unguarded historical launcher into a new recipe.
6. Qualify the exact package-backed unit/gate cohort with exact skip maps and record any non-packaged XQuery/XSLT/database product assemblies separately. Refresh actual owner workloads and migration checks against that same candidate. Review signing/provenance and release authorization only after the artifacts are concrete.

The command template is COMMANDS.json. Its unresolved placeholders prevent accidental presentation as a runnable release. Current metadata evaluation alone does not verify nupkg contents, package-only consumption, signatures, owner acceptance or reproducible builds. R6/R7 remain Pending.
