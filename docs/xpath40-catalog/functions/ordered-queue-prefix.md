<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: ordered dispatch queue">
  <br><br><h1>Ordered Queue Prefix</h1>
  <p>Frozen 4.0 — dispatch the ready prefix and retain the remainder</p>
</div>

## 1. Name and Classification

**Ordered queue prefix — Frozen 4.0.** [fn:take-while](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-take-while) and [fn:drop-while](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-drop-while).
Initial engine delivery: [PR #88](https://github.com/Fytala-Charles/Bosak/pull/88),
[revision 0a928bb8](https://github.com/Fytala-Charles/Bosak/commit/0a928bb81b81d0631cc681f86bba83259c0ed790), REQ-118 4.0-S5.
Definitions reviewed 2026-10-11 against the immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.

## 2. Intent

Split an ordered dispatch queue at its first non-ready order. Dispatch the ready
prefix and retain the blocking order together with every later order for investigation.

## 3. Also Known As

Prefix selection and prefix removal. These descriptive terms introduce no function aliases.

## 4. History / Supersedes

XPath 3.1 can locate the first failing position and use `subsequence` for both
parts. The complete comparison below uses explicit nested `let … return`
expressions and the same status rule. Filtering ready orders throughout the queue
implements a different business policy: it lets later orders overtake a blocker.

## 5. Motivation

A warehouse may preserve acceptance order because orders share a shipment,
reservation or review process. A held order blocks later dispatch even when a
later order is individually ready. The example makes that policy visible in its report.

## 6. Language Alignment

Both functions accept an `item()*` input and a predicate receiving the item and
its one-based input position, returning `xs:boolean?`. `false()` or `()` marks
the boundary. `take-while` excludes that item; `drop-while` includes it and all
later items. Empty input gives empty results; all matching input is returned by
`take-while` and removed by `drop-while`. A string such as `'ready'` is not a
boolean predicate result and raises XPTY0004. Bosak also accepts a map as a
one-argument callable predicate; a missing key returns empty and stops selection.
The fixtures use ordinary two-argument inline functions for the business rule.

## 7. Applicability

Use when input order is authoritative and the first failed condition blocks
subsequent work. If independent ready orders may proceed, use a filter instead.
The sample assumes nonempty, unique order identifiers supplied by upstream
validation. It does not validate identities, sort timestamps, persist a queue,
reserve inventory or send a shipment. Missing status is treated as non-ready.

## 8. Structure

`input → readiness predicate → ready prefix + retained remainder`.
The order sequence consists of XML element nodes, not their identifier strings.
The report projects identifiers only after the split.

## 9. Participants

The queue preserves arrival order. Each order supplies an identifier and status.
The predicate defines readiness. The two functions select nodes. The report
shows dispatch and retained identifiers so the boundary is reviewable.

## 10. Collaborations

Supply the same deterministic predicate to both calls. Concatenating the two
results reconstructs the input with order and node identity preserved. The calls
are independent and repeat predicate work; changing state between calls would
invalidate the business assumption behind the split.

## 11. Consequences

The first held order and every later ready order remain visible. No order is
silently discarded or moved. An empty queue needs no dispatch; an entirely ready
queue leaves no remainder. These are selection decisions, not operational side effects.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Prefix-only selection with first nonmatch excluded/included; Original input positions and empty/all-match queues; Boolean/empty predicates and tested callable maps. **Boundaries:** Not an arbitrary filter or partition; no operational side effects or general callable-subtyping promise. Follow-up groups: F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Compile the 4.0 expressions with `CompileOptions.Compatibility40`. The default
3.1 profile rejects the new functions. The comparison uses `Compatibility31`.
The existing [catalog test harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks exact text/error outcomes and default-profile refusal for successful 4.0 cases.
No external files, variables, services or context item are required.

## 13. Sample Code

The synthetic fixture represents a warehouse queue in arrival order. ORD103 is
held for investigation; ORD104 must wait behind it despite its ready status.

```xml
<queue>
  <order id="ORD101" status="ready"/>
  <order id="ORD102" status="ready"/>
  <order id="ORD103" status="hold"/>
  <order id="ORD104" status="ready"/>
</queue>
```

Complete XPath 4.0 expression ([runnable file](../examples/ordered-queue-prefix/main.xpath)):

```xpath
let $queue := parse-xml('<queue><order id="ORD101" status="ready"/><order id="ORD102" status="ready"/><order id="ORD103" status="hold"/><order id="ORD104" status="ready"/></queue>')/queue/order
let $ready := function($order, $position) { string($order/@status) eq 'ready' }
let $dispatch := take-while($queue, $ready)
let $retained := drop-while($queue, $ready)
return concat('dispatch=', string-join($dispatch/@id, ','), '; retained=', string-join($retained/@id, ','))
```

Exact output:

```text
dispatch=ORD101,ORD102; retained=ORD103,ORD104
```

Complete XPath 3.1 comparison ([runnable file](../examples/ordered-queue-prefix/baseline.xpath)):

```xpath
let $queue := parse-xml('<queue><order id="ORD101" status="ready"/><order id="ORD102" status="ready"/><order id="ORD103" status="hold"/><order id="ORD104" status="ready"/></queue>')/queue/order
return let $stop := ((for $i in 1 to count($queue) return if (string($queue[$i]/@status) eq 'ready') then () else $i)[1], count($queue) + 1)[1]
return let $dispatch := subsequence($queue, 1, $stop - 1)
return let $retained := subsequence($queue, $stop)
return concat('dispatch=', string-join($dispatch/@id, ','), '; retained=', string-join($retained/@id, ','))
```

Its exact output is identical. A whole-queue filter instead returns
`ORD101,ORD102,ORD104`, as the [filter comparison](../examples/ordered-queue-prefix/filter-comparison.xpath)
verifies. It would allow ORD104 to overtake the held order.

### Boundary and callback cases

Each file embeds its complete input. Empty output below means an empty string
from the report's `string-join`, not a string returned by the selection function.

| Runnable case | Exact result or error | Meaning |
|---|---|---|
| [main](../examples/ordered-queue-prefix/main.xpath) | `dispatch=ORD101,ORD102; retained=ORD103,ORD104` | Normal ordered split |
| [baseline](../examples/ordered-queue-prefix/baseline.xpath) | `dispatch=ORD101,ORD102; retained=ORD103,ORD104` | Earlier-version equivalent |
| [filter-comparison](../examples/ordered-queue-prefix/filter-comparison.xpath) | `ORD101,ORD102,ORD104` | Independent ready-order policy |
| [all-ready](../examples/ordered-queue-prefix/all-ready.xpath) | `dispatch=ORD101,ORD102,ORD103,ORD104; retained=` | No blocker |
| [first-held](../examples/ordered-queue-prefix/first-held.xpath) | `dispatch=; retained=ORD101,ORD102,ORD103,ORD104` | First order blocks all work |
| [empty](../examples/ordered-queue-prefix/empty.xpath) | `dispatch=; retained=` | No orders |
| [missing-status](../examples/ordered-queue-prefix/missing-status.xpath) | `dispatch=ORD101,ORD102; retained=ORD103,ORD104` | Missing status blocks dispatch |
| [position-cap](../examples/ordered-queue-prefix/position-cap.xpath) | `dispatch=ORD101; retained=ORD102,ORD103,ORD104` | One-based capacity boundary |
| [reconstruction](../examples/ordered-queue-prefix/reconstruction.xpath) | `true` | Both parts restore input |
| [map-predicate](../examples/ordered-queue-prefix/map-predicate.xpath) | `ready,ready` | Status map used as callable |
| [take-empty-decision](../examples/ordered-queue-prefix/take-empty-decision.xpath) | `(empty string)` | Empty decision stops immediately |
| [take-nonboolean](../examples/ordered-queue-prefix/take-nonboolean.xpath) | `XPTY0004` | Strict boolean result |
| [take-noncallable](../examples/ordered-queue-prefix/take-noncallable.xpath) | `XPTY0004` | Predicate must be callable |
| [take-early-stop](../examples/ordered-queue-prefix/take-early-stop.xpath) | `ORD101` | Later predicate error is not evaluated |
| [drop-empty-decision](../examples/ordered-queue-prefix/drop-empty-decision.xpath) | `ORD101,ORD102` | Empty decision retains everything |
| [drop-nonboolean](../examples/ordered-queue-prefix/drop-nonboolean.xpath) | `XPTY0004` | Strict boolean result |
| [drop-noncallable](../examples/ordered-queue-prefix/drop-noncallable.xpath) | `XPTY0004` | Predicate must be callable |
| [drop-early-stop](../examples/ordered-queue-prefix/drop-early-stop.xpath) | `ORD102,ORD103` | Later predicate error is not evaluated |

For the missing-status case the full input is the XML above with ORD103's
`status="hold"` attribute removed; the exact report remains the normal report.
The all-ready case changes that attribute to `status="ready"` and dispatches all
four identifiers. The first-held case changes ORD101 to `hold` and retains all
four. The empty fixture is `<queue/>` and reports `dispatch=; retained=`.
The position-cap fixture allows only position one: it reports
`dispatch=ORD101; retained=ORD102,ORD103,ORD104`.

## 14. Performance

Bosak stops invoking the predicate at the boundary, as the early-stop fixtures
verify. `drop-while` still copies the retained items, and both implementations
materialize their results. Two calls repeat the prefix scan. No streaming or
benchmark claim follows from this example; for an expensive predicate, locate
the boundary once and slice the queue as in the earlier-version comparison.

## 15. Known Uses

The warehouse scenario is a reproducible synthetic example, not a claim about
an identified production deployment. Existing named unit evidence includes
[`TakeWhile_StopsAtFirstNonMatch`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`TakeWhile_MapAsPredicate_LooksUpEachItem`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`TakeWhile_EmptyPredicateResultMeansFalse`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`DropWhile_DropsPrefixUpToFirstNonMatch`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`DropWhile_TakeWhile_ReconstructInput`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 gate runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
assigns exactly `fn-take-while` to GatedSets. No exact `fn-drop-while` set is
assigned; its unit tests and executable examples provide separate evidence.
This documentation delivery changes no gate membership.

## 16. Related Features

[Order sequence selection](order-sequence-selection.md) covers general selection
and positions. [Shipment-event sequence checks](shipment-event-sequences.md)
covers identity and contiguous event rules. [The completeness audit](../COMPLETENESS-AUDIT.md)
tracks the remaining higher-order families and reference coverage limits.
