# Private operation lease-set primitive

1 November 2026. Production source **1.40.0** adds the internal SchemaOperationLeaseSet primitive selected by the [composite ownership contract](../foundation-composite-ownership/CONTRACT.md). [Implementation basis](SPEC-BASIS.md) preceded the source change. No public types/members or friend grants are added. No package publication, consumer acceptance or API freeze occurs here.

The primitive validates explicit bounded same-session membership, rejecting null, duplicate and foreign members before acquisition. It copies membership, acquires leases in caller order, then rechecks cancellation and all members before returning. Partial entry failure releases only acquired leases, in reverse order, preserving the entry failure and every cleanup exception.

Completion invalidates guarded access before releasing all leases in reverse order. Repeated or concurrent completion claims ownership once. Work failures retain their exception objects and precede cleanup failures; nested provider AggregateException objects are not flattened. A single cleanup failure without a work error is rethrown directly. Revocation blocks access to the full set without completing its raw ownership. An unrelated set can remain live.

## Scope and qualification

This primitive is not connected to SchemaQuerySession or host input capture yet. Its trusted Member adapter represents one engine admission; later integration must create a single stable adapter per actual snapshot, supply real availability/Enter callbacks, and enforce input/assessment membership before getters. The primitive cannot authenticate arbitrary callbacks or infer node ownership. No joint multi-source query, native provider routing, XSLT role validation or cross-universe compatibility is claimed.

The raw callback/task owner must call Complete only after work ends. The primitive does not schedule work or implement DrainAsync. A losing concurrent Complete call can return while the winning call is still releasing leases; it is not a drain barrier. The canceled-wait test keeps completion in the raw task's finally block. Provider resources remain host-owned; rollback concerns operation leases only.

| Fresh production qualification | Result |
|---|---:|
| Full solution rebuild | 0 errors; same 5 existing warnings |
| Ten unit projects | 24,362 passed; zero failed/skipped |
| New primitive regressions | 21; Runtime total 338 |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Public inventory | 11 assemblies / 270 types / 5,770 declarations, unchanged |

The new tests exercise ordered success, empty/default/invalid membership, pre-cancellation and cancellation after acquisition, entry/cleanup failures, closure, independent revocation, nested sets, concurrent completion and canceled waits. These provide primitive-level evidence for the design; the prior 24 composite acceptance cases remain unexecuted as complete host integration cases. Existing tests and conformance runners qualify the fresh production cohort; the prior 151-check standalone ownership probe was not rerun in this step.

[Results](evidence/RESULTS.json), [unit TRX](evidence/unit-trx.zip), [gate exits](evidence/GATES.json), [actual execution layouts](evidence/INPUTS.json) and [API comparison](evidence/API-COMPARISON.json) retain exact evidence. All eleven product assembly versions are 1.40.0.0. Source and product archives retain the tested bytes and license notices; load reports identify engines in unit tests and both gates. Frozen skip maps and corpus pins are unchanged. Informational source revisions can precede this delivery commit; archive hashes identify the exact build source.

## Allocation evidence and next step

Repeated warmed measurements over 1,000 iterations allocate **136 / 288 / 304 / 984 bytes per operation** for **0 / 1 / 2 / 8 members**. The fixture prebuilds membership adapters and returns singleton no-op leases. These numbers measure acquisition/check/completion bookkeeping only; real provider lease allocations, host execution, concurrency throughput and memory budgets are excluded. Membership checks scan all members, and duplicate detection allocates a bounded reference-identity set once at entry. [Measurements](evidence/cost-0.json) and [repeat](evidence/cost-1.json) identify the actual runtime hash. No ordinary execution path calls the new primitive.

Next connect the primitive to a private same-session multi-snapshot operation with actual snapshot identity and assessment checks. Then extend typed input stamping and separate XSLT match/global membership, retaining the existing single-snapshot behavior until integration tests pass. Mixed providers, exact numeric execution, actual Schema/XmlDb/Braid qualification, C45 and API freeze remain pending.

Read-only verification: `python -B docs/foundation-operation-lease-set/tools/verify.py`. The full phase runner, metadata audit and cost fixture are retained under tools. Negative cases use caught exceptions and bounded waits; no deliberate unhandled crash test or machine-wide error configuration change is used.
