<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Union pattern default-priority follow-up">
  <h1>Union pattern default-priority follow-up</h1>
  <p>21 October 2026</p>
</div>

**Current-draft gap; no engine correction in this slice.** Source/public1.23.2 uses the maximum alternative priority for a4.0 node union. The reviewed draft requires the minimum. Historical adopted-specification provenance is not recovered, so this finding compares current published source with an explicit current review pin; it is not proof of a newly introduced regression.

Primary source: [immutable XSLT XML](https://raw.githubusercontent.com/qt4cg/qtspecs/3230573afe246f1b502ed2e22d187ce9928155b1/specifications/xslt-40/src/xslt.xml) at `3230573afe246f1b502ed2e22d187ce9928155b1`, SHA-256 `6d3038e21291abf761d883dd371cb9885972e0dd1c85b41844df70ca1ede45b8`, section `default-priority-for-xnode-patterns`. The relevant union rule selects the minimum default priority of alternatives. The same section separately changes3.0 split-rule handling to a single4.0 rule and explains why next-match must not revisit that rule for another matching alternative. These are distinct decisions.

## Bounded reproducer

The [symbolic union](examples/manual-patterns/union-max40.xsl) and [keyword union](examples/manual-patterns/union-word40.xsl) both apply templates to `<root/>` with competing rules:

```xml
<xsl:template match="root | missing/child">pattern</xsl:template>
<xsl:template match="." priority="0.25">competitor</xsl:template>
```

| Component | Priority / result |
|---|---|
| `root` alternative |0 |
| `missing/child` alternative |0.5; does not match this input |
| Explicit competitor |0.25 |
| Current maximum aggregation |0.5; result `pattern` |
| Pinned minimum aggregation |0; expected result `competitor` |

`TemplateRule.ComputeDefaultPriority` in [source](../../src/Bosak.Xslt/Stylesheet/TemplateRule.cs) uses branches.Max for node unions. [Xslt40UnionPatternTests.MaximumAlternativePriority](../../tests/Bosak.Xslt.Tests/Xslt40UnionPatternTests.cs) currently codifies that behavior. The two new manifest rows keep expected=`pattern` explicitly as implementation observations, with reviewStatus=`observed-current-draft-gap` and normative40Expected=`competitor`. Original execution manifest/receipts remain under `.guard-tmp/req126-compound-evidence`; no expected value or old test is silently changed.

## Proposed separate correction scope

1. Review/approve alignment of the tested4.0 node-union contract with this pinned minimum-priority rule. State the behavior change for consumers relying on implicit priorities; explicit application priorities remain the safe policy in the worked report.
2. Limit a correction to4.0 node-union priority aggregation. Preserve3.0 branch-specific dispatch,4.0 single-rule identity/next-match behavior, explicit priorities and effective local version selection. Do not replace unrelated maximum calculations for type choices or kind-test names without their own specification review.
3. Use specification-derived regressions for both union spellings, unequal/equal alternative priorities, nested parentheses/unions, an unmatched higher-priority alternative, explicit overrides, local version3.0/4.0, single-rule next-match and ambiguity controls. Preserve the old successful implementation-observation receipts and record the corrected normative outcomes separately.
4. Run the full solution plus both mandatory engine-touching gates. Review any native suite effects without weakening original assertions or promoting sets merely because this example passes. A released engine correction needs its own patch version and publication decision; this documentation slice changes neither.

This is not a request to implement all draft pattern changes, revise3.0 priorities, modify union membership or change the practical report's explicit policy. Independent processor confirmation, upstream acceptance and broad conformance remain separate. [Grammar review](COMPOUND-PATTERN-GRAMMAR.md).
