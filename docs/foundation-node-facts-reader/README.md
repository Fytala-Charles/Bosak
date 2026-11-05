# Internal original-node facts reader (TA-10)

5 November 2026. Source **1.61.0**, implementation base `b664d9a1`. The internal uncached reader authenticates an explicit original node/facts/input/origin tuple, then stages governing type, optional declaration, node kind, independent flags and a complete bounded typed value. It preserves **Absent**, **Unsupported** and **Supported** (including empty/list) outcomes. Every retained fact/container/atomic observation requires the live raw operation.

[Design and boundaries](DESIGN.md) · [Acceptance](ACCEPTANCE.md) · [Adopted basis](SPEC-BASIS.md) · [Status](STATUS.json) · [Case mapping](CASE-MAPPING.json)

Qualification: **25,185 unit tests**, including **71 new reader controls**; frozen QT3 **31,397/0/424** and gated qt4 **8,941/0/1,918**, with identical skip maps. LanguageServer builds successfully. Exported metadata remains 11 assemblies / 310 types / 6,369 rows; API source, field layout and friend grants are unchanged. [Exact source/binary pins](evidence/SOURCE-AND-BINARY-PINS.json), [units](evidence/UNIT-RESULTS.json), [loaded bindings](evidence/LOADED-BINDINGS.json) and [original receipts](evidence/EXECUTION-RECEIPTS.zip) retain the candidate identity.

The observed cold read allocates 35,051 bytes; warm metadata observation 64 bytes and 108 issuer callbacks per observation in this test fixture. [Cost receipt](evidence/FACT-ALLOCATION.json) is a local observation, not an accepted owner budget. Next attribute/reduce redundant callback work while preserving the guards before selecting a bounded public node-query profile.

Existing public identity/numeric behavior remains active and unchanged. The reader is not connected to the selected public advisor, general VM atomization or native providers. R3 remains InProgress; construction/compatibility/all-host/migration, native performance/owner acceptance, C45/freeze and publication remain pending. Verify with `python docs/foundation-node-facts-reader/tools/verify.py` against the qualified local binaries.
