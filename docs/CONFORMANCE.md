<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak conformance results and test exclusions">
  <br><br><h1>Bosak conformance</h1>
  <p>Recorded results, tested profiles, and reasons for excluded tests</p>
</div>

Last updated: 14 November 2026. The frozen release results were recorded on **8 November 2026**. The separate standalone XSLT 3.0 reference qualification below was executed on **12 November 2026**, with its own source, corpus and platform pins.

Observed diagnostic measurements now independently replay the adopted full result locally (817.285s) and hosted (1175.235s); the separate untouched required hosted gate passes in 1056.750s. These observations do not alter the conformance profile, exclusions or limits and do not establish a performance root cause.

The [telemetry trigger correction](xslt30-telemetry-delivery/README.md) restricts automatic diagnostic execution to PR opening; later attempts require explicit dispatch. The unintended extra workflow was cancelled before qualification. Original measured outcomes and the required gate are unchanged.

Operational timing diagnostics: the [separate telemetry candidate](xslt30-telemetry/README.md) preserves the adopted gate and exclusions. Diagnostic capture is not qualification success; no new conformance result is claimed.

The [telemetry delivery refresh](xslt30-telemetry-refresh/README.md) preserves the original measurements and failed attempts while incorporating qualified main with the merged allocation fix. Refreshed required qualification is pending; no new diagnostic capture is launched.

The copied-runner observation now reconciles all14,601 original outcomes (10,221/0/4,380) with11,224 measured cases and3,377 pre-loop exclusions explained by original reasons. It is diagnostic evidence, not an adopted qualification, benchmark or cause of the hosted failures.

The separate [case-cost diagnostic](xslt30-case-cost/README.md) measures a copied runner against exact retained engine/native identities. It does not change the adopted profile or create new qualification/admissions.

The [case-cost delivery refresh](xslt30-case-cost-refresh/README.md) incorporates qualified main while retaining the original diagnostic payload and measurements. No new copied sweep is run, and refreshed required qualification is pending.

The selected stack observation now completes54 cases with52passes and2unchangedschema-aware exclusions;14,547 othercases are unselected. The same trace was reanalyzed for execution-worker stacks, with no new workload or qualification.

The separate [sampled managed-stack investigation](xslt30-stack-profile/README.md) selects54 original comparison cases under exact caller/engine/native pins. It does not change the adopted profile, exclusions or qualification budgets; sampling weights are thread wall-time, not CPU percentages.

The [sampled-stack delivery refresh](xslt30-stack-refresh/README.md) retains the original trace and attribution corrections on qualified main. No trace is recollected and no conformance admission is made; refreshed required qualification is pending.

The [GC/allocation investigation](xslt30-gc-correlation/README.md) retains a failed broad trace and a completed54-case narrow capture (52passes/2unchanged schema-aware exclusions). It measures a copied caller without new gate qualification, admissions, exclusions or production-budget changes.

The [GC/allocation delivery refresh](xslt30-gc-refresh/README.md) preserves original captures, corrections and failed attempts on qualified main. No capture or conformance admission is added; refreshed required qualification is pending.

The [current allocation observation](current-allocation/README.md) is a54-case diagnostic, not a new conformance admission. Post-PR488 main independently retains10,221/0/4,380 across14,601 identities under unchanged policy; the original exclusion map remains authoritative.

## Results and scope

The following release table uses the successful hosted preparation of the **published Bosak 1.81.2** release. Passing the selected tests is evidence for the identified profile and corpus revision; it is not a claim of complete XPath, XQuery, XSLT, or XSD conformance. Skips are visible exclusions, not passes. Tests outside a selected gate are not counted as skips within that gate. The later standalone reference measurement is reported separately.

| Gate / test scope | Passed | Failed | Skipped | What the result establishes |
|---|---:|---:|---:|---|
| Frozen QT3 XPath / XQuery 3.1-compatible profile | 31,397 | 0 | 424 | The admitted QT3 cases pass under the frozen routing and dependency policy. [Log](conformance/evidence/qt3.txt), [skip inventory](conformance/qt3-skips.csv). |
| Selected XPath 4.0 gate: 230 test sets | 8,941 | 0 | 1,918 | Gate-only execution; excludes the rest of the corpus. **11 of these passes** required `XPath40Experimental`; they are included in 8,941, not additional passes. [Log](conformance/evidence/qt4.txt), [skip inventory](conformance/qt4-skips.csv). |
| Full 4.0 exploratory sweep | — | — | — | Not run in this selected gate-only receipt. No full-corpus result is asserted. Exploratory failures are reported separately and do not fail the gated run. |
| Solution implementation tests: ten projects | 26,345 | 0 | 0 | Regression and implementation coverage, including the XSLT and XQuery subsets below. [Original TRX archive](release-linux-collection-hosted/nuget-42479094503.zip), [project totals](conformance/summary.json). |
| XSLT implementation tests, included in solution total | 8,320 | 0 | 0 | `Bosak.Xslt.Tests`; not a standalone W3C XSLT corpus result. |
| XQuery implementation tests, included in solution total | 841 | 0 | 0 | `Bosak.XQuery.Tests`; not a separate full XQuery conformance claim. |
| Standalone W3C XSLT 3.0 / 4.0 corpus | — | — | — | No aggregate result is supplied by these selected release receipts. See the separate [12 November XSLT 3.0 reference qualification](xslt30-qualification/README.md). A standalone XSLT 4.0 aggregate remains unreported. |

Totals from different rows must not be added: the solution subsets overlap their parent total, and the QT3 and qt4 corpora contain overlapping tests. An em dash means no result is asserted here, not zero failures or zero exclusions.

## Standalone XSLT 3.0 reference qualification

**12 November 2026 — original reference unsuccessful; final retained-profile qualification successful.** The separate cohorts below retain their original evidence. These qualification receipts preceded the operational adoption recorded below.

| Scope | Passed | Failed | Skipped | Not run |
|---|---:|---:|---:|---:|
| Entire 234-set / 14,601-case W3C catalog; basic/non-schema-aware, Legacy processor, Xslt30 catalog profile | 10,199 | 22 | 4,380 | 0 |
| Subsequent unpublished1.81.3 output-isolation candidate; same pinned catalog and retained profile/exclusions | 10,211 | 10 | 4,380 | 0 |
| Subsequent unpublished 1.81.4 namespace correction; same pinned catalog and retained profile/exclusions | 10,219 | 2 | 4,380 | 0 |
| Subsequent unpublished 1.81.5 final corrections; same pinned catalog and retained profile/exclusions | 10,221 | 0 | 4,380 | 0 |

Source is `77301df7f36f1fb8674d67ec6c1fba36c726dac0`; corpus is `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`. Execution used Windows x64 and SDK10.0.301/Release, a 1,200-second watchdog and 8 GiB job-memory limit; the run completed in 870.282 seconds. These diagnostic limits are not performance acceptance. No test-set file was missing and no filter, resume or retry was used.

The legacy runner returned zero despite the failures; the qualification controller independently rejected success. All [22 failures](xslt30-qualification/failures.csv) and [4,380 skip explanations](xslt30-qualification/skips.csv) are retained. Printed reasons, silent-skip policy explanations and historical named-skip notes are distinguished.

**Write-audit caveat:** the runner changed four tracked resources in the disposable corpus, including an expected-output file, and created 250 files. Those bytes are retained. A [source-path review](xslt30-qualification/WRITE-ALIAS-REVIEW.json) identifies a produced/expected-file collision for `xslt-compat-003`; that case's observed XML trees agree, so no incorrect-result acceptance is claimed from this observation. Test-set definitions, the sealed runtime layout, and the original edited checkout's 13,229 inventoried files are unchanged. Fixture-write isolation and assertion independence need review before gate adoption or stronger reliance on passing-case counts. The [qualification dossier](xslt30-qualification/README.md) records the exact inputs, complete case ledger, integrity checks, failures and remaining work. No frozen gate, skip policy or engine behavior changed.

**Subsequent local candidate:** the [unpublished1.81.3 correction](xslt30-isolation-candidate/README.md) captures secondary output independently of golden files and fixes principal raw result-document delivery. Its complete678.915-second rerun has10,211 passes,10 failures and4,380 unchanged skips, with zero missing/unselected cases and zero corpus writes. All12 original result-document failures now pass. Synthetic controls prove that the original runner accepted deliberately wrong/missing outputs after a golden-file overwrite; the candidate refuses wrong/missing/stale outputs and preserves fixture bytes. This proof is separate from the original `xslt-compat-003` observation. Eight XML-version cases, `output-0707` and `try-022` remain failures; qualification is unsuccessful and no standalone gate is adopted. Exact uncommitted source hashes and all original/interrupted receipts are retained separately from the reference77301df7 measurement.

**Namespace follow-up:** the [separate 1.81.4 correction](xslt30-xml-version/README.md), based on delivered PR #474, resolves all eight XML-version failures. The complete 776.080-second rerun records **10,219 passed / 2 failed / 4,380 skipped**, with every original case accounted for and zero corpus writes. All skip identities and explanations are unchanged; `output-0707` and `try-022` remain failures. Raw namespace queries and serialized output have separate regression coverage. The legacy console's rounded `100.0%` line does not establish success: the independent controller rejects qualification. Earlier cohorts retain their own source hashes and receipts. No full-conformance claim or standalone gate adoption follows.

**Final follow-up:** The [final standalone corrections](xslt30-final-cases/README.md) resolve `output-0707` and `try-022`. The complete pinned W3C XSLT 3.0 catalog records **10,221 passed / 0 failed / 4,380 excluded** in 829.117 seconds, with all 14,601 cases accounted for, unchanged exclusion identities/reasons and zero corpus writes. All 26,359 solution tests and 90 separate language-server tests pass. Frozen QT3 remains 31,397/0/424 and selected qt4 remains 8,941/0/1,918 with identical named skip maps. Development Core/server metadata is 1.81.5, unpublished; published Core remains 1.81.2 and the extension remains 0.2.6. This qualifies the retained basic/non-schema-aware Legacy/Xslt30 profile; it does not establish complete conformance or adopt a standalone release gate. [All named exclusions and explanations](xslt30-final-cases/skips.csv) remain visible. The runner now prints selected-case results and counts; it retains its legacy exit behavior. Independent completeness and integrity checks establish this result. The rejected first attempt (10,220/1/4,380, namespace-2602) remains separate evidence; the corrected run includes a new nested-collector regression case.

## Hosted gate timing evidence

The [delivery refresh](xslt30-timing-refresh/README.md) incorporates merged PR #489 after independent hosted qualification. The earlier timing report and its original timeout remain historical evidence; the refreshed documentation PR awaits its own required check.

The [hosted XSLT timing investigation](xslt30-hosted-timing/README.md) replays seven retained attempts without new diagnostic execution or gate changes. Two same-checkout comparisons have identical32-file application manifests yet include both a success and a1200-second timeout. Four complete runs each replay10,221/0/4,380; three incomplete timeouts remain failed. Equivalent-tree/different-payload comparisons are separate. Twenty-second progress samples show earlier delay, but cannot identify CPU/GC/I/O causes or individual test durations. No failure rate, regional cause or performance acceptance is inferred. Next prepare separate host/process telemetry under unchanged bounds before another semantic slice. PR483 merged04eea731 after all six checks and hosted replay passed; parent timeouts remain original evidence.

## Scoped named-template prefix reporting follow-up

The [named-template prefix reporting correction](xslt30-template-name-fix/README.md) translates XPST0081 to XTSE0280 only around name expansion in named-template binding collection, for well-formed prefixed lexical names. Existing namespace resolution and shared QName expansion stay unchanged, as do XPath diagnostics, malformed-name precedence, EQNames, reserved names and preprocessing. Twenty-eight focused tests pass; identical sources against sealed 1.81.7 binaries give ten expected reporting failures and eighteen protective passes. All 26,422 solution cases pass in one invocation, including 8,397 XSLT cases; 90 separate language-server tests pass. Frozen QT3 stays 31,397/0/424 and selected qt4 stays 8,941/0/1,918 (including 11 experimental passes), with every skip name/reason unchanged. The complete pinned standalone gate stays 10,221/0/4,380 in 783.428 seconds, all 14,601 identities accounted for and unchanged corpus/runtime bytes under the original 1,200-second/8 GiB limits.

The original 579-case error audit remains 513/0/66. Only error-0280d/0280f change reported identities: both now match directly without aliases, while retaining the original XPST0081 exceptions as inner causes. Remaining 25 reported-code differences, 23 uncoded reports and 55 overlapping static-label/execute-call observations still need separate fixture/spec review. The separate parent-main timeout remains failed and retained, with no rerun or budget change. The first ineffective guard, failed focused run and caught stack are retained; the revised implementation decision was recorded before edits. Development Core/server 1.81.8 remains unpublished; no public signature, fixture, exclusion/admission, budget or ruleset change.

## Scoped module-placement reporting follow-up

The [module-placement reporting correction](xslt30-module-placement-fix/README.md) chooses XTSE0170/XTSE0190 only where the existing validator rejects nested include/import declarations. Other structural codes, acceptance paths, use-package/override parent exemptions, preprocessing, module resolution and legacy matching stay unchanged. Twenty-one focused tests pass; identical sources against sealed 1.81.6 binaries give eight expected reporting failures and thirteen protective passes. All 26,394 solution tests pass in one invocation, including 8,369 XSLT cases; 90 separate language-server tests pass. Frozen QT3 stays 31,397/0/424 and gated qt4 stays 8,941/0/1,918 (including 11 experimental passes), with identical skip names/reasons. The complete pinned standalone gate stays 10,221/0/4,380 in 825.655 seconds, all 14,601 case identities accounted for and unchanged corpus/runtime bytes under the original 1,200-second/8 GiB limits.

The same 579-case error audit remains 513/0/66. Six leading-code reports change on the shared placement path: error-0170b/0190b now match without aliases; four related cases already allow either the generic or specific code. Only two catalog-code assessments improve; no new pass or admission is claimed. Remaining 27 reported-code differences, 23 uncoded reports and 55 overlapping static-label/execute-call observations still require review. Specific diagnostics are chosen without blanket normative defect verdicts; generic/specific rules can overlap. Development Core/server 1.81.7 remains unpublished; no public signature, fixture, exclusion, gate-budget or ruleset change.

## Scoped analyze-string reporting follow-up

The [scoped analyze-string correction](xslt30-analyze-string-fix/README.md) reports XTDE1140/XTDE1145 for invalid effective regex/flags while preserving XPath errors from AVTs, child instructions and ordinary functions. Fourteen focused tests pass; identical sources against sealed 1.81.5 binaries give four expected failures and ten passes. All 26,373 solution cases pass across the retained restricted run and authorized 47-case database retry; 90 separate language-server tests pass. Frozen QT3 remains 31,397/0/424 and selected qt4 remains 8,941/0/1,918 (including 11 experimental passes), with every skip name/reason unchanged. The complete clean-source standalone gate remains 10,221/0/4,380 in 713.170 seconds, all 14,601 cases accounted for, with unchanged fixtures/runtime and original 1,200-second/8 GiB bounds.

The original 579-case error audit remains 513/0/66. Only error-1140a and error-1145a change reported identities, now the expected local codes without legacy aliases; named xsl:catch controls independently prove the standard QNames. Remaining 29 code differences, 23 uncoded reports and 55 execute-call/static-label observations still require review. Compile is public; TransformObserved is an internal capture helper; these call boundaries are not normative phase verdicts. No cases are admitted. Development Core/server 1.81.6 is unpublished; no public signature, fixture, skip, gate-budget or ruleset change.

## Adopted standalone CI gate

The [standalone XSLT gate is adopted](xslt30-gate-adoption/README.md): [PR #477](https://github.com/Fytala-Charles/Bosak/pull/477) merged at `b3e2839cfeb1c6e6e9add98b13d84ea7c0827b0c` after all six hosted checks passed. Independent replay of the hosted artifact confirms **10,221 passed / 0 failed / 4,380 excluded**, all 14,601 cases accounted for in 1,095.141 seconds, with unchanged fixture/runtime bytes and exclusion identities/reasons. The active main ruleset now requires `Standalone XSLT 3.0 (pinned basic profile)` from GitHub Actions. Existing pull-request/deletion rules, branch scope and bypass actors are preserved. The retained Legacy/Xslt30 basic/non-schema-aware profile, immutable corpus, 1,200-second/8 GiB bounds and frozen QT3/qt4 policies are unchanged. This is operational adoption; no engine/API/package change, publication, new exclusion or complete-conformance claim. The published-release results and all earlier qualification receipts remain historical.

## Separate targeted error-set measurement

The [targeted error-set audit](xslt30-error-audit/README.md) measures all 579 original cases at the unchanged pinned source/corpus: **513 passed / 0 failed / 66 skipped** under the existing Legacy oracle. Inputs and runtime bytes are unchanged. The set covers static and dynamic errors; its legacy matcher has code/message aliases, including dynamic codes accepted for some static expectations. Seventy-one passing cases share expected codes with alias tables, but actual alias use and error phase are not logged. These results are not admission evidence by themselves. The adopted required gate stays **10,221/0/4,380**, with all original exclusions and reasons retained. Next is an independent exact-code/phase audit; no engine, runner, workflow, version, ruleset or exclusion change is made. These cases overlap the required-gate exclusions and must not be added to its totals.

**Subsequent watchdog observation:** Separate from this targeted audit, [automatic main run43300842122](https://github.com/Fytala-Charles/Bosak/actions/runs/43300842122) timed out after1,200.250seconds at date-020, an intentional million-iteration current-dateTime stability test. There were no observed assertion-failure lines, but no complete result; the gate correctly refused success. Fixture/runtime bytes remained unchanged. The [original failed artifact](xslt30-error-audit/POSTMERGE-TIMEOUT.zip) and [observation](xslt30-error-audit/POSTMERGE-GATE-OBSERVATION.json) are retained. A fresh same-commit diagnostic run43305315323 is pending at this checkpoint; no limit, fixture, exclusion or rule was relaxed. The original failed run must not be relabeled as a pass.

## Separate identity and API-boundary observations

The [error identity/API-boundary audit](xslt30-error-oracle/README.md) preserves every legacy outcome (**513/0/66**) and all skip reasons. Independent observations distinguish5 explicit QName matches,445 local-code matches,31 differing reported codes,23 uncoded exceptions and9 normal-result passes. The71 potential alias-review cases resolve to54 observed tolerance branches and17 ordinary branches. Exceptions surfaced in255 compile calls and249 execute calls;55 static-labelled expectations surfaced in the latter, without automatic normative defect verdicts. Sixteen controls pass; all nine engine DLL hashes and fixture/runtime bytes are unchanged. The reviewed next correction is limited to xsl:analyze-string reporting for error-1140a/1145a (XTDE1140/XTDE1145 instead of FORX0002/FORX0001), preserving XPath errors elsewhere. No production implementation or gate admission occurs; required counts stay10,221/0/4,380. These classes and the55 phase candidates overlap; do not add them to adopted-gate totals.

## Historical standalone CI proposal

The [pinned standalone XSLT gate proposal](xslt30-gate/README.md) adds a separate Windows CI check for the previously qualified basic/non-schema-aware Legacy/Xslt30 profile: 10,221 passed / 0 failed / 4,380 excluded, with exact case coverage, unchanged exclusion identities/reasons and fixture/runtime integrity. The immutable corpus and sealed baseline are hash-bound; complete outcomes override legacy exit zero. Existing engine/API semantics, version1.81.5 and frozen QT3/qt4 gates are unchanged. No publication or branch-protection setting change. Adoption is proposed in this review branch; previous qualification dossiers remain historical. Fresh end-to-end gate execution passes: **10,221 passed / 0 failed / 4,380 excluded**, all 14,601 cases accounted for in 761.060 seconds, with unchanged corpus/runtime bytes and exact exclusion identities/reasons. The tested clean source is `d30087402ca49586b67bab5fc744b2bb18946ae6`. All fourteen gate controls pass; unchanged-engine solution coverage is supplied by successful merged-main CI43285185767 and Schema43285185880. No engine, fixture or public API change is included. Hosted checks for this proposal remain pending. The new check runs for main pull requests and pushes; making it a mandatory merge check additionally requires repository ruleset/branch-protection configuration, outside this PR. The published-release table and original standalone receipts above are unchanged.

## Why tests were skipped

Every recorded skip has a named entry and explanation in the [QT3 CSV](conformance/qt3-skips.csv) or [qt4 CSV](conformance/qt4-skips.csv). Both inventories include the original reason, the catalog file, inherited and case-level dependency declarations, and a readable explanation. For dependency refusals, the explanation identifies explicit profile mismatches from the pinned filter policy. It is documentation derived from the recorded exclusion, not a fresh test execution or a new capability assessment. Multiple applicable mismatches can be listed for one case.

### QT3: 424 recorded skips

| Recorded exclusion | Cases | Brief explanation |
|---|---:|---|
| Dependency / profile mismatch | 368 | Requirements do not match the frozen harness profile: for example exact earlier-language semantics, static typing, a different XML/XSD profile, or a requirement that an admitted feature be absent. Each case's specific requirements are listed in the CSV. |
| XMark benchmark set | 21 | Deliberately excluded from unattended sweeps; this is an execution-scope decision, not a passing benchmark. |
| Catalog consistency checks | 14 | Each loads the full corpus; excluded to avoid an unattended hang. |
| Schema-loading harness errors | 6 | `XmlSchemaException` rejects the fixture's multiple-name `substitutionGroup` value. These remain harness exclusions, not successful schema tests. |
| Demo applications | 5 | Deliberately excluded from unattended sweeps. |
| Decimal literal precision | 2 | The legacy .NET `decimal` route cannot retain the literal's required precision. |
| Extremely large duration component | 2 | A 10^27-year component is rejected lexically before the expected range detection. |
| Extremely large year | 2 | A 10^26-digit year is rejected lexically before the expected range detection. |
| Negative-year operation | 2 | These recorded cases need a `DateTimeOffset` operation involving year −2, outside its supported years. Extended-year construction elsewhere does not remove this operation limit. |
| Enormous integer range | 1 | The implementation does not detect the required range limit before attempting to enumerate billions of integers. Excluded to avoid runaway evaluation. |
| External HTTP resource | 1 | In the recorded environment, timeanddate.com returned a Cloudflare JavaScript challenge / HTTP 403 to .NET `HttpClient`. This is not a successful resource test. |
| **Total** | **424** | [Original names and reasons](conformance/evidence/qt3-skips.txt). |

### Selected qt4 gate: 1,918 recorded skips

All **1,918** skips are recorded as dependency refusals. The [per-case inventory](conformance/qt4-skips.csv) explains their language/version and feature requirements. Many require an XQuery-only profile; this harness executes XPath and has no XQuery 4.0 route. Other exclusions include unsupported features, negative requirements for admitted features, and version/default-language requirements outside the selected profile.

These are the skips **inside the selected 230 sets**. Ungated sets omitted by `BOSAK_QT4_GATE_ONLY=1` are outside this result, not 1,918 additional failed or skipped cases. The 11 experimental-level passes are a subset of the admitted passing cases; experimental level and exploratory gate membership are different distinctions.

### Interpreting exclusions

The historical admission of schema-related feature names is not certification of full XSD 1.1 validation or provider integration. See the [integration guide](INTEGRATION.md) and [known limits](../AGENTS.md#known-limitations). Likewise, opt-in exact arithmetic does not establish arbitrary-precision behavior on every legacy numeric route.

A known failure must remain a failure in its original receipt. A fixture problem, environmental restriction, unsupported capability, and a deliberate sweep exclusion must retain their separate explanations. Removing a skip requires evidence for the original case and an explicit baseline update; changing the summary must not weaken the gate or rewrite historical receipts.

## Revisions and execution evidence

| Item | Recorded identity |
|---|---|
| Product / source | Bosak 1.81.2, [`7249d0e01628c47f92b89a3380bc69cf58d70045`](https://github.com/Fytala-Charles/Bosak/commit/7249d0e01628c47f92b89a3380bc69cf58d70045) |
| Producer | [Successful hosted preparation run 42479094503](https://github.com/Fytala-Charles/Bosak/actions/runs/42479094503), 8 November 2026 |
| Platform | Ubuntu 24.04.5, GitHub runner image `20261101.360.1`; this receipt does not establish the same result on every platform |
| Runtime / build | .NET SDK 10.0.301, `net10.0`, Release |
| QT3 fixture revision | `201a6e466940cdfc727f4babfedcde5332b9f578` |
| qt4 fixture revision | `68910080d2d4d4092a1c6a908998ceec64ffc6d4` |
| Gate selection | Frozen QT3 profile; qt4 `BOSAK_QT4_GATE_ONLY=1`, 230 selected sets |
| Raw evidence | [Hosted packet, run metadata, logs and TRX](release-linux-collection-hosted/README.md); [release publication disposition](release-1.81.2-publication/README.md) |
| Derived inventory | [Machine-readable summary and hashes](conformance/summary.json); [generator](conformance/generate-summary.py) |

The preserved earlier [hosted preparation failure](release-1.81.2-hosted/README.md) had six QT3 collection-fixture URI failures. The [cross-platform fixture correction](release-linux-collection-fix/README.md) and later successful run are separate evidence; the original run remains failed.

## Reproducing and maintaining the summary

Verify the documentation against the retained producer archive and clean, pinned fixture checkouts:

```powershell
python docs/conformance/generate-summary.py --check
python -m unittest discover -s docs/conformance -p "test_*.py"
```

To regenerate these derived files from the **same sealed cohort**, omit `--check`. An isolated worktree may use `--corpus-root <existing-tests-directory>` to read existing clean, pinned `qt3tests` and `qt4tests` checkouts without copying or modifying them. The generator refuses mismatched corpus revisions, unresolved/duplicate skipped names, unexplained dependency refusals, and inconsistent counts. It copies the original logs and skip lists without modifying their bytes. Explanations do not replace the engine's admission logic.

For a new engine result, use fresh output paths and keep the old receipt intact. Record the source, fixture revisions, platform, SDK, selection, complete failures, and per-case skip reasons before changing the summary. Review the full skip-map difference, not only its size. Update the generator's selected cohort and expectations together with this page; do not relabel the 8 November run as a new execution.

The existing manual gate commands are:

```powershell
$env:BOSAK_QT3_DUMP_SKIPS = '<fresh-directory>/qt3-skips.txt'
dotnet run --project tests/Bosak.XPath.Conformance -c Release -- tests/qt3tests
$env:BOSAK_QT4_GATE_ONLY = '1'
$env:BOSAK_QT4_DUMP_SKIPS = '<fresh-directory>/qt4-skips.txt'
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- tests/qt4tests
```

Run these in a dedicated shell; remove `BOSAK_QT4_GATE_ONLY` to request the full exploratory sweep. Set `BOSAK_QT4_TEST_TIMEOUT_SECS` when an explicitly selected watchdog limit is needed (default 120 seconds per test). The 4.0 runner exits 2 for gated-set failures; exit 0 alone does not mean every exploratory case passed. See [AGENTS.md](../AGENTS.md#conformance-harnesses) for the frozen gate contract and [implementation status](IMPLEMENTATION_STATUS.md) for later engineering checkpoints.
