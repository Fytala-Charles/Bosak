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

Return every intermediate accumulator of a left fold as an array of single-member arrays: `fn:scan($input, $init, $action)` yields `[$init]`, then `[$action($acc, $item, $pos)]` for each input item — N input items produce N+1 arrays.

## 3. Also Known As

"Prefix scan", "running fold". **Do not implement** `scan-left` / `scan-right`: those names never existed in the accepted draft — the pair was consolidated into the single left-fold `fn:scan` before stabilization (see [mapping-arrow-gtgt.md](../rejected/mapping-arrow-gtgt.md) for the sibling rename story).

## 4. History / Supersedes

Supersedes hand-threaded fold state in 3.1:

| Before (3.1) | After (4.0-Exp) |
|---|---|
| `let $steps := fold-left($input, [$init], fn($acc, $it) { $acc, $it })` — building prefixes by appending to an accumulating sequence (O(n²) copying) | `fn:scan($input, $init, fn($acc, $it, $pos) { … })` |

## 5. Motivation

Fold gives you only the final accumulator; many pipelines (running totals, incremental averages, event-sourced projections) need *every* intermediate state. The 3.1 workaround materializes prefixes manually, which is both quadratic in effort-to-read and actually quadratic in sequence-copying cost.

## 6. Language Alignment

Direct prior art: Haskell `scanl`, Clojure `reductions`, F# `Seq.scan`, Kotlin `runningFold`. Drift warning: those return *flat* prefix lists; XPath returns an **array of single-member arrays**, because XDM sequences are flat and cannot nest — the array wrapper is the nesting vehicle. Arity-2 callbacks are legal per F&O §1.8 truncation (`fn($acc, $it) { … }` receives accumulator and item only).

## 7. Applicability

Use for: running aggregations, incremental derivations, anything where each input item emits a new accumulator state.

Do **not** use for: anything that must compile at frozen `XPath40` or 3.1 (static XPST0017); final-state-only folds (`fn:fold-left` is clearer and one array shorter); unstable-surface intolerance — this function postdates the June-2026 churn line and its exact shape may still move.

## 8. Structure

```
fn:scan(input as item()*, init as item()*, action as fn(*)) as array(*)
```

## 9. Participants

- `FunctionLibrary` — experimental-only signature template (`IsXPath40ExperimentalOnly`), invisible to `fn:function-lookup` / dynamic dispatch below the experimental level.
- `EvaluationContext.IsXPath40Experimental` — runtime level flag.
- Version gate (`XPath31Expression`) — static XPST0017 in call form *and* named-function-ref form at both lower levels.

## 10. Collaborations

- The **experimental level plumbing** (REQ-123 S1) is the collaboration that matters: `fn:scan` was the first function gated exclusively to `XPath40Experimental`, and it established the template every later experimental addition follows (dedicated signature templates, lookup invisibility, call+ref static errors).
- Destructuring `let` ([destructuring-let.md](../operators-and-syntax/destructuring-let.md)) is the natural consumer of each emitted single-member array's shape in surrounding code.

## 11. Consequences

Benefits: O(n) prefixes where 3.1 was O(n²); callback position parameter enables position-aware accumulations.

Liabilities: **experimental-tier semantics may change as the draft moves** — the N+1 / single-member-array shape is precisely the kind of detail a WG review adjusts. Pin your usage to a Bosak version and re-run the qt4tests `fn-scan` set (if promoted by then) on upgrades.

## 12. Implementation

Files: `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (experimental signature + implementation), `src/Bosak.XPath.Api/XPath31Expression.cs` (experimental-aware static gate), `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` (`IsXPath40Experimental`). Version gate: XPST0017 at `XPath31` **and** `XPath40`; legal only at `XPath40Experimental`. Unit evidence: experimental-level tests in `Bosak.XPath.Standard.Tests` (see REQ-123 S1).

## 13. Sample Code

~~~xquery
fn:scan((1, 2, 3), 0, fn($acc, $it) { $acc + $it })
  (: → [0], [1], [3], [6] — every running total, init first :)

fn:scan((), "start", fn($acc, $it) { $acc || $it })
  (: → ["start"] — empty input yields just the init :)
~~~

## 14. Performance Notes

Cost class: **small constant overhead** per item (one callback dispatch + one array allocation per step — the array-per-step shape is inherent to the spec, not an implementation choice). The O(n²) sequence-copying it replaces in 3.1 idioms is the actual win. No BenchmarkDotNet measurement exists as of 2026-10-10.

## 15. Known Uses

qt4tests set `fn-scan` (when promoted into the gate, this entry's Known Uses must be updated in the same PR — structural rule 3). Unit-level evidence only at landing.

## 16. Related Features

- [mapping-arrow-gtgt.md](../rejected/mapping-arrow-gtgt.md) — the sibling "consolidated/renamed before stabilization" story.
- [destructuring-let.md](../operators-and-syntax/destructuring-let.md) — frozen-tier contrast from the same delivery wave.
- [method-call-operator.md](method-call-operator.md) — another post-June-2026 deferred surface.

*Entry created 2026-10-10 (REQ-126 scaffold).*
