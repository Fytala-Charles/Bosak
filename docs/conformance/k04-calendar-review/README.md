# K04-C14: calendar fallback contract review ? 27 October 2026

**Review complete; implementation remains open.** Four unchanged calendar originals reproduce unexpected FOFD1340. Fifty-four additional controls across XPath 3.1 and 4.0 separate supported calendars, unsupported listed calendars, malformed/unknown names and namespaced fallback. All 58 bounded processes complete without timeout: 30 diagnostic assertions pass and 28 fail. Diagnostic failure is preserved; process exit zero does not mean assertion success.

## Reviewed contract

The immutable [F&O 3.1 Recommendation](https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#formatting-dates-and-times), sections 9.8.4.7?8, distinguishes calendar-name validity from calendar support. Names must be EQNames; unqualified names must use listed designators. CB and AH are listed. Supported calendars are implementation-defined. Fallback must identify the calendar actually used. Namespaced identifiers have implementation-defined meanings. Component C/E representations are implementation-defined. This review does not adopt new semantics. [Review pin](SPECIFICATION-REVIEW.json) identifies the dated source and exact retrieved-byte hash. The 4.0 observations use pinned original fixtures; affected 4.0 source sections must be pinned before implementing a shared semantic change.

## Actual observations

| Group | Checks | Pass / fail | Observation |
| --- | ---: | --- | --- |
| Original CB cases | 4 | 0 / 4 | FOFD1340 instead of original AD/month assertions |
| AD and ISO controls | 12 | 12 / 0 | Expected existing month/hour output |
| CB, AH and Q{}CB controls | 18 | 0 / 18 | FOFD1340 instead of fallback indication and formatted value |
| ZZ, 1bad and Q{}ZZ controls | 18 | 18 / 0 | Existing FOFD1340 preserved |
| Namespaced custom calendar controls | 6 | 0 / 6 | Formatting succeeds but lacks AD fallback indication |

The controls cover format-date, format-dateTime and format-time in both profiles. They require a calendar indication and the relevant month/hour, rather than asserting an exact localized fallback spelling. The fixture inventory finds four effective calendar declarations across the six date/time test sets: exactly the original negative CB cases. No admission change follows from this diagnostic bypass.

## Mechanism and next implementation

The preserved formatter source shows ValidateCalendar recognizing only AD/Gregorian/ISO/ISO8601, then rejecting every other unqualified calendar outside XSLT mode. It does not distinguish listed-but-unsupported CB from an invalid unlisted ZZ. Namespaced identifiers reach Gregorian formatting without an indication. Validation currently happens inside FormatMarker, where fallback text is attached to each formatted component. Literal-only and multiple-component pictures need explicit regression coverage before moving that responsibility.

Recommended K04-C15 scope:

1. Pin affected immutable 3.1 and 4.0 specification sections and record explicit adoption before changing semantics. Preserve historical review and REQ-126 provenance.
2. Separate recognized designators from implemented calendars. Implement bounded AD fallback for listed unsupported calendars, with an indication once per complete output; retain invalid-name errors and supported-calendar output.
3. Decide and document namespaced fallback consistently. Existing prefix parsing uses a placeholder URI without resolving static namespaces; record that separate limitation rather than claiming complete EQName resolution. Do not broaden accepted aliases or change case behavior incidentally.
4. Regress all three functions, both profiles, supported/unsupported/invalid/empty calendar arguments, literal-only/empty/multiple-component pictures and empty value/error boundaries. Check the shared XSLT mode, which currently allows unknown bare-name fallback; preserve it unless separately reviewed.
5. Re-execute the four original assertions and all controls, run the complete solution and frozen gates, and compare retained skip names/reasons. Admission remains separate: no QT3 exact-total update without the required owner disposition.

This review changes no engine, harness, filter, fixture, assertion, baseline, version or GatedSets. Source base 5da130d0; candidate 1.28.0 remains unpublished/public 1.26.3. Source-identical validation inherited from K04-C12 remains 22,198 solution passes, QT3 **31,162/0/659**, qt4 **8,587/0/1,892**; no fresh full-suite claim. Bosak.Schema owns staged XSD 1.1; REQ-126 remains Complete. Language admission and FULLY-NORMALIZED findings remain open separately.

[Execution receipts](execution.json), original requests, raw outputs, fixture hashes, control contract, formatter/driver source snapshots and binary identities preserve the diagnosis. Run `python docs/conformance/k04-calendar-review/validate-review.py` to verify identities and outcomes. `run-review.py` reproduces diagnostics with the recorded local driver; preserve committed receipts before any rerun.
