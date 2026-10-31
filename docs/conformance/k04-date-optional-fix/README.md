# K04-C19: optional date-formatting calls and gate promotion ? 28 October 2026

XPath 4.0 and Experimental now support all arities 2?5 for format-date, format-dateTime and format-time, with declared keyword names/defaults. XPath 3.1 retains its existing 2/5 signatures and rejects 3/4. The three repaired complete test sets are promoted into the qt4 gate only after fresh zero-failure qualification.

## Implementation

[Pre-implementation adoption](evidence/IMPLEMENTATION-PIN.json) pins immutable qt4cg revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e, the exact source hashes and optional signatures. The first two parameters are required; language, calendar and place each default to () when omitted. These are frozen compatibility semantics, not a claim of full latest-draft compliance.

Six new registrations carry 4.0-only profile guards, precise parameter types and optional xs:string? return metadata. Shared option-aware wrappers preserve the existing five-argument conversion path and supply defaults for missing trailing arguments. Keyword metadata declares value, picture, language, calendar and place, with required first two and () trailing defaults. Positional, named-reference and lookup calls agree; keyword calls expand through the existing compiler path. C18 true-empty semantics, nonempty formatting, argument errors and bounded calendar fallback are retained.

## Verification and promotion

The distinct new regression set reports **276 failures/162 passes before implementation** and **438 passes after**. Combined with the 729 existing calendar/empty checks, all **1,167 focused checks** pass. Coverage includes 3.1/4.0/Experimental, shared XSLT mode, optional/empty defaults, node atomization, wrong types, invalid calendars, named references, lookup and reordered keyword calls. An initial draft duplicated 18 data rows; its source/output is preserved separately, and the corrected baseline/final runs execute all 438 distinct rows without duplicate IDs.

All **eight original PR901 cases** and all **96 C17 controls** pass with unchanged frozen expressions, environments and assertions. Exactly 29 prior failed observations become passes (eight originals and 21 call-surface controls); the prior 75 controls remain passes. The adapter is freshly built against 1.29.0 assemblies.

Complete normal qualification before promotion:

| Set | Passed / failed / skipped |
| --- | --- |
| fn-format-date | **135 / 0 / 3** |
| fn-format-dateTime | **79 / 0 / 15** |
| fn-format-time | **94 / 0 / 4** |

The fn-format-date substring selects both date/dateTime sets: **214/0/18** across 232 cases. The time set is **94/0/4** across 98 cases. Pre-promotion logs/skip maps and [qualification](evidence/promotion-qualification.json) are preserved; the same complete scopes pass after promotion.

The qt4 gate grows from **226 to 229 sets**, reporting **8,895/0/1,914** over 10,809 cases. All 1,892 prior gated skip names/reasons remain identical. The only 22 additional gated skips are the existing legitimate dependency exclusions of the three newly gated sets; none are newly introduced refusals. The 11 Experimental passes remain unchanged. Normal QT3 stays exactly **31,164/0/657**, with every retained skip name/reason identical. The final complete solution build succeeds with zero errors; all **23,365 solution tests** pass, including **8,196 XSLT tests**. Results are in [validation](evidence/validation.json).

## Current examples and historical evidence

The first full solution run exposed twelve live catalog examples that still asserted missing date-function availability/reference support. Their original XPath files are unchanged. The current [cases manifest](../../xpath40-catalog/examples/cases.json) now requires 4.0 lookup presence and named-reference arity; it retains explicit 3.1 absence/refusal checks. A focused current-catalog run passes 18 checks: twelve updated examples plus six existing 2/5-arity inventory checks.

[Expectation update](evidence/catalog-expectation-update.json) records the exact twelve before/after rows, original expression hashes and source revision. Initial failed unit output is preserved. Dated C01/C04/C11/C13 structured records, original owner dispositions and all 48 unmet batch observations remain historical and are not automatically reconciled by this implementation. [Current example notes](../../xpath40-catalog/examples/changed31-contracts/README.md) distinguish the executable update from the retained [dated inventory](../../xpath40-catalog/CHANGED-31-CONTRACT-INVENTORY.md).

## Status

Candidate **1.29.0** remains unpublished; public **1.26.3**. FunctionLibrary history 5.158 and ConformanceRunner40 history 0.15 record the feature and qualified promotion. No original QT3/qt4 fixture or assertion, dependency filter, exact QT3 baseline, schema backend, release, upstream or Braid change. Bosak.Schema retains staged XSD 1.1 ownership; REQ-126 remains Complete under its original bounded acceptance. Remaining skip-policy and FULLY-NORMALIZED work stays separate.

Next: review the FULLY-NORMALIZED capability/semantics mismatch retained by K04-C13 before changing its engine behavior or admission. Source base: ace5f2bf; delivery revision: `git log -1 --format=%H -- docs/conformance/k04-date-optional-fix/README.md`. Run `python docs/conformance/k04-date-optional-fix/evidence/validate-evidence.py` to verify results, exact promotion/skip deltas, twelve current expectation changes and historical identities. HTML renderer receipts use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.
