<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak atomic pattern support">
  <br><br><h1>XSLT 4.0 atomic patterns</h1>
  <p>Atomic instances, alternatives and derivation priorities</p>
</div>

## Contract - 16 October 2026

Unpublished source candidate **1.11.0** adds `atomic(TypeName)` patterns to version 4.0 stylesheets. A pattern matches atomic instances of a named type, including derived instances. It never atomizes nodes or coerces strings, untyped values, maps or arrays. Built-in XML Schema atomic types, `xs:anyAtomicType`, `xs:untypedAtomic`, `xs:numeric` and aliases resolving solely to these atomic types or choices are supported. Structural, list and unknown types fail statically, even if no candidate is selected. Schema-defined atomic/union types remain outside this bounded slice.

Alternatives use `|` inside the atomic pattern. Names use the declaring applicable namespaces and the XPath element/type default namespace. EQNames and aliases retain their existing binding rules. Predicates use singleton focus and existing numeric/error behavior; current item and caller focus are restored by the established pattern wrappers. Atomic patterns can declare an atomic required context item. The old node-only static check is bypassed for this new pattern family, with normal runtime context-type validation retained.

```xml
<xsl:template match="atomic(xs:integer | xs:string)[string-length(string(.)) gt 0]">
  <value><xsl:value-of select="."/></value>
</xsl:template>
<xsl:template match="{'amounts':atomic(xs:decimal)+, *}">
  <total><xsl:value-of select="sum(?amounts)"/></total>
</xsl:template>
```

The second example requires an existing amounts entry with one or more decimal instances, including integers. All supplied entry items must match; no conversion occurs. The map pattern has constrained-map priority 0.25 regardless of the nested atomic priority.

## Priority and syntax

| Atomic type | Default priority |
|---|---|
| xs:anyAtomicType | -0.5 |
| Primitive atomic types, xs:untypedAtomic, xs:numeric | 0 |
| xs:integer | 0.25 |
| xs:long | 0.375 |
| xs:int | 0.4375 |
| Alternatives / atomic-only choices | Minimum member priority |
| Atomic pattern with predicates | 0.5 |

For a derived type the priority is `0.5 - 2^-N`, with N derivation steps from anyAtomicType. Existing legacy `~type` pattern behavior is preserved independently. Bare atomic patterns cannot be operands of node set patterns; parentheses introduce the existing node-only pattern restriction. Occurrence indicators apply to map entry sequences, not standalone atomic patterns. Malformed alternatives, unclosed predicates, path suffixes and non-4.0 use are rejected.

## Pinned draft and evidence

The reviewed qtspecs source is `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. See [atomic patterns](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L12986), [atomic priorities](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13767), and [filtered simple patterns](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13753).

PR320 passed exact-head CI run 38884697054 and merged at `ea78df7d` before this slice. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`, unchanged in its detached suite checkout. Native record aggregate remains 29/35/1 (record set 29/18/0, xsl-record 0/17/1); type-alias remains 22/9/1. Both family exits remain 1. No native pass gain, catalog change, expectation adjustment, upstream submission or gate promotion is claimed.

The first 43-case diagnostic fixture and its original 13-pass/30-fail run are preserved. Its priority competitor also matched the document node, so those counts are not an engine-only baseline. Subsequent logs retain the compile corrections, empty-alternative defect and fixture correction separately. The final fixture has 47 cases; together with existing map and legacy type pattern suites, 165 focused cases pass. A concurrent native run collided on the wrapper's millisecond timestamp filename; its failure is retained separately and the families are rerun sequentially. No harness change is included in this slice.

Execution/source hashes and unchanged raw native receipts are in [machine-readable evidence](XSLT40-ATOMIC-PATTERNS.json). Public libraries remain immutable 1.8.0; VS Code 0.2.4 bundles engine 1.8.0. No release, installed-extension change, REQ status change or independent Braid acceptance is performed.

Next bounded review: `record(TypeName)` patterns against the same current draft, including nominative/structural identity and map entry use. Array/JNode patterns, non-string map key constants and schema-defined atomic/union patterns remain separate.

## Verification

Release build: zero errors, two incremental warnings. Solution 8,762/0/0 (XSLT 5,876), language server 84/0/0, focused pattern suites 165/0/0 (47 new atomic cases). Frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894, both exit 0. Sequential native record aggregate 29/35/1 and type-alias 22/9/1 retain exit 1 because failures remain. All local jobs have completed. Canonical documentation is rendered with markdown-it and the Fytala stylesheet; kit-managed assets remain unchanged.
