<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.8.0 release publication">
  <br><br><h1>Bosak 1.8.0</h1>
  <p>Source-preserving value-of insertion — published 16 October 2026</p>
</div>

**Published and verified.** Immutable `v1.8.0` points to approved revision `20a5d9be757599848d7ceaa8885e59bea8dbb2ec`. The [release workflow](https://github.com/Fytala-Charles/Bosak/actions/runs/38846326481) passes 8,624 solution tests and publishes ten .NET 10 libraries. Fresh nuget.org-only consumers pass 141 public insertion tests and 153 broad regression checks; all 19 unchanged Braid probe outcomes match. All ten package assembly revisions and public package/cache/output payloads verify. [Publication evidence](RELEASE-1.8.0-PUBLICATION.json).

1.8.0 adds bounded source-preserving `xsl:value-of` insertion to `Bosak.Xslt.Authoring`. The minor version reflects the new public API. Existing expression-edit APIs retain their behavior.

- Discover snapshot-owned targets with `GetValueOfInsertionTarget` and submit `AuthoringValueOfInsertionProposal` through `ProposeValueOfInsertion`.
- Review an isolated candidate with exact original/inserted ranges, the actual inserted instruction/select slot, complete surviving-node correspondence and defensive source export.
- Preserve every original byte outside the zero-length insertion point, including BOM, encoding, declarations, comments, PIs, trivia and entity spelling.
- Use explicit capability, encoding and eight-MiB limits; unsupported parents, static contexts, resources, names and cancellations produce classified refusals.

The first slice supports explicit template/LRE constructors in a single principal XSLT 3.0/4.0 module, UTF-8 or BOM UTF-16 LE/BE. Static preprocessing, dependencies, mixed content, self-closing parents, unproven function signatures and deferred AST forms refuse. See the [API guide and exact coverage](REQ-128-IMPLEMENTATION.md).

REQ-128 engine delivery is implemented; Braid runtime adoption, Apply/history and native acceptance remain separate. Braid's production pin is unchanged. Published VS Code 0.2.3 continues to bundle engine 1.7.1.

Merged-source CI passes 8,624 solution tests, 84 language-server tests and highlighting checks. Frozen gates remain QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. The typed-node-test exclusion correction and focused consumer re-review are preserved in the [F1 review record](REQ-128-CONSUMER-REVIEW-F1.md); earlier local package receipts remain historical evidence.
