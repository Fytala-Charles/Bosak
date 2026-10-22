<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Manual publication: explicit pattern policy">
  <h1>Manual publication: explicit pattern policy</h1>
  <p>21 October 2026</p>
</div>

**22 October 2026 — source 1.23.3:** 4.0 node unions now use the minimum alternative default priority, aligned with the pinned current draft. The two catalog reproducers now expect `competitor`; their prior 1.23.2 observations remain recorded. 3.0 dispatch, explicit priorities and single-rule 4.0 `next-match` are preserved. [Correction and validation](../../UNION-MINIMUM-PRIORITY.md). Public release remains 1.23.2; REQ-126 stays In Progress. Earlier delivery snapshots below are historical.

A synthetic namespaced manual contains uniquely identified articles. Publish only ready/public articles that are not blocked; hold blocked or draft articles; visibly review everything else. State/audience/blocked values use exact case-sensitive tokens; this is not schema validation, permission enforcement or a production content-management policy.

Compare the [portable3.0 report](report-3.xsl) with the [4.0 report](report-4.xsl). The latter groups intersect/except operands and uses a node union, while both assign explicit routing priorities20/10/-10. Sorting by article ID gives stable output when input article order changes. The report uses ordinary text output and no external variables/resources.

| Fixture | Expected result from both reports |
|---|---|
| [normal](normal.xml) | `articles=4\|A-100:publish\|A-200:hold\|A-300:hold\|A-400:review` |
| [reordered](reordered.xml) | Same report, sorted by unique article ID. |
| [empty](empty.xml) | `articles=0` |

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter "FullyQualifiedName~CatalogExampleTests&DisplayName~manual-patterns"
```

There are30 checks: six complete reports, four mixed-set/grouping checks, four union-priority observations/controls, four next-match/version controls, four set-priority controls, six axis/grouping/lexical controls and two invalid-pattern controls. Technical cases use [probe.xml](probe.xml). The runner uses default Legacy host; version labels do not select a different processor profile.

**Two checks now verify the corrected source contract.** union-max40 and union-word40 return `competitor` in source1.23.3, aligned with the pinned minimum-priority rule. Their manifest metadata retains public1.23.2's previous `pattern` observations. Use explicit application priorities, as the complete report does. See the [engine finding](../../UNION-PRIORITY-ENGINE-FINDING.md).

The3.0 technical outcomes are bounded compatibility observations; use the explicit-predicate3.0 report for the portable task comparison. Parentheses around item/type patterns are not universally interchangeable with unparenthesized patterns: the paired atomic/node probes show the tested boundary. [Full grammar mapping](../../COMPOUND-PATTERN-GRAMMAR.md).
