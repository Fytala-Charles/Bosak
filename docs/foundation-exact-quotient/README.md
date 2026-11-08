# Exact quotient and remainder qualification â€” 4 November 2026

Source **1.50.0**, based on `e8dc3154`, adds bounded primitive integer/decimal `idiv` and `mod` through `bosak-exact-quotient-v3`. Existing arithmetic v1, scalar v2 and ordinary profile identities remain unchanged. R3 remains InProgress.

All **24,714 tests** pass (nine successful full-solution projects plus the unchanged database project rerun with local loopback permission), including 29 new controls (four Core tests, 23 public API cases and two additional lifecycle cases). QT3 remains **31,397/0/424** and gated qt4 **8,941/0/1,918**, with 11 experimental passes and unchanged skip maps. All 11 assemblies retain 310 public types, 6,369 declaration rows and friend grants.

The Core tests include 2,000 independent signed-decimal oracle comparisons, exact quotient/remainder reconstruction, coefficients exceeding CLR precision, extreme scale gaps and capacity/cancellation refusals. Public controls cover types, precedence, string/comparison composition, dynamic zero errors, reuse and close/cancellation with retained owner leases. These controls do not establish accepted owner performance budgets or hard time/aggregate-memory limits.

Read the [consumer guide](GUIDE.md), [immutable adoption basis](SPEC-BASIS.md), [acceptance results](ACCEPTANCE.json) and [status](STATUS.json). Actual loaded bindings, complete runner receipts, exact source/binary pins and replay archives are under `evidence/`. Verify without rebuilding using `python docs/foundation-exact-quotient/tools/verify.py`.

Next: settle decimal `div` precision/rounding and bounded-work policy before implementation, then schema/all-host integration. Native qualification, typed-default source migration disposition, C45, API freeze, final release SemVer and publication remain pending. Historical private-prototype receipts were not rerun. No tag or package publication follows from this checkpoint.

Execution correction: the initial sandboxed database fixture failed creating `HttpListener` (invalid handle), before its affected tests could execute. The permission-enabled project rerun passes all 47 tests with the same engine hashes. Both original and corrected receipts are preserved; no production or test-source workaround was introduced.
