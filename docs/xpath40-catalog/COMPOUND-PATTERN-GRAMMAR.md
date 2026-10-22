<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Compound, union and set-pattern grammar">
  <h1>Compound, union and set-pattern grammar</h1>
  <p>21 October 2026</p>
</div>

**22 October 2026 — source 1.23.3:** 4.0 node unions now use the minimum alternative default priority, aligned with the pinned current draft. The two catalog reproducers now expect `competitor`; their prior 1.23.2 observations remain recorded. 3.0 dispatch, explicit priorities and single-rule 4.0 `next-match` are preserved. [Correction and validation](UNION-MINIMUM-PRIORITY.md). Public release remains 1.23.2; REQ-126 stays In Progress. Earlier delivery snapshots below are historical.

This REQ-126 D3 slice maps existing compound-pattern parsing, membership, grouping, default priority and rule identity. The original 21 October documentation delivery left source/public1.23.2 unchanged; the separately validated 22 October source correction above supersedes its priority observations.

Reviewed PR #403 merged as `d7ccd80d901813876f6889f955c87ac760575b2c` after exact-head CI39637016178 passed10,408 solution +84 language-server tests. Its type/pattern inventory remains unchanged. New totals: **71 entries;2,743 feature +24 chapter checks =2,767**; four worked chapters and117 assigned function names unchanged. All2,713 prior manifest rows are retained;30 rows are appended.

## A report with explicit business policy

The [manual publication example](examples/manual-patterns/README.md) sorts unique article IDs, publishes ready/public/unblocked articles, holds blocked or draft articles, and sends remaining articles to visible review. The3.0 baseline uses predicate conditions;4.0 illustrates grouped intersect/except and node union patterns. Explicit priorities20/10/-10 express business policy, so this example does not rely on the default-union priority discrepancy below. Six complete reports cover normal, empty and reordered namespaced input.

## Current primary source and evidence scope

Fresh qtspecs HEAD is `3230573afe246f1b502ed2e22d187ce9928155b1`; [immutable XSLT source](https://raw.githubusercontent.com/qt4cg/qtspecs/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xslt-40/src/xslt.xml) SHA-256 `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`. Review sections: pattern-syntax, node-patterns, xnode-pattern-matching-process and default-priority-for-xnode-patterns. This is a current review pin, not recovered historical adoption.

**Priority correction:** public1.23.2 aggregates with maximum; source1.23.3 uses the pinned minimum. The two reproducers now expect `competitor`, retaining their earlier `pattern` observations as metadata. [Original finding](UNION-PRIORITY-ENGINE-FINDING.md) and [correction](UNION-MINIMUM-PRIORITY.md).

## Descriptive grammar and dispatch mapping

These shapes map the reviewed contexts; they are not an exhaustive normative grammar or a guarantee for every combination of item patterns and node patterns.

| Boundary | Implemented observation / reviewed expectation | New manifest IDs after `manual-patterns-` |
|---|---|---|
| Mixed `except` / `intersect` |4.0 evaluates the tested chain left to right; parentheses change membership. The3.0 result is recorded separately as a compatibility observation. | set-left40, set-grouped40, set-explicit-left40, set-earlier30 |
| Node union `\|` / `union` | Both spellings match alternatives. Source1.23.3 uses the pinned minimum default priority; public1.23.2 uses maximum. | union-max40, union-word40; both corrected in source1.23.3 with prior observations retained |
| Explicit priority | A declared priority overrides aggregation and keeps application policy visible. | union-explicit40 and both full reports |
|3.0 branch-specific ranking | A matching lower-ranked alternative loses to the explicit competitor in the tested3.0 fixture. This is a bounded observation, not a universal3.0 conformance certificate. | union-earlier30 |
|4.0 union rule identity | Matching duplicate alternatives do not cause a second visit by next-match;3.0 retains the tested repeated branch visits. Template-local effective versions govern the observed behavior under Legacy. | next40, next30, next-local40, next-local30 |
| Set default priority | The tested4.0 intersect/except chains use their first operand's default priority. Membership and priority are separate decisions. | set-priority-high/low/first/predicate |
| Axis operands |4.0 axis operands test the candidate node in the reviewed cases;3.0 output is retained as an earlier-profile observation. | axis40, axis30 |
| Parenthesized XNode context | `(~element(root))` matches a node; `(~xs:integer)` does not match the selected atomic42, although unparenthesized `~xs:integer` does. Parentheses are not a universal identity transformation across pattern categories. | parenthesized-node, parenthesized-atomic, direct-atomic |
| Lexical delimiters | Operator-like words and union punctuation inside the tested string literal are not treated as outer operators. EQName/other lexical edges remain covered by named existing tests, not new exhaustive claims. | quoted-operators |
| Invalid combinations | Missing mixed-set operand and unparenthesized array-pattern set operand raise XTSE0340. | missing-operand, invalid-item-set |

## Existing source and named tests

| Responsibility | Source / named evidence |
|---|---|
| Pattern validation, lexical splitting and matching | [PatternCompiler](../../src/Bosak.Xslt/Patterns/PatternCompiler.cs) |
| Default priorities, union-rule representation | [TemplateRule](../../src/Bosak.Xslt/Stylesheet/TemplateRule.cs); ComputeDefaultPriority currently uses branches.Max for node unions |
| Mixed set membership | [Xslt40SetPatternAssociativityTests](../../tests/Bosak.Xslt.Tests/Xslt40SetPatternAssociativityTests.cs): LeftAssociativeMembership, EarlierProfile, MissingOperand |
| Union priority and identity | [Xslt40UnionPatternTests](../../tests/Bosak.Xslt.Tests/Xslt40UnionPatternTests.cs): MinimumAlternativePriority (source1.23.3), NextMatchVisitsSingleRule, ExplicitPriority, TemplateProfileOverride |
| Set/predicate ranking | [Xslt40SetPatternPriorityTests](../../tests/Bosak.Xslt.Tests/Xslt40SetPatternPriorityTests.cs): FirstOperandPriority, PredicatePriority, OperatorTextBoundaries |
| Candidate axis focus | [Xslt40SetPatternAxisFocusTests](../../tests/Bosak.Xslt.Tests/Xslt40SetPatternAxisFocusTests.cs): AxisMembership, MixedPaths, AllPathPositions |
| Grouping and item boundary | [Xslt40ParenthesizedItemPatternTests](../../tests/Bosak.Xslt.Tests/Xslt40ParenthesizedItemPatternTests.cs): MatchesNodes, NonNodesDoNotMatch; [delimiter tests](../../tests/Bosak.Xslt.Tests/XsltPatternDelimiterTests.cs) |

Implementation/test delivery revisions are17bf95aa (left association),62fa90dd (single union rule/current maximum priority), b161cdd0 (set/predicate priority),8a80f2ee (axis focus) andc2290f67 (parenthesized item semantics). Full revisions are recorded in the audit. They establish source delivery, not the original adopted specification snapshot.

## Compatibility and acceptance limits

New examples use the **default Legacy host**. xslt31/xslt40 describe stylesheet versions, not different host profiles. Local template version overrides are tested explicitly; no new exhaustive Draft-host, package/import, authoring or schema-aware matrix is claimed. Existing3.0 observations are marked in the manifest; the portable task baseline avoids ambiguous mixed chains and relies on explicit policy.

The30 passing checks include two implementation observations for the same union-priority gap. Passing those rows is not normative conformance. Other untouched engine findings, including deep-equal options, remain separate. No XPath qt4tests GatedSets membership applies merely because these XSLT cases pass. AC-03's literal every-entry gate acceptance treatment is still unresolved and is not waived.

D3 now has bounded mappings for switch/array/select, types/structured patterns and these compound contexts; it is still not an exhaustive grammar/type/pattern inventory. D2 full function/options contracts and D4 historical adoption remain partial; REQ-126 stays **In Progress**. Next action recommended: review the separate narrow union-priority correction scope, then reconcile the remaining D3 coverage matrix and AC-03 acceptance treatment before promising closure.

**Validation:** Release build passes with the existing conformance Program.cs:1868 CS8604 warning. All10,438 solution tests pass, including7,470 XSLT tests; focused initial and annotated runs each30/0/0. The two explicit observed-gap rows remain distinct from normative support. All71 entries retain16 fields, new local links/XML/JSON/headers validate and all2,713 original manifest rows are unchanged. Engine/harness trees match published v1.23.2; no manual engine-gate rerun is claimed for this documentation-only diff. The audit records all ten TRX hashes, fixtures, original manifests and the exact current specification pin.
