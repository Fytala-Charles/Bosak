# Schema follow-up — separate design decisions from delivery evidence

Requested 2 November 2026 against BCAP proposal `ff475d712f6ef6aa5033113cc39afb887ae4cbad24705098c5c9cb2097641688`. Existing Schema O1/O2 remain **Revise** until Schema supplies a revised disposition. Core does not require full XSD 1.1 implementation or a shipping public SDK merely to accept a scoped R2 shape; no acceptance is inferred from an empty signature-gap list.

## S1: review the available native lane

The active public checkout 20090180755d0427d824b091cf838e9a03ca9adc and the native assessment lane are separate. Native source is available at:

`D:/Development/Bosak/.worktrees/Bosak.Schema.Assessment`

Its exact c3624998eca8c83018e121fa84af611f834a0501 source was clean and hash-pinned in [the Schema response review](../foundation-schema-response-review/INPUTS.json). Read that worktree's AGENTS.md and DOCUMENT-ASSESSMENT.md before any changes. Its commit/push rules remain in force; this request does not authorize publishing owner changes.

Review `src/Bosak.Schema/Assessment/AssessedDocument.cs`, `XsdDocumentAssessor.cs`, `docs/S009-02-PROVIDER-FACT-MAPPING.md` and `docs/S009-04-FOUNDATION-REQUIREMENTS.md`. Copy/CreateDocumentCopy preserve or strip existing assessment state, but neither proves the final before-getter membership/issuance adapter by itself. Map the actual trusted owner/snapshot/node/facts/origin checks, copy association and stale-edit policy, or give the concrete adapter feasibility plan and exact missing Core member if one is necessary. Do not authenticate labels/QNames/digests or copy/materialize a graph merely to admit it.

## S2: disposition the explicit-limit design

Core BCAP-O2 accepts **required caller-supplied finite limits and no implicit defaults**. SchemaQueryLimits requires positive snapshot/cache/atom/digit counts and nonnegative scale; the default struct is invalid. XdmExactDecimal takes explicit supplied coefficient-digit/scale capacity and checks before normalization; formatting has an explicit output capacity. Already allocated caller BigInteger memory is not retroactively bounded. This selects no numeric workload budget or global unlimited fallback.

Parsing/arithmetic/casts/map-key/serialization enforcement and the six public numeric intents remain R3 implementation obligations, not currently implemented production support. Real workload ceilings/default choices and native performance acceptance remain R4. Confirm whether the explicit finite/no-default design is sufficient. If not, quote the precise missing policy/observable setting or required public member and explain a concrete Schema operation that cannot be expressed. Do not leave 'defaults unselected' as an ambiguous blocker when absence of defaults is intentional.

## S3: return an actionable scope disposition

For each remaining O1/O2 item, classify it as:

| Class | Required answer |
|---|---|
| R2 API/policy blocker | Exact required member/semantics or an infeasible contract, with source/profile reason. This prevents design acceptance. |
| R2 source feasibility | Actual method mapping or a bounded positive/foreign/copy/stale/revoked/issuance plan on the pinned native lane. It need not claim executed R4 tests. |
| R3 implementation | Contract behavior already represented but not yet implemented/enforced. Track it explicitly; do not call the throwing proposal a shipping SDK. |
| R4 qualification | Actual native list/union/nil/PSVI/compatibility, lifetime/worker and workload evidence. Supported/refused profiles remain explicit. |

Preserve all known full-XSD foundation requirements, even where the bounded decimal backend returns Unsupported. Source-specific implementation gaps must not silently erase representation requirements. Conversely, unsupported wider profiles and no new signature gap do not compel Schema to accept: give AcceptWithScope only if the owner agrees to the exact supported/refused design, otherwise keep Revise with a precise outstanding design/feasibility action.

Submit revised BCAP-O1-Schema and BCAP-O2-Schema using the same exact manifest and original question IDs, plus the classified action list. Preserve the earlier responses as historical evidence. Core will review the actual source/profile disposition; no message, product pin, native mutation, C45/freeze or release acceptance is requested here.
