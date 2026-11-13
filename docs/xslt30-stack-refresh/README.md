# Sampled-stack PR delivery refresh

PR #487 incorporates qualified main `8e2b58ebf6fad4473fbe527dfc19ee571134e84d` after PR #486 passed all six checks and independent required-artifact replay. Original commits are retained by a merge, without rebase or force-push. The sampled-stack tool and its tests are byte-identical to original head `8fe48d697b84c01031d5135c247aee3c258f6a0b`. CI retains the inherited case-cost control command and adds the original sampled-stack control command exactly once. Production engine, tests, runner and required gate match main.

| Retained attempt | Result | Interpretation |
|---|---|---|
| Original PR487, 43475843856 | Timed out at 1,200.234 seconds; last started sx-treat-017 | Failed and retained |
| Qualified PR486, 43505029278 | 10,221 / 0 / 4,380 in 1,170.485 seconds | All 14,601 identities/exclusions replayed; all six checks green before merge |
| Prior main, 43503847924 | 10,221 / 0 / 4,380 in 1,025.234 seconds | Complete independent artifact replay |
| Refreshed PR487 | Required hosted qualification pending | Actual 1.81.9 engine fix inherited from main; original 1,200-second / 8 GiB limits |

The [original sampled-stack dossier](../xslt30-stack-profile/README.md) remains byte-for-byte sealed: one original trace, 54 selected cases, 52 passes, two original schema-aware exclusions and 14,547 unselected cases. The original caller, nine 1.81.8 engines, native runtime, profiler/package and API/source-review pins remain distinct from this later delivery. The corrected cross-thread analysis and its pre-analysis review remain intact. Verification replays that analysis without a new attribution change or trace collection. Existing tooling continues to enforce its original immutable baseline.

Sampling weight is thread wall-time, not CPU percentage. Converted virtual leaves and missing caller ancestry are not exclusive CPU attribution. Earlier monitor/GC/cache contexts identify investigation leads, not a particular cache's exact cost or a timeout cause. Separate hosted durations do not establish suite-wide speedup or a failure rate.

Sixteen sampled-stack controls and fifteen inherited case-cost controls pass, and release workflow validation passes using an existing workspace-isolated CI-pinned PyYAML 6.0.3 dependency. No global environment/package change, new .NET execution, trace capture, diagnostic dispatch or qualification retry. No engine behavior, public API, production test, required workflow, fixture, exclusion/admission, budget, ruleset or publication change. Core/server 1.81.9 remains unpublished.

[verify.py](verify.py) independently replays retained delivery artifacts and original dossiers without executing .NET. Historical checkout-specific boundaries are checked at their recorded commits, separately from the current tool/CI boundary. Sealed original verifiers remain unchanged. See the [manifest](MANIFEST.json), [controls](CONTROLS.json), [workflow validation](WORKFLOWS.json), [merge receipt](MERGE486.json) and [HTML receipt](DOCUMENTATION-CHECKS.json).
