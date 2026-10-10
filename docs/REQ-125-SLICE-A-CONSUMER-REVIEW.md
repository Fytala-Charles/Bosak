# REQ-125 Slice A Consumer Review

> Reviewed: 2026-10-10 · Consumer: Bosak.Braid · Decision: corrections required before acceptance

## 1. Reviewed Delivery and Evidence

Implementation commit: `a871fad` (PR #134). Reviewed checkout: `1eab1bd0337ee0835d1775e57a87042859a04b73`. Braid's production dependency remains `b5eb1b17249fe458095af2dc53445d459f30db11`; no dependency upgrade or production validation command is part of this review.

Independently ran the `XsltValidationTests` filter in `Bosak.Xslt.Tests`: **29 passed, 0 failed**. Also ran nine public-API consumer probes with an explicit deny-all `IAuthoringModuleResolver`, without executing transformations. Compared their outcomes with `XsltCompiler.Compile`. Compiler acceptance is supplementary evidence and is not treated as proof of semantic validity.

The new API correctly catches malformed XPath in an unused template, accepts a basic valid fixture and a stylesheet-defined function call, and reports schema imports as `UnsupportedCoverage`. Bosak reports wider unit/conformance gates in its delivery record; those wider gates were not rerun during this consumer review.

## 2. Findings

| ID | Severity | Reproducer inside an XSLT 3.0 stylesheet | Actual validation outcome | Expected correction |
|----|----------|-----------------------------------------|---------------------------|---------------------|
| F1 | Acceptance blocker | `<xsl:template match='/'><xsl:unknown-instruction/></xsl:template>` | `Valid`; ordinary compiler rejects with XTSE0010 | Reject structural/static XSLT errors, or explicitly return incomplete coverage rather than `Valid` |
| F2 | Acceptance blocker | `<xsl:template match='missing:item'><ok/></xsl:template>` with no `missing` namespace declaration | `Valid` | Resolve pattern QNames in their actual namespace context and reject undeclared prefixes |
| F3 | Correctness blocker | `<xsl:template match='/'><xsl:sequence use-when='false()' select='('/></xsl:template>` | `Invalid` with XPST0003; ordinary compilation accepts the excluded element | Evaluate supported exclusion before checking the excluded element's other attributes and descendants; explicitly report unsupported exclusion coverage |
| F4 | Static-context blocker | `<xsl:template name='t'><xsl:param name='a' select='$b'/><xsl:param name='b' select='1'/><xsl:sequence select='$a'/></xsl:template>` | `Valid` | Parameter default expressions must use declaration-order scope; later parameters must not be predeclared for an earlier parameter's default |

These findings concern the new validation contract. REQ-124 remains accepted. `missing-value-of-select` was also probed and accepted by both APIs; it is not listed as a confirmed finding.

## 3. Source Review and Expected Changes

In `XsltValidationEngine`, `VisitElement` checks coverage and all slots before `HasLiteralFalseUseWhen`, which explains F3. The exclusion currently skips only descendants. It also pre-adds every template/iterate parameter before traversing parameter defaults, which explains F4. Scope rules need engine-owned tests for template, function and iterate parameter defaults, self references, prior/later declarations and shadowing.

`CheckPatternSlot` invokes `PatternCompiler.Compile(text, context.XpathDefaultNamespace)` without providing the slot's prefix bindings. F2 needs contextual validation of QName-bearing pattern syntax and embedded expressions, including valid declared-prefix positive cases and undeclared-prefix negative cases.

The validation walk deliberately disables ordinary stylesheet compilation and primarily checks classified slots. F1 demonstrates that this omits an error the existing compiler already reports. Bosak should define a safe structural/static validation pass or mark checks that remain deferred. It must not regain those checks by enabling uncontrolled static-expression IO. A complete pass and a partial slot-analysis pass need distinguishable guarantees.

## 4. Required Regression Evidence

- F1: unknown instructions and representative engine-reported XSLT static errors cannot return `Valid`; normal fixtures still pass. Document the full static-validation coverage and any exclusions.
- F2: declared/undeclared prefixes in match and grouping patterns are validated in module-local static context. Include pattern expressions that reference variables/functions where supported.
- F3: excluded-element own attributes and descendants are skipped, while malformed exclusion expressions fail. Test globals excluded by use-when and non-literal exclusions; unsupported evaluation must not become a complete pass.
- F4: later/self parameter references fail where out of scope, earlier references pass, and body references retain correct visibility. Cover template/function/iterate rules independently.
- Re-run all validation tests, the relevant engine regressions and the agreed conformance smoke; record exact results and the fix commit. Braid will rerun the consumer probes against the delivered fix.

## 5. Acceptance and Ownership

**Slice A is not yet accepted for Braid's complete-validation state.** Its malformed-expression detection and outcome taxonomy are useful, but the confirmed false passes and false rejection prevent adopting `Valid` as the promised guarantee. This does not reject the whole REQ-125 design or require Braid to patch engine internals.

Bosak owns corrections, API guarantees and engine test delivery through REQ-125. Braid owns re-review, pinning, faithful outcome mapping and licensed/revision-safe session integration. Slices B/C/D and production preview remain pending independently of these Slice A findings.
