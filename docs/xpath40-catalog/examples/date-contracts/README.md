<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak timestamp and retention contract reference">
  <br><br><h1>Receipt Retention: Detailed Example</h1>
  <p>Gregorian components, epoch milliseconds and retention seconds</p>
</div>

## Business case and data policy

A warehouse receipt log stores arrival timestamps as epoch milliseconds and a configured retention window in seconds. A diagnostic report converts each arrival to UTC, calculates its expiry, reports the Gregorian length of the arrival month and exposes the retention seconds used. R2 arrives one minute before the end of leap-year February and expires in March; monthDays describes the arrival month, not the expiry month. This calculation performs no deletion or scheduling.

The synthetic receipts document contains ordered receipt elements with stable id, epochMs and retentionSeconds attributes. Validate unique/nonempty IDs, exactly one present value for each numeric attribute, nonnegative integer milliseconds, nonnegative decimal retention seconds, and supported calendar/duration ranges before using this workflow. The fixtures have complete valid data. Missing attributes or permissive conversion must not silently become epoch-zero records. Negative retention requires a separately defined policy. Host systems own clock measurement and actual retention enforcement.

UTC offsets identify instants; no implicit timezone or current clock is used. This is elapsed-duration retention, not a number of calendar/business days. Unix time assumes 86,400-second days and does not model leap seconds. Named timezone and daylight-saving rules must be resolved by the host for different local-calendar policies.

XPath 4.0 uses unix-dateTime for milliseconds, seconds for duration construction, build-dateTime for the first UTC date in the arrival month, days-in-month for its length, and duration-to-seconds for the total. XPath 3.1 adds PT0.001S times milliseconds to the epoch, multiplies PT1S by retention seconds, constructs the first date by subtracting elapsed calendar days, finds the next month's preceding day, and divides the duration by PT1S. Matching output does not imply that the portable dateTime result has the same stamp subtype. Nested let/return syntax keeps both expressions complete and portable.

| Field | Meaning |
|---|---|
| receipts | Number of source receipt records. |
| start | Arrival instant in UTC, converted from milliseconds. |
| expires | Arrival plus configured retention seconds. |
| monthDays | Length of the arrival's Gregorian UTC month. |
| seconds | Total retention duration in seconds, including its fraction. |

## Complete inputs, expressions and exact outputs

### Normal receipts

Complete input:

```xml
<receipts><receipt id="R1" epochMs="1709164800000" retentionSeconds="90.5"/><receipt id="R2" epochMs="1709251140000" retentionSeconds="120"/></receipts>
```

**XPath 31 — [report-31.xpath](report-31.xpath):**

```xquery
let $rows := parse-xml('<receipts><receipt id="R1" epochMs="1709164800000" retentionSeconds="90.5"/><receipt id="R2" epochMs="1709251140000" retentionSeconds="120"/></receipts>')/receipts/receipt
return concat('receipts=',count($rows),';details=',string-join(
  for $r in $rows return
    let $start := xs:dateTime('1970-01-01T00:00:00Z') + xs:dayTimeDuration('PT0.001S') * xs:integer($r/@epochMs)
    return let $duration := xs:dayTimeDuration('PT1S') * xs:decimal($r/@retentionSeconds)
    return let $first := xs:date($start) - xs:dayTimeDuration("P1D") * (day-from-dateTime($start)-1)
    return concat($r/@id,':start=',$start,',expires=',$start+$duration,',monthDays=',day-from-date($first + xs:yearMonthDuration("P1M") - xs:dayTimeDuration("P1D")),',seconds=',$duration div xs:dayTimeDuration("PT1S"))
,'|'))
```

Exact output:

```text
receipts=2;details=R1:start=2024-02-29T00:00:00Z,expires=2024-02-29T00:01:30.5Z,monthDays=29,seconds=90.5|R2:start=2024-02-29T23:59:00Z,expires=2024-03-01T00:01:00Z,monthDays=29,seconds=120
```

**XPath 40 — [report-40.xpath](report-40.xpath):**

```xquery
let $rows := parse-xml('<receipts><receipt id="R1" epochMs="1709164800000" retentionSeconds="90.5"/><receipt id="R2" epochMs="1709251140000" retentionSeconds="120"/></receipts>')/receipts/receipt
return concat('receipts=',count($rows),';details=',string-join(
  for $r in $rows return
    let $start := unix-dateTime(xs:integer($r/@epochMs))
    return let $duration := seconds(xs:decimal($r/@retentionSeconds))
    return let $first := build-dateTime(map{'year':year-from-dateTime($start),'month':month-from-dateTime($start),'day':1,'timezone':xs:dayTimeDuration('PT0S')})
    return concat($r/@id,':start=',$start,',expires=',$start+$duration,',monthDays=',days-in-month($first),',seconds=',duration-to-seconds($duration))
,'|'))
```

Exact output:

```text
receipts=2;details=R1:start=2024-02-29T00:00:00Z,expires=2024-02-29T00:01:30.5Z,monthDays=29,seconds=90.5|R2:start=2024-02-29T23:59:00Z,expires=2024-03-01T00:01:00Z,monthDays=29,seconds=120
```

### Empty log

Complete input:

```xml
<receipts/>
```

**XPath 31 — [report-empty-31.xpath](report-empty-31.xpath):**

```xquery
let $rows := parse-xml('<receipts/>')/receipts/receipt
return concat('receipts=',count($rows),';details=',string-join(
  for $r in $rows return
    let $start := xs:dateTime('1970-01-01T00:00:00Z') + xs:dayTimeDuration('PT0.001S') * xs:integer($r/@epochMs)
    return let $duration := xs:dayTimeDuration('PT1S') * xs:decimal($r/@retentionSeconds)
    return let $first := xs:date($start) - xs:dayTimeDuration("P1D") * (day-from-dateTime($start)-1)
    return concat($r/@id,':start=',$start,',expires=',$start+$duration,',monthDays=',day-from-date($first + xs:yearMonthDuration("P1M") - xs:dayTimeDuration("P1D")),',seconds=',$duration div xs:dayTimeDuration("PT1S"))
,'|'))
```

Exact output:

```text
receipts=0;details=
```

**XPath 40 — [report-empty-40.xpath](report-empty-40.xpath):**

```xquery
let $rows := parse-xml('<receipts/>')/receipts/receipt
return concat('receipts=',count($rows),';details=',string-join(
  for $r in $rows return
    let $start := unix-dateTime(xs:integer($r/@epochMs))
    return let $duration := seconds(xs:decimal($r/@retentionSeconds))
    return let $first := build-dateTime(map{'year':year-from-dateTime($start),'month':month-from-dateTime($start),'day':1,'timezone':xs:dayTimeDuration('PT0S')})
    return concat($r/@id,':start=',$start,',expires=',$start+$duration,',monthDays=',days-in-month($first),',seconds=',duration-to-seconds($duration))
,'|'))
```

Exact output:

```text
receipts=0;details=
```

### Reversed receipts

Complete input:

```xml
<receipts><receipt id="R2" epochMs="1709251140000" retentionSeconds="120"/><receipt id="R1" epochMs="1709164800000" retentionSeconds="90.5"/></receipts>
```

**XPath 31 — [report-reordered-31.xpath](report-reordered-31.xpath):**

```xquery
let $rows := parse-xml('<receipts><receipt id="R2" epochMs="1709251140000" retentionSeconds="120"/><receipt id="R1" epochMs="1709164800000" retentionSeconds="90.5"/></receipts>')/receipts/receipt
return concat('receipts=',count($rows),';details=',string-join(
  for $r in $rows return
    let $start := xs:dateTime('1970-01-01T00:00:00Z') + xs:dayTimeDuration('PT0.001S') * xs:integer($r/@epochMs)
    return let $duration := xs:dayTimeDuration('PT1S') * xs:decimal($r/@retentionSeconds)
    return let $first := xs:date($start) - xs:dayTimeDuration("P1D") * (day-from-dateTime($start)-1)
    return concat($r/@id,':start=',$start,',expires=',$start+$duration,',monthDays=',day-from-date($first + xs:yearMonthDuration("P1M") - xs:dayTimeDuration("P1D")),',seconds=',$duration div xs:dayTimeDuration("PT1S"))
,'|'))
```

Exact output:

```text
receipts=2;details=R2:start=2024-02-29T23:59:00Z,expires=2024-03-01T00:01:00Z,monthDays=29,seconds=120|R1:start=2024-02-29T00:00:00Z,expires=2024-02-29T00:01:30.5Z,monthDays=29,seconds=90.5
```

**XPath 40 — [report-reordered-40.xpath](report-reordered-40.xpath):**

```xquery
let $rows := parse-xml('<receipts><receipt id="R2" epochMs="1709251140000" retentionSeconds="120"/><receipt id="R1" epochMs="1709164800000" retentionSeconds="90.5"/></receipts>')/receipts/receipt
return concat('receipts=',count($rows),';details=',string-join(
  for $r in $rows return
    let $start := unix-dateTime(xs:integer($r/@epochMs))
    return let $duration := seconds(xs:decimal($r/@retentionSeconds))
    return let $first := build-dateTime(map{'year':year-from-dateTime($start),'month':month-from-dateTime($start),'day':1,'timezone':xs:dayTimeDuration('PT0S')})
    return concat($r/@id,':start=',$start,',expires=',$start+$duration,',monthDays=',days-in-month($first),',seconds=',duration-to-seconds($duration))
,'|'))
```

Exact output:

```text
receipts=2;details=R2:start=2024-02-29T23:59:00Z,expires=2024-03-01T00:01:00Z,monthDays=29,seconds=120|R1:start=2024-02-29T00:00:00Z,expires=2024-02-29T00:01:30.5Z,monthDays=29,seconds=90.5
```

The empty log returns receipts=0 and an empty details field. Reordering changes report order while each receipt's arrival/expiry remains unchanged. Stable IDs identify records; positions do not. R1 uses 90.5 seconds, preserving its half-second expiry; R2's 120-second window crosses the February/March boundary. This workflow makes no submillisecond precision claim.

## Focused contract fixtures

The [contract review](../../DATE-TIME-CONTRACT-REVIEW.md) links the pinned specification and explains accepted field sets, missing keywords, type/cardinality tolerance and precision follow-ups. The [manifest](../cases.json) carries exact case IDs/results. Diagnostic malformed/typed cases below are separate from validated business input. Successful 4.0 fixtures also verify default 3.1 refusal.

| Runnable case | Mode | Exact result/error |
|---|---|---|
| [build-dateTime-keyword-31.xpath](build-dateTime-keyword-31.xpath) | 31 | `XPST0003` |
| [build-dateTime-keyword-40.xpath](build-dateTime-keyword-40.xpath) | 40 | `XPST0017` |
| [build-dateTime-keyword-experimental.xpath](build-dateTime-keyword-experimental.xpath) | experimental | `XPST0017` |
| [days-in-month-keyword-31.xpath](days-in-month-keyword-31.xpath) | 31 | `XPST0003` |
| [days-in-month-keyword-40.xpath](days-in-month-keyword-40.xpath) | 40 | `XPST0017` |
| [days-in-month-keyword-experimental.xpath](days-in-month-keyword-experimental.xpath) | experimental | `XPST0017` |
| [unix-dateTime-keyword-31.xpath](unix-dateTime-keyword-31.xpath) | 31 | `XPST0003` |
| [unix-dateTime-keyword-40.xpath](unix-dateTime-keyword-40.xpath) | 40 | `XPST0017` |
| [unix-dateTime-keyword-experimental.xpath](unix-dateTime-keyword-experimental.xpath) | experimental | `XPST0017` |
| [seconds-keyword-31.xpath](seconds-keyword-31.xpath) | 31 | `XPST0003` |
| [seconds-keyword-40.xpath](seconds-keyword-40.xpath) | 40 | `XPST0017` |
| [seconds-keyword-experimental.xpath](seconds-keyword-experimental.xpath) | experimental | `XPST0017` |
| [duration-to-seconds-keyword-31.xpath](duration-to-seconds-keyword-31.xpath) | 31 | `XPST0003` |
| [duration-to-seconds-keyword-40.xpath](duration-to-seconds-keyword-40.xpath) | 40 | `XPST0017` |
| [duration-to-seconds-keyword-experimental.xpath](duration-to-seconds-keyword-experimental.xpath) | experimental | `XPST0017` |
| [build-dateTime-empty.xpath](build-dateTime-empty.xpath) | 40 | Empty string |
| [build-dateTime-empty-map.xpath](build-dateTime-empty-map.xpath) | 40 | `FODT0005` |
| [build-dateTime-year.xpath](build-dateTime-year.xpath) | 40 | `2024` |
| [build-dateTime-year-month.xpath](build-dateTime-year-month.xpath) | 40 | `2024-02` |
| [build-dateTime-date.xpath](build-dateTime-date.xpath) | 40 | `2024-02-29` |
| [build-dateTime-month.xpath](build-dateTime-month.xpath) | 40 | `--02` |
| [build-dateTime-month-day.xpath](build-dateTime-month-day.xpath) | 40 | `--02-29` |
| [build-dateTime-day.xpath](build-dateTime-day.xpath) | 40 | `---29` |
| [build-dateTime-time.xpath](build-dateTime-time.xpath) | 40 | `12:34:04.267` |
| [build-dateTime-dateTime.xpath](build-dateTime-dateTime.xpath) | 40 | `2024-02-29T12:34:04.25` |
| [build-dateTime-stamp.xpath](build-dateTime-stamp.xpath) | 40 | `2024-02-29T12:34:04.25+02:00` |
| [build-dateTime-empty-unused.xpath](build-dateTime-empty-unused.xpath) | 40 | `12:34:04.25` |
| [build-dateTime-partial.xpath](build-dateTime-partial.xpath) | 40 | `FODT0005` |
| [build-dateTime-invalid-day.xpath](build-dateTime-invalid-day.xpath) | 40 | `FORG0001` |
| [build-dateTime-hour24.xpath](build-dateTime-hour24.xpath) | 40 | `FORG0001` |
| [build-dateTime-seconds60.xpath](build-dateTime-seconds60.xpath) | 40 | `FORG0001` |
| [build-dateTime-timezone-limit.xpath](build-dateTime-timezone-limit.xpath) | 40 | `2024+14:00` |
| [build-dateTime-timezone-outside.xpath](build-dateTime-timezone-outside.xpath) | 40 | `FODT0003` |
| [build-dateTime-timezone-fraction.xpath](build-dateTime-timezone-fraction.xpath) | 40 | `FODT0003` |
| [build-dateTime-numeric-coercion.xpath](build-dateTime-numeric-coercion.xpath) | 40 | `2024-02-29` |
| [build-dateTime-fractional-field.xpath](build-dateTime-fractional-field.xpath) | 40 | `2024-02-01` |
| [build-dateTime-string-field.xpath](build-dateTime-string-field.xpath) | 40 | `2024-02-29` |
| [build-dateTime-untyped-field.xpath](build-dateTime-untyped-field.xpath) | 40 | `2024` |
| [build-dateTime-unknown-field.xpath](build-dateTime-unknown-field.xpath) | 40 | `2024` |
| [build-dateTime-multiple-field.xpath](build-dateTime-multiple-field.xpath) | 40 | `XPTY0004` |
| [build-dateTime-wrong-array.xpath](build-dateTime-wrong-array.xpath) | 40 | Empty string |
| [build-dateTime-wrong-integer.xpath](build-dateTime-wrong-integer.xpath) | 40 | `XPTY0004` |
| [build-dateTime-year-zero.xpath](build-dateTime-year-zero.xpath) | 40 | `0000-02-29` |
| [build-dateTime-large-year.xpath](build-dateTime-large-year.xpath) | 40 | `10000-01-01` |
| [build-dateTime-stamp-type.xpath](build-dateTime-stamp-type.xpath) | 40 | `true` |
| [build-dateTime-no-invented-zone.xpath](build-dateTime-no-invented-zone.xpath) | 40 | `true` |
| [build-dateTime-time-fraction-component.xpath](build-dateTime-time-fraction-component.xpath) | 40 | `4.267` |
| [build-dateTime-dateTime-fraction-component.xpath](build-dateTime-dateTime-fraction-component.xpath) | 40 | `4.267` |
| [build-dateTime-year-zero-dateTime.xpath](build-dateTime-year-zero-dateTime.xpath) | 40 | `0000-02-29T00:00:00` |
| [build-dateTime-nonempty-array.xpath](build-dateTime-nonempty-array.xpath) | 40 | `2024` |
| [days-in-month-empty.xpath](days-in-month-empty.xpath) | 40 | Empty string |
| [days-in-month-leap.xpath](days-in-month-leap.xpath) | 40 | `29` |
| [days-in-month-century.xpath](days-in-month-century.xpath) | 40 | `28` |
| [days-in-month-four-century.xpath](days-in-month-four-century.xpath) | 40 | `29` |
| [days-in-month-month.xpath](days-in-month-month.xpath) | 40 | `30` |
| [days-in-month-local-month.xpath](days-in-month-local-month.xpath) | 40 | `31` |
| [days-in-month-year-zero.xpath](days-in-month-year-zero.xpath) | 40 | `29` |
| [days-in-month-negative-year.xpath](days-in-month-negative-year.xpath) | 40 | `29` |
| [days-in-month-string.xpath](days-in-month-string.xpath) | 40 | `29` |
| [days-in-month-untyped.xpath](days-in-month-untyped.xpath) | 40 | `29` |
| [days-in-month-trailing-text.xpath](days-in-month-trailing-text.xpath) | 40 | `29` |
| [days-in-month-wrong-time.xpath](days-in-month-wrong-time.xpath) | 40 | `XPTY0004` |
| [days-in-month-wrong-map.xpath](days-in-month-wrong-map.xpath) | 40 | `XPTY0004` |
| [days-in-month-multiple.xpath](days-in-month-multiple.xpath) | 40 | `29` |
| [unix-dateTime-default.xpath](unix-dateTime-default.xpath) | 40 | `1970-01-01T00:00:00Z` |
| [unix-dateTime-empty.xpath](unix-dateTime-empty.xpath) | 40 | `1970-01-01T00:00:00Z` |
| [unix-dateTime-zero.xpath](unix-dateTime-zero.xpath) | 40 | `1970-01-01T00:00:00Z` |
| [unix-dateTime-millisecond.xpath](unix-dateTime-millisecond.xpath) | 40 | `1970-01-01T00:00:00.001Z` |
| [unix-dateTime-day.xpath](unix-dateTime-day.xpath) | 40 | `1970-01-02T00:00:00Z` |
| [unix-dateTime-negative.xpath](unix-dateTime-negative.xpath) | 40 | `FOCA0002` |
| [unix-dateTime-decimal.xpath](unix-dateTime-decimal.xpath) | 40 | `1970-01-01T00:00:00.001Z` |
| [unix-dateTime-double.xpath](unix-dateTime-double.xpath) | 40 | `1970-01-01T00:00:00.001Z` |
| [unix-dateTime-string.xpath](unix-dateTime-string.xpath) | 40 | `1970-01-01T00:00:00.001Z` |
| [unix-dateTime-untyped.xpath](unix-dateTime-untyped.xpath) | 40 | `1970-01-01T00:00:00.001Z` |
| [unix-dateTime-wrong-map.xpath](unix-dateTime-wrong-map.xpath) | 40 | `FOTY0013` |
| [unix-dateTime-multiple.xpath](unix-dateTime-multiple.xpath) | 40 | `1970-01-01T00:00:00.001Z` |
| [unix-dateTime-overflow.xpath](unix-dateTime-overflow.xpath) | 40 | `FODT0001` |
| [unix-dateTime-stamp-type.xpath](unix-dateTime-stamp-type.xpath) | 40 | `true` |
| [seconds-empty.xpath](seconds-empty.xpath) | 40 | Empty string |
| [seconds-zero.xpath](seconds-zero.xpath) | 40 | `PT0S` |
| [seconds-fraction.xpath](seconds-fraction.xpath) | 40 | `PT0.001S` |
| [seconds-negative.xpath](seconds-negative.xpath) | 40 | `-PT1H30M` |
| [seconds-day.xpath](seconds-day.xpath) | 40 | `P1D` |
| [seconds-string.xpath](seconds-string.xpath) | 40 | `XPTY0004` |
| [seconds-untyped.xpath](seconds-untyped.xpath) | 40 | `PT1.25S` |
| [seconds-double.xpath](seconds-double.xpath) | 40 | `XPTY0004` |
| [seconds-boolean.xpath](seconds-boolean.xpath) | 40 | `XPTY0004` |
| [seconds-multiple.xpath](seconds-multiple.xpath) | 40 | `XPTY0004` |
| [seconds-wrong-map.xpath](seconds-wrong-map.xpath) | 40 | `XPTY0004` |
| [seconds-overflow.xpath](seconds-overflow.xpath) | 40 | `FODT0002` |
| [duration-to-seconds-empty.xpath](duration-to-seconds-empty.xpath) | 40 | Empty string |
| [duration-to-seconds-minute.xpath](duration-to-seconds-minute.xpath) | 40 | `90` |
| [duration-to-seconds-day-fraction.xpath](duration-to-seconds-day-fraction.xpath) | 40 | `86400.5` |
| [duration-to-seconds-negative.xpath](duration-to-seconds-negative.xpath) | 40 | `-5400` |
| [duration-to-seconds-year-month.xpath](duration-to-seconds-year-month.xpath) | 40 | `XPTY0004` |
| [duration-to-seconds-general-month.xpath](duration-to-seconds-general-month.xpath) | 40 | `XPTY0004` |
| [duration-to-seconds-general-day.xpath](duration-to-seconds-general-day.xpath) | 40 | `XPTY0004` |
| [duration-to-seconds-string.xpath](duration-to-seconds-string.xpath) | 40 | `XPTY0004` |
| [duration-to-seconds-untyped.xpath](duration-to-seconds-untyped.xpath) | 40 | `90` |
| [duration-to-seconds-multiple.xpath](duration-to-seconds-multiple.xpath) | 40 | `XPTY0004` |
| [duration-to-seconds-wrong-map.xpath](duration-to-seconds-wrong-map.xpath) | 40 | `XPTY0004` |
| [seconds-roundtrip.xpath](seconds-roundtrip.xpath) | 40 | `90.125` |
