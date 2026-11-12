# Error identity and API-boundary audit — 12 November 2026

This diagnostic observes the same original 579-case `error` set, Legacy engine, Xslt30 catalog profile and basic/non-schema-aware policy as the [prior measurement](../xslt30-error-audit/README.md). It uses the exact nine previously sealed engine DLLs from source `df16de7f39a556c6f9afea3eee9f61ef9bbc602e`; production source is unchanged. An isolated runner copy records exception properties/chains, original matcher branches and compiler/runner call boundaries. It does not replace the matcher or switch to the 4.0 catalog profile.

All 579 legacy statuses and all 66 raw skip reasons remain identical: **513 passed / 0 failed / 66 skipped**. Removing observation statements exactly restores the original runner source. Sixteen caught synthetic controls verify the independent comparator, including wrong namespaces, misleading descriptions, code aliases, uncoded messages and known parser wrappers. Nine engine DLL hashes and every corpus/runtime input match before and after. The observed run completes in 3.761 seconds under the unchanged 1,200-second/8 GiB controller; this is not performance acceptance.

## Identity findings

| Observation | Cases | Meaning |
|---|---:|---|
| Explicit reported QName matches a listed alternative | 5 | Namespace/local properties or an explicit QName code property agree. |
| Reported local code matches a listed alternative | 445 | Leading/code-property identity agrees; namespace is not independently established. |
| Reported code differs from listed alternatives | 31 | These passes used a legacy code-alias branch; each needs fixture/specification review. |
| Uncoded exception report | 23 | These passes used a legacy message-label branch; exact identity remains unclassified. |
| Normal-result assertion pass | 9 | A normal result satisfied the original assertion path; no error identity is inferred. |
| Retained skips | 66 | Original dependency/named policies and explanations are unchanged. |
| **Total** | **579** | These are observations, not a new strict-gate pass/fail total. |

The earlier 71 potential alias-review cases resolve to **54 observed alias/message-label branches** and **17 cases not accepted through those branches**. Branch use is now directly recorded. It is still not a blanket defect verdict. [Per-case outcomes](outcomes.csv), [identity review candidates](review-candidates.csv), [candidate resolution](CANDIDATE-RESOLUTION.json) and [unchanged skip explanations](skips.csv) retain all original names.

## API boundaries

| Observed boundary | Exception cases |
|---|---:|
| `XsltCompiler.Compile` call | 255 |
| `XsltExecutable.TransformObserved` call | 249 |
| **Total exception observations** | **504** |

The other nine passes produced normal results. Fifty-five cases whose listed error alternatives all have static labels surfaced during the execute call; [their names and identities](late-static-labels.csv) are review candidates and overlap the identity classes above.

The compile boundary is the public compiler call; the execute boundary is the existing internal capture helper, not a new public API. [Member visibility and preserved initial metadata](BOUNDARY-CLARIFICATION.json) clarify the earlier observer wording. Call boundaries are not automatically normative phases. Error reporting can involve permitted early/deferred detection and multiple violated rules. The audit therefore makes no general conformance verdict from these phase observations. [Immutable XSLT 3.0 Recommendation, section 2.14](https://www.w3.org/TR/2017/REC-xslt-30-20170608/#errors). The [pre-implementation diagnostic policy](ADOPTION.json) records this distinction.

## Reviewed next correction

| Original case | Required/listed code | Observed code | Accepted branch |
|---|---|---|---|
| `error-1140a`: invalid analyze-string pattern | `XTDE1140` | `FORX0002` | Legacy code alias |
| `error-1145a`: invalid analyze-string flags | `XTDE1145` | `FORX0001` | Legacy code alias |

Both simple fixtures reach the execute call. The instruction currently lets XPath regex-helper codes escape. The [immutable section 17.1](https://www.w3.org/TR/2017/REC-xslt-30-20170608/#analyze-string) specifies the XSLT reporting codes. [Fixture/source review](REGEX-REVIEW.json) recommends a bounded correction at regex/flags preparation only, preserving XPath errors from attribute-value expressions, child instructions and ordinary XPath functions. No correction has been implemented, and no verdict is assigned to the remaining 29 code differences or the 55 phase candidates.

## Scope, provenance and gate health

The required gate remains **10,221 passed / 0 failed / 4,380 excluded**; all 579 error-set cases stay excluded there. No engine, production runner, workflow, version, rule or exclusion-policy change is made. No cases are admitted and no complete-conformance claim follows.

Prior audit [PR #479](https://github.com/Fytala-Charles/Bosak/pull/479) merged at `065db051f18b6b427c7fab83e840f4ce7547f11c` after all six hosted checks passed. The same-commit full-gate diagnostic [43305315323](https://github.com/Fytala-Charles/Bosak/actions/runs/43305315323) passed in 1,079.563 seconds with original counts and unchanged input/runtime bytes. The original [timed-out push run](https://github.com/Fytala-Charles/Bosak/actions/runs/43300842122) remains failed and preserved separately; the successful diagnostic does not rewrite it or establish stable timing on every runner. No limit or fixture was relaxed.

[Evidence](EVIDENCE.zip), [results](RESULTS.json), [manifest](MANIFEST.json) and `python docs/xslt30-error-oracle/verify.py` support read-only verification. The archive retains full traces, all matcher attempts, exception chains, original fixture definitions, diagnostic diffs/sources/project, controls, failed namespace-mismatch build and successful corrected build, controller and source/runtime/corpus hashes. The first build failed before any diagnostic ran; its source and log remain separate. The local packet is `D:/Development/Bosak/.guard-tmp/xslt30-error-oracle-20261112`; archived project paths refer to that retained Windows workspace. No unhandled negative .NET process or persistent Windows Error Reporting/power setting change was used.
