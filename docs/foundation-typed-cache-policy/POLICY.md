# Selected Core accounting and projection rules

## Scalar accounting

| Item | Selected rule |
|---|---|
| MaxCachedValuesPerSnapshot | Maximum resident issued scalar evidence cells for one admitted snapshot; retain current documented units. |
| MaxTypedValueAtoms | Maximum cells consumed from one typed result, including uncached staging. An extra member refuses before Current, as TA-01 does. |
| Cache identity | Exact environment/selected issuer/current snapshot/original-public-input scope, then container reference + ordinal + optional original-origin reference. Names, equal payloads, digests and matching original sources do not merge entries. |
| Empty result | Zero scalar slots; no persistent empty-container record/index. List/non-list shape is retained transiently for the operation and facts, without inferring nil/no-value. |
| Nonempty shape | Governing type, IsList and atom count may be retained with the scalar entries. Any shared shape record lives only through nonempty scalar entries, so their count also bounds resident shape count. |
| Repeated admitted container/cell | Reuse existing immutable scalar evidence under current membership/type/lifetime checks; no duplicate slot or new original assessment. |
| Insufficient scalar capacity | Refuse the entire new batch; no partial values, eviction, extra container ceiling or unmarked-value fallback. A result fitting the atom limit can still exceed remaining evidence capacity. |
| Closure/revocation | Snapshot close clears the registry and revokes operational use. Entries alone never prove current ownership. Revoked entries may occupy the finite ceiling until explicit close; no silent eviction changes previously returned evidence identities. |

No separate unbounded container index is allowed. A nonempty batch can discover its stored shape through ordinal zero, then reference-keyed scalar entries. Already-owned component handles need not be interned; authoritative identity comparisons use the selected authority. Cache keys do not substitute handle-reference equality for semantic component equality. The issuer's immutable/version-guarded container promise and ongoing membership checks remain essential.

These rules concern source-admission-associated reads, including an optional origin. They do not assign source-free ConstructSimpleValue results to an imaginary snapshot, reinterpret the scalar ceiling as container count, or force null origins through the old non-null-original helper. Source-free construction lifetime/accounting must be qualified separately. The two public ceilings are not a bound on bytes, upstream provider storage, string lengths or time.

## Transaction boundary

Complete TA-01 validation and all provider reads/cleanup occur before registration. Stage keys, immutable entries, return-view data and a bounded rollback record privately. Check raw operation, source/issuer/container/type ownership, cancellation and quotas before committing. Do not invoke provider getters or arbitrary callbacks while holding the registry lock.

Under the lock, recheck closure, cancellation and the actual missing-key count. Use checked wide count arithmetic; ensure dictionary capacity for actual new entries, not the configured maximum. Preallocate rollback bookkeeping and use a private reference comparer that invokes no caller equality/hash code. Reuse any concurrent same-key winner; if another batch used capacity, refuse this batch without changing its own or the winner's entries. Install all missing cells atomically with complete rollback on an internal precommit/insertion failure. No in-flight reservation survives an aborted registration.

The linearization point is the completed locked installation of the entire batch. A failure before that point leaves previous committed entries and available capacity unchanged. After that point, a delivery check can observe cancellation, revocation or cleanup failure: the complete batch remains committed, unless snapshot closure has cleared it. This is completed cache state, not a partial reservation. Retry of an unchanged live issued identity reuses the entries. There is no rollback of entries already visible to another operation and no guarantee of immediate cancellation inside a single dictionary primitive. All returned evidence still needs live operational admission.

## Authorized numeric projection

A future separately identified, explicitly assessor-bound typed scalar profile must validate exactly one current owner among the raw operation's admitted snapshots. Container ownership, stored component ownership, original-origin association and raw lifetime are checked before operational payload use. Initial scope is atomic identity plus the retained finite Integer/Decimal arithmetic/comparison lane; zero/multiple atomic values and structured graphs refuse where singleton operands are required. A singleton atomized list member can be numeric through its selected actual atomic type; the list's governing type is not substituted for that type.

Resolve and own the numeric built-in handles through the selected authority. Prove actual type sameness/derivation using its supported positive answers, classifying integer before the broader decimal family. An unsupported/negative relation or an inconsistent payload primitive kind refuses before numeric projection. Descriptive type names and a numeric-looking Value.Kind are insufficient authority. Copy only the authorized primitive payload into bounded arithmetic storage; do not globally remove or rewrite incoming evidence. Numeric limits remain those captured for the operation.

Identity transport returns the same admitted marked value with declared/actual type and optional origin intact. Arithmetic and comparison results are plain primitive computed values, with no source validation or fabricated provenance; even a mathematical no-op such as adding zero is computation. Existing plain numeric profiles keep their marker and annotation refusals. Typed schema constructors/casts are excluded from the first profile until the provider-owned target/conversion route is qualified. Existing exact provider no-op conversion continues to require authoritative type sameness and representation-preserving equality; general numeric equality does not preserve original evidence. Changed provider construction has fresh target evidence and no inherited original assessment, as already selected in the contract.

These are implementation policies, not a new public profile or native type checker. Public binding/ReadTypedValue/SchemaAtomics activation, actual IsDerivedFrom behavior and all-host use need separate positive and poison/lifetime qualification. No new public signature is demonstrated as necessary by these choices; they do not prove every future signature gap closed or establish final owner acceptance.
