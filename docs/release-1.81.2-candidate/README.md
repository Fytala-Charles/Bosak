# Local1.81.2 packages and VS Code0.2.6 candidate

8 November 2026. The user selected **Core/server1.81.2**, **extension0.2.6** and inclusion of **Bosak.XPath.Providers.Database as the tenth NuGet library**. The new local cohort is built and technically qualified. **Owner/workload/budget/API acceptance and publication remain open.** [Results](evidence/RESULTS.json) describe the exact scope and retained setup failures.

## Artifacts and identity

- [Ten NuGet packages](evidence/NUGet-PACKAGES-1.81.2.zip): the nine-library Bosak.Xslt closure plus the existing Database REST-loader package. This Database product is separate from the proprietary XmlDb production integration deferred from delivery.
- [VS Code0.2.6 VSIX](evidence/vscode-bosak-0.2.6-engine-1.81.2.vsix): matching1.81.2 language server and nine engine libraries, runtime dependencies, license/NOTICE and explicit Unicode license. The Database library is separately packaged and is not a VSIX runtime dependency.
- [Package hashes/assets/dependencies](evidence/PACKAGES.json) and [VSIX contents](evidence/VSIX-CONTENTS.json) bind every artifact to the actual built bytes. [The plan](PLAN.json) records immutable source checkpoint `e992d90b0efa01d45224771a9f8e53dee05096a6`, archive hashes and explicit Version/PackageVersion1.81.2 overrides. Source metadata in that commit remains1.81.1; tracked extension metadata remains0.2.5. The isolated extension manifest/lock are0.2.6. These build transformations are explicit, not relabeled preview artifacts or an unrecorded source commit.

## Fresh qualification

| Lane | Passing evidence |
|---|---|
| Fresh private-cache, local-feed package-only consumer | Seven bounded controls; all ten loaded DLL/package/cache/output hashes verified. No ProjectReference or HintPath engine substitution. |
| Retained unchanged foundation caller |20 through the required hash-qualified scoped launcher. |
| Source-built solution |26,342 distinct cases with exact prior case catalogs, across the initial run and focused XSLT fixture retry. |
| Language server |90, including six URI escaping/document-isolation regressions. |
| Unchanged native compatibility / adapter / public consumer binaries with package DLL overlays |451 /59 /10; these remain compatibility controls, not production adoption acceptance. |
| Frozen QT3 |31,397 passed /0 failed /424 skipped, exact prior skip map. |
| Gated qt4 |8,941 passed /0 failed /1,918 skipped, exact prior skip map;11 experimental passes retained separately. |
| API metadata |318 public types /6,465 declaration rows across11 products: ten packages and nonpackaged server. Only intended assembly-version identities change. |
| Installed VS Code |All five real commands and five live diagnostics/correction checks pass in a new disposable profile using the default bundled server and normal Chromium sandbox. |
| CLI fresh installation / upgrade from retained0.2.5 artifact |Both pass; installed bytes match the reviewed VSIX except VS Code-added package.json metadata. |

[Case/TRX receipts](evidence/TEST-RESULTS.json), [binding reports](evidence/bindings), [API comparison](evidence/API-COMPARISON.json), [gate summary](evidence/GATES.json), [installation receipts](evidence/INSTALLATION.json), [commands](evidence/COMMANDS.json) and [live diagnostics](evidence/EDITOR.json) retain original evidence. These are separate lanes; do not add repetitions as new distinct tests. Source-built engine hashes equal the new package DLLs and observed load hashes.

## Preserved setup failures

The first restricted build restored packages but could not create some MSBuild intermediate directories; a reviewed retry succeeded with zero errors and eight pre-existing warnings. No machine ACL was changed.

The first isolated solution run omitted documentation Content fixtures, producing25 XSLT failures and an incomplete5,173-case XSLT catalog. [The correction](FIXTURE-CORRECTION.json) supplies3,084 files from the same immutable source commit at their declared Content locations. No test assembly or engine body changed. The focused rerun passes all8,320 original XSLT cases; combined with the other nine green projects this is26,342 distinct passes. The initial failure is retained and is not a single green full-solution invocation. The server suite then passes90.

[Execution archive](evidence/EXECUTION-RECEIPTS.zip) retains successful and failed raw commands, watchdog receipts and log hashes; the earlier archive snapshot is retained separately. [Tool/cache provenance](evidence/TOOLS.json) records fresh offline npm ci, pinned cached vsce4.0.0 and the separately fresh local-only consumer cache. External build dependencies use existing offline caches. No vulnerability audit or reproducible-build guarantee is claimed.

## Still required before publication

Use the [release checklist](../RELEASE-READINESS-1.81.1.md) and [selected decisions](../release-final-candidate-plan/README.md). Scoped owner workloads, comparable baselines/numerical budgets, disposition of deferred provider requirements, C45/API acceptance and final signing/provenance/publication approval remain unfilled. No new1.81.2 owner performance measurements or XmlDb4,482-case replay occurred; earlier receipts keep their original cohort identity.

The existing release workflows still require reconciliation with the selected versions/package set, NOTICE inclusion, pinned packager and exact-artifact publication. Tracked production version files were not changed; a tag/manual dispatch of the stock workflow must not be used to publish this candidate implicitly. Representative interactive file-picker/cancellation and broader runtime/platform checks remain open. No owner repository, active editor profile, machine permission, tag or publication was changed.

Run `python docs/release-1.81.2-candidate/tools/verify.py` for read-only source, package/cache/load, case catalog, frozen gate/API, installed artifact and manifest verification.
