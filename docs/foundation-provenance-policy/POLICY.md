# Core policy for unleased atomic observation

Core selects this bounded policy for implementing the exact reviewed `SchemaAtomics.TryGetProvenance(in XdmValue, out ISchemaAtomicProvenance?)` declaration. It is not implemented yet, and this selection is not Schema/other-owner acceptance. [POLICY.json](POLICY.json) contains 22 decisions; [open requirements](OPEN-ITEMS.json) remain explicit.

The helper must observe engine-admitted metadata without acquiring a lease or recovering authority from a name/value. A successful observation means the exact carrier still has a valid engine registration and a describable cached declared/actual/origin tuple. It does not mean a provider currently owns those components, a compatibility proof is current, or a value may be evaluated in another operation. Provider-created provenance objects cannot mint an engine attachment.

## Selected routes

| Carrier | Future bounded observation | Lifetime and limits |
|---|---|---|
| Plain, computed, label-only or exact-numeric primitive | False, null out value | Never infer a component authority from payload/QName. |
| Genuine ordinary registered scalar | Cached declared/actual/origin references after engine registration/payload checks | Exact session/snapshot/cache live; no fresh provider verification. |
| Originless ordinary registered scalar | Same checks, with null origin | No manufactured original assessment. |
| Genuine operation-local constructed receipt | Cached declared/actual and null origin | Exact raw operation live; false after completion, cancellation or close. |
| Current-compatible stable cache entry | False, null out value in this context-free helper | A stable entry cannot identify the caller's current leased proof; required leased observation remains open. |
| Private legacy kernel marker | False until a separate public-contract adapter is qualified | Opaque object fields are not automatic public authority. |
| Existing flattened original-node atomization | False, null out value | It carries no marker; no reverse lookup or retroactive attachment. |
| Empty, non-atomic, expired, unknown, unregistered or rebound value | False, null out value | No traversal, speculative provider call or quota mutation. |

Ordinary cached metadata is descriptive of the admitted carrier, not a fresh claim about mutable provider state. Subsequent typed execution must still run exact source/authority/currentness checks. Returned engine views must guard every provenance getter with engine-only lifetime checks; retained views refuse after revocation. Returning opaque handle/origin references does not recursively guard arbitrary methods on those provider objects: consumers still need explicit owner/raw lifetimes for provider operations.

The helper and its getters must not call provider/authority/type/origin getters, proof operations, enumerators or native acquisition/release callbacks. They must not use ambient operations, a global value/name lookup, a previous operation issuer or a public marker-registration SPI. Exact transport payload and tags must match the sealed registration without user-defined equality/stringification. Source close/cancellation races require a callback-free engine linearization check. Failure clears the out value. Unexpected engine defects must not disappear behind blanket exception swallowing.

## Why compatible observation remains separate

Current compatible transport deliberately reuses stable scalar entry identities while proving ownership/compatibility under each new raw operation. Its cache does not retain the previous operation-bound issuer. The context-free helper has no operation parameter and cannot tell which active proof the caller owns. Choosing the latest proof, retaining an old issuer, or inferring current compatibility from the cached target-type references would weaken the existing boundary.

The bounded helper therefore refuses this route even when a caller happens to be inside a raw operation. This is an explicit limitation, not closure of the compatible provenance requirement. PV-H02 must resolve a separately leased observation/disposition before final freeze, preserving stable payload identity and operation-local reproof. No new public overload or ambient authority scheme is silently adopted by this policy.

## Atomic evidence and complete containers

The immutable [XDM 3.1 Recommendation, sections 2.5 and 2.7](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/) distinguishes sequence structure and atomic type/value information. The immutable [F&O 3.1 Recommendation, section 2.4](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-data) defines ordered atomization. Neither specification defines this host lease/observation API; those rules are explicit Bosak policy.

A provider typed container retains `GoverningType`, `IsList`, ordered cells, and each cell's declared/selected actual type and optional assessment. Observing one atom cannot recover list boundaries or cardinality. Empty lists retain valid container metadata but have no atomic item and consume zero per-atomic scalar slots. A singleton list remains a list; a one-cell observation never classifies its entire container as scalar. Mixed union members preserve their selected per-cell actual handles in order.

Validate a complete bounded container before publication or registration. Do not publish a fitting prefix, silently truncate, partially mutate quotas, or collapse empty/list/multiple-cell shapes into scalar success. Existing ordinary one-cell selection, stricter compatible singleton selection and constructor list refusals remain their current behavior until separately implemented changes are adopted and qualified. Existing flattened node-data output remains its selected plain route.

General public empty/list/union transport and marker-preserving routes remain required by N3/PV-T01. This helper alone cannot close them. Native assessment support, owner review, representative allocation/retention/latency budgets and C45/API freeze remain separate.

## Evidence boundary

The source inventory and prior 1.76.0 full qualification are unchanged. A fresh 475-case execution characterizes existing typed identity/numeric, scalar cache, constructor, compatible and atomization behavior. It does not test an absent helper. All 37 new implementation cases are **NotRun**. [ADOPTION.json](ADOPTION.json) keeps exact specification revisions, policy selection date, immutable proposal and source pins separate from historical acceptance and later qualification.
