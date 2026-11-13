# Streaming wrapper cache allocation candidate

The wrapper cache previously created a display class and callback on every lookup. The candidate passes the source and record index as a value tuple to a static `ConditionalWeakTable.GetOrAdd<TArg>` factory. The table, keys, values and record role are unchanged. Racing factories may still create more than one candidate, with one published wrapper returned to all callers. This tests cache publication only; it does not make the streaming pump concurrent.

The [adoption record](ADOPTION.json) was written before engine edits. XDM 3.1 sections 2.3 and 2.4 remain the identity/order contract; no intentional deviation or new language feature is adopted. The separate immutable .NET source review pin is explicitly not claimed as the installed runtime build commit.

| Check | Result |
|---|---|
| Candidate focused tests | 4 passed |
| Same tests, retained 1.81.8 binaries | 3 protective passes; allocation test fails as expected |
| Warmed 100,000 hits | Baseline 9,600,000 allocated bytes; candidate zero |
| Full solution and frozen gates | Pending |
| Complete pinned standalone qualification | Pending; original 1,200-second / 8 GiB limits apply |

The allocation test uses `GC.GetAllocatedBytesForCurrentThread` around only the warmed lookup loop. It makes no claim about total workload allocation, missed lookups, CPU percentages or whole-suite speed. Existing provider navigation/retention tests supplement the direct identity, distinct-source, race and weak-lifetime protections. No exclusions become passes and no test fixture, gate budget or workflow changes.

Development Core/server 1.81.9 is unpublished. Hosted qualification and independent artifact replay are required before merge.
