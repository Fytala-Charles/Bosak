# Telemetry PR delivery refresh

PR #485 now incorporates merged main `a40951308950b36970f10badc1d13e46833ed1f7` and the qualified 1.81.9 wrapper-cache fix. Original history is preserved by a merge, without rebase or force-push. The telemetry tool, its tests and its diagnostic workflow are byte-identical to original head `7210b46900044bc61513e99d61e31cf742017eed`. The production engine, tests and required gate match current main. Seventeen telemetry controls and fourteen gate controls pass.

| Retained attempt | Result | Treatment |
|---|---|---|
| Original PR485 delivery, 43453704094 | Timed out at 1,200.235 seconds; last started date-020 | Failed and retained |
| Earlier delivery, 43452912032 | Timed out at 1,200.063 seconds; last started sx-treat-015 | Failed and retained |
| Qualified PR484, 43495980056 | 10,221 / 0 / 4,380 in 1,028.266 seconds | All 14,601 identities/exclusions replayed; all six checks passed before merge |
| Prior main, 43494882223 | 10,221 / 0 / 4,380 in 1,038.500 seconds | Complete independent artifact replay |
| Refreshed PR485 | Required normal qualification pending | Different engine payload from original 1.81.8 deliveries; same 1,200-second / 8 GiB limits |

The [original telemetry dossier](../xslt30-telemetry/README.md) and [trigger-correction dossier](../xslt30-telemetry-delivery/README.md) remain byte-for-byte sealed. The cancelled unintended diagnostic was stopped before qualification; it remains separate from both original successful captures and failed required attempts. The workflow still allows automatic observation only on PR opening, plus explicit manual dispatch. Synchronizing this already-open PR and merging main do not start a diagnostic capture. No workflow was dispatched or retried here.

These separate observations do not establish a whole-suite speedup, a failure rate or a root cause for prior timeouts. Skips remain exclusions, not passes. No new engine behavior, fixture, admission, gate budget, required workflow or ruleset change. Development Core/server 1.81.9 remains unpublished.

The [merge receipt](MERGE484.json), artifact/metadata pairs, [controls](CONTROLS.json), [manifest](MANIFEST.json) and [offline verifier](verify.py) preserve delivery provenance separately from original implementation/API adoption and fixture pins. The verifier replays both complete runs and both original failures, and checks the original sealed reports without executing .NET. See the [HTML receipt](DOCUMENTATION-CHECKS.json).

The earlier timing-refresh verifier checks a documentation-only working tree, so it correctly refuses this branch's added telemetry tool/workflow. Its sealed code and receipts remain unchanged. This verifier independently replays those same archived attempts and verifies their documentation-only boundary at original checkpoint `2e56414097c1426d1d9c8279b7e3b38c267b9a17`, while checking the current telemetry boundary separately. The [initial context refusal](CONTEXT-REFUSAL.json) is retained separately from qualification outcomes.
