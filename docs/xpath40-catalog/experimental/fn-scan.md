<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: fn:scan">
  <br><br>
  <h1><code>fn:scan</code></h1>
  <p>Catalog entry — Functions · Experimental</p>
</div>

<!-- Catalog entry: fn:scan. Template: see ../README.md §1.1. -->

## 1. Name and Classification

**`fn:scan`** — **Experimental** (ships only at the `XPath40Experimental` level; static XPST0017 at both `XPath31` *and* frozen `XPath40`). Spec: F&O 4.0 §2.5.15, added by qt4cg Issue 2823 / PR 2824 (31 July 2026). Landed: REQ-123 S1 (1.1.0 track), level-plumbing slice 2026-10-09.

## 2. Intent

Return every intermediate accumulator of a left fold as a **sequence of single-member arrays**: `fn:scan($input, $init, $action)` yields `[$init]`, then `[$action($acc, $item, $pos)]` for each input item — N input items produce N+1 arrays. There is no additional outer array.

## 3. Also Known As

"Prefix scan", "running fold". **Do not implement** `scan-left` / `scan-right`: those names never existed in the accepted draft — the pair was consolidated into the single left-fold `fn:scan` before stabilization (see [mapping-arrow-gtgt.md](../rejected/mapping-arrow-gtgt.md) for the sibling rename story).

## 4. History / Supersedes

Supersedes hand-threaded accumulation of intermediate states in 3.1. The
[complete stock-ledger baseline](../examples/fn-scan/before.xpath) uses
`fold-left` and `array:append`; the [experimental case](../examples/fn-scan/main.xpath)
uses `fn:scan`. Both display `100,120,85,95` for an opening stock of 100 units
and movements of +20, -35, and +10.

| Before (3.1) | After (4.0-Exp) |
|---|---|
| `fold-left($movements, [100], function($states, $delta) { array:append($states, $states(array:size($states)) + $delta) })` | `fn:scan($movements, 100, function($balance, $delta) { $balance + $delta })` |

## 5. Motivation

Fold returns its final accumulator; stock ledgers and running totals need every
intermediate state. A 3.1 fold must explicitly retain that history. Scan expresses
the history requirement directly and includes the initial state for comparison.

## 6. Language Alignment

Direct prior art: Haskell `scanl`, Clojure `reductions`, F# `Seq.scan`, Kotlin `runningFold`. Drift warning: XPath returns a **sequence of single-member arrays**, allowing each accumulator to hold a sequence without flattening it into the other states. Arity-2 callbacks receive accumulator and item only; the runtime can supply position to an arity-3 callback.

## 7. Applicability

Use for: running aggregations, incremental derivations, anything where each input item emits a new accumulator state.

Do **not** use for: anything that must compile at frozen `XPath40` or 3.1 (static XPST0017); final-state-only folds (`fn:fold-left` is clearer and one array shorter); unstable-surface intolerance — this function postdates the June-2026 churn line and its exact shape may still move.

## 8. Structure

```
fn:scan(input as item()*, init as item()*, action as fn(*)) as array(*)*
```

## 9. Participants

- `FunctionLibrary` — experimental-only signature template (`IsXPath40ExperimentalOnly`), invisible to `fn:function-lookup` / dynamic dispatch below the experimental level.
- `EvaluationContext.IsXPath40Experimental` — runtime level flag.
- Version gate (`XPath31Expression`) — static XPST0017 in call form *and* named-function-ref form at both lower levels.

## 10. Collaborations

- The **experimental level plumbing** (REQ-123 S1) is the collaboration that matters: `fn:scan` was the first function gated exclusively to `XPath40Experimental`, and it established the template every later experimental addition follows (dedicated signature templates, lookup invisibility, call+ref static errors).
- Destructuring `let` ([destructuring-let.md](../operators-and-syntax/destructuring-let.md)) is the natural consumer of each emitted single-member array's shape in surrounding code.

## 11. Consequences

Benefits: direct expression of all intermediate states; the callback position
parameter enables position-aware accumulations. Retaining the history requires
storage for its states; no constant-memory guarantee is implied.

Liabilities: **experimental-tier semantics may change as the draft moves** — the N+1 / single-member-array shape is precisely the kind of detail a WG review adjusts. Pin your usage to a Bosak version and re-run the qt4tests `fn-scan` set (if promoted by then) on upgrades.

## 12. Implementation

Files: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (experimental signature + implementation), `src/Bosak.XPath.Api/XPath31Expression.cs` (experimental-aware static gate), `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` (`IsXPath40Experimental`). Version gate: XPST0017 at `XPath31` **and** `XPath40`; legal only at `XPath40Experimental`. Unit evidence: experimental-level tests in `Bosak.XPath.Standard.Tests` (see REQ-123 S1).

## 13. Sample Code

Practical case: [stock ledger](../examples/fn-scan/main.xpath). The
[empty-movement case](../examples/fn-scan/edge.xpath) returns only the opening
balance, displayed as `100`. Both cases require `XPath40Experimental`; the
[earlier baseline](../examples/fn-scan/before.xpath) runs at `XPath31`.

~~~xquery
fn:scan((1, 2, 3), 0, fn($acc, $it) { $acc + $it })
  (: → [0], [1], [3], [6] — every running total, init first :)

fn:scan((), "start", fn($acc, $it) { $acc || $it })
  (: → ["start"] — empty input yields just the init :)
~~~

## 14. Performance Notes

Implementation expectation: one callback per input item and one wrapper array
per retained state. Accumulator size can dominate storage and callback cost.
No measured comparison against the 3.1 array-append baseline is claimed.

## 15. Known Uses

Published examples: `fn-scan-main`, `fn-scan-edge`, `fn-scan-before` in
`CatalogExampleTests`. The successful experimental cases also verify refusal at
both lower compatibility levels. This is example evidence, separate from gate status.

qt4tests set `fn-scan` is promoted into `GatedSets` in
[ConformanceRunner40](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).
The 1.1.0 gate run records 11 experimental-level passes and zero gated failures.
Gate membership does not move the function into the frozen compatibility tier;
its successful examples still require `XPath40Experimental`.

## 16. Related Features

- [mapping-arrow-gtgt.md](../rejected/mapping-arrow-gtgt.md) — the sibling "consolidated/renamed before stabilization" story.
- [destructuring-let.md](../operators-and-syntax/destructuring-let.md) — frozen-tier contrast from the same delivery wave.
- [method-call-operator.md](../operators-and-syntax/method-call-operator.md) — another post-June-2026 deferred surface.

*Entry created 2026-10-10 (REQ-126 scaffold).*
