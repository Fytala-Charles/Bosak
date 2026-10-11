<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: shipment-event sequence checks">
  <br><br><h1>Shipment-event Sequence Checks</h1>
  <p>Frozen 4.0 — identities, adjacent processing steps and event boundaries</p>
</div>

## 1. Name and Classification

**Shipment-event sequence checks — Frozen 4.0.** Functions: [fn:all-equal](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-all-equal), [fn:contains-subsequence](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-contains-subsequence), [fn:starts-with-subsequence](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-starts-with-subsequence), [fn:ends-with-subsequence](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-ends-with-subsequence).
Initial engine delivery: [Bosak PR #83](https://github.com/Fytala-Charles/Bosak/pull/83),
immutable revision [`532d04f4`](https://github.com/Fytala-Charles/Bosak/commit/532d04f40c3f1cbad93752e3158fc3cdfd4c7c39),
REQ-118 4.0-S1 part 2. Definitions reviewed **2026-10-11** against immutable
[QT4CG function source `a014e1b4`](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml). This review pin does not recover
the unavailable historical adopted specification snapshot.

Reference example delivery: [`b75c0d26`](https://github.com/Fytala-Charles/Bosak/commit/b75c0d26), [PR #162](https://github.com/Fytala-Charles/Bosak/pull/162).

## 2. Intent

Check that a shipment history belongs to one shipment, starts with the required
opening events, contains an adjacent dispatch pair, and ends in delivery. The
report keeps each decision visible so a failed history can be investigated.

## 3. Also Known As

`fn:all-equal` was originally proposed as `fn:uniform`, according to the pinned
function source. Bosak exposes the adopted `all-equal` spelling; this entry
introduces no former-name alias. The three subsequence functions use the exact
spellings listed above.

## 4. History / Supersedes

XPath 3.1 expresses all-equal as `count(distinct-values($values)) le 1`, a
prefix/suffix check as `deep-equal` over the appropriate `subsequence`, and a
contiguous segment search as `some $i ... satisfies deep-equal(...)`. The complete
comparison below implements those operations for this string-valued history.
It agrees on the illustrated report; a customized comparator requires an
explicit equivalent comparison in the earlier idiom.

## 5. Motivation

An unordered membership check can find PACKED and DISPATCHED while accepting
the wrong order or an intervening event. An equal-identity check alone accepts
an empty sequence and cannot reveal a missing attribute omitted by path
selection. Combining presence/count guards with sequence checks makes those
business conditions explicit.

## 6. Language Alignment

Matching uses adjacent sequence positions, preserving order and repeated
events. The mathematical term subsequence can allow gaps; these functions
require a contiguous segment, prefix or suffix. Default item comparison is
`deep-equal`, so maps are compared by content, including unrelated fields.
The custom record comparator intentionally projects only the state code.

Comparators receive `(input-item, required-item)` and must have arity two. Their
direction is observable when the callback is asymmetric. A boolean or empty
sequence is returned: empty means false, while the string `'yes'` fails
XPTY0004 rather than being treated as a truthy JavaScript value.
`all-equal` follows atomized `distinct-values` semantics, including collation,
numeric comparison and NaN grouping, rather than comparing every item to the
first with general `=`.

## 7. Applicability

Use for adjacent event rules over a history whose order and identity contract
are established. The sample assumes a nonempty header identity, supplies no
timestamps and does not implement a full carrier state machine. Repeated events,
retries and every allowed transition require additional business rules. A
completed report flag does not itself trigger dispatch or publish output.

## 8. Structure

```xquery
all-equal($ids)
all-equal($ids, $collation)
contains-subsequence($events, $required)
starts-with-subsequence($events, $required, $compare)
ends-with-subsequence($events, $required, $compare)
```

All three subsequence functions accept two-argument default-comparison and
three-argument comparator forms. An empty comparator argument chooses the
default. The exact record-comparator case exercises all three supplied-callback
forms; callback-result and signature errors are verified separately.

## 9. Participants

The existing FunctionLibrary registrations mark all four names frozen
XPath40-only. [CatalogExampleTests](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
compiles complete files at their manifest modes, asserts text/errors, and
verifies default-XPath31 refusal of successful 4.0 forms. The main expressions
parse their own complete XML input and need no external context.

## 10. Collaborations

Use with [sequence selection](order-sequence-selection.md) when an operator
needs a window of a history, and [sequence identity checks](sequence-distinctness.md)
when duplicate event identities need separate investigation. The
[order-reconciliation chapter](../worked-examples/order-reconciliation/README.md)
demonstrates the related distinction between document processing and business
acceptance. These examples add no new worked chapter.

## 11. Consequences

Empty required sequences match any input, including empty input. A nonempty
requirement longer than the input fails. `all-equal` is true for empty and
singleton values; the report separately requires events and one identity per
event. Path selection of missing identities/codes drops values, so count guards
are deliberate. The empty-history example also explains vacuous `known=true`.

Callback selection can change business meaning: the record example ignores
timestamps and normalizes code casing while the default compares full maps.
The supplied ASCII case-insensitive collation treats SHIP-42 and ship-42 as equal;
the default codepoint comparison keeps them distinct. Use that collation only
when the identifier contract permits it. NaN grouping is an equality contract,
not evidence that invalid measurement values are acceptable.

## 12. Implementation

Source: [FunctionLibrary.cs](../../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs),
`ContainsSubsequence`, `StartsWithSubsequence`, `EndsWithSubsequence` and
`AllEqual`; named assertions are in
[XPath40FunctionTests.cs](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
Direct names refuse XPST0017 at default XPath31. Source/public package version
remains 1.1.2. This entry changes documentation and exact fixtures only, with no
engine code, API or gate membership changes.

## 13. Sample Code

The synthetic carrier vocabulary below has one shipment header and ordered
event records. Each event supplies its shipment identity and state code. The
fixtures use document order as event order; production feeds must establish that
ordering before applying the checks. No timestamps, carrier standard, resource
access or application variables are assumed.

**Normal input:**

```xml
<history shipment="SHIP-42">
  <event shipment="SHIP-42" code="CREATED"/>
  <event shipment="SHIP-42" code="PICKED"/>
  <event shipment="SHIP-42" code="PACKED"/>
  <event shipment="SHIP-42" code="DISPATCHED"/>
  <event shipment="SHIP-42" code="IN_TRANSIT"/>
  <event shipment="SHIP-42" code="DELIVERED"/>
</history>
```

| Report field | Business check |
|---|---|
| `shipment`, `events` | Header identity and observed event count. |
| `consistent` | There is at least one event, every event has an identity, and all identities agree with the header. |
| `known` | Every event has a code and each code belongs to the declared vocabulary. |
| `start` | The first two events are CREATED then PICKED. |
| `dispatch` | PACKED immediately precedes DISPATCHED somewhere in the history. |
| `end` | DELIVERED is the final event. |
| `ready` | All five checks hold. This report flag is the demo's acceptance decision. |

**Complete XPath40 report — mode 40:** [main.xpath](../examples/shipment-event-sequences/main.xpath)

```xquery
(: Synthetic carrier history; document order is the agreed event order. :)
let $history := parse-xml('<history shipment="SHIP-42"><event shipment="SHIP-42" code="CREATED"/><event shipment="SHIP-42" code="PICKED"/><event shipment="SHIP-42" code="PACKED"/><event shipment="SHIP-42" code="DISPATCHED"/><event shipment="SHIP-42" code="IN_TRANSIT"/><event shipment="SHIP-42" code="DELIVERED"/></history>')/history,
    $events := $history/event,
    $codes := $events/@code ! string(.),
    $ids := $events/@shipment ! string(.),
    $consistent := exists($events) and count($ids) eq count($events)
                   and (all-equal($ids)) and $ids[1] eq string($history/@shipment),
    $known := count($codes) eq count($events) and
              (every $code in $codes satisfies $code = ('CREATED','PICKED','PACKED','DISPATCHED','IN_TRANSIT','DELIVERED')),
    $start := starts-with-subsequence($codes,('CREATED','PICKED')),
    $dispatch := contains-subsequence($codes,('PACKED','DISPATCHED')),
    $end := ends-with-subsequence($codes,('DELIVERED'))
return concat('shipment=', $history/@shipment, ';events=', count($events),
              ';consistent=', $consistent, ';known=', $known, ';start=', $start,
              ';dispatch=', $dispatch, ';end=', $end,
              ';ready=', $consistent and $known and $start and $dispatch and $end)
```

Exact output:

```text
shipment=SHIP-42;events=6;consistent=true;known=true;start=true;dispatch=true;end=true;ready=true
```

**XPath31 comparison — mode 31:** [baseline.xpath](../examples/shipment-event-sequences/baseline.xpath)

```xquery
(: Synthetic carrier history; document order is the agreed event order. :)
let $history := parse-xml('<history shipment="SHIP-42"><event shipment="SHIP-42" code="CREATED"/><event shipment="SHIP-42" code="PICKED"/><event shipment="SHIP-42" code="PACKED"/><event shipment="SHIP-42" code="DISPATCHED"/><event shipment="SHIP-42" code="IN_TRANSIT"/><event shipment="SHIP-42" code="DELIVERED"/></history>')/history,
    $events := $history/event,
    $codes := $events/@code ! string(.),
    $ids := $events/@shipment ! string(.),
    $consistent := exists($events) and count($ids) eq count($events)
                   and (count(distinct-values($ids)) le 1) and $ids[1] eq string($history/@shipment),
    $known := count($codes) eq count($events) and
              (every $code in $codes satisfies $code = ('CREATED','PICKED','PACKED','DISPATCHED','IN_TRANSIT','DELIVERED')),
    $start := deep-equal(subsequence($codes,1,2),('CREATED','PICKED')),
    $dispatch := some $i in 1 to (count($codes)-1) satisfies deep-equal(subsequence($codes,$i,2),('PACKED','DISPATCHED')),
    $end := deep-equal(subsequence($codes,count($codes),1),('DELIVERED'))
return concat('shipment=', $history/@shipment, ';events=', count($events),
              ';consistent=', $consistent, ';known=', $known, ';start=', $start,
              ';dispatch=', $dispatch, ';end=', $end,
              ';ready=', $consistent and $known and $start and $dispatch and $end)
```

Exact output:

```text
shipment=SHIP-42;events=6;consistent=true;known=true;start=true;dispatch=true;end=true;ready=true
```

**Empty-history edge — mode 40:** [edge.xpath](../examples/shipment-event-sequences/edge.xpath)

```xquery
(: Synthetic carrier history; document order is the agreed event order. :)
let $history := parse-xml('<history shipment="SHIP-42"></history>')/history,
    $events := $history/event,
    $codes := $events/@code ! string(.),
    $ids := $events/@shipment ! string(.),
    $consistent := exists($events) and count($ids) eq count($events)
                   and (all-equal($ids)) and $ids[1] eq string($history/@shipment),
    $known := count($codes) eq count($events) and
              (every $code in $codes satisfies $code = ('CREATED','PICKED','PACKED','DISPATCHED','IN_TRANSIT','DELIVERED')),
    $start := starts-with-subsequence($codes,('CREATED','PICKED')),
    $dispatch := contains-subsequence($codes,('PACKED','DISPATCHED')),
    $end := ends-with-subsequence($codes,('DELIVERED'))
return concat('shipment=', $history/@shipment, ';events=', count($events),
              ';consistent=', $consistent, ';known=', $known, ';start=', $start,
              ';dispatch=', $dispatch, ';end=', $end,
              ';ready=', $consistent and $known and $start and $dispatch and $end)
```

Exact output:

```text
shipment=SHIP-42;events=0;consistent=false;known=true;start=false;dispatch=false;end=false;ready=false
```

The edge input is `<history shipment="SHIP-42"/>`. Its `known=true` reflects
the vacuous universal check over no codes; `consistent=false` and the required
nonempty prefix/segment/suffix checks keep `ready=false`.

**Unexpected event input:**

```xml
<history shipment="SHIP-42">
  <event shipment="SHIP-42" code="CREATED"/>
  <event shipment="SHIP-42" code="PICKED"/>
  <event shipment="SHIP-42" code="PACKED"/>
  <event shipment="SHIP-42" code="CUSTOMS"/>
  <event shipment="SHIP-42" code="DISPATCHED"/>
  <event shipment="SHIP-42" code="DELIVERED"/>
</history>
```

The complete [unexpected-event expression](../examples/shipment-event-sequences/unexpected-event.xpath)
uses the same report logic. CUSTOMS is outside this reduced vocabulary and breaks
the required adjacent dispatch pair. Its exact output is:

```text
shipment=SHIP-42;events=6;consistent=true;known=false;start=true;dispatch=false;end=true;ready=false
```

**Additional executable contracts:**

| Case | Mode | Exact text or error |
|---|---|---|
| [unexpected-event](../examples/shipment-event-sequences/unexpected-event.xpath) | 40 | `shipment=SHIP-42;events=6;consistent=true;known=false;start=true;dispatch=false;end=true;ready=false` |
| [mixed-shipment](../examples/shipment-event-sequences/mixed-shipment.xpath) | 40 | `shipment=SHIP-42;events=6;consistent=false;known=true;start=true;dispatch=true;end=true;ready=false` |
| [missing-shipment](../examples/shipment-event-sequences/missing-shipment.xpath) | 40 | `shipment=SHIP-42;events=6;consistent=false;known=true;start=true;dispatch=true;end=true;ready=false` |
| [missing-event-code](../examples/shipment-event-sequences/missing-event-code.xpath) | 40 | `shipment=SHIP-42;events=6;consistent=true;known=false;start=true;dispatch=false;end=true;ready=false` |
| [wrong-start](../examples/shipment-event-sequences/wrong-start.xpath) | 40 | `shipment=SHIP-42;events=6;consistent=true;known=true;start=false;dispatch=true;end=true;ready=false` |
| [unfinished-history](../examples/shipment-event-sequences/unfinished-history.xpath) | 40 | `shipment=SHIP-42;events=5;consistent=true;known=true;start=true;dispatch=true;end=false;ready=false` |
| [empty-requirement](../examples/shipment-event-sequences/empty-requirement.xpath) | 40 | `true/true/true/true` |
| [noncontiguous](../examples/shipment-event-sequences/noncontiguous.xpath) | 40 | `false` |
| [noncontiguous-baseline](../examples/shipment-event-sequences/noncontiguous-baseline.xpath) | 31 | `false` |
| [long-pattern](../examples/shipment-event-sequences/long-pattern.xpath) | 40 | `false/false/false` |
| [repeated-events](../examples/shipment-event-sequences/repeated-events.xpath) | 40 | `true` |
| [record-comparator](../examples/shipment-event-sequences/record-comparator.xpath) | 40 | `false/true/true/true` |
| [empty-comparator-default](../examples/shipment-event-sequences/empty-comparator-default.xpath) | 40 | `true` |
| [comparator-empty-result](../examples/shipment-event-sequences/comparator-empty-result.xpath) | 40 | `false` |
| [comparator-nonboolean](../examples/shipment-event-sequences/comparator-nonboolean.xpath) | 40 | `XPTY0004` |
| [comparator-wrong-arity](../examples/shipment-event-sequences/comparator-wrong-arity.xpath) | 40 | `XPTY0004` |
| [comparator-not-function](../examples/shipment-event-sequences/comparator-not-function.xpath) | 40 | `XPTY0004` |
| [equal-singleton](../examples/shipment-event-sequences/equal-singleton.xpath) | 40 | `true` |
| [unequal-identities](../examples/shipment-event-sequences/unequal-identities.xpath) | 40 | `false` |
| [case-sensitive-identities](../examples/shipment-event-sequences/case-sensitive-identities.xpath) | 40 | `false` |
| [identity-collation](../examples/shipment-event-sequences/identity-collation.xpath) | 40 | `true` |
| [identity-collation-baseline](../examples/shipment-event-sequences/identity-collation-baseline.xpath) | 31 | `true` |
| [numeric-equality](../examples/shipment-event-sequences/numeric-equality.xpath) | 40 | `true` |
| [nan-equality](../examples/shipment-event-sequences/nan-equality.xpath) | 40 | `true` |


## 14. Performance Notes

The shipped segment search materializes both sequences and tries candidate
start positions, making up to input-length times requirement-length item
comparisons. Prefix/suffix operations materialize input and compare the required
positions. Comparator work and recursive deep equality add their own costs;
all-equal uses the distinct-values implementation and retains distinct results.
The fixtures establish no streaming or benchmark timing claims.

## 15. Known Uses

Named unit evidence:
[`AllEqual_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ContainsSubsequence_ContiguousRange_ReturnsTrue`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ContainsSubsequence_NonContiguousOrLonger_ReturnsFalse`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`ContainsSubsequence_CallbackEmptyResult_TreatedAsFalse`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`StartsWithSubsequence_PrefixMatch`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`StartsWithSubsequence_WithCallback`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`EndsWithSubsequence_SuffixMatch`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`EndsWithSubsequence_WithCallback`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).

`fn-contains-subsequence` is in the actual
[GatedSets runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs).
No exact `fn-all-equal`, `fn-starts-with-subsequence` or
`fn-ends-with-subsequence` set is assigned. Named units and the 27 exact manifest
cases remain separate evidence; family assignment does not certify every
signature. No gate promotion or new conformance run is claimed for this
documentation delivery.

## 16. Related Features

[Sequence identity checks](sequence-distinctness.md),
[order-export selection](order-sequence-selection.md),
[higher-order selection](higher-order-selection.md),
[product-map indexing](product-map-index.md),
[completeness audit](../COMPLETENESS-AUDIT.md).
