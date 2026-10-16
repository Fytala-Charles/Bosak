# Non-node copy values in raw and constructed results

Unpublished source **1.14.15** delivers non-node xsl:copy results as values rather than immediately stringifying them. The normal dispatcher previously passed selected/context values to AppendAtomicText, losing XDM kinds and map/array/function payloads and bypassing complex-content errors.

A private delivery helper adds the original item to an active raw sequence accumulator; otherwise it uses existing CopyToResult behavior. Raw array items therefore keep empty/grouped members, while ordinary element/document construction still flattens arrays and joins atomics. Maps/functions retain raw payloads but raise XTDE0450 inside element content and SENR0001 for ordinary XML result construction. Function-dispatch value handling and copy-select focus/cardinality paths are unchanged. JNodes retain their previous delivery path because their copy construction contract is a separate review.

```xml
<xsl:template name="start" as="item()*">
  <xsl:copy select="[(),(1,2)]"><xsl:sequence select="error()"/></xsl:copy>
</xsl:template>
<!-- Raw result: one array, two members; copy content is ignored. -->
```

## Specification and integration base

PR339 passed exact-head CI run38929364960 at `d9d58861a5ea0637aa67fb09f96970ba9908a43a` and merged at `798b9eae1f15e50b77695e8047c28a573d30fb0d`, this branch's base. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`.

The [shallow-copy rules](https://qt4cg.org/specifications/xslt-40/Overview.html#shallow-copy) return selected atomic/function items unchanged, including maps and arrays, without evaluating copy content. Subsequent parent-node construction applies its existing complex-content rules. This slice preserves that distinction in result delivery.

## Regression controls and fixture history

The initial fixture used nonexistent XdmValue IsInteger/IsString/IsBoolean/IsDecimal properties and a nonexistent map Get method. A first API correction still assumed an indexer. Both compile-failure sources/logs are retained. The compilable fixture uses Kind and TryGetValue with the original expectations.

The compilable unprefixed27-case baseline passed6/failed21. Sixteen failures were unrelated static XTSE0090 errors: a default xmlns declaration on xsl:copy is incorrectly treated as an invalid ordinary attribute. The first implementation passed212/failed16 combined controls (27 new +201 previous): the remaining failures were that static validator limitation, not value-delivery regressions.

The fixture then used explicit xsl prefixes to isolate the runtime contract; assertions and selected values were unchanged. The legal default-namespace form and its original execution receipts remain preserved as a separate validator defect. Prefixed27-case baseline on the original engine: **12 passed / 15 failed**. Expanded33-case baseline: **14 passed / 19 failed**. Final combined controls: **234/0/0** (33 new +201 previous). The final fixture is byte-identical to the expanded pre-fix source.

Coverage includes select/context copying, normal/function dispatch, integer/boolean/decimal/string kinds and payloads, map entries, grouped/empty array members, raw function kind and callability, typed mixed-result order, typed array variables, empty selection, selected cardinality, ignored error-producing content, complex-content spaces/array flattening and map/function errors for element/XML construction. Original sources/logs and the first implementation are retained.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact passing identities are unchanged. PR339 receipts/logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. The companion JSON retains source/log/receipt SHA-256 hashes. No upstream expectations, aliases, skips or gate membership changed.

## Verification and next review

All local jobs completed: Release build zero errors/2 warnings; solution **9,281/0/0** (XSLT6,347), focused combined **234/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered using markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Source1.14.15 is unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next: exact-head CI/review, then fix default namespace declarations incorrectly rejected by xsl:copy validation, using independent controls. JNode construction, other constructors' namespace handling, nonraw text-only API delivery kind, processor capability and obsolete patterns/functions remain separate.
