# Standalone XSLT output isolation candidate - 12 November 2026

This local, unpublished 1.81.3 patch candidate follows the [unsuccessful original reference qualification](../xslt30-qualification/README.md). Its source base is `77301df7f36f1fb8674d67ec6c1fba36c726dac0`; the changed sources are identified separately by hash, not attributed to that commit. Original receipts and frozen release baselines remain historical evidence.

## Changes and adoption

The runner captures secondary outputs in memory through an internal observed-execution helper. It preserves logical output URIs, raw XDM values and serialized text with each output's effective properties. Serialization runs synchronously during output production, so `xsl:try` can catch failures. Secondary assertions use only the current transform's produced outputs. They never fall back to expected files or a prior case's output.

The engine also isolates the explicit principal result document from an enclosing sequence accumulator and returns that document's raw output before the enclosing initial-template result. Without both corrections, JSON values were intercepted by the outer accumulator and the wrong principal result was returned. Ordinary raw initial-template results retain their existing route. Public API signatures are unchanged; normal file delivery outside the conformance helper remains available.

The exact specification revision, affected sections and bounded scope were recorded in [ADOPTION.json](ADOPTION.json) before the changes. No fixture correction, skip admission, schema-coverage expansion or gate adoption is part of this candidate.

## Controls and verification

Four focused unit cases check independent expected files, catchable serialization errors, explicit principal raw output and the ordinary raw-template fallback. The runner's eleven isolation controls require eight genuine passes and three deliberate assertion failures: incorrect output, missing output and stale output must all be refused. The controls also exercise absolute and HTTPS output URIs, logical `current-output-uri()`, text output and duplicate URI errors. Fixture bytes must remain unchanged. Sixteen existing profile/error controls also pass.

Running the same synthetic controls against the original sealed `77301df7` runtime proves incorrect acceptance: its `mismatch` case writes `<wrong/>` over the `<expected/>` golden file and passes, and its following `missing` case also passes without producing that output. The new runner refuses both as assertion mismatches and preserves the golden bytes. This is a new controlled regression proof, separate from the original `xslt-compat-003` observation whose actual/expected XML trees agreed. All original-runner mutations are retained in the disposable control packet.

The targeted `result-document` selection, including the streaming-named set under existing exclusions, changes from 149 passed / 12 failed / 36 skipped to **161 passed / 0 failed / 36 skipped**. This targeted result is not a full-corpus qualification.

Solution verification passes **26,349 implementation tests** across ten projects, including 8,324 XSLT tests. The separate language-server suite passes 90 tests. Frozen QT3 stays 31,397/0/424 and the selected qt4 gate stays 8,941/0/1,918, with 11 experimental passes included in 8,941.

**Full pinned run completed; qualification unsuccessful.** All 234 sets and 14,601 cases reconcile, with zero unselected/not-run cases and zero missing test sets:

| Cohort | Passed | Failed | Skipped |
|---|---:|---:|---:|
| Original unchanged `77301df7` reference | 10,199 | 22 | 4,380 |
| Uncommitted 1.81.3 output-isolation candidate | 10,211 | 10 | 4,380 |

All 12 original result-document failures now pass; no new failures or skip admissions are introduced. All 4,380 skip identities and explanations are unchanged. The remaining failures are `output-0707`, `try-022` and eight `xml-version` cases. The candidate completes in 678.915 seconds under the existing diagnostic limits; this is not performance acceptance. Legacy exit zero is independently rejected as success.

The final audit finds **zero added, changed or removed corpus files** across 12,886 candidate inputs; the original edited checkout's 13,229 inventoried files and all 32 sealed runtime files are unchanged. The temporary idle reservation is released. See [results](RESULTS.json), [remaining failures](failures.csv), [all skip explanations](skips.csv), [sealed original evidence](EVIDENCE.zip), and [manifest](MANIFEST.json). Run `python docs/xslt30-isolation-candidate/verify.py` for read-only verification.

The qualification packet is `D:/Development/Bosak/.guard-tmp/xslt30-isolation-retry-20261112`. Its plan pins W3C `fddf1cf920087e791f13315d68dfbe874d97dc56`, all 234 sets / 14,601 cases, basic/non-schema-aware Legacy/Xslt30 execution, a 1,200-second watchdog and an 8 GiB job limit. These are diagnostic bounds, not performance acceptance. `SOURCE.zip` and `SOURCE-HASHES.json` identify the exact uncommitted candidate; runtime/corpus hashes and full logs remain separate from the original qualification. Successful implementation/gate/control receipts are reused only after verifying all five changed-source hashes and all 32 runtime-layout hashes agree; they are not relabeled as fresh executions.

## Retained preliminary attempts

The first disposable candidate attempt was stopped after detecting checkout line-ending drift in 231 test-set files. A subsequent LF attempt was interrupted by a long host pause: controller progress jumped from 621 to 5,471 elapsed seconds, triggering its timeout on resumption. Both partial receipts are retained and neither is accepted as a complete qualification. The replacement verifies the entire 12,886-file input inventory against the original reference before launch. A temporary helper-thread idle-sleep reservation protects this attempt without changing persistent power or display settings; it releases when the recorded child exits.

The initial 1.81.3 rebuild overlapped an active testhost and hit file locks; the retained repeat after test completion builds with zero warnings/errors. An initial preparation helper also refused fixture-junction Git metadata before launch; source enumeration now ignores submodules, and the temporary junctions were removed after the gates. These diagnostics remain separate from successful verification receipts.

## Next review

The [prepared PR description](PR-DRAFT.md) covers this bounded maintenance correction and its separate qualification evidence. Review and deliver that slice, then investigate the eight XML-version/namespace serialization failures under newly recorded immutable specification pins, followed by adaptive output `output-0707` and `try-022`. Do not waive the remaining failures or promote the standalone catalog to an adopted required gate on this result.

## Reproduction helpers

From the candidate repository root:

```powershell
python docs/xslt30-isolation-candidate/controls.py --output <fresh-control-directory> --assembly tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll
python docs/xslt30-isolation-candidate/prepare.py <fresh-packet-with-LF-corpus>
python docs/xslt30-qualification/run.py --packet <fresh-packet-with-LF-corpus>
python docs/xslt30-isolation-candidate/report.py <completed-packet>
python docs/xslt30-qualification/audit.py --packet <completed-packet> --original-corpus D:/Development/Bosak/tests/xslt30-test
```

The candidate report reconciles every case, checks source snapshots and pinned test-set hashes, and requires exactly the original skip identities before reusing their explanations. A legacy exit zero cannot override failed cases.
