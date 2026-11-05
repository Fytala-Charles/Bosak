# V3 assessment/advisor and compatible-view proposal ? 31 October 2026

The [V3 declaration file](SIGNATURES-V3.cs.txt) completes the previously missing **proposed signature families** for query-context capture, provider assessment and directional compatible views. It extends the 13-type [V2 transport/ownership draft](../foundation-provenance-lifetime/README.md) to a 35-type review surface. This is not approval of 35 permanent Core types: host operation interfaces still have placement pending, and exact stable-member selection remains open.

Fresh compilation enables XML documentation and treats warnings as errors: zero warnings/errors. Three repeated runs pass **54 distinct synthetic protocol controls**. The test doubles inject assessment outcomes and whitelist endpoint pairs; they do not validate schemas, capture a real executable's context or compute actual schema compatibility. [RESULTS.json](RESULTS.json), [compiled surface](evidence/SURFACE.json) and [requirements map](REQUIREMENT-MAP.json) retain the boundaries and remaining work.

## Shape and routing decisions

| Family | Proposed boundary |
|---|---|
| Captured query semantics | IQuerySemanticEnvironment retains actual host/version, immutable XDM revision, behavior profile and normalized IQueryStaticContext. Immutable dictionaries copy namespace, variable and enumeration mappings; defaults, base URI, collation and compatibility/normalization flags remain explicit. |
| Schema closure | ISchemaAssessor represents provider-prepared schema compilation with its own authority, XSD language and exact coverage profile. Bind produces an advisor for one engine-issued query environment. Core does not acquire a dependency on concrete Schema or implement its validator. |
| Assessment episode | ISchemaAssessmentEpisode supplies assessment-constant date/time and implicit timezone. This is separate from query static context, schema language and source/snapshot identity. |
| Input | ISchemaAssessmentInput borrows a root and separate source/snapshot tokens with a live/currentness guard. A strong reference or token is not a reader/generation lease. No required IXdmNode/sequence member or persisted XML kind changes. |
| Native assessment | Advisor.Assess receives input, mode, optional governing handles and episode. Provider authority validates handles and performs the assessment. Native assertions/alternatives remain Schema-owned. |
| Simple construction | Advisor.ConstructSimpleValue returns supported typed atomics/list metadata, Invalid or Unsupported. Fresh target-type evidence does not imply an original document assessment. The carrier and actual constructor/error semantics are still pending. |
| Lookup and result distinction | Authority gains ResolveType/ElementDeclaration/AttributeDeclaration. NotFound is a supported missing lookup; Unsupported is a missing capability. Generic SchemaResultKind gains Invalid only for owned simple construction. Supported false/empty relations are not refusals. |
| Diagnostics | Optional source/snapshot/node/URI/line/original-byte location descriptors and caller-tightened count/text budgets. Truncation is reported independently from assessment completeness. |
| Compatibility | Establish takes exact source and target advisors plus the target execution environment. It returns Compatible with a view, Incompatible from a supported proof, or Unsupported when proof is unavailable. The view has source-type derivation and declaration matching operations. |

The candidate API integration point is an additive native-assessor option on compilation, delegating bind/capture to the lower layers. Its final member name/placement and host wrappers are not implemented or accepted here. An executable must retain its accepted advisor/environment rather than silently reading a later mutable EvaluationContext. Enabling both native and legacy BCL schema routes requires an explicit disposition; do not invent fallback or priority. No-provider/default behavior and existing XmlSchemaSet APIs stay compatible. JIT/backend choices belong in artifact/build identity, not a substitute for semantic capture.

## Assessment invariants

- Query environments must come from actual engine/host capture; callers cannot authenticate a profile by supplying equal strings or a digest. The diagnostic uses an admitted-instance registry only as a stand-in for that producer. Immutable public getters alone do not enforce authenticity or correct normalization.
- A compiled schema authority can be reused for several environment bindings without conflating the bindings or requiring schema recompilation per expression. Reuse and cache bounds need real performance evidence.
- XSD assertion evaluation must follow its own schema-defined static/dynamic context, not the query's arbitrary namespaces, compatibility mode or resource capabilities. The exact [5 April 2012 Recommendation](https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/) ?3.13.4.2 fixes empty document/collection sets and assessment-constant time settings. [SPEC-BASIS.json](SPEC-BASIS.json) records ?3.12/3.13 as proposed design sources; no validator semantics or full XSD adoption is implemented.
- At ingress, validate request mode, required/foreign/wrong-role handles and episode bounds, then snapshot episode values once. The input must stay immutable or version-guarded through assessment and publication; an availability Boolean without actual snapshot authority is insufficient. Before/after checks do not certify arbitrary mutation/publication races.
- Only complete Valid evidence can expose ValidatedRoot to the ordinary validated-query path. Invalid and Unsupported have no such root. Optional original evidence retains invalid or partial assessment information; it is not silently promoted to valid PSVI. Concrete Schema may retain its full model behind the evidence boundary; Core is not that model.
- OwnsAssessment verifies origin issuance. OwnsNodeFacts verifies the immutable fact object itself, so copying valid handles/origin into a caller-implemented interface cannot authenticate forged facts. Production value/type consistency and fact construction need their actual provider design, not a public arbitrary attachment factory.

Diagnostic locations bind the actual source and snapshot; they grant no edit/assessment authority. Original-byte start/length must both be present or absent, nonnegative and within the retained original bytes; line/column conventions are one-based. Do not fabricate byte extents from reparsed/normalized XML. Host projection must validate currentness, redact sensitive URI/message data and refuse unavailable mappings. A location may be absent when no truthful mapping exists. The shape/control does not prove any actual source range.

Diagnostic limits may tighten finite provider defaults; omitting them is not permission for unbounded output. Diagnostic truncation does not by itself make validation incomplete; an Invalid complete assessment may still report fewer diagnostics. The diagnostic probe validates bad budgets and exercises an injected truncation flag, not actual validator allocation/cost limits or range mapping.

Cancellation throws without a successful partial result; closed/stale borrowed endpoints refuse before further dispatch. Diagnostic/error identifiers are provider codes until an adopted host mapping exists, never invented W3C names. Strict/lax/type mappings for each query host, partial-evidence interpretation and complete error/location projection still need final owner review. The synthetic request/results here exercise protocol gates, not those language semantics.

## Compatible-view invariants

The proof is directional and bound to exact source advisor, target advisor and captured target environment. Descriptive equality, the same component name/digest, similar interface implementations, same-provider branding and data equality grant no admission. Reverse and transitive views require separate proof; unsupported proof is not the same as proven incompatibility.

Source actual components must be source-issued; target expected components must be target-issued. Declaration matching additionally requires the source advisor to own the original facts and original assessment. The compatible view never changes those facts, their origin, their type identity or their snapshot ownership, and does not reassess instance data to manufacture compatibility. Qualifying all required component closure/derivation/substitution relations belongs to the real proof algorithm.

Advisors/inputs are borrowed. View operations check current availability of both endpoints; retained immutable original metadata may remain readable after closure. A view does not keep disposed resources alive, transfer snapshots, grant cross-process authority or silently reopen closed endpoints. Operation admission/drain remains host/provider orchestration unless real call sites justify an additional public Core facility.

## Diagnostic evidence and performance boundaries

The 54 controls cover copied context dictionaries, separate query/XSD versions, unadmitted/lookalike environments, cancellation, owned/missing/forged handles, wrong modes/roles, complete versus incomplete validity, invalid/unsupported evidence, stale input before/after dispatch, fixed episode capture, explicit constructor refusal, directional/negative/unsupported proofs, foreign endpoints and forged facts, original PSVI identity, no reassessment and closure.

All three runs have identical declaration surfaces and control IDs; expected exceptions are caught and stderr is empty. Synthetic Valid/Invalid statuses and compatibility decisions are injected test data, not schema or conformance findings. The initial probe build used a nonexistent IsString convenience member; it was corrected to the existing Kind check, with the failure log retained. No engine source or product expectation changed.

PF-02/E-03 must still measure lookup/proof granularity, closure allocation and cache/guard costs before freezing signatures. Establish should not be recomputed per atomic access; immutable metadata can be shared only under actual authority/profile rules. Input freshness checks must not require repeated whole-document copying/hashing by assumption. Closure remains a draft immutable-array shape pending bulk/cursor evidence. This protocol run supplies no latency, throughput or allocation budget acceptance.

## Remaining decisions and replay

[REQUIREMENT-MAP.json](REQUIREMENT-MAP.json) maps N1?N8 plus XmlDb/Braid scope, but records actual producer, backend, carrier, equality, lifetime, error/location, performance and consumer qualification gaps explicitly. Type/element descriptor detail, affiliation/blocking semantics, invalid/partial PSVI consumption, atomic evidence issuance, mutable-context/deferred execution and Braid protocol/fingerprint integration remain part of final member selection. Do not freeze using declaration completeness or test-double success.

Run `python docs/foundation-advisor-contract/tools/verify.py` for data-only verification. `tools/run.py` rebuilds a private project from the exact V3 source and 1.34.2 Core DLL and replaces raw outputs; run it in a copy to preserve the sealed packet. Production source/pins, old packets, full tests and gates are unchanged. Production implementation, normative adoption, performance budgets, C45 acceptance and shared freeze remain Pending.

Next perform the finite bulk/cursor/lookup experiment and reconcile this V3 surface with actual provider/consumer mappings, reducing rather than automatically accepting the review inventory.
