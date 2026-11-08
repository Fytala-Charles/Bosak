# K04-C16: owner-approved CB calendar adoption ? 27 October 2026

**The exact owner-approved admission change is adopted.** The owner selected ?Approve exact two-case CB adoption (Recommended)? for the proposal at **6df9a58b**. [Approval receipt](approval.json) records decision K04-CB-calendar-adoption-v1; [proposal identities](proposal-identities.json) preserve the presented patch, results and raw review artifacts.

The exact three-file patch recognizes only the reviewed unsupported `calendar=CB` capability in both filters, refuses positive CB dependencies and admits negative CB dependencies subject to every other guard. Filter source histories are 0.18/0.8. AGENTS.md now requires QT3 **31,164/0/657**, superseding 31,162/0/659. No rule is keyed by test name.

## Fresh normal verification

| Normal scope | Passed / failed / skipped | Exact result |
| --- | --- | --- |
| Complete QT3 | **31,164 / 0 / 657** | Only format-date-en152 and format-dateTime-en152 leave skips; all 657 retained names/reasons match |
| Complete qt4 gate | **8,587 / 0 / 1,892** | All 1,892 skip names/reasons, 226 GatedSets and 11 Experimental passes unchanged |
| Combined exploratory qt4 date/dateTime sets | **208 / 6 / 18** | Both CB cases pass; all six existing failure names/messages and 18 retained skip reasons unchanged |
| Narrower exploratory dateTime set | **77 / 2 / 15** | The same dateTime transition is independently checked; this overlaps the combined scope |

All four originals pass through actual normal single-case runners, with their original fixture, environment and assertion. The normal filters exactly match the approved prototype for **364 controls / 538 profile outcomes**, and all **40 projected controls** match the frozen review oracle. [Check receipts](check-receipts.json), raw outputs and [gate receipts](gate-receipts.json) record bounded fresh executions. Every retained gate/exploratory skip name and reason matches rehearsal; six exploratory failures remain open and the date-formatting sets stay ungated. Their non-gating exit zero does not mean all assertions pass.

The complete solution build succeeds with zero errors. All **22,579 fresh solution tests** pass across ten projects, including **8,196 XSLT tests**; exact project totals are recorded in [validation](validation.json). Documentation is rendered with unchanged Fytala CSS; HTML receipts do not claim screenshot/PDF acceptance.

## Preserved boundaries

The engine and its K04-C15 calendar fallback semantics are unchanged. Other calendar names, profile/schema/prolog guards, fixtures, assertions, known-skip dictionaries and GatedSets retain their prior policy. No new calendar implementation is claimed: unsupported CB requests still fall back to AD. Candidate **1.28.1** remains unpublished/public **1.26.3**; no package/version, release, push, tag, upstream or Braid action is included.

Earlier proposal notices and the root RESULTS.json (ownerApproved=false) describe the historical pre-approval checkpoint. They are preserved; this adoption record, [approval](approval.json) and [fresh validation](validation.json) supersede their pending disposition. The separate empty-value wrapper, language admission and FULLY-NORMALIZED findings remain open, as do the six exploratory missing-arity failures. Bosak.Schema retains staged XSD 1.1 ownership; REQ-126 remains Complete under its original bounded acceptance.

Source/documentation base: **6df9a58b**. Delivery revision: `git log -1 --format=%H -- docs/conformance/k04-calendar-admission/adoption/BASELINE-ADOPTION.md`. Next recommended slice: review the missing format-date/dateTime arities 3/4 contract before any engine implementation or GatedSets promotion.

Run `python docs/conformance/k04-calendar-admission/adoption/validate-adoption.py` to verify the exact approved patch, historical artifact identities and fresh normal acceptance results.
