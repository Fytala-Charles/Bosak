# Exact calendar foundation batch

25 November 2026. Twenty bounded contracts are grouped into one cohesive additive Core change, with their immutable [adoption](ADOPTION.json) and [contract](CONTRACT.md). This advances the prerequisite identified by the previous Schema batch; it does not deliver twenty unrelated schema features.

`XdmExactSecond` stores normalized decimal seconds under explicit digit/scale capacities. `XdmExactCalendarValue` validates dateTime/time/dateTimeStamp components, Gregorian dates including year zero, timezone presence and offset, and 24:00 rollover. Invariant formatting preserves component offsets; it is not canonical UTC serialization. Structural equality is representation identity, not equality of instants. Lossless DTO and legacy conversions refuse unavailable precision, absent timezone and year-range limits.

These are opt-in value objects. No existing `XdmValue` factory, expression constructor, calendar arithmetic, schema authority or Schema datatype admission changes. Source version is the unpublished 1.82.0 candidate; signed 1.81.17 and Schema pins remain unchanged. Full transport, signed package delivery and Schema adoption remain separate work. No registry push or tag is authorized by this batch.

Example:

```csharp
var value = XdmExactCalendarValue.Parse(
    "2000-01-02T03:04:05.123456789Z",
    XdmExactCalendarKind.DateTime, maxDigits: 64, maxScale: 32);
string exact = value.ToInvariantString(128);
// ToDateTimeOffsetExact() refuses this 9-digit fraction; it never rounds it.
```

The twenty C01-C20 test groups cover capacities, ranges, normalization, grammar, bounded formatting, fractional equality/hash/order, exact ticks/milliseconds, Gregorian and extended-year limits, clock/timezone/end-of-day rules, three lexical families, formatting roundtrip, interoperability and component identity. Qualification results are recorded separately before delivery.

Qualification: all 26,539 distinct solution tests pass after the sandbox-only database-listener failures are resolved by a host retry (47/47); the original failed receipt is retained. All 111 language-server and 51 fresh-cache package-consumer cases pass. Frozen QT3 is exactly 31,397/0/424; gated qt4 is 8,941/0/1,918 with zero gated failures. Build succeeds with 32 existing warnings. [Machine-readable qualification](QUALIFICATION.json), [contract status](STATUS.json) and [API review](API-REVIEW.md) preserve the scope and retry boundary.
