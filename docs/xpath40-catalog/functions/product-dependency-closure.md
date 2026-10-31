<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: product component dependency traversal">
  <br><br><h1>Product Dependency Closure</h1>
  <p>Frozen 4.0 — reachable components, shared dependencies and cycles</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Limited verified frozen contract**. Only XML nodes in one document are covered; no JNode/cross-document ordering, topological installation order or quantity explosion. Review completion is not final owner acceptance; earlier checkpoints remain historical.

## 1. Name and Classification

**Product dependency closure — Frozen 4.0.** [fn:transitive-closure](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-transitive-closure).
Initial engine delivery: [PR #88](https://github.com/Fytala-Charles/Bosak/pull/88),
[revision 0a928bb8](https://github.com/Fytala-Charles/Bosak/commit/0a928bb81b81d0631cc681f86bba83259c0ed790), REQ-118 4.0-S5.
Definitions reviewed **2026-10-11** against immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.

## 2. Intent

Find every component reachable from a product kit through its component references.
Return each component node once, including shared dependencies reached through
multiple paths, and terminate even when references form a cycle.

## 3. Also Known As

Reachability and dependency closure are descriptive terms. No alias is registered.
This is a set of reachable nodes, not a build-order or shortest-path algorithm.

## 4. History / Supersedes

XPath 3.1 can accumulate node unions through a bounded `fold-left`. The complete
comparison below uses at most the number of catalog parts as expansion rounds,
which suffices for this finite, stable-node graph. Unlike transitive-closure it
still visits the fixed range after reaching a stable set. A recursive helper
in a host language supporting function declarations is another earlier idiom.

## 5. Motivation

A kit's frame and motor may both require the same bolt. A component report should
list that bolt once, including indirect requirements. Naively following references
without tracking node identity can duplicate shared parts or loop on a cycle.
This report makes the required component set explicit for review.

## 6. Language Alignment

The signature takes a `node()?` start and a step callback returning `node()*`.
It finds nodes reachable in **one or more steps**, removes duplicates by node
identity, and delivers the result in document order. Empty start returns empty.
The start is absent unless a step path reaches it again; cycles can therefore
put the start in the result. Use node union to include it unconditionally.
Non-node input, multiple start nodes or non-node step results raise XPTY0004.

The pinned evolving specification also discusses JNodes. This entry verifies
XML nodes in one document and does not assert JSON-node or cross-document
ordering support. Node identity differs from equality of `@id` strings.

## 7. Applicability

Use for reachability in a finite graph of existing nodes. The fixture assumes
unique, nonempty part IDs and resolved references. References point into the
same catalog document; the callback returns those existing nodes, never freshly
parsed copies. A callback that continually constructs new node identities need
not terminate. The function does not validate missing references or detect a
cycle as a business error. The missing-reference examples show a separate check.

## 8. Structure

The start selects KIT. The step resolves each `needs/@ref` against catalog
`part/@id`. KIT reaches FRAME and MOTOR; both reach BOLT. The closure contains
BOLT, FRAME and MOTOR in catalog document order, regardless of traversal order.

## 9. Participants

The catalog defines stable part nodes. Reference elements define directed edges.
The resolver callback returns adjacent nodes. The closure performs reachability.
The report projects identifiers after traversal and counts the unique nodes.

## 10. Collaborations

Keep the part sequence in the callback's captured scope. Resolve links to nodes,
not atomic ID strings. Add the start with `$start | $reachable` if the report
requires it. Referential-integrity validation remains a separate responsibility.

## 11. Consequences

Shared components appear once. A cycle terminates on this finite catalog and may
include KIT itself. The result does not preserve dependency-path order and cannot
be used directly as an installation sequence. It also does not compute quantities:
several paths to BOLT still produce one BOLT node. Quantity explosion requires
separate path/multiplicity rules.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles One-or-more-step reachability and identity deduplication; Finite cycles may include the initial node; document-order results; Empty start, singleton/node typing and callback errors. **Boundaries:** Only XML nodes in one document are covered; no JNode/cross-document ordering, topological installation order or quantity explosion. Follow-up groups: F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile the new calls using `CompileOptions.Compatibility40`; the default 3.1
profile rejects them. Compile the bounded-fold comparison and integrity check
with `Compatibility31`. The existing [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks exact reports, diagnostic markers and default-profile refusal for successful
4.0 cases. Everything is inline, with no external context or catalog file required.

## 13. Sample Code

Full synthetic catalog input. BOLT is deliberately placed before its referring
components to make document order visibly different from traversal order.

```xml
<catalog>
  <part id="KIT"><needs ref="FRAME"/><needs ref="MOTOR"/></part>
  <part id="BOLT"/>
  <part id="FRAME"><needs ref="BOLT"/></part>
  <part id="MOTOR"><needs ref="BOLT"/></part>
</catalog>
```

[Complete XPath 4.0 expression](../examples/product-dependency-closure/main.xpath):

```xpath
let $parts := parse-xml('<catalog><part id="KIT"><needs ref="FRAME"/><needs ref="MOTOR"/></part><part id="BOLT"/><part id="FRAME"><needs ref="BOLT"/></part><part id="MOTOR"><needs ref="BOLT"/></part></catalog>')/catalog/part
let $start := $parts[@id eq 'KIT']
let $step := function($part) { $parts[@id = $part/needs/@ref] }
let $reachable := transitive-closure($start, $step)
return concat('dependencies=', string-join($reachable/@id, ','), '; count=', count($reachable))
```

Exact output:

```text
dependencies=BOLT,FRAME,MOTOR; count=3
```

[Complete XPath 3.1 comparison](../examples/product-dependency-closure/baseline.xpath):

```xpath
let $parts := parse-xml('<catalog><part id="KIT"><needs ref="FRAME"/><needs ref="MOTOR"/></part><part id="BOLT"/><part id="FRAME"><needs ref="BOLT"/></part><part id="MOTOR"><needs ref="BOLT"/></part></catalog>')/catalog/part
return let $start := $parts[@id eq 'KIT']
return let $step := function($part) { $parts[@id = $part/needs/@ref] }
return let $reachable := fold-left(1 to count($parts), $step($start) | (), function($seen, $round) { $seen | ($seen ! $step(.)) })
return concat('dependencies=', string-join($reachable/@id, ','), '; count=', count($reachable))
```

Exact output:

```text
dependencies=BOLT,FRAME,MOTOR; count=3
```

### Cycle input and result

Full input adds a link from BOLT back to KIT:

```xml
<catalog>
  <part id="KIT"><needs ref="FRAME"/><needs ref="MOTOR"/></part>
  <part id="BOLT"><needs ref="KIT"/></part>
  <part id="FRAME"><needs ref="BOLT"/></part>
  <part id="MOTOR"><needs ref="BOLT"/></part>
</catalog>
```

The [cycle expression](../examples/product-dependency-closure/cycle.xpath) reports:

```text
dependencies=KIT,BOLT,FRAME,MOTOR; count=4
```

### Complete fixture inventory

Each file embeds its full input and exact report or error marker.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/product-dependency-closure/main.xpath) | `dependencies=BOLT,FRAME,MOTOR; count=3` |
| [baseline](../examples/product-dependency-closure/baseline.xpath) | `dependencies=BOLT,FRAME,MOTOR; count=3` |
| [cycle](../examples/product-dependency-closure/cycle.xpath) | `dependencies=KIT,BOLT,FRAME,MOTOR; count=4` |
| [self-cycle](../examples/product-dependency-closure/self-cycle.xpath) | `dependencies=KIT,BOLT,FRAME,MOTOR; count=4` |
| [duplicate-edge](../examples/product-dependency-closure/duplicate-edge.xpath) | `dependencies=BOLT,FRAME,MOTOR; count=3` |
| [leaf](../examples/product-dependency-closure/leaf.xpath) | `dependencies=; count=0` |
| [missing-start](../examples/product-dependency-closure/missing-start.xpath) | `dependencies=; count=0` |
| [include-start](../examples/product-dependency-closure/include-start.xpath) | `KIT,BOLT,FRAME,MOTOR` |
| [node-identity](../examples/product-dependency-closure/node-identity.xpath) | `true` |
| [non-node-start](../examples/product-dependency-closure/non-node-start.xpath) | `XPTY0004` |
| [non-node-step](../examples/product-dependency-closure/non-node-step.xpath) | `XPTY0004` |
| [multiple-starts](../examples/product-dependency-closure/multiple-starts.xpath) | `XPTY0004` |
| [noncallable-step](../examples/product-dependency-closure/noncallable-step.xpath) | `XPTY0004` |
| [dangling-reference](../examples/product-dependency-closure/dangling-reference.xpath) | `dependencies=BOLT,MOTOR; count=2` |
| [reference-validation](../examples/product-dependency-closure/reference-validation.xpath) | `1` |

The dangling-reference fixture replaces FRAME's edge from KIT with MISSING.
Traversal reports only BOLT and MOTOR, because an unresolved edge selects no
node. The separate 3.1 validation expression counts one unresolved reference.
An empty closure from a missing start is likewise not proof of valid catalog data.
The node-identity check proves the returned BOLT is the original catalog node.

## 14. Performance

Traversal stops once expansion finds no new nodes in the finite input graph.
Bosak stores reached nodes and checks duplicates by identity using linear scans;
this is not a claim of constant-time lookup or a benchmark for large catalogs.
The example callback also scans catalog IDs. Large graphs may benefit from a
separately built node index. The result is materialized and sorted by document
order; the bounded 3.1 comparison also accumulates a node set.

## 15. Known Uses

This component catalog is a synthetic workflow, not an identified deployment.
Existing evidence includes
[`TransitiveClosure_ReachesAllDescendants`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`TransitiveClosure_ResultExcludesStartNode`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`TransitiveClosure_DuplicatesAndCycles_Terminate`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`TransitiveClosure_EmptyNode_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`TransitiveClosure_NonNode_RaisesXpty0004`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
has no exact `fn-transitive-closure` assignment in GatedSets. Named unit tests
and these 15 executable examples are separate evidence; no gate is promoted here.

## 16. Related Features

[Product map index](product-map-index.md) describes keyed product data.
[Configured label callbacks](configured-label-callbacks.md) describes binding
callback arguments. [Bounded retry state](bounded-retries.md) discusses finite
state iteration. [The completeness audit](../COMPLETENESS-AUDIT.md) records
reference coverage separately from signature and conformance completeness.
