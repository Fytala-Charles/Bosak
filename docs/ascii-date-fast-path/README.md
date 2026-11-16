# Valid ASCII date fast-path candidate

Development 1.81.17, unpublished. A private helper accepts only valid four-digit ASCII YYYY-MM-DD, optionally uppercase Z or a valid numeric timezone, before the existing TryParseXPathDate regex route. It reuses DaysInMonth and XPathDateTime construction. Invalid, signed/extended-year, Unicode, null, whitespace and other noncanonical forms fall back unchanged. DateTime/time parsing, public cast normalization/wrapping and all other VM code remain byte-identical apart from the required history row. No new public API, language acceptance or expanded-year operation support is claimed.

[Adoption evidence](ADOPTION.json) pins immutable XSD REC-20120405/F&O REC-20170321 sections and the qualified protocol/input closure before implementation. The closure/checkpoint is the 1.81.16 baseline; engineVersion1.81.17 denotes the planned candidate. The original external prototype remains separate at f7d27267. Its post-run generated-case count correction is historical evidence, not an engine qualification failure.

Nine focused tests pass: six new fast-path/API cast/castable, out-state/error, fallback, locale/concurrency and allocation checks, plus three existing date-parser calendar/overflow controls. Full solution 26,488 and language server 90 passed. Frozen QT3 remains exactly 31,397/0/424 and gated qt4 8,941/0/1,918 with 11 experimental passes; all skip names/reasons remain exact. Standalone qualification pending. [Build/tests](CHECKS.json) | [Frozen gates](FROZEN-CHECKS.json)

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
