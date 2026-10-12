<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak upstream XSLT 4.0 test contribution workflow">
  <br><br><h1>Contributing to the XSLT 4.0 Test Suite</h1>
  <p>Upstream-native tests, Bosak adapter and reproducible evidence</p>
</div>

## Local arrangement

The upstream suite is an independent Git checkout at `.upstream/xslt40-test`, ignored by Bosak. Its baseline is `6205faf9f32ace6338ac245c41f8eb9a1447926f` from [qt4cg/xslt40-test](https://github.com/qt4cg/xslt40-test). The remote is named `upstream`; local contribution branch: `bosak/schema-awareness-review`. No upstream test modifications, fork, upstream PR or external message have been made in this setup step.

Bosak owns the adapter, launch/validation scripts, local adapter controls and review notes. The upstream checkout owns its unchanged `catalog.xml`, `admin/catalog-schema.xsd`, `tests/` structure and future contribution files. It is not a Bosak submodule, vendored copy or parallel Bosak-format test collection. [Adapter configuration](../tests/xslt40-adapter.json) records the reviewed source pin and paths.

| Location | Responsibility |
|---|---|
| .upstream/xslt40-test/catalog.xml | Upstream list of test sets. |
| .upstream/xslt40-test/tests/.../_name-test-set.xml | Upstream environments, dependencies, scenarios and assertions. |
| .upstream/xslt40-test/tests/.../*.xsl / *.xml | Upstream stylesheets, inputs and expected outputs. |
| tests/Bosak.Xslt.Conformance | Bosak catalog adapter; existing 3.0 behavior retained by default. |
| tests/Bosak.Xslt.Conformance/Fixtures/Xslt40Adapter | Local adapter controls, not contribution tests. |
| .artifacts/xslt40 | Ignored logs and JSON run receipts. |

Recreate a checkout on another machine:

```powershell
git clone https://github.com/qt4cg/xslt40-test.git .upstream/xslt40-test
git -C .upstream/xslt40-test remote rename origin upstream
git -C .upstream/xslt40-test switch -c bosak/schema-awareness-review 6205faf9f32ace6338ac245c41f8eb9a1447926f
```

Fetch/review upstream changes explicitly before advancing the baseline. Record the new pin and rerun relevant tests. Do not automatically update a contribution branch or rewrite upstream expected results to match Bosak.

## Running with Bosak

From the Bosak root:

```powershell
pwsh -File ./run-xslt40-tests.ps1 -TestSet select
pwsh -File ./run-xslt40-tests.ps1 -TestSet xml-to-json -TestCase xml-to-json-D202-40 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet si-fork -SchemaAware -NoBuild
```

Test-set and case filters are the existing adapter's substring filters. Thus xml-to-json also selects sf-xml-to-json. Unselected cases can appear as skipped in summaries; inspect the log before treating skip counts as unsupported coverage. The wrapper requires a nonempty test-set filter to bound runs.

The wrapper selects `--xslt40`, which recognizes XSLT40/XSLT40+ catalog dependencies, uses exact declared error-code matching without Bosak's legacy aliases, and returns failure for failed tests or zero executed tests. It retains actual stylesheet versions; the flag does not rewrite 3.0 stylesheets into 4.0. Engine compilation derives its XPath profile from the stylesheet's effective version. Explicit XSLT30-only catalog dependencies are skipped in the 4.0 profile; XSLT30+ remains eligible.

This is a partial implementation profile, not an assertion that Bosak supports every XSLT 4.0 feature or every catalog assertion. Existing environment/assertion machinery and unsupported-feature policies remain inherited from the adapter. Review the specific environment, dependencies, assertion implementation and error expectations before claiming upstream evidence. Independent-processor confirmation is desirable where available.

Each wrapper run writes a timestamped log and JSON receipt with suite revision/status, Bosak revision/status, selected filters, schema-aware mode, exit code and counts. A dirty tree is recorded, not represented as a clean revision. Preserve the exact patch or contribution commit alongside dirty-run receipts; file names alone do not reconstruct edits. Receipts are local working artifacts and contain absolute paths.

## Catalog validation and adapter controls

Validate touched catalogs against the upstream schema, as its [contribution instructions](https://github.com/qt4cg/xslt40-test/blob/6205faf9f32ace6338ac245c41f8eb9a1447926f/readme.md) request:

```powershell
pwsh -File ./validate-xslt40-catalog.ps1
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/attr/select/_select-test-set.xml
pwsh -File ./test-xslt40-adapter.ps1 -NoBuild
```

Files are relative to the upstream suite root, or absolute. In an interactive PowerShell invocation, Files also accepts an array. The validator validates XML catalog structure; it does not certify stylesheet semantics or assertion correctness.

Four controls verify a version-4.0 keyword stylesheet and exact unknown-function error, the unchanged legacy profile's 4.0 dependency skips, failure for an empty selection, and failure for a deliberately mismatched error code that the legacy alias table could accept. The negative catalog is generated under ignored artifacts; it is not submitted upstream.

## Initial baseline — 2026-10-12

| Probe | Result | Meaning |
|---|---|---|
| Upstream select set | 157 passed / 0 failed / 1 skipped; wrapper exit 0 | Existing suite can run through the Bosak adapter. Does not prove new 4.0 feature coverage. |
| Upstream xml-to-json-D202-40 | 0 passed / 1 failed / 161 skipped; wrapper exit 1 | XSLT40+ case actually executes. Bosak returns -0 where the assertion expects -0e0. Other skip counts include filtered cases. |
| Nonexistent set selection | Zero executed; wrapper exit 1 | An empty run is not reported as successful coverage. |
| Local adapter controls | Four controls passed | Version gating, keyword profile, exact errors and negative exit statuses verified. |
| Upstream catalog and select test-set schema validation | Passed | Native catalog structure verified. |

The JSON case is existing upstream evidence, not a new contribution or permission to change its expected result. Triage whether a difference is in Bosak, its adapter, the test or draft alignment, using the exact source revision. No full-suite sweep or conformance percentage is claimed.

## Initial schema-awareness track (now secondary)

Review schema-awareness dependencies/environments in the current 4.0 suite, starting with the source-document and si-fork/si-map scenarios behind [issue #90](https://github.com/w3c/xslt30-test/issues/90). Michael confirmed that an environment schema element implies schema-awareness and [recommended focusing contributions on 4.0](https://github.com/w3c/xslt30-test/issues/90#issuecomment-5993508731). Check what has already changed before proposing metadata or new tests.

For a contribution:

1. Find the closest native test set and avoid duplicate coverage.
2. Record the exact draft clause/pin and required outcome; Bosak's current failures are evidence, not normative expectations.
3. Add stylesheets, input/output files and test cases inside the upstream tests directory, using its naming, created-by metadata, environments, dependencies and assertions. Add catalog entries only for a genuinely new group.
4. Validate touched test-set XML and run the selected cases through Bosak in basic/schema-aware modes as applicable. Keep refusals, unsupported features and failures visible.
5. Prepare a small upstream commit and contribution description. The upstream guidance requires consensus before changing established expected results believed incorrect; metadata corrections have a lighter process.

Keep the upstream PR focused on portable tests. A short implementation note may link [Bosak's README](../README.md) and the relevant [contract review](xpath40-catalog/CHANGED-31-CONTRACT-INVENTORY.md), accurately distinguishing supported behavior and recorded gaps. A fork/push/PR or maintainer message is a later delivery step; this setup sends none.

The REQ-126 equality/collation catalog queue remains recorded, but upstream contribution setup/review is now the user's priority. REQ-126 stays In Progress; version remains 1.1.3.


## Setup validation

Release solution build passed (12 existing warnings, zero errors); **6,806 tests / 0 failures / 0 skips** across ten assemblies. Four adapter controls passed; upstream select 157/0/1, genuine XSLT40+ JSON case 0/1/161 with exit 1, zero selection exit 1, upstream catalog/select and local control schema validation passed. Setup guide and six canonical/implementation documents rendered; local links/code fences and JSON run receipt counts/status verified. Manual engine gates were not rerun for adapter-only changes; engine/API unchanged.


## Current contribution priority

The [contribution priority queue](XSLT40_CONTRIBUTION_QUEUE.md) now starts with new XSLT 4.0 elements: array/member, select, switch, named item types and named records. Likely use is estimated, not measured; structural stylesheet counts are not semantic coverage. Next: N01 native array-case audit and a small nonduplicate order-export/member batch. Schema-awareness and inherited JSON findings remain secondary triage. Version 1.1.3; REQ-126 In Progress; no engine change or upstream submission. The current clean upstream branch is bosak/array-member-review; the earlier schema-awareness-review branch remains available. Earlier setup evidence stays historical.


**First upstream test batch — 2026-10-12:** N01 now has a [six-case native array/member batch](../contributions/xslt40/array-member-boundaries/README.md), committed locally on bosak/array-member-review at e72f1b0e8511f78478e66c112201317cfe7b7ad1. Five exact new cases pass; array-032 exposes missing XTTE4060 enforcement and returns A|B. Normative expectations are retained. A portable mail patch, source/expected-outcome mapping, clean-commit receipts and an upstream PR draft preserve the work; no upstream submission or independent processor confirmation yet. Version 1.1.3, REQ-126 In Progress, no Bosak engine/API/adapter change. Next: independent assertion review and separate error triage before submission.


**Array/member correction — 2026-10-12:** The [array/member assertion review](../contributions/xslt40/array-member-boundaries/ASSERTION_REVIEW.md) confirms the six native expectations. Bosak now preserves existing core JNode wrappers in both XSLT evaluation paths, unwraps member values at array construction, and raises XTTE4060 for multi-item per-focus results containing a JNode. All six unchanged native cases pass; the original five-pass/one-failure receipts remain historical. Five additional regression checks cover direct, empty, conditional and variable-carried errors plus valid grouped/nested wrappers. Source candidate 1.1.4 is unpublished; 1.1.3 remains the published version. N01 and REQ-126 stay In Progress; independent processor confirmation and upstream submission remain outstanding.


**Upstream submission — 2026-10-12:** The first six-case native array/member batch is [submitted as xslt40-test PR #22](https://github.com/qt4cg/xslt40-test/pull/22). All six exact cases pass with Bosak; specification review is recorded. No second processor was run: independent cross-check was requested in the PR. [Submission details](../contributions/xslt40/array-member-boundaries/SUBMISSION.md) explain the Saxon PE/EE and partial-for-each limitation and the decision to seek upstream review without making local independent execution a prerequisite. Review/acceptance remain pending; N01 and REQ-126 stay In Progress. Bosak fix PR #202 merged after CI 38295568498 passed (6811 unit tests); source 1.1.4 remains unpublished, public release 1.1.3 unchanged.
