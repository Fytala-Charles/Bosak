<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak warehouse comparison and selection reference">
  <br><br><h1>Warehouse Exceptions: Detailed Example</h1>
  <p>Warehouse exceptions, extrema and position-aware selection</p>
</div>

## Business case and data policy

A stock-feed importer needs a compact diagnostic report: which SKU appears on multiple lot records, the observed highest/lowest stock quantities, and the positions of records below the reorder threshold of three units. Repeated SKUs can legitimately represent separate lots; the report flags review rather than declaring duplicates invalid or merging lots. It performs no purchase, stock update or external write.

The synthetic warehouse contains ordered lot elements with id, sku and stock attributes. XPath paths select SKU strings and integer stock values. xs:integer establishes numeric ordering; lexicographic 10/2 ordering would be wrong. These samples have complete valid attributes. Production ingestion must validate missing/empty IDs and SKUs, missing or invalid stock, nonnegative quantity policy, unit consistency and source order before using the report. Position is the index in the selected stock sequence; dropped missing stock attributes would break its relationship to lot positions.

| Field | Meaning |
|---|---|
| lots | Observed lot count. |
| repeated | One SKU per repeated class, in second-occurrence order. |
| high / low | Numeric boundary values retaining repeated equal occurrences. |
| lowPositions | One-based positions where stock is below three. |

The 4.0 report uses duplicate-values, highest, lowest and index-where. The portable 3.1 report uses earlier-occurrence counting, max/min filtering and quantified position iteration. They agree for these validated string/integer inputs. The extrema alternative is not a general replacement for arbitrary keys or record equality. Both versions parse complete input and need no application variables or resource access. Nested let/return clauses keep the 3.1 syntax portable.

## Complete inputs, expressions and exact outputs

### Normal feed

Complete input:

```xml
<warehouse><lot id="L1" sku="A" stock="2"/><lot id="L2" sku="B" stock="10"/><lot id="L3" sku="A" stock="10"/><lot id="L4" sku="C" stock="1"/></warehouse>
```

**XPath 40 — [report-40.xpath](report-40.xpath):**

```xquery
let $lots := parse-xml('<warehouse><lot id="L1" sku="A" stock="2"/><lot id="L2" sku="B" stock="10"/><lot id="L3" sku="A" stock="10"/><lot id="L4" sku="C" stock="1"/></warehouse>')/warehouse/lot return let $skus := $lots/@sku ! string(.) return let $stock := $lots/@stock ! xs:integer(.) return concat('lots=',count($lots),';repeated=',string-join((duplicate-values($skus)),','),';high=',string-join((highest($stock)) ! string(.),','),';low=',string-join((lowest($stock)) ! string(.),','),';lowPositions=',string-join((index-where($stock,function($v,$p){$v lt 3})) ! string(.),','))
```

Exact output:

```text
lots=4;repeated=A;high=10,10;low=1;lowPositions=1,4
```

**XPath 31 — [report-31.xpath](report-31.xpath):**

```xquery
let $lots := parse-xml('<warehouse><lot id="L1" sku="A" stock="2"/><lot id="L2" sku="B" stock="10"/><lot id="L3" sku="A" stock="10"/><lot id="L4" sku="C" stock="1"/></warehouse>')/warehouse/lot return let $skus := $lots/@sku ! string(.) return let $stock := $lots/@stock ! xs:integer(.) return concat('lots=',count($lots),';repeated=',string-join((for $i in 1 to count($skus) return if (count($skus[position() lt $i][. eq $skus[$i]]) eq 1) then $skus[$i] else ()),','),';high=',string-join(($stock[. eq max($stock)]) ! string(.),','),';low=',string-join(($stock[. eq min($stock)]) ! string(.),','),';lowPositions=',string-join((for $i in 1 to count($stock) return if ($stock[$i] lt 3) then $i else ()) ! string(.),','))
```

Exact output:

```text
lots=4;repeated=A;high=10,10;low=1;lowPositions=1,4
```

### Empty feed

Complete input:

```xml
<warehouse/>
```

**XPath 40 — [report-empty-40.xpath](report-empty-40.xpath):**

```xquery
let $lots := parse-xml('<warehouse/>')/warehouse/lot return let $skus := $lots/@sku ! string(.) return let $stock := $lots/@stock ! xs:integer(.) return concat('lots=',count($lots),';repeated=',string-join((duplicate-values($skus)),','),';high=',string-join((highest($stock)) ! string(.),','),';low=',string-join((lowest($stock)) ! string(.),','),';lowPositions=',string-join((index-where($stock,function($v,$p){$v lt 3})) ! string(.),','))
```

Exact output:

```text
lots=0;repeated=;high=;low=;lowPositions=
```

**XPath 31 — [report-empty-31.xpath](report-empty-31.xpath):**

```xquery
let $lots := parse-xml('<warehouse/>')/warehouse/lot return let $skus := $lots/@sku ! string(.) return let $stock := $lots/@stock ! xs:integer(.) return concat('lots=',count($lots),';repeated=',string-join((for $i in 1 to count($skus) return if (count($skus[position() lt $i][. eq $skus[$i]]) eq 1) then $skus[$i] else ()),','),';high=',string-join(($stock[. eq max($stock)]) ! string(.),','),';low=',string-join(($stock[. eq min($stock)]) ! string(.),','),';lowPositions=',string-join((for $i in 1 to count($stock) return if ($stock[$i] lt 3) then $i else ()) ! string(.),','))
```

Exact output:

```text
lots=0;repeated=;high=;low=;lowPositions=
```

### Reversed feed

Complete input:

```xml
<warehouse><lot id="L4" sku="C" stock="1"/><lot id="L3" sku="A" stock="10"/><lot id="L2" sku="B" stock="10"/><lot id="L1" sku="A" stock="2"/></warehouse>
```

**XPath 40 — [report-reordered-40.xpath](report-reordered-40.xpath):**

```xquery
let $lots := parse-xml('<warehouse><lot id="L4" sku="C" stock="1"/><lot id="L3" sku="A" stock="10"/><lot id="L2" sku="B" stock="10"/><lot id="L1" sku="A" stock="2"/></warehouse>')/warehouse/lot return let $skus := $lots/@sku ! string(.) return let $stock := $lots/@stock ! xs:integer(.) return concat('lots=',count($lots),';repeated=',string-join((duplicate-values($skus)),','),';high=',string-join((highest($stock)) ! string(.),','),';low=',string-join((lowest($stock)) ! string(.),','),';lowPositions=',string-join((index-where($stock,function($v,$p){$v lt 3})) ! string(.),','))
```

Exact output:

```text
lots=4;repeated=A;high=10,10;low=1;lowPositions=1,4
```

**XPath 31 — [report-reordered-31.xpath](report-reordered-31.xpath):**

```xquery
let $lots := parse-xml('<warehouse><lot id="L4" sku="C" stock="1"/><lot id="L3" sku="A" stock="10"/><lot id="L2" sku="B" stock="10"/><lot id="L1" sku="A" stock="2"/></warehouse>')/warehouse/lot return let $skus := $lots/@sku ! string(.) return let $stock := $lots/@stock ! xs:integer(.) return concat('lots=',count($lots),';repeated=',string-join((for $i in 1 to count($skus) return if (count($skus[position() lt $i][. eq $skus[$i]]) eq 1) then $skus[$i] else ()),','),';high=',string-join(($stock[. eq max($stock)]) ! string(.),','),';low=',string-join(($stock[. eq min($stock)]) ! string(.),','),';lowPositions=',string-join((for $i in 1 to count($stock) return if ($stock[$i] lt 3) then $i else ()) ! string(.),','))
```

Exact output:

```text
lots=4;repeated=A;high=10,10;low=1;lowPositions=1,4
```

An empty feed has no quantities or repeated SKUs. Reversing this particular feed preserves the report values because the repeated SKU remains A, both maximum quantities are 10, and low stocks still occupy positions 1/4. The underlying lot IDs at those positions change; position references depend on source order and are not persistent identifiers.

## Focused contract fixtures

The [contract review](../../SELECTION-CONTRACT-REVIEW.md) explains pinned signatures, missing versus supported keywords, map tolerance and the observed NaN boundary. Diagnostics below are separate from the validated business report. Extrema over maps select boundary-equal records rather than all equal-score records; use explicit score filtering for that different policy.

| Runnable case | Mode | Exact result/error |
|---|---|---|
| [duplicate-values-keyword-31.xpath](duplicate-values-keyword-31.xpath) | 31 | `XPST0003` |
| [duplicate-values-keyword-40.xpath](duplicate-values-keyword-40.xpath) | 40 | `XPST0017` |
| [duplicate-values-keyword-experimental.xpath](duplicate-values-keyword-experimental.xpath) | experimental | `XPST0017` |
| [highest-keyword-31.xpath](highest-keyword-31.xpath) | 31 | `XPST0003` |
| [highest-keyword-40.xpath](highest-keyword-40.xpath) | 40 | `XPST0017` |
| [highest-keyword-experimental.xpath](highest-keyword-experimental.xpath) | experimental | `XPST0017` |
| [lowest-keyword-31.xpath](lowest-keyword-31.xpath) | 31 | `XPST0003` |
| [lowest-keyword-40.xpath](lowest-keyword-40.xpath) | 40 | `XPST0017` |
| [lowest-keyword-experimental.xpath](lowest-keyword-experimental.xpath) | experimental | `XPST0017` |
| [index-where-keyword-31.xpath](index-where-keyword-31.xpath) | 31 | `XPST0003` |
| [index-where-keyword-40.xpath](index-where-keyword-40.xpath) | 40 | `2` |
| [index-where-keyword-experimental.xpath](index-where-keyword-experimental.xpath) | experimental | `2` |
| [duplicate-values-empty.xpath](duplicate-values-empty.xpath) | 40 | `(empty string)` |
| [duplicate-values-singleton.xpath](duplicate-values-singleton.xpath) | 40 | `(empty string)` |
| [duplicate-values-second-order.xpath](duplicate-values-second-order.xpath) | 40 | `B,A` |
| [duplicate-values-numeric.xpath](duplicate-values-numeric.xpath) | 40 | `1,2` |
| [duplicate-values-nan.xpath](duplicate-values-nan.xpath) | 40 | `NaN` |
| [duplicate-values-nodes.xpath](duplicate-values-nodes.xpath) | 40 | `A` |
| [duplicate-values-empty-collation.xpath](duplicate-values-empty-collation.xpath) | 40 | `A` |
| [duplicate-values-case-collation.xpath](duplicate-values-case-collation.xpath) | 40 | `a,b` |
| [duplicate-values-invalid-collation.xpath](duplicate-values-invalid-collation.xpath) | 40 | `FOCH0002` |
| [duplicate-values-map.xpath](duplicate-values-map.xpath) | 40 | `(empty string)` |
| [highest-empty.xpath](highest-empty.xpath) | 40 | `(empty string)` |
| [highest-numeric.xpath](highest-numeric.xpath) | 40 | `9,9` |
| [highest-empty-defaults.xpath](highest-empty-defaults.xpath) | 40 | `9` |
| [highest-nan.xpath](highest-nan.xpath) | 40 | `1` |
| [highest-nodes.xpath](highest-nodes.xpath) | 40 | `10` |
| [highest-bad-node.xpath](highest-bad-node.xpath) | 40 | `FORG0001` |
| [highest-case-collation.xpath](highest-case-collation.xpath) | 40 | `A,a` |
| [highest-invalid-collation.xpath](highest-invalid-collation.xpath) | 40 | `FOCH0002` |
| [highest-empty-invalid-collation.xpath](highest-empty-invalid-collation.xpath) | 40 | `(empty string)` |
| [highest-wrong-key-arity.xpath](highest-wrong-key-arity.xpath) | 40 | `XPTY0004` |
| [highest-nonfunction-key.xpath](highest-nonfunction-key.xpath) | 40 | `XPTY0004` |
| [highest-empty-key.xpath](highest-empty-key.xpath) | 40 | `9` |
| [highest-tuple-key.xpath](highest-tuple-key.xpath) | 40 | `9` |
| [highest-map-boundary.xpath](highest-map-boundary.xpath) | 40 | `B` |
| [lowest-empty.xpath](lowest-empty.xpath) | 40 | `(empty string)` |
| [lowest-numeric.xpath](lowest-numeric.xpath) | 40 | `2` |
| [lowest-empty-defaults.xpath](lowest-empty-defaults.xpath) | 40 | `2` |
| [lowest-nan.xpath](lowest-nan.xpath) | 40 | `2` |
| [lowest-nodes.xpath](lowest-nodes.xpath) | 40 | `2` |
| [lowest-bad-node.xpath](lowest-bad-node.xpath) | 40 | `FORG0001` |
| [lowest-case-collation.xpath](lowest-case-collation.xpath) | 40 | `A,a` |
| [lowest-invalid-collation.xpath](lowest-invalid-collation.xpath) | 40 | `FOCH0002` |
| [lowest-empty-invalid-collation.xpath](lowest-empty-invalid-collation.xpath) | 40 | `(empty string)` |
| [lowest-wrong-key-arity.xpath](lowest-wrong-key-arity.xpath) | 40 | `XPTY0004` |
| [lowest-nonfunction-key.xpath](lowest-nonfunction-key.xpath) | 40 | `XPTY0004` |
| [lowest-empty-key.xpath](lowest-empty-key.xpath) | 40 | `2` |
| [lowest-tuple-key.xpath](lowest-tuple-key.xpath) | 40 | `2` |
| [lowest-map-boundary.xpath](lowest-map-boundary.xpath) | 40 | `C` |
| [index-where-empty.xpath](index-where-empty.xpath) | 40 | `(empty string)` |
| [index-where-position.xpath](index-where-position.xpath) | 40 | `1,3` |
| [index-where-empty-result.xpath](index-where-empty-result.xpath) | 40 | `(empty string)` |
| [index-where-string-result.xpath](index-where-string-result.xpath) | 40 | `XPTY0004` |
| [index-where-multiple-result.xpath](index-where-multiple-result.xpath) | 40 | `XPTY0004` |
| [index-where-untyped-result.xpath](index-where-untyped-result.xpath) | 40 | `1` |
| [index-where-nonfunction.xpath](index-where-nonfunction.xpath) | 40 | `XPTY0004` |
| [index-where-arity-three.xpath](index-where-arity-three.xpath) | 40 | `XPTY0004` |
| [index-where-reordered-keywords.xpath](index-where-reordered-keywords.xpath) | 40 | `2` |
| [index-where-missing-predicate.xpath](index-where-missing-predicate.xpath) | 40 | `XPST0017` |
| [duplicate-values-map-count.xpath](duplicate-values-map-count.xpath) | 40 | `0` |
| [highest-node-identity.xpath](highest-node-identity.xpath) | 40 | `true` |

## Verification and limitations

[cases.json](../cases.json) registers 68 exact checks for this review. [CatalogExampleTests](../../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) evaluates them at declared modes; successful 4.0 functions require default-3.1 refusal and successful experimental index-where also matches Frozen 4.0. Independent XML processing checks all six report outputs. Exact family units and gate applicability remain separate evidence. Version stays 1.1.2; full conversion, NaN ordering and real warehouse acceptance remain outside these bounded examples.
