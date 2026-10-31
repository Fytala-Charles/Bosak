# C139-01 draft: typed transport and operation ownership — 31 October 2026

This is the first concrete signature slice, not the complete production foundation API. [SIGNATURES.cs.txt](SIGNATURES.cs.txt) contains 13 isolated draft types in `C13901.Draft`, compiled against the exact 1.34.2 Core package DLL. Production namespace placement, constructors/factories and member acceptance remain Pending. Existing `IXdmNode`, sequence interfaces and `XdmValue` are unchanged.

The slice answers the transport/authority/ownership part of the [owner reconciliation](../foundation-owner-reconciliation/README.md) and supports [Braid BF140-01–05](../braid-foundation-requirements/README.md). It deliberately does not pretend that these 13 types finish N1–N8 or C45. [Performance PF-01–09](../foundation-performance/README.md) remains mandatory before final signatures.

## Exact proposed operations

The source file is the exact proposed declaration inventory; the following table explains its intentional behavior.

| Entry point | Proposed contract |
|---|---|
| `ISchemaAuthority.Owns(component)` | Verify issuance/membership at the already admitted provider authority. A handle's `Authority` property is a claim to check, not self-authentication. An authority instance identifies one immutable universe; independently compiled universes are distinct unless separately admitted. No public issuer factory is selected. |
| `IsSameComponent(first, second, token)` | Ask the owning authority; do not require pointer-equal handles or mandatory interning. Compare names/digests neither for authority nor for anonymous identity. |
| `IsDerivedFrom(actual, expected, token)` | Return a complete supported positive/negative relation or explicit Unsupported. An unknown relationship is not false. |
| `IsAffiliatedWith(member, head, token)` | Add the declaration-affiliation relation missing from a type-only derivation seam. Substitution eligibility/blocking details still need descriptor/profile review. |
| `GetComponentClosure(governingType, token)` | Return complete reachable original components or Unsupported. An ImmutableArray is a draft bulk shape, not permission to truncate or a frozen eager-memory decision; experiment E-03 compares it with paged/cursor alternatives. |
| `ISchemaAnnotatedNode.OriginalSchemaFacts` | Optional node channel; ordinary XML providers need not implement it. Facts retain original governing type/declaration/source assessment and nil/no-typed-value facts. Existing node flags/defaults remain compatibility obligations. |
| `ISchemaNodeFacts.ReadTypedValue(token)` | Read under the caller-held operation/snapshot lifetime. Preserve the declared governing type and `IsList`, including empty/single-item lists, plus ordered atomic values. No-typed-value is distinct from an empty list or nilled empty value. |
| `SchemaAtomics.TryGetProvenance(in value, out provenance)` | Observe original declared and actual atomic type (including selected union member), origin and source binding. It neither invokes validation nor acquires a lease. Its shape-only body refuses; no annotation attachment operation is accepted yet. |
| `IQueryOperationLifetime.Enter(token)` | Optional admission seam; obtain an operation lease before provider work. Existing nodes/sequences acquire no new required member or implicit resource ownership. |
| `IQueryOperationLease.ThrowIfUnavailable(token)` / inherited `Dispose()` | Recheck availability where deferred provider work resumes; release only this admission once. Disposal is idempotent by proposed contract and does not close a borrowed snapshot or caller stream. |

Opaque origin/source interfaces describe original provenance. They confer no authority, cross-process identity or live-resource guarantee. A copied snapshot has its own provider identity and lifetime; original assessment provenance remains original. Public interface implementation is not cryptographic authentication: only the host-admitted provider is trusted and its membership checks must reject forged/foreign handles. Immutable facts and payload/type consistency are construction requirements still to be enforced and tested.

## Atomic representation decision

**A — internal annotated payload, unchanged outer XdmValue layout:** retain ordinary no-schema values exactly as now; represent annotation in the existing reference storage on an opt-in path. Inline boolean/integer/double data can retain inline fields. Reference-backed values need a payload/provenance holder and audited unwrapping. Current `StringValue`, `DecimalValue`, QName/date access, EffectiveBooleanValue and string conversion already cast `_reference` directly. Simply storing annotation there would break them. Exact source pins are in [SOURCE-INPUTS.json](evidence/SOURCE-INPUTS.json).

**B — separate atomic transport wrapper:** keep `XdmValue` entirely untouched and pair a payload with provenance at the provider boundary. This makes ownership explicit but the current VM, maps, arrays and function signatures consume XdmValue. Without a complete propagation design, unwrapping at entry loses provenance and changing every public value boundary risks another API break. A parallel carrier is not accepted merely because its declarations compile.

A is the leading experiment because it could preserve the outer ABI and existing query signatures. It is **not selected for production**. Measure annotated allocation/access costs and inspect every carrier operation first. Reject both alternatives if they cannot preserve original provenance through required casts, atomization, equality/hash, comparisons, map/array/function transport and copy/serialization boundaries. Serialization carries a representation, not a transferable live schema authority; deliberate strip or refusal must be explicit. Do not fabricate provenance for a newly computed value; derivation/preservation rules need immutable semantic adoption before code.

`ISchemaTypedValue.Atomics` requires per-atomic provenance to survive transport; a single list-level annotation is insufficient. Actual values remain numeric/string/etc. rather than External objects, and provenance must not accidentally change XPath value equality or map-key semantics. The existing decimal rounding limit cannot be hidden by carrier metadata. New arithmetic/provenance semantics are not adopted by this design document.

## Ownership and errors

The host/provider owns the actual compiled assessor/session, snapshot and any reader/generation lease. An operation lease accounts for admitted work only; it never manufactures snapshot ownership. State progression is Active → Closing → Released: closing refuses new admission; admitted work retains its actual owners until completion or a supported cancellation path ends it. Canceling a wait is not evidence raw work ended. A host closes actual resources only after its admitted work and permitted deferred/nested use finish. No forced collection or unload-completion promise follows.

A result graph is borrowed unless a separately explicit owned/detached API is selected. Keeping an operation lease in a CLR reference does not keep a caller-disposed snapshot usable. Disposing the operation lease ends its admission; an escaped value cannot use it to resume work. Cached scalar/Current/default metadata may remain readable, while persistent node dereference checks its original live snapshot. Do not introduce universal all-access invalidation or recursive disposal through containers.

The core has no new blanket thread-safety promise. Initial experiments use sequential evaluation; overlap, callbacks, close-versus-dispatch, cancellation and reentry need explicit schedules. Braid's process owner must define how worker termination releases worker-owned versus broker/parent-owned resources; live session/lease handles cannot be JSON-serialized. Core semantic capture and Braid operation admission are related boundaries, not the same identity or lease.

| Condition | Proposed result/error boundary |
|---|---|
| Supported false relation or empty closure | Supported false/empty; never Unsupported by convention. |
| Missing named component | NotFound on future lookup operations only; not a negative relation or incomplete closure. Lookup signatures are outside this slice. |
| Unsupported owned operation/representation | Unsupported with provider diagnostic; do not downgrade to BCL or invent a W3C code. |
| Cancellation | OperationCanceledException; no successful partial payload. |
| Foreign/forged handle | Reject before dispatch; ArgumentException for invalid contract arguments, rather than guessing compatibility. Cross-universe admission needs its separate contract. |
| Closed admission/disposed resource | ObjectDisposedException on the owned live-work boundary; cached metadata access is not retroactively invalidated. |
| Invalid assessed XML | Outside this authority-query result enum; final assessment outcome must retain Valid/Invalid/Unsupported separately. |
| Node with no typed value | Preserve existing HasNoTypedValue / FOTY0012 atomization obligation; ReadTypedValue is not a way to silently return an empty list. |

The exact diagnostic mapping, constructor validation and exception precedence on close/cleanup need final disposition. No new invalid/cancellation success enum is copied from the older prototype.

## Requirement coverage and remaining signature gaps

| Owner needs | This slice contributes | Still needed before complete C139-01 / freeze |
|---|---|---|
| Schema N1/N8; D1 | Authority-owned handles, original source/origin separation, authoritative comparison | Exact semantic profile/XSD/static environment and persistent source/snapshot descriptor contracts; real positive/foreign identity cases. |
| Schema N2 | Derivation, affiliation and complete-closure queries | Named lookup, component kind/descriptor/blocking semantics and real provider evidence; bulk-versus-cursor choice. |
| Schema N3/N5 | Original node facts, declared/actual atomic provenance, list boundary | Accepted carrier representation, construction and full propagation; remaining required PSVI flags/defaults and provenance/copy rules. |
| Schema N4/N6 | Original facts are not rewritten by a view | Assessment/advisor entry points, compatible-universe admission and directional views are not delivered here. |
| Schema N7; XmlDb FXR-06–10/14, FXG-02/03/05 | Optional operation admission with borrowed resources | Real lease/cancellation/deferred-result schedules; no claim of fixing or replacing current provider behavior. |
| XmlDb FXR-01–05/11–13/15 | No required node/sequence/layout/enum change; no mandatory Schema dependency | Retain exact existing source/package/binary observations; changed production candidate requires refresh and final profile/typing disposition. |
| Braid BF140-01–05 | Engine-neutral provenance/operation boundary and explicit process ownership constraints | Real adapter selection, captured environment, input routes, versioned worker/fingerprint/currentness and end-to-end qualification. |

## Experiments and evidence

[EXPERIMENTS.json](EXPERIMENTS.json) defines four finite experiment packages with discriminating controls. E-01 carrier cost/layout, E-02 propagation and E-03 authority-operation granularity precede the final representation/signature decision; E-04 exercises lifetime and Braid process boundaries. All remain NotRun. No workload budget is invented; use PF-01–09 owner baselines and targets.

Fresh declaration compilation succeeds with **zero warnings/errors**, XML documentation enabled and warnings treated as errors. [SURFACE.json](evidence/SURFACE.json) records 13 draft types and current-carrier metadata. Metadata inspection loads the exact package Core hash `bcd14bcf…21c2fa`; on this 64-bit .NET 10 runtime the current XdmValue size is 48 bytes. This observes the unchanged baseline, not a candidate carrier or portable layout guarantee. Shape compilation does not prove behavior, binary replacement, owner acceptance, performance or full-XSD support.

Reproduction: extract the Core DLL from the feed named in [INPUTS.json](evidence/INPUTS.json), copy SIGNATURES.cs.txt to Draft.cs and use [Draft.csproj.txt](evidence/Draft.csproj.txt) with [NuGet.Config.txt](evidence/NuGet.Config.txt), in an isolated temporary directory. The metadata inspector source/project and build logs are retained. No product reference or package source is changed.

The next step is E-01/E-02 in an isolated prototype with explicit provenance laws, then the remaining exact profile/advisor/compatible-view signatures. Production implementation waits for the finite C139-01 disposition and required immutable semantic adoption. Acceptance, freeze, release and product adoption remain Pending.
