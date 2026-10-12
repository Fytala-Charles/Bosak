<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak existing XPath function contract inventory">
  <br><br><h1>Stock Lookup Defaults: Detailed Example</h1>
  <p>Dated 3.1 declarations, pinned 4.0 differences and consumer boundaries</p>
</div>

## Business case and policy

A warehouse diagnostic requests quantities for stock codes A and Z and the code at row positions 1 and 4. It must distinguish explicit zero stock from an absent code and avoid out-of-range array access. For this report, an absent stock code has diagnostic quantity zero and an absent row is labelled missing-row. This is an explicit reporting policy, not proof that a missing code means zero stock in an authoritative inventory.

Input stock/item elements have unique nonempty code attributes and present nonnegative integer quantity attributes in common units. Validate that contract before production use. Duplicate codes would require a defined aggregation or error policy; this fixture uses unique keys and does not certify duplicate handling. map:contains distinguishes a missing key from an entry whose value is empty. These inputs never supply empty quantity values. One-based positions refer to source order and change when the feed is reordered; use stable codes to identify items across feeds.

The pinned 4.0 catalog adds fallback parameters to map:get and array:get, but their #3 references/lookup are unavailable in this Bosak baseline. Both version reports therefore use the complete portable policy: map:contains before map:get#2, and 1 <= position <= array:size before array:get#2. The expressions run at XPath 3.1 and Frozen 4.0 with identical observed results; switching modes does not supply the missing overloads. They are compatibility solutions, not general emulations of all pinned fallback semantics. map:merge builds the index from one map:entry per source item; array construction preserves row codes; string-join/concat assemble the report. No external resource, current clock or application variable is used.

| Report field | Meaning |
|---|---|
| rows | Source row count. |
| quantities | A and Z quantities in request order, absent keys defaulted explicitly to zero. |
| selected | Codes at positions 1/4, or missing-row after a bounds check. |

## Complete inputs, expressions and exact outputs

### Normal stock

Complete input:

```xml
<stock><item code="A" quantity="5"/><item code="B" quantity="0"/><item code="C" quantity="2"/></stock>
```

**XPath 31 — [report-31.xpath](report-31.xpath):**

```xquery
let $rows := parse-xml('<stock><item code="A" quantity="5"/><item code="B" quantity="0"/><item code="C" quantity="2"/></stock>')/stock/item
return let $byCode := map:merge(for $r in $rows return map:entry(string($r/@code),xs:integer($r/@quantity)))
return let $codes := array { $rows/@code ! string(.) }
return concat('rows=',count($rows),';quantities=',string-join(
  for $code in ('A','Z') return concat($code,':',if (map:contains($byCode,$code)) then map:get($byCode,$code) else 0)
,','),';selected=',string-join(
  for $p in (1,4) return if ($p ge 1 and $p le array:size($codes)) then array:get($codes,$p) else 'missing-row'
,','))
```

Exact output:

```text
rows=3;quantities=A:5,Z:0;selected=A,missing-row
```

**XPath 40 — [report-40.xpath](report-40.xpath):**

```xquery
let $rows := parse-xml('<stock><item code="A" quantity="5"/><item code="B" quantity="0"/><item code="C" quantity="2"/></stock>')/stock/item
return let $byCode := map:merge(for $r in $rows return map:entry(string($r/@code),xs:integer($r/@quantity)))
return let $codes := array { $rows/@code ! string(.) }
return concat('rows=',count($rows),';quantities=',string-join(
  for $code in ('A','Z') return concat($code,':',if (map:contains($byCode,$code)) then map:get($byCode,$code) else 0)
,','),';selected=',string-join(
  for $p in (1,4) return if ($p ge 1 and $p le array:size($codes)) then array:get($codes,$p) else 'missing-row'
,','))
```

Exact output:

```text
rows=3;quantities=A:5,Z:0;selected=A,missing-row
```

### Empty stock

Complete input:

```xml
<stock/>
```

**XPath 31 — [report-empty-31.xpath](report-empty-31.xpath):**

```xquery
let $rows := parse-xml('<stock/>')/stock/item
return let $byCode := map:merge(for $r in $rows return map:entry(string($r/@code),xs:integer($r/@quantity)))
return let $codes := array { $rows/@code ! string(.) }
return concat('rows=',count($rows),';quantities=',string-join(
  for $code in ('A','Z') return concat($code,':',if (map:contains($byCode,$code)) then map:get($byCode,$code) else 0)
,','),';selected=',string-join(
  for $p in (1,4) return if ($p ge 1 and $p le array:size($codes)) then array:get($codes,$p) else 'missing-row'
,','))
```

Exact output:

```text
rows=0;quantities=A:0,Z:0;selected=missing-row,missing-row
```

**XPath 40 — [report-empty-40.xpath](report-empty-40.xpath):**

```xquery
let $rows := parse-xml('<stock/>')/stock/item
return let $byCode := map:merge(for $r in $rows return map:entry(string($r/@code),xs:integer($r/@quantity)))
return let $codes := array { $rows/@code ! string(.) }
return concat('rows=',count($rows),';quantities=',string-join(
  for $code in ('A','Z') return concat($code,':',if (map:contains($byCode,$code)) then map:get($byCode,$code) else 0)
,','),';selected=',string-join(
  for $p in (1,4) return if ($p ge 1 and $p le array:size($codes)) then array:get($codes,$p) else 'missing-row'
,','))
```

Exact output:

```text
rows=0;quantities=A:0,Z:0;selected=missing-row,missing-row
```

### Reversed rows

Complete input:

```xml
<stock><item code="C" quantity="2"/><item code="B" quantity="0"/><item code="A" quantity="5"/></stock>
```

**XPath 31 — [report-reordered-31.xpath](report-reordered-31.xpath):**

```xquery
let $rows := parse-xml('<stock><item code="C" quantity="2"/><item code="B" quantity="0"/><item code="A" quantity="5"/></stock>')/stock/item
return let $byCode := map:merge(for $r in $rows return map:entry(string($r/@code),xs:integer($r/@quantity)))
return let $codes := array { $rows/@code ! string(.) }
return concat('rows=',count($rows),';quantities=',string-join(
  for $code in ('A','Z') return concat($code,':',if (map:contains($byCode,$code)) then map:get($byCode,$code) else 0)
,','),';selected=',string-join(
  for $p in (1,4) return if ($p ge 1 and $p le array:size($codes)) then array:get($codes,$p) else 'missing-row'
,','))
```

Exact output:

```text
rows=3;quantities=A:5,Z:0;selected=C,missing-row
```

**XPath 40 — [report-reordered-40.xpath](report-reordered-40.xpath):**

```xquery
let $rows := parse-xml('<stock><item code="C" quantity="2"/><item code="B" quantity="0"/><item code="A" quantity="5"/></stock>')/stock/item
return let $byCode := map:merge(for $r in $rows return map:entry(string($r/@code),xs:integer($r/@quantity)))
return let $codes := array { $rows/@code ! string(.) }
return concat('rows=',count($rows),';quantities=',string-join(
  for $code in ('A','Z') return concat($code,':',if (map:contains($byCode,$code)) then map:get($byCode,$code) else 0)
,','),';selected=',string-join(
  for $p in (1,4) return if ($p ge 1 and $p le array:size($codes)) then array:get($codes,$p) else 'missing-row'
,','))
```

Exact output:

```text
rows=3;quantities=A:5,Z:0;selected=C,missing-row
```

Reversal changes the first selected code from A to C while code-based quantity lookup remains A:5. Z is absent in every fixture. Empty input defaults both requested quantities to zero and both positions to missing-row. The complete report retains all policy decisions for review without making a stock update.

## Focused declaration/default diagnostics

The [inventory review](../../CHANGED-31-CONTRACT-INVENTORY.md) explains seventeen named-reference refusals, empty-key/options failures and the fn:transform host boundary. These are separate from the validated business report. Raw declarations, source hashes, defaults, change-annotation attributes and contract prose/reference fingerprints are in the [machine-readable inventory](../../audit-changed31-contracts.json). Availability is not full invocation or conformance evidence.

| Runnable diagnostic | Exact result/error |
|---|---|
| [reference-array-get-3.xpath](reference-array-get-3.xpath) | `XPST0017` |
| [reference-fn-concat-0.xpath](reference-fn-concat-0.xpath) | `XPST0017` |
| [reference-fn-concat-1.xpath](reference-fn-concat-1.xpath) | `XPST0017` |
| [reference-fn-doc-2.xpath](reference-fn-doc-2.xpath) | `XPST0017` |
| [reference-fn-doc-available-2.xpath](reference-fn-doc-available-2.xpath) | `XPST0017` |
| [reference-fn-format-date-3.xpath](reference-fn-format-date-3.xpath) | `XPST0017` |
| [reference-fn-format-date-4.xpath](reference-fn-format-date-4.xpath) | `XPST0017` |
| [reference-fn-format-dateTime-3.xpath](reference-fn-format-dateTime-3.xpath) | `XPST0017` |
| [reference-fn-format-dateTime-4.xpath](reference-fn-format-dateTime-4.xpath) | `XPST0017` |
| [reference-fn-format-time-3.xpath](reference-fn-format-time-3.xpath) | `XPST0017` |
| [reference-fn-format-time-4.xpath](reference-fn-format-time-4.xpath) | `XPST0017` |
| [reference-fn-parse-xml-2.xpath](reference-fn-parse-xml-2.xpath) | `XPST0017` |
| [reference-fn-parse-xml-fragment-2.xpath](reference-fn-parse-xml-fragment-2.xpath) | `XPST0017` |
| [reference-fn-path-2.xpath](reference-fn-path-2.xpath) | `XPST0017` |
| [reference-fn-replace-2.xpath](reference-fn-replace-2.xpath) | `XPST0017` |
| [reference-fn-round-3.xpath](reference-fn-round-3.xpath) | `XPST0017` |
| [reference-map-get-3.xpath](reference-map-get-3.xpath) | `XPST0017` |
| [concat-zero.xpath](concat-zero.xpath) | `XPST0017` |
| [concat-sequence.xpath](concat-sequence.xpath) | `XPST0017` |
| [replace-default.xpath](replace-default.xpath) | `XPST0017` |
| [substring-empty-length.xpath](substring-empty-length.xpath) | `A` |
| [subsequence-empty-length.xpath](subsequence-empty-length.xpath) | `B,C` |
| [sort-empty-key.xpath](sort-empty-key.xpath) | `XPTY0004` |
| [map-merge-empty-options.xpath](map-merge-empty-options.xpath) | `XPTY0004` |
| [get-portable-map-default.xpath](get-portable-map-default.xpath) | `0` |
| [get-portable-array-default.xpath](get-portable-array-default.xpath) | `missing-row` |
| [format-date-full-arity.xpath](format-date-full-arity.xpath) | `2024-02-29` |

## Complete baseline-name availability inventory

Every row below has exact lookup fixtures at Frozen 4.0 with an independently observed XPath31 baseline. Values are lookup counts per sampled arity (0 or 1), not transformation/evaluation results. concat has unbounded arity; finite representatives do not impose an upper bound. fn:transform requires its XSLT-capable host hook.

| Function | Lookup fixtures | Observed 31 / 40 |
|---|---|---|
| `array:append` | [#2](inventory-array-append-2.xpath) | #2: 1/1 |
| `array:filter` | [#2](inventory-array-filter-2.xpath) | #2: 1/1 |
| `array:flatten` | [#1](inventory-array-flatten-1.xpath) | #1: 1/1 |
| `array:fold-left` | [#3](inventory-array-fold-left-3.xpath) | #3: 1/1 |
| `array:fold-right` | [#3](inventory-array-fold-right-3.xpath) | #3: 1/1 |
| `array:for-each` | [#2](inventory-array-for-each-2.xpath) | #2: 1/1 |
| `array:for-each-pair` | [#3](inventory-array-for-each-pair-3.xpath) | #3: 1/1 |
| `array:get` | [#2](inventory-array-get-2.xpath), [#3](inventory-array-get-3.xpath) | #2: 1/1, #3: 0/0 |
| `array:head` | [#1](inventory-array-head-1.xpath) | #1: 1/1 |
| `array:insert-before` | [#3](inventory-array-insert-before-3.xpath) | #3: 1/1 |
| `array:join` | [#1](inventory-array-join-1.xpath) | #1: 1/1 |
| `array:put` | [#3](inventory-array-put-3.xpath) | #3: 1/1 |
| `array:remove` | [#2](inventory-array-remove-2.xpath) | #2: 1/1 |
| `array:reverse` | [#1](inventory-array-reverse-1.xpath) | #1: 1/1 |
| `array:size` | [#1](inventory-array-size-1.xpath) | #1: 1/1 |
| `array:sort` | [#1](inventory-array-sort-1.xpath), [#2](inventory-array-sort-2.xpath), [#3](inventory-array-sort-3.xpath) | #1: 1/1, #2: 1/1, #3: 1/1 |
| `array:subarray` | [#2](inventory-array-subarray-2.xpath), [#3](inventory-array-subarray-3.xpath) | #2: 1/1, #3: 1/1 |
| `array:tail` | [#1](inventory-array-tail-1.xpath) | #1: 1/1 |
| `fn:QName` | [#2](inventory-fn-QName-2.xpath) | #2: 1/1 |
| `fn:abs` | [#1](inventory-fn-abs-1.xpath) | #1: 1/1 |
| `fn:adjust-date-to-timezone` | [#1](inventory-fn-adjust-date-to-timezone-1.xpath), [#2](inventory-fn-adjust-date-to-timezone-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:adjust-dateTime-to-timezone` | [#1](inventory-fn-adjust-dateTime-to-timezone-1.xpath), [#2](inventory-fn-adjust-dateTime-to-timezone-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:adjust-time-to-timezone` | [#1](inventory-fn-adjust-time-to-timezone-1.xpath), [#2](inventory-fn-adjust-time-to-timezone-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:analyze-string` | [#2](inventory-fn-analyze-string-2.xpath), [#3](inventory-fn-analyze-string-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:apply` | [#2](inventory-fn-apply-2.xpath) | #2: 1/1 |
| `fn:available-environment-variables` | [#0](inventory-fn-available-environment-variables-0.xpath) | #0: 1/1 |
| `fn:avg` | [#1](inventory-fn-avg-1.xpath) | #1: 1/1 |
| `fn:base-uri` | [#0](inventory-fn-base-uri-0.xpath), [#1](inventory-fn-base-uri-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:boolean` | [#1](inventory-fn-boolean-1.xpath) | #1: 1/1 |
| `fn:ceiling` | [#1](inventory-fn-ceiling-1.xpath) | #1: 1/1 |
| `fn:codepoint-equal` | [#2](inventory-fn-codepoint-equal-2.xpath) | #2: 1/1 |
| `fn:codepoints-to-string` | [#1](inventory-fn-codepoints-to-string-1.xpath) | #1: 1/1 |
| `fn:collation-key` | [#1](inventory-fn-collation-key-1.xpath), [#2](inventory-fn-collation-key-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:collection` | [#0](inventory-fn-collection-0.xpath), [#1](inventory-fn-collection-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:compare` | [#2](inventory-fn-compare-2.xpath), [#3](inventory-fn-compare-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:concat` | [#0](inventory-fn-concat-0.xpath), [#1](inventory-fn-concat-1.xpath), [#2](inventory-fn-concat-2.xpath), [#3](inventory-fn-concat-3.xpath), [#4](inventory-fn-concat-4.xpath), [#5](inventory-fn-concat-5.xpath), [#6](inventory-fn-concat-6.xpath), [#7](inventory-fn-concat-7.xpath), [#8](inventory-fn-concat-8.xpath), [#9](inventory-fn-concat-9.xpath), [#10](inventory-fn-concat-10.xpath), [#11](inventory-fn-concat-11.xpath), [#12](inventory-fn-concat-12.xpath), [#13](inventory-fn-concat-13.xpath), [#14](inventory-fn-concat-14.xpath), [#15](inventory-fn-concat-15.xpath), [#16](inventory-fn-concat-16.xpath), [#17](inventory-fn-concat-17.xpath), [#18](inventory-fn-concat-18.xpath), [#19](inventory-fn-concat-19.xpath), [#20](inventory-fn-concat-20.xpath), [#21](inventory-fn-concat-21.xpath), [#22](inventory-fn-concat-22.xpath), [#23](inventory-fn-concat-23.xpath), [#24](inventory-fn-concat-24.xpath), [#25](inventory-fn-concat-25.xpath), [#26](inventory-fn-concat-26.xpath), [#27](inventory-fn-concat-27.xpath), [#28](inventory-fn-concat-28.xpath), [#29](inventory-fn-concat-29.xpath), [#30](inventory-fn-concat-30.xpath), [#31](inventory-fn-concat-31.xpath), [#32](inventory-fn-concat-32.xpath) | #0: 0/0, #1: 0/0, #2: 1/1, #3: 1/1, #4: 1/1, #5: 1/1, #6: 1/1, #7: 1/1, #8: 1/1, #9: 1/1, #10: 1/1, #11: 1/1, #12: 1/1, #13: 1/1, #14: 1/1, #15: 1/1, #16: 1/1, #17: 1/1, #18: 1/1, #19: 1/1, #20: 1/1, #21: 1/1, #22: 1/1, #23: 1/1, #24: 1/1, #25: 1/1, #26: 1/1, #27: 1/1, #28: 1/1, #29: 1/1, #30: 1/1, #31: 1/1, #32: 1/1 |
| `fn:contains` | [#2](inventory-fn-contains-2.xpath), [#3](inventory-fn-contains-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:contains-token` | [#2](inventory-fn-contains-token-2.xpath), [#3](inventory-fn-contains-token-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:count` | [#1](inventory-fn-count-1.xpath) | #1: 1/1 |
| `fn:current-date` | [#0](inventory-fn-current-date-0.xpath) | #0: 1/1 |
| `fn:current-dateTime` | [#0](inventory-fn-current-dateTime-0.xpath) | #0: 1/1 |
| `fn:current-time` | [#0](inventory-fn-current-time-0.xpath) | #0: 1/1 |
| `fn:data` | [#0](inventory-fn-data-0.xpath), [#1](inventory-fn-data-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:dateTime` | [#2](inventory-fn-dateTime-2.xpath) | #2: 1/1 |
| `fn:day-from-date` | [#1](inventory-fn-day-from-date-1.xpath) | #1: 1/1 |
| `fn:day-from-dateTime` | [#1](inventory-fn-day-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:days-from-duration` | [#1](inventory-fn-days-from-duration-1.xpath) | #1: 1/1 |
| `fn:deep-equal` | [#2](inventory-fn-deep-equal-2.xpath), [#3](inventory-fn-deep-equal-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:default-collation` | [#0](inventory-fn-default-collation-0.xpath) | #0: 1/1 |
| `fn:default-language` | [#0](inventory-fn-default-language-0.xpath) | #0: 1/1 |
| `fn:distinct-values` | [#1](inventory-fn-distinct-values-1.xpath), [#2](inventory-fn-distinct-values-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:doc` | [#1](inventory-fn-doc-1.xpath), [#2](inventory-fn-doc-2.xpath) | #1: 1/1, #2: 0/0 |
| `fn:doc-available` | [#1](inventory-fn-doc-available-1.xpath), [#2](inventory-fn-doc-available-2.xpath) | #1: 1/1, #2: 0/0 |
| `fn:document-uri` | [#0](inventory-fn-document-uri-0.xpath), [#1](inventory-fn-document-uri-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:element-with-id` | [#1](inventory-fn-element-with-id-1.xpath), [#2](inventory-fn-element-with-id-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:empty` | [#1](inventory-fn-empty-1.xpath) | #1: 1/1 |
| `fn:encode-for-uri` | [#1](inventory-fn-encode-for-uri-1.xpath) | #1: 1/1 |
| `fn:ends-with` | [#2](inventory-fn-ends-with-2.xpath), [#3](inventory-fn-ends-with-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:environment-variable` | [#1](inventory-fn-environment-variable-1.xpath) | #1: 1/1 |
| `fn:error` | [#0](inventory-fn-error-0.xpath), [#1](inventory-fn-error-1.xpath), [#2](inventory-fn-error-2.xpath), [#3](inventory-fn-error-3.xpath) | #0: 1/1, #1: 1/1, #2: 1/1, #3: 1/1 |
| `fn:escape-html-uri` | [#1](inventory-fn-escape-html-uri-1.xpath) | #1: 1/1 |
| `fn:exactly-one` | [#1](inventory-fn-exactly-one-1.xpath) | #1: 1/1 |
| `fn:exists` | [#1](inventory-fn-exists-1.xpath) | #1: 1/1 |
| `fn:false` | [#0](inventory-fn-false-0.xpath) | #0: 1/1 |
| `fn:filter` | [#2](inventory-fn-filter-2.xpath) | #2: 1/1 |
| `fn:floor` | [#1](inventory-fn-floor-1.xpath) | #1: 1/1 |
| `fn:fold-left` | [#3](inventory-fn-fold-left-3.xpath) | #3: 1/1 |
| `fn:fold-right` | [#3](inventory-fn-fold-right-3.xpath) | #3: 1/1 |
| `fn:for-each` | [#2](inventory-fn-for-each-2.xpath) | #2: 1/1 |
| `fn:for-each-pair` | [#3](inventory-fn-for-each-pair-3.xpath) | #3: 1/1 |
| `fn:format-date` | [#2](inventory-fn-format-date-2.xpath), [#3](inventory-fn-format-date-3.xpath), [#4](inventory-fn-format-date-4.xpath), [#5](inventory-fn-format-date-5.xpath) | #2: 1/1, #3: 0/0, #4: 0/0, #5: 1/1 |
| `fn:format-dateTime` | [#2](inventory-fn-format-dateTime-2.xpath), [#3](inventory-fn-format-dateTime-3.xpath), [#4](inventory-fn-format-dateTime-4.xpath), [#5](inventory-fn-format-dateTime-5.xpath) | #2: 1/1, #3: 0/0, #4: 0/0, #5: 1/1 |
| `fn:format-integer` | [#2](inventory-fn-format-integer-2.xpath), [#3](inventory-fn-format-integer-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:format-number` | [#2](inventory-fn-format-number-2.xpath), [#3](inventory-fn-format-number-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:format-time` | [#2](inventory-fn-format-time-2.xpath), [#3](inventory-fn-format-time-3.xpath), [#4](inventory-fn-format-time-4.xpath), [#5](inventory-fn-format-time-5.xpath) | #2: 1/1, #3: 0/0, #4: 0/0, #5: 1/1 |
| `fn:function-arity` | [#1](inventory-fn-function-arity-1.xpath) | #1: 1/1 |
| `fn:function-lookup` | [#2](inventory-fn-function-lookup-2.xpath) | #2: 1/1 |
| `fn:function-name` | [#1](inventory-fn-function-name-1.xpath) | #1: 1/1 |
| `fn:generate-id` | [#0](inventory-fn-generate-id-0.xpath), [#1](inventory-fn-generate-id-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:has-children` | [#0](inventory-fn-has-children-0.xpath), [#1](inventory-fn-has-children-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:head` | [#1](inventory-fn-head-1.xpath) | #1: 1/1 |
| `fn:hours-from-dateTime` | [#1](inventory-fn-hours-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:hours-from-duration` | [#1](inventory-fn-hours-from-duration-1.xpath) | #1: 1/1 |
| `fn:hours-from-time` | [#1](inventory-fn-hours-from-time-1.xpath) | #1: 1/1 |
| `fn:id` | [#1](inventory-fn-id-1.xpath), [#2](inventory-fn-id-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:idref` | [#1](inventory-fn-idref-1.xpath), [#2](inventory-fn-idref-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:implicit-timezone` | [#0](inventory-fn-implicit-timezone-0.xpath) | #0: 1/1 |
| `fn:in-scope-prefixes` | [#1](inventory-fn-in-scope-prefixes-1.xpath) | #1: 1/1 |
| `fn:index-of` | [#2](inventory-fn-index-of-2.xpath), [#3](inventory-fn-index-of-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:innermost` | [#1](inventory-fn-innermost-1.xpath) | #1: 1/1 |
| `fn:insert-before` | [#3](inventory-fn-insert-before-3.xpath) | #3: 1/1 |
| `fn:iri-to-uri` | [#1](inventory-fn-iri-to-uri-1.xpath) | #1: 1/1 |
| `fn:json-doc` | [#1](inventory-fn-json-doc-1.xpath), [#2](inventory-fn-json-doc-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:json-to-xml` | [#1](inventory-fn-json-to-xml-1.xpath), [#2](inventory-fn-json-to-xml-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:lang` | [#1](inventory-fn-lang-1.xpath), [#2](inventory-fn-lang-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:last` | [#0](inventory-fn-last-0.xpath) | #0: 1/1 |
| `fn:load-xquery-module` | [#1](inventory-fn-load-xquery-module-1.xpath), [#2](inventory-fn-load-xquery-module-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:local-name` | [#0](inventory-fn-local-name-0.xpath), [#1](inventory-fn-local-name-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:local-name-from-QName` | [#1](inventory-fn-local-name-from-QName-1.xpath) | #1: 1/1 |
| `fn:lower-case` | [#1](inventory-fn-lower-case-1.xpath) | #1: 1/1 |
| `fn:matches` | [#2](inventory-fn-matches-2.xpath), [#3](inventory-fn-matches-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:max` | [#1](inventory-fn-max-1.xpath), [#2](inventory-fn-max-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:min` | [#1](inventory-fn-min-1.xpath), [#2](inventory-fn-min-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:minutes-from-dateTime` | [#1](inventory-fn-minutes-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:minutes-from-duration` | [#1](inventory-fn-minutes-from-duration-1.xpath) | #1: 1/1 |
| `fn:minutes-from-time` | [#1](inventory-fn-minutes-from-time-1.xpath) | #1: 1/1 |
| `fn:month-from-date` | [#1](inventory-fn-month-from-date-1.xpath) | #1: 1/1 |
| `fn:month-from-dateTime` | [#1](inventory-fn-month-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:months-from-duration` | [#1](inventory-fn-months-from-duration-1.xpath) | #1: 1/1 |
| `fn:name` | [#0](inventory-fn-name-0.xpath), [#1](inventory-fn-name-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:namespace-uri` | [#0](inventory-fn-namespace-uri-0.xpath), [#1](inventory-fn-namespace-uri-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:namespace-uri-for-prefix` | [#2](inventory-fn-namespace-uri-for-prefix-2.xpath) | #2: 1/1 |
| `fn:namespace-uri-from-QName` | [#1](inventory-fn-namespace-uri-from-QName-1.xpath) | #1: 1/1 |
| `fn:nilled` | [#0](inventory-fn-nilled-0.xpath), [#1](inventory-fn-nilled-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:node-name` | [#0](inventory-fn-node-name-0.xpath), [#1](inventory-fn-node-name-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:normalize-space` | [#0](inventory-fn-normalize-space-0.xpath), [#1](inventory-fn-normalize-space-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:normalize-unicode` | [#1](inventory-fn-normalize-unicode-1.xpath), [#2](inventory-fn-normalize-unicode-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:not` | [#1](inventory-fn-not-1.xpath) | #1: 1/1 |
| `fn:number` | [#0](inventory-fn-number-0.xpath), [#1](inventory-fn-number-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:one-or-more` | [#1](inventory-fn-one-or-more-1.xpath) | #1: 1/1 |
| `fn:outermost` | [#1](inventory-fn-outermost-1.xpath) | #1: 1/1 |
| `fn:parse-ietf-date` | [#1](inventory-fn-parse-ietf-date-1.xpath) | #1: 1/1 |
| `fn:parse-json` | [#1](inventory-fn-parse-json-1.xpath), [#2](inventory-fn-parse-json-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:parse-xml` | [#1](inventory-fn-parse-xml-1.xpath), [#2](inventory-fn-parse-xml-2.xpath) | #1: 1/1, #2: 0/0 |
| `fn:parse-xml-fragment` | [#1](inventory-fn-parse-xml-fragment-1.xpath), [#2](inventory-fn-parse-xml-fragment-2.xpath) | #1: 1/1, #2: 0/0 |
| `fn:path` | [#0](inventory-fn-path-0.xpath), [#1](inventory-fn-path-1.xpath), [#2](inventory-fn-path-2.xpath) | #0: 1/1, #1: 1/1, #2: 0/0 |
| `fn:position` | [#0](inventory-fn-position-0.xpath) | #0: 1/1 |
| `fn:prefix-from-QName` | [#1](inventory-fn-prefix-from-QName-1.xpath) | #1: 1/1 |
| `fn:random-number-generator` | [#0](inventory-fn-random-number-generator-0.xpath), [#1](inventory-fn-random-number-generator-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:remove` | [#2](inventory-fn-remove-2.xpath) | #2: 1/1 |
| `fn:replace` | [#2](inventory-fn-replace-2.xpath), [#3](inventory-fn-replace-3.xpath), [#4](inventory-fn-replace-4.xpath) | #2: 0/0, #3: 1/1, #4: 1/1 |
| `fn:resolve-QName` | [#2](inventory-fn-resolve-QName-2.xpath) | #2: 1/1 |
| `fn:resolve-uri` | [#1](inventory-fn-resolve-uri-1.xpath), [#2](inventory-fn-resolve-uri-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:reverse` | [#1](inventory-fn-reverse-1.xpath) | #1: 1/1 |
| `fn:root` | [#0](inventory-fn-root-0.xpath), [#1](inventory-fn-root-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:round` | [#1](inventory-fn-round-1.xpath), [#2](inventory-fn-round-2.xpath), [#3](inventory-fn-round-3.xpath) | #1: 1/1, #2: 1/1, #3: 0/0 |
| `fn:round-half-to-even` | [#1](inventory-fn-round-half-to-even-1.xpath), [#2](inventory-fn-round-half-to-even-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:seconds-from-dateTime` | [#1](inventory-fn-seconds-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:seconds-from-duration` | [#1](inventory-fn-seconds-from-duration-1.xpath) | #1: 1/1 |
| `fn:seconds-from-time` | [#1](inventory-fn-seconds-from-time-1.xpath) | #1: 1/1 |
| `fn:serialize` | [#1](inventory-fn-serialize-1.xpath), [#2](inventory-fn-serialize-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:sort` | [#1](inventory-fn-sort-1.xpath), [#2](inventory-fn-sort-2.xpath), [#3](inventory-fn-sort-3.xpath) | #1: 1/1, #2: 1/1, #3: 1/1 |
| `fn:starts-with` | [#2](inventory-fn-starts-with-2.xpath), [#3](inventory-fn-starts-with-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:static-base-uri` | [#0](inventory-fn-static-base-uri-0.xpath) | #0: 1/1 |
| `fn:string` | [#0](inventory-fn-string-0.xpath), [#1](inventory-fn-string-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:string-join` | [#1](inventory-fn-string-join-1.xpath), [#2](inventory-fn-string-join-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:string-length` | [#0](inventory-fn-string-length-0.xpath), [#1](inventory-fn-string-length-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:string-to-codepoints` | [#1](inventory-fn-string-to-codepoints-1.xpath) | #1: 1/1 |
| `fn:subsequence` | [#2](inventory-fn-subsequence-2.xpath), [#3](inventory-fn-subsequence-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:substring` | [#2](inventory-fn-substring-2.xpath), [#3](inventory-fn-substring-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:substring-after` | [#2](inventory-fn-substring-after-2.xpath), [#3](inventory-fn-substring-after-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:substring-before` | [#2](inventory-fn-substring-before-2.xpath), [#3](inventory-fn-substring-before-3.xpath) | #2: 1/1, #3: 1/1 |
| `fn:sum` | [#1](inventory-fn-sum-1.xpath), [#2](inventory-fn-sum-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:tail` | [#1](inventory-fn-tail-1.xpath) | #1: 1/1 |
| `fn:timezone-from-date` | [#1](inventory-fn-timezone-from-date-1.xpath) | #1: 1/1 |
| `fn:timezone-from-dateTime` | [#1](inventory-fn-timezone-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:timezone-from-time` | [#1](inventory-fn-timezone-from-time-1.xpath) | #1: 1/1 |
| `fn:tokenize` | [#1](inventory-fn-tokenize-1.xpath), [#2](inventory-fn-tokenize-2.xpath), [#3](inventory-fn-tokenize-3.xpath) | #1: 1/1, #2: 1/1, #3: 1/1 |
| `fn:trace` | [#1](inventory-fn-trace-1.xpath), [#2](inventory-fn-trace-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:transform` | [#1](inventory-fn-transform-1.xpath) | #1: 0/0 |
| `fn:translate` | [#3](inventory-fn-translate-3.xpath) | #3: 1/1 |
| `fn:true` | [#0](inventory-fn-true-0.xpath) | #0: 1/1 |
| `fn:unordered` | [#1](inventory-fn-unordered-1.xpath) | #1: 1/1 |
| `fn:unparsed-text` | [#1](inventory-fn-unparsed-text-1.xpath), [#2](inventory-fn-unparsed-text-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:unparsed-text-available` | [#1](inventory-fn-unparsed-text-available-1.xpath), [#2](inventory-fn-unparsed-text-available-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:unparsed-text-lines` | [#1](inventory-fn-unparsed-text-lines-1.xpath), [#2](inventory-fn-unparsed-text-lines-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:upper-case` | [#1](inventory-fn-upper-case-1.xpath) | #1: 1/1 |
| `fn:uri-collection` | [#0](inventory-fn-uri-collection-0.xpath), [#1](inventory-fn-uri-collection-1.xpath) | #0: 1/1, #1: 1/1 |
| `fn:xml-to-json` | [#1](inventory-fn-xml-to-json-1.xpath), [#2](inventory-fn-xml-to-json-2.xpath) | #1: 1/1, #2: 1/1 |
| `fn:year-from-date` | [#1](inventory-fn-year-from-date-1.xpath) | #1: 1/1 |
| `fn:year-from-dateTime` | [#1](inventory-fn-year-from-dateTime-1.xpath) | #1: 1/1 |
| `fn:years-from-duration` | [#1](inventory-fn-years-from-duration-1.xpath) | #1: 1/1 |
| `fn:zero-or-one` | [#1](inventory-fn-zero-or-one-1.xpath) | #1: 1/1 |
| `map:contains` | [#2](inventory-map-contains-2.xpath) | #2: 1/1 |
| `map:entry` | [#2](inventory-map-entry-2.xpath) | #2: 1/1 |
| `map:find` | [#2](inventory-map-find-2.xpath) | #2: 1/1 |
| `map:for-each` | [#2](inventory-map-for-each-2.xpath) | #2: 1/1 |
| `map:get` | [#2](inventory-map-get-2.xpath), [#3](inventory-map-get-3.xpath) | #2: 1/1, #3: 0/0 |
| `map:keys` | [#1](inventory-map-keys-1.xpath) | #1: 1/1 |
| `map:merge` | [#1](inventory-map-merge-1.xpath), [#2](inventory-map-merge-2.xpath) | #1: 1/1, #2: 1/1 |
| `map:put` | [#3](inventory-map-put-3.xpath) | #3: 1/1 |
| `map:remove` | [#2](inventory-map-remove-2.xpath) | #2: 1/1 |
| `map:size` | [#1](inventory-map-size-1.xpath) | #1: 1/1 |
| `math:acos` | [#1](inventory-math-acos-1.xpath) | #1: 1/1 |
| `math:asin` | [#1](inventory-math-asin-1.xpath) | #1: 1/1 |
| `math:atan` | [#1](inventory-math-atan-1.xpath) | #1: 1/1 |
| `math:atan2` | [#2](inventory-math-atan2-2.xpath) | #2: 1/1 |
| `math:cos` | [#1](inventory-math-cos-1.xpath) | #1: 1/1 |
| `math:exp` | [#1](inventory-math-exp-1.xpath) | #1: 1/1 |
| `math:exp10` | [#1](inventory-math-exp10-1.xpath) | #1: 1/1 |
| `math:log` | [#1](inventory-math-log-1.xpath) | #1: 1/1 |
| `math:log10` | [#1](inventory-math-log10-1.xpath) | #1: 1/1 |
| `math:pi` | [#0](inventory-math-pi-0.xpath) | #0: 1/1 |
| `math:pow` | [#2](inventory-math-pow-2.xpath) | #2: 1/1 |
| `math:sin` | [#1](inventory-math-sin-1.xpath) | #1: 1/1 |
| `math:sqrt` | [#1](inventory-math-sqrt-1.xpath) | #1: 1/1 |
| `math:tan` | [#1](inventory-math-tan-1.xpath) | #1: 1/1 |


Profile distinction: the empty-length substring example returns an empty string in XPath 3.1 and `A` in Frozen 4.0. The empty-length subsequence example refuses `XPTY0004` in 3.1 and returns `B,C` in Frozen 4.0. Manifest baseline assertions preserve these differences; the portable business reports have identical results in both profiles.
