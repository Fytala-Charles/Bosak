# K04-C18 proposed scope: correct shared empty date-formatting results

28 October 2026. Prerequisite to the separate 4.0 optional-argument implementation; no implementation is included in K04-C17.

## First: existing 2/5-argument empty-result correction

1. Record pre-implementation semantic adoption against the dated F&O 3.1 common formatting rules and the frozen qt4cg 4.0 rules, with immutable revisions/hashes and intentional limits. The C17 review pin does not itself adopt semantics.
2. Distinguish an empty value from a nonempty date/dateTime/time and return a true XDM empty result. Handle the observed zero-item sequence wrapper and undefined representation consistently. Verify optional return metadata so empty-result behavior survives compilation and evaluation; returning an empty string is insufficient.
3. Cover all three functions, existing arities 2/5, 3.1/4.0/Experimental and shared XSLT mode. Assert both empty(result) and string(result), with nonempty formatting contrasts, optional argument typing, invalid-picture/calendar boundaries, named references and dynamic calls. State argument-validation/error precedence explicitly rather than inferring it from an absent overload.
4. Update the nine existing CalendarFallbackTests rows that deliberately preserve XPTY0004, with dated history and a reference to closure of that independent defect. Preserve all original C15 logs/source snapshots and C16 acceptance receipts. Add real correctness tests; do not merely change expected error codes or bypass comparison.
5. Run focused before/after regressions, the complete solution, both exact normal gates and the affected exploratory sets. Keep QT3 **31,164/0/657**, qt4 gate **8,587/0/1,892**, retained skip names/reasons and admission policy unchanged. The eight optional-arity originals may still fail until the subsequent slice; report that honestly. Any unexpected gate delta requires investigation, not automatic baseline replacement.

## Then: separate 4.0 call-surface implementation

Adopt the frozen optional signatures before implementing six arity 3/4 registrations with 4.0-only profile guards. Preserve existing 3.1 arities and refusal of 3/4. Supply precise parameter/optional return metadata, existing argument conversion and defaults for omitted trailing strings. Add keyword entries for value, picture, language, calendar, place with required first two/default () trailing parameters; do not register permissive untyped stubs.

Test all three functions via positional calls, named references, function-lookup and reordered keyword calls, including omitted/empty language/calendar/place, empty values, valid/invalid calendars, node atomization, wrong types and unsupported arities. Re-execute all eight original fixtures unchanged and all C17 controls. Check public 3.1/4.0/Experimental and shared XSLT behavior.

Only after fresh complete sets show zero failures may their GatedSets promotion be reviewed. Keep remaining dependency skips and known limitations visible. There is no release, upstream contribution, fixture correction, schema backend, broad language admission or normalization change in either bounded scope. Versioning must follow the delivered change: an empty-result bug fix and an optional-call feature are separate decisions.
