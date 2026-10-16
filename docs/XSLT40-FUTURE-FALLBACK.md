# Future instruction fallback validation and execution

Milestone 3/3, unpublished patch 1.16.3. The draft processor rejects unknown future instructions without xsl:fallback at compile time (XTSE0010), including unused template/function bodies and included/imported/package modules. Unknown top-level declarations and ignored descendants remain ignored; use-when exclusion is applied first. Legacy profile behavior is preserved.

Valid future instructions in function bodies now evaluate their fallback sequence constructors. This uses the existing raw item collector and restores local variables after each fallback, including errors. Multiple fallbacks execute in document order; non-fallback siblings remain ignored. Maps remain maps rather than being serialized into text. The ordinary instruction dispatcher retains its existing fallback execution.

The pinned draft [forwards compatibility rules](https://qt4cg.org/specifications/xslt-40/Overview.html#forwards) define future versions relative to the processor's supported version and require fallback for unknown instructions. This correction applies to the opt-in draft processor; no public API or default profile changes.

Original 32-case baseline: 16 pass /16 fail. Expanded 37-case baseline: 16 pass /21 fail. Final focused suite: 156 pass /0 fail, including all 37 new controls. Original and expanded fixture bytes and baseline receipts are retained separately. No expected errors or assertions were weakened. Controls cover unused bodies, nested ignored content, local versions, alias prefixes, imports/includes/packages, static errors outside try/catch, raw map values, fallback ordering, variable scope and error recovery.

All six native selections retain identical case identities, statuses and diagnostic details: Legacy array155/5/20, record29/35/1, type-alias23/8/1; Draft array158/2/20, record29/35/1, type-alias23/8/1. All exit1. Draft total210 pass /45 fail /22 skip across277 cases. These remaining failures include catalog/draft drift and are not all demonstrated engine gaps. No skips, expected-code aliases or gate promotions were added.

PR348 exact-head CI38948711915 passed at6b5b8755b20bd8c4a223290606adb9908bd6f429 and merged at0bb60bf837769ac2bcf489169d4aec817a9c0b26. The precursor and merge whole trees match cb1580742693a99e6da9434bbff8d78d7c3f9b20; the final slice was fast-forward aligned before submission.

This completes the bounded three-slice implementation: PR347 audited retired xsl:field/@required without changing correct rejection behavior; PR348 corrected earlier-version fixed namespace contexts and module ownership; this final slice corrects future fallback validation/execution. Stop after this PR's exact-head CI and integration. Suggested next decision: review release readiness, distinguish obsolete catalog expectations from engine defects, then authorize a separately bounded backlog. Do not publish or start a fourth feature automatically. The preexisting ambiguous-prefix serialization finding remains a separate backlog item.


All processes completed: Release build0errors/2 existing warnings; solution **9509/0/0** (XSLT6575), language-server **84/0/0**, ten CLI controls pass. Fresh QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, both exit0. Verified fixture links removed; canonical documents synchronized and rendered.

Refreshed suite91328fbf0fed4a5d2813eb3661808ee53cb961ba; qtspecs72f222929768b411d8e88d96a1a8740483290078; xslt.xml blob59b9bdc79dca23b0c18fdd3e94130bc59037db3e. No upstream edits, aliases, skips or gate promotions. Source remains unpublished; public versions and independent acceptance unchanged.
