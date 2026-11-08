# VS Code editor execution blocker resolved

8 November 2026. **The installed candidate now passes real editor diagnostics and live correction.** [Result](RESULTS.json) and [editor report](EDITOR.json) retain the exact artifact identity and five successful checks.

The same VS Code installation started successfully after an explicitly reviewed launch outside Codex's restricted process context. Chromium's normal sandbox stayed enabled. No installation ACL, machine-wide error setting or active editor profile changed. The prior restricted launch failure remains in the original candidate packet; the successful retry does not establish that the installation itself needed repair.

The test used the final disposable fresh-install profile and its installed `fytala.vscode-bosak`0.2.6 payload with the default bundled language server. The harness refused a different extension version or a custom server override. Every installed non-manifest payload file was rechecked against the reviewed VSIX; package.json has only the previously recorded installer metadata difference. [Launch script](tools/launch.ps1) retains the exact isolated profile/extension arguments and clears inherited Electron CLI/IPC environment only during child launch. No `--no-sandbox` option is used.

Five checks passed: valid XSLT, invalid XSLT diagnostic, XSLT live correction, XPath invalid-to-valid live correction and XQuery invalid-to-valid live correction. The extension host exited0. Non-fatal crypto/deprecation/channel/trust-dialog messages remain in stderr; none prevented the tests. This is live editor coverage, not proof of every interactive command, operating system or runtime combination.

The [candidate packet](../release-core-vscode-candidate/README.md) retains VSIX contents, exact nine preview engine hashes, server84, protocol3 and disposable fresh/upgrade installation results. Its original blocked status is a historical snapshot; this follow-up resolves that execution blocker. [The release checklist](../RELEASE-READINESS-1.81.1.md) still requires remaining command/runtime and owner/release acceptance work. No source/version change, tag or publication occurred.

Run `python docs/release-vscode-editor-unblock/tools/verify.py` for read-only evidence and installed-payload checks. It does not launch an editor or alter permissions.
