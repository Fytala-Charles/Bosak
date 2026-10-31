# F140 — Immutable-source package-only qualification

30 October 2026. **Seven local candidate packages pass selected source/binary caller checks.** No NuGet publication, signing, tag, consumer adoption or API freeze occurred. Production source/version and delegated owner projects are unchanged.

## Source and package identity

Compiled exact source commit `c88eea4308c0e49a81e7bfe2f481319f2e4d208c`, candidate 1.34.2, from an isolated git archive. SourceRevisionId and RepositoryCommit were pinned explicitly. All seven assembly informational versions now name that commit; package nuspec repository commits agree. This replaces the earlier working-tree provenance ambiguity for this new artifact cohort; [old qualification receipts](../foundation-legacy-compatibility/README.md) remain unchanged.

| Package in the Core/API dependency closure | Version |
|---|---|
| `Bosak.XPath.Core` | 1.34.2 |
| `Bosak.XPath.Parser` | 1.34.2 |
| `Bosak.XPath.Compiler` | 1.34.2 |
| `Bosak.XPath.Providers` | 1.34.2 |
| `Bosak.XPath.Runtime` | 1.34.2 |
| `Bosak.XPath.Standard` | 1.34.2 |
| `Bosak.XPath.Api` | 1.34.2 |

This is the seven-package Core/API closure, not the complete product publication set. XSLT, XQuery, the optional database transport package and LanguageServer are not packaged or qualified as packages here.

The [recorded recipe](tools/build.py) packs existing committed project metadata plus [CandidateAssets.targets](tools/CandidateAssets.targets), which explicitly includes the exact committed NOTICE file. It does not edit production source or stock pack items. All packages retain exact committed license/NOTICE/README payloads; Standard also retains its Unicode license. Before any publication, the release pipeline must deliberately retain this asset recipe or adopt an equivalent separately reviewed stock-pack change. This local recipe is not silently represented as an unmodified stock-pack command.

## Package-only execution

The consumer has one exact PackageReference to `Bosak.XPath.Api [1.34.2]`, no engine ProjectReference or DLL HintPath. Restore uses only the private local feed, a fresh private cache and cleared fallback folders. Assets record exactly seven Bosak packages. Framework reference packs/runtime remain supplied by the installed .NET 10 SDK/runtime.

| Check | Result |
|---|---|
| Immutable-source engine build and package-consumer build | Zero warnings/errors |
| Existing 20-case caller rebuilt with PackageReference | 20/0; observations equal retained 1.34.1 baseline |
| Unchanged 1.34.1 caller with package-restored dependencies | 20/0; caller DLL/deps/runtimeconfig hashes retained |
| Retained feed replay with another fresh cache | 20/0; observations and loaded hashes identical |
| Package/archive/cache/output/loaded identity chain | All seven match; SHA-256 plus NuGet SHA-512 recorded |
| Compiled public API comparison | 2,205 metadata entries / 78 types unchanged in the seven selected assemblies |
| Verifier refusal controls | 8/0 |

The caller source is pinned independently to commit `becfc051219baf99743b1f5dd9ddb1707e7fb390`. Cases are the [existing legacy/no-schema matrix](../foundation-legacy-compatibility/CASES.md); no new engine behavior is adopted. There are 20 unique cases, two primary modes (40 executions) and one replay of those cases (20 more), not 60 independent tests. These counts are separate from the earlier 24,225 solution tests.

## Evidence and reproduce

- [Package and execution summary](evidence/packages.json), [source/asset pins](evidence/source-pins.json), [pre-execution scope](SCOPE.json)
- [Retained local feed](evidence/candidate-feed.zip), [package-only assets](evidence/project.assets.json), [private restore configuration](evidence/NuGet.Config)
- [Package source caller](evidence/package-source-run.json), [unchanged binary caller](evidence/unchanged-binary-run.json), [feed replay](evidence/replay.json)
- [Committed caller source](evidence/caller-source.txt), [public metadata](evidence/public-api.json), [commands and archive pin](evidence/execution.json), build/pack/restore logs and verifier test output

From the repository root:

```powershell
python docs/foundation-package-qualification/tools/verify.py
python -m unittest discover -s docs/foundation-package-qualification/tools -p test_receipts.py -v
python docs/foundation-package-qualification/tools/replay.py --workspace .guard-tmp/package-replay-new
```

To build a new local artifact cohort from the same pinned inputs, use a new empty workspace:

```powershell
python docs/foundation-package-qualification/tools/build.py --workspace .guard-tmp/package-build-new
```

New SDK/path/build timestamps may change archive bytes; do not overwrite the sealed cohort or claim independent reproducibility from matching version text. The retained package bytes/hashes, exact source and recipe pins identify this cohort. NuGet auditing was disabled for the isolated local-only restore; no vulnerability or signature attestation is supplied.

## Remaining gates

The current production source is unchanged. Earlier full solution/QT3/qt4 runs qualify their original artifacts and source; they were not rerun against these new package bytes in this step. Actual Schema/XmlDb/Braid source/package/binary and lifetime evidence remains with the delegated owners. Full XSD 1.1, neutral C45/session contracts and shared freeze remain Pending.

Next core task: run package-backed full solution and frozen gate qualification for the exact candidate package cohort, recording any remaining product assembly scope separately. Keep publication and permanent API acceptance as explicit later dispositions.
