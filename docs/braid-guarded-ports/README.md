# Isolated guarded exported-port facade proof — 29 October 2026

An independent facade over the actual unchanged Braid inspector passes **54 controls**. It returns guarded public ports/targets, refuses closed/foreign/revoked access and drops a late result during a controlled in-flight close. The underlying raw adapter still reproduces the earlier after-unload NoChange behavior, so the new enforcement is specifically the facade/gate. No Braid/core/Schema production source or actual pin is changed.

## Admitted diagnostic profile

The facade admits read-only inspection, literal Rename-target discovery and **exact same-name NoChange only**. It does not admit a changed candidate, Apply, deletion, moves, wrapping/unwrapping, other editors, preview or export. Eight unsupported authoring ports are removed; the one supported result port is a host-side facade. All mutation capability flags are disabled in the narrowed descriptor while the underlying engine descriptor remains unchanged. This is a diagnostic admitted-operation profile, not a production capability-contract change.

All nine current Application port properties and seven target-interface properties are inventoried. The public result contains no raw isolated-context port. The private wrapper target has no public raw target/port accessor; successful NoChange returns the exact original wrapper and no review/candidate. Metadata getters also check admission, preventing an old target from being used as an unguarded interface entry after close. Hostile private reflection/in-process code is outside this cooperative contract; no CLR sandbox/security-boundary claim is made.

## Fresh controls

| Boundary | Result |
|---|---|
| Actual adapter and fidelity | Real inspection succeeds; retained bytes and projected entries match the underlying actual inspector. Original qualified factory and shared Application/Core assemblies remain unchanged. |
| Finite profile | Unsupported operations, changed/null rename input and raw/foreign targets refuse without forwarding inappropriate calls. Other snapshot-port and other-session targets refuse. Pre-cancellation propagates. |
| Response sanitation | Successful real NoChange returns the wrapper. Two injected invalid underlying responses—missing NoChange authority and a fabricated candidate carrying raw inspection—are suppressed with InvalidResponse and expose no candidate/target. These are fault injections, not engine defects or source edits. |
| Closed entries | Inspector capability lookup, target discovery/proposal and every target metadata getter refuse after close. Inspector refusal returns no ports. No post-close call is forwarded; retained byte data remains readable as history. |
| Artifact drift | Replaced engine bytes refuse target metadata, inspection, discovery and proposal without forwarding. Restoring files does not readmit the revoked underlying session. Mutations affect generated private copies only. |
| Controlled in-flight close | A real host resource callback blocks an admitted inspection. Close rejects new work and defers underlying unload while that call is active. Releasing the callback lets raw execution finish; the facade refuses publication and returns no ports, then releases the underlying session. |
| Raw comparison | The separately held raw adapter port still returns its original NoChange target after the underlying unload request. That gap is not relabeled fixed in Braid itself. |

The first 48-control receipt is retained, followed by six artifact-drift/revocation controls for final 54. Final build has zero warnings/errors; execution exits zero within an external 120-second bound. The blocked callback/completion waits have ten-second bounds. No source edit is applied, no real worker protocol is changed, and no live product store or credential location is used.

## Gate behavior and limits

Each admitted call retains an operation lease. Close stops new admission and does not unload the underlying context until active calls have completed. A post-await admission check prevents a result that lost admission during that controlled interval from exposing raw capabilities. Already returned port/target wrappers remain gated after close. This is a real controlled interleaving with actual adapter execution, not exhaustive proof of arbitrary parallel calls, callback reentrancy, close/cancel races or UI synchronization.

The private raw port/target fields are implementation details of this isolated trusted diagnostic. No raw capability is exposed through the exercised supported public port/result/target interfaces. The proof does not protect against privileged reflection or malicious host code extracting private state. It does not certify GC/unload completion; the facade's closed admission state is the enforcement boundary.

The profile explicitly refuses/removes unsupported operations instead of pretending to guard every Braid feature. No changed candidate is published. Full candidate/review/Apply sanitation, complete other-port proxies, real preview/export/worker integration, history migration, public session API and native desktop acceptance remain unqualified. The controlled resource callback is a test coordination hook, not a production resource policy or new authority.

## Next step and state

Review/checkpoint this facade proof. Then scope the consumer-owned read-only session facade/lifecycle contract, including an honest capability profile, all returned authority-bearing surfaces and cancellation/concurrency/reentrancy acceptance, before production integration. Broader authoring operations require separate facade coverage and tests; do not silently enable them based on the NoChange proof.

All earlier sealed dossiers and original Braid/core/Schema source/pins remain unchanged. Prior full engine/Schema/source/binary/runtime/typed qualifications remain inherited, not rerun. Core remains 1.34.1 and Schema 0.9; there is no new core API need, request ID, release or actual Braid product guard. Current prototype/documentation and evidence remain uncommitted.
