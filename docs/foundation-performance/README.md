# Foundation performance and final API change intent — 31 October 2026

**Performance is a design input and an acceptance gate before the shared API freeze.** The owner’s intent is to make this the final breaking change to the named stable Bosak foundation for Schema, XmlDb and Braid. That intent requires resolving known functional, lifetime and performance-driven contract needs now; it is not a guarantee that every future requirement can be predicted.

Freeze the selected versioned public contract, not every experimental API or internal optimization. After freeze, prefer implementation improvements and compatible optional additions. Any unavoidable breaking change needs an explicit major-version/migration decision; do not silently widen required provider members, change layout, reinterpret identity or discard provenance to recover speed.

## Decisions before signatures become permanent

- Preserve cheap ordinary XML/no-schema paths. New optional schema authority must not force every node/query through schema allocation, validation or concrete provider dependencies.
- Keep lazy/prefix consumption possible where required; expose first-item behavior and owner lifetime honestly. Mandatory eager materialization or deep copies are not default solutions for ownership or worker transport.
- Audit hot-path interface dispatch, boxing, allocation and repeated conversions. Inspect opaque identity, atomic/list/union carrier and annotation alternatives without changing `XdmValue` layout or required node/sequence slots by assumption. Existing allocation-sensitive enumeration must be measured.
- Reuse immutable compiled schema/query metadata where safe. Define cache keys, bounds, eviction/currentness and authority separation first. Equal names or schema digests do not authorize sharing component identities. Resource snapshot/version changes must invalidate semantic reuse.
- Evaluate operation granularity before freezing it. Measure repeated component queries, provider crossings, storage reads and IPC; select bulk access or optional capabilities only where an actual consumer workload demonstrates the need. Do not lock in a slow per-item round-trip API or add speculative batch methods without a use case.
- Separate compile/assessment/setup from warm execution. Braid must distinguish cold process launch from steady execution, and account for serialization, hashing, duplicate parsing and data movement. A faster path cannot erase source fidelity, original PSVI, security/resource policy or snapshot ownership.
- Design cancellation and disposal cost together with throughput. Retaining references, finishing an outer enumerator or requesting unload does not prove that actual leases/handles are released. Do not promise bounded cancellation for uninterruptible work without evidence.

## Required owner work

[PLAN.json](PLAN.json) registers nine scenario families PF-01–09, all NotRun. Core owns ordinary-path and API-shape measurements; Schema owns compilation/authority/typed values; XmlDb owns persistent access/read amplification and snapshot leases; Braid owns end-to-end preview, worker transport and configuration/currentness. Shared boundaries require joint interpretation. This extends [C139-01](../REQ-139-OWNER-WORK-PACKAGES.md), [BF140-01–05](../braid-foundation-requirements/README.md), Schema S009 and XmlDb FX009; it is not a new backend feature or a claim those owners have already executed benchmarks.

The comparison must use representative owner-approved fixtures with size/shape and hashes. Include small/large inputs and selective/full consumption, exact default and opt-in profiles, successful/refused/invalid/error paths, and relevant cancellation/close cases. Where list/union/derivation/compatible views are required but no real provider exists yet, record the unexecuted gap. A fast decimal-only benchmark cannot certify the full required foundation contract.

## Measurement and acceptance protocol

Record exact source/package/assembly hashes, fixture bytes and query/settings, OS/runtime/CPU/memory, build configuration, process/load-context model and tool version. Compare equivalent semantics on the same environment. Preserve shipped consumer-pin baselines separately from the current candidate and any new API prototype. Refresh measurements on the final changed production candidate; old 1.34.2 results cannot qualify future contracts.

Use warmup and repeated measured samples, retaining raw distributions and repeat/run counts. Report latency percentiles with enough observations and disclose sample limitations; distinguish throughput from one-shot timings. Capture allocated bytes, GC activity and peak/retained memory with an identified method; include actual I/O/lease events for persistent inputs and phase/IPC costs for Braid. Separate cold/warm caches and process startup, and avoid mixing instrumentation overhead into comparisons without accounting for it. Correctness and exact supported outcomes are prerequisite to any speed comparison.

Budgets are **not yet selected**. Establish owner baselines and representative service goals, then agree explicit latency, throughput, allocation/memory, I/O and lifecycle budgets with tolerances and measurement uncertainty before accepting a design. Baseline parity alone is insufficient if the baseline misses the intended workload target. Do not invent a universal percentage or latency target. Each failed budget or API-shape limitation needs remediation or an explicit scoped decision by the affected owners; known required performance needs cannot be hidden by excluding the workload from freeze.

## Freeze gate and current state

Before freeze, all four owners must dispose required performance needs for the exact public manifest/profile and candidate. Record selected budgets, comparative reports and reasons for remaining intentional limits. The final signature review must show why expected growth—full-XSD typed transport/authority, persistent lazy inputs and Braid worker/session integration—can proceed within the stable API. Broader backend conformance and unrelated native/trust rollout remain separate.

This step registers the requirement and acceptance method only. No production API, normative semantic adoption, benchmark result, runtime execution or freeze acceptance is supplied. Existing correctness/compatibility receipts remain valid only for their original scopes and are not performance evidence.
