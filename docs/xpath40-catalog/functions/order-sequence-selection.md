<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Order-export sequence selection">
  <br><br><h1>Order-export sequence selection</h1>
  <p>Frozen 4.0 — verified reference contracts</p>
</div>

## 1. Name and Classification

**Order-export sequence selection — Frozen 4.0.** Shipped in [Bosak PR #82](https://github.com/Fytala-Charles/Bosak/pull/82) at immutable revision [`40ebaa79`](https://github.com/Fytala-Charles/Bosak/commit/40ebaa79032b17fc45d6d3bb3e58cb00e2fda742).

Functions: [fn:replicate](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-replicate), [fn:slice](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-slice), [fn:items-at](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-items-at), [fn:foot](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-foot), [fn:trunk](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-trunk), [fn:insert-separator](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-insert-separator). Definitions reviewed 2026-10-11 against immutable [QT4CG function source `a014e1b4`](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml). This is a review pin, not evidence of the historical adopted snapshot, which remains unavailable.

## 2. Intent

Export an inclusive page from an ordered dispatch queue, retain its final record as a boundary marker, and generate one label run for each of two packing stations. A separate selection probe follows an operator’s requested positions, including repeated positions.

## 3. Also Known As

Exact spellings are `fn:replicate`, `fn:slice`, `fn:items-at`, `fn:foot`, `fn:trunk` and `fn:insert-separator`. Unprefixed names in the examples use the default function namespace. No aliases are introduced.

## 4. History / Supersedes

The main XPath 3.1 comparison uses `subsequence($queue,2,3)`, predicates with `last()`, a repeated `for` result and `string-join`. The requested-position comparison iterates positions and selects `$queue[$p]`. These are equivalents for the illustrated data; `subsequence` takes a length, while `slice` takes an inclusive end and can reverse or stride.

## 5. Motivation

Queue pagination, final-record checkpoints and requested label order are different operations. `items-at` retains requested-position order and duplicates; a filter such as `$queue[position()=(5,2,2)]` would instead retain queue order and omit the repeated selection.

## 6. Language Alignment

XPath uses one-based positions and inclusive slice ends. Empty/zero bounds mean defaults, not JavaScript’s zero-based origin. Negative bounds count from the end. An omitted/zero step infers direction: `slice($queue,4,2)` reverses those positions. `replicate` repeats the entire input sequence, not each item in turn. `insert-separator` inserts an arbitrary sequence between adjacent items; it is not itself a string serializer.

## 7. Applicability

Use after a stable business ordering is established. Positional offsets are not durable pagination cursors if concurrent inserts can change the queue. `replicate` creates repeated references for nodes rather than copying trees; prevent duplicate shipment execution when repeated labels are only presentation data.

## 8. Structure

```xquery
slice($queue, $start, $end, $step)
items-at($queue, $positions)
foot($page)
trunk($page)
replicate($page, $count)
insert-separator($labels, $separator)
```

Positional and shorter callback forms are exercised by the exact files below. This entry covers the illustrated contracts rather than every arity or option.

## 9. Participants

Existing `FunctionLibrary` frozen registrations select these names only at `XPathCompatibility.XPath40` or higher. The [manifest harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) compiles each file at its declared mode and checks default XPath31 refusal for successful 4.0 forms.

## 10. Collaborations

The [routing chapter](../worked-examples/order-routing/README.md) preserves dispatch arrival order and splits it into consecutive route/capacity partitions. Selection is not partitioning: it chooses positions without assigning all records to batches.

## 11. Consequences

Out-of-range `items-at` positions are ignored; duplicate requested positions remain duplicated. Fractional positions fail XPTY0004. `foot(())` and `trunk` of an empty/singleton sequence are empty. `replicate` of empty input or with count zero is empty; negative count fails XPTY0004. A separator can contain several items, and no separator is inserted for empty or singleton input. Negative-step slices reverse traversal, while a mismatched explicit direction yields no items.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Whole-sequence repetition and separator insertion; One-based positions, duplicates, inclusive/negative/default slice bounds; Empty/singleton input, negative counts and invalid positions. **Boundaries:** Integral-decimal tolerance is not the declared integer-position contract; repeat counts must remain bounded. Follow-up groups: F01, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Implementation: [FunctionLibrary.cs](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs); [named unit tests](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [manifest](../examples/cases.json). Names refuse XPST0017 in default XPath31; the comparisons remain valid at XPath31. Source/package version stays 1.1.2. This reference work changes no engine code.

**Supplementary contract review (2026-10-12):** [Six-family review](../SEQUENCE-CONTRACT-REVIEW.md) against F&O e7ee4ab8 and [detailed dispatch-batch README](../examples/sequence-contracts/README.md) supply 65 checks, explicit keyword modes, defaults/empty/direction/types, node identity and complete normal/empty/reordered reports in 3.1/4.0. Five keyword families remain absent; slice keywords work in the tested forms. items-at tolerates integral decimal positions; replicate/slice reject them. Untyped-node positions refuse; precise coercion remains an explicit follow-up, not a conformance claim. Historical delivery/adopted-pin limits are unchanged.

## 13. Sample Code

Input is declared inside each expression: reduced order/product records, no document focus, external variables or resource access. Namespace prefixes `map` and `array` use the standard public API bindings. Outputs are exact text projections of XDM results, not serialized supplier or ERP formats.

**Main — mode 40:** [main.xpath](../examples/order-sequence-selection/main.xpath)

```xquery
(: Export positions 2 through 4; repeat the label set for two packing stations. :)
let $queue := ('ORD-101','ORD-102','ORD-103','ORD-104','ORD-105'),
    $page := slice($queue, 2, 4),
    $labels := replicate($page, 2)
return concat('page=', string-join($page, ','), ';last=', foot($page), ';body=', string-join(trunk($page), ','), ';labels=', string-join(insert-separator($labels, '|'), ''))
```

Expected text:

```text
page=ORD-102,ORD-103,ORD-104;last=ORD-104;body=ORD-102,ORD-103;labels=ORD-102|ORD-103|ORD-104|ORD-102|ORD-103|ORD-104
```

**Baseline — mode 31:** [baseline.xpath](../examples/order-sequence-selection/baseline.xpath)

```xquery
let $queue := ('ORD-101','ORD-102','ORD-103','ORD-104','ORD-105'), $page := subsequence($queue, 2, 3),
    $labels := for $station in 1 to 2 return $page
return concat('page=', string-join($page, ','), ';last=', $page[last()], ';body=', string-join($page[position() lt last()], ','), ';labels=', string-join($labels, '|'))
```

Expected text:

```text
page=ORD-102,ORD-103,ORD-104;last=ORD-104;body=ORD-102,ORD-103;labels=ORD-102|ORD-103|ORD-104|ORD-102|ORD-103|ORD-104
```

**Edge — mode 40:** [edge.xpath](../examples/order-sequence-selection/edge.xpath)

```xquery
concat(count(slice((),2,4)), '/', count(replicate((),2)), '/', count(foot(())), '/', count(trunk(())), '/', count(insert-separator((), '|')), '/', count(items-at((),1)))
```

Expected text:

```text
0/0/0/0/0/0
```

**Additional executable contracts:**

| Case | Mode | Expected text or error |
|---|---|---|
| [requested-positions](../examples/order-sequence-selection/requested-positions.xpath) | 40 | `ORD-105,ORD-102,ORD-102` |
| [requested-baseline](../examples/order-sequence-selection/requested-baseline.xpath) | 31 | `ORD-105,ORD-102,ORD-102` |
| [negative-bounds](../examples/order-sequence-selection/negative-bounds.xpath) | 40 | `ORD-104,ORD-105` |
| [reverse](../examples/order-sequence-selection/reverse.xpath) | 40 | `ORD-105,ORD-104,ORD-103,ORD-102,ORD-101` |
| [automatic-direction](../examples/order-sequence-selection/automatic-direction.xpath) | 40 | `ORD-104,ORD-103,ORD-102` |
| [step](../examples/order-sequence-selection/step.xpath) | 40 | `ORD-101,ORD-103,ORD-105` |
| [singleton](../examples/order-sequence-selection/singleton.xpath) | 40 | `ORD-101/0/ORD-101` |
| [zero-repeat](../examples/order-sequence-selection/zero-repeat.xpath) | 40 | `0` |
| [negative-repeat](../examples/order-sequence-selection/negative-repeat.xpath) | 40 | `XPTY0004` |
| [fractional-position](../examples/order-sequence-selection/fractional-position.xpath) | 40 | `XPTY0004` |
| [separator-block](../examples/order-sequence-selection/separator-block.xpath) | 40 | `ORD-101&#124;CHECK&#124;ORD-102` |


## 14. Performance Notes

Slice and positional selection materialize the input in this implementation. Replication grows the result with count times input length; it does not copy node trees. A multi-item separator grows output by its size for each boundary. No benchmark timings are claimed.

## 15. Known Uses

[`Slice_StartAndEnd_IsInclusive`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`Slice_NegativeStartPositiveEnd_ReversesRange`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ItemsAt_MultiplePositionsInAtOrder`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`ItemsAt_DuplicatePositionsKept`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`Replicate_MultiItem_RepeatsWholeSequence`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`Replicate_NegativeCount_RaisesXpty0004`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`Foot_EmptySequence_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`Trunk_Singleton_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs); [`InsertSeparator_MultiItemSeparator_InsertedAsBlock`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).

Exact sets `fn-replicate`, `fn-items-at`, `fn-foot`, `fn-trunk` and `fn-insert-separator` are in GatedSets. No exact `fn-slice` set is assigned. Unit/example coverage for slice is not a gate promotion. See the [actual gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs). Existing 1.1.1 release gate counts are historical evidence; no engine gates are rerun for this documentation slice.

## 16. Related Features

[Array batches](import-array-batches.md), [consecutive partitioning](consecutive-partition.md), [higher-order selection](higher-order-selection.md). [Completeness audit](../COMPLETENESS-AUDIT.md).
