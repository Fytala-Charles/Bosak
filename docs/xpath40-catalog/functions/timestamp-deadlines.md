<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: timestamp units and processing deadlines">
  <br><br><h1>Timestamp Deadlines</h1>
  <p>Frozen 4.0 — epoch milliseconds, duration seconds and elapsed budgets</p>
</div>

**Checked integer arithmetic — 24 October 2026:** [Correction and limits](../INTEGER-PROMOTION.md) prevent folded binary arithmetic wraparound and preserve integer identity through decimal-backed addition/subtraction/multiplication, with42 profile/call-path regression cases. General casts, unary/division and Unix-time wide conversion remain separate. Source1.25.1 unpublished/public1.25.0 accepted; REQ-126 stays partial.



**Wide-integer Unix-time investigation — 24 October 2026:** [Evidence and next fix](../UNIX-DATETIME-WIDE-INTEGER.md) add24 boundary tests and74 observed diagnostics. Valid wide literals fail Int64 conversion before date arithmetic; constant folding also wraps while runtime promotion loses integer typing. Defects remain explicit; no engine/version/release change. Source1.25.1 unpublished, public1.25.0 accepted; REQ-126 stays partial.



**Experimental signed Unix epochs — 24 October 2026:** [Implementation and limits](../UNIX-DATETIME-EXPERIMENTAL.md) record the opt-in correction, 27 new source tests, 11,407 passing tests and unchanged frozen gates. Source 1.25.1 is unpublished; public/Braid 1.25.0 remains current. REQ-126 stays partial.



## 1. Name and Classification

**Timestamp deadlines — Frozen 4.0.** [fn:unix-dateTime](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-unix-dateTime), [fn:seconds](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-seconds), and [fn:duration-to-seconds](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-duration-to-seconds).
Initial engine delivery: [PR #84](https://github.com/Fytala-Charles/Bosak/pull/84),
[revision f7f55b4e](https://github.com/Fytala-Charles/Bosak/commit/f7f55b4e3a6e92da26ac607e34f46aede695cb19), REQ-118 4.0-S2.
Definitions reviewed **2026-10-11** against immutable
[QT4CG function source a014e1b4](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xpath-functions-40/src/function-catalog.xml).
This review pin does not recover the unavailable historical adopted specification snapshot.

**Boundary review — 24 October 2026:** [Unix-time contract and current-draft comparison](../UNIX-DATETIME-CONTRACT.md) add52 source checks without changing the engine or the nonnegative frozen contract. Explicit current negative-epoch and wide-integer diagnostics remain unresolved; source/public1.25.0. Earlier source/public1.1.3 labels below are historical.

## 2. Intent

Turn a receipt's epoch timestamp and measured processing time into UTC instants,
then report elapsed and remaining seconds against a configured processing budget.
Keep milliseconds and seconds explicit so the deadline is not scaled incorrectly.

## 3. Also Known As

Epoch conversion and elapsed-budget accounting are descriptive terms.
`seconds` constructs a duration; `seconds-from-duration` extracts a component
and is not an alias for the total-seconds function.

## 4. History / Supersedes

XPath 3.1 adds a scaled `xs:dayTimeDuration` to the UTC Unix epoch. Multiplying
`PT1S` by a seconds count builds a duration, and dividing by `PT1S` gets its total
seconds. The complete comparison returns the same report. Its epoch addition
returns a dateTime; matching report text does not prove the same stamp subtype.

## 5. Motivation

A receipt log often stores epoch milliseconds while service budgets use seconds.
A 1,250-millisecond processing measurement is 1.25 seconds, not 1,250 seconds.
A duration containing minutes also needs total seconds; taking just its seconds
component would understate elapsed time. The report exposes both unit conversions.

## 6. Language Alignment

`unix-dateTime` uses **nonnegative epoch milliseconds**, returning a UTC
`xs:dateTimeStamp` in Bosak. Omitted or empty input means zero (the epoch),
not the current time. A negative argument raises FOCA0002. Unix time assumes
86,400-second days and does not account for leap seconds.

`seconds` accepts an optional decimal count, returning a day-time duration with
fractional and negative values preserved. `duration-to-seconds` returns an
optional decimal total for a day-time duration. Both return empty for empty input.
For example PT1M30S has total 90 seconds but a seconds component of 30.
Year-month durations cannot be converted to a fixed seconds count by this rule.

## 7. Applicability

Use for explicit epoch-unit conversion and elapsed-budget reports from supplied
measurements. The fixture assumes nonnegative integer epoch milliseconds,
nonnegative elapsed milliseconds, and a nonnegative decimal seconds budget.
Negative remaining time is allowed: it reports an overrun. These are measured
values, not a clock read. The host owns monotonic measurement, actual cancellation
and timeout enforcement; this XPath merely calculates a report.

The supported Gregorian date range and decimal precision remain implementation
limits. The examples do not establish unlimited timestamp or duration ranges.

## 8. Structure

Convert receipt milliseconds to a received UTC instant. Divide measured milliseconds
by 1,000 before building an elapsed duration. Build a budget duration directly
from seconds. Add both to received time, subtract finished time from the deadline,
and project total seconds for the report.

## 9. Participants

The receipt supplies timestamp, elapsed measurement and budget with named units.
The epoch converter creates the received instant. Duration functions normalize
units. DateTime arithmetic derives finished/deadline instants. The comparison
reports whether completion is within the budget.

## 10. Collaborations

All instants use explicit UTC, avoiding implicit-timezone dependence. The report
uses `$finished le $deadline`, so exact equality is within budget. Preserve signed
remaining seconds to distinguish unused allowance from an overrun.

## 11. Consequences

Fractional measurements survive the conversion. A negative remaining duration
signals lateness without losing its magnitude. None of these functions sleeps,
retries a request, cancels processing or checks the current wall clock.
Calling `unix-dateTime()` always returns the epoch, which must not be mistaken
for a receipt's missing timestamp.

## 12. Implementation

Use `CompileOptions.Compatibility40` for the new calls. The default 3.1 profile
rejects them; the comparison uses `Compatibility31`. The existing
[catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
checks exact reports/errors and default-profile refusal for successful 4.0 cases.
All values are inline and deterministic; no external context or clock is needed.

**Supplementary contract review (2026-10-12):** [Date/time review](../DATE-TIME-CONTRACT-REVIEW.md), pinned to F&O e7ee4ab8, and [detailed receipt-retention README](../examples/date-contracts/README.md) supply 108 checks across five families: keywords, Gregorian field sets, offsets, units, empty/default, precision and type/cardinality boundaries, plus complete XPath 3.1/4.0 reports. Keywords remain unavailable. Malformed/multi-item tolerance, record conversion and fractional-component loss remain engine follow-ups. Source/public version is 1.1.3; original delivery/unit/gate and historical adopted-pin limits remain.

## 13. Sample Code

Complete synthetic receipt input:

```xml
<receipt epochMilliseconds="1706702400000" elapsedMilliseconds="1250" budgetSeconds="2.5"/>
```

[Complete XPath 4.0 expression](../examples/timestamp-deadlines/main.xpath):

```xpath
let $receipt := parse-xml('<receipt epochMilliseconds="1706702400000" elapsedMilliseconds="1250" budgetSeconds="2.5"/>')/receipt
let $received := unix-dateTime(xs:integer($receipt/@epochMilliseconds))
let $elapsed := seconds(xs:decimal($receipt/@elapsedMilliseconds) div 1000)
let $budget := seconds(xs:decimal($receipt/@budgetSeconds))
let $finished := $received + $elapsed
let $deadline := $received + $budget
return concat('received=', $received, '; finished=', $finished, '; deadline=', $deadline, '; elapsedSeconds=', duration-to-seconds($elapsed), '; remainingSeconds=', duration-to-seconds($deadline - $finished), '; withinBudget=', $finished le $deadline)
```

Exact output:

```text
received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:01.25Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=1.25; remainingSeconds=1.25; withinBudget=true
```

[Complete XPath 3.1 comparison](../examples/timestamp-deadlines/baseline.xpath):

```xpath
let $receipt := parse-xml('<receipt epochMilliseconds="1706702400000" elapsedMilliseconds="1250" budgetSeconds="2.5"/>')/receipt
return let $received := xs:dateTime('1970-01-01T00:00:00Z') + xs:dayTimeDuration('PT0.001S') * xs:integer($receipt/@epochMilliseconds)
return let $elapsed := xs:dayTimeDuration('PT1S') * (xs:decimal($receipt/@elapsedMilliseconds) div 1000)
return let $budget := xs:dayTimeDuration('PT1S') * xs:decimal($receipt/@budgetSeconds)
return let $finished := $received + $elapsed
return let $deadline := $received + $budget
return concat('received=', $received, '; finished=', $finished, '; deadline=', $deadline, '; elapsedSeconds=', ($elapsed div xs:dayTimeDuration('PT1S')), '; remainingSeconds=', (($deadline - $finished) div xs:dayTimeDuration('PT1S')), '; withinBudget=', $finished le $deadline)
```

Exact output:

```text
received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:01.25Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=1.25; remainingSeconds=1.25; withinBudget=true
```

### Budget overrun

Full input increases measured processing to three seconds:

```xml
<receipt epochMilliseconds="1706702400000" elapsedMilliseconds="3000" budgetSeconds="2.5"/>
```

The [over-budget expression](../examples/timestamp-deadlines/over-budget.xpath) reports:

```text
received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:03Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=3; remainingSeconds=-0.5; withinBudget=false
```

### Complete fixture inventory

Every file embeds its full input and exact result or error marker.

| Runnable case | Exact result or diagnostic marker |
|---|---|
| [main](../examples/timestamp-deadlines/main.xpath) | `received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:01.25Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=1.25; remainingSeconds=1.25; withinBudget=true` |
| [baseline](../examples/timestamp-deadlines/baseline.xpath) | `received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:01.25Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=1.25; remainingSeconds=1.25; withinBudget=true` |
| [over-budget](../examples/timestamp-deadlines/over-budget.xpath) | `received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:03Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=3; remainingSeconds=-0.5; withinBudget=false` |
| [exact-budget](../examples/timestamp-deadlines/exact-budget.xpath) | `received=2024-01-31T12:00:00Z; finished=2024-01-31T12:00:02.5Z; deadline=2024-01-31T12:00:02.5Z; elapsedSeconds=2.5; remainingSeconds=0; withinBudget=true` |
| [epoch-default](../examples/timestamp-deadlines/epoch-default.xpath) | `1970-01-01T00:00:00Z` |
| [epoch-empty](../examples/timestamp-deadlines/epoch-empty.xpath) | `1970-01-01T00:00:00Z` |
| [one-millisecond](../examples/timestamp-deadlines/one-millisecond.xpath) | `1970-01-01T00:00:00.001Z` |
| [one-day](../examples/timestamp-deadlines/one-day.xpath) | `1970-01-02T00:00:00Z` |
| [negative-duration](../examples/timestamp-deadlines/negative-duration.xpath) | `-PT1H30M` |
| [fractional-duration](../examples/timestamp-deadlines/fractional-duration.xpath) | `PT0.001S` |
| [zero-duration](../examples/timestamp-deadlines/zero-duration.xpath) | `PT0S` |
| [empty-seconds](../examples/timestamp-deadlines/empty-seconds.xpath) | `0` |
| [empty-total](../examples/timestamp-deadlines/empty-total.xpath) | `0` |
| [total-versus-component](../examples/timestamp-deadlines/total-versus-component.xpath) | `total=90; component=30` |
| [duration-roundtrip](../examples/timestamp-deadlines/duration-roundtrip.xpath) | `true` |
| [stamp-type](../examples/timestamp-deadlines/stamp-type.xpath) | `true` |
| [negative-epoch](../examples/timestamp-deadlines/negative-epoch.xpath) | `FOCA0002` |

The exact-budget fixture reports zero remaining seconds and true. Epoch-default
and epoch-empty verify the same zero default. One millisecond distinguishes the
epoch function's unit from seconds. Signed/fractional duration cases and the
roundtrip verify total-seconds conversion, and empty cases show absence rather
than zero. The negative-epoch case verifies the nonnegative input restriction.

## 14. Performance

This is fixed-size numeric and dateTime arithmetic with one small inline record.
It does not run a timer or allocate a schedule. No benchmark is implied; decimal
precision and the processor's supported date range bound representable values.

## 15. Known Uses

This receipt-accounting scenario is synthetic. Existing evidence includes
[`UnixDateTime_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`UnixDateTime_LargeTimestamp`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`UnixDateTime_NegativeValue_RaisesFoca0002`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`Seconds_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`Seconds_EmptySequence_ReturnsEmpty`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`DurationToSeconds_SpecExamples`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
and [`SecondsAndDurationToSeconds_AreInverses`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
assigns exact fn-unix-dateTime to GatedSets. There is no exact fn-seconds or
fn-duration-to-seconds assignment: fn-seconds-from-duration/from-time are different
functions. Named tests and these 17 examples provide separate evidence; no gate
membership changes are included.

The additional [`UnixDateTimeContractTests`](../../../tests/Bosak.XPath.Standard.Tests/UnixDateTimeContractTests.cs) verify `SupportedMilliseconds_AreExact`, `Default_IsEpochRatherThanNow`, `ResultTypeAndUtc_AreIndependentOfImplicitTimezone` and complete `DeadlineReport_MatchesXPath31Baseline` outcomes. Fresh full-set `fn-unix-dateTime` evidence and the four current-native failures are in the linked contract review; no gate promotion or new catalog-manifest cases are implied.

## 16. Related Features

[Month-end cutoffs](month-end-cutoffs.md) covers Gregorian calendars and numeric
offsets. [Bounded retry state](bounded-retries.md) uses finite attempt budgets.
[The completeness audit](../COMPLETENESS-AUDIT.md) tracks reference names and
remaining coverage independently of exhaustive conformance.
