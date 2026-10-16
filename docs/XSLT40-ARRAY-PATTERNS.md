<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak array pattern support">
  <br><br><h1>XSLT 4.0 array patterns</h1>
  <p>Member sequences, recursive matching and priorities</p>
</div>

## Contract - 16 October 2026

Unpublished source candidate **1.13.0** adds `array(SequencePattern)` in version 4.0 stylesheets. The candidate must be an array, and every member must satisfy the contained sequence pattern. The empty array matches vacuously, while an empty member must satisfy the declared cardinality. A missing occurrence indicator requires one item; `?`, `*`, and `+` allow zero-or-one, zero-or-more, and one-or-more items. `empty-sequence()` requires empty members. Members are visited as sequences without flattening, copying or coercion.

Supported nested patterns compose through the existing node, predicate, string-key map, atomic, named record and array matchers. Static namespace/type bindings and nominal record identities are preserved. Outer predicates use singleton array focus; contained predicates use singleton focus for each tested item. Established pattern wrappers restore current item and caller focus. Numeric predicates and dynamic predicate errors retain existing behavior.

```xml
<xsl:template match="array(atomic(xs:integer)+)[array:size(.) gt 0]">
  <count><xsl:value-of select="array:size(.)"/></count>
</xsl:template>
<xsl:template match="array(map{'first', 'last'})">
  <people><xsl:apply-templates select="?*"/></people>
</xsl:template>
```

The consumer must declare `xs` and `array` namespaces. The first rule requires a nonempty array whose members are nonempty integer sequences. The second accepts an empty array or arrays of singleton maps with exactly the two named keys. Arrays can also appear within map entry sequences or other arrays.

## Priorities and static checks

| Array pattern | Default priority |
|---|---|
| array(.) | -1 |
| array(atomic(xs:integer)+) | 0.25 |
| array(array(atomic(xs:integer))) | 0.25 |
| array(record(t:point)) | 0 |
| array(empty-sequence()) | 0.5 |
| Any array pattern containing a predicate | 0.5 |
| Otherwise | Contained pattern priority, ignoring its occurrence indicator |

The predicate override includes predicates within nested members. A bracket inside a quoted map key or type EQName URI is not a predicate. Atomic alternative priorities retain their minimum-member rule. Explicit priorities remain supported. Parenthesized patterns retain their node-only restriction, and bare arrays cannot be operands of node set patterns.

Empty argument lists, top-level comma-separated patterns, standalone array occurrence indicators, path suffixes, unclosed delimiters, unknown member types and non-4.0 use are rejected. Compilation validates member patterns even when the selected array is empty. Two added negative controls exposed repeated indicators accepted by the permissive legacy node matcher: `.??` and `***`. Shared sequence validation now rejects these for map entries and array members; valid `**` (wildcard plus star occurrence) and quantified path wildcards remain supported.

This slice does not add JNode patterns, new array member functions, record conversion, non-string map key constants or schema-defined atomic/union patterns. Existing nested matcher limitations continue to apply.

## Pinned draft and native evidence

Reviewed qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. See [array matching](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13264), [sequence matching](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13044), and [array priorities](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13870).

PR322 passed exact-head CI run 38888159813 before integration. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; the detached suite is unchanged. The baseline array family was executed against the unchanged record-pattern worktree before this implementation was built. Its original log and raw receipt remain there, with byte-identical copies retained in this slice's artifacts.

| Native selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array substring aggregate | 142/18/20 | 143/17/20 |
| array instruction set | 15/14/8 | 15/14/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 71/4/12 | 72/3/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 22/9/1 | 22/9/1 |

Only unchanged native `arrays-024` newly passes, exercising an array of two-key map patterns. No prior native pass is lost. All three family selections still exit 1 because failures remain. Catalogs, expected results, dependency handling, skips, upstream branches and gate membership are unchanged; no upstream submission or family-wide conformance is claimed.

The initial 56-case fixture on the record-pattern baseline produced 10 pass / 46 fail. Its exact bytes/hash and log remain preserved. The first implementation passed 266 combined checks. Added syntax controls exposed two accepted malformed patterns (59 pass / 2 fail); expectations were retained while the sequence validator was corrected. The final fixture has 62 array cases and the combined patterns/context selection passes 272 cases.

Execution/source hashes and original native receipts are in [machine-readable evidence](XSLT40-ARRAY-PATTERNS.json). Public libraries remain 1.8.0; VS Code 0.2.4 bundles engine 1.8.0. No publication, installed-extension change, REQ status change or independent Braid acceptance occurs.

Next bounded review: current array:members / array:of-members rules and the related remaining native array failures before selecting an implementation slice. JNode pattern work remains separate.

## Verification

Release build passes with zero errors and 82 warnings. Solution 8,866/0/0 (XSLT 5,980), language server 84/0/0, focused patterns/context checks 272/0/0 (62 new array cases). Frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894, both exit 0. Native array aggregate improves to 143/17/20, with only arrays-024 newly passing; record 29/35/1 and type-alias 22/9/1 remain unchanged. All three family exits remain 1 because failures remain. All local jobs have completed. Canonical docs are rendered with markdown-it and Fytala CSS; kit-managed assets remain unchanged.
