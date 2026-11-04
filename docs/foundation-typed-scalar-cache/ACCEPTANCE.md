# TA-02 implementation disposition

| Retained obligation | Executable evidence |
|---|---|
| Scalar accounting and empty/list shape | Five shape cases; 1,000 distinct empty containers retain zero slots; mixed selected types and governing/list metadata survive warm reads. |
| Exact scope and identity | Foreign raw operation, null/original origin, same-label references with throwing equality/hash, and shared container across different snapshots/issuers. |
| Whole-batch capacity and rollback | Existing identities survive capacity refusal; fixed faults before insertion and after each of three insertions roll back the prefix; retry succeeds. |
| Concurrent publication | Two real raw operations reuse one complete winning group; distinct groups recheck remaining capacity under the commit lock. |
| No callbacks under commit | Issuer callback probes cache count from another task; commit uses only managed checks and reference hashing. |
| Current guards and warm reuse | Twenty fresh raw operations reuse identities without getters; current container/type/atom/numeric limits still apply, including wide exact payloads. |
| Cancellation and close | Precommit abort leaves zero slots; postcommit delivery cancellation leaves a complete group; close during delivery clears it. Escaped staged transactions and raw views refuse. |
| Scalar-mode exclusivity | Legacy-first, registry-first and concurrent selection cannot establish two independent registries on one snapshot. |
| Actual ownership and error ordering | File handle remains open through logical close until raw completion, releases once; primary work plus cleanup failure preserve order and faulted drain. |
| Cost observations | Cold scalar, warm three-cell and caught capacity refusal; runtime warmup 100 and measurement 1,000 reads. Local ceilings only. |
| Full regression boundary | 24,900 tests, unchanged QT3/qt4 gates and skip maps; exact loaded candidate hashes, unchanged API source, metadata and friends. |

The [original checklist](../foundation-typed-cache-policy/IMPLEMENTATION.md) and its unchecked boxes remain an immutable preparation record. The completed scope is recorded here and in [machine-readable status](STATUS.json). This disposition establishes internal registry qualification only. It does not establish public activation, native qualification, owner budgets, C45, freeze or publication.
