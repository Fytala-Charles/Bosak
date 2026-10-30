# REQ-140 — Core readiness and owner handoff

30 October 2026. **The existing 1.34.2 Core/API candidate is locally qualified for the recorded scope. The shared foundation is not yet ready for API freeze.** This packet consolidates evidence; it supplies no owner sign-off, permanent C45 signature acceptance, publication or consumer pin change.

## Exact handoff identity

| Item | Pinned identity |
|---|---|
| Production source | `c88eea4308c0e49a81e7bfe2f481319f2e4d208c` / candidate 1.34.2 |
| Evidence snapshot | `2441b5f3b6a5d074c64da66b07ed2a388985f173` |
| Local candidate feed | [candidate-feed.zip](../foundation-package-qualification/evidence/candidate-feed.zip) |
| Feed SHA-256 | `03bb793d3cd69d7e819e1568ab11eff98bd2cec356a963098b3d5f738b15dc39` |
| Machine-readable packet | [READINESS.json](READINESS.json): package/assembly hashes plus 13 pinned evidence inputs and immutable repository links |
| Owner return format | [Response instructions](OWNER-ACCEPTANCE.md) and [blank response template](OWNER-RESPONSE.template.json) |

Branch links are navigation only. Use the exact source/feed/file hashes for qualification and preserve original product pins. The seven packages are Core, Parser, Compiler, Providers, Runtime, Standard and Api, all 1.34.2. A consumer need not reference all seven directly; every package it uses must be tied to this exact cohort or an explicitly identified replacement.

## Evidence available now

| Scope | Recorded qualification | Practical limit |
|---|---|---|
| [Public API baseline and member draft](../foundation-api-contract-draft/README.md) | Full baseline: 270 types / 5,766 metadata entries, member IDs and planned compatibility cases | Proposed scope; all owner dispositions pending |
| [External/legacy boundary](../foundation-external-dependencies/README.md) | 3,506 public declarations, 176 external definitions; 26 schema-exposing declarations, nine inside proposed graph | Existing legacy obligations stay; no neutral seam accepted |
| [Ownership review and cleanup fix](../streaming-completion-cleanup/README.md) | Owned-reader completion fixed, including callback/cleanup failure handling | Historical LO-07 is pre-fix; early/nested/escaped ownership is still open |
| [Legacy source/binary harness](../foundation-legacy-compatibility/README.md) | 20 cases across baseline 1.34.1, unchanged binary replacement and source rebuild | Core-owned in-memory legacy fixtures, not actual consumers or published 1.26.x pin acceptance |
| [Committed-source package-only cohort](../foundation-package-qualification/README.md) | Exact source/repository/assembly revision; isolated restore, source/unchanged caller and feed replay pass; selected 78 types / 2,205 metadata entries unchanged | Seven local unsigned packages; explicit committed-NOTICE recipe; not a release |
| [Package-backed full tests/gates](../foundation-package-gates/README.md) | 24,225 passing tests; QT3 31,397/0/424; qt4 8,941/0/1,918, 230 sets/11 experimental passes; exact skip maps unchanged; loaded hashes verified | XSLT/XQuery/database product DLLs remain qualified source-built bytes; not an all-package product cohort |

Counts overlap across these scopes. They must not be added together or relabeled as independent/real-consumer execution. Original Runtime fixture-layout and XSLT observer corrections are preserved in the gate dossier, not hidden by skip changes.

## Required dispositions before shared freeze

| ID | Accountable role/input | Still required |
|---|---|---|
| FR-01 | Core + Schema + XmlDb | Exact named/versioned stable member manifest and legacy/experimental boundary; no unresolved required foundation need |
| FR-02 | Core + Schema | C139-01/C45 finite neutral signature/profile disposition with immutable adoption evidence before implementation |
| FR-03 | Schema + Core | Full-XSD-1.1 foundation needs mapped, representative actual provider provenance/typed/copy/refusal/lifetime behavior qualified; full backend conformance tracked separately |
| FR-04 | XmlDb + Core | Actual persistent-provider source/package/unchanged-binary, XML numeric mapping, identity/order and lease/early/nested/disposal qualification without changing original stores/pin |
| FR-05 | Core + both consumers | Agreed lazy/escaped-result ownership, cancellation/reentry and immutable semantic environment/cache behavior |
| FR-06 | Core + existing consumer owners | Refresh applicable Braid compatibility for this cohort; historical D01/native receipts do not accept a new engine pin |
| FR-07 | Core/release | Complete release package scope, reviewed NOTICE recipe or stock-pack equivalent, signing/publisher provenance and explicit publication authorization |
| FR-08 | Core + Schema + XmlDb | Explicit dated dispositions tied to exact final candidate and accepted manifest; no inference from local test passes |

All eight requirements remain Pending. The full XSD 1.1 **backend** may continue after a stable foundation is agreed, but every known **required foundation contract need** must be represented and qualified before freeze; a bounded decimal backend/profile cannot prove the full goal. Ordinary persistent XML must not acquire a required Schema dependency or storage migration merely to qualify XmlDb.

C45 remains [explicitly Pending](../conformance/k04-schema-compatibility-prototype/OWNER-DECISION.md). The current public APIs and bounded bridge are inputs; do not treat the entire 32-type prototype or every experimental/current-draft member as permanently accepted. Keep legacy compatibility and proposed neutral contracts in separate reviewed ledgers, without silently waiving old obligations.

## Owner execution and core follow-up

Schema and XmlDb owners execute their already delegated project-local work packages using this packet, returning exact revisions/profiles, package identities, requirement/member mappings, actual test/binary/lifetime results and unresolved gaps. Record responses as new dated evidence rather than filling old sealed receipts with inferred acceptance. No messages or changes to owner projects were made by this handoff step.

Core then reconciles those requirements into a finite C139-01/C45 contract decision and the named member-level freeze scope. Any new semantics first record immutable specification revisions, sections and deviations. Implement only accepted scope; refresh package/gate/consumer qualification when that candidate changes. Publication, signing, product adoption and joint API freeze each retain explicit separate disposition.

Verify this evidence snapshot from the repository root:

```powershell
python docs/foundation-readiness/tools/verify.py
```
