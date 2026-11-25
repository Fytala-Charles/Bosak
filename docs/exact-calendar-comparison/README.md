# Exact calendar comparison batch

25 November 2026. Twenty related contracts add explicit comparison and exact-only CLR key APIs on top of the atomic transport foundation. [Contract](CONTRACT.md), [immutable adoption](ADOPTION.json) and [API review](API-REVIEW.md) separate the new value semantics from representation identity and authority.

`XdmExactCalendarComparison.CompareXPath` requires an explicit implicit timezone; `CompareXsd` returns a four-way partial-order result. DateTimeStamp shares dateTime's comparison family. Time uses a common1972-12-31 reference date, retaining day displacement after timezone normalization. Mixed timezone XSD pairs can be indeterminate, including equality at only one uncertainty endpoint.

`SameKey`, `GetSameKeyHashCode` and `XdmExactCalendarKeyComparer.Instance` support explicit CLR dictionaries of exact calendar objects. Matching timezone presence is required; dateTimeStamp/dateTime use one family, while time is separate. Hashes normalize exact timeline seconds and are process-local, not serialization or authority fingerprints. Ordinary component equality and private marker binding remain unchanged even for equal instants.

```csharp
var a = XdmExactCalendarValue.Parse("2000-01-01T12:00:00.123456789Z", XdmExactCalendarKind.DateTime, 64, 32);
var b = XdmExactCalendarValue.Parse("2000-01-01T13:00:00.123456789+01:00", XdmExactCalendarKind.DateTimeStamp, 64, 32);
int order = XdmExactCalendarComparison.CompareXPath(a, b, implicitTimezoneMinutes: 0); // 0
var keys = new Dictionary<XdmExactCalendarValue, string>(XdmExactCalendarKeyComparer.Instance);
keys[a] = "first";
keys[b] = "replacement"; // same key; original objects keep their distinct components
```

Source1.84.0 is unpublished. Existing XdmValue map guards, default expression operators, legacy interop, arithmetic, UTC canonicalization, native Schema datatypes/facets and C45 acceptance are unchanged. General engine integration requires a separate exact/legacy/timezone-context contract. No registry/tag publication or signed/Schema pin migration. Qualification results are recorded before delivery; deterministic oracle iterations are not counted as independent specification cases.

Qualification:26,634solution/111language-server/44independent package cases pass, as do pinned Schema260+9and both examples. There are44new source cases(43Core/1Runtime); the1,000deterministic oracle iterations are assertions, not separate coverage cases. FrozenQT3 stays31,397/0/424; gatedqt4 8,941/0/1,918. The Core API preserves592rows and adds15rows in3new public types. Nine local package DLLs match source; all consumed runtime DLLs match. [Results](QUALIFICATION.json), [review](REVIEW.json), [status](STATUS.json).
