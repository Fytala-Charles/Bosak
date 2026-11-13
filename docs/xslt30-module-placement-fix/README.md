# Scoped module-placement reporting — 13 November 2026

The existing validator now reports `XTSE0170` for rejected nested `xsl:include` and `XTSE0190` for rejected nested `xsl:import`. This changes the selected error code, not which placements are accepted or rejected. Other structural errors keep `XTSE0010`; existing use-package/override parent exemptions and earlier checks remain unchanged. Preprocessing, module acquisition, legacy matching and public signatures are unchanged.

[Adoption evidence](ADOPTION.json) was recorded before implementation and pins [XSLT 3.0 Recommendation 8 June 2017](https://www.w3.org/TR/2017/REC-xslt-30-20170608/), sections 2.14, 3.11.2, 3.11.3 and 3.13. The original fixtures contain nested module directives in ordinary named templates. Specific placement codes are explicitly defined, but generic context rules overlap and some catalogs allow both. This is a scoped reporting alignment, not a blanket verdict that generic reports are nonconformant. No intentional specification deviation is adopted.

| Original pinned case | Before | After | Assessment |
|---|---|---|---|
| error-0010x | XTSE0010 | XTSE0170 | Both reports already allowed by catalog |
| error-0010z | XTSE0010 | XTSE0190 | Both reports already allowed by catalog |
| error-0170a | XTSE0010 | XTSE0170 | Both reports already allowed by catalog |
| error-0170b | XTSE0010 | XTSE0170 | New direct match; alias no longer needed |
| error-0190a | XTSE0010 | XTSE0190 | Both reports already allowed by catalog |
| error-0190b | XTSE0010 | XTSE0190 | New direct match; alias no longer needed |

All six cases still fail at the public compile call as before. Their leading local error codes are observed; namespace identity is not independently proved. The complete original error-set measurement remains **513 passed / 0 failed / 66 excluded** across all 579 identities, with identical raw skip reasons. Only two catalog-code assessments improve. Four additional matcher attempts are expected because four allowed-alternative cases now try their second listed code: the unchanged matcher records 521 attempts, 504 exceptions and 9 normal results. Observer removal exactly restores production runner source; its nine candidate engine DLLs match the qualified runtime.

| Verification | Result |
|---|---|
| Identical final tests against sealed 1.81.6 binaries | 8 expected reporting failures; 13 protective/happy-path passes |
| Candidate focused tests | 21 passed; no failures/skips |
| Complete solution, ten projects | 26,394 passed in one invocation, including 8,369 XSLT cases |
| Separate language-server tests | 90 passed |
| Frozen QT3 | 31,397 passed / 0 failed / 424 excluded; exact skip map unchanged |
| Selected qt4 | 8,941 passed / 0 failed / 1,918 excluded; includes 11 experimental passes; exact skip map unchanged |
| Complete pinned standalone XSLT 3.0 | **10,221 passed / 0 failed / 4,380 excluded**, all 14,601 identities accounted for |
| Independent classifier controls | 16 passed |

The solution build has zero errors, eight pre-existing code warnings and two NU1900 warnings because package advisory metadata could not be fetched. No warning points to the changed source or new tests. Original logs are retained.

Tests cover direct/deep/unused-template placement and rejection before resolver access; valid include/import acquisition and import precedence; use-when removal of directives/ancestors; foreign-namespace literal elements; unrelated structural declarations and missing top-level href. An initial test-fixture attempt had seven extra XML-declaration expectation failures (15 failed/6 passed); explicit serialization corrected those before the identical final baseline/candidate comparison. The initial source/log/identity remain retained. Its console-print encoding error also remains recorded separately from the actual caught test results.

Clean source commit **7584295ddaecd20a09eb22ad270f06d87af0cd86** is based on merged PR #481 **b435fcc5d9c0f7261f539ade0db857c4e6f6e185**. Corpus `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`, Windows/Release, Legacy/Xslt30, basic/non-schema-aware. The full gate completes in **825.655 seconds**, under unchanged 1,200-second/8 GiB bounds, with no missing/unselected cases and unchanged owned/source corpus and runtime bytes. Timing is diagnostic, not performance acceptance. Runtime binaries remain in the local qualified packet; hashes, transcripts, case ledgers and source snapshots are archived. Later delivery changes only documentation/evidence.

Development Core/server **1.81.7 remains unpublished**. Published Core 1.81.2 and extension 0.2.6 are unchanged. All [4,380 named exclusions and reasons](skips.csv) remain unchanged; the 579-case error set is still excluded from the required gate. The [prior skip explanation table](../xslt30-error-audit/README.md) describes the same 66 targeted exclusions. Remaining 27 reported-code differences and 23 uncoded reports (50 tolerance branches), plus 55 overlapping static-label/execute-call observations, require separate review. Public `Compile` and internal `TransformObserved` call boundaries are not normative phase verdicts. No admission, fixture/skip/ruleset change, publication or complete-conformance claim.

[Results](RESULTS.json), [six-case comparison](CASE-COMPARISON.json), [related-fixture review](RELATED-CASES-REVIEW.json), [evidence](EVIDENCE.zip), [manifest](MANIFEST.json), [renderer receipt](DOCUMENTATION-CHECKS.json). Run `python docs/xslt30-module-placement-fix/verify.py` to replay the baseline, all original error outcomes, controls, frozen gates and all 14,601 standalone cases. Earlier sealed dossiers, including failed hosted attempts, remain unchanged.
