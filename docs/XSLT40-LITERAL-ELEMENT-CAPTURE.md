# Literal-result-element content inside raw and typed results

Unpublished source **1.14.10** isolates literal-result-element content from raw/typed template capture. The existing path suspended its placeholder accumulator, but inherited document preservation and the typed raw-item side channel. Consequently, a document child could expose `__xdm_doc__`, and maps/functions could escape from element content into the surrounding result. A following atomic text node could also inherit adjacency from inside a nested element.

Literal content now saves and suspends atomic/document preservation and typed raw-item collection, restoring all three settings with the accumulator/container in `finally`. Finishing an element ends atomic adjacency. Literal-depth accounting is owned by a wrapper with a single `finally`, covering attribute/AVT errors, extension errors, extension fallbacks and EXSLT result-document exits. Default-mode name expansion occurs before content capture state changes.

```xml
<xsl:template name="start" as="item()*">
  <n><xsl:sequence select="$document"/></n>
</xsl:template>
<!-- The document contributes its children, including with rawResult:true. -->
```

## Specification and integration base

PR334 passed exact-head CI run38912410632 at `2087bea9f562f90a5a4f18a0e6f8758815f7b4d9` and merged at `4ad9ad5fbf3ea4059933edb58b7d6459aebb7a51`, the base of this isolated branch. Upstream was refreshed: xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`.

The [complex-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-complex-content) require document-node replacement by children and rejection of invalid map/function content. Raw delivery and template return types do not change parent-node construction rules. Atomic spacing was already correct inside literal elements; the new controls retain it.

## Regression controls

The original22-case fixture passed17 and failed5. Expanded26-case baseline passed21 and failed the same5. Both original fixtures/logs are preserved; no expectation or fixture corrections were made. Failures show a synthetic document wrapper, diverted map/function items, incorrect caught-error output, and an extra leading space after a nested element.

The final focused selection passes **84/0/0**:26 new literal-element cases,21 computed-element,21 document-construction and16 raw initial-template controls. It covers normal/raw and typed/untyped delivery, select/sequence atomic content, conditional on-empty/on-non-empty, illegal maps/functions, caught content/attribute-set/AVT/extension errors, nested documents, surrounding raw function items, exact child boundaries, namespaces/attributes, empty elements, extension fallback and XSLT3.0.

Early AVT/extension-error and fallback output controls already passed before the fix. The depth change repairs a statically identified exception-safety gap; these controls are not claimed as additional reproduced output failures.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact PASS identities are unchanged, with no gains/losses. PR334 receipts and logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. Source/log/receipt SHA-256 hashes are recorded in the companion JSON. No upstream expectation, profile selector, error alias, skip or gate membership changes are included.

## Verification and next review

All local jobs completed: Release build zero errors/22 warnings; solution **9,131/0/0** (XSLT6,197), focused combined **84/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered with markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged. Source1.14.10 is unpublished.

Next: exact-head CI/review before integration, then audit xsl:copy construction under raw/typed capture. Nonraw text-only API delivery kind, processor-capability architecture, legacy patterns/obsolete functions and complex-content JNode semantics remain separate.
