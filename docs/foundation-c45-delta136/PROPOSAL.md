# Exact C45 declaration proposal

31 October 2026. Review candidate, not accepted SDK or implementation. The [provider declarations](proposal/Provider.cs.txt), [lifetime interfaces](proposal/OperationLifetime.cs.txt) and [query facade](proposal/QueryFacade.cs.txt) compile into two isolated proposal assemblies against the actual 1.36.0 binaries. They add 40 proposed public types: 36 for Core and 4 for API. None is exported by the product.

Provider declarations retain V5's 34 types, relocated from C13901.Draft to proposed Bosak.XPath.Core.Schema. The atomic observer documentation explicitly covers current type evidence even when OriginalAssessment is null. Existing node/sequence/options/expression interfaces gain no required slots and no public arbitrary attachment factory is introduced. Core references only framework/XDM contracts; the API facade references Core and Runtime's existing EvaluationContext, with no Runtime dependency on API or concrete Schema/XmlDb/Braid dependency in Core.

| Proposed boundary | Exact selected review shape |
|---|---|
| Provider/component authority | ISchemaAuthority lookup, ownership, comparison, derivation, affiliation and complete closure; SchemaResult distinguishes Supported/NotFound/Unsupported/Invalid in the applicable operations. |
| Typed values/node facts | Governing type, IsList, ordered plain cells plus selected actual type/optional origin; issued-container checks before reading; optional ISchemaAnnotatedNode channel. Atomization flattens values; the original node/container retains list boundaries, including empty lists. |
| Assessment/compatibility | Engine-issued semantic environment, borrowed bound advisor, assessment request/result/diagnostics and directional admitted compatibility view preserving original PSVI. |
| Owner lifetime | New optional IQueryOperationLifetime.Enter and IQueryOperationLease.ThrowIfUnavailable/Dispose, over resources already acquired by their owner; no new IDisposable requirement on legacy nodes/sequences. |
| Finite host limits | SchemaQueryLimits(maxOpenSnapshots, maxCachedValuesPerSnapshot, maxTypedValueAtoms); all must be positive, default struct invalid. No numerical workload defaults or budgets are selected. Field layout is not accepted by this shape compilation. |
| Engine producer | Sealed SchemaQuerySession.CompileXPath returns SchemaResult<SchemaQuerySession>; captured Environment and optional Advisor; no public constructors for session/snapshot/operation. |
| Snapshot admission | OpenSnapshot accepts guarded input, actual owner lifetime, optional original assessment and optional explicit compatibility view. Disposal revokes engine admission without disposing caller-owned resources. |
| Multi-source operation | Run/RunAsync accept ImmutableArray<SchemaQuerySnapshot>; GetRoot identifies a member of that admitted set. Default/duplicate/foreign sets must reject; empty sets allow source-free work. Composite acquisition/release ordering and failures require C45-08 execution. |
| Typed execution | Callback-borrowed ReadTypedValue, ConstructSimpleValue and Evaluate. The returned graph is not recursively detached; deferred consumption occurs inside the raw callback/task or a separately selected owned profile. Ordinary legacy XmlDb APIs retain their own scope. |
| Close/drain | Dispose revokes new engine admission; DrainAsync waits for raw work. Canceling a wait does not end raw work or free its borrowed resources. |

Only declaration compilation and metadata inspection ran. Public facade operations throw Proposal only if invoked. The default scalar interface method and value-cell constructor remain V5 proposal code; their old test-double receipts are not new production qualification. Positive-limit validation, real binding, multi-source leases, assessment/constructor routing and atomic observer behavior are not implemented here.

## Required unresolved decisions before acceptance

1. **Full numeric domain/precision:** current XdmValue integer/decimal public access is bounded by long/decimal. The proposal's XdmValue cells do not establish arbitrary-precision/full-XSD numeric transport. Select exact representable domains and public access semantics before freeze; unsupported Core representation must not be silently rounded or reported as invalid schema data. Any required representation/API delta must be resolved now.
2. **All hosts:** only the XPath session facade is specified. Exact XQuery/XSLT opt-in and typed transform entry points remain required for N4 and actual Braid preview; QueryHostLanguage values alone do not supply them. This proposal is not a complete all-host C45 surface.
3. **Closure granularity:** V5's ImmutableArray closure is retained as an explicit review option, not an accepted eager-memory requirement. Decide complete bulk versus cursor/paged behavior from representative owner PF workloads before freezing that signature.
4. **Ownership/limits:** validate the multi-snapshot, prefix/nested/deferred and close/drain design against actual providers/workers. Synchronous lifetime entry admits already-owned resources; it must not force synchronous resource acquisition/I/O. Return a required async/bulk capability gap if demonstrated.
5. **Assessment and constructor routing:** select assertions/CTA host capture, legacy/native coexistence and general nested constructor dispatch with actual immutable semantic adoption and provider execution. Explicit ConstructSimpleValue compilation does not prove those paths.

All are pre-freeze obligations, not hidden backend exclusions. [ACCEPTANCE.json](ACCEPTANCE.json) has 12 concrete NotRun cases; [the requirement map](evidence/REQUIREMENT-MAPPING.json) connects 43 owner-review rows to existing IDs or proposed signatures without claiming semantic coverage. Owners may request changes; no declaration is frozen or pre-approved.
