# C45 numeric domain and host characterization

31 October 2026. Diagnostic checkpoint against the qualified 1.36.0 binaries; no production change or API acceptance. The [specification basis](SPEC-BASIS.md) was recorded before execution. The preceding [declaration proposal](../foundation-c45-delta136/PROPOSAL.md) remains immutable.

Two guarded processes each captured 42 observations (14 fixtures across XPath, XQuery and XSLT) and five passing host controls. All selected within-profile cases retain their exact expected values in all three hosts. The two final receipts are identical. Ten qualified engine DLL hashes remain unchanged. See [observations](evidence/probe-0.json), [repeat](evidence/probe-1.json), [source and binary pins](evidence/INPUTS.json) and [runner](tools/run.py).

| Fixture | Observed behavior in all three hosts |
|---|---|
| Integer 9223372036854775808 and unsignedLong maximum | Exact, still classified as integer. Int64 alone is not the current supported domain. |
| Integer 79228162514264337593543950335 | Exact through the decimal-backed integer path. |
| Next integer and a 40-digit integer literal | Double fallback; integer classification lost. |
| 40-digit xs:integer constructor | FORG0001. |
| Decimal with 28 fractional digits | Exact for the selected fixture. |
| Decimal with 39 fractional digits, literal and constructor | Rounded to 28 fractional digits. |
| Difference between selected adjacent 40-digit integers | Zero instead of the exact reference one. |

These are bounded observations, not a complete numeric domain guarantee or a new conformance verdict. Outside-profile observations are not admissions into the frozen gates. The deliberately invalid decimal constructor is caught. No unhandled crash test was launched.

The typed-node test double has lexical text 999 and decimal typed value 10. All three public hosts observe 10. This is ordinary node atomization evidence, not authenticated assessment/PSVI integration. XQuery preserves its effective prolog namespace after its compiler is mutated. XSLT named-template invocation without a context item raises the caught XPDY0002 even when a source argument is supplied; an explicit global stylesheet parameter successfully supplies the typed node.

## Selected implementation direction

Use an opt-in exact numeric lane with explicit digit/scale quotas. Preserve existing ordinary-host behavior and signatures until a separately reviewed semantic implementation. Proposed additions to XdmValue are BigInteger creation/access and exact decimal creation/access; a proposed XdmExactDecimal represents coefficient times ten to the negative scale. Require nonnegative scale, numeric equality/hash normalization, bounded formatting, and checked legacy narrowing. Capacity refusal must remain distinguishable from invalid lexical or schema data. These are design selections, not compiled declarations or accepted public APIs.

Extend proposed SchemaQueryLimits with MaxNumericDigits and MaxDecimalScale; select no workload defaults yet. The existing private inline evidence marker occupies the integer reference slot. A large-integer payload must have distinct storage and preserve evidence without confusing it with that marker. Preserve the inline fast path; measure reference-backed payload and evidence costs. Audit parser, casts, arithmetic, comparison, map keys, formatting, serialization and database narrowing before claiming end-to-end exact execution. The accessor audit in INPUTS.json is a starting inventory, not proof of semantic coverage.

Place XQuery and XSLT session producers in their existing upper-layer assemblies, returning the shared API operation/session abstractions. Do not add downward dependencies from Core or Runtime to these hosts or expose arbitrary authority attachment. XQuery must freeze effective prolog/module context. XSLT must freeze compiler settings and package state, and explicitly separate initial match input, global context input, initial template/mode, output settings and dynamic context. Every source must belong to the operation's admitted snapshot set. Source-free named templates must remain possible. Streaming requires an explicit supported or refused profile rather than implicit materialization.

## Remaining work and acceptance

Next implement an isolated exact numeric representation/accessor prototype, with happy-path and range/scale refusal cases, equality/hash checks, 48-byte carrier layout measurements and allocation costs with and without evidence. Then compile the revised numeric and all-host declarations. This checkpoint does not contain that prototype or revised compiled declarations. Public host capture/binding, exact evaluation, typed transforms, multi-source lifetime tests and owner acceptance remain NotRun/Pending. C45 and API freeze remain pending.

The initial caught XSLT focus failure and the subsequent missing-xs-namespace harness observations are preserved as attempt receipts. The final harness adds the namespace explicitly; neither attempt changed engine code. Existing full unit/conformance qualification belongs to the earlier transport checkpoint and was not rerun for this diagnostic/documentation-only step.
