# Optional decimal qualification evidence

The complete final core run passes 24,214 cases in ten projects; the complete Schema run passes 260 cases, including 56 new optional cases. Test scopes overlap with provider/example checks and must not be summed. The original 45-case regression cohort's intentional pre-implementation result is 9 passes/36 failures; eleven later boundary cases complete the 56-case optional suite. `cohort-comparison.json` matches the original test names to their final passing outcomes.

`initial-fixture-compile.txt` retains the initial test fixture's incorrect use of LINQ First on the unboxed sequence API. The qualified before receipt follows that correction. `initial-sandbox-nuget-audit.txt` retains a restore failure caused by sandbox access to NuGet vulnerability data; restore/tests succeeded with the required network access. Neither setup failure is hidden or counted as product qualification. The intermediate 249-case pass precedes eleven added boundary controls.

Core Release build succeeds with five warnings in unchanged Standard test/XSLT conformance files and zero errors; `build-warnings.json` records their paths and identities. Raw receipts retain their original encodings/line endings. Native gate dumps exactly match the sealed integration mappings, including names and reasons; no baseline, admission or fixture is changed.

`source-identities.json` and source copies identify changed implementation, test and project files. `example.txt` records the actual optional-gap whole-document XPath/XSLT example. Documentation receipts cover HTML/local destinations and the strict owning Schema checker, without screenshot, PDF or hosted-CI claims. All previous sealed scope/assessment/integration/bridge dossiers are verified unchanged.

These results qualify the finite unpublished Schema 0.6/core 1.34.1 candidate; they do not certify full XSD 1.1, general API/session interchange or product adoption. Candidate changes remain uncommitted.
