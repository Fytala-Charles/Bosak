# S009-02 — Provider facts, provenance and core-adapter mapping

> **Started 2026-10-30** (Schema owner work under REQ-009, registered in [REQ-009-OWNER-WORK-PACKAGES](REQ-009-OWNER-WORK-PACKAGES.md)).
> **Scope of this step:** internal provider-fact mapping onto the neutral requirements, plus the compatibility/lifetime fixture inventory. The production adapter binding remains blocked on the accepted core C139-01 contract shape ([core REQ-139](https://github.com/Fytala-Charles/Bosak/blob/checkpoint/restart-2026-10-29/docs/REQ-139-OWNER-WORK-PACKAGES.md)). This document adopts no core interface, freezes no API and claims no XSD 1.1 conformance. Parent REQ-009 remains In Progress.

## 1. Basis and boundaries

- Provider under mapping: the **native 0.9/v5 bounded document assessor** (`SchemaServices.CreateDocumentAssessor`), profile exactly as recorded in [DOCUMENT-ASSESSMENT](DOCUMENT-ASSESSMENT.md); candidate baseline `7e498a19`, registration `fd9f833` + `243cc60`.
- The mapping target (neutral requirement dimensions) is taken from the REQ-140 required Schema evidence and the C139-01 separation rules: query/XDM profile, XSD version, component authority/universe and source/snapshot identity stay **separate** dimensions at all times.
- Main-branch REQ-009 staged work (S1–S5c compiled descriptors, assertion/facet evaluation, CTA applicability, wildcard constraints) is separate add-side evidence; it is navigation context here, not provider fact for this candidate.
- Nothing in this document is new semantic adoption; the native assessment profile remains exactly the documented 0.9 candidate until a selected extension is adopted and implemented (S009-01).

## 2. Neutral requirement dimensions

| # | Dimension | Neutral requirement (mapping target) |
|---|---|---|
| D1 | Component / universe identity | A component carries an authority-scoped identity within a defined universe; equality of QNames, display names or digests alone never authenticates a component across schemas or processes. |
| D2 | Affiliation / derivation / closure | Types/elements expose their affiliation, derivation relationships and the closure of components reachable from a governing type; unsupported closure members refuse rather than disappear. |
| D3 | Original assessment / PSVI | The original assessment outcome and PSVI of a node survive every view, copy and transport; no view rewrites or re-derives the original state. |
| D4 | Actual typed values | Typed values transport as the actual datatypes with exact semantics (atomics now; list/union as full XSD 1.1 requires later); precision/rounding rules are explicit. |
| D5 | Compatible views and copies | Preserve and strip views, snapshots and copies retain or drop type state deliberately; a compatible view never rewrites original PSVI. |
| D6 | Lifetime / cancellation / concurrency | Ownership, disposal, cancellation and concurrency rules are explicit; cancellation never yields successful partial output. |
| D7 | Unsupported / refusal posture | Owned unsupported coverage refuses explicitly; there is no downgrade to a BCL XSD 1.0 validator and no invented W3C error names. |

## 3. Provider-fact map

| Dim | Actual provider fact today (0.9/v5) | Evidence | Foreign / not established | Must not be inferred |
|---|---|---|---|---|
| D1 | The assessor captures immutable schema text, original declaration/type objects and assertion plans per compiled assessor. Successful assessments own an `AssessmentIdentity`; `Copy()` creates an independent `SnapshotIdentity`. | [DOCUMENT-ASSESSMENT](DOCUMENT-ASSESSMENT.md) ownership section | No cross-process, cross-schema or persistent universe identity; no compatible-universe admission. | Process-local instance labels are not persistent source identities or compatibility proofs; **independent compilation creates independent anonymous component authority even when schema digests match**; QName/digest equality grants no admission. |
| D2 | Closure is exactly one named global element, one anonymous element-only complex type, and its bounded local decimal declarations plus assertions; sum of maxima ≤ 16; other constructs are outside the profile. | DOCUMENT-ASSESSMENT exact profile | Named types, type derivation (restriction/extension), substitution affiliation, list/union membership, identity constraints: none exist in the profile and no fact is established for them. | Do not infer list/union, general derivation, namespaces or broader structure from decimal count/presence examples; do not synthesize anonymous names. |
| D3 | Original declaration/type objects are retained; omitted optional declarations remain in `ChildDeclarations` without placeholder instance states; the root retains its anonymous complex type and has **no typed value** (FOTY0012 on atomization); decimal leaves retain governing local declaration and exact typed value; this profile never admits nilled elements. | DOCUMENT-ASSESSMENT ownership section | Nilled-element assessment, default/fixed injection, identity-constraint PSVI: not established. | A detached typed-value export is not a complete PSVI document. |
| D4 | Exact decimal lexical/value assessment after XML-whitespace collapse; non-decimal syntax is `Invalid`; valid values requiring rounding/overflow/underflow are `Unsupported` (not `Invalid`). `CreateTypedElementCopy` exports a detached built-in decimal value view. | DOCUMENT-ASSESSMENT values + engine projection boundary | List and union value construction, non-decimal datatypes, QName/NOTATION values: not established. | The detached decimal view is not local-declaration authority; no general typed-value transport exists yet. |
| D5 | `Copy()` preserves original PSVI, component references and identity without revalidation; `CreateDocumentCopy(preserveTypeAnnotations: true)` exports the whole document through the unpublished core 1.34.1 provider bridge (retain) and `false` exports stripped XML; edited content refuses stale values; namespace fixup/ignorable whitespace preserve state. | DOCUMENT-ASSESSMENT ownership + engine projection boundary | Compatible views across *different* schemas/universes; general PSVI serialization: not established. | A preserve/strip copy pair within one assessed snapshot is not a cross-schema compatibility proof. |
| D6 | Immutable compiled assessors process independent documents concurrently; each assessment owns a separate normalized snapshot; cancellation throws and produces no successful partial output; no session/resource is disposed on the caller's behalf; exported XML is caller-owned. | DOCUMENT-ASSESSMENT ownership section | Cross-session admission, session revocation/revocation-restore semantics (the Braid session design tracks those separately): not established here. | Concurrent *assessor* safety does not establish concurrent *snapshot* mutation safety; exported XML edits invalidate assumptions without touching the retained result. |
| D7 | The profile refuses everything outside the exact bounds; unsupported constructs are never dropped and sent to a 1.0 validator; legacy reassessment and declaration projection refuse explicitly. | DOCUMENT-ASSESSMENT profile + engine projection boundary | — | `Unsupported` means coverage was refused, not "passed elsewhere"; no silent downgrade exists to reinterpret. |

## 4. Compatibility and lifetime fixture inventory

Fixtures defined now (execution against a core candidate follows C139-02; defined ≠ executed):

| ID | Fixture | Expected outcomes to verify |
|---|---|---|
| F-01 | Supported-component round trip | Assess → native `Copy()` → original PSVI, component references and `AssessmentIdentity` preserved; independent snapshot identity; no revalidation side effects. |
| F-02 | Foreign/unsupported components | Constructs outside the profile (named type reference, attribute use, wildcard, open content, alternative, nil request, list/union value) are refused explicitly; no BCL 1.0 reassessment is invoked; outcome is `Unsupported`, never silent pass. |
| F-03 | Original-source identity | Original schema/instance objects, declaration/type objects and assertion plans are not mutated by assessment, copies, exports or concurrent assessments of other documents. |
| F-04 | Preserve/strip copies | `CreateDocumentCopy(true)` retains state through XQuery/XSLT preserve; `false` removes it; stale-value use after export edits refuses. |
| F-05 | Cancellation and disposal | Cancellation mid-assessment throws, leaves no partial successful result, retains caller ownership of resources. |
| F-06 | No-downgrade witness | A deliberately unsupported schema shape is observed to produce `Unsupported` end-to-end (compile and, where reachable, assessment), proving no 1.0 fallback path exists. |

Fixture inputs must be owner-authorized/public and hash-pinned when executed; happy-path and edge cases are both required per suite rule.

## 5. Blockers and next actions

1. **Blocked (external):** production adapter binding until core C139-01 records the accepted neutral contract (minimal C45 disposition first). Proposed core types or QName equality alone are not component authority.
2. **Done (internal, 2026-10-30):** fixture inputs for F-01…F-06 are hash-pinned in [S009-02-FIXTURE-IDENTITIES.json](S009-02-FIXTURE-IDENTITIES.json) against the 451-case suite source at commit `31f6661`. Next internal: keep the mapping current as S009-01 selects the next bounded slice (currently blocked on an owner fixture); supply this map as provider-fact input to C139-01 review.
3. **Not in scope:** general session/API acceptance, freeze sign-off (S009-04), package publication (S009-03), any new native grammar.

---

*Started 2026-10-30. Mapping only — no runtime behavior, test execution or API adoption is claimed by this document.*
