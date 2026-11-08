# Editor command URI correction - 8 November 2026

The command handler now parses URI-valued command arguments with `DocumentUri.From(...).ToString()` before looking up the synchronized document. The raw URI lookup previously rejected equivalent Windows drive-letter escaping. The same document identity parser is already used by typed document synchronization. Missing and distinct documents still fail explicitly; no provider lifetime, public Core API or language semantic change is introduced.

## Scope and provenance

[Immutable specification evidence](SPECIFICATION-ADOPTION.json) was recorded before implementation. The production change is confined to `ExecuteCommandHandler.cs`, plus six regressions in `ExecuteCommandHandlerTests.cs`. [The plan](PLAN.json) pins the separately built diagnostic server: **1.81.2-preview.urifix1**, identifying the C0 base plus the modified source hash rather than pretending the dirty snapshot is an immutable source commit. The nine engine DLLs remain byte-identical to **1.81.1-preview.c0b926de**. The tracked extension stays0.2.5; isolated extension0.2.6 is unpublished.

The [new VSIX](vscode-bosak-0.2.6-uri-fix.vsix) contains the corrected server, original preview engines, dependencies, NOTICE and explicit Unicode license. It supersedes the original candidate only for this local qualification; original artifacts and [failed command receipts](../release-vscode-command-smoke/README.md) remain unchanged. Existing offline NuGet/npm caches and pinned cached vsce4.0.0 were reused; this is not a fresh-cache vulnerability audit or reproducible-build claim.

## Qualification

- Language-server unit tests:90 passed, including escaped Windows drive letters, spaces/reserved characters, Unicode and percent-escape isolation; missing/unknown argument controls remain.
- The same90 tests also passed with the newly packaged server and exact preview engine DLLs, using the [pinned layout](SERVER-TEST-LAYOUT.json). These are repeated lanes, not180 distinct cases.
- [All five actual registered commands](COMMANDS.json) passed on the installed new candidate: XPath6, XQuery q1/q2/q3 join, XQuery alias42, XSLT XML transformation23 and named initial-template output. Real fixture paths include spaces, Unicode and a percent sign; no launcher/client/engine mocking.
- [Five live diagnostic/correction checks](EDITOR.json) passed for XPath, XQuery and XSLT after the server rebuild. Both editor hosts use disposable profiles and the normal Chromium sandbox, and exit0. The active editor and installation ACL are unchanged.
- [Installation receipts](INSTALLATION.json) retain new fresh install and upgrade from the earlier retained0.2.5 Marketplace artifact. The reviewed VSIX is inventoried in [contents](VSIX-CONTENTS.json).
- Repository-wide build passed with zero errors and eight pre-existing warnings. **26,342 distinct solution cases passed across the main run and one database environment retry**; this is not a claim of a single all-green full solution invocation. [Results](RESULTS.json) and [hashed TRX counts](TEST-COUNTS.json) retain the aggregate and raw evidence. The initial restricted database run failed45 listener-fixture cases; reviewed rerun passed47/47. Both receipts remain, without counting duplicate passing cases twice.

Final release identity, scoped owner workload/budget/C45 acceptance, signing/provenance, broader platform/runtime and interactive file-picker/cancellation coverage remain open. This does not authorize publication or relabel old receipts. Engine conformance totals are inherited from the unchanged selected engine hashes; no new engine conformance run is claimed.

Run `python docs/release-vscode-uri-fix/tools/verify.py` for read-only artifact, source, test and editor evidence verification. No owner repository, credential, tag or publication changed.
