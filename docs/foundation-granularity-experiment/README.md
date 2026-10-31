# E-03 granularity return and reduced Core candidate ? 31 October 2026

**Keep complete bulk closure as the candidate; do not add public scalar-dependency, batch-lookup or cursor APIs from this evidence alone.** Bounded reuse of immutable metadata can improve repeated access behind the existing proposed signature. A cursor helps prefix consumption only when complete coverage is already certified, and batch lookup does not consistently beat scalar lookup in this in-process pilot.

The [V4 Core candidate](CORE-CANDIDATE-V4.cs.txt) also excludes the two unreferenced host-operation interfaces from the [V3 inventory](../foundation-advisor-contract/README.md). [SURFACE-DELTA.json](SURFACE-DELTA.json) records **35 ? 33 review types**, with every retained declaration/member unchanged. IQueryOperationLifetime and IQueryOperationLease remain historical diagnostic contracts; ownership requirements are still mandatory, but orchestration belongs behind host/provider ports unless an actual Core call site justifies a stable public facility. This is a proposed scope reduction, not API acceptance.

## Scope and method

A standalone provider implements the proposed ISchemaAuthority over synthetic immutable component-reference graphs. The cyclic primary graph has self/next/skip edges; chain, diamond and cyclic graphs provide correctness controls. These are not XSD derivation semantics or a real Schema/XmlDb/Braid workload. The exact Core 1.34.2 DLL is used for declaration/reference identity only; the XPath VM is not executed here.

Three processes compare graph sizes 64, 4,096 and 65,536, with the middle process reversing mode order. Each of 33 size/mode groups has three warmups and seven timed samples. Allocation, GC activity, modeled provider-call counts, component reads, complete/prefix status and checksums are retained. TieredCompilation and ReadyToRun are disabled. Source graph construction and lookup request creation are outside timed regions; cache/certificate preparation is separately recorded. The cold label means an uncached closure calculation, not cold process/OS/JIT startup. The preparation record?s retainedReferenceBytesLowerBound field counts newly retained closure-array reference slots only; zero for certification does not mean its token/owner has no retained cost. No artificial RPC delay is injected, and call counts are not measured IPC/network round trips.

The 38 distinct correctness controls repeat across the three runs. A separate replay of the existing 54 V3 protocol controls passes with V4; it adds no new distinct protocol coverage. Both projects compile with XML documentation and warnings as errors, zero warnings/errors. Expected exceptions are caught, watchdogs are bounded and all final stderr records are empty.

## Measured boundaries

| Complete/prefix closure path, 65,536 components | Modeled provider calls | Component reads | Observation |
|---|---:|---:|---|
| Hypothetical scalar dependency traversal | 65,536 | 65,536 | Same complete ordered closure; fine-grained boundary overhead. This dependency API is not in V3/V4. |
| Existing proposed bulk shape, uncached | 1 | 65,536 | Complete result materialized after supported coverage is established. |
| Cached bulk result | 1 | 0 | Reuses immutable result; no new per-call allocation in this model. Enumerating the returned closure still costs O(N). |
| Cursor with per-open full preflight | 1,025 | 131,072 | Must prove coverage, then traverse again; not free streaming. |
| Cursor with prior certificate, full read | 1,025 | 65,536 | Less result materialization, but a traversal frame and prior certification are required. |
| Prior-certified cursor, prefix of 1 / 8 | 2 | 1 / 8 | Low prefix allocation; enumeration is explicitly incomplete. |

At this size, the uncached scalar/bulk construction allocates about 3.90 MB; full prior-certified cursor about 2.33 MB; prefix frames 416/968 bytes respectively. The cached bulk call allocates zero new bytes in these measurements. These numbers exclude shared input metadata and prior setup; raw small variations are preserved.

Preparing the 65,536-entry cache traverses the full graph and retains at least 524,288 bytes of array references, plus headers/owner metadata. Certifying closure coverage also traverses the full graph and allocates its visited/traversal state. A certificate is not a way to avoid proving the graph; the cost can be paid earlier only for the exact unchanged authority/root/snapshot. A returned metadata object does not acquire or transfer a resource lease.

An untimed control verifies every lookup entry?s expected status, request order and issued handle at all three sizes and both request distributions. Lookup comparisons use the actual proposed ResolveType shape against an experimental internal batch loop, with identical ordered success/missing results. Both use prebuilt immutable answer objects and allocate the same output vector. At 65,536 requests the scalar model counts 65,536 calls versus one for the batch. Per-process pilot timings vary: diverse batch lookup is not consistently faster, while repeated-name cases show only modest in-process differences. These samples do not justify a new public batch API or predict a remote-host result.

Raw distributions and preparation costs are in [RESULTS.json](RESULTS.json) and the [evidence directory](evidence/INPUTS.json). Very small timings approach timer resolution; no production ratio, p95, statistical significance, retained-memory/peak-memory result or workload-budget acceptance is claimed. The modeled paths include their checks and result consumption; dispatch cost is not isolated from validation/materialization cost.

## Correctness and lifetime constraints

- Complete closure results contain all reachable issued handles exactly once; cycle/diamond controls prevent duplicate/infinite traversal. A reachable unsupported component makes complete scalar/bulk/certification return Unsupported, never a silently shortened success.
- The deliberately unsafe iterator exposes a prefix before discovering unsupported coverage later. Its failure is caught. A public API promising complete supported closure cannot use that pattern.
- The candidate cursor uses a prior issued coverage proof tied to exact authority, root and version. Forged matching fields, foreign universes/roots, stale snapshots and canceled reads are rejected. Equal descriptive graph digests do not authenticate identity.
- Partial enumeration stays partial even when graph coverage is certified. Cursor disposal releases only its traversal state, not the borrowed authority or source snapshot. This is not an actual reader/generation lease or OS cleanup test.
- A Read failure can leave partially written destination storage; no successful batch count is returned. Callers must discard that buffer on failure. The experiment verifies pre-cancel and complete traversal cancellation, not every concurrent/mid-batch mutation schedule.
- Cache reuse is scoped to the immutable authority/root and availability. The model's invalidation closes that authority rather than silently rebinding identities. Compatibility-view caches would additionally need their source/target/profile keys; this closure model does not qualify them.

## Avoid a quadratic cache

The fixture cache retains at most eight roots and evicts older entries; its 256-component control retains eight arrays with 2,048 component references. Eight is a test setting, not a production default. Real policy needs byte/reference bounds, currentness and concurrency-safe eviction, not just a root count. Held caller results can outlive cache eviction, so cache accounting alone is not total retained memory.

If every root in a 65,536-node strongly connected graph reaches every component, distinct fully materialized arrays for every root would require **32 GiB of references alone** on this 64-bit model. That is an arithmetic projection, not an allocation performed by the test. Shared closure sets, bounded caching or internal traversal strategies can avoid it, but must preserve actual authority and result semantics. Do not introduce global QName/digest caches to achieve reuse.

## Proposed API disposition

1. Retain GetComponentClosure's complete bulk outcome for actual provider qualification; allow internal bounded immutable caching without a new public API.
2. Keep scalar named lookups. No new scalar dependency enumeration or mandatory batch lookup is selected. Reconsider an optional batch capability only if actual consumer dispatch/IPC or workload evidence demonstrates it is needed before freeze.
3. Do not add a mandatory public cursor. If real consumers need prefix access, define a separate optional complete-coverage/cursor capability, including proof freshness, incomplete-enumeration state, buffer failure, cancellation and borrowed ownership. A cursor is not a drop-in semantic replacement for complete bulk output.
4. Exclude the unused operation-owner interfaces from the Core candidate; keep operation ownership and raw-work retention behind the host/provider boundary. Actual input/advisor availability hooks remain in the 33-type candidate. The earlier lifetime model and Braid integration obligations still apply.

This narrows the review surface while leaving implementation optimizations possible after freeze. It does not accept the other 33 types or claim real provider scalability. Source/type evidence, CLR equality/carrier choice, true compiler capture, Schema/XmlDb/Braid qualification, owner performance budgets and final signatures remain open.

## Replay and next work

Run `python docs/foundation-granularity-experiment/tools/verify.py` for data-only verification. `tools/run.py` rebuilds private projects and replaces this dossier's raw outputs; use a copy to preserve sealed receipts. [INPUTS.json](evidence/INPUTS.json) pins exact declarations, package bytes, project recipe and settings. No production source/pins or earlier dossiers change; full engine tests/gates were not rerun.

Next reconcile V4's retained members with actual owner call sites and representative workloads, resolving type/descriptor/atomic issuance, equality and capture/routing decisions into one finite implementation disposition. E-03 is complete for this declared synthetic comparison only; real-provider performance acceptance and shared API freeze remain Pending.
