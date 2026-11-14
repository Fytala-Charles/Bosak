# Matching function-conversion allocation investigation - 14 November 2026

This local observation uses the exact qualified 1.81.10 nine-DLL set from source6f6829955f53b2f084bf66836b0abbbf1139b4d2; engine/runner source matches merged main9ffde22e0910990c0380c1e9d0a50b7e2c446b97. It contains no engine change. Development 1.81.10 remains unpublished. PR491 postmerge qualification independently reconciles10,221/0/4,380, all14,601 identities/exclusions in800.890s; all six main checks passed. Original1200-second/8-GiB limits, corpus/runtime/transcript hashes and source tree are intact.

## Bounded observation

Review was recorded before the diagnostic edit/execution. A new private diagnostic references exact qualified DLLs without rebuilding the engine. It repeats the earlier helper/conversion checks and adds isolated existing-call observations. Cached delegates/inputs are prepared before warmed loops; each row measures calling-thread allocated bytes across100,000 calls after10,000 warm-up calls. Constructor IL and captured fields are inspected outside measured loops. Reflection inventory does not execute the recorded constructors. All diagnostic failures are caught; expected lazy failures remain caught inside the diagnostic. This is not the retained foundation LegacyCaller and does not alter its runner.

| Existing route | Bytes/call |
|---|---:|
| Matching integer conversion | 112 |
| ValidateFunctionConversionTarget, xs:integer | 40 |
| ValueMatchesType, matching integer | 72 |
| NormalizeTypeName, xs:integer | 0 |
| Uncached NormalizeTypeNameForAtomicMatch | 40 |
| Choice check, xs:integer | 0 |
| Nominal-record lookup, xs:integer | 0 |
| Matching string/boolean/decimal/double conversion | 112 each |
| Matching string/boolean/decimal/double type check | 72 each |

Null context still gives72 bytes for matching integer type checks and112 for matching integer conversion. The five measured primitive matches pass, and all original protective checks succeed; RESULT.json retains28 check/observation entries including constructor inventory. The same probe confirms the merged integer general comparison remains0 bytes/call. Single launch under60s/512MiB completed successfully on .NET10.0.11 with roll-forward disabled. All nine engine hashes match before/after; build has zero warnings/errors. Source and binary hashes, exact command and elapsed time are in RUN.json. Raw binaries stay in the ignored local packet; no raw trace was captured, no suite rerun or machine-wide WER change.

## Interpretation and next implementation proposal

The exact compiled ValueMatchesType method starts at IL offset0 with a new compiler-generated closure holding the XdmValue and EvaluationContext. Its capturing lambdas are the element/attribute alternative-name Any calls. The measured scalar type-check route allocates72 bytes despite never entering those node-test branches. The compiler-generated label in RESULT.json is tied only to these exact DLLs and must not become a source compatibility contract.

The existing validator normalizes an already lowercase xs:integer with no allocation, then obtains the bare name through name[3..]; the isolated validator allocates40 bytes. The current scalar atomic-name cache already avoids repeated uncached normalization. The112-byte conversion observation agrees with40+72 from its validation and matching calls; these are bounded calling-thread observations, not object-count/per-type-byte sampling or proof about other targets/workloads.

Recommend removing both capturing element/attribute alternative-name lambdas through ordered loops using the existing matching calls, preserving short-circuit behavior, namespace/schema context and error order. First record immutable normative sections and add explicit alternative-name and context/atomization regressions plus a warmed allocation check. Re-run full solution, frozen QT3, gatedqt4 and pinned standalone for that engine change. Aim to remove the72-byte incidental matcher closure; any actual reduction must be measured after implementation. Keep the40-byte validator cost as a separate investigation; do not globally cache context-dependent schema validity or bypass node/choice/conversion rules. No new atomic type shortcut is proposed here.

## Evidence

Probe.cs/project, pre-edit REVIEW.json, RESULT.json, RUN.json, original build/stdout/stderr and executed controller are copied from `.guard-tmp/matching-conversion-20261114`; the project/controller retain their executed local paths and are not advertised as portable launch tools. MANIFEST.json binds these local files. Prior main artifact/receipts remain in `.guard-tmp/scalar-materialization-20261114/MAIN-491.json` and ZIP. No diagnostic publication, engine edit, commit, push or release is included in this investigation.
