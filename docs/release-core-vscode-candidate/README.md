# Isolated Core / VS Code candidate - 8 November 2026

**Current follow-up: editor execution blocker resolved.** [The successful isolated editor run](../release-vscode-editor-unblock/README.md) passes live XPath/XQuery/XSLT diagnostics and correction with the normal sandbox; no installation ACL change was required. The blocked results below remain the original preparation snapshot.

**Original preparation: local VSIX prepared; editor-host qualification was blocked.** [Results](RESULTS.json) identify the exact artifact and limitations. Nothing was tagged, published or installed into the active user profile.

The candidate is extension **0.2.6**, bundling **1.81.1-preview.c0b926de** engine bytes from immutable Core source `c0b926de34824ceb2010d5f814a4450b28b31dcc`. The tracked extension manifest/lock stay0.2.5. This local candidate version is not a publication or final-release decision.

## Artifact and source

Use [the candidate with notices](vscode-bosak-0.2.6-candidate-notices.vsix), not the preserved earlier artifact. [Final contents](VSIX-CONTENTS-NOTICES.json) enumerate every archive file and hash. It contains Apache license/NOTICE, the Standard package's Unicode17 license, runtime client dependencies and framework-dependent net10.0 server DLLs. All nine engine DLL hashes equal the selected preview packages.

[The plan](PLAN.json) pins the isolated source, derived project and cached VSIX packager4.0.0. Language-server C# bodies are unchanged copies of the archived qualified source. The isolated project replaces ProjectReferences with references to the exact nine preview DLLs so another build cannot silently substitute new engine bytes. Server informational identity reports the preview version plus the C0 revision. This qualifies a diagnostic bundle, not the unmodified production release workflow. Existing npm dependencies and offline NuGet cache were reused; no fresh-cache npm restore, vulnerability audit or reproducible-build guarantee is claimed.

## Verification

- Build, TypeScript compile, TextMate highlighting and mocked launch/registration checks passed; original command/log receipts are retained.
- **84/84 language-server tests passed** using unchanged retained test binaries with the newly built server and exact preview engines; [test layout](SERVER-TEST-LAYOUT.json) and raw TRX are retained. The Core engine suites were not rerun because no engine body or DLL changed.
- [Packaged protocol checks](PROTOCOL-NOTICES.json) passed initialization, valid/invalid bounded XSLT diagnostic cases and clean shutdown through `dotnet` launching the extracted VSIX server. These three protocol cases are not comprehensive editor or command coverage.
- [Fresh install and upgrade](INSTALLATION-NOTICES.json) passed in disposable profiles, upgrading from the retained local0.2.5 Marketplace artifact, not a fresh network download. [Installed content](INSTALLED-CONTENTS.json) matches the reviewed archive; only VS Code's added `__metadata` property changes package.json.
- The first editor-host launch inherited Electron's CLI mode and failed before startup. A retry with only child-launch environment corrected reached an installation ACL failure: Chromium reports that its sandbox cannot read the installed VS Code directory. Both stderr receipts are retained. No ACL, machine-wide error setting or Chromium sandbox setting was changed. No live editor diagnostics, correction or interactive command pass is claimed.
- The original VSIX omitted the explicit Unicode notice. A preserved second build adds only that license; every original payload file remains byte-identical. Protocol/install/upgrade checks were repeated against the final artifact.

## Remaining gates

Run the retained editor harness in an environment where the installed VS Code can start with its normal sandbox, then verify XPath/XQuery/XSLT activation, diagnostics and live correction. Representative extension commands and runtime/platform coverage remain separate checks. Resolve scoped owner workload/budget and C45/API acceptance, final versions/source identities and signing/provenance. Qualify any resulting new artifacts; do not relabel this preview or publish it via a workflow that rebuilds different bytes.

See [the selected release scope](../release-core-vscode/README.md) and [release checklist](../RELEASE-READINESS-1.81.1.md). This packet closes local VSIX preparation and CLI installation screening, not complete extension or release acceptance. The active editor, production source/version files and owner repositories were preserved.

Run `python docs/release-core-vscode-candidate/tools/verify.py` for retained artifact, layout, install and test evidence checks. This verifier never starts the editor or republishes artifacts.
