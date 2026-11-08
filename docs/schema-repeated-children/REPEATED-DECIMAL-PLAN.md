# Bounded repeated decimal implementation plan — 29 October 2026

This is a future slice, not implemented repetition support. Start from the reviewed optional checkpoint and the [exact adoption record](REPEATED-DECIMAL-ADOPTION-2026-10-29.md). Current Schema 0.6/core 1.34.1 and all candidate/product pins remain unchanged during this scoping step.

## Intended behavior

For `amount` minimum one/maximum two, followed by required `tax`, `<order><amount>2</amount><amount>3</amount><tax>1</tax></order>` should structurally match. Both amount states are distinct and reference the same original amount declaration; tax retains its own later declaration. Copies preserve both values and their order. All structure is checked before lexical/exact-value validation and assertions.

`amount eq 2` is a singleton value comparison: two amount nodes cause a cardinality error and cannot produce a valid assessment. An assertion such as `tax gt 0` or `1 eq 1` can validate a repeated structure without widening the grammar. Functions, general comparisons and arbitrary aggregation are deliberately deferred to a separate adoption. Do not rewrite the original assertion to make it pass.

## Implementation decisions

1. **Compile bounded particles.** Replace the internal optional Boolean with minimum/maximum values while retaining original declaration references. Accept minimum zero/one and finite maximum 1–16, then refuse schemas whose sum of maxima exceeds sixteen. Parse bounded unsigned lexical forms without overflow; malformed/out-of-coverage values are Unsupported under the existing finite compiler policy. Keep duplicate-name rejection and one enclosing sequence. Choose v3 only for maximum greater than one; existing v1/v2 behavior and refusals remain.
2. **Match contiguous occurrences.** Walk particles and actual child elements in separate loops. Consume only the current particle's name, no more than its maximum. Enforce minimum; then advance the particle. Unconsumed instances, too many copies, wrong order and missing required neighbors are Invalid. Unique declaration names avoid ambiguous greedy attribution and require no backtracking. Observe cancellation while consuming repetitions and skipping optional particles.
3. **Construct one state per instance.** Keep a declaration mapping for every consumed element. Create a distinct AssessedElement per actual occurrence, even if lexical values are equal. Reuse its original declaration/type objects. No dictionaries or Single calls that conflate a declaration with one instance. Keep root, state and XML ordering aligned; verify later particles after repetitions and omissions.
4. **Handle supported assertion cardinality errors narrowly.** Current engine comparison code emits InvalidOperationException with an anchored `XPTY0004:` code for multiple operands. In the closed decimal/name comparison grammar, that known type error is an assertion failure: Invalid with AssertionViolated, no assessed document. Prefer a structured XPathErrorException code when present; otherwise qualify a strict exception-type plus leading-code check for this actual engine contract. No substring search or catch-all conversion of arbitrary CLR errors to Invalid. Cancellation/out-of-memory propagate; unrelated unexpected engine/coverage failures retain explicit refusal. Respect the engine's actual logical evaluation/short-circuit rules; do not scan an entire assertion and reject unexecuted branches preemptively. Capture exception type/code diagnostics before changing this policy.
5. **Refuse ambiguous singular exports.** Count present matches before projecting. Existing exactly-one projections retain behavior; absent or multi-match requests refuse BSK-XSD-016. Repeated whole-document export carries each state by actual instance position. Native copies and XQuery/XSLT preserve/strip must retain order, exact values, distinct nodes and shared original declaration authority without reassessment. No occurrence-index API is required in this slice.
6. **Version and document.** Implement the Schema 0.7 development line/profile v3 only in that later step. Core remains 1.34.1 unless evidence requires a separately adopted core correction. Update changed C# histories/XML documentation, owning guide, canonical status and original-state semantics. Preserve all previous raw receipts and sealed dossiers. No consumer dependency upgrade, gate/fixture/admission change, package, tag or release.

## Acceptance matrix

| Area | Required controls |
|---|---|
| Compilation | Default/existing v1/v2; maxima 2/16 with leading zeros/XML whitespace; minimum zero/one; signed/unbounded/0/17 maximum refusal; minimum two refusal; total budget exactly 16/over 16; duplicate names and optional enclosing sequence refusal. |
| Matching | Zero/one/exact maximum; missing required first occurrence; too many repeats; repeated first/middle/last particles; adjacent distinct repeated particles; optional gaps; later required omission; interleaved/out-of-order/unknown nodes. |
| Values/state | Each lexical/representation failure (including a later occurrence); two equal values still distinct states/nodes; all occurrences share original declaration; later particle identity; independent snapshots and copy identities. |
| Assertions | Zero/single/multiple operand cardinality for eq/ne/order operators; multiple operands on either side; known XPTY0004 yields assertion Invalid; unrelated failures not misclassified; and/or evaluation controls; singleton legacy regression cohort; no new functions/general comparison admission. |
| Projections/copies | Singular absent/one/multiple behavior; occurrence-specific host query; exact decimal sequence ordering; whole-document original references; XQuery/XSLT preserve and strip with repeated and omitted neighbors; mutation refusal; source unchanged; empty-root FOTY0012 unaffected. |
| Bounds/lifetime | Aggregate budget, instance/node/schema/lexical limits, cancellation through repeated matching, escaped owner lifetime and original suite regression. |

Record before/after behavior using the actual assessor and real engine, not only a synthetic matcher. Keep intentional failures and setup errors separate from final accepted receipts. The matrix specifies behaviors; it is not a promised test count.

## Qualification and handover

The current scratch controls use the actual optional baseline to show repetition-schema refusal and the real XPath engine on typed zero/one/two-child inputs. They are diagnostic observations, not repeated-schema acceptance. Baseline full results are 260 Schema and 24,214 core passes, with unchanged QT3 31,397/0/424 and qt4 8,941/0/1,918 (230 sets).

For a Schema-only implementation, rerun the entire Schema solution, repeated-document example, real copy/error-policy controls and strict docs checks, and verify source-identical core/harness/fixture identities when inheriting prior full gates. If core production changes, run the complete core solution and both exact conformance gates with skip comparison under AGENTS. The owning worktree remains `feature/bounded-document-assessment`; original public main checkout and product pins stay untouched.

This planning step introduces no production source changes, API freeze, merge or release. The user authorized checkpointing/pushing these review/scope recommendations; delivery records identify their separate commits. Implement the bounded repeated slice only as the next implementation step.
