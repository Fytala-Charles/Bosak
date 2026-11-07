# Wider primitive transport policy

The selected next implementation widens the existing complete-container read to the eleven kinds constructible by current public XdmValue factories. It adds Double, Float, Date, Time, DateTime, Duration and QName to the four present kinds. This policy step leaves the executable source/binaries and current four-family behavior unchanged.

Preserve existing plain carriers rather than serialize or rebuild them. Floats retain stored bits; QName/NOTATION retains the name/URI/prefix triple and tags; temporal values retain every stored field and both timezone flags without conversion through DateTimeOffset. Strings and wrappers are immutable in the pinned implementation. A future byte-buffer/native representation would need its own lifetime policy.

Uri and Binary enum members do not have public factories. Current URI/binary lexical values use annotated String carriers, which four-family complete reads can already carry. These observations do not establish byte-array binary support or all-thirteen-enum coverage. PV-T06 records the separate required representation disposition.

Structural rejection covers null string/duration payloads, missing/default/marked/non-atomic cells, missing evidence and malformed QName field structure. Full XML Schema facets, lexical/calendar/NCName/URI/binary validation remain provider/validation responsibilities. Transport preserves supplied state and authenticated source metadata; it does not independently prove validity, rebind prefixes or grant authority from type-name tags.

Complete admission, atom/numeric bounds, ordered list/union shape, exact raw lifetime and compatible serialized paired proof windows remain unchanged. Plain copied values survive while evidence expires. Aggregate retained-output or string-length budgets remain unaccepted. No scalar arithmetic/evaluation, construction, persistence or all-host expansion is selected here. PP01-PP18 are complete policy decisions; PPV01-PPV36 are planned, not newly passed tests.

Immutable basis: [XDM 3.1 Recommendation, 21 March 2017](https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/), sections2.7.1,2.7.5,2.7.7,3.3.1.2,3.3.2,3.3.3 and5.14. The borrowed host boundary, structural subset and explicit unsupported representation disposition are Bosak Core policy. Exact source hashes are recorded in ADOPTION.json before selection. Final owner/manifest/native/budget/R4-R7 acceptance remains pending.
