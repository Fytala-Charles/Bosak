# Scoped named-template prefix reporting — 13 November 2026

Undeclared prefixes in well-formed lexical `xsl:template/@name` values now report `XTSE0280` at the existing named-template binding-collection call. Only the local call to name expansion is wrapped: the shared QName helper and namespace-resolution logic are unchanged. The original `XPST0081` remains the inner cause. Malformed-name diagnostic precedence, EQNames, reserved-name checks, the `xsl:initial-template` exemption, preprocessing and module-local contexts are preserved. XPath and other component-name diagnostics are not globally rewritten.

[Adoption](ADOPTION.json) predates implementation and pins [XSLT 3.0 Recommendation 8 June 2017](https://www.w3.org/TR/2017/REC-xslt-30-20170608/), sections 2.14, 3.13, 5.1.1 and 10.1. Both original fixtures contain ordinary named templates with undeclared valid prefixes. Binding validation precedes later instruction-tree validation, which already knows the XSLT code. No new acceptance rule or intentional specification deviation is adopted.

| Original pinned case | Before | After | Observation |
|---|---|---|---|
| error-0280d | XPST0081 | XTSE0280 | Direct local-code match; original cause retained |
| error-0280f | XPST0081 | XTSE0280 | Direct local-code match; original cause retained |

All 579 original error outcomes remain **513 passed / 0 failed / 66 excluded**, with identical raw skip reasons. Only these two primary identities change. Leading local codes do not independently prove an error namespace. Both surface in the public compile call; the observer's other entry point is internal `TransformObserved`, not a public API. Call boundaries are not normative phase verdicts. The unchanged matcher records 521 attempts; the observer records 504 exception events and 9 normal results. Its sources are unchanged and its nine candidate engine DLLs match the qualified runtime.

| Verification | Result |
|---|---|
| Identical 28 tests against sealed 1.81.7 binaries | 10 expected reporting failures; 18 protective/happy-path passes |
| Candidate focused tests | 28 passed; no failures/skips |
| Complete solution, ten projects | 26,422 passed in one invocation, including 8,397 XSLT cases |
| Separate language-server tests | 90 passed |
| Frozen QT3 | 31,397 passed / 0 failed / 424 excluded; exact skip map unchanged |
| Selected qt4 | 8,941 passed / 0 failed / 1,918 excluded; includes 11 experimental passes; exact skip map unchanged |
| Full pinned standalone XSLT 3.0 | **10,221 passed / 0 failed / 4,380 excluded**, all 14,601 identities accounted for |
| Independent classifier controls | 16 passed |

The tests cover 2.0/3.0 named and matched templates, trimmed names, unprefixed/default and local/implicit namespace bindings, lexical names/EQNames, import/include definition contexts, reserved names and the initial-template exemption, discarded templates and unchanged XPath authoring diagnostics. Two malformed-name tests preserve existing diagnostics rather than endorsing them as a complete lexical-conformance contract. Imported/included modules already report the specific code in the sealed baseline; those are protective passes. Baseline and final candidate use identical test-source bytes.

An initial guard in `ValidateNamedQName` was too late: the focused candidate still had 10 failures/18 passes. A **caught diagnostic** identified the earlier `BuildStaticContext` → binding collection → `ExpandVariableName` path. The [revised implementation review](IMPLEMENTATION-REVIEW-2.json) was recorded before replacing that ineffective guard with the scoped catch. Original diff, failed log, caught stack and diagnostic source remain archived; no unhandled negative .NET process was launched. This investigation did not expand the adopted semantic scope or change the tests.

Clean qualification source **de15d03c18884b4951fa9b9883fce42512fdeba0**, base **014494e9a6550d52c792fa47494d13462d5fc780** (PR #482). Immutable corpus `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`, Windows/Release, Legacy/Xslt30, basic/non-schema-aware. Full qualification completes in **783.428 seconds**, with unchanged 1,200-second/8 GiB bounds, no missing/unselected cases, no corpus writes and unchanged owned/source corpus and runtime hashes. Timing is diagnostic, not performance acceptance. Later delivery changes only documentation/evidence. Runtime binaries remain in the local qualified packet; hashes, transcripts, complete ledgers and source snapshots are archived. Original build warnings are retained; no warning points to the changed source or new tests.

A separate post-merge parent-main gate (`014494e9`, run 43416044519) timed out after **1,200.219 seconds** at `sx-instance-of-101`. It recorded no FAIL lines, but the incomplete attempt remains failed. Its source/owned corpus and runtime bytes are unchanged. The [original failure receipt](PARENT-MAIN-FAILURE.json) and hash-verified artifact are retained in the evidence archive; no retry or budget change was made. The successful parent PR gate (632.735 seconds) and this candidate qualification are separate receipts, not replacements for that failure.

Development Core/server **1.81.8 is unpublished**; published Core 1.81.2 and extension 0.2.6 remain unchanged. All [4,380 named exclusions and reasons](skips.csv) remain unchanged. The 579-case error set is still excluded from the required gate; [the prior skip table](../xslt30-error-audit/README.md) describes its same 66 exclusions. Remaining 25 reported-code differences and 23 uncoded reports (48 tolerance branches), plus 55 overlapping static-label/execute-call observations, require separate fixture/spec review. No admission, fixture/skip/ruleset change, publication or complete-conformance claim follows.

[Results](RESULTS.json), [comparison](CASE-COMPARISON.json), [evidence](EVIDENCE.zip), [manifest](MANIFEST.json), [renderer receipt](DOCUMENTATION-CHECKS.json). Run `python docs/xslt30-template-name-fix/verify.py` to replay the baseline, original outcomes/causes, exact catalog/fixture pins, classifier controls, frozen gates and all 14,601 standalone cases. Earlier dossiers and original hosted timeout/success records stay unchanged.
