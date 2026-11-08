# Draft owner workload and budget proposals

7 November 2026. This proposal uses the actual owner repository constraints and
the qualified **1.81.1-preview.c0b926de** candidate evidence. It is a review draft;
it does not supply owner approval, select a baseline or change release gates.

[Machine-readable proposal](PROPOSAL.json) maps all nine PF scenarios to concrete
workload choices and missing inputs. Source documentation, candidate pins and
measurement receipts are hash-bound in that file. The
[live owner review](../foundation-owner-repo-review/README.md) identifies the actual
repositories and distinguishes historical design scope from package acceptance.

| Scenario | Proposed starting workload | Input still needed |
|---|---|---|
| PF-01 | Untyped scalar/axes/map/array; first/full consumption | Shipped/candidate fixture equivalence and ordinary-path budgets |
| PF-02 | Schema compile/reuse and supported/foreign component lookup | Real schema sizes/profile and separate compile/lookup budgets |
| PF-03 | Measured decimal subset; separate list/union/nil/no-value/preserve/strip/foreign expansion | Owner-approved subset and genuinely supported native carrier fixtures |
| PF-04 | Persistent reader across two immutable store versions | Original store snapshots, cold/warm policy and read amplification |
| PF-05 | Partial consumption, close/cancel/reentry and cleanup faults | Exact lease events and deterministic release criteria |
| PF-06 | Real worker XSLT, same/distinct roots and source-free invocation | Typed/persistent routing decision and acquisition-to-output phase budgets |
| PF-07 | Reuse and schema/profile/source/snapshot invalidation; kill/reopen | Actual worker configuration, currentness and recovery observations |
| PF-08 | Size/depth/width ladder and refusal/cancellation schedules | Representative scales and responsiveness/resource limits |
| PF-09 | Actual callback/dispatch/conversion and metadata reuse | Demonstrated API-shape bottlenecks before any new interface proposal |

For the two measured decimal fixtures only, the draft diagnostic guardrails are:

| Fixture | Proposed latency ceiling | Proposed allocation ceiling |
|---|---|---|
| One child | 150 ms/cycle | 327,680 bytes/cycle (320 KiB) |
| Eight children | 1,050 ms/cycle | 458,752 bytes/cycle (448 KiB) |

These are deliberately provisional review starting points: twice the largest
observed session statistic, rounded up to 25 ms for latency or 64 KiB for
allocation. The factor of two is discretionary headroom, not a measured variance
bound, a business requirement or proof of acceptable performance. The latency
statistic is the maximum of three session medians, each over five measured
cycles. Allocation is the maximum session-average allocation per cycle on the
measuring thread. Compilation remains outside those execution windows. No
percentile or whole-process memory bound is inferred.

The observed [package workload](../foundation-package-workload/evidence/SUMMARY.json)
has one-child medians 65.755–72.437 ms and eight-child medians 515.909–519.002 ms.
All other numerical thresholds stay unset pending representative workloads.
No regression ratio or relative tolerance is proposed without an independently
selected comparable baseline. Candidate repetitions do not fill that role.
Forced-GC observations remain descriptive and cannot replace deterministic lease
release or prove zero retention.

Braid's recorded 8 MiB document, 64 MiB frame and 30-second post-launch deadline
remain historical operational controls. They are not accepted candidate PF
latency/allocation budgets or bounds on native memory and time between checks.
Earlier experimental snapshot/cache/atom/digit/scale ceilings remain experiments.

The [owner return packet](../foundation-package-workload/OWNER-REVIEW.md) remains
the place for dated dispositions over exact artifacts. Each owner must select
fixtures/profile/limits and baseline, revise or accept numerical budgets with
units/statistics/tolerances, and identify production adoption work. All nine
accepted-budget fields remain null; every approval remains false. Nothing has
been sent to owners or written into their repositories.

Next independent implementation preparation: inspect the live XmlDb ordinary
snapshot/operation route and prepare an isolated exact-package source/binary
qualification recipe. Keep persistent format bytes and ownership semantics
unchanged; do not fabricate Schema authority or infer worker routing approval.
Any semantic implementation change needs its immutable adoption evidence first.
Broader owner workload and budget choices remain explicit review inputs.

Run `python docs/foundation-owner-workload-draft/tools/verify.py` to check the
draft, evidence pins, arithmetic and local links. No new engine test, workload
measurement, rendered-document approval or release acceptance is claimed.
