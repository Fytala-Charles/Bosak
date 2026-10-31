# Private transport promotion patch

31 October 2026. The reviewable production patch proposes **1.36.0**. It is not applied to the live product source, which remains **1.35.0**.

The [six-file patch](PRODUCTION.patch) includes the three qualified implementation files, the minor version bump and two durable regression-test files (22 new cases). The implementation bodies preserve the qualified candidate's non-comment lines exactly. Experiment prefixes are removed; source headers gain current change-history rows and layer notes, and the equality overrides have explicit XML summaries, parameters and return documentation. Copyright and SPDX notices remain intact.

The [paired-override disposition](DISPOSITION.md) recommends retaining Equals(object) and GetHashCode() together. It records the visible reflection change: 78 exported types remain; declared members rise 1,199 to 1,201, with DeclaringType now XdmValue for those two inherited signatures. Inherited signature lists/base slots remain unchanged, and XdmValue stays 48 bytes. This technical proposal does not accept C45, a public Schema provider SDK or the final API freeze.

## Fresh preflight

The patch passes git apply --check against the live tree and applies successfully in a separate ignored materialization. Both affected projects pass: Core 162 and Runtime 281, totaling **443 tests**, including all 22 new regressions. Both final project builds have zero warnings/errors. All seven foundation assemblies report assembly/file version 1.36.0.0; their public metadata matches the qualified transport candidate, including the two equality declarations. The unchanged retained binary caller passes 20/0 against the exact new seven-DLL cohort.

The tests cover signed zero/NaN bits, equivalent duration/QName/timezone representations, fresh equivalent no-ops, cached getter/cancellation behavior, membership leases and primary/cleanup failures, CLR equality/hash collections, actual VM identity, foreign-snapshot conversion and expired operations. Exact output DLL hashes and original TRX bytes are retained. No owner's checkout or live src/tests file is changed.

Earlier receipts are retained under evidence/initial-preflight and evidence/before-explicit-xml. They include the first successful 443-test run with an analyzer warning, the warning-free run before explicit override XML comments, a CRLF materialization check adjustment and a process-scoped script-policy retry. The final patch/preflight is identified separately by exact hashes. Git may materialize CRLF according to repository attributes; the proposal uses LF, and both normalized and exact materialized hashes are recorded.

Full 1.36.0 solution/QT3/qt4 gates are **NotRun**. The previously sealed complete 1.35.0 transport candidate passed 24,245 tests and both exact gates; those receipts remain attached to that earlier cohort. This preflight does not relabel them as results for the new version or the 22 new tests.

## Review and next step

Read [PATCH.json](PATCH.json) for the immutable base revision, six pre/postimage hashes and qualified-body digests. Review the [source patch](PRODUCTION.patch) and [technical disposition](DISPOSITION.md). Run `python -B docs/foundation-transport-promotion/tools/verify.py`; `--check-current` additionally verifies the live preimages and current preflight directory. The [preparation](tools/prepare.py) and [preflight](tools/preflight.py) recipes use fresh directories; the latter applies only under .guard-tmp. Retained callers use the checked runner with bounded/scoped error handling.

Next: apply this patch and run the complete final-version solution/conformance gates before considering promotion complete. Preserve the paired equality-manifest disclosure. Public provider/C45, consumer, performance-budget and API-freeze acceptance remain separate pending work.
