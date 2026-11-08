# Pre-correction stock snapshot f0f3e9c8

This preserved source snapshot built ten stock1.81.2 packages, sealed their manifest, passed consumer7/retained20, source solution26,342/server90, frozen gates/API and native451/adapter59/consumer10. It is not the final VSIX cohort.

The clean checkout exposed70 legacy tracked files in `vscode-bosak/server`, contradicting the old workflow comment. An attempted fresh server staging correctly stopped because that directory already existed. No VSIX was built or published from this snapshot, and no old bundle was overwritten. [The tracking correction](../release-workflow-alignment/TRACKED-SERVER-CORRECTION.json) preserves local files/history and removes generated output from the index. Corrected source is `1d69d059c14888c1c43264efa8668909292af858`; [final-source qualification](../release-1.81.2-final/README.md) uses distinct paths/receipts and does not relabel these artifacts.

Original source worktree, package hashes, sealer output, binding reports and test receipts remain retained. Owner/API/budget/publication acceptance remains open.
