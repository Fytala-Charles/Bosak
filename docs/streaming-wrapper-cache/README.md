# Streaming wrapper cache allocation candidate

The wrapper cache previously created a display class and callback on every lookup. The candidate passes the source and record index as a value tuple to a static `ConditionalWeakTable.GetOrAdd<TArg>` factory. The table, keys, values and record role are unchanged. Racing factories may still create more than one candidate, with one published wrapper returned to all callers. This tests cache publication only; it does not make the streaming pump concurrent.

The [adoption record](ADOPTION.json) was written before engine edits. XDM 3.1 sections 2.3 and 2.4 remain the identity/order contract; no intentional deviation or new language feature is adopted. The separate immutable .NET source review pin is explicitly not claimed as the installed runtime build commit.

| Check | Result |
|---|---|
| Candidate focused tests | 4 passed |
| Same tests, retained 1.81.8 binaries | 3 protective passes; allocation test fails as expected |
| Warmed 100,000 hits | Baseline 9,600,000 allocated bytes; candidate zero |
| Full solution | 26,426 passed; 90 separate language-server tests passed |
| Frozen QT3 | 31,397 / 0 / 424; every skip name/reason unchanged |
| Gated qt4 | 8,941 / 0 / 1,918; every skip name/reason unchanged |
| Complete pinned standalone qualification | 10,221 passed / 0 failed / 4,380 excluded; 14,601 identities reconciled; original 1,200-second / 8 GiB limits |

The allocation test uses `GC.GetAllocatedBytesForCurrentThread` around only the warmed lookup loop. It makes no claim about total workload allocation, missed lookups, CPU percentages or whole-suite speed. Existing provider navigation/retention tests supplement the direct identity, distinct-source, race and weak-lifetime protections. No exclusions become passes and no test fixture, gate budget or workflow changes.

Development Core/server 1.81.9 is unpublished. Hosted qualification and independent artifact replay are required before merge.

The sandbox validation attempt failed to start HTTP listeners. The first owner-context attempt also lacked QT3 schema fixtures and had one local HTTP port collision. Both failed attempts are retained separately. The corrected fixture-backed full solution run passed in one invocation; no tests were omitted or combined into that count. The temporary QT3 junction was verified and removed before the standalone clean-source preflight.

See [results](RESULTS.json), [evidence archive](EVIDENCE.zip), [hash manifest](MANIFEST.json) and [offline verifier](verify.py).

The preceding GC-correlation PR #488 remains unmerged: its required hosted run 43484674916 timed out at 1,200.234 seconds, with namespace-3152 the last started case. The original failure artifact (14067932353; SHA-256 a4a86908f4308be35b30f80a068d9ad3e66f7200b877a285c146329fbd42b9a4) is retained separately inside this evidence archive. It was not retried or reclassified by this candidate.
