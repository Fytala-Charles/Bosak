# Direct private evidence transport

31 October 2026. Isolated candidate based on production 8080b18b (1.35.0); production sources remain unchanged.

The candidate moves transport attachment and evidence reads into Core and composes them directly with Runtime operations. The host-owned Bridge/Frame is removed. Runtime provides private issued-value reads and explicit constructor admission; native node atomization uses that path. XdmValue has no public diagnostic helpers and remains 48 bytes. Its candidate Equals/GetHashCode overrides still require compatibility disposition.

Two candidate processes passed 121 checks each; two control processes passed 103 each. Each evaluated 14 actual API/VM expressions with identical plain values and kinds across configurations. Both unchanged retained legacy callers passed 20/0. Five final isolated builds have zero warnings and errors. All seven loaded DLL hashes are checked in each process. The control replaces Runtime only; the candidate replaces Core and Runtime.

New controls cover unissued containers without reading getters, marked issuer payloads, plain conversion sources, conversion sources from another snapshot, expired operations, forged seals, unknown markers and reads after session closure. Existing native assessment identity/copy, no-op versus changed conversion, foreign-session, rebound payload/type, snapshot closure, canceled-wait and delayed raw-operation cleanup controls remain green. Unknown non-null markers now refuse; plain unmarked values return no evidence only while the session is open. Read validation invokes no provider callbacks.

This is a friend-test integration of six unchanged native assessment source files, not a build or qualification of the complete Schema package. Negative controls deliberately use existing friend access to raw internal helpers; those helpers are not a public authority boundary. The kernel checks its seal and live snapshot when evidence is read. Plain scalar getters remain ordinary payload access and do not themselves check evidence lifetime.

Constructor admission is explicit and bounded to host-issued built-in decimal/integer VM results. The integer type token is host-owned, not a native compiled integer component. Exact decimal no-ops retain source evidence; changed integer results detach assessment but remain session-bound. No automatic instrumentation of arbitrary nested casts, general facet validation, multi-source operation adoption, full XSD 1.1 or API freeze is claimed. No owner checkout or production engine changes were made, so full production solution/conformance receipts remain inherited.

Two compile-only failed host attempts are retained under evidence/attempt-1 and attempt-2; no host evaluation ran for those failed builds. The first successful 118-check candidate is retained separately under evidence/initial-success with its exact source. Final checks include three additional direct-admission/session-close controls.

Verify with `python -B docs/foundation-private-transport/tools/verify.py`. The [recipe](tools/run.py), [source diff](evidence/SOURCE-DIFF.txt), [specification basis](SPEC-BASIS.md) and exact inputs are sealed by MANIFEST.json. The recipe requires fresh evidence output paths for retained-caller execution; use a fresh working copy when reproducing it. Processes have bounded execution and scoped Windows error handling.

Next: qualify this private path across the complete atomic fixture set and measure attachment/read costs before production promotion. Public manifest/C45 acceptance, full consumer qualification and performance budgets remain pending.
