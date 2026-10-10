<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: mapping arrow =&gt;&gt;">
  <br><br>
  <h1>Mapping Arrow <code>=&gt;&gt;</code> (Original Spelling)</h1>
  <p>Catalog entry — Rejected / Superseded</p>
</div>

<!-- Catalog entry: mapping arrow =>>. Template: see ../README.md §1.1. -->

## 1. Name and Classification

**Mapping arrow, original spelling `=>>`** — **Rejected–Superseded**. Bosak never shipped this spelling; the engine implemented the renamed form directly. Disposition recorded: REQ-118 dossier §1.2 "Deliberately rejected / renamed (do not implement old shapes)".

## 2. Intent

(Of the accepted feature this spelling belonged to.) Apply a function to a **whole sequence** as one call: `$seq =!> f()` ≡ `f($seq)`. Contrast the 3.1 simple map operator `!`, which applies per item: `$seq ! f()` ≡ `for $x in $seq return f($x)`.

## 3. Also Known As

`=!>` — the accepted name (mapping arrow). Formerly spelled `=>>` in earlier drafts; renamed during draft stabilization. qt4tests pin the accepted spelling.

## 4. History / Supersedes

The rename is the entire history: the operator entered the drafts as `=>>`, was renamed to `=!>` before the WG Review Draft threshold, and Bosak implemented only the final form (REQ-118 4.0-S4 grammar slice). No 1.x–3.x syntax is involved — both spellings are 4.x-native; what the feature itself supersedes is the per-item-only story of `!` (see the accepted feature's own entry when the Operators chapter is completed).

## 5. Motivation

Recorded for completeness: the superseded spelling exists in draft-era examples, conference slides, and early blog posts. A user pasting such material gets a parse error and deserves a searchable answer.

## 6. Language Alignment

`=!>` has no direct prior-art relative; the nearest shapes are F#'s `|>` (pipeline, *different* feature — see `->`) and the rejected `=>>` sat visually halfway between pipeline and bit-shift operators in C-family languages, which was part of the rename motivation (ambiguity and typo-sensitivity in monospaced draft text).

## 7. Applicability

**Do not use.** `=>>` raises XPST0003 at every Bosak compatibility level, including frozen `XPath40`. Use `=!>`.

## 8. Structure

Not applicable — the spelling is not in the grammar. Accepted form: `Expr "=!>" ArrowFunction`.

## 9. Participants

None. The parser's operator table has no `=>>` token; drafts-era sources parse-fail positionally at the `>`.

## 10. Collaborations

None.

## 11. Consequences

Of the rejection: zero engine surface for the old spelling, one parse-error class for pasted old material, and no version-gate complexity (there is no "compat level that accepts `=>>`").

## 12. Implementation

None, deliberately. If a parser change ever accidentally accepts `=>>`, that is a defect against this entry.

## 13. Sample Code

~~~xquery
(1, 2, 3) =!>
fn:count#1
  (: → 3 — whole sequence, one call :)

(1, 2, 3) =>>
fn:count#1
  (: → XPST0003 at every level — old spelling, never implemented :)
~~~

## 14. Performance Notes

Not applicable.

## 15. Known Uses

qt4tests pin only the accepted `=!>` spelling (part of the frozen gate's grammar slices). No test uses `=>>`.

## 16. Related Features

- `->` pipeline operator — the `=`-family sibling; own entry pending (Operators chapter).
- [method-call-operator.md](../operators-and-syntax/method-call-operator.md) — the third `=?>` member of the family, deferred for semantic (not naming) churn.
- [fn-scan.md](../experimental/fn-scan.md) — the "consolidated before stabilization" sibling class (`scan-left`/`scan-right` → `fn:scan`); the same rule applies: old shapes do not parse.

*Entry created 2026-10-10 (REQ-126 scaffold).*
