# Copy-select focus and template-rule restoration

Unpublished source **1.14.14** restores the caller's context item, position, size, current template rule and next-match exclusions around selected copying. The normal dispatcher previously reset the rule/exclusions before selection but restored them only on success; both dispatchers could leave the selected node as the following instruction's focus.

Both copy dispatchers now use `try/finally` around their existing selection/construction paths. Selected content still receives singleton focus. In the item-collecting dispatcher, selected copying explicitly clears the current template rule and exclusions as the normal dispatcher already did. Context-item copying without select retains its inherited rule and focus semantics. Selection expressions, copying/value construction and error codes retain their existing paths.

```xml
<xsl:for-each select="$seed/*">
  <xsl:copy select="$seed"><xsl:value-of select="position()"/></xsl:copy>
  <xsl:value-of select="concat(name(), ':', position(), ':', last())"/>
</xsl:for-each>
<!-- Inside the selected copy: singleton focus. After it: each original child focus. -->
```

## Specification and integration base

PR338 passed exact-head CI run38923795787 at `12d6aceb4fb59534d25fddb2de0b73cb050a474b` and merged at `4ebbce9ce36fa1e8ac5e8a4ad72b8018b066d84c`, this branch's base. Refreshed xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`.

The [shallow-copy rules](https://qt4cg.org/specifications/xslt-40/Overview.html#shallow-copy) require singleton focus when select is present and unchanged context otherwise. The pinned current-template-rule rules clear the rule for copy content if and only if select is present, as part of instructions changing focus. Copy completion and caught errors must leave the caller's state available to following instructions, including next-match.

## Regression controls and fixture correction

The original generated fixture did not compile because C# string quoting around XML select attributes was malformed. Original source/log are retained. Only quoting was corrected (single-quoted XML attributes and encoded XPath string delimiters); assertions were unchanged.

Compilable15-case baseline: **6 passed / 9 failed**. First implementation: **191/0** combined controls (15 new +176 previous). Expanded25-case baseline: **14 passed / 11 failed**. Final combined controls: **201/0/0** (25 new +176 previous). The final test source is byte-identical to the expanded pre-fix fixture; original sources/logs and first implementation snapshot are retained.

Failures independently show focus leakage after successful element/document copies and caught construction errors, plus lost template-rule state after construction, selection and cardinality errors. Selection/cardinality-error focus checks already passed; the separate rule-recovery checks failed. Controls cover both instruction dispatchers, node/path selection forms, caller item/position/size, document focus, context copying without select, selected-copy rule clearing, no-select rule inheritance, and ignored content for empty/atomic selections.

The new ignored-content checks verify behavior and output text inside ordinary parent construction; they do not establish raw atomic/map/array/function value preservation. That is the next separate audit. JNode copying and general constructor behavior are not changed.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three runs exit1 because failures remain. Exact passing identities are unchanged. PR338 receipts/logs are copied byte-identically under `.artifacts/native-before`; fresh receipts are under `.artifacts/xslt40`. The companion JSON retains source/log/receipt SHA-256 hashes. No upstream expectations, aliases, skips or gate membership changed.

## Verification and next review

All local jobs completed: Release build zero errors/22 warnings; solution **9,248/0/0** (XSLT6,314), focused combined **201/0/0**, language server **84/0/0**, frozen QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. All test processes completed; fixture junctions were verified and removed.

Canonical documents are synchronized and rendered using markdown-it and canonical Fytala CSS; renderer receipt is in DOCUMENTATION_RENDERER_TEST.md. Source1.14.14 is unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.

Next: exact-head CI/review, then audit non-node xsl:copy raw values and ignored content in both dispatchers. JNode copying, other constructors' namespace handling, nonraw text-only API delivery kind, processor capability and obsolete patterns/functions remain separate.
