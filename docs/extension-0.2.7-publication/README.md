# VS Code extension 0.2.7 publication — 16 November 2026

The owner approved source publication, tags and extension delivery. Extension **0.2.7**, bundling unchanged engine/server **1.81.17**, is publicly available on the [Marketplace](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak).

Preparation [43872832340](https://github.com/Fytala-Charles/Bosak/actions/runs/43872832340) passed compilation, highlighting/launch controls and 90 language-server tests. Exact-artifact publisher [43873325768](https://github.com/Fytala-Charles/Bosak/actions/runs/43873325768) succeeded without rebuilding. Source/tag: `6e183a7563009e1ce52a162354c117da0beb13e2` / `vscode-v0.2.7`. Reviewed manifest SHA-256: `66a9b1e5c1a59fa99e450b60d03637ce37223091453f6f760b8faf468c758cc3`.

The public Marketplace version is listed and its downloaded VSIX is byte-identical to the approved hosted artifact. The downloaded server starts over stdio, reports the expected source/version, and passes three packaged diagnostic checks. Ten bundled Bosak assemblies have the intended 1.81.17 dependency cohort. The initial local legal-byte comparison encountered Windows checkout line endings; comparison with exact source Git blobs passed before publication. No validator or artifact bytes were weakened or changed.

Core tag `v1.81.17` points to the original NuGet source `6a7618e9dcf70eb8a67d5d1789ff7b787133b52d`. The separately recorded [NuGet publication](../release-1.81.17-publication/README.md) remains immutable. Extension tag preparation creates a second artifact for validation only; the publisher used preparation run 43872832340.

An initial direct URL returned HTML, and an early smoke attempt refused the missing VSIX before launching the server. Both observations are retained separately. A fresh query bypassed an older cached listing; the listed download was then verified before the successful bounded LSP-SMOKE-VERIFIED run. No engine or artifact changes were needed.

Existing editor, experimental 4.0, numeric, schema/provider and conformance profile limits remain. This engine refresh adds no editor UI and makes no performance or complete-conformance claim. Live VS Code UI interaction is not claimed by the packaged stdio checks.

JSON receipts and source-only EVIDENCE.zip retain validation; binary VSIX files stay in the ignored local packet. Earlier release status entries are historical.

The [GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.81.17) is public; its VSIX asset digest matches the approved artifact. A repository documentation encoding correction followed tagging; the immutable VSIX was not rebuilt.
