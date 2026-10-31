# K04-C18: true empty date-formatting results ? 28 October 2026

Existing 2- and 5-argument calls to format-date, format-dateTime and format-time now return a true XDM empty result for an empty value. The shared helper accepts both undefined and zero-item sequence representations; all six existing registrations declare the optional return type xs:string?. Nonempty formatting remains unchanged.

For example, `empty(format-date((), '[D]'))` now returns true, while string conversion still yields a zero-length string. The result itself is not a one-item empty string. Static calls, named references and function-lookup calls agree across 3.1/4.0/Experimental and shared XSLT mode.

## Adopted scope and precedence

[Pre-implementation adoption](evidence/IMPLEMENTATION-PIN.json) records the dated F&O 3.1 common formatting rules and immutable qt4cg revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e, affected sections and source hashes. This is separate from C17's review-only pin and historical REQ-126 acceptance.

Existing argument conversion and optional-string type checks remain before the helper. For well-typed arguments, an empty value returns empty before semantic picture/calendar parsing; malformed picture/calendar strings therefore do not replace that empty result. Wrong-type language/picture arguments retain XPTY0004. Nonempty values retain existing picture/calendar errors and formatting. No arity 3/4 registration, keyword metadata, general conversion rule, calendar policy or schema behavior changes.

Nine CalendarFallbackTests rows formerly preserved the known XPTY0004 defect. They now require the corrected result, with dated history. The C15 source snapshots/logs, C16 owner acceptance and C17 diagnostic failures remain byte-preserved historical evidence. [Scope clarification](evidence/SCOPE-CLARIFICATION.json) distinguishes these planned C# unit expectation changes from the unchanged frozen catalog assertions.

## Verification

- **348 new regression rows** cover all three functions and existing arities, public static/reference/lookup calls, empty node selections and filtered sequences, nonempty contrasts, error precedence, all three compatibility profiles and shared XSLT mode. Twenty-four direct registered-function rows separately test both empty representations and optional return metadata.
- Before the source change, the new set reported **240 failures / 108 passes**. After correction, all **729 focused checks** pass: 348 new plus 381 calendar checks, including the nine repaired expectations. Emptiness, item count and string conversion are checked together, preventing an empty string from masquerading as an empty sequence.
- Re-executing all 96 C17 controls gives **75 passes / 21 failures**, correcting exactly the 30 empty-value findings. The original 45 passes and all 21 missing-call-surface failures retain their outcomes/messages. All eight original arity 3/4 cases still fail unchanged with XPST0017; that separate feature remains open.
- Fresh normal exploratory scopes remain **208/6/18** for combined date/dateTime and **92/2/4** for time, retaining all eight failure names/messages and all skip names/reasons. Their exit zero reflects exploratory status, not zero failures.
- Both complete normal gates remain **QT3 31,164/0/657** and **qt4 8,587/0/1,892**. Every retained skip name/reason matches approved C16 adoption; 226 GatedSets and 11 Experimental passes are unchanged. The complete solution build succeeds with zero errors; all **22,927 solution tests** pass, including **8,196 XSLT tests**. Totals are recorded in [validation](evidence/validation.json).

The diagnostic adapter was freshly built against normal 1.28.2 assemblies before the 104 original/control probes, avoiding mixed-version execution. All 108 original/control, normal exploratory and complete-gate processes use bounded watchdogs and finish without timeout. Unit tests run as the complete solution; no test or fixture is excluded.

## Status and next slice

Candidate **1.28.2** remains unpublished; public **1.26.3**. FunctionLibrary history 5.157 records the helper and metadata correction; CalendarFallbackTests history 0.3 closes its deliberately preserved empty-result defect. No original catalog fixture/assertion, harness, admission filter, exact baseline, GatedSets, public API or schema backend change. Bosak.Schema owns staged XSD 1.1 and REQ-126 remains Complete under its original bounded acceptance. No release, push, tag, upstream or Braid action.

Next: separately adopt and implement the frozen 4.0 optional signatures and keyword metadata for all three functions, preserving 3.1 refusal of arities 3/4. Re-run the original eight cases and complete sets before considering any promotion; C18 does not promote them. See the preserved [C17 implementation sequence](../k04-date-arity-review/NEXT-SLICE.md).

Source base: 15bb2312. Delivery revision: `git log -1 --format=%H -- docs/conformance/k04-date-empty-fix/README.md`. Run `python docs/conformance/k04-date-empty-fix/evidence/validate-evidence.py` to verify the exact outcomes, current identities and preserved historical receipts. Documentation renders using unchanged Fytala CSS; HTML receipts do not claim screenshot/PDF acceptance.
