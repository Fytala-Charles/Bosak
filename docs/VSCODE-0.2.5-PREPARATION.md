<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak extension preparation">
<h1>VS Code 0.2.5 preparation</h1>
<p>Released engine 1.16.5 alignment — 17 October 2026</p>
</div>

The extension manifest and lock file advance from 0.2.4 to 0.2.5. The server is rebuilt from source matching released v1.16.5; publishing uses the existing DLL launcher. No engine or diagnostic implementation changes are required.

TypeScript, TextMate highlighting, launcher checks, 84 language-server tests, three packaged protocol cases and isolated VS Code validation/live correction pass. Ten bundled project assemblies verify at 1.16.5.0 with exact source revision; the archive matches fresh publish output. Production dependency audit reports zero vulnerabilities. [Immutable initial receipt](VSCODE-0.2.5-PREPARATION.json).

The initial runtime test attempt lacked local QT3 fixtures and failed 51 tests; after attaching the existing fixtures, all 269 runtime tests pass without code changes. Original logs are preserved. Full solution validation and exact-head CI are recorded separately when complete.

Integrate this single preparation PR after green CI, rebuild the exact merged revision and verify the final VSIX before requesting publication approval. Marketplace remains at 0.2.4/engine 1.8.0. Installed extension, Braid adoption, upstream contributions and all REQ statuses remain unchanged. Experimental 4.0 support remains bounded; editor package signatures and external acquisition keep existing coverage limits.
