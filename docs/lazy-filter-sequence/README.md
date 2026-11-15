# Specialized lazy node filtering

Development 1.81.14, unpublished. The VM now filters sequence nodes through a direct XDM sequence/enumerator instead of an intermediate generated IEnumerable iterator and adapter. Scalar/empty paths, deferred acquisition and errors, retained Current/end state, identity/mutation visibility, single-pass atomic acquisition/refusal and the existing non-disposal protocol are preserved. No public API, language acceptance, exclusion policy or resource limit changed.

[ADOPTION.json](ADOPTION.json) records immutable XPath/XDM REC-20170321 and qualified protocol source revisions before implementation. Its qualified runtime closure is the 1.81.13 input baseline; engineVersion records the planned 1.81.14 candidate. Actual candidate assembly hashes are separate in [results](RESULTS.json) and sealed controls.

| Verification | Result |
|---|---|
| Solution tests | 26,453 passed |
| Language server tests | 90 passed |
| Focused new regressions | 10 passed |
| Frozen QT3 | 31,397 passed / 0 failed / 424 skipped |
| Gated qt4 | 8,941 passed / 0 failed / 1,918 skipped; 11 experimental passes |
| Pinned standalone XSLT 3.0 | 10,221 passed / 0 failed / 4,380 skipped |
| Compatible semantic control | 139 scenarios + 14 fixture checks per version |

All frozen skip names/reasons remain exact. Standalone reconciles every 14,601 outcome against pinned corpus fddf1cf920087e791f13315d68dfbe874d97dc56 in 746.143s under unchanged 1,200-second/8-GiB limits. The basic non-schema-aware Legacy/Xslt30 profile is unchanged; exclusions are not passes. Qualified source checkpoint: `6ccb4c83ba81dcff3e934058304ed52e01eb6f6c`.

## Allocation evidence and regression budgets

The same production allocation worker executable was compiled against lowest qualified 1.81.13 references, then run once against baseline and candidate. Exact engine hashes were checked before/after; no binary references from a newer build were launched against older DLLs. Each fixed row uses 10,000 warmup and 100,000 measured calls, with fixture/delegate/thread setup outside measured regions. The worker has a 60-second watchdog, 256-MiB managed heap cap and 512-MiB control-job cap. No retry, minimum selection or post-failure budget widening occurred.

| Operation | Baseline bytes/call | Candidate bytes/call | Pre-recorded ceiling |
|---|---:|---:|---:|
| Ordinary construction | 200 | 32 | 48 |
| Marked construction | 208 | 40 | 56 |
| Construction plus first use | 256 | 160 | 192 |
| Ordinary replay | 232 | 128 | 160 |
| Cross-thread first use | 232 | 128 | 160 |

All five budgets reject baseline and accept candidate. Coarse ceilings were deliberately fixed above prototype observations and below legacy costs before implementation; they are not claims of exact per-call guarantees. First enumeration alone cost more in the prototype, so the combined construction-plus-first-use comparison is retained. These calling-thread byte observations are not an end-to-end speed benchmark.

[Allocation control](CALIBRATION.json) · [Control review](CALIBRATION-REVIEW.json) · [Semantic control](SEMANTIC-CONTROLS.json) · [Frozen receipts](FROZEN-CHECKS.json) · [Build/test receipts](CHECKS.json)

A separate identical semantic worker ran on both versions against the immutable proven prototype: 139 scenarios and 14 fixture checks passed per version, without running allocation loops. Comparisons cover mixed items, rejected predicates, failed acquisition/MoveNext/Current, null enumerators, scalar/empty paths, Current/end state, callback counts and exception type/message. Existing identity, mutation, marked-source and ownership checks are retained; concurrent marked acquisition allows one winner without eagerly pulling the source.

An initial test-file generation path mistake caused one invocation to match no new tests. Its zero-test receipt is retained as preparation only; the corrected focused invocation passed all ten regressions. It is not counted as successful validation. See [preparation correction](PREPARATION.json).

## Sealed evidence and next step

[Evidence archive](EVIDENCE.zip) · [Offline verifier](verify.py) · [Manifest](MANIFEST.json) · [Documentation checks](DOCUMENTATION-CHECKS.json)

Run `python docs/lazy-filter-sequence/verify.py` to replay all case identities, exact exclusions, original tests, compatible controls and source hashes without launching .NET. The archive retains original source snapshots, diagnostics, manifests, transcripts and test reports; raw traces and runtime binaries are not included.

Local candidate qualification is complete. Next: publication review of the concrete candidate and dossier, followed by hosted checks before any merge. No push, PR, release or public export has occurred for this candidate. Previous publication approvals do not cover this new payload. Original build/qualification packets remain local under `.guard-tmp/lazy-filter-engine-20261115` and `.guard-tmp/lazy-filter-qualified-20261115`.
