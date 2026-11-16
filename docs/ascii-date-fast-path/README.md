# Valid ASCII date fast-path candidate

Development 1.81.17, unpublished. A private helper accepts only valid four-digit ASCII YYYY-MM-DD, optionally uppercase Z or a valid numeric timezone, before the existing TryParseXPathDate regex route. It reuses DaysInMonth and XPathDateTime construction. Invalid, signed/extended-year, Unicode, null, whitespace and other noncanonical forms fall back unchanged. DateTime/time parsing, public cast normalization/wrapping and all other VM code remain byte-identical apart from the required history row. No new public API, language acceptance or expanded-year operation support is claimed.

[Adoption evidence](ADOPTION.json) pins immutable XSD REC-20120405/F&O REC-20170321 sections and the qualified protocol/input closure before implementation. The closure/checkpoint is the 1.81.16 baseline; engineVersion1.81.17 denotes the planned candidate. The original external prototype remains separate at f7d27267. Its post-run generated-case count correction is historical evidence, not an engine qualification failure.

Nine focused tests pass: six new fast-path/API cast/castable, out-state/error, fallback, locale/concurrency and allocation checks, plus three existing date-parser calendar/overflow controls. Full solution 26,488 and language server 90 passed. Frozen QT3 remains exactly 31,397/0/424 and gated qt4 8,941/0/1,918 with 11 experimental passes; all skip names/reasons remain exact. Pinned standalone qualification completed: 10,221 passed / 0 failed / 4,380 skipped in 811.353s. All 14,601 identities and exact exclusions reconcile against corpus fddf1cf920087e791f13315d68dfbe874d97dc56 under unchanged 1,200-second/8-GiB limits, Legacy/Xslt30 basic non-schema-aware profile. Qualified source `f733d828d523c272c2445a6dd34e607c3d7ec852`. Skips are not passes. [Build/tests](CHECKS.json) | [Frozen gates](FROZEN-CHECKS.json)

## Compatible allocation controls

One production worker executable compiled against lowest qualified 1.81.16 references ran once per version. Native framework and exact engine hashes were pinned, with a60s watchdog,256MiB managed and512MiB job caps. Each row has10k warmup/100k measured calls; fixture/delegate setup is outside measurement.

| Private parser operation | Baseline B/call | Candidate B/call | Fixed ceiling |
|---|---:|---:|---:|
| Plain four-digit date | 704 | 0 | 16 |
| Uppercase UTC date | 736 | 0 | 16 |
| Numeric-offset date | 736 | 0 | 16 |

All three ceilings were recorded before edits, reject baseline and accept candidate. Exact fallback observations remain unchanged: extended-year704, invalid-timezone736, lowercase-z rejection0 B/call. No retries, minimum selection or budget widening. These parser-only calling-thread measurements do not imply zero-allocation public casts or end-to-end speed.

A separate identical compatible semantic executable, without allocation loops, passes37,068 deterministic input/out-state/error comparisons,330 locale and1,024 concurrent checks per version. Its full outcome digest equals the immutable prototype's digest and both versions. Coverage includes yearzero/leap/century, timezone limits, malformed/Unicode/whitespace/null, signed/extended years, overflow exception type/message and rejected-date out state. Complete API/conformance qualification remains separate.

[Allocation receipts](CALIBRATION.json) | [Control review](CALIBRATION-REVIEW.json) | [Semantic control](SEMANTIC-CONTROLS.json) | [Source scope audit](SOURCE-AUDIT.json)

No push, public export or package release. Next after local qualification: review exact candidate/dossier for publication, then hosted checks before merge. Original packet .guard-tmp/date-fastpath-engine-20261116.


[Sealed evidence](EVIDENCE.zip) | [Results](RESULTS.json) | [Offline verifier](verify.py) | [Manifest](MANIFEST.json) | [Documentation checks](DOCUMENTATION-CHECKS.json)

Run `python docs/ascii-date-fast-path/verify.py` to reconcile all outcomes, frozen exclusions, test receipts, compatible controls and qualified source hashes offline without launching .NET. The archive retains prototype provenance and all compatible controls separately from qualification receipts. Runtime binaries and raw traces are excluded.

Local qualification complete. Next: review the exact candidate/dossier for publication, then hosted checks before merge. Nothing has been pushed or published; prior publication approval does not cover this new payload. Original standalone packet .guard-tmp/date-fastpath-qualified-20261116.
