<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak stock-feed container contract reference">
  <br><br><h1>Stock Feed Normalization: Detailed Example</h1>
  <p>Batch boundaries, product totals and callback contracts</p>
</div>

## Business case and data policy

A warehouse receives ordered batches of product stock quantities. One batch may contain several lines or no lines. The importer must preserve batch boundaries while aggregating quantities by SKU, then report products with positive total stock. Repeated SKU A is legitimate stock across batches; it is summed rather than deduplicated. SKU B has zero stock and is omitted by the positive-total reporting policy. This example produces a diagnostic report and performs no stock update or external write.

Input has stock/batch elements with stable batch id attributes; line elements carry sku and integer qty. These samples contain complete valid data. Production ingestion must validate unique/nonempty batch IDs, nonempty SKUs, present parseable nonnegative quantities and common units before normalization. Negative stock adjustments require a separate explicit business policy. Missing SKU attributes would silently disappear from key selection and must be rejected earlier. Empty batches are retained; no batches differs from a batch containing no lines.

XPath 4.0 builds one array member per batch with array:build, concatenates lines with array:items, aggregates using existing map:entry/map:merge and sum, filters totals with map:filter, and sums map:items. XPath 3.1 uses fold-left/array:append, array and map ?* lookups, and conditional map:merge. Both versions are complete expressions with nested let/return syntax, require no application variables, and agree for these validated inputs. map:entries is demonstrated separately because this report needs totals rather than single-entry maps.

| Output field | Meaning |
|---|---|
| batches | Array member count, including empty batches. |
| lines | Count after removing one batch boundary. |
| ids | Batch IDs in source order; this exposes feed reordering. |
| positive | Positive SKU totals, explicitly sorted by SKU for stable reporting. |
| units | Sum of positive totals, zero for an empty feed. |

## Complete inputs, expressions and outputs

### Normal feed

Complete input:

```xml
<stock><batch id="B1"><line sku="A" qty="2"/><line sku="B" qty="0"/></batch><batch id="B2"/><batch id="B3"><line sku="A" qty="3"/></batch></stock>
```

**XPath 31 — [report-31.xpath](report-31.xpath):**

```xquery
let $batches := parse-xml('<stock><batch id="B1"><line sku="A" qty="2"/><line sku="B" qty="0"/></batch><batch id="B2"/><batch id="B3"><line sku="A" qty="3"/></batch></stock>')/stock/batch return let $groups := fold-left($batches,[],function($a,$b){array:append($a,$b/line)}) return let $lines := $groups?* return let $keys := distinct-values($lines/@sku ! string(.)) return let $totals := map:merge(for $k in $keys return map:entry($k,sum($lines[@sku eq $k]/@qty ! xs:integer(.)))) return let $positive := map:merge(for $k in map:keys($totals) return if ($totals($k) gt 0) then map:entry($k,$totals($k)) else ()) return concat('batches=',array:size($groups),';lines=',count($lines),';ids=',string-join($batches/@id ! string(.),','),';positive=',string-join(for $k in sort(map:keys($positive)) return concat($k,':',$positive($k)),','),';units=',sum($positive?*))
```

Exact output:

```text
batches=3;lines=3;ids=B1,B2,B3;positive=A:5;units=5
```

**XPath 40 — [report-40.xpath](report-40.xpath):**

```xquery
let $batches := parse-xml('<stock><batch id="B1"><line sku="A" qty="2"/><line sku="B" qty="0"/></batch><batch id="B2"/><batch id="B3"><line sku="A" qty="3"/></batch></stock>')/stock/batch return let $groups := array:build($batches,function($b){$b/line}) return let $lines := array:items($groups) return let $keys := distinct-values($lines/@sku ! string(.)) return let $totals := map:merge(for $k in $keys return map:entry($k,sum($lines[@sku eq $k]/@qty ! xs:integer(.)))) return let $positive := map:filter($totals,function($k,$v){$v gt 0}) return concat('batches=',array:size($groups),';lines=',count($lines),';ids=',string-join($batches/@id ! string(.),','),';positive=',string-join(for $k in sort(map:keys($positive)) return concat($k,':',$positive($k)),','),';units=',sum(map:items($positive)))
```

Exact output:

```text
batches=3;lines=3;ids=B1,B2,B3;positive=A:5;units=5
```

### Empty feed

Complete input:

```xml
<stock/>
```

**XPath 31 — [report-empty-31.xpath](report-empty-31.xpath):**

```xquery
let $batches := parse-xml('<stock/>')/stock/batch return let $groups := fold-left($batches,[],function($a,$b){array:append($a,$b/line)}) return let $lines := $groups?* return let $keys := distinct-values($lines/@sku ! string(.)) return let $totals := map:merge(for $k in $keys return map:entry($k,sum($lines[@sku eq $k]/@qty ! xs:integer(.)))) return let $positive := map:merge(for $k in map:keys($totals) return if ($totals($k) gt 0) then map:entry($k,$totals($k)) else ()) return concat('batches=',array:size($groups),';lines=',count($lines),';ids=',string-join($batches/@id ! string(.),','),';positive=',string-join(for $k in sort(map:keys($positive)) return concat($k,':',$positive($k)),','),';units=',sum($positive?*))
```

Exact output:

```text
batches=0;lines=0;ids=;positive=;units=0
```

**XPath 40 — [report-empty-40.xpath](report-empty-40.xpath):**

```xquery
let $batches := parse-xml('<stock/>')/stock/batch return let $groups := array:build($batches,function($b){$b/line}) return let $lines := array:items($groups) return let $keys := distinct-values($lines/@sku ! string(.)) return let $totals := map:merge(for $k in $keys return map:entry($k,sum($lines[@sku eq $k]/@qty ! xs:integer(.)))) return let $positive := map:filter($totals,function($k,$v){$v gt 0}) return concat('batches=',array:size($groups),';lines=',count($lines),';ids=',string-join($batches/@id ! string(.),','),';positive=',string-join(for $k in sort(map:keys($positive)) return concat($k,':',$positive($k)),','),';units=',sum(map:items($positive)))
```

Exact output:

```text
batches=0;lines=0;ids=;positive=;units=0
```

### Reversed batches

Complete input:

```xml
<stock><batch id="B3"><line sku="A" qty="3"/></batch><batch id="B2"/><batch id="B1"><line sku="A" qty="2"/><line sku="B" qty="0"/></batch></stock>
```

**XPath 31 — [report-reordered-31.xpath](report-reordered-31.xpath):**

```xquery
let $batches := parse-xml('<stock><batch id="B3"><line sku="A" qty="3"/></batch><batch id="B2"/><batch id="B1"><line sku="A" qty="2"/><line sku="B" qty="0"/></batch></stock>')/stock/batch return let $groups := fold-left($batches,[],function($a,$b){array:append($a,$b/line)}) return let $lines := $groups?* return let $keys := distinct-values($lines/@sku ! string(.)) return let $totals := map:merge(for $k in $keys return map:entry($k,sum($lines[@sku eq $k]/@qty ! xs:integer(.)))) return let $positive := map:merge(for $k in map:keys($totals) return if ($totals($k) gt 0) then map:entry($k,$totals($k)) else ()) return concat('batches=',array:size($groups),';lines=',count($lines),';ids=',string-join($batches/@id ! string(.),','),';positive=',string-join(for $k in sort(map:keys($positive)) return concat($k,':',$positive($k)),','),';units=',sum($positive?*))
```

Exact output:

```text
batches=3;lines=3;ids=B3,B2,B1;positive=A:5;units=5
```

**XPath 40 — [report-reordered-40.xpath](report-reordered-40.xpath):**

```xquery
let $batches := parse-xml('<stock><batch id="B3"><line sku="A" qty="3"/></batch><batch id="B2"/><batch id="B1"><line sku="A" qty="2"/><line sku="B" qty="0"/></batch></stock>')/stock/batch return let $groups := array:build($batches,function($b){$b/line}) return let $lines := array:items($groups) return let $keys := distinct-values($lines/@sku ! string(.)) return let $totals := map:merge(for $k in $keys return map:entry($k,sum($lines[@sku eq $k]/@qty ! xs:integer(.)))) return let $positive := map:filter($totals,function($k,$v){$v gt 0}) return concat('batches=',array:size($groups),';lines=',count($lines),';ids=',string-join($batches/@id ! string(.),','),';positive=',string-join(for $k in sort(map:keys($positive)) return concat($k,':',$positive($k)),','),';units=',sum(map:items($positive)))
```

Exact output:

```text
batches=3;lines=3;ids=B3,B2,B1;positive=A:5;units=5
```

Reversing batches changes ids from B1,B2,B3 to B3,B2,B1 while quantities remain A:5. The empty B2 still contributes one batch. The empty feed has neither members nor lines. Aggregation preserves all line quantities, including repeated SKUs; sorting is applied only to output keys. Array positions are one-based and depend on feed order, so stable IDs should identify batches across imports.

## Focused contract fixtures

The [contract review](../../CONTAINER-CONTRACT-REVIEW.md) links the immutable specification and explains missing keywords, member/item distinctions, callback adaptation and boolean-result follow-ups. Diagnostic fixtures are separate from validated business inputs. Successful 4.0 cases verify default 3.1 refusal; explicit keyword cases cover all three modes. Exact case IDs and results are in the [manifest](../cases.json).

| Runnable case | Mode | Exact result/error |
|---|---|---|
| [array-build-keyword-31.xpath](array-build-keyword-31.xpath) | 31 | `XPST0003` |
| [array-build-keyword-40.xpath](array-build-keyword-40.xpath) | 40 | `XPST0017` |
| [array-build-keyword-experimental.xpath](array-build-keyword-experimental.xpath) | experimental | `XPST0017` |
| [array-empty-keyword-31.xpath](array-empty-keyword-31.xpath) | 31 | `XPST0003` |
| [array-empty-keyword-40.xpath](array-empty-keyword-40.xpath) | 40 | `XPST0017` |
| [array-empty-keyword-experimental.xpath](array-empty-keyword-experimental.xpath) | experimental | `XPST0017` |
| [array-items-keyword-31.xpath](array-items-keyword-31.xpath) | 31 | `XPST0003` |
| [array-items-keyword-40.xpath](array-items-keyword-40.xpath) | 40 | `XPST0017` |
| [array-items-keyword-experimental.xpath](array-items-keyword-experimental.xpath) | experimental | `XPST0017` |
| [map-entries-keyword-31.xpath](map-entries-keyword-31.xpath) | 31 | `XPST0003` |
| [map-entries-keyword-40.xpath](map-entries-keyword-40.xpath) | 40 | `XPST0017` |
| [map-entries-keyword-experimental.xpath](map-entries-keyword-experimental.xpath) | experimental | `XPST0017` |
| [map-filter-keyword-31.xpath](map-filter-keyword-31.xpath) | 31 | `XPST0003` |
| [map-filter-keyword-40.xpath](map-filter-keyword-40.xpath) | 40 | `XPST0017` |
| [map-filter-keyword-experimental.xpath](map-filter-keyword-experimental.xpath) | experimental | `XPST0017` |
| [map-items-keyword-31.xpath](map-items-keyword-31.xpath) | 31 | `XPST0003` |
| [map-items-keyword-40.xpath](map-items-keyword-40.xpath) | 40 | `XPST0017` |
| [map-items-keyword-experimental.xpath](map-items-keyword-experimental.xpath) | experimental | `XPST0017` |
| [array-build-empty.xpath](array-build-empty.xpath) | 40 | `0` |
| [array-build-identity.xpath](array-build-identity.xpath) | 40 | `3,1,3` |
| [array-build-empty-default.xpath](array-build-empty-default.xpath) | 40 | `3,1` |
| [array-build-one-argument-action.xpath](array-build-one-argument-action.xpath) | 40 | `4,6` |
| [array-build-position.xpath](array-build-position.xpath) | 40 | `1,2` |
| [array-build-empty-members.xpath](array-build-empty-members.xpath) | 40 | `2/0` |
| [array-build-grouped-members.xpath](array-build-grouped-members.xpath) | 40 | `2/2/4` |
| [array-build-nested-array.xpath](array-build-nested-array.xpath) | 40 | `true` |
| [array-build-nested-map.xpath](array-build-nested-map.xpath) | 40 | `true` |
| [array-build-node-identity.xpath](array-build-node-identity.xpath) | 40 | `true` |
| [array-build-wrong-arity.xpath](array-build-wrong-arity.xpath) | 40 | `XPTY0004` |
| [array-build-zero-arity.xpath](array-build-zero-arity.xpath) | 40 | `7,7` |
| [array-build-nonfunction.xpath](array-build-nonfunction.xpath) | 40 | `XPTY0004` |
| [array-build-empty-nonfunction.xpath](array-build-empty-nonfunction.xpath) | 40 | `0` |
| [array-empty-no-members.xpath](array-empty-no-members.xpath) | 40 | `true` |
| [array-items-no-members.xpath](array-items-no-members.xpath) | 40 | `0` |
| [array-empty-empty-member.xpath](array-empty-empty-member.xpath) | 40 | `false` |
| [array-items-empty-member.xpath](array-items-empty-member.xpath) | 40 | `0` |
| [array-empty-nested-empty.xpath](array-empty-nested-empty.xpath) | 40 | `false` |
| [array-items-nested-empty.xpath](array-items-nested-empty.xpath) | 40 | `1` |
| [array-empty-values.xpath](array-empty-values.xpath) | 40 | `false` |
| [array-items-values.xpath](array-items-values.xpath) | 40 | `3` |
| [array-empty-wrong-integer.xpath](array-empty-wrong-integer.xpath) | 40 | `XPTY0004` |
| [array-items-wrong-integer.xpath](array-items-wrong-integer.xpath) | 40 | `XPTY0004` |
| [array-empty-wrong-map.xpath](array-empty-wrong-map.xpath) | 40 | `XPTY0004` |
| [array-items-wrong-map.xpath](array-items-wrong-map.xpath) | 40 | `XPTY0004` |
| [array-empty-wrong-empty.xpath](array-empty-wrong-empty.xpath) | 40 | `XPTY0004` |
| [array-items-wrong-empty.xpath](array-items-wrong-empty.xpath) | 40 | `XPTY0004` |
| [array-empty-wrong-multiple.xpath](array-empty-wrong-multiple.xpath) | 40 | `XPTY0004` |
| [array-items-wrong-multiple.xpath](array-items-wrong-multiple.xpath) | 40 | `XPTY0004` |
| [map-entries-empty.xpath](map-entries-empty.xpath) | 40 | `0` |
| [map-items-empty.xpath](map-items-empty.xpath) | 40 | `0` |
| [map-entries-empty-value.xpath](map-entries-empty-value.xpath) | 40 | `1` |
| [map-items-empty-value.xpath](map-items-empty-value.xpath) | 40 | `0` |
| [map-entries-grouped.xpath](map-entries-grouped.xpath) | 40 | `2` |
| [map-items-grouped.xpath](map-items-grouped.xpath) | 40 | `3` |
| [map-entries-nested.xpath](map-entries-nested.xpath) | 40 | `2` |
| [map-items-nested.xpath](map-items-nested.xpath) | 40 | `2` |
| [map-entries-singletons.xpath](map-entries-singletons.xpath) | 40 | `true` |
| [map-entries-retained-value.xpath](map-entries-retained-value.xpath) | 40 | `1,2` |
| [map-items-nested-array.xpath](map-items-nested-array.xpath) | 40 | `true` |
| [map-items-nested-map.xpath](map-items-nested-map.xpath) | 40 | `true` |
| [map-items-node-identity.xpath](map-items-node-identity.xpath) | 40 | `true` |
| [map-filter-key.xpath](map-filter-key.xpath) | 40 | `B` |
| [map-filter-value.xpath](map-filter-value.xpath) | 40 | `A` |
| [map-filter-position.xpath](map-filter-position.xpath) | 40 | `B` |
| [map-filter-empty-result.xpath](map-filter-empty-result.xpath) | 40 | Empty string |
| [map-filter-string-result.xpath](map-filter-string-result.xpath) | 40 | `A,B` |
| [map-filter-numeric-result.xpath](map-filter-numeric-result.xpath) | 40 | `A,B` |
| [map-filter-multiple-result.xpath](map-filter-multiple-result.xpath) | 40 | `FORG0006` |
| [map-filter-untyped-result.xpath](map-filter-untyped-result.xpath) | 40 | `A,B` |
| [map-filter-arity-four.xpath](map-filter-arity-four.xpath) | 40 | `XPTY0004` |
| [map-filter-zero-arity.xpath](map-filter-zero-arity.xpath) | 40 | `A,B` |
| [map-filter-nonfunction.xpath](map-filter-nonfunction.xpath) | 40 | `XPTY0004` |
| [map-filter-empty.xpath](map-filter-empty.xpath) | 40 | `0` |
| [map-filter-empty-nonfunction.xpath](map-filter-empty-nonfunction.xpath) | 40 | `0` |
| [map-filter-retains-empty.xpath](map-filter-retains-empty.xpath) | 40 | `true` |
| [map-entries-wrong-integer.xpath](map-entries-wrong-integer.xpath) | 40 | `XPTY0004` |
| [map-entries-wrong-array.xpath](map-entries-wrong-array.xpath) | 40 | `XPTY0004` |
| [map-entries-wrong-empty.xpath](map-entries-wrong-empty.xpath) | 40 | `XPTY0004` |
| [map-entries-wrong-multiple.xpath](map-entries-wrong-multiple.xpath) | 40 | `XPTY0004` |
| [map-items-wrong-integer.xpath](map-items-wrong-integer.xpath) | 40 | `XPTY0004` |
| [map-items-wrong-array.xpath](map-items-wrong-array.xpath) | 40 | `XPTY0004` |
| [map-items-wrong-empty.xpath](map-items-wrong-empty.xpath) | 40 | `XPTY0004` |
| [map-items-wrong-multiple.xpath](map-items-wrong-multiple.xpath) | 40 | `XPTY0004` |
