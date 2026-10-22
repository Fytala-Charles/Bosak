<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Compound template patterns and dispatch">
  <h1>Compound template patterns and dispatch</h1>
  <p>21 October 2026</p>
</div>

## 1. Name and Classification

**Implemented bounded XSLT4.0 compound contexts, with an explicit current-draft union-priority gap.** Public/source1.23.2 unchanged. [Pinned review](../COMPOUND-PATTERN-GRAMMAR.md) distinguishes observed behavior from normative expectation.

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

Use supporting XSLT contexts with explicit business priorities. Avoid relying on implicit union priority while the recorded minimum-versus-maximum draft discrepancy remains.

## 8. Structure

Reviewed shapes include `(A intersect B) except C`, `A | B`, `A union B` and parenthesized XNode forms. These are descriptive shapes; complete XML namespaces and expression context are in the [reports](../examples/manual-patterns/README.md), not an exhaustive grammar.

## 9. Participants

[PatternCompiler](../../../src/Bosak.Xslt/Patterns/PatternCompiler.cs) handles validation/membership; [TemplateRule](../../../src/Bosak.Xslt/Stylesheet/TemplateRule.cs) computes priority and retains rule identity. No public API change.

## 10. Collaborations

Combine rule membership with explicit priority, modes, sorted apply-templates and bounded next-match chains. Type/map/array pattern categories are mapped [separately](structured-value-patterns.md).

## 11. Consequences

Grouping can change membership;4.0 node unions have a single rule identity in the tested chains. Current maximum alternative priority diverges from the pinned minimum rule. An explicit priority is an application policy, not a conformance fix for the engine.

## 12. Implementation

New manifest cases use default Legacy host with root/local effective3.0/4.0 versions. Local template overrides are tested. No new complete host/module/package/authoring context matrix or gate promotion is claimed.

## 13. Sample Code

Run normal, empty and reordered [manual reports](../examples/manual-patterns/README.md). Compare [set-left40](../examples/manual-patterns/set-left40.xsl) and [set-grouped40](../examples/manual-patterns/set-grouped40.xsl). The [union reproducer](../examples/manual-patterns/union-max40.xsl) is explicitly an observed gap; [explicit priority](../examples/manual-patterns/union-explicit40.xsl) is the safe supported control.

## 14. Performance Notes

Matching costs depend on operands, candidate count, paths and predicates. Priority/rule storage can affect dispatch work. No measured speedup, benchmark or universal memory result is claimed.

## 15. Known Uses

New manual-patterns cases; existing Xslt40SetPatternAssociativityTests, Xslt40UnionPatternTests, Xslt40SetPatternPriorityTests and Xslt40ParenthesizedItemPatternTests are linked in the [review](../COMPOUND-PATTERN-GRAMMAR.md). Current-implementation union tests do not certify draft minimum priority. No new qt4tests GatedSets member; AC-03 remains partial.

## 16. Related Features

[Structured-value patterns](structured-value-patterns.md), [type inventory](../TYPE-PATTERN-GRAMMAR.md), [union-priority finding](../UNION-PRIORITY-ENGINE-FINDING.md), [catalog](../README.md).
