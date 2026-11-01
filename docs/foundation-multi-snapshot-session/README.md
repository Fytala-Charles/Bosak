# Private multi-snapshot session integration

1 November 2026. Production source **1.41.0** connects the private operation lease set to actual same-session snapshot admissions and typed XPath/XQuery evaluation. [Implementation basis](SPEC-BASIS.md) records the policy before edits. Public declarations and original friend grants remain unchanged. No publication, native consumer acceptance, C45 acceptance or API freeze occurs here.

## Implemented boundary

New private Run/RunAsync overloads accept explicit immutable snapshot membership. They validate actual kernel ownership, liveness, uniqueness and the session's existing snapshot bound before acquiring operation leases. A transient member adapter is created only after each real admission is validated; callers cannot supply arbitrary adapters through these overloads. Empty explicit membership permits source-free XPath/XQuery work. Default/null/duplicate/foreign memberships refuse before work.

The operation checks the whole set on guarded use. Reads require an explicit member snapshot and retain that snapshot's exact original assessment. Implicit single-source reads, conversion admission and raw context evaluation refuse on the composite path. Existing single-snapshot behavior and request reuse remain unchanged.

Composite capture requires each input node to match exactly one admitted source according to the trusted host predicate. Unowned or ambiguously claimed nodes refuse. Supplied private scalar evidence must be valid for this kernel and, where snapshot-backed, originate in an operation member. A plain numeric binding does not acquire assessment provenance. Captures receive the exact operation identity; even an identical later member list cannot reuse them. Node graphs and ownership predicates remain trusted borrowed host inputs, not authenticated native provider proofs.

Lease completion follows the actual raw callback/task through deferred result consumption. Closing one source invalidates the group's guarded access while retaining all active leases until completion. Another operation that omits that source remains usable. Closing the session with raw work outstanding releases the group in reverse member order after raw completion. Work and all deferred cleanup failures retain their exception objects and ordering.

Actual compiled XPath and XQuery factories evaluate two separately admitted documents in the host probe. The XSLT factory explicitly disables composite evaluation, including source-free composite dispatch, until initial match/global-context roles can be validated against operation membership. Its existing single-snapshot path remains usable. Resource acquisition remains closed; this step does not add implicit documents, provider acquisition or mixed-authority routing.

## Qualification

| Fresh production qualification | Result |
|---|---:|
| Full solution rebuild | 0 errors; same 5 existing warnings |
| Ten unit projects | 24,380 passed; zero failed/skipped |
| New runtime integration cases | 18; Runtime total 356 |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Compiled-host probe | 3 controls / 11 assertions, passed twice |
| Existing standalone ownership probe | 151 checks passed twice on the new cohort |
| Public inventory | 11 assemblies / 270 types / 5,770 declarations, unchanged |

[Results](evidence/RESULTS.json), [unit TRX](evidence/unit-trx.zip), [execution inputs](evidence/INPUTS.json), [gate exits](evidence/GATES.json) and [API comparison](evidence/API-COMPARISON.json) retain exact evidence. All eleven product versions are 1.41.0.0. Actual loaded hashes identify the cohort in unit tests, gates and probes. Both frozen skip maps and clean corpus pins remain exact. Source and binary archives retain the tested bytes and license notices; informational revision fields can name the pre-commit baseline.

The runtime cases cover two assessments, wrong-assessment/nonmember scalar rejection, malformed memberships, source-free work, operation-token rejection, node ownership ambiguity, nonmember evidence, group-wide revocation, nested raw leases, canceled waits and work plus multiple cleanup failures. These are partial implementation evidence for the [composite contract](../foundation-composite-ownership/CONTRACT.md); the sealed 24-case design list is not retroactively marked accepted. Full native-provider and XSLT integration cases remain pending.

The initial test placement attempted to access private Core authority from the XSLT test assembly, which lacks that friend grant. The host controls now run as a separate fixture under the existing Runtime test identity, with reflection only at setup to bind upper-layer factory delegates. No new friend grant or project dependency was added. Initial build diagnostics are retained separately; final builds and probes pass. The standalone fixtures use test issuers and trusted ownership predicates, not native authenticated assessments.

## Performance and next step

Repeated warmed 1,000-iteration component measurements allocate **4,712 / 5,288 / 5,544 / 8,248 bytes per operation** for **0 / 1 / 2 / 8 real snapshot members**. This includes composite entry and actual kernel leases, bounded scalar input capture, a fresh child context, stub evaluation and guarded result visitation. It excludes compilation, provider acquisition, the factory's closed-resource policy, actual host execution and concurrency throughput. The adapter, membership, closure and copied-input allocations are visible costs; no consumer budget or throughput target is inferred. [Measurements](evidence/cost-0.json) and [repeat](evidence/cost-1.json) record the actual runtime hash.

The input record gains an operation-identity reference; older single-input allocation receipts remain historical. The replayed ownership probe records its fresh values separately. Ordinary public evaluation does not enter this opt-in private path.

Next admit separate XSLT match and global-context roles through the exact operation membership before enabling composite transforms. Then qualify native owner adapters, broader typed results, acquisition rollback and compatibility routes. Exact numeric execution, Schema/XmlDb/Braid acceptance and API freeze remain pending.

Read-only verification: `python -B docs/foundation-multi-snapshot-session/tools/verify.py`. Negative fixtures use scoped child error handling and bounded waits; no deliberate unhandled crash test or machine-wide error configuration change is used.
