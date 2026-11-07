# PV-Q06 — exact 1.81.1 workload cohort

7 November 2026. The separate finite runner uses the qualified **1.81.1** engine
binaries from checkpoint `c0b926de34824ceb2010d5f814a4450b28b31dcc`, with the
unchanged native Schema and immutable provider DLLs. The isolated Release build
has **zero warnings and errors**. No production or sibling source was rebuilt or
modified. This is observational preparation, not owner performance acceptance.

[Cohort](COHORT.json), [protocol](PROTOCOL.json), [raw measurements](evidence/MEASUREMENTS.json),
[summary](evidence/SUMMARY.json), [execution layout](evidence/EXECUTION-LAYOUT.json)
and [layout archive](evidence/EXECUTION-LAYOUT.zip) retain the exact references,
runtime configuration, source/fixture pins and measured samples. The
[execution receipts](evidence/EXECUTION-RECEIPTS.zip) retain scoped watchdog runs.
Actual loaded assembly hashes match the qualified engines and unchanged provider
and native binaries, in the default noncollectible context. Nine engines are
present in the layout; only assemblies actually needed by this workload are
claimed loaded. A metadata-only startup observer is pinned separately.

| Workload | Three repetition medians | Allocated bytes per cycle |
|---|---|---|
| One decimal child | 63.694 / 64.310 / 64.353 ms | 149,918–149,971 |
| Eight decimal children | 535.303 / 525.602 / 523.457 ms | 204,854–208,058 |

Six independent provider sessions perform **30 measured cycles and 135 checked
decimal reads**. Each session has two warmups and five measured cycles.
Compilation/binding is recorded separately. The timed unit remains parse,
assess, admit, navigate, evaluate all selected children in one raw operation and
dispose owners. Fixture bytes, limits, profile and timed workload body match the
earlier protocol; only version metadata and qualified references change.

No measured output root remained alive in the runner's forced-GC observation.
That does not prove zero retention or deterministic release timing. The small
sample does not establish tail-latency bounds, statistical significance or a
general performance guarantee. No percentage improvement is claimed: earlier
candidate observations are not an independently selected accepted baseline, and
these runs do not control all machine noise or observer differences.

The unchanged method comparator passes **14 synthetic controls**. A Python-only
negative check refuses this actual candidate receipt presented as a baseline;
no unhandled managed negative case is launched. The verifier independently checks
the cohort, source body, configuration, fixture and loaded-binding hashes and
the six measured rows. An initial verifier field-name mismatch (`errors` versus
the observer's `captureErrors`) was corrected; measurement and build receipts
were unchanged.

All nine PF owner workload/baseline/budget acceptance slots remain open in the
[gap review](../foundation-owner-gap-review/README.md). This two-fixture PF-03
decimal subset does not qualify general list/union, persistent store, worker,
concurrent, large-scale or all-host workloads. Full solution and frozen-gate
qualification remains the unchanged [production evidence](../foundation-captured-identity-production/README.md),
not a new full test run in this slice. No requirement is waived, API frozen,
release version selected, package built, tag created or publication authorized.

Next **PV-Q07**: review a concrete production-provider adoption proposal and the
owner workload/baseline/budget return, using this cohort as candidate evidence.
Owner-reviewed expansion beyond this bounded profile and fresh-cache package-only
consumer preparation remain separate work. No owner has been contacted.

Verify with `python docs/foundation-workload-1811/tools/verify.py`. Preparation
and launch scripts intentionally retain separate paths and refuse reuse of the
existing launch layout; preserve original receipts when preparing later runs.
Static local-link checks cover this packet; no new rendered-document approval is
claimed.
