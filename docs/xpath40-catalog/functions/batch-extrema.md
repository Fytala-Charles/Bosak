<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Batch Extrema and Component Codes">
  <br><br><h1>Batch Extrema and Component Codes</h1>
  <p>Frozen 4.0 — changed existing-function contracts and observed limits</p>
</div>

## 1. Name and Classification

[fn:min](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-min)
and [fn:max](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-max)
already exist in XPath 3.1. Their changed 4.0 contracts are outside the
117-name inventory of 4.0-only functions. Bosak's QName extension was delivered
in [PR #114](https://github.com/Fytala-Charles/Bosak/pull/114),
[33894747](https://github.com/Fytala-Charles/Bosak/commit/33894747f04da970a1f3c0f17fa960fa1f620605).
Reviewed 2026-10-12 against immutable
[QT4CG F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml).
This review does not recover the historical adopted draft.

## 2. Intent

Report a quality-control batch's accepted decimal range and the endpoints of its
namespace-aware component-code inventory. Preserve rejected row identifiers and
make empty batches explicit.

## 3. Also Known As

Minimum, maximum and extrema describe these functions. Component-code endpoints
are namespace/local-name ordering, not a severity ranking or business priority.

## 4. History / Supersedes

The stable [3.1 min](https://www.w3.org/TR/xpath-functions-31/#func-min) and
[3.1 max](https://www.w3.org/TR/xpath-functions-31/#func-max) contracts use numeric
promotion and a narrower set of orderable types. The reviewed 4.0 draft compares
converted values through the broader `fn:compare` ordering and returns a selected
converted input value. Bosak adds QName extrema, but its shared numeric and
duration paths retain earlier behavior. The 3.1 report below uses explicit
namespace/local-name folds instead of calling extrema on QNames.

## 5. Motivation

Imported readings need a useful range without allowing malformed values to
silently become NaN. Namespaced component codes also need a deterministic index
boundary independent of the XML prefix chosen by a sender. Both tasks benefit
from explicit conversion and ordering policies.

## 6. Language Alignment

Bosak registers one- and two-argument forms of both functions. Empty input returns
an empty sequence. Untyped atomic values are converted to doubles; explicitly
supplied strings remain strings. Thus untyped XML numbers `2` and `10` have a
numeric range, while string values `'2'` and `'10'` have a lexical range.
Numeric NaN contaminates either extreme; the application must screen it when
missing measurements should be excluded. Mixing number and string raises
`FORG0006` in these fixtures.

QNames are ordered by namespace URI and then local name in codepoint order.
Prefixes do not determine ordering. Equivalent QNames may carry different
prefixes, so the report prints expanded names rather than asserting a chosen
prefix. The draft leaves the chosen representative of equal extrema unspecified.

The second argument selects string collation. Bosak accepts the demonstrated
empty-collation form in both profiles; only the 4.0 draft's default-on-empty
contract is claimed. Keyword `values` matches the reviewed parameter name and
requires 4.0 syntax.

**Observed contract gaps:** mixed numeric values are still promoted in Bosak.
`min((3.1, 3.1e0))` produces a double whose exact comparison with decimal `3.1`
is `1`; under the pinned draft's ordering the minimum is the decimal and that
comparison should be `0`. The equal-highest integer/float example produces a
double even though that converted input value is not the selected extreme.
The integer/decimal probe also returns a decimal rather than preserving the
winning integer. Plain durations remain refused with `FORG0006`, although the
reviewed atomic comparator defines their ordering. Fixtures marked `observed`
record these limits; they are not conforming-result assertions.
When an engine correction lands, update those observed expectations and retain
the historical behavior and migration note alongside the corrected contract.

## 7. Applicability

The synthetic quality-control vocabulary is not an industry schema. IDs and
component-code text are assumed present, with resolvable namespace prefixes.
`castable as xs:decimal` screens malformed, absent and NaN value text. It does not
validate units, sensor calibration, engineering tolerances or business ranges.
An invalid QName or undeclared prefix requires separate validation. The host owns
data acquisition and corrective action.

## 8. Structure

Split accepted/rejected rows, explicitly convert accepted values to decimals,
resolve every row's component code in its XML namespace context, and calculate
numeric extrema plus code endpoints. Codes include rejected readings so the
inventory describes the full imported batch, while the numeric range uses only
accepted rows.

## 9. Participants

Readings carry values, IDs and code names. Decimal constructors implement the
numeric policy. `resolve-QName` creates namespace-aware codes. Extrema choose
boundaries. A label function formats expanded names without prefix dependence.

## 10. Collaborations

Every accepted value becomes a decimal before aggregation, avoiding the mixed
numeric promotion gap in this report. An empty range or code inventory prints
`-`; zero remains a real measurement. The 3.1 code comparator compares namespace
URIs first and local names second with explicit codepoint collation. Two folds
retain first and last endpoints without constructing an ambiguous combined key.

## 11. Consequences

Both reports agree on this deliberately normalized decimal/QName workflow.
That agreement does not establish mixed-type equivalence between the language
versions. The endpoints organize a code inventory and have no severity meaning.
Malformed values remain traceable; the decimal storage precision limit and
unvalidated code/unit/range constraints remain outside the demonstrated contract.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Empty input, atomization, numeric/string and QName extrema; Omitted/empty collation and values keyword; NaN contamination and incomparable-family errors. **Boundaries:** Use homogeneous validated atomic inputs and explicit collation; remaining coercion/mixed-family combinations require targeted review. Follow-up groups: F06, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Use `CompileOptions.Compatibility40` for QName extrema and keyword calls.
The numeric baseline runs with `Compatibility31` and its QName endpoints use
folds. Default 3.1 numeric/string extrema remain available, while these nonempty
QName extrema fail during evaluation with `FORG0006`.

The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
verifies exact text/error markers. `baselineExpected` checks shared default-mode
results; `baselineError` checks the declared default compile/evaluation error.
The observed gap cases are explicitly labeled rather than presented as complete
draft conformance. This delivery changes neither the engine nor gate membership.

## 13. Sample Code

Complete synthetic input:

```xml
<batch xmlns:q="urn:qa:codes" xmlns:e="urn:erp:codes"><reading id="A" value="2.5" code="q:low"/><reading id="B" value="7.5" code="q:high"/><reading id="C" value="5" code="e:normal"/><reading id="D" value="invalid" code="q:invalid"/><reading id="E" value="" code="q:missing"/></batch>
```

### XPath 4.0 report

[Runnable file](../examples/batch-extrema/main.xpath):

```xpath
let $rows := parse-xml('<batch xmlns:q="urn:qa:codes" xmlns:e="urn:erp:codes"><reading id="A" value="2.5" code="q:low"/><reading id="B" value="7.5" code="q:high"/><reading id="C" value="5" code="e:normal"/><reading id="D" value="invalid" code="q:invalid"/><reading id="E" value="" code="q:missing"/></batch>')/batch/reading
return let $valid := $rows[@value castable as xs:decimal]
return let $invalid := $rows[not(@value castable as xs:decimal)]
return let $values := $valid/@value ! xs:decimal(.)
return let $codes := $rows ! resolve-QName(string(@code), .)
return let $label := function($q) { if (empty($q)) then '-' else concat('Q{', namespace-uri-from-QName($q), '}', local-name-from-QName($q)) }
return let $first := min($codes)
return let $last := max($codes)
return let $low := min($values)
return let $high := max($values)
return concat('accepted=', string-join($valid/@id, ','), '; rejected=', string-join($invalid/@id, ','), '; min=', if (empty($low)) then '-' else string($low), '; max=', if (empty($high)) then '-' else string($high), '; first-code=', $label($first), '; last-code=', $label($last))
```

Exact output:

```text
accepted=A,B,C; rejected=D,E; min=2.5; max=7.5; first-code=Q{urn:erp:codes}normal; last-code=Q{urn:qa:codes}missing
```

A, B and C supply valid decimals; D and E remain rejected identifiers. The ERP
namespace sorts before the QA namespace; within QA, `missing` is the last local
name. This explains the code endpoints without assigning a business rank.

### XPath 3.1 comparison

[Runnable file](../examples/batch-extrema/baseline.xpath):

```xpath
let $rows := parse-xml('<batch xmlns:q="urn:qa:codes" xmlns:e="urn:erp:codes"><reading id="A" value="2.5" code="q:low"/><reading id="B" value="7.5" code="q:high"/><reading id="C" value="5" code="e:normal"/><reading id="D" value="invalid" code="q:invalid"/><reading id="E" value="" code="q:missing"/></batch>')/batch/reading
return let $valid := $rows[@value castable as xs:decimal]
return let $invalid := $rows[not(@value castable as xs:decimal)]
return let $values := $valid/@value ! xs:decimal(.)
return let $codes := $rows ! resolve-QName(string(@code), .)
return let $label := function($q) { if (empty($q)) then '-' else concat('Q{', namespace-uri-from-QName($q), '}', local-name-from-QName($q)) }
return let $cp := 'http://www.w3.org/2005/xpath-functions/collation/codepoint'
return let $less := function($a, $b) {
  let $ns := compare(namespace-uri-from-QName($a), namespace-uri-from-QName($b), $cp)
  return if ($ns eq 0) then compare(local-name-from-QName($a), local-name-from-QName($b), $cp) lt 0 else $ns lt 0
}
return let $first := fold-left($codes, (), function($best, $next) { if (empty($best)) then $next else if ($less($next, $best)) then $next else $best })
return let $last := fold-left($codes, (), function($best, $next) { if (empty($best)) then $next else if ($less($best, $next)) then $next else $best })
return let $low := min($values)
return let $high := max($values)
return concat('accepted=', string-join($valid/@id, ','), '; rejected=', string-join($invalid/@id, ','), '; min=', if (empty($low)) then '-' else string($low), '; max=', if (empty($high)) then '-' else string($high), '; first-code=', $label($first), '; last-code=', $label($last))
```

Exact output:

```text
accepted=A,B,C; rejected=D,E; min=2.5; max=7.5; first-code=Q{urn:erp:codes}normal; last-code=Q{urn:qa:codes}missing
```

The fold comparator performs two comparisons, preserving the namespace/local-name
ordering even when one namespace URI is a prefix of another. The numeric part
uses the same explicit decimal normalization policy as the 4.0 report.

### Complete fixture inventory

| Runnable case | Mode | Exact result/error marker | Default 3.1 outcome also checked |
|---|---|---|---|
| [main](../examples/batch-extrema/main.xpath) | 40 | `accepted=A,B,C; rejected=D,E; min=2.5; max=7.5; first-code=Q{urn:erp:codes}normal; last-code=Q{urn:qa:codes}missing` | error `FORG0006` |
| [baseline](../examples/batch-extrema/baseline.xpath) | 31 | `accepted=A,B,C; rejected=D,E; min=2.5; max=7.5; first-code=Q{urn:erp:codes}normal; last-code=Q{urn:qa:codes}missing` | separate 3.1 fixture |
| [empty](../examples/batch-extrema/empty.xpath) | 40 | `accepted=; rejected=; min=-; max=-; first-code=-; last-code=-` | `accepted=; rejected=; min=-; max=-; first-code=-; last-code=-` |
| [all-invalid](../examples/batch-extrema/all-invalid.xpath) | 40 | `accepted=; rejected=A,B,C,D,E; min=-; max=-; first-code=Q{urn:erp:codes}normal; last-code=Q{urn:qa:codes}missing` | error `FORG0006` |
| [decimal-range](../examples/batch-extrema/decimal-range.xpath) | 40 | `2.5\|7.5` | `2.5\|7.5` |
| [mixed-min-observed](../examples/batch-extrema/mixed-min-observed.xpath) | 40 | `true` | `true` |
| [mixed-max-observed](../examples/batch-extrema/mixed-max-observed.xpath) | 40 | `true` | `true` |
| [decimal-result-observed](../examples/batch-extrema/decimal-result-observed.xpath) | 40 | `true\|false` | `true\|false` |
| [nan-min](../examples/batch-extrema/nan-min.xpath) | 40 | `true` | `true` |
| [nan-max](../examples/batch-extrema/nan-max.xpath) | 40 | `true` | `true` |
| [untyped-values](../examples/batch-extrema/untyped-values.xpath) | 40 | `2\|10` | `2\|10` |
| [string-values](../examples/batch-extrema/string-values.xpath) | 40 | `10\|2` | `10\|2` |
| [empty-values](../examples/batch-extrema/empty-values.xpath) | 40 | `[]\|[]` | `[]\|[]` |
| [booleans](../examples/batch-extrema/booleans.xpath) | 40 | `false\|true` | `false\|true` |
| [empty-collation](../examples/batch-extrema/empty-collation.xpath) | 40 | `a\|b` | `a\|b` |
| [ascii-collation](../examples/batch-extrema/ascii-collation.xpath) | 40 | `a\|B` | `a\|B` |
| [codepoint-collation](../examples/batch-extrema/codepoint-collation.xpath) | 40 | `B\|a` | `B\|a` |
| [mixed-order-observed](../examples/batch-extrema/mixed-order-observed.xpath) | 40 | `1` | error `XPTY0004` |
| [qname-min](../examples/batch-extrema/qname-min.xpath) | 40 | `true` | error `FORG0006` |
| [qname-max](../examples/batch-extrema/qname-max.xpath) | 40 | `true` | error `FORG0006` |
| [qname-prefix](../examples/batch-extrema/qname-prefix.xpath) | 40 | `urn:q` | error `FORG0006` |
| [keyword-values](../examples/batch-extrema/keyword-values.xpath) | 40 | `1\|3` | error `XPST0003` |
| [mixed-min](../examples/batch-extrema/mixed-min.xpath) | 40 | `FORG0006` | not asserted for this error case |
| [mixed-max](../examples/batch-extrema/mixed-max.xpath) | 40 | `FORG0006` | not asserted for this error case |
| [bad-untyped](../examples/batch-extrema/bad-untyped.xpath) | 40 | `FORG0001` | not asserted for this error case |
| [qname-string](../examples/batch-extrema/qname-string.xpath) | 40 | `FORG0006` | not asserted for this error case |
| [plain-duration-min-observed](../examples/batch-extrema/plain-duration-min-observed.xpath) | 40 | `FORG0006` | not asserted for this error case |
| [plain-duration-max-observed](../examples/batch-extrema/plain-duration-max-observed.xpath) | 40 | `FORG0006` | not asserted for this error case |
| [unknown-collation](../examples/batch-extrema/unknown-collation.xpath) | 40 | `FOCH0002` | not asserted for this error case |

Empty and all-invalid batches keep missing numeric endpoints distinct from zero.
The untyped/string cases distinguish atomization from an explicit string policy.
NaN probes verify contamination rather than treating NaN as the lowest usable
measurement. Collation cases specify their string policy; QName-prefix tests
assert expanded identity, not representative prefix selection. Error fixtures
check the marker in the exception message.

## 14. Performance

The report scans and converts the small batch, then finds extrema by linear
selection. The 3.1 baseline uses two linear folds for code endpoints. No benchmark
or complete arbitrary-precision numeric claim is implied.

## 15. Known Uses

Synthetic quality-control batch reconciliation. Named evidence:
[`MinMax_QNameOrdering_OnlyIn40`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
does not assign exact `fn-min` or `fn-max` gated sets. These 29 documentation
checks supply separate evidence and expose incomplete contracts; no gate is
promoted and manual engine conformance gates are not rerun for this delivery.

## 16. Related Features

[Typed measurement comparison](measurement-comparison.md) explains the exact
numeric comparator and its difference from ordinary promotion.
[Numeric import diagnostics](numeric-import-diagnostics.md) explains NaN policy.
[Supplier offer ranking](supplier-offer-ranking.md) distinguishes boundary values
from selecting all equal-price rows.
[The supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md) tracks remaining
signatures/options and engine/specification follow-ups.
