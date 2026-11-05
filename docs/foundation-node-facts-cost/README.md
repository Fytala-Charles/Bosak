# Original-node metadata guard cost (TA-11)

5 November 2026. Patch source **1.61.1**, base `23b83b74`. The internal original-facts reader now shares adjacent ownership proofs and calls authority ownership directly under the existing availability checks. It retains a full facts/source/binding proof before and after each original type/declaration callback, without a cache across observations.

[Design and qualification](ACCEPTANCE.md) · [Adopted basis](SPEC-BASIS.md) · [Cost comparison](evidence/COST-COMPARISON.json) · [Machine status](STATUS.json) · [Retained 22-case mapping](CASE-MAPPING.json)

Full qualification passes **25,193 tests**, including **8 new authority-callback cases** and all 71 original reader cases. Frozen QT3 **31,397/0/424** and gated qt4 **8,941/0/1,918** retain exact skip maps. API source, exported metadata (11 assemblies / 310 types / 6,369 rows), field layout and friend grants remain unchanged. Exact sources, binaries, original commands/TRX and loaded binding observations are retained in evidence. Verify with `python docs/foundation-node-facts-cost/tools/verify.py`.

Local cold read allocation falls from **35,051 to 1,396 bytes/read** (about **96.0%**). Warm metadata observation falls from **64 to 0 bytes**, and selected issuer callbacks from **108 to 54**. The full-run receipt is authoritative; the focused run measured 1,247 cold bytes due to allocation/JIT variation. This is not a native workload, latency result or accepted owner performance budget.

Next select a bounded public original-node query profile and descriptor, including exact ownership, optional nilled results and absent-value atomization behavior. Public node-fact activation remains pending. Existing public identity/numeric profiles retain their behavior; native/all-host/migration, construction/compatibility, owner budgets/acceptance, C45/freeze and publication remain pending. R3 stays InProgress.
