# Hosted preparation ? 8 November 2026

User explicitly authorized evidence commit/push and both preparation-only workflows. Evidence checkpoint **e2893d1aa36c2a68545a2d0eed34f103ab65632d** was pushed to `checkpoint/foundation-1.81.1`. Its product source follows the locally qualified 1d69d059 cohort; hosted builds have their own source identity and artifact hashes.

- [NuGet preparation](https://github.com/Fytala-Charles/Bosak/actions/runs/42477093485): failed at QT3 after a successful solution build/test step. QT3 reports **31,391 passed / 6 failed / 424 skipped**; qt4 and NuGet packing were not reached. [Raw qualification](nuget-qualification-42477093485.zip), [workflow log](nuget-workflow-log.zip), [results](RESULTS.json).
- [Extension preparation](https://github.com/Fytala-Charles/Bosak/actions/runs/42477095428): successful. All 90 language-server tests pass. [Artifact and TRX archive](vscode-42477095428.zip), [verification](VSIX-VERIFICATION.json), [workflow log](vscode-workflow-log.zip).

Both publication jobs were skipped. No tag, publisher authentication or publication was performed. The hosted VSIX manifest SHA-256 is `f6011d57c799ef3a6ff24c70f8e3af9665a45f2adee8646271c0586a07c8d2da`.

The initial local verification against a Windows CRLF checkout refused the hosted NOTICE bytes. Verification then passed against a clean LF checkout of the exact hosted source commit without changing the VSIX. This preserves the artifact's legal bytes rather than normalizing the payload or weakening verification.

Owner workload/budget, deferred-provider scope, C45/API, signing/provenance and final publication decisions remain open in the [review packet](../release-1.81.2-review/README.md). Successful preparation does not close those decisions. The local review packet retains its original unfilled status; hosted results are recorded separately here.

## Next fix

All six QT3 failures load collection fixture `bib.xml` using the erroneous URL `http://www.w3.org/home/runner/work/Bosak/Bosak/tests/qt3tests/docs/bib.xml`. The harness registers collection members as absolute filesystem paths; on Linux those paths begin with `/`. Trace and correct the file-path/URI boundary with focused Windows and Unix controls, preserving URI semantics and resource-policy checks. Then qualify the corrected source and repeat hosted preparation. Do not waive these failures, relabel the earlier Windows success as Linux success, or publish the extension independently without its release acceptance.
