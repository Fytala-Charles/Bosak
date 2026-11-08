# C45 owner-review handoff — source 1.36.0

31 October 2026. This is a review-ready local handoff for already delegated owner work. No external checkout is edited, no issue/email/message is sent, no product pin changes and no acceptance is prefilled.

The Core source is **0126d492a8930001e0b947a0e27ae732af5a0653**, version **1.36.0**. Its [sealed qualification](../foundation-transport136/README.md) passes 24,267 unit tests, both exact gates, the unchanged caller 20/0 and native 121/atomic 376 controls. The two equality overrides are implemented and explicitly disclosed: 78 exported types, 1,201 declared members (previously 1,199), unchanged inherited signatures/base slots and 48-byte XdmValue. Exact tested source and seven DLL hashes are in [CANDIDATE.json](CANDIDATE.json). These source/build artifacts are not a published package/feed.

## Implemented foundation and open public contract

Private issued scalar transport, frozen compilation environment and operation/snapshot ownership are implemented. The proposed C45 public provider SDK is not exported; existing legacy typing members retain their supported scope. V5's 34 proposed public types, complete list/union/authority/derivation/affiliation/closure and compatible-universe contracts still require Core selection and joint review. Owners must not substitute private friend/reflection access for that contract.

The seven-assembly public reflection receipt is not the full eleven-assembly named freeze manifest and cannot silently replace the old 5,766-entry/176-mapping inventory. Core still owes that refreshed member-level manifest and exact public delta. The [Core decision queue](CORE-DECISIONS.md) makes those dependencies explicit.

## Refreshed owner inputs

[INPUTS.json](INPUTS.json) pins exact local documents, checkout revisions, committed-versus-working status and a fresh read of [issue #468](https://github.com/Fytala-Charles/Bosak/issues/468). Schema's requirements remain at c362499; the latest comment still requests a concrete public/authorized typed-consumer use case. XmlDb's current checkout is 280f7064b5ef with committed requirements; its matrix and prior qualification retain their original 2f9a36b/1.34.2 execution identities. The captured Braid queue matches d81f195. The uncommitted local XmlDb return is copied as identified historical input, without committing or modifying its original directory.

The [public bounded schema](fixtures/invoice.xsd), [instance](fixtures/invoice.xml) and [expected operations](fixtures/EXPECTED.json) are extracted from Core's already-qualified native integration probe. They supply an S009-02 integration starting point; this step does not execute a provider, select new grammar coverage or claim full XSD 1.1. Existing F-01–F-06 pins and previous XmlDb cases remain historical evidence for their original candidates.

## Owner actions and return criteria

| Role | Ready now | Needs selected public contract / exact cohort |
|---|---|---|
| [Schema](SCHEMA.md) | Review N1–N8; accept/refine the bounded integration fixture; identify missing public operations and required backend use cases. | Actual public provider binding, full required representation/capability acceptance and S009-02/04 disposition. |
| [XmlDb](XMLDB.md) | Reconcile FXR-01–15/FXG-01–06; refresh ordinary XML source/supported binary comparisons in isolated copies on the agreed source/build cohort. | Package checks once a package cohort exists; typed route if selected; joint member/profile sign-off. |
| [Braid](BRAID.md) | BF140-01–04 design: context, adapters, process route and currentness; preserve existing untyped profile. | BF140-05 actual typed/persistent previews and worker/lifetime qualification against accepted public contracts/providers. |
| [Core](CORE-DECISIONS.md) | Refresh the complete named inventory and propose the exact finite C45 delta and acceptance cases. | Adopt selected semantics with immutable spec evidence, qualify resulting artifacts/consumers and obtain four-role dispositions before freeze. |

[MATRIX.json](MATRIX.json) carries 43 explicit review rows: N1–N8, FXR-01–15, FXG-01–06, BF140-01–05 and PF-01–09. Required gaps remain open; no earlier requirement is declared satisfied by a scalar-only probe. All four roles must dispose the selected shared scope. The older Schema response's Braid-regression wording is superseded for foundation scope by the shared-owner BF140 registration; unrelated Braid D02–D07 rollout remains separate.

Return the [blank response template](OWNER-RETURN.template.json) with actual consumer revision, source/package/assembly and manifest identities, supported profile, per-row gaps and evidence, baseline differences, new regressions, budgets and a dated accountable disposition. Null requiredGaps means not returned, not an accepted empty gap list. Keep publication, product-pin adoption and freeze authorization explicit and separate.

## Verification and next step

Run `python -B docs/foundation-c45-handoff136/tools/verify.py`. It checks input snapshots, candidate/qualification pins, all 43 unique requirement IDs, unaccepted template defaults and the extracted fixture provenance. This documentation-only handoff adds no fresh runtime/consumer qualification and changes no production source. Prior 1.36 unit/gate receipts remain sealed.

Next: Core refreshes the full 1.36 named API inventory and final proposed C45 signature delta; owners proceed with the ready review/design/ordinary compatibility work in parallel. C45, real consumer qualification, full XSD 1.1, owner budgets, package publication and final API freeze remain pending.
