<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: method-call operator">
  <br><br>
  <h1>Method-Call Operator <code>=?&gt;</code></h1>
  <p>Catalog entry — Operators &amp; syntax · Deferred</p>
</div>

**C07 lifecycle review — 26 October 2026:** [Current disposition and correction receipts](../LIFECYCLE-RECONCILIATION.md) distinguish delivered scope, retained history and pending acceptance. Six closure workpackages remain; no feature or profile promotion.

<!-- Catalog entry: method-call operator. Template: see ../README.md §1.1. -->

## 1. Name and Classification

**Method-call operator `=?>`** — **Deferred** (deliberately not implemented). Spec: XPath 4.0 §4.22.3 (operator summary), [QT4CG issue #2219](https://github.com/qt4cg/qtspecs/issues/2219). Deferral recorded: REQ-118 dossier §1.2 "Still moving — do NOT build yet" (semantics changed 25 June 2026).

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: methods and method calls](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-methods). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #77](https://github.com/Fytala-Charles/Bosak/pull/77), [immutable mainline revision `27bc663b`](https://github.com/Fytala-Charles/Bosak/commit/27bc663b71b948318f69ee1f23173ac63046a525) (2026-10-08). This is the adoption/disposition dossier delivery, not implementation of the refused shape.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Object-style method invocation on a value: the right-hand name is resolved against a method dictionary for the *type* of the left-hand value, so `$value =?> method(args)` dispatches to the method registered for that type rather than a plain in-scope function.

## 3. Also Known As

"Method lookup operator". (No renames on record — the issue is semantic churn, not naming.)

## 4. History / Supersedes

None — new capability; there is no 3.x idiom it replaces. The nearest 3.1/4.0 workaround it would relieve:

| Before | After (if landed) |
|---|---|
| `map:get($m, "key")`, `array:get($a, 1)` — per-type function names a user must know | `$m =?> get("key")` — one call syntax, type-driven dispatch |

## 5. Motivation

Type-directed dispatch would let maps, arrays, and future extensible types share call syntax, easing the "which module owns this operation?" burden. It is also the gateway shape for any future extensible-type story.

## 6. Language Alignment

Prior art: Smalltalk/Objective-C message send, C# extension-method invocation syntax, Kotlin receiver types. Drift watch: XPath would dispatch on the **XDM type + registered method dictionary**, not on object identity or inheritance — and the *exact* resolution rules (what happens for unions, for `item()`, for user-defined types) are the part the WG changed in June 2026.

## 7. Applicability

Not applicable — Bosak does not implement this operator. This entry exists so the deferral is a recorded decision rather than an absence.

## 8. Structure

Not recorded — the grammar shape is among the things the draft may still adjust. Re-derive from the current WG Review Draft before any implementation attempt.

## 9. Participants

None in Bosak. Implementation seam when the time comes: parser operator table, `ApplyBinaryOperator` dispatch in `VmEngine`, and interaction with the type-matching machinery used by `instance of` / `typeswitch`.

## 10. Collaborations

Any future design would interact with delivered choice types, existing function lookup and bounded XSLT-defined nominative records. Their delivery does not implement method dispatch; full nominal declaration/model contracts remain separately limited.

## 11. Consequences

Deferral leaves the operator unavailable. Consumers can use the supplied direct `map:*`/`array:*` alternatives; refusal and alternative checks remain necessary. Their passing results do not implement method dispatch or certify every possible replacement.

## 12. Implementation

No implementation is adopted. The C07 probes verify `XPST0003` in XPath30, XPath31, XPath40 and XPath40Experimental. Any future implementation or tier decision requires a separately scoped review; this disposition makes no promotion commitment.

## 13. Sample Code

No method-call syntax is committed. Do not cargo-cult `$x =?> f()` from
draft-era blog posts. Use a verified stable alternative for a packing-plan array:
[main.xpath](../examples/method-call-alternative/main.xpath) evaluates
`array:size(['FILTER-A', 'BOLT-M8'])` to `2`; the
[empty-array case](../examples/method-call-alternative/edge.xpath) returns `0`.
Both run at `XPath31` and claim no support for `=?>`.

## 14. Performance Notes

Not applicable (not implemented). Design note for the future: type-dictionary dispatch would add a per-call dictionary probe on the hot path — measure against the direct-function-call baseline in `benchmarks/Bosak.Benchmarks` before promoting out of experimental.

## 15. Known Uses

No implementation evidence for the deferred operator is claimed. Published
stable alternatives are `method-call-alternative-main` and
`method-call-alternative-edge` in `CatalogExampleTests`. These checks do not
promote or implement method-call syntax.

**Named implementation evidence:** [`Compile_MappingArrow_31Mode_ThrowsXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_MappingArrow_AppliesFunctionPerItem`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_MappingArrow_WithArgumentsPrependsContextItem`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). Decision record, not an engine delivery or a qt4 set promotion. Tests below pin the supported/refused alternatives. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

- [mapping-arrow-gtgt.md](../rejected/mapping-arrow-gtgt.md) — the `=`-family rename history (`=>>` → `=!>`); if `=?>` ever ships, its name is the one stable thing about it so far.
- `fn:scan` ([experimental/fn-scan.md](../experimental/fn-scan.md)) — an example of a separately adopted Experimental feature. Its adoption does not supersede the recorded method-call deferral or determine a future implementation tier.

*Entry created 2026-10-10 (REQ-126 scaffold). Re-validate against the WG Review Draft before implementing.*
