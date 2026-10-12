<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak ordering and duplicate contract review">
  <br><br><h1>Supplier Offer Ranking: Detailed Example</h1>
  <p>Existing function defaults, comparison and duplicate policies</p>
</div>

## Business case and input contract

A purchasing diagnostic ranks supplier offers by price, builds an array view of the same ranking, reports the lowest/highest price and counts offers for product P. IDs A, B and C are unique offer identities; product P intentionally repeats. Prices are nonnegative integer minor units in one common currency. Validate these assumptions before production use; mixed currencies, malformed numbers, duplicate IDs and business authorization are outside this fixture.

The explicit ordering key is (integer price, string ID). Price is primary and ID breaks ties. This gives B,C,A for either normal or reordered input. Using price alone would preserve source order for ties rather than produce a stable business tie-break. Codepoint ID comparison is suitable for these uppercase ASCII identifiers; other identifier/collation rules need an explicit policy.

map:merge uses duplicates=combine to retain all offer IDs for each product. Omitting that option would keep only the first offer for a duplicate product. The report counts P offers rather than displaying combined IDs, since their sequence preserves source order. A missing product has count zero; an empty-valued key and an absent key are different in general. These fixtures contain nonempty IDs only.

min/max summarize the validated numeric prices; they do not return every cheapest/most expensive offer. The empty report says range=none explicitly. Both sorting calls pass a valid unary callback, and map:merge passes an actual options map. This portable 3.1 solution also runs at Frozen 4.0 without relying on unimplemented default-on-empty forms.

## Elements and functions

| Construct | Role |
|---|---|
| offers/offer and @id/@product/@price | Source row, stable offer identity, grouping code and price. |
| parse-xml and let | Build the complete local fixture and bind shared calculations. |
| fn:sort and inline unary function | Sort original offer nodes by numeric price plus ID; retain node identity. |
| array construction and array:sort | One offer node per member, sorted by the same policy. |
| xs:integer and string | Explicitly type comparison keys and diagnostic fields. |
| map:entry and map:merge | Group offer identities by product using explicit combine. |
| min/max, exists and conditional | Summarize a populated price range and handle empty input. |
| count, array:size, one-based calls, for | Count rows/offers and visit sorted array members safely. |
| simple map !, lookup ?P, string-join, concat | Preserve sequence ranking while projecting IDs and assemble the report. |

The sorted node projection uses $rank ! string(@id). Replacing it with $rank/@id would restore document order, because a path returns its nodes in document order. Array row access is one-based. These are local examples with no external resource, application variable or current clock.


## Normal input and output

Input XML:

```xml
<offers><offer id="A" product="P" price="10"/><offer id="B" product="P" price="7"/><offer id="C" product="Q" price="7"/></offers>
```

XPath 3.1 ([executable fixture](report-normal-31.xpath)):

```xquery
let $d := parse-xml('<offers><offer id="A" product="P" price="10"/><offer id="B" product="P" price="7"/><offer id="C" product="Q" price="7"/></offers>'),
    $offers := $d/offers/offer,
    $rank := sort($offers,(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $rows := array:sort(array{$offers},(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $index := map:merge($offers ! map:entry(string(@product),string(@id)),map{'duplicates':'combine'}),
    $prices := $offers/@price ! xs:integer(.)
return concat('offers=',count($offers),
  ';rank=',string-join($rank ! string(@id),','),
  ';rows=',string-join(for $i in 1 to array:size($rows) return string($rows($i)/@id),','),
  ';range=',if (exists($prices)) then concat(min($prices),'..',max($prices)) else 'none',
  ';P=',count($index?P))
```

XPath Frozen 4.0 ([executable fixture](report-normal-40.xpath)):

```xquery
let $d := parse-xml('<offers><offer id="A" product="P" price="10"/><offer id="B" product="P" price="7"/><offer id="C" product="Q" price="7"/></offers>'),
    $offers := $d/offers/offer,
    $rank := sort($offers,(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $rows := array:sort(array{$offers},(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $index := map:merge($offers ! map:entry(string(@product),string(@id)),map{'duplicates':'combine'}),
    $prices := $offers/@price ! xs:integer(.)
return concat('offers=',count($offers),
  ';rank=',string-join($rank ! string(@id),','),
  ';rows=',string-join(for $i in 1 to array:size($rows) return string($rows($i)/@id),','),
  ';range=',if (exists($prices)) then concat(min($prices),'..',max($prices)) else 'none',
  ';P=',count($index?P))
```

Exact output in both profiles:

```text
offers=3;rank=B,C,A;rows=B,C,A;range=7..10;P=2
```


## Empty input and output

Input XML:

```xml
<offers/>
```

XPath 3.1 ([executable fixture](report-empty-31.xpath)):

```xquery
let $d := parse-xml('<offers/>'),
    $offers := $d/offers/offer,
    $rank := sort($offers,(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $rows := array:sort(array{$offers},(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $index := map:merge($offers ! map:entry(string(@product),string(@id)),map{'duplicates':'combine'}),
    $prices := $offers/@price ! xs:integer(.)
return concat('offers=',count($offers),
  ';rank=',string-join($rank ! string(@id),','),
  ';rows=',string-join(for $i in 1 to array:size($rows) return string($rows($i)/@id),','),
  ';range=',if (exists($prices)) then concat(min($prices),'..',max($prices)) else 'none',
  ';P=',count($index?P))
```

XPath Frozen 4.0 ([executable fixture](report-empty-40.xpath)):

```xquery
let $d := parse-xml('<offers/>'),
    $offers := $d/offers/offer,
    $rank := sort($offers,(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $rows := array:sort(array{$offers},(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $index := map:merge($offers ! map:entry(string(@product),string(@id)),map{'duplicates':'combine'}),
    $prices := $offers/@price ! xs:integer(.)
return concat('offers=',count($offers),
  ';rank=',string-join($rank ! string(@id),','),
  ';rows=',string-join(for $i in 1 to array:size($rows) return string($rows($i)/@id),','),
  ';range=',if (exists($prices)) then concat(min($prices),'..',max($prices)) else 'none',
  ';P=',count($index?P))
```

Exact output in both profiles:

```text
offers=0;rank=;rows=;range=none;P=0
```


## Reordered input and output

Input XML:

```xml
<offers><offer id="C" product="Q" price="7"/><offer id="B" product="P" price="7"/><offer id="A" product="P" price="10"/></offers>
```

XPath 3.1 ([executable fixture](report-reordered-31.xpath)):

```xquery
let $d := parse-xml('<offers><offer id="C" product="Q" price="7"/><offer id="B" product="P" price="7"/><offer id="A" product="P" price="10"/></offers>'),
    $offers := $d/offers/offer,
    $rank := sort($offers,(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $rows := array:sort(array{$offers},(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $index := map:merge($offers ! map:entry(string(@product),string(@id)),map{'duplicates':'combine'}),
    $prices := $offers/@price ! xs:integer(.)
return concat('offers=',count($offers),
  ';rank=',string-join($rank ! string(@id),','),
  ';rows=',string-join(for $i in 1 to array:size($rows) return string($rows($i)/@id),','),
  ';range=',if (exists($prices)) then concat(min($prices),'..',max($prices)) else 'none',
  ';P=',count($index?P))
```

XPath Frozen 4.0 ([executable fixture](report-reordered-40.xpath)):

```xquery
let $d := parse-xml('<offers><offer id="C" product="Q" price="7"/><offer id="B" product="P" price="7"/><offer id="A" product="P" price="10"/></offers>'),
    $offers := $d/offers/offer,
    $rank := sort($offers,(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $rows := array:sort(array{$offers},(),function($o){(xs:integer($o/@price),string($o/@id))}),
    $index := map:merge($offers ! map:entry(string(@product),string(@id)),map{'duplicates':'combine'}),
    $prices := $offers/@price ! xs:integer(.)
return concat('offers=',count($offers),
  ';rank=',string-join($rank ! string(@id),','),
  ';rows=',string-join(for $i in 1 to array:size($rows) return string($rows($i)/@id),','),
  ';range=',if (exists($prices)) then concat(min($prices),'..',max($prices)) else 'none',
  ';P=',count($index?P))
```

Exact output in both profiles:

```text
offers=3;rank=B,C,A;rows=B,C,A;range=7..10;P=2
```


## Focused evidence index

These checks record current behavior, including gaps. Passing a refusal fixture does not certify pinned conformance. “()” is the probe representation of an empty XDM result; a string-join projection of empty values yields an empty string. Mode 31 keyword checks refuse XPST0003; successful 4.0 cases also carry independently probed 3.1 baseline assertions. min/max #3 and key/input labels are invalid in the pinned draft, not proposed overloads.

| Case | Mode | Current result or error |
|---|---|---|
| [order-contract-sort-basic](sort-basic.xpath) | 40 | 1,2,3 |
| [order-contract-sort-empty](sort-empty.xpath) | 40 |  |
| [order-contract-sort-empty-collation](sort-empty-collation.xpath) | 40 | A,a,b |
| [order-contract-sort-codepoint](sort-codepoint.xpath) | 40 | A,a,b |
| [order-contract-sort-case-insensitive](sort-case-insensitive.xpath) | 40 | A,a,b |
| [order-contract-sort-bad-collation](sort-bad-collation.xpath) | 40 | a |
| [order-contract-sort-mixed](sort-mixed.xpath) | 40 | XPTY0004 |
| [order-contract-sort-nan](sort-nan.xpath) | 40 | 2,NaN,1 |
| [order-contract-sort-untyped](sort-untyped.xpath) | 40 | 1,3 |
| [order-contract-sort-array-atomization](sort-array-atomization.xpath) | 40 | FOTY0014 |
| [order-contract-sort-empty-key](sort-empty-key.xpath) | 40 | XPTY0004 |
| [order-contract-sort-explicit-key](sort-explicit-key.xpath) | 40 | 3,2,1 |
| [order-contract-sort-multi-key](sort-multi-key.xpath) | 40 | 1,2,3 |
| [order-contract-sort-bad-key](sort-bad-key.xpath) | 40 | XPTY0004 |
| [order-contract-sort-empty-bad-key](sort-empty-bad-key.xpath) | 40 |  |
| [order-contract-sort-wrong-arity](sort-wrong-arity.xpath) | 40 | XPTY0004 |
| [order-contract-sort-keyword](sort-keyword.xpath) | 40 | 1,2,3 |
| [order-contract-sort-lookup3](sort-lookup3.xpath) | 40 | 1 |
| [order-contract-sort-reference3](sort-reference3.xpath) | 40 | 3,2,1 |
| [order-contract-min-basic](min-basic.xpath) | 40 | 1 |
| [order-contract-min-empty](min-empty.xpath) | 40 |  |
| [order-contract-min-empty-collation](min-empty-collation.xpath) | 40 | A |
| [order-contract-min-codepoint](min-codepoint.xpath) | 40 | A |
| [order-contract-min-case-insensitive](min-case-insensitive.xpath) | 40 | A |
| [order-contract-min-bad-collation](min-bad-collation.xpath) | 40 | FOCH0002 |
| [order-contract-min-mixed](min-mixed.xpath) | 40 | FORG0006 |
| [order-contract-min-nan](min-nan.xpath) | 40 | NaN |
| [order-contract-min-untyped](min-untyped.xpath) | 40 | 1 |
| [order-contract-min-array-atomization](min-array-atomization.xpath) | 40 | 1 |
| [order-contract-min-keyword](min-keyword.xpath) | 40 | XPST0017 |
| [order-contract-min-lookup3](min-lookup3.xpath) | 40 | 0 |
| [order-contract-max-basic](max-basic.xpath) | 40 | 3 |
| [order-contract-max-empty](max-empty.xpath) | 40 |  |
| [order-contract-max-empty-collation](max-empty-collation.xpath) | 40 | b |
| [order-contract-max-codepoint](max-codepoint.xpath) | 40 | b |
| [order-contract-max-case-insensitive](max-case-insensitive.xpath) | 40 | b |
| [order-contract-max-bad-collation](max-bad-collation.xpath) | 40 | FOCH0002 |
| [order-contract-max-mixed](max-mixed.xpath) | 40 | FORG0006 |
| [order-contract-max-nan](max-nan.xpath) | 40 | NaN |
| [order-contract-max-untyped](max-untyped.xpath) | 40 | 3 |
| [order-contract-max-array-atomization](max-array-atomization.xpath) | 40 | 3 |
| [order-contract-max-keyword](max-keyword.xpath) | 40 | XPST0017 |
| [order-contract-max-lookup3](max-lookup3.xpath) | 40 | 0 |
| [order-contract-array-sort-basic](array-sort-basic.xpath) | 40 | [1] / [2] / [3] |
| [order-contract-array-sort-empty](array-sort-empty.xpath) | 40 |  |
| [order-contract-array-sort-members](array-sort-members.xpath) | 40 | [] / [2] / [3,1] |
| [order-contract-array-sort-empty-collation](array-sort-empty-collation.xpath) | 40 | [A] / [a] / [b] |
| [order-contract-array-sort-explicit-key](array-sort-explicit-key.xpath) | 40 | [3] / [2] / [1] |
| [order-contract-array-sort-empty-key](array-sort-empty-key.xpath) | 40 | XPTY0004 |
| [order-contract-array-sort-bad-key](array-sort-bad-key.xpath) | 40 | XPTY0004 |
| [order-contract-array-sort-empty-bad-key](array-sort-empty-bad-key.xpath) | 40 |  |
| [order-contract-array-sort-wrong-arity](array-sort-wrong-arity.xpath) | 40 | XPTY0004 |
| [order-contract-array-sort-bad-collation](array-sort-bad-collation.xpath) | 40 | [a] |
| [order-contract-array-sort-multi-key](array-sort-multi-key.xpath) | 40 | [1] / [2] / [3] |
| [order-contract-array-sort-keyword](array-sort-keyword.xpath) | 40 | 1,2,3 |
| [order-contract-array-sort-reference](array-sort-reference.xpath) | 40 | 3,2,1 |
| [order-contract-merge-basic](merge-basic.xpath) | 40 | size=2;A=1;present=true |
| [order-contract-merge-empty](merge-empty.xpath) | 40 | size=0;A=;present=false |
| [order-contract-merge-empty-options](merge-empty-options.xpath) | 40 | XPTY0004 |
| [order-contract-merge-empty-input-options](merge-empty-input-options.xpath) | 40 | XPTY0004 |
| [order-contract-merge-use-first](merge-use-first.xpath) | 40 | size=1;A=1;present=true |
| [order-contract-merge-use-last](merge-use-last.xpath) | 40 | size=1;A=2;present=true |
| [order-contract-merge-combine](merge-combine.xpath) | 40 | size=1;A=1,2;present=true |
| [order-contract-merge-reject](merge-reject.xpath) | 40 | FOJS0003 |
| [order-contract-merge-bad-option](merge-bad-option.xpath) | 40 | FOJS0005 |
| [order-contract-merge-bad-option-type](merge-bad-option-type.xpath) | 40 | FOJS0005 |
| [order-contract-merge-unknown-option](merge-unknown-option.xpath) | 40 | size=1;A=1;present=true |
| [order-contract-merge-bad-input](merge-bad-input.xpath) | 40 | size=1;A=1;present=true |
| [order-contract-merge-empty-value](merge-empty-value.xpath) | 40 | size=1;A=;present=true |
| [order-contract-merge-keyword](merge-keyword.xpath) | 40 | 1 |
| [order-contract-merge-reference](merge-reference.xpath) | 40 | 2 |
| [order-contract-min-keyword-values](min-keyword-values.xpath) | 40 | 1 |
| [order-contract-min-key-not-a-parameter](min-key-not-a-parameter.xpath) | 40 | XPST0017 |
| [order-contract-min-reference2](min-reference2.xpath) | 40 | 1 |
| [order-contract-min-numeric-node](min-numeric-node.xpath) | 40 | 7 |
| [order-contract-min-invalid-untyped](min-invalid-untyped.xpath) | 40 | FORG0001 |
| [order-contract-min-empty-bad-collation](min-empty-bad-collation.xpath) | 40 | () |
| [order-contract-max-keyword-values](max-keyword-values.xpath) | 40 | 3 |
| [order-contract-max-key-not-a-parameter](max-key-not-a-parameter.xpath) | 40 | XPST0017 |
| [order-contract-max-reference2](max-reference2.xpath) | 40 | 3 |
| [order-contract-max-numeric-node](max-numeric-node.xpath) | 40 | 10 |
| [order-contract-max-invalid-untyped](max-invalid-untyped.xpath) | 40 | FORG0001 |
| [order-contract-max-empty-bad-collation](max-empty-bad-collation.xpath) | 40 | () |
| [order-contract-sort-bad-collation-two](sort-bad-collation-two.xpath) | 40 | 2 |
| [order-contract-sort-empty-key-empty-input](sort-empty-key-empty-input.xpath) | 40 | 0 |
| [order-contract-array-sort-bad-collation-two](array-sort-bad-collation-two.xpath) | 40 | 2 |
| [order-contract-array-sort-empty-key-empty-input](array-sort-empty-key-empty-input.xpath) | 40 | 0 |
| [order-contract-sort-case-order](sort-case-order.xpath) | 40 | a,B |
| [order-contract-sort-codepoint-order](sort-codepoint-order.xpath) | 40 | B,a |
| [order-contract-min-case-order](min-case-order.xpath) | 40 | a |
| [order-contract-min-codepoint-order](min-codepoint-order.xpath) | 40 | B |
| [order-contract-max-case-order](max-case-order.xpath) | 40 | B |
| [order-contract-max-codepoint-order](max-codepoint-order.xpath) | 40 | a |
| [order-contract-sort-bad-collation-strings](sort-bad-collation-strings.xpath) | 40 | a,b |
| [order-contract-array-sort-bad-collation-strings](array-sort-bad-collation-strings.xpath) | 40 | a,b |
| [order-contract-sort-empty-key-results](sort-empty-key-results.xpath) | 40 | B,A,C |
| [order-contract-sort-two-argument-callback](sort-two-argument-callback.xpath) | 40 | XPTY0004 |
| [order-contract-min-decimal-result-type](min-decimal-result-type.xpath) | 40 | true |
| [order-contract-max-decimal-result-type](max-decimal-result-type.xpath) | 40 | true |
| [order-contract-sort-stable-ties](sort-stable-ties.xpath) | 40 | B,A,C |
| [order-contract-sort-nan-first](sort-nan-first.xpath) | 40 | 1,2,NaN |
| [order-contract-sort-node-identity](sort-node-identity.xpath) | 40 | true |
| [order-contract-merge-empty-map-options](merge-empty-map-options.xpath) | 40 | 0 |
| [order-contract-merge-reject-without-duplicate](merge-reject-without-duplicate.xpath) | 40 | 1 |
| [order-contract-merge-combine-empty-values](merge-combine-empty-values.xpath) | 40 | true:0 |
| [order-contract-merge-option-many](merge-option-many.xpath) | 40 | 1 |
| [order-contract-merge-option-empty](merge-option-empty.xpath) | 40 | FOJS0005 |
| [order-contract-merge-numeric-key](merge-numeric-key.xpath) | 40 | first,second |
| [order-contract-sort-keyword-mode-31](sort-keyword-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-sort-keyword-mode-experimental](sort-keyword-mode-experimental.xpath) | experimental | 1,2,3 |
| [order-contract-min-keyword-mode-31](min-keyword-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-min-keyword-mode-experimental](min-keyword-mode-experimental.xpath) | experimental | XPST0017 |
| [order-contract-max-keyword-mode-31](max-keyword-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-max-keyword-mode-experimental](max-keyword-mode-experimental.xpath) | experimental | XPST0017 |
| [order-contract-array-sort-keyword-mode-31](array-sort-keyword-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-array-sort-keyword-mode-experimental](array-sort-keyword-mode-experimental.xpath) | experimental | 1,2,3 |
| [order-contract-merge-keyword-mode-31](merge-keyword-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-merge-keyword-mode-experimental](merge-keyword-mode-experimental.xpath) | experimental | 1 |
| [order-contract-min-keyword-values-mode-31](min-keyword-values-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-min-keyword-values-mode-experimental](min-keyword-values-mode-experimental.xpath) | experimental | 1 |
| [order-contract-min-key-not-a-parameter-mode-31](min-key-not-a-parameter-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-min-key-not-a-parameter-mode-experimental](min-key-not-a-parameter-mode-experimental.xpath) | experimental | XPST0017 |
| [order-contract-max-keyword-values-mode-31](max-keyword-values-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-max-keyword-values-mode-experimental](max-keyword-values-mode-experimental.xpath) | experimental | 3 |
| [order-contract-max-key-not-a-parameter-mode-31](max-key-not-a-parameter-mode-31.xpath) | 31 | XPST0003 |
| [order-contract-max-key-not-a-parameter-mode-experimental](max-key-not-a-parameter-mode-experimental.xpath) | experimental | XPST0017 |


## Limits and references

This is a diagnostic report over validated input, not a purchasing decision or full currency/quantity model. Normal and reordered inputs have identical rankings and ranges because the tie-break uses IDs. Empty input yields no rank or rows and no invented numeric bound. The review does not test every comparator type, numeric promotion, map option, host collation or higher-order conversion. See the [five-family contract review](../../ORDER-DEFAULT-CONTRACT-REVIEW.md), [raw evidence](../../audit-order-default-contracts.json), [dated W3C source](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/function-catalog.xml) and [pinned QT4CG source](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml).
