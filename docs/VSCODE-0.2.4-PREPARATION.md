<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala VS Code 0.2.4 candidate">
  <br><br><h1>VS Code 0.2.4 preparation</h1>
  <p>Released engine 1.8.0 alignment, 16 October 2026</p>
</div>

## VS Code 0.2.4 candidate - 16 October 2026

Unpublished extension 0.2.4 bundles released engine 1.8.0. Release build, 8,624 solution tests, 84 language-server tests, TypeScript/highlighting/launcher checks, packaged protocol diagnostics and isolated VS Code live correction pass. Ten bundled assemblies and archive payloads verify against source revision `915f344bb979e7f43c340c0c9d6c4310aa3f5162`; source/test trees match v1.8.0. The transitive brace-expansion lock is updated from 2.1.1 to 2.1.7; production dependency audit reports zero vulnerabilities.

See [preparation report](VSCODE-0.2.4-PREPARATION.md) and [immutable candidate evidence](VSCODE-0.2.4-PREPARATION.json). Marketplace remains at the previously verified 0.2.3/engine 1.7.1. Integration and exact merged-revision repackaging/verification precede separate publication approval. Installed extension and Braid files/pin are unchanged. REQ-128 engine delivery remains Implemented; Braid acceptance is independent. Package-dependent editor signatures and external module acquisition retain their existing limits; engine insertion APIs do not add an editor insertion UI.

The local reviewable VSIX is `D:/Development/Bosak/.guard-tmp/vscode-bosak-0.2.4-initial.vsix`; SHA-256: `fb261ac54d3b05ede6025ab75f3ff81467c492fafe5b1a6ae39fac18c3c5e04a`.

The initial manifest and dependency lock are working-tree changes atop the recorded source revision. All ten bundled project assemblies are 1.8.0.0 with that exact informational revision. Packaged server files match fresh framework-dependent publish output; runtime language-client files are present. Protocol checks verify initialization, a hover request and valid/partial/invalid-keyword diagnostics. Isolated VS Code 1.142.0 checks valid input, XPST0017 and live correction. The Release build has zero errors and 102 existing warnings. No engine or test source changed, so the published conformance receipts remain applicable; conformance gates were not rerun for this packaging slice.

PowerShell's npm script shim was blocked; npm.cmd was used. The initial restricted-network restore failed and was superseded by a successful network-enabled locked restore. The initial solution run lacked QT3 submodule fixtures for 51 runtime cases and sandbox local HTTP-listener access for database cases. Existing QT3 fixtures were copied into the isolated worktree; the runtime project then passed 269 cases and the database project passed 47 with local network access. Together with the eight unaffected passing projects, all 8,624 solution cases pass. Original logs remain preserved; these reruns supersede the two setup failures. The original dependency audit finding is closed by the bounded transitive update; the final production audit has zero findings.

Next: review and integrate the candidate, then rebuild and reverify the actual merged revision before requesting Marketplace publication approval. Do not create a release tag or dispatch the publish workflow during preparation. Braid adoption and acceptance remain separate.
