<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XSLT XPath compatibility correction">
  <br><br><h1>XSLT Expression Compatibility</h1>
  <p>Effective-version propagation for frozen XPath 4.0</p>
</div>

## Problem and resulting behavior

The routing chapter exposed a runtime defect in published Bosak 1.1.0:
`version="4.0"` stylesheets compiled embedded expressions as XPath31, so direct
`fn:partition` calls failed XPST0017. Validation and authoring already recognized
4.0 expressions, while actual execution rejected them.

The source correction targets **1.1.1**, not yet published. An expression owned
by an element whose effective XSLT version is at least 4.0 now selects frozen
`XPathCompatibility.XPath40`. Earlier versions keep XPath31 and existing 1.0
backwards-compatible behavior. The default public XPath compiler remains XPath31.
This is a compatibility bug fix within Bosak's supported subset, not a claim of
complete XSLT/XPath 4.0 conformance.

| Boundary | Source behavior after this correction |
|---|---|
| Root `version="4.0"` | Frozen XPath40 for its expressions and inheriting descendants. |
| Root `version="3.0"`, instruction `version="4.0"` | The local instruction opts in. |
| Root 4.0, instruction `version="3.0"` | The local expression still rejects 4.0-only syntax/functions. |
| Literal result element `xsl:version` | Controls its AVTs/text templates and inheriting descendants. |
| Include/import | Expression ownership follows the declaring module and local overrides. |
| Future version, such as 5.0 | Frozen XPath40; no experimental functions are granted. |
| `xsl:output/@version` | Serialization version; shadow AVTs use the parent's XSLT version. |
| Nested evaluations | Restore the caller's profile when the nested expression returns or fails. |

Version selection covers normal expressions, legacy instruction handlers,
function bodies, attribute/text value templates, accumulator expressions,
static variables, `use-when`, shadow AVTs, pattern predicates and `xsl:evaluate` target expressions.
Existing namespaces, schema context, base URI and dynamic-evaluation restrictions
are retained. This correction does not redesign pattern grammar, fallback policy,
trust policy, JNode templates or the experimental compatibility API.
Pattern predicates use the existing supported pattern grammar: for example,
`every((1,2), boolean#1)` is verified. The existing match-pattern check that rejects
curly-braced inline function bodies is not relaxed by this profile correction.

## Implementation contract

`Stylesheet.GetXPathCompatibility` derives the profile from expression ownership.
Runtime compilation passes it through both instruction-context and legacy schema
helpers, AVTs and dynamic evaluation. Static stylesheet compilation uses the same
selector. Per-instruction caches retain their existing element-and-expression keys.
Template match, grouping, numbering and accumulator pattern expressions receive
the declaring element's profile; nested pattern compilation inherits it. Validation
and authoring edits use the same numeric version rule, including future declarations.

XSLT installs frozen function signatures once because it normally skips repeated
standard-library population. `EvaluationContext.TryResolveFunction` hides signatures
outside the active profile for direct and dynamic lookup. The compiled expression
stamps its profile as before; nested XSLT evaluations restore the calling profile
before its VM continues. `xsl:evaluate` additionally restores the restricted function
registry and profile on exit. Experimental scan remains unavailable from stylesheet
version declarations.

## Specification provenance and Bosak policy

Reviewed on **2026-10-11** against the XSLT 4.0 Editor's Draft dated **10 October
2026**: [effective-version compatibility rules](https://qt4cg.org/specifications/xslt-40/Overview.html#backwards).
The specification-source snapshot retained for this delivery is QT4CG commit
[`a014e1b408baf5614a11401d3570a04498ab76be`](https://github.com/qt4cg/qtspecs/blob/a014e1b408baf5614a11401d3570a04498ab76be/specifications/xslt-40/src/xslt.xml).
It records effective-version inheritance, the special serialization meaning of
output version, and XPath 4.0 as the draft's expression language.

Bosak's deliberate compatibility policy is narrower than unrestricted draft
adoption: preserve older expressions at XPath31, enable only the shipped frozen
subset at effective version 4.0 or later, and grant no experimental tier through
stylesheet version. Existing forward-compatible instruction handling is retained.
This immutable snapshot establishes the source reviewed for this correction;
it does not reconstruct missing adopted revisions for older feature slices.

## Runnable evidence

[`XPathCompatibilityRuntimeTests`](../tests/Bosak.Xslt.Tests/XPathCompatibilityRuntimeTests.cs)
supplies 30 checks across ordinary/function-body expressions, schema-aware AVTs,
text expansion, dynamic evaluation, static expressions, local and module overrides,
serialization-version separation, nested lookup, experimental refusal, match/grouping
predicates and validation/runtime agreement.

The [standalone routing stylesheet](xpath40-catalog/worked-examples/order-routing/order-routing-40.xsl)
uses 4.0 functions directly. Five additional chapter checks compare complete
dispatch and repair outputs for normal/all-invalid/empty fixtures and verify both
identity refusals leave no outputs. The [two-stage chapter variant](xpath40-catalog/worked-examples/order-routing/README.md)
remains runnable against published 1.1.0 for migration comparison.

Final build, unit and conformance results are recorded in the current
[handover](AGENT_HANDOVER.md). Required gates are the full solution, unchanged
QT3 **31,142/0/679**, and zero failures in promoted qt4 sets. Targeted XSLT 3.0
`version` and `evaluate` sets supply additional host-language regression evidence;
they do not constitute a newly executed complete XSLT conformance sweep.

## Final verification (2026-10-11)

- Release solution build: 0 errors; one existing nullable warning.
- Isolated full solution: **4,103 passed / 0 failed / 0 skipped**.
- Focused runtime/routing checks: **41/0/0**; documentation suite contains 128 checks.
- QT3: **31,142/0/679**, unchanged.
- qt4 promoted sets: **8,278/0/1,894**, zero gated failures; no set promotion.
- XSLT 3.0 version/evaluate sets: **110/0/4** and **41/0/16**.
- Fixture, catalog provenance/link, Markdown rendering and whitespace checks pass.

An earlier full-suite run encountered a localhost port-registration collision in
the database test stub. The isolated full rerun passed all ten projects, including
all 47 database tests; no database code was changed. The targeted XSLT sets are
additional regression evidence, not a complete XSLT conformance claim.
