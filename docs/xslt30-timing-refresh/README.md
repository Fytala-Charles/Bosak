# Historical timing PR delivery refresh

PR #484 now includes merged main `0432de7c0a357eeeb748d05ec1e9f4148cb737f6` and the actual 1.81.9 allocation fix from PR #489. Source, tests, tools, workflows, fixtures and gate settings are identical to that main revision. Only historical timing documentation and this delivery receipt differ. Original commits remain in the merge history; there is no rebase or force-push.

| Attempt | Result | Interpretation |
|---|---|---|
| Original PR #484, run 43437283485, 1.81.8 payload | Timed out at 1,200.156 seconds; last started sx-instance-of-101 | Failed and retained; not retried or reclassified |
| PR #489, run 43491196305, 1.81.9 payload | 10,221 passed / 0 failed / 4,380 excluded in 1,023.329 seconds | All 14,601 identities and exclusions independently replayed; all six checks green before merge |
| Refreshed PR #484, incorporating merged engine fix | Required hosted qualification pending | Distinct engine payload from the original failed attempt; same 1,200-second / 8 GiB limits |

The [original seven-attempt investigation](../xslt30-hosted-timing/README.md), all raw evidence, four complete replays, three original timeouts and thirteen synthetic controls are byte-for-byte unchanged. The original investigation's proposed telemetry stages subsequently produced evidence in open PRs #485-#488; those tools are not claimed as merged here. PR #489 is merged and independently verified; its main-branch qualification is running separately.

The allocation microbenchmark proves 9,600,000 fewer allocated bytes per 100,000 warmed cache hits while preserving identity and weak lifetime. Separate hosted elapsed times do not establish a whole-suite speedup, failure rate or timeout cause. No exclusion becomes a pass, and no policy, fixture, budget, automatic retry or diagnostic lane is introduced. Development Core/server 1.81.9 is unpublished.

The archive pairs are [original failed attempt](ORIGINAL-PR484.zip) with [metadata](ORIGINAL-PR484.json), and [qualified PR #489](QUALIFIED-PR489.zip) with [independent replay](QUALIFIED-PR489.json). The [merge receipt](MERGE-489.json) records the verified resulting tree. Run [verify.py](verify.py) to replay both attempts and the unchanged seven-attempt investigation without executing .NET. See the [hash manifest](MANIFEST.json) and [HTML receipt](DOCUMENTATION-CHECKS.json).
