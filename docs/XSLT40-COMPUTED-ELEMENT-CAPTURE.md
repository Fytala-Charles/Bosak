# Computed-element content inside raw and typed results

Unpublished source **1.14.9** isolates `xsl:element` content from the caller's raw/typed template capture. The constructor already suspended the outer sequence accumulator, but retained atomic/document preservation and a raw-item side channel. Consequently, `(1,2)` could become text `12`, nested documents could expose a synthetic wrapper, and illegal maps/functions could escape into the outer result.

The constructor now saves and suspends all three additional capture settings while evaluating its attributes and content, restoring them together with the accumulator/container in `finally`. Attribute-set evaluation is inside this protected scope, so an attribute-set error no longer leaves the outer raw collector disconnected. Finishing an element also ends atomic adjacency: an atomic value after a constructed node must not inherit the last atomic value inside that node.

```xml
<xsl:template name="start" as="item()*">
  <xsl:element name="n"><xsl:select>1,2</xsl:select></xsl:element>
</xsl:template>
<!-- The element contains "1 2", including rawResult:true delivery. -->
```

Both direct and conditional (`xsl:on-empty`/`xsl:on-non-empty`) content paths are covered. Computed names, namespaces, attribute-set application, validation and the existing output API retain their paths. Outer raw atomics/maps/functions retain their kinds and order; nested documents contribute children; illegal map/function children raise XTDE0450. No public API, parser/select implementation, serializer, general atomizer, JNode representation, profile selector or conformance runner change is included. Literal-result-element and xsl:copy capture boundaries remain separate reviews.

## Specification and integration base

This work started on locally verified PR333 head `8d9d8909351607dca20685d94484d6112086b020` while its remote build CI was still running. PR333 passed exact-head CI run38911001715 and merged at `8da3b3ddb1701d7ca3dd6a79e8c44c3aa26e7b59`, the final base. The entire precursor/merged trees were verified identical before fast-forward alignment. Tests/native runs use the source-equivalent predecessor; no unrelated source or test changes are incorporated during alignment.

Refreshed upstream remains xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [complex-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-complex-content) govern parent-node construction: consecutive atomics use U+0020 separators, document nodes are replaced by children, and invalid element content is rejected. The caller's return type or raw delivery does not change those rules.

## Regression controls

The original21-case fixture and before log are retained: **3 passed / 18 failed**. Failures independently show missing atomic spaces, document wrapper name `__xdm_doc__`, a following text node starting with an incorrect space (` 4`), illegal function-item diversion and missing raw output after an attribute-set error. The implementation retains every expectation.

The final focused selection passes **58/0**:21 new computed-element controls,21 document-construction controls and16 raw initial-template controls. No fixture compile errors, expectation corrections or weakened assertions were needed.

The new cases cover select/sequence, normal/raw delivery, typed/untyped entry templates, conditional content/fallbacks, map/function errors, caught body/attribute-set errors, nested documents, surrounding raw functions, exact child-node boundaries, namespaces/attribute sets, an empty element between atomics, and XSLT3.0 behavior.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| array instruction set | 24/5/8 | 24/5/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three native family runs exit1 because failures remain. There are no gained or lost native passes. Baseline receipts/logs from PR333 are copied byte-identically into `.artifacts/native-before`; current receipts are separate under `.artifacts/xslt40`. No upstream fixture/expectation, error alias, skip, dependency or gate membership change is included.

## Verification and next review

All local jobs completed: Release build zero errors/two incremental warnings; solution **9,105/0/0** (XSLT6,171), focused combined **58/0/0**, language server **84/0/0**, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. Source, fixture, native receipt and log SHA-256 hashes are in the companion JSON. Canonical docs are synchronized and rendered with markdown-it and canonical Fytala CSS. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged. Source1.14.9 is unpublished.

Next: exact-head CI/review before integration, then audit literal-result-element content for equivalent nested-document or map/function capture leakage. xsl:copy, nonraw text-only API delivery kind, processor-capability architecture, legacy-pattern/obsolete-function review and complex-content JNode semantics remain separate.
