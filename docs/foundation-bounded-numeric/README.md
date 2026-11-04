# Bounded exact arithmetic profile

4 November 2026. Source **1.48.0** enables the selected `bosak-exact-arithmetic-v1` profile through existing session signatures. [Consumer guide](GUIDE.md), [adoption basis](SPEC-BASIS.md), [scope/status](STATUS.json), [acceptance](ACCEPTANCE.json).

## Implemented behavior

Integer/decimal literals and unprefixed numeric variables support parentheses, unary signs, +, - and *. Literal capacity is checked before constructing coefficients; supplied bindings (including unused values) and every aligned/intermediate/result coefficient are admitted under explicit numeric digit/scale limits. Addition/product preflights refuse before oversized result arithmetic. No legacy numeric parsing, rounding or VM fallback occurs.

The finite grammar compiles iteratively into an immutable postfix plan. Static context/profile/limits are captured by the actual compiler. Pooled evaluation storage is cleared on return, with raw availability/cancellation checks during dispatch. Ordinary string/boolean selection and legacy host execution are retained. Functions, division/floating point, comparisons, schema/typed operations, node atomization and full numeric/all-host behavior remain pending. Numeric capacity is not a hard total-memory, expression-size or time budget.

## Qualification

Release solution build and **24,635 tests** pass, including **seven new Core controls and 37 public arithmetic controls**, with all 38 previous public ordinary controls retained. Core 241, API 570, Runtime 451; all 8,234 XSLT tests pass. QT3 remains **31,397/0/424**, gated qt4 **8,941/0/1,918** with 11 experimental passes. Frozen skip maps remain byte-identical.

[Three warmed refusal observations](evidence/ALLOCATION-OBSERVATIONS.json) allocate 760 bytes for an oversized pre-owned input and 464 each for extreme alignment and scale-sum refusal, including exceptions. The regression ceiling is 16 KiB; inputs were already allocated. No owner workload/throughput/retained-memory budget is selected. Earlier carrier allocation observations remain historical.

All **310 public types / 6,369 rows**, XdmValue field rows and friend grants are byte-for-row identical to the previous inventory. New behavior is a separately named bounded profile, not new public declarations or complete numeric qualification. The known typed-default source migration from 1.47.0 still applies.

[Source/binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [unit counters](evidence/UNIT-RESULTS.json), [loaded hashes](evidence/LOADED-BINDINGS.json), [metadata audit](evidence/API-COMPARISON.json), [changed source](evidence/CHANGED-SOURCE.zip), [eleven engines](evidence/ENGINE-BINARIES.zip) and [execution receipts](evidence/EXECUTION-RECEIPTS.zip) retain this cohort. Initial focused failures exposed a primitive admission check that excluded absent type annotations; the final corrected controls/full gates pass, and the original receipts remain archived. The separate nonshipping 83-control prototype was not rerun. Verify with `python docs/foundation-bounded-numeric/tools/verify.py`.

## Next work

Implement exact comparisons/map-key equality/hashing and bounded serialization, then remaining numeric operators, schema and all-host integration. Actual native owner qualification, source-migration/final SemVer disposition, C45/freeze and publication remain pending. R3 stays InProgress; owner checkouts/pins and conformance admissions are unchanged.
