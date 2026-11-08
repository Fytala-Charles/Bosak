# Release planning and readiness

5 November 2026. Source candidate **1.72.0**; publication version/date are not selected. R2 remains ClosedWithScope; R3 is InProgress. [TA-28 public ordinary XQuery](../foundation-public-xquery/README.md) qualifies the reviewed captured-session entry with 25,936 tests, including 54 new controls. Public XSLT/BCAP, wider schema/structured XQuery and external owner/C45/freeze acceptance remain pending.

## Push, preview preparation and publication

The qualified cumulative changes remain uncommitted on `ec6daa0e538fe3c4e2ebc54177a1daeda5834b38`. Review/checkpoint them while preserving unrelated README and owner-return edits. Forty-one qualified/package inputs differ from HEAD. Package metadata covers eleven projects and the selected eight-library XQuery closure at 1.72.0; the explicit NOTICE overlay remains necessary. No package artifacts or push were performed.

A preview needs an immutable source/assets/recipe checkpoint, selected preview SemVer, artifact inspection and fresh-cache package-only consumers. Final publication also requires actual consumer/performance qualification and explicit C45/API-freeze/release dispositions. The previously recorded default-literal overload migration remains part of final SemVer and migration review. No reliable release date follows from passing internal or synthetic controls.

## Current gates

| Gate | Status | Remaining evidence |
|---|---|---|
| R0 private foundation | Qualified within bounded scope | Retain exact engine regression baseline. |
| R1 owner capability map | Audited | Existing source pins are evidence, not runtime owner acceptance. |
| R2 design disposition | ClosedWithScope | Seven original scoped returns remain historical; added public delta needs exact-candidate owner review. |
| R3 implementation | InProgress | Bounded ordinary/numeric/typed/node/construction/compatible routes qualified. TA-26 maps remaining requirements; AH-01/private bridge and AH-02/bounded ordinary XQuery are qualified; implement AH-03 public XSLT/BCAP and remaining broader query/provenance/numeric/assessment scope. |
| R4 actual consumers/performance | Pending; preparation parallel | Same immutable candidate, source/unchanged binaries, real Schema/XmlDb/Braid resources and representative agreed budgets. |
| R5 C45/API freeze | Pending R4 | Four-owner exact-manifest dispositions, including the implemented compatible delta and recorded source migration. |
| R6 packages/docs | Pending freeze; preview preparation parallel | Version, provenance/artifacts/migration/docs, fresh-cache package-only qualification. |
| R7 publication | Pending reviewed artifacts and release authorization | Publish/verify exact reviewed packages; product pin adoption remains separate. |

## Next concrete work

1. Implement **AH-03**, the public XSLT compiler/executable/invocation/BCAP capture and final-outcome route on the [qualified host bridge](../foundation-host-operation-bridge/README.md). The [current finite plan](../foundation-public-xquery/PLAN.json) retains PV-01/NU-01/SC-01, wider XQuery and all broader requirements. Ordinary public XQuery, compatible handoff/cache/CV01-CV32 stay qualified.
2. Prepare one immutable candidate for actual Schema, XmlDb and Braid source/binary/resource/workload qualification. Representative PF-01-09 targets and budgets remain unaccepted.
3. Follow the [preview recipe](../foundation-compatible-view-policy/packaging/RECIPE.md), select an exact checkpoint/version and inspect actual artifacts/package-only consumers. No release SemVer/date is inferred from the development version.
4. Obtain explicit R5 dispositions, then concrete artifact/release approval and publication receipts.

[GATES.json](GATES.json), [dependency plan](../foundation-r2-consolidation/PLAN.json), [owner capability map](OWNER-CAPABILITIES.md), [owner actions](../foundation-r2-consolidation/OWNER-ACTIONS.md) and [original inputs](INPUTS.json) retain their scopes. Historical engineering checkpoints remain in [development history](../DEVELOPMENT_HISTORY.md) and sealed dossiers; the pre-TA-14 overview remains [archived](inputs/README-before-ta14.txt).

Run `python -B docs/release-planning/tools/verify.py` for the current candidate and gate proof. No actual native/owner/budget acceptance, API freeze or publication is claimed.

TA-26 is an audit over unchanged 1.70.0, not a fresh engine test run: 40/47 proposed types present and six numeric member intents declared. Seven missing types and partial behavioral scope remain explicit. Thirteen audit controls pass; prior exact 25,831-test qualification is retained.

TA-27 refresh: unchanged public declarations/value layout and exact conformance/skip maps; the explicit friend/reference delta activates the existing XQuery/API project dependency internally. All seven missing public types remain absent, so private host success does not close R3. No package artifacts or fresh package-only consumers exist yet.

TA-28 refresh: the sole public addition is SchemaXQueryCompiler plus Compile; existing metadata/friends/references/value layouts and conformance skips remain unchanged. Forty-one of 47 proposed types are present. The [preview scope](../foundation-public-xquery/PACKAGE-SCOPE.json) now starts at Bosak.XQuery and contains eight libraries; the earlier seven-only recipe must not omit the new entry package. No artifacts or package-only consumers have been qualified.


Current source **1.73.0**: [TA-29 admission outcome prerequisite](../foundation-xslt-admission/README.md), **25,948/0** tests and 12 new controls; public API and frozen gates unchanged. AH-03 remains open. The eight-library preview closure is unchanged; packages, native owner/budget acceptance, API freeze and publication remain pending.


Current source **1.74.0** (6 November 2026): [TA-30 completion routing](../foundation-xslt-completion/README.md), **25,964/0** tests / 16 new controls; public API and frozen gates unchanged. AH-03 public XSLT remains open. Preview metadata retains the eight-library closure; native owner/budget acceptance, API freeze, artifacts and publication remain pending.


Current source **1.75.0** (6 November 2026): [TA-31 API host outcome routing](../foundation-host-outcome/README.md), **25,987/0** tests / 23 new controls; public API and frozen gates unchanged. AH-03 public XSLT remains open. The selected preview closure remains eight libraries; artifacts, actual owner/budget acceptance, API freeze and publication remain pending.


Current source **1.76.0** (6 November 2026): [TA-32 public ordinary XSLT/BCAP](../foundation-public-xslt/README.md), **26,054/0** tests / 67 new controls. Five reviewed public types are selected; existing API/layout/friends/references and frozen gates remain unchanged. AH-03 is complete within its ordinary profile; next PV-01. Proposed type presence is 46/47. Nine-library XSLT preview metadata is evaluated; artifacts, actual owner/native/budget acceptance, API freeze and publication remain pending. The original C01-C10 NotRun snapshot remains historical; current scoped evidence is in [TA-32 follow-up mapping](../foundation-public-xslt/FOLLOWUP-MAPPING.json).


6 November 2026: [TA-33 provenance policy](../foundation-provenance-policy/README.md) selects 22 Core decisions / 37 planned cases; 475 existing characterization tests pass on unchanged 1.76.0. TA-32 full/API/gate/package receipts remain retained and hash-verified. No helper or new public type is implemented yet. PV-H01 is next; required leased compatible observation, general list/union transport and actual owner/release gates remain open.


Current source **1.77.0** (6 November 2026): [TA-34 bounded SchemaAtomics](../foundation-public-provenance/README.md), **26,118/0** tests / 64 new controls, one public type/four rows with existing layout/friends/references and frozen gates unchanged. All 47 proposed type names are present; this does not close member/semantic gaps or freeze acceptance. PV-H01 selected scope is implemented; next PV-H02, with general container/legacy/native/owner requirements still unwaived. Nine-library preview metadata is refreshed; no artifacts or publication.


TA-35 (6 November 2026): development 1.78.0 qualifies bounded leased compatible provenance, 26,149/0 tests and 31 new controls. [Current checkpoint](../foundation-leased-provenance/README.md). Next PV-T01 general container policy/implementation; PV-01, actual owner/native budgets and R4-R7 remain open. No release version/date or package publication is selected.


TA-36 (6 November 2026) selects complete-container policy on unchanged 1.78.0: 16 decisions/32 planned cases, fresh364 existing controls. [Policy checkpoint](../foundation-container-policy/README.md). Next PV-T02 ordinary implementation then required PV-T03 compatible transport. No owner/freeze/publication acceptance.


TA-37 (6 November 2026) qualifies ordinary complete-container transport at development1.79.0: 26,193/0 tests, 44 new controls. [Current checkpoint](../foundation-public-container/README.md). Next PV-T03 compatible transport; wider scope and actual owner/native/budget/R4-R7 acceptance remain open. No package, commit, push or publication.


TA-38 (6 November 2026) qualifies compatible complete containers at development1.80.0:26,236/0 tests,43 new controls and87 focused cases. [Current checkpoint](../foundation-compatible-container/README.md). Next PV-T04 wider primitive payload policy/disposition; actual owner/native/budget and R4-R7 acceptance remain open. No package, commit, push or publication.


TA-39 (6 November 2026) selects wider primitive transport policy on unchanged1.80.0:18 decisions/36 planned cases,451 existing characterization controls. [Policy checkpoint](../foundation-primitive-policy/README.md). Next PV-T05 eleven-public-factory-kind implementation; enum-only representations and owner/native/budget/R4-R7 requirements remain open. No runtime activation, package, commit, push or publication.


TA-40 (6 November 2026) qualifies eleven-kind complete containers at development1.81.0:26,338/0 tests,102 new cases/189 focused. [Current checkpoint](../foundation-public-primitives/README.md). Next PV-T06 enum-only representation disposition; remaining legacy/native/owner/budget/R4-R7 scope is unwaived. No package, commit, push or publication.


TA-41 (6 November 2026) completes Core URI/binary representation disposition with scope on unchanged1.81.0:12 decisions,553 existing characterization controls. [Disposition checkpoint](../foundation-uri-binary-disposition/README.md). Retain annotated String carriers and enum identities; no new factory/activation. Next PV-L01 legacy disposition. Owner/native/budget/full-semantic/R4-R7 acceptance remains open. No package, commit, push or publication.


TA-42 (6 November 2026) completes private legacy Core disposition with scope on unchanged1.81.0:12 decisions,30 Core+259 Runtime existing controls. [Disposition checkpoint](../foundation-legacy-disposition/README.md). Public migration requires provider re-issuance;no opaque-field cast or marker promotion. Next PV-Q01 actual-provider/performance qualification and gap audit. Owner/native/migration/budget/R4-R7 acceptance remains open. No package,commit,push or publication.


TA-43 (6 November 2026) prepares provider/performance qualification on unchanged1.81.0: four read-only owner snapshots, nine PF budget gaps, 18 synthetic receipt controls and expected template refusal. [Readiness packet](../foundation-provider-qualification/README.md). Core26,338 qualification is retained; no actual native/owner benchmark or accepted budget. Current preview metadata is the nine-library Bosak.Xslt closure. Next PV-Q02 native lane artifact preflight. R3/R4-R7 and owner/freeze/publication requirements remain open.


TA-44 (6 November 2026) passes451 native cases in each frozen-baseline,isolated-source and unchanged-binary lane;451 distinct identities,not additive. [Native lane checkpoint](../foundation-native-schema-lane/README.md). Only nine engine DLLs differ in binary replacement;all loaded hashes match. Core1.81.0 and production sources remain unchanged. Next PV-Q03 actual public-provider adapter feasibility. Owner/performance/new-public-binding/R3/R4-R7 acceptance remains open;no package,commit,push or publication.


TA-45 records [native public adapter feasibility](../foundation-schema-adapter-feasibility/README.md): ten source-pinned boundaries and twelve planned controls; no adapter or owner/budget qualification. Next PV-Q03a is an isolated bounded provider prototype.


TA-46 [native decimal container prototype](../foundation-native-container-prototype/README.md) passes19 public transport controls. Whole-document assessment/node facts remain unsupported; next PV-Q03b. Owner/budget/release acceptance remains pending.


TA-47 [native original-node prototype](../foundation-native-node-prototype/README.md) passes34 controls. Public strict assessment remains unsupported; next PV-Q03c. Owner/budget/release acceptance remains pending.


TA-48 [strict native request prototype](../foundation-native-assessment-prototype/README.md) passes59 controls. Provider-internal capture/output helpers do not close generic public consumer handoff. Next PV-Q03d;owner/workload/budget/release acceptance remains pending.


TA-49 [public consumer handoff](../foundation-native-consumer-handoff/README.md) passes10 separate-assembly controls and records a100-cycle candidate-only pilot. No accepted owner budgets/baseline ratios. Next PV-Q04;production adoption/freeze/release remain pending.


TA-49 pilot finding:about31.5 GB allocated/cycle,localized by a separate phase probe to public data evaluation. Prioritize PV-Q04a allocation attribution/correction before owner-scale measurement or package preparation.


TA-50 [immutable output guard](../foundation-native-immutable-guard/README.md) corrects prototype allocation churn with59 adapter/10 consumer controls passing. Next PV-Q04b;owner workloads/budgets/production adoption remain pending.


TA-51 [owner workload protocol](../foundation-owner-workload-protocol/README.md) records6 independent measurements and14 comparer controls. No comparable baseline/accepted owner budgets. Next PV-Q04c CPU attribution and owner evidence;release gates remain pending.


TA-52 [callback attribution](../foundation-public-data-callback-attribution/README.md) records nested check amplification with59 instrumented regressions passing. No Core checks removed. Next PV-Q04d policy/isolated engine candidate;owner qualification/release gates remain open.


TA-53 [isolated captured-identity candidate](../foundation-captured-identity-candidate/README.md) has bounded Runtime/native/consumer validation and18% check-count reduction. Production unchanged;next PV-Q04e full exact-cohort/gate/API-ABI qualification before promotion.


TA-54 [exact candidate qualification](../foundation-captured-identity-qualification/README.md) passes26342 tests,frozen gates/API metadata and20 retained cases. Candidate remains unpromoted;next PV-Q04f production patch/versioned rebuild and fresh qualification. Owner/release gates remain pending.


TA-55 [development patch1.81.1](../foundation-captured-identity-production/README.md) is adopted and freshly qualified. Live source/binary checks now use1.81.1;earlier receipts stay historical. Next PV-Q05 owner workload/budget/adoption gaps;release obligations remain open.
