# TA-06 disposition

| Obligation | Executable evidence |
|---|---|
| Attribute registry setup | Same compiled `$value` and `upper-case($value)` with mutable versus isolated fresh contexts; identical expected outputs and 19,256-byte difference in both cases. |
| Actual owner-bound identity saving | Unchanged TA-05 measurement runs against 1.57.1: 23,360 to 4,104 bytes/call, under live pre/post guards and without payload getter replay. |
| Preserve fresh mutable state | Two identity calls under one raw operation receive different contexts; a deliberately injected variable from the first call is absent from the second. |
| Preserve selected semantics | Same recorded descriptor fingerprint before/after, unchanged API/profile source, all 46 compiled identity/poison/lifetime/output fault controls retained. |
| Function and profile boundaries | Standard function evaluation remains active; full sharing/mutation/compatibility and ordinary/numeric tests run with unchanged public guards. |
| Exact qualification | 25,009 unit passes, frozen gates and skip maps, final loaded hashes, unchanged exported metadata/friends. Diagnostic baseline images are separately identified. |

The focused run passes **271 controls**: 3 cost/isolation, 46 identity, 35 adapter, 25 evidence, 35 registry, 94 container and 33 association cases. Full solution qualification is separate. Local ceilings guard regressions; no native or owner performance acceptance is inferred.
