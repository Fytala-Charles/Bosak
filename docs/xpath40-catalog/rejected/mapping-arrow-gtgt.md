<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog entry: mapping arrow =&gt;&gt;">
  <br><br>
  <h1>Mapping Arrow <code>=&gt;&gt;</code> (Original Spelling)</h1>
  <p>Catalog entry — Rejected / Superseded</p>
</div>

<!-- Catalog entry: mapping arrow =>>. Template: see ../README.md §1.1. -->

## 1. Name and Classification

**Mapping arrow, original spelling `=>>`** — **Rejected–Superseded**. Bosak never shipped this spelling; the engine implemented the renamed form directly. Disposition recorded: REQ-118 dossier §1.2 "Deliberately rejected / renamed (do not implement old shapes)".

**External specification references (current drafts; links checked 2026-10-11):** [XPath 4.0: accepted mapping-arrow syntax](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-mapping-arrow-expression). See the [catalog source policy](../README.md#external-specifications-and-source-documentation) for adopted-revision provenance.

## 2. Intent

(Of the accepted feature this spelling belonged to.) Apply the function once per
item, passing that item as its first argument: `$seq =!> f()` corresponds to
`for $x in $seq return f($x)`. The ordinary arrow `$seq => f()` passes the whole
sequence as one argument. The simple map operator `!` instead establishes each
item as the context for its right-hand expression; argument passing and context
binding are distinct mechanisms.

## 3. Also Known As

`=!>` — the accepted name (mapping arrow). Formerly spelled `=>>` in earlier drafts; renamed during draft stabilization. qt4tests pin the accepted spelling.

## 4. History / Supersedes

The rejected spelling and its replacement are recorded in the REQ-118 audit;
Bosak implements the accepted form in its S4 grammar slice. A 3.1 baseline uses
`for $x in $seq return f($x)`. See the
[accepted mapping-arrow entry](../operators-and-syntax/mapping-arrow.md) for the
verified migration pair. The current specification link documents the accepted
syntax; an exact historical rename revision remains part of the provenance work.

## 5. Motivation

Recorded for completeness: the superseded spelling exists in draft-era examples, conference slides, and early blog posts. A user pasting such material gets a parse error and deserves a searchable answer.

## 6. Language Alignment

The accepted operator performs per-item function application, comparable in
purpose to mapping a collection. XPath passes each item as the first function
argument; it does not establish that item as the right-hand expression's context
in the way `!` does. The historical reason for choosing the replacement spelling
needs its own source; no visual resemblance to other operators establishes it.

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
string-join((' FILTER-A ', ' BOLT-M8 ') =!> normalize-space(), ',')
  (: → "FILTER-A,BOLT-M8" — one call per product code :)

(' FILTER-A ') =>> normalize-space()
  (: → XPST0003 at every level — old spelling, never implemented :)
~~~

## 14. Performance Notes

Not applicable.

## 15. Known Uses

`rejected-mapping-arrow-main` and `rejected-mapping-arrow-edge` in
`CatalogExampleTests` verify an accepted product-code normalization and rejection
of the old spelling. See [the accepted mapping-arrow case](../operators-and-syntax/mapping-arrow.md)
and [complete rejection file](../examples/rejected-mapping-arrow/edge.xpath).

qt4tests pin only the accepted `=!>` spelling (part of the frozen gate's grammar slices). No test uses `=>>`.

## 16. Related Features

- [Pipeline operator](../operators-and-syntax/pipeline-operator.md) — whole-value function application, distinct from per-item mapping.
- [method-call-operator.md](../operators-and-syntax/method-call-operator.md) — the third `=?>` member of the family, deferred for semantic (not naming) churn.
- [fn-scan.md](../experimental/fn-scan.md) — the "consolidated before stabilization" sibling class (`scan-left`/`scan-right` → `fn:scan`); the same rule applies: old shapes do not parse.

*Entry created 2026-10-10 (REQ-126 scaffold).*
