# Explicit XSLT context candidate qualification

1 November 2026. The corrected isolated candidate passes all ten retained unit projects and both frozen gates. Production sources/outputs, public APIs, version, corpus revisions and owner pins remain unchanged. [Scope](SCOPE.md), [correction identity](evidence/CORRECTION.json) and [final results](evidence/RESULTS.json) distinguish the exact passing bytes from the earlier candidate.

| Qualification | Result |
|---|---:|
| Ten retained unit assemblies | 24,267 passed, zero failed/skipped |
| XSLT project within that total | 8,196 passed |
| Standard-library project within that total | 13,797 passed |
| Database project within that total | 47 passed with local loopback access |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Explicit/legacy bridge controls | 22 passed twice |
| Public XSLT declarations | 157 types / 2,961 metadata rows, unchanged |

Exact QT3/qt4 skip names and reasons match the frozen baseline. Corpus revisions/status are unchanged and clean. [Load-time reports](evidence/INPUTS.json) identify the test/gate assemblies, candidate replacement and unchanged dependencies. The corrected candidate is actually loaded by the XSLT suite and both gates. The standard-library unit project does not load XSLT and is not attributed actual XSLT execution. Eight named passing fn:transform/nested-transform controls are extracted from the full XSLT TRX into RESULTS.json; no redundant filtered rerun was needed.

## Correction discovered by qualification

The [previous isolated build](../foundation-xslt-explicit-context/README.md) replaced the original project with a simplified project that omitted its two existing InternalsVisibleTo declarations. Its bounded controls and declaration-only inventory did not detect this assembly-level metadata omission. Full tests exposed 200 XSLT access failures: 7,996 passed and 200 failed. Separately, the sandbox blocked local database listener fixtures, giving 2 passed and 45 failed. Those results, original bindings and TRX are preserved in [attempt-1](evidence/attempt-1/UNITS.json); they are not passing qualification for the original bytes.

The correction copies the original project metadata, substitutes qualified DLL references for project references, and retains the exact two patched C# file hashes. Both original friend declarations are restored; no new friend grant is introduced. The corrected candidate has a distinct SHA-256. It builds with zero warnings/errors, replays all 22 bridge controls twice, and retains identical public declaration rows. This qualifies the corrected candidate only; it does not retroactively qualify the original binary.

Nine final unit projects run from a fresh temporary tree, including the XSLT Application Control workaround. The database project runs from a separate workspace copy with authorized loopback access and serial collections. An attempted elevated launch into the sandbox's temporary directory failed before executing tests; the dedicated database layout resolves that environment visibility issue. Actual test DLL bytes match the 1.36.0 retained qualification, and all other engine dependencies retain qualified hashes. This is a fresh execution of retained test assemblies, not a fresh full-solution source build.

## Evidence and remaining boundary

[Final unit counters](evidence/UNITS.json), [unit TRX archive](evidence/unit-trx.zip), [gate exit receipts](evidence/GATES.json), [QT3 log](evidence/qt3.txt), [qt4 log](evidence/qt4.txt), [explicit controls/API comparison](evidence/EXPLICIT-AND-API.json) and [source/binary archives](evidence/ARTIFACTS.json) retain the execution evidence. Archives preserve both original/corrected candidate DLLs, corrected source inputs, license and NOTICE. No portable bit-identical rebuild is claimed. Unprefixed binding directories belong to the initial failed run; corrected-bindings directories are the final cohort used by the verifier.

The candidate remains isolated. Its document-root initial-match profile and explicit refusals remain unchanged. Broader initial selection, atomic global input, independent named-template focus overrides, streaming, caller-context restoration, authenticated provider binding and owner resource lifetime remain separate work. No public facade or API freeze is accepted here. Consumer acceptance and C45 remain pending.

Next prepare the production promotion patch for the internal bridge, preserving the canonical project metadata and adding focused regression tests. Rebuild and qualify the actual production source cohort before considering promotion complete; these retained-binary receipts cannot replace that step.

The [tools](tools/run.py) retain phase logic and separate original/corrected cohorts. Use a fresh evidence destination for replay; do not overwrite this sealed packet. Read-only verification is `python -B docs/foundation-xslt-context-qualification/tools/verify.py`. No deliberate unhandled crash or machine-wide error-reporting setting change occurred.
