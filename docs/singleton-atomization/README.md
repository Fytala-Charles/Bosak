# Singleton atomization candidate

Development 1.81.16, unpublished. Only nonmaterialized sequence input to the private singleton Atomize helper changes: retain first value and exact count while consuming every input item, then recurse or report cardinality. All materialized/scalar/node/array/JNode branches and arithmetic callers remain unchanged. No early-exit optimization, length shortcut, disposal, shared-view change or new public API.

[Immutable adoption and budgets](ADOPTION.json) precede implementation. Qualified 1.81.15 closure/source is the input baseline; engineVersion 1.81.16 denotes the planned candidate. Sixteen focused regressions pass, including real streaming/PSVI child/attribute values, held-record replay, reader ownership, full length and tail-failure order, marked refusal, arithmetic callers and isolated allocation.

One compatible production worker executable, built against lowest qualified 1.81.15 references and run once per version, measured empty 64 -> 32 and singleton 352 -> 32 B/call. Pre-recorded ceilings 48/64 reject both legacy rows and accept both candidate rows. Same controlled 32-byte source enumerator, 10k warmup/100k measured, 60s watchdog/256MiB managed/512MiB job cap; setup outside measurement. No resampling, widening or end-to-end speed claim. A separate identical semantic executable passed 84 cases per version, with exact case results/event order equal across versions, without allocation loops.

The first focused run passed both provider checks and 13 runtime cases, but one new general-comparison caller fixture wrongly prohibited TryGetLength. Corrected caller fixture returns unknown; direct Atomize tests still prohibit shortcuts. Production code/budgets unchanged. Original failure/source retained separately. A semantic control support-loader path error occurred before workload launch; the already-built worker was reused after path correction.

[Allocation controls](CALIBRATION.json) | [Control review](CALIBRATION-REVIEW.json) | [Semantic controls](SEMANTIC-CONTROLS.json) | [Scope audit](SOURCE-AUDIT.json) | [Fixture correction](PREPARATION-CORRECTION.json)

Full solution 26,482 and language server 90 passed. Frozen QT3 remains exactly 31,397/0/424 and gated qt4 8,941/0/1,918 with 11 experimental passes; all skip names/reasons remain exact. Pinned standalone qualification completed: 10,221 passed / 0 failed / 4,380 skipped in 776.575s. All 14,601 identities and exact exclusions reconcile against corpus fddf1cf920087e791f13315d68dfbe874d97dc56 under unchanged 1,200-second/8-GiB limits, Legacy/Xslt30 basic non-schema-aware profile. Qualified source `5a85c75230b5bc1e2b09e046c0a58bb6d6ba96c4`. Skips are not passes. [Build/tests](CHECKS.json) | [Frozen gates](FROZEN-CHECKS.json) No push, public export or package release. Source counts approaching Int32 cardinality/resource exhaustion remain outside the bounded prototype; no new unbounded resource guarantee is claimed. Next after local qualification: review the exact publication payload, then hosted checks before merge. Original packet .guard-tmp/singleton-atomize-engine-20261116.


[Sealed evidence](EVIDENCE.zip) | [Results](RESULTS.json) | [Offline verifier](verify.py) | [Manifest](MANIFEST.json) | [Documentation checks](DOCUMENTATION-CHECKS.json)

Run `python docs/singleton-atomization/verify.py` to reconcile all outcomes, frozen exclusions, original/corrected tests, compatible controls and qualified source hashes offline without launching .NET. The archive retains original fixture failure/source and semantic prelaunch correction separately. Runtime binaries and raw traces are excluded.

Local qualification complete. Next: review the exact candidate/dossier for publication, then hosted checks before merge. Nothing has been pushed or published; prior publication approval does not cover this new payload. Original standalone packet .guard-tmp/singleton-atomize-qualified-20261116.
