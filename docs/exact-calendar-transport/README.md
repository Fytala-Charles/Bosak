# Exact calendar atomic transport batch

25 November 2026. Twenty contracts advance the previous exact calendar foundation through atomic storage and existing authenticated typed identity/container routes. [Adoption](ADOPTION.json), [contract](CONTRACT.md), [receiving-capacity addendum](CAPACITY-ADDENDUM.json) and [API review](API-REVIEW.md) distinguish implementation from broader datatype support. The twenty related contracts form one cohesive transport PR, not twenty unrelated Schema features.

`XdmValue.FromExactCalendar` preserves the validated family, fractional seconds, timezone presence and optional descriptive user annotation. `IsExactCalendar` and `ExactCalendarValue` make representation explicit. `ToString()` preserves exact normalized component text. Descriptive type names and plain public factories confer no authority.

Legacy DTO and XPathDateTime getters refuse exact carriers, even at millisecond precision. The map-key comparer refuses them because instant comparison and same-key hashing have not been adopted. Ordinary XdmValue equality remains component/annotation identity, not XPath instant equality. The existing legacy factories and getters remain compatible for their original payloads.

The existing selected-issuer, component/origin membership, original assessment, cache, cancellation and owner/snapshot lifetime rules remain mandatory. Typed reads apply the query's coefficient/scale limits to normalized stored exact seconds before publication. Public identity evaluation supports the already bounded `$value` profile and complete-container reads; it does not certify general calendar arithmetic, casts/functions, new typed construction or universal provider compatibility.

```csharp
var components = XdmExactCalendarValue.Parse(
    "2000-01-02T03:04:05.123456789Z", XdmExactCalendarKind.DateTime, 64, 32);
var atom = XdmValue.FromExactCalendar(components);
string exact = atom.ToString();
var preserved = atom.ExactCalendarValue;
// atom.DateTimeValue refuses this opt-in payload; no rounding occurs.
```

Source candidate 1.83.0 is unpublished. Signed packages, Schema pins and native dateTime/time/dateTimeStamp admissions remain unchanged. Full Schema validation, general calendar execution, UTC canonicalization and C45 acceptance remain separate work. Qualification receipts are recorded before delivery; no registry or tag publication is included.

Qualification: 26,590 distinct solution cases, 111 language-server cases, 42 independent package cases, pinned Schema integration260/public9 and both examples pass. There are51new source cases (35Core/16Runtime), not51new schema features. FrozenQT3 remains31,397/0/424; gatedqt4 is8,941/0/1,918. Core API has exactly three additive rows and no removals. All nine local package DLLs match source outputs; the seven consumed engine DLLs match the package consumer. Original runs and final comment-only rebuild/fresh-cache receipts are retained in [qualification](QUALIFICATION.json) and [review](REVIEW.json). No retained-binary claim or publication.
