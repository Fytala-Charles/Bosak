# PV-Q09 — preview package regression qualification

7 November 2026. This qualification targets the exact nine
**1.81.1-preview.c0b926de** package binaries prepared from checkpoint
`c0b926de34824ceb2010d5f814a4450b28b31dcc`. Production outputs and sealed prior
receipts remain unchanged. No package was uploaded or release tag created.

The [plan](PLAN.json) pins 16 isolated test/harness layouts. Their compiled test,
fixture, configuration and non-packaged product bytes come from the sealed
production qualification. Only the nine package DLLs and applicable XML docs are
overlaid. Stale engine PDBs are omitted. The original test projects are not rebuilt
against new APIs: this is an unchanged-binary regression lane, alongside the
fresh-cache package-only consumer already qualified in
[PV-Q08](../foundation-preview-packages/README.md).

| Lane | Final result |
|---|---|
| Ten solution test projects | 26,342 passed, zero failed/skipped |
| Separate language-server tests | 84 passed |
| Native Schema compatibility | 451 passed |
| Native adapter prototype | 59 passed |
| Independent public provider consumer | 10 passed |
| Frozen QT3 | 31,397 / 0 / 424 |
| Gated qt4 | 8,941 / 0 / 1,918; 11 experimental passes |
| Compiled API inventory | 318 public types / 6,465 rows; no observed metadata delta |

[Results](evidence/RESULTS.json) retain exact passing case names, test-result
hashes, execution identities and gate totals. QT3 and qt4 skip files match the
production baseline byte for byte, retaining names and reasons. The API inventory
compares public/protected declarations, defaults, enum constants, attributes and
value-type field rows, assembly identities, references and friend lists.

Language server and database are not part of the nine-package preview: their
unchanged product DLLs remain separately pinned in the corresponding lanes and
the eleven-product API inventory. Native Schema and the experimental provider
are also unchanged binaries. These compatibility results do not establish
production provider adoption, general supported-type coverage or owner acceptance.

[Actual binding reports](evidence/BINDING-SUMMARY.json) verify the package hashes
and preview informational identities loaded in every lane's Default,
noncollectible context. Only actually observed assemblies are claimed loaded in
each lane. All nine are verified through the earlier package-only consumer.
The unchanged retained foundation caller's 20 passing controls are reused from
PV-Q08 over identical package hashes, not rerun or added to solution totals.

Two setup failures are preserved separately:

- The first Runtime run passed 1,848 cases and failed 51 with
  `DirectoryNotFoundException`. Flattening the isolated test path changed where
  `../../../../qt3tests` resolved. Restoring the conventional project/bin/Release/
  net10.0 depth and a junction to the same pinned corpus fixed fixture access.
  All 1,899 Runtime cases then passed with unchanged package/configuration bytes.
  The preparation script now creates the required fixture layout. No engine or
  test assertion was changed.
- The first metadata inventory lacked the unchanged language server's third-party
  dependencies. Adding its supporting DLLs to the isolated metadata layout
  allowed the inventory to complete; all compared rows match. The first failed
  receipt remains distinct from the successful retry.

The original and corrected layout plans, initial/final TRX results, and execution
receipts remain available. XSLT tests run from a temporary layout for Application
Control compatibility. Database tests use the required authorized localhost
listener. Corpus revisions/statuses remain pinned and clean.

[TRX archive](evidence/TRX-RESULTS.zip) and
[execution receipts](evidence/EXECUTION-RECEIPTS.zip) preserve the runs. Full binary
layouts are reconstructible from the original sealed TA-55 execution archive plus
the PV-Q08 package archive and this plan, avoiding duplicate binary bundles.
The original partial failures are not included in the final passing totals.

This completes package-overlay regression, conformance and API qualification
within the stated scope. It does not close owner workloads/budgets, production
Schema/XmlDb/Braid adoption, four-owner C45/API freeze, final release SemVer,
signing/provenance review or publication approval. In particular, earlier workload
measurements used different compiled engine hashes and cannot be relabeled as
measurements of these package bytes.

Next **PV-Q10**: run the bounded candidate workload against these exact package
hashes, retain the separate cohort/load receipts, and prepare the concrete owner
review over package manifests and still-unfilled workload/baseline/budget slots.
No unsupported route or owner requirement is waived by passing regressions.

Run `python docs/foundation-preview-qualification/tools/verify.py` for layout,
package binding, exact-case, receipt, gate/skip-map, API and local-link checks.
Launch scripts refuse receipt reuse; preserve original evidence for later runs.
No rendered-document approval is claimed.
