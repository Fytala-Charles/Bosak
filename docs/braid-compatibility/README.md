# Isolated Braid source compatibility qualification — 29 October 2026

Braid's actual committed source builds against current core 1.34.1 without a production fix. The **unchanged full suite runs 1,876 cases on both baseline and candidate: 1,872 pass and four fail at the exact engine-version assertion**. Separate four-case identity follow-ups pass on both. This is complete case coverage across distinct receipts, not a single green unchanged full-suite command.

## Source and qualification method

Braid source: `0f1d949e758645f7944f617f2b4613e49161a195`, clean at capture. Baseline engine: pinned 1.26.2 source `6a3372fc3b91e733b71a426cf40f26feaaeb3bcb`. Candidate engine: current 1.34.1 source `2e33d3b7c02eb20a1bfe757a73bb78ebb034c980`. Two ignored consumer archives and an ignored baseline-engine archive keep the actual checkout/dependency untouched. All **259 selected consumer source/project/configuration files** match committed bytes before the explicit follow-up test migration. Proprietary consumer source, credential and licensing fixtures are not copied into this dossier.

The generated qualification solution includes all five Braid product projects and four test projects. An isolated targets override chooses the baseline/current source root and replaces the Git-checkout dependency guard with separately recorded archive/source verification. Original project and dependency-props files are unchanged. Engine builds are Debug through project references, while consumer builds/tests are Release, as shown in raw logs. `MinVerSkip=true` avoids archive-parent version inference; no warning suppression or production version change is introduced.

## Exact outcomes

| Test project | Baseline original | Current-core original |
|---|---|---|
| Core | 27 pass | 27 pass |
| Application | 514 pass | 514 pass |
| Workbench (headless) | 370 pass | 370 pass |
| Infrastructure | 961 pass / 4 version-identity failures | 961 pass / 4 version-identity failures |
| Separate identity follow-up | 4 pass, corrected archive SourceRevisionId | 4 pass, explicit candidate-version test expectation |

The same 1,876 original test identities run in each version. Among them are 274 cases whose names contain Preview or Worker; this is a name inventory, not a separate additional suite. Existing authoring fidelity, validation, byte preservation, resource boundaries, real headless preview workers, worker deadlines/cancellation and export/reopen workflows execute within their original test scope. No other failure was observed.

The four failures are the UTF-8/UTF-16 variants of `ReadableCandidate_EncodingAndRepeat_ExactEvidence`, each stopping at its literal expected `1.26.2+6a3372fc...` identity before its behavior assertions. The baseline archive initially picked up the parent core workspace's Git suffix; the follow-up supplies exact baseline SourceRevisionId and passes with its original test expectation. Candidate identity correctly reports `1.34.1+2e33d3b7...`; only the isolated candidate test expectation is migrated to that identity, after retaining its original full-run failure. The rest of the case body and all production source stay unchanged. [Migration receipt](evidence/pin-expectation-migration.json) identifies the exact assertion. No pin change is made in the actual Braid checkout.

Both original full commands finish within external 300-second bounds; four-case follow-ups finish within external 120-second bounds. Raw process exits, TRX, logs, runtime dependency graphs and source identities are preserved. Original full commands intentionally retain exit code 1; both follow-ups exit 0. Final engine binary identities are recorded separately; initial archive-metadata binaries were rebuilt in the baseline follow-up and are not claimed as retained.

## Limits and next step

This is Windows source and automated behavior qualification, with headless Workbench and actual preview-worker coverage. It is not unchanged-binary substitution, public-package verification, consumer dependency adoption, manual/native desktop functional or accessibility acceptance, full schema-aware product support, complete XPath/XSLT conformance or C45 approval. Original Braid/core/Schema production source, dependency pins, releases and prior sealed dossiers remain unchanged. Core 24,214 passes and exact QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets), and Schema 451 passes remain inherited, not fresh engine/Schema runs.

Next: review and checkpoint the consumer-priority, XmlDb and Braid qualification documentation together. Then select a concrete native Schema structural use case or separately scope unchanged-binary qualification; these source results do not automatically authorize a product upgrade.

This dossier and the preceding consumer-priority/XmlDb documentation are uncommitted. No further production implementation is needed for the observed source-compatibility scope.
