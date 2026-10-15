<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala VS Code 0.2.3 candidate">
  <br><br><h1>VS Code 0.2.3 preparation</h1>
  <p>Released engine 1.7.1 alignment, 15 October 2026</p>
</div>

Unpublished extension 0.2.3 bundles released engine 1.7.1, including lexical original binding in escaping inline functions. Source/test trees match the approved library release. Manifest/lockfile/readme are updated; published Marketplace version remains 0.2.2 with engine 1.7.0. Publication requires separate approval.

TypeScript compilation, highlighting/launcher/startup checks and all **84 language-server tests** pass. All ten bundled assemblies have version 1.7.1.0 and the exact initial source revision; server payload bytes match fresh publish output and the runtime language-client dependency is included. Packaged stdio initialization/hover/three diagnostics checks pass. An isolated VS Code profile verifies valid input, invalid-keyword XPST0017 and live correction; the user's installed extension/settings are unchanged. [Preparation evidence](VSCODE-0.2.3-PREPARATION.json).

After successful exact-head CI and integration, rebuild the exact merged revision and repeat archive, assembly, protocol and editor checks before Marketplace approval. The initial working-manifest proof remains immutable. Do not create a vscode-v0.2.3 tag or dispatch publication during preparation. Package-dependent editor signatures and module acquisition retain their existing limits; the newer bundle does not establish broader editor coverage. Requires .NET 10.
