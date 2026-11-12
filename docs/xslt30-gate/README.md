# Pinned standalone XSLT 3.0 gate — 12 November 2026

This review proposes a CI gate for the exact basic/non-schema-aware Legacy/Xslt30 profile qualified in merged [PR #476](https://github.com/Fytala-Charles/Bosak/pull/476). The [baseline policy](BASELINE.json) pins `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56` and the SHA-256 of its sealed qualification archive. Historical evidence remains unchanged. This is operational adoption of an existing qualified profile, with no new engine semantics or specification interpretation. [Existing immutable semantic adoption](../xslt30-final-cases/ADOPTION.json).

The gate requires **10,221 passed / 0 failed / 4,380 excluded**, all 14,601 original case identities across 234 sets, and the original exclusion names and explanations. Exclusions remain exclusions; this is not a complete XSLT conformance claim. A change to admitted cases or exclusion policy needs separate review and a new baseline, rather than silently updating expected totals.

The [workflow](../../.github/workflows/xslt30-gate.yml) runs on Windows for main pushes and pull requests, with manual dispatch also available. It pins SDK 10.0.301 and uses the existing Windows qualification controller: 1,200-second watchdog, 8 GiB aggregate job-memory limit, process-scoped error-mode flags and process-tree termination. These bounds are operational controls, not performance acceptance. The check is named `Standalone XSLT 3.0 (pinned basic profile)`. Repository branch-protection/ruleset settings are not changed by this PR; administrators must select the new check if they want GitHub to block merges on it. Release publication recipes retain their existing acceptance requirements.

## Refusal and evidence

The wrapper checks the complete LF corpus inventory before launch, requires a clean committed source checkout, copies fixtures into an owned disposable packet, seals the runtime layout and records source commit identity. It executes the full catalog without filters, resumes or retry. Legacy exit zero is insufficient: complete per-case outcomes, exact totals and exact exclusion names/printed reasons must reconcile. Historical explanations for silent skips are retained only for identical case identities, following the qualified reporter's existing policy.

The final audit inventories every fixture file, including ignored generated files. Any added, removed or changed fixture, changed runtime input, wrong/missing/duplicate outcome, exclusion drift, timeout or incomplete controller receipt refuses success. Added/changed fixture bytes are retained in `GENERATED-OUTPUTS.zip`; original files are never restored or cleaned. `GATE.json`, `RUN.json`, manifests, full process logs, case ledger and diagnostic work files are uploaded on success or failure for 30 days. Every attempt requires a fresh packet path.

## Local execution

Use a fresh independent Git checkout of the exact corpus with `core.autocrlf=false`. Do not use the owner's edited corpus or a submodule junction. Build from a clean committed Bosak checkout:

```powershell
dotnet build tests/Bosak.Xslt.Conformance -c Release
python -m unittest discover -s tools -p test_xslt30_gate.py
python tools/xslt30_gate.py --corpus <clean-pinned-LF-checkout> --output <fresh-packet-path-outside-checkout>
```

Twelve Python controls cover accepted case-ledger replay, legacy false success, timeout, missing/duplicate outcomes, changed exclusion identity/reason, ignored output inventory, retained fixture overwrites and refusal to overwrite earlier receipts. One control replays every case from the sealed successful qualification. Negative controls are caught Python failures; they do not launch unhandled .NET negative cases.

Fresh end-to-end execution and hosted workflow results will be recorded before delivery. Core/server metadata stays 1.81.5, unpublished; public APIs, packages, frozen QT3/qt4 routing, engine code and all historical dossiers remain unchanged.
