# Schema adoption of published Core 1.81.2

9 November 2026. **An isolated dependency-only Schema upgrade is prepared and qualified.** The original public checkout and separate native assessment candidate are preserved. No commit, push, production pin update, tag or publication was performed.

## Candidate

[Plan](PLAN.json): public Schema base `20090180755d0427d824b091cf838e9a03ca9adc`, branch `feature/core-1.81.2-adoption`, worktree `D:/Development/Bosak/.worktrees/Bosak.Schema.Core1812`. Four product references and the test project's direct provider reference change from1.26.3 to1.81.2. [Apply-only dependency patch](DEPENDENCY-UPGRADE.patch), [candidate project/documentation files](SCHEMA-CANDIDATE-FILES.zip). No existing C# source changes, version override or Core source project reference is needed. MinVer remains on Schema's0.4 development line.

The original Schema checkout has uncommitted owner/disposition documentation, including its own earlier temporary-override compatibility check. Those edits are excluded from this candidate and must be reconciled when reviewing documentation; do not replace the original checkout wholesale with this snapshot. The dependency patch touches only the two project files.

## Qualification

| Lane | Result |
|---|---|
| Actual1.26.3 baseline | Full Schema solution124/0/0; zero build warnings/errors. |
| Actual edited1.81.2 candidate | Full Schema solution124/0/0; zero build warnings/errors. All nine engine dependencies resolve to1.81.2. |
| Unchanged Schema/test binaries |124/0/0 with only the nine engine DLLs replaced by approved published1.81.2 DLLs; Schema/test/configuration bytes unchanged. |
| Existing price/staged-assertion integration | Eight additional cases pass, including named type/decimal arithmetic, unchanged caller input, explicit structural errors and retained XSD1.1 limitations. |
| Caller ownership | Three additional cases pass: stream remains caller-owned on success/failure; compiled model remains usable after input disposal; supplied reader is not closed by compilation. |
| Package identity |27 lane/DLL hash comparisons match the approved published package payloads. |
| Schema documentation hygiene | Passes with one pre-existing stale-date warning for `.kimi/HANDOVER.md`. |

The baseline/source/binary lanes exercise the **same124 case names**, not372 distinct cases. The candidate exercises **135 distinct cases** including11 additional integration/ownership cases. [Results](RESULTS.json), [case catalogs](TEST-CATALOGS.json), [raw logs/TRX/configuration and retained Schema/test binaries](EXECUTION-RECEIPTS.zip), [base source pins](BASE-SOURCE-PINS.json), [adoption boundary](ADOPTION.json), [harness inputs](INTEGRATION-INPUTS.json), [ownership tests](tools/SchemaOwnershipTests.cs).

## Reproduce the candidate solution

From this Core repository, with .NET10:

```powershell
$schemaCandidate = 'D:/Development/Bosak/.worktrees/Bosak.Schema.Core1812'
$schemaArtifacts = 'D:/Development/Bosak/.guard-tmp/schema-core1812-adoption/candidate'
dotnet build "$schemaCandidate/Bosak.Schema.slnx" -c Release --artifacts-path $schemaArtifacts
dotnet test "$schemaCandidate/Bosak.Schema.slnx" -c Release --artifacts-path $schemaArtifacts --no-build --no-restore
dotnet test .guard-tmp/schema-core1812-adoption/integration/Schema.Adoption.Tests.csproj -c Release
```

The linked integration harness is retained in the receipt archive; it references the actual candidate Schema project and public package dependencies, never Core source projects or DLL HintPaths. Separate private package/output caches preserve the original checkout. No test deliberately launches an unhandled negative child process.

## Remaining gaps and next step

This is compatibility of the current public Schema implementation with the released Core. It does not implement native assessor/advisor/session attachment, operation lease/currentness, compatible component-universe handoff or concurrent-disposal safety. Full XSD1.1 structural validation remains incomplete: the retained extra-child case demonstrates this explicitly. No accepted workload budget or performance guarantee is inferred.

The existing Core optional integration wrapper still pins older Schema source044d2cb2 and its114-test catalog. It is not silently repinned by this work. Review the dependency candidate in the Schema repository first; a later integration-pin refresh must retain its own source/assets and test identities. After explicit delivery direction, commit/push/review Schema's dependency change without overwriting the existing uncommitted owner documents. Native provider adoption and any Schema package release remain separate decisions.

Run `python docs/schema-core-1.81.2-adoption/tools/verify.py` for evidence, source and package-output consistency checks. This is not owner/API acceptance or permission to commit/publish.
