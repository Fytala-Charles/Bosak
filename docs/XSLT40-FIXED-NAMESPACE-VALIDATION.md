# Fixed namespace module validation

Unpublished source **1.16.1** fixes the module whitelist that rejected fixed-namespaces before its existing binding handler could run under the opt-in draft processor. Stylesheet/transform and package validation now add that module-only attribute to their local allowed sets when the module's effective version is4.0 or later. No runtime, binding parser, processor default, shared standard-attribute helper or public API is changed.

## Supported behavior and limits

The [draft fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) permit this attribute on the outermost xsl:stylesheet, xsl:transform or xsl:package of a module. The existing HasModuleFixedNamespaces helper activates bindings only for module roots declaring version4.0 or later. This correction aligns validation with that already implemented range; it does not expand activation to earlier module versions, including version3.0 modules compiled with Xslt40Draft. That remains a profile gap, not a claim that the draft forbids the attribute there.

Valid declarations now reach existing type/expression bindings and module isolation. Invalid bindings retain XTSE0122, unknown unqualified module attributes retain XTSE0090, and earlier legacy modules retain their unknown-attribute rejection. The module option is not added to the standard instruction set: misplaced attributes on sequence, copy and apply-templates remain errors. Foreign qualified metadata is not a binding declaration; XSLT-qualified spellings remain invalid on the tested module roots. A local earlier expression version does not remove its4.0 module's fixed bindings. Future-version tolerance does not bypass invalid known bindings.

## Independent regressions and preserved attempts

Initial source and sandbox build log are retained under .artifacts. The first attempt stopped on denied XML documentation output before the tests compiled. A C#/XML attribute quoting issue was corrected before the compilable34-case baseline: **28pass/6fail**. The first implementation combined with prior namespace/profile controls gave **98pass/1fail** (99 total). That remaining failure was an unrelated fixture name collision: its library exported template start, also declared by the caller. Renaming only the library fixture template preserved the expected function result and resolved the collision.

Expansion added qualified-metadata/XSLT-qualification and local earlier-version controls. The final42-case fixture was rerun on the original Stylesheet implementation: **35pass/7fail**. Original and implemented source snapshots, original/corrected/expanded fixtures and each log remain separate. Restored implementation: **107/0/0** combined controls (42 new +65 existing fixed namespace/profile cases). The final test source is byte-identical to the expanded pre-fix fixture. No expected result or error was weakened.

## Native before and after

| Profile/family | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| Legacy array |155/5/20 |155/5/20 |
| Legacy record |29/35/1 |29/35/1 |
| Legacy type-alias |23/8/1 |23/8/1 |
| Draft array |158/2/20 |158/2/20 |
| Draft record |29/35/1 |29/35/1 |
| Draft type-alias |16/15/1 |23/8/1 |

Only type-alias-016 through type-alias-022 change, all from failure to pass in Xslt40Draft. No passing identity is lost. Every other status and failure diagnostic is unchanged across the six selections. All six family runs still exit1 because failures remain. In particular019 now reaches its expected XPST0051 instead of being preempted by the module whitelist. Exact errors, source versions, upstream fixtures, skips and gates are untouched.

PR345 receipts/logs are copied byte-identically under .artifacts/native-before, including its selected wrapper controls. Fresh full-family receipts are under .artifacts/xslt40. Case comparison and SHA256 hashes are recorded in companion JSON. Runs used exact-path command-scoped Git trust; no global configuration change. Earlier failed draft receipts remain preserved as evidence of the regression.

## Verification and base

All local processes completed: Release build **0errors/13 existing warnings**; solution **9,426/0/0** (XSLT6,492), focused **107/0/0**, language-server **84/0/0**, ten CLI controls pass, frozen QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, both gates exit0. Verified fixture links removed. Canonical docs synchronized and ten pages rendered with markdown-it and canonical Fytala CSS; no visual preview approval claimed.

PR345 exact-head CI run38941231250 passed at2f6e711d8255d60efda1deda4a464dc2ceef27ec, including its new Ubuntu/pwsh controls. It merged at3c02eca7a9ee1ac8308e37b4f91f57d4dc4c0d1b, this slice's base. Refreshed xslt40-test remains91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs remains72f222929768b411d8e88d96a1a8740483290078, xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e. No upstream contribution was prepared.

## Next review

Audit field/@required against the current draft: record-023 currently fails XTSE0090 at that whitelist in the draft lane. Keep the existing failures and spec revisions while deciding the bounded correction. Earlier-version fixed-namespace activation, other direct validators, unknown-future/no-fallback handling, schema-role, and authoring/validation/nested fn:transform profile controls remain separate. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance remain unchanged.
