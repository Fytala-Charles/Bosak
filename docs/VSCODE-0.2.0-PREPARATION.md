<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak VS Code extension update">
  <br><br><h1>Bosak VS Code 0.2.0 candidate</h1>
  <p>Editor diagnostics for the released XSLT 4.0 function features</p>
</div>

## Scope

The extension candidate bundles the released Bosak 1.4.0 engine. XSLT diagnostics now use the controlled authoring validation boundary with the stylesheet's effective version, namespaces, variables and captured local function signatures. Optional/default argument calls and private no-namespace function calls no longer receive errors from standalone XPath 3.1 parsing. Errors retain their source ranges and codes; incomplete coverage is shown as a warning.

Validation observes the open principal document. Include/import resources are not acquired in this bounded editor integration: unresolved references produce warnings, and dependent checks remain incomplete. Package, conditional and other existing authoring coverage limits remain explicit. This does not establish complete XSLT 4.0 conformance.

The launcher runs DLL bundles using `dotnet` and apphosts as executables. The VSIX includes the runtime language-client dependencies. Fixed diagnostics and synchronization providers are advertised at startup; other early provider registrations wait for the client to run. Diagnostics refresh when editing, saving or opening tabs. A report converter scoped to Bosak's full reports works around the unimplemented converter in [OmniSharp 0.19.9](https://github.com/OmniSharp/csharp-language-server-protocol/blob/v0.19.9/src/Protocol/Features/Document/DiagnosticsFeature.cs). The release workflow builds a portable framework-dependent DLL bundle and runs extension and language-server checks before packaging. The .NET 10 runtime remains required.

## Verification

Seven new language-server diagnostic regressions cover optional keywords, private partial calls, earlier profiles, invalid keywords with source locations, unresolved imports, conditional coverage and malformed XML. They also verify report serialization. The language-server suite passes 84 tests. Extension compilation, existing TextMate highlighting checks and DLL/apphost/startup-registration checks pass. The solution build and all 8,298 solution tests pass. The packaged server passes initialization, hover and three diagnostic protocol checks. In isolated VS Code, the valid optional/private call is clear, the invalid keyword reports XPST0017, and correcting it clears the error during editing. [Preparation evidence](VSCODE-0.2.0-PREPARATION.json) records package and validation hashes.

Published NuGet libraries remain 1.4.0 and the Marketplace extension remains 0.1.6 until this candidate is published. This extension step changes no engine implementation, public engine API or conformance gate membership. The separately authorized next engine slice concerns optional/default arguments in used-package functions; optional xsl:override remains outside that initial slice.
