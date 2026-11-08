# Release preparation checkpoint - 8 November 2026

This checkpoint commits the reviewed editor URI fix and accumulated local package/XmlDb/extension qualification evidence on `checkpoint/foundation-1.81.1`. Parent evidence commit is `57865fc845c3eb081a38588cc4f73f5e11e42767`; the checkpoint commit is identified by Git history. The user authorized review, commit and push, with no tag or publication.

## Reviewed result

- [XmlDb follow-up](foundation-xmldb-manual-approval/README.md): the process-probe restriction is resolved. Integration862 full-run passes plus one layout-corrected focused pass complete863 distinct cases. With retained Storage3572 and XDM47 this is4,482 distinct passing cases across runs, preserving the original failure and approval history.
- [Release scope](RELEASE-READINESS-1.81.1.md): bounded Core packages plus the VS Code extension; production provider delivery is deferred from this scope, while applicable owner/workload/budget/C45 requirements still need explicit disposition.
- [Initial VSIX](release-core-vscode-candidate/README.md), [editor unblock](release-vscode-editor-unblock/README.md) and [failed command tests](release-vscode-command-smoke/README.md) remain historical and separately identified. Normal Chromium sandbox works through a reviewed isolated launch; no installation ACL or active profile change was needed.
- [Qualified URI correction](release-vscode-uri-fix/README.md): ExecuteCommandHandler parses command URIs consistently with synchronization. Six new regressions cover escaping and document isolation. Source and packaged server90 lanes, five real commands and five live diagnostics/correction checks pass. The build has zero errors/eight pre-existing warnings. All26,342 distinct solution cases pass across the main run and database environment retry47; the original45 listener failures are retained. No single wholly green initial solution run is claimed.

Core packages remain **1.81.1-preview.c0b926de**, from immutable sourceC0. Nine engine DLL hashes are unchanged. Fixed language server **1.81.2-preview.urifix1** is a separately hash-pinned modified diagnostic snapshot, not an immutable new source commit or final public version. The isolated extension candidate is0.2.6; tracked extension manifest/lock remain0.2.5. This evidence commit must not relabel any artifact with its new commit identity.

## Review and validation

[Screening report](RELEASE-CHECKPOINT-REVIEW-2026-11-08.json) records six successful evidence verifiers and targeted credential/archive/size checks. No strong credential patterns, loose DLL/PDB/EXE files, private owner archive paths or files at least50MiB were found. This is focused screening, not a general security audit. Proprietary owner source/binaries remain in ignored diagnostics.

One legacy-encoded test character was replaced with its equivalent C# Unicode escape; the test value is unchanged, UTF-8 decoding succeeds, and the checkpoint source test lane passes90. Original receipts and previous source hashes remain in their historical contexts. Manifested evidence is checked against staged Git blob bytes before commit. Six exact raw tool-output receipt paths retain their original whitespace with explicit Git whitespace attributes; no receipt bytes were rewritten. Existing large historical Git LFS artifacts are untouched.

Remaining work: applicable owner workload/baseline/budget and C45/API acceptance, final shipping versions/source identities, signing/provenance and final-artifact qualification; broader runtime/platform and interactive file-picker/cancellation coverage. NuGet and Marketplace publication need separate approval over concrete final artifacts. No external owner message, owner-repository write, tag or publication is included.
