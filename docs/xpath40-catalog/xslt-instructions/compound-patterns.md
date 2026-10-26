<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Compound template patterns and dispatch">
  <h1>Compound template patterns and dispatch</h1>
  <p>21 October 2026</p>
</div>

**C07 lifecycle review — 26 October 2026:** [Current disposition and correction receipts](../LIFECYCLE-RECONCILIATION.md) distinguish delivered scope, retained history and pending acceptance. Six closure workpackages remain; no feature or profile promotion.

**Public1.23.4 — 22 October 2026:** Union-minimum and EQName URI-punctuation priority corrections are [published and verified](../../RELEASE-1.23.4-PUBLICATION.md). Opening-bracket URI parsing remains separately limited. Earlier unpublished delivery records below are historical.

**22 October 2026 — EQName priority correction, source 1.23.4:** Effective 4.0 name-test priority ignores URI punctuation; earlier profiles retain their existing behavior. [Scope, regressions and validation](../EQNAME-PATTERN-PRIORITY.md). Opening-bracket URI parsing remains a separate limitation. Public release remains 1.23.2; REQ-126 stays In Progress. Prior delivery records below retain their historical versions and outcomes.

**22 October 2026 — source 1.23.3:** 4.0 node unions now use the minimum alternative default priority, aligned with the pinned current draft. The two catalog reproducers now expect `competitor`; their prior 1.23.2 observations remain recorded. 3.0 dispatch, explicit priorities and single-rule 4.0 `next-match` are preserved. [Correction and validation](../UNION-MINIMUM-PRIORITY.md). Public release remains 1.23.2; REQ-126 stays In Progress. Earlier delivery snapshots below are historical.

## 1. Name and Classification

**Implemented bounded XSLT 4.0 compound contexts in public 1.26.3.** The union-minimum, EQName URI-priority and opening-bracket parsing corrections are delivered. [C05 review](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) retains their verified scope and the remaining pattern/model limits. [The original review](../COMPOUND-PATTERN-GRAMMAR.md) and dated 1.23.x observations remain historical.

## 2. Intent

Combine node membership tests and dispatch priorities without confusing set parsing, rule identity or item-pattern context.

## 3. Also Known As

Union pattern, intersect/except pattern, grouped XNode pattern and effective-version-sensitive dispatch in the reviewed contexts.

## 4. History / Supersedes

The complete3.0 baseline uses predicate conditions and explicit priorities; the4.0 report uses grouped set/union patterns. Earlier technical outputs are observations, not universal semantic equivalence. Original source delivery revisions and historical adoption gaps remain in the review/audit.

## 5. Motivation

A manual publisher must hold blocked/draft articles even when other readiness conditions match, while keeping review cases visible and output order stable.

## 6. Language Alignment

Pattern operators do not inherit every XPath-expression or C#/SQL precedence intuition. Parenthesized XNode contexts can differ from unparenthesized item patterns. A priority selects a rule after membership; it does not change which nodes satisfy a set expression.

## 7. Applicability

Use supporting XSLT contexts with explicit business priorities. Source1.23.3 corrects implicit union priority to the pinned minimum; public1.23.2 retains the recorded discrepancy.

## 8. Structure

Reviewed shapes include `(A intersect B) except C`, `A | B`, `A union B` and parenthesized XNode forms. These are descriptive shapes; complete XML namespaces and expression context are in the [reports](../examples/manual-patterns/README.md), not an exhaustive grammar.

## 9. Participants

[PatternCompiler](../../../src/Bosak.Xslt/Patterns/PatternCompiler.cs) handles validation/membership; [TemplateRule](../../../src/Bosak.Xslt/Stylesheet/TemplateRule.cs) computes priority and retains rule identity. No public API change.

## 10. Collaborations

Combine rule membership with explicit priority, modes, sorted apply-templates and bounded next-match chains. Type/map/array pattern categories are mapped [separately](structured-value-patterns.md).

## 11. Consequences

Grouping can change membership;4.0 node unions have a single rule identity in the tested chains. Source1.23.3 uses the pinned minimum alternative priority; public1.23.2 retains maximum. An explicit priority is an application policy, not a conformance fix for the engine.

## 12. Implementation

**C05 grammar review — 26 October 2026, published 1.26.3:** [Type/collection/pattern reconciliation](../TYPE-COLLECTION-PATTERN-RECONCILIATION.md) covers S8-compound. Left-associative intersect/except and grouped membership pass. Delivered 4.0 union-minimum priority, EQName URI-punctuation priority and bracket parsing pass; older uncorrected notices remain historical. Earlier-profile priority compatibility, next-match identity and uncommon mixed pattern contexts retain named unit evidence rather than a blanket whole-pattern acceptance. Independent expected results and remaining ownership are retained; no whole-grammar/model or AC-04 acceptance follows.


New manifest cases use default Legacy host with root/local effective3.0/4.0 versions. Local template overrides are tested. No new complete host/module/package/authoring context matrix or gate promotion is claimed.

## 13. Sample Code

Run normal, empty and reordered [manual reports](../examples/manual-patterns/README.md). Compare [set-left40](../examples/manual-patterns/set-left40.xsl) and [set-grouped40](../examples/manual-patterns/set-grouped40.xsl). The [union reproducer](../examples/manual-patterns/union-max40.xsl) verifies the source1.23.3 minimum-priority correction; [explicit priority](../examples/manual-patterns/union-explicit40.xsl) is the safe supported control.

## 14. Performance Notes

Matching costs depend on operands, candidate count, paths and predicates. Priority/rule storage can affect dispatch work. No measured speedup, benchmark or universal memory result is claimed.

## 15. Known Uses

New manual-patterns cases; existing Xslt40SetPatternAssociativityTests, Xslt40UnionPatternTests, Xslt40SetPatternPriorityTests and Xslt40ParenthesizedItemPatternTests are linked in the [review](../COMPOUND-PATTERN-GRAMMAR.md). The bounded union regressions verify the pinned minimum rule in source1.23.3; the URI-punctuation name-priority limitation is corrected in source1.23.4, while opening-bracket URI parsing remains separately limited. No new qt4tests GatedSets member; AC-03 remains partial.

## 16. Related Features

[Structured-value patterns](structured-value-patterns.md), [type inventory](../TYPE-PATTERN-GRAMMAR.md), [union-priority finding](../UNION-PRIORITY-ENGINE-FINDING.md), [catalog](../README.md).
