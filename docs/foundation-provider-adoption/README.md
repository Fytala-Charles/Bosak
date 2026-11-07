# PV-Q07 — production-provider adoption proposal

7 November 2026. Proposal for qualified Core **1.81.1**, checkpoint
`c0b926de34824ceb2010d5f814a4450b28b31dcc`. This packet prepares review inputs;
it does not adopt new semantics, export a production provider or grant acceptance.

The [owner inventory](../foundation-owner-gap-review/SOURCE-REVIEW.json) and
[exact-cohort measurements](../foundation-workload-1811/README.md) establish the
available starting evidence. The public Schema project still references 1.26.3
Core ecosystem packages. Its 451-case compatibility result and the separate
59 adapter / 10 consumer controls serve different purposes; they do not add up
to production-provider acceptance.

| Owner | Proposed delivery | Evidence required before adoption |
|---|---|---|
| Schema | Port the reviewed bounded assessor/advisor and owned snapshot facade into the active public Schema repository; decide whether to retain it in Bosak.Schema or a Schema-owned integration assembly | Public API/profile review, exact package dependency cohort, source and unchanged-binary lanes, native/adapter/consumer regressions and owner budgets |
| XmlDb | Bind persistent inputs through its actual snapshot/reader and operation lease; retain original store formats | Pinned real stores, navigation/order/namespace correctness, partial consumption, owner close/cancellation/fault and deterministic release, read amplification and PF-04/05 budgets |
| Braid | Integrate the selected typed provider route with the actual worker/run configuration | Pinned worker and IPC/configuration inputs, explicit process boundary transport, source/binary lanes, phase timings, invalidation and PF-06/07 budgets |
| Core | Supply the qualified contracts and candidate artifacts, then qualify actual consumers | Exact loaded bindings, ordinary-path regression evidence, migration disposition and PF-01/shared scenarios; new API only on demonstrated missing capability |

The [Schema adoption plan](SCHEMA-ADOPTION.md) records the concrete ownership,
profile, packaging and verification choices. There is no proposed Core-to-Schema
reference, credential dependency or proprietary licensing seam. Implementation
belongs in `D:/Development/Bosak.Schema.Public`, with its applicable instructions.
The persistent assessment worktree remains a separately governed candidate;
the private licensing checkout is outside this proposal.

`returns/Core.template.json`, `Schema.template.json`, `XmlDb.template.json` and
`Braid.template.json` contain the exact current engine/API hashes. They remain
unfilled review inputs: select one assigned PF scenario and one source or
unchanged-binary lane per completed receipt, and retain separate returns for
each additional scenario/lane. Scenario ownership follows the
[decision matrix](../foundation-owner-gap-review/OWNER-DECISIONS.md).
Provide the baseline, workload, thresholds, units, statistics and tolerances;
no thresholds or owner signatures are invented here.

Run `python docs/foundation-provider-adoption/tools/preflight.py <receipt>` for
structural/hash screening against this candidate. File references are relative
to this packet and must remain within it. The reused checker is structural;
even a structurally complete receipt does not prove workload correctness,
authenticity, scenario ownership or acceptance. Attach a separately reviewed,
dated owner disposition over the exact candidate and profile. All four empty
templates correctly refuse. The checker's 18 synthetic controls pass.

Next **PV-Q08**: prepare local preview packages and fresh-cache package-only
consumer checks under R6's preparation allowance, keeping all R4/R5 gates open.
Choose and record an explicit local preview identity before packing; do not
silently treat the 1.81.1 development number as an approved public release.
Production provider changes and representative owner runs can proceed as their
inputs become available. No owner messages, external repository writes, pin
upgrades, tags or publication were performed in this proposal.

This is a documentation/tooling slice. The exact production qualification is
retained unchanged; no fresh engine suite is claimed. Run
`python docs/foundation-provider-adoption/tools/verify.py` to check the packet,
templates and local links. No rendered-document approval is claimed.
