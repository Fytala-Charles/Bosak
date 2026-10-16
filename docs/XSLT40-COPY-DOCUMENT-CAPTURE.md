# Copied-document construction inside raw and typed results

Unpublished source **1.14.12** routes the document branch of `xsl:copy` through the existing document-content constructor. The old branch evaluated content in two independent collectors, inheriting raw/typed preservation flags. Atomic text could lose spaces, illegal maps/functions could escape, nested documents could retain wrappers, and raw output could lose document identity. Its actual-XDocument variable path could not represent non-whitespace document text or multiple element children.

The branch now uses `EvaluateSequenceConstructor(..., wrapInDocumentNode:true)`, which already isolates capture state and represents mixed document content. It retains the source document base URI, copies unparsed-entity annotations, applies construction validation, and delivers the completed document to the surrounding sequence accumulator or existing result-copy path. Public APIs and the separate function-body copy helper are unchanged.

```xml
<xsl:variable name="v" as="document-node()">
  <xsl:copy select="$source-document">
    <xsl:sequence select="1,2"/>
  </xsl:copy>
</xsl:variable>
<!-- v is a document whose text is "1 2". -->
```

## Specification and integration base

PR336 passed exact-head CI run38920023709 at `cb7af1f4a5787484b93b8efddc4ad51ad6f6756f` and merged at `30382e25c07dcefc09a29a506103f947dfc38c31`, the final base. Baseline work began on the exact precursor while CI ran. The entire precursor/merge trees were verified identical (`49b748b6c5b2a9f5e64d80d12ca32a3bbf88bfe7`) before fast-forward alignment; no source changes occurred during alignment.

Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [shallow-copy rules](https://qt4cg.org/specifications/xslt-40/Overview.html#shallow-copy) and [complex-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-complex-content) govern copied-document construction, independently of template return type and raw delivery.

## Regression controls and preserved errors

Original21-case baseline: **5 passed / 16 failed**. First implementation: **133/0** combined controls. Expanded31-case baseline on the original engine: **9 passed / 22 failed**. Expanded first implementation: **29 passed / 2 failed**, exposing lost XTDE0420 namespace errors during pipeline reuse. Original fixtures/logs and that implementation snapshot are preserved; expectations were not weakened.

A private `rejectDocumentNamespaceNodes` option, enabled only for copied documents, retains the copy path's namespace prohibition on both item-collection and direct construction routes. Existing callers retain their previous behavior. Attribute errors remain enforced by the shared document constructor. This is not a general change to other constructors' namespace handling.

Final focused selection: **145/0/0** (33 new copied-document,28 copied-element,26 literal-element,21 computed-element,21 explicit/implicit document and16 raw initial-template controls). The final two additional controls verify source document base URI inside a typed variable and an exact document-node() template return type. The fixture's typed-entry documentation was clarified; assertions were not changed.

Coverage includes raw/normal and typed/untyped delivery, select/sequence content, arrays, atomic/text/node boundaries, surrounding raw values/functions, caught errors, nested documents, conditional content, empty/text-only/multiple-root typed variables, document identity, invalid map/function content, and direct/conditional attribute/namespace errors. XSLT3.0 behavior is covered too.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact passing identities are unchanged. PR336 receipts/logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. The companion JSON retains source/log/receipt SHA-256 hashes. No upstream expectations, aliases, profile selection, skips or gate membership changed.

## Verification and next review

All local jobs completed: Release build zero errors/2 warnings; solution **9,192/0/0** (XSLT6,258), focused combined **145/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered using markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Source1.14.12 is unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next: exact-head CI/review, then audit the separate CopyNodeForFunctionBody helper under raw/typed capture. Broader copy-select state restoration, other constructors' namespace handling, nonraw text-only API delivery kind, processor capability, legacy patterns/obsolete functions and complex-content JNode semantics remain separate.
