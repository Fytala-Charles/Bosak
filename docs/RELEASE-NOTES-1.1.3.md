<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.1.3 compact inspection patch notes">
  <br><br><h1>Bosak 1.1.3</h1>
  <p>Correct source extents for compact nested siblings</p>
</div>

## Release scope

Published [1.1.3](https://github.com/Fytala-Charles/Bosak/releases/tag/v1.1.3) on 2026-10-12 from `636a2f4d64c757a1f3c00bcf7ec1387416530ccc`. [Trusted Publishing](https://github.com/Fytala-Charles/Bosak/actions/runs/38271538007) passed. All ten public .NET 10 packages were downloaded and verified for version, internal dependencies, net10 DLLs, README/license and SHA-256 records. A fresh-cache consumer restored only from nuget.org and passed 29 compact-inspection/source-range/encoding, isolated sibling-edit and malformed-refusal checks. Braid verification was reported by the user; its exact adopted pin and consumer test totals were not supplied. REQ-127, requested by Bosak.Braid, corrects source-preserving authoring inspection for compact nested sibling elements. The raw scanner previously skipped the first character after a start tag, potentially missing an immediate child's opening markup. That produced premature element extents and alignment refusal for valid compact documents. Inspection success on a single group did not guarantee correct extents.

The scanner now uses the already-exclusive start-tag end directly. Original source bytes, BOM/encoding, base URI, entity spelling and existing authoring APIs/capabilities are preserved. No validation/execution profile or edit authority is added. [REQ-127](REQ-127-compact-source-inspection.md) records reproduction, criterion-level coverage and delivery/adoption procedure.

206 new regression checks cover 1/2/80 sibling groups, no/space/LF/CRLF separators and nested layout, supported UTF-8/BOM and UTF-16 LE/BE, exact element/slot byte ranges and line/column coordinates, mixed content, repeated/empty elements, namespace aliases, malformed-source refusal and isolated middle-sibling Expression edits. Release solution build passed (12 warnings, zero errors); **6,019 tests / 0 failures / 0 skips** across ten assemblies. QT3 stays exactly **31,142/0/679**. qt4 gated-only run: **8,278/0/1,894**, zero gated failures; no gate promotion. All 206 REQ-127 regressions pass.

Consumers should re-inspect original compact documents after adopting the delivered version; descriptor identities remain snapshot-scoped. Braid's source pin and acceptance tests are updated in its own adoption step. Release tag and NuGet publication are verified; Braid verification was reported by the user. Its exact package/pin adoption remains managed in the separate consumer session. REQ-126 remains In Progress; no catalog closure or gate promotion is included.


Implementation: [`5bab4799783569d538cec9b03d37a57454f0eebc`](https://github.com/Fytala-Charles/Bosak/commit/5bab4799783569d538cec9b03d37a57454f0eebc), [PR #192](https://github.com/Fytala-Charles/Bosak/pull/192). Implementation CI passed and PR #192 merged; publication is verified.


**1.1.3 release decision — 2026-10-12:** The user reports that Braid verified REQ-127 and explicitly authorized publication of 1.1.3. This records consumer verification without inventing its exact adopted pin or test totals. Existing fix CI passed; final documentation CI, merge/tag, Trusted Publishing and public-package verification are the remaining release steps. No additional features are included.
