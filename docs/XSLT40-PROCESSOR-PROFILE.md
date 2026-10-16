# Explicit XSLT processor profile

Unpublished source **1.15.0** adds host-selected `XsltCompiler.ProcessorProfile` with `XsltProcessorProfile.Legacy` (default) and `Xslt40Draft` (opt-in). This minor version introduces a public API; it is not published. Compiler defaults and frozen conformance runners retain their existing behavior.

## Contract

Legacy preserves the3.0 forward-compatibility ceiling and chooses XPath31/40 from effective version. The draft profile uses a4.0 ceiling and frozen XPath40 independently of stylesheet version. Invalid enum values are rejected with ArgumentOutOfRangeException by the string/document compile entry points. Each stylesheet captures its profile before loading; imports, includes and used packages inherit from the compilation root. Changing the compiler afterward does not alter existing executables.

In the draft lane, copy and sequence reject unknown unqualified attributes at effective3/4 and ignore them above4. Copy retains checks for recognized invalid boolean options and missing attribute sets. Sequence/fallback whitelists exclude module-only input-type-annotations. Namespace declarations, supported standard attributes and qualified metadata retain their paths. Legacy's historical copy/sequence inconsistencies remain observable by design; existing negative controls are unchanged.

An unversioned fallback in the draft lane resets effective version to4.0, including descendants unless they explicitly override version. Static validation and runtime forward behavior consult the captured ceiling. Static use-when/shadow expressions, AVTs, ordinary expressions, stylesheet functions, imported/included modules and used-package functions use frozen XPath40 in the draft lane. XPath40Experimental remains disabled.

The [draft forward rules](https://qt4cg.org/specifications/xslt-40/Overview.html#forwards), [3.0 compatibility rules](https://qt4cg.org/specifications/xslt-40/Overview.html#backwards-3.0) and fallback reset rule informed this implementation. This is an experimental partial4.0 processor profile, not a claim of complete4.0 conformance.

## Scope and remaining inventory

| Validation path | This slice |
|---|---|
| Shared ValidateAllowedAttributes callers |Use selected ceiling |
| Copy-specific whitelist |Draft suppression for unknown options; known-value checks retained |
| Sequence |Draft-only whitelist added |
| Fallback |Draft-only whitelist and effective-version reset added |
| Other instruction-specific literal whitelists |Unchanged; require individual review |
| Static string helper in authoring/validation APIs |Legacy behavior retained; no profile option yet |
| fn:transform separate compiler and CLI runners |Existing defaults retained; no automatic propagation |

A source-context inventory of XTSE0090 and shared-validator sites is preserved under .artifacts/validator-inventory.json. This inventory does not prove every instruction conforms to the draft. Known future instructions without fallback, other custom validator loops, schema-role and other earlier constructor limitations remain follow-up work. No aliases, skips, gate promotions, upstream patches or public release are introduced.

## Baselines and regressions

The initial41-case baseline ran after adding only the API/enum/profile-capture seam, with original compatibility behavior: **24pass/17fail**. First combined implementation run: **142pass/1fail** (143 total). Its remaining failure was a fixture API assumption: raw sequence ToString returns '(sequence)', so the same expected7/8 values are now asserted item by item. Expansion added module validation, used-package grammar/validation, known-version unknown instructions, fallback attributes and AVT/function coverage. The first expansion did not compile because XdmSequence has no LINQ Single; corrected to its struct enumerator, preserving the expected attribute value. All original sources/logs and corrections are retained separately.

The final49-case corrected fixture was rerun against the API-only seam: **26pass/23fail**. API-seam files and original runtime snapshot, then restored implementation snapshots, are retained. The restored implementation passes **151/0/0** in the selected profile/XPath/surface/standard-attribute suite, then **408/0/0** with all prior construction/copy controls. The final fixture is byte-identical to the expanded corrected fixture. Both legacy and draft matrices are asserted permanently; no old fixture expectation changed.

All local jobs completed: Release build **0errors/11 existing warnings**; solution **9,384/0/0** (XSLT6,450), final focused **408/0/0**, language-server **84/0/0**, frozen QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, both gates exit0. All processes completed; fixture junctions were verified and removed. Canonical documentation is synchronized and rendered with markdown-it and canonical Fytala CSS.

## Native accounting and revisions

Fresh default-runner accounting remains array **155/5/20**, record **29/35/1**, type-alias **23/8/1**, each family exit1; all passing identities match retained PR342 receipts byte-for-byte by case name. These runs use the unchanged legacy compiler default, not the new opt-in draft profile. Original wrapper launches failed at Git ownership validation before tests; failed logs are preserved separately. Successful reruns used exact-path, command-scoped safe.directory and diff.ignoreSubmodules settings only, with no global Git configuration change. Companion JSON records identities, receipts and hashes.

PR343 exact-head CI run38937727152 passed at099beefa70e9c4abbddbf2457a5b33f5ffa3b216 and merged at28474eefd2445b3cc151c46c161d953f1c025b80. Audit precursor and merge whole trees are identical497118d19c13665b05de080c7596192ead1522fc; this branch fast-forward aligned before broad verification. Baseline API seam began on the exact precursor. Refreshed xslt40-test remains91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs remains72f222929768b411d8e88d96a1a8740483290078, xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e.

## Next slice

Add an explicit draft-profile option to the XSLT conformance runner/wrapper while retaining the existing default invocation. Re-run original upstream3.0/4.0 fixtures without rewriting them; record every changed result by identity. Then review remaining instruction-specific validators and unsupported-instruction fallback errors under the opt-in profile. Authoring/validation and nested fn:transform profile controls need separate API decisions. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ status and independent Braid acceptance remain unchanged.
