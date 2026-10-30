# REQ-139 — Bosak owner execution packages

Registered: 30 October 2026. Requester: Bosak.Schema; registration directed by the user. XmlDb is a required foundation consumer under REQ-140; that does not make its untyped store a Schema consumer. Braid remains a compatibility stakeholder. Accountable owner role: **Bosak core maintainers**; individual scheduling/assignment belongs to that owner. Parent status remains **Pending**. This registration requests work; it does not accept C45, freeze a public API or authorize publication.

Shared goal: [REQ-140 foundation/API freeze](FOUNDATION-API-FREEZE-PLAN.md). This packet supplies the Schema-specific workstream; C139-03 must include real XmlDb acceptance, and C139-04 does not freeze the shared surface without the joint gate.

## Current basis and boundaries

Core registry/workspace baseline: `815e453ea584b9331cb793a0796e82d39aafd4e0`. Public Schema assessment baseline: `7e498a1922dbda4ae14005acc4b13a049896c75a`, native 0.9/v5 candidate. Braid D01 owner acceptance is recorded separately; its current adopted engine remains 1.26.2. The core's bounded 1.34.1 provider/document bridge already exists as a candidate; do not implement the historical typing-only callback proposal as if that existing work did not exist.

The [revised C45 decision](conformance/k04-schema-compatibility-prototype/OWNER-DECISION.md), [compiled candidate/qualification](conformance/k04-schema-compatibility-prototype/QUALIFICATION.md), [minimal public-transition review context](SCHEMA-APACHE-PUBLICATION-2026-10-29.md) and [current assessment](schema-assessment/README.md) are inputs. C45/general public-session acceptance remains pending. The earlier commercial/licensed plug-in assumption is historical: Bosak.Schema is Apache-2.0, while private issuance/billing belongs to Bosak.Licensing. Dependency direction, identity, PSVI, compatibility and lifetime requirements still apply.

## Owner queue

| Work package | Start condition | Concrete owner deliverable | Completion evidence |
|---|---|---|---|
| C139-01 Minimal core/schema contract disposition | Ready for owner review now; independent of Braid D02-D07 | Reconcile the existing bounded bridge, revised C45 proposal and public Schema ownership; select the smallest neutral contract and record exact accepted/deferred signatures, supported profile, deviations and implementation scope. Keep query/XDM profile, XSD version, component universe and source/snapshot identity separate. | Dated explicit owner disposition tied to immutable proposal/source hashes, compiled proposed surface and mapping to real Schema/provider capabilities. No automatic permanent acceptance of all 32 prototype types. |
| C139-02 Guarded opt-in core implementation | Blocked only on C139-01's explicit implementation scope and corresponding provider facts, not on full Braid product validation | Implement the accepted neutral contracts/routing, compile-time semantic-environment capture, original node/atomic provenance and preserve/strip/copy/lifetime rules. Keep no-provider behavior and existing APIs. Refuse owned unsupported coverage without BCL downgrade or invented W3C error names. | Happy/error/boundary/cancellation/reentry/lifetime tests for every implemented capability; atomization, casts, equality/hash, containers and serialization exercised end-to-end. Core production assemblies have no Schema/provider/licensing dependency. |
| C139-03 Compatibility and frozen-gate qualification | Can prepare fixtures/audits now; execute against C139-02 candidate when available | Audit actual public API/ABI/value layout/reference accessors and default-path performance. Qualify actual Schema, XmlDb and Braid source and unchanged-binary consumers in isolated copies; preserve product pins/storage. | Fresh complete core tests, source/ABI reports and unchanged QT3 **31,397/0/424** plus qt4 **8,941/0/1,918**, 230 gated sets and exact retained skip names/reasons. Any proposed promotion requires separate owner adoption. Synthetic contracts alone do not certify real consumers. |
| C139-04 Immutable source/package handoff | Build/provenance planning can start now; publication remains separately authorized | Supply a named source candidate, versioned API/profile/error documentation, exact package/archive/assembly identities, build recipe and consumer test entry points. Distinguish delivery of the existing bounded 1.34.1 bridge from a later general C45 feature. | Source/package provenance and reproducible package-only consumer qualification. No mutable latest branch reference as the sole identity; publication/tag and consumer pin adoption are separate decisions. |

Before any new semantic implementation, the owner must record exact immutable normative source revisions, affected sections and intentional deviations. This request and old prototype receipts are not a new normative adoption record. Preserve all sealed historical evidence.

## Acceptance checklist

- [ ] C139-01 records an actual owner decision and finite supported contract; registration alone does not satisfy it.
- [ ] C139-02 implements only that scope, with explicit unsupported/invalid outcomes and preserved original provenance/ownership.
- [ ] C139-03 proves additive/default-path compatibility with actual providers and consumers, including original state, copy and lifetime behavior.
- [ ] Fresh full tests and exact gates/skip maps pass; unexpected failures are retained and triaged rather than hidden by filtering.
- [ ] C139-04 identifies immutable deliverables and source/package evidence; release and API freeze have their own explicit disposition.

## Parallel work and handoff

Schema's [REQ-009 owner packages](https://github.com/Fytala-Charles/Bosak.Schema/blob/feature/bounded-document-assessment/docs/REQ-009-OWNER-WORK-PACKAGES.md) can produce fixture/coverage and provider-fact mapping while C139-01 is reviewed. Only the production adapter's final interface binding waits for the accepted core shape. Current bounded-assessment packaging may be prepared against existing APIs; a general C45 seam is not automatically a prerequisite for every bounded release.

No missing engine capability is established for Braid's existing inspection-only component. Braid's D02 trust design and later host/native gates do not block this Schema-requested queue. Conversely, source/API/package delivery here does not upgrade Braid's pin, accept its product host or turn its cohort-specific D01 decision into general schema/API acceptance. Issuer/key/revocation-service implementation is outside core and public Schema scope.

Owner handoff must name completed package IDs, immutable commits/packages, executed test commands/results, remaining unsupported coverage and the exact consumer action required. Status changes belong in [FEATURE_REQUESTS](FEATURE_REQUESTS.md); cross-repository links are navigation, not immutable specification adoption pins.
