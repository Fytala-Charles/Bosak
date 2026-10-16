# Record-subtyping milestone and release candidate validation

Completed 16 October 2026. Both approved engine PRs are integrated: [#350](https://github.com/Fytala-Charles/Bosak/pull/350) and [#351](https://github.com/Fytala-Charles/Bosak/pull/351), each after exact-head CI success. Engine work stops at two PRs. [Candidate notes](RELEASE-NOTES-1.16.5-CANDIDATE.md); [complete evidence and remaining-case inventory](RELEASE-1.16.5-CANDIDATE-VALIDATION.json).

## Validated candidate

Unpublished version **1.16.5** at exact revision `c95b56c0b18e4493c31f2285bbe98621c1027655`. PR 351 integrated it at `fc759cc25626eefe402a95bbd9296d70e876d465`; candidate and merge whole trees match `42a3f37c8bffb3389a4cbc3e3ec5c6c2874e3bd3`. The archives intentionally identify the reviewed PR head, not the merge revision. All archive/DLL/consumer provenance checks require that exact head revision; no receipt is relabeled as a merge build. The subsequent documentation checkpoint is not a repacked release candidate.

| Verification | Result |
|---|---|
| Source solution |9577 passed,0 failed,0 skipped; XSLT 6643 |
| Language server and CLI |84 tests and 10 runner-profile controls pass |
| Frozen gates |QT331142/0/679; qt4 gated 8278/0/1894, both exit 0 |
| Candidate packages |All ten net10.0 libraries packed; nuspec version/revision, dependency versions, XML docs, README, license, icon and included branding match source |
| Fresh broad consumer |153 checks pass, all ten library assembly versions 1.16.5.0 |
| Fresh public focused consumer |338 tests pass,0 failures/skips;141 prior public insertion checks plus 197 profile/audit/namespace/fallback/subtyping controls |
| Provenance |Package-only assets; source mapping confines Bosak packages to the local candidate feed; fresh independent caches; archive/cache/output DLL bytes, restore hashes and assembly informational versions verify |
| Dependency audit |Both consumer graphs queried NuGet advisories for top-level/transitive packages; no vulnerable packages reported |
| Read-only availability preflight |All ten 1.16.5 public package versions andv1.16.5 tag unused at check time; not a reservation |

Full source unit/gate runs were performed for both engine slices; exact-head CI completed separately. Consumers ran against the packaged PR 351 head while CI was running; whole-tree identity was checked again after integration. No public package fallback, source project reference, friend assembly identity or installed extension update was used.

## Preserved fixture correction

The first focused package consumer failed to compile because current internal unit fixtures inspect private module provenance and construct internal authoring descriptors. That attempt and cache remain preserved. A fresh public-only consumer uses the 141 previously published public insertion controls, unchanged current public tests, and a generated earlier-namespace fixture excluding only three internal-only cases: OwnedPrefixMetadata(3.0), OwnedPrefixMetadata(4.0), OwnedXml 11Metadata. All three remain in the passing engine suite. No engine assertion was weakened or hidden by a skip. The final public consumer uses a new cache and assembly identity; package provenance verifies it independently.

## Reassessment

Nine native record cases recover in both profiles:100,101,104,105,110,111,112,113,120. All other statuses/details remain unchanged. Draft selected array/record/type-alias totals are 219pass/36fail/22skip across 277 cases; this is not a full-suite XSLT 4 conformance result. Remaining failures: **33 catalog/draft drift and 3 unresolved contracts**. The JSON lists all 36 individually.

The unresolved contracts are array 025's missing jposition#1 and record 121/123's typed callable-record expectations. Their original failures remain visible. The pinned draft's callable-record key-enumeration contract and map-transitive function subtyping need reconciliation before broader typed-function behavior is changed. Historical 45-case assessment and all failed/intermediate attempts remain preserved; this checkpoint updates the current counts.

## Delivery boundary

Candidate engineering and package-consumer verification are complete within the declared scope. Public libraries remain 1.8.0; public VS Code 0.2.4 still bundles 1.8.0. Nothing was tagged, uploaded or published; Braid acceptance/adoption and REQ statuses are unchanged. No third engine PR is planned. Review this exact candidate and its release notes before any separate publication instruction. Upstream catalog contributions and specification clarification are independent follow-up work.

**Publication completed — 16 October 2026.** [Published release notes](RELEASE-NOTES-1.16.5.md) and [public-package verification](RELEASE-1.16.5-PUBLICATION.md) supersede the preparation status below. Earlier candidate revisions and executions remain historical evidence.
