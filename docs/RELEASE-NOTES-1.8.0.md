<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.8.0 release candidate notes">
  <br><br><h1>Bosak 1.8.0</h1>
  <p>Unpublished candidate — 15 October 2026</p>
</div>

1.8.0 adds bounded source-preserving `xsl:value-of` insertion to `Bosak.Xslt.Authoring`. The minor version reflects the new public API. Existing expression-edit APIs retain their behavior.

- Discover snapshot-owned targets with `GetValueOfInsertionTarget` and submit `AuthoringValueOfInsertionProposal` through `ProposeValueOfInsertion`.
- Review an isolated candidate with exact original/inserted ranges, the actual inserted instruction/select slot, complete surviving-node correspondence and defensive source export.
- Preserve every original byte outside the zero-length insertion point, including BOM, encoding, declarations, comments, PIs, trivia and entity spelling.
- Use explicit capability, encoding and eight-MiB limits; unsupported parents, static contexts, resources, names and cancellations produce classified refusals.

The first slice supports explicit template/LRE constructors in a single principal XSLT 3.0/4.0 module, UTF-8 or BOM UTF-16 LE/BE. Static preprocessing, dependencies, mixed content, self-closing parents, unproven function signatures and deferred AST forms refuse. See the [API guide and exact coverage](REQ-128-IMPLEMENTATION.md).

Public packages remain 1.7.1; published VS Code 0.2.3 remains on engine 1.7.1. This candidate does not include Braid UI, Apply/history or production adoption. Final revision, package provenance, fresh-consumer checks and publication approval remain required before delivery.

Verified source `526924f0`: 8,564 solution tests (XSLT 5,678), 84 language-server tests and 81 fresh package-consumer cases pass. Frozen QT3 and qt4 gated results remain 31,142/0/679 and 8,278/0/1,894. Ten local packages carry exact 1.8.0/revision metadata; nine transitive consumer payloads byte-match their archives and fresh cache. [Package receipt](REQ-128-PACKAGE-CANDIDATE.json). Review/CI and final integrated-revision verification remain before separate publication approval.
