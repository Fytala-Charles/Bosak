<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak VS Code 0.2.2 candidate">
  <br><br><h1>VS Code 0.2.2 preparation</h1>
  <p>Released engine 1.7.0 alignment, 15 October 2026</p>
</div>

Unpublished extension 0.2.2 bundles engine 1.7.0. Its source/test trees match the approved library release, including bounded direct and transitive abstract optional function overrides. Manifest/lockfile and extension readme are updated. Public Marketplace version remains 0.2.1 with engine 1.6.0; publication requires separate approval.

TypeScript compilation, TextMate highlighting, DLL/apphost launch selection, startup registration and all **84 language-server tests** pass. A framework-dependent VSIX contains all ten project assemblies at 1.7.0.0/exact source informational revision and matches the fresh publish output byte-for-byte. The runtime language-client dependency is included. Packaged stdio checks verify initialization, hover handling and three diagnostic cases. An isolated hidden VS Code profile verifies a valid stylesheet, invalid-keyword XPST0017 and live correction, without changing the user's installed extension/profile.

[Preparation evidence](VSCODE-0.2.2-PREPARATION.json) preserves this initial working-manifest candidate. After integration, rebuild from the exact merged revision, repeat archive/metadata/protocol/editor checks and present that revision for Marketplace approval. Do not create a vscode-v0.2.2 tag or dispatch the publishing workflow during preparation.

Package-dependent editor signatures and external module acquisition retain their current coverage limits. Bundling 1.7.0 does not establish new editor coverage for package composition. No engine API, conformance gate or REQ status changes are included.
