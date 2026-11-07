# Owner review — exact preview artifacts and outstanding decisions

Candidate: `c0b926de34824ceb2010d5f814a4450b28b31dcc`.
Preview: **1.81.1-preview.c0b926de**. Development source remains 1.81.1.
This is a prepared review request, not an approval or a message sent to owners.

Concrete evidence for review:

- [Nine package artifacts](../foundation-preview-packages/evidence/PREVIEW-PACKAGES.zip) and [package hashes/dependencies/assets](../foundation-preview-packages/evidence/PACKAGES.json).
- [Qualified regressions/conformance/API](../foundation-preview-qualification/evidence/RESULTS.json), including separately pinned non-packaged language-server/database products.
- [Package-cohort source/binary pins](CANDIDATE-PINS.json) and [API inventory](../foundation-preview-qualification/evidence/public-api.json).
- [Exact-package bounded workload](evidence/SUMMARY.json), retaining six candidate-only decimal sessions and no accepted regression baseline.
- [Production-provider adoption proposal](../foundation-provider-adoption/SCHEMA-ADOPTION.md), preserving dependency direction and explicit supported/refused behavior.
- [Machine-readable owner status](OWNER-REVIEW.json), with all nine PF acceptance slots unfilled and no requirement waived.

| Owner | Assigned/shared PF scenarios | Concrete decision needed |
|---|---|---|
| Core | PF-01 through PF-09 | Ordinary/typed workload and baseline choices, budgets, migration and API-shape disposition |
| Schema | PF-02/03/06/08/09 | Production provider location/profile/lifetime adoption, actual schema workloads, baselines and budgets |
| XmlDb | PF-04/05/06/08/09 | Persistent store/snapshot fixtures, source and unchanged-binary consumers, lease/read-amplification budgets |
| Braid | PF-06/07/08/09 | Actual worker/IPC/configuration and typed route, end-to-end phase and invalidation budgets |

For each scenario and source/unchanged-binary lane, complete the matching template
under `returns/`. Supply immutable fixture/store/schema/query/configuration hashes,
selected supported profile/limits, the cold/warm and first-item/full-consumption
boundary, a genuinely comparable baseline, measurement environment/repetitions,
and numerical thresholds with units, statistics and tolerances. Attach original
raw/correctness/load receipts and an explicit dated owner disposition over these
candidate pins. Record unsupported or unrun required routes separately; no waiver
or exemption is assumed.

Templates:

- [Core](returns/Core.template.json)
- [Schema](returns/Schema.template.json)
- [XmlDb](returns/XmlDb.template.json)
- [Braid](returns/Braid.template.json)

The new manifest contains nine package engines and two unchanged non-packaged
products. Do not substitute the earlier 1.81.1 production hashes merely because
assembly versions match. [Screening evidence](evidence/OWNER-REFUSALS.json) shows
the old production cohort is rejected by manifest and engine-hash checks.
Paths in a completed receipt are relative to this packet and must remain within
it. The preflight checker validates structure/hashes only, not authenticity,
scenario ownership, correctness or performance acceptance.

No owner identity, acceptance date, workload approval, baseline approval, numerical
budget or production adoption acceptance has been filled in by the agent.
The release gates cannot close until the corresponding decisions and evidence
are supplied. Further implementation required by those decisions must be scoped,
adopted and qualified separately before freeze or publication.
