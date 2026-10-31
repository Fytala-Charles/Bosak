# K01: schema-list binding correction and skip review


**K01 complete; approved QT3 baseline — 27 October 2026:** [Exact two-case adoption](BASELINE-ADOPTION.md) removes only CastAs-ListType-20 and -28 from known skips. The fresh full QT3 gate passes **31,144/0/677**; qt4 remains 8,436/0/1,894. The owner approval and original receipts are preserved; no fixture/assertion/filter change. Source candidate 1.26.4 remains unpublished and REQ-126 remains Complete. Earlier totals and pending-approval notices are historical.

**27 October 2026 — source candidate 1.26.4, unpublished; public 1.26.3 unchanged.**

`CastAs-ListType-20` is an invalid variable-binding item type, not a failed list cast. The query declares `xs:NMTOKENS` as a SequenceType item type and requires XPST0051. The old legacy EnforceType path checked the constructed sequence's cardinality first and reported XPTY0004. It now uses the existing illegal conversion-target validator before atomization and membership/cardinality checks. List constructors and list casts remain valid in their proper contexts.

The [immutable adoption record](ADOPTION.json) was written before implementation and pins the [21 March 2017 XQuery 3.1 Recommendation](https://www.w3.org/TR/2017/REC-xquery-31-20170321/#id-matching-item), sections 2.5.4/2.5.5.2 and 3.18.3–5. This is a bounded correction to executed bindings, not a complete static-error detection mode or XPath 4 adoption. Earlier REQ-126 acceptance and its 38 dispositions/48 unmet observations are unchanged historical records.

The 18 new [XQuery regressions](../../../tests/Bosak.XQuery.Tests/SchemaListBindingTests.cs) cover the three built-in list names, singleton/multi-item and empty/optional bindings, legal member sequence bindings, constructors, valid/invalid castability and an ordinary XPTY0004 mismatch. Before the fix: 6 failed/12 passed. After the fix: 18 passed. [Raw baseline and fixed TRX](evidence/manifest.json) preserve independent expectations.

Direct invocation of the released executor reproduces `-20` failing and `-28` passing; candidate execution passes both with their original pinned XML assertions. Each diagnostic runs separately under a 20-second external watchdog and bypasses only the runner's known-skip admission. These diagnostic passes are outside normal gate accounting. Fixture revision is `201a6e466940cdfc727f4babfedcde5332b9f578`; no upstream file or assertion changes.

The [baseline proposal](BASELINE-UPDATE-PROPOSAL.md) requests removal of exactly these two known skips and a separately approved QT3 total update. This implementation retains both skip entries and the exact frozen totals. The source candidate is not tagged, pushed or published. Canonical documentation is synchronized; new source history is recorded in VmEngine 2.207.

Validation: `python docs/conformance/k01-schema-list/evidence/validate-evidence.py`. Build, full-suite and both engine-gate receipts are retained in the evidence manifest. HTML rendering uses unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

Fresh Release validation: build succeeds with zero errors; all 21,816 solution tests pass across ten assemblies, including 18 K01 regressions. QT3 remains 31,142/0/679; qt4 remains 8,436/0/1,894 with 11 existing Experimental passes. Two build warnings are retained in the raw log: NU1900 vulnerability-feed connectivity and the existing CS8604 conformance-runner warning.
