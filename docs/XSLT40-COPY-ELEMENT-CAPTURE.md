# Copied-element content inside raw and typed results

Unpublished source **1.14.11** isolates the element branch of `xsl:copy` from the caller's raw/typed template capture. Previously, it suspended only the sequence accumulator. Inherited atomic/document preservation and the raw-item side channel caused missing spaces, exposed synthetic document wrappers, and diverted illegal maps/functions into the caller's result. Element content also left atomic adjacency active after the node, and errors could bypass container/accumulator restoration.

The element branch now saves and suspends all three capture settings and the accumulator before attribute sets/content. A single `try/finally` covers attribute sets, both content routes, normalization and validation; it restores the caller container and capture settings and ends atomic adjacency. Source-name/namespace copying and shallow-copy behavior retain their existing paths.

```xml
<xsl:copy select="$seed">
  <xsl:sequence select="1,2"/>
</xsl:copy>
<!-- Copied element content is "1 2", including rawResult:true. -->
```

## Specification and integration base

PR335 passed exact-head CI run38915461447 at `fd512d4ef2df0817a1cea2c1d17abb86644111ad` and merged at `50fbfedb79a644c914f91fc8014a301c21d92621`, the base of this isolated branch. Upstream was refreshed: xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`.

The [shallow-copy rules](https://qt4cg.org/specifications/xslt-40/Overview.html#shallow-copy) construct element content from the contained sequence constructor; the [complex-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-complex-content) require spaces between consecutive atomics, replacement of document nodes by children, and rejection of illegal function items. Raw delivery and template return types do not change these parent-node construction rules.

## Regression controls

The first fixture did not compile: `Assert.Empty` does not accept the XdmSequence struct returned by Attributes(). The original source/log are retained. The fixture was corrected to explicitly enumerate attributes into a list, preserving the empty-attribute expectation. This is a fixture compile correction, not an engine failure or a weakened assertion.

The compilable22-case baseline passed4 and failed18. Expanded28-case baseline passed5 and failed23. Both sources/logs are preserved. Failures independently show missing spaces (`12`), document wrapper `__xdm_doc__`, map/function diversion, incorrect caught-error output, and a following atomic text node with an extra leading space. Expanded controls additionally cover context-item copies without select, conditional illegal content and an enclosing typed-variable collector.

The final focused selection passes **112/0/0**:28 new copied-element cases,26 literal-element,21 computed-element,21 document-construction and16 raw initial-template controls. The final test source matches the expanded pre-fix fixture byte-for-byte.

Coverage includes select/sequence content, normal/raw delivery, typed/untyped entry templates, context-item and selected-source copying, conditional content/fallbacks and errors, caught body/attribute-set errors, nested documents, surrounding raw maps/functions, exact child-node boundaries, namespaces/attribute sets, explicit attribute content, empty elements, source attribute/child omission, enclosing variable capture and XSLT3.0 behavior.

The document branch and separate `CopyNodeForFunctionBody` helper are not changed. Broader copy-select focus/template-rule restoration is a separate review; this fix restores copied-element construction state.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact PASS identities are unchanged, with no gains/losses. PR335 receipts and logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. Source/log/receipt SHA-256 hashes are recorded in the companion JSON. No upstream expectation, profile selector, error alias, skip or gate membership changes are included.

## Verification and next review

All local jobs completed: Release build zero errors/22 warnings; solution **9,159/0/0** (XSLT6,225), focused combined **112/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered with markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged. Source1.14.11 is unpublished.

Next: exact-head CI/review before integration, then audit xsl:copy document construction under raw/typed capture. The separate function-body copying path, nonraw text-only API delivery kind, processor-capability architecture, legacy patterns/obsolete functions and complex-content JNode semantics remain separate.
