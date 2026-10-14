<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak timestamp and retention contract reference">
  <br><br><h1>Date and Time Construction Contracts</h1>
  <p>Gregorian components, epoch milliseconds and retention seconds</p>
</div>

**Primitive numeric coercion - 2026-10-14:** Source candidate 1.3.2 supports the permitted decimal/float/double casts in XPath and ordinary XSLT 4.0 function arguments, including defaults. Double-to-integer remains invalid. The previous broader changelog interpretation is corrected: [PR #32](https://github.com/qt4cg/xslt40-test/pull/32) retains its original 903 fixture and updates only the error expectation. [Contract and evidence](../XSLT40-PRIMITIVE-NUMERIC-COERCION.md). Published libraries remain 1.2.1 and extension 0.1.6; source 1.3.2 is unpublished. Earlier milestone entries below are historical.

## Scope and outcome

Reviewed 2026-10-12 against Bosak `675d8f96facf1fc72ed3a05cb2781fa493fd1f01`, published version 1.1.3. Five Frozen 4.0 families have 108 new representative checks: 15 explicit keyword/mode refusals, 87 focused Gregorian/timezone/unit/type/cardinality checks and six complete normal/empty/reordered receipt-retention reports in XPath 3.1/4.0. **32 of the original 37 absent-keyword families are reviewed; five remain.** Metadata is unchanged. REQ-126 stays In Progress, D2 partial. Catalog: 67 entries, 1,988 feature + 24 chapter checks = 2,012; four worked chapters. No engine/API/harness change, gate promotion or release.

Primary contract: [immutable QT4CG F&O catalog e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml). The [raw comparison](audit-signature-contracts.json) links exact case IDs and now preserves the build-dateTime argument's type-ref and seven record fields; its original null direct type attribute was not an untyped argument. Review pins do not reconstruct historical adopted snapshots. Implementation: [FunctionLibrary](../../src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs), BuildDateTime, DaysInMonth, UnixDateTime, Seconds_1 and DurationToSeconds.

## Declared contracts and keyword availability

| Function | Pinned parameter/default | Result |
|---|---|---|
| `fn:build-dateTime` | `value: fn:dateTime-record?` | `(xs:dateTime | xs:date | xs:time | xs:gYear | xs:gYearMonth | xs:gMonth | xs:gMonthDay | xs:gDay)?` |
| `fn:days-in-month` | `value: (xs:dateTime | xs:date | xs:gYearMonth)?` | `xs:integer?` |
| `fn:duration-to-seconds` | `value: xs:dayTimeDuration?` | `xs:decimal?` |
| `fn:seconds` | `value: xs:decimal?` | `xs:dayTimeDuration?` |
| `fn:unix-dateTime` | `value: xs:nonNegativeInteger? := 0` | `xs:dateTimeStamp` |

fn:dateTime-record has optional integer year/month/day/hours/minutes, optional decimal seconds and optional dayTimeDuration timezone fields. Not every record combination constructs a Gregorian value. All five tested keyword forms refuse XPST0003 at 3.1 grammar and XPST0017 at 4.0/experimental binding. Their positional registrations remain available in Frozen 4.0; experimental does not supply metadata.

## Gregorian fields, offsets and precision

build-dateTime returns empty for empty input, and FODT0005 for an empty map or year/day without month. Tested year, year/month, date, month, month/day, day, time and dateTime sets produce their respective Gregorian values. A complete timestamp plus timezone is a dateTimeStamp; without timezone, no offset is invented. An unused empty day does not disrupt a time-only record. Invalid February 29, hour 24 and seconds 60 refuse FORG0001. +14 hours works; +14 hours 1 minute and a 1-second timezone refuse FODT0003.

Integral numeric components, tested string/untyped fields and month 2.9 are accepted (the last truncates to 2); unknown field 'unknown' is ignored. These observed outcomes require full record-coercion policy review before generalizing. A multi-item year refuses XPTY0004. The tested [] argument returns empty and [map{'year':2024}] returns 2024, consistent with array atomization at the typed dispatch boundary; an integer refuses XPTY0004. Do not treat arrays as advertised record arguments.

**Precision follow-up:** supplying 4.2678 seconds produces time text 12:34:04.267; seconds-from-time and seconds-from-dateTime both report 4.267 in the tested forms. This establishes fractional-component loss beyond formatting alone. The business report uses whole milliseconds and a half-second retention value, so it does not depend on the lost precision. Year-zero date/dateTime and year-10000 date construction succeed in these fixtures. Earlier family documentation's blanket pre-year-1 limit is superseded for these observed forms; not every extended-year operation is certified. [XPathDateTime](../../src/Bosak.XPath.Core/Xdm/XPathDateTime.cs) already stores extended years; conversion to DateTimeOffset remains bounded to years 1–9999 and raises FODT0001 outside them. AGENTS.md now distinguishes this interop limit from Gregorian construction.

days-in-month applies the local year/month: March 1 at +02:00 returns 31 even though its UTC instant falls in February. February 2024/2000 returns 29, 1900 returns 28, and year-zero/negative-year gYearMonth fixtures follow the Gregorian rule. Empty input returns empty. Time/map inputs refuse XPTY0004. Plain/untyped '2024-02', malformed '2024-02garbage', and the tested two-date sequence are accepted, returning 29. The latter uses the first item. Type/lexical/cardinality enforcement remains an explicit engine follow-up; use validated Gregorian values in consumer code.

## Epoch units and duration totals

unix-dateTime uses milliseconds, not seconds: 1 gives 1970-01-01T00:00:00.001Z; 86400000 gives the next day. Omission and () mean epoch zero, not now. Stamp subtype and UTC are verified. Negative input refuses FOCA0002; the tested timestamp beyond year 9999 refuses FODT0001. Decimal 1.9 truncates to 1; string/untyped/double 1 and (1,2) are accepted, with the sequence selecting its first item. Map input refuses FOTY0013. Review conversion/cardinality enforcement separately from the validated nonnegative integer workflow.

seconds and duration-to-seconds preserve empty input, signs and tested fractional values. seconds(86400) is P1D; seconds(-5400) is -PT1H30M. duration-to-seconds(PT1M30S) gives 90, not the seconds component 30; P1DT0.5S gives 86400.5. The 90.125 roundtrip succeeds. Tested string/boolean/multi-item/map seconds arguments refuse XPTY0004, while untyped '1.25' converts. Source candidate 1.3.2 also coerces the double 1.25e0 to decimal, so seconds(1.25e0) returns PT1.25S under XPath 4.0; the previous double refusal is historical. The very large seconds count refuses FODT0002. Tested yearMonthDuration/general duration/string/multiple/map total-seconds arguments refuse XPTY0004, while untyped 'PT90S' converts. These examples do not establish unlimited decimal or duration precision.

## Workflow and remaining work

The [detailed receipt-retention README](examples/date-contracts/README.md) provides complete XML, paired code, exact outputs and every fixture. [Month-end cutoffs](functions/month-end-cutoffs.md) and [timestamp deadlines](functions/timestamp-deadlines.md) retain their original delivery/unit/gate evidence. These diagnostics are not another worked chapter.

Next review the final five absent-keyword families: build-uri, decode-from-uri, collation-available, identity and is-NaN, with validated import/link diagnostics. Missing metadata is still missing after targeted review. Complete function conversion/options, exhaustive changed-3.1 inventory, D3 grammar/pattern and D4 provenance/acceptance remain; historical adopted pins and AC-03 applicability are unresolved. No closure or release based on sample coverage.

## Delivery validation

Release solution build passed (12 existing warnings, zero errors); 6,216 tests passed with no failures or skips across ten assemblies. Render checks verified 97 catalog pages and six implementation/canonical pages. Five pinned signatures/record fields, exact case IDs, independent full report outputs, complete README expressions, local links, 67 sixteen-field entries, 241 cached primary links/anchors, 28 historical delivery records and 117 assigned names were checked. No manual engine gates were rerun for this documentation/example scope.


The subsequent [URI/import utility review](URI-UTILITY-CONTRACT-REVIEW.md) completes first targeted review for all 37 absent-keyword families, with none remaining unreviewed. All 37 still lack metadata. Current catalog: 67 entries, 2,093 feature + 24 chapter checks = 2,117, version 1.1.3. Earlier counts and queues are historical. Next: exhaustive changed-existing-3.1 contract inventory; D2 stays partial.
