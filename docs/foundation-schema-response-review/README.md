# Schema revised returns — 2 November 2026

Schema has now submitted **both O1 and O2**, each with decision **Revise**. Both pass the exact-manifest validator and their parsed JSON matches the [published submission comment](https://github.com/Fytala-Charles/Bosak/issues/468#issuecomment-6573672774). They identify **no additional required public signature**, but they do not accept the design. The designated signer is not independently authenticated; no native, C45, freeze or product acceptance is claimed.

| Return | Main remaining issues |
|---|---|
| [BCAP-O1-Schema](inputs/BCAP-O1-Schema.json) | Concrete native membership check before foreign getters; authenticated node/facts/input/origin and preserve/strip mapping; stale/edit/revocation and typed issuance feasibility. The response supplies a six-case plan but not executed native evidence. |
| [BCAP-O2-Schema](inputs/BCAP-O2-Schema.json) | Explicit finite numeric policy and supported/refused profiles; actual list/union/PSVI, authority/context/lifetime and directional compatibility evidence. Six numeric intents and the represented distinctions are retained, with no new member requested. |

## Source access and policy findings

O1 says the native assessor is unavailable in the reviewed public checkout. That checkout is correctly identified as 20090180755d0427d824b091cf838e9a03ca9adc, but the separate native candidate **is available here**, clean at the exact cited c3624998eca8c83018e121fa84af611f834a0501:

`D:/Development/Bosak/.worktrees/Bosak.Schema.Assessment`

Its AssessedDocument, XsdDocumentAssessor, DOCUMENT-ASSESSMENT and S009-02 provider mapping are hash-pinned in [INPUTS.json](INPUTS.json). Copy/CreateDocumentCopy and original state are visible there; this does not by itself establish the final OwnsNode/typed issuance adapter or its negative cases. The bounded native decimal lane and public staged backend must remain distinct. No assessment worktree file was modified.

O2's concern that defaults remain unselected needs a precise design distinction: SchemaQueryLimits already **requires explicit finite caller limits**, declares the default struct invalid and deliberately selects no workload defaults. Runtime enforcement/numeric implementation belongs to R3; actual provider cases and accepted workload budgets belong to R4. Full XSD 1.1 implementation is not a prerequisite merely to disposition an API shape if unsupported profiles are explicit. Schema's Revise must nevertheless remain recorded until its owner accepts the scoped contract or names the precise remaining policy/shape blocker; an empty signature-gap list is not acceptance.

## Consolidated status and next step

The revised draft now has **5/7 submitted: two scoped design accepts and three Revise**:

- Braid O3 and XmlDb O1: AcceptWithScope, unchanged from the previous review.
- Schema O1/O2 and XmlDb O4: Revise.
- Only revised Core O2/O4 remain unsubmitted.

Next Core completes its own two reviews and clarifies the explicit-limit policy. Schema should use the exact assessment source to finish its source mapping and distinguish R2 design blockers from later implementation/qualification work. XmlDb's native cleanup/fault/lifetime revision remains independent. Braid's bounded capture signature need stays resolved at design level. R2, public implementation, native qualification, freeze and publication remain gated; no acceptance is inferred from these submissions.

Original incoming files and prior reviews are preserved. This is document/source-evidence review only: no source changes, tests rerun, owner messages, product pin changes or production promotion. [Current ledger](STATUS.json); run `python docs/foundation-schema-response-review/tools/verify.py`.
