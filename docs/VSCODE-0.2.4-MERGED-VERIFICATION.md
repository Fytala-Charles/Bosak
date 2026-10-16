<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala VS Code 0.2.4 merged verification">
  <br><br><h1>VS Code 0.2.4 merged verification</h1>
  <p>Exact merged-revision release candidate, 16 October 2026</p>
</div>

**VS Code 0.2.4 ready for publication approval - 2026-10-16:** [PR #314](https://github.com/Fytala-Charles/Bosak/pull/314) is merged at `c1d4594dad2b030e104be068eb7e91fb10a9013f`. Exact merged-revision [CI](https://github.com/Fytala-Charles/Bosak/actions/runs/38858362612) passes 8,624 solution tests, 84 language-server tests and highlighting. The rebuilt VSIX verifies ten engine-1.8.0 assemblies at that revision, 286 payload files, packaged diagnostics and isolated VS Code live correction. Locked restore reports zero vulnerabilities. [Merged verification](VSCODE-0.2.4-MERGED-VERIFICATION.md) and [receipt](VSCODE-0.2.4-MERGED-VERIFICATION.json). Marketplace remains 0.2.3; 0.2.4 and its tag are unused. Publication requires separate approval. Installed extension and Braid files/pin are unchanged. Earlier preparation entries are historical.

## Candidate

Publish only the verified revision `c1d4594dad2b030e104be068eb7e91fb10a9013f` after explicit approval. VSIX: `D:/Development/Bosak/.guard-tmp/vscode-bosak-0.2.4-merged.vsix`. SHA-256: `16bb210e57a5355bccda21a5cddb15113d66ebb62fb807119b84b92b69011d39`.

All ten bundled assemblies are 1.8.0.0 with informational revision `1.8.0+c1d4594dad2b030e104be068eb7e91fb10a9013f`. Source/test trees match both the reviewed head and released engine v1.8.0. The initial working-manifest receipt is retained unchanged; this new receipt proves the merged build.

## Verification

Fresh framework-dependent publish, locked dependency restore, TypeScript compilation, highlighting/launcher checks and packaging pass. The complete 286-file runtime/client/grammar/asset/dependency payload matches staging; every server file matches fresh publish output. Runtime language-client dependencies are included, the DLL launch path is used and no Windows apphost is bundled. The transitive brace-expansion lock is 2.1.7; locked restore reports zero vulnerabilities.

Extracted-package protocol checks pass initialization, a hover request and valid, partial and unknown-keyword diagnostic cases. A fresh isolated VS Code profile verifies valid input has no diagnostics, an unknown keyword yields XPST0017 and live correction clears the error. The user's installed extension/settings are unchanged. Exact merged-revision CI confirms 8,624 solution tests plus 84 language-server tests and highlighting. No engine/test implementation changed; published conformance gate receipts remain applicable and were not rerun for this packaging slice.

Marketplace confirms public version 0.2.3 and no 0.2.4; the proposed vscode-v0.2.4 tag is unused. No tag or publication workflow was created. After approval, publish the verified revision once, then download and verify the public Marketplace VSIX against workflow bytes and repeat protocol/editor checks. Library publication, Braid adoption and additional editor insertion UI are separate work. Package-dependent editor signatures and external module acquisition retain their existing limits.
