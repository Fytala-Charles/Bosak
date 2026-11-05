# F140 — Legacy schema and no-schema compatibility harness

30 October 2026. **Twenty cases pass across the 1.34.1 to 1.34.2 source/binary boundary.** This is bounded core-owned compatibility evidence, not Schema/XmlDb/Braid acceptance or API freeze. Production source, API/version, consumer pins and external owner projects are unchanged.

## Exact execution modes

The [pre-execution scope](SCOPE.json) pins baseline source `4fc353f8fbbc770cdef2618d2d56b59cfa601420` / 1.34.1 and candidate source `c88eea43` / 1.34.2. The baseline was rebuilt from an exact git archive in an ignored isolated directory, with SourceRevisionId/RepositoryCommit set to the baseline revision. All three builds passed with zero warnings/errors.

| Mode | Caller compiled against | Seven loaded Bosak dependencies | Result |
|---|---|---|---|
| Original baseline | 1.34.1 | Rebuilt 1.34.1 | 20/0 |
| Unchanged binary replacement | 1.34.1; caller DLL/deps/runtimeconfig byte-identical | Exact previously qualified 1.34.2 assemblies | 20/0 |
| Recompiled source | 1.34.2; same caller source | Same exact 1.34.2 assemblies | 20/0 |

All ordered case observations are exactly equal. All three retained ZIPs also replay byte-identically in fresh isolated folders. The retained caller still requests four direct Bosak assembly references at 1.34.1 while the runtime loads seven dependencies at 1.34.2. The rebuilt caller requests 1.34.2. File hashes prove the replacement actually happened; compilation alone is not the binary check.

The [20-case matrix](CASES.md) covers all five logical options/context members identified by the [external signature review](../foundation-external-dependencies/BOUNDARY.md), existing schema-element/schema-attribute and no-schema errors, ordinary untyped XML, validation overloads/defaults, legacy annotation/atomization, preserve/strip and named-type paths. No-schema here means no supplied schema set or optional Schema backend; the ordinary XML query still uses Bosak's built-in in-memory node adapter.

## Retained evidence

- [Baseline execution](evidence/baseline.json), [unchanged binary execution](evidence/unchanged-binary.json), [recompiled source execution](evidence/recompiled-source.json)
- [Three runtime bundle manifests](evidence/bundles.json) with per-file hashes; ZIPs retain caller, seven dependencies, Apache license and NOTICE.
- [Validation](RESULTS.json), baseline engine/caller and candidate caller build logs, plus eight passing verifier/mutation checks.
- [Caller source/project](harness/Program.cs), with assembly references supplied explicitly through BosakAssemblyRoot.

The candidate DLLs are exactly those from [1.34.2 cleanup qualification](../streaming-completion-cleanup/evidence/api-comparison.json). Their informational revision still names the pre-commit base 4fc353f8 because that fix was built in its working tree before source commit c88eea43. The qualified content/source-file hashes establish the association; matching informational revision text alone would incorrectly suggest unchanged source. This is retained local evidence, not a publication-ready provenance claim. A committed-source/package build remains separate work.

## Verify or replay

From the repository root, with Python and .NET 10 available:

```powershell
python docs/foundation-legacy-compatibility/tools/verify.py
python -m unittest discover -s docs/foundation-legacy-compatibility/tools -p test_receipts.py -v
Expand-Archive -LiteralPath docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip -DestinationPath .guard-tmp/legacy-compat-replay -Force
dotnet .guard-tmp/legacy-compat-replay/LegacyCaller.dll .guard-tmp/legacy-compat-replay-results.json
```

Write new results to separate paths; keep the original receipts and runtime ZIPs unchanged. To compile the caller against a separately selected exact assembly directory:

```powershell
$legacyAssemblyDirectory = (Resolve-Path src/Bosak.XPath.Api/bin/Release/net10.0).Path
dotnet build docs/foundation-legacy-compatibility/harness/LegacyCaller.csproj -c Release "-p:BosakAssemblyRoot=$legacyAssemblyDirectory" -o .guard-tmp/legacy-compat-new-caller
```

Build the original baseline from its pinned archive, not the current mutable source directory. Reference hashes, caller artifact equality, loaded identities, individual observations and refusal controls are checked independently by the verifier. Its eight controls reject changed outputs, stale loaded versions, altered caller/bundle hashes, changed original reference versions, missing cases and failed cases.

## Scope limits and next task

Primary qualification has 20 unique cases / 60 executions, with another 60 executions replaying the same cases from retained ZIPs, plus eight verifier controls; do not add them to the existing 24,225 unit-test count. Current full engine suites and frozen gates were not rerun because production source is unchanged; their previous exact artifact qualification is retained. Existing API/dependency/lifetime receipts are untouched.

This does not qualify real Schema/XmlDb/Braid callers, published historical 1.26.x pins, signed packages, all legacy profiles, full XSD 1.1, general PSVI/compatible sessions or broad ownership/concurrency. Delegated owners still supply their actual requirements and qualification; C45/shared freeze remain Pending. No release, NuGet publication or consumer adoption occurred.

Next core task: prepare a candidate package set from immutable committed source and run this caller in package-only mode, recording source/package/assembly provenance. Keep it a local qualification candidate; publication, permanent API acceptance and consumer adoption remain separate decisions.
