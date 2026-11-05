# W1-A qualification after scoped R2 closure

**Do not execute while GATE.json is blocked.** All commands run from `D:/Development/Bosak` after a reviewed gate transition with the actual replacement and closure references. Commands below use the new scoped [launcher](tools/run.py), importing [process_error_mode](../foundation-runner-safety/tools/run_caller.py). It suppresses child OS error dialogs without inducing crashes or changing machine-wide settings. Every command must succeed; retain failures before using a fresh receipt filename for a retry. Never overwrite historical profile archives or receipts.

The current gate is Ready for implementation, with source edits still requiring immutable semantic adoption first. The acceptance checker remains the original W1 checklist tool and writes its named per-case receipts under `docs/schema-w1-gate-review/evidence/execution/acceptance/`; preserve and hash-bind those receipts into the new candidate execution record without modifying historical prepared evidence.

## Candidate receipts

Before qualification, record candidate Git commit and modified source hashes (a commit alone cannot identify a dirty candidate), accepted contract manifest, effective environment, SDK and corpus revisions, test list, actual loaded binary hashes and immutable semantic adoption references under `evidence/execution/inputs/`. The launcher records HEAD, working status, command, exit code and log hash; this is command provenance, not a substitute for source/binary binding receipts. Qualification remains incomplete until all those bindings and API comparisons are reviewed.

## Build and complete unit suite

```powershell
python docs/foundation-r2-closure/tools/run.py --log docs/foundation-r2-closure/evidence/execution/build.log -- dotnet build Bosak.sln -c Release
python docs/foundation-r2-closure/tools/run.py --log docs/foundation-r2-closure/evidence/execution/units.log -- dotnet test Bosak.sln -c Release --no-build --logger trx --results-directory D:/Development/Bosak/docs/foundation-r2-closure/evidence/execution/unit-trx
```

If and only if XSLT is blocked by Application Control (`0x800711C7`), retain the failed full-suite log/TRX, confirm every other project passed, then execute the existing [temporary-layout workaround](../../run-xslt-tests.ps1) through the same scoped launcher:

```powershell
python docs/foundation-r2-closure/tools/run.py --log docs/foundation-r2-closure/evidence/execution/xslt-workaround.log -- powershell -NoProfile -ExecutionPolicy Bypass -File ./run-xslt-tests.ps1 -Configuration Release
```

The workaround copies `tests/Bosak.Xslt.Tests/bin/Release/net10.0` to `%TEMP%/BosakXsltTestsRun`, uses `dotnet vstest`, and returns its exit code. Record copied DLL hashes, console counters and Application Control reason under `evidence/execution/xslt/`; the fallback does not excuse another failing project. The full-suite outcome is an explicit reviewed combination of the original per-project results and the matching XSLT temporary-copy result.

## Slice acceptance

Implementation must add runtime `OrdinarySourceAdmissionTests` with the checklistÃ¢â‚¬â„¢s `W1_01_` etc. test-name prefixes plus `W1_A_DRAIN_`. These tests do not exist yet; this is a future implementation requirement. The verifier refuses absent/skipped/failed cases and zero-test success.

```powershell
python docs/foundation-r2-closure/tools/run.py --log docs/foundation-r2-closure/evidence/execution/w1-acceptance.log -- dotnet test tests/Bosak.XPath.Runtime.Tests/Bosak.XPath.Runtime.Tests.csproj -c Release --no-build --filter FullyQualifiedName~OrdinarySourceAdmissionTests --logger "trx;LogFileName=w1-a.trx" --results-directory D:/Development/Bosak/docs/foundation-r2-closure/evidence/execution/acceptance-trx
python docs/schema-w1-gate-review/tools/check-acceptance.py --trx docs/foundation-r2-closure/evidence/execution/acceptance-trx/w1-a.trx
```

## Conformance

```powershell
python docs/foundation-r2-closure/tools/run.py --log docs/foundation-r2-closure/evidence/execution/qt3.log -- dotnet run --project tests/Bosak.XPath.Conformance -c Release --no-build -- tests/qt3tests
python docs/foundation-r2-closure/tools/run.py --qt4-gate --log docs/foundation-r2-closure/evidence/execution/qt4.log -- dotnet run --project tests/Bosak.XPath.Conformance40 -c Release --no-build -- tests/qt4tests
```

QT3 must remain exactly 31,397/0/424. Compare qt4 gated outcomes and both skip maps to the pinned qualified baseline; baseline qt4 is 8,941/0/1,918 plus 11 experimental passes. Capture source/corpus and loaded-binary bindings beside the logs. Do not claim gate qualification from exit code alone.

## Completion review

Retain the full suiteÃ¢â‚¬â„¢s per-project TRX/counters, all 18 W1-A case receipts, original 83-control regression receipts, original schema evidence regression results, metadata/ABI/friend comparison and bounded performance observations. Their candidate bindings must agree. Record W1-09/10 as pending W1-D and W1-18 as pending R4; do not claim them passed by ordinary fixture tests. Seal a new execution manifest and review every supported/refused profile before updating status. No current checklist row is changed from NotRun by this preparation.
