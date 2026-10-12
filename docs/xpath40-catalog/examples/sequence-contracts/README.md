<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak dispatch batch sequence examples">
  <br><br><h1>Dispatch Batch Exports</h1>
  <p>Full input/output, page boundaries, duplicate requests and station labels</p>
</div>

## Business case and inputs

A dispatch export receives an ordered queue. Export source positions 2 through 4, retain the page's final ID as a checkpoint, report preceding IDs as the body, and repeat the page labels for two packing stations. A separate operator request selects queue positions 5,2,2, preserving that requested order and its duplicate. Repetition here describes printed labels, not repeated shipment execution.

Complete input, embedded in the runnable files:

```xml
<dispatch><order id="ORD-101"/><order id="ORD-102"/><order id="ORD-103"/><order id="ORD-104"/><order id="ORD-105"/></dispatch>
```

order element order supplies queue order; id attributes supply text labels. The example does not sort or assign durable cursors. A mutable live queue needs its own stable ordering/checkpoint policy. The literal XML makes source order and all outputs reproducible without file IO or external variables.

## Complete Frozen 4.0 report

[Runnable expression](export-40.xpath):

```xquery
let $queue := parse-xml('<dispatch><order id="ORD-101"/><order id="ORD-102"/><order id="ORD-103"/><order id="ORD-104"/><order id="ORD-105"/></dispatch>')/dispatch/order/@id ! string(.) return let $page := slice($queue,2,4) return concat('page=',string-join($page,','),';last=',foot($page),';body=',string-join(trunk($page),','),';labels=',string-join(insert-separator(replicate($page,2),'|'),''),';requested=',string-join(items-at($queue,(5,2,2)),','))
```

slice selects inclusive positions 2–4. foot supplies the last selected ID, trunk the preceding IDs. replicate repeats the whole page twice. insert-separator places pipes between each label, and string-join emits the text report. items-at follows requested positions rather than filtering the source.

Exact output:

```text
page=ORD-102,ORD-103,ORD-104;last=ORD-104;body=ORD-102,ORD-103;labels=ORD-102|ORD-103|ORD-104|ORD-102|ORD-103|ORD-104;requested=ORD-105,ORD-102,ORD-102
```

## Complete portable 3.1 report

[Runnable comparison](export-31.xpath):

```xquery
let $queue := parse-xml('<dispatch><order id="ORD-101"/><order id="ORD-102"/><order id="ORD-103"/><order id="ORD-104"/><order id="ORD-105"/></dispatch>')/dispatch/order/@id ! string(.) return let $page := subsequence($queue,2,3) return concat('page=',string-join($page,','),';last=',$page[last()],';body=',string-join($page[position() lt last()],','),';labels=',string-join((for $station in 1 to 2 return $page),'|'),';requested=',string-join((for $position in (5,2,2) return $queue[$position]),','))
```

It returns the same output for this case. subsequence starts at 2 with length 3; last() predicates implement last/body selection. A for loop repeats the entire page and string-join inserts separators. Iterating requested positions preserves their order/duplicates. Nested let bindings use explicit return, keeping the baseline within XPath 3.1 grammar. This baseline is scoped to the illustrated forward page; it does not implement every slice direction/bound rule.

## Empty and reordered inputs

[Empty 4.0](export-empty-40.xpath) and [empty 3.1](export-empty-31.xpath) embed `<dispatch/>` and both return:

```text
page=;last=;body=;labels=;requested=
```

Both [reordered 4.0](export-reordered-40.xpath) and [reordered 3.1](export-reordered-31.xpath) embed:

```xml
<dispatch><order id="ORD-105"/><order id="ORD-104"/><order id="ORD-103"/><order id="ORD-102"/><order id="ORD-101"/></dispatch>
```

Output:

```text
page=ORD-104,ORD-103,ORD-102;last=ORD-102;body=ORD-104,ORD-103;labels=ORD-104|ORD-103|ORD-102|ORD-104|ORD-103|ORD-102;requested=ORD-101,ORD-104,ORD-104
```

The changed output follows the changed source order, demonstrating that positional selection does not normalize/sort IDs. Empty fields remain visible; the example does not impose required-field or page-availability business validation.

## Keywords, defaults and typed sources

Five functions lack keyword metadata and refuse 4.0/experimental keyword calls; slice supports keywords and defaults. 3.1 keyword grammar refuses XPST0003 for all six. Each published keyword expression runs in all three explicit modes. Missing slice input or unknown keyword refuses XPST0017.

Empty or zero slice bounds use defaults; negative bounds count from the end; zero step infers direction; explicit negative/positive stride controls traversal. Integer-typed bounds are the supported documented input. Integral decimals are accepted in tested items-at calls but rejected by replicate and slice. XML position nodes and string bounds have explicit refusals; precise coercion remains a review item.

foot/trunk and separator operations accept generic items, with map examples in the inventory. Replication's node-identity probe returns true; no nodes are cloned. Runtime materialization means the bounded examples do not establish streaming or allocation performance.

## Complete fixture inventory

| Runnable case | Current result/error |
|---|---|
| [replicate-keyword-31](replicate-keyword-31.xpath) | `XPST0003` |
| [replicate-keyword-40](replicate-keyword-40.xpath) | `XPST0017` |
| [replicate-keyword-experimental](replicate-keyword-experimental.xpath) | `XPST0017` |
| [items-at-keyword-31](items-at-keyword-31.xpath) | `XPST0003` |
| [items-at-keyword-40](items-at-keyword-40.xpath) | `XPST0017` |
| [items-at-keyword-experimental](items-at-keyword-experimental.xpath) | `XPST0017` |
| [foot-keyword-31](foot-keyword-31.xpath) | `XPST0003` |
| [foot-keyword-40](foot-keyword-40.xpath) | `XPST0017` |
| [foot-keyword-experimental](foot-keyword-experimental.xpath) | `XPST0017` |
| [trunk-keyword-31](trunk-keyword-31.xpath) | `XPST0003` |
| [trunk-keyword-40](trunk-keyword-40.xpath) | `XPST0017` |
| [trunk-keyword-experimental](trunk-keyword-experimental.xpath) | `XPST0017` |
| [insert-separator-keyword-31](insert-separator-keyword-31.xpath) | `XPST0003` |
| [insert-separator-keyword-40](insert-separator-keyword-40.xpath) | `XPST0017` |
| [insert-separator-keyword-experimental](insert-separator-keyword-experimental.xpath) | `XPST0017` |
| [slice-keyword-31](slice-keyword-31.xpath) | `XPST0003` |
| [slice-keyword-40](slice-keyword-40.xpath) | `C,B,A` |
| [slice-keyword-experimental](slice-keyword-experimental.xpath) | `C,B,A` |
| [replicate-page](replicate-page.xpath) | `A,B,A,B` |
| [replicate-zero](replicate-zero.xpath) | `0` |
| [replicate-empty](replicate-empty.xpath) | `0` |
| [replicate-negative](replicate-negative.xpath) | `XPTY0004` |
| [replicate-fraction](replicate-fraction.xpath) | `XPTY0004` |
| [replicate-integral-decimal](replicate-integral-decimal.xpath) | `XPTY0004` |
| [replicate-node-identity](replicate-node-identity.xpath) | `true` |
| [items-request-order](items-request-order.xpath) | `C,A,A` |
| [items-out-of-range](items-out-of-range.xpath) | `B` |
| [items-empty-positions](items-empty-positions.xpath) | `0` |
| [items-empty-source](items-empty-source.xpath) | `0` |
| [items-fraction](items-fraction.xpath) | `XPTY0004` |
| [items-string](items-string.xpath) | `XPTY0004` |
| [items-integral-decimal](items-integral-decimal.xpath) | `B` |
| [items-node-position](items-node-position.xpath) | `XPTY0004` |
| [foot-empty](foot-empty.xpath) | `0` |
| [foot-singleton](foot-singleton.xpath) | `A` |
| [foot-map](foot-map.xpath) | `B` |
| [trunk-empty](trunk-empty.xpath) | `0` |
| [trunk-singleton](trunk-singleton.xpath) | `0` |
| [trunk-maps](trunk-maps.xpath) | `A,B` |
| [separator-multi](separator-multi.xpath) | `A\|/B\|/C` |
| [separator-empty-input](separator-empty-input.xpath) | `0` |
| [separator-singleton](separator-singleton.xpath) | `A` |
| [separator-empty](separator-empty.xpath) | `A,B` |
| [slice-default](slice-default.xpath) | `A,B,C,D` |
| [slice-empty-defaults](slice-empty-defaults.xpath) | `A,B,C,D` |
| [slice-zero-defaults](slice-zero-defaults.xpath) | `A,B,C,D` |
| [slice-negative-bounds](slice-negative-bounds.xpath) | `B,C,D` |
| [slice-inferred-reverse](slice-inferred-reverse.xpath) | `D,C,B` |
| [slice-negative-step](slice-negative-step.xpath) | `D,C,B,A` |
| [slice-stride](slice-stride.xpath) | `A,C` |
| [slice-mismatched-direction](slice-mismatched-direction.xpath) | `0` |
| [slice-empty-source](slice-empty-source.xpath) | `0` |
| [slice-string-bound](slice-string-bound.xpath) | `XPTY0004` |
| [slice-decimal-bound](slice-decimal-bound.xpath) | `XPTY0004` |
| [slice-fraction-bound](slice-fraction-bound.xpath) | `XPTY0004` |
| [slice-node-bound](slice-node-bound.xpath) | `XPTY0004` |
| [slice-reordered-keywords](slice-reordered-keywords.xpath) | `B,C` |
| [slice-missing-required](slice-missing-required.xpath) | `XPST0017` |
| [slice-unknown-keyword](slice-unknown-keyword.xpath) | `XPST0017` |
| [export-40](export-40.xpath) | `page=ORD-102,ORD-103,ORD-104;last=ORD-104;body=ORD-102,ORD-103;labels=ORD-102\|ORD-103\|ORD-104\|ORD-102\|ORD-103\|ORD-104;requested=ORD-105,ORD-102,ORD-102` |
| [export-31](export-31.xpath) | `page=ORD-102,ORD-103,ORD-104;last=ORD-104;body=ORD-102,ORD-103;labels=ORD-102\|ORD-103\|ORD-104\|ORD-102\|ORD-103\|ORD-104;requested=ORD-105,ORD-102,ORD-102` |
| [export-empty-40](export-empty-40.xpath) | `page=;last=;body=;labels=;requested=` |
| [export-empty-31](export-empty-31.xpath) | `page=;last=;body=;labels=;requested=` |
| [export-reordered-40](export-reordered-40.xpath) | `page=ORD-104,ORD-103,ORD-102;last=ORD-102;body=ORD-104,ORD-103;labels=ORD-104\|ORD-103\|ORD-102\|ORD-104\|ORD-103\|ORD-102;requested=ORD-101,ORD-104,ORD-104` |
| [export-reordered-31](export-reordered-31.xpath) | `page=ORD-104,ORD-103,ORD-102;last=ORD-102;body=ORD-104,ORD-103;labels=ORD-104\|ORD-103\|ORD-102\|ORD-104\|ORD-103\|ORD-102;requested=ORD-101,ORD-104,ORD-104` |

## Run and interpret

Run `dotnet test tests/Bosak.Xslt.Tests -c Release --filter FullyQualifiedName~CatalogExampleTests`. The existing harness executes exact files through public APIs with fresh contexts, checking outputs/errors and applicable compatibility boundaries. No engine/API/harness change is included.

Read [the sequence contract review](../../SEQUENCE-CONTRACT-REVIEW.md), [raw signature matrix](../../audit-signature-contracts.json) and [family reference](../../functions/order-sequence-selection.md). These 65 checks supply representative contracts; D2/REQ-126 remain incomplete and version remains 1.1.2.
