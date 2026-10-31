# Shared Bosak, Bosak.Schema and Bosak.XmlDb owner handoff

Registered: 30 October 2026, at the user's request to put executable owner work in each FEATURE_REQUESTS registry. REQ-140 coordinates the shared foundation goal; project-local work packages retain their existing request IDs. Registration does not approve implementation or freeze.

| Repository / owner role | Registry parent and owner packet | Work ready without Braid production validation | Actual dependency |
|---|---|---|---|
| Bosak / core maintainers | [REQ-139 C139-01–04](REQ-139-OWNER-WORK-PACKAGES.md) | Minimal contract disposition; compatibility fixtures/API audits; bounded-source/package handoff preparation | Permanent C45 implementation needs explicit accepted scope; general provider integration consumes real Schema capability facts. |
| Public Bosak.Schema / schema maintainers | [REQ-009 S009-01–04](https://github.com/Fytala-Charles/Bosak.Schema/blob/feature/bounded-document-assessment/docs/REQ-009-OWNER-WORK-PACKAGES.md) | Native fixture/coverage inventory; provider authority/PSVI mapping; bounded candidate delivery preparation | Final general adapter binding needs accepted core shape; release needs available dependencies and separate publication authorization. |

| Bosak.XmlDb / database maintainers | [REQ-009 FX009-01–03](https://github.com/Fytala-Charles/Bosak.XmlDb/blob/docs/shared-foundation-api-freeze/docs/BOSAK-FOUNDATION-OWNER-WORK.md) | Real provider requirements, fixtures and API/lifetime audit | Final qualification uses the agreed candidate; XmlDb owner sign-off is required before shared foundation freeze. |

Braid D02-D07 do not block these engineering queues. Its cohort-specific D01 owner acceptance does not accept C45, a new engine pin or a schema provider. Issuer/key/billing work belongs to private Bosak.Licensing; it is intentionally not registered as an engine dependency in either public project.

Registration locations are the existing core checkpoint branch and Schema feature/bounded-document-assessment candidate branch, not an asserted merge to either default branch. Original public Schema checkout and consumer pins remain unchanged. Owners should inspect the named packets and registry entries, assign individuals and execute ready work; record the C45 decision before its implementation. Any new semantic work first records immutable specification revisions, sections and deviations.

No production implementation, API/ABI acceptance, conformance admission, version bump or release is supplied by this documentation-only registration. Existing test receipts remain historical qualification and are not claimed as fresh executions. Each owner reports exact commits/packages, commands/results and outstanding blockers when completing a package.

Earlier Schema registry delivery: `fd9f833c99edb10fdfd282652cb7b04c8d8114a7` on feature/bounded-document-assessment. Core registration delivery resolves through `git log -1 --format=%H -- docs/REQ-139-OWNER-WORK-PACKAGES.md`; neither registration is asserted merged into default main.

## Shared foundation goal — 30 October 2026

Bosak REQ-140 now registers the shared foundation for both Bosak.Schema and Bosak.XmlDb. XmlDb is a required acceptance consumer alongside Schema. [The coordinated plan](FOUNDATION-API-FREEZE-PLAN.md) defines F140-01 through F140-04: joint contract inventory, accepted gap implementation, actual dual-consumer qualification, then publication and a named versioned API freeze. Core REQ-139 supplies the neutral schema seam; Schema owns full XSD 1.1 implementation. The bounded assessment is not full support. Freeze and C45 acceptance remain pending; existing package pins, storage formats and Braid product gates remain unchanged.

Shared-foundation registration deliveries: Schema `243cc60` on `feature/bounded-document-assessment`; XmlDb `0808595` on `docs/shared-foundation-api-freeze`. Core delivery resolves with `git log -1 --format=%H -- docs/FOUNDATION-API-FREEZE-PLAN.md`. Documentation checks passed; runtime receipts are historical, not rerun. These branches are not asserted merged into default main.
