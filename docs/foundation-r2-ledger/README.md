# R2 Core public-contract decision ledger

1 November 2026. Core selection is recorded for **45 proposed types, all 615 baseline metadata/evidence rows, six numeric member intents and all 43 owner requirement rows**. [LEDGER.json](LEDGER.json) is the complete trace; [type dispositions](TYPES.md), [requirement dispositions](REQUIREMENTS.md) and [decision rules](DECISIONS.md) provide readable views. **R2 is in review, not complete:** Schema/XmlDb/Braid acceptance and exact supported-profile decisions remain pending. Production remains unchanged 1.42.0.

## Concrete review draft

Four isolated declaration assemblies compile with zero warnings/errors against the qualified 1.42.0 engines. Repeated metadata inventories are identical: **45 types / 619 rows**. Rows include attributes and layout evidence, not 619 callable APIs. All 45 previous type identities remain. No proposed API body was executed, no public production export was added and no new friend grant or production project dependency was introduced.

Three types are revised with four explicit method changes:

| Proposed boundary | Selected review revision |
|---|---|
| ISchemaAssessmentInput | Add `bool OwnsNode(IXdmNode node)` to establish actual source membership before node getters; it does not issue schema authority. |
| ISchemaAssessmentAdvisor | Add `bool OwnsNode(IXdmNode node, ISchemaNodeFacts facts, ISchemaAssessmentInput input, ISchemaAssessmentOrigin origin)` to authenticate the association rather than facts alone. |
| ISchemaAssessmentAdvisor | Replace the one-argument issuance check with `bool OwnsTypedValue(ISchemaTypedValue value, ISchemaAssessmentInput input, ISchemaAssessmentOrigin? originalAssessment)`. |
| SchemaQueryOperation | Replace the implicit-source read with `SchemaResult<XdmValue> ReadTypedValue(SchemaQuerySnapshot snapshot, ISchemaTypedValue value)`. |

The first check is trusted host/provider source membership; the second and third are schema issuer checks. Neither substitutes for the other. Issued facts copied onto a foreign node cannot authorize it. The source argument must be an actual operation member before value-container getters are consulted. Fresh simple construction remains separate and cannot mint original assessment.

The [draft sources](proposal/Provider.cs.txt), [operation facade](proposal/QueryFacade.cs.txt) and [metadata delta](evidence/DELTA.json) make this reviewable. Ordinary XML uses the existing proposal's optional assessor/origin and neutral input/lifetime concepts; no fake schema assessment or mandatory Schema dependency is selected for XmlDb. Existing IXdmNode/sequence implementers acquire no new required member. All other proposed types retain their declarations subject to the explicit rules in DECISIONS.md.

## Selected scope and limits

Retain exact numeric API intents, atomic/list/union representation, original PSVI, authority/closure, assessment/advisor and directional compatibility contracts. Backend refusals are explicit; they do not erase required future API needs. No full XSD 1.1 backend completion is required merely to represent these needs. Exact numeric execution is not implemented by compiling its declarations.

Retain callback-owned same-session snapshot sets and the existing lifetime/lease concepts, without a universal provider disposal method or new resource manager. Raw tasks retain actual owner resources through completion; canceled waits do not close them. Keep dynamic Evaluate/Transform context parameters, but enforce bounded copying/membership/resource policy internally. No new public capture/request factory is selected. Borrowed raw graphs are not recursively guarded or automatically detached.

Document-root XSLT match/global roles and source-free named templates form the initial bounded profile. Unsupported modes, mutable resource/package services, arbitrary initial sequences and streaming refuse. Any known required broader Braid signature must be resolved before freeze. No caller may mistake an engine reference, a QName/digest or a successful synthetic fixture for authority, a native lease or owner acceptance.

## Owner dispositions still required

| Return | Concrete question / evidence | Blocks |
|---|---|---|
| O1 Schema + XmlDb | Can actual input adapters establish source membership without eager graph copying, and can Schema authenticate preserved/copy node facts and typed containers against the revised associations? Return exact method/profile exceptions and positive/foreign cases. | R2 signature/profile closure; R3 adapter implementation |
| O2 Schema + Core | Confirm numeric narrowing/precision, list/union, advisor and compatible-view semantics against N1–N8. Record supported/refused profiles without hiding a required signature gap. | R2 semantic scope; R3 typed implementation |
| O3 Braid | Confirm bounded invocation signatures cover required previews, or identify exact missing initial-input/mode/worker-currentness operations before freeze. Process route and transport remain owner decisions. | R2 known signature completeness; R4 consumer qualification |
| O4 XmlDb + Core | Confirm actual acquisition failure preservation, reader/generation retention and finite cancellation/reentry/concurrent-close scope; fix or explicitly disposition the source-level cleanup masking risk. | R2 ownership profile; R4 actual resource acceptance |

These are concrete review returns, not newly sent messages or external repository edits. Original owner acceptance fields remain Pending. Core selection cannot supply another owner's sign-off. The isolated build is signature evidence only; actual adapter feasibility and runtime behavior remain unqualified here.

## Finite implementation queue after disposition

1. **W1 Ordinary source admission:** implement the selected neutral input/lifetime boundary without fabricated assessment. Test missing/foreign/ambiguous nodes before getters, source-free work, acquisition rollback and actual XmlDb snapshots.
2. **W2 Authority and original facts:** implement opaque identity and exact association checks, preserving original PSVI and copy/strip behavior; reject forged and independently compiled handles.
3. **W3 Values and numeric execution:** implement selected exact representations and all six numeric members, scalar/list/union limits, atomization/casts/arithmetic/map keys/serialization and narrowing/error regressions. Record immutable semantic adoption before code.
4. **W4 Advisor and compatibility:** bind after effective context capture, validate assessment outcomes/diagnostics and implement supported consultation/proof routes with explicit Unsupported elsewhere.
5. **W5 Public all-host operations:** connect the qualified private machinery to the selected surface, enforcing bounded contexts, XSLT roles, raw lease/drain/failure behavior and resource policies.
6. **W6 Candidate inventory/package handoff:** refresh exact public/ABI inventories, complete engine gates, migration documentation and immutable package provenance. No speculative new batch/cursor/group API.
7. **W7 Actual owner/performance acceptance:** run Schema/XmlDb/Braid source and unchanged-binary consumers plus representative PF-01–09 budgets on that one candidate, then seek the four explicit C45/freeze dispositions.

W1–W6 remain gated by the corresponding R2 accepted implementation scope; the ledger does not override the existing C139-01/C139-02 start conditions. Fixture/package planning can proceed in parallel. No new release version/date or publication approval is selected.

## Evidence

[Review pins](REVIEW-INPUTS.json), [build inputs](evidence/INPUTS.json), [repeated inventory](evidence/api-0.json) and [delta](evidence/DELTA.json) preserve exact identity. The 1.42.0 production source and engine hashes remain unchanged; prior 24,390 unit passes and frozen gates are inherited receipts. No full engine test run was repeated for this declaration/documentation-only step. Original proposals and their acceptance templates remain sealed.

Read-only verification: `python -B docs/foundation-r2-ledger/tools/verify.py`. It checks complete coverage, exact metadata changes, draft/engine hashes and pending acceptance. It does not establish semantic correctness or owner approval.
