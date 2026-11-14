# Primitive general-comparison candidate

Primitive scalar general comparisons previously built sequence iterators and two atomization buffers. A guarded route now reuses the existing untyped coercion and CompareCore logic directly for String/Integer/Decimal/Double/Float/Boolean pairs outside backwards compatibility mode. Other operands keep their existing route, including typed nodes/list values, lazy sequences and arrays. Materialization helpers, list ownership and public signatures are unchanged.

The [adoption record](ADOPTION.json) predates the engine edit and pins [XPath 3.1 REC-xpath-31-20170321](https://www.w3.org/TR/2017/REC-xpath-31-20170321/) sections 2.4.2, 3.7.1, 3.7.2 and 2.3.4, with no intentional deviation. Existing numeric precision limits remain. Development Core/server 1.81.10 is unpublished.

| Check | Result |
|---|---|
| New focused regressions | 4 passed |
| Existing typed-list regressions | 6 passed |
| Same new tests on qualified 1.81.9 binaries | 3 behavior passes; expected allocation regression fails |
| Warmed integer comparison | Baseline 1,008 bytes/call; candidate 0 across 100,000 calls per operator |
| Full solution | 26,430 passed |
| Separate language-server suite | 90 passed |
| Frozen QT3 | 31,397 / 0 / 424; all skip names/reasons unchanged |
| Gated qt4 | 8,941 / 0 / 1,918; all skip names/reasons unchanged, 11 experimental passes |
| Complete pinned standalone catalog | 10,221 / 0 / 4,380; all 14,601 identities/exclusions reconciled; 834.212s |

The four new tests cover all six operators with explicit expected results and warmed allocation checks; 726 primitive-pair/operator differential checks against the existing singleton-sequence route; explicit numeric promotion, NaN, untyped coercion, errors and collation; empty/array/multi-item/compatibility fallbacks; and lazy early-match/fault ordering with repeat enumeration. The earlier [local allocation probe](../scalar-materialization/README.md) is a historical snapshot: its pending qualification/publication statements refer to that earlier checkpoint.

Allocation is measured only with GC.GetAllocatedBytesForCurrentThread around warmed calling-thread loops. It establishes neither full-workload speed nor a hosted timeout cause. The same compiled regression assembly is used for candidate and retained-binary observations. The latter failure is a caught test assertion, not an unhandled .NET negative case. Original failed test-setup/compilation attempts are preserved separately; collation required FunctionLibrary.Populate and the lazy source required XdmSequence.FromSource. No machine-wide WER change.

The local source checkpoint is 6f6829955f53b2f084bf66836b0abbbf1139b4d2. The candidate nine DLLs used for qualification match every common engine DLL in the tested runtime output. The fixed corpus is fddf1cf920087e791f13315d68dfbe874d97dc56, Legacy/Xslt30 basic non-schema-aware profile, original 1200-second/8-GiB limits. No fixture, gate, exclusion or budget changes. The temporary QT3 fixture junction was verified and removed before the clean-source standalone preflight. Build passed with 10 warnings and no errors: existing compiler warnings and NU1900 vulnerability-feed availability warnings.

See [results](RESULTS.json),[compact evidence](EVIDENCE.zip),[manifest](MANIFEST.json),[offline verifier](verify.py) and[HTML receipt](DOCUMENTATION-CHECKS.json). Run `python docs/primitive-comparison/verify.py` from the repository to replay hashes, unit counts,all frozen skips and all 14,601 standalone outcomes without launching .NET. Evidence contains text transcripts/JSON/CSV and source snapshots; no runtime binary payload or raw profiler trace is added. Qualified runtime binaries and complete local packet remain in ignored `.guard-tmp/primitive-comparison-qualified-20261114` and `.guard-tmp/primitive-comparison-20261114`.

PR 490's postmerge run 43573964047 independently verifies the exact catalog result in 1124.188s. That historical run does not qualify this engine change. Hosted candidate checks and independent artifact replay remain required before merge. This work creates a reviewable PR; no release is performed.
