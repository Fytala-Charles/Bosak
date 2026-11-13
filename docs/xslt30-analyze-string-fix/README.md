# Scoped analyze-string error reporting — 13 November 2026

Invalid effective regular expressions in `xsl:analyze-string` now report `XTDE1140`; invalid flags report `XTDE1145`. The correction is limited to regex preparation. Select and regex/flags AVT evaluation, matching/non-matching children and ordinary XPath functions keep their own errors. Child validation and native regex construction retain their original order. The shared XPath regex helper and legacy conformance matcher are unchanged.

The [adoption record](ADOPTION.json), recorded before implementation, pins [XSLT 3.0 Recommendation 8 June 2017](https://www.w3.org/TR/2017/REC-xslt-30-20170608/), sections 2.14 and 17.1. No intentional deviation is adopted. Development Core/server **1.81.6 is unpublished**; published Core 1.81.2 and extension 0.2.6 are unchanged. Public signatures are unchanged; the two error identities are observable behavior changes.

| Original pinned case | Previous report | Corrected report | Audit acceptance |
|---|---|---|---|
| error-1140a | FORX0002 | XTDE1140 | Direct local-code match; no legacy alias |
| error-1145a | FORX0001 | XTDE1145 | Direct local-code match; no legacy alias |

The diagnostic observer uses the same sources as the sealed prior audit, bound to nine freshly built candidate assemblies. Removing instrumentation restores the production runner exactly. All 579 statuses and 66 raw skip reasons are unchanged: **513 passed / 0 failed / 66 excluded**. Only the two rows above change reported identities. Local code properties do not independently prove namespaces; separate named `xsl:catch` tests assert both the standard error namespace and local name. Public `Compile` and internal `TransformObserved` call boundaries are observations, not normative phase verdicts.

| Verification | Result |
|---|---|
| Identical 14 tests against sealed 1.81.5 assemblies | 4 expected failures; 10 protective/happy-path passes |
| Candidate focused tests | 14 passed; zero failed/skipped |
| Complete solution, ten projects | 26,373 distinct cases pass across original run and database retry; includes 8,348 XSLT cases |
| Separate language-server tests | 90 passed |
| Frozen QT3 | 31,397 passed / 0 failed / 424 excluded; all skip names/reasons unchanged |
| Selected qt4 | 8,941 passed / 0 failed / 1,918 excluded; includes 11 experimental passes; all skip names/reasons unchanged |
| Full pinned standalone XSLT 3.0 | **10,221 passed / 0 failed / 4,380 excluded**, all 14,601 identities accounted for |
| Independent error classifier controls | 16 passed |

The initial restricted solution run failed 45 database cases during localhost `HttpListener` setup; the authorized retry passes all 47 database cases. The original failure log is retained. Other solution projects, including all 8,348 XSLT cases, pass in the original invocation. No tests are counted twice. The 14 focused tests cover validation/native-regex failures, valid segment order, quote mode, both AVTs and both child callbacks in tree/typed-sequence routes. The baseline and candidate use identical test-source bytes.

The standalone gate qualifies clean semantic commit **50c9fd762fb12704dbd670b8f49f1415d70d2bfa**, based on PR #480 merge **2e5abb5112f7ce422caa11d930d87b8e3c263b6b**, against immutable `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`. Windows/Release, Legacy processor, Xslt30 catalog profile, basic/non-schema-aware; the original 1,200-second watchdog and 8 GiB job limit remain. It completes in **713.170 seconds**, with unchanged owned/source corpus bytes and runtime hashes, no missing/unselected cases and no fixture edits. Diagnostic timing is not performance acceptance. Later delivery changes only documentation/evidence. Runtime binaries remain in the local qualified packet; the archive retains hashes, transcripts, complete ledgers and source snapshots.

All [4,380 named exclusions and reasons](skips.csv) remain unchanged. The `error` set remains excluded from the required gate; measuring its 579 cases does not admit them. The [prior error-audit skip explanation table](../xslt30-error-audit/README.md) covers the same 66 exclusions. Remaining 29 code differences and 23 uncoded reports (52 observed tolerance branches), plus 55 overlapping execute-call/static-label observations, require fixture/spec review. They are not blanket defect verdicts. No gate/ruleset, fixture, skip, package-publication or complete-conformance claim follows.

[Results](RESULTS.json), [case comparison](CASE-COMPARISON.json), [evidence archive](EVIDENCE.zip), [manifest](MANIFEST.json), [renderer receipt](DOCUMENTATION-CHECKS.json). Run `python docs/xslt30-analyze-string-fix/verify.py` for independent replay of the regression baseline, original-case identities, frozen gates, every standalone case and the unchanged exclusions. Historical dossiers and the earlier rejected timeout retain their own evidence.
