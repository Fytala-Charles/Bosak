# Core packages and VS Code extension release preparation

8 November 2026. The user selected preparation of a bounded Core package release and explicitly included the VS Code extension. Schema, XmlDb and Braid production integrations are deferred from this delivery scope. Their existing requirements, budgets and owner acceptance remain open; this direction does not grant C45/API freeze or publication approval.

## Deliverables

| Deliverable | Candidate and boundary | Remaining qualification |
|---|---|---|
| Nine Core ecosystem packages | Existing `1.81.1-preview.c0b926de`, source `c0b926de34824ceb2010d5f814a4450b28b31dcc`; Bosak.Xslt dependency closure includes XPath libraries and XQuery. | Applicable owner acceptance; final version/recipe/provenance decision and qualification of resulting final bytes. |
| VS Code extension | `fytala.vscode-bosak`; source/lock version currently0.2.5. Proposed next patch version0.2.6, not yet applied or claimed available. | Fresh VSIX bundling the selected engine cohort, extension-host/install/upgrade checks and release notes. Its version is independent of NuGet SemVer. |
| Language server | Non-packable net10.0 executable bundled in the VSIX; not a tenth NuGet library. | Exact server/dependency/load identities and diagnostics against the selected nine-engine cohort, rather than an old ignored `server/` directory. |

Profiles and limitations stay bounded by the [qualified package evidence](../foundation-preview-qualification/README.md) and existing integration documentation. Do not advertise full XPath/XQuery/XSLT4.0, unrestricted decimal arithmetic or production Schema/XmlDb/Braid provider integration. Existing editor signature and external-module coverage limits remain unless separately implemented and qualified.

## Initial findings and completed checks

[Preparation inventory](PREPARATION.json) pins the extension manifest/lock, launcher/tests, workflows and server project. Both manifest and lock still identify0.2.5. The local README describes its historical engine1.16.5 alignment; that text is not a live Marketplace verification. The inventory records ten ignored Bosak server assemblies, including the language server. None of the nine comparable engine hashes matches the selected preview; the directory is unsuitable as release evidence.

- TypeScript compilation passed (`npm.cmd run compile`).
- Existing TextMate highlighting and mocked DLL/apphost launch/registration checks passed (`npm.cmd test`); [captured output](extension-tests.txt).
- These checks do not exercise a real extension host, install/upgrade or live language-server diagnostics. The existing separate84 language-server tests qualify their recorded cohort, not an unbuilt VSIX.

## Remaining work, in order

- [ ] Stage an isolated candidate extension from pinned source, manifest, lock and documentation. Use a fresh task directory; preserve earlier VSIX and server outputs. Record Node/npm/compiler/packager versions and dependency resolution. Pin the packager before running it; do not silently fetch an unspecified latest tool.
- [ ] Select and record the extension version alongside the final Core version. Keep manifest and lock aligned; do not reuse a published version or claim the preview already has final release identities.
- [ ] Publish the net10.0 language server into the isolated candidate with explicit source/version identities and the selected engine dependency cohort. Retain third-party runtime dependencies, runtime configuration and legal notices. Verify all nine selected engines and server hashes; use framework-dependent DLL launch as the existing workflow does.
- [ ] Build a local VSIX, inventory its manifest/assets/runtime dependencies/license/NOTICE and contents, exclude source/test/scratch/credentials/old VSIX files, and hash it. Verify packaged DLLs against the candidate manifest rather than relying on version strings.
- [ ] Run diagnostics/protocol checks against the unpacked VSIX payload: startup/initialization, XPath/XSLT/XQuery documents, valid/invalid diagnostics and live correction; representative command execution and clean shutdown. Exercise the default bundled path, not just a custom server path.
- [ ] Test installation in a disposable VS Code profile/extensions directory, preserving the active Codex session. Test fresh install and upgrade from the selected known prior artifact with recorded hashes. Verify activation, output/diagnostics and extension identity; no production profile install is implied.
- [ ] Write release notes for the engine refresh and supported editor behavior. Keep performance/provider claims within accepted evidence and record runtime/platform requirements and migration limits.
- [ ] Resolve applicable [release gates](../RELEASE-READINESS-1.81.1.md), including explicit disposition of deferred provider/workload requirements. Review and commit/push the evidence; prepare final artifacts and request approval for exact publication actions.

## Release workflow boundary

The inspected extension workflow publishes on `vscode-v*` tags **and manual dispatch**. It rebuilds the server and runs tests, packages a VSIX, uploads it, then runs Marketplace publication. Manual dispatch is therefore not a preparation-only operation. Use local packaging for this work; do not dispatch or create tags during preparation. The current workflow packages and then separately invokes `vsce publish`; before release, ensure the exact reviewed VSIX is the artifact published and retain its identity. The unpinned `npx --yes @vscode/vsce` also needs a concrete reviewed tool version for reproducible preparation.

NuGet uses the separate `v[0-9]*` tag workflow and trusted publishing. Its stock pack recipe must be reconciled with the already qualified NOTICE overlay and final nine-package set; do not assume an unmodified stock pack produces the reviewed preview. Neither workflow was invoked, and no credential/secret state was read or modified.

## Deferred delivery work

Schema public provider placement/profile, XmlDb snapshot/operation adapter and Braid worker/IPC production adoption are outside this selected delivery scope. Correctly stamped owner artifacts remain necessary for any later owner delivery. Existing compatibility and split-run process-probe receipts remain evidence; they do not become production adoption approval. Applicable owner workload/baseline/budget and C45/API decisions need explicit disposition before release acceptance.

The immediate next implementation is the isolated language-server/VSIX candidate, followed by real editor and install/upgrade qualification. No new engine semantics, production dependency pins, tags or publication changed in this preparation step.

## Candidate follow-up

[The isolated0.2.6 candidate](../release-core-vscode-candidate/README.md) now has a locally built VSIX with the exact nine preview engines and explicit Unicode notice. Compilation, existing extension checks,84 server tests, three packaged protocol cases and disposable-profile fresh/upgrade installation pass. Real editor-host qualification remains blocked by the local VS Code installation ACL; no live-editor pass or production version change is claimed. Final release/owner acceptance remains open. The checklist above is retained as the original plan.
