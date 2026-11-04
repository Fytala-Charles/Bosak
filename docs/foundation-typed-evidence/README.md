# TA-03: private resident scalar evidence

4 November 2026. Source **1.55.0**, based on `e7247910`, attaches private transport markers to the qualified transactional scalar entries. Marker construction occurs during staging, before batch publication. Explicit validation through a current admitted snapshot checks exact registered identity and payload representation, selected authority, current membership, raw lifetime/cancellation and supplied limits. No second registry or scalar accounting is introduced.

All **24,925 tests** pass, including **25 new evidence controls**, 35 retained registry controls, 94 container controls and 33 association controls. Runtime passes 605 cases. QT3 remains **31,397/0/424** and gated qt4 **8,941/0/1,918**, including 11 experimental passes, with identical skip maps. Eleven assemblies retain **310 public types / 6,369 declaration rows**, original friend grants and unchanged public API source. Exact source/binary archives, loaded hashes, complete runner receipts/TRX, allocation observations and documentation checks are retained under `evidence/`.

[Design](DESIGN.md), [acceptance](ACCEPTANCE.md), [case results](ACCEPTANCE.json), [adoption](SPEC-BASIS.md), [input pin](PLAN-INPUT.json) and [status](STATUS.json) define this internal qualification. Verify the exact candidate without rebuilding with `python docs/foundation-typed-evidence/tools/verify.py`.

This slice does not enable these markers in existing VM capture/evaluation. The explicit private validation helper returns a guarded registered container; it does not project a scalar into arithmetic or authenticate an arbitrary copied payload. Public advisor/container activation, SchemaAtomics and typed numeric profiles remain pending. Copies retain data and a private identity, not a resource lease.

Next: qualify the exact public advisor/container adapter and an explicitly selected VM identity route. Source-free construction, facts/nil/no-value, authorized typed numeric projection, compatibility, all-host/native/migration qualification, owner budgets, C45/freeze and publication remain separate work. R3 is InProgress.
