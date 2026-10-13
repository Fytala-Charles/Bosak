<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak shipment acceptance worked reference">
  <br><br><h1>Shipment Acceptance: Detailed Example</h1>
  <p>Shipment-event acceptance and predicate contracts</p>
</div>

## Business case and assumptions

A logistics importer receives one ordered shipment history. Before marking it ready for downstream processing, it checks shipment identity, uniqueness of event IDs and a minimal completion route: CREATED first, PACKED immediately followed by DISPATCHED somewhere, and DELIVERED last. Duplicate event IDs can indicate replayed feed records; a reversed dispatch pair can indicate out-of-order delivery. The report exposes each decision for investigation.

This synthetic vocabulary is not a carrier standard or a complete state machine. Document order is the agreed event order. Production ingestion must establish order from timestamps/sequence numbers, validate required nonempty attributes and known codes, and define retry/replay policy. Here presence counts catch missing attributes; empty attribute text is not separately rejected. Additional intermediate codes are allowed. A true ready flag reports this demo policy; it performs no dispatch or external write.

## XML and elements used

The history element carries the expected shipment identity. Each event has an id, shipment and code. Paths select those attributes and simple-map/string converts them to strings. Selecting a missing attribute drops a value, so counts are compared with the event count. exists guards the otherwise true empty equality check.

| Report field | Meaning |
|---|---|
| shipment / events | Header identity and observed event count. |
| consistent | Nonempty history, identity present on every event, identities all equal and equal to the header. |
| unique | One id per event, with no duplicate values under default codepoint comparison. |
| complete | One code per event, required prefix, contiguous dispatch pair and suffix. |
| ready | All three acceptance conditions. |

The 4.0 functions directly express these predicates. The 3.1 alternative uses distinct-values/count, deep-equal/subsequence and quantified candidate positions. It agrees for this string-valued policy; it does not implement arbitrary callback semantics or fix engine coercion boundaries. Both files parse their own input and require no application context or resources. Nested let/return clauses are valid in the portable 3.1 form.

## Complete inputs, expressions and exact outputs

### Normal shipment

Complete input:

```xml
<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="PACKED"/><event id="E3" shipment="SHIP-42" code="DISPATCHED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>
```

**XPath 40 — [report-40.xpath](report-40.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="PACKED"/><event id="E3" shipment="SHIP-42" code="DISPATCHED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (all-equal($shipments)) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (all-different($ids)) return let $complete := count($codes) eq count($events) and (starts-with-subsequence($codes,'CREATED')) and (contains-subsequence($codes,('PACKED','DISPATCHED'))) and (ends-with-subsequence($codes,'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=4;consistent=true;unique=true;complete=true;ready=true
```

**XPath 31 — [report-31.xpath](report-31.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="PACKED"/><event id="E3" shipment="SHIP-42" code="DISPATCHED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (count(distinct-values($shipments)) le 1) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (count(distinct-values($ids)) eq count($ids)) return let $complete := count($codes) eq count($events) and (deep-equal(subsequence($codes,1,1),'CREATED')) and (some $i in 1 to count($codes) satisfies deep-equal(subsequence($codes,$i,2),('PACKED','DISPATCHED'))) and (deep-equal(subsequence($codes,count($codes),1),'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=4;consistent=true;unique=true;complete=true;ready=true
```

### Empty history

Complete input:

```xml
<history shipment="SHIP-42"/>
```

**XPath 40 — [report-empty-40.xpath](report-empty-40.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"/>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (all-equal($shipments)) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (all-different($ids)) return let $complete := count($codes) eq count($events) and (starts-with-subsequence($codes,'CREATED')) and (contains-subsequence($codes,('PACKED','DISPATCHED'))) and (ends-with-subsequence($codes,'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=0;consistent=false;unique=true;complete=false;ready=false
```

**XPath 31 — [report-empty-31.xpath](report-empty-31.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"/>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (count(distinct-values($shipments)) le 1) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (count(distinct-values($ids)) eq count($ids)) return let $complete := count($codes) eq count($events) and (deep-equal(subsequence($codes,1,1),'CREATED')) and (some $i in 1 to count($codes) satisfies deep-equal(subsequence($codes,$i,2),('PACKED','DISPATCHED'))) and (deep-equal(subsequence($codes,count($codes),1),'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=0;consistent=false;unique=true;complete=false;ready=false
```

### Reordered dispatch pair

Complete input:

```xml
<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="DISPATCHED"/><event id="E3" shipment="SHIP-42" code="PACKED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>
```

**XPath 40 — [report-reordered-40.xpath](report-reordered-40.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="DISPATCHED"/><event id="E3" shipment="SHIP-42" code="PACKED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (all-equal($shipments)) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (all-different($ids)) return let $complete := count($codes) eq count($events) and (starts-with-subsequence($codes,'CREATED')) and (contains-subsequence($codes,('PACKED','DISPATCHED'))) and (ends-with-subsequence($codes,'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=4;consistent=true;unique=true;complete=false;ready=false
```

**XPath 31 — [report-reordered-31.xpath](report-reordered-31.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="DISPATCHED"/><event id="E3" shipment="SHIP-42" code="PACKED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (count(distinct-values($shipments)) le 1) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (count(distinct-values($ids)) eq count($ids)) return let $complete := count($codes) eq count($events) and (deep-equal(subsequence($codes,1,1),'CREATED')) and (some $i in 1 to count($codes) satisfies deep-equal(subsequence($codes,$i,2),('PACKED','DISPATCHED'))) and (deep-equal(subsequence($codes,count($codes),1),'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=4;consistent=true;unique=true;complete=false;ready=false
```

### Replayed event identity

Complete input:

```xml
<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="PACKED"/><event id="E2" shipment="SHIP-42" code="DISPATCHED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>
```

**XPath 40 — [report-duplicate-40.xpath](report-duplicate-40.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="PACKED"/><event id="E2" shipment="SHIP-42" code="DISPATCHED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (all-equal($shipments)) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (all-different($ids)) return let $complete := count($codes) eq count($events) and (starts-with-subsequence($codes,'CREATED')) and (contains-subsequence($codes,('PACKED','DISPATCHED'))) and (ends-with-subsequence($codes,'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=4;consistent=true;unique=false;complete=true;ready=false
```

**XPath 31 — [report-duplicate-31.xpath](report-duplicate-31.xpath):**

```xquery
let $history := parse-xml('<history shipment="SHIP-42"><event id="E1" shipment="SHIP-42" code="CREATED"/><event id="E2" shipment="SHIP-42" code="PACKED"/><event id="E2" shipment="SHIP-42" code="DISPATCHED"/><event id="E4" shipment="SHIP-42" code="DELIVERED"/></history>')/history return let $events := $history/event return let $ids := $events/@id ! string(.) return let $shipments := $events/@shipment ! string(.) return let $codes := $events/@code ! string(.) return let $consistent := exists($events) and count($shipments) eq count($events) and (count(distinct-values($shipments)) le 1) and $shipments[1] eq string($history/@shipment) return let $unique := count($ids) eq count($events) and (count(distinct-values($ids)) eq count($ids)) return let $complete := count($codes) eq count($events) and (deep-equal(subsequence($codes,1,1),'CREATED')) and (some $i in 1 to count($codes) satisfies deep-equal(subsequence($codes,$i,2),('PACKED','DISPATCHED'))) and (deep-equal(subsequence($codes,count($codes),1),'DELIVERED')) return concat('shipment=',$history/@shipment,';events=',count($events),';consistent=',$consistent,';unique=',$unique,';complete=',$complete,';ready=',$consistent and $unique and $complete)
```

Exact output:

```text
shipment=SHIP-42;events=4;consistent=true;unique=false;complete=true;ready=false
```

The empty history yields unique=true because no IDs are duplicated, but consistent=false and complete=false keep ready=false. Swapping PACKED/DISPATCHED preserves IDs and shipment consistency while breaking the adjacent route. Reusing E2 retains completion but sets unique=false. None of these reports deduplicates or reorders the source.

## Focused contracts and unavailable forms

The [contract review](../../PREDICATE-CONTRACT-REVIEW.md) explains the pinned signatures, missing keyword metadata and observed map/collation tolerance. These diagnostic cases are separate from the supported string-valued business example. A map accepted by an atomic-signature function is an engine follow-up. Callback record projection is intentional only when the business rule permits ignoring other fields.

| Runnable case | Mode | Exact result/error |
|---|---|---|
| [all-equal-keyword-31.xpath](all-equal-keyword-31.xpath) | 31 | `XPST0003` |
| [all-equal-keyword-40.xpath](all-equal-keyword-40.xpath) | 40 | `XPST0017` |
| [all-equal-keyword-experimental.xpath](all-equal-keyword-experimental.xpath) | experimental | `XPST0017` |
| [all-equal-empty.xpath](all-equal-empty.xpath) | 40 | `true` |
| [all-equal-singleton.xpath](all-equal-singleton.xpath) | 40 | `true` |
| [all-equal-same.xpath](all-equal-same.xpath) | 40 | `true` |
| [all-equal-different.xpath](all-equal-different.xpath) | 40 | `false` |
| [all-equal-numeric.xpath](all-equal-numeric.xpath) | 40 | `true` |
| [all-equal-nan.xpath](all-equal-nan.xpath) | 40 | `true` |
| [all-equal-incomparable.xpath](all-equal-incomparable.xpath) | 40 | `false` |
| [all-equal-nodes.xpath](all-equal-nodes.xpath) | 40 | `true` |
| [all-equal-map.xpath](all-equal-map.xpath) | 40 | `true` |
| [all-equal-empty-collation.xpath](all-equal-empty-collation.xpath) | 40 | `false` |
| [all-equal-case-collation.xpath](all-equal-case-collation.xpath) | 40 | `true` |
| [all-equal-invalid-collation.xpath](all-equal-invalid-collation.xpath) | 40 | `FOCH0002` |
| [all-equal-string-empty-collation.xpath](all-equal-string-empty-collation.xpath) | 40 | `false` |
| [all-equal-numeric-collation.xpath](all-equal-numeric-collation.xpath) | 40 | `FOCH0002` |
| [all-different-keyword-31.xpath](all-different-keyword-31.xpath) | 31 | `XPST0003` |
| [all-different-keyword-40.xpath](all-different-keyword-40.xpath) | 40 | `XPST0017` |
| [all-different-keyword-experimental.xpath](all-different-keyword-experimental.xpath) | experimental | `XPST0017` |
| [all-different-empty.xpath](all-different-empty.xpath) | 40 | `true` |
| [all-different-singleton.xpath](all-different-singleton.xpath) | 40 | `true` |
| [all-different-same.xpath](all-different-same.xpath) | 40 | `false` |
| [all-different-different.xpath](all-different-different.xpath) | 40 | `true` |
| [all-different-numeric.xpath](all-different-numeric.xpath) | 40 | `false` |
| [all-different-nan.xpath](all-different-nan.xpath) | 40 | `false` |
| [all-different-incomparable.xpath](all-different-incomparable.xpath) | 40 | `true` |
| [all-different-nodes.xpath](all-different-nodes.xpath) | 40 | `false` |
| [all-different-map.xpath](all-different-map.xpath) | 40 | `true` |
| [all-different-empty-collation.xpath](all-different-empty-collation.xpath) | 40 | `true` |
| [all-different-case-collation.xpath](all-different-case-collation.xpath) | 40 | `false` |
| [all-different-invalid-collation.xpath](all-different-invalid-collation.xpath) | 40 | `FOCH0002` |
| [all-different-string-empty-collation.xpath](all-different-string-empty-collation.xpath) | 40 | `true` |
| [all-different-numeric-collation.xpath](all-different-numeric-collation.xpath) | 40 | `FOCH0002` |
| [contains-subsequence-keyword-31.xpath](contains-subsequence-keyword-31.xpath) | 31 | `XPST0003` |
| [contains-subsequence-keyword-40.xpath](contains-subsequence-keyword-40.xpath) | 40 | `XPST0017` |
| [contains-subsequence-keyword-experimental.xpath](contains-subsequence-keyword-experimental.xpath) | experimental | `XPST0017` |
| [contains-subsequence-empty-both.xpath](contains-subsequence-empty-both.xpath) | 40 | `true` |
| [contains-subsequence-empty-pattern.xpath](contains-subsequence-empty-pattern.xpath) | 40 | `true` |
| [contains-subsequence-empty-input.xpath](contains-subsequence-empty-input.xpath) | 40 | `false` |
| [contains-subsequence-long-pattern.xpath](contains-subsequence-long-pattern.xpath) | 40 | `false` |
| [contains-subsequence-exact.xpath](contains-subsequence-exact.xpath) | 40 | `true` |
| [contains-subsequence-gaps.xpath](contains-subsequence-gaps.xpath) | 40 | `false` |
| [contains-subsequence-empty-comparator.xpath](contains-subsequence-empty-comparator.xpath) | 40 | `true` |
| [contains-subsequence-deep-maps.xpath](contains-subsequence-deep-maps.xpath) | 40 | `false` |
| [contains-subsequence-projected-maps.xpath](contains-subsequence-projected-maps.xpath) | 40 | `true` |
| [contains-subsequence-asymmetric.xpath](contains-subsequence-asymmetric.xpath) | 40 | `true` |
| [contains-subsequence-asymmetric-reversed.xpath](contains-subsequence-asymmetric-reversed.xpath) | 40 | `false` |
| [contains-subsequence-callback-empty.xpath](contains-subsequence-callback-empty.xpath) | 40 | `false` |
| [contains-subsequence-callback-string.xpath](contains-subsequence-callback-string.xpath) | 40 | `XPTY0004` |
| [contains-subsequence-callback-multiple.xpath](contains-subsequence-callback-multiple.xpath) | 40 | `XPTY0004` |
| [contains-subsequence-callback-wrong-arity.xpath](contains-subsequence-callback-wrong-arity.xpath) | 40 | `XPTY0004` |
| [contains-subsequence-callback-not-function.xpath](contains-subsequence-callback-not-function.xpath) | 40 | `XPTY0004` |
| [contains-subsequence-callback-untyped.xpath](contains-subsequence-callback-untyped.xpath) | 40 | `true` |
| [starts-with-subsequence-keyword-31.xpath](starts-with-subsequence-keyword-31.xpath) | 31 | `XPST0003` |
| [starts-with-subsequence-keyword-40.xpath](starts-with-subsequence-keyword-40.xpath) | 40 | `XPST0017` |
| [starts-with-subsequence-keyword-experimental.xpath](starts-with-subsequence-keyword-experimental.xpath) | experimental | `XPST0017` |
| [starts-with-subsequence-empty-both.xpath](starts-with-subsequence-empty-both.xpath) | 40 | `true` |
| [starts-with-subsequence-empty-pattern.xpath](starts-with-subsequence-empty-pattern.xpath) | 40 | `true` |
| [starts-with-subsequence-empty-input.xpath](starts-with-subsequence-empty-input.xpath) | 40 | `false` |
| [starts-with-subsequence-long-pattern.xpath](starts-with-subsequence-long-pattern.xpath) | 40 | `false` |
| [starts-with-subsequence-exact.xpath](starts-with-subsequence-exact.xpath) | 40 | `true` |
| [starts-with-subsequence-gaps.xpath](starts-with-subsequence-gaps.xpath) | 40 | `false` |
| [starts-with-subsequence-empty-comparator.xpath](starts-with-subsequence-empty-comparator.xpath) | 40 | `true` |
| [starts-with-subsequence-deep-maps.xpath](starts-with-subsequence-deep-maps.xpath) | 40 | `false` |
| [starts-with-subsequence-projected-maps.xpath](starts-with-subsequence-projected-maps.xpath) | 40 | `true` |
| [starts-with-subsequence-asymmetric.xpath](starts-with-subsequence-asymmetric.xpath) | 40 | `true` |
| [starts-with-subsequence-asymmetric-reversed.xpath](starts-with-subsequence-asymmetric-reversed.xpath) | 40 | `false` |
| [starts-with-subsequence-callback-empty.xpath](starts-with-subsequence-callback-empty.xpath) | 40 | `false` |
| [starts-with-subsequence-callback-string.xpath](starts-with-subsequence-callback-string.xpath) | 40 | `XPTY0004` |
| [starts-with-subsequence-callback-multiple.xpath](starts-with-subsequence-callback-multiple.xpath) | 40 | `XPTY0004` |
| [starts-with-subsequence-callback-wrong-arity.xpath](starts-with-subsequence-callback-wrong-arity.xpath) | 40 | `XPTY0004` |
| [starts-with-subsequence-callback-not-function.xpath](starts-with-subsequence-callback-not-function.xpath) | 40 | `XPTY0004` |
| [starts-with-subsequence-callback-untyped.xpath](starts-with-subsequence-callback-untyped.xpath) | 40 | `true` |
| [ends-with-subsequence-keyword-31.xpath](ends-with-subsequence-keyword-31.xpath) | 31 | `XPST0003` |
| [ends-with-subsequence-keyword-40.xpath](ends-with-subsequence-keyword-40.xpath) | 40 | `XPST0017` |
| [ends-with-subsequence-keyword-experimental.xpath](ends-with-subsequence-keyword-experimental.xpath) | experimental | `XPST0017` |
| [ends-with-subsequence-empty-both.xpath](ends-with-subsequence-empty-both.xpath) | 40 | `true` |
| [ends-with-subsequence-empty-pattern.xpath](ends-with-subsequence-empty-pattern.xpath) | 40 | `true` |
| [ends-with-subsequence-empty-input.xpath](ends-with-subsequence-empty-input.xpath) | 40 | `false` |
| [ends-with-subsequence-long-pattern.xpath](ends-with-subsequence-long-pattern.xpath) | 40 | `false` |
| [ends-with-subsequence-exact.xpath](ends-with-subsequence-exact.xpath) | 40 | `true` |
| [ends-with-subsequence-gaps.xpath](ends-with-subsequence-gaps.xpath) | 40 | `false` |
| [ends-with-subsequence-empty-comparator.xpath](ends-with-subsequence-empty-comparator.xpath) | 40 | `true` |
| [ends-with-subsequence-deep-maps.xpath](ends-with-subsequence-deep-maps.xpath) | 40 | `false` |
| [ends-with-subsequence-projected-maps.xpath](ends-with-subsequence-projected-maps.xpath) | 40 | `true` |
| [ends-with-subsequence-asymmetric.xpath](ends-with-subsequence-asymmetric.xpath) | 40 | `true` |
| [ends-with-subsequence-asymmetric-reversed.xpath](ends-with-subsequence-asymmetric-reversed.xpath) | 40 | `false` |
| [ends-with-subsequence-callback-empty.xpath](ends-with-subsequence-callback-empty.xpath) | 40 | `false` |
| [ends-with-subsequence-callback-string.xpath](ends-with-subsequence-callback-string.xpath) | 40 | `XPTY0004` |
| [ends-with-subsequence-callback-multiple.xpath](ends-with-subsequence-callback-multiple.xpath) | 40 | `XPTY0004` |
| [ends-with-subsequence-callback-wrong-arity.xpath](ends-with-subsequence-callback-wrong-arity.xpath) | 40 | `XPTY0004` |
| [ends-with-subsequence-callback-not-function.xpath](ends-with-subsequence-callback-not-function.xpath) | 40 | `XPTY0004` |
| [ends-with-subsequence-callback-untyped.xpath](ends-with-subsequence-callback-untyped.xpath) | 40 | `true` |

## Verification and evidence limits

[cases.json](../cases.json) contains 102 exact checks for this review; [CatalogExampleTests](../../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) evaluates them at their declared modes. Successful new 4.0 calls also require default-3.1 refusal. Explicit keyword cases distinguish grammar XPST0003 from binding XPST0017. Independent XML processing checks all eight report outputs. Existing entry-level named units and gated-set applicability remain separate evidence.

Version stays 1.1.2; no engine, API, harness, gate membership or release changes. Full collations, function conversion and state-machine acceptance are not established by these bounded fixtures.
