<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak catalog: Typed Measurement Comparison">
  <br><br><h1>Typed Measurement Comparison</h1>
  <p>Frozen 4.0 — a changed existing-function contract</p>
</div>

**C10 contract/evidence review — 26 October 2026:** [Per-entry receipt](../SEMANTIC-REVIEW-BATCH-2.md) records 3 contract groups with exact examples, context, applicable evidence and language-alignment limits. Status: **Exact positional comparison; keyword mismatch remains**. Do not substitute this comparator for engineering tolerance, generalized numeric conversion or automatic unit normalization. Review completion is not final owner acceptance; earlier checkpoints remain historical.

**Related current review:** [Equality/collation contracts](../EQUALITY-COLLATION-CONTRACT-REVIEW.md) refresh the five-family declarations, complete customer example, keyword boundaries and explicitly separate engine findings without replacing the historical evidence below.


## 1. Name and Classification

[fn:compare](https://qt4cg.org/specifications/xpath-functions-40/Overview.html#func-compare)
is an existing XPath 3.1 function with expanded 4.0 comparison semantics. It is
therefore absent from the 117-name inventory of **4.0-only** functions.
Bosak delivery: [PR #112](https://github.com/Fytala-Charles/Bosak/pull/112),
[89c90b4d](https://github.com/Fytala-Charles/Bosak/commit/89c90b4d5b6a216a4a37223e169d36e3a1a832bd).
Reviewed 2026-10-12 against immutable
[QT4CG F&O source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xpath-functions-40/src/function-catalog.xml).
This is a current review, not recovery of a historical adoption pin.

## 2. Intent

Classify imported measurements relative to declared decimal limits while keeping
their supplied numeric types. Show why a binary floating-point reading written
as `3.1` can compare above or below the decimal value `3.1`.

## 3. Also Known As

Three-way comparison, atomic ordering and mixed numeric comparison describe the
operation. There is no new registered function name or alias.

## 4. History / Supersedes

[F&O 3.1 fn:compare](https://www.w3.org/TR/xpath-functions-31/#func-compare)
compares optional strings. The reviewed 4.0 draft expands the comparands to
optional atomic values. Numeric ordering compares their mathematical values
without the usual mixed-type promotion rounding; NaN orders below other numbers
and compares equal to NaN. This does not change the meaning of ordinary `eq` or
`lt` expressions. The earlier-version report below deliberately normalizes to
double and therefore gives a different boundary classification for row B.

## 5. Motivation

A measurement feed may combine decimal instrument exports with binary float or
double readings. Converting every value to double hides some differences at a
decimal threshold. A deterministic comparison can expose those differences for
reconciliation. It cannot decide the engineering tolerance: the application must
define acceptable deviation and rounding separately.

## 6. Language Alignment

Bosak registers `compare#2` and `compare#3`. The result is `-1`, `0` or `1`, or an
empty sequence if either comparand is absent. Two numeric values use 4.0 atomic
ordering; unrelated type families such as number and string raise `XPTY0004`.
Multiple comparands are not a supported singleton contract.

The third argument selects string collation. The 4.0 empty-collation form uses the
default collation; the fixture's default is codepoint. Explicit ASCII-insensitive
and codepoint tests show two distinct label policies. Collation is not a numeric
tolerance or a conversion instruction.

**Keyword drift:** Bosak accepts `comparand1` and `comparand2`; the pinned draft
declares `value1` and `value2`. Current-draft keyword calls are refused with
`XPST0017`. Positional calls avoid this spelling mismatch. Keep the observed
engine form distinct from the reviewed draft rather than claiming full signature
conformance. The earlier-version keyword form is refused with `XPST0003`.

## 7. Applicability

Use this report to reconcile typed readings against decimal limits. Input IDs,
kind labels and decimal limits are assumed present and valid; kinds are restricted
to `decimal`, `float` and `double`. The fixture omits unit conversion, calibration,
measurement uncertainty, schema validation and real device acquisition. Missing
text and numeric NaN become `missing`; malformed nonempty numeric text still
raises a constructor error. This is not a general import validator.

## 8. Structure

Parse the synthetic XML, convert each supplied reading according to its declared
kind, read its decimal limit, screen absent/NaN values, and classify the sign of
the comparison. Emit IDs in input order as a compact reconciliation report.

## 9. Participants

The XML carries lexical values and numeric kinds. Constructors create typed
atomic values. The guard implements the application's missing-data policy.
`compare` returns ordering; the report maps it to `below`, `at-limit` or `above`.

## 10. Collaborations

`empty($value)` handles absent data, while `not($value eq $value)` identifies
numeric NaN using ordinary equality. The policy screens NaN before ordering it;
otherwise the function would place NaN below the limit. `string-join` preserves
the row classifications. No implicit number/string conversion is requested.

## 11. Consequences

Row B's double approximation is greater than decimal `3.1`; row C's float
approximation is smaller. The 3.1 comparison normalizes both sides to double,
so B becomes `at-limit`, while C remains `below`. Neither report is a tolerance
policy. Do not replace the explicit comparator with `eq`/`lt` and expect identical
mixed numeric boundary results. NaN's equality inside this function also does not
make `NaN eq NaN` true.

## 12. Implementation

**C02 reconciliation — 26 October 2026, published 1.26.3:** [Function/options review](../FUNCTION-OPTIONS-RECONCILIATION.md) reconciles Exact cross-numeric comparison, NaN ordering and empty comparands; Atomic family comparison and incomparable-family error; Default/explicit collation and released versus draft keyword labels. **Boundaries:** Do not substitute this comparator for engineering tolerance, generalized numeric conversion or automatic unit normalization. Follow-up groups: F01, F08. Normal/empty/error evidence and unsupported remainder are recorded per function in the [review matrix](../FUNCTION-OPTIONS-RECONCILIATION.json). This is bounded documentation reconciliation, not whole-signature or AC-04 acceptance.

Run expanded atomic comparison with `CompileOptions.Compatibility40`; the
earlier-version report uses `Compatibility31`. Default 3.1 still supports ordinary
string `compare`, but nonempty numeric comparands in these fixtures raise
`XPTY0004` when evaluated. Refusal is not necessarily a compile-time failure.

The [catalog harness](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs)
uses `baselineError` for evaluation-time default-mode errors and `baselineExpected`
for unchanged results shared by both modes. New 4.0-only syntax retains the
existing compile-refusal check when neither field is supplied. These declarations
let an existing function's changed semantics be tested without falsely asserting
that the function itself is unavailable in 3.1.

**Supplementary contract review (2026-10-12):** The [signature audit](../SIGNATURE-CONTRACT-AUDIT.md) now records this keyword drift alongside conversion/URI differences and selected defaults/options; it does not fix comparison semantics.

## 13. Sample Code

Complete synthetic input:

```xml
<readings><reading id="A" kind="decimal" value="2" limit="2"/><reading id="B" kind="double" value="3.1" limit="3.1"/><reading id="C" kind="float" value="3.1" limit="3.1"/><reading id="D" kind="double" value="NaN" limit="3.1"/><reading id="E" kind="double" value="" limit="3.1"/></readings>
```

### XPath 4.0 report

[Runnable file](../examples/measurement-comparison/main.xpath):

```xpath
let $rows := parse-xml('<readings><reading id="A" kind="decimal" value="2" limit="2"/><reading id="B" kind="double" value="3.1" limit="3.1"/><reading id="C" kind="float" value="3.1" limit="3.1"/><reading id="D" kind="double" value="NaN" limit="3.1"/><reading id="E" kind="double" value="" limit="3.1"/></readings>')/readings/reading
return string-join(for $row in $rows return
  let $value := if (string($row/@value) = '') then () else
    if ($row/@kind = 'decimal') then xs:decimal($row/@value) else
    if ($row/@kind = 'float') then xs:float($row/@value) else xs:double($row/@value)
  return let $limit := xs:decimal($row/@limit)
  return let $status := if (empty($value) or not($value eq $value)) then 'missing' else
    let $order := compare($value, $limit)
    return if ($order lt 0) then 'below' else if ($order eq 0) then 'at-limit' else 'above'
  return concat($row/@id, '=', $status), ';')
```

Exact output:

```text
A=at-limit;B=above;C=below;D=missing;E=missing
```

A is decimal 2 against decimal 2. B and C share the decimal limit 3.1 but retain
their distinct binary representations. D contains numeric NaN and E has no value;
both are reported as missing instead of being silently ranked below the limit.

### XPath 3.1 comparison with explicit normalization

[Runnable file](../examples/measurement-comparison/baseline.xpath):

```xpath
let $rows := parse-xml('<readings><reading id="A" kind="decimal" value="2" limit="2"/><reading id="B" kind="double" value="3.1" limit="3.1"/><reading id="C" kind="float" value="3.1" limit="3.1"/><reading id="D" kind="double" value="NaN" limit="3.1"/><reading id="E" kind="double" value="" limit="3.1"/></readings>')/readings/reading
return string-join(for $row in $rows return
  let $value := if (string($row/@value) = '') then () else
    if ($row/@kind = 'decimal') then xs:decimal($row/@value) else
    if ($row/@kind = 'float') then xs:float($row/@value) else xs:double($row/@value)
  return let $limit := xs:decimal($row/@limit)
  return let $status := if (empty($value) or not($value eq $value)) then 'missing' else
    let $normalized := xs:double($value)
    return let $boundary := xs:double($limit)
    return if ($normalized lt $boundary) then 'below' else if ($normalized eq $boundary) then 'at-limit' else 'above'
  return concat($row/@id, '=', $status), ';')
```

Exact output:

```text
A=at-limit;B=at-limit;C=below;D=missing;E=missing
```

This is a different numeric policy, not an exact 3.1 implementation of the 4.0
atomic-ordering function. Stringifying the numbers and calling 3.1 `compare`
would instead compare their text and would be unsuitable for numeric limits.

### Complete fixture inventory

| Runnable case | Mode | Exact result/error marker | Default 3.1 outcome also checked |
|---|---|---|---|
| [main](../examples/measurement-comparison/main.xpath) | 40 | `A=at-limit;B=above;C=below;D=missing;E=missing` | error `XPTY0004` |
| [baseline](../examples/measurement-comparison/baseline.xpath) | 31 | `A=at-limit;B=at-limit;C=below;D=missing;E=missing` | separate 3.1 fixture |
| [empty-report](../examples/measurement-comparison/empty-report.xpath) | 40 | empty string | empty string |
| [double-decimal](../examples/measurement-comparison/double-decimal.xpath) | 40 | `1` | error `XPTY0004` |
| [float-decimal](../examples/measurement-comparison/float-decimal.xpath) | 40 | `-1` | error `XPTY0004` |
| [equal-numeric](../examples/measurement-comparison/equal-numeric.xpath) | 40 | `0` | error `XPTY0004` |
| [reversed-numeric](../examples/measurement-comparison/reversed-numeric.xpath) | 40 | `-1` | error `XPTY0004` |
| [nan-low](../examples/measurement-comparison/nan-low.xpath) | 40 | `-1` | error `XPTY0004` |
| [nan-equal](../examples/measurement-comparison/nan-equal.xpath) | 40 | `0` | error `XPTY0004` |
| [infinity](../examples/measurement-comparison/infinity.xpath) | 40 | `-1` | error `XPTY0004` |
| [negative-zero](../examples/measurement-comparison/negative-zero.xpath) | 40 | `0` | error `XPTY0004` |
| [booleans](../examples/measurement-comparison/booleans.xpath) | 40 | `-1` | error `XPTY0004` |
| [binary-octets](../examples/measurement-comparison/binary-octets.xpath) | 40 | `0` | `-1` |
| [utc-instant](../examples/measurement-comparison/utc-instant.xpath) | 40 | `0` | error `XPTY0004` |
| [empty-left](../examples/measurement-comparison/empty-left.xpath) | 40 | empty string | empty string |
| [empty-right](../examples/measurement-comparison/empty-right.xpath) | 40 | empty string | empty string |
| [promoted-equality](../examples/measurement-comparison/promoted-equality.xpath) | 31 | `true\|false` | separate 3.1 fixture |
| [ascii-labels](../examples/measurement-comparison/ascii-labels.xpath) | 40 | `0` | `0` |
| [codepoint-labels](../examples/measurement-comparison/codepoint-labels.xpath) | 40 | `-1` | `-1` |
| [keyword-engine](../examples/measurement-comparison/keyword-engine.xpath) | 40 | `-1` | error `XPST0003` |
| [keyword-draft](../examples/measurement-comparison/keyword-draft.xpath) | 40 | `XPST0017` | not asserted for this error case |
| [keyword-31](../examples/measurement-comparison/keyword-31.xpath) | 31 | `XPST0003` | separate 3.1 fixture |
| [empty-collation](../examples/measurement-comparison/empty-collation.xpath) | 40 | `-1` | `-1` |
| [mixed-family](../examples/measurement-comparison/mixed-family.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [multiple-values](../examples/measurement-comparison/multiple-values.xpath) | 40 | `XPTY0004` | not asserted for this error case |
| [map-value](../examples/measurement-comparison/map-value.xpath) | 40 | `FOTY0013` | not asserted for this error case |
| [unknown-collation](../examples/measurement-comparison/unknown-collation.xpath) | 40 | `FOCH0002` | not asserted for this error case |

The label and empty-input cases verify both modes. Empty comparands are wrapped in `string()` so the published result is the empty string, independent of the API display of empty sequences. Bosak also accepts empty collation in 3.1 here; that observed result is not claimed as a portable earlier-version signature contract.

The binary probe returns `0` in 4.0 but `-1` in default 3.1: the current 3.1 implementation accepts their string-backed values and orders the different lexical spellings. F&O 3.1 defines string comparands; this probe records Bosak behavior, not portable binary comparison. Keep this type-validation boundary as a separate implementation follow-up. Numeric-success
cases also verify the declared default-3.1 error. The boolean, binary and explicit
UTC timestamp probes illustrate additional atomic families but do not certify all
duration, QName, calendar subtype, timezone or user-defined-type contracts.
Invalid-input cases assert an error marker in the exception message.

## 14. Performance

The report converts and compares each row once. Exact mixed numeric comparison
may cost more than promotion to double; no benchmark or arbitrary-precision
decimal arithmetic claim follows from these examples. Bosak's decimal storage
limitations remain separate from its comparison implementation.

## 15. Known Uses

Synthetic measurement reconciliation. Named implementation evidence:
[`Compare40_CrossNumericExact_DoubleVsDecimal`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`Compare40_NanOrdersBelowNegativeInfinity_AndEqualsItself`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`Compare40_BooleansAndBinariesAndQName`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs),
[`Compare40_MixedIncomparable_RaisesXpty0004`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs)
and [`Compare40_KeywordFormAndEmptyCollation`](../../../tests/Bosak.XPath.Standard.Tests/XPath40FunctionTests.cs).
The [4.0 runner](../../../tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs)
contains the exact `fn-compare` gated set. These 27 documentation cases are
additional evidence; this delivery changes neither gate membership nor engine
semantics and does not rerun the manual conformance gates.

## 16. Related Features

[Numeric import diagnostics](numeric-import-diagnostics.md) explains NaN screening
and type reflection. [Supplier offer ranking](supplier-offer-ranking.md) uses
business ordering and explicit equal-price selection.
[Comparator-based sorting](sort-with.md) covers callbacks.
[The supplementary audit](../SUPPLEMENTARY-COVERAGE-AUDIT.md) tracks remaining
changed signatures/options separately from name assignment.
