<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak VS Code 0.2.1 published engine alignment">
  <br><br><h1>VS Code 0.2.1</h1>
  <p>Align the extension bundle with released engine 1.6.0</p>
</div>

## Scope

The published 0.2.1 patch release refreshes the language-server bundle from engine 1.4.0 to released 1.6.0 source. The extension and lockfile versions agree. No editor feature, engine implementation or public API is added. The user approved exact final revision `10433195569273474d328208dcf5c0cdd97caeaa`; tag `vscode-v0.2.1` and the successful [publication workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38709119901) publish this version to the Marketplace. [Publication evidence](VSCODE-0.2.1-PUBLICATION.json) verifies public VSIX contents, ten assembly versions/revisions and isolated editor behavior. The preparation history below is preserved.

The runtime includes 1.5.0 used-package optional functions and 1.6.0 public optional overrides. Controlled editor validation still reports package-dependent signatures and unresolved external modules as coverage limits. Updating the bundle does not implement package acquisition or complete XSLT 4.0 editor validation. The .NET 10 runtime remains required.

## Verification

The initial isolated VSIX is built from unchanged engine/language-server source at `1b8209cdc8b65eacf5c8676a6a6e321ca6d10039` with candidate manifest/readme changes. Its ten bundled project versions are 1.6.0; DLL metadata identifies that exact source revision. The bundle is framework-dependent, launched with `dotnet`, without a platform apphost. Runtime client dependencies are included.

Extension compilation, TextMate highlighting and DLL/apphost/startup-registration checks pass. All **84 language-server tests** pass. The extracted VSIX server handles initialization and a hover request and passes three diagnostic protocol probes: valid keyword invocation, valid partial invocation and an invalid keyword reporting XPST0017. An isolated VS Code profile verifies a valid stylesheet without diagnostics, the invalid keyword error, and live correction clearing that error.

The first editor launch inherited `ELECTRON_RUN_AS_NODE` and exited before loading the application; its log is retained and excluded from acceptance. Removing that variable only for the isolated test process yields a successful editor run. The user's editor profile and installed extension are unchanged.

[Preparation evidence](VSCODE-0.2.1-PREPARATION.json) records source/manifest/archive hashes, bundled metadata and test/editor results. Engine and test trees match the published 1.6.0 source; existing frozen gates remain 31,142/0/679 and 8,278/0/1,894. Exact final merged-revision packaging and protocol/editor checks subsequently passed at `10433195569273474d328208dcf5c0cdd97caeaa`, followed by explicit publication approval. Public-package verification is complete; original preparation evidence remains unchanged.

## Adjacent completed and proposed work

[Braid compatibility](BRAID-1.6.0-COMPATIBILITY.md) passes 836 tests against public 1.6.0 in an isolated snapshot, preserving its production pin. The next engine slice is [scoped for abstract optional overrides](XSLT40-ABSTRACT-OPTIONAL-OVERRIDE-SCOPE.md), including static original-reference errors. That scope changes no engine code or version; overlapping families remain separate. REQ-126 stays In Progress.
