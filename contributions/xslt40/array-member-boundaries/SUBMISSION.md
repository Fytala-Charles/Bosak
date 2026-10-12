# First upstream submission — 2026-10-12

The six-case native batch is submitted as [xslt40-test PR #22](https://github.com/qt4cg/xslt40-test/pull/22), from [the contribution branch](https://github.com/Fytala-Charles/xslt40-test/tree/bosak/array-member-review) at e72f1b0e8511f78478e66c112201317cfe7b7ad1 against master at 6205faf9f32ace6338ac245c41f8eb9a1447926f. The branch contains only the catalog addition, six stylesheets and order fixture. The portable patch and expected outcomes are unchanged.

The PR reports six exact Bosak passes and the prior array-032 engine finding, and asks reviewers to cross-check with another processor. It explicitly says no independent XSLT 4.0 processor was available. Submission does not imply acceptance or independent confirmation. Original failing receipts and candidate post-fix evidence remain preserved.

## Independent processor route

The user has no local XSLT 4.0 processor. [Saxonica's array documentation](https://www.saxonica.com/html/documentation13/xsl-elements/array.html) requires PE or EE for this element and documents partial for-each support in Saxon 13.0. Its descriptions still discuss parcels, while our pinned/current specification uses JNodes. Treat those implementation notes as a version-specific limitation, not grounds to change the normative tests. [Enabling 4.0 features](https://www.saxonica.com/html/documentation13/v4extensions/enable-qt4.html) requires explicit language-version configuration. No alternate processor was installed or executed, and no evaluation license was requested.

The [upstream contribution guide](https://github.com/qt4cg/xslt40-test/blob/master/README.md) welcomes new cases and prefers PRs. It states no second-processor execution prerequisite. We therefore submitted specification-derived additions with honest execution evidence and requested reviewer cross-checking. This is our workflow decision, not a claim that upstream mandates or guarantees the cross-check.

## Next

Monitor PR #22 and respond to review or independent run findings. Preserve the normative assertions until a specification-based correction is justified. N01 remains In Progress, with its first batch submitted; no further additions are included in this PR. N02 xsl:select audit may proceed separately while review is pending. Bosak source candidate remains 1.1.4, unpublished; 1.1.3 remains the public release.
