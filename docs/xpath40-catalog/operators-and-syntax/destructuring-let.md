<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: destructuring let">
  <br><br>
  <h1>Destructuring <code>let</code></h1>
  <p>Catalog entry — Operators &amp; syntax · Frozen 4.0</p>
</div>

<!-- Catalog entry: destructuring let. Template: see ../README.md §1.1. -->

## 1. Name and Classification

**Destructuring `let` bindings** — **Frozen 4.0** (ships at the `XPath40` level; static XPST0003 at `XPath31`). Spec: XPath 4.0 §4.14 (FLWOR expressions), [QT4CG PR #1131](https://github.com/qt4cg/qtspecs/pull/1131). Landed: REQ-123 destructuring-let cluster, [PR #129](https://github.com/Fytala-Charles/Bosak/pull/129) (main `b8e4b9f`, 2026-10-10).

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: let expressions](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-let-expressions). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

**Delivery provenance:** [PR #129](https://github.com/Fytala-Charles/Bosak/pull/129), [immutable mainline revision `b8e4b9f5`](https://github.com/Fytala-Charles/Bosak/commit/b8e4b9f510543c04d88a7c10ee2af28808a0fd5c) (2026-10-10). This identifies the engine delivery; later documentation examples do not redefine that delivery.

**Adopted specification revision:** the historical dossiers refer to live draft sections, but do not retain an immutable specification-source revision for this entry. That provenance gap remains explicit. Current section links checked on 2026-10-11 are navigation references, not proof of the adopted snapshot. See the [provenance ledger](../PROVENANCE.md).

## 2. Intent

Bind several variables at once by decomposing a sequence, an array, or a map on the right-hand side — `let $($x, $y) := (1, 2)` binds `$x` to `1` and `$y` to `2` in one clause, evaluating the right-hand side exactly once.

## 3. Also Known As

"Destructuring binding" (spec), "tuple binding" (informal, from the tuple-builder relative). Not to be confused with the `for` clause's *tuple* destructuring, which is a separate (larger) construct.

## 4. History / Supersedes

Supersedes three 3.1 idioms, one per container kind:

| Before (3.1) | After (4.0) |
|---|---|
| `let $x := $seq[1] let $y := $seq[2] return …` — repeated positional indexing, RHS re-evaluated per binding if not manually cached | `let $($x, $y) := $seq return …` — one evaluation, positional bind |
| `let $a := $arr(1) let $b := $arr(2) return …` | `let $[$a, $b] := $arr return …` |
| `let $n := map:get($m, "name") let $a := map:get($m, "addr") return …` | `let ${$name, $addr} := $m return …` — keys are the variable local names |

## 5. Motivation

The 3.1 idioms are error-prone in two ways: the RHS is visually repeated (or manually hoisted into a helper variable, cluttering scope), and positional indexing silently does the wrong thing when the sequence is shorter than expected. Destructuring makes the arity contract explicit and puts the shape of the data next to the names it feeds.

## 6. Language Alignment

Prior art: ML/Haskell pattern bindings, ES6 `const {a, b} = obj` / `const [x, y] = arr`.

**Semantic drift — read before relying on intuition:**

- The **last variable of a sequence pattern takes all remaining items** (`let $($x, $y) := (1,2,3)` → `$y = (2,3)`); ES6 array destructuring instead discards surplus elements. Surplus *variables* (too few items) bind `()`.
- The **array form discards surplus members but requires each bound index**: `let $[$a, $b] := [1,2,3]` discards member 3, but too few members for the variables is **FOAY0001**, not `undefined`-style filling.
- The **map form binds by variable name** (`${$name}` looks up key `"name"`), closer to ES6 object destructuring than to map lookup — but a missing key binds `()` rather than throwing.

## 7. Applicability

Use for: unpacking fixed-shape sequences/arrays/maps at the head of a FLWOR; replacing two-or-more positional `let`s over the same value.

Do **not** use for: library expressions that must still compile at `XPath31` (static XPST0003); one-item unpacks (`let $x := $seq[1]` remains clearer); hot loops where the save/restore cost (§14) matters and the 3.1 form was already hoisted.

## 8. Structure

```
LetBinding ::= SimpleLetBinding | DestructuringLetBinding
DestructuringLetBinding ::= "$" ("(" | "[" | "{") VariableSpec ("," VariableSpec)* (")" | "]" | "}")
                            TypeDeclaration? ":=" ExprSingle
VariableSpec ::= "$" VarName (TypeDeclaration)?        (* per-variable type, optional *)
```

Three bracket kinds select the source container: `(` sequence, `[` single array, `{` single map.

## 9. Participants

- `XPathParser.ParseDestructuringLetBinding` — parses all three kinds; gated on the parser's xpath40 flag; empty pattern → XPST0003.
- `XPathAstNode.LetDestructuringKind`, `DestructuringVariable` — AST model.
- Opcodes `Destructure`, `SaveVariables`, `RestoreVariables` (`IrOpCode` 1.12) — binding extraction and lexical scoping, lowered by `IrLowerer` (1.54).
- `VmEngine` (2.173/2.174) — `Destructure` semantics, `EnforceType` §3.4.2 coercion, `TryRelabelToDerivedNumeric` (PR 254 relabeling).
- `EvaluationContext.TryGetDirectVariable` (2.36) — direct-binding probe for `SaveVariables`.

## 10. Collaborations

- **Typed simple lets** (`let $v as xs:T := e`) — parsed at XP40+ as part of this slice; `EnforceType` applies XPath 4.0 §3.4.2 coercion when `IsXPath40` (untypedAtomic cast, numeric promotion, record coercion, derived-integer relabeling).
- **Structural record types** — a `map(K,V)`/`record(...)` whole-pattern type validates the RHS shape (record patterns reject undeclared variables).
- **Lexical scoping** — `SaveVariables`/`RestoreVariables` wrap every `LetExpression` binding group; a trailing reference to a destructured variable outside the let's body raises XPST0008. (Caveat: emitted for `LetExpression` bindings only; XQuery full-FLWOR scoping has its own machinery.)

## 11. Consequences

Benefits: single RHS evaluation; arity made explicit; map unpacks name their keys at the binding site.

Liabilities: three binding semantics to learn (sequence rest / array strictness / map-by-name) — the drift table in §6 is the cheat sheet. Relabeling changes the *annotation*, not the datum (`42` → `xs:short` ✓; `2.5` → `xs:integer` is XPTY0004, never truncation). Save/Restore pairs are exception-unsafe by design (abandoned evaluations discard the frame with the whole stack — intentional).

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers REQ123-destructuring. Sequence rest/short inputs, array member groups/surplus/short errors, map missing keys, per-variable/whole-pattern types, duplicates and lexical shadowing are mapped. Undeclared annotated-record fields incorrectly bind empty when destructured. K03 retains the required record-lookup XPTY0004 expectation. Plain-map missing-key emptiness remains a different supported contract. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


Files: `src/Bosak.XPath.Parser/Ast/XPathParser.cs` (1.74), `src/Bosak.XPath.Parser/Ast/XPathAstNode.cs` (1.25), `src/Bosak.XPath.Compiler/Ir/IrOpCode.cs` (1.12), `src/Bosak.XPath.Compiler/Ir/IrLowerer.cs` (1.54), `src/Bosak.XPath.Runtime/Vm/VmEngine.cs` (2.173/2.174), `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` (2.36). Version gate: XPST0003 at `XPath31`; fully legal at `XPath40`. Regression: `VersionGateTests` (0.20, +10 rows), `ParserTests` (0.11, +6 AST tests).

## 13. Sample Code

Additional executable catalog cases unpack invoice metadata and explicitly test
a missing currency key: [main](../examples/destructuring-let/main.xpath) produces
`INV-1042 / EUR`; [edge](../examples/destructuring-let/edge.xpath) returns missing
currency cardinality `0`. These run at `XPath40` and verify default-mode refusal.

Start with [the invoice chapter's practical migration pair](../worked-examples/invoice-to-erp/README.md#where-xpath-40-can-improve-readability): unpack invoice ID and currency from a metadata map to build a display label. Both complete expressions are supplied, engine-verified, and produce `INV-2026-1042 / EUR`. The 4.0 form requires explicit opt-in. The probes below cover the individual binding shapes.

~~~xquery
let $($x, $y) := (1, 2, 3) return ($x, $y)
  (: → (1, (2,3)) — last variable takes the rest :)

let $[$a, $b] := [3, 4, 5] return `a={$a} b={$b}`
  (: → "a=3 b=4" — surplus member 5 discarded :)

let $[$a, $b] := [1] return $b
  (: → FOAY0001 :)

let ${$name} := map {"name": "Braid"} return $name
  (: → "Braid" — key = variable local name :)

let $($x as xs:short, $y) := (42, "ok") return ($x, $y)
  (: → (42 xs:short, "ok") — PR 254 relabeling; annotation changes, datum untouched :)
~~~

## 14. Performance Notes

Cost class: **small constant overhead**. One `Destructure` opcode per binding group plus one `SaveVariables`/`RestoreVariables` frame pair — no per-item re-evaluation (the 3.1 un-hoisted idiom this replaces was potentially *more* expensive). No hot-path interaction known. No BenchmarkDotNet measurement exists for this feature as of 2026-10-10; the cost class is an implementation expectation, not a measurement.

## 15. Known Uses

Practical case: `InvoiceReferenceGuideTests.InvoiceLabel_MigrationPairMatchesAndRequiresOptIn` executes both published label expressions and verifies XPST0003 in the default mode. This is a chapter-level example, separate from qt4tests gate evidence.

qt4tests `prod-LetClause` **137/0/52** (52 skips = XQuery-only dependencies) and `fn-compare` collateral — both promoted into the conformance gate (223 sets) at landing.

**Named implementation evidence:** [`Compile_DestructuringLet_DefaultOptions_ThrowXpst0003`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_DestructuringSequence_FrozenXPath40_Works`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs), [`Evaluate_DestructuringSequence_CoercionFailure_ThrowsXpty0004`](../../../tests/Bosak.XPath.Api.Tests/VersionGateTests.cs). No feature-specific qt4 set assignment recorded here; unit evidence is listed explicitly. This is not a claim of absent upstream tests. These references describe existing evidence; this documentation update does not rerun or promote the full conformance suite.

## 16. Related Features

- Typed simple lets and §3.4.2 coercion (collaboration, §10) — own entry pending (Type system chapter).
- Structural record types — own entry pending.
- `fn:scan` — [experimental/fn-scan.md](../experimental/fn-scan.md) (same delivery wave, opposite tier).

*Entry created 2026-10-10; updated 2026-10-11 with the verified invoice migration case (REQ-126).*
