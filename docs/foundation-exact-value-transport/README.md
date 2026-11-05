# Exact numeric value transport

4 November 2026. Source **1.47.0** implements the six reviewed exact XdmValue numeric factory/accessor signatures and disambiguates private evidence from numeric payloads without adding instance fields. [Consumer guide](GUIDE.md), [adoption basis](SPEC-BASIS.md), [consumer audit](AUDIT.md), [status](STATUS.json), [acceptance](ACCEPTANCE.json).

## Implemented scope

BigInteger integers and XdmExactDecimal decimals retain the complete value. Small integers stay inline. Private numeric holders and the existing marker box preserve payload/annotations through evidence attachment, replacement and removal. The actual evidence kernel admits equivalent exact holders and rejects changed values behind a copied marker. Numeric payloads are never mistaken for issuer markers.

Legacy Int64/decimal getters narrow only when exact; decimal conversion uses 96-bit coefficient/scale checks and direct bit construction. Legacy formatting uses the checked getters, while explicit full formatting uses the exact accessors. Direct Boolean value reads zero/nonzero without narrowing. CLR equality/hash remain representation-based and schema-neutral. No public provenance factory or arbitrary numeric query behavior is enabled.

## Qualification

Release solution build and **24,591 tests** pass, including **29 new Core controls and three public-only API controls**. Core 234, API 533, Runtime 451; all 8,234 XSLT tests pass. QT3 remains **31,397/0/424**, gated qt4 **8,941/0/1,918** with 11 experimental passes. Both frozen skip maps are byte-identical.

[Warm allocation observations](evidence/ALLOCATION-OBSERVATIONS.json) retain zero-allocation small-integer factories, inline marker attachment, cached exact reads and selected checked CLR-decimal reads. Wide-integer/exact-decimal holders cost 32/40 bytes; marking a prebuilt wide numeric carrier costs 32 more. Inputs are already owned. The qualified 64-bit XdmValue remains 48 bytes. No latency/retained-memory/owner budget or native ABI freeze is inferred.

All **310 types / 6,355 previous public rows**, field layout rows and friend grants remain. The six intended signatures are checked directly against the immutable accepted `IXdmValueNumericMemberWitness` source; they add **14 metadata rows**, for **310 types / 6,369 rows** total. The witness was an internal declaration-intent scaffold, so these additions are compared to its exact source instead of pretending they were already present in the 47-type public inventory.

**Known source migration:** `FromDecimal(default)` now gives expected compiler error CS0121; use `default(decimal)` for the existing CLR-decimal call. [Baseline/refusal/correction receipts](evidence/SOURCE-COMPATIBILITY.json) preserve this result. Existing binary signatures remain; actual owner compatibility and final release SemVer remain pending.

[Source/binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [unit counters](evidence/UNIT-RESULTS.json), [loaded hashes](evidence/LOADED-BINDINGS.json), [API/witness audit](evidence/API-COMPARISON.json), [changed source](evidence/CHANGED-SOURCE.zip), [engines](evidence/ENGINE-BINARIES.zip) and [receipts](evidence/EXECUTION-RECEIPTS.zip) retain this cohort. The separate nonshipping 83-control prototype was not rerun. Verify with `python docs/foundation-exact-value-transport/tools/verify.py`.

## Next work

Enforce operation numeric limits and route exact parsing, arithmetic, comparisons, map keys and serialization before enabling the numeric session profile. Then complete schema/all-host and actual owner source/unchanged-binary qualification, including the typed-default migration. R3 remains InProgress; C45/freeze and publication remain pending. No owner checkout/pin or conformance admissions change.
