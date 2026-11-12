# Targeted XSLT error-set audit — 12 November 2026

The unchanged current-main runner at `df16de7f39a556c6f9afea3eee9f61ef9bbc602e` measured the original `error` set at `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`. The existing substring filter uniquely selects this one set, bypassing its blanket full-sweep exclusion without editing the catalog or source. All 579 original case identities reconcile.

| Scope | Passed | Failed | Skipped |
|---|---:|---:|---:|
| Targeted `error` set; existing Legacy/Xslt30 basic/non-schema-aware oracle | 513 | 0 | 66 |

Execution completed in 1.258 seconds under the existing 1,200-second watchdog and 8 GiB job-memory limit. This is an observation, not performance acceptance. All 12,886 disposable corpus files, the independent source corpus and 32 runtime inputs remain unchanged. The source was clean and committed before its fresh Release build. No set/case retry or resume was used. Input preparation initially had not finished when an early launch was attempted; Python refused before any test or .NET process ran. That note is retained separately from the single actual attempt.

## What the 513 passes establish

They are acceptance by the existing Legacy assertion logic. The set contains static and dynamic errors, plus assertion alternatives; it is not exclusively compile-time validation. PASS lines do not record the actual exception identity, matching branch or compile/execute phase.

Source review finds 22 code-alias pairs and seven uncoded-message pairs in the legacy matcher, plus retired/recoverable-code aliases. Some accept dynamic codes for static expectations. Seventy-one passing cases have expected codes appearing in those tables; this is a review candidate list, **not evidence that all 71 used an alias or are defective**. [Oracle source review](ORACLE-REVIEW.json) records exact pairs and names. No independent exact-code or error-stage audit has yet been performed. The matcher and all production sources are unchanged.

## Why 66 cases remain skipped

| Retained reason | Cases | Interpretation |
|---|---:|---|
| Exact XSLT 2.0 dependency | 29 | Outside the retained Xslt30 catalog profile. |
| Schema-aware dependency | 29 | The observed run is basic/non-schema-aware. |
| Requires an admitted feature to be absent | 5 | Two disabling-output-escaping requirements and one each for backwards compatibility, XSD 1.1 and dynamic evaluation; these are harness dependency-policy mismatches, not certification of those capabilities. |
| Ignore document-load failure | 1 | The retained runner does not support that dependency policy. |
| Named forwards-compatibility policy | 1 | `error-0010bb`; the historical policy comment was retained, not revalidated. |
| Remote-resource policy | 1 | `error-1160a`; the endpoint was not fetched and the printed historical environmental condition was not re-probed. |
| **Total** | **66** | Every original name, printed reason and derived explanation is in [skips.csv](skips.csv). |

[Complete outcomes](outcomes.csv) preserve each case's observed status, expected error codes and original full-gate exclusion. [Failures](failures.csv) is header-only because the existing oracle reported zero failures. Silent refusals are explained from pinned dependency policy; historical comments and remote conditions are explicitly distinguished from fresh capability evidence.

## Gate status and next step

The adopted required gate stays **10,221 passed / 0 failed / 4,380 excluded**. All 579 error-set cases remain excluded there under the original policy; these targeted results must not be added to its counts. There is no engine, runner, workflow, version, exclusion-policy or ruleset change, no gate admission and no complete-conformance claim.

Next: independently audit exact error identities and static/dynamic phases under the same engine and catalog profile, prioritizing the 71 possible alias matches. Review original fixtures and normative requirements before proposing admission. Do not switch catalog profiles merely to obtain stricter matching or infer eligibility from these Legacy passes.

## Evidence and reproduction

[Sealed evidence](EVIDENCE.zip), [results](RESULTS.json) and [manifest](MANIFEST.json) retain the complete transcript, case ledger, original gate exclusions, source snapshots, controller diff, runtime/input hashes and integrity audit. The controller differs from the qualified original only by appending the unique `error` filter; its watchdog, Windows Job limit and process-scoped error-mode flags are unchanged. No persistent power or Windows Error Reporting setting was changed.

With a fresh owned copy of the exact LF corpus and a source-built Release runner, the selected command is:

```powershell
dotnet tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll --processor-profile legacy <owned-corpus>/catalog.xml error
```

Use the archived bounded controller and a fresh packet for an unattended run. Direct legacy exit zero alone is insufficient; independently reconcile all 579 identities and audit corpus/runtime bytes. `python docs/xslt30-error-audit/verify.py` provides read-only evidence verification. The retained local packet is `D:/Development/Bosak/.guard-tmp/xslt30-static-errors-20261112`.
