# Count assertion scope checkpoint — 29 October 2026

Schema 0.7 repetition is reviewed and checkpointed: Schema `a462022a852575f84f0cc504f86771deaa118e0e`, core `26ae9b06be97a8860d9f4b5089d41d70a5bd831f`. This separate [count adoption](COUNT-ASSERTION-ADOPTION-2026-10-29.md), [implementation plan](COUNT-ASSERTION-PLAN.md) and [source locks](COUNT-ASSERTION-SOURCES.json) prepare a future assertion grammar extension. **Native count assertion admission is not implemented.** Owning editable records live in the Schema feature worktree; these are review snapshots.

## Proposed first boundary

Expose standard fn:count only through unprefixed `count(DeclaredChildName)` as a scalar operand in existing value comparisons/and/or. Count returns one integer, including zero for absence, without changing native validation or original state. Retain bare child names such as count; parse actual call syntax and track admitted features, not substrings. Explicit prefixed/aliased calls, nested/empty/multiple arguments, paths, predicates, arbitrary sequences, other aggregates and wider operators remain Unsupported. Existing input/token/depth bounds include function-call parentheses.

Proposed implementation target is Schema 0.8/profile v4 only when a count call is actually admitted. Existing no-count schemas retain v1/v2/v3. Current Schema 0.7/core 1.34.1 and source/pins remain unchanged. One current count-refusal test is explicitly scheduled for adopted capability reclassification, with original source/receipts retained before edits.

## Evidence and delivery

Twelve isolated observations record five current function-schema refusals, four actual host count/type/comparison controls on native 0/1/2/16-node documents, one bare-count child-name control and two native invalid/unsupported value outcomes despite successful host node count. These are diagnostics, not native count-schema acceptance. The reviewed source-identical baseline remains 334 Schema cases, 24,214 core cases and exact QT3 31,397/0/424 / qt4 8,941/0/1,918, 230 gated sets; no new full run is claimed for this scope.

The F&O adoption explicitly selects the dated Second Edition as revised 13 April 2015. Normative revision, exact retrieved representation hash, publication/correction notices and current retrieval/review date are separately recorded; raw specification pages remain ignored local reading material, not copied into the public repository. The other adopted dated XPath/XSD sources have separate witnesses.

RESULTS.json and CHECKPOINT.json record source-preservation checks and paired delivery resolvers/targets. Earlier sealed implementation/review/scope evidence remains unchanged. The user authorized the review/checkpoint/count-scoping recommendations; this documentation checkpoint does not imply implementation, merge, tag, package release, general API freeze or product adoption.

Next: implement the bounded count plan with before/after assessor tests, native validation-order checks and real copy/typing controls.

Count-scope Schema delivery: `2f4bf417131ba70e1653eda3d9148d83a6ef6d8a`, remote-verified on `origin/feature/bounded-document-assessment`, clean tree. Reviewed repetition implementation base: `a462022`. Core scope delivery resolver: `git log -1 --format=%H -- docs/schema-count-assertions/README.md`; target `origin/checkpoint/restart-2026-10-29`. Core review base: `26ae9b06`. No merge or release.
