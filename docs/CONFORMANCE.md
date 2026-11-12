<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak conformance results and test exclusions">
  <br><br><h1>Bosak conformance</h1>
  <p>Recorded results, tested profiles, and reasons for excluded tests</p>
</div>

Last updated: 12 November 2026. The frozen release results were recorded on **8 November 2026**. The separate standalone XSLT 3.0 reference qualification below was executed on **12 November 2026**, with its own source, corpus and platform pins.

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

**12 November 2026 — completed, with qualification unsuccessful.** This is a reference measurement under existing exclusions, not an adopted required gate.

| Scope | Passed | Failed | Skipped | Not run |
|---|---:|---:|---:|---:|
| Entire 234-set / 14,601-case W3C catalog; basic/non-schema-aware, Legacy processor, Xslt30 catalog profile | 10,199 | 22 | 4,380 | 0 |
| Subsequent unpublished1.81.3 output-isolation candidate; same pinned catalog and retained profile/exclusions | 10,211 | 10 | 4,380 | 0 |

Source is `77301df7f36f1fb8674d67ec6c1fba36c726dac0`; corpus is `w3c/xslt30-test@fddf1cf920087e791f13315d68dfbe874d97dc56`. Execution used Windows x64 and SDK10.0.301/Release, a 1,200-second watchdog and 8 GiB job-memory limit; the run completed in 870.282 seconds. These diagnostic limits are not performance acceptance. No test-set file was missing and no filter, resume or retry was used.

The legacy runner returned zero despite the failures; the qualification controller independently rejected success. All [22 failures](xslt30-qualification/failures.csv) and [4,380 skip explanations](xslt30-qualification/skips.csv) are retained. Printed reasons, silent-skip policy explanations and historical named-skip notes are distinguished.

**Write-audit caveat:** the runner changed four tracked resources in the disposable corpus, including an expected-output file, and created 250 files. Those bytes are retained. A [source-path review](xslt30-qualification/WRITE-ALIAS-REVIEW.json) identifies a produced/expected-file collision for `xslt-compat-003`; that case's observed XML trees agree, so no incorrect-result acceptance is claimed from this observation. Test-set definitions, the sealed runtime layout, and the original edited checkout's 13,229 inventoried files are unchanged. Fixture-write isolation and assertion independence need review before gate adoption or stronger reliance on passing-case counts. The [qualification dossier](xslt30-qualification/README.md) records the exact inputs, complete case ledger, integrity checks, failures and remaining work. No frozen gate, skip policy or engine behavior changed.

**Subsequent local candidate:** the [unpublished1.81.3 correction](xslt30-isolation-candidate/README.md) captures secondary output independently of golden files and fixes principal raw result-document delivery. Its complete678.915-second rerun has10,211 passes,10 failures and4,380 unchanged skips, with zero missing/unselected cases and zero corpus writes. All12 original result-document failures now pass. Synthetic controls prove that the original runner accepted deliberately wrong/missing outputs after a golden-file overwrite; the candidate refuses wrong/missing/stale outputs and preserves fixture bytes. This proof is separate from the original `xslt-compat-003` observation. Eight XML-version cases, `output-0707` and `try-022` remain failures; qualification is unsuccessful and no standalone gate is adopted. Exact uncommitted source hashes and all original/interrupted receipts are retained separately from the reference77301df7 measurement.

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
