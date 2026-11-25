# Calendar interoperability contract

25 November 2026. Recorded before implementation at 6142be76487f928c7efea27598287d384a4f23ba. Normative dated revisions and byte hashes: [ADOPTION.json](ADOPTION.json).

Provide explicit Read, CompareXPath, CompareXsd, SameKey and GetSameKeyHashCode for plain legacy/exact XdmValue calendars, plus a separate CLR key comparer. Read returns validated exact component data, retaining stored precision and timezone presence. Legacy time erases its incidental embedded date. Millisecond storage does not recover discarded original DTO ticks.

Refuse all authority-bearing carriers, unknown or incompatible base labels, contradictory timezone flags, absent hidden offsets, malformed components, non-calendar values and default values. DateTimeStamp descriptive labels require timezone. Time and dateTime compare separately; same-key across valid families is false. Normalize valid 24:00 through the existing exact constructor. Descriptive user names grant no authority.

Keep default map equality, expression comparison, transport representation identity, legacy accessors and native Schema admissions unchanged. Typed query comparisons require a separate guarded profile and remain next work.

- C01: Legacy dateTime component projection.
- C02: Legacy time discards embedded reference date.
- C03: Legacy millisecond precision is preserved without inventing ticks.
- C04: Exact payload is preserved at full precision.
- C05: Timezone presence and offset preservation.
- C06: Contradictory wrapper and component timezone refusal.
- C07: Absent timezone with hidden offset refusal.
- C08: Known family annotations and descriptive user names.
- C09: Unknown or incompatible base annotation refusal.
- C10: Malformed clock and millisecond refusal.
- C11: Gregorian and extended-year validation.
- C12: End-of-day normalization and overflow refusal.
- C13: Mixed XPath comparison with explicit implicit timezone.
- C14: Mixed XSD partial ordering.
- C15: Mixed same-key equivalence and hash laws.
- C16: Explicit CLR mixed calendar dictionary comparer.
- C17: Default, non-calendar and cross-family refusal.
- C18: Existing getters, maps and default operators unchanged.
- C19: Plain projection grants no authority and never rebinds carriers.
- C20: Public live and expired authority carriers refuse projection.
