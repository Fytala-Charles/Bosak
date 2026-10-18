<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak build-warning maintenance and fallback regression coverage">
<h1>Build-warning maintenance and element-to-map fallback</h1>
<p>Source patch 1.19.1; unpublished - 18 October 2026</p>
</div>

## Problem and resulting behavior

A clean Release rebuild of main `f82495dd6ff7d9ef95f72b3e0c179d1205b376cb` reports **81 warnings**, predominantly malformed XML comments, unresolved or ambiguous cref links, displaced comments and missing parameter documentation. The nullable warning in element-to-map also exposes a runtime error: a named layout can fail while a wildcard entry exists without supplying any layout, leading to an unchecked nullable-value access.

For example:

```xpath
element-to-map(parse-xml('<a><b>7</b></a>')/*,
  {'plan': {'a': {'layout':'simple'}, '*': {}}})
```

The explicit simple layout cannot process the child element. An empty wildcard entry is not an actual fallback layout. This now raises the intended **FOJS0008** instead of the .NET message `Nullable object must have a value.` The same applies to wildcard entries containing only type or child metadata.

The implementation captures a concrete wildcard layout with a property pattern before using it. Explicit XML/deep-skip fallbacks retain their results; a successful named layout ignores an unused layout-free wildcard; normal inference still applies when no explicit layout has failed. No new layout inference, conversion-plan schema or public API is introduced. See the [F&O 4.0 draft](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-element-to-map), section 14.6.7 on selecting a layout and absence of a fallback after a selected layout fails. Existing bounded draft support remains the contract; this is not a wider conformance claim.

## Warning corrections

- Escape literal XML delimiters/entities and repair broken/misplaced summary and parameter comments.
- Resolve method overloads and existing validation/type references correctly. References from lower layers to higher layers remain formatted names, without adding project dependencies merely to resolve documentation links.
- Correct nullable annotations in test data, rename a private test helper that resembled an entry point, use xUnit's predicate Single overload, and make the conformance runner's already-established non-null error-code condition explicit.
- Append dated change-history rows while retaining author, copyright and SPDX notices. The solution source version advances to **1.19.1**, unpublished.

No warning suppressions, lowered analyzer settings, public API additions, conformance gate membership changes, extension changes, publishing workflows or Braid source/pin changes are included.

## Observed verification

| Check | Result |
|---|---|
| Baseline clean Release rebuild |81 warnings, zero errors. |
| Regression before fix |Three missing-layout cases fail on the .NET nullable message; four controls pass. Original TRX/log retained. |
| Corrected clean Release rebuild |**Zero warnings and zero errors**, without suppression. |
| Focused element-to-map tests |**29/0/0**, including all seven new regression/control cases. |
| Roslyn syntax audit |20 changed C# files are syntax-equivalent after trivia removal; the five syntax changes are the fallback guard and four test/harness warning corrections. New regression file is checked separately. |
| Language-server tests |**84/0/0**. |
| Runner controls |**10** pass. |
| Frozen QT3 |**31,142/0/679**. |
| Frozen qt4 gated |**8,278/0/1,894**, zero gated failures. |
| Full solution |**9,733/0/0**, ten test projects. |
| Exact-head CI |Required before merge; final receipt records the observed result. |

Receipts in `.guard-tmp/`: maintenance-baseline-build.log, maintenance-warning-inventory.json, maintenance-red-tests.log / maintenance-red-results, maintenance-final-build.log, maintenance-focused-tests.log / maintenance-focused-results, maintenance-syntax-proof.json, maintenance-solution.log / maintenance-solution-results, maintenance-ls.log / maintenance-ls-results, maintenance-runner-controls.log, maintenance-qt3.log and maintenance-qt4.log. Intermediate edit/build logs remain retained; only the corrected final build is described as warning-free.

## Delivery boundary

Public packages/tag 1.19.0 remain immutable and accepted by Braid. This source-only maintenance patch neither publishes 1.19.1 nor changes consumer acceptance. Release preparation/publication, new structural authoring operations and any independent consumer adoption remain separate steps. The product README retains its concise introduction; implementation evidence belongs here and in the handover.
