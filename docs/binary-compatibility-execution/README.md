# Unchanged-binary compatibility execution — 29 October 2026

The frozen baseline controls are green. Replacing only engine DLLs, without rebuilding consumers or changing hosts/runtime manifests, preserves the exercised XmlDb behavior and Braid's non-version-gated cases. A separate public-contract diagnostic confirms a Braid runtime-identity limitation: advertised inspector/preview identity remains at the original pinned revision while the loaded engine reports 1.34.1. These results do not qualify transparent product upgrades.

## Fresh results

| Frozen consumer | Baseline engine 1.26.2 | Engine-only replacement 1.34.1 |
|---|---|---|
| XmlDb original compiled Xdm tests | 27 pass / 0 fail | 27 pass / 0 fail |
| XmlDb unchanged compiled diagnostic | 44 observations | Same 44 observations, no difference |
| Braid original compiled full suite | 1,876 pass / 0 fail | 1,872 pass / 4 exact-version assertion failures |
| Braid unchanged diagnostic built against baseline APIs | Inspector/preview identity and capability version agree on old pin | Inspector/preview identity stays old pin; capability version reports new loaded engine |

All original test identities match baseline/replacement. Braid's four UTF-8/UTF-16 cases stop at their literal expected 1.26.2 identity, before their remaining behavior assertions; those bodies are not newly qualified by this run. No test expectation or compiled consumer was changed. The earlier source-rebuilt four-case follow-ups remain source evidence only. No other test failure or timeout was observed. The candidate Braid suite intentionally exits 1; this is not an all-green replacement suite.

## Frozen layout and binding proof

The [pre-execution layout manifests](evidence/layout-manifests.json) cover every file in all four isolated layouts: 613 files per XmlDb layout and 1,381 per Braid layout. The baseline/replacement file sets match; exactly fourteen XmlDb paths and thirty-six Braid paths differ, all allowlisted engine DLL copies. Every consumer DLL, test DLL, executable, .deps.json, .runtimeconfig.json, fixture and third-party file remains identical. Post-execution verification confirms no layout file was added, removed or changed by execution. Actual product checkouts, stores and dependency pins are untouched.

Seven engine assemblies are replaced for XmlDb; Braid's closure contains nine. A separately compiled metadata-only .NET startup hook records actual assembly loads in test hosts and spawned preview workers, with loaded path, assembly version and SHA-256. Baseline and replacement use identical instrumentation. Every observed Bosak assembly loads from the intended frozen layout and matches its recorded file hash. Replacement traces show 1.34.1.0, not fallback 1.26.2.0. [Load verification](evidence/assembly-load-verification.json) reports observed worker counts and engine names. All seven XmlDb engine assemblies are observed; eight Braid engine assemblies are observed. Bosak.XQuery is replaced but not observed loading, so its binary paths are not qualified by these cases.

No project build, restore, binding redirect, metadata rewrite or host-manifest adaptation occurs during these consumer runs. Tests use the installed compiled VSTest runner; XmlDb runs the previously compiled baseline Probe.dll. Only independent instrumentation and the additional identity diagnostic are compiled, with no Braid/core implementation edits. The identity diagnostic is compiled once against baseline public assemblies, then frozen and used unchanged with either engine closure.

All full commands have external 300-second process-tree watchdogs; probes have 90-second bounds. Exact commands, exits/timings, raw TRX, diagnostics and per-process load traces are preserved. The launcher initially refused to start before its observer build completed; no consumer command ran at that guard. Execution began after the observer build passed. Both diagnostic builds have zero warnings/errors.

## Braid identity finding

The unchanged public BosakSourceInspector.EngineIdentity and BosakPreviewExecutor.EngineIdentity both return `bosak:6a3372fc3b91e733b71a426cf40f26feaaeb3bcb` before and after replacement. BosakSourceInspector.Capabilities.EngineVersion changes from `1.26.2+6a3372fc...` to `1.34.1+2e33d3b7...`, and load traces independently confirm the candidate assembly bytes. Source inspection explains this: the advertised identity comes from the dependency revision stamped into the consumer assembly at build time.

This is an operational identity limitation under deliberate DLL replacement, not a demonstrated missing-member/layout failure or a defect in the currently pinned product workflow. The tests do not prove cache corruption occurred. It means binary execution success cannot be treated as valid runtime provenance for preview/history/export reuse. A real upgrade must either rebuild/review the consumer with the chosen pin or separately establish an explicit runtime identity/refusal policy, including parent/worker agreement and invalidation of stale retained results. No such product change is made here, and no new request identifier is invented.

## Limits and next step

Scope is Windows/.NET 10 and the exact inventoried assemblies/cases. No general ABI/layout promise, public-package adoption, native desktop/accessibility acceptance, dirty XmlDb storage work, full Storage/Integration suites, schema persistence, complete 4.0 semantics, C45 or release is inferred. All prior sealed dossiers remain unchanged. Core 1.34.1 and Schema 0.9 production source are unchanged; their 24,214 and 451 unit results and exact QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets) remain inherited, not freshly run.

Next: review and checkpoint this qualification with its scope. Before product adoption, resolve the Braid runtime-identity policy in a separately scoped consumer-owned change or retain the rebuild-and-reviewed-pin path. Current qualification/scope documentation remains uncommitted.
