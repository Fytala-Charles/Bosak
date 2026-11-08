# Hosted artifact owner and signing review

8 November 2026. Technical preparation is complete for ten NuGet packages **1.81.2** and extension **0.2.6**, producer source `7249d0e01628c47f92b89a3380bc69cf58d70045`. This packet prepares the remaining decisions; it grants no approval and no owner message has been sent.

[Candidate pins](CANDIDATE-PINS.json) bind the successful [hosted results](../release-linux-collection-hosted/README.md), both run IDs, manifest hashes and the API inventory boundary. The API inventory is historical local evidence from identical shipping source, not a new C45 acceptance. Approval must cite these hosted identities rather than an earlier preview or local Windows manifest.

## Owner returns

| Reviewer | Form | Required decision |
|---|---|---|
| Core | [Core](returns/Core.template.json) | Release scope/migration/API profile and assigned/shared PF scenarios. |
| Schema | [Schema](returns/Schema.template.json) | Assessed profile, provider/lifetime disposition and PF-02/03/06/08/09. |
| XmlDb | [XmlDb](returns/XmlDb.template.json) | Store/snapshot/adoption disposition and PF-04/05/06/08/09. |
| Braid | [Braid](returns/Braid.template.json) | Worker/IPC/configuration disposition and PF-06/07/08/09. |

Each return needs an identified reviewer and date, explicit accept/revision-required/not-run decision, exact fixture/configuration and baseline pins, numerical units/statistics/limits/tolerances, raw evidence, and supported/refused/deferred requirement dispositions. Shared scenarios need the relevant owners. Empty fields remain unfilled decisions; technical tests do not populate them. Do not silently waive budgets or provider requirements for the narrower shipping scope.

[Repository inspection](OWNER-EVIDENCE.json) records the three owner checkout revisions, changed-working-tree state and hashes of selected earlier design reviews. The bounded documentation search found no1.81.2/7249d0e0 match. That search is not proof that no approval exists elsewhere. The inspected returns bind older design proposals and do not close current workload/runtime/budget/API decisions. Owner files were read only; no proprietary source, binaries or private history were copied.

## Signing and provenance decision

[Archive observations](SIGNING-OBSERVATIONS.json) show no `.signature.p7s` entry in any of the ten hosted NuGet packages. Retained manifests are SHA-256 JSON and are not represented as signed attestations. Producer run/job/artifact metadata and exact-byte checks are available; certificate trust, service-side signatures, publisher authentication and cryptographic build attestations have not been established by preparation.

The release owner must specify the signing/provenance policy and accept the resulting evidence. If artifact signing or a rebuilt pipeline changes bytes, create new manifests and perform applicable checks before approval; do not reuse these hashes for changed packages. No credential, secret, environment protection or publication setting was changed.

## What is needed now

Identify the Core/Schema/XmlDb/Braid reviewers, or provide current approval file locations. Complete the scoped owner/API and signing decisions against these pins. Only after applicable decisions close can the release owner authorize the concrete tags and publication. This packet does not request publication and does not contact reviewers.

Run `python docs/release-1.81.2-owner-review/verify.py` to check candidate inputs and the unfilled draft forms. This validates consistency only.
