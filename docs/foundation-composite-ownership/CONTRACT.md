# Private composite operation contract

All rules here are selected design obligations; implementation and acceptance remain pending.

## Identity and membership

An operation belongs to exactly one session and its exact compilation environment. Its ordered member list contains only live snapshot admissions created by that session. Source/assessment identities are not substituted for admission object identity. Reject default/uninitialized lists, null entries, repeated admission references, foreign sessions and capacity overflow before acquiring any operation leases. Preserve caller order; do not sort by labels or provider addresses. An explicit empty list supports source-free work when the host profile supports it; a default list is invalid.

Copied membership is immutable for the entire raw operation. No acquisition or membership mutation is allowed from result callbacks. Each admitted snapshot retains its own assessment and issued values. Input capture is stamped with the exact operation membership token; requests cannot transfer merely because two groups contain the same snapshots. This stricter initial rule deliberately limits cross-operation request reuse; it changes the current single-snapshot reuse policy only when a later implementation explicitly adopts it. Existing snapshots may participate in more than one independently leased operation when their provider's concurrency profile supports that use.

## Entry and rollback

1. Validate the supplied bounds, session/environment, full member list and raw cancellation token. Do not invoke caller work during validation.
2. Acquire an engine operation lease for each member in caller order. A successful acquisition transfers responsibility for that lease to the operation builder. A throwing acquisition must leave no lease hidden from that builder; the per-snapshot acquisition contract must guarantee this.
3. After all leases are acquired, recheck session/provider availability, each snapshot and cancellation. A close or cancellation at any point before publication aborts entry.
4. Publish the immutable membership token and invoke work only after all checks succeed. Publication is the linearization point for engine admission; a later close may still make the first guarded use fail.
5. On failure, release every successfully acquired lease in reverse acquisition order, attempting all releases even after a cleanup failure. Never release a lease that was not acquired. Preserve the entry failure and all cleanup exceptions.

Rollback releases operation leases only. It does not close already-open snapshot admissions, dispose a provider or undo host resource acquisition. If opening the second source admission fails, its source resource and every earlier admission/resource are still owned by the host. The host must close its earlier admissions and release untransferred resources in its own finally/rollback scope. Successful OpenSnapshot transfers only the documented release obligation; a rejected OpenSnapshot never calls the supplied release callback. No transaction across separate OpenSnapshot calls is implied.

## Execution, revocation and release

Every guarded operation access checks the raw cancellation token, session/binding availability and every member's liveness. Revocation of any member invalidates guarded use of the entire operation, even if the next value happens to come from another member. It does not revoke an unrelated operation whose membership omits that source. Ownership checks happen before foreign node or issued-value getters. A trusted host must map each admitted node to exactly one source admission; overlapping or ambiguous ownership refuses. Merely retaining a CLR node reference supplies no lease or authority.

XSLT match input and global context are separate membership roles and may point to the same admission. That admission is listed once and leased once. All dynamic variable nodes must also be admitted. Closed-resource policy remains in force: a resource callback cannot silently add another source. Empty groups accept no input nodes. Same-session source grouping does not merge assessments or authorize cross-universe type equality.

The raw synchronous callback or actual asynchronous task owns all leases through evaluation and deferred consumption. Canceling a wait does not release leases while the raw task continues. Closing sessions or admissions revokes guarded use immediately and defers resource release until their actual raw leases end. Previously returned unguarded raw node graphs are not automatically revoked or detached; no universal all-access invalidation is promised.

On raw completion, invalidate the operation token before releasing leases. Release in reverse member order and attempt every release. Complete is idempotent, including after throwing cleanup: a cleanup callback is never retried automatically. Provider/store/worker disposal stays with its host. Native code or provider work that ignores cancellation remains live until its raw completion; this contract supplies no forced thread termination.

## Failure preservation

| Work / entry | Cleanup | Outcome |
|---|---|---|
| Success | Success | Return result |
| Failure | Success | Rethrow the original failure preserving stack |
| Success | One cleanup failure | Throw that failure preserving stack |
| Success | Multiple cleanup failures | Aggregate in attempted reverse-release order |
| Failure | Any cleanup failures | Aggregate with original failure first, then cleanup failures in attempted order |

Preserve exception objects, including provider-created AggregateException objects; do not recursively flatten or drop them. Cancellation is a failure under this rule: cleanup failure must not disappear merely because a caller observes cancellation. Later idempotent cleanup is a no-op; it must not replace the recorded completion failure. No failure is silently converted to Unsupported. Unsupported profile decisions occur before ownership transfer wherever possible.

## Integration and acceptance boundaries

Core/Runtime own the private membership token and lease-set lifecycle. Concrete Schema, XmlDb and Braid types do not become dependencies of those layers. Upper host producers validate their selected node roles against the token and use the captured compiled executable. Provider-specific immutable membership/availability capabilities need a concrete contract before native integration; arbitrary caller predicates are not authenticated ownership evidence.

The first implementation must qualify rollback failure ordering, nested/reentrant work, concurrent close and canceled waits with bounded deterministic fixtures. Provider concurrency remains explicit; no blanket thread-safety claim follows from immutable membership. Worker-local acquisition, broker choice, protocol versions, timeouts and process-kill reclamation remain Braid owner decisions. Live leases never become JSON authority.
