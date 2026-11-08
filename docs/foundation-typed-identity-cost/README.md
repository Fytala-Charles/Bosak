# TA-06: typed identity setup allocation

4 November 2026. Source **1.57.1**, based on `fc44efc3`, uses the existing isolated-context factory for typed identity. Each call still gets fresh mutable execution state; standard functions share the existing immutable template. Function population, closed resources and every pre/post ownership/lifetime/representation check remain active.

The controlled compiled identity measurement falls from **23,360 to 4,104 bytes/call**, saving **19,256 bytes (82.4%)**. An independent comparison of the same compiled VM with mutable versus isolated registries explains that exact difference: `$value` uses **22,272/3,016** bytes and `upper-case($value)` **22,576/3,320**. The captured descriptor fingerprint is identical before and after. These are local allocation observations, not accepted owner latency, retained-memory or native budgets.

All **25,009 tests** pass, including **3 new controls** and all 46 typed identity controls. Runtime passes 689 cases. QT3 remains **31,397/0/424**, gated qt4 **8,941/0/1,918**, with 11 experimental passes and identical skip maps. Eleven assemblies retain **310 public types / 6,369 declaration rows** and original friends. API source, profile/descriptor code and constructor signatures are unchanged.

[Design](DESIGN.md), [acceptance](ACCEPTANCE.md), [case results](ACCEPTANCE.json), [adoption](SPEC-BASIS.md), [input pin](PLAN-INPUT.json) and [status](STATUS.json) define the scope. Exact source/binaries, loaded hashes, complete logs/TRX, baseline images and allocation/fingerprint comparisons are retained under `evidence/`. Verify without rebuilding with `python docs/foundation-typed-identity-cost/tools/verify.py`.

The diagnostic baseline rebuild embeds commit `fc44efc3` in assembly informational versions, so its hashes differ from the prior sealed build. Actual diagnostic images and loads are pinned separately; the preceding qualified 23,360-byte identity receipt remains immutable input. Neither rebuild nor comparison is presented as a new owner qualification.

Next: qualify bounded public typed-identity activation with exact advisor binding and container projection/refusal scope. Public schema activation, numeric projection, facts/construction/compatibility/all-host/native/migration qualification, representative owner budgets, C45/freeze and publication remain pending. R3 is InProgress.
