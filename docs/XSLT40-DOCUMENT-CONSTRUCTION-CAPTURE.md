# Document construction inside raw and typed template results

Unpublished source **1.14.8** restores document complex-content rules when the caller is collecting a raw or typed template result. Previously, the caller's capture settings leaked into a nested document constructor: atomic values `(1,2)` became text `12` instead of `1 2`, nested document nodes could be preserved through synthetic wrappers, and maps/functions could be diverted into the outer raw result instead of rejected as document content.

The shared sequence-constructor evaluator now suspends three capture settings whenever it builds a document: preservation of separate atomic items, preservation of document nodes, and the typed-result raw-item side channel. It restores all three in `finally`, alongside the existing accumulator/focus restoration. Raw sequence constructors continue to preserve their items. The existing effective-XSLT4 binding disqualification rule is evaluated first, so bindings that legitimately return raw map/array sequences retain that behavior.

```xml
<xsl:template name="start" as="document-node()">
  <xsl:document><xsl:select>1,2</xsl:select></xsl:document>
</xsl:template>
<!-- The constructed document's string value is "1 2", including rawResult:true delivery. -->
```

The correction also applies to `xsl:sequence` and array-valued document content, explicit and implicit temporary documents, and the inherited XSLT3.0 profile. It preserves node/text boundaries, flattens nested documents, raises XTDE0450 for illegal document maps/functions, and restores outer raw atomic/map/function items after normal or caught-error exits. No public API, parser, xsl:select implementation, serializer, general atomizer, JNode model, profile selector or conformance runner change is included. The worktree's original select-spacing name records the initial hypothesis; the confirmed branch is `fix/document-construction-capture`.

## Specification and integration base

PR332 passed exact-head CI run38908581751 at02994062fa6b01aba8608af7ee865883a4c6acd5 and merged at `41ba033b20e79124bbeffd5cbdf1540185ecfcfa`, this branch's base. Worktree: D:/Development/Bosak/.guard-tmp/select-spacing-source.

Refreshed upstream remains xslt40-test `91328fbf0fed4a5d2813eb3661808ee53cb961ba`; qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`, xslt.xml blob `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. The [complex-content rules](https://qt4cg.org/specifications/xslt-40/Overview.html#constructing-complex-content) require atomic strings to be separated by U+0020, document nodes to be replaced by their children, and invalid content to be rejected. A caller's delivery mode does not change these construction rules.

## Regression controls and retained attempts

The original18-case fixture/log gave **1 pass / 17 fail**. Four cases incorrectly required Document kind for nonraw text-only API delivery, whose current representation is an Element. That unrelated delivery-kind observation is retained for separate review; the constructor-kind assertion is now scoped to raw delivery, and the text-spacing expectations remain. The corrected before fixture gives **3 pass / 15 fail**.

After the capture-state fix, the corrected18 cases pass without further expectation changes. Added nested-document, outer map/function and XSLT3.0 controls bring the new fixture to21 cases. The focused selection combines these with16 raw initial-template and18 xsl:select controls: **55 pass / 0 fail**.

The21 new cases cover select/sequence, normal/raw delivery, typed/untyped entry templates, explicit/implicit temporary documents, arrays, text/node boundaries, outer raw item kinds/counts, illegal maps/functions, restoration after caught errors, nested document child structure and the earlier profile. Original/corrected/final fixtures and logs are retained with hashes.

## Native accounting

| Selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array aggregate | 155/5/20 | 155/5/20 |
| array instruction set | 24/5/8 | 24/5/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 75/0/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 23/8/1 | 23/8/1 |

All three native family runs exit1 because failures remain; there are no gained or lost native passes. Baseline receipts/logs from PR332 are copied byte-identically into `.artifacts/native-before`; current receipts remain separate under `.artifacts/xslt40`. No upstream fixture/expectation, error alias, skip, dependency or gate membership change is included. Processor-capability architecture and original3.0 profile barriers remain separate.

## Verification and next review

All local jobs completed: Release build zero errors/two incremental warnings; solution **9,084/0/0** (XSLT6,150), focused combined **55/0/0**, language server **84/0/0**, QT3 **31,142/0/679** and qt4 gated **8,278/0/1,894**, both exit0. The companion JSON records source, fixture, native receipt and log SHA-256 hashes. Canonical docs are synchronized and rendered with markdown-it and canonical Fytala CSS. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged. Source1.14.8 is unpublished.

Next: exact-head CI/review before integration, then audit computed-element construction under raw/typed capture for the same boundary leak. Nonraw text-only delivery kind, processor-capability design, legacy-pattern/obsolete-function review and complex-content JNode semantics remain separate.
