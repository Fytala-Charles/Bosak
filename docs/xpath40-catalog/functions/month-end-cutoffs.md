<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: month-end invoice cutoff scheduling">
  <br><br><h1>Month-end Cutoffs</h1>
  <p>Frozen 4.0 — Gregorian components and explicit timezone offsets</p>
</div>

## 1. Name and Classification

**Month-end cutoffs — Frozen 4.0.** [fn:build-dateTime](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-build-dateTime) and [fn:days-in-month](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-days-in-month).
Initial engine delivery: [PR #84](https://github.com/Fytala-Charles/Bosak/pull/84),
[revision f7f55b4e](https://github.com/Fytala-Charles/Bosak/commit/f7f55b4e3a6e92da26ac607e34f46aede695cb19), REQ-118 4.0-S2.
Definitions reviewed **2026-10-11** against immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.

## 2. Intent

Construct the final-day invoice submission cutoff for a given month, local clock
time and explicit offset. Show both the local cutoff and its equivalent UTC instant.

## 3. Also Known As

Calendar construction and month-length lookup are descriptive terms; no aliases
are registered. The case is a calendar-month cutoff, not a last-business-day rule.

## 4. History / Supersedes

XPath 3.1 can find the final date by adding a year-month duration to the first
of the month, then subtracting one day. The complete comparison uses `dateTime`
to combine that date and a time, then attaches the supplied offset and converts
to UTC. Its report matches the 4.0 case; the constructor's result type is not
thereby claimed to match the new function's `xs:dateTimeStamp` subtype.

## 5. Motivation

Invoice submission calendars may specify a fixed local cutoff on the final day
of each month. Hardcoding day 31 fails in shorter months, and February requires
the Gregorian century rule. Reporting the UTC instant helps a scheduler consume
the local business rule without guessing the offset.

## 6. Language Alignment

`days-in-month` accepts an optional `xs:dateTime`, `xs:date` or `xs:gYearMonth`,
returning 28, 29, 30 or 31 for its **local** year/month. A value whose UTC instant
falls in February may still have a March local month. Leap years are divisible
by four, except centuries not divisible by 400. Empty input returns empty.

`build-dateTime` constructs a Gregorian value from a component map. Full date
and clock components plus timezone produce `xs:dateTimeStamp`; omitting timezone
produces `xs:dateTime` without inventing an offset. Date-only fields produce
`xs:date`, and clock-only fields produce `xs:time`. Empty input returns empty.
Unused components can be absent or empty; arbitrary incomplete field combinations
raise FODT0005. Invalid calendar/clock values raise FORG0001; an offset outside
±14 hours raises FODT0003. Midnight uses hour zero, not 24.

## 7. Applicability

Use when a rule supplies a calendar month, a local clock time and a numeric UTC
offset. This fixture assumes present, valid numeric fields and an explicit
whole-minute offset. It does not handle weekends, holidays or business calendars.
An offset such as +02:00 is not an IANA timezone and carries no daylight-saving
rules; the host must resolve a named zone for the scheduled date before supplying
its offset. No implicit timezone or current clock is used here.

The subsequent 1.1.3 contract review verifies year-zero date/dateTime and
year-10000 date construction, as well as year-zero month-length lookup. These
observed forms supersede the earlier blanket construction-limit statement;
not every extended-year operation or range is certified.

## 8. Structure

Read the schedule, build the first local date, ask for its month length, and build
a cutoff on that final day. Convert the already offset-bearing cutoff to UTC
with `adjust-dateTime-to-timezone`, preserving its instant.

## 9. Participants

Schedule attributes provide year, month, clock and offset. Month-length lookup
selects a valid final day. The component constructor creates the cutoff. The
UTC projection and report expose the resulting scheduling decision.

## 10. Collaborations

The first date has day one so month-length lookup can run before constructing
the last date. In the 3.1 comparison, attaching an offset to an unzoned local
value establishes its instant; the subsequent UTC conversion preserves that
instant. These are two distinct uses of timezone adjustment.

## 11. Consequences

The report remains correct for common and leap February and 30-day months.
Explicit offsets make both displayed instants deterministic. The function does
not silently repair invalid input, derive a timezone from geography, or decide
whether the configured cutoff is a working day.

## 12. Implementation

Use `CompileOptions.Compatibility40` for the new functions; default XPath 3.1
rejects them. The earlier-version comparison uses `Compatibility31`.
The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks exact text/error markers and default-profile refusal for successful 4.0
cases. All inputs are inline; no current date, external variables or context
item are needed.

**Supplementary contract review (2026-10-12):** [Date/time review](../DATE-TIME-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed receipt-retention README](../examples/date-contracts/README.md) supply 108 checks across five families: keywords, Gregorian field sets, offsets, units, empty/default, precision and type/cardinality boundaries, plus complete XPath 3.1/4.0 reports. Keywords remain unavailable. Malformed/multi-item tolerance, record conversion and fractional-component loss remain engine follow-ups. Source/public version is 1.1.3; original delivery/unit/gate and historical adopted-pin limits remain.

## 13. Sample Code

Complete synthetic schedule input:

```xml
<schedule year="2024" month="2" hours="17" minutes="30" timezone="PT2H"/>
```

[Complete XPath 4.0 expression](../examples/month-end-cutoffs/main.xpath):

```xpath
let $schedule := parse-xml('<schedule year="2024" month="2" hours="17" minutes="30" timezone="PT2H"/>')/schedule
let $year := xs:integer($schedule/@year)
let $month := xs:integer($schedule/@month)
let $days := days-in-month(build-dateTime(map {'year': $year, 'month': $month, 'day': 1}))
let $cutoff := build-dateTime(map {'year': $year, 'month': $month, 'day': $days, 'hours': xs:integer($schedule/@hours), 'minutes': xs:integer($schedule/@minutes), 'seconds': 0, 'timezone': xs:dayTimeDuration($schedule/@timezone)})
return concat('days=', $days, '; cutoff=', $cutoff, '; utc=', adjust-dateTime-to-timezone($cutoff, xs:dayTimeDuration('PT0S')))
```

Exact output:

```text
days=29; cutoff=2024-02-29T17:30:00+02:00; utc=2024-02-29T15:30:00Z
```

[Complete XPath 3.1 comparison](../examples/month-end-cutoffs/baseline.xpath):

```xpath
let $schedule := parse-xml('<schedule year="2024" month="2" hours="17" minutes="30" timezone="PT2H"/>')/schedule
return let $year := xs:integer($schedule/@year)
return let $month := xs:integer($schedule/@month)
return let $first := xs:date(concat(format-integer($year, '0000'), '-', format-integer($month, '00'), '-01'))
return let $last := $first + xs:yearMonthDuration('P1M') - xs:dayTimeDuration('P1D')
return let $local := dateTime($last, xs:time(concat(format-integer(xs:integer($schedule/@hours), '00'), ':', format-integer(xs:integer($schedule/@minutes), '00'), ':00')))
return let $cutoff := adjust-dateTime-to-timezone($local, xs:dayTimeDuration($schedule/@timezone))
return concat('days=', day-from-date($last), '; cutoff=', $cutoff, '; utc=', adjust-dateTime-to-timezone($cutoff, xs:dayTimeDuration('PT0S')))
```

Exact output:

```text
days=29; cutoff=2024-02-29T17:30:00+02:00; utc=2024-02-29T15:30:00Z
```

### Common-year boundary

Full input changes the year to 2023:

```xml
<schedule year="2023" month="2" hours="17" minutes="30" timezone="PT2H"/>
```

The [common-year expression](../examples/month-end-cutoffs/common-year.xpath) reports:

```text
days=28; cutoff=2023-02-28T17:30:00+02:00; utc=2023-02-28T15:30:00Z
```

### Complete fixture inventory

Each linked file contains its full expression and input.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/month-end-cutoffs/main.xpath) | `days=29; cutoff=2024-02-29T17:30:00+02:00; utc=2024-02-29T15:30:00Z` |
| [baseline](../examples/month-end-cutoffs/baseline.xpath) | `days=29; cutoff=2024-02-29T17:30:00+02:00; utc=2024-02-29T15:30:00Z` |
| [common-year](../examples/month-end-cutoffs/common-year.xpath) | `days=28; cutoff=2023-02-28T17:30:00+02:00; utc=2023-02-28T15:30:00Z` |
| [thirty-day-month](../examples/month-end-cutoffs/thirty-day-month.xpath) | `days=30; cutoff=2024-04-30T17:30:00+02:00; utc=2024-04-30T15:30:00Z` |
| [negative-offset](../examples/month-end-cutoffs/negative-offset.xpath) | `days=29; cutoff=2024-02-29T17:30:00-05:00; utc=2024-02-29T22:30:00Z` |
| [century-common](../examples/month-end-cutoffs/century-common.xpath) | `28` |
| [century-leap](../examples/month-end-cutoffs/century-leap.xpath) | `29` |
| [year-zero-month](../examples/month-end-cutoffs/year-zero-month.xpath) | `29` |
| [local-month](../examples/month-end-cutoffs/local-month.xpath) | `31` |
| [empty-month](../examples/month-end-cutoffs/empty-month.xpath) | `0` |
| [empty-build](../examples/month-end-cutoffs/empty-build.xpath) | `0` |
| [fractional-time](../examples/month-end-cutoffs/fractional-time.xpath) | `13:30:04.25` |
| [date-only](../examples/month-end-cutoffs/date-only.xpath) | `2024-02-29` |
| [without-timezone](../examples/month-end-cutoffs/without-timezone.xpath) | `dateTime=true; stamp=false; timezone=false` |
| [stamp-type](../examples/month-end-cutoffs/stamp-type.xpath) | `true` |
| [invalid-fields](../examples/month-end-cutoffs/invalid-fields.xpath) | `FODT0005` |
| [invalid-day](../examples/month-end-cutoffs/invalid-day.xpath) | `FORG0001` |
| [invalid-offset](../examples/month-end-cutoffs/invalid-offset.xpath) | `FODT0003` |
| [midnight-24](../examples/month-end-cutoffs/midnight-24.xpath) | `FORG0001` |

Century cases distinguish 1900 from 2000. The local-month case retains March's
31 days even though its instant is in February UTC. Other cases verify empty
inputs, date/time output forms, fractional seconds, stamp versus unzoned type,
invalid field sets, calendar dates, timezone offsets and the midnight-hour rule.

## 14. Performance

Month-length lookup and fixed component construction are small deterministic
operations. There is no network acquisition, timezone-database lookup or scheduling
loop. The example parses one small inline record; no benchmark claim is implied.

## 15. Known Uses

This invoice-calendar rule is synthetic, not an identified deployment. Existing evidence includes
[`DaysInMonth_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`DaysInMonth_ProlepticGregorianYearZeroIsLeap`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildDateTime_SpecDateTimeStampExample`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildDateTime_AllSevenComponentsWithoutTimezone_IsDateTime`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildDateTime_TimeWithFractionalSeconds`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildDateTime_InvalidFieldSet_RaisesFodt0005`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`BuildDateTime_TimezoneOutOfRange_RaisesFodt0003`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`BuildDateTime_OutOfRangeComponent_RaisesForg0001`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
has no exact fn-build-dateTime or fn-days-in-month assignment in GatedSets.
Named unit tests and these 19 examples provide separate evidence. No gate is promoted here.

## 16. Related Features

[Timestamp deadlines](timestamp-deadlines.md) covers elapsed durations and epoch
units. [Bounded retry state](bounded-retries.md) covers attempt budgets rather
than calendar instants. [The completeness audit](../COMPLETENESS-AUDIT.md)
tracks reference names separately from exhaustive signature conformance.
