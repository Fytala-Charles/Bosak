# Exact scalar qualification — 4 November 2026

Source **1.49.0**, based on `89cefa47`, qualifies exact singleton numeric comparisons, numeric map-key equality/hashing and bounded top-level scalar string output. R3 remains InProgress.

All **24,685 tests** pass, including 50 new controls. QT3 remains **31,397/0/424** and gated qt4 **8,941/0/1,918**, including 11 experimental passes; both skip maps match. All 11 assemblies retain 310 public types, 6,369 declaration rows and their friend grants. No public metadata was added.

Read the [consumer guide](GUIDE.md), [adoption evidence](SPEC-BASIS.md), [status](STATUS.json) and [acceptance results](ACCEPTANCE.json). Source/binary pins, actual loaded bindings, complete runner receipts and changed-source/engine archives are retained under `evidence/`. Replay the read-only audit with `python docs/foundation-exact-scalars/tools/verify.py`.

The 38 scalar API controls and ten new Core controls cover exact IEEE values, wide keys, comparison precedence/refusals, quotas and formatting. Two additional lifecycle cases exercise scalar close/cancellation during dispatch; the retained arithmetic controls now number 39. Key allocation observations use already-owned coefficients (500 warmups, 10,000 iterations); three measured operations allocate zero bytes per operation. These are local observations, not accepted latency, memory or owner budgets.

Numeric map-key hashes now derive from normalized coefficient/scale pairs; hash codes are process-local and must never be persisted as identifiers. The arithmetic-only v1 and ordinary profiles remain available. Existing general VM/parser arithmetic, wider serialization and schema behavior are outside this scalar qualification.

Next: qualify remaining numeric operators, then schema/all-host integration. Native owner qualification, typed-default source migration disposition, C45, API freeze, final release SemVer and publication remain pending. Earlier private-prototype receipts are historical and were not rerun. No tag or publication is authorized by these results.
