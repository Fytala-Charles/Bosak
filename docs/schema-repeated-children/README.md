# Bounded repeated decimal scope — 29 October 2026

The optional-child implementation is reviewed and checkpointed: core `5e277c6a8233dc63af350c4485cc87609c132643`, Schema `773fd69faa9ff7a5bb27f1c4f8469f3024efc043`. This separate [adoption](REPEATED-DECIMAL-ADOPTION-2026-10-29.md) and [implementation/acceptance plan](REPEATED-DECIMAL-PLAN.md) prepares the next finite extension. **Repetition support is not implemented.** Owning editable records are in the Schema feature worktree; these are review snapshots.

## Proposed boundary

Local decimal minimum zero/one, finite maximum 1–16, and sum of maxima at most sixteen. Keep unique declaration names and one enclosing sequence, existing exact-value/resource bounds and the existing assertion grammar. Repeated instances are distinct states/nodes sharing the same original declaration/type. New repeated schemas would use v3/Schema 0.7, with existing v1/v2 preserved; current versions remain Schema 0.6/core 1.34.1.

Value comparisons with multiple atomized values raise XPTY0004. The future implementation must classify this supported cardinality error as assertion Invalid, without swallowing unrelated engine/CLR failures or changing logical evaluation. No functions/aggregation/general comparisons are admitted by this slice. Singular element export must refuse multiple matches instead of silently returning the first; host XPath over the whole document provides occurrence access.

## Scope evidence and delivery

Sixteen isolated observations record four current repeated-schema refusals and twelve real-engine controls across zero/one/two typed children. Multi-item comparisons produce the recorded anchored XPTY0004 exception; these are provider-built engine diagnostics, not native repeated-schema assessments. Current source matches the qualified optional candidate: 260 Schema/24,214 core passes, QT3 31,397/0/424 and qt4 8,941/0/1,918 (230 sets). No new full solution/gate run is claimed for this documentation/scratch scope.

Original [optional implementation evidence](../schema-optional-implementation/README.md), [review checkpoint](../schema-optional-review/README.md) and all preceding sealed dossiers remain unchanged. See RESULTS.json, SOURCES.json, CHECKPOINT.json and evidence. The user's recommendation-continuation authorizes the separate scope documentation checkpoint/push; it does not imply merge, package publication, API freeze or consumer adoption. Delivery resolvers avoid self-referential commit hashes.

Next: implement this bounded plan with before/after assessor controls and real copy/cardinality-error tests. Keep any future core semantic change separately adopted and fully gate-qualified.

Repeated-scope Schema delivery: `77c740ca5c86f9a98afbeacbe8949b0076da7abd`, remote-verified on `origin/feature/bounded-document-assessment` with a clean worktree. Its optional implementation base remains `773fd69`. Core scope delivery resolver: `git log -1 --format=%H -- docs/schema-repeated-children/README.md`; remote target `origin/checkpoint/restart-2026-10-29`. The optional implementation base is `5e277c6a`. No merge or release.
