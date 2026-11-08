# PV-Q10 — exact package workload and owner review

7 November 2026. The bounded workload now runs against the qualified
**1.81.1-preview.c0b926de** package binaries from checkpoint
`c0b926de34824ceb2010d5f814a4450b28b31dcc`. The diagnostic runner restores its
nine engine dependencies through one Bosak.Xslt PackageReference from the local
feed into a newly empty private cache. Native Schema and the experimental provider
remain two separately pinned, unchanged binary references. There are no project
references or engine HintPath references. Build: **zero warnings and errors**.

[Cohort](COHORT.json), [protocol](PROTOCOL.json), [candidate pins](CANDIDATE-PINS.json),
[raw samples](evidence/MEASUREMENTS.json) and [summary](evidence/SUMMARY.json)
identify this separate execution. All engine package/cache/output/loaded hashes
match the preview package inventory. Actual loads use the Default noncollectible
context and preview informational identities; unused engines are present in the
verified layout but are not claimed behaviorally exercised by this workload.

| Fixture | Three repetition medians | Allocated bytes per cycle |
|---|---|---|
| One decimal child | 69.376 / 72.437 / 65.755 ms | 149,918–149,971 |
| Eight decimal children | 519.002 / 517.845 / 515.909 ms | 204,854–208,058 |

Six independent provider sessions complete **30 measured cycles and 135 checked
decimal reads**, with two warmups and five measured cycles per session. The
fixture/schema bytes, profile, finite limits and timed workload body are unchanged
from the original protocol. Compilation is recorded separately. Only cohort/version
metadata and dependency acquisition change. The runtime is .NET 10.0.11, x64,
Windows 10.0.26300 as recorded by the process.

No measured output root was observed alive after the runner's forced-GC check.
This does not prove zero retention or deterministic release timing. These small
candidate-only samples establish neither tail-latency bounds nor a regression
ratio. Earlier candidate runs are not an owner-selected comparable baseline.
All nine PF workload/baseline/budget acceptance slots remain open.

The unchanged comparator passes **14 method controls** and refuses this actual
candidate receipt presented as a baseline. [Execution receipts](evidence/EXECUTION-RECEIPTS.zip),
[layout archive](evidence/EXECUTION-LAYOUT.zip) and [source pins](evidence/SOURCE-PINS.json)
preserve the run. The earlier [full package regression qualification](../foundation-preview-qualification/README.md)
is reused over identical engine hashes: solution 26,342/0, language server 84,
native 451, adapter 59, consumer 10, frozen gates/skip maps and unchanged API.
Those lanes are not rerun or added to the 30 measurement cycles.

The [owner review](OWNER-REVIEW.md) now targets these exact package hashes and
their API inventory. Four refreshed templates remain deliberately unfilled;
screening refuses both them and the earlier production-binary templates. A
structurally complete return still requires separate owner review and a dated
disposition; this checker cannot authenticate approvals or validate budgets.

Next: obtain the concrete owner workload/baseline/budget and adoption decisions
listed in the review. Core-side artifacts and bounded evidence are prepared;
no further repetition can supply missing owner decisions. Production-provider
implementation, actual XmlDb/Braid workloads, four-owner C45/API freeze, final
release SemVer and signing/provenance review remain separate dependencies. Local
evidence can be checkpointed for review; no release tag, upload, owner message or
publication authorization has been performed in this slice.

Run `python docs/foundation-package-workload/tools/verify.py` for package/cache/
layout/load/source/method/receipt checks and unfilled/stale owner-return refusal.
Use `tools/owner_preflight.py <receipt>` for structural screening of a completed
return, keeping its file references within this packet. Preserve original receipt
paths for later runs. Static local links are checked; no new rendered-document
approval is claimed.
