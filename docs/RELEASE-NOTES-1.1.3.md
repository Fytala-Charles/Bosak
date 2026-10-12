<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.1.3 compact inspection patch notes">
  <br><br><h1>Bosak 1.1.3</h1>
  <p>Correct source extents for compact nested siblings</p>
</div>

## Release scope

**Unpublished; source version 1.1.3.** REQ-127, requested by Bosak.Braid, corrects source-preserving authoring inspection for compact nested sibling elements. The raw scanner previously skipped the first character after a start tag, potentially missing an immediate child's opening markup. That produced premature element extents and alignment refusal for valid compact documents. Inspection success on a single group did not guarantee correct extents.

The scanner now uses the already-exclusive start-tag end directly. Original source bytes, BOM/encoding, base URI, entity spelling and existing authoring APIs/capabilities are preserved. No validation/execution profile or edit authority is added. [REQ-127](REQ-127-compact-source-inspection.md) records reproduction, criterion-level coverage and delivery/adoption procedure.

206 new regression checks cover 1/2/80 sibling groups, no/space/LF/CRLF separators and nested layout, supported UTF-8/BOM and UTF-16 LE/BE, exact element/slot byte ranges and line/column coordinates, mixed content, repeated/empty elements, namespace aliases, malformed-source refusal and isolated middle-sibling Expression edits. Release solution build passed (12 warnings, zero errors); **6,019 tests / 0 failures / 0 skips** across ten assemblies. QT3 stays exactly **31,142/0/679**. qt4 gated-only run: **8,278/0/1,894**, zero gated failures; no gate promotion. All 206 REQ-127 regressions pass.

Consumers should re-inspect original compact documents after adopting the delivered version; descriptor identities remain snapshot-scoped. Braid's source pin and acceptance tests are updated in its own adoption step. No release tag, NuGet publication or Braid adoption is claimed here. REQ-126 remains In Progress; no catalog closure or gate promotion is included.
