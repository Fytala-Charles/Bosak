# Type-matcher closure candidate

Two capturing Any predicates for element/attribute name alternatives caused ValueMatchesType to create a closure even for scalar matches. Ordered loops now call the same matcher in the same order and return on the first successful alternative. Namespace/schema context, schema suffixes and exception order are preserved. The 40-byte primitive validation cost remains a separate concern; there is no new scalar shortcut or validity cache.

The [adoption record](ADOPTION.json) predates engine edits and pins [XPath 3.1 REC-xpath-31-20170321](https://www.w3.org/TR/2017/REC-xpath-31-20170321/) sections 2.5.5.3, 2.5.5.5, 3.1.5.2 and 2.3.4. No intentional deviation or new alternative-name grammar adoption. Development Core/server 1.81.11 is unpublished.

| Check | Result |
|---|---|
| New regressions | 5 passed |
| Existing typed-list regressions | 6 passed |
| Same new tests on qualified 1.81.10 | 4 behavior passes; expected allocation test fails |
| Warmed primitive type matching | Baseline 72 bytes/call; candidate 0 |
| Warmed matching primitive conversion | Baseline 112 bytes/call; candidate 40 |
| Full solution | 26,435 passed |
| Separate language-server suite | 90 passed |
| Frozen QT3 | 31,397 / 0 / 424; all skip names/reasons unchanged |
| Gated qt4 | 8,941 / 0 / 1,918; all skip names/reasons unchanged, 11 experimental passes |
| Complete pinned standalone | 10,221 / 0 / 4,380; all 14,601 identities/exclusions, 803.274s |

Five new tests check warmed matching/conversion across integer, string, boolean, decimal and double; element/attribute alternative ordering and first-match short-circuiting; undeclared-prefix errors; namespace context mutation; schema suffixes and typed-node conversion; choice/empty and namespace-sensitive conversion errors. The same compiled regression assembly is used with the retained old and candidate DLLs. The identical earlier allocation-diagnostic binary was also replayed on the exact qualified candidate DLLs under a 60-second/512-MiB bound: all 28 check/observation entries succeed, the five identical primitive matching rows change 72 to 0 bytes and conversion 112 to 40, and the compiled method has no entry closure constructor. The original failure is a caught test assertion, not an unhandled .NET negative case. Full build succeeds; existing compiler/feed-availability warnings are retained in the original log. All local tests passed on the first run for this implementation.

Allocations use GC.GetAllocatedBytesForCurrentThread across 100,000 calls after 10,000 warm-up calls, with input/setup outside the loops. Results are calling-thread micro-allocation observations on this qualified runtime, not a full-workload speed or hosted timeout diagnosis. The earlier [matching-conversion investigation](../matching-conversion/README.md) is a historical pre-implementation snapshot; its local/unpublished statements refer to that earlier state. Compiler-generated closure labels are not compatibility contracts.

Qualified source checkpoint: 7106ec760101a0aea3cffaf9d4766eb3a79285b3. Exact candidate engine hashes match every common engine DLL in the tested runtime output. The original frozen corpus is fddf1cf920087e791f13315d68dfbe874d97dc56, Legacy/Xslt30 basic non-schema-aware, original 1200-second/8-GiB limits. Corpus and runtime hashes remain unchanged. No fixture, exclusion, gate, workflow or budget change. The temporary QT3 fixture junction was verified and removed before clean-source standalone preflight.

See [results](RESULTS.json), [compact evidence](EVIDENCE.zip), [manifest](MANIFEST.json), [offline verifier](verify.py) and [HTML receipt](DOCUMENTATION-CHECKS.json). Run `python docs/matcher-closure/verify.py` to verify hashes, source binding, unit counts, allocation regression, every frozen skip and all 14,601 standalone outcomes without launching .NET. Evidence contains text/JSON/CSV/source snapshots; no new runtime binaries or raw profiler traces. Full local runtime/packet remain ignored under `.guard-tmp/matcher-closure-qualified-20261114` and `.guard-tmp/matcher-closure-20261114`.

Prior PR491 main independently qualified in 800.890s; that historical receipt does not qualify this change. Hosted checks and independent artifact replay remain required before merge. No release is performed.
