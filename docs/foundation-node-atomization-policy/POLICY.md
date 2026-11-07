# Selected original-node atomization policy

## Input and compilation boundary

Reserve behavior profile `bosak-original-node-data-v1` and descriptor `bosak-original-node-data-descriptor-v1`; both are **selected, inactive**. `SchemaQuerySession.CompileXPath` with a non-null supported assessor will select only trimmed complete `data($node)` / `fn:data($node)`. Canonical operation is fn:data; require standard fn prefix binding and reject foreign mapping before Bind. Freeze the same supported CompileOptions as existing profiles. Include all five explicit limits, static-context fingerprint, node subset, ordered materialization, provenance, cache-key/transaction and error policies in the distinct descriptor.

Reuse the closed original-node context policy: exactly one present unprefixed node variable; direct Node or explicit Undefined; absent focus; no extra variables, runtime namespaces/services or populated execution state. A missing variable is not empty input. Refuse atomic, arbitrary sequence, array/map/function inputs without enumerating or calling them. The produced sequence is an output representation, not permission to widen input capture.

Before NodeKind or annotation getters, resolve exactly one current admitted source owner. Empty input still checks raw operation and bound advisor. Only known Element and Attribute kinds enter the facts channel; known other kinds refuse without reading OriginalSchemaFacts, while invalid/combined/unknown kinds are malformed. Require selected original assessment and ISchemaAnnotatedNode; never read legacy TypedValue, HasNoTypedValue, string type names or node text as authority. Retrieve and authenticate original facts for the exact node/input/origin; validate type/declaration ownership and nilled/absence consistency before any typed getter.

## Four different empty/absent states

| Authenticated state | Selected result/error | Typed-value reads and accounting |
|---|---|---|
| Explicit Undefined input | Undefined | No node/facts/typed getter; no slots. |
| Element HasNoTypedValue=true, IsNilled=false | InvalidOperationException beginning `FOTY0012:` | Do not invoke ReadTypedValue or its outcome/container; no slots. Recheck currentness before raising. |
| Supported container with zero cells, list or non-list | Undefined | Validate supported container and complete iteration/cleanup; zero scalar slots, no persistent empty-container index. Do not infer absence/nilled. |
| Nilled element with a supported empty typed container | Undefined | Validate actual supported emptiness. Do not infer empty from IsNilled alone. A nonempty container is malformed. |

Absent+nilled is contradictory and rejects as ArgumentException before FOTY0012. An attribute claiming absent typed value or IsNilled=true is malformed, not a valid absent-element case. Unsupported typed outcome remains NotSupportedException; NotFound/Invalid/unknown/null outcome or Supported/null payload is malformed ArgumentException. Do not turn unsupported, failure or no value into empty. Provider exceptions and ordered iterator cleanup failures retain existing precedence; cancellation/disposal checks remain distinct from these semantic errors. FOTY0012 uses the existing exception/message convention; no structured public error API is invented.

## Result representation and provenance

Consume and validate a complete finite container before exposing any result. Preserve the existing full reader's exact governing-handle check against the original facts type (reference identity); semantically equivalent alternate handles or compatible views are not silently accepted by this first profile. Preserve provider atomic order and duplicates, including actual union member type per cell; do not substitute the governing list/union type for each actual atomic type. Only the existing reader's supported plain atomic carrier kinds are eligible. Owned issuer/type evidence does not establish complete schema-facet/backend conformance, and numeric-looking carriers alone do not authorize arithmetic.

| Atom count | Public XdmValue representation | Evidence |
|---|---|---|
| 0 | Undefined | No fabricated scalar/origin. |
| 1 | Direct registered atomic transport | Same exact payload representation, declared/actual type and selected original assessment. |
| 2 or more | Sequence over a private immutable materialized ordered transport buffer | One registered transport per ordinal. No lazy provider enumerator/getter or arbitrary callback in result traversal. |

The multi-result source implements existing IXdmSequence internally; no new public result type/member is selected. Preallocate the bounded output buffer/wrapper before commit. Fill it only from the installed winning group and publish only after complete fill and final delivery validation. Never publish staged markers or a prefix if validation, allocation, cancellation or capacity fails. A failure after the atomic commit retains the complete batch under the established transaction rules, without returning a partial result.

Atomization extracts unchanged issued atomic values. It preserves each cell's original assessment identity; it does not construct a new assessment, retype a lexical string, apply arithmetic, or attach the parent node's declaration/type as the cell's actual type. Node identity, location and original node facts are validated during extraction but are not encoded as permanent per-cell claims. If two nodes legitimately yield the same container/origin in one admitted input, the existing scalar identity can be reused; no hidden parent-node identity is invented.

Later operational use of an extracted cell validates the issued immutable container, original input/source/assessment, selected authority/component handles and current admission. It does not require retaining the extraction node/facts view forever. Revoking the container/input/assessment or owned atomic components prevents later authorization. Revoking only a former node/facts association does not fabricate revocation of an independently still-issued container; the next data extraction must reauthenticate its node/facts tuple. This boundary requires explicit controls before implementation acceptance.

## Cache and quota rules

Retain TA-02 units and identity: MaxTypedValueAtoms limits cells consumed from one result, including staging; the excess member refuses after MoveNext before Current. MaxCachedValuesPerSnapshot counts resident scalar entries, not containers or query calls. Scope remains exact selected issuer/environment/admitted snapshot/original input/authority, with key `(container reference, original-origin reference, ordinal)`; do not add node identity or merge equal payloads across containers. Empty results retain no persistent container index. Equal duplicate values at different ordinals consume different slots.

Every data query rereads and authenticates the original node metadata and typed outcome/issued container identity. A registered immutable container may reuse its saved payload cells without replaying AtomicItems/iterators, after current source/issuer/origin/type/representation checks. This is existing scalar-cache reuse, not a persistent node-facts cache. Fresh empty containers are checked again because there is no empty index. An immutability/version-contract violation is not repaired by silently merging/changing resident cells; supported provider currentness must refuse it.

Reuse the transactional registry: stage all cells, keys/markers and result storage privately; perform provider/node/facts/container reads and cleanup before the registry lock. Immediately before commit revalidate raw/source/original facts and all cell evidence. Under the lock use only existing internal membership/closure/cancellation checks, private reference hashing and bounded insertion; no provider callback, node getter or output traversal. Handle same-key/capacity races, checked counts and complete rollback before the installation linearization point. Recheck original facts plus installed cell evidence before delivery. No eviction, persistent reservation, extra container ceiling or unmarked fallback.

Integer/decimal digit/scale bounds are the existing captured limits, including original representation. MaxOpenSnapshots retains admission units. These limits are not a byte, string-length, provider-work, retained-memory or wall-clock budget; actual PF workload limits remain owner work.

## Payload readability versus operational lifetime

Materialized transport values are shallow copies of the supported immutable/version-guarded carrier representations, not a recursively detached provider object graph. The output buffer never calls the provider. Its count/order and copied carrier payload can remain readable after raw completion/close, just as existing marked scalar copies do. This does not keep a raw operation/resource lease alive or authorize schema operations. A transport can retain borrowed evidence references; retaining it is not ownership transfer, disposal or guaranteed resource detachment.

Raw fact/container/atomic views expire at completion; snapshot closure clears/revokes registry use. A later raw operation may authorize resident extracted cells only with exactly the original live admitted snapshot/environment/issuer/container/origin. Foreign sessions, missing membership, stale/forged/tampered markers and canceled/closed sources refuse. The currently separate compiled data/identity/numeric sessions do not become interchangeable: a data result is not automatically accepted by another compiled session merely because it uses the same assessor/source. No new public cross-session import or all-host capability is claimed.

## Implementation status

These are selected finite Core policies. The complete source-node-to-registered-output bridge, materialized multi-result producer and public data dispatch do not exist yet. Current full reader hides the provider container identity, and public ReadTypedValue intentionally projects singleton containers only. The implementation must add a private staged bridge without duplicate getter reads within one query or publishing raw views, while retaining full-reader defaults and nilled's metadata-only path. No new public signature gap is demonstrated here; actual consumer/all-host and compatibility review can still find one. Do not freeze based on this selection.
