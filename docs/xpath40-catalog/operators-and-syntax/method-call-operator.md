<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: method-call operator">
  <br><br>
  <h1>Method-Call Operator <code>=?&gt;</code></h1>
  <p>Catalog entry — Operators &amp; syntax · Deferred</p>
</div>

<!-- Catalog entry: method-call operator. Template: see ../README.md §1.1. -->

## 1. Name and Classification

**Method-call operator `=?>`** — **Deferred** (deliberately not implemented). Spec: XPath 4.0 §4.22.3 (operator summary), qt4cg Issue 2219. Deferral recorded: REQ-118 dossier §1.2 "Still moving — do NOT build yet" (semantics changed 25 June 2026).

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

Would interact with: choice item types and union matching (Deferred entries elsewhere), the function-lookup machinery (method dictionaries would need `fn:function-lookup` visibility decisions), and nominative record types (also deferred).

## 11. Consequences

Deferral consequences: zero engine surface, zero test debt, zero risk to the frozen gate. Cost: consumers must keep using per-type functions (`map:*`, `array:*`), which are stable and gated green today.

## 12. Implementation

None. Gate posture if attempted later: it would land at `XPath40Experimental` first (post-churn-line feature), with static XPST0003 at 3.1, until the WG Review Draft stabilizes its semantics.

## 13. Sample Code

None — no syntax is committed. Do not cargo-cult `$x =?> f()` from draft-era blog posts; it does not parse in any Bosak compatibility level.

## 14. Performance Notes

Not applicable (not implemented). Design note for the future: type-dictionary dispatch would add a per-call dictionary probe on the hot path — measure against the direct-function-call baseline in `benchmarks/Bosak.Benchmarks` before promoting out of experimental.

## 15. Known Uses

None. qt4tests has no gated dependence on this operator.

## 16. Related Features

- [mapping-arrow-gtgt.md](../rejected/mapping-arrow-gtgt.md) — the `=`-family rename history (`=>>` → `=!>`); if `=?>` ever ships, its name is the one stable thing about it so far.
- `fn:scan` ([experimental/fn-scan.md](../experimental/fn-scan.md)) — an example of a post-June-2026 feature that *did* ship, behind the experimental level; `=?>` differs only in that its semantics, not its age, are the churn source.

*Entry created 2026-10-10 (REQ-126 scaffold). Re-validate against the WG Review Draft before implementing.*
