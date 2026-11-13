# Telemetry trigger correction - 13 November 2026

This correction supersedes the trigger statement in the original [sealed telemetry dossier](../xslt30-telemetry/README.md). The measurement evidence and its original seal remain unchanged.

GitHub PR path filters evaluate the full PR diff, so the documentation-only delivery commit `861eff1f` queued diagnostic run **43452915632**. It was cancelled during SDK setup; the qualification step was **skipped**, so no second .NET qualification was launched. The original cancellation metadata and logs are retained in [EVIDENCE.zip](EVIDENCE.zip). The original successful observed run43448130067 and required run43448128961 remain separate and unchanged.

The workflow now explicitly uses `pull_request.types: [opened]`. It permits one automatic observation when a qualifying PR opens; subsequent pushes, including observer-code changes, do not automatically launch another observation. Later attempts require explicit `workflow_dispatch` against a selected ref after the workflow is available on the default branch. No dispatch has been requested here. This policy makes repeated diagnostic attempts deliberate; it does not affect the required standalone workflow.

| Event | Diagnostic behavior |
| --- | --- |
| PR opened with a matching tool/workflow path | One automatic observation |
| PR synchronize, documentation or code changes | No automatic diagnostic workflow |
| Manual workflow dispatch | One explicitly requested observation |

[Controls](CONTROLS.json) prove that removing only the event-type line and explanatory comment exactly restores the qualified workflow, including every launcher/build/limit/artifact step. Observer Python code, engine and runner assemblies, required gate/workflow, corpus/exclusions, 1200-second/8 GiB child bounds and package versions remain unchanged. Existing source qualification and measurements remain valid for the unchanged observer; no fresh complete catalog is claimed for this event-selection correction. Ordinary required CI still runs for each PR head.

The [original verifier](../xslt30-telemetry/verify.py) still replays all sealed local/hosted evidence, complete successful catalogs, retained PR484 timeout, ten corruption refusals and hosted26422/90 tests. The [correction seal](MANIFEST.json) and [renderer receipt](DOCUMENTATION-CHECKS.json) cover this separate addendum. PR484 remains open/blocked without retry or merge. PR485 is ready for review; final-head normal checks are pending. No merge or publication has been performed.

Offline correction replay: `python docs/xslt30-telemetry-delivery/verify.py`.
