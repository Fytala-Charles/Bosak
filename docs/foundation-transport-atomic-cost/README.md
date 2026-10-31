# Atomic transport qualification and measured costs

31 October 2026. Isolated candidate; production remains 1.35.0 at source 8080b18b.

The private transport checkpoint 31cc767e exposed twelve failures in six representation-sensitive cases: signed double/float zero, NaN bit patterns, duration spelling, QName prefix and timezone offset. Map-key equality admitted changed payloads and could reuse source evidence while replacing an explicit result with the source representation. The candidate now binds exact representation without invoking map-key comparison. Newly boxed values with the same representation still retain no-op evidence. This does not redefine XPath equality.

The candidate also replaces cached-read closure/heap-lease allocation with a synchronous stack-owned read lease and a revocation-checked cache lookup. Membership runs inside the lease; close revokes access immediately and defers cleanup until the read exits. A primary membership failure and secondary cleanup failure are both retained. Cache misses preserve the original owned path. Runtime source is unchanged from the prior direct transport candidate; two Core source overlays change. No production or owner checkout edits occurred.

Final correctness: 39 fixtures spanning all 11 atomic kinds available through public factories, with **376 atomic/lifecycle checks per process**, twice for candidate and control. This includes the previous 23 fixtures, float/date/time/duration edges, year zero and year 10000, legacy type fields, three VM transports per fixture, evidence identity, snapshot closure and representation-sensitive conversions. The unchanged native integration host passes candidate 121/control 103 checks twice each, with 14 expressions per process. Two unchanged retained legacy callers pass 20/0. Seven final isolated builds have zero warnings/errors; exact bindings of all seven engine DLLs are recorded.

The native host retains an inherited scope string describing an unchanged kernel; that string predates these overlays. Actual kernel identity is established by the source/assembly manifests, not that label.

The control deliberately records existing private-kernel conversion reuse; corrected representation preservation is asserted for the candidate. Full public Schema/native-XSD type conversion is not supplied by the synthetic issuer. Reserved Uri/Binary enum kinds have no public factories; their existing URI/binary string annotations are tested without fabricating reserved values. Complete enum coverage, list/union, full XSD 1.1 and consumer qualification are not claimed.

## Measured operation costs

Both reported cohorts use the exact same final atomic host source, same machine/runtime and unchanged operation loops. Each operation has three warmup batches and seven measured batches, at 1,000 and 20,000 operations, in two processes per variant with reversed control/candidate order. Values and issuer containers are pre-created; cache filling, compilation, XML assessment, process startup and fixture creation are outside these warm loops. The table uses the 20,000-operation batches (14 samples per cell). Allocation is current-thread allocated bytes; GC counters and raw timing distributions are retained. Timing order between before and corrected cohorts is not randomized and latency has no acceptance budget.

| Operation | Kind | Before bytes/op | Corrected bytes/op | Before median ns/op | Corrected median ns/op |
|---|---|---:|---:|---:|---:|
| cached-kernel-read | integer | 176 | 0 | 187.9 | 121.5 |
| cached-kernel-read | string | 176 | 0 | 185.2 | 117.6 |
| cached-kernel-read | decimal | 176 | 0 | 208.1 | 118.5 |
| issued-value-read | integer | 176 | 0 | 215.6 | 159.1 |
| issued-value-read | string | 208 | 32 | 227.4 | 166.7 |
| issued-value-read | decimal | 208 | 32 | 235.9 | 159.9 |
| evidence-read | integer | 0 | 0 | 108.6 | 87.8 |
| evidence-read | string | 0 | 0 | 103.5 | 97.9 |
| evidence-read | decimal | 64 | 0 | 452.9 | 95.0 |
| exact-noop | integer | 32 | 32 | 345.9 | 313.0 |
| exact-noop | string | 64 | 64 | 387.8 | 350.7 |
| exact-noop | decimal | 192 | 64 | 270.4 | 313.3 |

The decimal exact-no-op median rose from 270.4 to 313.3 ns/op in these samples despite lower allocation. No general latency improvement is established; short warmups, tiering and cohort order can affect these distributions.

Raw payload access measures zero allocated bytes in both candidates. Corrected string/decimal attachment still allocates a 32-byte carrier box per read; integer attachment uses the unused reference slot. Explicit no-op admission still allocates its existing 32-byte conversion lease and, for reference payloads, another carrier box. Evidence lookup is allocation-free, including decimal; the cache-hit lifetime lease allocates no heap object. These are measured local costs, not universal platform guarantees or approved performance budgets. Cold cache misses, retained/peak memory, I/O, worker IPC, list/union and provider costs remain unmeasured here. Control evidence-read validates an already known Evidence object because it has no carrier; that control is not a transport-lookup equivalent.

## Evidence and reproduction

`evidence/red` retains the first 363-check host and its twelve failed candidate assertions. `initial-before` retains the first 373-check matched baseline; `initial-fast-candidate` retains the first successful 373-check candidate whose cache-hit membership lease was incomplete. That cohort is not lifecycle qualification. `before` repeats the original candidate using the final host (376 checks, twelve expected failures); `final` contains the corrected candidate and green final controls. Each phase also retains native runs, builds and checked legacy replays. All failures are caught diagnostic assertions; no unhandled crash probe is launched.

Verify with `python -B docs/foundation-transport-atomic-cost/tools/verify.py`. The [recipe](tools/run.py), [atomic host](tools/AtomicProbe.cs.txt), [source diff](evidence/SOURCE-DIFF.txt) and [pre-execution specification basis](SPEC-BASIS.md) are sealed. Reproduction requires fresh output directories; run the before/final phases in a fresh working copy. Existing friend access is used by the hosts; no public diagnostic factory or public provider API is added. Equals/GetHashCode override compatibility and C45 acceptance remain pending.

Next: run full solution and conformance qualification against this exact isolated composed candidate, and review the equality overrides against the public manifest before a production promotion decision. Public API freeze and owner-approved performance budgets remain pending. Historical production unit/gate receipts are inherited, not rerun by this trial.
