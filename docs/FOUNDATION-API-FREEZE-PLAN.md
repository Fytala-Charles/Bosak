# REQ-140 — Shared foundation and versioned API freeze

Registered: 30 October 2026. Requester: repository owner/user. Accountable owner: **Bosak maintainers**, with required **Bosak.Schema and Bosak.XmlDb owner sign-off**. Status: **Pending contract/implementation/freeze qualification**. The strategic goal is explicitly selected; no existing proposed signature or API freeze is accepted by this registration.

Finite C139-01/C45 review — 30 October 2026: [contract decision matrix](foundation-contract-decision/README.md) partitions all 32 prototype types / 377 reflected entries into six review groups. Selected committed owner inputs are pinned read-only: public Schema descriptor work has advanced, while core session integration and full structural validation remain incomplete; XmlDb provider qualification remains proposed. No returned candidate acceptance appears in the selected records. Exact production signatures, immutable semantic adoption and bounded implementation scope remain Pending; C45, API freeze and publication remain unaccepted. No product change or new runtime test run. Delivery: `git log -1 --format=%H -- docs/foundation-contract-decision/README.md`.

Proposed C139-01 implementation ordering — 30 October 2026: [requirements-to-scope review](foundation-implementation-scope/README.md) maps twelve registered needs to existing XML contracts and missing optional Schema authority/session boundaries. No new required XmlDb provider member is demonstrated. Annotation/authority and real provider facts precede exact session signatures and routing; atomic/list/union, compatible views and lifetime remain required. Accepted signatures remain empty; C139-01, C45 and freeze stay Pending. No source or package change. Delivery: `git log -1 --format=%H -- docs/foundation-implementation-scope/README.md`.

C139-01 owner reconciliation — 31 October 2026: [concrete owner-return resolutions](foundation-owner-reconciliation/README.md) acknowledge Schema D1–D7/N1–N8 and XmlDb FXR-01–15 delivery, answer four Schema questions and propose dispositions for FXG-01–06. Newer bounded Windows/Linux FX009-02 evidence also passes its packet verifier: same 27 cases/37 observations across four modes per platform; exact package/binary bindings and owned-store invariants checked. Historical missing-return status is superseded. Final signatures, semantic adoption, C45 implementation disposition and joint freeze remain pending. No product change or fresh runtime run. Delivery: `git log -1 --format=%H -- docs/foundation-owner-reconciliation/README.md`.

## Selected outcome

Bosak provides the common query/XDM/provider and schema-integration foundation needed by **both Bosak.Schema and Bosak.XmlDb**, then publishes a qualified versioned stable foundation API. The freeze is a named public contract/version, not a promise that every experimental or future Bosak API can never evolve.

Bosak.Schema remains the implementation owner for full XSD 1.1 validation. Its current bounded 0.9/v5 assessment is not full support. Bosak must expose sufficient neutral contracts for that full goal, without taking a dependency on the schema implementation. Bosak.XmlDb is a required foundation consumer in its own right, including ordinary untyped persistent XML; it is not merely a schema regression stakeholder and does not need schema storage to qualify that baseline.

Keep core independent of both concrete providers and private licensing. Schema depends on the core foundation; XmlDb's Xdm adapter depends on Core/Storage and Bosak's XDM contract, while storage metadata/formats retain their own boundaries. Braid is a required foundation design/integration consumer. Its adapters and worker protocol may change to support the selected Schema/XmlDb foundation; its unrelated D02/native/product release gates do not block this work. See [BF140-01–05](braid-foundation-requirements/README.md).

## Performance and API stability intent

The owner intends this to be the final breaking change to the named stable shared foundation. [PF-01–09](foundation-performance/README.md) makes upfront performance design and final-candidate measurement mandatory for Core, Schema, XmlDb and Braid. Resolve known batching/dispatch, allocation/value-carrier, streaming, semantic-cache, persistent-read, snapshot/lease and worker/serialization needs before signatures are frozen. Budgets, representative baseline/target workloads and explicit owner dispositions are required; no numeric performance or future-change guarantee is inferred. Compatible implementation optimizations remain possible after freeze; any unavoidable later break requires explicit major-version/migration review.

## Coordinated owner queue

| Package | Deliverable | Readiness / dependency |
|---|---|---|
| F140-01 Joint contract and scope inventory | One mapping from full-XSD-1.1 Schema, real XmlDb provider and Braid integration requirements to existing/needed public signatures, invariants, errors, ownership and versioning. Identify the exact freeze surface and all unresolved contract gaps. | Ready now with Schema, XmlDb and Braid owners. Reuse [REQ-139 C139-01](REQ-139-OWNER-WORK-PACKAGES.md); the C45 decision is still required for its permanent schema shape. |
| F140-02 Missing foundation implementation | Implement only explicitly accepted gaps through REQ-139 and any individually registered XmlDb findings. Preserve no-provider/legacy behavior, XML kind numeric values, source/snapshot identity, original PSVI, lazy/container ownership and existing contracts. | After the corresponding owner scope/adoption decisions. Never make plain XmlDb nodes implement schema contracts merely to satisfy a core default. |
| F140-03 Required three-consumer qualification | Build and execute actual Schema, XmlDb and Braid consumers against one exact candidate/package set. Cover source/API and unchanged-binary replacement, behavior, layout, provenance, copies, nested/deferred values, disposal/cancellation and supported concurrency. Include Braid typed/persistent inputs, semantic environment, worker ownership and fingerprint/currentness evidence. Preserve existing Braid regression evidence as a separate baseline. | Fixtures and audits ready now; final execution follows the accepted candidate. Required contract gaps in any consumer keep freeze pending. |
| F140-04 Publish and freeze foundation v1 | Deliver qualified immutable core source/packages and a reviewed public-surface manifest; record explicit Core, Schema, XmlDb and Braid owner disposition for the named stable scope/version and its compatibility policy. | Only after F140-01–03, F140-05 and fresh engine/gate qualification. Publication and freeze require their own explicit release/disposition; registration is not either action. |
| F140-05 Performance and API longevity qualification | Execute [PF-01–09](foundation-performance/README.md) on actual representative consumer workloads; compare equivalent baselines/candidate profiles, choose measured budgets and resolve performance-driven signature needs before freeze. | Design/fixture/baseline preparation ready now; exact final candidate reports and all affected owner dispositions required before F140-04. All scenarios currently NotRun. |

## Required consumer evidence

**Schema:** map every known foundation need for full XSD 1.1, including complete component/universe identity, derivation/affiliation, typed atomic/list/union transport, assertions/type alternatives, original assessment/PSVI and compatible views. Exercise representative actual provider operations and positive/refusal/invalid/error/copy/lifetime cases. Track backend coverage separately; prototype-only or decimal-only success cannot prove the full goal. Any unresolved required API need blocks freeze. Full validator conformance and final Schema release remain separately evidenced outcomes.

**Braid:** execute [BF140-01–05](braid-foundation-requirements/README.md): map schema-aware/persistent input needs and semantic capture, design explicit worker/resource ownership and versioned fingerprint/currentness, then qualify representative actual Schema and XmlDb previews against the shared candidate. Existing adapter shape is changeable. Preserve supported behavior and engine-free Application/Core boundaries. Required Braid contract gaps block freeze; unrelated host trust/native release gates do not.

**XmlDb:** retain and qualify its explicit XML-only provider baseline: IXdmNode/sequence contracts, exact XML enum/storage mappings, snapshot-relative identity/order, atomization/errors, axes/namespaces, composite results retaining nodes, deferred enumeration, cancellation and reader/generation lease disposal. Reopen existing supported stores in owned test fixtures without migration or format rewrites. Existing formats, release/stop ownership, CanReclaim=false and active host/recovery work are untouched. Persistent JSON/general XDM/schema annotations remain separate requests.

The [existing consumer matrix](compatibility/xdm40-2026-10-29/CONSUMER-MATRIX.md) and current [REQ-139 review](REQ-139-OWNER-WORK-PACKAGES.md) are inputs, not a replacement for execution. Refresh exact consumer revisions and requirements before implementation; historical source inspections do not certify current APIs or the moving XDM draft.

## Freeze acceptance checklist

- [ ] Agree representative workload targets/budgets and qualify PF-01–09 for the exact candidate/profile, including default-path regression, allocations/retained memory, streaming/read amplification, lease/cancellation boundaries and Braid worker costs. Record all four owner dispositions; unresolved required performance-driven contract needs block freeze.
- [ ] Review the full required functional/performance evolution against the final signature manifest, recording how future backend implementation can proceed without required-member/layout/identity breaks. Final-breaking-change intent is recorded; no absolute future guarantee is claimed.

- [ ] Enumerate exact assemblies, public types/members, required interface members, overload/default behavior, enum values, managed layout and error/ownership/resource contracts in an immutable manifest.
- [ ] Core and all three consumer owners agree the known required foundation needs are represented; no unresolved required gap is hidden as an implicit future breaking change.
- [ ] Real Schema, XmlDb and Braid source/package/unchanged-binary qualification passes on the same candidate; preserve exact outputs/refusals and any supported-profile exclusions. For intentionally revised Braid adapters, record the API/protocol delta and applicable binary support rather than requiring every new operation to work through the old adapter. Unrelated native/trust rollout remains separate.
- [ ] Full core solution and exact normal gates pass: QT3 31,397/0/424, qt4 8,941/0/1,918 over 230 gated sets, with retained skip names/reasons unchanged unless separately adopted.
- [ ] Publish exact artifact/source/API identities under explicit release authorization, then record the named freeze disposition with all four owner sign-offs (Core, Schema, XmlDb and Braid). The [freeze record template](FOUNDATION-API-FREEZE.template.json) starts Pending.
- [ ] Define backward compatibility: reviewed nonbreaking additions may evolve through minor releases; required-member/layout/semantic breaks need an explicit migration/deprecation and major-version decision. Label experimental surfaces outside the freeze manifest.

Freeze does not require every unrelated product feature, M2 storage/host workflow or Braid native acceptance to finish. It does require the foundation contracts those consumers actually need to be agreed and qualified. Do not label all future XSD 1.1 or XDM behavior complete merely because an interface exists.

## Owner handoff and scope control

Schema [REQ-009/S009-01–04](https://github.com/Fytala-Charles/Bosak.Schema/blob/feature/bounded-document-assessment/docs/REQ-009-OWNER-WORK-PACKAGES.md) owns backend coverage and foundation acceptance. XmlDb [REQ-009/FX009-01–03](https://github.com/Fytala-Charles/Bosak.XmlDb/blob/docs/shared-foundation-api-freeze/docs/BOSAK-FOUNDATION-OWNER-WORK.md) owns its persistent-provider qualification and sign-off; optional schema adapter stays separate REQ-008. Braid REQ-122 / [BF140-01–05](braid-foundation-requirements/README.md) owns pre-freeze consumer design, integration and disposition. Core REQ-139 owns the neutral schema seam under this shared REQ-140 goal.

Before semantic changes, pin immutable specification revisions/sections/deviations. Preserve original checkouts, package pins, stores and sealed receipts. Report exact source/package/test/API-manifest identities and outstanding gaps. No permanent API, package publication, storage migration, issuer/key or product adoption is supplied by this documentation-only goal registration.

## F140-01 progress — 30 October 2026

The [core API baseline](foundation-api-baseline/README.md) is complete at exact source f758a29f; the joint contract inventory is **In Progress**. All 270 observed type dispositions remain proposed/Pending. Member-level selection, consumer requirements and the nine open decisions still block final scope and freeze. External execution remains with the delegated owners. No F140-02 implementation or F140-04 acceptance occurs here.

## F140-01 member-level draft progress — 30 October 2026

The [member manifest and compatibility matrix](foundation-api-contract-draft/README.md) now cover every baseline entry. Conservative public-type closure exposes 11 additional Runtime dependencies beyond Core/query seeds. This advances FG-01/04/07 preparation, not owner acceptance: all 20 compatibility cases are NotRun, all owner dispositions Pending, and no interface or scope is frozen. External requirements remain delegated. Next core task is the focused lifetime/ownership decision note.

## REQ-140 / F140-01 ownership review progress — 30 October 2026

The [lazy/escaped-result decision note](foundation-result-ownership/DECISION.md) supplies source tracing and ten bounded memory-only observations. Existing explicit provider/host ownership is proposed; joint requirements and any optional scope stay open. A throwing StreamCompleted callback reproducibly skips owned-reader disposal; a focused exception-safe completion fix is the next core-owned slice, tracked here as Pending implementation/qualification. No production source/API/pin or external owner work changed. Full compatibility cases, C45 and freeze remain pending.

## Bounded ownership fix qualified — 30 October 2026

The [completion cleanup slice](streaming-completion-cleanup/README.md), source candidate 1.34.2, resolves the reproduced owned-reader disposal gap with eleven regressions and complete local engine/gate qualification. Existing metadata/interfaces/layouts are unchanged; aggregate dual-failure behavior is explicitly documented. FG-04 remains open for early-abandonment/escaped-result and actual consumer ownership; this is not general scope/API acceptance. Next core preparation is external public-signature dependency/legacy-schema boundary inventory (FG-01/02/07).

## F140-01 external boundary proposal — 30 October 2026

The [external dependency review](foundation-external-dependencies/BOUNDARY.md) completes the core inventory of public external signature types and proposes an explicit legacy compatibility ledger alongside future neutral contracts. Five logical options/context members expose schema types inside the proposed graph; they cannot be silently removed by calling the rest neutral. Core/Schema/XmlDb scope disposition, exact neutral signatures and compatibility/freeze acceptance remain Pending. No production change occurred. Prepare the existing legacy/no-provider compatibility harness next, while owners execute their delegated packages.

## Bounded legacy caller evidence — 30 October 2026

The [retained-caller harness](foundation-legacy-compatibility/README.md) demonstrates selected source/default/error and unchanged-binary paths from local source-built 1.34.1 to qualified 1.34.2 across 20 cases/three modes. Exact observations match and loaded assembly hashes are verified. This advances core-owned FC-01/02/03/15 preparation; it does not certify the actual required consumers, their older published pins, packages, C45 or joint freeze. The candidate informational revision still reflects its pre-commit build base; committed-source/package provenance is the next explicit qualification task.

## Local immutable-source package evidence — 30 October 2026

The [seven-package Core/API candidate](foundation-package-qualification/README.md) binds repository and assembly identity to c88eea43 / 1.34.2 and retains package/archive/cache/output/loaded hashes. Existing caller source and unchanged binaries pass their twenty selected legacy cases using only restored package dependencies. This is a local prepared artifact cohort, not F140-04 completion: package-backed full solution/gates, actual consumer qualification, neutral C45 scope and joint freeze remain pending. Release packaging must deliberately retain the reviewed NOTICE asset recipe or adopt an equivalent stock-pack change.

## Exact package-cohort full gates — 30 October 2026

[Full solution and frozen gate execution](foundation-package-gates/README.md) now qualifies the seven selected package DLLs in an explicitly recorded mixed environment, with 24,225 passing unit tests, unchanged QT3/qt4 and actual binding hashes. It is bounded core evidence, not actual dual-consumer or wholly package-derived product certification. Preserved setup corrections are not skip admissions. General C45/required contract gaps and all owner freeze/release dispositions remain pending; prepare the consolidated core handoff packet next.

## Consolidated owner handoff — 30 October 2026

Use [the readiness packet](foundation-readiness/README.md) as the entry point: exact source/feed/API/evidence identities, qualified scope, pending requirements and role-specific acceptance return instructions. Readiness means local evidence is available, not that the full shared foundation is ready for freeze. No owner sign-off is inferred and no prior frozen template/receipt is filled in automatically. Core next maps returned required needs to the finite C139-01/C45 decision and named member manifest; required gaps and all joint release/freeze dispositions remain open.

## Concrete transport/ownership draft — 31 October 2026

The [C139-01 first signature slice](foundation-contract-design/README.md) replaces a generic prototype inventory with 13 concrete draft types, explicit ownership/error rules and four distinguishing experiment plans. Declaration compilation against the exact 1.34.2 Core DLL passes without warnings; no production implementation or performance behavior is proved. Final carrier representation and the remaining profile/advisor/compatible-view API need real experiments and owner disposition. Full known requirements remain freeze blockers.

## Partial carrier experiment return — 31 October 2026

[E-01/E-02 cloned-Core observations](foundation-carrier-experiments/README.md) reject unsafe payload replacement and quantify the prototype overlay holder allocation versus wrapper size/transport loss. No representation or production performance budget is accepted. Existing plans remain historical; current evidence is partial and does not close full VM, real-provider, lifetime or final signature gates.
