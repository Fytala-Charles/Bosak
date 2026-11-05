# XSLT composite source membership

1 November 2026. Production source **1.42.0** admits the captured XSLT initial-match and global-context inputs through the exact composite operation before transform dispatch. [Implementation basis](SPEC-BASIS.md) preceded source edits. Public declarations and original friend grants remain unchanged. No publication, C45/consumer acceptance or API freeze occurs here.

## Implemented boundary

Composite input capture retains its trusted, operation-bound ownership predicate. A private upper-host callback validates each non-null XSLT role against that predicate and the complete live operation membership. A role must belong to exactly one admitted source; missing or ambiguous ownership refuses before the transform reads input node getters. The operation identity must match before the callback runs, and operation availability is rechecked after admission and before dispatch. Unsupported hosts without an admission capability still refuse.

Match and global inputs remain separate roles. Distinct admitted documents produce `A|B` for matched-template focus versus global initialization; a named template uses global focus and produces `B|B`. One admission can serve both roles without a duplicate operation lease. A source-free named template can run with an explicit empty membership set. The existing single-snapshot route remains available.

The retained predicate is an internal trusted-host capability, not authenticated native assessment. Input graphs and ownership must remain stable under the supported host/provider contract. This boundary does not guard every raw node getter against arbitrary concurrent mutation or interrupt a running transform immediately on close. Guarded operation/result access rejects closure; raw callback ownership continues until work ends. No input document copy, eager result materialization or serialization roundtrip is added.

## Fresh qualification

| Check | Result |
|---|---:|
| Full solution rebuild | 0 errors; same 5 existing warnings |
| Ten unit projects | 24,390 passed; zero failed/skipped |
| New runtime admission cases | 10; Runtime total 366 |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Compiled-host controls | 18 passed twice; 37 assertions per run |
| Existing standalone ownership controls | 151 passed twice on the new cohort |
| Public inventory | 11 assemblies / 270 types / 5,770 declarations, unchanged |

[Final counters](evidence/RESULTS.json), [unit TRX](evidence/unit-trx.zip), [actual layouts](evidence/INPUTS.json), [gate exits](evidence/GATES.json) and [API comparison](evidence/API-COMPARISON.json) retain exact evidence. All eleven product assembly versions are 1.42.0.0. Actual load hashes identify engines in suites, gates and probes. Frozen skip maps and clean corpus pins remain exact. Source/binary archives retain license notices; hashes identify the tested bytes independently of informational pre-commit revision fields.

The new unit cases qualify distinct/shared roles, omission and ambiguity, dispatch-time ownership changes, foreign tokens, callback-time closure, source-free work, unsupported capability refusal and operation-end invalidation. The [host probe](evidence/host-probe-0.json) and [repeat](evidence/host-probe-1.json) execute compiled XPath/XQuery controls and real XSLT matched/named, distinct/shared and source-free transforms. Poison nodes prove both unadmitted match and global roles refuse with zero getter calls. Canceled waits and provider/session closure retain both source leases until the raw callback ends. A real transform error plus both cleanup exceptions survive in original-first, reverse-release order, with exactly-once cleanup.

Fixtures use the existing Runtime test identity. Reflection binds upper-host delegates only at setup; it is absent from input capture and dispatch. Test issuers and membership predicates remain fixture boundaries. The sealed 24-case composite contract is not retroactively marked fully executed or owner accepted. Native Schema/XmlDb/Braid resources, compatibility routes and worker failures still require qualification.

## Performance and remaining work

The ownership delegate is reused for capture and later role admission, saving **64 bytes per composite operation** in both repeated measurements. Preliminary measurements exposed the duplicate delegate allocation; its raw source/build/test/probe receipts are preserved separately under pre-delegate-reuse. Final qualification rebuilds and executes the corrected source cohort.

Repeated [component measurements](evidence/cost-0.json) and [repeat](evidence/cost-1.json) allocate **4,728 / 5,304 / 5,560 / 8,264 bytes** for 0/1/2/8 actual snapshot members, composite entry, scalar capture, a fresh child, stub evaluation and guarded visitation. They exclude provider acquisition, bound closed-resource policy, full host execution and concurrency throughput. The [host probe](evidence/host-probe-0.json) additionally measures warmed real XSLT transforms at **5,096 / 5,704 / 5,960 bytes** for 0/1/2 admitted sources, including capture, role checks, closed-resource policy and flat result draining. Stylesheets and source admissions are prepared before measurement; source-free and document-backed cases perform different stylesheet work and are not a controlled membership-only delta. No consumer budget or throughput target is inferred from these allocation figures.

The input record retains one additional ownership delegate reference. Earlier input-capture receipts remain historical; replayed compatibility measurements record the current costs. Ordinary public evaluation does not enter this opt-in private path.

Next qualify host-owned acquisition and admission rollback across multiple sources, including primary and cleanup failures before a complete operation can start. Then bind actual native owner capabilities and consumer workloads. Broader XSLT invocation profiles, exact numeric execution, C45/consumer acceptance and API freeze remain pending.

Read-only verification: `python -B docs/foundation-xslt-source-membership/tools/verify.py`. Negative controls run with caught exceptions, scoped child error mode and bounded waits; no machine-wide error configuration change or intentional unhandled crash test is used.
