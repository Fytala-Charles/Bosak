# K04-C17: date-formatting optional-arity review ? 28 October 2026

**Review complete; implementation remains open.** The pinned 4.0 contract makes language, calendar and place optional for format-date, format-dateTime and format-time. All eight PR901 originals currently fail because arities 3/4 are unavailable. The review also verifies a shared empty-result defect and missing keyword metadata; adding registrations alone would not complete the contract.

## Frozen contract

[SPECIFICATION-REVIEW.json](evidence/SPECIFICATION-REVIEW.json) pins immutable qt4cg revision e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e, both source-file hashes, the three signatures and common formatting rules. It separately pins the dated F&O 3.1 Recommendation for existing signatures and empty-result semantics. This is review evidence, not a new semantic adoption or feature-origin revision.

| Property | Reviewed requirement |
| --- | --- |
| Required arguments | value (optional date/dateTime/time item) and picture (string) |
| Optional 4.0 arguments | language, calendar, place: optional strings, each defaulting to () when omitted |
| Effective arities | 2?5 in 4.0; preserve 2/5 and refusal of 3/4 in 3.1 |
| Return | xs:string?; an empty value returns an empty sequence, distinct from a zero-length string |
| Call surfaces | Positional calls, named references, lookup and declared keyword names/defaults must agree |

The frozen signature metadata does not mark these optional strings default-on-empty. Their function-specific empty/default behavior stays distinct from omitted-argument expansion. Review pins, original PR901 attribution and any later implementation adoption remain separate.

## Reproduced observations

Eight unchanged originals span all three functions: four format-date, two format-dateTime and two format-time cases. Seven expect formatting or a specified calendar error; format-date-050 requires an empty result. All eight currently fail with missing-function XPST0017, including the case whose expected error is FOFD1340.

Ninety-six controls across 3.1/4.0 produce **45 passes and 51 failures**:

- Thirty failures expose empty-value handling in existing arities 2/5, including direct empty assertions, string conversion and an invalid-picture boundary. The helper checks only IsUndefined and otherwise raises XPTY0004 for the observed empty-sequence wrapper. Its undefined branch returns an empty string, which also cannot satisfy a true empty-result contract.
- Twenty-one failures expose the 4.0 call-surface gaps: six positional calls, six named references, six lookup-presence assertions and three keyword calls. The signature table lacks the three date-formatting keyword entries.
- Forty-five controls pass: supported nonempty 2/5 calls, 3.1 refusal/absence checks, invalid calendar and language-type checks. Passing absence assertions does not establish argument validation for the missing 4.0 forms.

Fresh complete normal exploratory runs preserve the current findings:

| Scope | Passed / failed / skipped |
| --- | --- |
| Combined date/dateTime sets (232 cases) | **208 / 6 / 18** |
| Time set (98 cases) | **92 / 2 / 4** |

The runner's fn-format-date substring selects both date and dateTime sets. These two scopes do not overlap. All eight failed names correspond exactly to the PR901 originals, with missing-arity errors. Exit zero reflects their exploratory status; neither set coverage nor process success is a claim of assertion success. No GatedSets promotion is justified yet.

All **106 diagnostic/normal-set processes** complete under unchanged external watchdogs without timeout. [Raw receipts](evidence/probe-receipts.json), original requests/assertions, controls, source excerpts and binary/fixture identities preserve the diagnosis. The diagnostic adapter source/project is snapshotted; no fixture or assertion is altered.

## Next implementation sequence

[Next-slice scope](NEXT-SLICE.md) recommends the shared empty-result correction first, then a separate 4.0 optional-argument/keyword implementation. The existing nine CalendarFallbackTests compatibility rows explicitly preserve the old empty-input error; they need a documented correction when that defect is fixed. Preserve their historical logs and source snapshots. Update current tests to assert correct empty semantics, not to retain the defect forever.

No source, harness, admission, version, exact baseline or GatedSets change. Candidate **1.28.1** remains unpublished/public **1.26.3**. Complete solution **22,579 passes**, QT3 **31,164/0/657** and qt4 gate **8,587/0/1,892** are inherited source-identical evidence from approved K04-C16 adoption, not fresh full-suite runs. Bosak.Schema retains staged XSD 1.1 ownership; REQ-126 remains Complete under its original bounded acceptance. Calendar, language and FULLY-NORMALIZED policy findings are not broadened by this review.

Source base: e88e5ba5. Delivery revision: `git log -1 --format=%H -- docs/conformance/k04-date-arity-review/README.md`. Run `python docs/conformance/k04-date-arity-review/evidence/validate-evidence.py` to verify preserved artifacts, source identities, original/control results and normal scopes. HTML renderer receipts use unchanged Fytala CSS and do not claim screenshot/PDF acceptance.
