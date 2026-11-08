# Corrected stock1.81.2 / extension0.2.6 qualification

8 November 2026. **The tracked release metadata and local stock artifacts are aligned and qualified. Nothing is published.** Source is immutable commit **`1d69d059c14888c1c43264efa8668909292af858`**, after the generated-bundle tracking correction. [Results](evidence/RESULTS.json) retain the exact scope and remaining acceptance boundaries.

## Reviewed local artifacts

- [Ten stock NuGet1.81.2 packages and their manifest](evidence/NUGet-1.81.2-reviewed.zip). The set includes the existing Database REST-loader library and excludes the nonpackable language server. [Package/cache/output/load hashes](evidence/PACKAGES.json).
- [VS Code0.2.6 VSIX](evidence/vsix-out/vscode-bosak-0.2.6.vsix), with the matching1.81.2 server/nine engine libraries, legal assets and no stale apphost. [Exact contents](evidence/VSIX-CONTENTS.json).
- [NuGet manifest](evidence/nuget-release-manifest.json), [VSIX manifest](evidence/vsix-out/release-manifest.json) and [successful exact-byte verification](evidence/SEAL-VERIFICATION.json). Both manifests were generated and verified by the committed release helper from the clean source worktree.

The build uses tracked1.81.2/0.2.6 defaults and normal NOTICE pack metadata: **no Version/PackageVersion override and no preview asset overlay**. The source worktree and source trees are pinned in [the plan](PLAN.json). Existing old preview/override artifacts keep their original identities.

## Qualification completed

| Check | Result |
|---|---|
| Stock solution build | Zero errors; eight pre-existing warnings. |
| Fresh private-cache package-only consumer | Seven bounded controls; ten-package local-only restore and exact load/cache/output provenance. |
| Retained unchanged foundation caller |20 through the mandatory hash-qualified scoped runner. |
| Full solution |**26,342/0 in one invocation**, with all case names matching the frozen prior catalog; no fixture retry in this cohort. |
| Language server |**90/0**, including six URI escaping/document-isolation regressions. |
| Unchanged native / adapter / public consumer binaries with new package overlays |451/0,59/0,10/0; compatibility controls, not production-provider adoption. |
| Frozen QT3 |31,397/0/424 with exact skip map. |
| Gated qt4 |8,941/0/1,918 with exact skip map;11 experimental passes separate. |
| Public API |318 types/6,465 declaration rows across ten packages and the server; no declaration or reference/friend delta beyond the intended version identities against the historical baseline. |
| Installed extension |Five actual commands and five live XPath/XQuery/XSLT diagnostic/correction checks pass. Fresh install and upgrade from retained0.2.5 pass. Installed bytes match the reviewed VSIX except VS Code-added package.json metadata. |
| Workflow/artifact controls |14 artifact/gate controls, valid plus nine refused source-run cases, parsed YAML/tool isolation, actual artifact validation and source/tag preflights. Hosted Actions execution is not claimed. |

[TRX/case receipts](evidence/TEST-RESULTS.json), [gate summary](evidence/GATES.json), [API comparison](evidence/API-COMPARISON.json), [binding reports](evidence/bindings), [execution archive](evidence/EXECUTION-RECEIPTS.zip), [installation](evidence/INSTALLATION.json), [commands](evidence/COMMANDS.json) and [live correction](evidence/EDITOR.json) are retained. The corpora were clean and matched committed gitlinks, recorded in [CORPORA.json](CORPORA.json). Native owner source and active editor profile were unchanged; Chromium's normal sandbox stayed enabled.

## Corrected legacy generated-output tracking

The first alignment snapshotf0f3e9c8 successfully qualified Core stock packages but exposed70 legacy tracked files under `vscode-bosak/server`. [The correction](../release-workflow-alignment/TRACKED-SERVER-CORRECTION.json) removed index entries only: all local files and Git history were preserved. The generated directory is now ignored and rebuilt from source. Tag/version preflight rejects tracked generated output. The [preceding snapshot](../release-1.81.2-stock/README.md) remains historical; its artifacts were not relabeled as this corrected cohort.

## Still open

[Release instructions](../RELEASING.md) now separate preparation from exact reviewed prior-run publication. Tags/default dispatch prepare only. This local run did not push, create a tag, dispatch Actions, exercise cross-run download or authenticate/publish to NuGet/Marketplace. A future hosted preparation produces its own manifest and artifact bytes; do not assume this local manifest matches them.

Owner workload/baseline/budget decisions, deferred requirement dispositions, C45/API acceptance, final signing/provenance and explicit publication approval remain open in the [release checklist](../RELEASE-READINESS-1.81.1.md). No new owner performance or XmlDb4,482-case qualification is claimed for these bytes. Broader runtime/platform and interactive file-picker/cancellation coverage remain separate. Existing external build caches and pinned cached packager were used; npm ci is fresh/offline and the consumer cache is separately fresh/local-only. No vulnerability audit or reproducible-build guarantee is claimed.

Run `python docs/release-1.81.2-final/tools/verify.py` for read-only source, manifest, package/cache/load, TRX/case, frozen gate/API and installed-editor checks. Do not use a historical cohort's live-source version preflight as approval of this new source.
