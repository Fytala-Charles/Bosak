# Exact-decimal capacity foundation

3 November 2026. Source **1.46.0** promotes the reviewed exact-decimal value type and implements internal bounded lexical ingress. [Consumer guide](GUIDE.md), [adoption basis](SPEC-BASIS.md), [scope/status](STATUS.json), [acceptance](ACCEPTANCE.json).

## Implemented behavior

`XdmExactDecimal` stores a normalized `BigInteger` coefficient and nonnegative scale; default is zero. Supplied digit/scale capacity is checked before normalization. Equality and hashing use the normalized value. Invariant formatting checks sign, decimal point, digits and padding against explicit output capacity before coefficient conversion or padding allocation.

The coefficient capacity check uses safe bit-length bounds, then an exact power-of-ten threshold only in the uncertain interval. It does not allocate a refused input's full decimal string. Normalization removes trailing zeros in nine-digit blocks before its final single-digit divisions. Internal lexical ingress validates ASCII decimal grammar and supplied digit/scale quotas before coefficient construction, with no floating-point/legacy decimal fallback. Capacity is distinct from malformed syntax and schema Invalid.

## Qualification

Release build and **24,559 tests** pass, including **43 new Core controls and three public-only API controls**. Core 205, API 530, Runtime 451; all 8,234 XSLT tests pass. QT3 stays **31,397/0/424**, gated qt4 **8,941/0/1,918** with 11 experimental passes. Both frozen skip maps remain byte-identical.

[Six warmed capacity-refusal observations](evidence/ALLOCATION-OBSERVATIONS.json) each allocate **464 bytes**, including caught exceptions. Inputs were already allocated: positive/negative 100,001-digit coefficients, a large formatted coefficient, extreme scale and 200,000-digit lexical inputs. These controls enforce a 16 KiB regression ceiling to detect full rejected-value formatting/coefficient/padding allocation; they do not accept owner workload, retained-memory or latency budgets.

All **309 types / 6,335 public rows** from 1.45.0 and original friend grants remain intact. One reviewed public type adds 20 rows: **310 types / 6,355 rows** total, 40 of the 47 reviewed types promoted. Callable signatures match; the real coefficient/scale storage and compiler-generated annotations replace the throwing proposal's empty struct metadata and remain explicitly recorded. Actual managed size/native ABI/freeze are not inferred.

[Source/binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [unit counters](evidence/UNIT-RESULTS.json), [loaded bindings](evidence/LOADED-BINDINGS.json), [API comparison](evidence/API-COMPARISON.json), [changed source](evidence/CHANGED-SOURCE.zip), [engines](evidence/ENGINE-BINARIES.zip) and [execution receipts](evidence/EXECUTION-RECEIPTS.zip) retain this cohort. The separate nonshipping 83-control prototype was not rerun. Run `python docs/foundation-exact-decimal/tools/verify.py` to verify this cohort.

## Next work

Audit and integrate exact numeric storage/accessors into `XdmValue`, preserving schema evidence and checked legacy narrowing. Then route bounded parser/arithmetic/schema semantics and extend all-host behavior. Numeric session evaluation remains disabled until those paths are qualified. R3 stays InProgress; actual native owner qualification, C45/freeze and publication remain pending. No owner checkout/pin or frozen conformance admission changes.
