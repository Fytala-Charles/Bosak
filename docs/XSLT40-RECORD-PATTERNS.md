<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak named record pattern support">
  <br><br><h1>XSLT 4.0 named record patterns</h1>
  <p>Record identity, aliases and entry sequence constraints</p>
</div>

## Contract - 16 October 2026

Unpublished source candidate **1.12.0** adds `record(TypeName)` patterns in version 4.0 stylesheets. The single EQName must identify a nominative record type or an item-type alias resolving to a record type. Structural record aliases and aliases of nominative record types are supported. Aliases resolving to choices, maps, atomic types or other non-record item types are rejected, even when no candidate is selected. An inline anonymous type such as `record(x as xs:integer)` is not this pattern syntax; declare an alias or use an instance-of predicate.

Existing runtime instance checks determine the result. A nominative pattern requires the exact record type identity. Two separately declared record types with identical fields remain distinct. Structural aliases test the record shape and values under existing record matching rules; ordinary maps are not records and are never converted by matching. Existing legacy `~type` syntax and record construction rules are unchanged.

```xml
<xsl:record-type name="t:point" constructor="yes">
  <xsl:field name="x" as="xs:integer"/>
</xsl:record-type>
<xsl:template match="record(t:point)[?x gt 0]">
  <xsl:context-item as="t:point"/>
  <positive><xsl:value-of select="?x"/></positive>
</xsl:template>
<xsl:template match="{'points':record(t:point)+, *}">
  <count><xsl:value-of select="count(?points)"/></count>
</xsl:template>
```

The namespace prefix `t` must be declared by the consumer. Unprefixed pattern type names use the XPath element/type default namespace. EQName namespace punctuation is preserved. Map entry occurrence indicators use the existing cardinality checks: every supplied entry item must match, and an absent key never satisfies an optional entry constraint.

## Predicates, priorities and context types

Bare record patterns have default priority **0**, independent of record shape. Predicates give priority **0.5** and use singleton focus, numeric predicate semantics and the existing dynamic-error non-match behavior. Pattern wrappers restore current item and caller focus. A containing constrained map retains map priority **0.25**. Parenthesized patterns retain their node-only restriction. Bare record patterns cannot be operands of node set patterns. Path suffixes, multiple type arguments and non-4.0 profiles are rejected.

The context declaration test exposed a pre-existing gap: xsl:context-item rejected named record types before pattern compilation. The 4.0 fallback now validates a complete single ItemType and compiles its instance check with applicable namespaces, default namespace, aliases and named record identities from the declaring package. The runtime evaluates that cached check without coercion and restores caller focus. Nominal mismatches raise XTTE0590. Existing built-in context-type checks and earlier profiles retain their prior path. Occurrence indicators, empty-sequence(), unknown type names and arbitrary XPath expressions are not valid required context item types.

This does not add record inheritance, record conversion, array/JNode pattern families, non-string map key constants or schema-defined atomic/union patterns.

## Draft and native evidence

The reviewed source is qt4cg/qtspecs `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [record pattern rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13240) require a named type resolving to a record; the [record priority clause](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13855) specifies 0 and carries an editorial revisit note. Filtered simple-pattern priority follows the [general clause](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L13753).

PR321 passed exact-head CI run 38886368722 before integration. Upstream xslt40-test was fetched and remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`, unchanged in the detached suite checkout. Native record aggregate remains 29/35/1 (record set 29/18/0, xsl-record 0/17/1); type-alias remains 22/9/1. Both family exits remain 1. No native gain, catalog/expectation change, upstream submission, new skip or gate promotion is claimed.

The initial exact 41-case fixture on the atomic-pattern baseline produced 13 pass / 28 fail. Its bytes and log are preserved. The first implementation passed 206 combined pattern cases. An added required-context test then exposed the named-type gap, which is corrected with static and dynamic controls. Intermediate compile fixes and a test-fixture replacement that accidentally created a circular alias are retained separately; the alias fixture was corrected without changing the expected match. The final new fixture has 42 cases; combined patterns and context tests total 210 passing cases.

Source and execution hashes, raw native receipts and verification details are in [machine-readable evidence](XSLT40-RECORD-PATTERNS.json). Public libraries remain 1.8.0; VS Code 0.2.4 bundles engine 1.8.0. No publication, installed-extension change, REQ status change or independent Braid acceptance occurs.

Next bounded review: array patterns, including member sequence matching and recursive default priorities, against the current draft. Preserve the existing map, atomic, record and node-pattern behavior.

## Verification

Release builds pass with zero errors (initial 22 warnings; final incremental 20). Solution 8,804/0/0 (XSLT 5,918), language server 84/0/0, focused patterns/context checks 210/0/0 (42 new record cases). Frozen QT3 31,142/0/679 and qt4 gated 8,278/0/1,894, both exit 0. Native record aggregate 29/35/1 and type-alias 22/9/1 retain exit 1 because failures remain. All local jobs have completed. Canonical docs are rendered with markdown-it and Fytala CSS; kit-managed assets remain unchanged.
