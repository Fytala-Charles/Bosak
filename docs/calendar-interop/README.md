# Explicit calendar interoperability

The opt-in `XdmCalendarInterop` API reads plain legacy and exact calendar atoms into validated exact components. It offers mixed XPath comparison with a mandatory implicit timezone, XSD partial ordering and same-key equality/hash. `XdmCalendarInteropKeyComparer.Instance` supports a separate CLR calendar dictionary.

```csharp
var legacy = XdmValue.FromDateTime(new DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero));
var exact = XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse(
    "2000-01-02T03:04:05Z", XdmExactCalendarKind.DateTime, 64, 32));
bool equal = XdmCalendarInterop.CompareXPath(legacy, exact, 0) == 0;
```

Legacy time discards its embedded date. Only stored milliseconds can be recovered from the legacy payload; ticks previously discarded by its factory stay lost. Exact payload precision is retained within the existing coefficient/scale bounds. Inconsistent timezone representations and unsupported base labels refuse. All authority-bearing carriers refuse, whether live or expired; descriptive names grant no authority. Existing maps, getters and expression operators retain their behavior.

Twenty related contracts, not twenty new XSD features: [contract](CONTRACT.md), [immutable adoption](ADOPTION.json), [API review](API-REVIEW.md), [qualification](QUALIFICATION.json), [review](REVIEW.json), [status](STATUS.json). Source 1.85.0 is unpublished. Guarded typed-query comparisons and native Schema calendar admission remain separate next work. No registry/tag or signed-pin change.
