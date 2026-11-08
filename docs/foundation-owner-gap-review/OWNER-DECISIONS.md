# Concrete owner decisions required

Use candidate `c0b926de34824ceb2010d5f814a4450b28b31dcc` / source 1.81.1 and the
qualified [source/binary manifest](../foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json).
The historical [run contract](../foundation-provider-qualification/RUN-CONTRACT.json)
and [owner receipt template](../foundation-provider-qualification/OWNER-RUN.template.json)
describe evidence structure; refresh candidate-specific fields rather than
submitting an old 1.81.0 receipt as current acceptance.

For each assigned PF scenario, return:

1. Workload: fixture/store/schema/query hashes, size distribution, supported
   profile and finite limits; cold/warm and first-item/full-consumption boundaries.
2. Baseline: independently selected engine/provider/configuration hashes and
   capability equivalence, or an explicit proposed alternative for owner review.
   No baseline or regression exemption is assumed.
3. Budget: metric, unit, statistic, absolute bound and applicable regression
   tolerance, repetition policy and runtime/OS/architecture. Distinguish latency,
   allocation, deterministic cleanup and retained-resource observations.
4. Provider adoption: production location, source and unchanged-binary consumers,
   loaded assembly identities, ownership/cancellation/error behavior, and explicit
   supported/refused routes. Identify absent routes separately.
5. Disposition: dated owner identity, exact candidate/profile/manifest and
   accepted/revision-required/not-run result. Design agreement is not runtime
   or performance acceptance.

| Owner | Scenario decisions | Adoption evidence needed |
|---|---|---|
| Core | PF-01; share PF-02/03/04/05/06/07/08/09 | Ordinary and bounded typed paths, exact cohort, migration and API shape |
| Schema | PF-02/03; share PF-06/08/09 | Production assessor/advisor placement and supported native type/fact/container routes |
| XmlDb | PF-04/05; share PF-06/08/09 | Persistent snapshot/store fixtures, source/binary compatibility and deterministic leases |
| Braid | PF-06/07; share PF-08/09 | Worker execution, typed route, IPC/configuration, phase timing and invalidation |

No owner has been contacted by this review. These are review inputs, not
fabricated approvals. Where a required route is unsupported, return its explicit
disposition and dependency; do not record a passing run for it.
