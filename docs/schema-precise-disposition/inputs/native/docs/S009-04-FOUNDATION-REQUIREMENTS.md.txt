# S009-04 — Full-XSD-1.1 foundation requirements and joint freeze acceptance

> **Started 2026-10-30** (Schema owner work under REQ-009, registered in [REQ-009-OWNER-WORK-PACKAGES](REQ-009-OWNER-WORK-PACKAGES.md)).
> **Scope of this step:** the full-validator coverage/requirements roadmap and the mapping of all known core needs. Final sign-off remains blocked on the agreed bounded candidate, actual qualification evidence and core REQ-140 gap closure — with Schema signing off jointly with XmlDb. This document accepts no API, freezes nothing and claims no full-XSD-1.1 coverage. Parent REQ-009 remains In Progress.

## 1. Basis and method

Requirement sources (navigation, not adoption pins):

- [XSD11_GAP_MATRIX](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/XSD11_GAP_MATRIX.md) — the full XSD 1.1 Structures/Datatypes gap inventory (rows 1.1–1.12, 2.1–2.5, 3.1–3.6) and the 30 confirmed W3C skip IDs.
- [REQ-140 required Schema evidence](https://github.com/Fytala-Charles/Bosak/blob/checkpoint/restart-2026-10-29/docs/FOUNDATION-API-FREEZE-PLAN.md) — every known foundation need for full XSD 1.1 must be mapped and exercised with representative actual provider operations.
- [Core REQ-139 C139-01–04](https://github.com/Fytala-Charles/Bosak/blob/checkpoint/restart-2026-10-29/docs/REQ-139-OWNER-WORK-PACKAGES.md) — the neutral contract workstream this roadmap feeds.
- [ADR-006](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/ADR-006-xsd11-engine-seam-compatibility.md) — additive, opt-in seam posture; never silent acceptance.

Status legend: **Required** (a freeze blocker until met) · **Partial** (real evidence exists inside a finite profile; insufficient alone) · **Pending** (no current evidence; awaits core contract and/or Schema backend coverage).

## 2. Foundation needs roadmap

| # | Foundation need | Full-XSD-1.1 features depending on it (gap-matrix rows) | Current Schema evidence | Required neutral core contract element | Status |
|---|---|---|---|---|---|
| N1 | Complete component / universe identity | 1.2 alternatives, 1.4 override, 1.8 conditional inclusion, 3.1 component model | 0.9/v5: process-local instance authority only — original declaration/type objects, `AssessmentIdentity`/`SnapshotIdentity`; digest/QName equality explicitly grants nothing ([S009-02 map](S009-02-PROVIDER-FACT-MAPPING.md) D1) | Authority-scoped component identity separate from source/snapshot identity; no identity inference from names/digests | **Required — unmet** |
| N2 | Derivation / affiliation / closure | 1.5 all composability, 1.10 QName/NOTATION derivation, 2.5 list/union composability, substitution groups | 0.9/v5: single anonymous element-only type; no derivation, affiliation or closure facts established (S009-02 D2). Main-branch REQ-009 records named simple-type restriction *chains* for facet inheritance (S3) — descriptor-level, not structural authority | Derivation/affiliation relations and component closure traversal over a governing type | **Required — unmet** |
| N3 | Typed atomic / list / union value transport | 2.1 assertion facets, 2.2 new datatypes, 2.3 explicitTimezone, 2.4 pattern on QName/NOTATION | 0.9/v5: exact decimal atomics only; rounding/overflow/underflow refused (S009-02 D4). Main-branch S2/S3 typed view annotates declared built-in simple types for assertion evaluation | Neutral typed-value transport (atomics now; list/union as the backend lands), exact precision rules, refuse-don't-round | **Partial** |
| N4 | Assertions / type alternatives | 1.1 xs:assert, 1.2 xs:alternative CTA, 2.1 xs:assertion facets | Main-branch S2/S3/S4a/S4b implemented add-side: W3C-constrained XPath 2.0 environment, facet chains, governing-type CTA with xsi:type pre-emption — evaluation-only, no structural authority. 0.9/v5: bounded count/presence decimal assertions natively | Consultation surface so engine assessment can ask the schema authority (advisor shape owned by C139-01; ADR-006 posture) | **Partial** |
| N5 | Original assessment / PSVI | all rows — every typed feature produces PSVI | 0.9/v5: original declaration/type objects, no-typed-value element-only root, exact leaf PSVI, never-nil profile; preserve/strip copies retain/drop state (S009-02 D3/D5) | Original PSVI preserved across views/copies/transport; no view rewrites original state | **Partial** |
| N6 | Compatible views / cross-universe handoff | 1.4 override, 3.1 component model, compatible-schema interchange | 0.9/v5: copy/preserve/strip within one assessed snapshot only; compatible universes explicitly not established | Compatible-view contract that preserves original PSVI across accepted component universes | **Required — unmet** |
| N7 | Lifetime / cancellation / concurrency | cross-cutting (3.6 caching/incremental validation later) | 0.9/v5: immutable concurrent assessor, owned snapshots, throwing cancellation, caller-owned exports (S009-02 D6) | Neutral lifetime/ownership rules for sessions, snapshots and deferred values (core FG-04 tracks ownership) | **Partial** |
| N8 | Identity separation invariants | cross-cutting | C139-01 separation rules restated as invariants: query/XDM profile, XSD version, component authority/universe, source/snapshot identity remain distinct; never conflated | Invariants encoded in the accepted contract, not just prose | **Required — unmet** |

## 3. Known core needs — consume / supply map

What Schema needs from core, and what Schema must supply back, per need:

| Need | Schema consumes from core | Schema supplies back (provider facts) |
|---|---|---|
| N1 | Neutral identity handles and comparison semantics that never collapse to QName/digest equality | Real provider-fact tests: supported components round-trip identity; foreign components refuse ([S009-02](S009-02-PROVIDER-FACT-MAPPING.md) F-01/F-02) |
| N2 | Derivation/affiliation/closure traversal over the governing type | Nothing yet — no derivation facts exist in the 0.9/v5 profile; first evidence awaits an S009-01-selected slice |
| N3 | Typed-value transport for atomics (list/union later) with exact precision | Decimal atomic transport evidence; explicit refusal evidence for rounding/overflow/underflow |
| N4 | Assessment-time consultation entry points (advisor seam, opt-in) | Assertion/CTA consultation semantics per main-branch S2–S4b and 0.9/v5 native assertions |
| N5 | PSVI-preserving view/copy/transport primitives | Preserve/strip behavior, no-rewrite guarantee, stale-value refusal evidence |
| N6 | Cross-universe compatible-view contract | None yet — blocked with N1/N2 |
| N7 | Ownership/disposal/cancellation contracts for sessions and deferred values | Cancellation-throws, caller-ownership, concurrent-assessor evidence |
| N8 | Contract-level invariants | Verification fixtures that would detect identity conflation (F-02/F-06 style) |

## 4. Backend coverage tracking (separate lane)

Full XSD 1.1 validation coverage (gap-matrix rows 1.1–1.12, 2.1–2.5) is Schema backend work tracked separately under S009-01/S009-04 completion; the foundation freeze does not require every backend feature, but every **known required API need** (N1–N8) must be represented in the contract before freeze. Decimal-only or prototype success cannot prove the full goal; each new semantic slice records immutable specification revision/sections/deviations before code, with happy-path and edge regressions.

## 5. Open questions for core (C139-01 / REQ-140)

1. Exact neutral identity handle shape and its separation from source/snapshot labels (N1, N8).
2. Whether derivation/closure traversal is contract or advisory data (N2) — depends on the accepted C45 disposition.
3. Session vs. per-context advisor lifetime and the admission model (N4, N7); the Braid session design is a separate consumer and must not be conflated.
4. List/union transport timing: contract now with refusal defaults, or defer until Schema backend evidence exists (N3)?

## 6. Sign-off prerequisites (Schema side, from REQ-140)

- [ ] N1–N8 mapped above are represented in the accepted contract; no unresolved required gap remains implicit.
- [ ] Representative actual provider operations qualified on one exact candidate (F-01…F-06 executed; source/package pins recorded).
- [ ] Backend coverage roadmap reconciled with the gap matrix; conformance/release separately evidenced.
- [ ] Joint sign-off recorded alongside XmlDb; Braid compatibility retained as regression evidence only.

---

*Started 2026-10-30. Requirements mapping only — no runtime behavior, test execution, API acceptance or freeze is claimed by this document.*
