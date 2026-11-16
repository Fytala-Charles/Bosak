# VS Code extension 0.3.0 publication — 16 November 2026

Extension **0.3.0** is published on the [Marketplace](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak), adding 20 snippets for XPath 3.1, XQuery 3.1 and XSLT 3.0. [User guide](../VSCODE_SNIPPETS.md).

All six exact-head PR checks passed before [PR #501](https://github.com/Fytala-Charles/Bosak/pull/501) merged at `046493f1d472968276f36c36d52d356630553718`. The original feature source and tag `vscode-v0.3.0` identify `1dbfbc34d4dd3c7844d399ac67aaf6a706c4301e`. Core/server remains 1.81.17; no engine source or public API changes, new NuGet packages or Core tag movement.

Hosted preparation [43883918663](https://github.com/Fytala-Charles/Bosak/actions/runs/43883918663) passed compilation, highlighting, launch/snippet controls and 111 language-server tests. The exact reviewed manifest SHA-256 is `32dcfde6954ad34048aec0f1a40166fd2e48a20ac4052cf2d56eb879ada0807d`. The VSIX is 4,810,616 bytes, SHA-256 `25216ff6067e585c3966bfec34901c173533678b6f027f286dc5219be0ca661e`.

An isolated VS Code 1.146.0 profile installed that hosted VSIX. All 295 installed payload files match, apart from installer package metadata. Five automated editor scenarios passed in both normal and server-unavailable profiles: registered XPath insertion/linked edit/forward-backward navigation/final cursor; XQuery dollar-prefixed mirrors; XSLT stylesheet insertion and final cursor; XSLT function parameter mirrors; actual prefix suggestion acceptance. The offline control moved only its isolated installed server directory. Owner profiles/extensions were untouched. This is actual editor-command automation, not human visual, accessibility or all-version acceptance.

Initial harness launches retained two setup failures: inherited ELECTRON_RUN_AS_NODE caused exit 9 before tests, then a case-sensitive drive-letter containment check refused the valid isolated installation. The corrected launcher changes only its child environment; normalized path containment retains workspace confinement. No product or VSIX changes were needed.

Publisher [43891666461](https://github.com/Fytala-Charles/Bosak/actions/runs/43891666461) verified and reused the same VSIX without rebuilding. A fresh Marketplace query lists 0.3.0 and the public download is byte-identical. The [extension GitHub release](https://github.com/Fytala-Charles/Bosak/releases/tag/vscode-v0.3.0) attaches that same VSIX; asset digest verified. Tag-triggered preparation is a separate validation artifact and was not used by the publisher.

Original local qualification remains in [the candidate dossier](../vscode-snippets/README.md): 26,488 solution tests plus 111 language-server tests, including 21 snippet expansions. Original fixture/test-expectation corrections remain unchanged there. This publication does not expand language, schema/provider or performance claims.

The immutable VSIX README retains its preparation-time development wording. Current repository documentation and the user guide reflect public delivery; the package was not rebuilt to change text. Raw VSIX files remain in the ignored local packet; JSON receipts and source-only EVIDENCE.zip retain verification. Run `python docs/extension-0.3.0-publication/verify.py` for offline evidence checks without launching the editor or publishing.

Fresh merged-main CI and Schema integration passed. The separate merged-main standalone run was in_progress at this delivery snapshot; original pre-merge exact-head standalone passed. These identities remain separate in the retained receipts.
